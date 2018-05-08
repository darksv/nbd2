using System.Collections.Generic;
using System.ComponentModel;
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
        public DeeplyObservableCollection<PersonViewModel> Persons { get; } = new DeeplyObservableCollection<PersonViewModel>();
        public IBidirectionalGraph<object, IEdge<object>> Graph { get; private set; }
        public PersonViewModel SelectedPerson { get; set; }
        public IEnumerable<string> PossibleInheritors { get; set; }
        public bool CanShowCommonAncestors { get; set; }
        public IEnumerable<string> CommonAncestors { get; set; }

        public ICommand CreateCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainViewModel(IPersonService personService, TreeCreator treeCreator)
        {
            _personService = personService;
            _treeCreator = treeCreator;
            Persons.ItemPropertyChanged += ItemOnPropertyChanged;
            CreateCommand = new RelayCommand(() =>
            {
                var createViewModel = new PersonCreateEditViewModel(_personService, _treeCreator);
                createViewModel.OnSaved += (s, e) => Persons.Add(e.Person);
                var window = new PersonCreateEdit(createViewModel);
                window.ShowDialog();
            });
            EditCommand = new RelayCommand<PersonViewModel>(p =>
            {
                var editViewModel = new PersonCreateEditViewModel(p, _personService, _treeCreator);
                editViewModel.OnSaved += (s, e) =>
                {
                    foreach (var personViewModel in Persons)
                    {
                        if (personViewModel.FatherName == e.OriginalName)
                        {
                            personViewModel.FatherName = e.Person.Name;
                        }

                        if (personViewModel.MotherName == e.OriginalName)
                        {
                            personViewModel.MotherName = e.Person.Name;
                        }
                    }
                    OnSelectedPersonChanged();
                };
                var window = new PersonCreateEdit(editViewModel);
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
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                _personService.DeletePerson(p.Name);
                if (SelectedPerson == p)
                {
                    SelectedPerson = null;
                    OnSelectedPersonChanged();
                    OnPropertyChanged();
                }
                UpdateList();
            });
            RefreshCommand = new RelayCommand(() =>
            {
                UpdateList();
                OnSelectedPersonChanged();
            });
            UpdateList();
        }


        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PersonViewModel.IsSelected))
            {
                return;
            }

            var selectedPersons = Persons.Where(x => x.IsSelected).ToArray();
            CanShowCommonAncestors = selectedPersons.Length == 2;
            if (CanShowCommonAncestors)
            {
                CommonAncestors = _treeCreator.GetCommonAncestors(selectedPersons[0].Name, selectedPersons[1].Name);
            }
        }

        private void UpdateList()
        {
            Persons.Clear();
            var persons = _personService.GetAll();
            foreach (var person in persons)
            {
                Persons.Add(new PersonViewModel
                {
                    Name = person.Name,
                    DateOfBirth = person.DateOfBirth,
                    DateOfDeath = person.DateOfDeath,
                    Sex = person.Sex,
                    MotherName = person.MotherName,
                    FatherName = person.FatherName,
                });
            }
        }

        protected void OnSelectedPersonChanged()
        {
            PossibleInheritors = SelectedPerson == null ? null : _treeCreator.GetPossibleInheritors(SelectedPerson.Name).Select(x => x.Name);

            var graph = new BidirectionalGraph<object, IEdge<object>>();
            if (SelectedPerson != null)
            {
                graph.AddVertex(SelectedPerson);
                foreach (var relation in _treeCreator.GetRelationsForTreeOfDescdendants(SelectedPerson.Name))
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
            }

            Graph = graph;
//            PossibleInheritors = _treeCreator.GetDescendantsOf(SelectedPerson.Name)
//                .Where(x => !x.DateOfDeath.HasValue)
//                .Select(x => x.Name);
            
        }
    }
}