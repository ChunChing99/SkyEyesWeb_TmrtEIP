# `clsPostgreDB`

檔案位置：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsPostgreDB.cs`

本文件描述 `clsPostgreDB` 的使用慣例與行為，供 Controller/Service 撰寫查詢與更新資料時參考。

`clsPostgreDB` 是專案目前**實際使用**的資料存取共用類別（PostgreSQL／`Npgsql`），比照舊版 `clsDapper`（SQL Server，已停用，見 `docs/server/clsDapper.md`）的架構開發，介面設計盡量一致，方便對照閱讀；`Program.cs`、`App_Code/clsApiBase.cs`、`App_Code/clsApiBase.MenuAuth.cs`、`BackgroundWorker/Tasks/TaskDeleteLog.cs` 皆已改用本類別。

## 目的與定位

`clsPostgreDB` 封裝 Dapper 的基本查詢/分頁查詢/更新 SQL（non-query）的操作，底層連線使用 `Npgsql`。

主要特性：

- 統一連線字串：`clsPostgreDB.ConnectionStrings`（`Program.cs` 啟動時賦值，需為 **Npgsql 連線字串格式**，例如 `Host=...;Port=5432;Database=...;Username=...;Password=...;`）
- 統一預設 timeout：`clsPostgreDB.timeoutInSeconds`（預設 30 秒）
- `getList(...)` 回傳 `List<Dictionary<string, object>>`，key **不分大小寫**（`StringComparer.OrdinalIgnoreCase`）——這點也讓 PostgreSQL 對未加雙引號識別字一律轉小寫存放的行為（`SystemTime` 實際存成 `systemtime`）不影響查詢結果存取。
- `getListPagination(...)` 直接把呼叫端傳入的 SELECT 語句包成 CTE 分頁，**不需要**（也無法）像 T-SQL 那樣先宣告 table variable，詳見下方。

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
public static NpgsqlConnection getConnection()
```

- 以 `ConnectionStrings` 建立並回傳新的 `NpgsqlConnection`。

## 查詢

### `getList<T>(string strSql, object Datas, int? commandTimeout = null, CommandType? CommandType = CommandType.Text)`

以 Dapper 執行查詢並映射成強型別集合。

#### 參數

- `strSql`：SQL 字串（PostgreSQL 語法）。
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
  - PostgreSQL 回傳的欄位名稱實際上是全小寫（例如 `SystemTime` 會變成 `systemtime`），但因為是不分大小寫比對，`row["SystemTime"]` 一樣能正確取值

## 分頁查詢

### `getListPagination(string strSql, object Datas, int PageRowCount, int NowPageNum, string KeyColumn, enumOrder Ord = enumOrder.ASC, int commandTimeout = -1)`

此方法會把傳入的 `strSql`（一般的 SELECT 查詢）整段包成 CTE，並在外層加上分頁邏輯，回傳分頁資訊。

#### 與 `clsDapper.getListPagination` 的差異（重要）

`clsDapper`（T-SQL）要求呼叫端先自行：

```sql
DECLARE @DT Table ( -- 欄位定義 );
INSERT INTO @DT SELECT ... FROM ... WHERE ...;
```

再讓方法把分頁 SQL 接在後面對 `@DT` 做 `ROW_NUMBER()`。**PostgreSQL 沒有 table variable**，因此 `clsPostgreDB.getListPagination` 改為：呼叫端只需傳入一般的 `SELECT ...` 查詢語句（不需要 `DECLARE`/`INSERT`），本方法會自動包成：

```sql
WITH T AS (
    -- 呼叫端傳入的 strSql
)
SELECT *, COUNT(*) OVER() AS TotalCount
FROM T
ORDER BY {KeyColumn} {Ord}
LIMIT @pageSize OFFSET @pageOffset;
```

用 window function `COUNT(*) OVER()` 一次取得總筆數，並用 PostgreSQL 慣用的 `LIMIT`/`OFFSET` 分頁（`@pageOffset` 由 `(NowPageNum - 1) * PageRowCount` 在 C# 端算好帶入）。

#### 參數補充

- `strSql`：一般 SELECT 查詢語句（**不需要** `@DT`）。
- `PageRowCount`：每頁筆數（會帶入 `@pageSize`）。
- `NowPageNum`：頁碼（用於計算 `@pageOffset`）。
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

