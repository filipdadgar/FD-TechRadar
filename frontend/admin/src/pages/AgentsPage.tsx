import { useEffect, useState } from 'react';
import { adminApi, type AgentRun } from '../services/adminApi';

const STATUS_COLORS: Record<string, string> = { Running: '#3498db', Completed: '#2ecc71', Failed: '#e74c3c' };

export function AgentsPage() {
  const [runs, setRuns] = useState<AgentRun[]>([]);
  const [triggering, setTriggering] = useState(false);
  const [message, setMessage] = useState('');

  const reload = () => adminApi.getAgentRuns(20).then(r => setRuns(r.runs));
  useEffect(() => { reload(); }, []);

  const triggerScan = async () => {
    setTriggering(true); setMessage('');
    try {
      const res = await adminApi.triggerScan();
      setMessage(res.message ?? 'Scan triggered!');
      setTimeout(() => { reload(); setMessage(''); }, 3000);
    } catch (err) { setMessage(`Error: ${(err as Error).message}`); }
    finally { setTimeout(() => setTriggering(false), 5000); }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ margin: 0 }}>Agent Runs</h2>
        <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
          {message && <span style={{ color: '#27ae60', fontSize: 14 }}>{message}</span>}
          <button onClick={triggerScan} disabled={triggering}
            style={{ background: triggering ? '#95a5a6' : '#1a1a2e', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 4, cursor: triggering ? 'not-allowed' : 'pointer' }}>
            {triggering ? 'Scan triggered\u2026' : 'Trigger Scan Now'}
          </button>
        </div>
      </div>
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 14 }}>
        <thead>
          <tr style={{ background: '#f5f5f5' }}>
            {['Started', 'Trigger', 'Status', 'Sources', 'Signals', 'Proposals', 'Errors'].map(h => (
              <th key={h} style={{ padding: '10px 12px', textAlign: 'left', borderBottom: '2px solid #ddd' }}>{h}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {runs.map(r => (
            <tr key={r.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '10px 12px' }}>{new Date(r.startedAt).toLocaleString()}</td>
              <td style={{ padding: '10px 12px', textTransform: 'capitalize' }}>{r.triggerType}</td>
              <td style={{ padding: '10px 12px' }}>
                <span style={{ background: STATUS_COLORS[r.status] ?? '#999', color: '#fff', padding: '2px 8px', borderRadius: 4, fontSize: 12 }}>{r.status}</span>
              </td>
              <td style={{ padding: '10px 12px', textAlign: 'center' }}>{r.sourcesScanned}</td>
              <td style={{ padding: '10px 12px', textAlign: 'center' }}>{r.signalsFound}</td>
              <td style={{ padding: '10px 12px', textAlign: 'center' }}>{r.proposalsGenerated}</td>
              <td style={{ padding: '10px 12px', textAlign: 'center', color: r.errorCount > 0 ? '#e74c3c' : '#27ae60' }}>{r.errorCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {!runs.length && <p style={{ color: '#999', textAlign: 'center', marginTop: 40 }}>No agent runs yet.</p>}
    </div>
  );
}
