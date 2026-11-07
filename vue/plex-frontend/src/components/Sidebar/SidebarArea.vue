<script setup>
import { useRouter } from "vue-router";
import { useSidebarStore } from "@/stores/sidebar";
import { useThemeStore } from "@/stores/theme";
import { useAuthStore } from "@/stores/auth";
import AppIcon from "@/components/AppIcon.vue";
import SidebarItem from "./SidebarItem.vue";

const menuGroups = [
  {
    name: "MENU",
    menuItems: [
      { label: "Início", to: "/dashboard", icon: "home" },
      {
        label: "Bibliotecas",
        to: "/libraries",
        icon: "library",
        match: ["/series"],
      },
      { label: "Sincronização", to: "/sync", icon: "sync" },
      { label: "Configurações", to: "/settings", icon: "settings" },
    ],
  },
];

const sidebar = useSidebarStore();
const theme = useThemeStore();
const auth = useAuthStore();
const router = useRouter();

const actionClasses =
  "flex w-full items-center gap-2 rounded-lg p-2 text-body hover:text-primary hover:bg-stroke dark:hover:bg-strokedark focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900";

function handleLogout() {
  auth.logout();
  router.push({ name: "login" });
}
</script>

<template>
  <div
    class="sidebar flex h-screen flex-col bg-gray-50 text-body text-sm border-r dark:border-gray-800 dark:bg-gray-900"
    :class="sidebar.isCollapsed ? 'w-16' : 'w-64'"
  >
    <div class="flex flex-col gap-1 p-2">
      <button
        :class="actionClasses"
        type="button"
        :aria-label="sidebar.isCollapsed ? 'Expandir menu' : 'Recolher menu'"
        :aria-expanded="!sidebar.isCollapsed"
        :title="sidebar.isCollapsed ? 'Expandir menu' : 'Recolher menu'"
        @click="sidebar.toggleCollapsed"
      >
        <AppIcon name="menu" size="sm" />
        <span v-if="!sidebar.isCollapsed" class="font-medium">Menu</span>
      </button>

      <button
        :class="actionClasses"
        type="button"
        :aria-label="theme.mode === 'dark' ? 'Tema claro' : 'Tema escuro'"
        :title="theme.mode === 'dark' ? 'Tema claro' : 'Tema escuro'"
        @click="theme.toggle"
      >
        <AppIcon :name="theme.mode === 'dark' ? 'sun' : 'moon'" size="sm" />
        <span v-if="!sidebar.isCollapsed">Tema</span>
      </button>
    </div>

    <ul class="flex-1 flex flex-col p-2 pt-2 overflow-y-auto space-y-1">
      <li v-for="group in menuGroups" :key="group.name" class="flex flex-col space-y-1">
        <span
          v-if="!sidebar.isCollapsed"
          class="text-xs text-bodydark font-medium uppercase tracking-widest capitalize"
        >{{ group.name }}</span>
        <div class="flex flex-col space-y-1">
          <SidebarItem
            v-for="item in group.menuItems"
            :key="item.to"
            :to="item.to"
            :icon="item.icon"
            :label="item.label"
            :collapsed="sidebar.isCollapsed"
          />
        </div>
      </li>
    </ul>

    <div class="p-2 border-t dark:border-gray-800">
      <button :class="actionClasses" type="button" aria-label="Sair" title="Sair" @click="handleLogout">
        <AppIcon name="logout" size="sm" />
        <span v-if="!sidebar.isCollapsed">Sair</span>
      </button>
    </div>
  </div>
</template>
