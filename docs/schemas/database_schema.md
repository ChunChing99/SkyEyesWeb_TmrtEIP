# 資料庫 Schema 說明

> ⚠️ **資料庫引擎：PostgreSQL**：本專案資料庫引擎已由 SQL Server 改為 **PostgreSQL**，下列型別以 PostgreSQL 型別描述。後端查詢執行層已改用 `Application/Common/Infrastructure/Data/clsPostgreDB.cs`（`Npgsql`），程式碼中既有 SQL 已改寫為 PostgreSQL 語法，詳見 `docs/server/README.md` 的「資料庫引擎（PostgreSQL）」章節與 `docs/server/clsPostgreDB.md`。⚠️ 尚未在真正的 PostgreSQL 伺服器上驗證過（本機開發環境無可用的 PostgreSQL 執行個體），僅完成程式碼層級的語法轉換與編譯驗證；已停用的 SQL Server 實作（`clsDapper.cs`）仍保留在專案中供參考，詳見 `docs/server/clsDapper.md`。

## 系統資料表

### SYS_tbMenu (選單資料表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | integer | - | NO | - | 流水號 (PK) |
| Type | varchar | 10 | NO | 'app' | 型態 |
| Name | varchar | 20 | YES | - | 節點名稱 |
| Path | varchar | 50 | YES | - | 節點路徑 |
| Ord | integer | - | NO | 0 | 排序 |
| ParentId | integer | - | NO | 0 | 父節點id (FK: SYS_tbMenu.Id) |
| Icon | varchar | 50 | YES | - | Icon路徑 |
| NewWindow | varchar | 1 | NO | 'N' | 是否開新分頁 |
| Enabled | varchar | 1 | NO | 'Y' | 是否啟用 |
| Visible | varchar | 1 | NO | 'Y' | 是否顯示 |
| Global | varchar | 1 | NO | 'N' | 是否為全域 |
| Auth | varchar | 1 | NO | 'Y' | 是否驗證登入的狀態 |
| CreateDate | timestamp | - | NO | now() | 新增時間 |

### SYS_tbPasswordValidationRules (密碼驗證規則表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | integer | - | NO | - | 流水號 (PK) |
| RuleName | varchar | 100 | NO | - | 名稱 |
| MinLength | integer | - | NO | 8 | 密碼最小長度 |
| PasswordChangeFrequencyDays | integer | - | NO | 90 | 密碼更新頻率 |
| MinUppercaseLength | integer | - | NO | 1 | 大寫英文最小長度 |
| MinLowercaseLength | integer | - | NO | 1 | 小寫英文最小長度 |
| MinSpecialCharLength | integer | - | NO | 1 | 特殊符號最小長度 |
| MinNumericLength | integer | - | NO | 1 | 數字最小長度 |
| TokenLimit | integer | - | NO | 480 | Token期限(分鐘) |
| MaxFailedLogin | integer | - | NO | 3 | 登入失敗次數上限 |
| AccountLockoutTime | integer | - | NO | 30 | 帳號鎖定時間(分鐘) |
| IsActive | boolean | - | NO | true | 是否啟用 |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | YES | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

## 編碼/序號管理資料表

各模組（表單、電子簽核、知識管理…）產生單據編號、圖檔編號時，統一透過此紀錄表發號，而非各模組自行 `DateTime.Now.ToString()` 拼字串；對應的發號服務規劃放在 `Application/Common` 底下（介面命名 `ICodeGeneratorService`，尚未實作）。

編碼格式樣板（如 `{DocType}-{OrgCode}-{Date}-{Seq}`）、補零位數等「規則」**不做成資料庫可配置項目，改由程式碼（常數/小型 enum 或 dictionary）定義**，理由：新增一個模組/單據類型本來就得改程式碼，格式規則沒有理由脫離程式碼獨立存在；目前也沒有規劃對應的後台設定畫面，做成 DB 配置只是多一張沒人會用的表，還多一處「DB 配置 vs 程式碼常數」的真相來源風險。`PeriodKey` 的歸零週期統一固定為「日」，格式固定為 `YYYYMMDD`，不再依模組彈性調整為 `YYYY`/`YYYYMM`，避免同一欄位長度隨模組不同而變動。

### SYS_ttIssuedSerialNumber (已發行序號紀錄表)

不是計數器，而是**每發行一個號碼就是一列**：

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DocType | varchar | 4 | NO | - | 業務/單據類型碼 (PK)，2~4 碼，如 `PA`（採購單）、`DRAW`（工程圖紙）、`LEAVE`（請假單） |
| OrgCode | varchar | 10 | NO | '' | 發起組織/部門代碼 (PK)，選填，如 `HQ`、`RD`、`TC`；不分部門的模組留空字串（哨兵值，比照 `SYS_tbMenu.ParentId=0` 慣例，非 NULL 以維持複合主鍵可用） |
| PeriodKey | varchar | 8 | NO | - | 日期序列碼 (PK)，固定 8 碼，格式統一為 `YYYYMMDD`（每日歸零重新編號） |
| Seq | integer | - | NO | - | 動態流水號 (PK)，本次實際發出的號碼 |
| IssuedCode | varchar | 50 | NO | - | 完整編號字串，如 `PA-RD-20260911-0042`；保留當時實際發出的樣子，即使日後格式樣板改變也不影響已發行的歷史紀錄 |
| IssuedUser_No | varchar | 20 | YES | - | 開立帳號；邏輯參照 `SYS_tbUser.User_No`（比照 `SYS_ttLog.User_No` 慣例，背景排程觸發時可為空，不建立強制 FK） |
| IssuedTime | timestamp | - | NO | now() | 發行時間 |

PK 為 `(DocType, OrgCode, PeriodKey, Seq)` 四欄複合鍵——直接對應編碼規則的四段組成，同一個號碼不可能被插入兩列，**由資料庫本身的主鍵約束物理上保證不會重複發放**，不再依賴應用層正確執行原子遞增。這張表本身就是完整的發行歷程，`IssuedUser_No`/`IssuedTime` 逐筆記錄「誰在何時拿到哪個號碼」，不需要再另外查 `SYS_ttLog` 拼湊。

> **取號方式**：沒有另外維護的計數器，取下一個號碼時查詢目前週期已發過的最大 `Seq`、加 1、嘗試插入；若與其他並發請求衝突（`ON CONFLICT DO NOTHING` 沒有插入任何列）就重試一次：
> ```sql
> INSERT INTO SYS_ttIssuedSerialNumber (DocType, OrgCode, PeriodKey, Seq, IssuedCode, IssuedUser_No, IssuedTime)
> SELECT @DocType, @OrgCode, @PeriodKey,
>        COALESCE(MAX(Seq), 0) + 1,
>        @IssuedCodeTemplate, -- 應用層依補零規則組出的完整編號字串
>        @User_No,
>        now()
> FROM SYS_ttIssuedSerialNumber
> WHERE DocType = @DocType AND OrgCode = @OrgCode AND PeriodKey = @PeriodKey
> ON CONFLICT (DocType, OrgCode, PeriodKey, Seq) DO NOTHING
> RETURNING Seq;
> ```
> 若 `RETURNING` 沒有回傳任何列，代表跟別的並發請求撞號，應用層重新執行同一句語句即可（此時會看到剛剛那筆已提交的資料，重新算出下一個號碼）。`(DocType, OrgCode, PeriodKey, Seq)` 已是 PK 索引的前綴，`MAX(Seq)` 是有索引支援的範圍掃描，不是全表掃描，效能無虞；目前規模也不需要 Redis 等額外元件。
>
> **斷號/補號原則**（設計指引，非資料庫強制）：正式簽核/會計類單據採「不可跳號、不可補號」——即使單據作廢，已發出的編號依然佔用，只在對應業務單據上把狀態改為「已作廢」，藉此維持稽核軌跡完整；草稿階段不應呼叫發號服務取得正式編號，應先用暫時識別碼，待正式送出時才呼叫 `ICodeGeneratorService` 取得正式編號。

## 文件檔案管理資料表

### DOC_tbDocument (表單／文件主檔)

每筆代表一份表單或文件；檔案本體存放於檔案儲存服務，資料庫保存文件資訊及各版本的儲存位置。文件編號沿用 `SYS_ttIssuedSerialNumber`，`DocType` 固定為 `'DOC'`；`DocumentId` 為四欄複合 PK 串接而成的衍生唯一鍵。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DocType | varchar | 4 | NO | 'DOC' | 文件類型碼 (PK)，固定為 `'DOC'` |
| OrgCode | varchar | 10 | NO | '' | 組織／部門代碼 (PK)，不分部門時為空字串 |
| PeriodKey | varchar | 8 | NO | - | 日期序列碼 (PK)，格式 `YYYYMMDD` |
| Seq | integer | - | NO | - | 動態流水號 (PK) |
| DocumentId | varchar | 40 | NO | (計算欄位) | `DocType-OrgCode-PeriodKey-Seq`，由前四欄自動計算產生，並建立 `UNIQUE` 約束 |
| DocumentName | varchar | 100 | NO | - | 表單／文件名稱 |
| Description | varchar | 500 | YES | - | 文件說明 |
| LatestVersionNo | integer | - | YES | - | 目前主要發行版本號 (與 `DocumentId` 組成 FK: `DOC_ttDocumentVersion.(DocumentId, VersionNo)`)；尚未發行時為 `NULL` |
| Enabled | boolean | - | NO | true | 是否啟用 |
| insert_dt | timestamp | - | NO | now() | 建立時間 |
| insert_user | varchar | 20 | YES | - | 建立人員 |
| update_dt | timestamp | - | YES | - | 最後更新時間 |
| update_user | varchar | 20 | YES | - | 最後更新人員 |

> PK 為 `(DocType, OrgCode, PeriodKey, Seq)`，並以相同四欄參照 `SYS_ttIssuedSerialNumber`；`DocumentId` 是供版本表使用的衍生唯一鍵。`LatestVersionNo` 指向目前主要發行版本，尚無已發行版本時為 `NULL`。發布版本與更新主檔指標應在同一交易內完成；應用層須確認指標指向的版本已發行，且為該文件最新的已發行版本。

### DOC_ttDocumentVersion (檔案版本紀錄表)

每列代表一個文件版本。版本發行後視為歷程資料，不覆寫已發行檔案；新版本另新增一列，再更新 `DOC_tbDocument.LatestVersionNo`。`StorageKey` 指向檔案儲存服務中的檔案，不將二進位內容存入此表。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DocumentId | varchar | 40 | NO | - | 所屬文件代碼 (PK，FK: `DOC_tbDocument.DocumentId`) |
| VersionNo | integer | - | NO | - | 文件版本號 (PK)，由 1 起算 |
| FileName | varchar | 255 | NO | - | 原始檔名 |
| StorageKey | varchar | 500 | NO | - | 檔案儲存服務中的物件鍵或路徑 |
| ContentType | varchar | 100 | YES | - | MIME 類型 |
| FileSize | bigint | - | NO | 0 | 檔案大小 (bytes) |
| Sha256 | varchar | 64 | NO | - | 檔案內容 SHA-256 雜湊值 |
| Published_dt | timestamp | - | YES | - | 發行時間；`NULL` 表示草稿，非 `NULL` 表示已發行 |
| Published_user | varchar | 20 | YES | - | 發行人員 |
| insert_dt | timestamp | - | NO | now() | 建立時間 |
| insert_user | varchar | 20 | YES | - | 建立人員 |
| update_dt | timestamp | - | YES | - | 最後修改時間 |
| update_user | varchar | 20 | YES | - | 最後修改人員 |

> PK 為 `(DocumentId, VersionNo)`；`DocumentId` 參照 `DOC_tbDocument.DocumentId`。`VersionNo` 必須大於 0，`FileSize` 不得小於 0。草稿可編輯；已發行版本不可覆寫或刪除，修改時須新增版本。`LatestVersionNo` 與版本列間的複合 FK 保證指向同一份文件的既有版本；是否已發行及是否為最新已發行版本，由應用層在發布交易中驗證。

### DOC_ttDocumentAttachment (文件附件關聯表)

