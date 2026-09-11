# API 溝通契約 (API_CONTRACT.md)

本文件定義 `SkyEyesWeb_TmrtEIP`（Vue 3 + ASP.NET Core Web API/.NET 8）之前後端 API 溝通契約。

> 專案後端 Controller 位置：`SkyEyesWeb_TmrtEIP.Server/Controllers/`
>
> 本專案後端目前僅包含登入、選單、通用系統設定（`SystemSetting`）等 Controller；派遣業務與 Ollama AI 客服機器人相關 API 尚未建立。詳見 `docs/server/README.md`。

---

## 文件使用原則

### 可參考範本來源

撰寫或整理 API 文件時，可參考：

- `docs/templates/README.md`
- `docs/templates/controller/basic-controller.cs`

以上範本主要用途是：

- 協助辨識標準 Controller 結構
- 對齊既有 API 命名、權限驗證與回傳模式
- 提供新增 API 文件時的基本骨架參考

### API 文件輸出原則

雖可參考 `docs/templates` 內的範本，但**實際 API 文件內容仍應以後端 Controller 與 action 註解為主**。

也就是說：

- `docs/templates/*`：作為**參考範本**
- `Controllers` 原始碼內的 XML 註解、action 註解、實作內容：作為**實際輸出依據**

建議判讀優先順序如下：

1. Controller / action 註解
2. Controller 實作內容
3. 共用基底類別與 App_Code 文件
4. `docs/templates/*` 範本

### 文件整理建議

整理 API 契約時，建議以「實際行為」優先，而不是只依範本推導。特別是以下內容，應優先以程式碼與註解確認：

- 路由名稱
- 請求參數
- 權限驗證方式
- 回傳欄位名稱
- 特殊欄位（如 `Token`、`Menu`、`HeadImg`、分頁欄位等）
- 例外處理與錯誤訊息

---

## 6.z 檔案上傳/儲存規則（Base64）

後端若需將前端傳入的 Base64 內容落地存檔，預設儲存目錄規則如下：

- 預設目錄：專案執行目錄下的 `/upload/`（等同 `Directory.GetCurrentDirectory() + "/upload/"`）。
- 子目錄：依功能開設子目錄區分；Controller 呼叫 `clsApiBase.Base64ToFile(code, exFileName, Dir, setFileName)` 時，透過 `Dir` 指定子資料夾（例如 `"HeadImg"`）。
  - 當 `Dir != ""` 時，實際寫入目錄為：`/upload/{Dir}/`；若不存在會自動建立。
- 檔名：
  - 未指定 `setFileName` 時：以時間戳 `yyyyMMddHHmmssfff` 產生檔名。
  - 指定 `setFileName` 時：使用 `{setFileName}.{exFileName}`。

> 實作來源：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsApiBase.cs` 的 `clsApiBase.Base64ToFile`。


## 1. 基本資訊

### 1.1 Base URL

- 開發環境（以實際 IIS Express/Kestrel 啟動為準）：`https://localhost:{port}`
- 生產環境：依部署網址

### 1.2 Content-Type

- `application/json; charset=utf-8`

---

## 1.2.a 請求資料封裝（`clsPostData` / `PostData` 輸入形狀）

本專案多數 API action 會以 `clsPostData` 作為參數模型（定義於 `SkyEyesWeb_TmrtEIP.Server/App_Code/clsApiBase.cs`），其結構為：

- request body 最外層為一個 JSON object
- 其中包含一個 key：`PostData`
- `PostData` 的值可為：
  1) 一般 JSON（object）
  2) 將「參數 JSON 字串」做 Base64 後的字串

後端解析邏輯由 `clsPostData.ToHashtable()` 負責，其行為重點：

- 若 `PostData` 內容可視為 JSON（或 URL encode 後的 JSON 字串），則會先 `Uri.UnescapeDataString(...)` 再反序列化成 `Hashtable`
- 若上述反序列化失敗，會嘗試將 `PostData` 視為 Base64 字串，先 base64 decode → `Uri.UnescapeDataString(...)` → 反序列化成 `Hashtable`

