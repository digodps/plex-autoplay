<script setup>
defineProps({
  show: {
    type: Boolean,
    default: false
  }
})

function closeSidebar() {
  // This will be handled by the parent component
}
</script>

<template>
  <div 
    v-if="show" 
    class="fixed inset-0 z-20 bg-black bg-opacity-50 md:hidden"
    @click="closeSidebar"
  ></div>
</template>