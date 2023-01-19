using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverter.ChartData
{
    public enum NoteType { None, Don, Kat, BigDon, BigKat, Drumroll, BigDrumroll, Balloon, DrumrollEnd }
    class Note
    {
        public NoteType Type { get; set; }
        public float BPM { get; set; }
        public float Scroll { get; set; }
        public bool isGoGo { get; set; }
        public float Offset { get; set; }

        // DrumrollLength is in ms
        public float DrumrollLength { get; set; }
        public int BalloonCount { get; set; }
    }
}
