<script setup lang="ts">
// 待我簽核清單。DEMO 版：核准／退回只在前端移除該筆並顯示提示，不呼叫後端。
import { HomeDemoService, type PendingSignItem, type SignUrgency } from '@/web/Home/homeDemoService';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';

const toast = useToast();

const items = ref<PendingSignItem[]>([]);
const selected = ref<PendingSignItem | null>(null);
const dialogVisible = ref(false);
const comment = ref('');

const urgencySeverity: Record<SignUrgency, string> = {
    一般: 'secondary',
    急件: 'warn',
    特急: 'danger'
};

function openSign(item: PendingSignItem) {
    selected.value = item;
    comment.value = '';
    dialogVisible.value = true;
}

function doSign(approve: boolean) {
    const item = selected.value;
    if (!item) return;

    items.value = items.value.filter((x) => x.formNo !== item.formNo);
    dialogVisible.value = false;
    toast.add({
        severity: approve ? 'success' : 'warn',
        summary: approve ? '已核准' : '已退回',
        detail: `${item.formName}（${item.formNo}）`,
        life: 3000
    });
}

onMounted(async () => {
    items.value = await HomeDemoService.getPendingSigns();
});
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-4">
            <div class="font-semibold text-xl">待我簽核 <Badge :value="items.length" severity="warn" class="ml-2" /></div>
            <Button label="查看全部" icon="pi pi-arrow-right" iconPos="right" text size="small" />
        </div>
        <DataTable :value="items" dataKey="formNo" :rows="5" paginator scrollable size="small">
            <template #empty>
                <div class="text-center text-muted-color py-6"><i class="pi pi-check-circle mr-2"></i>目前沒有待簽核的表單</div>
            </template>
            <Column field="formName" header="表單" style="min-width: 12rem">
                <template #body="{ data }">
                    <div class="font-medium text-surface-900 dark:text-surface-0">{{ data.formName }}</div>
                    <div class="text-muted-color text-sm">{{ data.formNo }}</div>
                </template>
            </Column>
            <Column header="申請人" style="min-width: 8rem">
                <template #body="{ data }">
                    <div>{{ data.applicant }}</div>
                    <div class="text-muted-color text-sm">{{ data.dept }}</div>
                </template>
            </Column>
            <Column field="nodeName" header="目前關卡" style="min-width: 7rem" />
            <Column field="submitTime" header="送出時間" style="min-width: 9rem" />
            <Column header="緩急" style="min-width: 5rem">
                <template #body="{ data }">
                    <Tag :value="data.urgency" :severity="urgencySeverity[data.urgency as SignUrgency]" />
                </template>
            </Column>
            <Column style="width: 6rem">
                <template #body="{ data }">
                    <Button label="簽核" icon="pi pi-pen-to-square" size="small" outlined @click="openSign(data)" />
                </template>
            </Column>
        </DataTable>

        <Dialog v-model:visible="dialogVisible" header="簽核" modal :style="{ width: '32rem' }" :breakpoints="{ '575px': '95vw' }">
            <div v-if="selected" class="flex flex-col gap-4">
                <div class="grid grid-cols-3 gap-y-2 gap-x-4">
                    <span class="text-muted-color">單據編號</span>
                    <span class="col-span-2">{{ selected.formNo }}</span>
                    <span class="text-muted-color">表單</span>
                    <span class="col-span-2">{{ selected.formName }}</span>
                    <span class="text-muted-color">申請人</span>
                    <span class="col-span-2">{{ selected.applicant }}（{{ selected.dept }}）</span>
                    <span class="text-muted-color">目前關卡</span>
                    <span class="col-span-2">{{ selected.nodeName }}</span>
                    <span class="text-muted-color">緩急</span>
                    <span class="col-span-2"><Tag :value="selected.urgency" :severity="urgencySeverity[selected.urgency]" /></span>
                </div>
                <div class="flex flex-col gap-2">
                    <label for="signComment" class="font-medium">簽核意見</label>
                    <Textarea id="signComment" v-model="comment" rows="3" autoResize placeholder="（選填）" />
                </div>
            </div>
            <template #footer>
                <Button label="退回" icon="pi pi-replay" severity="danger" outlined @click="doSign(false)" />
                <Button label="核准" icon="pi pi-check" @click="doSign(true)" />
            </template>
        </Dialog>
    </div>
</template>
