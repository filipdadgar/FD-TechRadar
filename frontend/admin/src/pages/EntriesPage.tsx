import { useEffect, useState } from 'react';
import { adminApi, type Entry, type EntryInput } from '../services/adminApi';

const QUADRANTS = ['ConnectivityProtocols', 'EdgePlatforms', 'ToolsAndFrameworks', 'StandardsAndTechniques'];
const RINGS = ['Adopt', 'Trial', 'Assess', 'Hold'];

function EntryForm({ entry, onSave, onCancel }: {
  entry?: Entry; onSave: (data: EntryInput) => Promise<void>; onCancel: () => void;
}) {
  const [form, setForm] = useState<EntryInput>({
    name: entry?.name ?? '', description: entry?.description ?? '',
    rationale: entry?.rationale ?? '', quadrant: entry?.quadrant ?? 'ConnectivityProtocols',
    ring: entry?.ring ?? 'Assess', tags: entry?.tags ?? [], changeReason: ''
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setError('');
    try { await onSave(form); }
    catch (err) { setError((err as Error).message); }
    finally { setSaving(false); }
  };

  const f = (field: keyof EntryInput) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  const inputStyle = { width: '100%', padding: '8px 12px', border: '1px solid #ddd', borderRadius: 4, boxSizing: 'border-box' as const, marginTop: 4 };
  const labelStyle = { display: 'block', fontWeight: 600, fontSize: 13, color: '#555', marginBottom: 12 } as const;

  return (
    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
      {error && <div style={{ background: '#ffeaea', color: '#c0392b', padding: 10, borderRadius: 4 }}>{error}</div>}
      <label style={labelStyle}>Name *<input value={form.name} onChange={f('name')} required style={inputStyle} /></label>
      <label style={labelStyle}>Description *<textarea value={form.description} onChange={f('description')} required rows={3} style={{ ...inputStyle, resize: 'vertical' }} /></label>
      <label style={labelStyle}>Rationale *<textarea value={form.rationale} onChange={f('rationale')} required rows={4} style={{ ...inputStyle, resize: 'vertical' }} /></label>
      <label style={labelStyle}>Quadrant *
        <select value={form.quadrant} onChange={f('quadrant')} style={inputStyle}>
          {QUADRANTS.map(q => <option key={q} value={q}>{q.replace(/([A-Z])/g, ' $1').trim()}</option>)}
        </select>
      </label>
      <label style={labelStyle}>Ring *
        <select value={form.ring} onChange={f('ring')} style={inputStyle}>
          {RINGS.map(r => <option key={r} value={r}>{r}</option>)}
        </select>
      </label>
      <label style={labelStyle}>Tags (comma-separated)
        <input value={form.tags?.join(', ') ?? ''} onChange={e => setForm(p => ({ ...p, tags: e.target.value.split(',').map(t => t.trim()).filter(Boolean) }))} style={inputStyle} />
      </label>
      {entry && <label style={labelStyle}>Change Reason
        <input value={form.changeReason ?? ''} onChange={f('changeReason')} style={inputStyle} placeholder="Optional" />
      </label>}
      <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
        <button type="submit" disabled={saving} style={{ background: '#1a1a2e', color: '#fff', border: 'none', padding: '8px 20px', borderRadius: 4, cursor: 'pointer' }}>
          {saving ? 'Saving\u2026' : (entry ? 'Save Changes' : 'Create Entry')}
        </button>
        <button type="button" onClick={onCancel} style={{ background: '#eee', border: 'none', padding: '8px 20px', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
      </div>
    </form>
  );
}

const RING_COLORS: Record<string, string> = { Adopt: '#2ecc71', Trial: '#3498db', Assess: '#e67e22', Hold: '#e74c3c' };

export function EntriesPage() {
  const [entries, setEntries] = useState<Entry[]>([]);
  const [showArchived, setShowArchived] = useState(false);
  const [editing, setEditing] = useState<Entry | 'new' | null>(null);
  const [loading, setLoading] = useState(true);

  const reload = () => adminApi.getEntries(showArchived ? 'All' : 'Active').then(setEntries).finally(() => setLoading(false));
  useEffect(() => { reload(); }, [showArchived]);

  const handleSave = async (data: EntryInput) => {
    if (editing === 'new') await adminApi.createEntry(data);
    else if (editing) await adminApi.updateEntry(editing.id, data);
    setEditing(null); reload();
  };

  const handleArchive = async (id: string) => {
    if (!confirm('Archive this entry? It will be hidden from the public radar.')) return;
    await adminApi.archiveEntry(id); reload();
  };

  if (loading) return <p>Loading\u2026</p>;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ margin: 0 }}>Technology Entries</h2>
        <div style={{ display: 'flex', gap: 12 }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer' }}>
            <input type="checkbox" checked={showArchived} onChange={e => setShowArchived(e.target.checked)} />
            Show Archived
          </label>
          <button onClick={() => setEditing('new')} style={{ background: '#1a1a2e', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 4, cursor: 'pointer' }}>
            + Add Technology
          </button>
        </div>
      </div>

      {editing && (
        <div style={{ background: '#f9f9f9', border: '1px solid #ddd', borderRadius: 8, padding: 24, marginBottom: 24 }}>
          <h3 style={{ marginTop: 0 }}>{editing === 'new' ? 'Add Technology' : `Edit: ${editing.name}`}</h3>
          <EntryForm entry={editing === 'new' ? undefined : editing} onSave={handleSave} onCancel={() => setEditing(null)} />
        </div>
      )}

      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 14 }}>
        <thead>
          <tr style={{ background: '#f5f5f5' }}>
            {['Name', 'Quadrant', 'Ring', 'Status', 'Actions'].map(h => (
              <th key={h} style={{ padding: '10px 12px', textAlign: 'left', borderBottom: '2px solid #ddd' }}>{h}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {entries.map(e => (
            <tr key={e.id} style={{ borderBottom: '1px solid #eee', opacity: e.status === 'Archived' ? 0.6 : 1 }}>
              <td style={{ padding: '10px 12px', fontWeight: 600 }}>{e.name}</td>
              <td style={{ padding: '10px 12px', color: '#666' }}>{e.quadrant.replace(/([A-Z])/g, ' $1').trim()}</td>
              <td style={{ padding: '10px 12px' }}>
                <span style={{ background: RING_COLORS[e.ring] ?? '#999', color: '#fff', padding: '2px 8px', borderRadius: 4, fontSize: 12 }}>{e.ring}</span>
              </td>
              <td style={{ padding: '10px 12px', color: e.status === 'Archived' ? '#999' : '#27ae60' }}>{e.status}</td>
              <td style={{ padding: '10px 12px' }}>
                <div style={{ display: 'flex', gap: 6 }}>
                  <button onClick={() => setEditing(e)} style={{ background: '#3498db', color: '#fff', border: 'none', padding: '4px 10px', borderRadius: 3, cursor: 'pointer', fontSize: 12 }}>Edit</button>
                  {e.status !== 'Archived' && (
                    <button onClick={() => handleArchive(e.id)} style={{ background: '#e74c3c', color: '#fff', border: 'none', padding: '4px 10px', borderRadius: 3, cursor: 'pointer', fontSize: 12 }}>Archive</button>
                  )}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {!entries.length && <p style={{ color: '#999', textAlign: 'center', marginTop: 40 }}>No entries found.</p>}
    </div>
  );
}
