using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverter.ChartData
{
    class Measure
    {
        public List<Note> Notes { get; set; }
        public int NoteCount { get { return Notes.Count; } }

        public bool isBarline { get; set; }
        public bool isGoGo { get; set; }
        public float BPM { get; set; }
        public float Scroll { get; set; }

        public int MeasureTop { get; set; }
        public int MeasureBottom { get; set; }
        public Measure()
        {
            Notes = new List<Note>();
            isBarline = true;
            Scroll = 1.0f;
            MeasureTop = 4;
            MeasureBottom = 4;
            BPM = 180;
        }

        public void ExpandMeasure(int noteCount = 0, int MeasureTop = 0)
        {
            List<int> noteIndices = new List<int>();
            List<Note> notes = new List<Note>();

            if (noteCount == 0)
            {
                noteCount = NoteCount;
            }
            if (MeasureTop == 0)
            {
                MeasureTop = this.MeasureTop;
            }


            for (int i = 0; i < Notes.Count; i++)
            {
                if (Notes[i].Type != NoteType.None)
                {
                    noteIndices.Add(i);
                    notes.Add(Notes[i]);
                }
            }

            // Pretty bad looping, but it works probably
            double Multiplier = 1.5;
            bool finished = false;
            while (!finished)
            {
                while (noteCount * Multiplier % MeasureTop != 0)
                {
                    Multiplier += 0.5;
                }
                int numTested = 0;
                for (int i = 0; i < noteIndices.Count; i++)
                {
                    if (noteIndices[i] * Multiplier % 1 != 0)
                    {
                        Multiplier += 0.5;
                        break;
                    }
                    else
                    {
                        numTested++;
                    }
                }
                if (numTested == noteIndices.Count)
                {
                    finished = true;
                }
            }
            for (int i = 0; i < noteIndices.Count; i++)
            {
                noteIndices[i] = (int)(noteIndices[i] * Multiplier);
            }
            Notes.Clear();
            for (int i = 0; i < noteCount * Multiplier; i++)
            {
                bool isNote = false;
                for (int j = 0; j < noteIndices.Count; j++)
                {
                    if (noteIndices[j] == i)
                    {
                        isNote = true;
                        Notes.Add(notes[j]);
                        break;
                    }
                }
                if (!isNote)
                {
                    var note = new Note();

                    note.Type = NoteType.None;
                    if (Notes.Count == 0)
                    {
                        note.BPM = this.BPM;
                        note.Scroll = this.Scroll;
                        note.isGoGo = this.isGoGo;
                    }
                    else
                    {
                        note.BPM = Notes[Notes.Count - 1].BPM;
                        note.Scroll = Notes[Notes.Count - 1].Scroll;
                        note.isGoGo = Notes[Notes.Count - 1].isGoGo;
                    }

                    Notes.Add(note);
                }
            }
        }
    }
}
