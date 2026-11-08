const HOME = { label: "Início", to: { name: "dashboard" } };
const LIBRARIES = { label: "Bibliotecas", to: { name: "libraries" } };

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
  const items = [{ ...HOME }];

  if (name === "libraries") {
    items.push({ ...LIBRARIES });
  } else if (name === "series") {
    items.push({ ...LIBRARIES });
    if (!loading) items.push({ label: lib || "Biblioteca indisponível" });
  } else if (name === "series-markers") {
    items.push({ ...LIBRARIES });
    if (lib) {
      items.push({
        label: lib,
        to: key ? { name: "series", params: { key } } : undefined,
      });
    }
    items.push({ label: text(query.seriesTitle) || "Série" });
  } else if (name !== "dashboard") {
    items.push({ label: route?.meta?.title || "Página" });
  }

  const filtered = items.filter((item) => item.label);
  return filtered.map((item, index) => ({
    ...item,
    isCurrent: index === filtered.length - 1,
  }));
}
