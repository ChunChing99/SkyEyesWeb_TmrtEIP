<script setup lang="ts">
// 獨立於 AppLayout 之外的路由，需自行載入 Sakai 樣式（見 layout/AppLayout.vue 的說明）。
import '@/sakai/assets/tailwind.css';
import '@/sakai/assets/styles.scss';
import FeaturesWidget from '@/sakai/components/landing/FeaturesWidget.vue';
import FooterWidget from '@/sakai/components/landing/FooterWidget.vue';
import HeroWidget from '@/sakai/components/landing/HeroWidget.vue';
import HighlightsWidget from '@/sakai/components/landing/HighlightsWidget.vue';
import PricingWidget from '@/sakai/components/landing/PricingWidget.vue';
import TopbarWidget from '@/sakai/components/landing/TopbarWidget.vue';
</script>

<template>
    <div class="bg-surface-0 dark:bg-surface-900">
        <div id="home" class="landing-wrapper overflow-hidden">
            <div class="py-6 px-6 mx-0 md:mx-12 lg:mx-20 lg:px-20 flex items-center justify-between relative lg:static">
                <TopbarWidget />
            </div>
            <HeroWidget />
            <FeaturesWidget />
            <HighlightsWidget />
            <PricingWidget />
            <FooterWidget />
        </div>
    </div>
</template>
