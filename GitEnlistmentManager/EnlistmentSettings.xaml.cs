using System.Windows;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for EnlistmentSettings.xaml
    /// </summary>
    public partial class EnlistmentSettings : Window
    {
        private EnlistmentSettingsDialogResult dialogResult;

        public EnlistmentSettings(string? bucketName, string? enlistmentName, bool bucketNameIsEnabled = false)
        {
            InitializeComponent();
            txtBucketName.IsEnabled = bucketNameIsEnabled;
            dialogResult = new EnlistmentSettingsDialogResult()
            {
                BucketName = bucketName,
                EnlistmentName = enlistmentName,
                ScopeToBranch = true,
                GitAutoCrlf = false
            };
            this.DtoToForm();
        }

        public new EnlistmentSettingsDialogResult? ShowDialog()
        {
            var result = base.ShowDialog();
            return (result.HasValue && result.Value) ? dialogResult : null;
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
            dialogResult.BucketName = this.txtBucketName.Text;
            dialogResult.EnlistmentName = this.txtEnlistmentName.Text;
            dialogResult.ScopeToBranch = this.chkScopeToBranch.IsChecked.HasValue && this.chkScopeToBranch.IsChecked.Value;
            dialogResult.GitAutoCrlf = this.chkGitAutoCrlf.IsChecked.HasValue && this.chkGitAutoCrlf.IsChecked.Value;
        }

        private void DtoToForm()
        {
            this.txtBucketName.Text = dialogResult.BucketName;
            this.txtEnlistmentName.Text = dialogResult.EnlistmentName;
            this.chkScopeToBranch.IsChecked = dialogResult.ScopeToBranch;
            this.chkGitAutoCrlf.IsChecked = dialogResult.GitAutoCrlf;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtEnlistmentName.Focus();
        }

        public class EnlistmentSettingsDialogResult
        {
            public string? BucketName { get; set; }
            public string? EnlistmentName { get; set; }
            public bool ScopeToBranch { get; set; } = true;
            public bool GitAutoCrlf { get; set; } = false;
        }

    }
}
