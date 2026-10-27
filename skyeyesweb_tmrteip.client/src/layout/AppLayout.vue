<script setup lang="ts">
// 本專案的主版型，結構比照 Sakai 範本（src/sakai/layout/AppLayout.vue）：
// Topbar + Sidebar(選單) + 主內容(router-view) + Footer。
// 版面樣式與狀態（選單收合、深色模式）直接沿用 Sakai 的 layout scss 與 useLayout，避免重複維護兩份。
import { useLayout } from '@/sakai/layout/composables/layout';
import { computed } from 'vue';
import AppFooter from './AppFooter.vue';
import AppSidebar from './AppSidebar.vue';
import AppTopbar from './AppTopbar.vue';

import '@/sakai/assets/tailwind.css';
import '@/sakai/assets/styles.scss';

const { layoutConfig, layoutState, hideMobileMenu } = useLayout();

const containerClass = computed(() => {
    return {
        'layout-overlay': layoutConfig.menuMode === 'overlay',
        'layout-static': layoutConfig.menuMode === 'static',
        'layout-overlay-active': layoutState.overlayMenuActive,
        'layout-mobile-active': layoutState.mobileMenuActive,
        'layout-static-inactive': layoutState.staticMenuInactive
    };
});
</script>

<template>
    <div class="layout-wrapper" :class="containerClass">
        <AppTopbar />
        <AppSidebar />
        <div class="layout-main-container">
            <div class="layout-main">
                <router-view />
            </div>
            <AppFooter />
        </div>
        <div class="layout-mask animate-fadein" @click="hideMobileMenu" />
    </div>
    <Toast />
</template>
