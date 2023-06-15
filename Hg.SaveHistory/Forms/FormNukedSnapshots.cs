using System;
using System.Linq;
using System.Windows.Forms;
using Hg.SaveHistory.API;
using Hg.SaveHistory.Managers;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Forms
{
    public partial class FormNukedSnapshots : Form
    {
        #region Fields & Properties

        private readonly FormMain _formMain;
        private readonly LuaManager _luaManager;

        #endregion

        #region Members

        public FormNukedSnapshots(FormMain formMain, LuaManager luaManager)
        {
            InitializeComponent();

            _formMain = formMain;
            _luaManager = luaManager;

            buttonForget.Enabled = false;

            // Load list columns
            listViewSnapshot.BeginUpdate();
            listViewSnapshot.Columns.Clear();
            foreach (var columnDefinition in _luaManager.ActiveEngine.SnapshotColumnsDefinition.OrderBy(c => c.Order))
            {
                listViewSnapshot.Columns.Add(columnDefinition.HeaderText).Tag = columnDefinition;
            }

            listViewSnapshot.EndUpdate();
            listViewSnapshot.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void buttonForget_Click(object sender, EventArgs e)
        {
            if (_formMain.Message("All selected snapshots will be forgotten, do you want to continue?", "Confirm memory wipe?",
                    MessageType.Question, MessageMode.MessageBox) != DialogResult.Yes)
            {
                return;
            }

            foreach (ListViewItem item in listViewSnapshot.SelectedItems)
            {
                if (item.Tag is EngineSnapshot snapshot)
                {
                    _luaManager.ActiveEngine.Snapshots.Remove(snapshot);
                }
            }

            buttonForget.Enabled = false;
            _formMain.RefreshSnapshotsListView(listViewSnapshot, EngineSnapshotStatus.Nuked, null);
        }

        private void FormNukedSnapshots_Load(object sender, EventArgs e)
        {
            _formMain.RefreshSnapshotsListView(listViewSnapshot, EngineSnapshotStatus.Nuked, null);
        }

        private void listViewSnapshot_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonForget.Enabled = listViewSnapshot.SelectedItems.Count > 0;
            if (listViewSnapshot.SelectedItems.Count == 1 && listViewSnapshot.SelectedItems[0].Tag is EngineSnapshot snapshot)
            {
                _formMain.SetSnapshotDetails(listViewDetails, snapshot);
            }
            else
            {
                listViewDetails.Enabled = false;
                listViewDetails.BeginUpdate();
                try
                {
                    listViewDetails.Items.Clear();
                }
                finally
                {
                    listViewDetails.EndUpdate();
                }
            }
        }

        #endregion
    }
}