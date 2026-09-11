# Client（Vue 3 + Vite）

本文件描述前端專案 `skyeyesweb_tmrteip.client/` 的開發、設定與建置方式。

> 整體專案概述與啟動方式請參考根目錄 `README.md`。

## 現況（重要）

`skyeyesweb_tmrteip.client/` 已完成 **PrimeVue + TailwindCSS** 基礎工具鏈安裝與最小可運作骨架（`npm run build` 通過 type-check 與 Tailwind CSS 產生）：

- **已安裝**：`primevue`/`@primeuix/themes`（Aura 主題）/`@primevue/forms`/`primeicons`、`tailwindcss`/`@tailwindcss/vite`、`vue-router`、`zod`、`sweetalert2`、`flatpickr`、`highcharts`、`unplugin-vue-components`+`@primevue/auto-import-resolver`（PrimeVue 元件自動引入，比照舊模版慣例）。
- **刻意不裝**：`bootstrap`/`bootstrap-icons`（本專案不用 Bootstrap）；舊模版 `package.json` 內實際未被任何 `.vue` 引用的 `sass`、`vue-draggable-next`、`vuedraggable`、`highcharts-angular`（Angular 專用，Vue 專案誤裝）也未搬過來。
- **已建立（比照舊模版 `main.ts`/`router.ts` 架構搬移，移除 Bootstrap 依賴部分）**：
  - `src/main.ts`：掛載 PrimeVue + Aura 主題 + Vue Router；沿用舊模版的 `clsAjaxAsync`（XHR 封裝，含 `PostData`/Base64 包裝）、`clsSweetMsg`（SweetAlert2 封裝）、`clsFlatpickr`、`clsPaginationPage`、`clsConfig`（`SystemTitle` 已改為「台中捷運 EIP」）、`clsValidateForm`（Zod 表單驗證）、`Date.prototype` 擴充（`format`/`addDays`/`addMinutes`）、`GetHome`/`CmpAuth`/`CmpSignStatus`/`GetToken`/`DoSignOut`/`DoUpdatePathData`/`DoRouterPush`/`updateObjectProperties`。
  - `src/router.ts`：`createWebHashHistory`，含 `fetchDynamicRoutes()`（呼叫後端 `POST /api/Base/GetRouterData` 依 `Menu` 動態註冊路由）與 `initializeRouter()`。
  - **與舊模版的差異**：
    - 移除 `data-bs-theme`/`data-bs-color`（Bootstrap 主題切換）與 `.offcanvas-backdrop` 清理邏輯（Bootstrap 專屬）；新增 `DoSetTheme(theme)`，改以 `<html data-theme>` + Tailwind 的 `dark` class 切換深淺色（`main.css` 已加上 `@custom-variant dark (&:where(.dark, .dark *));`）。
    - `Menu`/`clsConfig.MenuData`/`clsConfig.PathData` 相關欄位已從舊的 `m_Id`/`m_Path`/`m_Name`/`m_Type`/`m_Icon`/`m_Auth` 改為對應 `SYS_tbMenu` 正規化後的 `Id`/`Path`/`Name`/`Type`/`Icon`/`Auth`（見 `docs/schemas/database_schema.md`）。
    - `router.ts` 的 `fetch` 呼叫已把請求 body 的 `postdata`（舊模版小寫，內容其實是空字串）正名為 `PostData`，並改用 ASP.NET Core 端的忽略大小寫綁定慣例保持一致。
    - `aliasComponentModules[mName]` 找不到對應頁面元件時，新增執行期防護（`console.warn` 並跳過該路由），而非讓舊模版原本的行為（元件為 `undefined` 直接注入路由）留到執行期才炸掉——這是新版 TypeScript（`noUncheckedIndexedAccess`）在型別檢查階段就攔到的問題，順手一併修正。
  - `src/web/Login/Login.vue`：極簡佔位頁（`/` 路由對應元件，PrimeVue `Card`），尚無登入表單邏輯——後端 `LoginController.DoSignIn` 也還沒實作（見 `docs/server/README.md`）。
  - `src/assets/main.css`：`@import "tailwindcss";` + 深色模式 `@custom-variant`，已移除 VS 精靈預設的示範樣式。移除了 VS 精靈預設的 `HelloWorld.vue`/`TheWelcome.vue`/`WelcomeItem.vue`/`icons/` 示範元件與 `logo.svg`。
- **尚未建立**：`master/`/`menu/` 版型元件、除 `Login.vue` 外的業務頁面、登入表單實際邏輯。

也就是說，工具鏈、`main.ts`/`router.ts` 基礎架構已就緒，但**還沒有實際業務頁面**——下一階段是後端使用者/選單權限資料表就緒後接上登入頁，並依 `docs/client/VUE_PAGE_TEMPLATE.skill.md` 的 Tailwind 改寫版本開發各業務頁面。