關聯文件的特定版本與任意模組的業務資料，支援同一文件版本供多筆業務資料引用，也支援一筆業務資料附加多個文件版本。`BizTableName`／`BizDataId` 是多型關聯，因目標資料表不固定而不建立實體 FK；`BizDocNo` 僅供顯示或查詢，不作為關聯鍵。附件固定指向版本，避免文件新增版本後回溯改變既有簽核或業務資料所引用的檔案內容。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DocumentId | varchar | 40 | NO | - | 文件代碼 (PK，與 `VersionNo` 組成 FK: `DOC_ttDocumentVersion.(DocumentId, VersionNo)`) |
| VersionNo | integer | - | NO | - | 附加的文件版本 (PK) |
| BizTableName | varchar | 100 | NO | - | 業務資料表名稱；由程式碼定義，非資料庫 FK |
| BizDataId | varchar | 50 | NO | - | 業務資料主鍵 ID，以文字儲存；由應用層依 `BizTableName` 解析 |
| BizDocNo | varchar | 50 | YES | - | 業務單號快照，僅供顯示或查詢，不作為關聯鍵 |
| Attached_dt | timestamp | - | NO | now() | 建立附件關聯時間 |
| Attached_user | varchar | 20 | YES | - | 建立附件關聯人員 |

> PK 為 `(DocumentId, VersionNo, BizTableName, BizDataId)`，避免同一文件版本重複關聯同一筆業務資料；複合 FK `(DocumentId, VersionNo)` 參照 `DOC_ttDocumentVersion`。解除附件時刪除關聯列，不刪除文件或版本檔案；若業務需要保留解除關聯稽核歷程，應再增加解除時間／人員欄位或另建事件紀錄。

## 人事組織資料表

### SYS_tbOrg (組織資料表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | integer | - | NO | - | 流水號 (PK) |
| Code | varchar | 20 | YES | - | 組織代碼（可對應人事系統組織代碼） |
| Name | varchar | 50 | NO | - | 組織名稱 |
| ParentId | integer | - | NO | 0 | 父組織id (邏輯上參照 SYS_tbOrg.Id；0 代表頂層組織，比照 SYS_tbMenu.ParentId 慣例，不建立強制 FK) |
| OrgType | varchar | 10 | YES | - | 組織類型（如：處/部/課/組） |
| Ord | integer | - | NO | 0 | 排序 |
| Manager_User_No | varchar | 20 | YES | - | 主管帳號 (FK: SYS_tbUser.User_No) |
| Enabled | boolean | - | NO | true | 是否啟用 |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

### SYS_tcRank (職等代碼表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | integer | - | NO | - | 流水號 (PK) |
| Code | varchar | 20 | YES | - | 職等代碼（如：薦8） |
| Name | varchar | 50 | NO | - | 職等名稱（如：薦任八職等） |
| Level | integer | - | NO | 0 | 職等層級數值，數字越大代表職等越高，供簽核層級比較、排序使用 |
| Enabled | boolean | - | NO | true | 是否啟用 |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

> `SYS_tcRank` 是「命名規則」中 `SYS_tc`（系統代碼表）前綴的第一個實例。

### SYS_tbPosition (職稱資料表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | integer | - | NO | - | 流水號 (PK) |
| Code | varchar | 20 | YES | - | 職稱代碼 |
| Name | varchar | 50 | NO | - | 職稱名稱 |
| Rank_id | integer | - | YES | - | 所屬職等id (FK: SYS_tcRank.Id)。一個職等可對應多個職稱；職稱本身不直接帶數值層級 |
| Ord | integer | - | NO | 0 | 排序 |
| Enabled | boolean | - | NO | true | 是否啟用 |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

### SYS_tbGroup (權限群組資料表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | integer | - | NO | - | 流水號 (PK) |
| Code | varchar | 20 | YES | - | 群組代碼 |
| Name | varchar | 50 | NO | - | 群組名稱 |
| Description | varchar | 200 | YES | - | 群組說明 |
| Enabled | boolean | - | NO | true | 是否啟用 |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

### SYS_tbUser (使用者/員工資料表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| User_No | varchar | 20 | NO | - | 帳號/工號 (PK) |
| User_Name | varchar | 50 | NO | - | 姓名 |
| Password | varchar | 255 | YES | - | 密碼（雜湊後儲存） |
| Org_id | integer | - | YES | - | 所屬組織id (FK: SYS_tbOrg.Id) |
| Position_id | integer | - | YES | - | 職稱id (FK: SYS_tbPosition.Id) |
| UserGroup_id | integer | - | YES | - | 權限群組id (FK: SYS_tbGroup.Id) |
| Manager_User_No | varchar | 20 | YES | - | 直屬主管帳號 (FK: SYS_tbUser.User_No，自我參考；比照 SYS_tbOrg.Manager_User_No，以 ALTER TABLE 補上約束) |
| User_Tel | varchar | 20 | YES | - | 聯絡電話 |
| Email | varchar | 100 | YES | - | 電子郵件 |
| SystemSource | varchar | 10 | YES | - | 系統來源（如 Web/App） |
| UserType | varchar | 10 | YES | - | 使用者類別 |
| IsActive | boolean | - | NO | true | 是否在職/啟用 |
| HireDate | date | - | YES | - | 到職日 |
| ResignDate | date | - | YES | - | 離職日 |
| FailedLoginCount | integer | - | NO | 0 | 登入失敗次數（配合 SYS_tbPasswordValidationRules.MaxFailedLogin） |
| LockoutEndDate | timestamp | - | YES | - | 帳號鎖定解除時間 |
| PasswordUpdateDate | timestamp | - | YES | - | 密碼最後更新時間（配合 PasswordChangeFrequencyDays 判斷是否需強制改密碼） |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

> ⚠️ `TokenModels.User`（[`clsToken.cs`](../../SkyEyesWeb_TmrtEIP.Server/Application/Common/Infrastructure/Token/clsToken.cs)）目前的 `UserGroup_id` 為 `string` 型別，與此處設計的 `integer` FK 型別不同；待後續開發登入/使用者管理功能、實際串接本資料表時，應用層程式碼需一併調整型別轉換方式，屬應用層待辦事項，非資料庫設計限制。

## 代理人資料表

員工請假/出差期間，可設定代理人代為處理待辦事項與簽核；供電子簽核模組等判斷「目前這筆待辦該轉給誰」使用。

### SYS_tbDelegate (代理人設定表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | integer | - | NO | - | 流水號 (PK) |
| Delegator_User_No | varchar | 20 | NO | - | 被代理人（委任人）帳號 (FK: SYS_tbUser.User_No) |
| Delegate_User_No | varchar | 20 | NO | - | 代理人帳號 (FK: SYS_tbUser.User_No) |
| ModuleCode | varchar | 10 | YES | - | 代理範圍模組代碼；空值代表全模組皆代理，非空則只代理該模組（如電子簽核 `WF`）。模組清單由程式碼定義，非資料庫 FK |
| StartDate | timestamp | - | NO | - | 代理生效時間 |
| EndDate | timestamp | - | NO | - | 代理結束時間；提前結束代理時直接更新此欄位，不另設狀態機 |
| Reason | varchar | 100 | YES | - | 代理原因（如：請假、出差、其他） |
| Enabled | boolean | - | NO | true | 是否啟用；管理者可隨時整組停用，不需連動修改日期 |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

> 「目前是否代理中」＝ `Enabled = true AND now() BETWEEN StartDate AND EndDate`。約束：`Delegate_User_No <> Delegator_User_No`（不能代理自己）、`EndDate >= StartDate`。同一被代理人可能同時對不同 `ModuleCode` 設定不同代理人（例如工作代理給 A、簽核代理給 B），因此不用複合唯一鍵限制同期間只能有一筆，重疊時段的合理性由應用層判斷。多層代理鏈（A 代理 B、B 又代理 C 時待辦該轉給誰）也是應用層解析範圍，不在資料庫用遞迴表示。

## 電子簽核資料表

### WF_tbDefine (流程定義主檔／版本索引表)

電子簽核流程定義的主檔與版本索引；每筆資料代表一個流程定義，`VersionNo` 記錄目前正式使用的版本號。`VersionNo` 為 `NULL` 表示尚未有正式版本；一旦填值，必須指向同一 `DefineId` 下存在且 `Published_dt IS NOT NULL` 的版本。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DocType | varchar | 4 | NO | 'WFD' | 樣板類型碼 (PK)，固定為 `'WFD'` |
| OrgCode | varchar | 10 | NO | '' | 組織／部門代碼 (PK)，不分部門時為空字串 |
| PeriodKey | varchar | 8 | NO | - | 日期序列碼 (PK)，依歸零週期產生：`YYYYMMDD` |
| Seq | integer | - | NO | - | 動態流水號 (PK) |
| DefineId | varchar | 40 | NO | (計算欄位) | `DocType-OrgCode-PeriodKey-Seq`，由前四欄自動計算產生，並建立 `UNIQUE` 約束 |
| VersionNo | integer | - | YES | - | 目前正式使用且已發行的版本號；必須指向 `WF_tbDefineProcess` 中 `Published_dt IS NOT NULL` 的同一版本；`NULL` 表示尚未發行 |
| insert_dt | timestamp | - | NO | now() | 建立時間 |
| insert_user | varchar | 20 | YES | - | 建立人員 |
| update_dt | timestamp | - | YES | - | 最後更新時間 |
| update_user | varchar | 20 | YES | - | 最後更新人員 |

> PK 為 `(DocType, OrgCode, PeriodKey, Seq)` 四欄複合鍵；`DefineId` 為 PK 四欄以 `-` 串接而成的衍生欄位，供後續流程定義版本資料或流程實例以單欄 FK 參照。`VersionNo` 若非 `NULL`，必須指向同一 `DefineId` 下存在且已發行的 `WF_tbDefineProcess` 版本；尚未發行時為 `NULL`。此規則由資料庫 trigger 驗證。

### WF_tbDefineProcess (流程定義樣板表)

`WF_tbDefine` 的一對多版本內容表；每筆資料代表某一流程定義的一個版本，保存該版本的流程名稱與說明。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DefineId | varchar | 40 | NO | - | 流程定義代碼 (PK，FK: `WF_tbDefine.DefineId`) |
| VersionNo | integer | - | NO | - | 流程版本號 (PK)，由 1 起算 |
| ProcessName | varchar | 100 | NO | - | 流程名稱 |
| ProcessDesc | varchar | 500 | YES | - | 流程說明 |
| Published_dt | timestamp | - | YES | - | 發行時間；`NULL` 表示尚未發行；非 `NULL` 表示版本曾被發行，之後不可編輯 |
| Published_user | varchar | 20 | YES | - | 發行人員 |
| insert_dt | timestamp | - | NO | now() | 建立時間 |
| insert_user | varchar | 20 | YES | - | 建立人員 |
| update_dt | timestamp | - | YES | - | 最後修改時間 |
| update_user | varchar | 20 | YES | - | 最後修改人員 |

> PK 為 `(DefineId, VersionNo)`；`DefineId` 對 `WF_tbDefine.DefineId` 建立實體 FK，確保版本必隸屬既有流程定義。`Published_dt IS NULL` 時為可編輯草稿；發行時應在同一交易內寫入 `Published_dt`／`Published_user` 並更新 `WF_tbDefine.VersionNo`，且 `WF_tbDefine.VersionNo` 只能指向這筆已發行版本。`Published_dt` 非 `NULL` 代表此版本曾被發行；已發行版本與其關聯節點、簽核人設定均不得編輯或更新，版本切換、修改與發行規則一致由系統應用層負責。若需修改，必須新增下一個 `VersionNo`。

### WF_tbDefineNode (流程定義關卡節點表)

電子簽核的樣板節點；每一列代表某個流程版本下的一個簽核關卡。`PrevNodeCode` 以自我參考方式表示父節點；同一個父節點可以有多個子節點，因此可表示平行分支。`NodeSeq` 由系統依流程圖拓撲排序後回填，僅供查詢或顯示，不是流程關係的唯一來源。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DefineId | varchar | 40 | NO | - | 流程定義代碼 (PK，FK: `WF_tbDefineProcess.DefineId`) |
| VersionNo | integer | - | NO | - | 流程版本號 (PK，FK: `WF_tbDefineProcess.VersionNo`) |
| NodeCode | varchar | 20 | NO | - | 關卡代碼 (PK)，同一流程版本下唯一；格式為 `A`~`ZZ`，配合 `PrevNodeCode` 使用 |
| NodeName | varchar | 100 | NO | - | 關卡顯示名稱 |
| PrevNodeCode | varchar | 20 | YES | - | 上一流程節點；同一流程版本的另一個 `NodeCode`，起始關卡為 `NULL` |
| RejectNodeCode | varchar | 20 | YES | - | 退回流程節點；同一流程版本的另一個 `NodeCode`；`NULL` 時交由簽核引擎預設處理 |
| NodeSeq | integer | - | NO | - | 拓撲排序快取；由系統依流程圖計算後填入，同一版本下不重複 |
| NodeType | varchar | 20 | NO | - | 節點行為／流程控制類型：`Start`、`Approve`、`Countersign`、`Notify`、`Gateway`、`Initiator`、`End` |
| ApproveMode | varchar | 20 | YES | - | 決策／過關規則：`Single`、`AndSign`、`OrSign`；`Start`／`Notify`／`Gateway`／`Initiator`／`End` 不適用時為 `NULL` |
| TimeoutDays | integer | - | NO | 7 | 逾時天數；超過後觸發 `TimeoutAction` |
| TimeoutAction | varchar | 20 | NO | 'RETURN_TO_INITIATOR' | 逾時處理類型：`RETURN_TO_INITIATOR`、`FORWARD_TO_SUPERIOR`、`AUTO_APPROVE_NEXT` |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

