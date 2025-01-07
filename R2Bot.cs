#define DEBUG_STOPWATCH
using System.CodeDom.Compiler;
using Interceptor;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using Keys = System.Windows.Forms.Keys;
using Newtonsoft.Json.Converters;

namespace R2Bot
{
    public partial class R2Bot : Form
    {
        private KeyboardHook KeyboardHook { get; } = new KeyboardHook();
        private bool IsWorking { get; set; } = false;
        private Input Input { get; set; }
        private string DefaultConfigPath { get; set; } = "default.config.json";
        private string ConfigPath { get; set; } = "bot.config.json";
        static AutoResetEvent LoggerStringEvent = new AutoResetEvent(false);

        private R2BotVar1 R2BotVar1 { get; set; }
        private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new[] { new StringEnumConverter() }
        };

        public R2Bot()
        {
            InitializeComponent();
            InitializeKeyboardHooks();
            InitializeDriverImitator();
            InitializeBot(Input);
            WorkingLabel.Text = "NOT WORKING";

            if (!File.Exists(ConfigPath))
            {
                BotConfiguration config = DefaultBotConfig();
                if (!File.Exists(DefaultConfigPath))
                {
                    var json = JsonConvert.SerializeObject(config, Formatting.Indented, JsonSettings);
                    File.WriteAllText(DefaultConfigPath, json);
                    File.WriteAllText(ConfigPath, json);
                }
                else
                {
                    try
                    {
                        var json = File.ReadAllText(DefaultConfigPath);
                        config = JsonConvert.DeserializeObject<BotConfiguration>(json, JsonSettings);
                        File.WriteAllText(ConfigPath, json);
                    }
                    catch (Exception exception)
                    {
                        Logger(exception.Message);
                    }
                }
            }
            var files = Directory.GetFiles(".\\","*.config.json", SearchOption.TopDirectoryOnly);
            if(files.Length != 0 )
            {
                foreach(var file in files)
                {
                    comboBox1.Items.Add(file);
                }
                comboBox1.SelectedIndex = 0;
            }

            Logger("Choose config; Go to the Game; Activate bot; Enjoy!");
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


        private void InitializeDriverImitator()
        {
            Input = new Input();
            Input.KeyboardFilterMode = KeyboardFilterMode.All;
            var loaded = Input.Load();
        }

        private void InitializeBot(Input input)
        {
            R2BotVar1 = new R2BotVar1(input, Logger);
        }

        private void Logger(string message)
        {
            LogBox.Text = message;
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

        private BotConfiguration ReadBotConfig()
        {
            BotConfiguration config = DefaultBotConfig();
            var path = (string)comboBox1.Items[comboBox1.SelectedIndex];

            try
            {
                var json = File.ReadAllText(path);
                config = JsonConvert.DeserializeObject<BotConfiguration>(json, JsonSettings);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Something went wrong {exception.Message}");
                return DefaultBotConfig();
            }

            return config;
        }

        private BotConfiguration DefaultBotConfig()
        {
            var botConfig = new BotConfiguration();

            botConfig.HpKey = Interceptor.Keys.Q;
            botConfig.HpThreshold = 0.6;
            botConfig.HpSkillThreshold = 0.8;

            botConfig.ManaThreshold = 0.4;

            botConfig.TpKey = Interceptor.Keys.Eight;
            botConfig.TpThreshold = 0.4;

            // Attack
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.One,
                SkillType = BotConfiguration.SkillType.Attack,
                Delay = new TimeSpan(0, 0, 0, 7)
            });

            // Ogon
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.Three,
                SkillType = BotConfiguration.SkillType.Luring,
                Delay = new TimeSpan(0, 0, 0, 35),
            });

            // Totem
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.Four,
                SkillType = BotConfiguration.SkillType.HP,
                Delay = new TimeSpan(0, 0, 3, 0),
            });

            // Master
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.F1,
                SkillType = BotConfiguration.SkillType.Buff,
                Delay = new TimeSpan(0, 0, 9, 0),
            });

            // Speed
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.F2,
                SkillType = BotConfiguration.SkillType.Buff,
                Delay = new TimeSpan(0, 0, 9, 0),
            });

            // Morf
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.F4,
                SkillType = BotConfiguration.SkillType.Buff,
                Delay = new TimeSpan(0, 2, 0, 0),
            });

            return botConfig;
        }

        private void Bot(bool start)
        {
            Console.WriteLine($"Bot start = {start}");
            if (start)
            {
                Logger("The user started execution.");
                var config = ReadBotConfig();
                R2BotVar1.Start(config);
            }
            else
            {
                Logger("The user stopped execution.");
                R2BotVar1.Exit();
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
            //Console.WriteLine($"Testing...");
            //ImageAnalyzer imgAnalyzer = new ImageAnalyzer();
            //var stopwatch = Stopwatch.StartNew();
            //var info = ImageAnalyzer.CaptureScreen();
            //stopwatch.Stop();
            //Console.WriteLine($"stopwatch {stopwatch.ElapsedMilliseconds}; {stopwatch.ElapsedTicks}"); 

            ////var info = ImageAnalyzer.LoadImage(ImageAnalyzer.DefaultPath + "25-01-05-10-47_x=1013_y=543.png"); // no_attack
            ////info.Item2.X = 1013;
            ////info.Item2.Y = 543;

            //imgAnalyzer.ProcessImage(info.Item1, info.Item2, ImageProcessing.Cursor);
        }
    }
}