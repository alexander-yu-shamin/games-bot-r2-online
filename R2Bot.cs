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
using System.Collections.Concurrent;

namespace R2Bot
{
    public partial class R2Bot : Form
    {
        private KeyboardHook KeyboardHook { get; } = new KeyboardHook();
        private bool IsWorking { get; set; } = false;
        private Input Input { get; set; }
        private string DefaultConfigPath { get; set; } = "default.config.json";
        private string ConfigPath { get; set; } = "bot.config.json";
        private string ClientConfigPath { get; set; } = "bot.client.json";
        private string DefaultClientConfigPath { get; set; } = "default.client.json";
        static AutoResetEvent LoggerStringEvent = new AutoResetEvent(false);

        private R2BotVar1 R2BotVar1 { get; set; }
        private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new[] { new StringEnumConverter() }
        };

        StreamWriter Writer;
        public R2Bot()
        {

            StreamWriter Writer = new StreamWriter("log.txt", true);// `true` → добавляет в конец файла
            Writer.AutoFlush = true;
            Console.SetOut(Writer);

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

            var files = Directory.GetFiles(".\\", "*.config.json", SearchOption.TopDirectoryOnly);
            if (files.Length != 0)
            {
                foreach (var file in files)
                {
                    comboBox1.Items.Add(file);
                }
                comboBox1.SelectedIndex = 0;
            }

            if (!File.Exists(ClientConfigPath))
            {
                ImageAnalyzerConfig config = new ImageAnalyzerConfig();
                if (!File.Exists(DefaultClientConfigPath))
                {
                    var json = JsonConvert.SerializeObject(config, Formatting.Indented, JsonSettings);
                    File.WriteAllText(DefaultClientConfigPath, json);
                    File.WriteAllText(ClientConfigPath, json);
                }
                else
                {
                    try
                    {
                        var json = File.ReadAllText(DefaultClientConfigPath);
                        config = JsonConvert.DeserializeObject<ImageAnalyzerConfig>(json, JsonSettings);
                        File.WriteAllText(ClientConfigPath, json);
                    }
                    catch (Exception exception)
                    {
                        Logger(exception.Message);
                    }
                }
            }

            files = Directory.GetFiles(".\\", "*.client.json", SearchOption.TopDirectoryOnly);
            if (files.Length != 0)
            {
                foreach (var file in files)
                {
                    comboBox2.Items.Add(file);
                }
                comboBox2.SelectedIndex = 0;
            }

            foreach (var cursor in Enum.GetValues<CursorType>())
            {
                comboBox3.Items.Add(cursor.ToString());
            }
            comboBox3.SelectedIndex = 0;

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
            Input.Unload();
            var loaded = Input.Load();
            if (!loaded)
            {
                Logger("Input isn't loaded");
            }
        }

        private void InitializeBot(Input input)
        {
            R2BotVar1 = new R2BotVar1(input, Logger);
        }

        private List<string> LogBuffer = new List<string>();
        private const int MaxLines = 5;

        private void Logger(string message)
        {
            Console.WriteLine(message);
            LogBuffer.Add(message);
            if(LogBuffer.Count > MaxLines)
            {
                LogBuffer.RemoveAt(0);
            }

            if (LogBox.InvokeRequired)
            {
                LogBox.Invoke(() => { LogBox.Text = string.Join(Environment.NewLine, LogBuffer); });
            }
            else
            {
                LogBox.Text = string.Join(Environment.NewLine, LogBuffer);
            }
        }

        private void InitializeKeyboardHooks()
        {
            KeyboardHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(KeyboardHook_KeyPressed);

            KeyboardHook.RegisterHotKey(global::R2Bot.ModifierKeys.Alt, Keys.F12);
            KeyboardHook.RegisterHotKey(global::R2Bot.ModifierKeys.Alt, Keys.F11);
            KeyboardHook.RegisterHotKey(global::R2Bot.ModifierKeys.Alt, Keys.F10);
            KeyboardHook.RegisterHotKey(global::R2Bot.ModifierKeys.Alt, Keys.F9);
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
                    SaveImage();
                }

                if (e.Key == Keys.F10)
                {
                    TestingClient();
                }

