using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace WorkshopTimer
{
    // Basic setup swiped from https://www.geoffstratton.com/cnet-countdown-timer
    public partial class Form1 : Form
    {
        private int timeLeft;
        private List<TimeSection> sections;
        WMPLib.WindowsMediaPlayer Player = new WMPLib.WindowsMediaPlayer();
        Dictionary<string,List<string>> MP3s = new Dictionary<string, List<string>>();

        public Form1()
        {
            InitializeComponent();
            InitialSetup();
        }

        #region Setting Up

        private void InitialSetup()
        {
            sections = GetSectionsFromFile();
            ScaleTextToLabelSize(lblTimer);
            RegisterTimerEvents();
            BuildListOfMp3s();
        }

        private void RegisterTimerEvents()
        {
            timer1.Tick += new EventHandler(timer1_Tick);
            timer2.Tick += new EventHandler(timer2_Tick);
            timer3.Tick += new EventHandler(timer3_Tick);
        }

        private void SetUpConsecutiveTimers()
        {
            int totalTime = 0;

            foreach (TimeSection section in sections)
            {
                timeLeft = section.SecondsInEvent + totalTime;
                section.TimeLeft = timeLeft;
                totalTime = timeLeft;
            }
        }

        // Stolen from - https://stackoverflow.com/users/3968207/andro72
        private void ScaleTextToLabelSize(Label lab)
        {
            Image fakeImage = new Bitmap(1, 1); //As we cannot use CreateGraphics() in a class library, so the fake image is used to load the Graphics.
            Graphics graphics = Graphics.FromImage(fakeImage);


            SizeF extent = graphics.MeasureString(lab.Text, lab.Font);


            float hRatio = lab.Height / extent.Height;
            float wRatio = lab.Width / extent.Width;
            float ratio = (hRatio < wRatio) ? hRatio : wRatio;

            float newSize = lab.Font.Size * ratio;

            lab.Font = new Font(lab.Font.FontFamily, newSize, lab.Font.Style);

        }

        private void BuildListOfMp3s()
        {
            MP3s.Add("oneminute", GetMP3sForTimePeriod("60"));
            MP3s.Add("thirty", GetMP3sForTimePeriod("30"));
            MP3s.Add("ten", GetMP3sForTimePeriod("10"));
            MP3s.Add("end", GetMP3sForTimePeriod("End"));
            MP3s.Add("halfway", GetMP3sForTimePeriod("halfway"));
        }

        private List<string> GetMP3sForTimePeriod(string folder)
        {
            List<string> result = new List<string>();
            foreach (string file in Directory.EnumerateFiles(Application.StartupPath + "\\Sounds\\" + folder, "*.mp3"))
            {
                result.Add(file);
            }

            return result;
        }

        private List<TimeSection> GetSectionsFromFile()
        {
            List<TimeSection> result = new List<TimeSection>();

            StreamReader sr = new StreamReader("sections.txt");

            List<string> lines = new List<string>();

            string line = sr.ReadLine();

            while (line != null)
            {
                lines.Add(line);
                line = sr.ReadLine();
            }

            for (int i = 0; i < lines.Count; i = i + 2)
            {
                TimeSection section = new TimeSection(lines[i], Convert.ToInt32(lines[i + 1]));
                result.Add(section);
            }

            return result;
        }

        #endregion

        #region Sound Playing

        private void PlayFile(List<string> files, bool force)
        {
            Console.WriteLine("Player Status on PlayFile Call - " + Player.status);
            if (force || Player.status == "Stopped" || Player.status == "")
            {
                PlayFile(files);
            }
            else
            {
                Console.WriteLine("Skipping output because previous sound playing and force flag not passed.");
            }
        }

        private void PlayFile(List<string> files)
        {
            Random rnd = new Random();
            string url = files[rnd.Next(files.Count)];
            PlayFile(url);
        }

        private void PlayFile(string url)
        {
            Player.controls.stop();
            Player.URL = url;
            Player.controls.play();
            Console.WriteLine("Playing new sound - " + url);
        }

        private void StopFile()
        {
            Player.controls.stop();
        }

        #endregion

        #region Mid-Countdown Methods

        private void UpdateTimeDisplay(TimeSection section)
        {
            var timespan = TimeSpan.FromSeconds(section.TimeLeft);
            lblTimer.Text = section.EventName + Environment.NewLine + timespan.ToString(@"mm\:ss");
            ScaleTextToLabelSize(lblTimer);
        }

        private void RunSoundChecks()
        {
            if (timeLeft > 0)
            {
                timeLeft = timeLeft - 1;

                foreach (TimeSection section in sections)
                {
                    section.TimeLeft = section.TimeLeft - 1;
                }

                // Display time remaining as mm:ss
                foreach (TimeSection section in sections)
                {
                    if (section.TimeLeft > 0)
                    {
                        UpdateTimeDisplay(section);

                        // Dear god make this whole thing better, I can hear my CS lecturers crying
                        if (section.TimeLeft == 1)
                        {
                            // Yes, I feel bad, this is horrific.
                            timer3.Start();
                        }

                        if (section.TimeLeft == 60 && section.OriginalTime > 74)
                        {
                            PlayFile(MP3s["oneminute"], false);
                        }

                        if (section.TimeLeft == 30 && section.OriginalTime > 59)
                        {
                            PlayFile(MP3s["thirty"], false);
                        }

                        if (section.TimeLeft == 10 && section.OriginalTime > 29)
                        {
                            PlayFile(MP3s["ten"], false);
                        }

                        if (section.TimeLeft == section.OriginalTime % 2 && section.OriginalTime > 121)
                        {
                            PlayFile(MP3s["halfway"], false);
                        }
                        break;
                    }
                }
            }
            else
            {
                timer1.Stop();
                var timespan = TimeSpan.FromSeconds(0);
                lblTimer.Text = "Time's Up!";
                btnGo.Enabled = true;
            }
        }

#endregion

        #region Timer Events

        private void timer1_Tick(object sender, EventArgs e)
        {
            RunSoundChecks();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // Only used to stop End of section sounds at 5 seconds.
            StopFile();
            timer2.Stop();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            PlayFile(MP3s["End"], true);
            timer2.Start(); // plays end noises for 5 seconds
            timer3.Stop();
        }

        #endregion

        #region Form Events

        private void btnGo_Click(object sender, EventArgs e)
        {
            btnGo.Enabled = false;

            SetUpConsecutiveTimers();

            timer1.Start();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            ScaleTextToLabelSize(lblTimer);
        }

#endregion

    }
}
