namespace HWorld.ImageCore
{
    public enum ImageSizeMode
    {
        Normal,     // Uses ImageWidth/ImageHeight + ImageAlign
        Stretch,    // Stretches to fill entire area
        Zoom        // Fills area, maintains aspect ratio
    }
}
