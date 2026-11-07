<script setup>
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useSidebarStore } from "@/stores/sidebar";
import { useThemeStore } from "@/stores/theme";
import { useAuthStore } from "@/stores/auth";
import AppIcon from "@/components/AppIcon.vue";

const route = useRoute();
const router = useRouter();
const sidebar = useSidebarStore();
const theme = useThemeStore();
const auth = useAuthStore();

const pageTitle = computed(() => route.meta.title ?? "Dashboard");

function handleLogout() {
  auth.logout();
  router.push({ name: "login" });
}

function toggleTheme() {
  theme.mode === "dark" ? theme.enterLight() : theme.enterDark();
}
</script>

<template>
  <header
    class="sticky top-0 z-999 flex w-full bg-white drop-shadow-1 dark:bg-boxdark dark:drop-shadow-none"
  >
    <div class="flex flex-grow items-center justify-between px-4 py-4 shadow-2 md:px-6 2xl:px-11">
      <div class="flex items-center gap-2 sm:gap-4 lg:hidden">
        <button
          class="z-99999 block rounded-sm border border-stroke bg-white p-1.5 shadow-sm dark:border-strokedark dark:bg-boxdark"
          type="button"
          aria-label="Abrir menu"
          @click="sidebar.toggleSidebar"
        >
          <span class="relative block h-5.5 w-5.5 cursor-pointer">
            <span class="du-block absolute right-0 h-full w-full">
              <span
                class="relative left-0 top-0 my-1 block h-0.5 w-0 rounded-sm bg-black delay-[0] duration-200 ease-in-out dark:bg-white"
                :class="{ '!w-full delay-300': sidebar.isSidebarOpen }"
              ></span>
              <span
                class="relative left-0 top-0 my-1 block h-0.5 w-0 rounded-sm bg-black delay-150 duration-200 ease-in-out dark:bg-white"
                :class="{ '!w-full delay-400': sidebar.isSidebarOpen }"
              ></span>
              <span
                class="relative left-0 top-0 my-1 block h-0.5 w-0 rounded-sm bg-black delay-200 duration-200 ease-in-out dark:bg-white"
                :class="{ '!w-full delay-500': sidebar.isSidebarOpen }"
              ></span>
            </span>
            <span class="du-block absolute right-0 h-full w-full rotate-45">
              <span
                class="absolute left-2.5 top-0 block h-full w-0.5 rounded-sm bg-black delay-300 duration-200 ease-in-out dark:bg-white"
                :class="{ '!h-full! delay-500': sidebar.isSidebarOpen }"
              ></span>
              <span
                class="absolute right-2.5 top-0 block h-full w-0.5 rounded-sm bg-black delay-300 duration-200 ease-in-out dark:bg-white"
                :class="{ '!h-full! delay-500': sidebar.isSidebarOpen }"
              ></span>
            </span>
          </span>
        </button>
      </div>

      <div class="flex items-center gap-3 2xsm:gap-6">
        <h1 class="hidden text-title-md2 font-semibold text-black dark:text-white sm:block">
          {{ pageTitle }}
        </h1>
      </div>

      <div class="flex items-center gap-3 2xsm:gap-7">
        <button
          class="flex h-9 w-9 items-center justify-center rounded-full border border-stroke bg-gray-2 text-body transition hover:text-primary dark:border-strokedark dark:bg-meta-4 dark:text-bodydark"
          type="button"
          :title="theme.mode === 'dark' ? 'Tema claro' : 'Tema escuro'"
          @click="toggleTheme"
        >
          <AppIcon :name="theme.mode === 'dark' ? 'sun' : 'moon'" size="sm" />
        </button>

        <button
          class="flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-body transition hover:text-primary dark:text-bodydark"
          type="button"
          @click="handleLogout"
        >
          <AppIcon name="logout" size="sm" />
          <span class="hidden sm:inline">Sair</span>
        </button>
      </div>
    </div>
  </header>
</template>