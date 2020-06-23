using System.Windows.Forms;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.API
{
    public delegate DialogResult MessageEventHandler(string text, string caption, MessageType type, MessageMode mode);
}