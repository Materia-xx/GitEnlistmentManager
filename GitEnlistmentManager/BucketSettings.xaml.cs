using GitEnlistmentManager.DTOs;
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

        public BucketSettings(Bucket bucket)
        {
            InitializeComponent();
            this.bucket = bucket;
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
            this.DialogResult = true;
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
