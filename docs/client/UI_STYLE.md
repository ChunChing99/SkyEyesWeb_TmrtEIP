# 前端 UI 風格與樣式規範（舊模版，待重寫）

> ⚠️ **不適用於本專案**：以下內容完整沿用自舊模版前端專案，技術棧為 **Bootstrap 5 + PrimeVue**（含 `data-bs-theme`/`data-bs-color` 主題切換機制）。本專案 `skyeyesweb_tmrteip.client/` 規劃改用 **PrimeVue + TailwindCSS**，**不使用 Bootstrap**，且目前仍是空白 Vue 3 + Vite 骨架，尚未安裝任何 UI 套件。待前端實際導入 PrimeVue/TailwindCSS 後，本文件需要整份改寫（移除 Bootstrap 相關章節、改用 Tailwind 的 `dark:` 主題切換與 utility class 版面規範），暫不可依此文件開發新頁面。

本文件整理舊模版前端專案目前的前端技術棧、主題/色彩切換方式、字級縮放方式、全域樣式覆寫點，以及撰寫新頁面時建議遵循的 UI 一致性規範，僅供改寫時參考舊有慣例。

---

## 1. 技術棧與 UI 相關套件

- Framework：Vue 3
- Build Tool：Vite
- Router：Vue Router
- UI / CSS：
  - Bootstrap 5（`bootstrap`, `bootstrap-icons`）
  - PrimeVue 4（`primevue`, `@primevue/themes`, `primeicons`, `@primevue/forms`）
- 互動/訊息：SweetAlert2（`sweetalert2`，封裝為 `clsSweetMsg`）
- 日期時間：flatpickr（已載入中文語系 `zh`，封裝為 `clsFlatpickr`）

入口初始化請參考 `<舊模版前端目錄>/src/main.ts`。

---

## 2. 全域樣式載入順序

在 `src/main.ts` 主要載入順序如下（摘錄重點）：

1. `import './assets/main.css'`（專案全域樣式入口）
2. Bootstrap：
   - JS：`bootstrap/dist/js/bootstrap.bundle.min.js`
   - CSS：`bootstrap/dist/css/bootstrap.min.css`
   - Icons：`bootstrap-icons/font/bootstrap-icons.css`
3. PrimeVue：
   - `PrimeVue` Config
   - Theme preset：`@primevue/themes/aura`（Aura）
   - `primeicons/primeicons.css`
4. flatpickr：`flatpickr/dist/flatpickr.css`

### 2.1 `main.css` / `base.css`

- `src/assets/main.css`
  - `@import './base.css';`
  - 放置全域 reset、版面預設、PrimeVue/Bootstrap 的局部覆寫（例如 overlay z-index、`.p-button` 字級等）
- `src/assets/base.css`
  - 定義 palette 變數（`--vt-c-*`、`--color-*`）
  - 定義專案自訂 layout 變數（`--ct-*`）
  - 定義 Bootstrap / PrimeVue 的交互覆寫（例如 `--bs-secondary-bg`、`--p-card-background`）

---

## 3. 主題（Theme）與色彩（Primary color）策略

專案同時使用：

- Bootstrap 的 `data-bs-theme`（light/dark/auto 類似策略）
- PrimeVue 的主題 preset（Aura），並透過 CSS variables 調整 primary 色票

### 3.1 Bootstrap 深淺色模式：`data-bs-theme`

`DoSetTheme(theme)`（位於 `src/main.ts`）會將 `theme` 寫入：

- `localStorage["theme"]`
- `<html>` 的 `data-bs-theme`

實務：

- 建議以 `DoSetTheme('dark' | 'light' | 'auto')` 作為唯一入口。
- 若頁面要讀取目前模式，以 `clsConfig.currentTheme` 或 `localStorage["theme"]` 為準。

### 3.2 PrimeVue Primary 色彩切換：`data-bs-color`

在 `src/main.ts` 內註冊了 `MutationObserver` 監聽 `<html>` 上的：

- `data-bs-color`
- `data-font-size`

若 `data-bs-color != "auto"`，會將 PrimeVue 的：

- `--p-primary-50 ... --p-primary-950`

改指定到 Aura 內建色票（`--p-${color}-50 ...`）。

可用顏色（程式碼中有列舉提示）：

- `emerald`, `green`, `lime`
- `red`, `orange`, `amber`
- `yellow`, `teal`, `cyan`
- `sky`, `blue`, `indigo`
- `violet`, `fuchsia`, `pink`
- `rose`, `slate`, `gray`
- `zinc`, `neutral`, `stone`

建議做法：

- 切換主色：`document.documentElement.setAttribute('data-bs-color', 'blue')`
- 回到預設：設定為 `auto` 或移除 attribute

> 注意：目前 `updateThemeStyle()` 的策略是「只要 `data-bs-color` 或 `data-font-size` 任一不是 `auto`，就會寫入 inline style；否則清空 `style`」。

---

## 4. 字級縮放策略：`data-font-size`

`src/main.ts` 內的 `updateThemeStyle()` 會在 `data-font-size != "auto"` 時設定：

- `--bs-body-font-size`
- `--bs-btn-font-size`

且 `main.css` 的相關覆寫會使用 `var(--bs-body-font-size)`：

- `body { font-size: var(--bs-body-font-size); }`
- `.input-group-text, .btn { font-size: var(--bs-body-font-size) !important; }`
- `.p-button { font-size: calc(var(--bs-body-font-size) - 0.025rem) !important; }`

建議做法：

- 字級切換 UI：以 `<html data-font-size="0.9">`（單位 `rem`）調整。
- 元件內避免寫死 `px` 字級，優先用 `rem` / `var(--bs-body-font-size)`。

---

## 5. 版面與容器規範（目前全域預設）

### 5.1 `#app` 外層

定義於 `src/assets/main.css`：

- 一般：
  - `width: 100%`
  - `max-width: none`
  - `padding: 2rem`，且 `padding-bottom: 50px`
- `max-width: 800px`：
  - `#app { padding: 0; }`
- `min-width: 1024px`：
  - `#app { display: grid; padding: 0 2rem 50px 2rem; }`

### 5.2 小螢幕側邊欄

`handleResize()`：當螢幕寬 `<= 800px` 時，會設定：

- `<html data-sidenav-size="condensed">`

若有側邊欄元件，建議以 `data-sidenav-size` 作為切換依據。

---

## 6. Bootstrap / PrimeVue 覆寫點（已存在）

### 6.1 Overlay z-index

`src/assets/main.css`：

- `.p-multiselect-overlay.p-component, .p-select-overlay.p-component, .p-password-overlay.p-component { z-index: 1060 !important; }`

目的：避免 PrimeVue overlay 被 Bootstrap modal/offcanvas 等蓋住。

### 6.2 PrimeVue 表單元件與 Bootstrap `.form-control` 的整合

已存在的整合：

- `.p-multiselect.form-control { padding: 0; max-width: 70vw; }`
- `.p-password.form-control { padding: 0; border: none; }`
- `.p-password.form-control .p-inputtext { width: 100%; border-top-left-radius: 0; border-bottom-left-radius: 0; }`

建議：PrimeVue 元件需要套入 Bootstrap 表單排版時，優先以外層排版配合小範圍覆寫。

### 6.3 Card 背景

- `src/assets/main.css`：`.card-body { background: var(--p-card-background); }`
- `src/assets/base.css`：`[data-bs-theme=dark]` 下設定 `--p-card-background`（目前指向 `--bs-gray-600`）

### 6.4 Bootstrap secondary background 與 PrimeVue primary

`src/assets/base.css`：

- `:root { --bs-secondary-bg: var(--p-primary-100) !important; }`

表示 Bootstrap 的 secondary 背景色導向 PrimeVue primary palette。

---

## 7. Icon 規範

- Bootstrap Icons：`bi bi-xxx`
- PrimeIcons：`pi pi-xxx`

建議：同一個 UI 區塊（例如側欄選單）盡量統一 icon 系統，避免視覺粗細不一致。

---

