# Server（ASP.NET Core Web API / .NET 8）

本文件描述後端專案 `SkyEyesWeb_TmrtEIP.Server/` 的開發、設定與執行重點。

> 整體專案概述與前後端整合啟動方式請參考根目錄 `README.md`。

## 專案資訊

- 專案目錄：`SkyEyesWeb_TmrtEIP.Server/`
- Target Framework：`.NET 8`（`net8.0`）
- 類型：ASP.NET Core Web API（含 MVC/Controllers，並提供 SPA 靜態檔案與 fallback）
- 常見套件：Dapper、`Npgsql`、`Newtonsoft.Json`、NPOI、`System.Drawing.Common`、Swashbuckle（Swagger）（以實際 `csproj` 為準；`Microsoft.Data.SqlClient` 仍留在 `csproj` 中，但已無任何程式碼路徑使用，詳見下方）

## 資料庫引擎（PostgreSQL）

專案資料庫引擎已由 SQL Server 改為 **PostgreSQL**，且**查詢執行層已完成遷移**：

- 所有實際會被呼叫的資料庫查詢/更新，已從 `clsDapper`（SQL Server／`Microsoft.Data.SqlClient`）改為 `App_Code/clsPostgreDB.cs`（PostgreSQL／`Npgsql`）：`App_Code/clsApiBase.cs`、`App_Code/clsApiBase.MenuAuth.cs`、`BackgroundWorker/Tasks/TaskDeleteLog.cs`、`Program.cs`（啟動時讀取 `SYS_tbPasswordValidationRules`）皆已改用 `clsPostgreDB.*`。
- 既有 SQL 已從 T-SQL 語法改寫為 PostgreSQL 語法：`GETDATE()` → `now()`、`SELECT TOP N` → `LIMIT N`、`IsActive=1` → `IsActive=true`、`DATEADD(DAY,-7,GETDATE())` → `now() - interval '7 days'`（`TaskDeleteLog` 清除過期 log 的日期比較）。
- `Program.cs` 啟動時設定的是 `clsPostgreDB.ConnectionStrings`（而非 `clsDapper.ConnectionStrings`）；`appsettings.json` 的 `ConnectionStrings:DefaultConnectionString`/`DebugConnectionString` 解密後**必須是 Npgsql 連線字串格式**（例如 `Host=...;Port=5432;Database=...;Username=...;Password=...;`），而非原本的 SQL Server 格式。目前該值仍是佔位符 `__REPLACE_ME__`，尚未實際測試連線一台真正的 PostgreSQL 伺服器。
- `App_Code/clsPostgreDB.cs` 的分頁方法 `getListPagination` 呼叫慣例與 `clsDapper` 不同：不需要（也無法）像 T-SQL 那樣先 `DECLARE @DT Table` 再 INSERT，改為直接把呼叫端傳入的 SELECT 語句包成 CTE，並用 `LIMIT`/`OFFSET` 分頁，詳見檔案內 XML 註解與 `docs/server/clsPostgreDB.md`。

⚠️ **保留但不再使用**：`App_Code/clsDapper.cs`（SQL Server 實作）與 `.csproj` 的 `Microsoft.Data.SqlClient` 套件參考**仍保留在專案中**（未刪除，因本機專案目前非 git 版控，刪除後難以復原，故未在未經確認下移除），但已無任何程式碼呼叫，屬於死碼；如確認不再需要，可另行清除。詳見 `docs/server/clsDapper.md` 開頭的停用說明。

`docs/schemas/database_schema.md` 描述的是 PostgreSQL 目標 Schema，與目前 `clsPostgreDB.cs` 產生/預期的 SQL 相符；但**尚未在真正的 PostgreSQL 伺服器上驗證過**（本機開發環境無可用的 PostgreSQL 執行個體），僅完成程式碼層級的語法轉換與編譯驗證（`dotnet build` 通過）。

## 沿革與現況範圍

本專案後端的共用基礎設施（登入、選單、權限等）是以既有模版專案挑選其中與業務無關的通用部分搬移而來，不含派遣業務（訂單/派車/任務追蹤）與 Ollama AI 客服機器人相關程式碼。目前已搬移/建立的部分：

