using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapSegment : MonoBehaviour
{
    public static MapSegment Instance { get; private set; }

    [Header("Build")]
    [Tooltip("발판 구간으로 분석할 Tilemap 오브젝트의 레이어입니다.")]
    [SerializeField] private LayerMask landingLayer;

    [Tooltip("Awake 시점에 발판 구간 캐시를 자동으로 생성합니다.")]
    [SerializeField] private bool buildOnAwake = true;

    [Tooltip("같은 발판으로 합칠 수 있는 최대 높이 차이입니다. 값이 클수록 약간 다른 높이도 하나로 묶입니다.")]
    [SerializeField] private float surfaceHeightTolerance = 0.08f;

    [Tooltip("서로 이어진 발판 조각을 하나로 합칠 최대 간격입니다. 값이 클수록 가까운 조각이 하나의 구간으로 묶입니다.")]
    [SerializeField] private float mergeGapTolerance = 0.08f;

    [Header("Debug")]
    [Tooltip("씬에 생성된 모든 발판 구간 Gizmo를 표시합니다.")]
    [SerializeField] private bool drawSegmentGizmos = true;

    private readonly List<Segment> segments = new();
    private readonly List<Segment> buildBuffer = new();

    public IReadOnlyList<Segment> Segments => segments;

    private void Awake()
    {
        Instance = this;

        if (buildOnAwake)
            Rebuild();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Rebuild()
    {
        segments.Clear();
        buildBuffer.Clear();

        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap == null || !tilemap.gameObject.activeInHierarchy)
                continue;

            if ((landingLayer.value & (1 << tilemap.gameObject.layer)) == 0)
                continue;

            ExtractSegments(tilemap, buildBuffer);
        }

        MergeSegments(buildBuffer, segments);
    }

    public void GetSegmentsInBounds(Bounds bounds, List<Segment> results)
    {
        if (results == null)
            return;

        foreach (Segment segment in segments)
        {
            if (!segment.Overlaps(bounds))
                continue;

            if (!results.Contains(segment))
                results.Add(segment);
        }
    }

    public bool TryFindSegmentAtPoint(Vector2 point, float heightTolerance, out Segment result)
    {
        result = null;
        float bestHeightDiff = float.MaxValue;

        foreach (Segment segment in segments)
        {
            if (!segment.ContainsX(point.x))
                continue;

            float heightDiff = Mathf.Abs(point.y - segment.y);
            if (heightDiff > heightTolerance)
                continue;

            if (heightDiff < bestHeightDiff)
            {
                bestHeightDiff = heightDiff;
                result = segment;
            }
        }

        return result != null;
    }

    public bool TryFindSegmentOverlappingFootprint(
        float leftX,
        float rightX,
        float y,
        float heightTolerance,
        out Segment result
    )
    {
        result = null;
        float bestHeightDiff = float.MaxValue;

        foreach (Segment segment in segments)
        {
            if (!segment.OverlapsX(leftX, rightX))
                continue;

            float heightDiff = Mathf.Abs(y - segment.y);
            if (heightDiff > heightTolerance)
                continue;

            if (heightDiff < bestHeightDiff)
            {
                bestHeightDiff = heightDiff;
                result = segment;
            }
        }

        return result != null;
    }

    public bool TryFindHighestSegmentBelowPoint(Vector2 point, float maxDownDistance, float heightTolerance, out Segment result)
    {
        result = null;
        float bestY = float.NegativeInfinity;
        float maxY = point.y + heightTolerance;
        float minY = point.y - maxDownDistance;

        foreach (Segment segment in segments)
        {
            if (!segment.ContainsX(point.x))
                continue;

            if (segment.y > maxY || segment.y < minY)
                continue;

            if (segment.y > bestY)
            {
                bestY = segment.y;
                result = segment;
            }
        }

        return result != null;
    }

    private void ExtractSegments(Tilemap tilemap, List<Segment> results)
    {
        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cell))
                continue;

            if (tilemap.HasTile(cell + Vector3Int.up))
                continue;

            AddCellTopSegment(tilemap, cell, results);
        }
    }

    private void AddCellTopSegment(Tilemap tilemap, Vector3Int cell, List<Segment> results)
    {
        Vector3Int topLeftCell = cell + Vector3Int.up;
        Vector3Int topRightCell = topLeftCell + Vector3Int.right;
        Vector2 left = tilemap.CellToWorld(topLeftCell);
        Vector2 right = tilemap.CellToWorld(topRightCell);
        AddSegment(left, right, tilemap, results);
    }

    private void AddSegment(Vector2 a, Vector2 b, Tilemap source, List<Segment> results)
    {
        float leftX = Mathf.Min(a.x, b.x);
        float rightX = Mathf.Max(a.x, b.x);
        float y = (a.y + b.y) * 0.5f;

        if (rightX - leftX <= 0.01f)
            return;

        results.Add(new Segment(leftX, rightX, y, source));
    }

    private void MergeSegments(List<Segment> source, List<Segment> results)
    {
        source.Sort((a, b) =>
        {
            int yCompare = a.y.CompareTo(b.y);
            return yCompare != 0 ? yCompare : a.leftX.CompareTo(b.leftX);
        });

        Segment current = null;
        foreach (Segment segment in source)
        {
            if (current == null || !current.CanMerge(segment, surfaceHeightTolerance, mergeGapTolerance))
            {
                current = segment.Clone();
                results.Add(current);
                continue;
            }

            current.Merge(segment);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawSegmentGizmos)
            return;

        Gizmos.color = Color.gray;
        foreach (Segment segment in segments)
        {
            Vector3 left = new Vector3(segment.leftX, segment.y, 0f);
            Vector3 right = new Vector3(segment.rightX, segment.y, 0f);
            Gizmos.DrawLine(left, right);
            Gizmos.DrawSphere(left, 0.04f);
            Gizmos.DrawSphere(right, 0.04f);
        }
    }

    public class Segment
    {
        public float leftX;
        public float rightX;
        public float y;
        public Tilemap source;

        public Segment(float leftX, float rightX, float y, Tilemap source)
        {
            this.leftX = leftX;
            this.rightX = rightX;
            this.y = y;
            this.source = source;
        }

        public bool ContainsX(float x)
        {
            return x >= leftX && x <= rightX;
        }

        public bool OverlapsX(float left, float right)
        {
            return rightX >= left && leftX <= right;
        }

        public bool Overlaps(Bounds bounds)
        {
            return rightX >= bounds.min.x
                && leftX <= bounds.max.x
                && y >= bounds.min.y
                && y <= bounds.max.y;
        }

        public bool CanMerge(Segment other, float heightTolerance, float gapTolerance)
        {
            return Mathf.Abs(y - other.y) <= heightTolerance
                && other.leftX - rightX <= gapTolerance;
        }

        public void Merge(Segment other)
        {
            float width = Mathf.Max(0.001f, rightX - leftX);
            float otherWidth = Mathf.Max(0.001f, other.rightX - other.leftX);
            y = ((y * width) + (other.y * otherWidth)) / (width + otherWidth);
            rightX = Mathf.Max(rightX, other.rightX);
        }

        public Segment Clone()
        {
            return new Segment(leftX, rightX, y, source);
        }
    }
}
