<script setup lang="ts">
// 近 7 日簽核統計。圖表配色寫法比照 src/sakai/components/dashboard/RevenueStreamWidget.vue，
// 深色模式或主題色切換時重新取 CSS 變數。
import { useLayout } from '@/sakai/layout/composables/layout';
import { HomeDemoService, type SignTrend } from '@/web/Home/homeDemoService';
import { onMounted, ref, watch } from 'vue';

const { layoutConfig, isDarkTheme } = useLayout();

const trend = ref<SignTrend | null>(null);
const chartData = ref<Record<string, unknown> | null>(null);
const chartOptions = ref<Record<string, unknown> | null>(null);

function setChartData(data: SignTrend) {
    const documentStyle = getComputedStyle(document.documentElement);

    return {
        labels: data.labels,
        datasets: [
            {
                type: 'bar',
                label: '送出件數',
                backgroundColor: documentStyle.getPropertyValue('--p-primary-300'),
                data: data.submitted,
                borderRadius: 6,
                barThickness: 14
            },
            {
                type: 'bar',
                label: '完成簽核',
                backgroundColor: documentStyle.getPropertyValue('--p-primary-500'),
                data: data.approved,
                borderRadius: 6,
                barThickness: 14
            }
        ]
    };
}

function setChartOptions() {
    const documentStyle = getComputedStyle(document.documentElement);
    const borderColor = documentStyle.getPropertyValue('--surface-border');
    const textColor = documentStyle.getPropertyValue('--text-color');
    const textMutedColor = documentStyle.getPropertyValue('--text-color-secondary');

    return {
        maintainAspectRatio: false,
        plugins: {
            legend: { labels: { color: textColor, usePointStyle: true } }
        },
        scales: {
            x: {
                ticks: { color: textMutedColor },
                grid: { color: 'transparent' }
            },
            y: {
                beginAtZero: true,
                ticks: { color: textMutedColor, precision: 0 },
                grid: { color: borderColor, drawTicks: false }
            }
        }
    };
}

function refresh() {
    if (!trend.value) return;
    chartData.value = setChartData(trend.value);
    chartOptions.value = setChartOptions();
}

watch([() => layoutConfig.primary, () => layoutConfig.surface, isDarkTheme], refresh);

onMounted(async () => {
    trend.value = await HomeDemoService.getSignTrend();
    refresh();
});
</script>

<template>
    <div class="card">
        <div class="font-semibold text-xl mb-4">近 7 日簽核統計</div>
        <Chart type="bar" :data="chartData ?? undefined" :options="chartOptions ?? undefined" class="h-72" />
    </div>
</template>
