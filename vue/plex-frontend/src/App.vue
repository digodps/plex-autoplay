<script setup>
import { RouterView } from "vue-router";
import { useThemeStore } from "./stores/theme";

const theme = useThemeStore();
// Load persisted mode from backend once at app start (fire and forget).
theme.loadTheme();
</script>

<template>
  <RouterView />
</template>