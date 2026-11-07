<script setup>
import AppIcon from "@/components/AppIcon.vue";
import { isPathActive } from "@/lib/navigation";

const props = defineProps({
  items: { type: Array, default: () => [] },
  collapsed: { type: Boolean, default: false },
  activePath: { type: String, default: "" },
  error: { type: String, default: "" },
});

const itemClass =
  "flex min-h-[44px] items-center gap-2 rounded-md px-3 py-2 text-sm text-gray-700 transition-colors hover:bg-gray-100 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary motion-reduce:transition-none dark:text-bodydark dark:hover:bg-gray-800 dark:hover:text-primary";
const activeClass = "bg-gray-100 font-medium text-primary dark:bg-gray-800";
</script>

<template>
  <aside
    class="sidebar flex min-h-0 flex-col border-r bg-gray-50 text-sm text-gray-700 dark:border-gray-800 dark:bg-gray-900 dark:text-bodydark"
    :class="collapsed ? 'w-16' : 'w-64 max-[768px]:w-16'"
  >
    <p
      v-if="error"
      role="alert"
      class="m-2 rounded-md bg-red-50 px-3 py-2 text-xs text-red-700 dark:bg-red-950 dark:text-red-300"
    >
      {{ error }}
    </p>
    <ul v-else class="m-0 flex list-none flex-col gap-1 overflow-y-auto p-2">
      <li v-for="lib in props.items" :key="lib.key">
        <RouterLink
          :to="`/libraries/${lib.key}/series`"
          :class="[
            itemClass,
            isPathActive(props.activePath, `/libraries/${lib.key}`) ? activeClass : '',
          ]"
          :aria-current="
            isPathActive(props.activePath, `/libraries/${lib.key}`) ? 'page' : undefined
          "
          :aria-label="lib.title"
          :title="collapsed ? lib.title : undefined"
        >
          <AppIcon name="library" size="sm" />
          <span v-if="!collapsed" class="truncate max-[768px]:hidden">{{ lib.title }}</span>
        </RouterLink>
      </li>
    </ul>
  </aside>
</template>
