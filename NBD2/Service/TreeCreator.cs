using System.Collections.Generic;
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

        public class TreeNode
        {
            public Person Parent { get; }
            public HashSet<TreeNode> Children { get; } = new HashSet<TreeNode>();

            public TreeNode(Person parent)
            {
                Parent = parent;
            }
        }

        public TreeNode BuildTreeForPerson(string name)
        {
            var relations = new Dictionary<string, HashSet<string>>();
            var peopleByName = new Dictionary<string, Person>();

            foreach (var person in _personService.GetAll())
            {
                InsertRelation(relations, person.FatherName, person.Name);
                InsertRelation(relations, person.MotherName, person.Name);
                peopleByName.Add(person.Name, person);
            }

            return CreateTreeRecursivelyFor(name, relations, peopleByName);
        }

        private TreeNode CreateTreeRecursivelyFor(
            string personName, 
            Dictionary<string, HashSet<string>> relations,
            Dictionary<string, Person> peopleByName)
        {
            TreeNode parent = null;
            if (relations.TryGetValue(personName, out var children))
            {
                parent = new TreeNode(peopleByName[personName]);
                foreach (var childName in children)
                {
                    parent.Children.Add(CreateTreeRecursivelyFor(childName, relations, peopleByName));
                }
            }
            return parent;
        }
//
//        private TreeNode CreateTreeRecursivelyFor2(
//            string personName,
//            Dictionary<string, HashSet<string>> relations,
//            Dictionary<string, Person> peopleByName)
//        {
//            TreeNode parent = null;
//            if (relations.TryGetValue(personName, out var children))
//            {
//                parent = new TreeNode(peopleByName[personName]);
//                foreach (var childName in children)
//                {
//                    parent.Children.Add(CreateTreeRecursivelyFor(childName, relations, peopleByName));
//                }
//            }
//            return parent;
//        }

        private void InsertRelation(IDictionary<string, HashSet<string>> relations, string parentName, string childName)
        {
            if (parentName != null)
            {
                if (!relations.ContainsKey(parentName))
                {
                    relations.Add(parentName, new HashSet<string>());
                }

                relations[parentName].Add(childName);
            }
        }
    }
}
