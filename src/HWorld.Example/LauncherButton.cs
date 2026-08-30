using System.Drawing;
using System.Windows.Forms;
using HWorld.WinForms.Helpers.Button;

namespace HWorld.Example
{
    /// <summary>
    /// Example-only HButton presentation adapter.
    /// Keeps the launcher on HButton while guaranteeing visible native GDI text.
    /// </summary>
    internal sealed class LauncherButton : HButton
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (string.IsNullOrEmpty(Text))
                return;

            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
                using (var brush = new SolidBrush(ForeColor))
                {
                    e.Graphics.DrawString(Text, Font, brush, ClientRectangle, format);
                }
            }
        }
    }
}
