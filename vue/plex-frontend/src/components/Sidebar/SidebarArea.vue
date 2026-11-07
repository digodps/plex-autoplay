<script setup>
import { useSidebarStore } from "@/stores/sidebar";
import SidebarItem from "./SidebarItem.vue";

const menuGroups = [
  {
    name: "MENU",
    menuItems: [
      { name: "Dashboard", to: "/dashboard", icon: "home" },
      { name: "Bibliotecas", to: "/libraries", icon: "library" },
      { name: "Séries", to: "/libraries", icon: "tv" },
      { name: "Sincronizar", to: "/sync", icon: "sync" },
      { name: "Ajustes", to: "/settings", icon: "settings" },
    ],
  },
];

const sidebar = useSidebarStore();
</script>

<template>
  <div class="sidebar w-64 flex h-screen flex-col bg-gray-50 dark:bg-gray-900 text-body text-sm border-r dark:border-gray-800">
    <button
      class="flex w-full items-center gap-3 rounded-lg px-3 py-3 text-body hover:text-primary dark:hover:text-bodydark hover:bg-stroke dark:hover:bg-strokedark"
      type="button"
      @click="sidebar.toggleSidebar"
    >
      <span class="relative block h-5.5 w-5.5 rounded-sm bg-white dark:bg-boxdark dark:border md:hidden">
        <span class="relative right-0 h-full w-full">
          <span class="relative block my-1 rounded-sm bg-gray-700"></span>
          <span class="relative block my-1 rounded-sm bg-gray-700"></span>
          <span class="relative block my-1 rounded-sm bg-gray-700"></span>
        </span>
        <span class="absolute right-0 h-full w-full rotate-45 bg-gray-700"></span>
      </span>
    </button>

    <ul class="flex-1 flex flex-col p-2 pt-2 overflow-y-auto space-y-1" @click="sidebar.closeSidebar">
      <li v-for="group in menuGroups" :key="group.name" class="flex flex-col space-y-1">
        <span class="text-xs text-bodydark font-medium uppercase tracking-widest capitalize">{{ group.name }}</span>
        <div class="flex flex-col space-y-1">
          <SidebarItem
            v-for="item in group.menuItems"
            :key="item.to"
            :to="item.to"
            :icon="item.icon"
          />
        </div>
      </li>
    </ul>
  </div>
</template>