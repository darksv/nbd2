using System;
using System.Collections.Generic;
using System.Linq;
using NBD2.Model;

namespace NBD2.Service
{
    public class TreeCreator
    {
        private readonly IPersonService _personService;

        public TreeCreator(IPersonService personService)
        {
            _personService = personService;
        }

        public IEnumerable<(string, string)> GetRelationsForTreeOfDescdendants(string person)
        {
            var stack = new Stack<string>();
            stack.Push(person);
            while (stack.Any())
            {
                var parent = stack.Pop();
                foreach (var child in _personService.GetChildrenOf(parent))
                {
                    yield return (parent, child.Name);
                    stack.Push(child.Name);
                }
            }
        }

        public IEnumerable<string> GetCommonAncestors(string firstPerson, string secondPerson)
        {
            var queue = new Queue<Person>();

            var firstAncestors  = new HashSet<Person>();
            queue.Enqueue(_personService.Get(firstPerson));
            while (queue.Any())
            {
                var child = queue.Dequeue();
                firstAncestors.Add(child);

                if (child.FatherName != null)
                {
                    queue.Enqueue(_personService.Get(child.FatherName));
                }

                if (child.MotherName != null)
                {
                    queue.Enqueue(_personService.Get(child.MotherName));
                }
            }

            var secondAncestors = new HashSet<Person>();
            queue.Enqueue(_personService.Get(secondPerson));
            while (queue.Any())
            {
                var child = queue.Dequeue();
                secondAncestors.Add(child);

                if (child.FatherName != null)
                {
                    queue.Enqueue(_personService.Get(child.FatherName));
                }

                if (child.MotherName != null)
                {
                    queue.Enqueue(_personService.Get(child.MotherName));
                }
            }

            return firstAncestors.Intersect(secondAncestors).Select(x => x.Name).Except(new[]{firstPerson, secondPerson});
        }

        public bool CanBeMotherOf(Person mother, Person child)
        {
            if (mother.Sex != Sex.Female)
            {
                return false;
            }

            if (mother.DateOfBirth.HasValue && child.DateOfBirth.HasValue)
            {
                var ageOfMother = (child.DateOfBirth.Value - mother.DateOfBirth.Value).TotalDays / 365;
                if (ageOfMother < 10 || ageOfMother > 60)
                {
                    return false;
                }

                if (mother.DateOfDeath.HasValue)
                {
                    if (child.DateOfBirth.Value <= mother.DateOfBirth ||
                        child.DateOfBirth.Value >= mother.DateOfDeath.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool CanBeFatherOf(Person father, Person child)
        {
            if (father.Sex != Sex.Male)
            {
                return false;
            }

            if (father.DateOfBirth.HasValue && child.DateOfBirth.HasValue)
            {
                var ageOfFather = (child.DateOfBirth.Value - father.DateOfBirth.Value).TotalDays / 365;
                if (ageOfFather < 12 || ageOfFather > 70)
                {
                    return false;
                }

                if (father.DateOfDeath.HasValue)
                {
                    if (child.DateOfBirth.Value <= father.DateOfBirth ||
                        child.DateOfBirth.Value >= father.DateOfDeath.Value + TimeSpan.FromDays(270))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool CanBeParentOf(string parentName, string childName)
        {
            var stack = new Stack<Person>();
            stack.Push(_personService.Get(parentName));
            while (stack.Any())
            {
                var person = stack.Pop();
                if (person.Name == childName)
                {
                    return false;
                }

                if (person.FatherName != null)
                {
                    stack.Push(_personService.Get(person.FatherName));
                }

                if (person.MotherName != null)
                {
                    stack.Push(_personService.Get(person.MotherName));
                }
            }

            return true;
        }

//        public IEnumerable<Person> GetDescendantsOf(string person)
//        {
//            var queue = new Queue<string>();
//            queue.Enqueue(person);
//            var emitted = new HashSet<Person>();
//            while (queue.Any())
//            {
//                var parent = queue.Dequeue();
//                foreach (var child in _personService.GetChildrenOf(parent))
//                {
//                    if (!emitted.Contains(child) && child.Name != person)
//                    {
//                        yield return child;
//                        emitted.Add(child);
//                    }
//                    queue.Enqueue(child.Name);
//                }
//            }
//        }

        public IEnumerable<Person> GetPossibleInheritors(string person)
        {
            var queue = new Queue<string>();
            queue.Enqueue(person);
            while (queue.Any())
            {
                var parent = queue.Dequeue();
                foreach (var child in _personService.GetChildrenOf(parent))
                {
                    if (child.DateOfDeath.HasValue)
                    {

                        queue.Enqueue(child.Name);
                    }
                    else
                    {
                        yield return child;

                    }
                }
            }
        }
    }
}
