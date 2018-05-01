﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using NBD2.Model;
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
        public ObservableCollection<PersonViewModel> Persons { get; set; }

        public ICommand CreateCommand { get; }
        public ICommand EditCommand { get; }

        public MainViewModel(IPersonService personService, TreeCreator treeCreator)
        {
            _personService = personService;
            _treeCreator = treeCreator;
            CreateCommand = new RelayCommand(() =>
            {
                var w = new PersonCreateEdit(new PersonCreateEditViewModel(_personService));
                w.ShowDialog();
            }, () => true);
            EditCommand = new RelayCommand<PersonViewModel>(p =>
            {
                var w = new PersonCreateEdit(new PersonCreateEditViewModel(p, _personService));
                w.ShowDialog();
            }, p => true);

            Persons = new ObservableCollection<PersonViewModel>();
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