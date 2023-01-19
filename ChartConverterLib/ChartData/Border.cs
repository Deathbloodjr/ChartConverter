using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverterLib.ChartData
{
    public enum ConditionType { SoulGauge, Goods, OKs, Bads, Score, Drumrolls, HitCount, Combo }
    public enum ConditionComparer { More, Less }
    public class Border
    {
        public ConditionType Condition { get; set; }
        public ConditionComparer ConditionComparer { get; set; }
        /// <summary>
        /// True if all songs share the same requirements
        /// False if each song has its own requirements
        /// </summary>
        public bool IsGoThrough { get; set; }
        public List<int> RedRequirement { get; set; }
        public List<int> GoldRequirement { get; set; }

        public Border(ConditionType type, ConditionComparer comparer, bool isGoThrough, List<int> red, List<int> gold)
        {
            RedRequirement = new List<int>();
            GoldRequirement = new List<int>();
            Condition = type;
            ConditionComparer = comparer;
            IsGoThrough = isGoThrough;
            for (int i = 0; i < red.Count; i++)
            {
                RedRequirement.Add(red[i]);
            }
            for (int i = 0; i < gold.Count; i++)
            {
                GoldRequirement.Add(gold[i]);
            }
        }
    }
}
