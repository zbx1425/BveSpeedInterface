using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace BveDebugWindowInput {

    class ControlReference {

        Form debugForm;
        ToolStripTextBox speedTSTB, positionTSTB;

        public event EventHandler SpeedChanged;
        public event EventHandler PositionChanged;

        public double Speed {
            get {
                return double.Parse(speedTSTB.Text);
            }
            set {
                debugForm.Invoke((Action)(() => {
                    speedTSTB.Text = value.ToString();
                    FireWinformEvent(speedTSTB, "KeyDown", new KeyEventArgs(Keys.Enter));
                }));
            }
        }
        public double Position {
            get {
                return double.Parse(positionTSTB.Text);
            }
            set {
                debugForm.Invoke((Action)(() => {
                    positionTSTB.Text = value.ToString();
                    FireWinformEvent(positionTSTB, "KeyDown", new KeyEventArgs(Keys.Enter));
                }));
            }
        }

        public bool CanGetSet {
            get {
                return debugForm != null && speedTSTB != null && positionTSTB != null;
            }
        }

        public void OpenAndGet() {
            if (debugForm != null && (debugForm.Disposing || debugForm.IsDisposed)) {
                debugForm = null;
                positionTSTB = null;
                speedTSTB = null;
            }
            if (positionTSTB != null && positionTSTB.IsDisposed) positionTSTB = null;
            if (speedTSTB != null && speedTSTB.IsDisposed) speedTSTB = null;
            if (speedTSTB != null && positionTSTB != null) return;

            ContextMenuStrip mainCtxMenu = null;
            foreach (FieldInfo fi in Application.OpenForms[0].GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)) {
                if (fi.FieldType == typeof(ContextMenuStrip)) {
                    mainCtxMenu = (ContextMenuStrip)fi.GetValue(Application.OpenForms[0]);
                }
            }
            for (int i = mainCtxMenu.Items.Count - 1; i >= 0; --i) {
                ToolStripItem outItem = mainCtxMenu.Items[i];
                if (outItem is ToolStripMenuItem outMenuItem && outMenuItem.HasDropDownItems) {
                    outMenuItem.DropDownItems[0].PerformClick();
                    break;
                }
            }
            foreach (Form form in Application.OpenForms) {
                foreach (Control control in form.Controls) {
                    if (control is ToolStripContainer container) {
                        positionTSTB = (ToolStripTextBox)(container.TopToolStripPanel.Controls[1] as ToolStrip).Items[0];
                        speedTSTB = (ToolStripTextBox)(container.TopToolStripPanel.Controls[2] as ToolStrip).Items[0];
                        debugForm = form;
                        positionTSTB.TextChanged += (sender, e) => {
                            if (PositionChanged != null && double.TryParse(positionTSTB.Text, out _))
                                PositionChanged(this, e);
                        };
                        speedTSTB.TextChanged += (sender, e) => {
                            if (SpeedChanged != null && double.TryParse(speedTSTB.Text, out _))
                                SpeedChanged(this, e);
                        };
                        form.Hide();
                        break;
                    }
                }
            }
        }

        public static void FireWinformEvent(Object targetObject, string eventName, EventArgs e) {
            string methodName = "On" + eventName;
            MethodInfo mi = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(targetObject, new object[] { e });
        }
    }
}
