<template>
  <Master01>

    <Card>

      <template #content>

        <Form class="row g-3" @submit="DoFormSubmit" :initialValues="dataSearchForm" :resolver="resolverSearchForm" v-slot="$SearchForm">

          <div class="col-lg-3 col-md-6">
            <div class="input-group mb-1">
              <span class="input-group-text">關鍵字</span>
              <InputText class="form-control" name="keyword" v-model="dataSearchForm.keyword" placeholder="請輸入關鍵字" />
            </div>
          </div>

          <div class="col-lg-3 col-md-6">
            <button type="submit" class="btn btn-primary"><i class="bi bi-search"></i> 搜尋</button>
          </div>

        </Form>

      </template>

    </Card>

    <div class="row mt-3">

      <div class="col-md-12 mb-1">

        <div class="card border-primary">
          <div class="card-header bg-primary text-white">
            查詢結果
          </div>
          <div class="card-body">

            <DataTable scrollable scrollHeight="var(--table-height)"
                       v-model:selection="PageData.selectedRow" :value="PageData.Data" stripedRows selectionMode="single">
              <Column field="RowNo" header="序號"></Column>
              <Column field="Name" header="名稱"></Column>
              <Column field="Create_dt" header="建立時間"></Column>
            </DataTable>

            <Pagination :dataPagination="PageData" />

          </div>
        </div>

      </div>

    </div>

  </Master01>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import * as main from '@/main';
  import Master01 from '@/master/master01.vue';
  import Pagination from '@/components/componentPagination.vue';

  const ValidateSearchForm = new main.clsValidateForm(null);

  export default defineComponent({
    components: {
      Master01,
      Pagination,
    },
    data() {
      return {
        PageData: new main.clsPaginationPage() as any,

        //#region 查詢表單
        dataSearchForm: {
          keyword: "" as string,
        } as any,
        resolverSearchForm: ValidateSearchForm.GetZodResolver(),
        //#endregion
      }
    },
    created() {
      this.PageData.CallBack = this.GetData;
      this.PageData.SetPage(1);
    },
    methods: {

      /**
       * 取得資料
       */
      GetData() {

        new main.clsAjaxAsync({
          loadingMsg: "Loading",
          url: main.clsConfig.ApiPath + "/GetData",
          body: {
            ...this.dataSearchForm,
            PageRowCount: this.PageData.PageRowCount,
            NowPageNum: this.PageData.NowPageNum,
          },
          isPostModel: true,
          encrypt: true,
          async: true,
          success: (res: any) => {
            if (res.Result == true) {
              this.PageData.Data = res.Data;
              this.PageData.DataCount = res.DataCount;
              this.PageData.PageCount = res.PageCount;
              this.PageData.GetPageItem();
            } else {
              this.PageData.Data = [];
              main.clsSweetMsg.alert(res.ReturnMsg, res.ReturnNumber, "error");
            }
          },
        }).ajax();

      },

      /**查詢表單送出 */
      DoFormSubmit({ valid }: { valid: boolean }) {

        if (!valid) return ValidateSearchForm.Validate(this.dataSearchForm);

        this.PageData.selectedRow = null;

        Object.entries(this.dataSearchForm).forEach(([key, value]) => {
          this.PageData[key] = value;
        });

        this.PageData.SetPage(1);
      },

    },
  });
</script>

<style scoped>
  @import './PageListBasic.css';
</style>