### 1.2.a.1 形態 1：`PostData` 為一般 JSON（建議）

```json
{
  "PostData": {
    "key1": "value1",
    "key2": 123,
    "PageRowCount": 10,
    "NowPageNum": 1
  }
}
```

### 1.2.a.2 形態 2：`PostData` 為 Base64(JSON 字串)

範例概念：先將 `{"key1":"value1"}` 轉成字串，再做 Base64，放入 `PostData`。

```json
{
  "PostData": "ZXlKcmVYRXhJam9pZG1Gc2RXVXhJaXdpYTJWNUlqbzBNVDA9"
}
```

> 注意：此 Base64 字串僅為示意；實際值以轉換結果為準。

### 1.2.a.3 備註

- 多數 Controller 會直接呼叫 `PostData.ToHashtable()` 後從 `Hashtable` 取值（例如 `HT["sdate"]`），因此前端需確保送入的內容能被正確解出為 JSON object。
- `ToHashtable()` 解析失敗時會回傳 `null`（例外被吞掉），呼叫端若未做 null 防護可能導致後續例外。

### 1.3 時間格式

- 若 API 回傳/接收含時間欄位，建議使用 ISO 8601（例：`2026-01-31T12:34:56+08:00`）。

---

## 2. 認證與授權

### 2.1 Token 取得（系統 Web Token）

本系統的 Web Token 由後端 `SkyEyesWeb_TmrtEIP.Server/App_Code/clsToken.cs` 產生與驗證（`TokenModels.TokenManager`）。

- Token 內容：`access_token` 由 `iv.encrypt.signature` 以 `.` 串接
- 有效時間：由 `User.kenLimit`（分鐘）決定，實際回傳為 `expires_in`（秒）

#### 2.1.1 搭配 API 呼叫方式

後續需授權的 API，建議使用 HTTP Header：

- `Authorization: {access_token}`

> 實際驗證方式與登入 API 路由，請以 `Controllers` 內實作為準並補齊於 6.x 章節。

### 2.2 Web 端登入/授權（待補）

專案內存在 Token/登入相關資料表（`SYS_ttToken`、`SYS_ttLog`；規劃中的 `SYS_ttLoginLog` 尚未建立，見 `docs/schemas/database_schema.md`），實際 Web API 的登入流程與 Header 規格請以 `Controllers` 現況補齊於下列章節。

---

## 3. 共用回傳格式（clsResult）

本專案後端常用回傳封裝為 `clsResult`（`SkyEyesWeb_TmrtEIP.Server/App_Code/clsResult.cs`）。
如無特殊說明，各 API 預期至少包含以下欄位：

- `Result`：`true/false`
- `ReturnNumber`：訊息代碼（`clsResult.MsgNumber`，例如 `N000`）
- `ReturnMsg`：訊息內容（例如：`正常`、`未登入或登入逾時`）

> 若特定 API 另有回傳資料（例如清單、明細），通常會在同一 JSON 內額外加上資料欄位（欄位名稱依各 Controller 實作）。

### 3.1 成功回傳範例（對應 MsgNumber: N000）

```json
{
  "Result": true,
  "ReturnNumber": "N000",
  "ReturnMsg": "正常"
}
```

### 3.2 失敗回傳範例（未登入或登入逾時，對應 MsgNumber: N003）

```json
{
  "Result": false,
  "ReturnNumber": "N003",
  "ReturnMsg": "未登入或登入逾時"
}
```

### 3.3 常用 ReturnNumber 對照（節錄）

| ReturnNumber | 意義 | Status Codes |
|---|---|
| `N000` | 正常 | 200 |
| `N001` | 驗證碼錯誤 | 400 |
| `N002` | 登入失敗 | 400 |
| `N003` | 未登入或登入逾時 | 401 |
| `N004` | 無權限進行操作 | 403 |
| `N005` | 無權限進行此頁 | 403 |
| `N099` | 無效或逾時Token | 401 |
| `N999` | 未預期錯誤 | 500 |

---

## 3.a `clsResult` 擴充欄位最小範例（可操作範本）

