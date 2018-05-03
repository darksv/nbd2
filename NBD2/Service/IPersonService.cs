using System.Collections.Generic;
using NBD2.Model;

namespace NBD2.Service
{
    public interface IPersonService
    {
        bool IsNameTaken(string name);
        void Create(Person person);
        IEnumerable<Person> GetPossibleParentsFor(Person child, Sex sex);
        IEnumerable<Person> GetAll();
        void Update(string name, Person person);
        void DeletePerson(string name);
        void CreateRelation(string parent, string child, RelationType relationType);
        IEnumerable<Person> GetChildrenOf(string parentName);
        IEnumerable<Person> GetLivingDescendants(string parentName);
    }
}
