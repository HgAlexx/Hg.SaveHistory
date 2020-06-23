using System;
using System.Windows.Forms;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Controls
{
    public partial class HotKeyControl : UserControl
    {
        #region Fields & Properties

        public Keys Key;

        #endregion

        #region Members

        public HotKeyControl()
        {
            InitializeComponent();

            checkBoxEnabled.Checked = false;
            checkBoxAlt.Checked = false;
            checkBoxControl.Checked = false;
            checkBoxShift.Checked = false;
            textBoxKey.Text = "";

            SetStates(false);
        }

        public void FromHotKeyToAction(HotKeyToAction hotKeyToAction)
        {
            SetStates(hotKeyToAction.Enabled);
            checkBoxControl.Checked = hotKeyToAction.HotKey.Control;
            checkBoxAlt.Checked = hotKeyToAction.HotKey.Alt;
            checkBoxShift.Checked = hotKeyToAction.HotKey.Shift;
            Key = hotKeyToAction.HotKey.Key;
            SetTextBoxKey();
        }

        public void ToHotKeyToAction(HotKeyToAction hotKeyToAction)
        {
            hotKeyToAction.Enabled = checkBoxEnabled.Checked;
            hotKeyToAction.HotKey.Control = checkBoxControl.Checked;
            hotKeyToAction.HotKey.Alt = checkBoxAlt.Checked;
            hotKeyToAction.HotKey.Shift = checkBoxShift.Checked;
            hotKeyToAction.HotKey.Key = Key;
        }

        private void checkBoxEnable_CheckedChanged(object sender, EventArgs e)
        {
            SetStates(checkBoxEnabled.Checked);
        }

        private void SetStates(bool enable)
        {
            checkBoxEnabled.Checked = enable;
            checkBoxAlt.Enabled = enable;
            checkBoxControl.Enabled = enable;
            checkBoxShift.Enabled = enable;
            textBoxKey.Enabled = enable;
        }

        private void SetTextBoxKey()
        {
            // Some special cases... thx windows api!
            switch (Key)
            {
                case Keys.PageDown:
                    textBoxKey.Text = @"PageDown";
                    break;
                default:
                    textBoxKey.Text = Key.ToString();
                    break;
            }
        }

        private void textBoxKey_KeyDown(object sender, KeyEventArgs e)
        {
            Key = e.KeyCode;
            SetTextBoxKey();
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void textBoxKey_KeyUp(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void textBoxKey_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        #endregion
    }
}