## 8. 表單、訊息與互動（既有封裝）

### 8.1 SweetAlert2：`clsSweetMsg`

位於 `src/main.ts`：

- `OpenLoading(msg?)` / `CloseLoading(msg?)` / `CloseAll()`
- `alert(msg, title?, icon?, callback?)`

建議：API 非同步呼叫的 loading 與訊息彈窗優先使用此封裝，維持一致性。

### 8.2 flatpickr：`clsFlatpickr`

- `.flatpickr-date`：日期
- `.flatpickr-datetime`：日期+時間（`Y-m-d H:i`）

建議：新表單沿用上述 class 命名，統一初始化方式。

### 8.3 表單驗證：`clsValidateForm`（Zod）

- `Validate()` 失敗時會彙整錯誤訊息並以 `clsSweetMsg.alert()` 顯示。

---

## 9. 開發建議（維持一致性）

1. 避免寫死顏色：使用 Bootstrap variables 或 PrimeVue tokens（例如 `--p-primary-*`）。
2. 字級以 `rem`/變數管理：避免各頁自行用 `px`，確保 `data-font-size` 能一致縮放。
3. 覆寫集中化：全域覆寫放 `src/assets/main.css`/`src/assets/base.css`，頁面局部放元件 `scoped`。
4. Breakpoint：目前既有 `800px`（小螢幕）與 `1024px`（桌面）兩段。

---

## 10. 快速索引

- 前端入口：`<舊模版前端目錄>/src/main.ts`
- 全域樣式：`<舊模版前端目錄>/src/assets/main.css`
- 基礎變數：`<舊模版前端目錄>/src/assets/base.css`
- Vite 設定：`<舊模版前端目錄>/vite.config.ts`

---

## 11. UI 區塊建議結構

> 以下各元件的 `Grid` 屬性皆為 Bootstrap 網格 class，建立節點時會帶入該元件類型的預設值（例如容器類型多為 `col-12`），但屬可自訂欄位，實際值依頁面版面需求調整，並非固定寫死的值；以下範例中的 `Grid` 數值僅表示預設值/常見值。

### Master

`## Master` 底下的 `Master Name` 是每頁 `.skill.md` 各自的欄位值，決定該頁要 import 哪個版型元件，需依該頁 `.skill.md` 實際欄位值撰寫，而非固定寫死某一個版型。對應的 `.vue` 樣板：

<template>
  <{{Master Name}}>

    <Card> ... </Card>
    <!-- 其餘 Card / DataTable / Modal 等頁面內容，對應 Master 之下巢狀的 Card 區塊 -->

  </{{Master Name}}>
</template>

<script lang="ts">
  import {{Master Name}} from '@/master/{{對應檔名}}.vue';

  export default defineComponent({
    components: {
      {{Master Name}},
      // ...其他頁面元件
    },
    // ...
  });
</script>

**說明：**
- `Master Name`：決定要 import 哪個版型元件，需依該頁 `.skill.md` 實際欄位值置換 import 名稱與路徑。
- `Grid`（預設 `col-12`，可自訂）：目前僅為版面預留欄位，實務上未直接套用於版型元件標籤。
- `## Master` 底下巢狀的 `### Card: ...` 即為頁面內容區塊，對應包在該版型元件標籤之中（Card 對照見下一節）。

### Card

`Card: {{Title}}` 為頁面內的卡片區塊，帶有 `Grid`/`Title`/`Db Table`/`Db Column`/`Description` 屬性。`Db Table`/`Db Column` 描述卡片內欄位對應的資料表/欄位，供 AI/開發者判斷用途，並非直接對應到程式碼；`Description` 為補充說明；卡片實際內容（欄位、DataTable 等）為巢狀在 Card 之下的其他元件。

對應的 PrimeVue `Card` 樣板，依是否需要顯示標題列分為兩種寫法：

#### 不顯示標題列

<Card>
  <template #content>
    *** 欄位區塊 / Table 區塊 ***
  </template>
</Card>

**說明：**
- 適用於卡片不需要顯示標題文字的情境（例如搜尋表單、或版面已用其他文字說明用途的區塊）。
- `Title` 屬性此時僅供識別用途，不會渲染到頁面上。

#### 顯示標題列

<Card>
  <template #title>
    {{Title}}
  </template>
  <template #content>
    *** 欄位區塊 / Table 區塊 ***
  </template>
</Card>

**說明：**
- 適用於需要明確顯示卡片標題的情境，`#title` 插槽對應 Card 的 `Title` 屬性。
- `#title` 插槽內也可視需要加入操作按鈕（例如「新增」），與標題並列顯示。

兩種寫法皆為專案中實際使用中的樣式，依頁面是否需要視覺化標題列擇一即可，`Card` 的 `Title` 屬性本身不強制決定要用哪一種寫法。

### Dialog

`Dialog` 建立時固定帶有三個巢狀子區塊，皆比 `Dialog` 本身深一層標題：

    ## Dialog: {{Title}}

    - Grid: `col-12`
    - Title: {{Title}}

    ### DialogHeader

    - Grid: `col-12`

    ### DialogContent

    - Grid: `col-12`

    ### DialogFooter

    - Grid: `col-12`

`Dialog` 屬性為 `Grid`/`Title`/`Db Table`/`Db Column`/`Description`（與 Card 相同，`Db Table`/`Db Column`/`Description` 無值時不會輸出）。建立時固定帶有三個巢狀子區塊，各自只有 `Grid`（`Db Table`/`Db Column`/`Description` 同樣僅在有值時才輸出），沒有自己的 `Title`：
- `DialogHeader`：對應浮窗標題列
- `DialogContent`：對應浮窗內容（表單欄位、DataTable 等）
- `DialogFooter`：對應浮窗底部按鈕（確定/取消等）

其他元件（例如 `Button` 的 `Event`、`DataTable` 的 `Row Select`）可設為「開啟此 Dialog」，藉此觸發浮窗開啟；匯出的 `.skill.md` 中這類欄位存的是內部節點 id（例如 `Event: el-xxxxx`），不是人類可讀名稱，需比對同一 `.skill.md` 內其他區塊的 id 才能得知實際指向哪個 Dialog/Method。

