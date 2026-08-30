namespace HWorld.Core.World
{
    /// <summary>
    /// Renderer-independent vector shape requested for a world item.
    /// Renderers decide how the shape is painted.
    /// </summary>
    public enum WorldShapeKind
    {
        Rectangle,
        Ellipse,
        Triangle,
        Diamond,
        Hexagon,
        Star,
        Tree,
        House,
        Rock,
        Flower,
        Pillar,
        Cross
    }
}