                if (e.Key == Keys.F9)
                {
                    SetupClient();
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

        private void SaveImageAnalyzerConfig(ImageAnalyzerConfig config)
        {
            var path = (string)comboBox2.Items[comboBox2.SelectedIndex];
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented, JsonSettings);
                File.WriteAllText(path, json);
            }
            catch (Exception exception)
            {
                Logger($"Cannot save config {path}; {exception.Message}");
                Console.WriteLine($"Something went wrong {exception.Message}");
            }
        }

        private ImageAnalyzerConfig ReadImageAnalyzerConfig()
        {
            ImageAnalyzerConfig config = null;

            var path = (string)comboBox2.Items[comboBox2.SelectedIndex];

            try
            {
                var json = File.ReadAllText(path);
                config = JsonConvert.DeserializeObject<ImageAnalyzerConfig>(json, JsonSettings);
            }
            catch (Exception exception)
            {
                Logger("The client config has an error.");
                Console.WriteLine($"Something went wrong {exception.Message}");
                return null;
            }

            return config;
        }

        private BotConfiguration ReadBotConfig()
        {
            BotConfiguration config = null;
            var path = (string)comboBox1.Items[comboBox1.SelectedIndex];

            try
            {
                var json = File.ReadAllText(path);
                config = JsonConvert.DeserializeObject<BotConfiguration>(json, JsonSettings);
            }
            catch (Exception exception)
            {
                Logger("The bot config has an error.");
                Console.WriteLine($"Something went wrong {exception.Message}");
                return null;
            }

            return config;
        }


        private BotConfiguration DefaultBotConfig()
        {
            var botConfig = new BotConfiguration();

            botConfig.HpKey = Interceptor.Keys.F1;
            botConfig.HpThreshold = 0.6;
            botConfig.HpSkillThreshold = 0.8;

            botConfig.ManaThreshold = 0.4;

            botConfig.TpKey = Interceptor.Keys.Q;
            botConfig.TpThreshold = 0.4;

            // Attack
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.One,
                SkillType = BotConfiguration.SkillType.Attack,
                Delay = new TimeSpan(0, 0, 0, 7)
            });

            // Goblin
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.Two,
                SkillType = BotConfiguration.SkillType.Attack,
                Delay = new TimeSpan(0, 0, 0, 7),
                ExecutionMs = 1500
            });

            // Ogon
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.Three,
                SkillType = BotConfiguration.SkillType.Luring,
                Delay = new TimeSpan(0, 0, 0, 35),
                ExecutionMs = 1500
            });

            // Totem
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.Four,
                SkillType = BotConfiguration.SkillType.HP,
                Delay = new TimeSpan(0, 0, 3, 0),
                ExecutionMs = 1000
            });

            // Master
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.F2,
                SkillType = BotConfiguration.SkillType.Buff,
                Delay = new TimeSpan(0, 0, 9, 0),
                ExecutionMs = 3000
            });

            // Speed
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.F3,
                SkillType = BotConfiguration.SkillType.Buff,
                Delay = new TimeSpan(0, 0, 9, 0),
                ExecutionMs = 3000
            });

            // Morf
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.F4,
                SkillType = BotConfiguration.SkillType.Buff,
                Delay = new TimeSpan(0, 2, 0, 0),
                ExecutionMs = 0
            });

            // Speed bank
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.F5,
                SkillType = BotConfiguration.SkillType.Buff,
                Delay = new TimeSpan(0, 0, 15, 0),
                ExecutionMs = 0
            });

            // eat
            botConfig.AllSkills.Add(new BotConfiguration.Skill
            {
                Key = Interceptor.Keys.F7,
                SkillType = BotConfiguration.SkillType.Buff,
                Delay = new TimeSpan(0, 0, 45, 0),
                ExecutionMs = 0
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
                var clientConfig = ReadImageAnalyzerConfig();
                if (config == null)
                {
                    Logger("The config is bad. Choose another one.");
                    return;
                }
                if (clientConfig == null)
                {
                    Logger("The client config is bad. Choose another one.");
                    return;
                }

                R2BotVar1.Start(config, clientConfig);
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
                Logger($"Image saved as {filename}");
                Console.WriteLine($"Saved capture screen image to {filename}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Cannot save file. Exception = {exception.Message}");
            }
        }

        private void TestingInput()
        {
            Task.Run(DoTestingInputTask);
        }

        private async Task DoTestingInputTask()
        {
            Logger("Testing input. Mouse should move in the screen center...");
            Input.MoveMouseTo(1920 / 2, 1080 / 2);
            Logger("The mouse pointer should be in the screen center.");

            for (var i = 1920 / 2 - 200; i <= 1920 / 2 + 200; i += 50)
            {
                for (var j = 1080 / 2 - 200; j <= 1080 / 2 + 200; j += 50)
                {
                    await Task.Delay(50);
                    Input.MoveMouseTo(i, j);
                }
            }
            Logger("The input test is over.");
        }

        private void TestingClient()
        {
            Logger("Testing client...");
            var config = ReadImageAnalyzerConfig();
            if (config == null)
            {
                Logger("Cannot read config. Check it.");
                return;
            }

            var imgAnalyzer = new ImageAnalyzer(config);
            var info = ImageAnalyzer.CaptureScreen();
            Logger(imgAnalyzer.DrawDebug(info.Item1, info.Item2, ImageProcessing.None));
        }


        private void SetupClient()
        {
            var config = ReadImageAnalyzerConfig();
            if (config == null)
            {
                Logger("Cannot read config. Check it.");
                return;
            }

            var imgAnalyzer = new ImageAnalyzer(config);
            var info = ImageAnalyzer.CaptureScreen();

            var cursorName = (string)comboBox3.Items[comboBox3.SelectedIndex];
            CursorType cursor = (CursorType)Enum.Parse(typeof(CursorType), cursorName);
            Logger($"Find cursor {cursor}");

            var data = imgAnalyzer.GetCursorData(info.Item1, info.Item2);

            switch (cursor)
            {

                case CursorType.Normal:
                    {
                        config.NormalImageBgra = data;
                        break;
                    }

                case CursorType.Take:
                    {
                        config.TakeImageBgra = data;
                        break;
                    }

                case CursorType.Attack:
                    {
                        config.AttackImageBgra = data;
                        break;
                    }

                case CursorType.NoAttack:
                    {
                        config.NoAttackImageBgra = data;
                        break;
                    }

                case CursorType.None:
                default:
                    {
                        Logger("You should choose the cursor type first.");
                        return;
                    }
            }

            Logger("Config has been updated");
            SaveImageAnalyzerConfig(config);
        }

        private void R2Bot_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (Input.IsLoaded)
                {
                    Input.Unload();
                }
                Writer.Flush();
                Writer.Close();
            }
            catch
            {

            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}