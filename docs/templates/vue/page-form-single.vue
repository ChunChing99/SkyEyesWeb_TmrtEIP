<template>
  <Master01>

    <Card>

      <template #content>

        <Form id="EditForm" ref="refEditForm" class="row g-3" v-slot="$EditForm" @submit="DoUpdate" :resolver="resolverEditForm" :initialValues="dataEditForm">
          <div class="row g-3">

            <div class="col-md-6">
              <div class="input-group mb-1">
                <span class="input-group-text">
                  <span class="text-warning">*</span>設定名稱
                  <Message v-if="$EditForm.Name?.invalid" severity="error" size="small" variant="simple">{{ $EditForm.Name.error?.message }}</Message>
                </span>
                <InputText class="form-control" name="Name" v-model="dataEditForm.Name" placeholder="請輸入設定名稱" />
              </div>
            </div>

            <div class="col-md-6">
              <div class="input-group mb-1">
                <span class="input-group-text">設定值</span>
                <InputText class="form-control" name="Value" v-model="dataEditForm.Value" placeholder="請輸入設定值" />
              </div>
            </div>

            <div class="col-md-12">
              <div class="input-group mb-1">
                <span class="input-group-text">備註</span>
                <InputText class="form-control" name="Note" v-model="dataEditForm.Note" placeholder="請輸入備註" />
              </div>
            </div>

          </div>

        </Form>

      </template>

      <template #footer>
        <div style="text-align:right">
          <Button label="確定" severity="contrast" icon="pi pi-check" type="submit" form="EditForm" />
        </div>
      </template>

    </Card>

  </Master01>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import * as main from '@/main';
  import Master01 from '@/master/master01.vue';

  const ValidateEditForm = new main.clsValidateForm({
    Name: main.Zod.string().max(50, { message: "設定名稱不能超過 50 個字元" }).nonempty({ message: "請輸入設定名稱" }),
  });

  export default defineComponent({
    components: {
      Master01,
    },
    data() {
      return {
        PageData: {} as any,

        //#region 編輯表單
        dataEditForm: {
          Name: "" as string,
          Value: "" as string,
          Note: "" as string,
        } as any,
        resolverEditForm: ValidateEditForm.GetZodResolver(),
        //#endregion
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
          loadingMsg: "Loading",
          url: main.clsConfig.ApiPath + "/GetData",
          body: this.PageData,
          isPostModel: true,
          encrypt: true,
          async: true,
          success: (res: any) => {
            if (res.Result == true) {
              if (res.data != null) {
                this.dataEditForm = {
                  ...this.dataEditForm,
                  ...res.data,
                };
              }

              (this.$refs.refEditForm as any)?.reset();

            } else {
              main.clsSweetMsg.alert(res.ReturnMsg, res.ReturnNumber, "error");
            }
          },
        }).ajax();

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
                this.GetData();
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
  @import './PageFormSingle.css';
</style>