- `Data` 會包含一欄額外的 `TotalCount`（由 `COUNT(*) OVER()` 產生）。
- 若 `Data.Count > 0`：
  - `DataCount = Convert.ToInt32(Data[0]["TotalCount"])`
  - `PageCount = ((DataCount - 1) / PageRowCount) + 1`

### 範例

```csharp
var page = clsPostgreDB.getListPagination(
    "SELECT Id, SystemTime FROM SYS_ttLog WHERE SystemTime >= @sdate AND SystemTime < @edate",
    new { sdate = "2026-01-01", edate = "2026-02-01" },
    PageRowCount: 10,
    NowPageNum: 1,
    KeyColumn: "SystemTime",
    Ord: clsPostgreDB.enumOrder.DESC
);
```

## 寫入/更新（Non-Query）

### `execNonQuery(string strSql, object Datas, int? commandTimeout = null, CommandType? CommandType = CommandType.Text)`

使用 Dapper `Execute(...)` 執行 `INSERT/UPDATE/DELETE` 等不需回傳資料列的 SQL。支援單一命令內以 `;` 分隔多個語句（Npgsql 預設支援），例如 `TaskDeleteLog` 一次執行兩個 `DELETE`。

## 輔助方法

### `ConvertStringProperties(object Datas)`

將 `Datas` 物件轉為 `DynamicParameters`。PostgreSQL 的 `text`/`varchar` 一律為 UTF-8，沒有 SQL Server `varchar`（ANSI）/`nvarchar`（Unicode）的區分，因此不像 `clsDapper.ConvertStringProperties` 需要用 `DbString.IsAnsi` 特別標註，字串屬性直接以一般參數帶入。保留此方法僅為與 `clsDapper` 呼叫介面一致。

### `ToDataTable(List<Dictionary<string, object>> dt)`

透過 JSON 序列化/反序列化將 `List<Dictionary<string, object>>` 轉成 `System.Data.DataTable`（與資料庫引擎無關的純 C# 邏輯，行為與 `clsDapper.ToDataTable` 相同）。

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
var rows = clsPostgreDB.getList(
    "SELECT * FROM SYS_tbMenu WHERE Id = @id",
    new { id = 1 }
);
```

### 2) 常見 T-SQL → PostgreSQL 語法對照（本次遷移實際用到的）

| T-SQL（`clsDapper`，已停用） | PostgreSQL（`clsPostgreDB`） | 出現位置 |
|---|---|---|
| `GETDATE()` | `now()` | `clsApiBase.MenuAuth.cs`（`DoSignOut`/`GetToken`：更新 `SYS_ttToken.SignOut`） |
| `SELECT TOP 1 ...` | `SELECT ... LIMIT 1` | `clsApiBase.cs`（`GetPasswordValidationRules`）、`Program.cs`（啟動讀取 `TokenLimit`） |
| `IsActive=1` | `IsActive=true` | `clsApiBase.cs`（`GetPasswordValidationRules`，`IsActive` 為 `boolean`） |
| `DATEADD(DAY,-7,GETDATE())` | `now() - interval '7 days'` | `TaskDeleteLog.cs`（清除 7 天前的 `SYS_ttLog`/`SYS_ttToken`） |
| `SUBSTRING(TokenTime,0,9)`（依賴 T-SQL 對 start=0 的寬鬆解讀取前 8 碼） | `LEFT(TokenTime, 8)` | `TaskDeleteLog.cs`（`SYS_ttToken.TokenTime` 是 `varchar(20)` 字串，非真正 timestamp 欄位，取前 8 碼日期部分與 `to_char(now() - interval '7 days', 'YYYYMMDD')` 以文字比較） |
| `;WITH T AS (...) ... From @DT`（table variable 分頁） | `WITH T AS (呼叫端 SELECT) ... LIMIT/OFFSET` | `getListPagination` 內部實作，見上方「與 `clsDapper.getListPagination` 的差異」 |

⚠️ 上述語法轉換僅完成**程式碼層級**且已通過 `dotnet build` 編譯驗證；本機開發環境沒有可用的 PostgreSQL 執行個體，尚未實際對真正的 PostgreSQL 伺服器執行驗證。
