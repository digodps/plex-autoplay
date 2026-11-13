export function msToReligious(ms) {
  if (ms == null || isNaN(ms)) return "00:00:00.00";
  const totalSec = Math.floor(Number(ms) / 1000);
  const hours = Math.floor(totalSec / 3600);
  const minutes = Math.floor((totalSec % 3600) / 60);
  const seconds = totalSec % 60;
  const cs = Math.floor((Number(ms) % 1000) / 10);
  
  return `${String(hours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}.${String(cs).padStart(2, "0")}`;
}

export function religiousToMs(str) {
  if (!str || typeof str !== "string") return 0;
  const parts = str.trim().split(".");
  const timePart = parts[0];
  const csPart = parts[1] ? parseInt(parts[1].padEnd(2, "0").slice(0, 2), 10) : 0;
  
  const timeSubParts = timePart.split(":").map(p => parseInt(p, 10) || 0);
  let hours = 0, minutes = 0, seconds = 0;
  
  if (timeSubParts.length === 3) {
    [hours, minutes, seconds] = timeSubParts;
  } else if (timeSubParts.length === 2) {
    [minutes, seconds] = timeSubParts;
  } else if (timeSubParts.length === 1) {
    seconds = timeSubParts[0];
  }
  
  return (hours * 3600 + minutes * 60 + seconds) * 1000 + csPart * 10;
}
