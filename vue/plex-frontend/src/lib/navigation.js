export function isPathActive(path, to, match = []) {
  return [to, ...match].some((p) => path === p || path.startsWith(`${p}/`));
}
