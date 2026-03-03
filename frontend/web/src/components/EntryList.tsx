import { useState } from 'react';
import type { Entry } from '../services/api';

interface Props {
  entries: Entry[];
  onEntryClick: (entry: Entry) => void;
}

const RINGS = ['', 'Adopt', 'Trial', 'Assess', 'Hold'];

const RING_COLORS: Record<string, string> = {
  Adopt: '#2ecc71', Trial: '#3498db', Assess: '#e67e22', Hold: '#e74c3c',
};

export function EntryList({ entries, onEntryClick }: Props) {
  const [quadrantFilter, setQuadrantFilter] = useState('');
  const [ringFilter, setRingFilter] = useState('');

  const filtered = entries.filter(e =>
    (!quadrantFilter || e.quadrant === quadrantFilter) &&
    (!ringFilter || e.ring === ringFilter)
  );

  return (
    <div>
      <div style={{ display: 'flex', gap: 12, marginBottom: 16, flexWrap: 'wrap' }}>
        <select value={quadrantFilter} onChange={e => setQuadrantFilter(e.target.value)}
          style={{ padding: '6px 12px', borderRadius: 4, border: '1px solid #ddd' }}>
          <option value="">All Quadrants</option>
          <option value="ConnectivityProtocols">Connectivity Protocols</option>
          <option value="EdgePlatforms">Edge Platforms</option>
          <option value="ToolsAndFrameworks">Tools & Frameworks</option>
          <option value="StandardsAndTechniques">Standards & Techniques</option>
        </select>
        <select value={ringFilter} onChange={e => setRingFilter(e.target.value)}
          style={{ padding: '6px 12px', borderRadius: 4, border: '1px solid #ddd' }}>
          <option value="">All Rings</option>
          {RINGS.filter(r => r).map(r => <option key={r} value={r}>{r}</option>)}
        </select>
        <span style={{ alignSelf: 'center', color: '#666', fontSize: 14 }}>
          {filtered.length} entries
        </span>
      </div>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        {filtered.map(e => (
          <div key={e.id}
            onClick={() => onEntryClick(e)}
            style={{
              padding: '10px 14px', background: '#f9f9f9', borderRadius: 6,
              cursor: 'pointer', border: '1px solid #eee',
              display: 'flex', alignItems: 'center', gap: 10
            }}>
            <span style={{
              background: RING_COLORS[e.ring] ?? '#999', color: '#fff',
              padding: '2px 8px', borderRadius: 4, fontSize: 11, minWidth: 50, textAlign: 'center'
            }}>{e.ring}</span>
            <span style={{ fontWeight: 600 }}>{e.name}</span>
            <span style={{ color: '#888', fontSize: 13 }}>{e.quadrant.replace(/([A-Z])/g, ' $1').trim()}</span>
          </div>
        ))}
        {!filtered.length && (
          <p style={{ color: '#999', fontStyle: 'italic' }}>No entries match the current filters.</p>
        )}
      </div>
    </div>
  );
}
