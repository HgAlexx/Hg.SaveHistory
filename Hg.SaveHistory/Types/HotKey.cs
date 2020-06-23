using System;
using System.Windows.Forms;

namespace Hg.SaveHistory.Types
{
    [Serializable]
    public class HotKey
    {
        #region

        public bool Alt;
        public bool Control;
        public Keys Key;
        public bool Shift;

        #endregion

        #region

        public HotKey(Keys keys, bool ctrl, bool alt, bool shift)
        {
            Control = ctrl;
            Alt = alt;
            Shift = shift;
            Key = keys;
        }

        public bool Match(Keys key, bool ctrl, bool alt, bool shift)
        {
            if (key == Key && ctrl == Control && alt == Alt && shift == Shift)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}