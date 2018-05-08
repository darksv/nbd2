using System;
using System.Collections.ObjectModel;
using NBD2.Model;

namespace NBD2.ViewModel
{
    public class PersonViewModel : BindableBase
    {
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public Sex? Sex { get; set; }
        public PersonViewModel Mother { get; set; }
        public string MotherName { get; set; }
        public PersonViewModel Father { get; set; }
        public string FatherName { get; set; }
//        public ObservableCollection<PersonViewModel> Children { get; set; }
        public bool IsSelected { get; set; }

        public Person GetModel()
        {
            return new Person
            {
                Name = Name,
                DateOfBirth = DateOfBirth,
                DateOfDeath = DateOfDeath,
                Sex = Sex,
                FatherName = Father?.Name,
                MotherName = Mother?.Name,
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
