import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useUIStore = defineStore('ui', () => {
  const showProfile = ref(false);
  const showLanguageMenu = ref(false);

  function toggleProfile() {
    showProfile.value = !showProfile.value;
    if (!showProfile.value) {
      showLanguageMenu.value = false;
    }
  }

  function closeProfile() {
    showProfile.value = false;
    showLanguageMenu.value = false;
  }

  function toggleLanguageMenu() {
    showLanguageMenu.value = !showLanguageMenu.value;
  }

  function closeLanguageMenu() {
    showLanguageMenu.value = false;
  }

  return {
    showProfile,
    showLanguageMenu,
    toggleProfile,
    closeProfile,
    toggleLanguageMenu,
    closeLanguageMenu,
  };
});