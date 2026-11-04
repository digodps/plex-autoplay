<script setup>
/**
 * Rail de navegação no estilo Canva: ícone acima do rótulo.
 * Apresentacional puro — recebe itens/rota ativa por props e emite eventos;
 * não importa vue-router, store, composable ou api.
 */
import AppIcon from "./AppIcon.vue";

defineProps({
  items: { type: Array, required: true },
  activePath: { type: String, default: "" },
  statusLabel: { type: String, default: "" },
});

defineEmits(["select", "logout"]);
</script>

<template>
  <aside
    class="flex w-[88px] shrink-0 flex-col items-center gap-3 border-r border-solid border-rail-border bg-rail px-2 py-4 max-md:w-full max-md:flex-row max-md:gap-2 max-md:overflow-x-auto max-md:border-r-0 max-md:border-b max-md:px-3 max-md:py-2"
  >
    <img src="/favicon.svg" alt="Plex Autoplay" class="mb-2 h-8 w-8 max-md:hidden" />

    <nav
      class="flex w-full flex-col gap-2 max-md:w-auto max-md:flex-row"
      aria-label="Navegação principal"
    >
      <button
        v-for="item in items"
        :key="item.to"
        type="button"
        class="flex w-full min-h-[60px] flex-col items-center justify-center gap-1 rounded-xl px-0.5 py-2 text-center text-[0.6875rem] leading-tight text-brand-strong transition-colors duration-150 hover:bg-brand-soft focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand max-md:min-h-11 max-md:w-auto max-md:min-w-16 max-md:flex-row max-md:gap-2 max-md:px-3"
        :class="activePath === item.to ? 'bg-brand-soft font-semibold' : ''"
        :aria-current="activePath === item.to ? 'page' : undefined"
        @click="$emit('select', item.to)"
      >
        <AppIcon
          :name="item.icon"
          class="h-6 w-6 shrink-0"
          :class="activePath === item.to ? 'text-brand-strong' : 'text-brand'"
        />
        <span class="max-w-full truncate">{{ item.label }}</span>
      </button>
    </nav>

    <div
      class="mt-auto flex w-full flex-col items-center gap-3 max-md:mt-0 max-md:ml-auto max-md:w-auto max-md:flex-row"
    >
      <span
        class="h-2 w-2 rounded-full bg-[var(--success)]"
        role="img"
        :aria-label="statusLabel"
      ></span>

      <button
        type="button"
        class="flex h-11 w-11 items-center justify-center rounded-xl text-brand-strong transition-colors duration-150 hover:bg-brand-soft focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand"
        aria-label="Sair"
        title="Sair"
        @click="$emit('logout')"
      >
        <AppIcon name="logout" class="h-6 w-6" />
      </button>
    </div>
  </aside>
</template>
