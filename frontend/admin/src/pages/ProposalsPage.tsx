import { useEffect, useState } from 'react';
import { adminApi, type Proposal } from '../services/adminApi';

const STATUS_COLORS: Record<string, string> = { Pending: '#e67e22', Accepted: '#2ecc71', EditedAndAccepted: '#27ae60', Rejected: '#e74c3c' };
const RING_COLORS: Record<string, string> = { Adopt: '#2ecc71', Trial: '#3498db', Assess: '#e67e22', Hold: '#e74c3c' };
const QUADRANTS = ['ConnectivityProtocols', 'EdgePlatforms', 'ToolsAndFrameworks', 'StandardsAndTechniques'];
const RINGS = ['Adopt', 'Trial', 'Assess', 'Hold'];

function ProposalReview({ proposal, onDone }: { proposal: Proposal; onDone: () => void }) {
  const [quad, setQuad] = useState(proposal.recommendedQuadrant ?? '');
  const [ring, setRing] = useState(proposal.recommendedRing ?? '');
  const [rationale, setRationale] = useState(proposal.evidenceSummary ?? '');
  const [rejectReason, setRejectReason] = useState('');
  const [mode, setMode] = useState<'view' | 'reject'>('view');
  const [working, setWorking] = useState(false);
  const [error, setError] = useState('');

  const accept = async () => {
    if (!proposal.isLlmEnriched && (!quad || !ring || !rationale)) {
      setError('Quadrant, Ring, and Rationale are required.'); return;
    }
    setWorking(true); setError('');
    try {
      const overrides = (!proposal.isLlmEnriched || quad !== proposal.recommendedQuadrant || ring !== proposal.recommendedRing || rationale !== proposal.evidenceSummary)
        ? { quadrant: quad || undefined, ring: ring || undefined, rationale: rationale || undefined }
        : undefined;
      await adminApi.acceptProposal(proposal.id, overrides);
      onDone();
    } catch (err) { setError((err as Error).message); }
    finally { setWorking(false); }
  };

  const reject = async () => {
    setWorking(true); setError('');
    try { await adminApi.rejectProposal(proposal.id, rejectReason); onDone(); }
    catch (err) { setError((err as Error).message); }
    finally { setWorking(false); }
  };

  const inp = { width: '100%', padding: '7px 10px', border: '1px solid #ddd', borderRadius: 4, boxSizing: 'border-box' as const, marginTop: 3 };

  return (
    <div style={{ background: '#f9f9f9', border: '1px solid #ddd', borderRadius: 8, padding: 24, marginBottom: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between' }}>
        <h3 style={{ marginTop: 0 }}>{proposal.proposedName}</h3>
        <button onClick={onDone} style={{ background: 'none', border: 'none', fontSize: 18, cursor: 'pointer' }}>\u2715</button>
      </div>
      {error && <div style={{ background: '#ffeaea', color: '#c0392b', padding: 10, borderRadius: 4, marginBottom: 12 }}>{error}</div>}

      {!proposal.isLlmEnriched && (
        <div style={{ background: '#fff3cd', border: '1px solid #ffc107', borderRadius: 4, padding: 10, marginBottom: 16 }}>
          Manual classification required \u2014 LLM enrichment was not available when this proposal was created.
        </div>
      )}

      {proposal.evidenceSummary && <p style={{ color: '#555', lineHeight: 1.6 }}>{proposal.evidenceSummary}</p>}

      {proposal.sourceReferences?.length > 0 && (
        <div style={{ marginBottom: 16 }}>
          <strong>Sources:</strong>
          <ul style={{ margin: '4px 0', paddingLeft: 20 }}>
            {proposal.sourceReferences.map((ref, i) => (
              <li key={i}><a href={ref.url} target="_blank" rel="noreferrer">{ref.title}</a></li>
            ))}
          </ul>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12, marginBottom: 16 }}>
        <label>Quadrant {!proposal.isLlmEnriched && '*'}
          <select value={quad} onChange={e => setQuad(e.target.value)} style={inp} required={!proposal.isLlmEnriched}>
            <option value="">\u2014 Select \u2014</option>
            {QUADRANTS.map(q => <option key={q} value={q}>{q.replace(/([A-Z])/g, ' $1').trim()}</option>)}
          </select>
        </label>
        <label>Ring {!proposal.isLlmEnriched && '*'}
          <select value={ring} onChange={e => setRing(e.target.value)} style={inp} required={!proposal.isLlmEnriched}>
            <option value="">\u2014 Select \u2014</option>
            {RINGS.map(r => (
              <option key={r} value={r} style={{ background: RING_COLORS[r] }}>{r}</option>
            ))}
          </select>
        </label>
      </div>

      <label>Rationale / Evidence {!proposal.isLlmEnriched && '*'}
        <textarea value={rationale} onChange={e => setRationale(e.target.value)} rows={3}
          style={{ ...inp, resize: 'vertical' }} required={!proposal.isLlmEnriched} />
      </label>

      {mode === 'view' ? (
        <div style={{ display: 'flex', gap: 8, marginTop: 16 }}>
          <button onClick={accept} disabled={working || (!proposal.isLlmEnriched && (!quad || !ring || !rationale))}
            style={{ background: '#2ecc71', color: '#fff', border: 'none', padding: '8px 20px', borderRadius: 4, cursor: 'pointer' }}>
            Accept
          </button>
          <button onClick={() => setMode('reject')} style={{ background: '#e74c3c', color: '#fff', border: 'none', padding: '8px 20px', borderRadius: 4, cursor: 'pointer' }}>
            Reject
          </button>
        </div>
      ) : (
        <div style={{ marginTop: 16 }}>
          <label>Rejection reason (optional)
            <textarea value={rejectReason} onChange={e => setRejectReason(e.target.value)} rows={2}
              style={{ ...inp, resize: 'vertical' }} />
          </label>
          <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
            <button onClick={reject} disabled={working} style={{ background: '#e74c3c', color: '#fff', border: 'none', padding: '8px 20px', borderRadius: 4, cursor: 'pointer' }}>Confirm Reject</button>
            <button onClick={() => setMode('view')} style={{ background: '#eee', border: 'none', padding: '8px 20px', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
          </div>
        </div>
      )}
    </div>
  );
}

export function ProposalsPage() {
  const [proposals, setProposals] = useState<Proposal[]>([]);
  const [statusFilter, setStatusFilter] = useState('Pending');
  const [selected, setSelected] = useState<Proposal | null>(null);

  const reload = () => adminApi.getProposals(statusFilter || undefined).then(setProposals);
  useEffect(() => { reload(); setSelected(null); }, [statusFilter]);

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ margin: 0 }}>Proposals</h2>
        <div style={{ display: 'flex', gap: 8 }}>
          {['', 'Pending', 'Accepted', 'EditedAndAccepted', 'Rejected'].map(s => (
            <button key={s} onClick={() => setStatusFilter(s)}
              style={{ background: statusFilter === s ? '#1a1a2e' : '#eee', color: statusFilter === s ? '#fff' : '#333', border: 'none', padding: '6px 12px', borderRadius: 4, cursor: 'pointer', fontSize: 13 }}>
              {s || 'All'}
            </button>
          ))}
        </div>
      </div>

      {selected && <ProposalReview proposal={selected} onDone={() => { setSelected(null); reload(); }} />}

      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 14 }}>
        <thead>
          <tr style={{ background: '#f5f5f5' }}>
            {['Technology', 'Quadrant', 'Ring', 'Type', 'Status', 'Detected'].map(h => (
              <th key={h} style={{ padding: '10px 12px', textAlign: 'left', borderBottom: '2px solid #ddd' }}>{h}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {proposals.map(p => (
            <tr key={p.id} onClick={() => setSelected(p)} style={{ borderBottom: '1px solid #eee', cursor: 'pointer' }}>
              <td style={{ padding: '10px 12px', fontWeight: 600 }}>
                {p.proposedName}
                {p.isStale && <span style={{ marginLeft: 6, fontSize: 10, color: '#e67e22' }}>stale</span>}
                {!p.isLlmEnriched && <span style={{ marginLeft: 6, fontSize: 10, color: '#888' }}>manual</span>}
              </td>
              <td style={{ padding: '10px 12px', color: '#666' }}>{p.recommendedQuadrant?.replace(/([A-Z])/g, ' $1').trim() ?? '\u2014'}</td>
              <td style={{ padding: '10px 12px' }}>
                {p.recommendedRing
                  ? <span style={{ background: RING_COLORS[p.recommendedRing] ?? '#999', color: '#fff', padding: '2px 8px', borderRadius: 4, fontSize: 12 }}>{p.recommendedRing}</span>
                  : <span style={{ color: '#999' }}>\u2014</span>}
              </td>
              <td style={{ padding: '10px 12px', color: '#666' }}>{p.proposalType}</td>
              <td style={{ padding: '10px 12px' }}>
                <span style={{ background: STATUS_COLORS[p.status] ?? '#999', color: '#fff', padding: '2px 8px', borderRadius: 4, fontSize: 12 }}>{p.status}</span>
              </td>
              <td style={{ padding: '10px 12px', color: '#888' }}>{new Date(p.detectedAt).toLocaleDateString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {!proposals.length && <p style={{ color: '#999', textAlign: 'center', marginTop: 40 }}>No proposals found.</p>}
    </div>
  );
}
