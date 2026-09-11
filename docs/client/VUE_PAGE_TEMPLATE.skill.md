
> ⚠️ **待重寫**：本文件沿用自舊模版前端專案，整套頁面規格/樣板（`Grid: col-md-4 col-12` 等）建立在 **Bootstrap 5** 網格系統之上。本專案前端規劃改用 **PrimeVue + TailwindCSS**，且 `skyeyesweb_tmrteip.client/` 目前仍是空白骨架，尚未建立對應的頁面產生慣例。待前端基礎骨架與 Tailwind 版面慣例確立後，本文件需要整份改寫（Bootstrap Grid class → Tailwind utility class），暫不可直接依此文件產生新頁面。

## 目的

此文件定義本專案 Vue 頁面（`.vue` SFC）撰寫規範。

---

## 檔案位置與命名

- 頁面檔案：依功能模組放置，例如 `src/web/<Module>/<Page>/<Page>.vue`
- 同頁面樣式：同資料夾建立 `<Page>.css`，並由頁面以 `@import` 引入
- 頁面 Skill 文件：各頁的 Skill 說明請放在該頁相同目錄下，檔名使用 `*.skill.md`（例如 `src/web/<Module>/<Page>/<Page>.skill.md`）
- 命名慣例
  - 頁面：`PascalCase.vue`（例：`<Page>.vue`）
  - 共用元件：`componentXxx.vue`（例：`componentSelectAirport.vue`）

---

## `*.skill.md`（頁面 Skill 文件）撰寫方式

每個頁面目錄下的 `*.skill.md` 用來描述該頁「大致版面與主要互動」，文件內容允許用**精簡過的標記語言**取代部分 HTML 區塊，以利快速閱讀與產生一致的頁面描述。

### 設計原則

- 只描述「結構/版面/元件意圖」，不需要逐行貼 HTML。
- 優先用自訂標籤描述常見區塊（搜尋欄位、表格欄位、操作按鈕、Dialog/Modal）。
- 內容可混用 Markdown（標題、清單、分隔線）。
- 自訂標籤不要求能被瀏覽器解析，重點是 **AI/人可讀**、可用來對照實作。

### 精簡標記語言（建議）

以下為建議的標籤集合（可依頁面需要擴充）：

- `<頁面>`：頁面根描述
- `<區塊 name='...'>`：頁面中的一個區塊（例如 查詢、列表、明細）
- `<搜尋列>` / `<搜尋欄位 ...>`：查詢表單
- `<表格>` / `<欄位 ...>`：資料表
- `<操作 ...>`：表格列操作按鈕/連結
- `<Dialog ...>` / `<Modal ...>`：浮動視窗/彈窗

屬性建議：

- `type`：欄位型態，例如 `text|date|select|multiselect`
- `model`：對應 `v-model` 欄位（例如 `dataSearchForm.keyword`）
- `component`：使用的元件（例如 `componentSelectAirport`）
- `name`：表單欄位名（例如 `origin_id`）
- `required`：`true|false`

### 置換示例（對照 HTML）

在 `*.skill.md` 中可以用：

`<搜尋欄位 type='multiselect' name='TaskStatus_id' model='dataSearchForm.TaskStatus_id' component='componentMultiSelectTaskStatus'>任務狀態</搜尋欄位>`

取代頁面中冗長的 HTML：

- `div.col-*` 版面容器
- `input-group` 包裝
- `input-group-text` label
- 內部實際元件（下拉、多選、輸入框等）

### Skill 範本參照（建議結構）

每頁 `*.skill.md` 建議至少包含以下段落，並以精簡標籤描述 UI：

- `## 頁面說明`：頁面用途
- `## 查詢區`：搜尋條件（`<搜尋列>` / `<搜尋欄位>`）
- `## 列表區`：表格欄位與操作（`<表格>` / `<欄位>` / `<操作>`）
- `## 視窗`：Dialog/Modal（`<Dialog>` / `<Modal>`）
- `## API`：本頁呼叫的 endpoints（例如 `GetData`, `GetXxx`）

