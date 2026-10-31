import { ref, onMounted, onUnmounted, watch } from "vue";

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

  function findScrollContainer(element) {
    if (!element || typeof window === "undefined") return null;

    let parent = element.parentElement;
    while (parent && parent !== document.body && parent !== document.documentElement) {
      const style = window.getComputedStyle(parent);
      const overflowY = style.overflowY;
      if (/auto|scroll|overlay/.test(overflowY)) {
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
    const sentinelRect = sentinel.getBoundingClientRect();
    const containerRect = container.getBoundingClientRect();
    const rootMarginPx = parseInt(rootMargin) || 200;
    const distanceFromBottom = containerRect.bottom - sentinelRect.top;
    if (distanceFromBottom <= rootMarginPx + container.clientHeight) {
      loadMore();
    }
  }

  function setupScrollFallback() {
    if (scrollHandler && detectedRoot) {
      detectedRoot.removeEventListener("scroll", scrollHandler, { passive: true });
    }
    const container = root ?? findScrollContainer(sentinelRef.value);
    detectedRoot = container;
    if (!container) return;
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
    scrollCheckTimeout = setInterval(() => {
      if (hasMore.value && !isLoading.value) {
        checkScrollPosition();
      }
    }, 1000);
  }

  function setupObserver() {
    cleanupObserver();
    if (typeof window === "undefined" || typeof window.IntersectionObserver === "undefined") return;
    if (!sentinelRef.value || !hasMore.value) return;
    const observerRoot = root ?? findScrollContainer(sentinelRef.value);
    detectedRoot = observerRoot;
    if (!observerRoot) {
      if (import.meta.env.DEV) {
        console.warn("[useInfiniteScroll] Nenhum container rolável detectado...");
      }
    }
    observer = new window.IntersectionObserver(
      (entries) => {
        const [entry] = entries;
        if (entry && entry.isIntersecting && !isLoading.value && hasMore.value) {
          loadMore();
        }
      },
      { root: observerRoot, rootMargin, threshold }
    );
    observer.observe(sentinelRef.value);
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

  watch(
    () => (root ? root : findScrollContainer(sentinelRef.value)),
    (newRoot) => {
      if (!root && newRoot !== detectedRoot) {
        setupObserver();
      }
    }
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