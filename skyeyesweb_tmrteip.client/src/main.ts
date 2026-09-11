import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import router, { initializeRouter } from './router'

import Swal from 'sweetalert2'

//#region flatpickr
import flatpickr from 'flatpickr'
import 'flatpickr/dist/flatpickr.css'
import { Mandarin } from 'flatpickr/dist/l10n/zh.js'
//#endregion

//#region primevue
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import ToastService from 'primevue/toastservice'
import Aura from '@primeuix/themes/aura'

import 'primeicons/primeicons.css'

import { zodResolver } from '@primevue/forms/resolvers/zod'
import { z } from 'zod'
//#endregion

//#region Sakai 範本（僅供參考/開發時瀏覽，見 src/sakai/ 與 docs/client/README.md）
import { useLayout as useSakaiLayout } from '@/sakai/layout/composables/layout'
import { applyThemeConfig as applySakaiThemeConfig, fetchDefaultThemeConfig } from '@/sakai/utils/themeConfig'
//#endregion

/**
 * 應用程式初始化入口。
 * - 先初始化 router（包含動態路由/權限等設定）
 * - 建立 Vue App 並掛載 PrimeVue 與 router
 */
async function bootstrap() {
  await initializeRouter()
  const app = createApp(App)
  app.use(router)

  app.use(PrimeVue, {
    theme: {
      preset: Aura,
      options: {
        // 與 Sakai 範本（src/sakai/）的深色模式 class 一致，讓 PrimeVue 元件的深色樣式
        // 能同時被本專案的 DoSetTheme（.dark）與 Sakai 自己的主題設定（.app-dark）觸發。
        darkModeSelector: '.app-dark',
      },
    },
  })
  app.use(ToastService)
  app.use(ConfirmationService)

  // 若有 Sakai 匯出的 /theme-config.json，套用其自訂主題（僅影響 Sakai 範本區）
  const sakaiThemeConfig = await fetchDefaultThemeConfig()
  if (sakaiThemeConfig) {
    const { layoutConfig: sakaiLayoutConfig, setDarkTheme: setSakaiDarkTheme } = useSakaiLayout()
    applySakaiThemeConfig(sakaiThemeConfig, sakaiLayoutConfig, setSakaiDarkTheme)
  }

  app.mount('#app')
}

bootstrap()

// 套用啟動時已儲存的主題（DoSetTheme 內部已會寫回 localStorage，故不需另外監聽 data-theme 屬性變化）
document.addEventListener('DOMContentLoaded', function () {
  DoSetTheme()
})

// 監聽路由變更事件
router.afterEach((to, _from) => {
  // 安全取得路由名稱，避免 undefined
  const currentRouteName: string = (to.name as string) || ''

  // 只在有路由名稱時才處理
  if (currentRouteName) {
    clsConfig.ApiPath = clsConfig.getFullApiPath(`/api/${currentRouteName}`)
    clsConfig.RouterPath = `/${currentRouteName.replace(/_/g, '/')}`
  } else {
    // 備援方案：從 matched 路由取得路徑
    const matchedPath = to.matched.length > 0 ? (to.matched[to.matched.length - 1]?.path ?? '/') : '/'
    // 移除 BASE_URL 前綴
    const cleanPath = matchedPath.replace(new RegExp(`^${clsConfig.BasePath}`), '')
    clsConfig.RouterPath = cleanPath || '/'
    clsConfig.ApiPath = clsConfig.getFullApiPath(`/api${clsConfig.RouterPath.replace(/\//g, '_')}`)
  }

  clsSweetMsg.CloseAll()

  // 安全檢查 meta.requiresAuth，使用可選鏈接與預設值
  const requiresAuth = to.meta?.requiresAuth ?? false

  if (
    (!localStorage['token'] || !CmpAuth(clsConfig.RouterPath)) &&
    requiresAuth &&
    to.name
  ) {
    router.push({ path: '/' })
  }

  clsConfig.MenuData.forEach((mItem: any) => {
    mItem.subItems.forEach((sItem: any) => {
      sItem.isActive = sItem.Path === clsConfig.RouterPath
    })
  })

  if (clsConfig.RouterPath != '/login') {
    if (clsConfig.RouterPath != window.localStorage['LastPath']) {
      window.localStorage['LastPath'] = clsConfig.RouterPath
    } else {
      window.localStorage.removeItem('LastPath')
    }
  }

  DoUpdatePathData()
})

