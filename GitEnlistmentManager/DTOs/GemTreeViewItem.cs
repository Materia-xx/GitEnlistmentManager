using System.Windows.Media;

namespace GitEnlistmentManager.DTOs
{
    public class GemTreeViewItem
    {
        public ImageSource? Icon { get; set; }

        public string? GemName { get; set; }

        public bool IsExpanded {  get; set; }

        public bool IsSelected {  get; set; }

    }
}
