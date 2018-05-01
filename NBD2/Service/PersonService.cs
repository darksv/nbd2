﻿using System.Collections.Generic;
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
                if (oldPerson != null)
                {
                    oldPerson.Name = person.Name;
                    oldPerson.DateOfBirth = person.DateOfBirth;
                    oldPerson.DateOfDeath = person.DateOfDeath;
                    oldPerson.Sex = person.Sex;
                    oldPerson.FatherName = person.FatherName;
                    oldPerson.MotherName = person.MotherName;
                    context.Store(oldPerson);
                }
            }
        }
    }
}