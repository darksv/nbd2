using System;
using System.Collections.Generic;
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
        private readonly PersonViewModel _person;
        private readonly TreeCreator _treeCreator;
        public string Name { get; set; }
        public Sex? Sex { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public string MotherName { get; set; }
        public string FatherName { get; set; }
        public ICommand SaveCommand { get; }
        public Mode Mode { get; }
        public string OriginalName => _person?.Name;
        public IEnumerable<ParentItem> PossibleMothers { get; private set; }
        public IEnumerable<ParentItem> PossibleFathers { get; private set; }

        public class ParentItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public PersonCreateEditViewModel(IPersonService personService, TreeCreator treeCreator)
        {
            _personService = personService;
            _treeCreator = treeCreator;
            _person = new PersonViewModel();

            SaveCommand = new RelayCommand(Save, CanSave);
            Mode = Mode.Create;

            UpdatePossibleParents();
        }

        public PersonCreateEditViewModel(PersonViewModel person, IPersonService personService, TreeCreator treeCreator)
        {
            _personService = personService ?? throw new ArgumentNullException(nameof(personService));
            _treeCreator = treeCreator;
            _person = person ?? throw new ArgumentNullException(nameof(person));

            Name = person.Name;
            Sex = person.Sex;
            DateOfBirth = person.DateOfBirth;
            DateOfDeath = person.DateOfDeath;
            MotherName = person.MotherName;
            FatherName = person.FatherName;

            SaveCommand = new RelayCommand(Save, CanSave);
            Mode = Mode.Edit;

            UpdatePossibleParents();
        }

        private void UpdatePossibleParents()
        {
            var nullParent = EnumerableExt.FromSingle(new ParentItem {Name = "(nie podano)", Value = null}).ToArray();
            PossibleFathers =
                nullParent.Concat(GetPossibleParents(Model.Sex.Male).Select(x => new ParentItem {Name = x, Value = x}));
            PossibleMothers = nullParent.Concat(GetPossibleParents(Model.Sex.Female)
                .Select(x => new ParentItem {Name = x, Value = x}));
        }

        private IEnumerable<string> GetPossibleParents(Sex sex)
        {
            var child = GetModel();
            var creator = new TreeCreator(_personService);
            var persons = _personService.GetAll();
            persons = sex == Model.Sex.Male
                ? persons.Where(x => creator.CanBeFatherOf(x, child))
                : persons.Where(x => creator.CanBeMotherOf(x, child));

            if (Mode == Mode.Edit)
            {
                persons = persons.Where(x => creator.CanBeParentOf(x.Name, _person.Name));
            }

            return persons.Select(x => x.Name);
        }

        private void Save()
        {
            string originalName = _person?.Name;
            if (Mode == Mode.Create)
            {
                _personService.Create(GetModel());
            }
            else
            {
                _personService.Update(originalName, GetModel());
            }

            _person.Name = Name;
            _person.Sex = Sex;
            _person.DateOfBirth = DateOfBirth;
            _person.DateOfDeath = DateOfDeath;
            _person.MotherName = MotherName;
            _person.FatherName = FatherName;

            OnSaved?.Invoke(this, new PersonCreateEditEventArgs {Person = _person, OriginalName = originalName});
        }

        private Person GetModel()
        {
            return new Person
            {
                Name = Name,
                Sex = Sex,
                DateOfBirth = DateOfBirth,
                DateOfDeath = DateOfDeath,
                MotherName = MotherName,
                FatherName = FatherName,
            };
        }

        private bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return false;
            }

            if (Mode == Mode.Create || Mode == Mode.Edit && Name != _person.Name)
            {
                if (_personService.IsNameTaken(Name))
                {
                    return false;
                }
            }

            if (MotherName != null)
            {
                if (!_treeCreator.CanBeMotherOf(_personService.Get(MotherName), GetModel()))
                {
                    return false;
                }
            }

            if (FatherName != null)
            {
                if (!_treeCreator.CanBeFatherOf(_personService.Get(FatherName), GetModel()))
                {
                    return false;
                }
            }

            if (Mode == Mode.Edit)
            {
                var children = _personService.GetChildrenOf(_person.Name);
                var parent = GetModel();

                foreach (var child in children)
                {
                    if (child.MotherName == _person.Name && !_treeCreator.CanBeMotherOf(parent, child))
                    {
                        return false;
                    }

                    if (child.FatherName == _person.Name && !_treeCreator.CanBeFatherOf(parent, child))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public event EventHandler<PersonCreateEditEventArgs> OnSaved;
    }

    public class PersonCreateEditEventArgs : EventArgs
    {
        public PersonViewModel Person { get; set; }
        public string OriginalName { get; set; }
    }

    public enum Mode
    {
        Create,
        Edit,
    }
}