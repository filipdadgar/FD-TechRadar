import { useEffect, useState } from 'react';
import { api } from './services/api';
import type { Entry, EntryDetail, RadarState } from './services/api';
import { RadarChart } from './components/RadarChart';
import { EntryList } from './components/EntryList';
import { EntryDetailPanel } from './components/EntryDetail';

type View = 'radar' | 'list';

export function App() {
  const [view, setView] = useState<View>('radar');
  const [radarData, setRadarData] = useState<RadarState>({});
  const [allEntries, setAllEntries] = useState<Entry[]>([]);
  const [selectedEntry, setSelectedEntry] = useState<EntryDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([api.getCurrentRadar(), api.getEntries()])
      .then(([radar, entries]) => {
        setRadarData(radar);
        setAllEntries(entries);
        setLoading(false);
      })
      .catch(err => {
        setError((err as Error).message);
        setLoading(false);
      });
  }, []);

  const handleEntryClick = async (entry: Entry) => {
    try {
      const detail = await api.getEntry(entry.id);
      setSelectedEntry(detail);
    } catch {
      setSelectedEntry({ ...entry, createdAt: entry.lastReviewedAt, ringHistory: [] });
    }
  };

  return (
    <div style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <header style={{
        background: '#1a1a2e', color: '#fff', padding: '16px 32px',
        display: 'flex', alignItems: 'center', gap: 24
      }}>
        <h1 style={{ margin: 0, fontSize: 22 }}>IoT Tech Radar</h1>
        <nav style={{ display: 'flex', gap: 16 }}>
          {(['radar', 'list'] as View[]).map(v => (
            <button key={v} onClick={() => setView(v)} style={{
              background: view === v ? '#fff' : 'transparent',
              color: view === v ? '#1a1a2e' : '#fff',
              border: '1px solid rgba(255,255,255,0.4)',
              padding: '6px 16px', borderRadius: 4, cursor: 'pointer',
              textTransform: 'capitalize'
            }}>{v === 'radar' ? 'Radar View' : 'List View'}</button>
          ))}
        </nav>
        <a href="/admin" style={{ marginLeft: 'auto', color: '#aaa', fontSize: 14 }}>
          Admin &rarr;
        </a>
      </header>

      <main style={{ padding: 32, maxWidth: 1200, margin: '0 auto' }}>
        {loading && <p>Loading radar data&hellip;</p>}
        {error && <p style={{ color: 'red' }}>Error: {error}</p>}
        {!loading && !error && (
          view === 'radar'
            ? <RadarChart data={radarData} onEntryClick={handleEntryClick} />
            : <EntryList entries={allEntries} onEntryClick={handleEntryClick} />
        )}
      </main>

      {selectedEntry && (
        <EntryDetailPanel entry={selectedEntry} onClose={() => setSelectedEntry(null)} />
      )}
    </div>
  );
}

export default App;
