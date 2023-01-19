using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartConverterLib.ChartData;

namespace ChartConverterLib
{
    public class DaniDojo
    {
        public void WriteTJADan(string FilePath, DaniData daniData, bool isNijiroStyle = true)
        {
            if (!daniData.isValidRequirementsCount())
            {
                return;
            }
            while (!isNijiroStyle && daniData.Borders.Count > 3)
            {
                for (int i = 0; i < daniData.Borders.Count; i++)
                {
                    if (daniData.Borders[i].Condition == ConditionType.SoulGauge)
                    {
                        daniData.Borders.RemoveAt(i);
                        break;
                    }
                }
            }
            List<string> TJAFileText = new List<string>();
            TJAFileText.Add("TITLE:" + daniData.Title);
            TJAFileText.Add("SUBTITLE:--" + daniData.DanSeries);
            TJAFileText.Add("WAVE:" + daniData.Charts[0].Wave);
            TJAFileText.Add("BPM:" + daniData.Charts[0].Measures[0].BPM);
            TJAFileText.Add("GENRE:段位-銀");
            TJAFileText.Add("");
            TJAFileText.Add("COURSE:Dan");
            TJAFileText.Add("LEVEL:" + daniData.Charts[0].Level);

            string balloonString = "BALLOON:";
            foreach (Chart chart in daniData.Charts)
            {
                foreach (Measure measure in chart.Measures)
                {
                    foreach (Note note in measure.Notes)
                    {
                        if (note.Type == NoteType.Balloon)
                        {
                            balloonString += note.BalloonCount + ",";
                        }
                    }
                }
            }
            TJAFileText.Add(balloonString);
            TJAFileText.Add("EXAM1:g," + daniData.Borders[0].RedRequirement[0] + "," + daniData.Borders[0].GoldRequirement[0] + ",m");

            for (int i = 1; i < daniData.Borders.Count; i++)
            {
                if (!isNijiroStyle && i >= 3)
                {
                    break;
                }
                if (daniData.Borders[i].IsGoThrough || !isNijiroStyle)
                {

                    string examString = "EXAM" + (i + 1) + ":";
                    switch (daniData.Borders[i].Condition)
                    {
                        case ConditionType.SoulGauge:
                            examString += "g,";
                            break;
                        case ConditionType.Goods:
                            examString += "jp,";
                            break;
                        case ConditionType.OKs:
                            examString += "jg,";
                            break;
                        case ConditionType.Bads:
                            examString += "jb,";
                            break;
                        case ConditionType.Score:
                            examString += "s,";
                            break;
                        case ConditionType.Drumrolls:
                            examString += "r,";
                            break;
                        case ConditionType.HitCount:
                            examString += "h,";
                            break;
                        case ConditionType.Combo:
                            examString += "c,";
                            break;
                        default:
                            return;
                    }
                    examString += daniData.Borders[i].RedRequirement.Sum() + ",";
                    examString += daniData.Borders[i].GoldRequirement.Sum() + ",";
                    switch (daniData.Borders[i].ConditionComparer)
                    {
                        case ConditionComparer.More:
                            examString += "m";
                            break;
                        case ConditionComparer.Less:
                            examString += "l";
                            break;
                        default:
                            return;
                    }
                    TJAFileText.Add(examString);
                }
                else if (!isNijiroStyle)
                {
                    string examString = "EXAM" + (i + 1) + ":";
                    switch (daniData.Borders[i].Condition)
                    {
                        case ConditionType.SoulGauge:
                            examString += "g,";
                            break;
                        case ConditionType.Goods:
                            examString += "jp,";
                            break;
                        case ConditionType.OKs:
                            examString += "jg,";
                            break;
                        case ConditionType.Bads:
                            examString += "jb,";
                            break;
                        case ConditionType.Score:
                            examString += "s,";
                            break;
                        case ConditionType.Drumrolls:
                            examString += "r,";
                            break;
                        case ConditionType.HitCount:
                            examString += "h,";
                            break;
                        case ConditionType.Combo:
                            examString += "c,";
                            break;
                        default:
                            return;
                    }
                    if (daniData.Borders[i].ConditionComparer == ConditionComparer.Less)
                    {
                        examString += (daniData.Borders[i].RedRequirement.Sum() - daniData.Borders[i].RedRequirement.Count) + ",";
                        examString += (daniData.Borders[i].GoldRequirement.Sum() - daniData.Borders[i].GoldRequirement.Count) + ",";
                    }
                    else
                    {
                        examString += daniData.Borders[i].RedRequirement.Sum() + ",";
                        examString += daniData.Borders[i].GoldRequirement.Sum() + ",";

                    }

                    switch (daniData.Borders[i].ConditionComparer)
                    {
                        case ConditionComparer.More:
                            examString += "m";
                            break;
                        case ConditionComparer.Less:
                            examString += "l";
                            break;
                        default:
                            return;
                    }
                    TJAFileText.Add(examString);
                }
            }

            if (isNijiroStyle)
            {
                // Don't ask me why this goes 2 1 3 4, I'm just copying what other people did and assuming it's correct
                switch (daniData.Title.Remove(2))
                {
                    case "初段":
                    case "二段":
                    case "三段":
                    case "四段":
                    case "五段":
                        TJAFileText.Add("DANTICK:2");
                        break;
                    case "六段":
                    case "七段":
                    case "八段":
                    case "九段":
                    case "十段":
                        TJAFileText.Add("DANTICK:1");
                        break;
                    case "玄人":
                    case "名人":
                    case "超人":
                        TJAFileText.Add("DANTICK:3");
                        break;
                    case "達人":
                        TJAFileText.Add("DANTICK:4");
                        break;
                    default:
                        break;
                }
            }

            TJAFileText.Add("");
            TJAFileText.Add("#START");

            int index = 0;
            bool prevIsBarline = true;
            bool prevIsGoGo = false;
            int prevMeasureTop = 4;
            int prevMeasureBotton = 4;
            float prevScroll = 1;
            float prevBPM = daniData.Charts[0].Measures[0].BPM;
            foreach (var chart in daniData.Charts)
            {
                TJAFileText.Add(GetNextSongString(chart, isNijiroStyle));
                for (int j = 1; j < daniData.Borders.Count; j++)
                {
                    if (!daniData.Borders[j].IsGoThrough && isNijiroStyle)
                    {
                        string examString = "EXAM" + (j + 1) + ":";
                        switch (daniData.Borders[j].Condition)
                        {
                            case ConditionType.SoulGauge:
                                examString += "g,";
                                break;
                            case ConditionType.Goods:
                                examString += "jp,";
                                break;
                            case ConditionType.OKs:
                                examString += "jg,";
                                break;
                            case ConditionType.Bads:
                                examString += "jb,";
                                break;
                            case ConditionType.Score:
                                examString += "s,";
                                break;
                            case ConditionType.Drumrolls:
                                examString += "r,";
                                break;
                            case ConditionType.HitCount:
                                examString += "h,";
                                break;
                            case ConditionType.Combo:
                                examString += "c,";
                                break;
                            default:
                                return;
                        }
                        examString += daniData.Borders[j].RedRequirement[index] + ",";
                        examString += daniData.Borders[j].GoldRequirement[index] + ",";
                        switch (daniData.Borders[j].ConditionComparer)
                        {
                            case ConditionComparer.More:
                                examString += "m";
                                break;
                            case ConditionComparer.Less:
                                examString += "l";
                                break;
                            default:
                                return;
                        }
                        TJAFileText.Add(examString);
                    }
                }
                TJAFileText.Add("#DELAY " + (chart.Offset / 1000));

                if (chart.Measures[0].isBarline != prevIsBarline)
                {
                    if (chart.Measures[0].isBarline)
                    {
                        TJAFileText.Add("#BARLINEON");
                    }
                    else
                    {
                        TJAFileText.Add("#BARLINEOFF");
                    }
                }
                if (chart.Measures[0].MeasureBottom != prevMeasureBotton || chart.Measures[0].MeasureTop != prevMeasureTop)
                {
                    TJAFileText.Add("#MEASURE " + chart.Measures[0].MeasureTop + "/" + chart.Measures[0].MeasureBottom);
                }
                if (chart.Measures[0].isGoGo != prevIsGoGo)
                {
                    if (chart.Measures[0].isGoGo)
                    {
                        TJAFileText.Add("#GOGOSTART");
                    }
                    else
                    {
                        TJAFileText.Add("#GOGOEND");
                    }
                }
                if (chart.Measures[0].Scroll != prevScroll)
                {
                    TJAFileText.Add("#SCROLL " + chart.Measures[0].Scroll);
                }
                // I wouldn't need this check if I used [MeasureStart] for the initial BPM, but it doesn't matter either way
                if (chart.Measures[0].BPM != prevBPM)
                {
                    TJAFileText.Add("#BPMCHANGE " + chart.Measures[0].BPM);
                }

                // This is where I start inputting the actual notes and measures of the chart
                bool isDrumroll = false;
                for (int i = 0; i < chart.Measures.Count; i++)
                {
                    if (i != 0)
                    {
                        if (chart.Measures[i].isBarline != chart.Measures[i - 1].isBarline)
                        {
                            if (chart.Measures[i].isBarline)
                            {
                                TJAFileText.Add("#BARLINEON");
                            }
                            else
                            {
                                TJAFileText.Add("#BARLINEOFF");
                            }
                        }
                        if (chart.Measures[i].MeasureTop != chart.Measures[i - 1].MeasureTop || chart.Measures[i].MeasureBottom != chart.Measures[i - 1].MeasureBottom)
                        {
                            TJAFileText.Add("#MEASURE " + chart.Measures[i].MeasureTop + "/" + chart.Measures[i].MeasureBottom);
                        }
                    }
                    Measure previousMeasure = null;
                    if (i != 0)
                    {
                        previousMeasure = chart.Measures[i - 1];
                    }
                    var measureText = MeasureToTJA(previousMeasure, chart.Measures[i], ref isDrumroll);
                    for (int j = 0; j < measureText.Count; j++)
                    {
                        TJAFileText.Add(measureText[j]);
                    }
                }
                index++;
                Measure measure = chart.Measures[chart.Measures.Count - 1];
                //prevIsGoGo = measure.Notes[measure.Notes.Count - 1].isGoGo;
                prevIsGoGo = measure.isGoGo;
                prevIsBarline = measure.isBarline;
                prevMeasureTop = measure.MeasureTop;
                prevMeasureBotton = measure.MeasureBottom;
                prevScroll = measure.Notes[measure.Notes.Count - 1].Scroll;
                prevBPM = measure.Notes[measure.Notes.Count - 1].BPM;
            }

            TJAFileText.Add("");
            TJAFileText.Add("#END");

            Directory.CreateDirectory(FilePath.Remove(FilePath.LastIndexOf("\\")));

            File.WriteAllLines(FilePath, TJAFileText.ToArray(), Encoding.UTF8);
            for (int i = 0; i < daniData.AudioFilePaths.Count; i++)
            {
                string sourceFilePath = FilePath.Remove(FilePath.LastIndexOf("\\")) + daniData.AudioFilePaths[i].Remove(0, daniData.AudioFilePaths[i].LastIndexOf("\\"));
                if (File.Exists(sourceFilePath))
                {
                    continue;
                }
                Utility.SetAudioVolume(daniData.AudioFilePaths[i]);
                File.Copy(daniData.AudioFilePaths[i], sourceFilePath);
            }
        }