本專案後端以 `clsResult` 作為統一回傳外殼，並依 API 需求在同一個 JSON 內「額外加上資料欄位」。

為了讓前後端與 AI 產碼能有一致的最小依循，以下提供幾個「常見回傳形狀」的範例。**商業欄位/資料內容仍以各 Controller action 的註解與實作為準**。

> 欄位命名慣例（依現況歸納）：
>
> - 基本欄位固定：`Result`、`ReturnNumber`、`ReturnMsg`
> - 資料載體常見鍵名：`data`（小寫，常用於清單/查詢結果）
> - 特定用途/功能性欄位：多用 `PascalCase`（例如 `Menu`、`Token`、`HeadImg`、`Token_Time`）

### 3.a.1 成功但不含資料（最小）

```json
{
  "Result": true,
  "ReturnNumber": "N000",
  "ReturnMsg": "正常"
}
```

### 3.a.2 失敗（以未登入為例）

```json
{
  "Result": false,
  "ReturnNumber": "N003",
  "ReturnMsg": "未登入或登入逾時"
}
```

### 3.a.3 回傳單一物件（以 `data` 承載）

適用：取單筆明細、取得設定、取得狀態等。

```json
{
  "Result": true,
  "ReturnNumber": "N000",
  "ReturnMsg": "正常",
  "data": {
    "UserGroup_id": 1,
    "PerGroupName": "系統管理員"
  }
}
```

### 3.a.4 回傳清單（以 `data` 承載 array）

適用：下拉選單、列表資料等（例如 `clsApiBase.GetPasswordValidationRules` 會以 `Res["data"] = dt` 回傳）。

```json
{
  "Result": true,
  "ReturnNumber": "N000",
  "ReturnMsg": "正常",
  "data": [
    {
      "UserGroup_id": 1,
      "PerGroupName": "系統管理員"
    },
    {
      "UserGroup_id": 2,
      "PerGroupName": "一般使用者"
    }
  ]
}
```

### 3.a.5 回傳功能性欄位（例如 Token / Menu / HeadImg）

適用：登入、刷新 Token、取得選單、認證檢核等。鍵名依實作通常為 `PascalCase`。

```json
{
  "Result": true,
  "ReturnNumber": "N000",
  "ReturnMsg": "正常",
  "Token": "iv.encrypt.signature",
  "Token_Time": "202604011234567890",
  "HeadImg": "<base64>",
  "Menu": [
    {
      "ParentId": 0,
      "Id": 1,
      "Name": "系統設定",
      "Path": null,
      "Type": "folder",
      "target": "_self",
      "Icon": "pi pi-cog",
      "Enabled": true,
      "subItems": [
        {
          "Name": "密碼設定",
          "Path": "/SystemSetting/Pwd",
          "target": "_self",
          "Icon": "pi pi-lock",
          "Enabled": true,
          "Visible": true,
          "Global": false
        }
      ]
    }
  ]
}
```

> 欄位對應資料表 `SYS_tbMenu`（詳見 `docs/schemas/database_schema.md`），已改為不加 `m_` 前綴的欄位命名。
>
> ⚠️ **待補**：`App_Code/clsApiBase.cs` 的 `Menu` getter 現已改用 `SYS_tbMenu`/`ParentId`/`Id`/`Ord`/`Type`/`Enabled` 等正式欄位名稱，回傳形狀與上方範例一致。但因 `SYS_tbUser`、`SYS_tbGroup`、`SYS_tbMenuGroupList` 尚未建立，目前 `Enabled` 僅依 `SYS_tbMenu` 自身欄位判斷，尚未套用依角色的頁面/功能權限與密碼到期狀態過濾；`fun_Search`/`fun_Insert`/`fun_Update`/`fun_Delete`/`fun_Download` 欄位亦未回傳。待使用者/群組權限資料表建立後需補回此邏輯。

### 3.a.6 分頁回傳（搭配 `clsPostgreDB.getListPagination`）

適用：清單型 API 需要分頁時。

