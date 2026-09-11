<template>
  <Master01>

    <Card class="mb-1">

      <template #content>

        <Form class="row g-3" @submit="GetData" :initialValues="dataSearchForm" v-slot="$SearchForm">

          <div class="col-lg-3 col-md-6">
            <div class="input-group mb-1">
              <span class="input-group-text">關鍵字</span>
              <InputText class="form-control" name="keyword" v-model="dataSearchForm.keyword" placeholder="請輸入關鍵字" />
            </div>
          </div>

          <div class="col-lg-3 col-md-6">
            <Button severity="info" label="查詢" icon="pi pi-search" type="submit" :loading="MapLoading" />
          </div>

        </Form>

      </template>

    </Card>

    <iframe id="map" src="/map.html"></iframe>

    <Card style="height: calc(40vh - 130px); overflow-y:auto;">

      <template #content>

        <DataTable :value="dataData"
                   v-model:selection="dataSelected" selectionMode="single"
                   stripedRows scrollable
                   scrollHeight="calc(100vh - 400px)">
          <Column field="Id" header="編號"></Column>
          <Column field="Name" header="名稱"></Column>
          <Column field="Status" header="狀態"></Column>
          <Column field="Update_dt" header="更新時間"></Column>
        </DataTable>

      </template>

    </Card>

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

        //#region 圖台
        MapLoading: true as boolean,
        //#endregion

        //#region 查詢表單
        dataSearchForm: {
          keyword: "" as string,
        } as any,
        //#endregion

        //#region 資料表
        dataData: [] as any,
        dataSelected: null as any,
        //#endregion

      }
    },
    watch: {
      dataSelected(newVal) {
        if (newVal) {
          let MapRoot = this.oMapRoot;
          MapRoot.map.closeInfoWindow();
          this.DoOpenInfoWindow(newVal);
        }
      }
    },
    mounted() {
      this.MapInit();
    },
    methods: {
      oMapRoot: null as any,
      Timer_DrawMarker: null as any,

      /**地圖初始化 */
      MapInit() {

        const mapElement = document.getElementById("map") as HTMLIFrameElement;

        if (mapElement) {
          mapElement.onload = () => {
            let m = mapElement.contentWindow as any;
            if (m && typeof m.map != undefined) {
              this.oMapRoot = m;
            }

            let MapRoot = this.oMapRoot;
            MapRoot.map.ZoomXY(121, 23.5, 8);
            this.MapLoading = false;
          };
        }

      },

      /**
       * 取得資料
       */
      GetData() {

        let MapRoot = this.oMapRoot;
        const self = this;

        new main.clsAjaxAsync({
          loadingMsg: "Loading",
          url: main.clsConfig.ApiPath + "/GetData",
          body: this.dataSearchForm,
          isPostModel: true,
          encrypt: true,
          async: true,
          success: (res: any) => {
            if (res.Result == true) {
              this.dataData = res.data ?? [];

              let zoomX = 0;
              let zoomY = 0;

              this.dataData.forEach((item: any) => {
                let img = document.createElement('img');
                img.src = "/images/marker-icon.png";
                img.title = item.Name;
                img.style.width = "32px";

                zoomX += item.WGS_X;
                zoomY += item.WGS_Y;

                MapRoot.map.CreateMarker({
                  id: "Item_" + item.Id,
                  group: "Item",
                  x: item.WGS_X,
                  y: item.WGS_Y,
                  marker: img,
                  zindex: 10,
                  data: item,
                  click: function () {
                    self.DoOpenInfoWindow(item);
                  }
                });
              });

              if (this.dataData.length > 0) {
                MapRoot.map.ZoomXY(zoomX / this.dataData.length, zoomY / this.dataData.length, 12);
              }

            } else {
              this.dataData = [];
              main.clsSweetMsg.alert(res.ReturnMsg, res.ReturnNumber, "error");
            }
          },
        }).ajax();

      },

      /** 開啟訊息視窗 */
      DoOpenInfoWindow(item: any) {
        let MapRoot = this.oMapRoot;
        MapRoot.map.closeInfoWindow();

        let htmlMsg = `
        <div style="min-width: 200px;">
          <div style="margin-bottom: 4px;">編號：${item.Id}</div>
          <div style="margin-bottom: 4px;">名稱：${item.Name}</div>
          <div style="margin-bottom: 4px;">狀態：${item.Status}</div>
          <div style="margin-bottom: 4px;">座標：${item.WGS_X}, ${item.WGS_Y}</div>
        </div>
        `;

        MapRoot.map.openInfoWindow(
          new MapRoot.dgXY(item.WGS_X, item.WGS_Y),
          htmlMsg
        );
        MapRoot.map.ZoomXY(item.WGS_X, item.WGS_Y, 15);
      },

    },
  });
</script>

<style scoped>
  @import './PageMapBasic.css';
</style>
