<script setup>
import { useAuthStore } from './stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()
</script>

<template>
  <router-view v-slot="{ Component }">
    <transition name="fade" mode="out-in">
      <component v-if="authStore.isAuthenticated || $route.name === 'login'" :is="Component" />
      <template v-else>
        <div style="text-align:center; padding: 2rem;">
          <p>Redirecionando para login...</p>
        </div>
      </template>
    </transition>
  </router-view>
</template>

<style>
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>
