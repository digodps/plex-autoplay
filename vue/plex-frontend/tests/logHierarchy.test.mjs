import { describe, it } from 'node:test'
import assert from 'node:assert'
import { parseHierarchy, parseFullHierarchy, getHierarchyTree, hasHierarchyTree, getHierarchyParts, hasHierarchy } from '../src/components/Logs/cards/logHierarchy.js'

describe('logHierarchy.js - parseHierarchy', () => {
  it('should extract library from "Processing library: Movies"', () => {
    const result = parseHierarchy('Processing library: Movies')
    assert.strictEqual(result.library, 'Movies')
  })

  it('should extract library from "Library: TV Shows"', () => {
    const result = parseHierarchy('Library: TV Shows')
    assert.strictEqual(result.library, 'TV Shows')
  })

  it('should extract series from "Processing series: Breaking Bad"', () => {
    const result = parseHierarchy('Processing series: Breaking Bad')
    assert.strictEqual(result.series, 'Breaking Bad')
  })

  it('should extract episode from "Processing episode: S01E01 - Pilot"', () => {
    const result = parseHierarchy('Processing episode: S01E01 - Pilot')
    assert.strictEqual(result.episode, 'S01E01 - Pilot')
  })

  it('should extract episode from "S02E05 - Episode Title"', () => {
    const result = parseHierarchy('S02E05 - Episode Title')
    assert.strictEqual(result.episode, 'S02E05 - Episode Title')
  })

  it('should extract all three levels from combined message', () => {
    const result = parseHierarchy('Processing library: Movies, Processing series: Breaking Bad, Processing episode: S01E01 - Pilot')
    assert.strictEqual(result.library, 'Movies')
    assert.strictEqual(result.series, 'Breaking Bad')
    assert.strictEqual(result.episode, 'S01E01 - Pilot')
  })

  it('should handle Portuguese messages', () => {
    const result = parseHierarchy('Processando biblioteca: Filmes')
    assert.strictEqual(result.library, 'Filmes')
  })

  it('should return object with null values for unrelated messages', () => {
    const result = parseHierarchy('Some random log message')
    assert.strictEqual(result.library, null)
    assert.strictEqual(result.series, null)
    assert.strictEqual(result.episode, null)
    assert.strictEqual(result.raw, 'Some random log message')
  })
})

describe('logHierarchy.js - parseFullHierarchy', () => {
  it('should build nested structure with library only', () => {
    const result = parseFullHierarchy('Processing library: Movies')
    assert.deepStrictEqual(result.libraries, [{
      name: 'Movies',
      icon: '📚',
      series: []
    }])
  })

  it('should build nested structure with library and series', () => {
    const result = parseFullHierarchy('Processing library: TV Shows, Processing series: Breaking Bad')
    assert.deepStrictEqual(result.libraries, [{
      name: 'TV Shows',
      icon: '📚',
      series: [{
        name: 'Breaking Bad',
        icon: '📺',
        episodes: []
      }]
    }])
  })

  it('should build nested structure with library, series, and episode', () => {
    const result = parseFullHierarchy('Processing library: TV Shows, Processing series: Breaking Bad, Processing episode: S01E01 - Pilot')
    assert.deepStrictEqual(result.libraries, [{
      name: 'TV Shows',
      icon: '📚',
      series: [{
        name: 'Breaking Bad',
        icon: '📺',
        episodes: [{
          name: 'S01E01 - Pilot',
          icon: '▶️'
        }]
      }]
    }])
  })

  it('should return empty libraries for unrelated messages', () => {
    const result = parseFullHierarchy('Some random log message')
    assert.deepStrictEqual(result.libraries, [])
  })
})

describe('logHierarchy.js - getHierarchyTree', () => {
  it('should return libraries array from hierarchy', () => {
    const hierarchy = { libraries: [{ name: 'Test', icon: '📚', series: [] }] }
    const result = getHierarchyTree(hierarchy)
    assert.deepStrictEqual(result, hierarchy.libraries)
  })

  it('should return empty array for invalid hierarchy', () => {
    assert.deepStrictEqual(getHierarchyTree(null), [])
    assert.deepStrictEqual(getHierarchyTree({}), [])
    assert.deepStrictEqual(getHierarchyTree({ libraries: null }), [])
  })
})

describe('logHierarchy.js - hasHierarchyTree', () => {
  it('should return true for hierarchy with series', () => {
    const hierarchy = { 
      libraries: [{ name: 'Test', icon: '📚', series: [{ name: 'Series', icon: '📺', episodes: [] }] }] 
    }
    assert.strictEqual(hasHierarchyTree(hierarchy), true)
  })

  it('should return false for hierarchy without series', () => {
    const hierarchy = { libraries: [{ name: 'Test', icon: '📚', series: [] }] }
    assert.strictEqual(hasHierarchyTree(hierarchy), false)
  })

  it('should return false for invalid hierarchy', () => {
    assert.strictEqual(hasHierarchyTree(null), false)
    assert.strictEqual(hasHierarchyTree({}), false)
  })
})

describe('logHierarchy.js - getHierarchyParts', () => {
  it('should return flat parts for backward compatibility', () => {
    const hierarchy = { library: 'Movies', series: 'Breaking Bad', episode: 'S01E01' }
    const result = getHierarchyParts(hierarchy)
    assert.deepStrictEqual(result, [
      { label: 'Biblioteca', value: 'Movies', icon: '📚' },
      { label: 'Série', value: 'Breaking Bad', icon: '📺' },
      { label: 'Episódio', value: 'S01E01', icon: '▶️' }
    ])
  })

  it('should handle partial hierarchy', () => {
    const hierarchy = { library: 'Movies' }
    const result = getHierarchyParts(hierarchy)
    assert.deepStrictEqual(result, [
      { label: 'Biblioteca', value: 'Movies', icon: '📚' }
    ])
  })
})

describe('logHierarchy.js - hasHierarchy', () => {
  it('should return true when any level exists', () => {
    assert.strictEqual(hasHierarchy({ library: 'Movies' }), true)
    assert.strictEqual(hasHierarchy({ series: 'Breaking Bad' }), true)
    assert.strictEqual(hasHierarchy({ episode: 'S01E01' }), true)
  })

  it('should return false for empty hierarchy', () => {
    assert.strictEqual(hasHierarchy({}), false)
    assert.strictEqual(hasHierarchy(null), false)
  })
})