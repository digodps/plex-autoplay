<script setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import AppIcon from "../components/AppIcon.vue";
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
    credentials: "include",
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

  try {
    const response = await fetch("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify({ password: password.value }),
    });

    if (response.ok) {
      authStore.login();
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

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100dvh;
  background: var(--bg-sunken, #f5f5f5);
}
.login-card {
  background: var(--surface, white);
  padding: 2rem;
  border-radius: var(--radius-lg, 8px);
  box-shadow: var(--shadow-2, 0 2px 10px rgba(0, 0, 0, 0.1));
  width: 100%;
  max-width: 400px;
}
.login-card h1 {
  margin: 0 0 0.5rem;
  color: var(--accent, #e5a00d);
}
.login-card p {
  margin: 0 0 1.5rem;
  color: var(--text-secondary, #666);
}
.alert.error {
  margin-bottom: 1rem;
}
.form-group {
  margin-bottom: 1rem;
}
.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}
.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--border, #ddd);
  border-radius: var(--radius-sm, 4px);
  box-sizing: border-box;
}
button {
  width: 100%;
  padding: 0.75rem;
  background: var(--accent, #e5a00d);
  color: var(--accent-contrast, white);
  border: none;
  border-radius: var(--radius-sm, 4px);
  font-size: 1rem;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  font-weight: 600;
}
button:hover:not(:disabled) {
  background: var(--accent-hover, #f5b21e);
}
button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.spin {
  animation: rotate 0.9s linear infinite;
}
@keyframes rotate {
  to {
    transform: rotate(360deg);
  }
}
</style>