//#region 函式擴充
declare global {

  //#region 擴充 Date 
  interface Date {

    /**
     * 格式化日期
     * @param format 格式化字串，例如 "yyyy-MM-dd hh:mm:ss"
     * @returns 格式化後的日期字串
     */
    format(format: string): string;

    /**
     * 加減天數
     * @param days 要加減的天數
     * @returns 修改後的日期物件（同一個參考）
     */
    addDays(days: number): Date;

    /**
     * 加減分鐘
     * @param minutes 要加減的分鐘數
     * @returns 修改後的日期物件（同一個參考）
     */
    addMinutes(minutes: number): Date;

  }
  //#endregion

}

Date.prototype.format = function (format: string): string {
  const o: { [key: string]: number } = {
    "M+": this.getMonth() + 1, //月份
    "d+": this.getDate(), //日
    "h+": this.getHours(), //小時
    "m+": this.getMinutes(), //分
    "s+": this.getSeconds(), //秒
    "q+": Math.floor((this.getMonth() + 3) / 3), //季度
    "S": this.getMilliseconds() //毫秒
  };

  if (/(y+)/.test(format)) {
    format = format.replace(RegExp.$1, (this.getFullYear() + "").slice(4 - RegExp.$1.length));
  }

  for (let k in o) {
    if (new RegExp(`(${k})`).test(format)) {
      format = format.replace(RegExp.$1, RegExp.$1.length === 1 ? o[k].toString() : ("00" + o[k]).slice(("" + o[k]).length));
    }
  }

  return format;
};

/**
 * 將日期物件加減天數（會直接修改原本 Date 物件）。
 * @param days 要加減的天數（可為負數）
 * @returns 修改後的日期物件（同一個參考）
 */
Date.prototype.addDays = function (days: number): Date {
  this.setDate(this.getDate() + days);
  return this;
}

/**
 * 將日期物件加減分鐘（會直接修改原本 Date 物件）。
 * @param minutes 要加減的分鐘數（可為負數）
 * @returns 修改後的日期物件（同一個參考）
 */
Date.prototype.addMinutes = function (minutes: number): Date {
  this.setMinutes(this.getMinutes() + minutes);
  return this;
}

//#endregion

/**
 * Ajax 非同步請求類別
 */
export class clsAjaxAsync {

  private ReturnVal = null;

  private xhr = new XMLHttpRequest();

  private method = "post";
  private url = "";
  private async = true;
  private cache = false;
  private body = {};
  //private contentType = "application/x-www-form-urlencoded; charset=UTF-8";
  private contentType = "application/json";
  private headers = new Array<{ name: string, value: string }>;
  private encrypt = false;
  private isPostModel = false;
  private callback = new Function;
  private parent = null;
  private loadingMsg: string | null = "Loading";
  private before = new Function;
  private success = new Function;
  private error = new Function;
  private final = new Function;

