<script setup lang="ts">
// 最新公告。DEMO 版：點擊公告開啟內容視窗並標記為已讀（僅前端狀態）。
import { HomeDemoService, type Announcement } from '@/web/Home/homeDemoService';
import { computed, onMounted, ref } from 'vue';

const announcements = ref<Announcement[]>([]);
const category = ref('全部');
const selected = ref<Announcement | null>(null);
const dialogVisible = ref(false);

const categories = ['全部', '公告', '營運', '行政', '資訊', '人事'];

const categorySeverity: Record<Announcement['category'], string> = {
    公告: 'danger',
    營運: 'info',
    行政: 'secondary',
    資訊: 'contrast',
    人事: 'success'
};

const filtered = computed(() => {
    const list = category.value === '全部' ? announcements.value : announcements.value.filter((a) => a.category === category.value);
    // 置頂優先，其餘依發布日期新到舊
    return [...list].sort((a, b) => Number(b.isTop) - Number(a.isTop) || b.publishDate.localeCompare(a.publishDate));
});

function open(item: Announcement) {
    item.isNew = false;
    selected.value = item;
    dialogVisible.value = true;
}

onMounted(async () => {
    announcements.value = await HomeDemoService.getAnnouncements();
});
</script>

<template>
    <div class="card">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-4">
            <div class="font-semibold text-xl">最新公告</div>
            <SelectButton v-model="category" :options="categories" :allowEmpty="false" size="small" />
        </div>
        <ul class="list-none p-0 m-0">
            <li
                v-for="item in filtered"
                :key="item.id"
                class="flex items-center gap-3 py-3 px-2 border-b border-surface last:border-b-0 cursor-pointer rounded-border hover:bg-emphasis transition-colors"
                @click="open(item)"
            >
                <Tag :value="item.category" :severity="categorySeverity[item.category]" class="shrink-0" />
                <div class="flex-1 min-w-0">
                    <div class="truncate text-surface-900 dark:text-surface-0" :class="{ 'font-semibold': item.isNew }">
                        <i v-if="item.isTop" class="pi pi-thumbtack text-red-500 mr-1" title="置頂"></i>
                        {{ item.title }}
                    </div>
                    <div class="text-muted-color text-sm">{{ item.dept }} · {{ item.publishDate }}</div>
                </div>
                <Badge v-if="item.isNew" value="NEW" severity="danger" class="shrink-0" />
            </li>
            <li v-if="filtered.length === 0" class="text-center text-muted-color py-6">此分類目前沒有公告</li>
        </ul>

        <Dialog v-model:visible="dialogVisible" :header="selected?.title" modal :style="{ width: '40rem' }" :breakpoints="{ '575px': '95vw' }">
            <div v-if="selected">
                <div class="flex items-center gap-2 mb-4 text-muted-color">
                    <Tag :value="selected.category" :severity="categorySeverity[selected.category]" />
                    <span>{{ selected.dept }}</span>
                    <span>·</span>
                    <span>{{ selected.publishDate }}</span>
                </div>
                <p class="leading-relaxed m-0">（DEMO 公告內容）各位同仁好，{{ selected.title }}。詳細內容請參閱附件，如有疑問請洽{{ selected.dept }}承辦人員，謝謝配合。</p>
            </div>
        </Dialog>
    </div>
</template>
