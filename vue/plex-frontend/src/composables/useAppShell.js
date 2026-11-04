import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";

export function useAppShell() {
  const authStore = useAuthStore();
  const route = useRoute();
  const router = useRouter();

  const isLoginRoute = computed(() => route.name === "login");

  const activePath = computed(() => route.path);

  const navItems = [
    { to: "/dashboard", label: "Início", icon: "home" },
    { to: "/libraries", label: "Bibliotecas", icon: "library" },
    { to: "/sync", label: "Sincronizar", icon: "sync" },
    { to: "/settings", label: "Ajustes", icon: "settings" },
  ];

  const navigate = (to) => {
    if (to !== route.path) {
      router.push(to);
    }
  };

  const handleLogout = () => {
    authStore.logout();
    router.push({ name: "login" });
  };

  return {
    isLoginRoute,
    navItems,
    activePath,
    navigate,
    handleLogout,
  };
}
