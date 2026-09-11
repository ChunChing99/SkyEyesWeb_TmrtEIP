import AppLayout from '@/sakai/layout/AppLayout.vue';
import type { RouteRecordRaw } from 'vue-router';

// Sakai 範本路由：整合進主專案時統一掛在 /sakai 之下（見 skyeyesweb_tmrteip.client/src/router.ts）。
const sakaiRoutes: RouteRecordRaw[] = [
    {
        path: '/sakai',
        component: AppLayout,
        children: [
            {
                path: '/sakai',
                name: 'sakai-dashboard',
                component: () => import('@/sakai/views/Dashboard.vue')
            },
            {
                path: '/sakai/uikit/formlayout',
                name: 'sakai-formlayout',
                component: () => import('@/sakai/views/uikit/FormLayout.vue')
            },
            {
                path: '/sakai/uikit/input',
                name: 'sakai-input',
                component: () => import('@/sakai/views/uikit/InputDoc.vue')
            },
            {
                path: '/sakai/uikit/button',
                name: 'sakai-button',
                component: () => import('@/sakai/views/uikit/ButtonDoc.vue')
            },
            {
                path: '/sakai/uikit/table',
                name: 'sakai-table',
                component: () => import('@/sakai/views/uikit/TableDoc.vue')
            },
            {
                path: '/sakai/uikit/list',
                name: 'sakai-list',
                component: () => import('@/sakai/views/uikit/ListDoc.vue')
            },
            {
                path: '/sakai/uikit/tree',
                name: 'sakai-tree',
                component: () => import('@/sakai/views/uikit/TreeDoc.vue')
            },
            {
                path: '/sakai/uikit/panel',
                name: 'sakai-panel',
                component: () => import('@/sakai/views/uikit/PanelsDoc.vue')
            },

            {
                path: '/sakai/uikit/overlay',
                name: 'sakai-overlay',
                component: () => import('@/sakai/views/uikit/OverlayDoc.vue')
            },
            {
                path: '/sakai/uikit/media',
                name: 'sakai-media',
                component: () => import('@/sakai/views/uikit/MediaDoc.vue')
            },
            {
                path: '/sakai/uikit/message',
                name: 'sakai-message',
                component: () => import('@/sakai/views/uikit/MessagesDoc.vue')
            },
            {
                path: '/sakai/uikit/file',
                name: 'sakai-file',
                component: () => import('@/sakai/views/uikit/FileDoc.vue')
            },
            {
                path: '/sakai/uikit/menu',
                name: 'sakai-menu',
                component: () => import('@/sakai/views/uikit/MenuDoc.vue')
            },
            {
                path: '/sakai/uikit/charts',
                name: 'sakai-charts',
                component: () => import('@/sakai/views/uikit/ChartDoc.vue')
            },
            {
                path: '/sakai/uikit/misc',
                name: 'sakai-misc',
                component: () => import('@/sakai/views/uikit/MiscDoc.vue')
            },
            {
                path: '/sakai/uikit/timeline',
                name: 'sakai-timeline',
                component: () => import('@/sakai/views/uikit/TimelineDoc.vue')
            },
            {
                path: '/sakai/blocks/free',
                name: 'sakai-blocks',
                meta: {
                    breadcrumb: ['Prime Blocks', 'Free Blocks']
                },
                component: () => import('@/sakai/views/utilities/Blocks.vue')
            },
            {
                path: '/sakai/pages/empty',
                name: 'sakai-empty',
                component: () => import('@/sakai/views/pages/Empty.vue')
            },
            {
                path: '/sakai/pages/crud',
                name: 'sakai-crud',
                component: () => import('@/sakai/views/pages/Crud.vue')
            },
            {
                path: '/sakai/start/documentation',
                name: 'sakai-documentation',
                component: () => import('@/sakai/views/pages/Documentation.vue')
            }
        ]
    },
    {
        path: '/sakai/landing',
        name: 'sakai-landing',
        component: () => import('@/sakai/views/pages/Landing.vue')
    },
    {
        path: '/sakai/pages/notfound',
        name: 'sakai-notfound',
        component: () => import('@/sakai/views/pages/NotFound.vue')
    },

    {
        path: '/sakai/auth/login',
        name: 'sakai-login',
        component: () => import('@/sakai/views/pages/auth/Login.vue')
    },
    {
        path: '/sakai/auth/access',
        name: 'sakai-accessDenied',
        component: () => import('@/sakai/views/pages/auth/Access.vue')
    },
    {
        path: '/sakai/auth/error',
        name: 'sakai-error',
        component: () => import('@/sakai/views/pages/auth/Error.vue')
    }
];

export default sakaiRoutes;
