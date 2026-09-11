# Utilities Guide

> ✅ 本文件描述的 `clsAjaxAsync`/`clsConfig`/`clsSweetMsg`/`clsFlatpickr`/`clsPaginationPage`/`clsValidateForm` 已搬移至 `skyeyesweb_tmrteip.client/src/main.ts`，內容與下方說明一致，僅第 6 節主題切換機制因本專案不使用 Bootstrap 而有調整（見該節說明），且 `clsConfig.MenuData`/選單相關欄位已改用 `SYS_tbMenu` 正規化後的命名（`Id`/`Path`/`Name`/`Type`/`Icon`/`Auth`，不再是 `m_Id`/`m_Path` 等）。

本文件整理前端常用的公用程式（Utilities），來源主要為 `skyeyesweb_tmrteip.client/src/main.ts` 內輸出的 class 與 function。

> 使用前提
>
> - 路由切換時會由 `router.afterEach(...)` 更新：
>   - `clsConfig.ApiPath`：依目前 route 推導 API base（`/api/{RouteName}`）
>   - `clsConfig.RouterPath`：依目前 route 推導頁面路徑（`/{RouteName}`，並將 `_` 轉成 `/`）
> - 多數 API 呼叫透過 `clsAjaxAsync`，並自動附帶以下 header：
>   - `Page: clsConfig.RouterPath`
>   - `Authorization: localStorage["token"]`（若存在）

---

## 文件使用原則

### 可參考範本來源

撰寫新頁面或整理頁面 Skill 文件時，可參考：

- `docs/templates/README.md`
- `docs/templates/vue/page-basic.vue`
- `docs/templates/vue/page-list-basic.vue`
- `docs/templates/vue/page-crud-master-detail.vue`
- `docs/templates/vue/page-form-single.vue`
- `docs/templates/vue/page-dashboard-basic.vue`
- `docs/templates/vue/page-map-basic.vue`

以上範本主要用途是：

- 協助辨識頁面類型
- 統一頁面常見區塊命名
- 對齊專案既有 `Vue` 頁面結構

### Skill 文件輸出位置原則

雖可參考 `docs/templates` 內的範本，但**實際輸出仍應以 `src/web` 目錄下的頁面 Skill 文件為主**。

建議規則如下：

- 頁面實作檔：`skyeyesweb_tmrteip.client/src/web/<Module>/<Page>/<Page>.vue`
- 頁面 Skill 檔：`skyeyesweb_tmrteip.client/src/web/<Module>/<Page>/<Page>.skill.md`

例如：

- `skyeyesweb_tmrteip.client/src/web/Task/TaskList/TaskList.vue`
- `skyeyesweb_tmrteip.client/src/web/Task/TaskList/TaskList.skill.md`

也就是說：

- `docs/templates/*`：作為**參考範本**
- `skyeyesweb_tmrteip.client/src/web/**/**/*.skill.md`：作為**實際產出與維護文件**

### Skill 文件撰寫方向

Skill 文件建議以「頁面用途 + UI 結構 + 主要互動 + API」為核心，內容應盡量貼近該頁實際實作，而不是只複製範本。

可參考 `TaskList.skill.md` 這類型式：

- 說明頁面目的與範圍
- 用結構化區塊描述搜尋區、結果區、Dialog、Modal
- 標示欄位、按鈕、資料來源與互動行為

---

## 1) `clsAjaxAsync`：XMLHttpRequest 封裝

用來統一呼叫後端 API（支援同步/非同步）。預設使用 JSON（`Content-Type: application/json`）。

### 建構參數

| 參數 | 型別 | 預設 | 說明 |
|---|---|---:|---|
| `method` | `string` | `post` | HTTP Method |
| `url` | `string` | `""` | API 完整網址 |
| `async` | `boolean` | `true` | 是否非同步（`false` 會阻塞 UI） |
| `cache` | `boolean` | `false` | 是否允許快取（設定 `Cache-Control`） |
| `body` | `object` | `{}` | body 內容 |
| `contentType` | `string` | `application/json` | Content-Type |
| `headers` | `{name,value}[]` | `[]` | 額外自訂 header |
| `encrypt` | `boolean` | `false` | 搭配 `isPostModel` 時，將 JSON 字串再做 Base64 |
| `isPostModel` | `boolean` | `false` | 是否送出 `{ PostData: "..." }` 格式 |
| `loadingMsg` | `string \| null` | `"Loading"` | 顯示 loading 文字；設 `null` 則不顯示 |
| `before` | `Function` |  | 發送前執行；回傳 `false` 會中止 |
| `success` | `Function` |  | status=200 且 JSON parse 後呼叫 |
| `error` | `Function` |  | status!=200 時呼叫 |
| `final` | `Function` |  | 結束後一定呼叫 |
| `callback` | `Function` |  | 類似 `success` 的額外回呼 |
| `parent` | `any` |  | 讓 `callback` 綁定 this（`callback.bind(parent)`） |

### 非同步使用範例

```ts
new clsAjaxAsync({
  url: clsConfig.ApiPath + '/GetHome',
  body: {},
  isPostModel: true,
  encrypt: true,
  loadingMsg: 'Loading',
  async: true,
  success: (res: any) => {
    if (res.Result) {
      // ...
    } else {
      clsSweetMsg.alert(res.ReturnMsg, res.ReturnNumber, 'error');
    }
  },
  error: () => {
    clsSweetMsg.alert('API 呼叫失敗', 'Error', 'error');
  },
  final: () => {
    // always
  }
}).ajax();
```

### 同步使用範例（不建議）

