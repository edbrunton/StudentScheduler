using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudentScheduler
{
    class Program
    {

        static void Main(string[] args)
        {
            UserInteraction.Initialize(args);
            SessionDetails session = UserInteraction.RuntimeSession;
            int numSectionTimes = 6;
            int maxStudents = 7;
            int otheringsPerSection = 2;
            if (int.TryParse(session.ReadLine(string.Format("The default number of section times is {0}. Enter an override or anything else to continue", numSectionTimes), true), out int newNum) && newNum > 0)
            {
                numSectionTimes = newNum;
            }
            if (int.TryParse(session.ReadLine(string.Format("The default number of student per section segment is {0}. Enter an override or anything else to continue", maxStudents), true), out newNum) && newNum > 0)
            {
                maxStudents = newNum;
            }
            if (int.TryParse(session.ReadLine(string.Format("The default number of segments per section is {0}. Enter an override or anything else to continue", otheringsPerSection), true), out newNum) && newNum > 0)
            {
                otheringsPerSection = newNum;
            }
            List<Class> classes = new List<Class>()
            {
                 new Class(numSectionTimes, "Foundary", maxStudents, otheringsPerSection),
                 new Class(numSectionTimes, "Machining", maxStudents,otheringsPerSection)
            };
            StudentBody studentBody = new StudentBody(numSectionTimes, classes.Count);
            IEnumerable<Section> sectionsNeedingAssignment = classes.SelectMany(c => c.Sections);
            LeastPopularDecider leastPopularDecider = new LeastPopularDecider(sectionsNeedingAssignment, studentBody);
            leastPopularDecider.InitialAssignment();
            foreach (Student student in studentBody.Students)
            {
                session.WriteLine(student.Details());
            }
            foreach (Section section in sectionsNeedingAssignment)
            {
                section.PrintStudents();
            }
            string forFile = string.Join("\n", studentBody.Students.Select(s => s.Details()).
                Concat(sectionsNeedingAssignment.Select(sec => sec.GetPrintable())));
            File.WriteAllText(UserInteraction.RootFilePath + "assignments.txt", forFile);
            int i = 0;
            foreach (Student student in studentBody.Students.Where(s => s.IsUnhappy()))
            {
                session.WriteLine(student.Name + " is unhappy");
                i++;
            }
            session.ReadLine("Total unhappy: " + i, true);
        }

    }
}
