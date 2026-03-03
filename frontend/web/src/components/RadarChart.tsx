import { useMemo } from 'react';
import type { RadarState, Entry } from '../services/api';

interface Props {
  data: RadarState;
  onEntryClick: (entry: Entry) => void;
}

// SVG radar dimensions
const CX = 290;
const CY = 290;

const RING_RADII: Record<string, number> = {
  Adopt: 65,
  Trial: 130,
  Assess: 200,
  Hold: 270,
};

const RING_ORDER = ['Adopt', 'Trial', 'Assess', 'Hold'] as const;
type RingName = typeof RING_ORDER[number];

// Quadrant angle ranges in SVG space (y-axis points down)
// top-left  = angles π   → 3π/2  (cos<0, sin<0)
// top-right = angles 3π/2 → 2π   (cos>0, sin<0)
// bot-left  = angles π/2  → π    (cos<0, sin>0)
// bot-right = angles 0    → π/2  (cos>0, sin>0)
const QUADRANT_ANGLES: Record<string, [number, number]> = {
  ConnectivityProtocols:  [Math.PI,           (3 * Math.PI) / 2],
  EdgePlatforms:          [(3 * Math.PI) / 2, 2 * Math.PI],
  ToolsAndFrameworks:     [Math.PI / 2,       Math.PI],
  StandardsAndTechniques: [0,                 Math.PI / 2],
};

const QUADRANT_COLORS: Record<string, string> = {
  ConnectivityProtocols:  '#9b59b6',
  EdgePlatforms:          '#1abc9c',
  ToolsAndFrameworks:     '#e74c3c',
  StandardsAndTechniques: '#f39c12',
};

const QUADRANT_LABELS: Record<string, string> = {
  ConnectivityProtocols:  'Connectivity Protocols',
  EdgePlatforms:          'Edge Platforms',
  ToolsAndFrameworks:     'Tools & Frameworks',
  StandardsAndTechniques: 'Standards & Techniques',
};

// Ring appearance: outer rings lighter, inner (Adopt) slightly darker
const RING_FILL: Record<string, string> = {
  Hold:   '#f9f9f9',
  Assess: '#f2f2f2',
  Trial:  '#ebebeb',
  Adopt:  '#e3e3e3',
};

function hashStr(s: string): number {
  let h = 0;
  for (let i = 0; i < s.length; i++) {
    h = (Math.imul(31, h) + s.charCodeAt(i)) | 0;
  }
  return Math.abs(h);
}

function seededRand(seed: number): number {
  const x = Math.sin(seed + 1) * 10000;
  return x - Math.floor(x);
}

function getDotPos(entry: Entry, quadrant: string): { x: number; y: number } {
  const h = hashStr(entry.id + entry.name);
  const [a0, a1] = QUADRANT_ANGLES[quadrant];
  // Keep dots away from the dividing axes (10% margin each side)
  const angle = a0 + (0.1 + seededRand(h) * 0.8) * (a1 - a0);
  const ri = RING_ORDER.indexOf(entry.ring as RingName);
  const innerR = ri <= 0 ? 4 : RING_RADII[RING_ORDER[ri - 1]];
  const outerR = RING_RADII[entry.ring as RingName] ?? RING_RADII.Hold;
  const band = outerR - innerR;
  // Keep dots away from ring boundary lines (15% margin each side)
  const r = innerR + band * (0.15 + seededRand(h + 17) * 0.7);
  return { x: CX + r * Math.cos(angle), y: CY + r * Math.sin(angle) };
}

interface LegendPanelProps {
  quadrant: string;
  entries: Entry[];
  separator: boolean;
  onEntryClick: (e: Entry) => void;
}

function LegendPanel({ quadrant, entries, separator, onEntryClick }: LegendPanelProps) {
  const color = QUADRANT_COLORS[quadrant];
  return (
    <div style={{
      flex: 1,
      padding: '16px 14px',
      borderTop: separator ? '1px solid #eee' : undefined,
      minHeight: 0,
      overflow: 'auto',
    }}>
      <div style={{
        fontSize: 11,
        fontWeight: 700,
        color,
        marginBottom: 10,
        textTransform: 'uppercase',
        letterSpacing: '0.6px',
      }}>
        {QUADRANT_LABELS[quadrant]}
      </div>
      {entries.length === 0 ? (
        <div style={{ color: '#bbb', fontSize: 11, fontStyle: 'italic' }}>No entries</div>
      ) : (
        entries.map(e => (
          <div
            key={e.id}
            onClick={() => onEntryClick(e)}
            style={{ display: 'flex', alignItems: 'center', gap: 7, cursor: 'pointer', marginBottom: 6 }}
          >
            <div style={{ width: 8, height: 8, borderRadius: '50%', background: color, flexShrink: 0 }} />
            <span style={{ fontSize: 12, color: '#444', lineHeight: 1.3 }}>{e.name}</span>
          </div>
        ))
      )}
    </div>
  );
}

