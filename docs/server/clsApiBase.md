# `clsApiBase`

檔案位置：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsApiBase.cs`

本文件描述 `clsApiBase` 的主要行為與使用慣例，供各 Controller 繼承後呼叫共用功能參考。

## 類別總覽

`clsApiBase` 繼承 `Microsoft.AspNetCore.Mvc.ControllerBase`，提供：

- 從 `Authorization` header 解析目前使用者（`GetUser`）
- 取得選單/權限資料（`Menu`）
- API 記錄（log 檔 + DB log）（`DoAppRecLog`）
- 檔案與 Base64 轉換工具（`Base64ToFile`、`GetFileBase64`、`GetUserHeadimg`）
- 登入狀態/頁面權限/操作權限檢核（`CmpSignStatus`、`CmpAuth`、`CmpAppSignStatus`）
- 共用的 `clsPostData` 解析模型與 `FunctionType` 權限列舉

> 本專案未包含派遣業務端點（`GetTaskStatus`/`GetAirportStation`/`GetCarType`/`GetOrderStatus`）與 Ollama 聊天機器人相關程式碼，下列僅描述本專案實際保留的內容。

## Header 依賴

`clsApiBase` 多數功能依賴下列 request headers：

- `Authorization`：Token 字串（用於解析 `User`）
- `Page`：目前前端頁面路徑（用於 Web 權限判斷與 log）

## Basic

### `GetUser`

```csharp
public User GetUser { get; }
```

透過 `TokenManager.GetUser(Request.Headers["Authorization"].ToString())` 解析使用者。可能回傳 `null`（未登入、token 無效、解析失敗）。

### `Menu`

```csharp
public List<Dictionary<string, object>> Menu { get; }
```

先解析 `GetUser`，若為 `null` 則回傳 `null`；否則透過 `clsPostgreDB.getList(...)` 查詢選單資料（`SYS_tbMenu`），回傳 `List<Dictionary<string, object>>`（dictionary key 不分大小寫）。

### `Base64ToFile(string code, string exFileName, string Dir = "", string setFileName = null)`

用途：將 Base64 內容寫入檔案。

- 基底路徑：`Directory.GetCurrentDirectory() + "/upload/"`
- 若 `Dir != ""`：實際寫入 `.../upload/{Dir}/`（不存在會自動建立）
- 檔名：未指定 `setFileName` 時用時間戳 `yyyyMMddHHmmssfff.{exFileName}`；有指定則用 `{setFileName}.{exFileName}`
- 成功回傳產生的 `FileName`，失敗回傳空字串

### `WsMethodName`

```csharp
public static string WsMethodName { get; }
```

透過 `StackTrace` 取得呼叫端方法名稱，格式為 `{ReflectedType}.{MethodName}`，用於 log 與權限檢核時湊出 API 名稱。

## Do（記錄與共用動作）

### `DoAppRecLog(string api, string PostData, Hashtable Res, bool isLogFile = true)`

寫入「檔案 log」與「資料庫 log（`SYS_ttLog`）」。

- 檔案 log 路徑：`{CurrentDirectory}/log/{yyyyMMdd}/{api}/{yyyyMMddHHmmssffff}.log`，內容含 `PostData` 與 `Res` 的 JSON。
- DB log 寫入 `SYS_ttLog`：`User_No`（來自 `GetUser`，無使用者時為空字串）、`WebPage`（`Request.Headers["Page"]`）、`Api`、`PostData`、`Token`（`Authorization` header）、`ReturnNumber`、`ReturnMsg`。
- 兩段皆有 try/catch 吞掉例外並寫 Console，避免影響 API 主流程回傳。

## Get（通用共用 API）

`clsApiBase` 內建的通用端點（皆繼承自 `BaseController` 或其他 Controller 呼叫）：

- `GetRouterData`、`GetMenuData`、`GetHomePage`、`GetHome`：路由/選單/首頁資料。
- `GetToken`：刷新使用者 Token（查核/註銷舊 Token、產生新 Token、寫入 `SYS_ttToken`）。
- `GetUserHeadimg(User_No)` / `GetFileBase64(FilePath)`：頭像/檔案轉 Base64（無頭像時動態產生文字大頭貼）。
- `GetPasswordValidationRules`：查詢密碼驗證規則。

## Cmp（登入狀態/權限檢核）

### `CmpSignStatus(clsPostData PostData)`（API 動作）

檢查內容：

1. `GetUser` 是否為空（否則 `N003`）
2. Token 是否已登出（查 `SYS_ttToken` 的 `SignOut is not null`）（否則 `N003`）
3. 若 `user.SystemSource == "Web"`：以 `Request.Headers["Page"]` 進行頁面權限判斷；無權限且路徑不是 `/login` 則回 `N005`
4. 成功時會在 `Res` 中加入：`user`、`HeadImg`、`Token_Time`

### `CmpAuth(clsPostData PostData)`

與 `CmpSignStatus` 類似，權限檢查來源改為 `PostData` 中的 `path`。成功時額外加入 `TokenLimit = clsConfig.TokenLimit`。

### `CmpSignStatus(string api, FunctionType FunctionType = FunctionType.Null)`（Controller 共用）

供各 Controller 在進入主要邏輯前檢查登入狀態、頁面權限、操作權限。

- 預設用 `Request.Headers["Page"]`；也會將 `api`（通常傳入 `WsMethodName`）反推出 `ApiPath`。
- 權限判斷：未登入 → `N003`；找不到對應 Menu（無頁面權限）→ `N005`。
- ⚠️ **待補**：`SYS_tbGroup`、`SYS_tbMenuGroupList` 尚未建立，`FunctionType` 對應的操作權限（新增/修改/刪除/下載）目前**未檢核**（不論傳入何種 `FunctionType`，只要頁面權限通過即放行）。待群組權限資料表建立後需補回依 `fun_{FunctionType}` 判斷的邏輯。

### `CmpAppSignStatus()`

檢查是否登入且 `user.SystemSource == "App"`，否則回 `N003`。

## `clsPostData`

用於接收前端送入的 `PostData`，提供：

- `ToHashtable()`：將 `PostData` 解析為 `Hashtable`
- `ToJson()`：取得解析後的原始 JSON 字串

解析邏輯：優先嘗試 URL decode 後直接反序列化 JSON；若失敗則視為 Base64 字串，先 base64 decode → URL decode → 反序列化 JSON。解析失敗時會回傳 `null`（例外被吞掉），呼叫端需自行做 null 防護。

## `FunctionType`

用於操作權限檢核，對應概念上的 Menu 權限欄位：`fun_Search`、`fun_Insert`、`fun_Update`、`fun_Delete`、`fun_Download`。這些欄位來自尚未建立的 `SYS_tbMenuGroupList`，`Menu` 目前不會回傳這些欄位，`CmpSignStatus(string, FunctionType)` 因此暫不執行此項檢核（見上方 Cmp 章節說明）。

## `clsMenu`

`clsMenu` 內含前端選單使用的模型：`MenuItem`（含 `subItems: List<SubItem>`）、`SubItem`，內容主要由 `Menu` 查詢結果轉換而來。
