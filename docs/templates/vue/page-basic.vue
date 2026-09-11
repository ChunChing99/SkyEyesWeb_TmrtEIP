<template>
  <Master01>



  </Master01>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import * as main from '@/main';
  import Master01 from '@/master/master01.vue';

  export default defineComponent({
    components: {
      Master01,
    },
    data() {
      return {
        PageData: new main.clsPaginationPage() as any,
      }
    },
    computed: {
    },
    watch: {
    },
    beforeCreate() {
    },
    created() {
    },
    beforeMount() {
    },
    mounted() {
    },
    updated() {
    },
    activated() {
    },
    deactivated() {
    },
    beforeUnmount() {
    },
    unmounted() {
    },
    methods: {
    },
  });
</script>

<style scoped>
  @import './PageBasic.css';
</style>
