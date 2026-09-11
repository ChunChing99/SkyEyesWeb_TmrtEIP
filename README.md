# SkyEyesWeb_TmrtEIP

## 專案概述

`SkyEyesWeb_TmrtEIP`（台中捷運 EIP）為前後端分離的 Web 系統：

- 後端：ASP.NET Core Web API（.NET 8，專案：`SkyEyesWeb_TmrtEIP.Server`）
- 前端：Vue 3 + Vite（專案目錄：`skyeyesweb_tmrteip.client`，以 SPA 方式提供 `index.html`），規劃以 **PrimeVue + TailwindCSS** 為基礎開發

後端專案設定了 SPA Proxy：開發模式下可將前端開發伺服器（Vite）整合到同一個啟動流程。

## 專案沿革

本專案後端的共用基礎設施（登入、選單、權限、共用 API 回傳格式等）是挑選既有模版專案中與業務無關的通用部分搬移、正規化而來，已移除派遣業務（訂單/派車/任務追蹤）與 Ollama AI 客服機器人等本專案用不到的部分。前端是全新的 Vue 3 + Vite 骨架，已安裝 PrimeVue + TailwindCSS 基礎工具鏈與最小可運作頁面，尚無實際業務頁面。詳見 `docs/server/README.md` 與 `docs/client/README.md`。

## 文件
- 資料庫 Schema：[`docs/schemas/database_schema.md`](./docs/schemas/database_schema.md)
- API 契約：[`docs/API_CONTRACT.md`](./docs/API_CONTRACT.md)
- 後端說明：[`docs/server/README.md`](./docs/server/README.md)
- 前端說明：[`docs/client/README.md`](./docs/client/README.md)
- 貢獻/開發流程：[`CONTRIBUTING.md`](./CONTRIBUTING.md)
- Controller/頁面範本：[`docs/templates/README.md`](./docs/templates/README.md)

## 專案結構

```
SkyEyesWeb_TmrtEIP/
  README.md
  AI_RULES.md                             # AI 開發準則
  CONTRIBUTING.md                         # 貢獻/開發流程規範
  docs/                                   # 其他相關文件（例如：API 文件、部署說明等）
    API_CONTRACT.md
    schemas/                              # 資料庫 Schema 文件
      database_schema.md
    server/
      README.md
    client/
      README.md
  SkyEyesWeb_TmrtEIP.Server/               # ASP.NET Core (.NET 8)
    Program.cs
    appsettings.json
    appsettings.Development.json
    App_Code/                              # 共用基礎類別
    Controllers/                           # API Controllers
    Options/                               # 強型別設定類別
  skyeyesweb_tmrteip.client/                # Vue 3 + Vite
    package.json
    src/
    public/
    dist/                                  # 建置輸出 (若有)

> 後端/前端的詳細設定與進階說明已拆分至：
>
> - `docs/server/README.md`
> - `docs/client/README.md`

## 本機開發/啟動

### 需求

- .NET SDK 8
- Node.js（建議 20+；實際以專案前端依賴可正常安裝為準）
- PostgreSQL（資料庫 Schema 見 `docs/schemas/database_schema.md`）
  - 後端資料存取層已改用 `App_Code/clsPostgreDB.cs`（`Npgsql`），程式碼中的 SQL 已改寫為 PostgreSQL 語法。⚠️ 尚未在真正的 PostgreSQL 伺服器上驗證過（`appsettings.json` 的連線字串仍是佔位符 `__REPLACE_ME__`）。詳見 `docs/server/README.md`。

### 啟動方式

#### 方式 A：由後端啟動（使用 SPA Proxy）

1. 於 `skyeyesweb_tmrteip.client/` 安裝前端套件：`npm install`
2. 啟動後端專案 `SkyEyesWeb_TmrtEIP.Server`
3. 後端設定的 SPA Proxy 會以 `npm run dev` 啟動 Vite，並使用 `https://localhost:58488` 作為前端網址

#### 方式 B：前後端分開啟動

1. 前端：`cd skyeyesweb_tmrteip.client && npm run dev`
2. 後端：啟動 `SkyEyesWeb_TmrtEIP.Server`（預設 `https://localhost:7249`）

## 設定檔

- 後端：`SkyEyesWeb_TmrtEIP.Server/appsettings.json`
  - `ConnectionStrings:DefaultConnectionString` / `DebugConnectionString` 目前為佔位符 `"__REPLACE_ME__"`，需換成中捷資料庫的加密連線字串後才能啟動（未設定會在啟動階段丟出例外並寫入 `startup_error.txt`）。加解密流程見 `docs/server/README.md`。

## 授權

若有授權資訊請於此補上（目前 workspace 未看到明確授權宣告）。

## AI 開發準則

本專案的 AI 開發準則請以 `AI_RULES.md` 為準（單一規範來源）：

- [`AI_RULES.md`](./AI_RULES.md)

README 只保留專案概述與啟動方式，避免與 `AI_RULES.md` 重覆導致規範不一致。