本專案常見作法為：`clsResult` 外層固定，並額外回傳 `PaginationInfo` 物件（名稱依各 Controller 實作；本文件以 `PaginationInfo` 為範例）。

```json
{
  "Result": true,
  "ReturnNumber": "N000",
  "ReturnMsg": "正常",
  "PaginationInfo": {
    "Result": true,
    "DataCount": 128,
    "PageCount": 13,
    "Data": [
      {
        "Id": 128,
        "Api": "Login/DoSignIn",
        "TotalCount": 128
      }
    ]
  }
}
```

> `clsPostgreDB.getListPagination` 用 `COUNT(*) OVER()` 產生 `TotalCount`，不像舊版 `clsDapper`（已停用）會額外附加 `RowNo` 欄位，故範例已移除 `RowNo`。詳見 `docs/server/clsPostgreDB.md`。

---

## 4. HTTP 狀態碼規範

- `200 OK`：成功並回傳資料
- `201 Created`：成功建立（若有）
- `204 No Content`：成功但不回傳內容
- `400 Bad Request`：參數錯誤/驗證失敗
- `401 Unauthorized`：未登入/Token 無效
- `403 Forbidden`：已登入但權限不足
- `404 Not Found`：資源不存在
- `409 Conflict`：資料衝突（例如重複建立）
- `500 Internal Server Error`：未預期錯誤

> 實際狀態碼仍以 Controller 內的實作為準；此處為文件規範。

---

## 5. API 命名與路由約定

依 `README.md` 描述：

- Controller 主要集中於 `SkyEyesWeb_TmrtEIP.Server/Controllers/` 並依功能分子資料夾
- 命名常見：`{子資料夾名稱}_{網頁名稱}Controller`（例如 `SystemSetting_UserController`）
- 少數入口/通用 Controller：`{功能名稱}Controller`（例如 `LoginController`）
- 路由以 Controller 上的 `Route` / Action 上的 `HttpGet` / `HttpPost` 標記為準

---

## 5.a App_Code 共用方法索引（後端）

### 5.a.0 文件索引（建議必讀）

以下為本專案後端共用基礎類別之詳細文件（位於 `docs/server/`），撰寫/維護 Controller 時建議優先閱讀：

- `clsApiBase`：`docs/server/clsApiBase.md`
- `clsPostgreDB`：`docs/server/clsPostgreDB.md`（實際使用中；PostgreSQL）
- `clsDapper`：`docs/server/clsDapper.md`（⚠️ 已停用，SQL Server，僅保留參考）
- `clsResult`：`docs/server/clsResult.md`

本專案後端大量共用邏輯集中於 `SkyEyesWeb_TmrtEIP.Server/App_Code/`。新增/修改 API 時，優先沿用既有共用方法，避免在各 `Controller` 重複貼 SQL 或重複實作通用處理。

> 下列僅列出「常用入口/方法」作為索引；實際參數與行為請以原始碼為準。

### `clsPostgreDB`（Dapper／PostgreSQL 資料存取共用）

檔案：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsPostgreDB.cs`

本類別封裝 Dapper 的查詢與分頁查詢，底層透過 `Npgsql` 連線 PostgreSQL，常用於 Controller 直接撰寫 SQL。**是目前專案實際使用的資料存取類別**（取代已停用的 `clsDapper`）。

- 詳細契約與使用方式：`docs/server/clsPostgreDB.md`
- 注意：`getListPagination` 直接把呼叫端傳入的 SELECT 語句包成 CTE 分頁（不需要、也沒有 `@DT` 這種 T-SQL table variable 機制）；且 `KeyColumn`/`Ord` 會以字串串入 SQL，需限制可用欄位避免 SQL Injection。

### `clsResult`（API 共用回傳封裝）

檔案：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsResult.cs`

本類別為 API 統一回傳外殼（`Hashtable`），並透過 `MsgNumber` 設定 `Result/ReturnNumber/ReturnMsg`。

- 詳細契約與使用模式：`docs/server/clsResult.md`

### `clsToken`（Web Token 產生/驗證）