範例骨架：

- `# <Page>`
- `<區塊 name='查詢'>`
  - `<搜尋列>`
    - `<搜尋欄位 type='date' name='sdate' model='dataSearchForm.sdate' required='true'>訂單時間(起)</搜尋欄位>`
    - `<搜尋欄位 type='date' name='edate' model='dataSearchForm.edate' required='true'>訂單時間(迄)</搜尋欄位>`
    - `<搜尋欄位 type='multiselect' name='TaskStatus_id' model='dataSearchForm.TaskStatus_id' component='componentMultiSelectTaskStatus'>任務狀態</搜尋欄位>`
    - `<搜尋欄位 type='select' name='origin_id' model='dataSearchForm.origin_id' component='componentSelectAirport'>起飛點</搜尋欄位>`
    - `<搜尋欄位 type='select' name='dest_id' model='dataSearchForm.dest_id' component='componentSelectAirport'>降落點</搜尋欄位>`
    - `<搜尋欄位 type='text' name='keyword' model='dataSearchForm.keyword'>關鍵字</搜尋欄位>`
  - `</搜尋列>`
- `</區塊>`
- `<區塊 name='列表'>`
  - `<表格 source='PageData.Data' selection='single'>`
    - `<欄位 field='Task_id'>任務編號</欄位>`
    - `<欄位 field='Order_id' action='openModal:modal_OrderContents'>訂單編號</欄位>`
    - `<欄位 field='origin_name'>起點</欄位>`
    - `<欄位 field='dest_name'>終點</欄位>`
    - `<欄位 field='Car_id'>載具編號</欄位>`
    - `<欄位 field='TaskStatus_name' action='openModal:modal_TaskStatusLog'>任務狀態</欄位>`
    - `<欄位 field='Pl_Start_dt|Ac_Start_dt' format='datetime'>預計/已送達時間</欄位>`
    - `<操作 label='即時貨態' action='openVehicleTrackingDialog' enabledWhen='Car_id'>` 
  - `</表格>`
- `</區塊>`

備註：範例中的 `field/action/enabledWhen` 為建議欄位，用於描述互動，不強制。

---

## SFC 結構

頁面固定使用以下區塊：

1. `<template>`
2. `<script lang="ts">`
3. `<style scoped>`

### Template 規範

- 以版型元件包覆：`<Master01>`
- 以區塊切分：
  - 查詢條件：`Card` + `Form`
  - 查詢結果：`DataTable` + `Pagination`
  - 額外功能：PrimeVue `Dialog`、Bootstrap `modal`

建議順序：查詢表單 → 表格 → 分頁 → Dialog → Modals。

### Script 規範

- 一律 `lang="ts"`
- 使用 `defineComponent`（Options API）
- 引入專案共用：`import * as main from '../../../main'`（相對路徑依實際目錄調整）
- `components` 明確註冊頁面會用到的元件

### Style 規範

- 使用 `<style scoped>`
- 只做匯入：`@import './<PageName>.css';`

---

## 表單（Form）與驗證

### Form 使用方式

- 表單元件需綁定：
  - `@submit="DoFormSubmit"`
  - `:initialValues="dataSearchForm"`
  - `:resolver="resolverSearchForm"`
  - `v-slot="$SearchForm"`（用於欄位 `invalid` / `error.message` 顯示）

### 驗證（Zod + clsValidateForm）

- schema 定義放在 `export default` 之外。模式如下：
  - `const baseSchema = main.Zod.object({...}).superRefine(...)`
  - `const ValidateSearchForm = new main.clsValidateForm(null);`
  - `ValidateSearchForm.schema = baseSchema;`
  - `resolverSearchForm: ValidateSearchForm.GetZodResolver()`

常見規則：

