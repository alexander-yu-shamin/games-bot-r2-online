
using Interceptor;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Keys = System.Windows.Forms.Keys;

namespace R2Bot
{
    public partial class R2Bot : Form
    {
        private KeyboardHook KeyboardHook { get; } = new KeyboardHook();
        private bool IsWorking { get; set; } = false;
        private Input Input { get; set; }

        private R2BotVar1 Var1 { get; set; }

        public R2Bot()
        {
            InitializeComponent();
            InitializeGroups();
            InitializeKeyboardHooks();
            InitializeDriverImitator();
            InitializeBot(Input);
            WorkingLabel.Text = "NOT WORKING";
        }


        private enum TypeSkill
        {
            Hill,
            Tp,
            Skill,
        }

        private class SkillElement
        {
            public TypeSkill TypeSkill { get; set; }
            public GroupBox GroupBox { get; set; }
            public CheckBox CheckBox { get; set; }
            public TextBox Duration { get; set; }
            public TextBox Key { get; set; }
        }

        private List<SkillElement> Elements { get; } = new List<SkillElement>();

        private void InitializeGroups()
        {
            var startY = 40;
            var spacing = 100;
            var defaultDuration = TimeSpan.FromSeconds(50);

            for (var i = 0; i < 5; i++)
            {
                var groupBox = new GroupBox
                {
                    Name = $"Skill #{i + 1}",
                    Text = $"Skill #{i + 1}",
                    Size = new Size(450, 100),
                    Location = new Point(10, startY + (i * spacing)),
                };

                var checkBox = new CheckBox
                {
                    Text = "Enable",
                    Location = new Point(10, 20),
                    Checked = true,
                    AutoSize = true
                };

                Label durationLabel = new Label
                {
                    Text = "Duration:",
                    Location = new System.Drawing.Point(8, 45),
                    AutoSize = true
                };

                TextBox durationTextBox = new TextBox
                {
                    Location = new System.Drawing.Point(70, 40),
                    Width = 100,
                    Text = defaultDuration.ToString()
                };

                Label keyLabel = new Label
                {
                    Text = "Key:",
                    Location = new System.Drawing.Point(180, 45),
                    AutoSize = true
                };

                TextBox keyTextBox = new TextBox
                {
                    Location = new System.Drawing.Point(210, 40),
                    Width = 100,
                    Text = $"{i + 1}"
                };

                groupBox.Controls.Add(checkBox);
                groupBox.Controls.Add(durationLabel);
                groupBox.Controls.Add(durationTextBox);
                groupBox.Controls.Add(keyLabel);
                groupBox.Controls.Add(keyTextBox);

                var element = new SkillElement
                {

                    GroupBox = groupBox,
                    CheckBox = checkBox,
                    Duration = durationTextBox,
                    Key = keyTextBox,
                    TypeSkill = TypeSkill.Skill
                };

                this.Controls.Add(groupBox);

                Elements.Add(element);
            }

            // TP
            var groupTPBox = new GroupBox
            {
                Name = $"TP",
                Text = $"TP",
                Size = new Size(450, 100),
                Location = new Point(10, startY + (5 * spacing)),
            };

            var checkTPBox = new CheckBox
            {
                Text = "Enable",
                Location = new Point(10, 20),
                Checked = true,
                AutoSize = true
            };

            Label keyTPLabel = new Label
            {
                Text = "Key:",
                Location = new System.Drawing.Point(180, 45),
                AutoSize = true
            };

            TextBox keyTPTextBox = new TextBox
            {
                Location = new System.Drawing.Point(210, 40),
                Width = 100,
                Text = $"8"
            };

            groupTPBox.Controls.Add(checkTPBox);
            groupTPBox.Controls.Add(keyTPLabel);
            groupTPBox.Controls.Add(keyTPTextBox);

            var elementTP = new SkillElement
            {
                GroupBox = groupTPBox,
                CheckBox = checkTPBox,
                Key = keyTPTextBox,
                TypeSkill = TypeSkill.Tp
            };

            this.Controls.Add(groupTPBox);

            Elements.Add(elementTP);

            // HILL
            var groupHIllBox = new GroupBox
            {
                Name = $"HIll",
                Text = $"HIll",
                Size = new Size(450, 100),
                Location = new Point(10, startY + (6 * spacing)),
            };

            var checkHIllBox = new CheckBox
            {
                Text = "Enable",
                Location = new Point(10, 20),
                Checked = true,
                AutoSize = true
            };

            Label keyHIllLabel = new Label
            {
                Text = "Key:",
                Location = new System.Drawing.Point(180, 45),
                AutoSize = true
            };

            TextBox keyHIllTextBox = new TextBox
            {
                Location = new System.Drawing.Point(210, 40),
                Width = 100,
                Text = $"q"
            };

            groupHIllBox.Controls.Add(checkHIllBox);
            groupHIllBox.Controls.Add(keyHIllLabel);
            groupHIllBox.Controls.Add(keyHIllTextBox);

            var elementHIll = new SkillElement
            {
                GroupBox = groupHIllBox,
                CheckBox = checkHIllBox,
                Key = keyHIllTextBox,
                TypeSkill = TypeSkill.Hill
            };

            this.Controls.Add(groupHIllBox);

            Elements.Add(elementHIll);
        }

