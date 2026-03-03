import type { EntryDetail } from '../services/api';

interface Props {
  entry: EntryDetail;
  onClose: () => void;
}

const RING_COLORS: Record<string, string> = {
  Adopt: '#2ecc71', Trial: '#3498db', Assess: '#e67e22', Hold: '#e74c3c',
};

export function EntryDetailPanel({ entry, onClose }: Props) {
  return (
    <div style={{
      position: 'fixed', right: 0, top: 0, height: '100vh', width: 400,
      background: '#fff', boxShadow: '-4px 0 20px rgba(0,0,0,0.15)',
      overflowY: 'auto', padding: 24, zIndex: 100
    }}>
      <button onClick={onClose} style={{
        float: 'right', background: 'none', border: 'none', fontSize: 20, cursor: 'pointer'
      }}>&#x2715;</button>
      <h2 style={{ marginTop: 0, marginRight: 30 }}>{entry.name}</h2>
      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginBottom: 16 }}>
        <span style={{
          background: RING_COLORS[entry.ring] ?? '#999', color: '#fff',
          padding: '3px 10px', borderRadius: 4, fontSize: 13
        }}>{entry.ring}</span>
        <span style={{
          background: '#eee', padding: '3px 10px', borderRadius: 4, fontSize: 13
        }}>{entry.quadrant.replace(/([A-Z])/g, ' $1').trim()}</span>
      </div>
      <p style={{ color: '#444', lineHeight: 1.6 }}>{entry.description}</p>
      <h4 style={{ color: '#555' }}>Rationale</h4>
      <p style={{ color: '#666', lineHeight: 1.6 }}>{entry.rationale}</p>
      {entry.tags.length > 0 && (
        <div style={{ marginBottom: 16 }}>
          {entry.tags.map(tag => (
            <span key={tag} style={{
              display: 'inline-block', background: '#e8f4fd', color: '#2980b9',
              padding: '2px 8px', borderRadius: 12, fontSize: 12, marginRight: 6, marginBottom: 4
            }}>{tag}</span>
          ))}
        </div>
      )}
      <p style={{ color: '#aaa', fontSize: 12 }}>
        Last reviewed: {new Date(entry.lastReviewedAt).toLocaleDateString()}
      </p>
      {entry.ringHistory.length > 0 && (
        <>
          <h4 style={{ color: '#555' }}>Ring History</h4>
          <table style={{ width: '100%', fontSize: 12, borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ background: '#f5f5f5' }}>
                <th style={{ padding: 6, textAlign: 'left' }}>Date</th>
                <th style={{ padding: 6, textAlign: 'left' }}>Change</th>
                <th style={{ padding: 6, textAlign: 'left' }}>By</th>
              </tr>
            </thead>
            <tbody>
              {entry.ringHistory.map((h, i) => (
                <tr key={i} style={{ borderBottom: '1px solid #eee' }}>
                  <td style={{ padding: 6 }}>{new Date(h.changedAt).toLocaleDateString()}</td>
                  <td style={{ padding: 6 }}>
                    {h.previousRing ? `${h.previousRing} \u2192 ` : ''}
                    <strong>{h.newRing}</strong>
                    {h.changeReason && <div style={{ color: '#888' }}>{h.changeReason}</div>}
                  </td>
                  <td style={{ padding: 6 }}>{h.changedBy}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
}
