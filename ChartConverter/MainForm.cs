using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChartConverterLib;
using ChartConverterLib.ChartData;

namespace ChartConverter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            TJA tja = new TJA();
            DonScore donScore = new DonScore();
            Fumen fumen = new Fumen();
            DaniDojo daniDojo = new DaniDojo();

            //var chart = fumen.ReadFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\Nijiiro_Songs\clsh69\clsh69_m.bin", true);
            //var chart = fumen.ReadFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\Nijiiro_Songs\kooryu\kooryu_x.bin", true);
            //tja.WriteTJA(@"D:\Downloads\New\kooryu.tja", chart);

            //var chart = donScore.ReadDonScore(@"D:\Downloads\New\love ura.png");
            //tja.WriteTJA(@"D:\Downloads\New\love ura.tja", chart);
            //AddUraChartToTDM(@"D:\Downloads\New\love ura.tja", "lov193", @"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\Nijiiro_Songs\lov193\lov193_m.bin");


            //ConvertPNGsInNewToTJA();

            //var chart = tja.ReadTJA(@"D:\Downloads\New\カルメン 組曲一番終曲(裏譜面) (裏).tja", Difficulty.Ura);
            //fumen.WriteFumen(@"D:\Downloads\New\カルメン 組曲一番終曲(裏譜面) (裏).bin", chart);

            //AddUraChartToTDM(@"D:\Downloads\New\カルメン 組曲一番終曲(裏譜面) (裏).tja", "clsca2", 28.2880001068115f);

            ConvertPNGsInNewToTJA();


            //var chart = tja.ReadTJA(@"D:\Downloads\New\そして勇者は眠りにつく.tja", Difficulty.Ura);
            //chart.Offset = 0;
            //fumen.WriteFumen(@"D:\Downloads\New\そして勇者は眠りにつく ura new.bin", chart);

            //var chart = fumen.ReadFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\Nijiiro_Songs\hatara\hatara_m.bin");
            //var (points, score) = chart.GetPointsAndScore();

            //chart = fumen.ReadFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\Nijiiro_Songs\ufoswn\ufoswn_m.bin");
            //(points, score) = chart.GetPointsAndScore();

            //chart = fumen.ReadFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\Nijiiro_Songs\589him\589him_m.bin");
            //(points, score) = chart.GetPointsAndScore();

            //chart = fumen.ReadFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\Nijiiro_Songs\ten9nr\ten9nr_m.bin");
            //(points, score) = chart.GetPointsAndScore();

            //int i = 64;
            //Utility.SetAudioVolume(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\TJA_Nijiro_Songs\そして勇者は眠りにつく\そして勇者は眠りにつく.ogg");
            //Utility.SetAudioVolume(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\TJA_Nijiro_Songs\狂瀾怒濤\狂瀾怒濤.ogg");

            //var newChart = donScore.ReadDonScore(@"D:\Downloads\New\vt1op.png");
            //tja.WriteTJA(@"D:\Downloads\New\vt1op.tja", newChart);

            //AddUraChartToTDM(@"D:\Downloads\New\vt1op.tja", "vt1op", 245.520004272461f);

            //string filePath = @"D:\Downloads\New\Let's 貢献！～恋の懲役は1,000,000年～─.tja";
            //
            //var chart = tja.ReadTJA(filePath, Difficulty.Ura);
            //
            //chart.Offset = 125f;
            //
            //string NewFolderPath = @"D:\Downloads\New\customSongs";
            //string NewSongFolderPath = @"Vita_Songs";
            //string songID = "freedw";
            //
            //fumen.WriteFumen(Path.Combine(NewFolderPath, NewSongFolderPath, songID, songID + "_x.bin"), chart);
            //File.Copy(Path.Combine(NewFolderPath, NewSongFolderPath, songID, songID + "_x.bin"), Path.Combine(NewFolderPath, NewSongFolderPath, songID, songID + "_x_1.bin"));
            //File.Copy(Path.Combine(NewFolderPath, NewSongFolderPath, songID, songID + "_x.bin"), Path.Combine(NewFolderPath, NewSongFolderPath, songID, songID + "_x_2.bin"));
            //
            //var (Points, Score) = chart.GetPointsAndScore();
            //File.WriteAllText(Path.Combine(NewFolderPath, NewSongFolderPath, songID, songID + "_data.txt"), "Level = " + chart.Level + "\nPoints = " + Points + "\nScore = " + Score);

            //var chart = donScore.ReadDonScore(@"D:\Downloads\New\on say go say ura.png", Difficulty.Ura);
            //tja.WriteTJA(@"D:\Downloads\New\test.tja", chart);

            //
            //tja.WriteTJA(@"D:\Downloads\New\業 -善なる神とこの世の悪について- (裏) 94-122.tja", chart, 93, 121, "業 -善なる神とこの世の悪について- (裏) 94-122");

            //CreateDonScoreImage(@"D:\Downloads\New\test.txt");
            //OpenImage(@"D:\Downloads\New\test.png");

            //var chart = fumen.ReadFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\3DS1_Songs\ace3ds\ace3ds_m.bin");
            //D:\Games\Taiko\Arcade Nijiro\Taiko Nijiro Nov 2020\太鼓の達人 (game_ver=08.18)\Data\x64\fumen\sweep1
            //var chart = fumen.ReadFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\Nijiiro_Songs\calc\calc_m.bin");
            //chart.GetPointsAndScore();

            //var chart = fumen.ReadFumen(@"D:\Games\Taiko\Arcade Nijiro\Taiko Nijiro Nov 2020\太鼓の達人 (game_ver=08.18)\Data\x64\fumen\zzff14\zzff14_e.bin");
            //tja.AppendTJA(@"D:\Downloads\New\LimitedCharts\" + "zzff14" + @"\" + "zzff14" + ".tja", chart);


            //DaniData Dan = new DaniData();
            //
            //Dan.Charts.Add(tja.ReadTJA(@"D:\Games\Taiko\TJA Songs\7 stars\1・2・さんしのでドンドカッカッ！\1・2・さんしのでドンドカッカッ！.tja", Difficulty.Oni));
            //Dan.Charts.Add(tja.ReadTJA(@"D:\Games\Taiko\TJA Songs\9 stars\ぺた・PETA！？パンプキン\ぺた・PETA！？パンプキン.tja", Difficulty.Oni));
            //Dan.Charts.Add(tja.ReadTJA(@"D:\Games\Taiko\TJA Songs\10 stars\うちゅうひこうし冒険譚\うちゅうひこうし冒険譚.tja", Difficulty.Oni));
            //
            //Dan.AudioFilePaths.Add(@"D:\Games\Taiko\TJA Songs\7 stars\1・2・さんしのでドンドカッカッ！\1・2・さんしのでドンドカッカッ！.ogg");
            //Dan.AudioFilePaths.Add(@"D:\Games\Taiko\TJA Songs\9 stars\ぺた・PETA！？パンプキン\ぺた・PETA！？パンプキン.ogg");
            //Dan.AudioFilePaths.Add(@"D:\Games\Taiko\TJA Songs\10 stars\うちゅうひこうし冒険譚\うちゅうひこうし冒険譚.ogg");
            //
            //Dan.Title = "steμからの挑戦状 #1";
            //Dan.DanSeries = "ニジイロ Nijiro 2022 Gaiden";
            //
            //Dan.Borders.Add(new Border(ConditionType.SoulGauge, ConditionComparer.More, true, new List<int>() { 98 }, new List<int>() { 100 }));
            //Dan.Borders.Add(new Border(ConditionType.OKs, ConditionComparer.Less, false, new List<int>() { 30, 70, 160 }, new List<int>() { 15, 35, 80 }));
            //Dan.Borders.Add(new Border(ConditionType.Bads, ConditionComparer.Less, true, new List<int>() { 25 }, new List<int>() { 3 }));
            //Dan.Borders.Add(new Border(ConditionType.Drumrolls, ConditionComparer.More, false, new List<int>() { 130, 120, 6 }, new List<int>() { 140, 130, 8 }));
            //
            //daniDojo.WriteTJADan(@"D:\Games\Taiko\OpenTaiko\OpenTaiko v0.5.3.1\Songs\S1 Dan-i Dojo\" + Dan.DanSeries + @" test\" + Dan.Title + @"\" + Dan.Title + ".tja", Dan, true);

            //tja.ReadTJA(@"D:\Games\Taiko\TJA Songs\8 stars\Mani Mani(Prod. TAKU INOUE)\Mani Mani(Prod. TAKU INOUE).tja", Difficulty.Oni);

            //var chart = tja.ReadTJA(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\TJA_Nijiro_Songs\test\神竜 ～Shinryu～\神竜 ～Shinryu～.tja", Difficulty.Ura);


            //var chart = fumen.ReadFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\TJA_Nijiro_Songs\new MEGALOVANIA\37022325_x_old.bin", true);

            //chart.Offset = 1103;

            //fumen.WriteFumen(@"D:\Taiko no Tatsujin PC\BepInEx\plugins\TakoTako\customSongs\TJA_Nijiro_Songs\new MEGALOVANIA\37022325_x.bin", chart);

        }

        // In oni chart file, find the first line of FFFFs, 4 bytes before that should be 0s and 1s, 4 bytes before that is offset as a Single, 4 before that is BPM
        private void AddUraChartToTDM(string TJAfilePath, string songID, float msOffset)
        {
            TJA tja = new TJA();
            Fumen fumen = new Fumen();

            var chart = tja.ReadTJA(TJAfilePath, Difficulty.Ura);

            chart.Offset = msOffset;

            string NewFolderPath = @"D:\Downloads\New\customSongs";

            fumen.WriteFumen(Path.Combine(NewFolderPath, songID, songID + "_x.bin"), chart);
            File.Copy(Path.Combine(NewFolderPath, songID, songID + "_x.bin"), Path.Combine(NewFolderPath, songID, songID + "_x_1.bin"));
            File.Copy(Path.Combine(NewFolderPath, songID, songID + "_x.bin"), Path.Combine(NewFolderPath, songID, songID + "_x_2.bin"));

            var (Points, Score) = chart.GetPointsAndScore();
            File.WriteAllText(Path.Combine(NewFolderPath, songID, songID + "_data.txt"), "Level = " + chart.Level + "\nPoints = " + Points + "\nScore = " + Score);
        }

        private void AddUraChartToTDM(string TJAfilePath, string songID, string fumenFilePath)
        {
            Fumen fumen = new Fumen();
            var chart = fumen.ReadFumen(fumenFilePath);
            AddUraChartToTDM(TJAfilePath, songID, chart.Measures[0].Offset);
        }

        private void CreateDonScoreImage(string sourceFilePath)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"D:\Games\Taiko\DonScore\donscore.exe", "\"" + sourceFilePath + "\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            p.Start();
        }

        private void OpenImage(object filePath)
        {
            int attempts = 100;
            while (!File.Exists(filePath.ToString()) && attempts-- > 0)
            {
                Thread.Sleep(100);
            }
            if (File.Exists(filePath.ToString()))
            {
                Process.Start(filePath.ToString());
            }
        }

        private void ConvertPNGsInNewToTJA()
        {
            TJA tja = new TJA();
            DonScore donScore = new DonScore();

            // Converts all PNGs in Downloads\New into TJAs
            DirectoryInfo dirInfo = new DirectoryInfo(@"D:\Downloads\New");
            foreach (var file in dirInfo.EnumerateFiles("*.*").Where(s => s.Name.EndsWith(".png") || s.Name.EndsWith(".txt")))
            {
                var chart = donScore.ReadDonScore(file.FullName, file.Name.Contains("ura") ? Difficulty.Ura : Difficulty.Oni);
                if (chart != null)
                {
                    tja.WriteTJA(Path.Combine(Path.GetDirectoryName(file.FullName), chart.Title + (chart.Difficulty == Difficulty.Ura ? " (裏)" : "") + ".tja"), chart);
                }
            }
            //foreach (var file in dirInfo.GetFiles("*.txt"))
            //{
            //    var chart = donScore.ReadDonScore(file.FullName, file.Name.Contains("ura") ? Difficulty.Ura : Difficulty.Oni);
            //    tja.WriteTJA(chart.Title + (chart.Difficulty == Difficulty.Ura ? " (裏)" : "") + ".tja", chart);
            //}
        }
    }
}
