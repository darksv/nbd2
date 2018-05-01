using System;
using System.Windows;
using NBD2.ViewModel;

namespace NBD2.View
{
    public partial class PersonCreateEdit : Window
    {
        public PersonCreateEdit(PersonCreateEditViewModel personCreateEditViewModel)
        {
            InitializeComponent();
            DataContext = personCreateEditViewModel;
            Loaded += (s, e) => personCreateEditViewModel.OnSaved += PersonCreateEditViewModelOnOnSaved;
            Closed += (s, e) => personCreateEditViewModel.OnSaved -= PersonCreateEditViewModelOnOnSaved;
        }

        private void PersonCreateEditViewModelOnOnSaved(object sender, EventArgs e)
        {
            Close();
        }
    }
}