export function RadarChart({ data, onEntryClick }: Props) {
  const allDots = useMemo(() =>
    Object.keys(QUADRANT_ANGLES).flatMap(q =>
      (data[q] ?? []).map(e => ({ entry: e, quadrant: q, pos: getDotPos(e, q) }))
    ),
    [data],
  );

  return (
    <div style={{
      display: 'flex',
      background: '#fff',
      borderRadius: 8,
      boxShadow: '0 2px 12px rgba(0,0,0,0.08)',
      overflow: 'hidden',
    }}>
      {/* Left legend: ConnectivityProtocols (top-left) + ToolsAndFrameworks (bottom-left) */}
      <div style={{ width: 200, display: 'flex', flexDirection: 'column', borderRight: '1px solid #eee' }}>
        <LegendPanel
          quadrant="ConnectivityProtocols"
          entries={data.ConnectivityProtocols ?? []}
          separator={false}
          onEntryClick={onEntryClick}
        />
        <LegendPanel
          quadrant="ToolsAndFrameworks"
          entries={data.ToolsAndFrameworks ?? []}
          separator
          onEntryClick={onEntryClick}
        />
      </div>

      {/* Radar SVG */}
      <svg viewBox="0 0 580 580" style={{ flex: '0 0 580px', display: 'block' }}>
        {/* Ring fills — draw outer to inner so inner overwrites outer */}
        {([...RING_ORDER].reverse() as RingName[]).map(ring => (
          <circle
            key={ring}
            cx={CX}
            cy={CY}
            r={RING_RADII[ring]}
            fill={RING_FILL[ring]}
            stroke="#d0d0d0"
            strokeWidth={0.8}
          />
        ))}

        {/* Quadrant dividing lines (clipped to outermost ring) */}
        <line x1={CX - RING_RADII.Hold} y1={CY} x2={CX + RING_RADII.Hold} y2={CY} stroke="#ccc" strokeWidth={1} />
        <line x1={CX} y1={CY - RING_RADII.Hold} x2={CX} y2={CY + RING_RADII.Hold} stroke="#ccc" strokeWidth={1} />

        {/* Ring labels along the top vertical axis */}
        {RING_ORDER.map(ring => {
          const ri = RING_ORDER.indexOf(ring);
          const innerR = ri === 0 ? 0 : RING_RADII[RING_ORDER[ri - 1]];
          const outerR = RING_RADII[ring];
          const labelY = CY - (innerR + outerR) / 2;
          return (
            <text
              key={ring}
              x={CX + 5}
              y={labelY + 4}
              fontSize={10}
              fill="#999"
              fontFamily="sans-serif"
              fontStyle="italic"
            >
              {ring}
            </text>
          );
        })}

        {/* Entry dots */}
        {allDots.map(({ entry, quadrant, pos }) => (
          <g key={entry.id} onClick={() => onEntryClick(entry)} style={{ cursor: 'pointer' }}>
            <circle
              cx={pos.x}
              cy={pos.y}
              r={7}
              fill={QUADRANT_COLORS[quadrant]}
              opacity={0.85}
            />
            <title>{entry.name} · {entry.ring}</title>
          </g>
        ))}
      </svg>

      {/* Right legend: EdgePlatforms (top-right) + StandardsAndTechniques (bottom-right) */}
      <div style={{ width: 200, display: 'flex', flexDirection: 'column', borderLeft: '1px solid #eee' }}>
        <LegendPanel
          quadrant="EdgePlatforms"
          entries={data.EdgePlatforms ?? []}
          separator={false}
          onEntryClick={onEntryClick}
        />
        <LegendPanel
          quadrant="StandardsAndTechniques"
          entries={data.StandardsAndTechniques ?? []}
          separator
          onEntryClick={onEntryClick}
        />
      </div>
    </div>
  );
}
