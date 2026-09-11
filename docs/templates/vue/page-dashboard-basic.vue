<template>
  <Master01>

    <div class="row g-3">

      <div class="col-lg-3 col-md-6">
        <Card>
          <template #content>
            <div class="text-muted">
              <div>今日總數</div>
              <div class="fs-1">{{ PageData.summary.total }}</div>
            </div>
          </template>
        </Card>
      </div>

      <div class="col-lg-3 col-md-6">
        <Card>
          <template #content>
            <div class="text-muted">
              <div>進行中</div>
              <div class="fs-1">{{ PageData.summary.processing }}</div>
            </div>
          </template>
        </Card>
      </div>

    </div>

    <div class="row g-3 mt-2">

      <div class="col-lg-6 col-md-12">
        <Card>
          <template #title>
            圓餅圖
          </template>
          <template #content>
            <div class="Highcharts-box" id="charts01"></div>
          </template>
        </Card>
      </div>

      <div class="col-lg-6 col-md-12">
        <Card>
          <template #title>
            長條圖
          </template>
          <template #content>
            <div class="Highcharts-box" id="charts02"></div>
          </template>
        </Card>
      </div>

    </div>

  </Master01>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import * as main from '@/main';
  import Master01 from '@/master/master01.vue';
  import * as Highcharts from 'highcharts';

  export default defineComponent({
    components: {
      Master01,
    },
    data() {
      return {
        PageData: {
          summary: {
            total: 0,
            processing: 0,
          } as any,
          chartData: [] as any,
        } as any,
      }
    },
    created() {
      this.GetData();
    },
    methods: {

      /**
       * 取得資料
       */
      GetData() {

        new main.clsAjaxAsync({
          loadingMsg: null,
          url: main.clsConfig.ApiPath + "/GetData",
          body: {},
          isPostModel: true,
          encrypt: true,
          async: true,
          success: (res: any) => {
            if (res.Result == true) {

              this.PageData.summary = res.summary ?? this.PageData.summary;
              this.PageData.chartData = res.chartData ?? [];

              Highcharts.chart('charts01', {
                exporting: { enabled: false },
                credits: { enabled: false },
                title: { text: null as any },
                series: [
                  {
                    type: 'pie',
                    data: this.PageData.chartData,
                  } as any
                ]
              } as any);

              Highcharts.chart('charts02', {
                exporting: { enabled: false },
                credits: { enabled: false },
                chart: { type: 'column' },
                title: { text: null as any },
                xAxis: {
                  categories: this.PageData.chartData.map((item: any) => item.name),
                },
                series: [
                  {
                    name: '數量',
                    data: this.PageData.chartData.map((item: any) => item.y),
                    type: 'column'
                  } as any
                ]
              } as any);

            } else {
              main.clsSweetMsg.alert(res.ReturnMsg, res.ReturnNumber, "error");
            }
          },
        }).ajax();

      },

    },
  });
</script>

<style scoped>
  @import './PageDashboardBasic.css';
</style>
