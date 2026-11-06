<script setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import AppIcon from "../components/AppIcon.vue";
import { deleteCookie, AUTH_COOKIE } from "../api";
import { useAuthStore } from "../stores/auth";

const authStore = useAuthStore();
const route = useRoute();
const router = useRouter();
const password = ref("");
const error = ref("");
const loading = ref(false);

const getRedirect = () =>
  typeof route.query.redirect === "string" ? route.query.redirect : "/dashboard";

onMounted(async () => {
  if (!authStore.isAuthenticated) return;

  const response = await fetch("/api/status", {
    headers: { Authorization: `Bearer ${authStore.token}` },
  });
  if (response.ok) {
    await router.replace(getRedirect());
    return;
  }
  authStore.logout();
});

const handleLogin = async () => {
  error.value = "";
  loading.value = true;
  deleteCookie(AUTH_COOKIE);

  try {
    const response = await fetch("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ password: password.value }),
    });

    if (response.ok) {
      const data = await response.json();
      authStore.login(data.token);
      await router.push(getRedirect());
    } else {
      error.value = "Senha inválida";
    }
  } catch {
    error.value = "Erro ao conectar com o servidor";
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="login-container">
    <div class="login-card">
      <h1>Plex Autoplay</h1>
      <p>Painel Administrativo</p>

      <div v-if="error" class="alert error" role="alert">
        <AppIcon name="warning" size="sm" />
        {{ error }}
      </div>

      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label for="password">Senha</label>
          <input id="password" v-model="password" type="password" placeholder="Digite a senha" />
        </div>

        <button :disabled="loading" type="submit">
          <AppIcon v-if="!loading" name="check" size="sm" />
          <AppIcon v-else name="spinner" size="sm" class="spin" />
          {{ loading ? "Entrando..." : "Entrar" }}
        </button>
      </form>
    </div>
  </div>
</template>

