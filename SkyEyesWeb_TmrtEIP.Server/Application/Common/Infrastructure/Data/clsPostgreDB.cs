using Npgsql;
using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Data;

// clsPostgreDB：比照 clsDapper（SQL Server）架構另開發的 PostgreSQL 版資料存取共用類別。
// 兩者並存、互不取代：clsDapper 仍是目前實際連線的 SQL Server 實作，clsPostgreDB 供之後
// 遷移到 PostgreSQL 的 Controller/Service 改用。詳見 docs/server/README.md「資料庫引擎（PostgreSQL 遷移中）」。
public class clsPostgreDB
{

    public static string? ConnectionStrings;
    public static int timeoutInSeconds = 30;

    public static NpgsqlConnection getConnection()
    {
        string strConn = ConnectionStrings ?? throw new InvalidOperationException("Connection string is not set.");
        return new NpgsqlConnection(strConn);
    }

    /// <summary>
    /// 執行 SQL 查詢並返回結果列表。
    /// </summary>
    /// <typeparam name="T">結果列表中元素的類型。</typeparam>
    /// <param name="strSql">要執行的 SQL 查詢字串。</param>
    /// <param name="Datas">查詢參數。</param>
    /// <param name="commandTimeout">查詢超時時間（秒），預設為 30 秒。</param>
    /// <returns>查詢結果的列表。</returns>
    public static List<T> getList<T>(string strSql, object Datas, int? commandTimeout = null, CommandType? CommandType = System.Data.CommandType.Text)
    {
        try
        {
            commandTimeout = commandTimeout == null ? timeoutInSeconds : commandTimeout;

            using var conn = getConnection();
            var results = conn.Query<T>(strSql, Datas, commandTimeout: commandTimeout, commandType: CommandType).ToList();
            return results;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// 執行 SQL 查詢並返回結果列表，每個結果為字典形式。
    /// </summary>
    /// <param name="strSql">要執行的 SQL 查詢字串。</param>
    /// <param name="Datas">查詢參數。</param>
    /// <param name="commandTimeout">查詢超時時間（秒），預設為 30 秒。</param>
    /// <param name="CommandType">查詢命令的類型，預設為 CommandType.Text。</param>
    /// <returns>查詢結果的列表，每個結果為字典形式。</returns>
    public static List<Dictionary<string, object>> getList(string strSql, object Datas, int? commandTimeout = null, CommandType? CommandType = System.Data.CommandType.Text)
    {
        try
        {
            commandTimeout = commandTimeout == null ? timeoutInSeconds : commandTimeout;

            using var conn = getConnection();
            var results = conn.Query<object>(strSql, Datas, commandTimeout: commandTimeout, commandType: CommandType).ToList();
            var rs = new List<Dictionary<string, object>>();
            foreach (var row in results)
            {
                var expando = (IDictionary<string, object>)row;
                var dict = new Dictionary<string, object>(expando, StringComparer.OrdinalIgnoreCase);
                rs.Add(dict);
            }
            return rs;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// 執行 SQL 查詢並返回分頁結果。
    /// </summary>
    /// <remarks>
    /// ⚠️ 呼叫慣例與 <c>clsDapper.getListPagination</c> 不同：clsDapper 是 T-SQL 專屬，
    /// 呼叫端須先自行 <c>DECLARE @DT Table (...)</c> 並 INSERT 資料，再讓本方法把分頁 SQL
    /// 接在後面對 <c>@DT</c> 做 <c>ROW_NUMBER()</c>；PostgreSQL 沒有 table variable，因此
    /// 本方法改為直接把呼叫端傳入的 <paramref name="strSql"/>（一般的 SELECT 查詢）整段包成 CTE，
    /// 不需要事先宣告/INSERT 任何暫存結構，並改用 PostgreSQL 慣用的 <c>LIMIT</c>/<c>OFFSET</c> 分頁。
    /// </remarks>
    /// <typeparam name="T">結果列表中元素的類型。</typeparam>
    /// <param name="strSql">要執行的 SQL 查詢字串（一般 SELECT 語句，本方法會將其包成 CTE，不需要 <c>@DT</c>）。</param>
    /// <param name="Datas">查詢參數。</param>
    /// <param name="PageRowCount">每頁顯示的資料筆數。</param>
    /// <param name="NowPageNum">當前頁碼。</param>
    /// <param name="KeyColumn">用於排序的欄位名稱（直接以字串插入 SQL，須確保來源可信，避免注入）。</param>
    /// <param name="Ord">排序方式，預設為 ASC。</param>
    /// <param name="commandTimeout">查詢超時時間（秒），預設為 30 秒。</param>
    /// <returns>包含分頁結果的 PaginationInfo 物件。</returns>
    public static PaginationInfo getListPagination(
        string strSql,
        object Datas,
        int PageRowCount,
        int NowPageNum,
        string KeyColumn,
        enumOrder Ord = enumOrder.ASC,
        int commandTimeout = -1
        )
    {
        try
        {

            PaginationInfo PaginationInfo = new PaginationInfo();

            string pagedSql = string.Format(@"
                WITH T AS (
                    {0}
                )
                SELECT *, COUNT(*) OVER() AS TotalCount
                FROM T
                ORDER BY {1} {2}
                LIMIT @pageSize OFFSET @pageOffset;
            ", strSql, KeyColumn, Ord);

            var Datas2 = new { pageSize = PageRowCount, pageOffset = (NowPageNum - 1) * PageRowCount };

            // 合併 Datas 和 Datas2
            var combinedDatas = new DynamicParameters(Datas);
            combinedDatas.AddDynamicParams(Datas2);

            commandTimeout = commandTimeout == -1 ? timeoutInSeconds : commandTimeout;

            PaginationInfo.Result = true;
            PaginationInfo.PageCount = 0;
            PaginationInfo.DataCount = 0;
            PaginationInfo.Data = getList(pagedSql, combinedDatas, commandTimeout: commandTimeout);

            if (PaginationInfo.Data.Count > 0) {
                PaginationInfo.PageCount = ((Convert.ToInt32(PaginationInfo.Data[0]["TotalCount"]) - 1) / PageRowCount) + 1;
                PaginationInfo.DataCount = Convert.ToInt32(PaginationInfo.Data[0]["TotalCount"]);
            }

            return PaginationInfo;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static void execNonQuery(string strSql, object Datas, int? commandTimeout = null, CommandType? CommandType = System.Data.CommandType.Text)
    {
        try
        {

            commandTimeout = commandTimeout == null ? timeoutInSeconds : commandTimeout;

            using var conn = getConnection();
            conn.Execute(strSql, Datas, commandTimeout: commandTimeout, commandType: CommandType);

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// 將物件中的所有屬性轉換為 Dapper 查詢參數。
    /// </summary>
    /// <remarks>
    /// PostgreSQL 的 <c>text</c>/<c>varchar</c> 一律為 UTF-8，沒有 SQL Server
    /// <c>varchar</c>（ANSI）/<c>nvarchar</c>（Unicode）的區分，因此不像
    /// <c>clsDapper.ConvertStringProperties</c> 需要透過 <c>DbString.IsAnsi</c> 特別標註，
    /// 字串屬性直接以一般參數帶入即可。保留此方法僅為與 <c>clsDapper</c> 呼叫介面一致。
    /// </remarks>
    /// <param name="Datas">包含查詢參數的物件。</param>
    /// <returns>轉換後的 DynamicParameters 物件。</returns>
    public static DynamicParameters ConvertStringProperties(object Datas)
    {
        var parameters = new DynamicParameters(Datas);
        return parameters;
    }

    public static System.Data.DataTable ToDataTable(List<Dictionary<string, object>> dt)
    {
        try
        {

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(dt);
            System.Data.DataTable Res = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(json);
            return Res;

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public enum enumOrder
    {
        ASC,
        DESC
    }

    public class PaginationInfo
    {
        /// <summary>
        /// 回傳結果
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// 資料筆數
        /// </summary>
        public int DataCount { get; set; }
        /// <summary>
        /// 資料分頁數
        /// </summary>
        public int PageCount { get; set; }
        /// <summary>
        /// 分頁資料
        /// </summary>
        public List<Dictionary<string, object>> Data { get; set; }
    }

}
