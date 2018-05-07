using System.Collections.Generic;
using NBD2.Model;

namespace NBD2.Service
{
    public interface IPersonService
    {
        bool IsNameTaken(string name);
        void Create(Person person);
        IEnumerable<Person> GetAll();
        void Update(string name, Person person);
        void DeletePerson(string name);
        IEnumerable<Person> GetChildrenOf(string parentName);
        IEnumerable<Person> GetDescendantsOf(string person);
        Person Get(string name);
    }
}