        private List<string> MeasureToTJA(Measure previousMeasure, Measure currentMeasure, ref bool isDrumroll)
        {
            List<string> text = new List<string>();
            string currentLine = string.Empty;
            for (int i = 0; i < currentMeasure.Notes.Count; i++)
            {
                // First check for parameter changes
                bool isBPMChange = false;
                bool isScrollChange = false;
                bool isGoGoChange = false;
                // BPM Checks
                if (i == 0 && previousMeasure != null)
                {
                    if (currentMeasure.Notes[i].BPM != previousMeasure.Notes[previousMeasure.Notes.Count - 1].BPM)
                    {
                        isBPMChange = true;
                    }
                }
                else if (i != 0)
                {
                    if (currentMeasure.Notes[i].BPM != currentMeasure.Notes[i - 1].BPM)
                    {
                        isBPMChange = true;
                    }
                }
                // Scroll Checks
                if (i == 0 && previousMeasure != null)
                {
                    if (currentMeasure.Notes[i].Scroll != previousMeasure.Notes[previousMeasure.Notes.Count - 1].Scroll)
                    {
                        isScrollChange = true;
                    }
                }
                else if (i != 0)
                {
                    if (currentMeasure.Notes[i].Scroll != currentMeasure.Notes[i - 1].Scroll)
                    {
                        isScrollChange = true;
                    }
                }
                // GoGo Checks
                if (i == 0 && previousMeasure != null)
                {
                    if (currentMeasure.Notes[i].isGoGo != previousMeasure.Notes[previousMeasure.Notes.Count - 1].isGoGo)
                    {
                        isGoGoChange = true;
                    }
                }
                else if (i != 0)
                {
                    if (currentMeasure.Notes[i].isGoGo != currentMeasure.Notes[i - 1].isGoGo)
                    {
                        isGoGoChange = true;
                    }
                }
                // Apply changes
                if (isBPMChange || isScrollChange || isGoGoChange)
                {
                    text.Add(currentLine);
                    currentLine = string.Empty;
                }
                if (isBPMChange)
                {
                    text.Add("#BPMCHANGE " + currentMeasure.Notes[i].BPM);
                }
                if (isScrollChange)
                {
                    text.Add("#SCROLL " + currentMeasure.Notes[i].Scroll);
                }
                if (isGoGoChange)
                {
                    if (currentMeasure.Notes[i].isGoGo)
                    {
                        text.Add("#GOGOSTART");
                    }
                    else
                    {
                        text.Add("#GOGOEND");
                    }
                }
                // Write the notes as text
                switch (currentMeasure.Notes[i].Type)
                {
                    case NoteType.None:
                        currentLine += "0";
                        break;
                    case NoteType.Don:
                        currentLine += "1";
                        break;
                    case NoteType.Kat:
                        currentLine += "2";
                        break;
                    case NoteType.BigDon:
                        currentLine += "3";
                        break;
                    case NoteType.BigKat:
                        currentLine += "4";
                        break;
                    case NoteType.Drumroll:
                        currentLine += "5";
                        isDrumroll = true;
                        break;
                    case NoteType.BigDrumroll:
                        currentLine += "6";
                        isDrumroll = true;
                        break;
                    case NoteType.Balloon:
                        currentLine += "7";
                        isDrumroll = true;
                        break;
                    case NoteType.DrumrollEnd:
                        if (!isDrumroll)
                        {
                            currentLine += "0";
                        }
                        else
                        {
                            currentLine += "8";
                            isDrumroll = false;
                        }
                        break;
                    default: // idk if a default could possibly break a chart, or if this would fix a potentially broken chart
                        currentLine += "0";
                        break;
                }
            }

            // Analyze the lines to compress the line to as short as it could be
            int numNoteLines = 0;
            string noteChars = "012345678";
            for (int i = 0; i < text.Count; i++)
            {
                if (numNoteLines > 1)
                {
                    break;
                }
                for (int j = 0; j < noteChars.Length; j++)
                {
                    if (text[i].StartsWith(noteChars[j].ToString()))
                    {
                        numNoteLines++;
                        break;
                    }
                }
            }
            for (int j = 0; j < noteChars.Length; j++)
            {

                if (currentLine.StartsWith(noteChars[j].ToString()))
                {
                    numNoteLines++;
                    break;
                }
            }

            if (numNoteLines == 1 || text.Count == 0)
            {
                currentLine = Compress(currentLine);
            }
            currentLine += ",";
            text.Add(currentLine);

            return text;

        }

