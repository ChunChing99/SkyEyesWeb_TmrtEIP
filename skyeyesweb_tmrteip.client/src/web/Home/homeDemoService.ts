// 首頁（Home）DEMO 假資料。
// 目前不串接後端與資料庫，僅供展示版面與互動；之後接上 API 時，
// 只要把這裡的各個 getXxx() 改成呼叫 API client，回傳型別維持不變，Home 各元件不需修改。

export interface HomeUser {
    name: string;
    dept: string;
    position: string;
}

export interface HomeStat {
    label: string;
    value: string;
    icon: string;
    /** 圖示底色（Tailwind class，需寫完整字串才會被 Tailwind 掃描產生） */
    iconBgClass: string;
    /** 圖示顏色（Tailwind class） */
    iconClass: string;
    highlight: string;
    note: string;
}

export type SignUrgency = '一般' | '急件' | '特急';

export interface PendingSignItem {
    /** 單據編號，格式比照 SYS_ttIssuedSerialNumber：{DocType}-{OrgCode}-{YYYYMMDD}-{Seq} */
    formNo: string;
    formName: string;
    applicant: string;
    dept: string;
    nodeName: string;
    submitTime: string;
    urgency: SignUrgency;
}

export interface Announcement {
    id: number;
    title: string;
    category: '公告' | '行政' | '營運' | '資訊' | '人事';
    dept: string;
    publishDate: string;
    isTop: boolean;
    isNew: boolean;
}

export interface ScheduleItem {
    time: string;
    title: string;
    location: string;
    icon: string;
    color: string;
}

export interface QuickLink {
    label: string;
    icon: string;
    iconBgClass: string;
    iconClass: string;
}

export interface SignTrend {
    labels: string[];
    submitted: number[];
    approved: number[];
}

/** 以今天為基準往前推 n 天的日期字串（讓 DEMO 資料看起來永遠是「最近」的） */
function daysAgo(n: number, time = ''): string {
    const date = new Date().addDays(-n).format('yyyy-MM-dd');
    return time ? `${date} ${time}` : date;
}

/** 以今天為基準往前推 n 天的 YYYYMMDD（單據編號用） */
function periodKey(n: number): string {
    return new Date().addDays(-n).format('yyyyMMdd');
}

