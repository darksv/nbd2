﻿using System;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using NBD2.Model;

namespace NBD2.Service
{
    class PersonService : IPersonService, IDisposable
    {
        private readonly IEmbeddedObjectContainer _context;
        private const string DatabaseFileName = "persons.db";

        public PersonService()
        {
            _context = Db4oEmbedded.OpenFile(DatabaseFileName);
        }

        public bool IsNameTaken(string name)
        {
            return _context.AsQueryable<Person>().Any(x => x.Name == name);
        }

        public void Create(Person person)
        {
            _context.Store(person);
            _context.Store(person.Children);
            _context.Commit();
        }

        public IEnumerable<Person> GetAll()
        {
            return _context.Query<Person>().ToArray();
        }

        public void Update(string name, Person person)
        {
            var originalPerson = _context.Query<Person>().FirstOrDefault(x => x.Name == name);
            if (originalPerson == null)
            {
                return;
            }

            originalPerson.Name = person.Name;
            originalPerson.DateOfBirth = person.DateOfBirth;
            originalPerson.DateOfDeath = person.DateOfDeath;
            originalPerson.Sex = person.Sex;
            originalPerson.FatherName = person.FatherName;
            originalPerson.MotherName = person.MotherName;

            if (originalPerson.Name != name)
            {
                foreach (var child in _context.Query<Person>().Where(x => x.FatherName == name))
                {
                    child.FatherName = person.Name;
                    _context.Store(child);
                }

                foreach (var child in _context.Query<Person>().Where(x => x.MotherName == name))
                {
                    child.MotherName = person.Name;
                    _context.Store(child);
                }

                foreach (var parent in _context.Query<Person>().Where(x => x.Children.Contains(name)))
                {
                    parent.Children.Remove(name);
                    parent.Children.Add(person.Name);
                    _context.Store(parent.Children);
                }
            }

            _context.Store(originalPerson);
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


            foreach (var parent in _context.Query<Person>(x => x.Children?.Contains(name) ?? false))
            {
                parent.Children.Remove(name);
                _context.Store(parent.Children);
            }

            _context.Delete(person);
            _context.Commit();
        }

        public IEnumerable<Person> GetChildrenOf(string parentName)
        {
            return _context.Query<Person>(p => p.MotherName == parentName || p.FatherName == parentName).ToArray();
        }

        public Person Get(string name)
        {
            return _context.Query<Person>().FirstOrDefault(x => x.Name == name);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}