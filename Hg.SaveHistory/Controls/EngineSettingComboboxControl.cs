using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Controls
{
    public partial class EngineSettingComboboxControl : UserControl
    {
        #region Fields & Properties

        public event SettingControlEventHandler ValueChanged;

        private readonly Dictionary<int, int> _indexToId = new Dictionary<int, int>();

        public int Value
        {
            get
            {
                if (comboBoxValues.SelectedIndex >= 0)
                {
                    return _indexToId[comboBoxValues.SelectedIndex];
                }

                return -1;
            }
        }

        #endregion

        #region Members

        public EngineSettingComboboxControl()
        {
            InitializeComponent();
        }

        public void SetComboboxValues(Dictionary<int, string> settingComboboxValues)
        {
            foreach (KeyValuePair<int, string> pair in settingComboboxValues)
            {
                int index = comboBoxValues.Items.Add(pair.Value);
                _indexToId.Add(index, pair.Key);
            }
        }

        public void SetValue(int value)
        {
            if (_indexToId.ContainsValue(value))
            {
                int index = _indexToId.Where(p => p.Value == value).Select(p => p.Key).FirstOrDefault();
                comboBoxValues.SelectedIndex = index;
            }
        }

        private void comboBoxValues_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke();
        }

        #endregion
    }
}