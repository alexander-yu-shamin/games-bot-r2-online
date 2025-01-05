
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
            KeyboardHook.RegisterHotKey(global::R2Bot.ModifierKeys.Alt, Keys.F10);
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

                if (e.Key == Keys.F10)
                {
                    SaveImage();
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

        private void SaveImage()
        {
            try
            {
                var info = ImageAnalyzer.CaptureScreen();
                var filename = DateTime.Now.ToString("yy-MM-dd-hh-mm");
                ImageAnalyzer.SaveImage(info.Item1, info.Item2, filename);
                Console.WriteLine($"Saved capture screen image to {filename}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Cannot save file. Exception = {exception.Message}");
            }
        }

        private void Testing()
        {
            Console.WriteLine($"Testing...");
            ImageAnalyzer imgAnalyzer = new ImageAnalyzer();
            //var info = imgAnalyzer.LoadImage(ImageAnalyzer.DefaultPath + "2025_01_02 19_54_43.jpg");
            //var info = imgAnalyzer.LoadImage(ImageAnalyzer.DefaultPath + "2025_01_05 19_51_53.jpg");
            var info = ImageAnalyzer.LoadImage(ImageAnalyzer.DefaultPath + "2025_01_03 23_49_32.jpg");
            imgAnalyzer.ProcessImage(info.Item1, info.Item2, ImageProcessing.Health | ImageProcessing.Mana);
        }
    }
}