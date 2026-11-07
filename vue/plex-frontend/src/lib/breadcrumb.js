const HOME = { label: "Início", to: { name: "dashboard" } };

function text(value) {
  return Array.isArray(value) ? String(value[0] ?? "") : value ? String(value) : "";
}

function libraryTitle(libraries, key) {
  if (key == null) return "";
  return (libraries ?? []).find((lib) => String(lib.key) === String(key))?.title ?? "";
}

export function buildBreadcrumb(route, libraries = [], { loading = false } = {}) {
  if (route?.name === "dashboard") return [{ label: HOME.label, isCurrent: true }];

  const params = route?.params ?? {};
  const query = route?.query ?? {};
  const key = params.key ?? query.libraryKey;
  const lib = libraryTitle(libraries, key) || text(query.libraryTitle);
  const items = [{ ...HOME }];

  if (route?.name === "libraries") {
    items.push({ label: "Bibliotecas", isCurrent: true });
  } else if (route?.name === "series") {
    items.push({ label: loading ? "" : lib || "Biblioteca indisponível", isCurrent: true });
  } else if (route?.name === "series-markers") {
    if (lib) {
      items.push({
        label: lib,
        to: key ? { name: "series", params: { key } } : undefined,
      });
    }
    items.push({ label: text(query.seriesTitle) || "Série", isCurrent: true });
  } else {
    items.push({ label: route?.meta?.title ?? "", isCurrent: true });
  }

  return items.filter((item) => item.label);
}
