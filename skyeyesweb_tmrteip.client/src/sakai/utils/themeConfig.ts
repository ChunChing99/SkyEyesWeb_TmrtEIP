import type { LayoutConfig } from '@/sakai/layout/composables/layout';
import { generatePalette } from '@/sakai/utils/colorPalette';
import { $t } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';
import Lara from '@primeuix/themes/lara';
import Nora from '@primeuix/themes/nora';

export interface ThemeConfigFile {
    version: 1;
    preset: string;
    primary: string;
    surface: string;
    content: string;
    menu: string;
    menuMode: string;
    darkTheme: boolean;
}

export const PRESETS = { Aura, Lara, Nora };
export const PRESET_NAMES = Object.keys(PRESETS);
export const MENU_MODE_VALUES = ['static', 'overlay'];

const HEX_COLOR_PATTERN = /^#?[0-9a-fA-F]{6}$/;

export function isValidThemeConfig(value: unknown): value is ThemeConfigFile {
    if (!value || typeof value !== 'object') {
        return false;
    }

    const v = value as Record<string, unknown>;

    return (
        typeof v.preset === 'string' &&
        PRESET_NAMES.includes(v.preset) &&
        typeof v.primary === 'string' &&
        HEX_COLOR_PATTERN.test(v.primary) &&
        typeof v.surface === 'string' &&
        HEX_COLOR_PATTERN.test(v.surface) &&
        typeof v.content === 'string' &&
        HEX_COLOR_PATTERN.test(v.content) &&
        typeof v.menu === 'string' &&
        HEX_COLOR_PATTERN.test(v.menu) &&
        typeof v.menuMode === 'string' &&
        MENU_MODE_VALUES.includes(v.menuMode) &&
        typeof v.darkTheme === 'boolean'
    );
}

export function buildPresetExtension(colors: { primary: string; content: string; menu: string }) {
    const primaryPalette = generatePalette(colors.primary);
    const contentPalette = generatePalette(colors.content);
    const menuPalette = generatePalette(colors.menu);

    return {
        semantic: {
            primary: primaryPalette,
            colorScheme: {
                light: {
                    primary: {
                        color: '{primary.500}',
                        contrastColor: '#ffffff',
                        hoverColor: '{primary.600}',
                        activeColor: '{primary.700}'
                    },
                    highlight: {
                        background: '{primary.50}',
                        focusBackground: '{primary.100}',
                        color: '{primary.700}',
                        focusColor: '{primary.800}'
                    },
                    content: {
                        background: contentPalette['50']
                    },
                    overlay: {
                        popover: {
                            background: menuPalette['50']
                        }
                    }
                },
                dark: {
                    primary: {
                        color: '{primary.400}',
                        contrastColor: '{surface.900}',
                        hoverColor: '{primary.300}',
                        activeColor: '{primary.200}'
                    },
                    highlight: {
                        background: 'color-mix(in srgb, {primary.400}, transparent 84%)',
                        focusBackground: 'color-mix(in srgb, {primary.400}, transparent 76%)',
                        color: 'rgba(255,255,255,.87)',
                        focusColor: 'rgba(255,255,255,.87)'
                    },
                    content: {
                        background: contentPalette['900']
                    },
                    overlay: {
                        popover: {
                            background: menuPalette['900']
                        }
                    }
                }
            }
        }
    };
}

export function buildSurfacePalette(surfaceHex: string) {
    return { 0: '#ffffff', ...generatePalette(surfaceHex) };
}

export function computeMenuHoverVars(menuHex: string) {
    const menuPalette = generatePalette(menuHex);

    return { light: menuPalette['100'] ?? '', dark: menuPalette['800'] ?? '' };
}

function applyMenuHoverVars(menuHex: string) {
    const hover = computeMenuHoverVars(menuHex);

    document.documentElement.style.setProperty('--menu-hover-light', hover.light);
    document.documentElement.style.setProperty('--menu-hover-dark', hover.dark);
}

/**
 * Applies a full theme configuration: mutates layoutConfig, swaps the active
 * PrimeVue preset/palettes, sets dark mode, and updates the menu-hover vars.
 */
export function applyThemeConfig(config: ThemeConfigFile, layoutConfig: LayoutConfig, setDarkTheme: (value: boolean) => void) {
    layoutConfig.primary = config.primary;
    layoutConfig.surface = config.surface;
    layoutConfig.content = config.content;
    layoutConfig.menu = config.menu;
    layoutConfig.menuMode = config.menuMode;
    layoutConfig.preset = config.preset;

    setDarkTheme(config.darkTheme);

    const presetValue = PRESETS[config.preset as keyof typeof PRESETS];

    $t().preset(presetValue).preset(buildPresetExtension(config)).surfacePalette(buildSurfacePalette(config.surface)).use({ useDefaultOptions: true });

    applyMenuHoverVars(config.menu);
}

/**
 * Fetches /theme-config.json (served from public/) if present, validating its
 * shape. Returns null when the file is missing or invalid so callers can fall
 * back to the built-in defaults without treating it as an error.
 */
export async function fetchDefaultThemeConfig(): Promise<ThemeConfigFile | null> {
    try {
        const response = await fetch('/theme-config.json', { cache: 'no-store' });

        if (!response.ok) {
            return null;
        }

        const data: unknown = await response.json();

        if (!isValidThemeConfig(data)) {
            console.warn('Ignoring /theme-config.json: file does not match the expected theme configuration shape.');

            return null;
        }

        return data;
    } catch {
        return null;
    }
}
