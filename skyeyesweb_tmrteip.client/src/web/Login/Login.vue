<script setup lang="ts">
// TODO: 登入表單尚未實作。
// 後端 LoginController.DoSignIn 目前也因 SYS_tbUser/SYS_ttLoginLog 尚未建立而未實作，
// 待資料表就緒、且此頁登入邏輯（呼叫 DoSignIn、寫入 localStorage token、呼叫 main.ts 的 CmpSignStatus）補上後即可串接。
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-slate-50 p-8 dark:bg-slate-900">
    <Card class="w-full max-w-sm">
      <template #title>SkyEyesWeb_TmrtEIP</template>
      <template #content>
        <p class="text-surface-600 dark:text-surface-300">登入頁面尚未實作（待後端使用者資料表建立）。</p>
      </template>
      <template #footer>
        <router-link to="/home">
          <Button label="進入首頁（DEMO）" icon="pi pi-home" class="w-full" />
        </router-link>
      </template>
    </Card>
  </div>
</template>
