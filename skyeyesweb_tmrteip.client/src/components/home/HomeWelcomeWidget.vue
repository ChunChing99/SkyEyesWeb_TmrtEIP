<script setup lang="ts">
import { HomeDemoService, type HomeUser } from '@/web/Home/homeDemoService';
import { computed, onMounted, ref } from 'vue';

const emit = defineEmits<{ (e: 'demo', label: string): void }>();

const user = ref<HomeUser | null>(null);
const now = new Date();

const greeting = computed(() => {
    const hour = now.getHours();
    if (hour < 12) return '早安';
    if (hour < 18) return '午安';
    return '晚安';
});

const weekDay = ['日', '一', '二', '三', '四', '五', '六'][now.getDay()];
const today = `${now.format('yyyy 年 M 月 d 日')}（星期${weekDay}）`;

onMounted(async () => {
    user.value = await HomeDemoService.getUser();
});
</script>

<template>
    <div class="card mb-0 flex flex-col md:flex-row md:items-center justify-between gap-6 bg-linear-to-r from-primary-50 to-surface-0 dark:from-primary-400/10 dark:to-surface-900">
        <div class="flex items-center gap-4">
            <Avatar :label="user?.name.charAt(0)" size="xlarge" shape="circle" class="bg-primary! text-primary-contrast! shrink-0" />
            <div>
                <div class="text-2xl font-semibold text-surface-900 dark:text-surface-0 mb-1">{{ greeting }}，{{ user?.name }}</div>
                <div class="text-muted-color">{{ user?.dept }} · {{ user?.position }}</div>
                <div class="text-muted-color mt-1"><i class="pi pi-calendar mr-2"></i>{{ today }}</div>
            </div>
        </div>
        <div class="flex flex-wrap gap-2">
            <Button label="新增申請" icon="pi pi-plus" @click="emit('demo', '新增申請')" />
            <Button label="待我簽核" icon="pi pi-pen-to-square" severity="secondary" outlined @click="emit('demo', '待我簽核')" />
        </div>
    </div>
</template>
