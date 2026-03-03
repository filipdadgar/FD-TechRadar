import { useEffect, useState } from 'react';
import { adminApi, type DataSource, type SourceInput } from '../services/adminApi';

type SourceType = 'RssFeed' | 'GitHubTopics';

function buildConnectionDetails(type: SourceType, url: string, topics: string, minStars: number, maxAgeDays: number) {
  if (type === 'RssFeed') return JSON.stringify({ url });
  return JSON.stringify({ topics: topics.split(',').map(t => t.trim()).filter(Boolean), minStars, maxAgeDays });
}

export function SourcesPage() {
  const [sources, setSources] = useState<DataSource[]>([]);
  const [editing, setEditing] = useState<DataSource | 'new' | null>(null);
  const [form, setForm] = useState({ name: '', type: 'RssFeed' as SourceType, url: '', topics: '', minStars: 50, maxAgeDays: 180, enabled: true });
  const [error, setError] = useState('');

  const reload = () => adminApi.getSources().then(setSources);
  useEffect(() => { reload(); }, []);

  const startEdit = (s: DataSource) => {
    const details = s.connectionDetails as Record<string, unknown>;
    setForm({
      name: s.name, type: s.sourceType as SourceType, enabled: s.enabled,
      url: (details.url as string) ?? '',
      topics: Array.isArray(details.topics) ? (details.topics as string[]).join(', ') : '',
      minStars: (details.minStars as number) ?? 50,
      maxAgeDays: (details.maxAgeDays as number) ?? 180
    });
    setEditing(s);
  };

  const startNew = () => {
    setForm({ name: '', type: 'RssFeed', url: '', topics: '', minStars: 50, maxAgeDays: 180, enabled: true });
    setEditing('new');
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault(); setError('');
    const input: SourceInput = {
      name: form.name, sourceType: form.type,
      connectionDetails: buildConnectionDetails(form.type, form.url, form.topics, form.minStars, form.maxAgeDays),
      enabled: form.enabled
    };
    try {
      if (editing === 'new') await adminApi.createSource(input);
      else if (editing) await adminApi.updateSource(editing.id, input);
      setEditing(null); reload();
    } catch (err) { setError((err as Error).message); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this source?')) return;
    await adminApi.deleteSource(id); reload();
  };

  const inp = { width: '100%', padding: '7px 10px', border: '1px solid #ddd', borderRadius: 4, boxSizing: 'border-box' as const, marginTop: 3 };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ margin: 0 }}>Data Sources</h2>
        <button onClick={startNew} style={{ background: '#1a1a2e', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 4, cursor: 'pointer' }}>+ Add Source</button>
      </div>

      {editing && (
        <div style={{ background: '#f9f9f9', border: '1px solid #ddd', borderRadius: 8, padding: 24, marginBottom: 24 }}>
          <h3 style={{ marginTop: 0 }}>{editing === 'new' ? 'Add Source' : `Edit: ${(editing as DataSource).name}`}</h3>
          {error && <div style={{ background: '#ffeaea', color: '#c0392b', padding: 10, borderRadius: 4, marginBottom: 12 }}>{error}</div>}
          <form onSubmit={handleSave} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            <label>Name *<input value={form.name} onChange={e => setForm(p => ({ ...p, name: e.target.value }))} required style={inp} /></label>
            <label>Source Type
              <select value={form.type} onChange={e => setForm(p => ({ ...p, type: e.target.value as SourceType }))} style={inp}>
                <option value="RssFeed">RSS Feed</option>
                <option value="GitHubTopics">GitHub Topics</option>
              </select>
            </label>
            {form.type === 'RssFeed' && (
              <label>Feed URL *<input value={form.url} onChange={e => setForm(p => ({ ...p, url: e.target.value }))} required style={inp} placeholder="https://..." /></label>
            )}
            {form.type === 'GitHubTopics' && (
              <>
                <label>Topics (comma-separated) *<input value={form.topics} onChange={e => setForm(p => ({ ...p, topics: e.target.value }))} required style={inp} placeholder="mqtt, zigbee, lorawan" /></label>
                <label>Min Stars<input type="number" value={form.minStars} onChange={e => setForm(p => ({ ...p, minStars: Number(e.target.value) }))} style={inp} /></label>
                <label>Max Age (days)<input type="number" value={form.maxAgeDays} onChange={e => setForm(p => ({ ...p, maxAgeDays: Number(e.target.value) }))} style={inp} /></label>
              </>
            )}
            <label style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <input type="checkbox" checked={form.enabled} onChange={e => setForm(p => ({ ...p, enabled: e.target.checked }))} /> Enabled
            </label>
            <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
              <button type="submit" style={{ background: '#1a1a2e', color: '#fff', border: 'none', padding: '8px 20px', borderRadius: 4, cursor: 'pointer' }}>Save</button>
              <button type="button" onClick={() => setEditing(null)} style={{ background: '#eee', border: 'none', padding: '8px 20px', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
            </div>
          </form>
        </div>
      )}

      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 14 }}>
        <thead>
          <tr style={{ background: '#f5f5f5' }}>
            {['Name', 'Type', 'Enabled', 'Last Scan', 'Actions'].map(h => (
              <th key={h} style={{ padding: '10px 12px', textAlign: 'left', borderBottom: '2px solid #ddd' }}>{h}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {sources.map(s => (
            <tr key={s.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '10px 12px', fontWeight: 600 }}>{s.name}</td>
              <td style={{ padding: '10px 12px' }}>{s.sourceType}</td>
              <td style={{ padding: '10px 12px', color: s.enabled ? '#27ae60' : '#e74c3c' }}>{s.enabled ? 'Yes' : 'No'}</td>
              <td style={{ padding: '10px 12px', color: '#888' }}>{s.lastSuccessfulScanAt ? new Date(s.lastSuccessfulScanAt).toLocaleString() : '\u2014'}</td>
              <td style={{ padding: '10px 12px' }}>
                <div style={{ display: 'flex', gap: 6 }}>
                  <button onClick={() => startEdit(s)} style={{ background: '#3498db', color: '#fff', border: 'none', padding: '4px 10px', borderRadius: 3, cursor: 'pointer', fontSize: 12 }}>Edit</button>
                  <button onClick={() => handleDelete(s.id)} style={{ background: '#e74c3c', color: '#fff', border: 'none', padding: '4px 10px', borderRadius: 3, cursor: 'pointer', fontSize: 12 }}>Delete</button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {!sources.length && <p style={{ color: '#999', textAlign: 'center', marginTop: 40 }}>No sources configured.</p>}
    </div>
  );
}