  constructor(obj: {
    /** HTTP Method */
    method?: string,
    /** API網址 */
    url?: string,
    /** 非同步 */
    async?: boolean,
    /** 快取 */
    cache?: boolean,
    /** 資料 */
    body?: object,
    /** 資料型態 */
    contentType?: string,
    /** 標頭 */
    headers?: Array<{ name: string, value: string }>,
    /** 是否加密 */
    encrypt?: boolean,
    /** 是否PostData包裏傳值 */
    isPostModel?: boolean,
    /** Loading訊息 */
    loadingMsg?: string | null,
    /** 非同步執行前 */
    before?: Function,
    /** 非同步執行成功 */
    success?: Function,
    /** 非同步執行失敗 */
    error?: Function,
    /** 非同步執行後 */
    final?: Function,
    /** 非同步回呼 */
    callback?: Function
    /** 父物件 */
    parent?: any
  }) {

    let xObj = this.xhr;

    if (obj.method != undefined) this.method = obj.method;
    if (obj.url != undefined) this.url = obj.url;
    if (obj.async != undefined) this.async = obj.async;
    if (obj.cache != undefined) this.cache = obj.cache;
    if (obj.body != undefined) this.body = obj.body;
    if (obj.contentType != undefined) this.contentType = obj.contentType;
    if (obj.headers != undefined) this.headers = obj.headers;
    if (obj.encrypt != undefined) this.encrypt = obj.encrypt;
    if (obj.isPostModel != undefined) this.isPostModel = obj.isPostModel;
    if (obj.callback != undefined) this.callback = obj.callback;
    if (obj.parent != undefined) this.callback = obj.callback?.bind(obj.parent);
    if (obj.loadingMsg != undefined) this.loadingMsg = obj.loadingMsg ? obj.loadingMsg : this.loadingMsg;
    if (obj.loadingMsg == null) this.loadingMsg = null;
    if (obj.before != undefined) this.before = obj.before;
    if (obj.success != undefined) this.success = obj.success;
    if (obj.error != undefined) this.error = obj.error;
    if (obj.final != undefined) this.callback = obj.final;

    this.xhr.open(this.method, this.url, this.async);
    if (this.cache) {
      this.xhr.setRequestHeader("Cache-Control", "max-age=3600");
    } else {
      this.xhr.setRequestHeader("Cache-Control", "no-cache");
    }

    this.xhr.setRequestHeader("Content-type", this.contentType);

    xObj.setRequestHeader("Page", clsConfig.RouterPath);
    if (localStorage["token"]) xObj.setRequestHeader("Authorization", localStorage["token"]);
    this.headers.forEach(function (o, i) {
      xObj.setRequestHeader(o.name, o.value);
    });

    if (this.isPostModel) this.xhr.setRequestHeader("Base64-Encrypt", "true");

  }

  /**
   * 發送 XMLHttpRequest。
   * - `async=true` 時以 callback/success/error/final 接收結果
   * - `async=false` 時會阻塞 UI，並在 `send()` 後直接回傳 response JSON（僅 status=200）
   */
  public ajax(): any {

    try {

      // 執行 before 函數，若回傳 false 則跳脫
      if (this.before != undefined && this.before() === false) {
        return;
      } else {
        setTimeout(() => {
          if (this.loadingMsg != null) clsSweetMsg.OpenLoading(this.loadingMsg);
        });
      }

      this.xhr.onload = () => {

        if (this.xhr.status === 200) {
          this.ReturnVal = JSON.parse(this.xhr.responseText);

          if (this.success) {
            try {
              this.success.call(null, this.ReturnVal);
            } catch (ex) {
              console.log(ex);
            }
          }

          if (this.callback) {
            try {
              this.callback.call(null, this.ReturnVal);
            } catch (ex) {
              console.log(ex);
            }
          }

        } else {

          if (this.error) {
            try {
              this.error.call(null, this.ReturnVal);
            } catch (ex) {
              console.log(ex);
            }
          }

        }

        setTimeout(() => {
          if (this.loadingMsg != null) clsSweetMsg.CloseLoading(this.loadingMsg);
        });

        if (this.final) {
          try {
            this.final.call(null);
          } catch (ex) {
            console.log(ex);
          }
        }


      };
      this.xhr.onerror = (e: any) => {
        Swal.fire({
          title: 'Error!',
          text: e,
          icon: 'error',
          confirmButtonText: 'Ok'
        })
      };

      if (this.isPostModel) {
        let strJson: string = encodeURI(JSON.stringify(this.body));
        if (this.encrypt) strJson = window.btoa(strJson);
        this.xhr.send(JSON.stringify({ PostData: strJson }));
      } else {
        this.xhr.send(JSON.stringify(this.body));
      }

      //#region 同步狀態回傳
      if (!this.async) {
        if (this.xhr.status === 200) {
          return JSON.parse(this.xhr.responseText);
        } else {
          console.error(this.xhr.status);
        }
      }
      //#endregion

    } catch (ex: any) {
      console.log(ex);
      throw ex;
    }

  }


}

export class clsSweetMsg {

  private static oAlert: Array<{ msg: string, title: string, icon: string, callback: Function }> = [];
  private static oOpen: Array<string> = [];
  private static TimerAlert: any;

  public static Swal: any = Swal;

