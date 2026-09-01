using HAgent.WinForms;
using System.Windows.Forms;

namespace HWorld.HAgent
{
    /// <summary>
    /// Opens the HAgent configuration UI using its public WinForms integration surface.
    /// </summary>
    public static class HAgentConfiguration
    {
        public static void Show(IWin32Window owner = null)
        {
            AISettings.ShowMainAISettingsForm(owner);
        }
    }
}
