<script setup>
import { RouterView, useRoute } from "vue-router";
import { computed } from "vue";
import AppHeader from "@/components/AppHeader.vue";
import { buildBreadcrumb } from "@/lib/breadcrumb";

const route = useRoute();

const showShell = computed(() => route.name !== "login" && route.name !== "setup");

const breadcrumbItems = computed(() => buildBreadcrumb(route, []));
</script>

<template>
  <div>
    <div v-if="showShell" class="shell">
      <AppHeader :items="breadcrumbItems" />
      <main class="main">
        <router-view v-slot="{ Component }">
          <transition name="page" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
    </div>
    <RouterView v-else />
  </div>
</template>