> PK 為 `(DefineId, VersionNo, NodeCode)`，前兩欄整組對 `WF_tbDefineProcess` 建立實體 FK，確保關卡屬於指定流程版本。`PrevNodeCode`／`RejectNodeCode` 以 `(DefineId, VersionNo, NodeCode)` 對本表建立可延遲的複合自我 FK，確保節點只能連至同一版本。多個節點可以擁有相同的 `PrevNodeCode`，表示同一父節點展開多個平行分支。

> `PrevNodeCode` 只能表示每個節點的一個直接父節點；平行分支的合流以分支下游的 `Gateway` 節點作為合流標記。流程推進至等待合流時，`WF_ttProcess.CurrentNodeCode` 指向該執行期 `Gateway` 節點；Gateway 不配置簽核人，是否所有平行分支均已完成、何時通過 Gateway，以及通過後要推進至哪個下游節點，皆由系統應用層依流程圖在同一交易中判斷與更新。資料庫只負責確保 Gateway 與其節點關係存在，不以 `NodeSeq` 取代合流判斷。
>
> **節點行為／流程控制類型 (NodeType)**：`Start`（簽核發起）為流程起點，由系統自動通過且不配置簽核人；`Approve`（審核）為阻塞式關卡，需完成才可推進；`Countersign`（會辦）為多部門平行或平行分支關卡；`Notify`（知會）為非阻塞分支，發出通知後主流程直接通過、不等待回覆；`Gateway`（網關）為純邏輯分流或合流點，無簽核人；`Initiator`（發起人處理）通常在正常流程中自動通過，但流程由其他節點退件至此時，改由發起人處理，可選擇修改後重送、取消流程或依系統提供的其他處置方式，完成處理後再進入其下一個節點；`End`（結案）為流程終點，由系統自動完成且不配置簽核人。本表以 `CHECK` 約束限制上述七種值；`Start`、`Gateway`、`Initiator`、`End` 不可於 `WF_tbDefineNodeApprover` 配置簽核人。
>
> **決策／過關規則 (ApproveMode)**：`Single`（單人）代表關卡預設只有一位簽核人；`AndSign`（會簽／全過）代表名單上所有人都須同意；`OrSign`（或簽／一人過）代表任一人同意後關卡即完成，其餘人自動設為 `Skipped`。`Approve`／`Countersign` 節點必須指定其中一種規則；`Start`／`Notify`／`Gateway`／`Initiator`／`End` 不適用，固定為 `NULL`。
>
> `NodeSeq` 是流程圖的拓撲排序快取：新增、刪除或調整分支時，應用層必須重新計算並回填；資料庫以 `UNIQUE (DefineId, VersionNo, NodeSeq)` 防止同一版本出現重複顯示排序值，但平行分支的先後不代表簽核依序。已發行版本的節點不得編輯或更新，相關版本控制由應用層負責；若需修改流程，必須建立新的 `VersionNo`。

> **流程循環與可完成性**：`PrevNodeCode`／`RejectNodeCode` 允許形成流程循環，這是系統開放式流程設計的一部分，資料庫不以 constraint 或 trigger 禁止循環。應用層應提供流程可完成性分析，例如找出可能無法抵達終止狀態的路徑，並將分析結果與警示提供給流程設計師；是否修正設計、是否發行版本，以及設計是否完整，均由流程設計師決定。`NodeSeq` 僅供查詢、顯示或應用層推演使用，不代表流程必須為無環圖。

### WF_tbDefineNodeApprover (流程定義關卡簽核人表)

`WF_tbDefineNode` 的一對多子表；一個 `Approve` 或 `Countersign` 關卡可對應多筆簽核角色或人員設定。`Notify` 節點則以本表設定被知會的角色或人員，系統僅發出通知、不等待回覆。`Start`、`Gateway`、`Initiator`、`End` 為流程控制或發起人處理節點，不配置簽核角色或人員。只有一位簽核人時，本表就只有一筆列。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DefineId | varchar | 40 | NO | - | 流程定義代碼 (PK，FK: `WF_tbDefineNode.DefineId`) |
| VersionNo | integer | - | NO | - | 流程版本號 (PK，FK: `WF_tbDefineNode.VersionNo`) |
| NodeCode | varchar | 20 | NO | - | 關卡代碼 (PK，FK: `WF_tbDefineNode.NodeCode`) |
| ApproverSeq | integer | - | NO | - | 簽核人序號 (PK)，同一關卡下由 1 起算；僅代表資料列識別，不代表簽核順序 |
| ApproveRole | varchar | 20 | YES | - | `Approve`／`Countersign` 時為簽核角色代碼；`Notify` 時為被知會角色代碼；由程式碼定義，非資料庫 FK |
| ApproveUser_No | varchar | 20 | YES | - | `Approve`／`Countersign` 時為簽核人員帳號；`Notify` 時為被知會人員帳號 (FK: `SYS_tbUser.User_No`) |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

> PK 為 `(DefineId, VersionNo, NodeCode, ApproverSeq)`，前 3 欄整組對 `WF_tbDefineNode` 建立實體 FK，表示該筆設定所屬的流程版本與關卡。`Approve`／`Countersign` 節點中，`ApproveRole`／`ApproveUser_No` 二者至少填一：指定角色時，由簽核當下依角色動態解析實際簽核人；指定人員時，則固定由該員簽核；同一列也可兩者都填，表示限定某角色中的特定人員。`Notify` 節點中，這兩欄改表示被知會對象，系統依角色解析或直接通知指定人員，不建立簽核待辦、也不等待回覆。`Start`／`Gateway`／`Initiator`／`End` 不建立本表資料；`Initiator` 只有在退件進入發起人處理時由應用層建立待辦或作業狀態，完成後依流程定義進入下一節點。已發行版本的設定不得編輯或更新，版本控制由應用層負責。

### WF_tbDefineField (流程定義動態表單欄位表)

`WF_tbDefineProcess` 的一對多子表；每一列代表某個流程版本表單上的一個欄位（PrimeVue 4.5.5 元件），並以 12 分割格線自訂欄位寬度。表單設計隨流程版本保存，版本發行後不可修改。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DefineId | varchar | 40 | NO | - | 流程定義代碼 (PK，FK: `WF_tbDefineProcess.DefineId`；邏輯上關聯 `WF_tbDefine.DefineId`) |
| VersionNo | integer | - | NO | - | 流程版本號 (PK，FK: `WF_tbDefineProcess.VersionNo`) |
| FieldCode | varchar | 30 | NO | - | 欄位代碼 (PK)，同一流程版本下唯一；僅限英數與底線，作為表單資料 key |
| FieldLabel | varchar | 100 | NO | - | 欄位顯示名稱 |
| FieldType | varchar | 20 | NO | - | 元件類型：`InputText`、`InputNumber`、`Select`、`Textarea`、`DatePicker`；以 `CHECK` 限制 |
| DateMode | varchar | 10 | YES | - | 僅 `FieldType = 'DatePicker'` 必填：`Date`（日期）、`DateTime`（日期時間）、`Time`（時間）；其他類型必須為 `NULL` |
| ColSpan | smallint | - | NO | 12 | 桌面版寬度，佔 12 分割中的格數，`CHECK (ColSpan BETWEEN 1 AND 12)` |
| ColSpanMd | smallint | - | YES | - | 平板寬度（1~12）；`NULL` 時沿用 `ColSpan` |
| ColSpanSm | smallint | - | YES | - | 手機寬度（1~12）；`NULL` 時預設 12 |
| MaxLength | smallint | - | NO | 200 | 資料長度上限，`CHECK (MaxLength BETWEEN 1 AND 200)`；`InputText`／`Textarea` 作為字元上限，`Select`／`DatePicker` 值轉字串存放亦不得超過此長度 |
| IsRequired | boolean | - | NO | false | 是否必填 |
| Placeholder | varchar | 100 | YES | - | 提示文字 |
| DefaultValue | varchar | 200 | YES | - | 預設值（以字串保存，依 `FieldType` 解析） |
| MinValue | numeric | - | YES | - | 僅 `InputNumber`：最小值 |
| MaxValue | numeric | - | YES | - | 僅 `InputNumber`：最大值 |
| OptionTable | varchar | 50 | YES | - | 僅 `Select`：選項來源的資料庫資料表名稱（例如 `SYS_tbCode`）；`Select` 必填，其他類型必須為 `NULL`；非 FK，由應用層白名單驗證 |
| SortSeq | integer | - | NO | - | 表單顯示排序（由 1 起算），同一版本下唯一；依序排列並依 `ColSpan` 累計滿 12 自動換行 |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

> PK 為 `(DefineId, VersionNo, FieldCode)`，前兩欄整組對 `WF_tbDefineProcess` 建立實體 FK（`ON DELETE CASCADE` 與否比照節點表）；因 `WF_tbDefineProcess.DefineId` 已 FK 至 `WF_tbDefine`，欄位亦間接隸屬該流程定義。另建 `UNIQUE (DefineId, VersionNo, SortSeq)`。已發行版本（`Published_dt IS NOT NULL`）的欄位不得編輯，需修改時新增下一個 `VersionNo` 並複製欄位，規則由應用層負責，與節點表一致。
>
> **前端對應（PrimeVue 4.5.5）**：`InputText`→`<InputText>`；`InputNumber`→`<InputNumber>`；`Select`→`<Select :options>`（選項由後端依 `OptionTable` 查詢，需限定為白名單內的資料表）；`Textarea`→`<Textarea>`；`DatePicker` 依 `DateMode`：`Date`→`<DatePicker dateFormat="yy/mm/dd">`、`DateTime`→`<DatePicker showTime hourFormat="24">`、`Time`→`<DatePicker timeOnly hourFormat="24">`。寬度以 `col-span-{ColSpan}` 搭配 `grid grid-cols-12`（Tailwind／PrimeFlex 皆可，依專案 UI 規範），`ColSpanMd`／`ColSpanSm` 對應 `md:`／`sm:` 斷點。
>
> **表單資料保存**：每個欄位值皆限 200 字元，以字串保存；日期格式 `Date`＝`YYYY-MM-DD`、`DateTime`＝`YYYY-MM-DD HH:mm`、`Time`＝`HH:mm`，`InputNumber` 以字串存十進位數值。流程實例的填寫值由 `WF_ttProcessFormValue` 以 key／value 保存，`Id` FK 至 `WF_ttProcess.Id`，並於送出時依本表定義驗證（必填、長度、數值範圍、Select 選項）。
>
> **CHECK 規則**：`DateMode` 與 `FieldType` 互斥一致；`MinValue`／`MaxValue` 僅限 `InputNumber`；`OptionTable` 僅限 `Select` 且為必填；`MinValue <= MaxValue`。

### WF_ttProcess (流程實例表)

