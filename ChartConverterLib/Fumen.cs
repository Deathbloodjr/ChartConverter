using ChartConverterLib.ChartData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverterLib
{
    public class Fumen
    {
        public Chart ReadFumen(string FilePath, bool AddTimeSignature = false)
        {
            var chart = new Chart();
            int offset = 0x1B0;
            var Bytes = File.ReadAllBytes(FilePath);

            //Chart.HasDivergePaths = BitConverter.ToBoolean(Bytes, offset);
            offset += 0x04;
            //Chart.MaxHP = BitConverter.ToInt32(Bytes, offset);
            offset += 0x04;
            //Chart.ClearHP = BitConverter.ToInt32(Bytes, offset);
            offset += 0x04;
            //Chart.HPPerGood = BitConverter.ToInt32(Bytes, offset);
            offset += 0x04;
            //Chart.HPPerOk = BitConverter.ToInt32(Bytes, offset);
            offset += 0x04;
            //Chart.HPPerBad = BitConverter.ToInt32(Bytes, offset);
            offset = 0x200;
            var NumMeasures = BitConverter.ToInt32(Bytes, offset);
            offset += 0x08;
            for (int i = 0; i < NumMeasures; i++)
            {
                var measure = ReadMeasure(Bytes, ref offset);
                if (measure == null)
                {
                    return null;
                }
                chart.Measures.Add(measure);
            }

            AddDrumrollEnds(chart);


            if (AddTimeSignature)
            {

                AddCorrectTimeSignatures(chart);

                FillEmptyNotes(chart);

                // TODO: Fill Empty Notes sometimes removes notes, which ruins the point/score calculations
            }


            AddNoteInfo(chart);

            if (chart.TotalNoteCount == 0)
            {
                return null;
            }

            return chart;
        }

        private Measure ReadMeasure(byte[] Bytes, ref int offset)
        {
            try
            {
                Measure Measure = new Measure();
                Measure.BPM = BitConverter.ToSingle(Bytes, offset);
                offset += 0x04;
                Measure.Offset = BitConverter.ToSingle(Bytes, offset);
                offset += 0x04;
                Measure.isGoGo = BitConverter.ToBoolean(Bytes, offset);
                offset += 0x01;
                Measure.isBarline = BitConverter.ToBoolean(Bytes, offset);
                offset += 0x03 + (0x04 * 7);

                var NumNotes = BitConverter.ToInt16(Bytes, offset);
                offset += 0x04;
                Measure.Scroll = BitConverter.ToSingle(Bytes, offset);
                offset += 0x04;

                for (int i = 0; i < NumNotes; i++)
                {
                    var note = ReadNote(Bytes, ref offset);
                    if (note == null)
                    {
                        return null;
                    }
                    Measure.Notes.Add(note);
                }
                // This is assuming no branch paths, which I want to ignore anyway because I'm lazy
                offset += (0x04 * 4);
                return Measure;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Note. 01 - ドン, 02 - ド, 03 - コ, 04 - カッ, 05 - カ, 06 - 連打, 07 - ドン(大), 08 - カッ,(大) 09 - 連打（大）, 0A - Balloon, 0B - , 0C - Bell.
        private Note ReadNote(byte[] Bytes, ref int offset)
        {
            Note Note = new Note();
            if (offset >= Bytes.Length)
            {
                return null;
            }
            var noteTypeValue = BitConverter.ToInt32(Bytes, offset);
            switch (noteTypeValue)
            {
                case 0x01:
                    Note.Type = NoteType.Don;
                    break;
                case 0x02:
                    Note.Type = NoteType.Don;
                    break;
                case 0x03:
                    Note.Type = NoteType.Don;
                    break;
                case 0x04:
                    Note.Type = NoteType.Kat;
                    break;
                case 0x05:
                    Note.Type = NoteType.Kat;
                    break;
                case 0x06:
                    Note.Type = NoteType.Drumroll;
                    break;
                case 0x07:
                    Note.Type = NoteType.BigDon;
                    break;
                case 0x08:
                    Note.Type = NoteType.BigKat;
                    break;
                case 0x09:
                    Note.Type = NoteType.BigDrumroll;
                    break;
                case 0x0A:
                    Note.Type = NoteType.Balloon;
                    break;
                case 0x0C: // Should be mallet (probably), definitely not balloon
                    Note.Type = NoteType.Balloon;
                    break;
                default:
                    Note.Type = NoteType.Don; // Just in case
                    break;
            }
            offset += 0x04;
            Note.Offset = BitConverter.ToSingle(Bytes, offset);
            offset += 0x0C;
            if (Note.Type == NoteType.Balloon) // Is balloon
            {
                Note.BalloonCount = BitConverter.ToInt32(Bytes, offset);
            }
            offset += 0x04;
            if (Note.Type == NoteType.Balloon || Note.Type == NoteType.Drumroll || Note.Type == NoteType.BigDrumroll) // Probably also need Mallets in here, idk what bell is, might be mallets
            {
                Note.DrumrollLength = BitConverter.ToSingle(Bytes, offset);
            }
            if (Note.Type == NoteType.Drumroll || Note.Type == NoteType.BigDrumroll)
            {
                offset += 0x08;
            }
            offset += 0x04;
            return Note;
        }
        public void AddCorrectTimeSignatures(Chart chart, bool highAccuracy = false)
        {
            Utility.AddCorrectTimeSignatures(chart, highAccuracy);
        }
        public void AddNoteInfo(Chart chart)
        {
            Utility.AddNoteInfo(chart);
        }
        public void FillEmptyNotes(Chart chart, bool fullAccuracy = false)
        {
            Utility.FillEmptyNotes(chart, fullAccuracy);
        }

        public void AddDrumrollEnds(Chart chart)
        {
            Utility.AddDrumrollEnds(chart);
        }

        int numMeasures = 0;
        public void WriteFumen(string FilePath, Chart chart)
        {
            List<byte> Bytes = new List<byte>();

            #region Header Bytes
            // The initial timing window data
            for (int i = 0; i < 36; i++)
            {
                Bytes.Add(0x34);
                Bytes.Add(0x33);
                Bytes.Add(0xC8);
                Bytes.Add(0x41);

                Bytes.Add(0x67);
                Bytes.Add(0x26);
                Bytes.Add(0x96);
                Bytes.Add(0x42);

                Bytes.Add(0x22);
                Bytes.Add(0xE2);
                Bytes.Add(0xD8);
                Bytes.Add(0x42);
            }

            // Whether the chart has divergent paths
            for (int i = 0; i < 4; i++)
            {
                Bytes.Add(0x00);
            }

            // Max HP
            byte[] tmpBytes = BitConverter.GetBytes(10000);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            // Clear HP
            tmpBytes = BitConverter.GetBytes(8000);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            // HP Per Good
            int goodHP = 13000 / chart.TotalNoteCount;
            tmpBytes = BitConverter.GetBytes(goodHP);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            // HP Per Ok
            tmpBytes = BitConverter.GetBytes(goodHP / 2);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            // HP Per Bad
            tmpBytes = BitConverter.GetBytes(goodHP * -2);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            // Dummy data
            tmpBytes = BitConverter.GetBytes(65536);
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < tmpBytes.Length; i++)
                {
                    Bytes.Add(tmpBytes[i]);
                }
            }
            // Good diverge points
            tmpBytes = BitConverter.GetBytes(20);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // OK diverge points
            tmpBytes = BitConverter.GetBytes(10);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // Bad diverge points
            for (int i = 0; i < 4; i++)
            {
                Bytes.Add(0x00);
            }
            // Drumroll diverge points
            tmpBytes = BitConverter.GetBytes(1);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // Big note Good diverge points
            tmpBytes = BitConverter.GetBytes(20);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // Big note OK diverge points
            tmpBytes = BitConverter.GetBytes(10);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // Big Drumroll diverge points
            tmpBytes = BitConverter.GetBytes(1);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // Balloon diverge points
            tmpBytes = BitConverter.GetBytes(30);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // Bell/yam (mallet thing?) diverge points
            tmpBytes = BitConverter.GetBytes(30);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // Number of diverge points? maybe deprecated?
            tmpBytes = BitConverter.GetBytes(20);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // deprecated variable, dummy value now
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }


            // Number of measures
            // Set it to 0 for now, come back to it later when the real value is determined
            //int numMeasures = 0;
            int measureNumberIndex = Bytes.Count;
            for (int i = 0; i < 4; i++)
            {
                Bytes.Add(0x00);
            }
            // Dummy data
            for (int i = 0; i < 4; i++)
            {
                Bytes.Add(0x00);
            }
            #endregion

            // Measure Data:
            float offset = chart.Offset;
            for (int i = 0; i < chart.Measures.Count; i++)
            {
                //while(chart.Measures[i].NoteCount % chart.Measures[i].MeasureTop != 0)
                //{
                //    chart.Measures[i].ExpandMeasure();
                //}
                List<byte> measureData = new List<byte>();
                if (i < chart.Measures.Count - 1)
                {
                    measureData = WriteMeasure(chart.Measures[i], ref offset, chart.Measures[i + 1]);
                }
                else
                {
                    measureData = WriteMeasure(chart.Measures[i], ref offset);
                }
                for (int j = 0; j < measureData.Count; j++)
                {
                    Bytes.Add(measureData[j]);
                }
            }
            tmpBytes = BitConverter.GetBytes(numMeasures);
            for (int i = 0; i < 4; i++)
            {
                Bytes[measureNumberIndex + i] = tmpBytes[i];
            }

            Directory.CreateDirectory(FilePath.Remove(FilePath.LastIndexOf("\\")));
            File.WriteAllBytes(FilePath, Bytes.ToArray());
        }
        private List<byte> WriteMeasure(Measure measure, ref float offset, Measure nextMeasure = null, int startIndex = 0)
        {
            List<byte> Bytes = new List<byte>();
            float nextBPM = FindNextBPM(measure, startIndex, nextMeasure);

            // BPM
            var tmpBytes = BitConverter.GetBytes(measure.Notes[startIndex].BPM);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            // Offset
            //offset += (240000 / previousBpm) + ((240000 / previousBpm) * (measure.MeasureTop / measure.MeasureBottom)) - (240000 / measure.BPM);
            tmpBytes = BitConverter.GetBytes(offset);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            // Gogo time
            tmpBytes = BitConverter.GetBytes(measure.Notes[startIndex].isGoGo);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            var isBarline = false;
            if (startIndex == 0)
            {
                isBarline = measure.isBarline;
            }
            tmpBytes = BitConverter.GetBytes(isBarline);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            for (int i = 0; i < 2; i++)
            {
                Bytes.Add(0x00);
            }
            for (int i = 0; i < 4 * 6; i++)
            {
                Bytes.Add(0xFF);
            }
            for (int i = 0; i < 4; i++)
            {
                Bytes.Add(0x00);
            }

            int numNotes = 0;
            int noteCountIndex = Bytes.Count;
            // 2 Bytes for numNotes, 2 Bytes for dummy data
            for (int i = 0; i < 4; i++)
            {
                Bytes.Add(0x00);
            }

            // Scroll
            tmpBytes = BitConverter.GetBytes(measure.Notes[startIndex].Scroll);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }

            float noteOffset = 0.0f;
            for (int i = startIndex; i < measure.Notes.Count; i++)
            {
                if (i > startIndex)
                {
                    bool splitMeasure = false;
                    //if (measure.MeasureTop > measure.MeasureBottom && i % ((measure.NoteCount / measure.MeasureTop) * measure.MeasureBottom) == 0)
                    //{
                    //    splitMeasure = true;
                    //}


                    if ((MeasureParametersChanged(measure.Notes[i - 1], measure.Notes[i])) || splitMeasure)
                    {
                        // This measure has to end, start the next one
                        offset += ((240000 / measure.Notes[startIndex].BPM) + ((240000 / measure.Notes[startIndex].BPM) * ((measure.MeasureTop / (float)measure.MeasureBottom) * ((i - startIndex) / (float)measure.Notes.Count))) - (240000 / nextBPM));
                        
                        for (int k = 0; k < 2; k++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                Bytes.Add(0x00);
                            }
                            tmpBytes = BitConverter.GetBytes(measure.Notes[startIndex].Scroll);
                            for (int j = 0; j < tmpBytes.Length; j++)
                            {
                                Bytes.Add(tmpBytes[j]);
                            }
                        }
                        numMeasures++;
                        tmpBytes = BitConverter.GetBytes(numNotes);
                        Bytes[noteCountIndex] = tmpBytes[0];
                        Bytes[noteCountIndex + 1] = tmpBytes[1];

                        var measureChangeData = WriteMeasure(measure, ref offset, nextMeasure, i);
                        for (int j = 0; j < measureChangeData.Count; j++)
                        {
                            Bytes.Add(measureChangeData[j]);
                        }
                        return Bytes;
                    }
                }
                var noteBytes = WriteNote(measure.Notes[i], noteOffset);
                noteOffset += ((60000 * (measure.MeasureTop / (float)measure.MeasureBottom) * 4) / measure.Notes[startIndex].BPM) * (1 / (float)measure.Notes.Count);
                if (noteBytes != null)
                {
                    numNotes++;
                    for (int j = 0; j < noteBytes.Count; j++)
                    {
                        Bytes.Add(noteBytes[j]);
                    }
                }
            }

            offset += ((240000 / measure.Notes[startIndex].BPM) + ((240000 / measure.Notes[startIndex].BPM) * ((measure.MeasureTop / (float)measure.MeasureBottom) * ((measure.Notes.Count - startIndex) / (float)measure.Notes.Count))) - (240000 / nextBPM));

            for (int k = 0; k < 2; k++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Bytes.Add(0x00);
                }
                tmpBytes = BitConverter.GetBytes(measure.Notes[startIndex].Scroll);
                for (int j = 0; j < tmpBytes.Length; j++)
                {
                    Bytes.Add(tmpBytes[j]);
                }
            }
            tmpBytes = BitConverter.GetBytes(numNotes);
            Bytes[noteCountIndex] = tmpBytes[0];
            Bytes[noteCountIndex + 1] = tmpBytes[1];
            numMeasures++;

            return Bytes;
        }
        /// <summary>
        /// Basically just a comparison function to see if there's a change between the 2 notes
        /// </summary>
        /// <param name="note1"></param>
        /// <param name="note2"></param>
        /// <returns></returns>
        private bool MeasureParametersChanged(Note note1, Note note2)
        {
            // Parameters to check:
            // BPM
            // Gogo Time
            // Scroll Speed

            return note1.isGoGo != note2.isGoGo || note1.BPM != note2.BPM || note1.Scroll != note2.Scroll;
        }
        private float FindNextBPM(Measure currentMeasure, int currentIndex, Measure nextMeasure = null)
        {
            for (int i = currentIndex; i < currentMeasure.Notes.Count; i++)
            {
                if (i + 1 == currentMeasure.Notes.Count)
                {
                    if (nextMeasure != null)
                    {
                        return nextMeasure.BPM;
                    }
                    else
                    {
                        return currentMeasure.Notes[currentIndex].BPM;
                    }
                }
                else
                {
                    if (MeasureParametersChanged(currentMeasure.Notes[i], currentMeasure.Notes[i + 1]))
                    {
                        return currentMeasure.Notes[i + 1].BPM;
                    }
                }
            }
            return currentMeasure.Notes[currentIndex].BPM;
        }
        private List<byte> WriteNote(Note note, float offset)
        {
            List<byte> Bytes = new List<byte>();

            switch (note.Type)
            {
                case NoteType.None:
                    return null;
                case NoteType.Don:
                    Bytes.Add(0x01);
                    break;
                case NoteType.Kat:
                    Bytes.Add(0x04);
                    break;
                case NoteType.BigDon:
                    Bytes.Add(0x07);
                    break;
                case NoteType.BigKat:
                    Bytes.Add(0x08);
                    break;
                case NoteType.Drumroll:
                    Bytes.Add(0x06);
                    break;
                case NoteType.BigDrumroll:
                    Bytes.Add(0x09);
                    break;
                case NoteType.Balloon:
                    Bytes.Add(0x0A);
                    break;
                case NoteType.DrumrollEnd:
                    return null;
                default:
                    return null;
            }
            for (int i = 0; i < 3; i++)
            {
                Bytes.Add(0x00);
            }

            var tmpBytes = BitConverter.GetBytes(offset);
            for (int i = 0; i < tmpBytes.Length; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                Bytes.Add(0x00);
            }
            for (int j = 0; j < 2; j++)
            {
                if (note.Type == NoteType.Balloon)
                {
                    tmpBytes = BitConverter.GetBytes(note.BalloonCount);
                    for (int i = 0; i < 2; i++)
                    {
                        Bytes.Add(tmpBytes[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Bytes.Add(0x00);
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    Bytes.Add(0x00);
                }
            }
            
            // This should be 0 if the note isn't a drumroll/balloon
            tmpBytes = BitConverter.GetBytes(note.DrumrollLength);
            for (int i = 0; i < 4; i++)
            {
                Bytes.Add(tmpBytes[i]);
            }
            // IDK why I need to do this, the data doesn't seem to be used for anything, and it's always 0 anyway
            if (note.Type == NoteType.Drumroll || note.Type == NoteType.BigDrumroll)
            {
                for (int i = 0; i < 8; i++)
                {
                    Bytes.Add(0x00);
                }
            }

            return Bytes;
        }
    }
}
