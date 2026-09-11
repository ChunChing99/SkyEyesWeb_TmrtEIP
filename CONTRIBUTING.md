# Contributing

本文件定義 `SkyEyesWeb_TmrtEIP` 的貢獻與開發規範，目標是讓多人協作與 AI 產碼在格式、API 習慣與文件契約上保持一致。

> AI 相關規範以 `AI_RULES.md` 為主要來源；本文件補足工作流程與提交時的最低要求。

---

## 1. 開發環境

- .NET SDK：8.x
- IDE：Visual Studio 2022
- 前端：Node.js（建議 20+）

---

## 2. 分支與提交

- 分支命名建議：
  - `feature/<topic>`
  - `bugfix/<topic>`
  - `hotfix/<topic>`
- Commit 建議：描述「做了什麼」與「為什麼」；避免只有 `fix`、`update`。

---

## 3. 程式碼風格與格式

### 3.1 `.editorconfig`

Repository 根目錄提供 `.editorconfig` 作為主要格式規範來源。

- C# / .NET：以 `.editorconfig` 規則為準。
- Markdown：需保持一致的換行、文件結構與避免不必要的格式差異。

> 若格式規則不足，請先調整 `.editorconfig`，避免只靠個人 IDE 設定造成差異。

### 3.2 AI 產碼一致性（後端）

新增/修改 Controller 時，請沿用既有模式：

- 回傳外殼：`clsResult`
- 權限檢核：`CmpSignStatus(WsMethodName, FunctionType.Xxx)`
- 例外處理：沿用既有 try/catch 模式（成功狀態進 catch 才轉 `N999`）
- 記錄：`DoAppRecLog(WsMethodName, PostData.ToJson(), Res)`

---

## 4. API 與資料存取規範（後端）

### 4.1 `clsPostData` 輸入形狀

請遵守 `docs/API_CONTRACT.md` 的 `clsPostData`/`PostData` 輸入形狀規範（一般 JSON / Base64(JSON 字串)）。

### 4.2 Dapper / `clsDapper`

- 一律使用參數化查詢，禁止以字串拼接使用者輸入。
- 分頁查詢：使用 `clsDapper.getListPagination(...)`，且 SQL 內必須建立/填入 `@DT`。
- `IN @ParamName` 清單參數：空清單不得直接進 SQL（應略過該段條件）。

### 4.3 資料表/欄位命名

新增資料表請遵守 `docs/schemas/database_schema.md` 的命名規則章節（`SYS_tb`/`SYS_tt`/`SYS_tc` 前綴、欄位不加實體前綴），避免延用舊模版 `Web_` 前綴或 `m_` 欄位前綴等未正規化的命名。

---

## 5. 文件更新要求

- 若新增/修改 API，且涉及共用輸入/輸出形狀、Header、Base64 規則等：
  - 更新 `docs/API_CONTRACT.md`
- 若新增/修改資料表：
  - 更新 `docs/schemas/database_schema.md`
- 若修改 `App_Code` 內共用類別行為（例如 `clsApiBase`/`clsDapper`/`clsResult`）：
  - 更新 `docs/server/README.md` 對應章節

---

## 6. 建置與測試

- 提交前請確保後端可建置成功。
- 若有單元測試，提交前需通過測試。

---

## 7. 安全性

- 禁止提交機密資訊（Token、密碼、可用連線字串）。
- 設定檔請保留結構與範例值，實際值使用 user-secrets / 環境變數 / 部署覆寫。