  /**
   * Sweet Loading 畫面
   * @param msg 訊息
   */
  public static OpenLoading(msg?: string): any {
    try {

      this.oOpen.push(msg == null ? "Loading" : msg);

      console.log(this.oOpen);

      if (this.oOpen.length > 0) {
        let d = this.oOpen[0];
        Swal.fire({
          text: d,
          didOpen: () => {
            Swal.showLoading();
          },
          allowOutsideClick: false,
          allowEscapeKey: false,
          willOpen: () => {
            document.addEventListener('keydown', (event) => {
              if (event.key === 'Enter') {
                event.preventDefault();
              }
            });
          },
        });
      }

    } catch (ex: any) {
      console.log(ex);
    }
  }

  /** Sweet Loading 畫面關閉 */
  public static CloseLoading(msg?: string): any {
    try {

      if (!msg) {
        this.oOpen.shift();
      } else {
        this.oOpen.forEach((o, i) => {
          if (o == msg) {
            this.oOpen.splice(i, 1);
            return;
          }
        });
      }

      //console.log(this.oOpen);

      if (this.oOpen.length == 0) {
        Swal.close();
        return;
      }

      if (this.oAlert.length > 0) {
        this.TimerAlert = window.setInterval(() => this.OpenAlert(), 50);
      }

    }
    catch (ex: any) {
      console.log(ex);
    }
  }

  /** Sweet Loading 訊息清除 */
  public static CloseAll(): any {
    try {

      this.oOpen = [];
      this.oAlert = [];
      Swal.close();

    }
    catch (ex: any) {
      console.log(ex);
    }
  }

  /**
   * 網頁訊息
   * @param msg 訊息
   * @param title 標題
   * @param icon 圖示
   */
  public static alert(
    msg: string,
    title: string = "",
    icon: 'success' | 'error' | 'warning' | 'info' | 'question' = "warning",
    callback: Function = function () { }
  ): any {
    try {

      if (!this.isSwalOpen()) {

        Swal.fire({
          title: title,
          html: msg,
          icon: icon
        }).then((result) => {
          //console.log(result);
          if (callback && result.isConfirmed) {
            callback.call(null);
          }
        });

      } else {
        this.oAlert.push({
          msg: msg,
          title: title,
          icon: icon,
          callback: callback
        });

        this.TimerAlert = window.setInterval(() => this.OpenAlert(), 50);
      }

    } catch (ex: any) {
      console.log(ex);
    }
  }

  private static OpenAlert(): any {
    try {

      if (this.oAlert.length > 0) {

        if (!this.isSwalOpen()) {
          let d = this.oAlert[0];
          // 檢查 d.icon 是否為合法的圖示類型
          const validIcons: Array<'success' | 'error' | 'warning' | 'info' | 'question'> = ['success', 'error', 'warning', 'info', 'question'];
          if (validIcons.includes(d.icon as any)) {
            this.alert(d.msg, d.title, d.icon as 'success' | 'error' | 'warning' | 'info' | 'question', d.callback);
          } else {
            console.error(`Invalid icon type: ${d.icon}`);
          }
          this.oAlert = this.oAlert.splice(1, 1);
        }

      } else {
        clearInterval(this.TimerAlert);
        //this.CloseLoading();
      }
    } catch (ex: any) {
      console.log(ex);
    }
  }

  /**
   * 判斷是否有 Swal 訊息在使用中
   * @returns {boolean} 是否有訊息在使用中
   */
  private static isSwalOpen(): boolean {
    return !!document.querySelector('.swal2-container');
  }

}

export class clsFlatpickr {

  /**
   * 配置 flatpickr
   * @param element 元素
   * @param config 配置
   */
  public static setFlatpickr(element: Element, config: object) {
    flatpickr(element, config);
  }

  /**
   * 配置日期選擇器
   * @param Document 全域的 Document 物件
   */
  public static setDate(Document = document) {
    // 取得所有具有 flatpickr-date 類別的元素
    const datePickers = Document.querySelectorAll('.flatpickr-date');

    // 對每個元素進行 flatpickr 初始化
    datePickers.forEach((datePicker) => {

      flatpickr(datePicker, {
        locale: Mandarin,
      });

    });

    // 取得所有具有 flatpickr-time 類別的元素
    const timePickers = Document.querySelectorAll('.flatpickr-time');

    // 對每個元素進行 flatpickr 初始化
    timePickers.forEach((timePicker) => {

      flatpickr(timePicker, {
        locale: Mandarin,
        enableTime: true,
        noCalendar: true,
        dateFormat: "H:i",
      });

    });

    // 取得所有具有 flatpickr-datetime 類別的元素
    const datetimePickers = Document.querySelectorAll('.flatpickr-datetime');

    // 對每個元素進行 flatpickr 初始化
    datetimePickers.forEach((datePicker) => {

      flatpickr(datePicker, {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        locale: Mandarin,
      });

    });
  }

}