        private string Compress(string line)
        {
            string returnLine = line;
            int lowestI = 0;
            for (int i = 1; i < line.Length; i++)
            {
                if (lowestI != 0)
                {
                    break;
                }
                for (int j = 0; j < line.Length; j += 1)
                {
                    // if it's a major beat, we don't want to check it right now
                    if (j % ((float)line.Length / i) == 0)
                    {
                        continue;
                    }

                    // Check to see if all minor beats are 0
                    if (returnLine[j] != '0')
                    {
                        break;
                    }

                    // if they are, then it could potentially be compressed to that
                    if (j == line.Length - 1)
                    {
                        lowestI = i;
                        break;
                    }
                }
            }
            if (lowestI == 0)
            {
                return line;
            }
            for (int i = line.Length; i > 0; i--)
            {
                if (i % ((float)line.Length / lowestI) == 0)
                {
                    continue;
                }
                returnLine = returnLine.Remove(i, 1);
            }
            return returnLine;
        }

        public string GetGenreString(Genre genre)
        {
            switch (genre)
            {
                case Genre.Pops:
                    return "J-POP";
                case Genre.Anime:
                    return "アニメ";
                case Genre.Kids:
                    return "どうよう";
                case Genre.Vocaloid:
                    return "ボーカロイド";
                case Genre.GameMusic:
                    return "ゲームミュージック";
                case Genre.Variety:
                    return "バラエティ";
                case Genre.Classical:
                    return "クラシック";
                case Genre.Namco:
                    return "ナムコオリジナル";
                default:
                    return "ナムコオリジナル";
            }
        }

        private string GetNextSongString(Chart chart, bool isNijiroStyle)
        {
            string nextSongString = "#NEXTSONG " + chart.Title + "," + chart.Subtitle + ",";
            nextSongString += GetGenreString(chart.Genre) + "," + chart.Wave + ",";
            nextSongString += chart.GetPointsAndScore().Item1 + ",0";

            if (isNijiroStyle)
            {
                nextSongString += "," + chart.Level + "," + (int)(chart.Difficulty);
            }
            return nextSongString;
        }
    }
}
