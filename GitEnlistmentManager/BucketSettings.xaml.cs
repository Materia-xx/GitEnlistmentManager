using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Windows;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for BucketSettings.xaml
    /// </summary>
    public partial class BucketSettings : Window
    {
        private readonly Bucket bucket;
        private readonly MainWindow mainWindow;

        public BucketSettings(Bucket bucket, MainWindow mainWindow)
        {
            InitializeComponent();
            this.bucket = bucket;
            this.mainWindow = mainWindow;
            this.DtoToForm();
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Transfer data from form to DTO
            FormToDto();

            // Force directory for the bucket to be created
            if (this.bucket.GetDirectoryInfo() != null)
            {
                // Run any "AfterBucketCreate" command sets 
                var afterBucketCreateCommandSets = this.bucket.Repo.RepoCollection.Gem.GetCommandSets(CommandSetPlacement.AfterBucketCreate, this.bucket.Repo.RepoCollection, this.bucket.Repo, this.bucket);
                await mainWindow.RunCommandSets(afterBucketCreateCommandSets, this.bucket.GetTokens(), this.bucket.GetDirectoryInfo()?.FullName).ConfigureAwait(false);
                this.Close();
            }
        }

        private void FormToDto()
        {
            this.bucket.Name = this.txtBucketName.Text;
        }

        private void DtoToForm()
        {
            this.txtBucketName.Text = this.bucket.Name;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtBucketName.Focus();
        }
    }
}
