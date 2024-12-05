using ChartConverterLib.ChartData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverterLib
{
    static public class Utility
    {
        static string FFMPEGFilePath = @"D:\My Stuff\My Programs\Taiko\ChartConverter\ffmpeg\bin\ffmpeg.exe";

        public static void SetAudioVolume(string FilePath)
        {
            string cmdLine = "-i \"" + FilePath + "\" -af astats=reset=1:metadata=1,ametadata=print:file=volume.log -f null /dev/null";

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(FFMPEGFilePath, cmdLine)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            p.Start();
            string errorOutput = p.StandardError.ReadToEnd();
            string standardOutput = p.StandardOutput.ReadToEnd();
            p.Close();

            var lines = File.ReadAllLines("volume.log");
            float highestPeak = -100000;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("astats.Overall.Peak_level"))
                {
                    string peakString = lines[i].Remove(0, lines[i].IndexOf("=") + 1);
                    if (float.TryParse(peakString, out float currentPeak))
                    {
                        highestPeak = Math.Max(currentPeak, highestPeak);
                    }
                }
            }
            if (highestPeak >= 0)
            {
                return;
            }

            string outputFilePath = FilePath.Replace(".ogg", "_new.ogg");

            cmdLine = "-i \"" + FilePath + "\" -filter:a \"volume=" + (highestPeak * -1) + "dB\" \"" + outputFilePath + "\"";

            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            p = new Process();
            p.StartInfo = new ProcessStartInfo(FFMPEGFilePath, cmdLine)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            p.Start();
            errorOutput = p.StandardError.ReadToEnd();
            standardOutput = p.StandardOutput.ReadToEnd();
            p.Close();


            File.Delete(FilePath);
            File.Move(outputFilePath, FilePath);
        }

        public static void AddCorrectTimeSignatures(Chart chart, bool fumenCalculation = false)
        {
            for (int i = 0; i < chart.Measures.Count - 1; i++)
            {
                float offset1 = chart.Measures[i].Offset;
                float offset2 = chart.Measures[i + 1].Offset;

                float difference = offset2 - offset1;
                float bpm1 = chart.Measures[i].BPM;
                float bpm2 = chart.Measures[i + 1].BPM;
                float wholeNoteBeatLength1 = 240000.0f / bpm1;
                float wholeNoteBeatLength2 = 240000.0f / bpm2;

                float timeSignatureRatio = (-((wholeNoteBeatLength1) - (wholeNoteBeatLength2) - (difference))) / wholeNoteBeatLength1;


                if (float.IsNaN(wholeNoteBeatLength1) || float.IsPositiveInfinity(wholeNoteBeatLength1) || float.IsNaN(bpm1))
                {
                    continue;
                }

                int precision = 10000;

                
                var GCF = GCD((int)(timeSignatureRatio * precision), precision);
                int measureTop = (int)((timeSignatureRatio * precision) / GCF);
                int measureBottom = (int)(precision / GCF);

                //int measureTop = (int)Math.Round(timeSignatureRatio * 4);
                //int measureBottom = (int)Math.Round(measureTop / timeSignatureRatio);

                // This part probably isn't actually needed, but 4/4 is sorta standard for time signatures
                if (measureTop == measureBottom)
                {
                    measureTop = 4;
                    measureBottom = 4;
                }

                chart.Measures[i].MeasureTop = measureTop;
                chart.Measures[i].MeasureBottom = measureBottom;
            }
        }

        public static void FillEmptyNotes(Chart chart, bool fullAccuracy = false)
        {
            for (int i = 0; i < chart.Measures.Count - 1; i++)
            {
                List<Note> tmpNotes = new List<Note>();
                List<float> offsets = new List<float>();
                for (int j = 0; j < chart.Measures[i].Notes.Count; j++)
                {
                    tmpNotes.Add(chart.Measures[i].Notes[j]);
                    offsets.Add(chart.Measures[i].Notes[j].Offset);
                }

                //float measureLength = chart.Measures[i + 1].Offset - chart.Measures[i].Offset;
                float measureLength = (240000 / chart.Measures[i].BPM) * ((float)chart.Measures[i].MeasureTop / chart.Measures[i].MeasureBottom);

                if (measureLength == 0)
                {
                    continue;
                }

                int divisor = 1;
                float interval = measureLength / divisor;

                List<double> values = new List<double>();

                int numAttempts = 100;
                while (numAttempts > 0)
                {
                    List<double> currentDivisorValues = new List<double>();
                    numAttempts--;
                    interval = measureLength / divisor;
                    bool failed = false;

                    for (int j = 0; j < offsets.Count; j++)
                    {
                        var value = Math.Round((offsets[j] / interval), 2) % 1;
                        if (value > 0.5)
                        {
                            value = 1 - value;
                        }
                        currentDivisorValues.Add(value);

                        if (value > 0.01 && value < 0.99)
                        {
                            divisor++;
                            failed = true;
                            break;
                        }
                    }
                    if (!failed)
                    {
                        break;
                    }
                    values.Add(currentDivisorValues.Max());
                }

                if (numAttempts <= 0)
                {
                    divisor = values.IndexOf(values.Min()) + 1;
                }

                chart.Measures[i].Notes.Clear();
                chart.Measures[i].Notes = new List<Note>();
                for (int j = 0; j < divisor; j++)
                {
                    bool newNoteAdded = false;
                    for (int k = 0; k < offsets.Count; k++)
                    {
                        float offsetDistance = Math.Abs(offsets[k] - (interval * j));
                        if (j == 0)
                        {
                            if (offsetDistance < Math.Abs(offsets[k] - (interval * (j + 1))))
                            {
                                chart.Measures[i].Notes.Add(tmpNotes[k]);
                                tmpNotes.RemoveAt(k);
                                offsets.RemoveAt(k);
                                newNoteAdded = true;
                                break;
                            }
                        }
                        else if (j == divisor - 1)
                        {
                            if (offsetDistance < Math.Abs(offsets[k] - (interval * (j - 1))))
                            {
                                chart.Measures[i].Notes.Add(tmpNotes[k]);
                                tmpNotes.RemoveAt(k);
                                offsets.RemoveAt(k);
                                newNoteAdded = true;
                                break;
                            }
                        }
                        else
                        {
                            if (offsetDistance < Math.Abs(offsets[k] - (interval * j - 1)) && offsetDistance < Math.Abs(offsets[k] - (interval * (j + 1))))
                            {
                                chart.Measures[i].Notes.Add(tmpNotes[k]);
                                tmpNotes.RemoveAt(k);
                                offsets.RemoveAt(k);
                                newNoteAdded = true;
                                break;
                            }
                        }
                    }
                    if (!newNoteAdded)
                    {
                        Note newNote = new Note();
                        newNote.Type = NoteType.None;
                        chart.Measures[i].Notes.Add(newNote);
                    }
                }

            }
        }

        public static void AddDrumrollEnds(Chart chart)
        {
            for (int i = 0; i < chart.Measures.Count; i++)
            {
                for (int j = 0; j < chart.Measures[i].Notes.Count; j++)
                {
                    if (chart.Measures[i].Notes[j].Type == NoteType.Drumroll ||
                        chart.Measures[i].Notes[j].Type == NoteType.BigDrumroll ||
                        chart.Measures[i].Notes[j].Type == NoteType.Balloon)
                    {
                        float realDrumrollEndTime = chart.Measures[i].Offset + chart.Measures[i].Notes[j].Offset + chart.Measures[i].Notes[j].DrumrollLength;
                        int drumrollEndMeasure = i;
                        for (int k = i; k < chart.Measures.Count; k++)
                        {
                            if (realDrumrollEndTime <= chart.Measures[k].Offset)
                            {
                                drumrollEndMeasure = k - 1;
                                break;
                            }
                        }
                        Note drumrollEndNote = new Note();
                        drumrollEndNote.Type = NoteType.DrumrollEnd;
                        drumrollEndNote.BPM = chart.Measures[drumrollEndMeasure].BPM;
                        drumrollEndNote.Offset = realDrumrollEndTime - chart.Measures[drumrollEndMeasure].Offset;

                        drumrollEndNote.isGoGo = chart.Measures[drumrollEndMeasure].isGoGo;
                        drumrollEndNote.Scroll = chart.Measures[drumrollEndMeasure].ScrollSpeed;

                        if (chart.Measures[drumrollEndMeasure].Notes.Count == 0)
                        {
                            chart.Measures[drumrollEndMeasure].Notes.Add(drumrollEndNote);
                        }
                        else
                        {
                            bool isAdded = false;
                            for (int k = 0; k < chart.Measures[drumrollEndMeasure].Notes.Count; k++)
                            {
                                if (chart.Measures[drumrollEndMeasure].Notes[k].Offset > drumrollEndNote.Offset)
                                {
                                    chart.Measures[drumrollEndMeasure].Notes.Insert(k, drumrollEndNote);
                                    isAdded = true;
                                    break;
                                }
                            }
                            if (!isAdded)
                            {
                                chart.Measures[drumrollEndMeasure].Notes.Add(drumrollEndNote);
                            }
                        }

                    }
                }
            }
        }

        public static void AddNoteInfo(Chart chart)
        {
            for (int i = 0; i < chart.Measures.Count; i++)
            {
                if (chart.Measures[i].Notes.Count == 0)
                {
                    Note newNote = new Note();
                    newNote.Type = NoteType.None;
                    chart.Measures[i].Notes.Add(newNote);
                }
                for (int j = 0; j < chart.Measures[i].Notes.Count; j++)
                {
                    chart.Measures[i].Notes[j].BPM = chart.Measures[i].BPM;
                    chart.Measures[i].Notes[j].Scroll = chart.Measures[i].ScrollSpeed;
                    chart.Measures[i].Notes[j].isGoGo = chart.Measures[i].isGoGo;
                }
            }
        }

        private static int GCD(int a, int b)
        {
            int numAttempts = 100000;
            while (a != 0 && b != 0 && numAttempts > 0)
            {
                numAttempts--;
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }
    }
}
