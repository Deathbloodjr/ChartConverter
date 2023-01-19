using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverter.ChartData
{
    public enum Genre { Pops, Anime, Kids, Vocaloid, Game, Variety, Classic, Namco }
    public enum Difficulty { Easy, Normal, Hard, Oni, Ura }
    class Chart
    {
        public List<Measure> Measures { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }

        // Offset and DemoStart are in ms
        public float Offset { get; set; } // This one's weird in Fumen
        public float DemoStart { get; set; }

        public string Level { get; set; }
        public Difficulty Difficulty { get; set; }
        public Genre Genre { get; set; }

        // These are useless for TJA
        public string Wave { get; set; }
        public string ScoreInit { get; set; }
        public string ScoreDiff { get; set; }

        public int TotalNoteCount { get {
                int count = 0;
                for (int i = 0; i < Measures.Count; i++)
                {
                    for (int j = 0; j < Measures[i].Notes.Count; j++)
                    {
                        if (Measures[i].Notes[j].Type != NoteType.Balloon && Measures[i].Notes[j].Type != NoteType.Drumroll
                            && Measures[i].Notes[j].Type != NoteType.BigDrumroll && Measures[i].Notes[j].Type != NoteType.DrumrollEnd && Measures[i].Notes[j].Type != NoteType.None)
                        {
                            count += 1;
                        }
                    }
                }
                return count;
            } 
        }
        public Chart()
        {
            Measures = new List<Measure>();
        }
    }
}
