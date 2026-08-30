using System;
using System.Collections.Generic;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    /// <summary>
    /// Lightweight uniform-grid spatial index for broad-phase world item queries.
    /// The index owns no world state; callers update it when items move or change size.
    /// </summary>
    public sealed class WorldSpatialIndex
    {
        private readonly Dictionary<long, List<WorldItem>> _cells = new Dictionary<long, List<WorldItem>>();
        private readonly Dictionary<Guid, CellRange> _ranges = new Dictionary<Guid, CellRange>();
        private readonly double _cellSize;
        private readonly double _worldWidth;
        private readonly double _worldHeight;

        public WorldSpatialIndex(double worldWidth, double worldHeight, double cellSize = 16.0)
        {
            if (worldWidth <= 0) throw new ArgumentOutOfRangeException(nameof(worldWidth));
            if (worldHeight <= 0) throw new ArgumentOutOfRangeException(nameof(worldHeight));
            if (cellSize <= 0) throw new ArgumentOutOfRangeException(nameof(cellSize));
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;
            _cellSize = cellSize;
        }

        public double CellSize { get { return _cellSize; } }

        public void Clear()
        {
            _cells.Clear();
            _ranges.Clear();
        }

        public void Rebuild(IReadOnlyList<WorldItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Clear();
            for (int i = 0; i < items.Count; i++)
                Add(items[i]);
        }

        public void Add(WorldItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (_ranges.ContainsKey(item.Id))
                Update(item);
            else
                Insert(item, GetRange(item));
        }

        public bool Remove(Guid id)
        {
            CellRange range;
            if (!_ranges.TryGetValue(id, out range))
                return false;

            RemoveFromRange(id, range);
            _ranges.Remove(id);
            return true;
        }

        public void Update(WorldItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            CellRange next = GetRange(item);
            CellRange current;
            if (!_ranges.TryGetValue(item.Id, out current))
            {
                Insert(item, next);
                return;
            }

            if (current.Equals(next))
                return;

            RemoveFromRange(item.Id, current);
            Insert(item, next);
        }

        public List<WorldItem> Query(WorldPoint point)
        {
            int cellX = ToCellX(point.X);
            int cellY = ToCellY(point.Y);
            List<WorldItem> result;
            if (!_cells.TryGetValue(Key(cellX, cellY), out result))
                return new List<WorldItem>();
            return new List<WorldItem>(result);
        }

        public List<WorldItem> Query(WorldPoint min, WorldPoint max)
        {
            double minX = Math.Min(min.X, max.X);
            double minY = Math.Min(min.Y, max.Y);
            double maxX = Math.Max(min.X, max.X);
            double maxY = Math.Max(min.Y, max.Y);

            int minCellX = ToCellX(minX);
            int minCellY = ToCellY(minY);
            int maxCellX = ToCellX(maxX);
            int maxCellY = ToCellY(maxY);

            var result = new List<WorldItem>();
            var seen = new HashSet<Guid>();
            for (int y = minCellY; y <= maxCellY; y++)
            {
                for (int x = minCellX; x <= maxCellX; x++)
                {
                    List<WorldItem> bucket;
                    if (!_cells.TryGetValue(Key(x, y), out bucket)) continue;
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        var item = bucket[i];
                        if (!seen.Add(item.Id)) continue;
                        if (Intersects(item, minX, minY, maxX, maxY))
                            result.Add(item);
                    }
                }
            }
            return result;
        }

        private void Insert(WorldItem item, CellRange range)
        {
            _ranges[item.Id] = range;
            for (int y = range.MinY; y <= range.MaxY; y++)
            {
                for (int x = range.MinX; x <= range.MaxX; x++)
                {
                    long key = Key(x, y);
                    List<WorldItem> bucket;
                    if (!_cells.TryGetValue(key, out bucket))
                    {
                        bucket = new List<WorldItem>();
                        _cells.Add(key, bucket);
                    }
                    bucket.Add(item);
                }
            }
        }

        private void RemoveFromRange(Guid id, CellRange range)
        {
            for (int y = range.MinY; y <= range.MaxY; y++)
            {
                for (int x = range.MinX; x <= range.MaxX; x++)
                {
                    long key = Key(x, y);
                    List<WorldItem> bucket;
                    if (!_cells.TryGetValue(key, out bucket)) continue;
                    for (int i = bucket.Count - 1; i >= 0; i--)
                    {
                        if (bucket[i].Id == id)
                            bucket.RemoveAt(i);
                    }
                    if (bucket.Count == 0)
                        _cells.Remove(key);
                }
            }
        }

        private CellRange GetRange(WorldItem item)
        {
            double maxX = Math.Min(_worldWidth, item.Position.X + Math.Max(0, item.Width));
            double maxY = Math.Min(_worldHeight, item.Position.Y + Math.Max(0, item.Height));
            return new CellRange(
                ToCellX(item.Position.X),
                ToCellY(item.Position.Y),
                ToCellX(maxX),
                ToCellY(maxY));
        }

        private int ToCellX(double x)
        {
            if (x < 0) return 0;
            if (x >= _worldWidth) return Math.Max(0, (int)Math.Ceiling(_worldWidth / _cellSize) - 1);
            return (int)Math.Floor(x / _cellSize);
        }

        private int ToCellY(double y)
        {
            if (y < 0) return 0;
            if (y >= _worldHeight) return Math.Max(0, (int)Math.Ceiling(_worldHeight / _cellSize) - 1);
            return (int)Math.Floor(y / _cellSize);
        }

        private static long Key(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }

        private static bool Intersects(WorldItem item, double minX, double minY, double maxX, double maxY)
        {
            double itemMaxX = item.Position.X + item.Width;
            double itemMaxY = item.Position.Y + item.Height;
            return item.Position.X <= maxX && itemMaxX >= minX &&
                   item.Position.Y <= maxY && itemMaxY >= minY;
        }

        private readonly struct CellRange : IEquatable<CellRange>
        {
            public CellRange(int minX, int minY, int maxX, int maxY)
            {
                MinX = minX; MinY = minY; MaxX = maxX; MaxY = maxY;
            }
            public readonly int MinX, MinY, MaxX, MaxY;
            public bool Equals(CellRange other) { return MinX == other.MinX && MinY == other.MinY && MaxX == other.MaxX && MaxY == other.MaxY; }
            public override bool Equals(object obj) { return obj is CellRange && Equals((CellRange)obj); }
            public override int GetHashCode() { unchecked { int h = MinX; h = h * 31 + MinY; h = h * 31 + MaxX; h = h * 31 + MaxY; return h; } }
        }
    }
}
