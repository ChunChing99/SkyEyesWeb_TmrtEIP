# `clsResult`

檔案位置：`SkyEyesWeb_TmrtEIP.Server/App_Code/clsResult.cs`

本文件描述 `clsResult` 的使用慣例與行為，供各 API 回傳統一格式時參考。

## 類別總覽

`clsResult` 繼承自 `System.Collections.Hashtable`，用作 API 的標準回傳容器（response envelope）。

呼叫端可用 `Res["Key"]` 的方式讀寫內容，例如：

- `Res["Result"]`：是否成功（`bool`）
- `Res["ReturnNumber"]`：訊息代碼（`string`，例如 `"N000"`）
- `Res["ReturnMsg"]`：訊息內容（`string`，例如 `"正常"`）
- `Res["data"]`、`Res["DataCount"]`、`Res["PageCount"]`：各 API 自行擴充

> 注意：`clsResult` 本身是 `Hashtable`，欄位並沒有編譯期型別保護，建議各 API 回傳欄位命名保持一致。

## 建構子行為

```csharp
public clsResult()
{
    this.SetMsgNumber = MsgNumber.N000;
}
```

建立 `clsResult` 時，會立即設定 `SetMsgNumber = N000`，因此預設狀態為：

- `ReturnNumber = "N000"`
- `Result = true`
- `ReturnMsg = "正常"`

## `SetMsgNumber`（重要：具有副作用）

```csharp
public MsgNumber SetMsgNumber
{
    set
    {
        this["ReturnNumber"] = value.ToString();
        this["Result"] = false;
        switch (value)
        {
            case MsgNumber.N000:
                this["Result"] = true;
                this["ReturnMsg"] = "正常";
                break;
            case MsgNumber.N001:
                this["ReturnMsg"] = "驗證碼錯誤";
                break;
            ...
        }
    }
}
```

### 規則

1. 每次設定 `SetMsgNumber` 都會覆寫：
   - `ReturnNumber`
   - `Result`
   - `ReturnMsg`（除了 `N999`）
2. `Result` 預設會被設為 `false`，**只有 `N000` 會將 `Result` 改為 `true`**。
3. `N999` 只設定 `ReturnNumber` 與 `Result=false`，**不主動設定 `ReturnMsg`**。
   - 因此呼叫端常見做法是在 catch 中自行設定 `Res["ReturnMsg"] = ex.Message`。

## `ThrowError(MsgNumber)`

```csharp
public void ThrowError(MsgNumber SetMsgNumber)
{
    this.SetMsgNumber = SetMsgNumber;
    throw new Exception(this["ReturnMsg"].ToString());
}
```

用途：

- 快速設定錯誤碼並拋出對應例外，中斷後續流程。

注意：

- 若傳入 `N999`，因 `ReturnMsg` 不會自動被賦值，`this["ReturnMsg"]` 可能為 `null`，可能導致 `NullReferenceException`。
- 因此 `ThrowError` 較適合搭配 `N001~N099` 這類有明確固定訊息的代碼。

## `MsgNumber` 定義

`MsgNumber` 以字串形式寫入 `ReturnNumber`，常用值：

| MsgNumber | Result | ReturnMsg（預設） |
|---|---:|---|
| `N000` | `true` | `正常` |
| `N001` | `false` | `驗證碼錯誤` |
| `N002` | `false` | `登入失敗` |
| `N003` | `false` | `未登入或登入逾時` |
| `N004` | `false` | `無權限進行操作` |
| `N005` | `false` | `無權限進行此頁` |
| `N099` | `false` | `無效或逾時Token` |
| `N999` | `false` |（未設定，通常由呼叫端覆寫）|

## 常見使用模式

### 1) 成功回傳

```csharp
var res = new clsResult();
// 預設已是 N000
res["data"] = data;
return res;
```

### 2) Controller 常見 try/catch 模式

```csharp
clsResult Res = CmpSignStatus(WsMethodName, FunctionType.Search);

try
{
    if (!(bool)Res["Result"]) throw new Exception(Res["ReturnMsg"].ToString());

    // ...do something...
    Res["data"] = data;
}
catch (Exception ex)
{
    if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
    Res["ReturnMsg"] = ex.Message != "" ? ex.Message : ex.ToString();
}
```

說明：

- 若前置權限檢查已失敗，`Res["Result"]` 通常已為 `false` 且已有對應 `ReturnNumber/ReturnMsg`，catch 內不會再覆寫成 `N999`。
- 若原本是成功狀態（`Result=true`）卻進入 catch，則會將錯誤碼改為 `N999`，並以例外訊息覆寫 `ReturnMsg`。
