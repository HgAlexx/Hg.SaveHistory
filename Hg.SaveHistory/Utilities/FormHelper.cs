using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Hg.SaveHistory.Utilities
{
    public class FormHelper
    {
        #region Members

        public static IEnumerable<T> FindControls<T>(Control control) where T : Control
        {
            var controls = control.Controls.Cast<Control>().ToList();
            return controls.SelectMany(FindControls<T>).Concat(controls).Where(c => c.GetType() == typeof(T)).Cast<T>();
        }

        #endregion
    }
}