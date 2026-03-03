const BASE = '/api';

function getToken() { return localStorage.getItem('auth_token') ?? ''; }

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${getToken()}`,
      ...(options?.headers ?? {}),
    },
  });
  if (res.status === 401) { localStorage.removeItem('auth_token'); window.location.href = '/admin/'; }
  if (!res.ok) {
    const body = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(body.error ?? `HTTP ${res.status}`);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const adminApi = {
  // Auth
  login: (username: string, password: string) =>
    fetch(`${BASE}/admin/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password }),
    }).then(async r => {
      if (!r.ok) throw new Error('Invalid credentials');
      return r.json() as Promise<{ token: string; expiresAt: string }>;
    }),

  // Entries
  getEntries: (status?: string) =>
    request<Entry[]>(`/admin/entries${status ? `?status=${status}` : ''}`),
  createEntry: (data: EntryInput) =>
    request<Entry>('/admin/entries', { method: 'POST', body: JSON.stringify(data) }),
  updateEntry: (id: string, data: EntryInput) =>
    request<Entry>(`/admin/entries/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  archiveEntry: (id: string) =>
    request<void>(`/admin/entries/${id}`, { method: 'DELETE' }),

  // Sources
  getSources: () => request<DataSource[]>('/admin/sources'),
  createSource: (data: SourceInput) =>
    request<DataSource>('/admin/sources', { method: 'POST', body: JSON.stringify(data) }),
  updateSource: (id: string, data: SourceInput) =>
    request<DataSource>(`/admin/sources/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  deleteSource: (id: string) =>
    request<void>(`/admin/sources/${id}`, { method: 'DELETE' }),

  // Agent
  triggerScan: () =>
    request<{ message: string }>('/admin/agents/trigger', { method: 'POST', body: '{}' }),
  getAgentRuns: (limit = 20) =>
    request<{ runs: AgentRun[] }>(`/admin/agent-runs?limit=${limit}`),
  getAgentRun: (id: string) => request<AgentRunDetail>(`/admin/agent-runs/${id}`),

  // Proposals
  getProposals: (status?: string, staleOnly?: boolean) => {
    const params = new URLSearchParams();
    if (status) params.set('status', status);
    if (staleOnly) params.set('staleOnly', 'true');
    const qs = params.toString() ? `?${params}` : '';
    return request<Proposal[]>(`/admin/proposals${qs}`);
  },
  acceptProposal: (id: string, overrides?: Record<string, unknown>) =>
    request<Proposal>(`/admin/proposals/${id}/accept`, {
      method: 'POST', body: JSON.stringify(overrides ?? {})
    }),
  rejectProposal: (id: string, reason?: string) =>
    request<Proposal>(`/admin/proposals/${id}/reject`, {
      method: 'POST', body: JSON.stringify({ reason })
    }),

  // Snapshots
  getSnapshots: () => request<Snapshot[]>('/radar/snapshots'),
  getSnapshot: (id: string) => request<SnapshotDetail>(`/radar/snapshots/${id}`),
  compareSnapshots: (fromId: string, toId: string) =>
    request<SnapshotDiff>(`/radar/snapshots/compare?from=${fromId}&to=${toId}`),
};

export interface Entry { id: string; name: string; quadrant: string; ring: string; description: string; rationale: string; tags: string[]; status: string; lastReviewedAt: string; }
export interface EntryInput { name: string; description: string; rationale: string; quadrant: string; ring: string; tags?: string[]; changeReason?: string; }
export interface DataSource { id: string; name: string; sourceType: string; connectionDetails: unknown; enabled: boolean; lastSuccessfulScanAt: string | null; }
export interface SourceInput { name: string; sourceType: string; connectionDetails: string; enabled?: boolean; }
export interface AgentRun { id: string; startedAt: string; completedAt: string | null; triggerType: string; status: string; sourcesScanned: number; signalsFound: number; proposalsGenerated: number; proposalsDropped: number; errorCount: number; }
export interface AgentRunDetail extends AgentRun { errors: Array<{sourceId: string; message: string; occurredAt: string}>; }
export interface Proposal { id: string; proposedName: string; recommendedQuadrant: string | null; recommendedRing: string | null; evidenceSummary: string | null; sourceReferences: Array<{title: string; url: string; publishedAt: string}>; isLlmEnriched: boolean; proposalType: string; status: string; detectedAt: string; isStale: boolean; reviewerNotes: string | null; resultingEntryId: string | null; }
export interface Snapshot { id: string; capturedAt: string; triggerEvent: string; entryCount: number; }
export interface SnapshotDetail { id: string; capturedAt: string; triggerEvent: string; entries: Record<string, Array<{id: string; name: string; quadrant: string; ring: string}>>; }
export interface SnapshotDiff { fromSnapshotId: string; fromCapturedAt: string; toSnapshotId: string; toCapturedAt: string; added: SnapshotEntry[]; removed: SnapshotEntry[]; moved: MovedEntry[]; unchangedCount: number; }
export interface SnapshotEntry { id: string; name: string; quadrant: string; ring: string; description: string; rationale: string; }
export interface MovedEntry { name: string; fromQuadrant: string; toQuadrant: string; fromRing: string; toRing: string; }
