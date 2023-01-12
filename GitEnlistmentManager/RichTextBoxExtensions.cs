using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GitEnlistmentManager
{
    public static class RichTextBoxExtensions
    {
        public static void AppendLine(this RichTextBox box, string text, Brush brush)
        {
            var range = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd)
            {
                Text = text + Environment.NewLine
            };
            range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }

        public static void FormatLinesWithoutExtraLineReturns(this RichTextBox box)
        {
            var p = box.Document.Blocks.FirstBlock as Paragraph;
            p.LineHeight = 10;
            p.Margin = new Thickness(0);
        }
    }
}
