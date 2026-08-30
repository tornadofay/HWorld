namespace HWorld.WinForms.Helpers.Button.HButtonCore.Rendering
{
    public interface IRendererResourceProvider
    {
        T GetOrCreate<T>() where T : HButtonRendererResources, new();
    }
}