電子簽核發起後的簽核主檔；一筆代表一次依某個 `WF_tbDefineProcess` 樣板版本實際發起的簽核流程，記錄目前進度與整體狀態。關卡層級的逐筆簽核歷程由後續執行期資料表記錄。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| DocType | varchar | 4 | NO | 'WF' | 業務／單據類型碼 (PK)，固定為 `'WF'`，與樣板的 `'WFD'` 區分 |
| OrgCode | varchar | 10 | NO | '' | 組織／部門代碼 (PK) |
| PeriodKey | varchar | 8 | NO | - | 日期序列碼 (PK)，格式 `YYYYMMDD` |
| Seq | integer | - | NO | - | 動態流水號 (PK) |
| Id | varchar | 40 | NO | (計算欄位) | `DocType-OrgCode-PeriodKey-Seq`，由前四欄自動計算產生，並建立 `UNIQUE` 約束；展示用衍生欄位，非本表 PK |
| DefineId | varchar | 40 | NO | - | 關聯使用的流程定義代碼 (FK: `WF_tbDefineProcess.DefineId`) |
| DefineVersionNo | integer | - | NO | - | 關聯使用的流程版本號 (與 `DefineId` 組成 FK: `WF_tbDefineProcess.(DefineId, VersionNo)`) |
| BizTableName | varchar | 100 | YES | - | 綁定的業務系統／表名；由程式碼定義，非資料庫 FK |
| BizDataId | varchar | 50 | YES | - | 綁定的業務資料主鍵 ID，以文字儲存 |
| BizDocNo | varchar | 50 | YES | - | 業務單號，如採購單號、請假單號 |
| Subject | varchar | 200 | NO | - | 簽核主旨 |
| Content | text | - | YES | - | 簽核內容／申請正文；保存主旨以外的詳細說明 |
| InitiatorUser_No | varchar | 20 | NO | - | 發起人帳號 (FK: `SYS_tbUser.User_No`) |
| CurrentNodeCode | varchar | 20 | YES | - | 目前停留的執行期關卡代碼；草稿狀態為 `NULL`（與 `Id` 組成 FK: `WF_ttProcessNode.(Id, NodeCode)`） |
| ProcessStatus | varchar | 20 | NO | 'DRAFT' | 流程總狀態：`DRAFT`、`IN_PROGRESS`、`APPROVED`、`WITHDRAWN` |
| StartTime | timestamp | - | YES | - | `IN_PROGRESS` 時由應用層寫入；`DRAFT` 時必須為 `NULL` |
| EndTime | timestamp | - | YES | - | `APPROVED` 或 `WITHDRAWN` 時由應用層寫入；未進入終態時為 `NULL` |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

> PK 為 `(DocType, OrgCode, PeriodKey, Seq)`，直接沿用 `SYS_ttIssuedSerialNumber` 的發號結果（`DocType='WF'`），並建立實體 FK。`Id` 為 PK 四欄串接而成的衍生欄位，供未來子表以單欄 FK 參照流程實例。
>
> `WF_tbDefineProcess` 的 PK 是 `(DefineId, VersionNo)`，單獨 `DefineId` 不唯一，因此本表除 `DefineId` 外另存 `DefineVersionNo`，以複合 FK 固定指向發起當下採用的流程版本，避免日後版本切換造成實例的樣板來源不明。流程由 `DRAFT` 正式送出時，`DefineVersionNo` 必須等於 `WF_tbDefine.VersionNo`，且該版本必須已發行；此後流程實例固定使用該版本，不會因主檔日後切換版本而改變。
>
> `CurrentNodeCode` 與 `Id` 組成複合 FK，參照 `WF_ttProcessNode.(Id, NodeCode)`，確保目前停留的關卡必屬於本流程實例底下的執行期節點。`ProcessStatus` 以 `CHECK` 約束限制合法列舉；狀態轉換規則由應用層負責。簽核人員的逐人狀態不儲存在 `WF_ttProcessNodeAssignee`，而以 append-only 的 `WF_ttProcessSign` 事件歷程為判斷依據；應用層依該歷程及節點的 `ApproveMode` 分析關卡是否完成、下一個可處理關卡為何，再更新 `WF_ttProcess.CurrentNodeCode`。`BizTableName`、`BizDataId`、`BizDocNo` 的不可變規則亦由應用層在送出後鎖定。

> **流程狀態欄位規則**：以下狀態轉換、時間欄位與目前關卡的一致性皆由系統應用層在同一交易中處理，不以本表 `CHECK` 約束取代：`DRAFT` 時 `StartTime` 與 `CurrentNodeCode` 必須為 `NULL`；正式送出進入 `IN_PROGRESS` 時寫入 `StartTime` 並設定 `CurrentNodeCode`；平行分支等待合流時，`CurrentNodeCode` 指向下游 `Gateway`，由應用層判斷所有前置分支是否完成；進入 `APPROVED` 時寫入 `EndTime`；進入 `WITHDRAWN` 時寫入 `EndTime`，並將受撤回影響的執行期節點 `NodeStatus` 更新為 `REJECTED`。

> **同時簽核處理**：同一關卡可能有多位人員同時送出簽核。資料庫以部分唯一索引保證同一流程／關卡／簽核人的第一筆人工 `APPROVE`／`REJECT`／`TRANSFER` 事件成功寫入，後續同一簽核人的人工事件因唯一性衝突而失敗；不同簽核人仍可在 `AndSign` 等模式下各自寫入有效事件。應用層只需分析當下 `WF_ttProcessSign` 的簽核事件歷程，依 `ApproveMode` 判斷節點完成條件並更新流程狀態與 `CurrentNodeCode`。

> **同一業務單據的送簽限制**：同一組 `(BizTableName, BizDataId)` 是否可以建立流程，屬於應用層業務規則，不由本表建立唯一約束。正式送簽前，應用層必須檢查該業務單據是否已有 `ProcessStatus = 'IN_PROGRESS'` 的流程；簽核進行中時，業務單據應鎖定，禁止修改、再次送簽或建立另一筆進行中的流程。業務單據進入 `APPROVED` 或 `WITHDRAWN` 等終態後，是否允許重新送簽，依各業務模組規則判斷。資料庫僅在 `(BizTableName, BizDataId)` 建立一般索引，供上述應用層檢查使用。

### WF_ttProcessFormValue (流程實例表單資料表)

`WF_ttProcess` 的一對多子表，以 key／value 保存發起簽核時填寫的動態表單資料；每一列代表一個欄位的填寫值。欄位定義來自該實例所用流程版本的 `WF_tbDefineField`。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | varchar | 40 | NO | - | 所屬流程實例鍵 (PK，FK: `WF_ttProcess.Id`) |
| FieldCode | varchar | 30 | NO | - | 欄位代碼／Key (PK)，對應 `WF_tbDefineField.FieldCode` |
| FieldValue | varchar | 200 | YES | - | 欄位值／Value；一律以字串保存，長度不超過 200 |
| FieldText | varchar | 200 | YES | - | 僅 `Select`：送出當下的選項顯示文字，避免來源資料表日後異動後無法顯示；其他類型為 `NULL` |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |
| update_dt | timestamp | - | YES | - | 更新時間（僅 `DRAFT` 階段可更新） |
| update_user | varchar | 20 | YES | - | 更新人員 |

> PK 為 `(Id, FieldCode)`，`Id` 對 `WF_ttProcess.Id` 建立實體 FK（`ON DELETE CASCADE`）；`FieldValue`、`FieldText` 以 `varchar(200)` 由資料庫限制長度。`FieldCode` 不另建 FK，因為 `WF_tbDefineField` 的 PK 含版本，需由應用層以 `WF_ttProcess.(DefineId, DefineVersionNo)` 對應查詢驗證。
>
> **寫入規則**：流程為 `DRAFT` 時可新增、修改；正式送出（進入 `IN_PROGRESS`）時，應用層必須在同一交易內依 `WF_tbDefineField` 驗證必填、長度、`InputNumber` 的 `MinValue`／`MaxValue`、`Select` 選項（`OptionTable` 白名單）與日期格式後寫入；送出後資料鎖定，不得再更新或刪除，修改須另行發起。非必填且未填寫的欄位可不建立資料列。
>
> **值格式**：`Date`＝`YYYY-MM-DD`、`DateTime`＝`YYYY-MM-DD HH:mm`、`Time`＝`HH:mm`；`InputNumber` 以十進位數值字串保存；`Select` 的 `FieldValue` 存選項值、`FieldText` 存顯示文字。

### WF_ttProcessNode (執行期關卡節點/狀態)

電子簽核發起後的實際簽核關卡：簽核發起前，由 `WF_tbDefineNode` 將樣板關卡逐筆**複製**到本表（快照，非即時查詢樣板），供流程實例執行期間使用；執行期若發生前加簽/後加簽/會辦，則在本表新增動態插入的關卡列（`IsDynamicNode = true`），並將原後繼節點的 `PrevNodeCode` 改指向新關卡，將新關卡接入原流程；若同一父節點新增多筆子節點，則形成平行分支。並行加簽則不新增關卡列，而是直接在既有關卡下的 `WF_ttProcessNodeAssignee`（節點簽核人員表，見下）加入新簽核人員，並將該關卡的 `ApproveMode` 改為 `AndSign`（會簽）。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | varchar | 40 | NO | - | 所屬流程實例鍵 (PK，FK: `WF_ttProcess.Id`) |
| NodeCode | varchar | 20 | NO | - | 關卡代碼 (PK)；簽核發起前複製自 `WF_tbDefineNode.NodeCode`，執行期動態插入的關卡則另行產生新代碼，格式同樣為 `A`~`ZZ` |
| NodeName | varchar | 100 | NO | - | 關卡顯示名稱（複製自樣板，或加簽/會辦時填入） |
| PrevNodeCode | varchar | 20 | YES | - | 上一流程節點（同一流程實例內的自我參考；起始關卡為 `NULL`）；複製自樣板後，執行期若插入前加簽/後加簽/會辦關卡，會由簽核引擎改寫為新關卡的代碼 |
| RejectNodeCode | varchar | 20 | YES | - | 退回流程節點（同一流程實例內的另一個 `NodeCode`，指定本關卡被退件時應返回的關卡）；複製自樣板的 `WF_tbDefineNode.RejectNodeCode`，或加簽/會辦時另行指定；`NULL` 表示退件時交由簽核引擎預設處理 |
| NodeSeq | integer | - | NO | - | 拓撲排序快取；由系統依 `PrevNodeCode` 與流程圖計算後填入，比照 `WF_tbDefineNode.NodeSeq` 慣例 |
| NodeType | varchar | 20 | NO | - | 節點行為／流程控制類型；樣板複製而來的關卡為 `Start`、`Approve`、`Countersign`、`Notify`、`Gateway`、`Initiator`、`End`，執行期動態插入的關卡則另為 `PRE_ADD`（前加簽）、`POST_ADD`（後加簽）、`COORDINATE`（會辦），共 10 種（並行加簽不新增關卡列，故無對應的 `NodeType` 值） |
| ApproveMode | varchar | 20 | YES | - | 決策／過關規則：`Single`、`AndSign`、`OrSign`；`Start`／`Notify`／`Gateway`／`Initiator`／`End` 及動態控制節點不適用時為 `NULL` |
| TimeoutDays | integer | - | NO | 7 | 逾時天數（複製自樣板，或加簽/會辦動作指定） |
| TimeoutAction | varchar | 20 | NO | 'RETURN_TO_INITIATOR' | 逾時處理類型；`RETURN_TO_INITIATOR`/`FORWARD_TO_SUPERIOR`/`AUTO_APPROVE_NEXT`，同 `WF_tbDefineNode.TimeoutAction` 列舉 |
| IsDynamicNode | boolean | - | NO | false | 是否為執行期臨時動態插入的節點（`true`＝簽核發起後因前加簽/後加簽/會辦新增，非樣板原有關卡） |
| NodeStatus | varchar | 20 | NO | 'PENDING' | 節點簽核狀態；`PENDING`(待處理)、`APPROVED`(已核准)、`REJECTED`(已退回)、`SKIPPED`(已跳過，如逾時自動同意)、`WITHDRAWN`(隨主流程撤回而終止) 五種 |
| GeneratedTime | timestamp | - | NO | now() | 生成時間（簽核發起時整批複製，或加簽/會辦時單筆產生的時間） |
| CompleteTime | timestamp | - | YES | - | 完成時間（進入 `APPROVED`/`REJECTED`/`SKIPPED`/`WITHDRAWN` 等終態的時間） |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

