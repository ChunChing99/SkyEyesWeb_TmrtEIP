<script setup lang="ts">
// DEMO 選單：先以靜態資料呈現 EIP 預計的功能模組（對應 docs/schemas/database_schema.md 規劃的資料表）。
// 正式版會改由 SYS_tbMenu（POST /api/Base/GetRouterData）動態產生。
// 尚未開發的功能點擊時只跳提示，不做導頁。
import AppMenuItem from '@/sakai/layout/AppMenuItem.vue';
import type { MenuItem } from 'primevue/menuitem';
import { useToast } from 'primevue/usetoast';
import { ref } from 'vue';

const toast = useToast();

function demoItem(label: string, icon: string): MenuItem {
    return {
        label,
        icon: `pi pi-fw ${icon}`,
        command: () => toast.add({ severity: 'info', summary: label, detail: 'DEMO 版：此功能尚在開發中', life: 2500 })
    };
}

const model = ref<MenuItem[]>([
    {
        label: '首頁',
        items: [{ label: '首頁', icon: 'pi pi-fw pi-home', to: '/home' }]
    },
    {
        label: '電子簽核',
        items: [demoItem('待我簽核', 'pi-pen-to-square'), demoItem('我的申請', 'pi-send'), demoItem('簽核歷程', 'pi-history'), demoItem('代理人設定', 'pi-user-edit')]
    },
    {
        label: '文件管理',
        items: [demoItem('表單／文件', 'pi-file'), demoItem('文件版本', 'pi-copy')]
    },
    {
        label: '人事組織',
        items: [demoItem('組織架構', 'pi-sitemap'), demoItem('員工資料', 'pi-id-card')]
    },
    {
        label: '系統管理',
        path: '/system',
        items: [
            {
                label: '系統設定',
                icon: 'pi pi-fw pi-cog',
                path: '/setting',
                items: [demoItem('選單管理', 'pi-bars'), demoItem('權限群組', 'pi-lock'), demoItem('密碼規則', 'pi-key')]
            },
            demoItem('排程管理', 'pi-stopwatch'),
            demoItem('操作日誌', 'pi-list')
        ]
    }
]);
</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="i">
            <AppMenuItem v-if="!item.separator" :item="item" :index="i" />
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
    </ul>
</template>
