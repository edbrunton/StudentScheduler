using System.Collections.Generic;

namespace StudentScheduler
{
    public class Class
    {
        private List<Section> _sections { get; } = new List<Section>();
      public  IReadOnlyList<Section> Sections => _sections;
        public Class(int numSections, string className, int maxStudentsPerSection, int otheringsPerSection)
        {
            for(int i = 1; i <= numSections; i++)
            {
                _sections.Add(new Section(className, maxStudentsPerSection, i, otheringsPerSection));
            }
        }
    }
}
