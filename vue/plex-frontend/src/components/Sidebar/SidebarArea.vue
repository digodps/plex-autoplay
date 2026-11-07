<script setup>
import { useSidebarStore } from "@/stores/sidebar";
import { useThemeStore } from "@/stores/theme";
import AppIcon from "@/components/AppIcon.vue";

const sidebar = useSidebarStore();
const theme = useThemeStore();

const menuGroups = [
  {
    name: "MENU",
    menuItems: [{ label: "Bibliotecas", to: "/libraries", icon: "library" }],
  },
];

const actionClasses =
  "flex w-full items-center gap-2 rounded-lg p-2 text-gray-700 hover:text-primary hover:bg-gray-100 dark:text-bodydark dark:hover:bg-gray-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900";
</script>

<template>
  <div
    class="sidebar flex h-screen flex-col bg-gray-50 text-gray-700 text-sm border-r dark:text-bodydark dark:border-gray-800 dark:bg-gray-900"
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
        <span v-if="!sidebar.isCollapsed">{{ theme.mode === 'dark' ? 'Light' : 'Dark' }}</span>
      </button>

      <div class="flex-1"></div>
    </div>

    <ul class="flex-1 flex flex-col p-2 pt-2 overflow-y-auto space-y-1">
      <li
        v-for="group in menuGroups"
        :key="group.name"
        class="flex flex-col space-y-1"
      >
        <router-link
          v-for="menuItem in group.menuItems"
          :key="menuItem.label"
          :to="menuItem.to"
          class="flex items-center gap-2 rounded-md px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 hover:text-primary dark:text-bodydark transition-colors dark:hover:bg-gray-800 dark:hover:text-primary"
        >
          <AppIcon :name="menuItem.icon" size="sm" />
          <span v-if="!sidebar.isCollapsed">{{ menuItem.label }}</span>
        </router-link>
      </li>
    </ul>
  </div>
</template>
