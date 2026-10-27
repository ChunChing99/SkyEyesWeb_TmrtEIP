<script setup lang="ts">
import { HomeDemoService, type QuickLink } from '@/web/Home/homeDemoService';
import { onMounted, ref } from 'vue';

const emit = defineEmits<{ (e: 'demo', label: string): void }>();

const links = ref<QuickLink[]>([]);

onMounted(async () => {
    links.value = await HomeDemoService.getQuickLinks();
});
</script>

<template>
    <div class="card">
        <div class="font-semibold text-xl mb-4">常用功能</div>
        <div class="grid grid-cols-3 gap-3">
            <button
                v-for="link in links"
                :key="link.label"
                type="button"
                class="flex flex-col items-center gap-2 p-3 rounded-border cursor-pointer text-color hover:bg-emphasis transition-colors"
                @click="emit('demo', link.label)"
            >
                <span class="flex items-center justify-center w-12 h-12 rounded-full" :class="link.iconBgClass">
                    <i class="text-xl!" :class="[link.icon, link.iconClass]"></i>
                </span>
                <span class="text-sm">{{ link.label }}</span>
            </button>
        </div>
    </div>
</template>
