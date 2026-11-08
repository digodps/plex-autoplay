import { ref, watch } from "vue";
import { fetchLibraries } from "@/api";

export function useLibraryTitle(key, queryTitle) {
  const title = ref("");
  const loading = ref(true);
  const error = ref("");

  let reqKey = "";

  async function load(targetKey) {
    reqKey = targetKey;
    loading.value = true;
    title.value = "";
    error.value = "";

    try {
      const libraries = await fetchLibraries();
      if (reqKey !== targetKey) return; // navegou para outra biblioteca
      const lib = (libraries ?? []).find((l) => String(l.key) === String(targetKey));
      title.value = lib?.title ?? "";
    } catch (err) {
      if (reqKey === targetKey) error.value = err.message || "Erro ao carregar título da biblioteca";
    } finally {
      if (reqKey === targetKey) loading.value = false;
    }
  }

  watch(
    [key, () => queryTitle?.value],
    ([newKey, newTitle]) => {
      if (newTitle) {
        reqKey = newKey ?? "";
        title.value = newTitle;
        error.value = "";
        loading.value = false;
        return;
      }
      if (!newKey) {
        reqKey = "";
        title.value = "";
        error.value = "";
        loading.value = false;
        return;
      }
      load(newKey);
    },
    { immediate: true },
  );

  return { title, loading, error };
}
