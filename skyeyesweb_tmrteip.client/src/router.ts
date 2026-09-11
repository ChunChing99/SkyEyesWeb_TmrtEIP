// src/router.ts
import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import * as main from './main'
import sakaiRoutes from './sakai/router'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'login',
    component: () => import('@/web/Login/Login.vue'),
  },
  // Sakai 前端範本/模版（僅供參考、開發時瀏覽用），見 docs/client/README.md
  ...sakaiRoutes,
  // 其他路由配置（依 Menu 動態註冊，見下方 fetchDynamicRoutes）
  {
    path: '/:pathMatch(.*)*',
    redirect: '/',
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// 從 API 獲取動態路由資料並添加到路由配置中
async function fetchDynamicRoutes() {
  try {
    const response = await fetch(`${import.meta.env.BASE_URL}api/Base/GetRouterData`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: localStorage['token'],
      },
      body: JSON.stringify({ PostData: '' }),
    })
    const res = await response.json()

    if (res.Result) {
      const componentModules = import.meta.glob('@/web/**/*.vue')
      const aliasComponentModules: Record<string, () => Promise<unknown>> = {}
      for (const key in componentModules) {
        let arrKey = key.split('/')
        //刪除陣列中有 'src' 和 'web' 的元素
        arrKey.splice(0, 3)
        //刪除最後一個元素
        arrKey.pop()
        //將陣列轉換成字串
        const aliasKey = arrKey.join('_')
        const moduleLoader = componentModules[key]
        if (moduleLoader) aliasComponentModules[aliasKey] = moduleLoader
      }

      const addMenuRoute = (path: string, auth: boolean, title: string) => {
        const mName: string = path.split('/').slice(1, 3).join('_')
        const component = aliasComponentModules[mName]

        if (!component) {
          console.warn(`找不到選單對應的頁面元件：${mName}（路徑 ${path}）`)
          return
        }

        router.addRoute({
          path,
          name: mName,
          component,
          meta: {
            requiresAuth: auth,
            title,
          },
        })
      }

      res.Menu.forEach((mItem: any) => {
        if (mItem.Type == 'app') {
          addMenuRoute(mItem.Path, mItem.Auth, mItem.Name)
        } else {
          mItem.subItems.forEach((sItem: any) => {
            addMenuRoute(sItem.Path, sItem.Auth, sItem.Name)
          })
        }
      })

      // 獲取當前的 URL 路徑並重新導頁
      const currentPath = window.location.pathname
      router.push({ path: currentPath })
    } else {
      console.log(res)
    }
  } catch (error) {
    console.error('Error fetching dynamic routes:', error)
  }
}

// 在應用啟動時獲取動態路由
export async function initializeRouter() {
  await fetchDynamicRoutes()
}

// 動態設定標題
router.beforeEach((to, from, next) => {
  const title = (to.meta.title as string) || ''
  main.clsConfig.setTitle(title)
  next()
})

export default router
