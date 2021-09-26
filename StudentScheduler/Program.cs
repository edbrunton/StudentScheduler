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
            int offeringsPerSection = 2;
            session.WriteLine(@"Howdy, user. Source code is here https://github.com/edbrunton/StudentScheduler");
            if (int.TryParse(session.ReadLine(string.Format("The default number of section times is {0}. Enter an override or anything else to continue", numSectionTimes), true), out int newNum) && newNum > 0)
            {
                numSectionTimes = newNum;
            }
            if (int.TryParse(session.ReadLine(string.Format("The default number of segments per section is {0}. Enter an override or anything else to continue", offeringsPerSection), true), out newNum) && newNum > 0)
            {
                offeringsPerSection = newNum;
            }
            if (int.TryParse(session.ReadLine(string.Format("The default number of student per section segment is {0}. Enter an override or anything else to continue", maxStudents), true), out newNum) && newNum > 0)
            {
                maxStudents = newNum;
            }
            List<string> classNames = new List<string>()
            {
                "Foundary",
                "Machining"
            };
            if (session.ReadLine(string.Format("The default number classes are {0}. Enter an override or anything else to continue", string.Join(",", classNames)), true) is string overrideClasses && !string.IsNullOrWhiteSpace(overrideClasses))
            {
                if (overrideClasses.Contains(',') || session.GetYesNo(string.Format("You want to reduce down to only a single class called {0}, correct?", overrideClasses)) == UserInteraction.YesNo.Yes)
                {
                    classNames.Clear();
                    classNames.AddRange(overrideClasses.Split(','));
                }
            }
            List<Class> classes = new List<Class>(classNames.Count);
            classes.AddRange(classNames.Select(cn => new Class(numSectionTimes, cn, maxStudents, offeringsPerSection)));
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
            int i = 0;
            foreach (Student student in studentBody.Students.Where(s => s.IsUnhappy()))
            {
                session.WriteLine(student.Name + " is unhappy");
                i++;
            }
            session.WriteLine("Total unhappy: " + i);
            string fileLoc = UserInteraction.RootFilePath + "assignments.txt";
            File.WriteAllText(fileLoc, forFile);
            session.ReadLine(string.Format("File written to {0}", fileLoc), true);
            session.WriteExitLog(UserInteraction.Exit.Expected);
        }

    }
}
