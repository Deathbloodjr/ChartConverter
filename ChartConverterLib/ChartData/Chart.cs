using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverterLib.ChartData
{
    public enum Genre { Pops, Anime, Kids, Vocaloid, GameMusic, Variety, Classical, Namco }
    public enum Difficulty { Easy, Normal, Hard, Oni, Ura }
    public class Chart
    {
        public List<Measure> Measures { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }

        /// <summary>
        /// In ms.
        /// </summary>
        public float Offset { get; set; } // This one's weird in Fumen
        /// <summary>
        /// In ms.
        /// </summary>
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
                    count += Measures[i].NoteCount;
                    //for (int j = 0; j < Measures[i].Notes.Count; j++)
                    //{
                    //    if (Measures[i].Notes[j].Type != NoteType.Balloon && Measures[i].Notes[j].Type != NoteType.Drumroll
                    //        && Measures[i].Notes[j].Type != NoteType.BigDrumroll && Measures[i].Notes[j].Type != NoteType.DrumrollEnd && Measures[i].Notes[j].Type != NoteType.None)
                    //    {
                    //        count += 1;
                    //    }
                    //    else
                    //    {
                    //        if (Measures[i].Notes[j].Type != NoteType.None)
                    //        {
                    //            var tmp = Measures[i].Notes[j].Type;
                    //        }
                    //    }
                    //}
                }
                return count;
            } 
        }
        public Chart()
        {
            Measures = new List<Measure>();
            Level = "1";
        }

        public (int points, int score) GetPointsAndScore()
        {
            int numNotes = TotalNoteCount;
            int balloonCount = 0;
            float drumrollTime = 0.0f;

            for (int i = 0; i < Measures.Count; i++)
            {
                for (int j = 0; j < Measures[i].Notes.Count; j++)
                {
                    if (Measures[i].Notes[j].Type == NoteType.Drumroll || 
                        Measures[i].Notes[j].Type == NoteType.BigDrumroll)
                    {
                        var additionalDrumrollTime = (60000 / Measures[i].BPM) / (Measures[i].MeasureTop / (float)Measures[i].MeasureBottom) * (1.0f/12.0f);
                        drumrollTime += Measures[i].Notes[j].DrumrollLength + additionalDrumrollTime;
                    }
                    else if (Measures[i].Notes[j].Type == NoteType.Balloon)
                    {
                        var additionalDrumrollTime = (60000 / Measures[i].BPM) / (Measures[i].MeasureTop / (float)Measures[i].MeasureBottom) * (1.0f/12.0f);
                        int maxBalloon = (int)Math.Round(((Measures[i].Notes[j].DrumrollLength + additionalDrumrollTime) / 25));
                        if (maxBalloon < Measures[i].Notes[j].BalloonCount)
                        {
                            balloonCount += maxBalloon;
                        }
                        else
                        {
                            balloonCount += Measures[i].Notes[j].BalloonCount;
                        }
                    }
                }
            }

            //if (Balloon < 0 || Balloon > 10000)
            //{
            //    Balloon = 0;
            //}



            int TotalDrumrollPoints = (balloonCount * 100) + (int)(Math.Floor(((decimal)drumrollTime / 58)) * 100);

            int Points = (int)Math.Ceiling(((100000 - (TotalDrumrollPoints / 10)) / (decimal)numNotes)) * 10;

            int ScoreRank = Points * numNotes + TotalDrumrollPoints;

            return (Points, ScoreRank);
        }


    }
}