- 必填：`.nonempty({ message: '...' })`
- 格式檢查：`.refine(...)`
- 跨欄位檢核（例如日期起訖）：`.superRefine((data, ctx) => ctx.addIssue({ path: ['field'], ... }))`

### 送出（DoFormSubmit）

- 方法簽名：`DoFormSubmit({ valid }: { valid: boolean })`
- 若 `valid === false`：必須 `return ValidateSearchForm.Validate(this.dataSearchForm)`
- 成功送出：
  - `this.PageData.selectedRow = null`
  - 將 `dataSearchForm` 值同步回 `PageData`（範本使用 `Object.entries`）
  - `this.PageData.SetPage(1)` 觸發查詢

---

## 分頁與資料查詢（clsPaginationPage）

- `PageData: new main.clsPaginationPage() as any`
- `created()`：
  - `this.PageData.CallBack = this.GetData`
  - `this.PageData.SetPage(1)`

### GetData 規範

- 以 `clsAjaxAsync` 呼叫後端：
  - `url: main.clsConfig.ApiPath + '/GetData'`（Action 依頁面調整）
  - `body` 含查詢條件 + `PageRowCount` + `NowPageNum`
  - `isPostModel: true`, `encrypt: true`, `async: true`
- 成功：更新 `PageData.Data / DataCount / PageCount`，並呼叫 `GetPageItem()`
- 失敗：清空資料並使用 `main.clsSweetMsg.alert(...)`

---

## 表格（DataTable）規範

- 建議屬性：
  - `scrollable` + `scrollHeight="var(--table-height)"`
  - `:value="PageData.Data"`
  - `stripedRows`
  - `v-model:selection="PageData.selectedRow"` + `selectionMode="single"`

欄位規範：

- 直接欄位：`<Column field="..." header="..." />`
- 客製顯示：使用 `#body="slotProps"`
- 操作欄：集中於一欄，用 `Button` 並以 `:disabled` 控制

日期顯示：

- 統一使用 `new Date(value).format('yyyy-MM-dd hh:mm')`
- 空值顯示 `'-'`

---

## Dialog（PrimeVue）規範

- 開關：`v-model:visible="dialogVisible"`
- 關閉清理：`@hide="onDialogClose"`
- 若有輪詢/計時器：`beforeUnmount()` 必須停止（例如 `stopAutoRefresh()`）

---

## Modal（Bootstrap）規範

- 使用 Bootstrap 結構：`modal fade` + `modal-dialog` + `modal-content`
- `data-bs-backdrop="static"`（避免點背景關閉）
- 內容需加 `v-if="PageData.selectedRow!==null"` 避免空資料渲染
- 從表格開啟：
  - 先設定 `PageData.selectedRow`
  - 使用 `data-bs-toggle="modal" data-bs-target="#modal_xxx"`

---

## AJAX（clsAjaxAsync）規範

共通設定（依範本）：

- `url: main.clsConfig.ApiPath + '/<Action>'`
- `body: { ... }`
- `isPostModel: true`
- `encrypt: true`
- `async: true`

回呼：

- `success: (res: any) => { ... }`
- 有 loading 狀態時用 `final: () => { ... }` 收尾

---

## 計時器/輪詢規範

- 欄位：`refreshTimer`
- 啟動：`startAutoRefresh()` 使用 `setInterval`
- 停止：`stopAutoRefresh()` 清除並設回 `null`

---

## data 分區（#region）建議

依範本可用 `//#region` 分段：

- 查詢表單：`dataSearchForm` / `resolverSearchForm`
- 列表與分頁：`PageData`
- Dialog 相關狀態
- 明細/歷程資料（例如 `dataTaskStatusLog`）

---

## 方法命名

- 查詢/取資料：`GetData`、`GetXxx`
- 表單送出：`DoFormSubmit`
- UI 事件：`openXxxDialog`、`onDialogClose`
- 輪詢：`startAutoRefresh` / `stopAutoRefresh`
