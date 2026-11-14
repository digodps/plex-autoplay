/**
 * Hierarchy parsing utilities for log messages
 * Extracts Library > Series > Episode information from log messages
 */

// Common patterns in log messages
const PATTERNS = {
  // "Processing library: Movies" or "Library: Movies" or "library Movies" or "biblioteca 1"
  library: [
    /(?:processing|library|biblioteca)\s*[:=]\s*([^,\n\r]+)/i,
    /^library\s+([^,\n\r]+)/i,
    /biblioteca\s+([^,\n\r]+)/i,
  ],
  // "Processing series: Breaking Bad (tvdb-123)" or "Series: Breaking Bad"
  series: [
    /(?:processing\s+)?series\s*[:=]\s*([^,\n\r]+)/i,
    /^series\s+([^,\n\r]+)/i,
  ],
  // "Processing episode: S01E01 - Pilot" or "Episode: S01E01"
  episode: [
    /(?:processing\s+)?episode\s*[:=]\s*([^,\n\r]+)/i,
    /^episode\s+([^,\n\r]+)/i,
    /(S\d{2}E\d{2}[^,\n\r]*)/i,
  ],
  // "Scan completed for library: TV Shows"
  scanCompleted: [
    /scan\s+completed\s+for\s+library\s*[:=]\s*([^,\n\r]+)/i,
  ],
}

/**
 * Extract hierarchy info from a log message
 * @param {string} message - Log message
 * @param {string} suggestion - Optional suggestion field
 * @returns {Object} Hierarchy object with library, series, episode
 */
export function parseHierarchy(message, suggestion = '') {
  const text = `${message} ${suggestion || ''}`.trim()
  const hierarchy = {
    library: null,
    series: null,
    episode: null,
    raw: text,
  }

  // Try library patterns
  for (const pattern of PATTERNS.library) {
    const match = text.match(pattern)
    if (match) {
      hierarchy.library = match[1].trim()
      break
    }
  }

  // Try series patterns
  for (const pattern of PATTERNS.series) {
    const match = text.match(pattern)
    if (match) {
      hierarchy.series = match[1].trim()
      break
    }
  }

  // Try episode patterns
  for (const pattern of PATTERNS.episode) {
    const match = text.match(pattern)
    if (match) {
      hierarchy.episode = match[1].trim()
      break
    }
  }

  // Try scan completed pattern (library only)
  if (!hierarchy.library) {
    for (const pattern of PATTERNS.scanCompleted) {
      const match = text.match(pattern)
      if (match) {
        hierarchy.library = match[1].trim()
        break
      }
    }
  }

  return hierarchy
}

/**
 * Check if log has any hierarchy info
 * @param {Object} hierarchy - Parsed hierarchy object
 * @returns {boolean}
 */
export function hasHierarchy(hierarchy) {
  return !!(hierarchy.library || hierarchy.series || hierarchy.episode)
}

/**
 * Get hierarchy display parts for rendering
 * @param {Object} hierarchy - Parsed hierarchy object
 * @returns {Array} Array of { label, value, icon } objects
 */
export function getHierarchyParts(hierarchy) {
  const parts = []
  
  if (hierarchy.library) {
    parts.push({ label: 'Biblioteca', value: hierarchy.library, icon: '📚' })
  }
  if (hierarchy.series) {
    parts.push({ label: 'Série', value: hierarchy.series, icon: '📺' })
  }
  if (hierarchy.episode) {
    parts.push({ label: 'Episódio', value: hierarchy.episode, icon: '▶️' })
  }
  
  return parts
}

/**
 * Format timestamp for display
 * @param {string} timestamp - ISO timestamp
 * @returns {Object} { time: string, date: string, full: string }
 */
export function formatTimestamp(timestamp) {
  const d = new Date(timestamp)
  if (Number.isNaN(d.getTime())) {
    return { time: timestamp, date: '', full: timestamp }
  }
  return {
    time: d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' }),
    date: d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' }),
    full: d.toLocaleString('pt-BR'),
  }
}

/**
 * Get level badge config
 * @param {string} level - Log level
 * @returns {Object} { label, class, dotClass }
 */
export function getLevelConfig(level) {
  const configs = {
    info: { label: 'Info', class: 'bg-blue-500/20 text-blue-400 border-blue-500/30', dotClass: 'bg-blue-400' },
    success: { label: 'Sucesso', class: 'bg-green-500/20 text-green-400 border-green-500/30', dotClass: 'bg-green-400' },
    warning: { label: 'Aviso', class: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30', dotClass: 'bg-yellow-400' },
    error: { label: 'Erro', class: 'bg-red-500/20 text-red-400 border-red-500/30', dotClass: 'bg-red-400' },
    skip: { label: 'Ignorado', class: 'bg-gray-500/20 text-gray-400 border-gray-500/30', dotClass: 'bg-gray-400' },
  }
  return configs[level] || configs.info
}

/**
 * Truncate text to max length
 * @param {string} text - Text to truncate
 * @param {number} maxLength - Maximum length
 * @returns {string} Truncated text with ellipsis if needed
 */
export function truncate(text, maxLength = 80) {
  if (!text || text.length <= maxLength) return text
  return text.slice(0, maxLength).trim() + '…'
}