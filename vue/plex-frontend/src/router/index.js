import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import SyncView from '../views/SyncView.vue'

const routes = [
  {
    path: '/',
    name: 'login',
    component: LoginView
  },
  {
    path: '/login',
    name: 'login',
    component: LoginView
  },
  {
    path: '/dashboard',
    name: 'dashboard',
    component: () => import('../views/DashboardView.vue')
  },
  {
    path: '/libraries',
    name: 'libraries',
    component: () => import('../views/LibrariesView.vue')
  },
  {
    path: '/markers',
    name: 'markers',
    component: () => import('../views/MarkersView.vue')
  },
  {
    path: '/sync',
    name: 'sync',
    component: SyncView
  },
  {
    path: '/settings',
    name: 'settings',
    component: () => import('../views/SettingsView.vue')
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
