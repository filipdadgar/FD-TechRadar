import { useState } from 'react';
import { useAuth } from './hooks/useAuth';
import { LoginPage } from './pages/LoginPage';
import { EntriesPage } from './pages/EntriesPage';
import { AgentsPage } from './pages/AgentsPage';
import { SourcesPage } from './pages/SourcesPage';
import { ProposalsPage } from './pages/ProposalsPage';
import { HistoryPage } from './pages/HistoryPage';

type Page = 'entries' | 'proposals' | 'agents' | 'sources' | 'history';

const NAV: { id: Page; label: string }[] = [
  { id: 'entries', label: 'Entries' },
  { id: 'proposals', label: 'Proposals' },
  { id: 'agents', label: 'Agents' },
  { id: 'sources', label: 'Sources' },
  { id: 'history', label: 'History' },
];

export function App() {
  const { isAuthenticated, login, logout } = useAuth();
  const [page, setPage] = useState<Page>('entries');

  if (!isAuthenticated) return <LoginPage onLogin={login} />;

  return (
    <div style={{ minHeight: '100vh', background: '#f0f2f5', display: 'flex', flexDirection: 'column' }}>
      <header style={{ background: '#1a1a2e', color: '#fff', padding: '0 24px', display: 'flex', alignItems: 'center', height: 56 }}>
        <span style={{ fontWeight: 700, fontSize: 18, marginRight: 32 }}>IoT Radar Admin</span>
        <nav style={{ display: 'flex', gap: 4 }}>
          {NAV.map(n => (
            <button key={n.id} onClick={() => setPage(n.id)}
              style={{
                background: page === n.id ? 'rgba(255,255,255,0.15)' : 'transparent',
                color: '#fff', border: 'none', padding: '16px 14px',
                cursor: 'pointer', borderBottom: page === n.id ? '2px solid #fff' : '2px solid transparent',
                fontSize: 14
              }}>{n.label}</button>
          ))}
        </nav>
        <div style={{ marginLeft: 'auto', display: 'flex', gap: 16, alignItems: 'center' }}>
          <a href="/" style={{ color: '#aaa', fontSize: 13 }}>\u2190 Public View</a>
          <button onClick={logout} style={{ background: 'rgba(255,255,255,0.1)', color: '#fff', border: '1px solid rgba(255,255,255,0.3)', padding: '6px 14px', borderRadius: 4, cursor: 'pointer', fontSize: 13 }}>
            Sign Out
          </button>
        </div>
      </header>

      <main style={{ flex: 1, padding: 32, maxWidth: 1200, margin: '0 auto', width: '100%', boxSizing: 'border-box' }}>
        {page === 'entries' && <EntriesPage />}
        {page === 'proposals' && <ProposalsPage />}
        {page === 'agents' && <AgentsPage />}
        {page === 'sources' && <SourcesPage />}
        {page === 'history' && <HistoryPage />}
      </main>
    </div>
  );
}

export default App;
