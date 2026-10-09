
using System.Collections.Generic;
using UnityEngine;

namespace Caso1
{
    public class PrisonNavigation : MonoBehaviour
    {
        public static PrisonNavigation Instance
        {
            get;
            private set;
        }

        [Header("Mapa")]
        public Vector2 mapCenter = Vector2.zero;
        public Vector2 mapSize = new Vector2(22f, 14f);

        [Min(0.15f)]
        public float cellSize = 0.45f;

        [Min(0f)]
        public float clearance = 0.15f;

        [Header("Obstaculos")]
        public LayerMask obstacleMask;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public Vector3 KeepInside(Vector3 p)
        {
            Vector2 half = mapSize * 0.5f;

            p.x = Mathf.Clamp(
                p.x,
                mapCenter.x - half.x + cellSize,
                mapCenter.x + half.x - cellSize
            );

            p.y = Mathf.Clamp(
                p.y,
                mapCenter.y - half.y + cellSize,
                mapCenter.y + half.y - cellSize
            );

            return p;
        }

        private Vector2Int Grid(
            Vector3 p, int nx, int ny)
        {
            Vector2 min =
                mapCenter - mapSize * 0.5f;

            int x = Mathf.Clamp(
                Mathf.FloorToInt(
                    (p.x - min.x) / cellSize),
                0, nx - 1
            );

            int y = Mathf.Clamp(
                Mathf.FloorToInt(
                    (p.y - min.y) / cellSize),
                0, ny - 1
            );

            return new Vector2Int(x, y);
        }

        private Vector3 World(Vector2Int g, float z)
        {
            Vector2 min =
                mapCenter - mapSize * 0.5f;

            return new Vector3(
                min.x + (g.x + 0.5f) * cellSize,
                min.y + (g.y + 0.5f) * cellSize,
                z
            );
        }

        public bool IsFree(Vector3 point)
        {
            return Physics2D.OverlapCircle(
                point,
                Mathf.Max(0.02f, clearance),
                obstacleMask
            ) == null;
        }

        public bool HasLine(Vector3 a, Vector3 b)
        {
            if (!IsFree(b))
                return false;

            Vector2 v = (Vector2)(b - a);
            float d = v.magnitude;

            if (d < 0.01f)
                return true;

            return Physics2D.CircleCast(
                a,
                Mathf.Max(0.02f, clearance),
                v.normalized,
                d,
                obstacleMask
            ).collider == null;
        }

        public List<Vector3> FindPath(
            Vector3 start, Vector3 end)
        {
            List<Vector3> result =
                new List<Vector3>();

            start = KeepInside(start);
            end = KeepInside(end);

            if (HasLine(start, end))
            {
                result.Add(end);
                return result;
            }

            int nx = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    mapSize.x / cellSize)
            );

            int ny = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    mapSize.y / cellSize)
            );

            int count = nx * ny;

            if (count > 18000)
            {
                Debug.LogWarning(
                    "Navigation grid too large; " +
                    "enlarge cellSize."
                );

                return result;
            }

            Vector2Int s = Grid(start, nx, ny);
            Vector2Int g = Grid(end, nx, ny);

            int si = s.y * nx + s.x;
            int gi = g.y * nx + g.x;

            float[] cost = new float[count];
            int[] parent = new int[count];
            bool[] closed = new bool[count];

            for (int i = 0; i < count; i++)
            {
                cost[i] = float.PositiveInfinity;
                parent[i] = -1;
            }

            List<int> open = new List<int> { si };
            cost[si] = 0;

            Vector2Int[] steps =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            while (open.Count > 0)
            {
                int best = 0;
                float bestScore =
                    float.PositiveInfinity;

                for (int i = 0; i < open.Count; i++)
                {
                    int id = open[i];

                    float score =
                        cost[id] +
                        Mathf.Abs(id % nx - g.x) +
                        Mathf.Abs(id / nx - g.y);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = i;
                    }
                }

                int cur = open[best];
                open.RemoveAt(best);

                if (cur == gi)
                {
                    List<Vector3> reverse =
                        new List<Vector3>();

                    int at = cur;

                    while (at != si && at >= 0)
                    {
                        reverse.Add(World(
                            new Vector2Int(
                                at % nx,
                                at / nx
                            ),
                            start.z
                        ));

                        at = parent[at];
                    }

                    reverse.Reverse();
                    result.AddRange(reverse);

                    Vector3 last =
                        result.Count > 0
                        ? result[result.Count - 1]
                        : start;

                    if (HasLine(last, end))
                        result.Add(end);

                    return result;
                }

                closed[cur] = true;

                foreach (var step in steps)
                {
                    int x = cur % nx + step.x;
                    int y = cur / nx + step.y;

                    if (x < 0 || x >= nx ||
                        y < 0 || y >= ny)
                        continue;

                    int next = y * nx + x;

                    Vector3 nextWorld = World(
                        new Vector2Int(x, y),
                        start.z
                    );

                    if (closed[next] ||
                        !IsFree(nextWorld))
                        continue;

                    float trial = cost[cur] + 1;

                    if (trial >= cost[next])
                        continue;

                    cost[next] = trial;
                    parent[next] = cur;

                    if (!open.Contains(next))
                        open.Add(next);
                }
            }

            return result;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireCube(
                mapCenter,
                new Vector3(
                    mapSize.x,
                    mapSize.y,
                    0
                )
            );
        }
    }
}
