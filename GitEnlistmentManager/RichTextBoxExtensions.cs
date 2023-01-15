using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GitEnlistmentManager
{
    public static class RichTextBoxExtensions
    {
        public async static Task AppendLine(this RichTextBox box, string text, Brush brush)
        {
            await box.Dispatcher.BeginInvoke(() =>
            {
                var range = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd)
                {
                    Text = text + Environment.NewLine
                };
                range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            });
        }

        public async static Task FormatLinesWithoutExtraLineReturns(this RichTextBox box)
        {
            await box.Dispatcher.BeginInvoke(() =>
            {
                if (box.Document.Blocks.FirstBlock is Paragraph p)
                {
                    p.LineHeight = 10;
                    p.Margin = new Thickness(0);
                }
            });
        }
    }
}
