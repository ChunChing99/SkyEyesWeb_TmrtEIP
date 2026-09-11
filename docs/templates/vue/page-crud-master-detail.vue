<template>
  <Master01>

    <Card>

      <template #content>

        <Form class="row g-3" @submit="DoFormSubmit">

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

      <div class="col-md-5 mb-1">

        <div class="card border-primary">
          <div class="card-header bg-primary text-white">
            查詢
            <Button severity="contrast" class="float-end" label="新增" icon="pi pi-file-plus" size="small" type="button" data-bs-toggle="modal" data-bs-target="#modal_InsertForm" @click="DoResetInsertForm" />
          </div>
          <div class="card-body">

            <DataTable scrollable scrollHeight="var(--table-height)"
                       v-model:selection="PageData.selectedRow" :value="PageData.Data" stripedRows selectionMode="single">
              <Column field="RowNo" header="序號"></Column>
              <Column field="Name" header="名稱"></Column>
              <Column field="update_user" header="最後修改人員"></Column>
            </DataTable>

            <Pagination :dataPagination="PageData" />

          </div>
        </div>

      </div>

      <div class="col-md-7 mb-1">

        <div class="card border-primary">
          <div class="card-header bg-primary text-white">
            檢視明細
            <Button v-if="!disabledEditForm" severity="secondary" class="float-end mx-1" label="取消" icon="pi pi-times" size="small" type="button" @click="onCancelClick" />
            <Button v-if="disabledEditForm" :disabled="PageData.selectedRow==null" severity="contrast" class="float-end mx-1" label="編輯" icon="pi pi-file-edit" size="small" type="button" @click="onEditClick" />
            <Button v-if="!disabledEditForm" severity="contrast" class="float-end mx-1" label="儲存" icon="pi pi-save" size="small" type="submit" form="EditForm" />
          </div>
          <div class="card-body">

            <Form id="EditForm" ref="refEditForm" class="row g-3" v-slot="$EditForm" @submit="DoUpdate" :resolver="!disabledEditForm ? resolverEditForm : undefined" :initialValues="dataEditForm">

              <div class="input-group mb-1">
                <span class="input-group-text">編號</span>
                <InputText class="form-control" :value="dataEditForm.Id" disabled />
              </div>

              <div class="input-group mb-1">
                <span class="input-group-text">
                  <span class="text-warning">*</span>名稱
                  <Message v-if="$EditForm.Name?.invalid && !disabledEditForm" severity="error" size="small" variant="simple">{{ $EditForm.Name.error?.message }}</Message>
                </span>
                <InputText class="form-control" name="Name" v-model="dataEditForm.Name" placeholder="請輸入名稱" :disabled="disabledEditForm" />
              </div>

              <div class="input-group mb-1">
                <span class="input-group-text">備註</span>
                <InputText class="form-control" v-model="dataEditForm.Note" placeholder="請輸入備註" :disabled="disabledEditForm" />
              </div>

            </Form>

          </div>
        </div>

      </div>

    </div>

    <div class="modal fade" id="modal_InsertForm" tabIndex="-1" data-bs-backdrop="static" aria-labelledby="modal_InsertFormLabel" aria-hidden="true" data-bs-focus="false">
      <div class="modal-dialog modal-xl">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="modal_InsertFormLabel">新增資料</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" ref="refCloseInsertForm"></button>
          </div>

          <div class="modal-body">

            <Form id="InsertForm" ref="refInsertForm" class="row g-3" v-slot="$InsertForm" :resolver="resolverInsertForm" :initialValues="dataInsertForm" @submit="DoInsert">

              <div class="input-group mb-1">
                <span class="input-group-text">
                  <span class="text-warning">*</span>名稱
                  <Message v-if="$InsertForm.Name?.invalid" severity="error" size="small" variant="simple">{{ $InsertForm.Name.error?.message }}</Message>
                </span>
                <InputText class="form-control" name="Name" v-model="dataInsertForm.Name" placeholder="請輸入名稱" />
              </div>

              <div class="input-group mb-1">
                <span class="input-group-text">備註</span>
                <InputText class="form-control" name="Note" v-model="dataInsertForm.Note" placeholder="請輸入備註" />
              </div>

            </Form>

          </div>

          <div class="modal-footer">
            <Button label="取消" severity="secondary" icon="pi pi-times" type="button" data-bs-dismiss="modal" />
            <Button label="確定" severity="contrast" icon="pi pi-check" type="submit" form="InsertForm" />
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

  const ValidateEditForm = new main.clsValidateForm({
    Name: main.Zod.string().max(50, { message: "名稱不能超過 50 個字元" }).nonempty({ message: "請輸入名稱" }),
  });

  const ValidateInsertForm = new main.clsValidateForm({
    Name: main.Zod.string().max(50, { message: "名稱不能超過 50 個字元" }).nonempty({ message: "請輸入名稱" }),
  });

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
        //#endregion

        //#region 編輯表單
        dataEditForm: {
          Id: null as number | null,
          Name: "" as string,
          Note: "" as string,
        } as any,
        disabledEditForm: true as boolean,
        resolverEditForm: ValidateEditForm.GetZodResolver(),
        //#endregion

        //#region 新增表單
        dataInsertForm: {
          Name: "" as string,
          Note: "" as string,
        } as any,
        resolverInsertForm: ValidateInsertForm.GetZodResolver(),
        //#endregion

      }
    },
    watch: {
      'PageData.selectedRow': {
        handler(newValue) {
          main.updateObjectProperties(newValue, this.dataEditForm);
          this.disabledEditForm = true;
          (this.$refs.refEditForm as any)?.reset();
        },
        deep: true
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
          body: this.PageData,
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
      DoFormSubmit() {

        this.PageData.selectedRow = null;

        Object.entries(this.dataSearchForm).forEach(([key, value]) => {
          this.PageData[key] = value;
        });

        this.PageData.SetPage(1);
      },

      /**重置新增表單 */
      DoResetInsertForm() {
        (this.$refs.refInsertForm as any)?.reset();
        Object.assign(this.dataInsertForm, {
          Name: "",
          Note: "",
        });
      },

      /** 編輯按鈕點擊事件 */
      onEditClick() {
        if (this.PageData.selectedRow == null) return false;
        this.disabledEditForm = false;
        (this.$refs.refEditForm as any)?.reset();
      },

      /** 取消按鈕點擊事件 */
      onCancelClick() {
        this.disabledEditForm = true;
        this.dataEditForm = { ...this.PageData.selectedRow };
      },

      /**
       * 更新資料
       */
      DoUpdate({ valid }: { valid: boolean }) {

        if (!valid) return ValidateEditForm.Validate(this.dataEditForm);

        new main.clsAjaxAsync({
          loadingMsg: "更新中",
          url: main.clsConfig.ApiPath + "/DoUpdate",
          body: this.dataEditForm,
          isPostModel: true,
          encrypt: true,
          async: true,
          success: (res: any) => {
            if (res.Result == true) {
              main.clsSweetMsg.alert("已更新", "完成", "success", () => {
                this.PageData.selectedRow = null;
                this.PageData.SetPage();
              });
            } else {
              main.clsSweetMsg.alert(res.ReturnMsg, res.ReturnNumber, "error");
            }
          },
        }).ajax();

      },

      /**
       * 新增資料
       */
      DoInsert({ valid }: { valid: boolean }) {

        if (!valid) return ValidateInsertForm.Validate(this.dataInsertForm);

        new main.clsAjaxAsync({
          loadingMsg: "處理中",
          url: main.clsConfig.ApiPath + "/DoInsert",
          body: this.dataInsertForm,
          isPostModel: true,
          encrypt: true,
          async: true,
          success: (res: any) => {
            if (res.Result == true) {
              main.clsSweetMsg.alert("已新增", "完成", "success", () => {
                (this.$refs.refCloseInsertForm as HTMLElement)?.click();
                this.PageData.SetPage();
              });
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
  @import './PageCrudMasterDetail.css';
</style>