> PK 為 `(Id, NodeCode)`；`Id` 對 `WF_ttProcess.Id` 建立實體 FK（該欄為 `WF_ttProcess` 的衍生 `UNIQUE` 欄位，詳見該表說明），確保關卡節點必屬於指定的流程實例。`PrevNodeCode`／`RejectNodeCode` 則以 `(Id, NodeCode)` 對本表建立可延遲的複合自我 FK，確保前置或退回的關卡屬於同一流程實例；多個節點可以共用同一個 `PrevNodeCode` 形成平行分支。
>
> 本表**不**對 `WF_tbDefineNode` 建立實體 FK：`NodeName`/`NodeType`/`TimeoutDays`/`TimeoutAction` 皆為簽核發起當下的**複製快照**，樣板日後若被修改或發行新版本，不應回溯影響已在執行/已完成的流程實例，比照 `SYS_ttIssuedSerialNumber.IssuedCode`「保留當時實際發出的樣子」的慣例。
>
> **節點行為／流程控制類型 (NodeType) 列舉範圍**：樣板複製而來的關卡沿用 `WF_tbDefineNode` 的 `Start`(簽核發起)、`Approve`(審核)、`Countersign`(會辦)、`Notify`(知會)、`Gateway`(分流或合流)、`Initiator`(發起人處理)、`End`(結案) 七種；其中 `Start`、`Gateway`、`Initiator`、`End` 不配置簽核人，`Initiator` 在正常流程通過時直接進入下一節點，但收到退件時則等待發起人處理、重送或取消。執行期動態插入的關卡另有 `PRE_ADD`(前加簽)、`POST_ADD`(後加簽)、`COORDINATE`(會辦) 三種，正是「簽核發起後才會用到」的動態插入動作，本表就是這些動作實際發生、留下紀錄的地方，通常會同時將 `IsDynamicNode` 設為 `true`。**並行加簽不在此列舉中**：並行加簽不插入新的關卡列，只在既有關卡下的 `WF_ttProcessNodeAssignee` 加入新簽核人員、並將該關卡既有的 `NodeType`/`ApproveMode` 改為會簽（`AndSign`），因此 `NodeType` 不會出現 `PARALLEL_ADD` 這個值。
>
> 本表刻意不含逐位簽核人員欄位：一個關卡節點對應多位簽核人（`或簽`/`會簽`）的名單，比照樣板端 `WF_tbDefineNode`→`WF_tbDefineNodeApprover` 的拆分方式，另建執行期子表 `WF_ttProcessNodeAssignee`（節點簽核人員表，見下）存放。
>
> **轉簽限制**：因轉簽（`TRANSFER`）而動態插入的新關卡（`IsDynamicNode = true`），其簽核人**只能選擇同意或退件，不得再次轉簽**，避免轉簽鏈無限延伸。此為應用層規則，資料庫不另建欄位/約束限制——`IsDynamicNode` 無法單獨判斷「本節點是否為轉簽產生」，因為前加簽/後加簽/會辦同樣會將此欄位設為 `true`；判斷本節點是否轉簽而來，須回頭查執行期簽核歷程子表 `WF_ttProcessSign`（見下）及動態節點建立事件判斷。
>
> **前加簽**：原流程若為 `A-B`，應變更為 `A-(加簽)-(A+)-B`，在原 `A` 節點下游插入一個加簽節點與一個 `A+` 節點。
>
> **後加簽**：原流程若為 `A-B`，應變更為 `A-(加簽)-B`，在原 `A` 節點下游插入一個加簽節點。
>
> **並行加簽**：不插入新關卡，改在 `WF_ttProcessNodeAssignee`（節點簽核人員表）為既有關卡加入新的簽核人員，並將該關卡的 `ApproveMode` 修正為會簽（`AndSign`）。

### WF_ttProcessNodeAssignee (節點簽核人員表)

`WF_ttProcessNode` 的子表：當簽核發起、或流程推演至某關卡時，簽核引擎依據該關卡在 `WF_tbDefineNodeApprover` 中設定的簽核角色/人員規則，**計算出這一次實際要簽核的所有人員**（角色動態解析為具體帳號、指定人員則直接採用），逐一寫入本表，作為該關卡實際的簽核人員名單。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | varchar | 40 | NO | - | 所屬流程實例鍵 (PK，與 `NodeCode` 組成 FK: `WF_ttProcessNode.(Id, NodeCode)`) |
| NodeCode | varchar | 20 | NO | - | 關卡代碼 (PK) |
| ApproveUser_No | varchar | 20 | NO | - | 簽核人帳號 (PK)，FK: `SYS_tbUser.User_No` |
| update_dt | timestamp | - | YES | - | 更新時間 |
| update_user | varchar | 20 | YES | - | 更新人員 |
| insert_dt | timestamp | - | NO | now() | 新增時間 |
| insert_user | varchar | 20 | YES | - | 新增人員 |

> PK 為 `(Id, NodeCode, ApproveUser_No)`，前兩欄 `(Id, NodeCode)` 整組對 `WF_ttProcessNode` 建立實體 FK（同一關卡下的簽核人集合）。與樣板端 `WF_tbDefineNodeApprover` 不同，本表不需要額外的 `ApproverSeq` 序號欄位：樣板階段允許用「角色」表示一群尚未確定人選的簽核人，所以要靠序號區分多筆角色/人員設定；但本表是引擎**已經解析完成**的結果，每一列都對應一個確定的實際帳號，`ApproveUser_No` 本身即可保證同一關卡下不會重複指派同一人，直接以此欄收尾複合鍵即可。
>
> 本表僅記錄「這個關卡這一次要哪些人簽」，不含每位簽核人目前的簽核狀態、意見、時間等欄位，也不作為逐人狀態投影；逐人簽核狀態與動作紀錄一律以 append-only 的 `WF_ttProcessSign` 為準。應用層分析 `WF_ttProcessSign` 搭配節點的 `ApproveMode` 後，更新 `WF_ttProcessNode.NodeStatus`、流程的 `ProcessStatus` 與 `WF_ttProcess.CurrentNodeCode`。

### WF_ttProcessSign (簽核意見與紀錄表)

當簽核人員、發起人或系統對某關卡執行**同意**、**退件**、**轉簽**、**重送**、**撤回／取消**、**跳過**或**逾時處理**時，產生一筆不可回溯變更的歷程紀錄；本表為 append-only 執行期紀錄表，只允許 `INSERT`，禁止 `UPDATE` 與 `DELETE`。資料庫以 trigger 拒絕修改或刪除，應用程式資料庫角色亦應只授予 `INSERT`／`SELECT` 權限；若需更正，必須新增更正事件，不得覆寫原紀錄。比照 `SYS_ttLog`/`SYS_ttIssuedSerialNumber` 慣例，不設 `update_dt`/`update_user`，也不另設 `insert_dt`/`insert_user`——`SignTime` 與 `ApproveUser_No` 本身即是該筆紀錄唯一的建立時間與建立人。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | varchar | 40 | NO | - | 所屬流程實例鍵 (PK，FK: `WF_ttProcess.Id`) |
| NodeCode | varchar | 20 | NO | - | 關卡代碼 (PK) |
| SignSeq | integer | - | NO | - | 簽核流水號 (PK)，同一關卡下第幾筆簽核紀錄，由 1 起算 |
| ApproveUser_No | varchar | 20 | YES | - | 實際簽核或執行事件的人員帳號；系統產生的 `WITHDRAW`／`SKIP`／`TIMEOUT` 事件可為 `NULL`，比照背景排程觸發時可為空的慣例 |
| SignStatus | varchar | 20 | NO | - | 簽核事件；`APPROVE`(同意)、`REJECT`(退件)、`TRANSFER`(轉簽)、`RESUBMIT`(重送，包含發起人修改後重送)、`WITHDRAW`(撤回或取消)、`SKIP`(跳過)、`TIMEOUT`(逾時) |
| SignComment | varchar | 500 | YES | - | 簽核意見/評語 |
| Signature | bytea | - | YES | - | 自然人憑證數位簽章 |
| SignTime | timestamp | - | NO | now() | 簽核時間 |
| TimeoutAction | varchar | 20 | YES | - | 逾時處理類型；`SignStatus = 'TIMEOUT'` 時必填，記錄 `RETURN_TO_INITIATOR`/`FORWARD_TO_SUPERIOR`/`AUTO_APPROVE_NEXT`；其他事件為 `NULL` |

> PK 為 `(Id, NodeCode, SignSeq)`；`SignSeq` 只作為同一關卡事件的歷程排序鍵，取號衝突時由資料庫唯一性處理並由寫入端重試。資料庫另以部分唯一索引保證同一 `(Id, NodeCode, ApproveUser_No)` 的人工 `APPROVE`／`REJECT`／`TRANSFER` 只有第一筆可寫入；不同簽核人及 `ApproveUser_No IS NULL` 的系統事件不受此限制。歷程列只能新增，禁止更新或刪除；任何修正、撤銷或補充都必須新增一筆事件保存原始紀錄。
>
> `WF_ttProcessSign` 是逐人簽核狀態與動作的唯一歷程來源；`WF_ttProcessNodeAssignee` 只負責保存本次關卡的指派名單，不保存目前狀態。應用層必須在同一個簽核交易中寫入本表、依 `ApproveMode` 分析節點完成條件，並更新 `WF_ttProcessNode.NodeStatus`、`WF_ttProcess.ProcessStatus` 及 `WF_ttProcess.CurrentNodeCode`。`Id` 對 `WF_ttProcess.Id` 建立實體 FK；`(Id, NodeCode)` 整組對 `WF_ttProcessNode.(Id, NodeCode)` 建立實體 FK，確保這筆簽核紀錄所屬的關卡存在（無論 `ApproveUser_No` 是否為 `NULL` 都會檢查）。人工 `APPROVE`／`REJECT`／`TRANSFER` 事件須由該關卡的指派簽核人留下；系統產生的 `WITHDRAW`／`SKIP`／`TIMEOUT` 事件可由受控服務寫入，`ApproveUser_No` 可為 `NULL`。複合 FK 在 `ApproveUser_No` 為 `NULL` 時依 PostgreSQL 預設的 `MATCH SIMPLE` 語意略過指派人檢查，因此系統事件不能只依賴 NULL 辨識來源，仍須由權限與服務流程控管。

> 多人同時簽核時，資料庫唯一索引確保同一簽核人的第一筆人工事件為有效事件；唯一性衝突的後續請求不得視為有效簽核。應用層只依 `WF_ttProcessSign` 當下事件歷程分析節點狀態，不在 `WF_ttProcessNodeAssignee` 另存逐人狀態。
>
> **簽核事件 (SignStatus) 列舉**：`APPROVE`(同意)、`REJECT`(退件)、`TRANSFER`(轉簽)、`RESUBMIT`(重送，包含發起人修改後重送)、`WITHDRAW`(撤回或取消)、`SKIP`(跳過)、`TIMEOUT`(逾時)，以 `CHECK` 約束限制。發起人修改後重新送出不另設事件，統一記為 `RESUBMIT`；取消沿用既有的 `WITHDRAW`，不另設 `CANCEL`。撤回／取消、跳過與逾時都必須寫入本表；其中 `TIMEOUT` 事件須在 `TimeoutAction` 記錄實際觸發的逾時處理類型，不再把逾時動作假裝成其他簽核事件。
>
> `TimeoutAction` 與 `SignStatus` 必須一致：`SignStatus = 'TIMEOUT'` 時必須填入逾時處理類型；其他事件類型的 `TimeoutAction` 必須為 `NULL`。`RESUBMIT` 代表發起人完成修改或處理後重新送出，`WITHDRAW` 代表流程或關卡撤回，亦包含發起人選擇取消流程；`SKIP` 代表因或簽完成、流程分支判定或其他受控規則而跳過，不應使用 `TimeoutAction` 假裝成逾時。

## 系統交易/紀錄表

### SYS_ttLog (操作日誌表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | bigint | - | NO | - | 流水號 (PK) |
| User_No | varchar | 20 | YES | - | 帳號 (邏輯上參照 SYS_tbUser.User_No；未登入時寫入空字串而非 NULL，故不建立強制 FK，詳見下方「關聯說明」) |
| WebPage | varchar | 100 | YES | - | 網頁路徑 |
| Api | varchar | 100 | YES | - | API |
| PostData | text | - | YES | - | 傳送資料 |
| Token | text | - | YES | - | web token |
| ReturnNumber | varchar | 5 | YES | - | 回傳狀態代碼 |
| ReturnMsg | text | - | YES | - | 回傳訊息 |
| SystemTime | timestamp | - | NO | now() | 系統時間 |

### SYS_ttToken (Token管理表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| TokenTime | varchar | 20 | NO | - | 時間戳記 (PK) |
| Token | text | - | YES | - | token |
| SignOut | timestamp | - | YES | - | 登出時間 |

## 排程資料表

仿 Windows 工作排程器（Task Scheduler）的結構，供 .NET `BackgroundWorker`（`IHostedService`）使用，取代以 `BackGroundSettings.json` 存放排程設定與執行結果的作法。與 Windows 工作排程器的對應：

