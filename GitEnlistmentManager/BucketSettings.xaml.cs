using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System;
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
            this.Icon = Icons.GemIcon;
            this.bucket = bucket;
            this.mainWindow = mainWindow;
            this.DtoToForm();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtBucketName.Text.Equals("archive", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The archive bucket is reserved for archiving only");
                return;
            }

            // Transfer data from form to DTO
            FormToDto();
            this.Close();
        }

        private void FormToDto()
        {
            this.bucket.GemName = this.txtBucketName.Text;
        }

        private void DtoToForm()
        {
            this.txtBucketName.Text = this.bucket.GemName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtBucketName.Focus();
        }
    }
}
