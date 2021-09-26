using System.Collections.Generic;
using System.Linq;

namespace StudentScheduler
{
    public class StudentBody
    {
        public IReadOnlyList<Student> Students { get; } = new List<Student>();
        public StudentBody(int sectionCount, int classes)
        {
            IEnumerable<string[]> allStudentPrefs = CSVReader.GetLinesFromFile().Skip(1).Select(line => line.Split(','));
            List<Student> students = new List<Student>();
            foreach (string[] studentPref in allStudentPrefs)
            {
                string name = studentPref.FirstOrDefault();
                List<int> prefs = studentPref.Skip(1).Where(s => int.TryParse(s, out int i) && i > 0).Select(i => int.Parse(i)).ToList();
                bool studentDoesntDeserveNiceThings = false;
                if (prefs.Count < classes)
                {
                    studentDoesntDeserveNiceThings = true;
                    UserInteraction.RuntimeSession.WriteLine(string.Format("{0} failed to provide enough selections :(", name));
                }
                if (prefs.Any(i => i > sectionCount))
                {
                    studentDoesntDeserveNiceThings = true;
                    UserInteraction.RuntimeSession.WriteLine(string.Format("{0} provided an out of range section selection :(", name));
                }
                students.Add(new Student(name, prefs, studentDoesntDeserveNiceThings, sectionCount));
            }
            Students = students;
        }
    }
}
