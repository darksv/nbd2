using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using NBD2.Model;

namespace NBD2.Service
{
    class PersonService : IPersonService
    {
        public bool IsNameTaken(string name)
        {
            using (var context = Open())
            {
                return context.AsQueryable<Person>().Any(x => x.Name == name);
            }
        }

        public void Create(Person person)
        {
            using (var context = Open())
            {
                context.Store(person);
            }
        }

        private static IEmbeddedObjectContainer Open()
        {
            return Db4oEmbedded.OpenFile("persons.db");
        }

        public IEnumerable<Person> GetPossibleParentsFor(Person child, Sex sex)
        {
            return GetAll(); // TODO: implement proper logic
        }

        public IEnumerable<Person> GetAll()
        {
            using (var context = Open())
            {
                return context.Query<Person>().ToArray();
            }
        }

        public void Update(string name, Person person)
        {
            using (var context = Open())
            {
                var oldPerson = context.Query<Person>().FirstOrDefault(x => x.Name == name);
                if (oldPerson == null)
                {
                    return;
                }

                oldPerson.Name = person.Name;
                oldPerson.DateOfBirth = person.DateOfBirth;
                oldPerson.DateOfDeath = person.DateOfDeath;
                oldPerson.Sex = person.Sex;
                oldPerson.FatherName = person.FatherName;
                oldPerson.MotherName = person.MotherName;

                if (oldPerson.Name != name)
                {
                    foreach (var child in context.Query<Person>().Where(x => x.FatherName == oldPerson.Name))
                    {
                        child.FatherName = name;
                        context.Store(child);
                    }

                    foreach (var child in context.Query<Person>().Where(x => x.MotherName == oldPerson.Name))
                    {
                        child.MotherName = name;
                        context.Store(child);
                    }
                }

                context.Store(oldPerson);
            }
        }

        public void DeletePerson(string name)
        {
            using (var context = Open())
            {
                var person = context.Query<Person>().FirstOrDefault(x => x.Name == name);
                if (person == null)
                {
                    return;
                }

                foreach (var child in context.Query<Person>().Where(x => x.FatherName == name))
                {
                    child.FatherName = null;
                    context.Store(child);
                }

                foreach (var child in context.Query<Person>().Where(x => x.MotherName == name))
                {
                    child.MotherName = null;
                    context.Store(child);
                }

                context.Delete(person);
            }
        }

        public void CreateRelation(string parent, string child, RelationType relationType)
        {
            using (var context = Open())
            {
                var c = context.Query<Person>(p => p.Name == child).FirstOrDefault();
                if (c != null)
                {
                    if (relationType == RelationType.Mother)
                    {
                        c.MotherName = parent;
                    }
                    else
                    {
                        c.FatherName = parent;
                    }

                    context.Store(c);
                }
            }
        }

        public IEnumerable<string> GetChildrenOf(string parentName)
        {
            using (var context = Open())
            {
                return context
                    .Query<Person>(p => p.MotherName == parentName || p.FatherName == parentName)
                    .Select(x => x.Name).ToArray();
            }
        }
    }
}
