﻿using GitEnlistmentManager.DTOs;
using System.Windows;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for EnlistmentSettings.xaml
    /// </summary>
    public partial class EnlistmentSettings : Window
    {
        private readonly Enlistment enlistment;

        public EnlistmentSettings(Enlistment enlistment)
        {
            InitializeComponent();
            this.enlistment = enlistment;
            this.DtoToForm();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Transfer data from form to DTO
            FormToDto();

            // Close the form, the main UI will create the enlistment
            // so that the commands can be viewed in the UI.
            this.Close();
        }

        private void FormToDto()
        {
            this.enlistment.Name = this.txtEnlistmentName.Text;
        }

        private void DtoToForm()
        {
            this.txtEnlistmentName.Text = this.enlistment.Name;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtEnlistmentName.Focus();
        }
    }
}