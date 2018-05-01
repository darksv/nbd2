﻿using System;
using System.Linq;
using System.Windows.Input;
using NBD2.Model;
using NBD2.Service;
using NBD2.Util;

namespace NBD2.ViewModel
{
    public class PersonCreateEditViewModel : BindableBase
    {
        private readonly IPersonService _personService;
//        private readonly string _originalName;
        public PersonViewModel Person { get; set; }
//        public string[] MotherNames { get; set; }
//        public string[] FatherNames { get; set; }
        public Sex[] Sexes { get; } = EnumUtils.GetValues<Sex>().ToArray();
        public ICommand SaveCommand { get; }
        private Mode Mode { get; set; }


        public PersonCreateEditViewModel(IPersonService personService)
        {
            _personService = personService;
            SaveCommand = new RelayCommand(Save, CanSave);
//            UpdateNames();
            Person = new PersonViewModel();
            Mode = Mode.Create;
        }

        public PersonCreateEditViewModel(PersonViewModel person, IPersonService personService)
        {
            _personService = personService ?? throw new ArgumentNullException(nameof(personService));
            Person = person ?? throw new ArgumentNullException(nameof(person));

            SaveCommand = new RelayCommand(Save, CanSave);
//            UpdateNames();
//            _originalName = person.Name;
            Mode = Mode.Edit;
        }

        private void Save()
        {
            if (Mode == Mode.Create)
            {
                _personService.Create(Person.GetModel());
            }
            else
            {
                _personService.Update(Person.Name, Person.GetModel());
            }
            OnSaved?.Invoke(this, EventArgs.Empty);
        }

        private bool CanSave()
        {
            if (Mode == Mode.Create)
            {
                return !string.IsNullOrWhiteSpace(Person.Name) && !_personService.IsNameTaken(Person.Name);
            }

            return true;
        }
//
//        private void UpdateNames()
//        {
//            MotherNames = _personService
//                .GetPossibleParentsFor(Person.GetModel(), Sex.Female)
//                .Select(x => x.Name)
//                .ToArray();
//
//            FatherNames = _personService
//                .GetPossibleParentsFor(Person.GetModel(), Sex.Male)
//                .Select(x => x.Name)
//                .ToArray();
//        }

        public event EventHandler OnSaved;
    }

    internal enum Mode
    {
        Create,
        Edit,
    }
}