- `App_Code/clsApiBase.cs`：Controller 共用基底，提供登入狀態檢核、選單/權限查詢、Log 記錄、Base64 檔案處理等通用端點。**已移除**原模版內的派遣業務端點（`GetTaskStatus`/`GetAirportStation`/`GetCarType`/`GetOrderStatus`）與整個 Ollama 聊天機器人區塊（`#region CLI`）；SQL 已改用實際存在的 `SYS_tbMenu`/`SYS_ttToken`/`SYS_ttLog`（詳見 `docs/schemas/database_schema.md`）。`DoUpdatePw`、`GetGroupType` 因完全依賴尚未建立的 `SYS_tbUser`/`Web_Group` 已移除；`Menu`/`CmpSignStatus(string, FunctionType)` 中依角色的頁面/操作權限判斷（`fun_*` 欄位）暫未套用，詳見 `docs/server/clsApiBase.md`。
- `App_Code/clsConfig.cs`：僅保留 `ConnectionStrings`/`TokenLimit`/`GetHttpClient(Async)`，已移除派遣系統/易圖 Token 相關屬性與方法。
- `App_Code/clsDapper.cs`、`clsResult.cs`、`clsExcel.cs`、`clsToken.cs`、`App_Code/code64/`：原封不動搬移。
- `Controllers/BaseController.cs`：原封不動搬移（僅改命名空間）。
- `Controllers/LoginController.cs`：`GetVerificationCode`（驗證碼）保留可用；`DoSignIn` 因完全依賴尚未建立的 `SYS_tbUser`/`SYS_ttLoginLog` 已移除，待該等資料表建立後需重新實作（見檔案內 TODO 註解）。
- `Controllers/SystemSetting/*.cs`：僅保留 `Parameters`（密碼規則設定，對應 `SYS_tbPasswordValidationRules`）與 `Pwd`（空殼，僅繼承 `clsApiBase` 共用端點）。原模版的 `Corp`/`Dept`/`Permission`/`User` 四個 Controller 因完全依賴不存在或未於本文件規劃的資料表（`SYS_tbCorp`/`SYS_tbDept`/`Web_Group`/`Web_MenuGroupList`/`SYS_tbUser`），且帶有其他專案的殘留命名空間（`Hwaseng`/`SkyEyesWeb_Baogang`），已整份移除。
- `Options/ConnectionStringsOptions.cs`：搬移。
- `BackgroundWorker/`（`BackgroundWorker.cs`/`SchedulerModels.cs`/`Tasks/*.cs`）與 `BackGroundSettings.json`：背景排程服務框架（`IHostedService`，每分鐘輪詢一次 `Tasks` 清單，依 `TaskName` 反射呼叫對應方法），僅保留通用的 `TaskDeleteLog`（清除 7 天前的檔案 log 與 `SYS_ttLog`/`SYS_ttToken` 資料庫紀錄）。**預設 `Enabled: false`**，需要時自行在 `BackGroundSettings.json` 開啟。

**尚未搬移/建立**（中捷業務範圍內若有需要，需另行規劃）：

- 舊模版的派遣業務 Controller（`Dispatch`/`Ground`/`Order`/`Report`/`Statistics`/`Task`）與根目錄 `AppController.cs`/`TaskController.cs`（外部系統整合端點）。
- 舊模版 `BackgroundWorker` 內綁定派遣業務 SQL 的排程任務（`TaskDdataxaiOrderSearch`/`TaskDdataxaiDronesStatus`，呼叫易圖 API 並更新 `TMS_ttTask_*`/`SYS_tbCar`），未搬移。
- Ollama AI 客服機器人（`clsLlama.cs`/`clsSkillBag.cs`/`Skills/*.skill.md`/`CustomerService.json`/`OllamaSettings`）。
- `SYS_tbUser`、`Web_Group`（規劃新名 `SYS_tbGroup`）、`Web_MenuGroupList`（規劃新名 `SYS_tbMenuGroupList`）、`SYS_ttLoginLog` 等資料表尚未建立，登入（`LoginController.DoSignIn`）、使用者管理、群組權限管理相關功能因此暫未實作（詳見 `docs/schemas/database_schema.md` 備註）。

## 執行與開發

### 使用 Visual Studio

- 將啟動專案設為 `SkyEyesWeb_TmrtEIP.Server`
- 直接 F5 / Ctrl+F5（開發環境預設會開啟 `/swagger`）

### 使用 CLI

於 repository 根目錄執行：

```bash
dotnet run --project SkyEyesWeb_TmrtEIP.Server
```

## 設定檔（Configuration）

### `appsettings.json`

- `ConnectionStrings:DefaultConnectionString` / `DebugConnectionString`：資料庫連線字串（加密字串）。**目前為佔位符 `"__REPLACE_ME__"`**，需換成中捷資料庫的加密連線字串後才能啟動；未設定會在啟動階段丟出例外，並寫入 `startup_error.txt`。

### `appsettings.Development.json`