```ts
const res = new clsAjaxAsync({
  url: clsConfig.ApiPath + '/CmpAuth',
  body: { path: '/Some/Path' },
  isPostModel: true,
  encrypt: true,
  async: false,
  loadingMsg: null
}).ajax();
```

> 注意
>
> - `isPostModel=true` 會送出：`{ PostData: "..." }`
> - `encrypt=true` 會對 `encodeURI(JSON.stringify(body))` 再做 `btoa`

---

## 2) `clsSweetMsg`：SweetAlert2 封裝

整合 SweetAlert2：

- Loading（可多筆堆疊）
- Alert queue（當 Swal 已開啟時，後續訊息排隊）

### 常用方法

| 方法 | 說明 |
|---|---|
| `clsSweetMsg.OpenLoading(msg?)` | 開啟 Loading（預設 `"Loading"`） |
| `clsSweetMsg.CloseLoading(msg?)` | 關閉 Loading（未帶 msg 會 shift 第一筆） |
| `clsSweetMsg.CloseAll()` | 清空所有 Loading/Alert 並關閉 Swal |
| `clsSweetMsg.alert(msg, title?, icon?, callback?)` | 顯示訊息（若已開啟則排隊） |

### 範例

```ts
clsSweetMsg.OpenLoading('處理中');
clsSweetMsg.alert('完成', 'Success', 'success', () => {
  // confirmed
});
clsSweetMsg.CloseLoading('處理中');
```

---

## 3) `clsFlatpickr`：Flatpickr 小工具

已預設載入 `Mandarin` locale。

| 方法 | 說明 |
|---|---|
| `clsFlatpickr.setFlatpickr(element, config)` | 對單一元素套用 flatpickr 設定 |
| `clsFlatpickr.setDate(Document?)` | 掃描 `.flatpickr-date` 與 `.flatpickr-datetime` 並初始化 |

---

## 4) `clsPaginationPage`：前端分頁模型

用來管理表格分頁狀態（頁碼、每頁筆數、頁碼列表、前後跳頁）。

### 常用方法

| 方法 | 說明 |
|---|---|
| `GetPageItem()` | 依 `NowPageNum/PageCount` 產生 `PageItem` 與 `PagePrevious/PageNext` |
| `SetPage(NowPageNum?)` | 設定頁碼並呼叫 `CallBack` |

---

## 5) `clsConfig`：全域設定/路徑/標題

集中管理前端會用到的全域狀態。

### 常用欄位

- `SystemTitle`
- `BasePath`（`import.meta.env.BASE_URL`）
- `ApiPath`
- `RouterPath`
- `User`
- `currentTheme`
- `MenuData`
- `PathData`

### 常用方法

| 方法 | 說明 |
|---|---|
| `setTitle(title)` | 設定 `document.title`（組合 `SystemTitle`） |
| `getFullApiPath(apiPath)` | 將 `BasePath` 與 API 子路徑組合成完整 API path |

---

## 6) 主題：`DoSetTheme(theme?)`

寫入 `localStorage["theme"]`，並設定 `html[data-theme]` 屬性；`theme` 為 `'dark'` 時額外加上 `.dark` class（`'light'`/`'auto'` 移除），供 `main.css` 的 `@custom-variant dark (&:where(.dark, .dark *));` 判斷 Tailwind `dark:` 樣式。

> 與舊模版差異：舊版是 `html[data-bs-theme]`（Bootstrap 慣例，並額外處理 `data-bs-color`/`data-font-size` 覆寫 Bootstrap/PrimeVue CSS 變數）；本專案不使用 Bootstrap，已簡化為單純的淺色/深色切換，未搬移主色/字級切換功能。

```ts
DoSetTheme('dark');
DoSetTheme('auto');
```

---

## 7) 登入/權限/Token 常用函式

| 函式 | 說明 |
|---|---|
| `GetHome()` | 取得首頁路徑並導向（會使用 `LastPath`） |
| `CmpAuth(SysPath)` | 同步驗證登入狀態與權限（回傳 `boolean`，並回寫 `clsConfig.User`） |
| `CmpSignStatus()` | 非同步驗證登入狀態；失敗會提示並登出，成功會 `GetHome()` |
| `GetToken()` | 更新 token（成功寫回 `localStorage["token"]`） |
| `DoSignOut()` | 登出並導向 `#/login` |

---

## 8) 路徑/麵包屑：`DoUpdatePathData()`

依 `clsConfig.MenuData` 與 `clsConfig.RouterPath` 產生 `clsConfig.PathData`，提供麵包屑或路徑顯示元件使用。

---

## 9) `updateObjectProperties(Source, Target)`：覆蓋物件欄位

將 `Target` 的每個 key 以 `Source[key]` 覆蓋（若 `null/undefined` 則設為 `null`）。內部會在 300ms 後再做一次覆蓋。

```ts
updateObjectProperties(apiResult, formModel);
```

---

## 10) 表單驗證：`clsValidateForm`（Zod + PrimeVue resolver）

用 Zod schema 驗證表單資料，失敗會用 `clsSweetMsg.alert(...)` 彙整錯誤訊息。

```ts
const vf = new clsValidateForm({
  name: Zod.string().min(1, '請輸入姓名'),
});

if (!vf.Validate(formData)) return;

const resolver = vf.GetZodResolver();
```

另外也有匯出：

- `Zod`
- `ZodResolver`

---

## 11) Date 擴充

專案在全域擴充：

- `Date.prototype.format(format)`
- `Date.prototype.addDays(days)`（會修改原 Date）
- `Date.prototype.addMinutes(minutes)`（會修改原 Date）

```ts
const s = new Date().format('yyyy-MM-dd hh:mm:ss');

