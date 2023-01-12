using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnRunCommand_Click(object sender, RoutedEventArgs e)
        {
            txtCommandPrompt.FormatLinesWithoutExtraLineReturns();
            txtCommandPrompt.AppendLine($">git.exe blah blah blah..", Brushes.White);

            for (int i = 0;i<4;i++)
            {
                txtCommandPrompt.AppendLine($"Results #{i}...", Brushes.Gray);
            }

            txtCommandPrompt.ScrollToEnd();
        }
    }
}
