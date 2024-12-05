using ChartConverterLib.ChartData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartConverterLib
{
    public class TJA
    {
        public Chart ReadTJA(string FilePath, Difficulty difficulty)
        {
            if (!FilePath.EndsWith(".tja"))
            {
                return null;
            }

            Chart chart = new Chart();
            float BPM = 0.0f;
            float Scroll = 1.0f;
            bool isGoGo = false;
            bool isBarline = true;
            int measureTop = 4;
            int measureBottom = 4;

            List<int> BalloonCounts = new List<int>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var lines = File.ReadAllLines(FilePath, Encoding.GetEncoding(932));
            int startingLine = 0;
            // Song Metadata First
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("TITLE:"))
                {
                    chart.Title = lines[i].Remove(0, "TITLE:".Length);
                    if (!chart.Title.Contains(" (裏)") && difficulty == Difficulty.Ura)
                    {
                        chart.Title += " (裏)";
                    }
                }
                else if (lines[i].StartsWith("SUBTITLE:"))
                {
                    chart.Subtitle = lines[i].Remove(0, "SUBTITLE:".Length);
                    if (chart.Subtitle.StartsWith("--"))
                    {
                        chart.Subtitle = chart.Subtitle.Remove(0, 2);
                    }
                }
                else if (lines[i].StartsWith("BPM:"))
                {
                    BPM = float.Parse(lines[i].Remove(0, "BPM:".Length));
                }
                else if (lines[i].StartsWith("WAVE:"))
                {
                    chart.Wave = lines[i].Remove(0, "WAVE:".Length);
                }
                else if (lines[i].StartsWith("OFFSET:"))
                {
                    if (float.TryParse(lines[i].Remove(0, "OFFSET:".Length), out float tmpOffset))
                    {
                        chart.Offset = tmpOffset * -1000;
                    }
                    else
                    {
                        chart.Offset = 0;
                    }
                }
                else if (lines[i].StartsWith("DEMOSTART:"))
                {
                    if (float.TryParse(lines[i].Remove(0, "DEMOSTART:".Length), out float tmpDemoStart))
                    {
                        chart.DemoStart = tmpDemoStart * 1000;
                    }
                    else
                    {
                        chart.DemoStart = 0;
                    }
                }
                else if (lines[i].StartsWith("GENRE:"))
                {
                    var genre = lines[i].Remove(0, "GENRE:".Length);
                    switch (genre)
                    {
                        case "J-POP":
                            chart.Genre = Genre.Pops;
                            break;
                        case "アニメ":
                            chart.Genre = Genre.Anime;
                            break;
                        case "どうよう":
                            chart.Genre = Genre.Kids;
                            break;
                        case "ボーカロイド":
                            chart.Genre = Genre.Vocaloid;
                            break;
                        case "ゲームミュージック":
                            chart.Genre = Genre.GameMusic;
                            break;
                        case "バラエティ":
                            chart.Genre = Genre.Variety;
                            break;
                        case "クラシック":
                            chart.Genre = Genre.Classical;
                            break;
                        case "ナムコオリジナル":
                            chart.Genre = Genre.Namco;
                            break;
                        default:
                            chart.Genre = Genre.Namco;
                            break;
                    }
                }
                //else if (lines[i].StartsWith("COURSE:"))
                //{
                //    var chartDifficulty = lines[i].Remove(0, "COURSE:".Length);
                //    switch (chartDifficulty)
                //    {
                //        case "0":
                //        case "easy":
                //        case "Easy":
                //            chart.Difficulty = Difficulty.Easy;
                //            break;
                //        case "1":
                //        case "normal":
                //        case "Normal":
                //            chart.Difficulty = Difficulty.Normal;
                //            break;
                //        case "2":
                //        case "hard":
                //        case "Hard":
                //            chart.Difficulty = Difficulty.Hard;
                //            break;
                //        case "3":
                //        case "oni":
                //        case "Oni":
                //            chart.Difficulty = Difficulty.Oni;
                //            break;
                //        case "4":
                //        case "Ura":
                //        case "Edit":
                //        case "edit":
                //            chart.Difficulty = Difficulty.Ura;
                //            break;
                //        default:
                //            break;
                //    }
                //}

                // I could read these, but they're completely useless anyway, 
                // I can calculate the proper values from the chart itself
                //else if (lines[i].StartsWith("SCOREINIT:"))
                //{
                //    chart.ScoreInit = lines[i].Remove(0, "SCOREINIT:".Length);
                //}
                //else if (lines[i].StartsWith("SCOREDIFF:"))
                //{
                //    chart.ScoreDiff = lines[i].Remove(0, "SCOREDIFF:".Length);
                //}

            }

            // Find the correct Course

            int[] CourseIndexes = new int[] { -1, -1, -1, -1, -1};
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("COURSE"))
                {
                    var chartDifficulty = lines[i].Remove(0, "COURSE:".Length);
                    switch (chartDifficulty)
                    {
                        case "0":
                        case "easy":
                        case "Easy":
                            CourseIndexes[(int)Difficulty.Easy] = i;
                            break;
                        case "1":
                        case "normal":
                        case "Normal":
                            CourseIndexes[(int)Difficulty.Normal] = i;
                            break;
                        case "2":
                        case "hard":
                        case "Hard":
                            CourseIndexes[(int)Difficulty.Hard] = i;
                            break;
                        case "3":
                        case "oni":
                        case "Oni":
                            CourseIndexes[(int)Difficulty.Oni] = i;
                            break;
                        case "4":
                        case "Ura":
                        case "Edit":
                        case "edit":
                            CourseIndexes[(int)Difficulty.Ura] = i;
                            break;
                        default:
                            break;
                    }
                }
            }
            if (CourseIndexes[(int)difficulty] != -1)
            {
                startingLine = CourseIndexes[(int)difficulty];
                chart.Difficulty = difficulty;
            }
            else
            {
                for (int i = 4; i >= 0; i--)
                {
                    if (CourseIndexes[i] != -1)
                    {
                        startingLine = CourseIndexes[i];
                        chart.Difficulty = (Difficulty)i;
                        break;
                    }
                }
            }
            

            for (int i = startingLine; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("LEVEL:"))
                {
                    chart.Level = lines[i].Remove(0, "LEVEL:".Length);
                }
                else if (lines[i].StartsWith("BALLOON:"))
                {
                    var baseBalloonString = lines[i].Remove(0, "BALLOON:".Length);
                    var splitBalloons = baseBalloonString.Replace(" ", "").Split(',');
                    for (int j = 0; j < splitBalloons.Length; j++)
                    {
                        if (splitBalloons[j] != string.Empty)
                        {
                            BalloonCounts.Add(int.Parse(splitBalloons[j]));
                        }
                    }
                }
                else if (lines[i].StartsWith("#START"))
                {
                    startingLine = i;
                    break;
                }
            }

            // Chart Data Next
            Measure measure = new Measure();
            measure.BPM = BPM;
            measure.ScrollSpeed = Scroll;
            measure.isGoGo = isGoGo;
            measure.isBarline = isBarline;

            for (int i = startingLine; i < lines.Length; i++)
            {
                string noteChars = "012345678,";
                if (lines[i].StartsWith("#MEASURE"))
                {
                    // "#MEASURE 2/4" -> "#MEASURE", "2/4" -> "2", "4"
                    var measureSigValues = lines[i].Split(' ')[1].Split('/');
                    measure.MeasureTop = int.Parse(measureSigValues[0]);
                    measureTop = measure.MeasureTop;
                    measure.MeasureBottom = int.Parse(measureSigValues[1]);
                    measureBottom = measure.MeasureBottom;
                }
                else if (lines[i].StartsWith("#BARLINEON"))
                {
                    isBarline = true;
                }
                else if (lines[i].StartsWith("#BARLINEOFF"))
                {
                    isBarline = false;
                }
                else if (lines[i].StartsWith("#GOGOSTART"))
                {
                    isGoGo = true;
                }
                else if (lines[i].StartsWith("#GOGOEND"))
                {
                    isGoGo = false;
                }
                else if (lines[i].StartsWith("#BRANCH"))
                {
                    return null;
                }
                else if (lines[i].StartsWith("#SCROLL"))
                {
                    Scroll = float.Parse(lines[i].Remove(0, "#SCROLL ".Length));
                }
                else if (lines[i].StartsWith("#BPMCHANGE"))
                {
                    BPM = float.Parse(lines[i].Remove(0, "#BPMCHANGE ".Length));
                }
                else if (lines[i].StartsWith("//") || lines[i].StartsWith(";") || lines[i].StartsWith("#LYRIC"))
                {
                    continue;
                }
                else if (lines[i].StartsWith("#END"))
                {
                    break;
                }
                else if (lines[i].Length != 0)
                {
                    if (measure.Notes.Count == 0)
                    {
                        measure.isBarline = isBarline;
                        measure.isGoGo = isGoGo;
                        measure.ScrollSpeed = Scroll;
                        measure.BPM = BPM;
                        measure.MeasureTop = measureTop;
                        measure.MeasureBottom = measureBottom;
                    }
                    bool comment = false;
                    for (int j = 0; j < lines[i].Length; j++)
                    {
                        if (comment)
                        {
                            break;
                        }
                        Note note = new Note();
                        note.isGoGo = isGoGo;
                        note.BPM = BPM;
                        note.Scroll = Scroll;
                        switch (lines[i][j])
                        {
                            case '0':
                                note.Type = NoteType.None;
                                break;
                            case '1':
                                note.Type = NoteType.Don;
                                break;
                            case '2':
                                note.Type = NoteType.Kat;
                                break;
                            case '3':
                                note.Type = NoteType.BigDon;
                                break;
                            case '4':
                                note.Type = NoteType.BigKat;
                                break;
                            case '5':
                                note.Type = NoteType.Drumroll;
                                break;
                            case '6':
                                note.Type = NoteType.BigDrumroll;
                                break;
                            case '7':
                                note.Type = NoteType.Balloon;
                                note.BalloonCount = BalloonCounts[0];
                                BalloonCounts.RemoveAt(0);
                                break;
                            case '8':
                                note.Type = NoteType.DrumrollEnd;
                                break;
                            case ',':
                                if (measure.Notes.Count == 0)
                                {
                                    note.Type = NoteType.None;
                                    measure.Notes.Add(note);
                                }
                                chart.Measures.Add(measure);
                                measure = new Measure();
                                measure.BPM = BPM;
                                measure.ScrollSpeed = Scroll;
                                measure.isGoGo = isGoGo;
                                measure.isBarline = isBarline;
                                measure.MeasureTop = measureTop;
                                measure.MeasureBottom = measureBottom;
                                break;
                            case '/':
                                comment = true;
                                break;
                            default:
                                break;
                        }
                        if (lines[i][j] != ',' && noteChars.Contains(lines[i][j]))
                        {
                            measure.Notes.Add(note);
                        }
                    }
                }
            }
            CalculateDrumrollLengths(ref chart);

            return chart;
        }
        private void CalculateDrumrollLengths(ref Chart chart)
        {
            float length = 0.0f;
            bool isDrumroll = false;
            int measureIndex = 0;
            int noteIndex = 0;
            for (int i = 0; i < chart.Measures.Count; i++)
            {
                for (int j = 0; j < chart.Measures[i].Notes.Count; j++)
                {
                    if (chart.Measures[i].Notes[j].Type == NoteType.Drumroll
                        || chart.Measures[i].Notes[j].Type == NoteType.BigDrumroll
                        || chart.Measures[i].Notes[j].Type == NoteType.Balloon)
                    {
                        isDrumroll = true;
                        measureIndex = i;
                        noteIndex = j;
                    }
                    if (chart.Measures[i].Notes[j].Type == NoteType.DrumrollEnd && isDrumroll)
                    {
                        isDrumroll = false;
                        chart.Measures[measureIndex].Notes[noteIndex].DrumrollLength = length + ((240000 / chart.Measures[i].Notes[j].BPM) / 64);
                        length = 0.0f;
                        measureIndex = 0;
                        noteIndex = 0;
                    }
                    if (isDrumroll)
                    {
                        length += (60000 * (chart.Measures[i].MeasureTop / (float)chart.Measures[i].MeasureBottom) * 4 / chart.Measures[i].Notes[j].BPM) * (1 / (float)chart.Measures[i].Notes.Count);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="chart"></param>
        /// <param name="MeasureStart">Inclusive</param>
        /// <param name="MeasureEnd">Inclusive</param>
        /// <param name="NewTitle">If the name in the file will be different than the read chart's title</param>
        /// <param name="ScrollSpeedChanges"></param>
        public void WriteTJA(string FilePath, Chart chart, int MeasureStart = 0, int MeasureEnd = 0, string NewTitle = null, bool ScrollSpeedChanges = true, bool isAppend = false)
        {
            if (MeasureStart > chart.Measures.Count)
            {
                return;
            }
            if (MeasureEnd <= 0 || MeasureEnd >= chart.Measures.Count)
            {
                MeasureEnd = chart.Measures.Count - 1;
            }
            List<string> TJAFileText = new List<string>();
            if (!isAppend || !File.Exists(FilePath))
            {
                string value = (NewTitle != null) ? "TITLE:" + NewTitle : "TITLE:" + chart.Title;
                TJAFileText.Add(value);
                TJAFileText.Add("SUBTITLE:--" + chart.Subtitle);
                // Not really sure if I should put [0] or [MeasureStart] here
                // [0] is more accurate to the original chart, if this is creating a practice mode chart
                // [MeasureStart] is more accurate for the practice mode chart
                // Either way, the chart stays the same
                TJAFileText.Add("BPM:" + chart.Measures[0].BPM);
                value = (NewTitle != null) ? "WAVE:" + NewTitle + ".ogg" : "WAVE:" + chart.Title + ".ogg";
                TJAFileText.Add(value);
                TJAFileText.Add("OFFSET:" + (chart.Offset / -1000.0f));
                TJAFileText.Add("DEMOSTART:" + chart.DemoStart / 1000.0f);
                switch (chart.Genre)
                {
                    case Genre.Pops:
                        TJAFileText.Add("GENRE:J-POP");
                        break;
                    case Genre.Anime:
                        TJAFileText.Add("GENRE:アニメ");
                        break;
                    case Genre.Kids:
                        TJAFileText.Add("GENRE:どうよう");
                        break;
                    case Genre.Vocaloid:
                        TJAFileText.Add("GENRE:ボーカロイド");
                        break;
                    case Genre.GameMusic:
                        TJAFileText.Add("GENRE:ゲームミュージック");
                        break;
                    case Genre.Variety:
                        TJAFileText.Add("GENRE:バラエティ");
                        break;
                    case Genre.Classical:
                        TJAFileText.Add("GENRE:クラシック");
                        break;
                    case Genre.Namco:
                        TJAFileText.Add("GENRE:ナムコオリジナル");
                        break;
                    default:
                        TJAFileText.Add("GENRE:ナムコオリジナル");
                        break;
                }
            }
            

            TJAFileText.Add("");
            switch (chart.Difficulty)
            {
                case Difficulty.Easy:
                    TJAFileText.Add("COURSE:Easy");
                    break;
                case Difficulty.Normal:
                    TJAFileText.Add("COURSE:Normal");
                    break;
                case Difficulty.Hard:
                    TJAFileText.Add("COURSE:Hard");
                    break;
                case Difficulty.Oni:
                    TJAFileText.Add("COURSE:Oni");
                    break;
                case Difficulty.Ura:
                    TJAFileText.Add("COURSE:Edit");
                    break;
                default:
                    break;
            }
            TJAFileText.Add("LEVEL:" + chart.Level);

            // Time to read all the balloons in the given measures
            string balloonString = "BALLOON:";
            for (int i = MeasureStart; i < MeasureEnd + 1; i++)
            {
                for (int j = 0; j < chart.Measures[i].Notes.Count; j++)
                {
                    if (chart.Measures[i].Notes[j].Type == NoteType.Balloon)
                    {
                        balloonString += chart.Measures[i].Notes[j].BalloonCount + ",";
                    }
                }
            }
            TJAFileText.Add(balloonString);

            // I believe this formula is correct, if ignoring all the drumrolls
            TJAFileText.Add("SCOREINIT:" + chart.GetPointsAndScore().Item1);
            TJAFileText.Add("SCOREDIFF:0"); // Nijiro style scoring doesn't use this anymore

            TJAFileText.Add("");
            TJAFileText.Add("#START");
            TJAFileText.Add("");

            // Initialize beginning parameters
            if (chart.Measures[MeasureStart].isBarline == false)
            {
                TJAFileText.Add("#BARLINEOFF");
            }
            if (chart.Measures[MeasureStart].MeasureBottom != 4 || chart.Measures[MeasureStart].MeasureTop != 4)
            {
                TJAFileText.Add("#MEASURE " + chart.Measures[MeasureStart].MeasureTop + "/" + chart.Measures[MeasureStart].MeasureBottom);
            }
            if (chart.Measures[MeasureStart].isGoGo)
            {
                TJAFileText.Add("#GOGOSTART");
            }
            if (ScrollSpeedChanges)
            {
                if (chart.Measures[MeasureStart].ScrollSpeed != 1)
                {
                    TJAFileText.Add("#SCROLL " + chart.Measures[MeasureStart].ScrollSpeed);
                }
            }
            // I wouldn't need this check if I used [MeasureStart] for the initial BPM, but it doesn't matter either way
            if (chart.Measures[MeasureStart].BPM != chart.Measures[0].BPM)
            {
                TJAFileText.Add("#BPMCHANGE " + chart.Measures[MeasureStart].BPM);
            }

            // This is where I start inputting the actual notes and measures of the chart
            bool isDrumroll = false;
            for (int i = MeasureStart; i < MeasureEnd + 1; i++)
            {
                if (i != MeasureStart)
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
                    if (chart.Measures[i].MeasureTop != chart.Measures[i-1].MeasureTop || chart.Measures[i].MeasureBottom != chart.Measures[i - 1].MeasureBottom)
                    {
                        TJAFileText.Add("#MEASURE " + chart.Measures[i].MeasureTop + "/" + chart.Measures[i].MeasureBottom);
                    }
                }
                Measure previousMeasure = null;
                if (i != MeasureStart)
                {
                    previousMeasure = chart.Measures[i - 1];
                }
                var measureText = MeasureToTJA(previousMeasure, chart.Measures[i], ScrollSpeedChanges, ref isDrumroll);
                for (int j = 0; j < measureText.Count; j++)
                {
                    TJAFileText.Add(measureText[j]);
                }
            }
            TJAFileText.Add("#END");

            Directory.CreateDirectory(FilePath.Remove(FilePath.LastIndexOf("\\")));

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (isAppend && File.Exists(FilePath))
            {
                var oldLines = File.ReadAllLines(FilePath);
                List<string> oldFile = new List<string>();
                for (int i = 0; i < oldLines.Length; i++)
                {
                    oldFile.Add(oldLines[i]);
                }
                for (int i = 0; i < TJAFileText.Count; i++)
                {
                    oldFile.Add(TJAFileText[i]);
                }
                File.WriteAllLines(FilePath, oldFile.ToArray(), Encoding.UTF8);
            }
            else
            {
                File.WriteAllLines(FilePath, TJAFileText.ToArray(), Encoding.UTF8);
            }
        }

        public void AppendTJA(string FilePath, Chart chart)
        {
            WriteTJA(FilePath, chart, 0, 0, null, true, true);
        }

        private List<string> MeasureToTJA(Measure previousMeasure, Measure currentMeasure, bool ScrollSpeedChanges, ref bool isDrumroll)
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
    }
}