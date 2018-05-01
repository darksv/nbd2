using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using NBD2.Service;
using NBD2.Util;
using NBD2.View;
using QuickGraph;

namespace NBD2.ViewModel
{
    public class MainViewModel : BindableBase
    {
        private readonly IPersonService _personService;
        private readonly TreeCreator _treeCreator;
        public ObservableCollection<PersonViewModel> Persons { get; } = new ObservableCollection<PersonViewModel>();

        public ICommand CreateCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public MainViewModel(IPersonService personService, TreeCreator treeCreator)
        {
            _personService = personService;
            _treeCreator = treeCreator;
            CreateCommand = new RelayCommand(() =>
            {
                var createViewModel = new PersonCreateEditViewModel(_personService);
                createViewModel.OnSaved += (s, e) => Persons.Add(e.Person);
                var window = new PersonCreateEdit(createViewModel);
                window.ShowDialog();
            });
            EditCommand = new RelayCommand<PersonViewModel>(p =>
            {
                var window = new PersonCreateEdit(new PersonCreateEditViewModel(p, _personService));
                window.ShowDialog();
            });
            DeleteCommand = new RelayCommand<PersonViewModel>(p =>
            {
                var result = MessageBox.Show(
                    $"Czy na pewno usunąć {p.Name}?",
                    "Usuwanie",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                if (result == MessageBoxResult.Yes)
                {
                    _personService.DeletePerson(p.Name);
                    Persons.Remove(p);
                }
            });

            foreach (var person in _personService.GetAll())
            {
                Persons.Add(new PersonViewModel
                {
                    Name = person.Name,
                    DateOfBirth = person.DateOfBirth,
                    DateOfDeath = person.DateOfDeath,
                    Sex = person.Sex,
                });
            }
        }

        public IBidirectionalGraph<object, IEdge<object>> Graph
        {
            get
            {
                var g = new BidirectionalGraph<object, IEdge<object>>();
                _treeCreator.BuildTreeForPerson("a");

                foreach (var person in Persons)
                {
                    g.AddVertex(person);
                    if (person.Father != null)
                    {
                        g.AddEdge(new Edge<object>(person, person.Father));
                    }

                    if (person.Mother != null)
                    {
                        g.AddEdge(new Edge<object>(person, person.Mother));
                    }
                }

                return g;
            }
        }
    }
}