export class clsPaginationPage {
  // 每頁顯示的資料列數
  PageRowCount: number = 10;

  // 當前頁碼
  NowPageNum: number = 1;

  // 總資料數
  DataCount: number = 0;

  // 總頁數
  PageCount: number = 0;

  // 頁碼項目陣列，用於存放頁碼
  PageItem: Array<number> = [];

  // 前一頁的頁碼
  PagePrevious: number = 0;

  // 下一頁的頁碼
  PageNext: number = 0;

  // Table資料
  Data: any = [];

  // 選取的資料
  selectedRow: any = null;

  // 回呼函式
  CallBack: any = function () {};

  constructor(init?: Partial<clsPaginationPage>) {
    Object.assign(this, init);
  }

  /**
   * 取得頁碼項目
   * @param NowPageNum
   */
  public GetPageItem(): void {

    this.PageItem = [];
    this.selectedRow = null;
    let sp: number = (this.NowPageNum > 5 ? this.NowPageNum - 4 : 1);
    let ep: number = this.NowPageNum + 5;
    if (ep > this.PageCount) ep = this.PageCount;
    if (ep - sp < 9) sp = ep - 10;
    if (sp < 1) sp = 1;
    for (let i = sp; i <= ep; i++) {
      this.PageItem.push(i);
    }

    this.PagePrevious = this.NowPageNum - 10 < 0 ? 1 : this.NowPageNum - 10;
    this.PageNext = this.NowPageNum + 10 > this.PageCount ? this.PageCount : this.NowPageNum + 10;

  }

  /**
   * 設定頁碼
   * @param NowPageNum
   */
  public SetPage(NowPageNum?: number): void {

    try {

      if (NowPageNum != null) this.NowPageNum = NowPageNum;

      if (this.CallBack) {
        try {
          this.CallBack.call(null);
        } catch (ex) {
          console.log(ex);
        }

      }
    } catch (ex) {
      console.log(ex);
    }

  }
}

export class clsConfig {
  /** 系統標題 */
  public static SystemTitle: string = '台中捷運 EIP';

  /** 基礎路徑 (從 Vite 環境變數取得) */
  public static BasePath: string = import.meta.env.BASE_URL || '/';

  /** API 路徑 */
  public static ApiPath: string = '/api/login';

  /** 目前路徑 */
  public static RouterPath: string = '';

  /** 使用者資訊 */
  public static User: any = {};

  /** 目前的主題 */
  public static currentTheme: string = 'auto';

  /** 選單資料 */
  public static MenuData: any = [];

  /** 路徑資料 */
  public static PathData: any = [];

  /** 當前頁面標題 */
  public static currentTitle: string = '';

  /** 完整頁面標題 */
  public static fullTitle: string = this.SystemTitle;

  /**
   * 設定頁面標題
   * @param title 標題
   */
  public static setTitle(title: string): void {
    this.currentTitle = title;
    this.fullTitle = title ? `${title}-${this.SystemTitle}` : this.SystemTitle;
    document.title = this.fullTitle;
  }

  /**
   * 取得當前標題
   */
  public static getTitle(): string {
    return this.currentTitle;
  }

  /**
   * 取得完整標題
   */
  public static getFullTitle(): string {
    return this.fullTitle;
  }

  /**
  * 取得完整的 API 路徑
  * @param apiPath API 子路徑
  * @returns 完整的 API 路徑
  */
  public static getFullApiPath(apiPath: string): string {
    // 確保 BasePath 以 / 結尾，apiPath 不以 / 開頭
    const basePath = this.BasePath.endsWith('/') ? this.BasePath.slice(0, -1) : this.BasePath;
    const cleanApiPath = apiPath.startsWith('/') ? apiPath : `/${apiPath}`;
    return `${basePath}${cleanApiPath}`;
  }
}