檔案：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsToken.cs`

- `new TokenModels.TokenManager().Create(user)`：產生 Token（`access_token`）
- `new TokenModels.TokenManager().GetUser(token)`：解析 `Authorization` Token 並取得 `User`（無效/過期回傳 `null`）

### `clsConfig`（環境設定 / 對外 HTTP 呼叫）

檔案：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsConfig.cs`

- `clsConfig.ConnectionStrings`：服務啟動時設定的解密後連線字串
- `clsConfig.GetHttpClient(url, parameters = null, header = null)`：同步呼叫（內部包裝 `Async`；避免在長流程擴散阻塞呼叫）
- `clsConfig.GetHttpClientAsync(url, parameters = null, header = null)`：非同步呼叫（建議優先使用）

### `clsApiBase`（Controller 共用基底/工具）

檔案：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsApiBase.cs`

本類別為多數 Controller 的共用基底，包含使用者解析、選單權限、log、Base64 檔案處理與登入/授權檢核。

- 詳細契約、Header 依賴與權限判斷規則：`docs/server/clsApiBase.md`

### `clsExcel`（Excel 匯出共用，NPOI）

檔案：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsExcel.cs`

- `DoDataTableToExcel(models)`：回傳 `MemoryStream`
- `DoDataTableToFile(models, setFileName = "")`：回傳檔案路徑
- `DoDataTableToBase64(models)`：回傳 Base64 字串

---

## 6. API 清單（依實際 Controller 為主）

實際 Controller 檔案清單，請參照實際 Controller 目錄（`SkyEyesWeb_TmrtEIP.Server/Controllers/` 及子資料夾）。

- 各 Controller 內的 API method/路由/參數/回傳內容：**以 Controller 原始碼內的註解與標記為準**
- 本文件不在此逐一展開每個 method 的細節（避免與程式碼不同步）

---

## 6.x 分頁處理方式（後端 Dapper／PostgreSQL 共用）

本專案部分清單型 API 會使用後端共用函式 `SkyEyesWeb_TmrtEIP.Server/App_Code/clsPostgreDB.cs` 的 `getListPagination(...)` 進行分頁查詢，回傳 `PaginationInfo`：

- `Result`：是否成功
- `DataCount`：總筆數（TotalCount）
- `PageCount`：總頁數
- `Data`：本頁資料（`List<Dictionary<string, object>>`，包含分頁欄位）

### 6.x.1 呼叫參數（概念）

- `strSql`：一般的 SELECT 查詢 SQL（**不需要**任何暫存資料集/table variable，本方法會自動把整段 SQL 包成 CTE）
- `Datas`：原查詢參數物件
- `PageRowCount`：每頁筆數（對應 `@pageSize`）
- `NowPageNum`：頁碼（用於計算 `@pageOffset = (NowPageNum - 1) * PageRowCount`，1-based）
- `KeyColumn`：排序欄位（會被直接串入 SQL 的 `ORDER BY {KeyColumn}`）
- `Ord`：排序方向（`ASC`/`DESC`）
- `commandTimeout`：逾時秒數（預設 `clsPostgreDB.timeoutInSeconds = 30`）

### 6.x.2 實作方式（CTE + LIMIT/OFFSET 分頁）

`getListPagination` 會把傳入的 `strSql` 整段包成以下分頁 SQL：

```sql
WITH T AS (
    -- 呼叫端傳入的 strSql
)
SELECT *, COUNT(*) OVER() AS TotalCount
FROM T
ORDER BY {KeyColumn} {Ord}
LIMIT @pageSize OFFSET @pageOffset;
```

- 用 window function `COUNT(*) OVER()` 一次算出總筆數 `TotalCount`（每列相同）
- 用 PostgreSQL 慣用的 `LIMIT`/`OFFSET` 取出指定頁範圍（取代舊版 T-SQL 的 `ROW_NUMBER() BETWEEN ...`）

分頁參數會以 Dapper `DynamicParameters` 合併：

- `@pageSize = PageRowCount`
- `@pageOffset = (NowPageNum - 1) * PageRowCount`

