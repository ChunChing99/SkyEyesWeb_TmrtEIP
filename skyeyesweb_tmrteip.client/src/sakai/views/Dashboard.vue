<script setup lang="ts">
import BestSellingWidget from '@/sakai/components/dashboard/BestSellingWidget.vue';
import NotificationsWidget from '@/sakai/components/dashboard/NotificationsWidget.vue';
import RecentSalesWidget from '@/sakai/components/dashboard/RecentSalesWidget.vue';
import RevenueStreamWidget from '@/sakai/components/dashboard/RevenueStreamWidget.vue';
import StatsWidget from '@/sakai/components/dashboard/StatsWidget.vue';
</script>

<template>
    <div class="grid grid-cols-12 gap-8">
        <StatsWidget />

        <div class="col-span-12 xl:col-span-6">
            <RecentSalesWidget />
            <BestSellingWidget />
        </div>
        <div class="col-span-12 xl:col-span-6">
            <RevenueStreamWidget />
            <NotificationsWidget />
        </div>
    </div>
</template>