| Windows 工作排程器 | 本設計 |
|---|---|
| 工作（Task）：一般／動作／設定 | `SYS_tbSchedule` |
| 觸發程序（Trigger），一個工作可有多個 | `SYS_tbScheduleTrigger`（一對多） |
| 一次／每天／每週／每月／啟動時 | `TriggerType`：`ONCE`／`DAILY`／`WEEKLY`／`MONTHLY`／`STARTUP` |
| 重複工作間隔與持續時間 | `RepeatIntervalMinutes`／`RepeatDurationMinutes` |
| 延遲工作最多／隨機延遲 | `RandomDelayMinutes` |
| 動作（啟動程式） | `HandlerName`＋`Parameters`（`BackgroundWorker` 以反射呼叫的方法與其 JSON 參數） |
| 若工作已在執行，適用規則 | `MultipleInstancePolicy` |
| 如果工作執行超過…就停止 | `ExecutionTimeLimitMinutes` |
| 失敗時重新啟動 | `RestartCount`／`RestartIntervalMinutes` |
| 錯過排程時儘速啟動 | `RunMissed` |
| 立即執行（Run） | `ForceRun` |
| 歷程記錄 | `SYS_ttScheduleLog` |
| 上次執行時間／結果、下次執行時間 | `LastRunTime`／`LastRunResult`、觸發程序的 `NextRunTime` |

`BackgroundWorker` 每個輪詢週期（例如 1 分鐘）找出 `NextRunTime` 已到的觸發程序，執行對應工作，並於每次執行寫入 `SYS_ttScheduleLog`。

### SYS_tbSchedule (排程工作表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| TaskName | varchar | 50 | NO | - | 工作名稱 (PK) |
| Description | varchar | 200 | YES | - | 工作說明 |
| Enabled | varchar | 1 | NO | 'Y' | 是否啟用 (`Y`/`N`)；`N` 時所有觸發程序皆不執行（含 `ForceRun` 以外的自動觸發） |
| HandlerName | varchar | 100 | NO | - | 動作：`BackgroundWorker` 以反射呼叫的方法名稱（如 `TaskDeleteLog`） |
| Parameters | text | - | YES | - | 動作參數（JSON），傳給 Handler |
| MultipleInstancePolicy | varchar | 15 | NO | 'IGNORE_NEW' | 工作已在執行時再次觸發的處理：`IGNORE_NEW`（不啟動新執行個體，記錄 `SKIPPED`）／`PARALLEL`（平行執行新執行個體） |
| ExecutionTimeLimitMinutes | integer | - | YES | 4320 | 執行逾時（分鐘），超過即視為 `TIMEOUT` 並放棄；NULL 代表不限制。預設 4320＝3 天，同 Windows 預設 |
| RestartCount | integer | - | NO | 0 | 失敗時最多重試次數 |
| RestartIntervalMinutes | integer | - | YES | - | 重試間隔（分鐘）；`RestartCount > 0` 時必填 |
| RunMissed | varchar | 1 | NO | 'Y' | 錯過排程（服務停機或前次未執行）時，服務恢復後是否儘速補執行一次 (`Y`/`N`) |
| ForceRun | varchar | 1 | NO | 'N' | 立即執行旗標 (`Y`/`N`)，相當於右鍵「執行」；Worker 領取後重設為 `N` |
| LastRunTime | timestamp | - | YES | - | 上次執行開始時間 |
| LastRunResult | varchar | 10 | YES | - | 上次執行結果：`RUNNING`／`SUCCESS`／`FAIL`／`TIMEOUT` |
| LastRunMessage | text | - | YES | - | 上次執行訊息（失敗時為錯誤訊息） |
| LockedBy | varchar | 100 | YES | - | 目前執行中的主機／程序識別（`IGNORE_NEW` 於多實例部署時避免重複執行） |
| LockedUntil | timestamp | - | YES | - | 執行鎖到期時間（依 `ExecutionTimeLimitMinutes`，不限制時使用 Worker 心跳逾時）；逾時視為異常中斷，可被接手 |
| CreateDate | timestamp | - | NO | now() | 新增時間 |
| UpdateDate | timestamp | - | NO | now() | 最後異動時間 |

### SYS_tbScheduleTrigger (排程觸發程序表)
一個工作可有多個觸發程序，(`TaskName`, `TriggerSeq`) 為 PK。

| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| TaskName | varchar | 50 | NO | - | 工作名稱 (PK，FK: `SYS_tbSchedule.TaskName`) |
| TriggerSeq | smallint | - | NO | - | 觸發程序序號 (PK) |
| Enabled | varchar | 1 | NO | 'Y' | 此觸發程序是否啟用 (`Y`/`N`) |
| TriggerType | varchar | 10 | NO | - | `ONCE`（一次）／`DAILY`（每天）／`WEEKLY`（每週）／`MONTHLY`（每月）／`STARTUP`（服務啟動時） |
| StartBoundary | timestamp | - | NO | - | 開始時間：首次生效日期與每次執行的時間點 |
| EndBoundary | timestamp | - | YES | - | 到期時間（對應「到期」）；NULL 代表永不到期 |
| RecurEvery | integer | - | NO | 1 | `DAILY`：每 N 天；`WEEKLY`：每 N 週；其他類型忽略 |
| DaysOfWeek | smallint | - | YES | - | `WEEKLY` 使用，位元遮罩：週日=1、週一=2、週二=4、週三=8、週四=16、週五=32、週六=64 |
| MonthsOfYear | smallint | - | YES | - | `MONTHLY` 使用，位元遮罩：1 月=1 … 12 月=2048；全選為 4095 |
| DaysOfMonth | varchar | 100 | YES | - | `MONTHLY` 使用，逗號分隔日期，`L` 代表當月最後一天，如 `1,15,L` |
| RepeatIntervalMinutes | integer | - | YES | - | 重複工作間隔（分鐘）：每次觸發時間點後，每隔多久再執行一次 |
| RepeatDurationMinutes | integer | - | YES | - | 重複持續時間（分鐘）；`RepeatIntervalMinutes` 有值而此欄為 NULL 代表無限期重複（直到 `EndBoundary`） |
| RandomDelayMinutes | integer | - | NO | 0 | 隨機延遲上限（分鐘），避免多工作同時啟動 |
| NextRunTime | timestamp | - | YES | - | 下次預計執行時間，由 Worker 依上列規則計算；NULL 代表已無下次（`ONCE` 已執行或超過 `EndBoundary`） |
| LastRunTime | timestamp | - | YES | - | 此觸發程序上次觸發時間 |

> **觸發類型欄位規則**（以 `CHECK` 約束）：`ONCE` 只用 `StartBoundary`；`DAILY` 用 `RecurEvery`；`WEEKLY` 用 `RecurEvery`＋`DaysOfWeek`（必填）；`MONTHLY` 用 `MonthsOfYear`＋`DaysOfMonth`（皆必填）；`STARTUP` 不設 `NextRunTime`，由 Worker 啟動時直接觸發一次。`RepeatDurationMinutes` 須大於 `RepeatIntervalMinutes`。
>
> **領取排程（避免重複執行）**：Worker 先以樂觀更新領取觸發程序，成功更新筆數為 1 才執行：
> `UPDATE SYS_tbScheduleTrigger SET NextRunTime=@computedNext, LastRunTime=now() WHERE TaskName=@name AND TriggerSeq=@seq AND NextRunTime=@oldNext`（只查 `Enabled='Y'` 且 `SYS_tbSchedule.Enabled='Y'`、`NextRunTime <= now()` 的列）。
> 接著依 `MultipleInstancePolicy`：`IGNORE_NEW` 時再以 `UPDATE SYS_tbSchedule SET LockedBy=@host, LockedUntil=@until, LastRunTime=now(), LastRunResult='RUNNING' WHERE TaskName=@name AND (LockedUntil IS NULL OR LockedUntil < now())` 搶鎖，筆數為 0 表示已有執行個體，寫入 `SKIPPED` 紀錄；`PARALLEL` 則不搶鎖。執行結束更新 `LastRunResult`／`LastRunMessage` 並清除鎖。
> 錯過排程：Worker 啟動時，若 `RunMissed='Y'` 且 `NextRunTime < now()`，只補執行一次，之後依規則重算 `NextRunTime`（不逐次補跑）。`ForceRun='Y'` 的工作不受 `NextRunTime` 限制，立即執行並記錄為 `MANUAL`。排程設定的新增／修改只改資料表，不需重啟服務。

### SYS_ttScheduleLog (排程執行紀錄表)
| 欄位名稱 | 資料型別 | 長度 | 允許空值 | 預設值 | 說明 |
|---------|---------|------|---------|-------|------|
| Id | bigint | - | NO | - | 流水號 (PK) |
| TaskName | varchar | 50 | NO | - | 工作名稱 (FK: SYS_tbSchedule.TaskName) |
| TriggerSeq | smallint | - | YES | - | 觸發的觸發程序序號；`MANUAL`／`RETRY` 為 NULL（邏輯參照，不建立 FK） |
| TriggerType | varchar | 10 | NO | 'SCHEDULE' | 觸發來源：`SCHEDULE`（到期）／`STARTUP`（服務啟動）／`MISSED`（補執行）／`MANUAL`（立即執行）／`RETRY`（失敗重試） |
| Attempt | integer | - | NO | 1 | 第幾次嘗試（含重試） |
| HostName | varchar | 100 | YES | - | 執行的主機／程序識別 |
| StartTime | timestamp | - | NO | now() | 開始執行時間 |
| EndTime | timestamp | - | YES | - | 結束時間；NULL 代表執行中或異常中斷 |
| Status | varchar | 10 | NO | 'RUNNING' | `RUNNING`／`SUCCESS`／`FAIL`／`TIMEOUT`／`SKIPPED`（因 `IGNORE_NEW` 略過） |
| Message | text | - | YES | - | 執行結果或錯誤訊息 |

> 本表為執行歷程（開始時 `INSERT`、結束時回填 `EndTime`/`Status`/`Message`），可依 `StartTime` 定期清理舊資料（例如由 `TaskDeleteLog` 類排程處理）。

