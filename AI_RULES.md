# AI 開發準則 (AI_RULES.md)

> **Context**: 本專案遵循 [.NET 8 / C# 12] 開發規範。
> **Goal**: AI Agent 在生成、重構或解釋代碼時，必須嚴格遵守以下準則。

> **Scope**:
> - 後端：`SkyEyesWeb_TmrtEIP.Server`（ASP.NET Core Web API）
> - 前端：`skyeyesweb_tmrteip.client`（TypeScript，規劃以 PrimeVue + TailwindCSS 為基礎）
> - 設定：`appsettings.json` 、 `App_Code/clsConfig.cs`（請避免破壞既有讀取方式）

---

## 1. 程式碼風格與命名規範 (Coding Style)
- **語言**: 使用 C# 強型別開發，避免使用 `dynamic` 或過度使用 `var`（除非型別顯而易見）。
- **命名**: 
    - 類別/方法使用 `PascalCase`。
    - 私有欄位使用 `_camelCase`。
    - 介面必須以 `I` 開頭（例如 `IRepository`）。
- **非同步**:
    - 原則：所有非同步方法名稱必須以 `Async` 結尾，並使用 `await` 實作（避免 `.Result`、`.Wait()`、`GetAwaiter().GetResult()` 造成 thread blocking / deadlock 風險）。
    - **相容性例外（本專案現況）**：若既有程式碼為了相容舊呼叫點而保留「同步包裝非同步」入口（sync wrapper），允許存在於邊界層（例如 legacy helper / 相容層），但必須遵守：
        - 新增/修改功能時優先提供 `Async` 版本並在可行範圍內改為「async all the way」。
        - 同步包裝應集中在單一位置，避免在業務流程或 Controller 內散落 blocking 呼叫。
        - 若必須保留 blocking 呼叫，需在 PR/變更說明中交代原因與風險。
        - **現況提醒**：目前 `App_Code/clsConfig.cs` 已存在同步包裝非同步（例如 `GetHttpClient()` 內部呼叫 `GetHttpClientAsync(...)`）。新增呼叫請優先使用 `GetHttpClientAsync(...)`，避免在 Controller/服務流程中擴散 blocking。
- **例外**: 不允許空的 `catch`；不做「吞掉例外」的處理。
- **註釋**: 複雜邏輯必須附帶 XML 文檔註釋 (`///`)，簡單方法則保持代碼自釋義。

## 2. 專案結構與分層 (Project Structure)
- **Server**: 本專案以 `Controller` 作為主要 Web API 層，直接對前端提供資料與操作介面。
    - **允許範圍**：Controller 可包含「資料查詢、結果組裝（projection）、輸入參數整理、簡單流程協調」等以回應前端需求為主的邏輯。
    - **建議邊界**：當邏輯被多處重用、流程判斷複雜、或牽涉多個外部系統/交易一致性時，優先抽離到 `Services` / `Domain`（或既有對應層）以降低 Controller 體積並提升可測試性。
    - **禁止事項**：避免在 Controller 內堆疊大量重複資料加工/重複 SQL；避免在 Controller 內硬編碼設定或敏感資訊。
- **Service/Domain**: 商業邏輯與規則集中於 `Services` 或 `Domain` 層。
- **Data Access**: 本專案後端目前以 **Dapper** 為主，資料存取請集中在可重用的方法/類別（例如既有 `clsPostgreDB`），避免在多個 action 中複製貼上 SQL。
- **Client**: TypeScript 邏輯應維持分層（API client / views / state），避免在 UI 元件內堆疊大量資料加工。

## 3. 架構與設計模式 (Architecture)
- **依賴注入**: 
    - 優先使用「建構函式注入」(Constructor Injection)。
    - 避免在類別內部實例化外部服務（不要使用 `new` 建立 Service）。
- **職責分離**: 
    - Controller 以「提供前端所需 API」為核心，保持 action 方法可讀性與可測試性。
    - 複雜或可重用規則逐步下沉到 `Services` / `Domain`。
- **資料存取**: 
    - 本專案不使用 EF Core；資料存取以 Dapper 為主。
    - 一律使用參數化查詢，避免字串串接組 SQL 以降低 SQL Injection 風險。

## 4. 設定與機密管理 (Configuration & Secrets)
- **禁止提交真實機密**: API Key、密碼、連線字串不得寫死在程式碼中，且**不得提交真實值到 Repo**。
- **相容性說明（本專案現況）**：
    - 本專案歷史/部署相容性因素下，`appsettings.json` 內**可能仍存在**敏感設定（例如 Token 的 `ID/PW`、或加密/混淆過的連線字串）。這是「現況描述」，**不代表允許長期維持**。
    - AI 在修改設定或相關程式碼時，必須同時滿足「不破壞既有讀取方式」與「逐步提升安全性」兩個目標，遵循下列原則：
        - 不移除既有的 `key/section`（避免破壞既有讀取方式與部署流程）。
        - 若需要在 Repo 內提供範例，請使用「假資料/佔位符」（例如 `"__REPLACE_ME__"`）而非真實值。
        - 若發現 repo 內含疑似真實憑證/帳密，應在變更說明中標註風險，並優先提出**移出 repo**的修改建議（見下方做法）。
        - **執行口徑（避免誤提交）**：Repo 內的 `appsettings.json` 應僅保留結構與範例值（placeholder），實際值請以 `dotnet user-secrets`、環境變數或部署環境覆寫提供；不得在 PR 中提交可用的帳密/Token。
- **設定來源**:
    - 以 `appsettings.json` / `appsettings.{Environment}.json` 為主。
    - 開發機使用 `dotnet user-secrets` 或環境變數覆寫。
- **型別化設定**: 若新增設定，優先採用 Options pattern（`IOptions<T>` / `IOptionsSnapshot<T>`），並維持與既有 `clsConfig` 的相容性（不可任意刪改既有 key/section 行為）。

### 建議做法（移出敏感資訊）
- **開發/測試環境**：使用 `dotnet user-secrets` 設定 `ConnectionStrings:DefaultConnectionString`、`ConnectionStrings:DebugConnectionString` 等。
- **部署環境**：改採環境變數或外部化設定檔覆寫（例如在 IIS/容器/K8S 以環境變數注入）。
- **Repo 內設定檔**：僅保留必要的 `section/key` 結構與範例值（例如 `"__REPLACE_ME__"`），避免提交真實帳密。

## 5. 錯誤處理與防禦性編程 (Error Handling)
- **異常處理**: 
    - 避免在一般業務流程捕獲通用 `Exception`；啟動階段/最上層例外攔截可例外。
    - 避免空緩衝 (Empty Catch Blocks)。
- **驗證**: 
    - 進入業務邏輯前，必須進行參數驗證（例如 `ArgumentNullException.ThrowIfNull`）。
    - API 回傳應使用專案既有的統一封裝格式（例如 `clsResult`；若未來導入 `ResultDTO<T>` 或其他封裝，則以專案既有慣例為準）。

## 6. API 設計與模型驗證 (API Design & Validation)
- **HTTP 規範**: 依語意使用正確狀態碼（`200/201/204/400/401/403/404/409/422/500`）。
- **DTO**:
    - 對外使用 DTO（Request/Response），避免直接暴露 EF Entity。
    - DTO 命名：`XxxRequest`、`XxxResponse`、`XxxDto`。
- **驗證框架**: 優先使用 FluentValidation；Controller 不寫複雜 if-else 驗證。
- **一致回傳**: 新增 API 必須沿用專案既有的回傳封裝格式（目前以 `clsResult` 為主）；若未來導入 `ResultDTO<T>`（或同等封裝）並完成全站推行，則改依新標準。

## 7. 資料存取與資料庫規範 (Data Access)

> **適用性**：本專案後端目前以 **Dapper** 為主；以下規範請依「實際使用的資料存取技術」套用。

### 7.1 Dapper（本專案主要做法）
- **參數化查詢**：一律使用參數化（匿名物件/動態參數），禁止字串串接組 SQL 以避免 SQL Injection。
- **回傳模型**：優先投影/組裝成 DTO 或 View Model，避免把資料表欄位原封不動外拋。
- **連線/交易**：
    - 優先沿用既有 `clsPostgreDB` 的連線管理方式。
    - 需要一致性時才使用 transaction，並確保 `commit/rollback` 路徑完整。
- **查詢效能**：避免 `SELECT *`（除非既有存量/必要相容），優先列出實際需要欄位。

## 8. 記錄、追蹤與可觀測性 (Logging & Observability)
- **日誌**: 使用既有 Serilog 設定（若已存在），避免自行建立新的 logging pipeline。
- **內容**:
    - 不記錄敏感資訊（密碼、Token、身分證字號等）。
    - 記錄必要的 correlation id / request id（若專案已有做法，請沿用）。

## 9. 單元測試要求 (Testing)
- **測試框架**: 使用 [xUnit / Moq]。
- **規則**: 
    - 每個新功能必須附帶單元測試。
    - 測試方法命名遵循 `MethodName_StateUnderTest_ExpectedBehavior` 格式。

## 10. 前端 (TypeScript) 開發規範 (Client)
- **UI 框架**: 元件庫使用 PrimeVue，版面/間距/樣式使用 TailwindCSS 撰寫；**不使用 Bootstrap**，避免多套 CSS reset/utility 系統互相衝突。新頁面請優先以 Tailwind utility class 排版，PrimeVue 元件維持原生樣式，不額外包 Bootstrap 慣用的 `input-group`/`form-control`/`row col-*` 寫法。
- **型別**: 避免濫用 `any`；優先使用明確的型別/介面、泛型、`unknown` + type guard。若因既有程式或外部套件限制必須使用 `any`，需將影響範圍最小化（集中於 adapter / API client 層），並避免向 UI/業務邏輯層擴散。
- **API 呼叫**: 集中於 API client 模組，避免在 UI 層散落 `fetch/axios`。
- **錯誤處理**: 必須處理 API 錯誤（顯示訊息/回退流程），避免無聲失敗。

## 11. AI 互動偏好 (Interaction Preferences)
- **簡明扼要**: 解釋代碼時請保持簡潔，除非我詢問細節，否則不要輸出冗長的教學。
- **最佳實踐**: 如果我的要求違反了 Clean Code 或效能原則，請主動提出警告並給出建議的修改方案。
- **安全性**: 絕對禁止將 API Key、密碼或連線字串寫死在代碼中，應提醒我使用 `user-secrets` 或環境變數。

---

## 專案特定技術清單 (Tech Stack)
- **Web (後端)**: ASP.NET Core Web API（Controllers），.NET 8
- **Web (前端)**: Vue 3 + Vite + TypeScript，PrimeVue（元件庫）+ TailwindCSS（樣式），Vue Router
- **Database**: PostgreSQL（後端主要使用 Dapper，透過 `App_Code/clsPostgreDB.cs`／`Npgsql` 連線；已停用的舊版 SQL Server 實作 `clsDapper.cs` 僅保留參考，不再被呼叫，詳見 `docs/server/README.md`。若專案已導入 EF Core 則依本準則補充規範）
- **Scripting**: PowerShell (用於系統管理與自動化)
- **Tools**:
    - 目前已使用/可見：Dapper、Newtonsoft.Json、NPOI、Swashbuckle（Swagger）
    - 若專案已導入則沿用：AutoMapper、FluentValidation、Serilog
- **不使用**: Bootstrap、Ollama/本地 LLM 相關套件（`OllamaSharp` 等 AI 客服機器人功能，本專案未使用）
