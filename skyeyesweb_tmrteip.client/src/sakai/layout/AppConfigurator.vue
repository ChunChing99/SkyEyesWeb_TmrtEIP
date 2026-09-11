<script setup lang="ts">
import { useLayout } from '@/sakai/layout/composables/layout';
import { applyThemeConfig, buildPresetExtension, buildSurfacePalette, computeMenuHoverVars, isValidThemeConfig, PRESET_NAMES, PRESETS, type ThemeConfigFile } from '@/sakai/utils/themeConfig';
import { $t, updatePreset, updateSurfacePalette } from '@primeuix/themes';
import { useToast } from 'primevue/usetoast';
import { ref } from 'vue';

const { layoutConfig, changeMenuMode, setDarkTheme } = useLayout();
const toast = useToast();
const fileInput = ref<HTMLInputElement | null>(null);

const preset = ref(layoutConfig.preset);
const presetOptions = ref(PRESET_NAMES);

const menuMode = ref(layoutConfig.menuMode);
const menuModeOptions = ref([
    { label: 'Static', value: 'static' },
    { label: 'Overlay', value: 'overlay' }
]);

function toHex(value: string): string {
    return value.startsWith('#') ? value : `#${value}`;
}

function applyMenuHoverVars() {
    const hover = computeMenuHoverVars(layoutConfig.menu);

    document.documentElement.style.setProperty('--menu-hover-light', hover.light);
    document.documentElement.style.setProperty('--menu-hover-dark', hover.dark);
}

function onPrimaryColorChange(value: string) {
    layoutConfig.primary = toHex(value);
    updatePreset(buildPresetExtension(layoutConfig));
}

function onSurfaceColorChange(value: string) {
    layoutConfig.surface = toHex(value);
    updateSurfacePalette(buildSurfacePalette(layoutConfig.surface));
}

function onContentColorChange(value: string) {
    layoutConfig.content = toHex(value);
    updatePreset(buildPresetExtension(layoutConfig));
}

function onMenuColorChange(value: string) {
    layoutConfig.menu = toHex(value);
    updatePreset(buildPresetExtension(layoutConfig));
    applyMenuHoverVars();
}

function applyFullTheme() {
    const presetValue = PRESETS[layoutConfig.preset as keyof typeof PRESETS];

    $t().preset(presetValue).preset(buildPresetExtension(layoutConfig)).surfacePalette(buildSurfacePalette(layoutConfig.surface)).use({ useDefaultOptions: true });
    applyMenuHoverVars();
}

function onPresetChange() {
    layoutConfig.preset = preset.value;
    applyFullTheme();
}

function exportConfig() {
    const data: ThemeConfigFile = {
        version: 1,
        preset: layoutConfig.preset,
        primary: layoutConfig.primary,
        surface: layoutConfig.surface,
        content: layoutConfig.content,
        menu: layoutConfig.menu,
        menuMode: layoutConfig.menuMode,
        darkTheme: layoutConfig.darkTheme
    };

    const blob = new Blob([JSON.stringify(data, null, 4)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');

    link.href = url;
    link.download = 'theme-config.json';
    link.click();
    URL.revokeObjectURL(url);

    toast.add({ severity: 'success', summary: 'Exported', detail: 'Theme configuration exported.', life: 3000 });
}

function triggerImport() {
    fileInput.value?.click();
}

function applyImportedConfig(config: ThemeConfigFile) {
    applyThemeConfig(config, layoutConfig, setDarkTheme);
    preset.value = layoutConfig.preset;
    menuMode.value = layoutConfig.menuMode;
}

async function onImportFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    input.value = '';

    if (!file) {
        return;
    }

    try {
        const text = await file.text();
        const parsed: unknown = JSON.parse(text);

        if (!isValidThemeConfig(parsed)) {
            throw new Error('This file is not a valid theme configuration.');
        }

        applyImportedConfig(parsed);
        toast.add({ severity: 'success', summary: 'Imported', detail: 'Theme configuration imported.', life: 3000 });
    } catch (error) {
        toast.add({ severity: 'error', summary: 'Import failed', detail: error instanceof Error ? error.message : 'Could not read the file.', life: 4000 });
    }
}
</script>

<template>
    <div
        class="config-panel hidden absolute top-[3.25rem] right-0 w-64 p-4 bg-surface-0 dark:bg-surface-900 border border-surface rounded-border origin-top shadow-[0px_3px_5px_rgba(0,0,0,0.02),0px_0px_2px_rgba(0,0,0,0.05),0px_1px_4px_rgba(0,0,0,0.08)]"
    >
        <div class="flex flex-col gap-4">
            <div>
                <span class="text-sm text-muted-color font-semibold">Primary</span>
                <div class="pt-2 flex items-center gap-3">
                    <ColorPicker :modelValue="layoutConfig.primary" format="hex" @update:modelValue="onPrimaryColorChange" />
                    <span class="text-sm text-muted-color">{{ layoutConfig.primary }}</span>
                </div>
            </div>
            <div>
                <span class="text-sm text-muted-color font-semibold">Surface</span>
                <div class="pt-2 flex items-center gap-3">
                    <ColorPicker :modelValue="layoutConfig.surface" format="hex" @update:modelValue="onSurfaceColorChange" />
                    <span class="text-sm text-muted-color">{{ layoutConfig.surface }}</span>
                </div>
            </div>
            <div>
                <span class="text-sm text-muted-color font-semibold">Header / Content</span>
                <div class="pt-2 flex items-center gap-3">
                    <ColorPicker :modelValue="layoutConfig.content" format="hex" @update:modelValue="onContentColorChange" />
                    <span class="text-sm text-muted-color">{{ layoutConfig.content }}</span>
                </div>
            </div>
            <div>
                <span class="text-sm text-muted-color font-semibold">Menu Background</span>
                <div class="pt-2 flex items-center gap-3">
                    <ColorPicker :modelValue="layoutConfig.menu" format="hex" @update:modelValue="onMenuColorChange" />
                    <span class="text-sm text-muted-color">{{ layoutConfig.menu }}</span>
                </div>
            </div>
            <div class="flex flex-col gap-2">
                <span class="text-sm text-muted-color font-semibold">Presets</span>
                <SelectButton v-model="preset" @change="onPresetChange" :options="presetOptions" :allowEmpty="false" />
            </div>
            <div class="flex flex-col gap-2">
                <span class="text-sm text-muted-color font-semibold">Menu Mode</span>
                <SelectButton v-model="menuMode" @change="changeMenuMode" :options="menuModeOptions" :allowEmpty="false" optionLabel="label" optionValue="value" />
            </div>
            <div class="flex flex-col gap-2">
                <span class="text-sm text-muted-color font-semibold">Import / Export</span>
                <div class="flex gap-2">
                    <Button label="Import" icon="pi pi-upload" size="small" outlined class="flex-1" @click="triggerImport" />
                    <Button label="Export" icon="pi pi-download" size="small" class="flex-1" @click="exportConfig" />
                </div>
                <input ref="fileInput" type="file" accept="application/json,.json" class="hidden" @change="onImportFileChange" />
            </div>
        </div>
    </div>
</template>
