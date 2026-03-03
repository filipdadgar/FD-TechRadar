const BASE = '/api';

export interface Entry {
  id: string;
  name: string;
  quadrant: string;
  ring: string;
  description: string;
  rationale: string;
  tags: string[];
  lastReviewedAt: string;
}

export interface EntryDetail extends Entry {
  createdAt: string;
  ringHistory: RingHistoryItem[];
}

export interface RingHistoryItem {
  changedAt: string;
  changedBy: string;
  previousRing: string | null;
  newRing: string;
  previousQuadrant: string | null;
  newQuadrant: string;
  changeReason: string | null;
}

export type RadarState = Record<string, Entry[]>;

async function request<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE}${path}`);
  if (!res.ok) throw new Error(`API error ${res.status}: ${path}`);
  return res.json();
}

export const api = {
  getCurrentRadar: () => request<RadarState>('/radar/current'),
  getEntries: (filters?: { quadrant?: string; ring?: string; tag?: string }) => {
    const params = new URLSearchParams();
    if (filters?.quadrant) params.set('quadrant', filters.quadrant);
    if (filters?.ring) params.set('ring', filters.ring);
    if (filters?.tag) params.set('tag', filters.tag);
    const qs = params.toString() ? `?${params}` : '';
    return request<Entry[]>(`/radar/entries${qs}`);
  },
  getEntry: (id: string) => request<EntryDetail>(`/radar/entries/${id}`),
};