        private void InitializeDriverImitator()
        {
            Input = new Input();
            Input.KeyboardFilterMode = KeyboardFilterMode.All;
            var loaded = Input.Load();
            Console.WriteLine($"Initialize Input {loaded}");
        }

        private void InitializeBot(Input input)
        {
            Var1 = new R2BotVar1(input);
            Console.WriteLine($"Initialize bot");
        }

        private void InitializeKeyboardHooks()
        {
            KeyboardHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(KeyboardHook_KeyPressed);

            KeyboardHook.RegisterHotKey(global::R2Bot.ModifierKeys.Alt, Keys.F12);
            KeyboardHook.RegisterHotKey(global::R2Bot.ModifierKeys.Alt, Keys.F11);
        }

        void KeyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            var defaultModifier = global::R2Bot.ModifierKeys.Alt;
            if (e.Modifier == defaultModifier)
            {
                if (e.Key == System.Windows.Forms.Keys.F12)
                {
                    IsWorking = !IsWorking;
                    WorkingLabel.Text = IsWorking ? "WORKING" : "NOT WORKING";
                    Bot(IsWorking);
                }

                if (e.Key == Keys.F11)
                {
                    Testing();
                }
            }
        }

        private void R2Bot_Load(object? sender, EventArgs e)
        {
        }

        private void R2Bot_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.trayIcon.Visible = false;
            }
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.Show();
            }
        }

        private void Bot(bool start)
        {
            Console.WriteLine($"Bot start = {start}");
            if (start) 
            {
                Var1.Start();
            }
            else{
                Var1.Exit();
            }
        }

        private void Testing()
        {
            Console.WriteLine($"Testing...");
            ImageAnalyzer imgAnalyzer = new ImageAnalyzer();
            var info = imgAnalyzer.LoadImage(ImageAnalyzer.DefaultPath + "2025_01_02 19_54_43.jpg");
            var img =  info.Item1.ToImage<Bgra, Byte>();
            CvInvoke.NamedWindow("bgra");
            CvInvoke.Imshow("bgra", img);
            var gray = new Image<Gray, byte>(img.Size);
            CvInvoke.CvtColor(img, gray, ColorConversion.Bgra2Gray);

            CvInvoke.NamedWindow("gray");
            CvInvoke.Imshow("gray", gray);

            //var rect = gray.GetSubRect(new Rectangle(856, 929, 208, 43)); // окно моба
            var rect = gray.GetSubRect(new Rectangle(858, 940, 206, 20));

            CvInvoke.NamedWindow("rect");
            CvInvoke.Imshow("rect", rect);

            var threshold = rect.ThresholdBinary(new Gray(128), new Gray(255));

            CvInvoke.NamedWindow("threshold");
            CvInvoke.Imshow("threshold", threshold);


            //System.IO.Directory.CreateDirectory("./tessdata");
            //imgAnalyzer.TesseractDownloadLangFile("./tessdata", "rus");

            var tesseract = new Tesseract("./tessdata/", "rus", OcrEngineMode.Default);
            tesseract.SetImage(threshold);
            var result = tesseract.Recognize();
            Console.WriteLine($"tesseract result {result}");

            Console.WriteLine(tesseract.GetHOCRText());
            Console.WriteLine(tesseract.GetUTF8Text());
            Console.WriteLine(tesseract.GetTSVText());
            Console.WriteLine(tesseract.GetUNLVText());











        }
    }
}