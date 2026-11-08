<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import AppIcon from "@/components/AppIcon.vue";
import { isPathActive } from "@/lib/navigation.js";

const props = defineProps({
  to: { type: String, required: true },
  icon: { type: String, required: true },
  label: { type: String, default: "" },
  collapsed: { type: Boolean, default: false },
  match: { type: Array, default: null },
});

const route = useRoute();

const isActive = computed(() => isPathActive(route.path, props.to, props.match ?? []));
</script>

<template>
  <li>
    <router-link
      :to="to"
      class="flex items-center rounded-md px-3 py-1.5 text-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900"
      :class="[
        isActive
          ? 'bg-primary text-white font-medium'
          : 'text-body hover:bg-stroke dark:text-bodydark dark:hover:bg-strokedark',
        collapsed && 'justify-center px-1.5',
      ]"
      :aria-label="collapsed ? label : undefined"
      :title="collapsed ? label : undefined"
      :aria-current="isActive ? 'page' : undefined"
      v-bind="$attrs"
    >
      <AppIcon :name="icon" size="sm" class="m-1" />
      <span v-if="!collapsed">{{ label || to }}</span>
    </router-link>
  </li>
</template>
