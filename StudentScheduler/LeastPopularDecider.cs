using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentScheduler
{
    public class LeastPopularDecider
    {
        private List<Section> AllSections { get; }
        private StudentBody StudentBody { get; }

        public LeastPopularDecider(IEnumerable<Section> sectionsNeedingAssignment, StudentBody studentBody)
        {
            AllSections = sectionsNeedingAssignment.ToList();
            StudentBody = studentBody;
        }

        internal void InitialAssignment()
        {
            List<int> orderToAssign = GetOrderToAssignIn();
            List<string> sectionTypes = AllSections.Select(s => s.SectionName).Distinct().ToList();
            foreach (int time in orderToAssign)
            {
                List<Section> eligibleSections = AllSections.Where(s => !s.IsFull() && s.SectionTime == time).ToList();
                List<Student> coolStudents = StudentBody.Students.Where(s => s.WouldConsider(time)).ToList();
                coolStudents.Sort(delegate (Student s1, Student s2)
                {
                    int pointsL = orderToAssign.Count - s1.Preferences.IndexOf(time);
                    pointsL += orderToAssign.Count - s1.Preferences.Count;
                    int pointsR = orderToAssign.Count - s2.Preferences.IndexOf(time);
                    pointsR += orderToAssign.Count - s2.Preferences.Count;
                    return pointsR.CompareTo(pointsL);
                });
                while (eligibleSections.Any(e => e.NeedsStudents()) && coolStudents.Count > 0)
                {
                    Student chillStudent = coolStudents.First();
                    coolStudents.RemoveAt(0);
                    Section potentialSection = eligibleSections.FirstOrDefault(s => !chillStudent.AssignedSections.Any(assigned => assigned.SectionName == s.SectionName));
                    if (potentialSection == null)
                    {
                        continue;
                    }
                    potentialSection.AddStudent(chillStudent);
                    if (potentialSection.IsFull())
                    {
                        eligibleSections.Remove(potentialSection);
                    }
                }
            }
            int ExpectedCount = AllSections.Select(s => s.SectionName).Distinct().Count();
            IEnumerable<Student> unassignedStudents = StudentBody.Students.Where(s => s.AssignedSections.Count != ExpectedCount);
            IEnumerable<Section> remaining = AllSections.Where(s => s.NeedsStudents());
            int count = 0;
            while (TryReduceStrain(unassignedStudents, remaining) && count < 100)
            {
                count++;
            }
            AssignUnhappyStudents(sectionTypes, unassignedStudents, remaining);
        }

        private static void AssignUnhappyStudents(List<string> sectionTypes, IEnumerable<Student> unassignedStudents, IEnumerable<Section> remaining)
        {
            List<Section> remainingSections = remaining.ToList();
            foreach (Student student in unassignedStudents)
            {
                foreach (string section in sectionTypes.Where(section => !student.HasSection(section)))
                {
                    Section re = remainingSections.FirstOrDefault(r => r.SectionName == section);
                    re.AddStudent(student);
                    if (re.IsFull())
                    {
                        remainingSections.Remove(re);
                    }
                }
            }
        }

        private bool TryReduceStrain(IEnumerable<Student> unassignedStudents, IEnumerable<Section> remaining)
        {
            List<Section> remainingSections = remaining.ToList();
            List<Student> fixers = StudentBody.Students.Where(stud => remainingSections.Any(sec => stud.WouldConsider(sec))).ToList();
            if (fixers.Count == 0)
            {
                return false;
            }
            foreach (Student student in unassignedStudents)
            {
                List<Section> preferedSections = student.GetLikableSections(AllSections);
                Student bestFixer = null;
                int minPriorityIndex = int.MaxValue;
                Section sectionToChangeIn = null;
                Section sectionToChangeOut = null;
                foreach (Section preffed in preferedSections)
                {
                    foreach ((Student fixer, Section rSection) in fixers.
                        Where(f => f.IsEnrolledIn(preffed)).
                        SelectMany(fixer => remainingSections.
                            Where(rs => fixer.WouldConsiderInsteadOf(rs, preffed)).
                            Where(openSection => fixer.GetPriorityIndex(openSection) < minPriorityIndex).
                            Select(rSection => (fixer, rSection))))
                    {
                        bestFixer = fixer;
                        minPriorityIndex = fixer.GetPriorityIndex(rSection);
                        sectionToChangeIn = rSection;
                        sectionToChangeOut = preffed;
                    }
                }
                if (bestFixer != null)
                {
                    bestFixer.ChangeSection(sectionToChangeIn);
                    student.EnrollInSection(sectionToChangeOut);
                    return true;
                }
            }
            return false;
        }

        private List<int> GetOrderToAssignIn()
        {
            int iterations = StudentBody.Students.Max(s => s.Preferences.Count);
            List<int> OrderToAssign = new List<int>();
            List<int> wouldConsiderCounts = new List<int>();
            for (int i = 1; i < iterations + 1; i++)
            {
                wouldConsiderCounts.Add(StudentBody.Students.Count(s => s.WouldConsider(i)));
            }
            List<int> UsableSorted = wouldConsiderCounts.ToList();
            UsableSorted.Sort();
            OrderToAssign = UsableSorted.Select(count => wouldConsiderCounts.IndexOf(count) + 1).Distinct().ToList();
            OrderToAssign = OrderToAssign.Concat(Enumerable.Range(1, iterations).Where(i => !OrderToAssign.Contains(i))).ToList();
            return OrderToAssign;
        }
    }
}
