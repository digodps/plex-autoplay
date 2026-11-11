<script setup>
defineProps({
  /** Alinhamento dos botões: 'right' (padrão) | 'center' | 'between' */
  align: {
    type: String,
    default: "right",
    validator: (v) => ["right", "center", "between"].includes(v),
  },
  /** Espaçamento entre botões */
  gap: { type: String, default: "0.75rem" },
});
</script>

<template>
  <div
    class="action-bar"
    :class="`action-bar--${align}`"
    :style="{ columnGap: gap }"
    role="group"
    aria-label="Ações do dashboard"
  >
    <slot name="secondary" />
    <slot name="primary" />
  </div>
</template>

<style scoped>
.action-bar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  padding: 0.5rem 0;
  margin-top: auto; /* empurra para baixo no card se houver espaço */
}

.action-bar--right {
  justify-content: flex-end;
}

.action-bar--center {
  justify-content: center;
}

.action-bar--between {
  justify-content: space-between;
}

/* Ordenação visual: secondary à esquerda, primary à direita */
.action-bar ::slotted(*:first-child) {
  order: 1; /* secondary */
}
.action-bar ::slotted(*:last-child) {
  order: 2; /* primary */
}

/* Garante que botões nativos e componentes com .btn funcionem */
.action-bar :deep(.btn),
.action-bar :deep(button),
.action-bar :deep(a) {
  white-space: nowrap;
}

/* Mobile: empilha se não couber */
@media (max-width: 480px) {
  .action-bar {
    width: 100%;
    justify-content: stretch;
  }
  .action-bar :deep(.btn) {
    flex: 1;
    justify-content: center;
  }
}
</style>