### Sakai 範本（`src/sakai/`，僅供參考/開發時瀏覽）

`src/sakai/` 是 [PrimeVue 官方 Sakai 範本](https://sakai.primevue.org/)整份搬移進來的版型/元件庫，供後續開發業務頁面時參考、複製其 Layout 與元件寫法，**不是本專案的業務功能**：

- **存放與路由都多一層 `sakai`**：檔案在 `src/sakai/`（內部 `@/` import 已全部改為 `@/sakai/`），路由掛在 `/#/sakai/...`（見 `src/sakai/router/index.ts` 匯出的 `sakaiRoutes`，由 `src/router.ts` 併入主路由表）。開發時可直接開 `/#/sakai` 看 Dashboard、`/#/sakai/uikit/*` 看各元件範例。
- **PrimeVue / vue-router 版本**：整合初期曾將 Sakai 程式碼升級至 v5 API（`primevue@^5.0.1`、`vue-router@^5.2.0`），但發現 PrimeVue 5 起要求商業授權（`@primevue/core` 的 `app.use(PrimeVue, ...)` 會無條件呼叫 `@primeui/license-manager` 驗證授權，未設定授權金鑰時畫面右下角會固定顯示紅色「Invalid PrimeUI License」浮水印，且是**全站**都會出現、並非只有 Sakai 或 Prime Blocks 頁面），故已**改為維持 PrimeVue 4**（`primevue@^4.5.5`、`vue-router@^4.6.4`、`@primeuix/themes@^2.0.3`、`@primevue/forms@^4.5.5`、`@primevue/auto-import-resolver@^4.5.5`）——PrimeVue 4 不含授權驗證機制，無此浮水印。

  > 主題套件說明：官方曾提供 `@primevue/themes`（版號跟隨 PrimeVue 本身，如 `4.5.4`），但該套件已在 npm 標示 **deprecated**（"No longer maintained, please migrate to `@primeuix/themes`"）。`@primeuix/themes` 是實際維護中的底層套件，`@primevue/themes` 只是已停止維護的薄包裝層，故本專案採用 `@primeuix/themes`。注意其 `3.0.0`（最新大版本）要求 `@primeuix/styled@^1.0.0`，與目前 PrimeVue 4.5.5 全家桶固定使用的 `@primeuix/styled@0.7.4` 不相容（`3.x` 是配合 PrimeVue 5 的版本），因此固定在相容的 `^2.0.3`。型別檢查（`vue-tsc --build`，含本專案 `noUncheckedIndexedAccess`）與 `vite build` 皆已過；若之後要換回 v5，需先取得 primeui.store 的授權金鑰並在 `main.ts` 的 `app.use(PrimeVue, { license: '<key>' })` 填入。
- **深色模式**：Sakai 自己的主題設定用 `.app-dark` class（PrimeVue `darkModeSelector`），本專案 `DoSetTheme` 用 `.dark`（Tailwind `dark:`）。`src/main.ts` 與 `src/sakai/layout/composables/layout.ts` 已互相同步，切換其中一個 class 兩者都會一起套用，兩套深色模式視覺上一致；但 `layoutConfig.darkTheme`（Sakai 自己的 reactive 狀態，控制其 topbar 圖示等）目前只有透過 Sakai 自己的切換鈕才會更新，從本專案外部（`DoSetTheme`）切換不會同步這個狀態，屬已知小限制。
- **樣式為獨立 Tailwind 進入點**：`src/sakai/assets/tailwind.css`（含 `tailwindcss-primeui` plugin）與 `styles.scss`（Sakai 自己的 layout SCSS）只在 Sakai 路由的元件內 import（`AppLayout.vue` 及各獨立頁面），理論上會隨路由 code-split；但實測 `vite build` 後仍被打進全域的 `index-*.css`（Rollup/Rolldown 的 chunk 合併策略所致），所以**目前是全站都會載入 Sakai 的 Tailwind 輸出**（多约 100KB+ CSS），並非嚴格路由隔離。視覺上未觀察到與本專案自己樣式衝突（已用瀏覽器實測 `/`、`/#/sakai` 亮/暗模式皆正常），但這點若之後要優化載入體積，需要另外處理 Vite 的 CSS chunk 策略。
- **靜態資源**：Sakai 原本 `public/demo/` 下的 demo 圖片已複製到本專案 `public/demo/`（`/demo/images/...` 這類絕對路徑參照才能正常載入）。
- **後端整合**：Sakai 內部路由是靜態註冊（非本專案 `router.ts` 依 `SYS_tbMenu` 動態註冊的機制），彼此不衝突；Sakai 的頁面目前都是自己的假資料（`src/sakai/service/*.ts`），未串接本專案後端 API。

### 已修正項目

- `vite.config.ts` 的 `server.proxy` 已從 VS 精靈預設值 `^/weatherforecast` 改為 `^/api`，並加上 `unplugin-vue-components`（`PrimeVueResolver`）與 `@tailwindcss/vite` plugin。

## 目錄與技術

- 專案目錄：`skyeyesweb_tmrteip.client/`
- 框架：Vue 3 + TypeScript
- 建置工具：Vite
- UI：PrimeVue（元件庫，Aura 主題）+ TailwindCSS（樣式），**不使用 Bootstrap**
- 路由：Vue Router（`createWebHashHistory`）
- 其他：Zod（表單驗證）、SweetAlert2（訊息互動）、flatpickr（日期選擇）、Highcharts（圖表）
- Lint：ESLint + oxlint（VS 精靈預設）

## 專案結構（`public/` / `src/`，現況）

### `src/`

- `src/main.ts`：程式進入點，掛載 Vue Router 與 PrimeVue/Aura 主題，並提供前端共用工具類（見上方「現況」）
- `src/App.vue`：根元件（`<router-view />`）
- `src/router.ts`：路由設定（`createWebHashHistory` + 動態選單路由）
- `src/web/`：業務頁面（目前僅 `Login/Login.vue` 佔位頁）
- `src/assets/`：靜態資源（`main.css` 為 Tailwind 進入點）
- `src/components/`：可重用元件（目前為空）
- `components.d.ts`：`unplugin-vue-components` 自動產生的元件型別宣告，**不要手動編輯**（每次 `npm run dev`/`build` 會依實際使用到的元件重新產生）

### 放置規則（建議，比照舊模版慣例）

- 需要「被打包、可 tree-shaking、可跟著版本管理/引用關係走」的資源（圖片/樣式/模組）優先放 `src/`，並用 `import`/`url()` 引用。
- 需要「固定 URL、原樣輸出、不想被打包處理」的資源再放 `public/`，並用 `/...` 路徑引用。

## 環境需求

- Node.js：建議 20+（`package.json` 的 `engines` 要求 `^22.18.0 || >=24.12.0`）
- npm：隨 Node 安裝

## 安裝與指令

在 `skyeyesweb_tmrteip.client/` 目錄下執行：

```bash
npm install
```

### 開發模式

```bash
npm run dev
```

- Vite dev server 預設埠：`https://localhost:58488`（見 `vite.config.ts` 的 `DEV_SERVER_PORT`，與 `SkyEyesWeb_TmrtEIP.Server.csproj` 的 `SpaProxyServerUrl` 對應）
- 若由後端使用 SPA Proxy 啟動，請見根目錄 `README.md`

### 建置

```bash
npm run build
```

### 預覽

```bash
npm run preview
```

## 設定（Config / Env）

### Proxy（開發時轉發 API）

設定於 `vite.config.ts` 的 `server.proxy`，已設定轉發 `^/api` 到後端（見上方「已修正項目」）。

建議原則：

- 前端呼叫 API 盡量使用相對路徑（例如 `/api/...`）
- 由 Vite dev server 代理到後端，避免 CORS 問題（後端目前也未啟用 CORS，見 `docs/server/README.md`）

## 產出與部署

- `npm run build` 產物預設輸出到 `skyeyesweb_tmrteip.client/dist/`
- 後端以 SPA 方式提供 `index.html`（含 `MapFallbackToFile("/index.html")`）
- 若採用後端提供靜態檔案：請確認 `dist/` 產物有被複製/發佈到後端可服務的靜態檔案目錄（以後端專案設定為準）

## Troubleshooting

- 依賴安裝問題：確認 Node 版本後，清除後重裝

```bash
rm -rf node_modules package-lock.json
npm cache verify
npm install
```

- HTTPS 憑證問題：`vite.config.ts` 會自動嘗試用 `dotnet dev-certs https` 產生憑證；若失敗請確認本機 .NET SDK 憑證設定。

## 相關文件

- API 契約：`docs/API_CONTRACT.md`
- DB Schema：`docs/schemas/database_schema.md`
- Utilities（公用程式，**待重建**，目前描述的是舊模版內容）：`docs/client/utils/UTILS_GUIDE.md`
- 地圖整合（**待確認是否需要**）：`docs/client/utils/map.skill.md`
- UI 風格規範（**沿用舊模版 Bootstrap 版本，待改寫成 Tailwind**）：`docs/client/UI_STYLE.md`
- Vue 頁面撰寫規範（**沿用舊模版 Bootstrap 版本，待改寫成 Tailwind**）：`docs/client/VUE_PAGE_TEMPLATE.skill.md`
