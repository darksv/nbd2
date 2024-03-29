﻿using System;
using System.Collections.Generic;

namespace NBD2.Model
{
    public class Person
    {
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public Sex? Sex { get; set; }
        public string MotherName { get; set; }
        public string FatherName { get; set; }
        public List<string> Children { get; set; } = new List<string>();
    }
}
