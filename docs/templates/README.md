# Templates

本目錄整理專案可重用的頁面範本，供建立新頁面時快速複製與調整。

> ⚠️ **Vue 範本待重寫**：下方「Vue 頁面範本分類」與「共通規格 > Vue 範本」段落，是直接沿用舊模版前端專案的內容，整套建立在 **Bootstrap 5**（`row`/`col-*`/`input-group`/`modal` 等）與該專案的前端工具類（`main.clsAjaxAsync`/`clsPaginationPage`/`clsValidateForm`、`Master01` 版型）之上。本專案前端規劃改用 **PrimeVue + TailwindCSS**，且 `skyeyesweb_tmrteip.client/` 目前仍是空白的 Vue 3 + Vite 骨架（尚未安裝 PrimeVue/Tailwind、尚未建立 router/menu/工具類），下列 Vue 範本**尚不可直接套用**，需等前端基礎骨架建立、並將 Bootstrap 慣例改寫為 Tailwind 寫法後再更新本文件。Controller 範本（`basic-controller.cs`）不受影響，可直接使用。

## Vue 頁面範本分類

| 分類 | 範本檔案 | 用途說明 | 主要結構 |
|---|---|---|---|
| 空白骨架 | `docs/templates/vue/page-basic.vue` | 用於新頁面起手式，保留專案標準 SFC 結構與 lifecycle 骨架 | `Master01`、`PageData`、`defineComponent` |
| 查詢 + 表格 + 分頁 | `docs/templates/vue/page-list-basic.vue` | 用於一般資料查詢頁，包含查詢表單、資料表與分頁 | `Form`、`DataTable`、`Pagination` |
| 列表 + 明細 + 新增視窗 | `docs/templates/vue/page-crud-master-detail.vue` | 用於主從 CRUD 維護頁，左側列表、右側明細、另含新增 `modal` | 查詢區、列表區、編輯表單、Bootstrap `modal` |
| 單表設定 | `docs/templates/vue/page-form-single.vue` | 用於單一設定頁，進頁載入資料後直接編輯儲存 | `Card`、單一 `Form`、儲存按鈕 |
| 卡片 + 圖表 | `docs/templates/vue/page-dashboard-basic.vue` | 用於儀表板或統計首頁，顯示 KPI 卡片與圖表 | `Card`、摘要區、`Highcharts` |
| 地圖監控 | `docs/templates/vue/page-map-basic.vue` | 用於圖台監控頁，含查詢、地圖 iframe 與下方列表同步 | 查詢列、`iframe map.html`、`DataTable` |

## Controller 範本分類

| 分類 | 範本檔案 | 用途說明 | 主要結構 |
|---|---|---|---|
| 基本 Controller | `docs/templates/controller/basic-controller.cs` | 用於建立標準 API Controller，包含查詢、新增、更新與 App 範例方法 | `clsApiBase`、`GetData`、`DoInsert`、`DoUpdate`、權限驗證 |

## Controller 範本說明

`docs/templates/controller/basic-controller.cs` 依目前後端 Controller 慣例整理，適合作為新功能 API 的起始範本。

### 主要內容

- 繼承 `clsApiBase`
- 使用 `[ApiController]` 與 `[Route("api/[controller]/[action]")]`
- 提供以下範例方法：
  - `GetData`：查詢資料 + 分頁
  - `DoInsert`：新增資料
  - `DoUpdate`：更新資料
  - `AppSample`：App Token 驗證範例

> 舊模版的範本另有 `TaskSample`（呼叫 `CmpTaskSignStatus()`），該方法屬於派遣業務的外部 Task 系統整合，本專案 `clsApiBase.cs` 未包含此方法，範本已移除對應範例，避免複製後編譯失敗。

### 共通規格

- Web API 入口參數使用 `clsPostData`
- 回傳型別使用 `clsResult`
- Web 權限驗證使用：
  - `base.CmpSignStatus(WsMethodName, FunctionType.Search)`
  - `base.CmpSignStatus(WsMethodName, FunctionType.Insert)`
  - `base.CmpSignStatus(WsMethodName, FunctionType.Update)`
- App 驗證使用：`CmpAppSignStatus()`
- 例外處理後統一呼叫：`DoAppRecLog(WsMethodName, PostData.ToJson(), Res)`
- 資料庫查詢方式以 `clsPostgreDB` 為主（PostgreSQL；已停用的 `clsDapper` 為舊版 SQL Server 實作，僅保留參考，見 `docs/server/clsDapper.md`）

### 使用建議

1. 新增 Web 功能 API 時，優先複製 `basic-controller.cs`。
2. 依功能調整 `namespace`、`Controller` 名稱與路由目錄。
3. 將範例 SQL 中的 `YourTable`、`YourField`、`YourSortField` 改為實際資料表與欄位。
4. 若頁面為 CRUD 類型，通常至少保留：
   - `GetData`
   - `DoInsert`
   - `DoUpdate`
5. 若功能只提供查詢，可刪除不需要的方法並保留一致的權限與 log 寫法。

## 共通規格

以上範本皆依專案現有慣例整理，主要遵循以下規格：

### Vue 範本

- 使用 `Vue 3` + `TypeScript`
- 採 `Options API` + `defineComponent`
- 主頁版型使用 `Master01`
- API 呼叫使用 `main.clsAjaxAsync`
- 表單驗證使用 `Zod + clsValidateForm`
- 分頁頁面使用 `main.clsPaginationPage`
- 樣式區塊固定為 `<style scoped>` + `@import './PageName.css';`

### Controller 範本

- 使用 ASP.NET Core Controller
- Controller 繼承 `clsApiBase`
- API 參數使用 `clsPostData`
- API 回傳使用 `clsResult`
- 權限驗證與操作記錄依 `clsApiBase` 既有模式
- 資料存取以 `clsPostgreDB` 為主（PostgreSQL；已停用的 `clsDapper` 為舊版 SQL Server 實作，僅保留參考）

## 使用建議

1. 先依頁面或 API 需求選擇最接近的範本。
2. 複製後修改檔名、類別名稱、資料欄位、API 名稱與 CSS 檔名。
3. 若頁面包含查詢結果，優先使用 `page-list-basic.vue` 或 `page-crud-master-detail.vue`。
4. 若頁面以監控圖台為主，優先使用 `page-map-basic.vue`。
5. 若頁面只是單純設定資料，優先使用 `page-form-single.vue`。
6. 若是新增後端 API，優先使用 `basic-controller.cs`。

## 建議對應情境

| 情境 | 建議範本 |
|---|---|
| 新功能頁尚未決定版型 | `page-basic.vue` |
| 一般查詢清單頁 | `page-list-basic.vue` |
| 帳號、權限、主檔維護 | `page-crud-master-detail.vue` |
| 系統參數、單筆設定 | `page-form-single.vue` |
| 統計首頁、儀表板 | `page-dashboard-basic.vue` |
| 任務位置、載具監控、圖台頁 | `page-map-basic.vue` |
| 新增標準 Web API Controller | `basic-controller.cs` |
