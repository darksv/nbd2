using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using NBD2.Model;

namespace NBD2.Service
{
    class PersonService : IPersonService
    {
        private readonly IEmbeddedObjectContainer _context;

        public PersonService()
        {
            _context = Open();
        }

        public bool IsNameTaken(string name)
        {
            return _context.AsQueryable<Person>().Any(x => x.Name == name);
        }

        public void Create(Person person)
        {
            _context.Store(person);
            _context.Commit();
        }

        private static IEmbeddedObjectContainer Open()
        {
            return Db4oEmbedded.OpenFile("persons.db");
        }

        public IEnumerable<Person> GetAll()
        {
            return _context.Query<Person>().ToArray();
        }

        public void Update(string name, Person person)
        {
            var oldPerson = _context.Query<Person>().FirstOrDefault(x => x.Name == name);
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
                foreach (var child in _context.Query<Person>().Where(x => x.FatherName == oldPerson.Name))
                {
                    child.FatherName = name;
                    _context.Store(child);
                }

                foreach (var child in _context.Query<Person>().Where(x => x.MotherName == oldPerson.Name))
                {
                    child.MotherName = name;
                    _context.Store(child);
                }
            }

            _context.Store(oldPerson);
            _context.Commit();
        }

        public void DeletePerson(string name)
        {
            var person = _context.Query<Person>().FirstOrDefault(x => x.Name == name);
            if (person == null)
            {
                return;
            }

            foreach (var child in _context.Query<Person>().Where(x => x.FatherName == name))
            {
                child.FatherName = null;
                _context.Store(child);
            }

            foreach (var child in _context.Query<Person>().Where(x => x.MotherName == name))
            {
                child.MotherName = null;
                _context.Store(child);
            }

            _context.Delete(person);
            _context.Commit();
        }

        public void CreateRelation(string parent, string child, RelationType relationType)
        {
            var c = _context.Query<Person>(p => p.Name == child).FirstOrDefault();
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

                _context.Store(c);
            }
        }

        public IEnumerable<Person> GetChildrenOf(string parentName)
        {
            return _context.Query<Person>(p => p.MotherName == parentName || p.FatherName == parentName).ToArray();
        }

        public IEnumerable<Person> GetLivingDescendants(string parentName)
        {
            var stack = new Stack<string>();
            stack.Push(parentName);
            while (stack.Any())
            {
                var parent = stack.Pop();
                foreach (var child in GetChildrenOf(parent))
                {
                    yield return child;
                    stack.Push(child.Name);
                }
            }
        }
    }
}