開發環境覆寫設定（例如記錄層級）。

## 資料庫連線字串（加解密流程）

`appsettings.json` 的 `ConnectionStrings:DefaultConnectionString` 為加密字串。
啟動時會透過 `clsCode64.DeCode(...)` 解密後指定到：

- `clsConfig.ConnectionStrings`
- `clsPostgreDB.ConnectionStrings`（實際查詢執行使用；`clsDapper.ConnectionStrings` 已不再被賦值/使用，詳見上方「資料庫引擎（PostgreSQL）」）

若遇到 DB 連線失敗，建議依下列順序檢查：

1. `DefaultConnectionString` 是否已換成實際加密值（而非 `__REPLACE_ME__`）
2. 解密方法是否可正常運作（環境差異、字串格式）
3. 解密後的 connection string 是否為 **Npgsql 連線字串格式**（例如 `Host=...;Port=5432;Database=...;Username=...;Password=...;`），而非舊的 SQL Server 格式（`Server=...;User Id=...`）——格式錯誤會在 `NpgsqlConnection` 建立/開啟連線時失敗
4. PostgreSQL 服務網路/帳密/權限（防火牆、`pg_hba.conf` 允許的連線來源、使用者密碼）

## 啟動管線與 SPA 行為（概要）

後端同時負責：

- API Controllers（路由以各 Controller 的 `Route` / `HttpGet` / `HttpPost` 為準）
- 靜態檔案：`UseStaticFiles()`
- SPA fallback：`MapFallbackToFile("/index.html")`（提供 SPA 的前端路由支援）
- 開發環境 Swagger UI（`/swagger`）

開發模式下，專案設定了 SPA Proxy，可將前端 Vite dev server 整合到同一個啟動流程（細節見根目錄 `README.md`）。

## 背景服務（Hosted Service）

專案註冊 `BackgroundWorker`（`IHostedService`），並以 `BackGroundSettings.json` 的排程設定進行綁定。

- 啟動時載入 `BackGroundSettings.json`（`optional: false`，檔案不存在會啟動失敗），並將其內容整份綁定到 `SchedulerSettings`。
- `BackgroundWorker` 內建一個 1 分鐘週期的 `Timer`，逐一檢查 `Tasks` 清單，時間到就以反射呼叫與 `TaskName` 同名的方法。
- 目前只有一個通用任務 `TaskDeleteLog`：清除 7 天前的檔案 log（`log/{yyyyMMdd}/`）與 `SYS_ttLog`/`SYS_ttToken` 資料庫紀錄。
- 執行結果（`LastRunTime`/`LastRunResult`/`NextRunTime`）會寫回 `BackGroundSettings.json`；`Forced_Execution_Task` 可用來強制立即執行指定任務（下一輪 Timer 觸發時執行，執行後自動清空）。
- **`BackGroundSettings.json` 的 `Enabled` 預設為 `false`**，需要啟用排程時自行改為 `true`。

常見排查方向：

- 設定檔是否被正確載入（檔名/路徑/`CopyToOutputDirectory`）
- 設定綁定是否成功（欄位名稱/型別需與 `SchedulerSettings`/`SchedulerTask` 一致）
- `BackGroundSettings.json` 的 `Enabled` 是否為 `true`
- 執行時例外是否有記錄到 log（`ILogger`/`log/` 目錄）

## CORS

目前 `Program.cs` **未註冊 CORS**（前後端預期同網域部署，開發模式透過 Vite dev server proxy 轉發 API，不需要跨網域）。若未來前後端需分開部署在不同網域，再依需求加上 `AddCors`/`UseCors`。

## 常見問題（Troubleshooting）

### 啟動時拋出例外並寫入 `startup_error.txt`

最常見原因是 `ConnectionStrings:DefaultConnectionString` 還是佔位符 `__REPLACE_ME__`，或加密字串格式錯誤導致 `clsCode64.DeCode` 失敗。

### 404 但 SPA 頁面未顯示

確認：

- 有無 `MapFallbackToFile("/index.html")`
- `index.html` 是否存在於後端正在提供的靜態檔案目錄

### Swagger / API 測試

開發環境可透過 `/swagger` 測試（`Program.cs` 已預設啟用 `AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI`）。

## 相關文件

- API 契約：`docs/API_CONTRACT.md`
- DB Schema：`docs/schemas/database_schema.md`
- 資料存取共用類別（PostgreSQL，使用中）：`docs/server/clsPostgreDB.md`
- 資料存取共用類別（SQL Server，已停用）：`docs/server/clsDapper.md`
