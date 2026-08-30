using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HWorld.WinForms.Helpers
{
    internal static class NavigationOrder
    {
        private static readonly string[] Order =
        {
            "Overview",
            "Providers",
            "Agents",
            "Tools",
            "About"
        };

        public static void Apply(Control root)
        {
            if (root == null) return;

            foreach (Control child in root.Controls)
            {
                if (child is FlowLayoutPanel panel && ContainsNavigationButtons(panel))
                {
                    Reorder(panel);
                    return;
                }

                Apply(child);
            }
        }

        private static bool ContainsNavigationButtons(FlowLayoutPanel panel)
        {
            var texts = panel.Controls.Cast<Control>()
                .Select(x => x.Text)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return Order.All(texts.Contains);
        }

        private static void Reorder(FlowLayoutPanel panel)
        {
            var buttons = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase);

            foreach (Control control in panel.Controls)
            {
                if (!string.IsNullOrWhiteSpace(control.Text))
                    buttons[control.Text] = control;
            }

            for (var i = Order.Length - 1; i >= 0; i--)
            {
                Control control;
                if (buttons.TryGetValue(Order[i], out control))
                    panel.Controls.SetChildIndex(control, 0);
            }
        }
    }
}
