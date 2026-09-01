using System.Windows.Forms;

namespace HWorld.HAgent
{
    // HWorld.HAgent files may import both System.Threading and System.Windows.Forms.
    // Keep the unqualified Timer used by the live diagnostic bound to the WinForms UI timer.
    internal sealed class Timer : System.Windows.Forms.Timer
    {
    }
}