## 資料表關聯圖
```mermaid
erDiagram
    %% 選單自我參考
    SYS_tbMenu ||--o{ SYS_tbMenu : "parent-child"

    %% 組織自我參考（樹狀結構）
    SYS_tbOrg ||--o{ SYS_tbOrg : "parent-child"
    %% 主管亦為使用者（邏輯上為 1 位使用者可能是多個組織的主管）
    SYS_tbUser ||--o{ SYS_tbOrg : "manages"
    %% 使用者自我參考（直屬主管）
    SYS_tbUser ||--o{ SYS_tbUser : "manager-subordinate"
    SYS_tbOrg ||--o{ SYS_tbUser : "assigns"
    SYS_tbPosition ||--o{ SYS_tbUser : "holds"
    SYS_tbGroup ||--o{ SYS_tbUser : "grants"
    SYS_tcRank ||--o{ SYS_tbPosition : "classifies"
    SYS_tbUser ||--o{ SYS_ttIssuedSerialNumber : "issues"
    %% 代理人設定：一位使用者可能是多筆代理設定的被代理人，也可能是多筆設定的代理人
    SYS_tbUser ||--o{ SYS_tbDelegate : "delegator-of"
    SYS_tbUser ||--o{ SYS_tbDelegate : "delegate-of"
    %% 文件編號沿用共用發號結果，主檔保存目前發行版本
    SYS_ttIssuedSerialNumber ||--o| DOC_tbDocument : "issues"
    DOC_tbDocument ||--o{ DOC_ttDocumentVersion : "has-versions"
    DOC_ttDocumentVersion ||--o| DOC_tbDocument : "current-version-of"
    DOC_ttDocumentVersion ||--o{ DOC_ttDocumentAttachment : "attached-version"
    %% 一個流程定義有多個版本內容
    WF_tbDefine ||--o{ WF_tbDefineProcess : "has-versions"
    %% 一個流程版本有多個關卡節點
    WF_tbDefineProcess ||--o{ WF_tbDefineNode : "has-nodes"
    %% 關卡節點自我參考（前一關、下一關、退回關）
    WF_tbDefineNode ||--o{ WF_tbDefineNode : "prev-next-reject"
    %% 關卡可對應多筆簽核設定或被知會對象
    WF_tbDefineNode ||--o{ WF_tbDefineNodeApprover : "has-targets"
    SYS_tbUser ||--o{ WF_tbDefineNodeApprover : "assigned-or-notified"
    %% 一個流程版本可發起多個流程實例
    WF_tbDefineProcess ||--o{ WF_ttProcess : "instantiates"
    SYS_tbUser ||--o{ WF_ttProcess : "initiates"
    %% 一個流程實例有多個執行期關卡節點
    WF_ttProcess ||--o{ WF_ttProcessNode : "has-nodes"
    %% 執行期關卡節點自我參考（前一關、下一關、退回關）
    WF_ttProcessNode ||--o{ WF_ttProcessNode : "prev-next-reject"
    %% 流程實例目前停留的執行期關卡
    WF_ttProcessNode ||--o| WF_ttProcess : "current-node-of"
    %% 一個執行期關卡節點可對應多筆已解析完成的簽核人員
    WF_ttProcessNode ||--o{ WF_ttProcessNodeAssignee : "assigned-to"
    SYS_tbUser ||--o{ WF_ttProcessNodeAssignee : "signs-as"
    %% 一個執行期關卡節點可累積多筆簽核歷程紀錄
    WF_ttProcessNode ||--o{ WF_ttProcessSign : "signed-at"
    %% 執行動作的人須本來就是該關卡的指派簽核人之一
    WF_ttProcessNodeAssignee ||--o{ WF_ttProcessSign : "acts-as"

    SYS_tbMenu {
        integer Id PK
        varchar Type
        varchar Name
        varchar Path
        integer Ord
        integer ParentId FK
        varchar Icon
        varchar Enabled
        varchar Visible
    }

    SYS_tbPasswordValidationRules {
        integer Id PK
        varchar RuleName
        integer MinLength
        integer PasswordChangeFrequencyDays
        boolean IsActive
    }

    SYS_ttIssuedSerialNumber {
        varchar DocType PK
        varchar OrgCode PK
        varchar PeriodKey PK
        integer Seq PK
        varchar IssuedCode
        varchar IssuedUser_No FK
        timestamp IssuedTime
    }

    SYS_tbOrg {
        integer Id PK
        varchar Code
        varchar Name
        integer ParentId FK
        varchar OrgType
        varchar Manager_User_No FK
        boolean Enabled
    }

    SYS_tcRank {
        integer Id PK
        varchar Code
        varchar Name
        integer Level
        boolean Enabled
    }

    SYS_tbPosition {
        integer Id PK
        varchar Code
        varchar Name
        integer Rank_id FK
        boolean Enabled
    }

    SYS_tbGroup {
        integer Id PK
        varchar Code
        varchar Name
        varchar Description
        boolean Enabled
    }

    SYS_tbUser {
        varchar User_No PK
        varchar User_Name
        integer Org_id FK
        integer Position_id FK
        integer UserGroup_id FK
        varchar Manager_User_No FK
        boolean IsActive
        date HireDate
        date ResignDate
    }

    SYS_tbDelegate {
        integer Id PK
        varchar Delegator_User_No FK
        varchar Delegate_User_No FK
        varchar ModuleCode
        timestamp StartDate
        timestamp EndDate
        boolean Enabled
    }

    DOC_tbDocument {
        varchar DocType PK
        varchar OrgCode PK
        varchar PeriodKey PK
        integer Seq PK
        varchar DocumentId
        varchar DocumentName
        integer LatestVersionNo FK
        boolean Enabled
    }

    DOC_ttDocumentVersion {
        varchar DocumentId PK FK
        integer VersionNo PK
        varchar FileName
        varchar StorageKey
        varchar ContentType
        bigint FileSize
        varchar Sha256
        timestamp Published_dt
        varchar Published_user
    }

    DOC_ttDocumentAttachment {
        varchar DocumentId PK FK
        integer VersionNo PK FK
        varchar BizTableName PK
        varchar BizDataId PK
        varchar BizDocNo
        timestamp Attached_dt
        varchar Attached_user
    }

    WF_tbDefine {
        varchar DocType PK
        varchar OrgCode PK
        varchar PeriodKey PK
        integer Seq PK
        varchar DefineId
        integer VersionNo
    }

    WF_tbDefineProcess {
        varchar DefineId PK FK
        integer VersionNo PK
        varchar ProcessName
        varchar ProcessDesc
        timestamp Published_dt
        varchar Published_user
    }

    WF_tbDefineNode {
        varchar DefineId PK FK
        integer VersionNo PK FK
        varchar NodeCode PK
        varchar NodeName
        varchar PrevNodeCode FK
        varchar RejectNodeCode FK
        integer NodeSeq
        varchar NodeType
        varchar ApproveMode
        integer TimeoutDays
        varchar TimeoutAction
    }

    WF_tbDefineNodeApprover {
        varchar DefineId PK FK
        integer VersionNo PK FK
        varchar NodeCode PK FK
        integer ApproverSeq PK
        varchar ApproveRole
        varchar ApproveUser_No FK
    }

    WF_ttProcess {
        varchar DocType PK
        varchar OrgCode PK
        varchar PeriodKey PK
        integer Seq PK
        varchar Id
        varchar DefineId FK
        integer DefineVersionNo FK
        varchar BizTableName
        varchar BizDataId
        varchar BizDocNo
        varchar Subject
        varchar InitiatorUser_No FK
        varchar CurrentNodeCode
        varchar ProcessStatus
        timestamp StartTime
        timestamp EndTime
    }

    WF_ttProcessNode {
        varchar Id PK FK
        varchar NodeCode PK
        varchar NodeName
        varchar PrevNodeCode FK
        varchar RejectNodeCode FK
        integer NodeSeq
        varchar NodeType
        varchar ApproveMode
        integer TimeoutDays
        varchar TimeoutAction
        boolean IsDynamicNode
        varchar NodeStatus
        timestamp GeneratedTime
        timestamp CompleteTime
    }

    WF_ttProcessNodeAssignee {
        varchar Id PK FK
        varchar NodeCode PK FK
        varchar ApproveUser_No PK FK
    }

    WF_ttProcessSign {
        varchar Id PK FK
        varchar NodeCode PK FK
        integer SignSeq PK
        varchar ApproveUser_No FK
        varchar SignStatus
        varchar SignComment
        bytea Signature
        timestamp SignTime
        varchar TimeoutAction
    }

    SYS_ttLog {
        bigint Id PK
        varchar User_No FK
        varchar WebPage
        varchar Api
        varchar ReturnNumber
        timestamp SystemTime
    }

    SYS_ttToken {
        varchar TokenTime PK
        varchar Token
        timestamp SignOut
    }

    %% 排程
    SYS_tbSchedule ||--o{ SYS_ttScheduleLog : "executes"
    SYS_tbSchedule ||--o{ SYS_tbScheduleTrigger : "has triggers"
    SYS_tbSchedule {
        varchar TaskName PK
        varchar Enabled
        varchar HandlerName
        text Parameters
        varchar MultipleInstancePolicy
        integer ExecutionTimeLimitMinutes
        integer RestartCount
        varchar RunMissed
        varchar ForceRun
        timestamp LastRunTime
        varchar LastRunResult
        varchar LockedBy
        timestamp LockedUntil
    }

    SYS_tbScheduleTrigger {
        varchar TaskName PK
        smallint TriggerSeq PK
        varchar TriggerType
        timestamp StartBoundary
        timestamp EndBoundary
        integer RecurEvery
        smallint DaysOfWeek
        smallint MonthsOfYear
        varchar DaysOfMonth
        integer RepeatIntervalMinutes
        integer RepeatDurationMinutes
        timestamp NextRunTime
    }

    SYS_ttScheduleLog {
        bigint Id PK
        varchar TaskName FK
        varchar TriggerType
        timestamp StartTime
        timestamp EndTime
        varchar Status
    }

```

### 關聯說明

- `SYS_ttScheduleLog.TaskName` 為一般強制 FK 參照 `SYS_tbSchedule.TaskName`（一個工作對應多筆執行紀錄）；`SYS_tbScheduleTrigger.TaskName` 亦為 FK（`ON DELETE CASCADE`，刪除工作時連同觸發程序刪除）。`SYS_ttScheduleLog.TriggerSeq` 僅為邏輯參照（`MANUAL`／`RETRY` 無觸發程序，且觸發程序被刪除後歷程仍須保留）。排程資料表與其他業務資料表無關聯。

- `SYS_tbMenu` 透過 `ParentId` 自我參考，組成選單樹狀結構（`ParentId=0` 代表頂層節點）。
- `SYS_ttIssuedSerialNumber` 以 `(DocType, OrgCode, PeriodKey, Seq)` 四欄複合鍵識別每一次發行，`IssuedUser_No` 邏輯參照 `SYS_tbUser.User_No`（比照 `SYS_ttLog.User_No` 的處理方式，背景排程觸發時可為空，不建立強制 FK）。
- `SYS_tbDelegate.Delegator_User_No`／`Delegate_User_No` 皆為一般強制 FK 參照 `SYS_tbUser.User_No`（不像 `SYS_ttLog`/`SYS_ttIssuedSerialNumber` 有匿名/系統觸發的情況，代理雙方必為實際使用者，故直接建立約束）。
- `SYS_tbOrg` 透過 `ParentId` 自我參考，組成組織樹狀結構，慣例與 `SYS_tbMenu.ParentId` 相同（`ParentId=0` 代表頂層組織，不建立強制 FK）。
- `SYS_tbUser.Org_id`／`Position_id`／`UserGroup_id` 分別參照 `SYS_tbOrg`／`SYS_tbPosition`／`SYS_tbGroup`，皆為可為空的一般 FK（尚未分派組織/職稱/群組時為 `NULL`）。
- `SYS_tbPosition.Rank_id` 參照 `SYS_tcRank`（職等代碼表），一個職等可對應多個職稱（1 對多）；職等的名稱與比較用數值統一維護在 `SYS_tcRank`，職稱本身不直接帶數值層級，符合職稱與職等分屬不同維度的慣例。
- `SYS_tbUser.Manager_User_No` 自我參考至 `SYS_tbUser.User_No`，表示直屬主管，供電子簽核模組判斷簽核層級/組織架構使用。雖然單一 `CREATE TABLE` 內宣告自我參考 FK 在 PostgreSQL 語法上合法，但為避免分段執行腳本時（例如只選取部分片段執行）對執行順序產生疑慮，統一改成建表後以 `ALTER TABLE` 補上約束（詳見 SQL 檔）。
- `SYS_tbOrg.Manager_User_No` 參照 `SYS_tbUser.User_No`（組織主管）。這兩張表互相參照形成循環相依，實作上先建立兩張表（`SYS_tbOrg.Manager_User_No` 先不加約束），最後再以 `ALTER TABLE` 補上 `SYS_tbOrg` 對 `SYS_tbUser` 的 FK（詳見 SQL 檔）；因兩欄位皆可為 `NULL`，不影響資料建置順序。
- `SYS_ttLog.User_No` 概念上對應 `SYS_tbUser.User_No`，但依現行程式碼（`clsApiBase.DoAppRecLog`），未登入的請求會寫入空字串 `''` 而非 `NULL`，若建立強制 FK 會導致匿名操作的寫入失敗，因此維持邏輯關聯、不建立強制 FK 約束。
- `SYS_ttToken` 沒有直接的 FK 欄位；`TokenTime` 與登入時產生的使用者 Token payload（`User.datetime`）對應，屬性上是一組使用者 session 的識別鍵，而非結構化外鍵。
- `DOC_tbDocument.(DocType, OrgCode, PeriodKey, Seq)` 整組參照 `SYS_ttIssuedSerialNumber` 的同名複合鍵（`DocType` 固定為 `'DOC'`）；`DocumentId` 為該複合鍵衍生的 `UNIQUE` 欄位。`DOC_ttDocumentVersion.DocumentId` 參照主檔；主檔的 `(DocumentId, LatestVersionNo)` 參照版本表 `(DocumentId, VersionNo)`，確保目前版本屬於該文件。發布狀態及最新版判斷由應用層在同一交易內驗證。
- `DOC_ttDocumentAttachment.(DocumentId, VersionNo)` 整組參照 `DOC_ttDocumentVersion`，固定記錄業務資料附加的檔案版本；`(BizTableName, BizDataId)` 是多型關聯，因目標資料表依模組而異，不建立實體 FK，由應用層解析及驗證。`BizDocNo` 僅為顯示用快照，不作關聯鍵。
- `WF_tbDefine` 以 `(DocType, OrgCode, PeriodKey, Seq)` 複合鍵識別流程定義，`DefineId` 為該複合鍵串接而成的 `UNIQUE` 衍生欄位，供後續流程版本明細或流程實例使用單欄 FK 參照。
- `WF_tbDefineProcess.(DefineId, VersionNo)` 為流程定義的版本內容，`DefineId` 對 `WF_tbDefine.DefineId` 建立一般強制 FK。`Published_dt` 為 `NULL` 時可編輯；一旦 `Published_dt` 非 `NULL`，即代表版本曾被發行，已發行版本及其關聯資料不得編輯或更新，版本控制與發行規則一致由應用層處理，修改時須新增版本。`WF_tbDefine.VersionNo` 非 `NULL` 時，應由應用層驗證其指向同一 `DefineId` 下已存在且已發行的版本。
- `WF_tbDefineNode.(DefineId, VersionNo)` 整組參照 `WF_tbDefineProcess` 的同名複合鍵，表示該關卡所屬的流程版本。`PrevNodeCode`／`RejectNodeCode` 皆以 `(DefineId, VersionNo, NodeCode)` 對本表建立複合自我 FK；多個節點可共用 `PrevNodeCode`，因此可表示平行分支。
- `WF_tbDefineNodeApprover.(DefineId, VersionNo, NodeCode)` 整組參照 `WF_tbDefineNode` 的同名複合鍵。`Approve`／`Countersign` 節點中，該表表示簽核角色或人員設定；`Notify` 節點中，則表示被知會的角色或人員，系統發出通知後不等待回覆。`ApproveUser_No` 為可為空 FK，參照 `SYS_tbUser.User_No`，而 `ApproveRole` 由程式碼定義。`Gateway` 節點由 trigger 禁止配置簽核人。
- `WF_ttProcess.(DocType, OrgCode, PeriodKey, Seq)` 整組參照 `SYS_ttIssuedSerialNumber` 的同名複合鍵（`DocType` 固定為 `'WF'`）。`(DefineId, DefineVersionNo)` 整組參照 `WF_tbDefineProcess.(DefineId, VersionNo)`，固定保存流程發起時採用的版本；正式送出時由應用層在同一交易內驗證該版本等於當時的 `WF_tbDefine.VersionNo` 且已發行。流程送出後不再要求該版本是目前版本，既有流程仍可依原版本完成或撤回。`InitiatorUser_No` 參照 `SYS_tbUser.User_No`；`(Id, CurrentNodeCode)` 整組參照 `WF_ttProcessNode.(Id, NodeCode)`。
- `WF_ttProcessNode.Id` 對 `WF_ttProcess.Id` 建立一般強制 FK，確保關卡節點必屬於指定的流程實例；`PrevNodeCode`／`RejectNodeCode` 皆以 `(Id, NodeCode)` 對本表建立複合自我 FK，比照 `WF_tbDefineNode` 的父子關係慣例。本表不對 `WF_tbDefineNode` 建立實體 FK（複製快照，避免樣板異動回溯影響已執行/已完成的流程實例）。
- `WF_ttProcessNodeAssignee.(Id, NodeCode)` 整組參照 `WF_ttProcessNode` 的同名複合鍵，表示該筆簽核人設定所屬的執行期關卡；`ApproveUser_No` 參照 `SYS_tbUser.User_No`。本表沒有目前狀態欄位，也不作為逐人狀態投影；與樣板端 `WF_tbDefineNodeApprover` 不同，本表無角色欄位、也無需 `ApproverSeq` 序號——每一列都是引擎解析完成的確定帳號。
- `WF_ttProcessSign.Id` 參照 `WF_ttProcess.Id`；`(Id, NodeCode)` 整組參照 `WF_ttProcessNode` 的同名複合鍵，確保簽核紀錄所屬的關卡存在。`(Id, NodeCode, ApproveUser_No)` 另整組參照 `WF_ttProcessNodeAssignee` 的同名複合鍵，確保執行動作的人本來就是該關卡的指派簽核人之一；逐人簽核狀態以本表歷程為準，應用層分析後更新 `WF_ttProcess.CurrentNodeCode`。`ApproveUser_No` 為 `NULL`（系統事件）時，此複合 FK 依 PostgreSQL 預設的 `MATCH SIMPLE` 語意略過檢查。

