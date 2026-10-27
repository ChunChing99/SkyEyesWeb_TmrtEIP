<script setup lang="ts">
// 比照 src/sakai/components/dashboard/StatsWidget.vue，改為資料驅動。
import { HomeDemoService, type HomeStat } from '@/web/Home/homeDemoService';
import { onMounted, ref } from 'vue';

const stats = ref<HomeStat[]>([]);

onMounted(async () => {
    stats.value = await HomeDemoService.getStats();
});
</script>

<template>
    <div v-for="stat in stats" :key="stat.label" class="col-span-12 lg:col-span-6 xl:col-span-3">
        <div class="card mb-0">
            <div class="flex justify-between mb-4">
                <div>
                    <span class="block text-muted-color font-medium mb-4">{{ stat.label }}</span>
                    <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ stat.value }}</div>
                </div>
                <div class="flex items-center justify-center rounded-border w-10 h-10" :class="stat.iconBgClass">
                    <i class="text-xl!" :class="[stat.icon, stat.iconClass]"></i>
                </div>
            </div>
            <span class="text-primary font-medium">{{ stat.highlight }} </span>
            <span class="text-muted-color">{{ stat.note }}</span>
        </div>
    </div>
</template>