/**
 * 設定主題（light / dark / auto）
 * - 'auto' 時依 prefers-color-scheme 決定實際套用的深淺色
 * - 於 <html> 設定 data-theme 屬性，並同步切換 Tailwind 的 `dark` class
 * @param theme 主題
 */
export function DoSetTheme(theme: string = '') {
  if (theme == '') theme = localStorage['theme'] ? localStorage['theme'] : 'auto'

  localStorage['theme'] = theme
  clsConfig.currentTheme = theme

  const resolved =
    theme === 'auto'
      ? window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
        ? 'dark'
        : 'light'
      : theme

  const root = document.documentElement
  root.setAttribute('data-theme', theme)
  root.classList.toggle('dark', resolved === 'dark')
  // 同步切換 Sakai 範本（src/sakai/）使用的 .app-dark，讓 PrimeVue 元件深色樣式全站一致。
  root.classList.toggle('app-dark', resolved === 'dark')
}

/**
 * 取得首頁
 */
export function GetHome(): void {

  new clsAjaxAsync({
    loadingMsg: "Loading",
    url: clsConfig.ApiPath + "/GetHome",
    body: {
      LastPath: window.localStorage["LastPath"] ? window.localStorage["LastPath"] : null,
    },
    isPostModel: true,
    encrypt: true,
    async: true,
    success: (res: any) => {
      //console.log(res);
      window.localStorage.removeItem("LastPath");
      if (res.Result == true) {

        try {
          if (res.Home && (clsConfig.RouterPath == "/login" || clsConfig.RouterPath == "/SystemSetting/Pwd" || clsConfig.RouterPath == "/AdminLogin"))
            location.href = `${import.meta.env.BASE_URL}#${res.Home[0].m_Path}`;
        } catch {
          clsSweetMsg.alert("沒有權限，請洽系統管理員", "無法登入", "warning"
            , function () {
              DoSignOut();
            }
          );

        }

      } else {
        clsSweetMsg.alert(res.ReturnMsg, res.ReturnNumber, "error");
      }
    },

  }).ajax();

}

/**
 * 驗證登入狀態與權限
 * @param SysPath 要驗證的系統路徑（通常為路由路徑）
 */
export function CmpAuth(SysPath: string): boolean {

  try {

    let res = new clsAjaxAsync({
      url: clsConfig.ApiPath + "/CmpAuth",
      body: {
        path: SysPath
      },
      isPostModel: true,
      encrypt: true,
      async: false
    }).ajax();

    let user = res.user;
    user["HeadImg"] = res.HeadImg;
    clsConfig.User = user;

    if (Number(new Date().format("yyyyMMddhhmmss")) - Number(res.Token_Time) / 10000 > (res.TokenLimit * 60 - 30)) GetToken();

    return res.Result;

  } catch (error) {
    return false;
  }
}

/**
 * 驗證登入狀態與權限
 */
export function CmpSignStatus(): void {

  new clsAjaxAsync({
    loadingMsg: "驗證登入狀態與權限",
    url: clsConfig.ApiPath + "/CmpSignStatus",
    body: {},
    isPostModel: true,
    encrypt: true,
    async: true,
    success: (res: any) => {
      console.log(res);
      if (res.Result == false) {

        if (res.ReturnNumber == "N003" || res.ReturnNumber == "N005") {

          if (clsConfig.RouterPath == "/login") {

            DoSignOut();

          } else {

            clsSweetMsg.alert(res.ReturnMsg, res.ReturnNumber, "error"
              , function () {
                DoSignOut();
              }
            );

          }

        } else {

          Swal.fire({
            title: 'Error!',
            text: '發生錯誤，請稍後再試。',
            icon: 'error',
            confirmButtonText: '確定'
          });

        }

      } else {

        GetHome();

      }

    },
  }).ajax();

}

/**
 * 取得新的Token
 */
export function GetToken(): void {

  new clsAjaxAsync({
    loadingMsg: "更新金鑰",
    url: clsConfig.ApiPath + "/GetToken",
    body: {},
    isPostModel: true,
    encrypt: true,
    async: true,
    success: (res: any) => {
      console.log(res);
      if (res.Result == true) {
        localStorage["token"] = res.Token;
      } else {
        if (res.ReturnNumber == "N003" || res.ReturnNumber == "N005") {

          if (clsConfig.RouterPath == "/login") {

            DoSignOut();

          } else {

            clsSweetMsg.alert(res.ReturnMsg, res.ReturnNumber, "error"
              , function () {
                DoSignOut();
              }
            );

          }

        }

      }
    },
  }).ajax();
}

