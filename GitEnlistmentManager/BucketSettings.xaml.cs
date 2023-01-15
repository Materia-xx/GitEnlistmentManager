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

        public BucketSettings(Bucket bucket)
        {
            InitializeComponent();
            this.bucket = bucket;
            this.DtoToForm();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Transfer data from form to DTO
            FormToDto();

            // Force directory for the bucket to be created
            if (this.bucket.GetDirectoryInfo() != null)
            {
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
    }
}
