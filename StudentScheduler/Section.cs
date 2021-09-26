using System;
using System.Collections.Generic;
using System.Linq;

namespace StudentScheduler
{
    public class Section
    {
        public IReadOnlyCollection<Student> Students => _Students;
        private HashSet<Student> _Students { get; } = new HashSet<Student>();
        public Section(string sectionName, int maxStudentsPerOffering, int sectionTime, int offeringsPerSection)
        {
            SectionName = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
            MaxStudents = maxStudentsPerOffering * offeringsPerSection;
            SectionTime = sectionTime;
            OfferingsPerSection = offeringsPerSection;
        }
        public bool IsFull() => Students.Count >= MaxStudents;


        public string SectionName { get; }
        public int MaxStudents { get; }
        public int SectionTime { get; }
        private int OfferingsPerSection { get; }
        private int MaxStudentsPerOffering { get; }
        internal bool NeedsStudents()
        {
            return !IsFull();
        }

        internal void AddStudent(Student chillStudent)
        {
            if (_Students.Count == MaxStudents)
            {
                throw new Exception("Too many students!");
            }
            if (_Students.Contains(chillStudent))
            {
                throw new Exception("Already enrolled");
            }
            _Students.Add(chillStudent);
            chillStudent.AssignedSections.Add(this);
        }
        internal void RemoveStudent(Student student)
        {
            _Students.Remove(student);
            student.AssignedSections.Remove(this);
        }
        public string GetPrintable()
        {
            List<string> list = new List<string>(Students.Count + OfferingsPerSection);
            List<int> stusForEach = new List<int>();
            int target = MaxStudents / OfferingsPerSection;
            int takenSoFar = 0;
            for (char i = (char)0; i < OfferingsPerSection; i++)
            {
                char offeringLetter = (char)(i + 'A');
                list.Add(string.Format("{0} {1}{2}", SectionName, SectionTime,offeringLetter));
                list.AddRange(Students.Skip(takenSoFar).Take(target).Select(s => string.Format(" {0}", s.ClassName(this))));
                takenSoFar += target;
            }
            return string.Join("\n", list);
        }
        internal void PrintStudents()
        {
            UserInteraction.RuntimeSession.WriteLine(GetPrintable());
        }
    }
}
