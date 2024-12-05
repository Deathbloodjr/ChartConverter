using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverterLib.ChartData
{
    public class Measure
    {
        public List<Note> Notes { get; set; }
        public List<Note> ProfBranchNotes { get; set; }
        public List<Note> MasterBranchNotes { get; set; }
        public int NoteCount { 
            get 
            {
                int count = 0;
                for (int j = 0; j < Notes.Count; j++)
                {
                    if (Notes[j].Type == NoteType.Don || Notes[j].Type == NoteType.BigDon ||
                        Notes[j].Type == NoteType.Kat || Notes[j].Type == NoteType.BigKat)
                    {
                        count += 1;
                    }
                    //if (Notes[j].Type != NoteType.Balloon && Notes[j].Type != NoteType.Drumroll
                    //    && Notes[j].Type != NoteType.BigDrumroll && Notes[j].Type != NoteType.DrumrollEnd && Notes[j].Type != NoteType.None)
                    //{
                    //    count += 1;
                    //}
                    else
                    {
                        if (Notes[j].Type != NoteType.None)
                        {
                            var tmp = Notes[j].Type;
                        }
                    }
                }
                return count;
            } 
        }

        public int ProfessionalNoteCount
        {
            get
            {
                int count = 0;
                for (int j = 0; j < ProfBranchNotes.Count; j++)
                {
                    if (ProfBranchNotes[j].Type == NoteType.Don || ProfBranchNotes[j].Type == NoteType.BigDon ||
                        ProfBranchNotes[j].Type == NoteType.Kat || ProfBranchNotes[j].Type == NoteType.BigKat)
                    {
                        count += 1;
                    }
                    //if (Notes[j].Type != NoteType.Balloon && Notes[j].Type != NoteType.Drumroll
                    //    && Notes[j].Type != NoteType.BigDrumroll && Notes[j].Type != NoteType.DrumrollEnd && Notes[j].Type != NoteType.None)
                    //{
                    //    count += 1;
                    //}
                    else
                    {
                        if (ProfBranchNotes[j].Type != NoteType.None)
                        {
                            var tmp = ProfBranchNotes[j].Type;
                        }
                    }
                }
                return count;
            }
        }

        public int MasterNoteCount
        {
            get
            {
                int count = 0;
                for (int j = 0; j < MasterBranchNotes.Count; j++)
                {
                    if (MasterBranchNotes[j].Type == NoteType.Don || MasterBranchNotes[j].Type == NoteType.BigDon ||
                        MasterBranchNotes[j].Type == NoteType.Kat || MasterBranchNotes[j].Type == NoteType.BigKat)
                    {
                        count += 1;
                    }
                    //if (Notes[j].Type != NoteType.Balloon && Notes[j].Type != NoteType.Drumroll
                    //    && Notes[j].Type != NoteType.BigDrumroll && Notes[j].Type != NoteType.DrumrollEnd && Notes[j].Type != NoteType.None)
                    //{
                    //    count += 1;
                    //}
                    else
                    {
                        if (MasterBranchNotes[j].Type != NoteType.None)
                        {
                            var tmp = MasterBranchNotes[j].Type;
                        }
                    }
                }
                return count;
            }
        }

        // offset = RealTime - (240000/BPM)
        // RealTime = offset + (240000/BPM)
        public float Offset { get; set; }

        public bool isBarline { get; set; }
        public bool isGoGo { get; set; }
        public float BPM { get; set; }
        public float ScrollSpeed { get; set; }
        public float ProfScrollSpeed { get; set; }
        public float MasterScrollSpeed { get; set; }

        public int MeasureTop { get; set; }
        public int MeasureBottom { get; set; }
        public Measure()
        {
            Notes = new List<Note>();
            ProfBranchNotes = new List<Note>();
            MasterBranchNotes = new List<Note>();
            isBarline = true;
            ScrollSpeed = 1.0f;
            MeasureTop = 4;
            MeasureBottom = 4;
            BPM = 180;
        }

        public int GetBeatChar()
        {
            while (Notes.Count % MeasureTop != 0)
            {
                ExpandMeasure(Notes.Count, MeasureTop);
            }
            return Notes.Count / MeasureTop;
        }
        // Only used for WriteTXT
        private void ExpandMeasure(int noteCount, int MeasureTop)
        {
            List<int> noteIndices = new List<int>();
            List<Note> notes = new List<Note>();

            for (int i = 0; i < Notes.Count; i++)
            {
                noteIndices.Add(i);
                notes.Add(Notes[i]);
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
                    // I didn't set note.Offset to anything
                    // Only fumen uses note.Offset
                    // Only notes with type of None will be missing Offset
                    // Fumen doesn't use notes with type of None
                    if (i == 0)
                    {
                        // I don't think it should ever go in here
                        // I'll set the parameters to the base measure parameters
                        note.BPM = BPM;
                        note.Scroll = ScrollSpeed;
                        note.isGoGo = isGoGo;
                    }
                    else
                    {
                        note.BPM = Notes[i - 1].BPM;
                        note.Scroll = Notes[i - 1].Scroll;
                        note.isGoGo = Notes[i - 1].isGoGo;
                    }
                    note.Type = NoteType.None;
                    Notes.Add(note);
                }
            }
        }

        public void AdjustForBalloons()
        {
            bool needsExpanding = false;
            for (int i = 0; i < Notes.Count; i++)
            {
                if (Notes[i].Type == NoteType.Balloon)
                {
                    if (needsExpanding)
                    {
                        break;
                    }
                    int emptySpacesRequired = 2 + Notes[i].BalloonCount.ToString().Length;
                    if (emptySpacesRequired + 1 + i > Notes.Count)
                    {
                        needsExpanding = true;
                        break;
                    }
                    for (int j = i + 1; j < i + emptySpacesRequired; j++)
                    {
                        if (Notes[j].Type != NoteType.None)
                        {
                            needsExpanding = true;
                            break;
                        }
                    }
                }
            }

            if (needsExpanding)
            {
                List<Note> oldNotes = new List<Note>();
                for (int i = 0; i < Notes.Count; i++)
                {
                    oldNotes.Add(Notes[i]);
                }
                Notes = new List<Note>();
                for (int i = 0; i < oldNotes.Count; i++)
                {
                    Notes.Add(oldNotes[i]);
                    Note newNote = new Note();
                    newNote.BPM = oldNotes[i].BPM;
                    newNote.Scroll = oldNotes[i].Scroll;
                    newNote.isGoGo = oldNotes[i].isGoGo;
                    newNote.Type = NoteType.None;
                    Notes.Add(newNote);
                }
                AdjustForBalloons();
            }
        }

        public string GenerateParameterChangeString()
        {
            string returnString = string.Empty;
            for (int i = 1; i < Notes.Count; i++)
            {
                if (Notes[i].BPM != Notes[i - 1].BPM)
                {
                    int adjustedBeatChar = GetBeatChar() * (MeasureBottom / 4);

                    int GCF = GCD(i, adjustedBeatChar);
                    int firstNum = adjustedBeatChar / GCF;

                    int secondNum = i / GCF;

                    returnString += "#bpm " + Notes[i].BPM + " " + firstNum + " " + secondNum + "\n";
                }
                if (Notes[i].Scroll != Notes[i - 1].Scroll)
                {
                    int adjustedBeatChar = GetBeatChar() * (MeasureBottom / 4);

                    int GCF = GCD(i, adjustedBeatChar);
                    int firstNum = adjustedBeatChar / GCF;

                    int secondNum = i / GCF;

                    returnString += "#hs " + Notes[i].Scroll + " " + firstNum + " " + secondNum + "\n";
                }
                if (Notes[i].isGoGo != Notes[i - 1].isGoGo)
                {
                    int adjustedBeatChar = GetBeatChar() * (MeasureBottom / 4);

                    int GCF = GCD(i, adjustedBeatChar);
                    int firstNum = adjustedBeatChar / GCF;

                    int secondNum = i / GCF;

                    if (Notes[i].isGoGo)
                    {
                        returnString += "#begingogo " + firstNum + " " + secondNum + "\n";
                    }
                    else
                    {
                        returnString += "#endgogo " + firstNum + " " + secondNum + "\n";
                    }
                }
            }

            return returnString;
        }

        private static int GCD(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }
    }
}
