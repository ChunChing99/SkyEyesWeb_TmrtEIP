<script setup lang="ts">
import { HomeDemoService, type ScheduleItem } from '@/web/Home/homeDemoService';
import { onMounted, ref } from 'vue';

const schedule = ref<ScheduleItem[]>([]);

onMounted(async () => {
    schedule.value = await HomeDemoService.getTodaySchedule();
});
</script>

<template>
    <div class="card">
        <div class="font-semibold text-xl mb-4">今日行程</div>
        <Timeline :value="schedule" class="customized-timeline">
            <template #opposite="{ item }">
                <span class="text-muted-color font-medium">{{ item.time }}</span>
            </template>
            <template #marker="{ item }">
                <span class="flex w-8 h-8 items-center justify-center text-white rounded-full z-10 shadow-sm" :style="{ backgroundColor: item.color }">
                    <i :class="item.icon" class="text-sm!"></i>
                </span>
            </template>
            <template #content="{ item }">
                <div class="mb-4">
                    <div class="font-medium text-surface-900 dark:text-surface-0">{{ item.title }}</div>
                    <div class="text-muted-color text-sm"><i class="pi pi-map-marker text-xs! mr-1"></i>{{ item.location }}</div>
                </div>
            </template>
        </Timeline>
    </div>
</template>

<style scoped>
/* 讓時間欄位不要佔一半寬度 */
:deep(.p-timeline-event-opposite) {
    flex: 0 0 3.5rem;
}
</style>
