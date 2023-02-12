using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using System.Windows;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for EnlistmentSettings.xaml
    /// </summary>
    public partial class EnlistmentSettings : Window
    {
        public string? BucketName { get; set; }
        public string? EnlistmentName { get; set; }

        public EnlistmentSettings(string? bucketName, string? enlistmentName, bool bucketNameIsEnabled = false)
        {
            InitializeComponent();
            this.BucketName = bucketName;
            this.EnlistmentName= enlistmentName;
            txtBucketName.IsEnabled = bucketNameIsEnabled;
            this.DtoToForm();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Transfer data from form to DTO
            FormToDto();
            this.DialogResult = true;
            // Close the form, the main UI will create the enlistment
            // so that the commands can be viewed in the UI.
            this.Close();
        }

        private void FormToDto()
        {
            this.BucketName = this.txtBucketName.Text;
            this.EnlistmentName = this.txtEnlistmentName.Text;
        }

        private void DtoToForm()
        {
            this.txtBucketName.Text = this.BucketName;
            this.txtEnlistmentName.Text = this.EnlistmentName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtEnlistmentName.Focus();
        }
    }
}
