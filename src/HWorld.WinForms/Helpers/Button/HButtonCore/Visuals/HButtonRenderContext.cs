namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{
    public readonly struct HButtonRenderContext
    {
        public readonly HButtonState State;
        public readonly HButtonGeometry Geometry;
        public readonly HButtonContent Content;
        public readonly HButtonAppearance Appearance;
      //  public readonly IHyperTheme Theme;

        public HButtonRenderContext(HButtonState state, HButtonGeometry geometry, HButtonContent content, HButtonAppearance appearance)
        { State = state; Geometry = geometry; Content = content; Appearance = appearance; }
    }
}