### 6.x.3 回傳資料結構與注意事項

- `Data` 每一列都會包含：
  - 原始欄位（來自呼叫端的 `strSql`）
  - `TotalCount`（同頁每列相同）
- `PageCount` 計算方式：`((TotalCount - 1) / PageRowCount) + 1`
- `KeyColumn` / `Ord` 會以字串插入 SQL（非參數化），Controller 呼叫端需自行限制可用欄位，避免 SQL Injection。

> 對應程式：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsPostgreDB.cs` 之 `getListPagination`。舊版 T-SQL（`ROW_NUMBER()` + `@DT` table variable）實作見已停用的 `clsDapper.cs`／`docs/server/clsDapper.md`。

---

## 6.y Excel 匯出處理方式（後端 NPOI 共用）

本專案部分 API 會使用後端共用類別 `SkyEyesWeb_TmrtEIP.Server/App_Code/clsExcel.cs` 的 `clsExcel` 將 `DataTable` 匯出為 Excel（`.xlsx`）。底層使用 NPOI `XSSFWorkbook` 產生活頁簿。

### 6.y.1 輸入模型（clsDtToExcelModel）

匯出方法皆以 `clsDtToExcelModel[] models` 作為輸入（可一次產多個 Sheet）。每個 model 主要欄位：

- `DataTable`：資料來源
- `ColKey`：要輸出的欄位 key（對應 `DataTable` 欄位名）
- `ColName`：表頭顯示文字（與 `ColKey` 一一對應）
- `arrWidth`：欄寬（未提供時會預設每欄 25）
- `Note`：備註列（會寫在最上方，每筆備註佔一列）
- `Merged`：合併儲存格設定（字串陣列，每筆格式為 `rowFrom,rowTo,colFrom,colTo`）
- `Height`：列高（預設 `15 * 20`）
- `SheetName`：Sheet 名稱（未提供則使用預設）

### 6.y.2 產出方式（3 種）

`clsExcel` 提供三種匯出方法（皆輸出 `.xlsx`）：

1. `DoDataTableToExcel(models)`
   - 回傳：`MemoryStream`
   - 實作重點：會先寫入專案目錄下 `temp` 資料夾的暫存檔，再讀回成 `MemoryStream` 後刪除暫存檔。

2. `DoDataTableToFile(models, setFileName = "")`
   - 回傳：`string`（檔案完整路徑）
   - 實作重點：輸出到 `Directory.GetCurrentDirectory() + "\\temp\\"`，若成功建立檔案則回傳路徑，否則回傳空字串。

3. `DoDataTableToBase64(models)`
   - 回傳：`string`（Base64）
   - 實作重點：直接寫入 `MemoryStream` 後 `Convert.ToBase64String(bytes)`。

### 6.y.3 儲存格格式（程式既定）

- 字型：微軟正黑體
- 表頭：14px、粗體、置中、灰底
- 內容列：12px、置中
- Note：靠左
- 型別輸出：
  - `DateTime`：`yyyy-MM-dd HH:mm:ss`
  - 數值型別：以 `double` 輸出
  - 其他：以字串輸出

### 6.y.4 API 回傳建議（契約層說明）

實際各匯出 API 的路由與回傳格式以 Controller 實作為準；若需定義前後端契約，常見做法：

- 下載檔：API 回傳檔案串流（`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`）
- Base64：API 回傳 JSON（例如 `{ base64: "..." }`），前端自行轉檔下載
- 檔案路徑：通常不建議直接暴露實體路徑給前端；若實作採用檔案路徑，建議再提供下載端點以檔名/Token 取檔。

> 對應程式：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsExcel.cs` 之 `clsExcel`（`DoDataTableToExcel` / `DoDataTableToFile` / `DoDataTableToBase64`）。

## 7. 與資料庫 Schema 對應（摘要）

本專案主要資料表請參考：`database_schema.md`。

---

## 8. 變更紀錄

| 版本 | 日期 | 說明 |
|---|---|---|
| 0.1 | 2026-03-31 | 建立文件骨架（依專案架構整理） |
