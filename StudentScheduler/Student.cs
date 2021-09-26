using System;
using System.Collections.Generic;
using System.Linq;

namespace StudentScheduler
{
    public class Student
    {
        public string Name { get; }
        public List<int> Preferences { get; }
        private HashSet<int> PrefSet { get; }
        public List<Section> AssignedSections { get; } = new List<Section>();
        public Student(string name, List<int> prefs, bool studentDoesntDeserveNiceThings, int sectionCount)
        {
            Name = name;
            if (studentDoesntDeserveNiceThings)
            {
                List<int> okayPrefs = prefs.Where(p => p <= sectionCount).ToList();
                Preferences = okayPrefs.Concat(Enumerable.Range(1, sectionCount).Where(i => !okayPrefs.Contains(i))).ToList();
            }
            else
            {
                Preferences = prefs;
            }
            PrefSet = Preferences.ToHashSet();
        }
        public bool HelpsGeneralCauseToChangeInto(Section s)
        {
            if (!CanChangeInto(s) || s.IsFull())
            {
                return false;
            }
            return s.Students.Count > AssignedSections.FirstOrDefault(sec => sec.SectionName == s.SectionName)?.Students.Count;
        }
        public bool CanChangeInto(Section s)
        {
            if (!WouldConsider(s))
            {
                return false;
            }
            Section conflict = AssignedSections.FirstOrDefault(sec => sec.SectionTime == s.SectionTime);
            if(conflict == null)
            {
                return true;
            }
            return false;//already in it or has competing section
        }
        public bool IsEnrolledIn(Section section)
        {
            return AssignedSections.Contains(section);
        }
        public bool WouldConsider(Section newEnrollment)
        {
            return WouldConsider(newEnrollment.SectionTime);
        }
        public bool WouldConsiderInsteadOf(Section newEnrollment, Section current)
        {
            return WouldConsider(newEnrollment.SectionTime) && current.SectionName == newEnrollment.SectionName && IsEnrolledIn(current);
        }
        public bool WouldConsider(int i)
        {
            return PrefSet.Contains(i);
        }
        public bool IsBusyAtTime(int time)
        {
            return AssignedSections.Count > 0 && AssignedSections.Any(s => s.SectionTime == time);
        }
        public bool HasSection(string section)
        {
            return AssignedSections.Count > 0 && AssignedSections.Any(a => a.SectionName == section);
        }
        public bool IsUnhappy()
        {
            return AssignedSections.Any(s => IsUnhappy(s));
        }
        private bool IsUnhappy(Section section)
        {
            return !PrefSet.Contains(section.SectionTime);
        }
        public int GetPriorityIndex(Section section)
        {
            return Preferences.IndexOf(section.SectionTime);
        }
        public void ChangeSection(Section section)
        {
            Section otherSection = AssignedSections.FirstOrDefault(a => a.SectionName == section.SectionName);
            if(otherSection == null)
            {
                throw new Exception("Cannot change to a section if not being removed from another");
            }
            otherSection.RemoveStudent(this);
            section.AddStudent(this);
        }
        public void EnrollInSection(Section section)
        {
            section.AddStudent(this);
        }

        internal string ClassName(Section section)
        {
            if (IsUnhappy(section))
            {
                return Name + " (unhappy)";
            }
            return Name;
        }
        public string Details() => string.Format("{0}: {1}\tPreferences [{2}]", Name,
            string.Join(", ", AssignedSections.Select(s => string.Format("{0} {1}", s.SectionName, s.SectionTime))),
            string.Join(",", Preferences));
        internal List<Section> GetLikableSections(List<Section> sectionsNeedingAssignment)
        {
            return sectionsNeedingAssignment.Where(sec => WouldConsider(sec.SectionTime) && !IsBusyAtTime(sec.SectionTime) && !AssignedSections.Any(s => s.SectionName == sec.SectionName)).ToList();
        }
    }
}
