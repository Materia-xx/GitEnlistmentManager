using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Windows;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for GemSettings.xaml
    /// </summary>
    public partial class GemSettings : Window
    {
        private readonly Gem gem;

        public GemSettings(Gem gem)
        {
            InitializeComponent();
            this.gem = gem;
            this.DtoToForm();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Transfer data from form to DTO
            FormToDto();

            // Write the metadata.json.
            if (this.gem.WriteMetadata())
            {
                this.Close();
            }
        }

        private void FormToDto()
        {
            this.gem.Metadata.ReposFolder = this.txtReposFolder.Text;
        }

        private void DtoToForm()
        {
            this.txtReposFolder.Text = this.gem.Metadata.ReposFolder;
        }
    }
}
