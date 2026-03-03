import { useEffect, useState } from 'react';
import { adminApi, type Snapshot, type SnapshotDiff } from '../services/adminApi';

export function HistoryPage() {
  const [snapshots, setSnapshots] = useState<Snapshot[]>([]);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [diff, setDiff] = useState<SnapshotDiff | null>(null);
  const [error, setError] = useState('');

  useEffect(() => { adminApi.getSnapshots().then(setSnapshots); }, []);

  const toggleSelect = (id: string) => {
    setSelected(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else if (next.size < 2) next.add(id);
      return next;
    });
    setDiff(null);
  };

  const compare = async () => {
    const [from, to] = Array.from(selected);
    setError('');
    try {
      const result = await adminApi.compareSnapshots(from, to);
      setDiff(result);
    } catch (err) { setError((err as Error).message); }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ margin: 0 }}>Radar History</h2>
        {selected.size === 2 && (
          <button onClick={compare} style={{ background: '#1a1a2e', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 4, cursor: 'pointer' }}>
            Compare Selected
          </button>
        )}
      </div>
      {selected.size > 0 && selected.size < 2 && <p style={{ color: '#888', fontSize: 13 }}>Select one more snapshot to compare.</p>}
      {error && <div style={{ background: '#ffeaea', color: '#c0392b', padding: 10, borderRadius: 4, marginBottom: 16 }}>{error}</div>}

      {diff && (
        <div style={{ background: '#f9f9f9', border: '1px solid #ddd', borderRadius: 8, padding: 24, marginBottom: 24 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
            <h3 style={{ margin: 0 }}>Comparison</h3>
            <button onClick={() => setDiff(null)} style={{ background: 'none', border: 'none', fontSize: 18, cursor: 'pointer' }}>\u2715</button>
          </div>
          <p style={{ color: '#888', fontSize: 13 }}>
            {new Date(diff.fromCapturedAt).toLocaleString()} \u2192 {new Date(diff.toCapturedAt).toLocaleString()}
          </p>
          {diff.added.length > 0 && (
            <div style={{ marginBottom: 16 }}>
              <h4 style={{ color: '#27ae60', margin: '0 0 8px' }}>Added ({diff.added.length})</h4>
              {diff.added.map(e => (
                <div key={e.name} style={{ background: '#eafaf1', padding: '6px 12px', borderRadius: 4, marginBottom: 4 }}>
                  <strong>{e.name}</strong> \u2014 {e.quadrant.replace(/([A-Z])/g, ' $1').trim()} / {e.ring}
                </div>
              ))}
            </div>
          )}
          {diff.moved.length > 0 && (
            <div style={{ marginBottom: 16 }}>
              <h4 style={{ color: '#e67e22', margin: '0 0 8px' }}>Moved ({diff.moved.length})</h4>
              {diff.moved.map(e => (
                <div key={e.name} style={{ background: '#fef9e7', padding: '6px 12px', borderRadius: 4, marginBottom: 4 }}>
                  <strong>{e.name}</strong>: {e.fromRing} \u2192 <strong>{e.toRing}</strong>
                </div>
              ))}
            </div>
          )}
          {diff.removed.length > 0 && (
            <div style={{ marginBottom: 16 }}>
              <h4 style={{ color: '#e74c3c', margin: '0 0 8px' }}>Removed ({diff.removed.length})</h4>
              {diff.removed.map(e => (
                <div key={e.name} style={{ background: '#fdecea', padding: '6px 12px', borderRadius: 4, marginBottom: 4 }}>
                  <strong>{e.name}</strong>
                </div>
              ))}
            </div>
          )}
          <p style={{ color: '#888' }}>{diff.unchangedCount} entries unchanged.</p>
        </div>
      )}

      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 14 }}>
        <thead>
          <tr style={{ background: '#f5f5f5' }}>
            {['Select', 'Captured At', 'Trigger', 'Entries'].map(h => (
              <th key={h} style={{ padding: '10px 12px', textAlign: 'left', borderBottom: '2px solid #ddd' }}>{h}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {snapshots.map(s => (
            <tr key={s.id} style={{ borderBottom: '1px solid #eee', background: selected.has(s.id) ? '#eaf4fb' : 'transparent' }}>
              <td style={{ padding: '10px 12px' }}>
                <input type="checkbox" checked={selected.has(s.id)} onChange={() => toggleSelect(s.id)} />
              </td>
              <td style={{ padding: '10px 12px' }}>{new Date(s.capturedAt).toLocaleString()}</td>
              <td style={{ padding: '10px 12px', color: '#666' }}>{s.triggerEvent}</td>
              <td style={{ padding: '10px 12px', textAlign: 'center' }}>{s.entryCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {!snapshots.length && <p style={{ color: '#999', textAlign: 'center', marginTop: 40 }}>No snapshots yet.</p>}
    </div>
  );
}
