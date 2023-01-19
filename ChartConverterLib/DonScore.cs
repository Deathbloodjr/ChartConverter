using ChartConverterLib.ChartData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChartConverterLib
{
    public class DonScore
    {
        public Chart ReadDonScore(string FilePath, Difficulty difficulty = Difficulty.Oni)
        {
            List<string> lines = new List<string>();
            if (FilePath.EndsWith(".png"))
            {
                var donScoreLines = GetDonScoreStringFromPNG(FilePath);
                if (donScoreLines.Count == 0 || donScoreLines == null)
                {
                    return null;
                }
                for (int i = 0; i < donScoreLines.Count; i++)
                {
                    lines.Add(donScoreLines[i]);
                }
            }
            else // ".txt"
            {
                var donScorelines = File.ReadAllLines(FilePath);
                for (int i = 0; i < donScorelines.Length; i++)
                {
                    lines.Add(donScorelines[i]);
                }
            }

            File.WriteAllLines(FilePath.Replace(".png", ".txt"), lines);

            Chart chart = new Chart();
            chart = ReadDonScoreMetaData(FilePath, difficulty);

            Parameters currentParameters = new Parameters();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith("#bpm "))
                {
                    currentParameters.BPM = float.Parse(lines[i].Remove(0, "#bpm ".Length));
                    break;
                }
            }
            // Actual Chart Data
            MeasureParameters measureParameters = new MeasureParameters();
            List<ParameterChange> ParamChanges = new List<ParameterChange>();
            int numBranches = 1;
            int currentBranchLine = 1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith("#title ") || lines[i].StartsWith("#difficulty ") || lines[i].StartsWith("#level ") || lines[i].StartsWith("#genre "))
                {
                    continue;
                }
                // Just for future reference, #newline isn't needed at all. All it does is put the next measure on the next line in the png created by DonScore
                if (lines[i].StartsWith("#beatchar "))
                {
                    measureParameters.BeatChar = int.Parse(lines[i].Remove(0, "#beatchar ".Length));
                }
                else if (lines[i].StartsWith("#hs ") && lines[i].LastIndexOf(" ") == "#hs".Length)
                {
                    // This should mean it only has 1 space in it, which would be directly after "#hs"
                    currentParameters.Scroll = float.Parse(lines[i].Remove(0, "#hs ".Length));
                }
                else if (lines[i].StartsWith("#bpm ") && lines[i].LastIndexOf(" ") == "#bpm".Length)
                {
                    // This should mean it only has 1 space in it, which would be directly after "#hs"
                    currentParameters.BPM = float.Parse(lines[i].Remove(0, "#bpm ".Length));
                    measureParameters.BPM = currentParameters.BPM;
                }
                else if (lines[i].StartsWith("#meter "))
                {
                    var splitMeter = lines[i].Split(' ');
                    measureParameters.MeasureTop = int.Parse(splitMeter[2]);
                    measureParameters.MeasureBottom = int.Parse(splitMeter[1]);
                }
                else if (lines[i].StartsWith("#barlineon"))
                {
                    measureParameters.isBarline = true;
                }
                else if (lines[i].StartsWith("#barlineoff"))
                {
                    measureParameters.isBarline = false;
                }
                else if (lines[i].StartsWith("#begingogo") && !lines[i].Contains(" "))
                {
                    currentParameters.isGoGo = true;
                }
                else if (lines[i].StartsWith("#endgogo") && !lines[i].Contains(" "))
                {
                    currentParameters.isGoGo = false;
                }
                // Time for the mid-measure changes
                else if (lines[i].StartsWith("#bpm"))
                {
                    var splitBPM = lines[i].Split(' ');
                    ParameterChange parameterChange = new ParameterChange();
                    parameterChange.Type = ChangeType.BPM;
                    parameterChange.Value = float.Parse(splitBPM[1]);
                    parameterChange.ChangeNumBottom = int.Parse(splitBPM[2]);
                    parameterChange.ChangeNumTop = int.Parse(splitBPM[3]);
                    ParamChanges.Add(parameterChange);
                }
                else if (lines[i].StartsWith("#hs"))
                {
                    var splitScroll = lines[i].Split(' ');
                    ParameterChange parameterChange = new ParameterChange();
                    parameterChange.Type = ChangeType.Scroll;
                    parameterChange.Value = float.Parse(splitScroll[1]);
                    parameterChange.ChangeNumBottom = int.Parse(splitScroll[2]);
                    parameterChange.ChangeNumTop = int.Parse(splitScroll[3]);
                    ParamChanges.Add(parameterChange);
                }
                else if (lines[i].StartsWith("#begingogo"))
                {
                    var splitScroll = lines[i].Split(' ');
                    ParameterChange parameterChange = new ParameterChange();
                    parameterChange.Type = ChangeType.GoGo;
                    parameterChange.BoolValue = true;
                    parameterChange.ChangeNumBottom = int.Parse(splitScroll[1]);
                    parameterChange.ChangeNumTop = int.Parse(splitScroll[2]);
                    ParamChanges.Add(parameterChange);
                }
                else if (lines[i].StartsWith("#endgogo"))
                {
                    var splitScroll = lines[i].Split(' ');
                    ParameterChange parameterChange = new ParameterChange();
                    parameterChange.Type = ChangeType.GoGo;
                    parameterChange.BoolValue = false;
                    parameterChange.ChangeNumBottom = int.Parse(splitScroll[1]);
                    parameterChange.ChangeNumTop = int.Parse(splitScroll[2]);
                    ParamChanges.Add(parameterChange);
                }
                else if (lines[i].StartsWith("#branch "))
                {
                    numBranches = lines[i].Count(f => f == 'o');
                }
                string noteChars = " oxOX(<[=]>)3";
                for (int j = 0; j < noteChars.Length; j++)
                {
                    if (lines[i].StartsWith(noteChars[j].ToString()))
                    {
                        if (currentBranchLine < numBranches)
                        {
                            currentBranchLine += 1;
                            break;
                        }
                        currentBranchLine = 1;
                        var newMeasures = AnalyzeTXTLine(lines[i], currentParameters, ParamChanges, measureParameters);
                        for (int k = 0; k < newMeasures.Count; k++)
                        {
                            chart.Measures.Add(newMeasures[k]);
                        }
                        currentParameters.UpdateParameters(ParamChanges, measureParameters.BeatChar);
                        ParamChanges.Clear();
                        break;
                    }
                }
            }

            return chart;
        }

        private List<Measure> AnalyzeTXTLine(string line, Parameters parameters, List<ParameterChange> parameterChanges, MeasureParameters measureParameters)
        {
            List<Measure> measures = new List<Measure>();
            List<int> Balloons = new List<int>();
            int OldBeatChar = measureParameters.BeatChar;

            var result = NormalizeTXTLine(line, measureParameters.BeatChar, measureParameters.MeasureTop);
            line = result.Item1;
            measureParameters.BeatChar = result.Item2;
            for (int i = 0; i < result.Item3.Count; i++)
            {
                Balloons.Add(result.Item3[i]);
            }

            Measure measure = new Measure();
            measure.BPM = measureParameters.BPM;
            measure.Scroll = measureParameters.Scroll;
            measure.MeasureTop = measureParameters.MeasureTop;
            measure.MeasureBottom = measureParameters.MeasureBottom;
            measure.isBarline = measureParameters.isBarline;
            //if (line.Contains("3") && (line[line.IndexOf('3') + 1] == ' ' || line[line.IndexOf('3') + 1] == 'o' || line[line.IndexOf('3') + 1] == 'O' || line[line.IndexOf('3') + 1] == 'x' || line[line.IndexOf('3') + 1] == 'X'))
            //{
            //    var result = Remove3(line, measureParameters.BeatChar, measureParameters.MeasureTop);
            //    line = result.Item1;
            //    measureParameters.BeatChar = result.Item2;
            //}

            for (int i = 0; i < line.Length; i++)
            {
                if (measureParameters.BeatChar * measureParameters.MeasureTop <= measure.Notes.Count)
                {
                    measures.Add(measure);
                    measure = new Measure();
                    measure.BPM = measureParameters.BPM;
                    measure.Scroll = measureParameters.Scroll;
                    measure.MeasureTop = measureParameters.MeasureTop;
                    measure.MeasureBottom = measureParameters.MeasureBottom;
                    measure.isBarline = measureParameters.isBarline;
                }
                for (int j = 0; j < parameterChanges.Count; j++)
                {
                    if (measure.Notes.Count == ((measureParameters.BeatChar / (float)parameterChanges[j].ChangeNumBottom) * parameterChanges[j].ChangeNumTop) * (measureParameters.MeasureBottom / 4))
                    {
                        if (parameterChanges[j].Type == ChangeType.Scroll)
                        {
                            parameters.Scroll = parameterChanges[j].Value;
                        }
                        else if (parameterChanges[j].Type == ChangeType.BPM)
                        {
                            parameters.BPM = parameterChanges[j].Value;
                        }
                        else if (parameterChanges[j].Type == ChangeType.GoGo)
                        {
                            parameters.isGoGo = parameterChanges[j].BoolValue;
                        }
                        parameterChanges.RemoveAt(j);
                        j--;
                        continue;
                    }
                }

                Note note = new Note();
                note.BPM = parameters.BPM;
                note.Scroll = parameters.Scroll;
                note.isGoGo = parameters.isGoGo;
                switch (line[i])
                {
                    case ' ':
                        note.Type = NoteType.None;
                        break;
                    case 'o':
                        note.Type = NoteType.Don;
                        break;
                    case 'O':
                        note.Type = NoteType.BigDon;
                        break;
                    case 'x':
                        note.Type = NoteType.Kat;
                        break;
                    case 'X':
                        note.Type = NoteType.BigKat;
                        break;
                    case '<':
                        note.Type = NoteType.Drumroll;
                        break;
                    case '(':
                        note.Type = NoteType.BigDrumroll;
                        break;
                    case '[':
                        note.Type = NoteType.Balloon;
                        if (Balloons.Count > 0)
                        {
                            note.BalloonCount = Balloons[0];
                            Balloons.RemoveAt(0);
                        }
                        break;
                    case '>':
                    case ')':
                    case ']':
                        note.Type = NoteType.DrumrollEnd;
                        break;
                    case '=':
                    case '@':
                        note.Type = NoteType.None;
                        break;
                    default:
                        note.Type = NoteType.None;
                        break;
                }

                measure.Notes.Add(note);
            }

            int missingSpots = (measureParameters.BeatChar * measureParameters.MeasureTop) - measure.Notes.Count;
            if (missingSpots >= 1)
            {
                for (int i = 0; i < missingSpots; i++)
                {
                    var note = new Note();
                    note.BPM = measure.Notes[measure.Notes.Count - 1].BPM;
                    note.Scroll = measure.Notes[measure.Notes.Count - 1].Scroll;
                    note.isGoGo = measure.Notes[measure.Notes.Count - 1].isGoGo;
                    note.Type = NoteType.None;
                    measure.Notes.Add(note);
                }
            }

            measureParameters.BeatChar = OldBeatChar;

            measures.Add(measure);

            return measures;
        }

        public (string, int, List<int>) NormalizeTXTLine(string line, int BeatChar, int MeasureTop)
        {
            // This function will do 3 things as of now:
            // Get all the balloon numbers in the line,
            // Remove any 3s that are in the line, (which will update the BeatChar as well)
            // Add any additional spaces that may be needed at the end

            while (line.Length < BeatChar * MeasureTop)
            {
                line += " ";
            }

            List<int> newBalloons = new List<int>();

            if (line.Contains('@'))
            {
                var match = Regex.Matches(line, "(@|^)([^@=\\]]*)(=|]|$)");
                for (int i = 0; i < match.Count; i++)
                {
                    if (match[i].Groups.Count >= 2)
                    {
                        if (match[i].Groups[2].Value != string.Empty)
                        {
                            if (int.TryParse(match[i].Groups[2].Value, out int tmpInt))
                            {
                                newBalloons.Add(tmpInt);
                            }
                        }
                        string replacementString = string.Empty;
                        for (int j = 0; j < match[i].Groups[2].Value.Length; j++)
                        {
                            replacementString += " ";
                        }
                        int index = line.IndexOf(match[i].Groups[2].Value);
                        line = line.Remove(index, replacementString.Length);
                        line = line.Insert(index, replacementString);
                    }
                }
            }




            if (!line.Contains('3'))
            {
                // Job complete, good work
                return (line, BeatChar, newBalloons);
            }

            List<double> noteIndices = new List<double>();
            List<char> noteStrings = new List<char>();
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '3')
                {
                    // The 2nd and 3rd index values will not be integers, which is the point of this function
                    // (They won't just be i+1, i+2)
                    // Measure would look like 3xox, in reality that's 3 notes split evenly among 4 places
                    // They're doubles, and this function figures out how to make them integers while keeping the measure the same
                    noteIndices.Add(i);
                    noteStrings.Add(line[i + 1]);
                    noteIndices.Add(((double)(i + 4 - i) / 3) + i);
                    noteStrings.Add(line[i + 2]);
                    noteIndices.Add((((double)(i + 4 - i) / 3) * 2) + i);
                    noteStrings.Add(line[i + 3]);

                    i += 3;
                    continue;
                }
                else if (line[i] != ' ')
                {
                    noteIndices.Add(i);
                    noteStrings.Add(line[i]);
                }
            }

            double Multiplier = 1.5;

            for (int i = 0; i < noteIndices.Count; i++)
            {
                if (noteIndices[i] == 0)
                {
                    continue;
                }
                if ((noteIndices[i] * Multiplier) % 1 != 0)
                {
                    Multiplier *= 2;
                    i = -1;
                    continue;
                }
            }
            for (int i = 0; i < noteIndices.Count; i++)
            {
                noteIndices[i] = (int)(noteIndices[i] * Multiplier);
            }
            string newLine = string.Empty;
            for (int i = 0; i < (int)(BeatChar * Multiplier * MeasureTop); i++)
            {
                bool match = false;
                for (int j = 0; j < noteIndices.Count; j++)
                {
                    if (noteIndices[j] == i)
                    {
                        match = true;
                        newLine += noteStrings[j];
                        break;
                    }
                }
                if (match == false)
                {
                    newLine += " ";
                }
            }

            return (newLine, (int)(BeatChar * Multiplier), newBalloons);
        }
        public List<string> GetDonScoreStringFromPNG(string FilePath)
        {
            if (!FilePath.EndsWith(".png"))
            {
                return null;
            }
            List<char> result = new List<char>();
            var bytes = File.ReadAllBytes(FilePath);
            List<byte> startOffset = new List<byte>()
            {
                0x23, 0x00, 0x74, 0x00, 0x69, 0x00, 0x74, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x20, 0x00
            };
            int startIndex = -1;
            for (int i = 0; i < bytes.Length; i++)
            {
                bool isStartIndex = true;
                for (int j = 0; j < startOffset.Count; j++)
                {
                    if (bytes[i + j] != startOffset[j])
                    {
                        isStartIndex = false;
                        break;
                    }
                }
                if (isStartIndex)
                {
                    startIndex = i;
                    break;
                }
            }
            if (startIndex == -1)
            {
                return null;
            }

            List<byte> titleString = new List<byte>();

            for (int i = startIndex; i < bytes.Length; i++)
            {
                if (bytes[i] == 0x0A && bytes[i + 1] == 0x00)
                {
                    startIndex = i + 2;
                    break;
                }
                titleString.Add(bytes[i]);
            }

            var title = Encoding.Unicode.GetString(titleString.ToArray());

            for (int i = startIndex; i < bytes.Length; i += 2)
            {
                if (bytes[i] == 0)
                {
                    break;
                }
                result.Add(BitConverter.ToChar(bytes, i));
            }

            List<string> finalString = new List<string>()
            {
                title
            };
            string currentString = string.Empty;
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] == '\n')
                {
                    finalString.Add(currentString);
                    currentString = string.Empty;
                }
                else
                {
                    currentString += result[i];
                }
            }


            return finalString;
        }
        public Chart ReadDonScoreMetaData(string FilePath, Difficulty difficulty = Difficulty.Oni)
        {
            List<string> lines = new List<string>();
            if (FilePath.EndsWith(".png"))
            {
                var donScoreLines = GetDonScoreStringFromPNG(FilePath);
                for (int i = 0; i < donScoreLines.Count; i++)
                {
                    lines.Add(donScoreLines[i]);
                }
            }
            else
            {
                var donScorelines = File.ReadAllLines(FilePath);
                for (int i = 0; i < donScorelines.Length; i++)
                {
                    lines.Add(donScorelines[i]);
                }
            }
            return ReadDonScoreMetaData(lines, difficulty);
        }
        public Chart ReadDonScoreMetaData(List<string> lines, Difficulty difficulty = Difficulty.Oni)
        {
            Chart chart = new Chart();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith("#title "))
                {
                    chart.Title = lines[i].Remove(0, "#title ".Length);
                }
                else if (lines[i].StartsWith("#level "))
                {
                    chart.Level = lines[i].Remove(0, "#level ".Length);
                }
                else if (lines[i].StartsWith("#genre "))
                {
                    switch (lines[i].Remove(0, "#genre ".Length))
                    {
                        case "1":
                            chart.Genre = Genre.Pops;
                            break;
                        case "2":
                            chart.Genre = Genre.Anime;
                            break;
                        case "3":
                            chart.Genre = Genre.Vocaloid;
                            break;
                        case "4":
                            chart.Genre = Genre.Kids;
                            break;
                        case "5":
                            chart.Genre = Genre.Variety;
                            break;
                        case "6":
                            chart.Genre = Genre.Classical;
                            break;
                        case "7":
                            chart.Genre = Genre.GameMusic;
                            break;
                        case "8":
                            chart.Genre = Genre.Namco;
                            break;
                        default:
                            chart.Genre = Genre.Namco;
                            break;
                    }
                }
                else if (lines[i].StartsWith("#difficulty "))
                {
                    // There doesn't seem to be a DonScore option for setting the difficulty to Ura
                    // So I will allow manually overwriting the difficulty
                    if (difficulty != Difficulty.Oni)
                    {
                        chart.Difficulty = difficulty;
                    }
                    else
                    {
                        switch (lines[i].Remove(0, "#difficulty ".Length))
                        {
                            case "おに":
                                chart.Difficulty = Difficulty.Oni;
                                break;
                            case "むずかしい":
                                chart.Difficulty = Difficulty.Hard;
                                break;
                            case "ふつう":
                                chart.Difficulty = Difficulty.Normal;
                                break;
                            case "かんたん":
                                chart.Difficulty = Difficulty.Easy;
                                break;
                            default:
                                chart.Difficulty = Difficulty.Oni;
                                break;
                        }
                    }
                }
            }
            chart.DemoStart = 50000;
            return chart;
        }
        public void WriteDonScore(string FilePath, Chart chart)
        {
            int firstMeasureWithNotes = 0;
            for (int i = 0; i < chart.Measures.Count; i++)
            {
                if (chart.Measures[i].Notes.Count != 0)
                {
                    firstMeasureWithNotes = i;
                    break;
                }
            }
            List<string> TXTFileText = new List<string>();
            TXTFileText.Add("#title " + chart.Title);
            TXTFileText.Add("#bpm " + chart.Measures[0].BPM);
            TXTFileText.Add("#level " + chart.Level);
            TXTFileText.Add("#difficulty おに");
            if (chart.Measures[0].isBarline == false)
            {
                TXTFileText.Add("#barlineoff");
            }
            if (chart.Measures[0].MeasureBottom != 4 || chart.Measures[0].MeasureTop != 4)
            {
                TXTFileText.Add("#meter " + chart.Measures[0].MeasureBottom + " " + chart.Measures[0].MeasureTop);
            }
            if (chart.Measures[0].isGoGo == true)
            {
                TXTFileText.Add("#begingogo");
            }
            if (chart.Measures[0].Scroll != 1)
            {
                TXTFileText.Add("#hs " + chart.Measures[0].Scroll);
            }
            chart.Measures[0].AdjustForBalloons();
            int BeatChar = chart.Measures[0].GetBeatChar();
            if (BeatChar == 0)
            {
                BeatChar = 4;
            }
            TXTFileText.Add("#beatchar " + BeatChar);

            for (int i = 0; i < chart.Measures.Count; i++)
            {
                for (int j = 0; j < chart.Measures[i].Notes.Count; j++)
                {
                    if (chart.Measures[i].Notes[j].Type == NoteType.Balloon)
                    {
                        chart.Measures[i].AdjustForBalloons();
                        break;
                    }
                }
                bool parameterNewLine = false;
                if (BeatChar != chart.Measures[i].GetBeatChar())
                {
                    parameterNewLine = true;
                    TXTFileText.Add("");
                    if (chart.Measures[i].GetBeatChar() != 0)
                    {
                        BeatChar = chart.Measures[i].GetBeatChar();
                    }
                    TXTFileText.Add("#beatchar " + BeatChar);
                }
                if (i != 0)
                {
                    // This is ugly
                    if (chart.Measures[i].BPM != chart.Measures[i-1].Notes[chart.Measures[i - 1].Notes.Count - 1].BPM ||
                        chart.Measures[i].Scroll != chart.Measures[i - 1].Notes[chart.Measures[i - 1].Notes.Count - 1].Scroll ||
                        chart.Measures[i].isBarline != chart.Measures[i - 1].isBarline ||
                        chart.Measures[i].isGoGo != chart.Measures[i - 1].Notes[chart.Measures[i - 1].Notes.Count - 1].isGoGo ||
                        chart.Measures[i].MeasureTop != chart.Measures[i - 1].MeasureTop ||
                        chart.Measures[i].MeasureBottom != chart.Measures[i - 1].MeasureBottom)
                    {
                        if (parameterNewLine == false)
                        {
                            parameterNewLine = true;
                            TXTFileText.Add("");
                        }
                        if (chart.Measures[i].isBarline != chart.Measures[i - 1].isBarline)
                        {
                            if (chart.Measures[i].isBarline)
                            {
                                TXTFileText.Add("#barlineon");
                            }
                            else
                            {
                                TXTFileText.Add("#barlineoff");
                            }
                        }
                        if (chart.Measures[i].MeasureBottom != chart.Measures[i - 1].MeasureBottom || chart.Measures[i].MeasureTop != chart.Measures[i - 1].MeasureTop)
                        {
                            TXTFileText.Add("#meter " + chart.Measures[i].MeasureBottom + " " + chart.Measures[i].MeasureTop);
                        }
                        if (chart.Measures[i].Scroll != chart.Measures[i-1].Notes[chart.Measures[i-1].Notes.Count - 1].Scroll)
                        {
                            TXTFileText.Add("#hs " + chart.Measures[i].Scroll);
                        }
                        if (chart.Measures[i].BPM != chart.Measures[i - 1].Notes[chart.Measures[i - 1].Notes.Count - 1].BPM)
                        {
                            TXTFileText.Add("#bpm " + chart.Measures[i].BPM);
                        }
                        if (chart.Measures[i].isGoGo != chart.Measures[i - 1].Notes[chart.Measures[i - 1].Notes.Count - 1].isGoGo)
                        {
                            if (chart.Measures[i].isGoGo)
                            {
                                TXTFileText.Add("#begingogo");
                            }
                            else
                            {
                                TXTFileText.Add("#endgogo");
                            }
                        }
                    }
                }

                string parameterChangeString = chart.Measures[i].GenerateParameterChangeString();
                if (parameterChangeString != string.Empty)
                {
                    TXTFileText.Add(parameterChangeString);
                }
                TXTFileText.Add(WriteTXTMeasure(chart.Measures[i]));
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            File.WriteAllLines(FilePath, TXTFileText, Encoding.GetEncoding(932));
        }

        enum RollType { Drumroll, BigDrumroll, Balloon, None };
        RollType txtRollType = RollType.None;

        private string WriteTXTMeasure(Measure measure)
        {
            string line = string.Empty;


            for (int i = 0; i < measure.Notes.Count; i++)
            {
                switch (measure.Notes[i].Type)
                {
                    case NoteType.None:
                        if (txtRollType == RollType.None)
                        {
                            line += " ";
                        }
                        else
                        {
                            line += "=";
                        }
                        break;
                    case NoteType.Don:
                        line += "o";
                        break;
                    case NoteType.Kat:
                        line += "x";
                        break;
                    case NoteType.BigDon:
                        line += "O";
                        break;
                    case NoteType.BigKat:
                        line += "X";
                        break;
                    case NoteType.Drumroll:
                        line += "<";
                        txtRollType = RollType.Drumroll;
                        break;
                    case NoteType.BigDrumroll:
                        line += "(";
                        txtRollType = RollType.BigDrumroll;
                        break;
                    case NoteType.Balloon:
                        // This isn't 100% accurate, but i'm lazy and this should work decently at least
                        line += "[@";
                        txtRollType = RollType.Balloon;
                        int BalloonCount = measure.Notes[i].BalloonCount;
                        i += 1 + BalloonCount.ToString().Length;
                        line += BalloonCount.ToString();
                        break;
                    case NoteType.DrumrollEnd:
                        if (txtRollType == RollType.Drumroll)
                        {
                            line += ">";
                        }
                        else if (txtRollType == RollType.BigDrumroll)
                        {
                            line += ")";
                        }
                        else if (txtRollType == RollType.Balloon)
                        {
                            line += "]";
                        }
                        txtRollType = RollType.None;
                        break;
                    default:
                        break;
                }
            }
            return line;
        }

        // These are for reading TXT from my other program
        public class Parameters
        {
            public float BPM { get; set; }
            public float Scroll { get; set; }
            public bool isGoGo { get; set; }

            public Parameters()
            {
                BPM = 0;
                Scroll = 1;
                isGoGo = false;
            }
            public Parameters(Parameters newParams)
            {
                BPM = newParams.BPM;
                Scroll = newParams.Scroll;
                isGoGo = newParams.isGoGo;
            }

            public void UpdateParameters(List<ParameterChange> newParams, int BeatChar)
            {
                for (int j = 0; j < 3; j++)
                {
                    int latestChar = 0;
                    float latestValue = 0;
                    bool latestBoolValue = false;
                    for (int i = 0; i < newParams.Count; i++)
                    {
                        if (j == (int)ChangeType.Scroll && newParams[i].Type == ChangeType.Scroll)
                        {
                            if ((BeatChar / newParams[i].ChangeNumBottom) * newParams[i].ChangeNumTop > latestChar)
                            {
                                latestChar = (BeatChar / newParams[i].ChangeNumBottom) * newParams[i].ChangeNumTop;
                                latestValue = newParams[i].Value;
                            }
                        }
                        if (j == (int)ChangeType.BPM && newParams[i].Type == ChangeType.BPM)
                        {
                            if ((BeatChar / newParams[i].ChangeNumBottom) * newParams[i].ChangeNumTop > latestChar)
                            {
                                latestChar = (BeatChar / newParams[i].ChangeNumBottom) * newParams[i].ChangeNumTop;
                                latestValue = newParams[i].Value;
                            }
                        }
                        if (j == (int)ChangeType.GoGo && newParams[i].Type == ChangeType.GoGo)
                        {
                            if ((BeatChar / newParams[i].ChangeNumBottom) * newParams[i].ChangeNumTop > latestChar)
                            {
                                latestChar = (BeatChar / newParams[i].ChangeNumBottom) * newParams[i].ChangeNumTop;
                                latestBoolValue = newParams[i].BoolValue;
                            }
                        }
                    }
                    if (latestChar == 0 && latestValue == 0)
                    {
                        continue;
                    }
                    else
                    {
                        if (j == (int)ChangeType.Scroll)
                        {
                            Scroll = latestValue;
                        }
                        else if (j == (int)ChangeType.BPM)
                        {
                            BPM = latestValue;
                        }
                        else if (j == (int)ChangeType.GoGo)
                        {
                            isGoGo = latestBoolValue;
                        }
                    }
                }

            }
            public static bool operator ==(Parameters a, Parameters b)
            {
                return (a.BPM == b.BPM) && (a.Scroll == b.Scroll) && (a.isGoGo == b.isGoGo);
            }
            public static bool operator !=(Parameters a, Parameters b)
            {
                return (a.BPM != b.BPM) || (a.Scroll != b.Scroll) || (a.isGoGo != b.isGoGo);
            }
        }
        public enum ChangeType { Scroll, BPM, GoGo }
        public class ParameterChange
        {
            public ChangeType Type { get; set; }
            public float Value { get; set; }
            public bool BoolValue { get; set; }
            public int ChangeNumBottom { get; set; }
            public int ChangeNumTop { get; set; }
        }

        public class MeasureParameters
        {
            public bool isBarline { get; set; }
            public int MeasureTop { get; set; }
            public int MeasureBottom { get; set; }
            public int BeatChar { get; set; }
            public float BPM { get; set; }
            public float Scroll { get; set; }
            public MeasureParameters()
            {
                isBarline = true;
                MeasureTop = 4;
                MeasureBottom = 4;
                BeatChar = 4;
                BPM = 180;
                Scroll = 1;
            }
            public MeasureParameters(MeasureParameters newParams)
            {
                isBarline = newParams.isBarline;
                MeasureTop = newParams.MeasureTop;
                MeasureBottom = newParams.MeasureBottom;
                BeatChar = newParams.BeatChar;
                BPM = newParams.BPM;
                Scroll = newParams.Scroll;
            }

            public static bool operator ==(MeasureParameters a, MeasureParameters b)
            {
                return (a.MeasureTop == b.MeasureTop) && (a.MeasureBottom == b.MeasureBottom) && (a.isBarline == b.isBarline)
                    && (a.BeatChar == b.BeatChar) && (a.BPM == b.BPM) && (a.Scroll == b.Scroll);
            }
            public static bool operator !=(MeasureParameters a, MeasureParameters b)
            {
                return (a.MeasureTop != b.MeasureTop) || (a.MeasureBottom != b.MeasureBottom) || (a.isBarline != b.isBarline)
                    || (a.BeatChar != b.BeatChar) || (a.BPM != b.BPM) || (a.Scroll != b.Scroll);
            }
        }
    }
}
