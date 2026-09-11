using Microsoft.Data.SqlClient;
using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Globalization;

public class clsDapper
{

    public static string? ConnectionStrings;
    public static int timeoutInSeconds = 30;

    public static SqlConnection getConnection()
    {
        string strConn = ConnectionStrings ?? throw new InvalidOperationException("Connection string is not set.");
        return new SqlConnection(strConn);
    }

    /// <summary>
    /// 執行 SQL 查詢並返回結果列表。
    /// </summary>
    /// <typeparam name="T">結果列表中元素的類型。</typeparam>
    /// <param name="strSql">要執行的 SQL 查詢字串。</param>
    /// <param name="Datas">查詢參數。</param>
    /// <param name="commandTimeout">查詢超時時間（秒），預設為 30 秒。</param>
    /// <returns>查詢結果的列表。</returns>
    public static List<T> getList<T>(string strSql, object Datas, int? commandTimeout = null, System.Data.CommandType? CommandType = CommandType.Text)
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
    public static List<Dictionary<string, object>> getList(string strSql, object Datas, int? commandTimeout = null, System.Data.CommandType? CommandType = CommandType.Text)
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
    /// <typeparam name="T">結果列表中元素的類型。</typeparam>
    /// <param name="strSql">要執行的 SQL 查詢字串。</param>
    /// <param name="Datas">查詢參數。</param>
    /// <param name="PageRowCount">每頁顯示的資料筆數。</param>
    /// <param name="NowPageNum">當前頁碼。</param>
    /// <param name="KeyColumn">用於排序的主鍵欄位。</param>
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

            strSql += string.Format(@"
                ;WITH T
                AS (
                    SELECT ROW_NUMBER() OVER (ORDER BY {0} {1}) AS RowNo,* From @DT 
                    ),
                T2 AS (
                    SELECT COUNT(1) TotalCount FROM T
                )
                SELECT *
                FROM T2, T
                WHERE RowNo BETWEEN (@pageNo - 1) * @pageSize  + 1 
                AND @pageNo * @pageSize;
            ", KeyColumn, Ord);

            var Datas2 = new { @pageSize = PageRowCount, @pageNo = NowPageNum };

            // 合併 Datas 和 Datas2
            var combinedDatas = new DynamicParameters(Datas);
            combinedDatas.AddDynamicParams(Datas2);

            commandTimeout = commandTimeout == -1 ? timeoutInSeconds : commandTimeout;

            PaginationInfo.Result = true;
            PaginationInfo.PageCount = 0;
            PaginationInfo.DataCount = 0;
            PaginationInfo.Data = getList(strSql, combinedDatas, commandTimeout: commandTimeout);

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

    public static void execNonQuery(string strSql, object Datas, int? commandTimeout = null, System.Data.CommandType? CommandType = CommandType.Text)
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
    /// 將物件中的所有 string 型態屬性轉換為資料庫 varchar 型態。
    /// </summary>
    /// <param name="Datas">包含查詢參數的物件。</param>
    /// <returns>轉換後的 DynamicParameters 物件。</returns>
    public static DynamicParameters ConvertStringProperties(object Datas)
    {
        var parameters = new DynamicParameters(Datas);

        foreach (var property in Datas.GetType().GetProperties())
        {
            if (property.PropertyType == typeof(string))
            {
                var value = (string)property.GetValue(Datas);
                parameters.Add(property.Name, value != null ? value.ToConstVarChar() : value);
            }
            else
            {
                parameters.Add(property.Name, property.GetValue(Datas));
            }
        }

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
        /// 分頁DataTable
        /// </summary>
        public List<Dictionary<string, object>> Data { get; set; }
    }

}

#region 字串轉資料庫varchar型態
public static class ExtMethods
{
    public static DbString ToConstVarChar(this string str, int maxLength = 0) =>
        new DbString { Value = str, IsAnsi = true, Length = maxLength == 0 ? str.Length : maxLength };

}
#endregion