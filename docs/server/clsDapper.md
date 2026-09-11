# `clsDapper`

檔案位置：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsDapper.cs`

本文件描述 `clsDapper` 的使用慣例與行為，供 Controller/Service 撰寫查詢與更新資料時參考。

> ⚠️ **已停用（保留但不再使用）**：專案資料庫引擎已由 SQL Server 改為 PostgreSQL，且查詢執行層已全面改用 `App_Code/clsPostgreDB.cs`（見 `docs/server/clsPostgreDB.md`）。目前專案中**沒有任何程式碼呼叫 `clsDapper`**；此檔案與本文件僅保留供參考/比對（本機專案非 git 版控，刪除後難以復原，故未在未經確認下移除）。以下內容描述的是這支「已停用」類別過去/目前程式碼本身的行為，不代表目前系統實際執行的資料存取路徑。詳見 `docs/server/README.md`「資料庫引擎（PostgreSQL）」。

## 目的與定位

`clsDapper` 是專案的資料存取共用類別，封裝 Dapper 的基本查詢/分頁查詢/更新 SQL（non-query）的操作。

主要特性：

- 統一連線字串：`clsDapper.ConnectionStrings`
- 統一預設 timeout：`clsDapper.timeoutInSeconds`（預設 30 秒）
- `getList(...)` 回傳 `List<Dictionary<string, object>>`，key **不分大小寫**（`StringComparer.OrdinalIgnoreCase`）
- `getListPagination(...)` 採用「呼叫端建立 `@DT` 變數」的分頁模式

## 靜態設定

### `ConnectionStrings`

```csharp
public static string? ConnectionStrings;
```

- 必須在使用前設定（`Program.cs` 啟動時透過解密後的連線字串賦值）。
- 若為 `null`，呼叫 `getConnection()` 會拋出 `InvalidOperationException("Connection string is not set.")`。

### `timeoutInSeconds`

```csharp
public static int timeoutInSeconds = 30;
```

- 在 `getList(...)` / `getListPagination(...)` / `execNonQuery(...)` 未帶入 timeout 時使用。

## 連線

### `getConnection()`

```csharp
public static SqlConnection getConnection()
```

- 以 `ConnectionStrings` 建立並回傳新的 `SqlConnection`。
- ⚠️ SQL Server 專屬：遷移至 PostgreSQL 後，回傳型別需改為 `Npgsql.NpgsqlConnection`。

## 查詢

### `getList<T>(string strSql, object Datas, int? commandTimeout = null, CommandType? CommandType = CommandType.Text)`

以 Dapper 執行查詢並映射成強型別集合。

#### 參數

- `strSql`：SQL 字串。
- `Datas`：查詢參數物件（匿名物件或 `DynamicParameters`）。
- `commandTimeout`：秒數；`null` 則使用 `timeoutInSeconds`。
- `CommandType`：預設 `CommandType.Text`。

#### 回傳

- `List<T>`

#### 例外

- 內部例外會直接往外拋出（目前實作為 `throw ex;`）。

### `getList(string strSql, object Datas, int? commandTimeout = null, CommandType? CommandType = CommandType.Text)`

以 Dapper 執行查詢，並將每列結果轉換為 `Dictionary<string, object>`。

#### 回傳資料型別特性

- 回傳 `List<Dictionary<string, object>>`
- 每個 `Dictionary` 的 key 採用 `StringComparer.OrdinalIgnoreCase`，因此：
  - `row["User_No"]` 與 `row["user_no"]` 等價

## 分頁查詢

### `getListPagination(string strSql, object Datas, int PageRowCount, int NowPageNum, string KeyColumn, enumOrder Ord = enumOrder.ASC, int commandTimeout = -1)`

此方法會在傳入的 `strSql` **後方追加**分頁用 SQL，並回傳分頁資訊。

⚠️ SQL Server 專屬：以下 `@DT` table variable + `ROW_NUMBER()` 的分頁模式為 T-SQL 語法，遷移至 PostgreSQL 後需改用 PostgreSQL 對應寫法（例如以 CTE/暫存資料 + `LIMIT`/`OFFSET`，或維持 `ROW_NUMBER()` window function 但改寫周邊 T-SQL 專屬語法如 `DECLARE ... Table`）。

#### 重要前置條件（必要）

`strSql` 內必須先建立並填入名為 `@DT` 的 table variable，例如：

```sql
DECLARE @DT Table (
  -- 欄位定義
);

