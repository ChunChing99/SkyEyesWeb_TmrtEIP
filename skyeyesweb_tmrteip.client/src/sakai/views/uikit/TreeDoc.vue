<script setup lang="ts">
import { NodeService } from '@/sakai/service/NodeService';
import type { TreeSelectionKeys } from 'primevue/tree';
import type { TreeNode } from 'primevue/treenode';
import type { TreeTableSelectionKeys } from 'primevue/treetable';
import { onMounted, ref } from 'vue';

const treeValue = ref<TreeNode[]>([]);
const selectedTreeValue = ref<TreeSelectionKeys | undefined>(undefined);
const treeTableValue = ref<TreeNode[]>([]);
const selectedTreeTableValue = ref<TreeTableSelectionKeys | undefined>(undefined);

onMounted(() => {
    NodeService.getTreeNodes().then((data) => (treeValue.value = data));
    NodeService.getTreeTableNodes().then((data) => (treeTableValue.value = data));
});
</script>

<template>
    <div class="card">
        <div class="font-semibold text-xl">Tree</div>
        <Tree :value="treeValue" selectionMode="checkbox" v-model:selectionKeys="selectedTreeValue"></Tree>
    </div>

    <div class="card">
        <div class="font-semibold text-xl mb-4">TreeTable</div>
        <TreeTable :value="treeTableValue" selectionMode="checkbox" v-model:selectionKeys="selectedTreeTableValue">
            <Column field="name" header="Name" :expander="true"></Column>
            <Column field="size" header="Size"></Column>
            <Column field="type" header="Type"></Column>
        </TreeTable>
    </div>
</template>
