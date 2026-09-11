import { computed, reactive } from 'vue';

export interface LayoutConfig {
    preset: string;
    primary: string;
    surface: string;
    content: string;
    menu: string;
    darkTheme: boolean;
    menuMode: string;
}

interface LayoutState {
    staticMenuInactive: boolean;
    overlayMenuActive: boolean;
    profileSidebarVisible: boolean;
    configSidebarVisible: boolean;
    sidebarExpanded: boolean;
    menuHoverActive: boolean;
    anchored: boolean;
    mobileMenuActive: boolean;
    activeMenuItem: string | null;
    activePath: string | null;
}

const layoutConfig = reactive<LayoutConfig>({
    preset: 'Aura',
    primary: '#10b981',
    surface: '#64748b',
    content: '#64748b',
    menu: '#64748b',
    darkTheme: false,
    menuMode: 'static'
});

const layoutState = reactive<LayoutState>({
    staticMenuInactive: false,
    overlayMenuActive: false,
    profileSidebarVisible: false,
    configSidebarVisible: false,
    sidebarExpanded: false,
    menuHoverActive: false,
    anchored: false,
    mobileMenuActive: false,
    activeMenuItem: null,
    activePath: null
});

export function useLayout() {
    const toggleDarkMode = () => {
        if (!document.startViewTransition) {
            executeDarkModeToggle();

            return;
        }

        document.startViewTransition(() => executeDarkModeToggle());
    };

    const executeDarkModeToggle = () => {
        layoutConfig.darkTheme = !layoutConfig.darkTheme;
        // .dark 是主專案（src/main.ts 的 DoSetTheme）使用的深色模式 class，一併同步
        // 讓離開 /sakai 後 Tailwind 的 dark: 樣式與 PrimeVue 深色樣式維持一致。
        document.documentElement.classList.toggle('app-dark', layoutConfig.darkTheme);
        document.documentElement.classList.toggle('dark', layoutConfig.darkTheme);
    };

    const setDarkTheme = (value: boolean) => {
        layoutConfig.darkTheme = value;
        document.documentElement.classList.toggle('app-dark', value);
        document.documentElement.classList.toggle('dark', value);
    };

    const toggleMenu = () => {
        if (isDesktop()) {
            if (layoutConfig.menuMode === 'static') {
                layoutState.staticMenuInactive = !layoutState.staticMenuInactive;
            }

            if (layoutConfig.menuMode === 'overlay') {
                layoutState.overlayMenuActive = !layoutState.overlayMenuActive;
            }
        } else {
            layoutState.mobileMenuActive = !layoutState.mobileMenuActive;
        }
    };

    const toggleConfigSidebar = () => {
        layoutState.configSidebarVisible = !layoutState.configSidebarVisible;
    };

    const hideMobileMenu = () => {
        layoutState.mobileMenuActive = false;
    };

    const changeMenuMode = (event: { value: string }) => {
        layoutConfig.menuMode = event.value;
        layoutState.staticMenuInactive = false;
        layoutState.mobileMenuActive = false;
        layoutState.sidebarExpanded = false;
        layoutState.menuHoverActive = false;
        layoutState.anchored = false;
    };

    const isDarkTheme = computed(() => layoutConfig.darkTheme);
    const isDesktop = () => window.innerWidth > 991;

    const hasOpenOverlay = computed(() => layoutState.overlayMenuActive);

    return {
        layoutConfig,
        layoutState,
        isDarkTheme,
        toggleDarkMode,
        setDarkTheme,
        toggleConfigSidebar,
        toggleMenu,
        hideMobileMenu,
        changeMenuMode,
        isDesktop,
        hasOpenOverlay
    };
}