對應的 PrimeVue `Dialog` 樣板（參考 [PrimeVue Dialog 官方文件](https://primevue.dev/dialog/)）：

<Dialog v-model:visible="{{顯示控制變數}}" :modal="true" :closable="true" @hide="{{關閉時處理方法}}">

  <template #header>
    <h4>{{Title}}</h4>
  </template>

  *** DialogContent 對應內容（Form / DataTable 等）***

  <template #footer>
    <Button label="取消" severity="secondary" type="button" @click="{{顯示控制變數}} = false" />
    <Button label="確定" type="submit" />
  </template>

</Dialog>

**說明：**
- `v-model:visible`：控制浮窗顯示的變數，對應開啟該 Dialog 的 `Event`/`Row Select`。
- `#header`：對應 `DialogHeader` 子區塊。
- 預設插槽（不需額外 `<template>` 包裝）：對應 `DialogContent` 子區塊，內容通常是 `<Form>` 或 `<DataTable>`。
- `#footer`：對應 `DialogFooter` 子區塊；若表單本身已有 `type="submit"` 的送出按鈕，`#footer` 可省略。
- `@hide`：浮窗關閉時的清理邏輯（重置表單、清空選取列等），視需要加上。

### Tabs / Tab

`Tabs` 為分頁容器，只能容納 `Tab` 子區塊；`Tab` 為個別分頁，僅能被放在 `Tabs` 之下，本身也是容器，巢狀在其下的其他元件（欄位、DataTable 等）即為該分頁的實際內容。兩者屬性皆為 `Grid`/`Title`/`Db Table`/`Db Column`/`Description`（與 Card 相同，無值時不輸出）：

    ## Tabs: {{Title}}

    - Grid: `col-12`
    - Title: {{Title}}

    ### Tab: {{Tab 1 標題}}

    - Grid: `col-12`
    - Title: {{Tab 1 標題}}

    ### Tab: {{Tab 2 標題}}

    - Grid: `col-12`
    - Title: {{Tab 2 標題}}

對應的 PrimeVue `Tabs` 樣板（參考 [PrimeVue Tabs 官方文件](https://primevue.dev/tabs/)）：

<Tabs value="0">
  <TabList>
    <Tab value="0">{{Tab 1 標題}}</Tab>
    <Tab value="1">{{Tab 2 標題}}</Tab>
  </TabList>
  <TabPanels>
    <TabPanel value="0">
      *** Tab 1 巢狀內容 ***
    </TabPanel>
    <TabPanel value="1">
      *** Tab 2 巢狀內容 ***
    </TabPanel>
  </TabPanels>
</Tabs>

**說明：**
- `Tabs` 的 `value`：目前作用中的分頁值。
- `TabList` 內的每個 `Tab` 對應一個 `.skill.md` 的 `Tab` 區塊，顯示其 `Title`；`Tab` 與對應的 `TabPanel` 需帶相同的 `value` 才能配對。
- `TabPanels` 內的每個 `TabPanel` 對應同一個 `Tab` 區塊巢狀的內容。

### DataTable / Column

`DataTable` 為表格容器，只能容納 `Column` 子節點；`Column` 為欄位定義，只能被放在 `DataTable` 之下（`restrictedTo: 'DataTable'`），本身也是容器（`container: true`），可再巢狀其他元件於欄位內（例如自訂儲存格內容：連結、Tag、操作按鈕），並與 `Card` 共用可摺疊 header/body UI。

    ## DataTable: {{Title}}

    - Grid: `col-12`
    - Title: {{Title}}
    - Selection: `Single` / `Multiple`（可留空）
    - Row Select: {{參照 Method 或 Dialog 節點}}
    - Pagination: true / false
    - Data Source: `custom` 或 呼叫 Method：{{Method Name}}
    - Db Table / Db Column / Description（無值時不輸出）

    ### Column: {{Label}}

    - Grid: `col-12`
    - Label: {{Label}}
    - Selection Mode: `single` / `multiple`（可留空）
    - Sortable: false
    - Db Table / Db Column / Description（無值時不輸出）

**屬性說明**

- `DataTable.Selection`：整表選取模式，下拉選項為 `Single` / `Multiple`（可清空），最終對應 PrimeVue DataTable 的 `selectionMode` prop（本專案 `.skill.md` 以大寫命名區隔於 `Column` 自己的版本，實際輸出到樣板時一律轉為小寫 `single` / `multiple`）。
- `DataTable.Row Select`：參照屬性，指向一個 `Method`（列被選取時呼叫 API）或 `Dialog`（列被選取時開啟浮窗）。本專案目前並未使用 PrimeVue DataTable 原生的 `@row-select` 事件，而是在對應 `Column` 的 `#body` 插槽內，於連結／按鈕的 `@click` 中先設定 `PageData.selectedRow = slotProps.data`，再觸發呼叫 Method 或開啟 Dialog/Modal（見下方樣板）。
- `DataTable.Pagination`：布林值。`true` 時，`DataTable` 下方需另外加上分頁元件 `<Pagination :dataPagination="PageData" />`（對應 `clsPaginationPage`，詳見頁面樣板規範文件的分頁與資料查詢章節）；`false` 時不加此元件，`:value` 改綁一次取得的完整資料陣列。本專案未使用 PrimeVue DataTable 內建的 `paginator` prop。
- `DataTable.Data Source`：`custom`（`:value` 綁定頁面自行維護的陣列，例如查詢浮窗內的暫存清單）或某個 `Method` 節點（`:value` 綁定該 Method 對應的 `PageData.Data`，並依頁面樣板規範文件的「分頁與資料查詢」規範實作 `GetData`）。
- `Column.Selection Mode`：對應 PrimeVue Column 元件本身的 `selectionMode` prop（原生即為小寫 `single` / `multiple`），用於將該欄位設為選取欄（例如 checkbox/radio 欄），與 `DataTable.Selection` 是兩組不同來源的欄位。
- `Column.Sortable`：布林值，對應 PrimeVue Column 的 `sortable` prop。
- `Column` 也是容器，巢狀在其下的其他元件即為該欄位的自訂顯示內容（對應樣板中的 `#body` 插槽）。

對應的 PrimeVue `DataTable` / `Column` 樣板（參考 [PrimeVue DataTable 官方文件](https://primevue.dev/datatable/)）：

<DataTable scrollable scrollHeight="calc(var(--table-height) - 50px)"
           :value="PageData.Data"
           v-model:selection="PageData.selectedRow"
           stripedRows
           selectionMode="single">

  <Column field="{{Db Column}}" header="{{Label}}"></Column>

  <!-- 客製顯示：例如可點擊連結，對應 Row Select 屬性開啟 Dialog/Modal -->
  <Column field="{{Db Column}}" header="{{Label}}">
    <template #body="slotProps">
      <a href="#" class="btn btn-info" style="padding:2px"
         @click.prevent="PageData.selectedRow = slotProps.data;"
         data-bs-toggle="modal" data-bs-target="#modal_xxx">
        {{ slotProps.data.{{Db Column}} }}
      </a>
    </template>
  </Column>

  <!-- 日期欄位：統一以 new Date(value).format('yyyy-MM-dd hh:mm') 顯示，空值顯示 '-' -->
  <Column header="{{Label}}">
    <template #body="slotProps">
      {{ slotProps.data.{{Db Column}} ? new Date(slotProps.data.{{Db Column}}).format('yyyy-MM-dd hh:mm') : '-' }}
    </template>
  </Column>

</DataTable>

<Pagination :dataPagination="PageData" />

**說明：**
- `scrollable` + `scrollHeight="var(--table-height)"`（或 `calc(var(--table-height) - 50px)`，視查詢區高度而定）：`--table-height` 由版型元件（Master）依視窗高度動態計算並寫入 `<html>` 的 inline style，讓表格高度隨版面自適應，新頁面不需自行計算。
- `v-model:selection` + `selectionMode="single"`：對應 `DataTable.Selection` 為 `Single` 的情境；`Selection` 為 `Multiple` 時改為 `selectionMode="multiple"`，`PageData.selectedRow` 對應改為陣列。
- 不需分頁（`Pagination: false`）時省略 `<Pagination>` 區塊。
- `Column` 未提供 `#body` 客製內容時，直接使用 `<Column field="..." header="..." />` 即可，`field` 對應 `Column.Db Column`、`header` 對應 `Column.Label`。

### Checkbox

`Checkbox` 屬於「Primevue Field」分類，為葉節點（不可容納子元件），代表單一布林欄位（例如資料庫的 `bit` 欄位），與 `InputText`/`Select`/`ToggleSwitch` 等欄位型元件共用同一組共通屬性。

    #### Checkbox: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

**屬性說明**

- `Grid`：Bootstrap 網格 class，建立節點時預設帶入 `col-md-4 col-12`，但屬可自訂欄位，實際值依頁面版面需求調整（例如與同排其他欄位並列時改用 `col-lg-3 col-md-6`），並非固定寫死的值。
- `Label`：欄位標籤文字，對應樣板中 `input-group-text` 的顯示文字。
- `Db Table` / `Db Column`：綁定的資料庫表格／欄位（供 AI/開發者判斷用途，並非直接對應到程式碼）。
- `Disabled`：布林值，對應 PrimeVue Checkbox 的 `disabled` prop。
- `Required` / `Validate`：布林值，是否必填、是否啟用驗證；實作方式依 [8.3 表單驗證：`clsValidateForm`（Zod）](#83-表單驗證clsvalidateformzod) 的規則，於 Zod schema 對該欄位加上對應規則（例如 `.refine(v => v === true, { message: '請勾選...' })`），驗證錯誤時同樣以 `Message` 元件顯示（見下方樣板註解）。
- `Description`：元件說明備註，僅供設計/開發參考，不會輸出到頁面。

對應的 PrimeVue `Checkbox` 樣板（參考 [PrimeVue Checkbox 官方文件](https://primevue.dev/checkbox/)；本專案目前以布林（單選）情境為主，對應官方文件的 `binary` 用法，樣式沿用既有 `ToggleSwitch` 欄位的包法）：

<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">
      {{Label}}
      <!-- 若 Required/Validate 為 true，加上驗證錯誤訊息，寫法同其他欄位 -->
      <Message v-if="$Form.{{Db Column}}?.invalid" severity="error" size="small" variant="simple">{{ $Form.{{Db Column}}.error?.message }}</Message>
    </span>
    <div class="form-control p-0">
      <Checkbox class="mt-2 mx-2" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" binary :disabled="{{Disabled}}" />
    </div>
  </div>
</div>

**說明：**
- `binary`：使 `v-model` 直接綁定布林值（`true`/`false`），對應單一 `Db Column` 欄位，而非 PrimeVue Checkbox 預設的「多選群組（陣列值）」用法。
- 外層 `<div class="form-control p-0">` 包法沿用專案既有的 `ToggleSwitch` 欄位樣式（見 [6.2 PrimeVue 表單元件與 Bootstrap `.form-control` 的整合](#62-primevue-表單元件與-bootstrap-form-control-的整合)），讓元件與其他 `input-group` 欄位對齊。
- 不需要顯示驗證錯誤（`Required`/`Validate` 皆為 `false`）時，可省略 `Message` 那一行。

### InputPassword

`InputPassword` 屬於「Primevue Field」分類，為葉節點，代表密碼輸入欄位。注意設計器/`.skill.md` 中的類型名稱是 `InputPassword`，但實際對應並渲染為 PrimeVue 的 `Password` 元件（`<Password>`），撰寫樣板時標籤名稱不同，需留意。

    #### InputPassword: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Placeholder: {{Placeholder}}
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

**屬性說明**

- `Label`：欄位標籤文字，對應樣板中 `input-group-text` 的顯示文字。
- `Db Table` / `Db Column`：綁定的資料庫表格／欄位（供 AI/開發者判斷用途，並非直接對應到程式碼）。
- `Placeholder`：對應 PrimeVue `Password` 的 `placeholder` prop。本專案部分頁面常見寫法是將 placeholder 動態換成驗證錯誤訊息：`:placeholder="($Form.xxx?.invalid) ? ($Form.xxx.error?.message) : '{{Placeholder}}'"`，作為 `Message` 元件之外的另一種錯誤提示寫法（兩種寫法在專案中並存，可依頁面既有風格擇一）。
- `Disabled`：布林值，對應 PrimeVue `Password` 的 `disabled` prop。
- `Required` / `Validate`：布林值，依 [8.3 表單驗證：`clsValidateForm`（Zod）](#83-表單驗證clsvalidateformzod) 的規則加上對應 Zod 規則；密碼複雜度規則可比照長度、大小寫、數字、特殊字元皆加上 `.regex(...)` 檢查的寫法。
- `Description`：元件說明備註，僅供設計/開發參考，不會輸出到頁面。

對應的 PrimeVue `Password` 樣板（參考 [PrimeVue Password 官方文件](https://primevue.dev/password/)）：

<div class="input-group mb-1">
  <span class="input-group-text">
    {{Label}}
  </span>
  <Password class="form-control" name="{{Db Column}}" v-model="dataForm.{{Db Column}}"
            :placeholder="{{Placeholder}}" :disabled="{{Disabled}}" autocomplete="new-password" />
</div>

**說明：**
- `class="form-control"` 直接加在 `<Password>` 上即可與 Bootstrap `input-group` 對齊，樣式由 [6.2 PrimeVue 表單元件與 Bootstrap `.form-control` 的整合](#62-primevue-表單元件與-bootstrap-form-control-的整合) 的 `.p-password.form-control` 覆寫負責，不需像 `ToggleSwitch`/`Checkbox` 那樣額外包一層 `div.form-control.p-0`。
- PrimeVue `Password` 預設帶密碼強度提示面板（`feedback` 預設為 `true`），本專案未覆寫此行為，適合用在「輸入新密碼」情境。「輸入原密碼」「再次輸入密碼確認」等不需要強度提示的欄位，專案實際做法改用 `<InputText type="password" class="form-control" ...>`，而非 `Password` 元件；若設計稿的一組密碼欄位包含「新密碼＋確認密碼」，通常只有新密碼欄位標記為 `InputPassword`，確認欄位可視為一般文字欄位處理。
- `autocomplete="new-password"`：供瀏覽器判斷此為新密碼欄位，避免自動帶入已儲存密碼；若欄位語意是「輸入現有密碼」，改用 `autocomplete="current-password"`。

### InputText

`InputText` 屬於「Primevue Field」分類，為葉節點，1:1 對應 PrimeVue 的 `InputText` 元件，是最基本的單行文字輸入欄位，也是其他文字類欄位型元件（`Select`/`Textarea`/`InputPassword` 等）共用屬性結構的基準。

    #### InputText: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Placeholder: {{Placeholder}}（可留空）
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

實際輸出範例（模糊查詢用的搜尋欄位，僅設定 `Description` 補充查詢方式）：

    #### InputText: 訂單單號

    - Grid: `col-md-3 col-12`
    - Label: 訂單單號
    - Db Table: dbo.TMS_ttTask_Order
    - Db Column: Order_id
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: 模糊查詢

**屬性說明**

- `Label`：欄位標籤文字，對應樣板中 `input-group-text` 的顯示文字。
- `Db Table` / `Db Column`：綁定的資料庫表格／欄位（供 AI/開發者判斷用途，並非直接對應到程式碼）。
- `Placeholder`：對應 PrimeVue `InputText` 的 `placeholder` prop，本專案常見寫法為「請輸入{{Label}}」或直接說明查詢方式（如上方範例的模糊查詢情境，改用 `Description` 補充說明，`Placeholder` 可留空）。
- `Disabled`：布林值，對應 `disabled` prop；可為靜態布林，亦可綁定一個表示表單是否可編輯的變數（例如檢視模式下整份表單停用）。
- `Required` / `Validate`：布林值，依 [8.3 表單驗證：`clsValidateForm`（Zod）](#83-表單驗證clsvalidateformzod) 的規則加上對應 Zod 規則；`Required` 為 `true` 時，本專案慣例會在 Label 前加上 `<span class="text-warning">*</span>` 星號提示必填。
- `Description`：元件說明備註；若欄位本身不需要 `Placeholder` 提示操作方式（例如模糊查詢、格式限制），也常直接寫在 `Description` 供設計/開發參考。

對應的 PrimeVue `InputText` 樣板（參考 [PrimeVue InputText 官方文件](https://primevue.dev/inputtext/)）：

<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">
      <!-- Required 為 true 時加上星號，寫法同其他必填欄位 -->
      <span class="text-warning">*</span>{{Label}}
      <Message v-if="$Form.{{Db Column}}?.invalid" severity="error" size="small" variant="simple">{{ $Form.{{Db Column}}.error?.message }}</Message>
    </span>
    <InputText class="form-control" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" placeholder="{{Placeholder}}" :disabled="{{Disabled}}" />
  </div>
</div>

**說明：**
- 星號 `<span class="text-warning">*</span>`：僅在 `Required` 為 `true` 時加入；`Required` 為 `false` 時省略該行即可，不需要動態判斷。
- 驗證錯誤顯示採用與大部分欄位一致的 `Message` 元件寫法，而非 `InputPassword` 採用的 placeholder 替換錯誤訊息寫法。
- 若欄位是唯讀顯示既有資料（例如明細 Dialog 中呈現已選取列的資料、不可編輯），改用 `:value="..."` 搭配 `readonly="true"`，不綁定 `v-model`/`name`，此時通常也不需要星號與 `Message`。
- `maxlength`：如需限制輸入長度，使用原生 HTML 屬性 `maxlength="{{長度}}"`。

### InputNumber

`InputNumber` 屬於「Primevue Field」分類，為葉節點，1:1 對應 PrimeVue 的 `InputNumber` 元件，用於數值輸入（例如長度、次數、期限等整數/數字欄位）。

    #### InputNumber: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Placeholder: {{Placeholder}}
    - Min: {{Min}}（可留空）
    - Max: {{Max}}（可留空）
    - Step: {{Step}}（可留空）
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

**屬性說明**

- `Label` / `Db Table` / `Db Column` / `Placeholder` / `Disabled` / `Required` / `Validate` / `Description`：與 `InputText` 相同，詳見上一節。
- `Min` / `Max` / `Step`：對應 PrimeVue `InputNumber` 的 `min`、`max`、`step` prop，用於限制輸入範圍與遞增/減幅度；三者皆可留空（不輸出即代表不限制）。

對應的 PrimeVue `InputNumber` 樣板（參考 [PrimeVue InputNumber 官方文件](https://primevue.dev/inputnumber/)）：

<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">
      <span class="text-warning">*</span>{{Label}}
      <Message v-if="$Form.{{Db Column}}?.invalid" severity="error" size="small" variant="simple">{{ $Form.{{Db Column}}.error?.message }}</Message>
    </span>
    <InputNumber class="form-control" name="{{Db Column}}" v-model="dataForm.{{Db Column}}"
                 placeholder="{{Placeholder}}" :min="{{Min}}" :max="{{Max}}" :step="{{Step}}" :disabled="{{Disabled}}" />
  </div>
</div>

**說明：**
- 星號、`Message` 驗證錯誤顯示寫法皆與 `InputText` 一致。
- 未設定 `Min`/`Max`/`Step` 時，樣板中對應的 `:min`/`:max`/`:step` 可整個省略，不需要傳入空字串。
- `v-model` 綁定的資料型別為 `number`（或 `null`），與 `InputText` 綁定 `string` 不同，撰寫 `dataForm` 初始值時需注意型別。

### Select

`Select` 屬於「Primevue Field」分類，為葉節點，對應 PrimeVue 的 `Select` 元件（PrimeVue v5 由 `Dropdown` 更名而來），用於下拉選單，`Multiple` 為 `true` 時代表多選情境。

    #### Select: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Options: {{自訂選項說明}}（`Data Source` 為 `custom` 時使用）
    - Multiple: false
    - Data Source: `custom` 或 呼叫 Method：{{Method Name}}
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

實際輸出範例（多選、選項來自 API）：

    #### Select: 任務狀態

    - Grid: `col-md-3 col-12`
    - Label: 任務狀態
    - Db Table: dbo.TMS_ttTask_Order
    - Db Column: Order_Status
    - Multiple: true
    - Data Source: 呼叫 Method：GetTaskStatus
    - Disabled: false
    - Required: false
    - Validate: false

**屬性說明**

- `Options`：`Data Source` 為 `custom` 時填寫的自由格式文字，用來描述自訂選項內容，供 AI/開發者依此撰寫實際的選項陣列，並非固定的資料格式（例如逗號分隔字串）。
- `Multiple`：布林值，是否支援多選；`false` 時對應 PrimeVue 的單選 `Select`，`true` 時樣板改用 PrimeVue 的 `MultiSelect` 元件（非同一個標籤，撰寫樣板時需注意置換）。
- `Data Source`：`custom`（依 `Options` 內容自行提供選項陣列）或某個 `Method` 節點（選項改由該 API 回傳資料產生，`optionLabel`/`optionValue` 依 API 回傳欄位決定）。
- 其餘 `Label`/`Db Table`/`Db Column`/`Disabled`/`Required`/`Validate`/`Description` 與 `InputText` 相同。

對應的 PrimeVue `Select` / `MultiSelect` 樣板（參考 [PrimeVue Select 官方文件](https://primevue.dev/select/)、[MultiSelect 官方文件](https://primevue.dev/multiselect/)）：

<!-- Multiple: false -->
<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">{{Label}}</span>
    <Select class="form-control" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" filter
            :options="{{選項陣列}}" optionLabel="{{顯示欄位}}" optionValue="{{值欄位}}"
            placeholder="請選擇" :disabled="{{Disabled}}" />
  </div>
</div>

<!-- Multiple: true，選項來自 Method -->
<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">{{Label}}</span>
    <MultiSelect class="form-control" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" filter display="chip"
                 :options="{{選項陣列}}" optionLabel="{{顯示欄位}}" optionValue="{{值欄位}}"
                 placeholder="請選擇" :disabled="{{Disabled}}" />
  </div>
</div>

**說明：**
- `filter`：本專案下拉選單習慣加上 `filter`，讓選項可搜尋，選項較多時體驗較好。
- `Data Source` 指向 `Method` 時，選項陣列需在頁面 `created()` 或對應時機呼叫該 API 取得後存入一個 `data`（例如 `dataXxxOptions`），再交給 `:options` 綁定，寫法與 [8.3 表單驗證：`clsValidateForm`（Zod）](#83-表單驗證clsvalidateformzod) 之外的一般 AJAX 呼叫規範相同。
- 若同一個「選項來自 Method」的下拉選單會在多個頁面重複使用（例如任務狀態、部門等共用清單），本專案慣例會抽成獨立元件（自行呼叫 API 取得選項、封裝 `v-model`），頁面內直接以該元件標籤取代這裡的 `Select`/`MultiSelect` 樣板。
- `optionLabel` / `optionValue`：依 API 回傳資料或 `dbSchema` 決定，樣板中以 `{{顯示欄位}}`/`{{值欄位}}` 表示需依實際情況替換。

### Textarea

`Textarea` 屬於「Primevue Field」分類，為葉節點，1:1 對應 PrimeVue 的 `Textarea` 元件，用於多行文字輸入（例如備註、說明）。

    #### Textarea: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Placeholder: {{Placeholder}}
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

**屬性說明**

- 各屬性意義與 `InputText` 相同，差異僅在於對應的 PrimeVue 元件為多行文字輸入。

對應的 PrimeVue `Textarea` 樣板（參考 [PrimeVue Textarea 官方文件](https://primevue.dev/textarea/)）：

<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">{{Label}}</span>
    <Textarea class="form-control" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" rows="5" cols="30" :disabled="{{Disabled}}" />
  </div>
</div>

**說明：**
- `rows`/`cols`：本專案常見寫法固定為 `rows="5" cols="30"`，可依版面需求調整。
- 星號、`Message` 驗證錯誤顯示寫法與 `InputText` 一致（本例未加驗證，若 `Required`/`Validate` 為 `true` 請比照 `InputText` 加上）。

### ToggleSwitch

`ToggleSwitch` 屬於「Primevue Field」分類，為葉節點，代表單一布林欄位（例如「狀態」啟用/停用），對應 PrimeVue 的 `ToggleSwitch` 元件，與 `Checkbox` 意義相近但外觀為開關樣式。

    #### ToggleSwitch: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

**屬性說明**

- 各屬性意義與 [Checkbox](#checkbox) 相同；`ToggleSwitch` 沒有 `Checkbox` 的 `binary` 用法差異，`v-model` 本身即直接綁定布林值。

對應的 PrimeVue `ToggleSwitch` 樣板（參考 [PrimeVue ToggleSwitch 官方文件](https://primevue.dev/toggleswitch/)；本專案已有多處「狀態」欄位採此寫法）：

<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">{{Label}}</span>
    <div class="form-control p-0">
      <ToggleSwitch class="mt-2 mx-2" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" :disabled="{{Disabled}}" />
    </div>
  </div>
</div>

**說明：**
- 外層需額外包一層 `<div class="form-control p-0">`，讓元件與其他 `input-group` 欄位對齊，這點與 `Checkbox` 相同（見 [6.2 PrimeVue 表單元件與 Bootstrap `.form-control` 的整合](#62-primevue-表單元件與-bootstrap-form-control-的整合)）。
- 本專案目前「狀態」類欄位（啟用/停用）皆採 `ToggleSwitch` 而非 `Checkbox`，可作為兩者選用時的參考慣例：語意為「開關/狀態切換」用 `ToggleSwitch`，語意為「勾選同意/選取」用 `Checkbox`。

### FileUpload

`FileUpload` 屬於「Primevue Field」分類，為葉節點，對應 PrimeVue 的 `FileUpload` 元件，用於檔案上傳（例如圖片）。

    #### FileUpload: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Accept: {{MIME 類型，例如 image/*}}
    - Max File Size: {{位元組數}}
    - Multiple: false
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

**屬性說明**

- `Accept`：對應 PrimeVue `FileUpload` 的 `accept` prop，限制可選檔案類型（MIME type）。
- `Max File Size`：對應 `maxFileSize` prop，單位為位元組（byte）。
- `Multiple`：是否允許一次選取多個檔案。
- 其餘屬性意義與 `InputText` 相同。

對應的 PrimeVue `FileUpload` 樣板（參考 [PrimeVue FileUpload 官方文件](https://primevue.dev/fileupload/)）：

<div class="{{Grid}}">
  <FileUpload name="{{Db Column}}"
              mode="basic"
              :accept="{{Accept}}"
              customUpload
              :auto="true"
              :maxFileSize="{{Max File Size}}"
              :multiple="{{Multiple}}"
              @select="on{{Label}}Select" />
</div>

```ts
on{{Label}}Select(event: any) {
  const file = event.files[0];
  const reader = new FileReader();
  reader.onload = (e: any) => {
    dataForm.{{Db Column}} = e.target.result; // Base64 字串
  };
  reader.readAsDataURL(file);
},
```

**說明：**
- `customUpload` + `:auto="true"` + `@select`：本專案的後端 API 慣例是以 JSON body 傳遞資料（見 `clsAjaxAsync` 規範），並非 multipart/form-data，因此**不使用** PrimeVue `FileUpload` 內建的 `url` 自動上傳機制，而是在 `@select` 內用 `FileReader.readAsDataURL()` 將檔案轉為 Base64 字串，存入表單資料後再與其他欄位一併以一般 AJAX 呼叫送出。
- `mode="basic"`：本專案目前的檔案上傳欄位皆採簡易模式（單一按鈕，不顯示進度條/檔案清單），需要更複雜的上傳體驗（多檔、進度顯示）時可改用預設的 `mode="advanced"`，並移除 `customUpload`／`FileReader` 轉換邏輯，改為官方文件的標準上傳流程。
- 若欄位用途是圖片上傳，通常會另外用一個 `<img :src="dataForm.{{Db Column}}" v-if="dataForm.{{Db Column}}">` 顯示預覽。

### Button

`Button` 屬於「Primevue Field」分類，`Grid` 預設為 `col-auto`，對應 PrimeVue 的 `Button` 元件。

    #### Button: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-auto`}}
    - Label: {{Label}}
    - Type: `button` / `submit`
    - Event: `custom`、呼叫 Method：{{Method Name}}、或開啟 Dialog：{{Dialog Title}}
    - Disabled: false
    - Description: {{說明}}（無值時不輸出）

實際輸出範例（查詢區的搜尋按鈕）：

    #### Button: Button

    - Grid: `col-auto`
    - Label: Button
    - Type: button
    - Event: 呼叫 Method：GetData
    - Disabled: false

**屬性說明**

- `Type`：對應原生 `<button type>`／PrimeVue `Button` 的 `type` prop，`button` 或 `submit`。
- `Event`：參照屬性。`custom` 代表自訂處理（樣板中直接寫 `@click` 呼叫的方法名稱不特別規範）；指向 `Method` 代表點擊後呼叫該 API；指向 `Dialog` 代表點擊後開啟該浮窗（`{{顯示控制變數}} = true`）。
- 若按鈕與 `HtmlButton`（HTML 分類的純 HTML 版本）擇一使用：兩者屬性結構相同，差異僅在於 `Button` 渲染為 PrimeVue `<Button>`，`HtmlButton` 渲染為原生 `<button>`。

對應的 PrimeVue `Button` 樣板（參考 [PrimeVue Button 官方文件](https://primevue.dev/button/)）：

<!-- Event 指向 Method，且此 Button 位於查詢表單內：實際以 Form 的 @submit 觸發，而非按鈕自身 @click -->
<Form class="row g-3" @submit="Do{{Method Name}}Submit" :initialValues="dataSearchForm">
  ...
  <button type="submit" class="btn btn-primary"><i class="bi bi-search"></i> {{Label}}</button>
</Form>

<!-- Event 指向 Method，且此 Button 為表單外的獨立動作按鈕：直接以 @click 呼叫 -->
<Button label="{{Label}}" type="button" :disabled="{{Disabled}}" @click="{{Method Name}}" />

<!-- Event 指向 Dialog：開啟浮窗 -->
<Button label="{{Label}}" type="button" :disabled="{{Disabled}}" @click="{{顯示控制變數}} = true" />

**說明：**
- 當 `Button` 位於某個 `<Form>` 內、`Type` 為 `submit` 時，`Event: 呼叫 Method` 實際上是透過 Form 的 `@submit` 處理函式（依 [8.3 表單驗證：`clsValidateForm`（Zod）](#83-表單驗證clsvalidateformzod) 規則驗證後）呼叫該 Method，而非按鈕本身掛 `@click`；此情境下本專案慣例甚至直接使用原生 `<button type="submit" class="btn btn-primary">`（Bootstrap 樣式）取代 PrimeVue `<Button>`，兩者視覺風格皆可接受，可依頁面既有風格擇一。
- 查詢區以外的一般動作按鈕（新增、刪除、送出、關閉等），以 PrimeVue `<Button>` 搭配 `severity`（`secondary`/`danger`/`contrast` 等）與 `icon`（PrimeIcons）呈現，樣式與圖示可參考 [7. Icon 規範](#7-icon-規範)。
- `Disabled` 亦可綁定條件式表達式（例如尚未選取資料時停用），非僅能是固定布林值。

### Map

`Map` 屬於「Other」分類，無直接對應的 PrimeVue 元件（自訂實作），為葉節點，代表地圖選點欄位：讓使用者在地圖上點選一個位置，取得經緯度後存入表單資料。

    #### Map: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Description: {{自訂義的描述或補充說明}}（無值時不輸出）

對應樣板：於頁面插入地圖容器 HTML，並於掛載時透過地圖模組初始化圖台（依實際採用的模組，容器可能是 `<div>` 直接建立地圖實例：

<div id="map" class="map-container"></div>

```ts
mounted() {
  this.initMap();
},
methods: {
  initMap() {
    // 依 utils/map.skill.md 選用對應模組，以容器 id 初始化圖台
    // 例如建立地圖實例、設定圖層/圖磚來源與初始視圖範圍
  },
},
```

**說明：**
- `Map` 元件對應到頁面上插入一個地圖容器（例如 `id="map"`），地圖本身（圖層、標記、事件、繪製工具等）皆由地圖模組於 `mounted()` 依此容器 id 初始化建立，不是以既有 PrimeVue/HTML 表單元件呈現。
- 地圖模組完整 API（建立標記、監聽點擊座標、縮放、繪製等）請參考 `utils/map.skill.md`。

### Custom

`Custom` 屬於「Other」分類，無固定對應元件，為葉節點，是供使用者插入任意自訂元件的萬用節點。

    #### Custom: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Description: {{說明}}（無值時不輸出）

**說明：**
- `Custom` 沒有固定對應的 PrimeVue/HTML 元件，也沒有 `Db Table`/`Db Column`/`Disabled`/`Required`/`Validate` 等其他欄位型元件常見的屬性。
- 主要依 `Description` 內容理解實際需求並生成對應的元件/區塊；`Label` 僅供識別用途，不代表輸出結果的名稱或顯示文字。
- 若 `Description` 描述的是專案既有元件，直接引用該元件；若描述的是全新需求，則依說明內容撰寫對應的 HTML/PrimeVue 組合，不受限於本文件其他章節列出的元件類型。

### Div

`Div` 屬於「HTML」分類，為容器（`container: true`），對應原生 `<div>`，用於單純的版面分組，不像 `Card` 帶有標題列/邊框視覺。

    #### Div

    - Grid: {{Grid class，依版面調整，預設 `col-12`}}
    - Class Name: {{className}}（可留空）
    - Description: {{說明}}（無值時不輸出）

實際輸出範例：

    #### Div

    - Grid: `col-3`
    - Description: 群組內容下的元件

對應樣板：

<div class="{{Grid}} {{Class Name}}">
  *** 巢狀內容 ***
</div>

**說明：**
- `Div` 沒有 `Label`/`Title`，純粹作版面分組容器，巢狀內容比照其他容器元件（例如 `Card` 之下的欄位、`DataTable` 等）。
- `Class Name` 對應原生 `class` 屬性，附加在 `Grid` 之後；留空時樣板中省略即可。

### Text

`Text` 屬於「HTML」分類，為葉節點，代表純文字節點，用於顯示靜態或資料驅動的文字內容。

    #### Text: {{Text}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Text: {{顯示文字，預設 `Sample text`}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

對應樣板：

<span class="{{Grid}}">{{Text}}</span>

<!-- 若設定 Db Column，代表顯示該欄位的資料內容，而非固定文字 -->
<span class="{{Grid}}">{{ dataForm.{{Db Column}} }}</span>

**說明：**
- 有 `Db Column` 時顯示對應資料欄位內容，否則直接輸出 `Text` 屬性的固定文字。
- `Disabled`/`Required`/`Validate` 雖列在預設屬性中，但純文字顯示沒有實際輸入行為，一般不會用到，可忽略。

### Image

`Image` 屬於「HTML」分類，為葉節點，對應原生 `<img>`。

    #### Image: {{Alt}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Src: {{圖片網址或說明}}
    - Alt: {{替代文字}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

對應樣板：

<img class="{{Grid}}" :src="{{Src}}" :alt="{{Alt}}" />

<!-- 若圖片內容綁定某個 Db Column（例如簽收照片、已上傳的頭像），改用資料欄位 -->
<img class="{{Grid}}" :src="dataForm.{{Db Column}}" :alt="{{Alt}}" />

**說明：**
- 有 `Db Column` 時 `:src` 改綁對應資料欄位（常見於圖片為 Base64 字串或圖片網址的情境），`Src` 屬性此時可作為預設值/佔位圖片說明。
- `Disabled`/`Required`/`Validate` 對純顯示用的 `Image` 一般同樣不會用到。

### Link

`Link` 屬於「HTML」分類，`Grid` 預設為 `col-auto`，對應原生 `<a>`。

    #### Link: {{Text}}

    - Grid: {{Grid class，依版面調整，預設 `col-auto`}}
    - Text: {{顯示文字，預設 `Link text`}}
    - Href: {{連結網址}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

對應樣板：

<a class="{{Grid}}" :href="{{Href}}">{{Text}}</a>

**說明：**
- 原生 `<a>` 沒有 `disabled` 屬性，`Disabled` 為 `true` 時需自行處理（例如以 `:class="{{Disabled}} ? 'disabled' : ''"` 加上停用樣式，並視需要以 `@click.prevent` 阻擋點擊）。
- `Db Column` 若有設定，通常代表 `Text` 或 `Href` 其中之一改為動態資料（例如顯示訂單編號、連結到該筆資料的明細頁），依 `Description` 判斷實際綁定哪一個屬性。

### HtmlCheckbox

`HtmlCheckbox` 屬於「HTML」分類，為葉節點，是 [Checkbox](#checkbox) 的純 HTML 版本，對應原生 `<input type="checkbox">`。屬性結構與 `Checkbox` 完全相同（`Label`/`Db Table`/`Db Column`/`Disabled`/`Required`/`Validate`/`Description`），差異僅在於渲染的標籤。

對應樣板：

<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">{{Label}}</span>
    <div class="form-control p-0 d-flex align-items-center">
      <input type="checkbox" class="form-check-input mx-2" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" :disabled="{{Disabled}}" />
    </div>
  </div>
</div>

**說明：**
- 原生 checkbox 搭配 Bootstrap 的 `form-check-input` class，而非 PrimeVue `Checkbox` 的 `binary` 用法；`v-model` 同樣直接綁定布林值。
- 其餘驗證、必填星號等寫法比照 [Checkbox](#checkbox)。

### HtmlSelect

`HtmlSelect` 屬於「HTML」分類，為葉節點，是 [Select](#select) 的純 HTML 版本，對應原生 `<select>`。屬性結構與 `Select` 相同（`Label`/`Db Table`/`Db Column`/`Options`/`Multiple`/`Data Source`/`Disabled`/`Required`/`Validate`/`Description`）。

對應樣板：

<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">{{Label}}</span>
    <select class="form-select" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" :multiple="{{Multiple}}" :disabled="{{Disabled}}">
      <option v-for="opt in {{選項陣列}}" :key="opt.{{值欄位}}" :value="opt.{{值欄位}}">{{ opt.{{顯示欄位}} }}</option>
    </select>
  </div>
</div>

**說明：**
- 原生 `<select>` 使用 Bootstrap 的 `form-select` class（而非 PrimeVue `Select` 掛 `form-control` 的包法）。
- `Multiple` 為 `true` 時是原生 `<select multiple>`（需按 Ctrl/Cmd 多選），使用體感與 PrimeVue `MultiSelect` 的 chip/搜尋介面差異較大；若需要較佳的多選體驗，建議改用 [Select](#select) 而非 `HtmlSelect`。
- `Data Source` 指向 `Method` 時的選項取得方式與 [Select](#select) 相同。

### HtmlTextarea

`HtmlTextarea` 屬於「HTML」分類，為葉節點，是 [Textarea](#textarea) 的純 HTML 版本，對應原生 `<textarea>`。屬性結構與 `Textarea` 相同。

對應樣板：

<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">{{Label}}</span>
    <textarea class="form-control" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" :placeholder="{{Placeholder}}" :disabled="{{Disabled}}" rows="5"></textarea>
  </div>
</div>

**說明：**
- 原生 `<textarea>` 直接套用 Bootstrap 的 `form-control` class 即可，不需要像 PrimeVue `Textarea` 那樣額外考慮 6.2 節的整合覆寫。
- 其餘屬性意義與樣板寫法比照 [Textarea](#textarea)。

### HtmlButton

`HtmlButton` 屬於「HTML」分類，`Grid` 預設為 `col-auto`，是 [Button](#button) 的純 HTML 版本，對應原生 `<button>`。比 `Button` 多一個 `Link` 屬性，用於純 HTML 場景常見的跳轉連結情境。

    #### HtmlButton: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-auto`}}
    - Label: {{Label}}
    - Type: `button` / `submit`
    - Link: {{跳轉網址}}（可留空）
    - Event: `custom`、呼叫 Method：{{Method Name}}、或開啟 Dialog：{{Dialog Title}}
    - Disabled: false
    - Description: {{說明}}（無值時不輸出）

實際輸出範例（點擊後開啟新增訂單浮窗）：

    #### HtmlButton: Button

    - Grid: `col-auto`
    - Label: Button
    - Type: button
    - Event: 開啟 Dialog：新增訂單
    - Disabled: false

對應樣板：

<!-- Link 未設定：一般動作按鈕，Event 對應規則同 Button 一節 -->
<button type="{{Type}}" class="btn btn-primary" :disabled="{{Disabled}}" @click="{{顯示控制變數}} = true">{{Label}}</button>

<!-- Link 有設定：純跳轉連結，改用 <a> 呈現按鈕外觀 -->
<a href="{{Link}}" class="btn btn-primary">{{Label}}</a>

**說明：**
- `Event`（`custom`/呼叫 Method/開啟 Dialog）的對應規則與 [Button](#button) 一節相同；位於查詢表單內、`Type` 為 `submit` 時，同樣可能改由 Form 的 `@submit` 觸發，而非按鈕自身 `@click`。
- `Link` 有值時，代表此按鈕的用途是導覽到其他頁面/外部網址，此時通常直接改用 `<a class="btn ...">` 而非 `<button>`，`Event` 保持 `custom` 即可，不需另外呼叫 Method 或開啟 Dialog。

### flatpickr

`Flatpickr` 屬於第三方 flatpickr 分類，為葉節點，非 PrimeVue 元件，也不是獨立的 Vue 元件標籤——實際渲染仍是 PrimeVue 的 `InputText`，日期選擇行為改由額外的 CSS class 與 JS 初始化掛上去（詳見 [8.2 flatpickr：`clsFlatpickr`](#82-flatpickrclsflatpickr)）。

    #### Flatpickr: {{Label}}

    - Grid: {{Grid class，依版面調整，預設 `col-md-4 col-12`}}
    - Label: {{Label}}
    - Db Table: {{dbTable}}
    - Db Column: {{dbColumn}}
    - Placeholder: {{Placeholder}}
    - Type: `flatpickr-date` / `flatpickr-time` / `flatpickr-datetime`
    - Disabled: false
    - Required: false
    - Validate: false
    - Description: {{說明}}（無值時不輸出）

實際輸出範例（查詢區的日期起訖欄位）：

    ##### Flatpickr: 訂單日期(起)

    - Grid: `col-md-4 col-12`
    - Label: 訂單日期(起)
    - Db Table: dbo.TMS_ttTask_Order
    - Db Column: Create_dt
    - Type: flatpickr-date
    - Disabled: false
    - Required: true
    - Validate: true

**屬性說明**

- `Type`：決定套用的 class，`flatpickr-date` 對應 `.flatpickr-date`（純日期）、`flatpickr-datetime` 對應 `.flatpickr-datetime`（日期+時間，格式 `Y-m-d H:i`）；`flatpickr-time`（純時間）雖是設計器提供的選項，但**本專案目前的 flatpickr 初始化工具尚未實作對應的 `.flatpickr-time` 處理邏輯**，選用此類型時需自行擴充初始化工具或改為手動呼叫 flatpickr 設定，否則欄位不會套用時間選擇器行為。
- 其餘 `Label`/`Db Table`/`Db Column`/`Placeholder`/`Disabled`/`Required`/`Validate`/`Description` 意義與 [InputText](#inputtext) 相同。

對應樣板：

<div class="{{Grid}}">
  <div class="input-group mb-1">
    <span class="input-group-text">{{Label}}</span>
    <InputText class="form-control {{Type}}" name="{{Db Column}}" v-model="dataForm.{{Db Column}}" placeholder="{{Placeholder}}" :disabled="{{Disabled}}" />
  </div>
</div>

```ts
mounted() {
  // 掛載時掃描目前 DOM 中所有 .flatpickr-date / .flatpickr-datetime 元素並套用 flatpickr
  main.clsFlatpickr.setDate(document);
},
```

**說明：**
- `class="form-control {{Type}}"`：直接把 `Type` 對應的 class（`flatpickr-date`/`flatpickr-datetime`）疊加在 `form-control` 之後，不需要額外的 `v-model`/事件處理，日期選擇行為完全由初始化工具依 class 掃描 DOM 後掛上。
- 初始化必須在欄位已經渲染到 DOM 之後執行，本專案慣例統一在 `mounted()` 呼叫一次 `main.clsFlatpickr.setDate(document)`。若欄位位於掛載後才動態顯示的區塊（例如以 `v-if` 控制、或稍後才開啟的 Dialog 內容），該次 `mounted()` 呼叫時這些元素還不存在，需要在該區塊顯示後（例如 `$nextTick` 內）再呼叫一次 `setDate()`，否則欄位不會套用日期選擇器。
- `setDate()` 可傳入 `Document` 參數（預設為全域 `document`），一般頁面情境直接呼叫 `setDate(document)`（或省略參數）即可。

### Method

`Method` 屬於「API」分類，是後端 API 設定專用節點（`apiOnly: true`），不會渲染成頁面元件，也沒有 Grid 版面的概念（`hideGridEditor: true`）。`Method` 對應到後端 Controller 中的一個 Action，是本文件中唯一對應後端程式碼、而非前端畫面的元件類型。**本節只說明 `Method` 各屬性如何對應到要生成的 Controller Action**；回傳格式、狀態碼、`clsResult`/`clsPostgreDB`/`clsPostData` 等共用類別的完整行為已有專案內的 API 溝通契約文件詳述，不在此重複，僅在必要處以連結/一句話帶過。

    ### Method

    - Grid: `col-12`
    - Method: GET,POST
    - Method Name: {{MethodName}}
    - Description: {{建議直接以 C# XML 文件註解格式撰寫，逐字放到產生的 Method 上方}}
    - Sql: {{SQL 陳述式或查詢意圖線索}}

**屬性說明**

- `Method`：以逗號分隔的 HTTP 動詞字串，對應要疊加在 Action 上的 `[Http*]` attribute（例如 `GET,POST` → `[HttpGet]` + `[HttpPost]`）。
- `Method Name`：對應 Action 的方法名稱，同時是其他元件（`Select.Data Source`、`Button.Event`、`DataTable.Data Source`/`Row Select` 等）用來引用這個 API 的識別名稱；也決定最終呼叫路徑 `api/{Controller}/{Method Name}`。
- `Description`：建議直接撰寫成最終要放在 Action 正上方的 C# XML 文件註解（`/// <summary>`/`<param>`/`<returns>`/`<remarks>`），而非一般敘述文字，本專案實際產生的 Controller 程式碼會把這段文字原封不動貼到 Action 上方；簡單查詢也可以只寫一行摘要文字，生成時再補齊成完整 XML 註解格式即可。
- `Sql`：提供給 AI 理解此 API 查詢意圖的線索，不等同於最終原封不動貼上執行的語句。單純查詢（例如下拉選單選項清單）時 `Sql` 常常就直接是最終可執行的完整語句，AI 幾乎逐字沿用；但遇到分頁、動態條件、多表 Join 等較複雜情境時，`Sql` 可能只是草稿甚至留空，需由 AI 一併理解 `Description` 描述的參數與業務規則後，自行設計出實際可執行的完整 SQL 與參數綁定。

新增 Method 時，Controller 骨架直接複製 `docs/templates/controller/basic-controller.cs` 作為起點（該範本已包含查詢+分頁、新增、更新、App/Task 驗證等範例方法與固定寫法），不需要另外重新設計骨架；上述各屬性對應填入骨架的位置如下：

- `Method` → 疊加在挑選的範例方法上的 `[Http*]` attribute（例如 `GET,POST` → `[HttpGet]` + `[HttpPost]`）。
- `Method Name` → 取代骨架中的方法名稱（例如把 `GetData`/`DoInsert`/`DoUpdate` 改成實際名稱），同時決定呼叫路徑 `api/{Controller}/{Method Name}`，也是其他元件（`Select.Data Source`、`Button.Event`、`DataTable.Data Source`/`Row Select` 等）引用這個 API 的識別名稱。
- `Description` → 取代該方法上方原有的 `/// <summary>` 範例註解，逐字貼上完整 C# XML 文件註解。
- `Sql` → 取代骨架中 `strSql`/`clsPostgreDB.getList(...)` 範例裡的 `YourTable`/`YourField`/`YourSortField` 等佔位內容，依理解後設計出實際查詢。

**說明：**
- 依查詢型態挑選骨架中最接近的範例方法調整：純查詢（含分頁）比照 `GetData`、新增比照 `DoInsert`、修改比照 `DoUpdate`；需要 App/Task Token 驗證的情境比照 `AppSample`/`TaskSample`。
- Controller/路由命名慣例、`FunctionType` 選擇、`ReturnNumber` 代碼、回傳鍵名慣例、分頁所需的 `@DT` 寫法等，骨架內已示範，其完整規則見 API 溝通契約文件，不在此重複。
- 前端對應的 AJAX 呼叫慣例（`clsAjaxAsync`、`url: main.clsConfig.ApiPath + '/' + {{Method Name}}`）請依頁面樣板規範文件的 AJAX 呼叫規範實作，與此處後端骨架互為一組。

