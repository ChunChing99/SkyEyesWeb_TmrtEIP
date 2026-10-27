<script setup lang="ts">
// 首頁（DEMO）：版面比照 Sakai Dashboard（src/sakai/views/Dashboard.vue），
// 以 12 欄 grid 排列各 widget；資料全部來自 homeDemoService.ts 的假資料，不串接後端。
import HomeAnnouncementWidget from '@/components/home/HomeAnnouncementWidget.vue';
import HomePendingSignWidget from '@/components/home/HomePendingSignWidget.vue';
import HomeQuickLinksWidget from '@/components/home/HomeQuickLinksWidget.vue';
import HomeScheduleWidget from '@/components/home/HomeScheduleWidget.vue';
import HomeSignTrendWidget from '@/components/home/HomeSignTrendWidget.vue';
import HomeStatsWidget from '@/components/home/HomeStatsWidget.vue';
import HomeWelcomeWidget from '@/components/home/HomeWelcomeWidget.vue';
import { useToast } from 'primevue/usetoast';

const toast = useToast();

function showDemo(label: string) {
    toast.add({ severity: 'info', summary: label, detail: 'DEMO 版：此功能尚在開發中', life: 2500 });
}
</script>

<template>
    <div class="grid grid-cols-12 gap-8">
        <div class="col-span-12">
            <HomeWelcomeWidget @demo="showDemo" />
        </div>

        <HomeStatsWidget />

        <!-- min-w-0：避免表格等寬內容把 grid 欄位撐出畫面（手機版會出現水平捲軸） -->
        <div class="col-span-12 xl:col-span-8 min-w-0">
            <HomePendingSignWidget />
            <HomeAnnouncementWidget />
        </div>
        <div class="col-span-12 xl:col-span-4 min-w-0">
            <HomeQuickLinksWidget @demo="showDemo" />
            <HomeScheduleWidget />
            <HomeSignTrendWidget />
        </div>
    </div>
</template>