## 索引建議
PostgreSQL 沒有 SQL Server 「CLUSTERED / NONCLUSTERED」的索引型態區分（PK 預設建立一般的 unique B-tree 索引，資料實體排序與索引無關），以下皆為一般 `CREATE INDEX`（B-tree）建議：

- `SYS_tbMenu`：在 `ParentId`、`Type`、`Enabled` 上建立索引
- `SYS_tbPasswordValidationRules`：在 `IsActive` 上建立索引（查詢啟用中規則）
- `SYS_ttIssuedSerialNumber`：`(DocType, OrgCode, PeriodKey, Seq)` 已是 PK；另在 `IssuedCode`（唯一索引）、`IssuedUser_No`、`PeriodKey` 上建立索引
- `SYS_tbDelegate`：在 `Delegator_User_No`、`Delegate_User_No` 上建立索引；`(Delegate_User_No, Enabled, StartDate, EndDate)` 建立複合索引，供「查目前正在代理我的人」這類高頻查詢使用
- `SYS_tbOrg`：在 `ParentId`、`Enabled`、`Manager_User_No` 上建立索引
- `SYS_tcRank`：在 `Enabled` 上建立索引
- `SYS_tbPosition`：在 `Rank_id`、`Enabled` 上建立索引
- `SYS_tbGroup`：在 `Enabled` 上建立索引
- `SYS_tbUser`：在 `Org_id`、`Position_id`、`UserGroup_id`、`Manager_User_No`、`IsActive` 上建立索引
- `DOC_tbDocument`：四欄複合 PK、`DocumentId` `UNIQUE` 約束及 `(DocumentId, LatestVersionNo)` 複合 FK；另在 `Enabled` 上建立索引
- `DOC_ttDocumentVersion`：`(DocumentId, VersionNo)` 已是 PK；另在 `Published_dt` 上建立索引，供查詢草稿與已發行版本
- `DOC_ttDocumentAttachment`：`(DocumentId, VersionNo, BizTableName, BizDataId)` 已是 PK；另在 `(BizTableName, BizDataId)` 上建立索引，供查詢業務資料的附件
- `WF_tbDefine`：`(DocType, OrgCode, PeriodKey, Seq)` 已是 PK，`DefineId` 已是 `UNIQUE` 約束（自動具備索引）；另在 `VersionNo` 上建立索引，供查詢目前正式使用的版本使用
- `WF_tbDefineProcess`：`(DefineId, VersionNo)` 已是 PK；另在 `Published_dt` 上建立索引，供查詢已發行版本與草稿使用
- `WF_tbDefineNode`：`(DefineId, VersionNo, NodeCode)` 已是 PK；`(DefineId, VersionNo, NodeSeq)` 為 `UNIQUE` 約束，供依排序列出版本關卡
- `WF_tbDefineNodeApprover`：`(DefineId, VersionNo, NodeCode, ApproverSeq)` 已是 PK；另在 `ApproveUser_No` 上建立索引
- `WF_ttProcess`：`(DocType, OrgCode, PeriodKey, Seq)` 已是 PK，`Id` 已是 `UNIQUE` 約束；另在 `(DefineId, DefineVersionNo)`、`InitiatorUser_No`、`ProcessStatus`、`(BizTableName, BizDataId)`、`CurrentNodeCode` 上建立一般索引。`(BizTableName, BizDataId)` 不建立唯一約束；簽核中鎖檔、禁止修改與防止重複送簽由應用層處理
- `WF_ttProcessNode`：`(Id, NodeCode)` 已是 PK；`(Id, NodeSeq)` 為 `UNIQUE` 約束，供依排序列出流程實例的執行期關卡；另在 `NodeStatus`、`IsDynamicNode` 上建立索引，供查詢待處理關卡／動態插入關卡使用
- `WF_ttProcessNodeAssignee`：`(Id, NodeCode, ApproveUser_No)` 已是 PK；另在 `ApproveUser_No` 上建立索引，供查詢「某人目前有哪些待簽關卡」使用
- `WF_ttProcessSign`：`(Id, NodeCode, SignSeq)` 已是 PK；另以部分唯一索引限制同一 `(Id, NodeCode, ApproveUser_No)` 僅能有一筆人工 `APPROVE`／`REJECT`／`TRANSFER` 事件，並在 `ApproveUser_No`、`SignStatus` 上建立索引，供查詢「某人簽過哪些單」／依狀態統計使用
- `SYS_ttLog`：在 `User_No`、`SystemTime`、`Api` 上建立索引
- `SYS_ttToken`：`TokenTime` 已是 PK（自動具備 unique 索引），若常以 `SignOut IS NULL` 查詢有效 Token，可另建立部分索引（partial index，`WHERE SignOut IS NULL`）
- `SYS_tbSchedule`：`TaskName` 已是 PK
- `SYS_tbScheduleTrigger`：`(TaskName, TriggerSeq)` 已是 PK；另建立部分索引 `(NextRunTime) WHERE Enabled = 'Y'`，供 Worker 輪詢到期觸發程序
- `SYS_ttScheduleLog`：在 `(TaskName, StartTime)`、`StartTime` 上建立索引，供查詢單一排程歷程與清理舊資料

## 命名規則
- **系統基本資料表**：`SYS_tb` 開頭（如 `SYS_tbMenu`、`SYS_tbPasswordValidationRules`、`SYS_tbOrg`、`SYS_tbPosition`、`SYS_tbGroup`、`SYS_tbUser`）
- **系統交易/紀錄表**：`SYS_tt` 開頭（如 `SYS_ttLog`、`SYS_ttToken`、`SYS_ttScheduleLog`；排程設定為 `SYS_tbSchedule`／`SYS_tbScheduleTrigger`；規劃中的 `SYS_ttLoginLog` 亦同）
- **系統代碼表**：`SYS_tc` 開頭（沿用舊專案慣例，供狀態/代碼類資料表使用，如 `SYS_tcRank`）
- **模組資料表**：以模組代碼取代 `SYS`，其餘沿用 `_tb`/`_tt`/`_tc` 規則，藉此區分「可被覆寫的設定」（`_tb`，設計期樣板）與「不可回溯變動的歷史紀錄」（`_tt`，執行期資料）
- 欄位命名不加資料表實體前綴（例如 `SYS_tbMenu` 用 `Name`/`Path` 而非 `Menu_Name`/`Menu_Path`），僅在下列情況加前綴：
  - 欲強調欄位所屬實體以避免歧義時（例如 FK 欄位視情況可用 `<實體>_id`，如舊專案 `TMS_ttTask_Master.Car_id`）
  - 欄位名稱與 SQL 保留字/型別關鍵字衝突時，改用語意更明確的名稱（例如 `Web_Token.[datetime]` 正名為 `SYS_ttToken.TokenTime`）

## 備註
- 本文件涵蓋目前資料庫 schema 設計的所有資料表，依應用程式串接狀況分兩類：
  - 已有應用程式碼（Controller/Service）實際查詢使用：`SYS_tbMenu`、`SYS_tbPasswordValidationRules`、`SYS_ttLog`、`SYS_ttToken`；其中 `SYS_tbMenu`/`SYS_ttLog`/`SYS_ttToken` 是由舊模版的 `Web_Menu`/`Web_Log`/`Web_Token` 正規化重新命名而來（詳見遷移對話紀錄）。
    - 新設計、尚未串接應用程式碼：`SYS_tbOrg`、`SYS_tcRank`、`SYS_tbPosition`、`SYS_tbGroup`、`SYS_tbUser`（人事組織資料表）、`SYS_ttIssuedSerialNumber`（編碼／序號管理資料表）、`SYS_tbDelegate`（代理人資料表）、`DOC_tbDocument`／`DOC_ttDocumentVersion`／`DOC_ttDocumentAttachment`（表單／文件主檔、檔案版本紀錄與附件關聯）、`WF_tbDefine`／`WF_tbDefineProcess`／`WF_tbDefineNode`／`WF_tbDefineNodeApprover`／`WF_ttProcess`／`WF_ttProcessNode`／`WF_ttProcessNodeAssignee`／`WF_ttProcessSign`（電子簽核流程定義主檔／版本內容／關卡節點／關卡簽核人／流程實例／執行期關卡節點／節點簽核人員／簽核意見與紀錄表）。已完成欄位定義與 PostgreSQL DDL，待後續開發相關功能模組時實際串接使用。
- 舊專案的 `Web_Login_log`（規劃新名 `SYS_ttLoginLog`）、`Web_MenuGroupList`（選單與群組權限對應表）等資料表，中捷 EIP 尚未建立，待建立後需補進本文件。
- 所有日期時間欄位預設使用 `now()`（PostgreSQL；等同 `CURRENT_TIMESTAMP`）。
- ⚠️ 上述型別為 **PostgreSQL Schema**，後端查詢執行層（`Application/Common/Infrastructure/Data/clsPostgreDB.cs`）已依此改寫；但尚未在真正的 PostgreSQL 伺服器上建立/驗證過這些資料表，詳見 `docs/server/README.md`。
- 對應的 PostgreSQL `CREATE TABLE` 語法見 [`docs/schemas/postgresql_create_tables.sql`](./postgresql_create_tables.sql)。
- 可貼到 [dbdiagram.io](https://dbdiagram.io/d) 顯示互動式 ER 圖的 DBML 檔見 [`docs/schemas/dbdiagram.dbml`](./dbdiagram.dbml)。
