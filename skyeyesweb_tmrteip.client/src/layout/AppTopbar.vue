<script setup lang="ts">
// 比照 src/sakai/layout/AppTopbar.vue：左側選單收合鈕 + Logo，右側深色模式、通知、使用者選單。
// DEMO 版：通知與使用者資訊為假資料，登出只回到登入頁（/），不清除 token。
import { useLayout } from '@/sakai/layout/composables/layout';
import type Menu from 'primevue/menu';
import type { MenuItem } from 'primevue/menuitem';
import type Popover from 'primevue/popover';
import { useToast } from 'primevue/usetoast';
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const { toggleMenu, toggleDarkMode, isDarkTheme } = useLayout();
const router = useRouter();
const toast = useToast();

const notifyPanel = ref<InstanceType<typeof Popover> | null>(null);
const userMenu = ref<InstanceType<typeof Menu> | null>(null);

const notifications = ref([
    { icon: 'pi pi-pen-to-square', color: 'text-orange-500', text: '陳怡君 送出「採購申請單」待您簽核', time: '10 分鐘前' },
    { icon: 'pi pi-check-circle', color: 'text-green-500', text: '您的「請假單」已核准', time: '1 小時前' },
    { icon: 'pi pi-megaphone', color: 'text-cyan-500', text: '總務處發布新公告：年度消防演練', time: '今天 08:30' }
]);

const userMenuItems: MenuItem[] = [
    { label: '王小明', items: [
        { label: '個人資料', icon: 'pi pi-user', command: () => showDemo('個人資料') },
        { label: '變更密碼', icon: 'pi pi-key', command: () => showDemo('變更密碼') },
        { separator: true },
        { label: '登出', icon: 'pi pi-sign-out', command: () => router.push('/') }
    ] }
];

function showDemo(label: string) {
    toast.add({ severity: 'info', summary: label, detail: 'DEMO 版：此功能尚在開發中', life: 2500 });
}
</script>

<template>
    <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" @click="toggleMenu">
                <i class="pi pi-bars"></i>
            </button>
            <router-link to="/home" class="layout-topbar-logo">
                <span class="flex items-center justify-center w-9 h-9 rounded-lg bg-primary text-primary-contrast text-lg! font-bold">T</span>
                <span class="hidden sm:inline">台中捷運 EIP</span>
            </router-link>
        </div>

        <div class="layout-topbar-actions">
            <div class="layout-config-menu">
                <button type="button" class="layout-topbar-action" title="切換深色模式" @click="toggleDarkMode">
                    <i :class="['pi', { 'pi-moon': isDarkTheme, 'pi-sun': !isDarkTheme }]"></i>
                </button>
                <button type="button" class="layout-topbar-action notify-dot relative" title="通知" @click="notifyPanel?.toggle($event)">
                    <i class="pi pi-bell"></i>
                </button>
                <Popover ref="notifyPanel">
                    <div class="w-80">
                        <div class="font-semibold mb-3">通知</div>
                        <ul class="list-none p-0 m-0">
                            <li v-for="(n, i) in notifications" :key="i" class="flex items-start gap-3 py-2 border-b border-surface last:border-b-0">
                                <i :class="[n.icon, n.color, 'mt-1']"></i>
                                <div>
                                    <div class="text-surface-900 dark:text-surface-0 leading-normal">{{ n.text }}</div>
                                    <div class="text-muted-color text-sm">{{ n.time }}</div>
                                </div>
                            </li>
                        </ul>
                    </div>
                </Popover>
            </div>

            <!-- 不套 layout-topbar-action：Sakai 會把該 class 內的 span 隱藏，且固定為 2.5rem 圓形按鈕 -->
            <button type="button" class="flex items-center gap-2 rounded-full p-1 lg:pr-3 cursor-pointer text-color hover:bg-emphasis transition-colors" @click="userMenu?.toggle($event)">
                <Avatar label="王" shape="circle" class="bg-primary! text-primary-contrast!" />
                <span class="hidden lg:inline">王小明</span>
                <i class="pi pi-angle-down hidden lg:inline"></i>
            </button>
            <Menu ref="userMenu" :model="userMenuItems" popup />
        </div>
    </div>
</template>

<style scoped>
.notify-dot::after {
    content: '';
    position: absolute;
    top: 0.45rem;
    right: 0.45rem;
    width: 0.5rem;
    height: 0.5rem;
    border-radius: 50%;
    background-color: var(--p-red-500);
}
</style>