INSERT INTO @DT
SELECT ...
FROM ...
WHERE ...;
```

因為 `getListPagination(...)` 追加的分頁段會直接對 `@DT` 做 `ROW_NUMBER()` 與 `COUNT(*)`。

若未宣告 `@DT`，SQL 會執行失敗。

#### 參數補充

- `PageRowCount`：每頁筆數（會帶入 `@pageSize`）。
- `NowPageNum`：頁碼（會帶入 `@pageNo`）。
- `KeyColumn`：排序欄位名稱（直接以字串插入 SQL，須確保來源可信，避免注入）。
- `Ord`：排序方向 `ASC`/`DESC`。
- `commandTimeout`：
  - `-1`：使用 `timeoutInSeconds`
  - 其他：使用指定秒數

#### 回傳：`PaginationInfo`

```csharp
public class PaginationInfo
{
    public bool Result { get; set; }
    public int DataCount { get; set; }
    public int PageCount { get; set; }
    public List<Dictionary<string, object>> Data { get; set; }
}
```

計算邏輯：

- `Data` 會包含一欄額外的 `TotalCount`（由 `COUNT(1)` 產生）。
- 若 `Data.Count > 0`：
  - `DataCount = Convert.ToInt32(Data[0]["TotalCount"])`
  - `PageCount = ((DataCount - 1) / PageRowCount) + 1`

---

## Dapper `IN` 清單參數（`IEnumerable`）支援

本專案的 `clsDapper` 底層以 Dapper 的 `Query(...)` / `Execute(...)` 執行 SQL，因此可直接使用 Dapper 內建的「清單參數展開」能力。

當 SQL 使用 `IN @ParamName`（非 `IN (@ParamName)`），參數 `@ParamName` 傳入的是 `IEnumerable<T>`（例如 `List<string>`、`string[]`、`List<int>`）時，Dapper 會自動將其展開成多個參數，例如：

- SQL：`... WHERE UserGroup_id IN @UserGroup_id`
- 參數：`new { UserGroup_id = new List<int> { 1, 2 } }`

Dapper 會在送出前將 SQL 轉換成類似：

- `... WHERE UserGroup_id IN (@UserGroup_id1, @UserGroup_id2)`

並自動帶入對應的參數值。

### 注意：空清單（empty list）

若清單為空，Dapper 無法展開出有效的 `IN (...)`，可能導致 SQL 語法錯誤或查詢行為不如預期。

因此 Controller 端常見做法是：

- 清單為空：不組出 `IN ...` 條件（等同不加該段 WHERE）
- 清單有值：才組出 `IN @ParamName`

### 範例

```csharp
var userGroupIds = new List<int> { 1, 2 };

var sql = @"
SELECT *
FROM SYS_tbMenu
WHERE 1=1
" + (userGroupIds.Count == 0 ? "" : " AND ParentId IN @ParentId");

var rows = clsDapper.getList(sql, new { ParentId = userGroupIds });
```

## 寫入/更新（Non-Query）

### `execNonQuery(string strSql, object Datas, int? commandTimeout = null, CommandType? CommandType = CommandType.Text)`

使用 Dapper `Execute(...)` 執行 `INSERT/UPDATE/DELETE` 等不需回傳資料列的 SQL。

## 輔助方法

### `ConvertStringProperties(object Datas)`

將 `Datas` 物件的所有 `string` 屬性轉換為資料庫 `varchar` 參數（透過 `ToConstVarChar()` 擴充方法），並回傳 `DynamicParameters`。

> 注意：此方法依賴 `ToConstVarChar()` 擴充方法的存在與行為。

### `ToDataTable(List<Dictionary<string, object>> dt)`

透過 JSON 序列化/反序列化將 `List<Dictionary<string, object>>` 轉成 `System.Data.DataTable`。

## `enumOrder`

```csharp
public enum enumOrder
{
    ASC,
    DESC
}
```

## 使用範例

### 1) 一般查詢

```csharp
var rows = clsDapper.getList(
    "SELECT * FROM SYS_tbMenu WHERE Id = @id",
    new { id = 1 }
);
```

### 2) 分頁查詢（需先建立 `@DT`）

```csharp
// ⚠️ SQL Server 專屬語法（T-SQL）；遷移至 PostgreSQL 後需改寫（例如 DATEADD → sdate/edate + interval '1 day'）
var sql = @"
DECLARE @DT Table (
    Id bigint,
    SystemTime datetime
);

INSERT INTO @DT
SELECT Id, SystemTime
FROM SYS_ttLog
WHERE SystemTime >= @sdate AND SystemTime < DATEADD(Day, 1, @edate);
";

var page = clsDapper.getListPagination(
    sql,
    new { sdate = "2026-01-01", edate = "2026-01-31" },
    PageRowCount: 10,
    NowPageNum: 1,
    KeyColumn: "SystemTime",
    Ord: clsDapper.enumOrder.DESC
);
```