/**
 * 登出
 */
export function DoSignOut(): void {
  new clsAjaxAsync({
    loadingMsg: "登出",
    url: clsConfig.ApiPath + "/DoSignOut",
    body: {},
    isPostModel: true,
    encrypt: true,
    async: true,
    success: (res: any) => {
      //console.log(res);
      if (res.Result == true) {
        //清空選單資料
        clsConfig.MenuData = [];
        window.localStorage.removeItem("token");
        window.localStorage.removeItem("LastPath");
        clsConfig.User = {};
        location.href = `${import.meta.env.BASE_URL}#/login`;
      }
    },
  }).ajax();
}

/**
 * 更新路徑資料
 */
export function DoUpdatePathData(): void {

  try {

    clsConfig.PathData = [
      {
        icon: 'pi pi-home',
        url: null,
      }
    ];

    for (const menu of clsConfig.MenuData) {
      if (menu.m_Type != "app") {

        for (const subItem of menu.subItems) {
          if (subItem.m_Path === clsConfig.RouterPath) {
            clsConfig.PathData.push({
              icon: menu.m_Icon,
              label: menu.m_Name,
              url: null,
            });
            clsConfig.PathData.push({
              icon: subItem.m_Icon,
              label: subItem.m_Name,
              url: null,
            });


            break;
          }
        }

      } else {

        if (menu.m_Path === clsConfig.RouterPath) {
          clsConfig.PathData.push({
            icon: menu.m_Icon,
            label: menu.m_Name,
            url: menu.m_Path,
          });
          break;
        }

      }

    }

    //console.log(clsConfig.MenuData);
    console.log(clsConfig.PathData);

  } catch (ex) {
    console.log(ex);
  }

}

/**
 * 路由跳轉
 * @param path 路徑
 */
export function DoRouterPush(path: string): void {
  router.push({ path: path });
}

/**
 * 複製物件值
 * @param Source 來源物件
 * @param Target 目標物件
 */
export function updateObjectProperties(Source: any, Target: any) {
  Object.entries(Target).forEach(([key, value]) => {
    Target[key] = Source && Source[key] != null && Source[key] != undefined ? Source[key] : null;
  });
  setTimeout(() => {
    Object.entries(Target).forEach(([key, value]) => {
      Target[key] = Source && Source[key] != null && Source[key] != undefined ? Source[key] : null;
    });
  }, 300);
}

//#region 表單驗證

// 定義一個表單驗證類別
export class clsValidateForm {

  // 使用 Zod 進行驗證
  private Zod: any = z;
  private ZodResolver: any = zodResolver;
  public schema: any = this.Zod.object();
  private ZodObject: any;
  private errors: any = [];

  // 驗證參數
  constructor(obj: any) {
    if (obj) {
      // 如果有傳入物件，則建立 Zod schema
      this.schema = this.Zod.object(obj);
    }
  }

  // 新增或更新 schema
  public AddSchema(obj: any): void {
    this.schema = this.Zod.object(obj);
  }

  // 取得 ZodResolver
  public GetZodResolver() {
    return this.ZodResolver(this.schema);
  }

  // 驗證表單資料
  public Validate(dataFrom: any): boolean {
    console.log(this.schema);
    const result = this.schema.safeParse(dataFrom);
    if (!result.success) {
      // 如果驗證失敗，顯示錯誤訊息
      this.errors = result.error.errors;
      let strErrMsg = "";
      result.error.errors.forEach(function (o: any, i: Number) {
        strErrMsg += o.message + "<br/>";;
      });
      clsSweetMsg.alert(strErrMsg, "送出失敗", "error");
      return false;
    }
    this.errors = [];
    return true;
  }

  // 驗證表單資料
  public ValidateResult(dataFrom: any): any {
    const result = this.schema.safeParse(dataFrom);
    return result;
  }

  // 清除錯誤訊息
  public ClearErrors(): void {
    this.errors = [];
  }

}

// 匯出 Zod 物件
export const Zod = z;
export const ZodResolver = zodResolver;

//#endregion
