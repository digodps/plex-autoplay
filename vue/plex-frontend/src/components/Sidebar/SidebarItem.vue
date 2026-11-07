<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import { useSidebarStore } from "@/stores/sidebar";
import AppIcon from "@/components/AppIcon.vue";

const sidebar = useSidebarStore();

const props = defineProps({
  to: { type: String, required: true },
  icon: { type: String, required: true },
});

const route = useRoute();
const isActive = computed(() => route.path === props.to);
</script>

<template>
  <li>
    <router-link
      :to="to"
      class="flex items-center rounded-md px-3 py-1.5 text-body text-sm transition-colors dark:text-bodydark dark:hover:bg-stroke dark:hover:hover:bg-strokedark"
      v-bind="$attrs"
      @click="sidebar.closeSidebar"
    >
      <AppIcon :name="icon" size="sm" />
      <span v-if="!isActive">{{ to }}</span>
      <span v-else class="text-primary font-medium">
        {{ to }}
      </span>
    </router-link>
  </li>
</template>