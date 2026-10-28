<script setup>
import { useAuthStore } from '../stores/auth'
const authStore = useAuthStore()
</script>

<template>
  <div class="dashboard">
    <h1>Dashboard</h1>
    <p>Bem-vindo, administrador!</p>
    <button @click="authStore.logout">Sair</button>
  </div>
</template>

<style scoped>
.dashboard { padding: 2rem; }
</style>
