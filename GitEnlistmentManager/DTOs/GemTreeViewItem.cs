using GitEnlistmentManager.Globals;
using System.Windows.Media;

namespace GitEnlistmentManager.DTOs
{
    public class GemTreeViewItem
    {
        public ImageSource? Icon { get; set; }

        public bool IsExpanded { get; set; }
    }
}
