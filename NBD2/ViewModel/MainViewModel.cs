using System.Collections.ObjectModel;
using System.Linq;
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
        public IBidirectionalGraph<object, IEdge<object>> Graph { get; private set; }

        public ICommand CreateCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ShowDescendantsCommand { get; set; }

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
            ShowDescendantsCommand = new RelayCommand<PersonViewModel>(p =>
            {
                var graph = new BidirectionalGraph<object, IEdge<object>>();
                graph.AddVertex(p);
                foreach (var relation in _treeCreator.GetRelationsForTreeOfDescdendants(p.Name))
                {
                    var parent = Persons.First(x => x.Name == relation.Item1);
                    var child = Persons.First(x => x.Name == relation.Item2);

                    if (!graph.ContainsVertex(parent))
                    {
                        graph.AddVertex(parent);
                    }

                    if (!graph.ContainsVertex(child))
                    {
                        graph.AddVertex(child);
                    }

                    graph.AddEdge(new Edge<object>(parent, child));
                }
                Graph = graph;
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
    }
}