export const HomeDemoService = {
    getUser(): Promise<HomeUser> {
        return Promise.resolve({ name: '王小明', dept: '資訊處 系統開發課', position: '工程師' });
    },

    getStats(): Promise<HomeStat[]> {
        return Promise.resolve([
            {
                label: '待我簽核',
                value: '6',
                icon: 'pi pi-pen-to-square',
                iconBgClass: 'bg-orange-100 dark:bg-orange-400/10',
                iconClass: 'text-orange-500',
                highlight: '2 件',
                note: '為急件'
            },
            {
                label: '我的申請',
                value: '3',
                icon: 'pi pi-send',
                iconBgClass: 'bg-blue-100 dark:bg-blue-400/10',
                iconClass: 'text-blue-500',
                highlight: '1 件',
                note: '本週已核准'
            },
            {
                label: '未讀公告',
                value: '4',
                icon: 'pi pi-megaphone',
                iconBgClass: 'bg-cyan-100 dark:bg-cyan-400/10',
                iconClass: 'text-cyan-500',
                highlight: '1 則',
                note: '置頂公告'
            },
            {
                label: '今日行程',
                value: '3',
                icon: 'pi pi-calendar',
                iconBgClass: 'bg-purple-100 dark:bg-purple-400/10',
                iconClass: 'text-purple-500',
                highlight: '10:00',
                note: '下一場會議'
            }
        ]);
    },

    getPendingSigns(): Promise<PendingSignItem[]> {
        return Promise.resolve([
            { formNo: `PA-RD-${periodKey(0)}-0012`, formName: '採購申請單', applicant: '陳怡君', dept: '資訊處', nodeName: '課長審核', submitTime: daysAgo(0, '08:42'), urgency: '特急' },
            { formNo: `LV-HR-${periodKey(0)}-0031`, formName: '請假單', applicant: '林志豪', dept: '人資處', nodeName: '主管核准', submitTime: daysAgo(0, '08:15'), urgency: '一般' },
            { formNo: `DRAW-EN-${periodKey(1)}-0004`, formName: '工程圖紙審查', applicant: '張家瑋', dept: '工務處', nodeName: '技術審查', submitTime: daysAgo(1, '16:30'), urgency: '急件' },
            { formNo: `OT-OP-${periodKey(1)}-0018`, formName: '加班申請單', applicant: '黃佩琪', dept: '營運處', nodeName: '課長審核', submitTime: daysAgo(1, '14:05'), urgency: '一般' },
            { formNo: `PA-MT-${periodKey(2)}-0007`, formName: '採購申請單', applicant: '吳建宏', dept: '維修處', nodeName: '經理核決', submitTime: daysAgo(2, '10:20'), urgency: '一般' },
            { formNo: `LV-FN-${periodKey(3)}-0009`, formName: '請假單', applicant: '李雅婷', dept: '財務處', nodeName: '主管核准', submitTime: daysAgo(3, '09:48'), urgency: '一般' }
        ]);
    },

    getAnnouncements(): Promise<Announcement[]> {
        return Promise.resolve([
            { id: 1, title: '本公司年度消防演練將於下週三上午辦理，請各單位配合', category: '公告', dept: '總務處', publishDate: daysAgo(0), isTop: true, isNew: true },
            { id: 2, title: '綠線尖峰時段加開班次調整說明', category: '營運', dept: '營運處', publishDate: daysAgo(1), isTop: false, isNew: true },
            { id: 3, title: 'EIP 系統將於週六 22:00 至 24:00 進行例行維護', category: '資訊', dept: '資訊處', publishDate: daysAgo(2), isTop: false, isNew: true },
            { id: 4, title: '年度員工健康檢查報名開始，請於月底前完成登記', category: '人事', dept: '人資處', publishDate: daysAgo(3), isTop: false, isNew: true },
            { id: 5, title: '差旅費報支作業要點修正公告', category: '行政', dept: '財務處', publishDate: daysAgo(6), isTop: false, isNew: false },
            { id: 6, title: '資訊安全教育訓練（線上課程）開放選課', category: '資訊', dept: '資訊處', publishDate: daysAgo(8), isTop: false, isNew: false }
        ]);
    },

    getTodaySchedule(): Promise<ScheduleItem[]> {
        return Promise.resolve([
            { time: '10:00', title: 'EIP 專案週會', location: '行政大樓 3F 第二會議室', icon: 'pi pi-users', color: '#3b82f6' },
            { time: '14:00', title: '電子簽核流程需求訪談', location: '線上會議（Teams）', icon: 'pi pi-video', color: '#8b5cf6' },
            { time: '16:30', title: '資安教育訓練', location: '北屯機廠 訓練教室 B', icon: 'pi pi-shield', color: '#f97316' }
        ]);
    },

    getQuickLinks(): Promise<QuickLink[]> {
        return Promise.resolve([
            { label: '請假申請', icon: 'pi pi-calendar-minus', iconBgClass: 'bg-blue-100 dark:bg-blue-400/10', iconClass: 'text-blue-500' },
            { label: '加班申請', icon: 'pi pi-clock', iconBgClass: 'bg-orange-100 dark:bg-orange-400/10', iconClass: 'text-orange-500' },
            { label: '採購申請', icon: 'pi pi-shopping-cart', iconBgClass: 'bg-green-100 dark:bg-green-400/10', iconClass: 'text-green-500' },
            { label: '文件查詢', icon: 'pi pi-folder-open', iconBgClass: 'bg-cyan-100 dark:bg-cyan-400/10', iconClass: 'text-cyan-500' },
            { label: '代理人設定', icon: 'pi pi-user-edit', iconBgClass: 'bg-purple-100 dark:bg-purple-400/10', iconClass: 'text-purple-500' },
            { label: '通訊錄', icon: 'pi pi-address-book', iconBgClass: 'bg-pink-100 dark:bg-pink-400/10', iconClass: 'text-pink-500' }
        ]);
    },

    getSignTrend(): Promise<SignTrend> {
        const labels: string[] = [];
        for (let i = 6; i >= 0; i--) {
            labels.push(new Date().addDays(-i).format('MM/dd'));
        }
        return Promise.resolve({
            labels,
            submitted: [12, 18, 9, 15, 21, 6, 14],
            approved: [10, 15, 11, 13, 18, 7, 11]
        });
    }
};
