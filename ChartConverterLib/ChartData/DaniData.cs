using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverterLib.ChartData
{
    public class DaniData
    {
        public string Title { get; set; }
        public string DanSeries { get; set; }
        public List<Chart> Charts { get; set; }
        public List<Border> Borders { get; set; }

        /// <summary>
        /// This isn't used for anything, but it's nice to keep things organized and I'm bad at programming
        /// </summary>
        public List<string> AudioFilePaths { get; set; }
        public DaniData()
        {
            Charts = new List<Chart>();
            Borders = new List<Border>();
            AudioFilePaths = new List<string>();
        }

        public bool isValidRequirementsCount()
        {
            for (int i = 0; i < Borders.Count; i++)
            {
                if (!Borders[i].IsGoThrough)
                {
                    if (Charts.Count != Borders[i].RedRequirement.Count || Charts.Count != Borders[i].GoldRequirement.Count)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
