using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Hg.SaveHistory.Utilities;
using Timer = System.Timers.Timer;

namespace Hg.SaveHistory.Forms
{
    public partial class FormDebugConsole : Form
    {
        #region Fields & Properties

        private readonly Timer _timer;

        private List<string> _logs;

        #endregion

        #region Members

        public FormDebugConsole()
        {
            InitializeComponent();
            _timer = new Timer { Enabled = false, Interval = 500 };
            _timer.Elapsed += (sender, args) => { RefreshList(); };
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            listViewLog.Items.Clear();
            Logger.ClearLogs();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void checkBoxDebug_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDebug.Checked)
            {
                Logger.Level = LogLevel.Debug;
            }
            else
            {
                Logger.Level = LogLevel.Information;
            }
        }

        private void FormDebugConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.OnLog -= OnLogEvent;

            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Visible = false;
            }
        }

        private void FormDebugConsole_Shown(object sender, EventArgs e)
        {
            Logger.OnLog += OnLogEvent;
        }

        private void LoadListView()
        {
            listViewLog.BeginUpdate();

            int currentCount = listViewLog.Items.Count;
            if (currentCount < _logs.Count)
            {
                for (int i = currentCount; i < _logs.Count; i++)
                {
                    listViewLog.Items.Add(_logs[i]);
                }
            }

            if (listViewLog.Items.Count > 0)
            {
                listViewLog.EnsureVisible(listViewLog.Items.Count - 1);
            }

            listViewLog.EndUpdate();
        }

        private void OnLogEvent()
        {
            _timer.Stop();
            _timer.Start();
        }


        private void RefreshList()
        {
            _timer.Stop();
            _logs = Logger.GetLogs();
            if (InvokeRequired)
            {
                Invoke(new Action(LoadListView));
            }
            else
            {
                LoadListView();
            }
        }

        #endregion
    }
}