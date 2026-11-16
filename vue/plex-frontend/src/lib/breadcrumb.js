// ponytail: acesso à home fica com o logo do AppHeader; a trilha começa abaixo dela.
import i18n from '@/i18n';

function t(key) {
  return i18n.global.t(key);
}

const LIBRARIES = { label: "breadcrumb.libraries", to: { name: "libraries" } };

function text(value) {
  return Array.isArray(value) ? String(value[0] ?? "") : value ? String(value) : "";
}

function libraryTitle(libraries, key) {
  if (key == null) return "";
  return (libraries ?? []).find((lib) => String(lib.key) === String(key))?.title ?? "";
}

export function buildBreadcrumb(route, libraries = [], { loading = false } = {}) {
  const name = route?.name;
  const params = route?.params ?? {};
  const query = route?.query ?? {};
  const key = params.key ?? query.libraryKey;
  const lib = libraryTitle(libraries, key) || text(query.libraryTitle);
  const items = [];

  if (name === "dashboard") return [];

  if (name === "libraries") {
    items.push({ ...LIBRARIES });
  } else if (name === "series") {
    items.push({ ...LIBRARIES });
    if (!loading) items.push({ label: lib || t("breadcrumb.libraryUnavailable") });
  } else if (name === "series-markers") {
    items.push({ ...LIBRARIES });
    if (lib) {
      items.push({
        label: lib,
        to: key ? { name: "series", params: { key } } : undefined,
      });
    }
    items.push({ label: text(query.seriesTitle) || lib || t("breadcrumb.library") });
  } else {
    items.push({ label: route?.meta?.title || t("breadcrumb.page") });
  }

  const filtered = items.filter((item) => item.label);
  return filtered.map((item, index) => ({
    ...item,
    isCurrent: index === filtered.length - 1,
  }));
}
