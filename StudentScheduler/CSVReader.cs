using System.Collections.Generic;
using System.IO;

namespace StudentScheduler
{
    public static class CSVReader
    {
        public static IEnumerable<string> GetLinesFromFile()
        {
            UserInteraction.RuntimeSession.WriteLine(string.Format("A folder {0} was just made on your desktop. Copy and paste your csv file into it", UserInteraction.RootFilePath));
            UserInteraction.RuntimeSession.GetYesNo("Enter to continue");
            string[] list = Directory.GetFiles(UserInteraction.RootFilePath, "*.csv");
            if (list.Length == 0)
            {
                UserInteraction.RuntimeSession.WriteLine("Are you sure you copied and pasted? You might have an excel file. Open it in excel and convert it to a csv file. Restart to continue");
                return new List<string>();
            }
            if (list.Length > 1)
            {
                UserInteraction.RuntimeSession.WriteLine(string.Format("Too many files here: {0}\n\n Reduce this to one and then I'll run", string.Join("\n", list)));
                return new List<string>();
            }
            return File.ReadAllLines(list[0]);
        }
    }
}
