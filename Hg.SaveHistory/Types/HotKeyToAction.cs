using System.Windows.Forms;

namespace Hg.SaveHistory.Types
{
    public delegate void HotKeyEventHandler(object sender, KeyEventArgs e, HotKeyToAction hotKeyToAction);

    public class HotKeyToAction
    {
        #region Fields & Properties

        public HotKeyAction Action { get; set; }

        public bool Enabled { get; set; }

        public HotKey HotKey { get; set; }

        #endregion
    }
}