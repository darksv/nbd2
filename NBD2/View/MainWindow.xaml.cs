using System.Windows;
using NBD2.Service;
using NBD2.ViewModel;

namespace NBD2.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var p = new PersonService();
            DataContext = new MainViewModel(p , new TreeCreator(p));
        }
    }
}
