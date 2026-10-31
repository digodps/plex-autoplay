import { ref, onMounted, onUnmounted, watch } from "vue";

/**
 * Composable para rolagem infinita baseado em IntersectionObserver com fallback de scroll.
 * Observa um elemento sentinela e dispara `onLoadMore` quando ele se aproxima do viewport
 * ou do container rolável mais próximo.
 *
 * @param {Object} options
 * @param {Function} options.onLoadMore - Função chamada para buscar mais itens (deve ser assíncrona ou retornar boolean/promessa)
 * @param {Function} [options.onEndReached] - Callback executado quando o fim da lista é alcançado
 * @param {import("vue").Ref<boolean>} [options.hasMore] - Ref indicando se ainda há mais páginas a buscar
 * @param {import("vue").Ref<boolean>} [options.isLoading] - Ref externa para controle de carregamento
 * @param {string} [options.rootMargin="200px"] - Margem de pré-carregamento do viewport/container
 * @param {number} [options.threshold=0.1] - Fração visível da sentinela para acionamento
 * @param {Element|null} [options.root] - Container raiz para IntersectionObserver (auto-detectado se não fornecido)
 */
export function useInfiniteScroll(options = {}) {
  const { onLoadMore, onEndReached, rootMargin = "200px", threshold = 0.1, root = null } = options;

  const sentinelRef = ref(null);
  const internalIsLoading = ref(false);
  const isLoading = options.isLoading || internalIsLoading;
  const internalHasMore = ref(true);
  const hasMore = options.hasMore || internalHasMore;

  let observer = null;
  let detectedRoot = null;
  let scrollHandler = null;
  let scrollCheckTimeout = null;

  /**
   * Encontra o ancestral rolável mais próximo do elemento.
   * Verifica overflow-y: auto|scroll|overlay e se o elemento tem scrollHeight > clientHeight.
   */
  function findScrollContainer(element) {
    if (!element || typeof window === "undefined") return null;

    let parent = element.parentElement;
    while (parent && parent !== document.body && parent !== document.documentElement) {
      const style = window.getComputedStyle(parent);
      const overflowY = style.overflowY;
      const isScrollable = /(auto|scroll|overlay)/.test(overflowY);
      if (isScrollable) {
        if (import.meta.env.DEV) {
          console.log("[useInfiniteScroll] Encontrou container rolável:", parent, {
            overflowY,
            scrollHeight: parent.scrollHeight,
            clientHeight: parent.clientHeight,
          });
        }
        return parent;
      }
      parent = parent.parentElement;
    }
    return null;
  }

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
    // Clean up scroll fallback
    if (scrollHandler && detectedRoot) {
      detectedRoot.removeEventListener("scroll", scrollHandler, { passive: true });
      scrollHandler = null;
    }
    if (scrollCheckTimeout) {
      clearTimeout(scrollCheckTimeout);
      scrollCheckTimeout = null;
    }
  }

  function checkScrollPosition() {
    if (!sentinelRef.value || !detectedRoot || isLoading.value || !hasMore.value) return;

    const sentinel = sentinelRef.value;
    const container = detectedRoot;

    // Get sentinel position relative to container
    const sentinelRect = sentinel.getBoundingClientRect();
    const containerRect = container.getBoundingClientRect();

    // Check if sentinel is near the bottom of the scrollable area
    // Using rootMargin equivalent logic: trigger when sentinel is within rootMargin of container bottom
    const rootMarginPx = parseInt(rootMargin) || 200;
    const distanceFromBottom = containerRect.bottom - sentinelRect.top;

    if (distanceFromBottom <= rootMarginPx + container.clientHeight) {
      if (import.meta.env.DEV) {
        console.log("[useInfiniteScroll] Scroll fallback triggered:", {
          distanceFromBottom,
          rootMarginPx,
          containerHeight: container.clientHeight,
          scrollTop: container.scrollTop,
          scrollHeight: container.scrollHeight,
        });
      }
      loadMore();
    }
  }

  function setupScrollFallback() {
    // Remove existing handler
    if (scrollHandler && detectedRoot) {
      detectedRoot.removeEventListener("scroll", scrollHandler, { passive: true });
    }

    // Find scroll container again in case it changed
    const container = root ?? findScrollContainer(sentinelRef.value);
    detectedRoot = container;

    if (!container) return;

    // Throttled scroll handler
    let ticking = false;
    scrollHandler = () => {
      if (!ticking && hasMore.value && !isLoading.value) {
        ticking = true;
        requestAnimationFrame(() => {
          checkScrollPosition();
          ticking = false;
        });
      }
    };

    container.addEventListener("scroll", scrollHandler, { passive: true });

    // Also check periodically in case scroll event doesn't fire (e.g., programmatic scroll)
    scrollCheckTimeout = setInterval(() => {
      if (hasMore.value && !isLoading.value) {
        checkScrollPosition();
      }
    }, 1000);
  }

  function setupObserver() {
    cleanupObserver();

    if (typeof window === "undefined" || typeof window.IntersectionObserver === "undefined") {
      return;
    }

    if (!sentinelRef.value || !hasMore.value) return;

    // Usa root fornecido ou auto-detecta o container rolável
    const observerRoot = root ?? findScrollContainer(sentinelRef.value);
    detectedRoot = observerRoot;

    if (import.meta.env.DEV) {
      console.log("[useInfiniteScroll] setupObserver:", {
        hasSentinel: !!sentinelRef.value,
        hasMore: hasMore.value,
        observerRoot: observerRoot?.className || observerRoot,
        rootMargin,
        threshold,
      });
    }

    if (!observerRoot) {
      // Fallback para viewport - loga aviso em desenvolvimento
      if (import.meta.env.DEV) {
        console.warn(
          "[useInfiniteScroll] Nenhum container rolável detectado para a sentinela. Usando viewport como fallback. Verifique se o elemento sentinela está dentro de um container com overflow-y: auto|scroll.",
        );
      }
    }

    observer = new window.IntersectionObserver(
      (entries) => {
        const [entry] = entries;
        if (import.meta.env.DEV) {
          console.log("[useInfiniteScroll] IntersectionObserver callback:", {
            isIntersecting: entry?.isIntersecting,
            intersectionRatio: entry?.intersectionRatio,
            isLoading: isLoading.value,
            hasMore: hasMore.value,
          });
        }
        if (entry && entry.isIntersecting && !isLoading.value && hasMore.value) {
          if (import.meta.env.DEV) {
            console.log(
              "[useInfiniteScroll] loadMore triggered, hasMore:",
              hasMore.value,
              "isLoading:",
              isLoading.value,
            );
          }
          loadMore();
        }
      },
      {
        root: observerRoot,
        rootMargin,
        threshold,
      },
    );

    observer.observe(sentinelRef.value);

    // Setup scroll fallback
    setupScrollFallback();
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

  // Re-detecta container se root não foi fornecido explicitamente (ex: layout muda)
  watch(
    () => (root ? root : findScrollContainer(sentinelRef.value)),
    (newRoot) => {
      if (!root && newRoot !== detectedRoot) {
        setupObserver();
      }
    },
  );

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
