import { ref, onMounted, onUnmounted, watch } from "vue";

/**
 * Composable para rolagem infinita baseado em IntersectionObserver.
 * Observa um elemento sentinela e dispara `onLoadMore` quando ele se aproxima do viewport.
 *
 * @param {Object} options
 * @param {Function} options.onLoadMore - Função chamada para buscar mais itens (deve ser assíncrona ou retornar boolean/promessa)
 * @param {Function} [options.onEndReached] - Callback executado quando o fim da lista é alcançado
 * @param {import("vue").Ref<boolean>} [options.hasMore] - Ref indicando se ainda há mais páginas a buscar
 * @param {import("vue").Ref<boolean>} [options.isLoading] - Ref externa para controle de carregamento
 * @param {string} [options.rootMargin="200px"] - Margem de pré-carregamento do viewport
 * @param {number} [options.threshold=0.1] - Fração visível da sentinela para acionamento
 */
export function useInfiniteScroll(options = {}) {
  const { onLoadMore, onEndReached, rootMargin = "200px", threshold = 0.1 } = options;

  const sentinelRef = ref(null);
  const internalIsLoading = ref(false);
  const isLoading = options.isLoading || internalIsLoading;
  const internalHasMore = ref(true);
  const hasMore = options.hasMore || internalHasMore;

  let observer = null;

  async function loadMore() {
    if (isLoading.value || !hasMore.value) return;

    isLoading.value = true;
    try {
      if (typeof onLoadMore === "function") {
        const result = await onLoadMore();
        // Se a chamada sinalizar que não há mais itens (result === false ou hasMore for false)
        if (result === false || !hasMore.value) {
          hasMore.value = false;
          if (typeof onEndReached === "function") {
            onEndReached();
          }
        }
      }
    } finally {
      isLoading.value = false;
    }
  }

  function cleanupObserver() {
    if (observer) {
      observer.disconnect();
      observer = null;
    }
  }

  function setupObserver() {
    cleanupObserver();

    if (typeof window === "undefined" || typeof window.IntersectionObserver === "undefined") {
      return;
    }

    if (!sentinelRef.value || !hasMore.value) return;

    observer = new window.IntersectionObserver(
      (entries) => {
        const [entry] = entries;
        if (entry && entry.isIntersecting && !isLoading.value && hasMore.value) {
          loadMore();
        }
      },
      {
        rootMargin,
        threshold,
      },
    );

    observer.observe(sentinelRef.value);
  }

  watch(sentinelRef, () => {
    setupObserver();
  });

  watch(hasMore, (newVal) => {
    if (newVal) {
      setupObserver();
    } else {
      cleanupObserver();
    }
  });

  onMounted(() => {
    setupObserver();
  });

  onUnmounted(() => {
    cleanupObserver();
  });

  return {
    sentinelRef,
    isLoading,
    hasMore,
    loadMore,
  };
}
