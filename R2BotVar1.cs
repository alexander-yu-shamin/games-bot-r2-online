//#define DEBUG_STOPWATCH
using Interceptor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Keys = Interceptor.Keys;
using Timer = System.Timers.Timer;

namespace R2Bot
{

    internal class BotConfiguration
    {

        public enum SkillType 
        {
            Attack,
            HP,
            Buff,
            Luring,
            Special
        }
        public class Skill
        {
            public SkillType SkillType{ get; set; }
            public Interceptor.Keys Key { get; set; }
            public TimeSpan Delay { get; set; }
            public DateTime? Timestamp { get; set; } = null;
            public Double Threshold { get; set; }
            public int ExecutionMs { get; set; }
        }

        public Interceptor.Keys TpKey { get; set; }
        public double TpThreshold { get; set; }
        public Interceptor.Keys HpKey { get; set; }
        public double HpThreshold { get; set; }
        public double HpSkillThreshold{ get; set; }
        public double ManaThreshold { get; set; }
        public int NumberFailedSearchBeforeMove { get; set; } = 7;
        public bool IsLuringEnabled { get; set; } = true;
        public bool IsTakeEnabled { get; set; } = true;
        public bool MoveAfterEachKill { get; set; } = false;
        public int RotateDelayMs { get; set; }
        public TimeSpan MaxAttackTime { get; set; } = TimeSpan.FromMinutes(2);

        public List<Skill> AllSkills { get; set; } = new List<Skill>();
    }

    internal class BotInternalConfiguration : BotConfiguration
    {
        public BotInternalConfiguration(BotConfiguration config)
        {
            Health = new Skill()
            {
                SkillType = SkillType.Special,
                Key = config.HpKey,
                Threshold = config.HpThreshold
            };

            Mana = new Skill()
            {
                SkillType = SkillType.Special,
                Threshold = config.ManaThreshold
            };

            Tp = new Skill()
            {
                SkillType = SkillType.Special,
                Key = config.TpKey,
                Threshold = config.TpThreshold
            };

            HpSkillThreshold = config.HpSkillThreshold;
            NumberFailedSearchBeforeMove = config.NumberFailedSearchBeforeMove;
            IsLuringEnabled = config.IsLuringEnabled;
            IsTakeEnabled = config.IsTakeEnabled;
            MoveAfterEachKill = config.MoveAfterEachKill;
            RotateDelayMs = config.RotateDelayMs;
            MaxAttackTime = config.MaxAttackTime;

            AllSkills = config.AllSkills;

            AttackSkills = AllSkills.Where(e => e.SkillType == SkillType.Attack).ToList();
            BuffSkills = AllSkills.Where(e => e.SkillType == SkillType.Buff).ToList();
            LuringSkills = AllSkills.Where(e => e.SkillType == SkillType.Luring).ToList();
            HPSkills = AllSkills.Where(e => e.SkillType == SkillType.HP).ToList();
        }

        public Skill Health { get; }
        public Skill Mana { get; }
        public Skill Tp { get; }

        public List<Skill> AttackSkills { get; }
        public List<Skill> HPSkills { get; }
        public List<Skill> BuffSkills { get; }
        public List<Skill> LuringSkills { get; }
    }

    internal class R2BotVar1
    {
        public Thread MainThread { get; set; }
        private bool ExitFlag { get; set; } = false;

        private enum State
        {
            None,
            Search,
            Kill,
            Take,
            Wait,
            TP,
            Skills,
            Move,
            Luring,
            Exit
        }

        private State CurrentState { get; set; } = State.None;
        private Input Input { get; }
        private ImageAnalyzer ImageAnalyzer { get; } = new ImageAnalyzer();
        private BotInternalConfiguration Config { get; set; }
        private DateTime StartTime { get; set; }
        private Action<string> Logger { get; set; }
        private ImageDescription Result { get; set; }

        public R2BotVar1(Input input, Action<string> logger)
        {
            Input = input;
            //Logger = logger;
        }

        ~R2BotVar1()
        {
            
        }

        public void Start(BotConfiguration config)
        {
            Debug("Bot started");
            ExitFlag = false;
            CurrentState = State.None;
            StartTime = DateTime.Now;
            Config = new BotInternalConfiguration(config);
            MainThread = new Thread(Run);
            MainThread.Start();
        }

        public void Exit()
        {
            ExitFlag = true;
        }


        private int NumberOfFailedSearch { get; set; } = 0;
        private void Run()
        {
            while (!ExitFlag)
            {
                switch (CurrentState)
                {
                    case State.None:
                        {
                            Debug("State: None");
                            CurrentState = State.Search;
                            break;
                        }

                    case State.Search:
                        {
                            Debug("State: Search");
                            if (Search())
                            {
                                Debug($"Searched success!"); 
                                NumberOfFailedSearch = 0;
                            }
                            else
                            {
                                Debug($"Searched failed {NumberOfFailedSearch}");
                                NumberOfFailedSearch++;
                                ProcessBuffs();
                                ProcessLurings();
                            }

                            if (NumberOfFailedSearch > Config.NumberFailedSearchBeforeMove)
                            {
                                CurrentState = State.Move;
                                NumberOfFailedSearch = 0;
                            }
                            break;
                        }

                    case State.Kill:
                        {
                            Debug("State: Kill");
                            var result = KillMonster();
                            if(ExitFlag)
                            {
                                CurrentState = State.Exit;
                            }
                            if (result)
                            {
                                FiredAttackSkills();
                                CurrentState = State.Take;
                            }
                            else
                            {
                                CurrentState = State.TP;
                            }
                            break;
                        }

                    case State.Take:
                        {
                            Debug("State: Take");
                            Take();
                            CurrentState = State.Skills;
                            break;
                        }

                    case State.TP:
                    {
                        Debug("State: TP");

                        if(DateTime.Now - StartTime <  TimeSpan.FromSeconds(20))
                        {
                            Logger?.Invoke($"Something went wrong; Health: {Result?.Health};");
                        }
                        else
                        {
                            var info = ImageAnalyzer.CaptureScreen();
                            var filename = DateTime.Now.ToString("yy-MM-dd-hh-mm");
                            ImageAnalyzer.SaveImage(info.Item1, info.Item2, filename);
                            for (int i = 0; i < 5; i++)
                            {
                                Input.SendKey(Config.Tp.Key);
                                Thread.Sleep(250);
                            }
                            Logger?.Invoke($"The Bot TP. Started: {StartTime.ToString()}; Worked: {(DateTime.Now - StartTime).ToString()}");
                        }
                        Exit();
                        CurrentState = State.Exit;
                        break;
                    }

                    case State.Skills:
                    {
                        Debug("State: Skills");
                        ProcessBuffs();
                        if (Config.MoveAfterEachKill)
                        {
                            CurrentState = State.Move;
                        }
                        else
                        {
                            CurrentState = State.Luring; 
                        }
                        break;
                    }

                    case State.Wait:
                    {
                        Debug("State: Wait");
                        break;
                    }

                    case State.Move:
                    {
                        Debug("State: Move");
                        Move();
                        if (Config.MoveAfterEachKill)
                        {
                            CurrentState = State.Luring; 
                        }
                        else
                        {
                            CurrentState = State.Search;
                        }
                        break;
                    }
                    case State.Luring:
                        {
                            ProcessLurings();
                            CurrentState = State.Search;

                            break;
                        }

                    case State.Exit:
                    default: 
                        {
                            Debug("State: Exit, default");
                            return;
                        }
                }
            }
        }

        private void Move()
        {
            Input.SendKeyWithDelay(Keys.D, Config.RotateDelayMs);
        }

        private void ProcessAttackSkills()
        {
            ProcessSkills(Config.AttackSkills, false, false); 
        }

        private void FiredAttackSkills()
        {
            foreach(var skill in Config.AttackSkills)
            {
                skill.Timestamp = null;
            }
        }

        private void ProcessBuffs()
        {
            ProcessSkills(Config.BuffSkills, true, true); 
        }

        private void ProcessLurings()
        {
            if(Config.IsLuringEnabled)
            {
                if(Config.IsTakeEnabled)
                {
                    Input.SendKey(Interceptor.Keys.E);
                }
                ProcessSkills(Config.LuringSkills, true, true); 
            }
        }

        private bool ProcessHP()
        {
            var anyCanBeCalled = Config.HPSkills.Any(el =>
            {
                if (el.Timestamp != null)
                {
                    return DateTime.Now - el.Timestamp >= el.Delay;

                }
                return true;
            });

            if(anyCanBeCalled)
            {
                ProcessSkills(Config.HPSkills, true, false);
                return true;
            }
            else{
                return false;
            }
        }

        private void ProcessSkills(List<BotConfiguration.Skill> skills, bool shouldBeInit, bool shouldWaitAfterUse)
        {
            foreach(var skill in skills) 
            {
                if(skill.Timestamp == null)
                {
                    skill.Timestamp = DateTime.Now;
                    if(shouldBeInit)
                    {
                        Input.SendKey(skill.Key);
                        if(shouldWaitAfterUse)
                        {
                            Thread.Sleep(skill.ExecutionMs); //2750
                        }
                        continue;
                    }
                }

                if(DateTime.Now - skill.Timestamp >= skill.Delay)
                {
                    skill.Timestamp = DateTime.Now;
                    Input.SendKey(skill.Key);
                    if(shouldWaitAfterUse)
                    {
                        Thread.Sleep(skill.ExecutionMs);
                    }
                }
            }

        }
        private bool Search()
        {
            var max = 65536;
            var border_min = 4096;
            var border_max = max - border_min;

            var max_x = border_max;
            var max_y = border_max;

            var center_x = max / 2;
            var center_y = max / 2;

            var x = center_x;
            var y = center_y;

            MoveMouseTo(x, y);

            var steps = 40;
            var step_x = max_x / steps;
            var step_y = max_y / steps;
            var speed = 64;

            Debug("Search started!");
            for (var i = 0; i < 20; i++)
            {
                if(ExitFlag)
                {
                    CurrentState = State.Exit;
                    return true;
                }

                for (var j = 0; j < i; j++)
                {
                    x += step_x;
                    if(x < border_min || x > border_max)
                    {
                        break;
                    }

                    MoveMouseTo(x, y);
                    if(FindMonster())
                    {
                        return true;
                    }
                }

                for (var j = 0; j < i; j++)
                {
                    y += step_y;
                    if(y < border_min || y > border_max)
                    {
                        break;
                    }

                    MoveMouseTo(x, y);
                    if(FindMonster())
                    {
                        return true;
                    }
                }

                step_x = step_x + Math.Sign(step_x) * speed;
                step_y = step_y + Math.Sign(step_y) * speed;

                step_x = -step_x;
                step_y = -step_y;
                ProcessLurings();
            }
            Debug("Search Finished!");
            return false;
        }

        private ImageDescription ProcessImage(ImageProcessing flag)
        {
#if DEBUG_STOPWATCH
            var stopwatchScreen = Stopwatch.StartNew();
#endif
        var info = ImageAnalyzer.CaptureScreen();
#if DEBUG_STOPWATCH
            stopwatchScreen.Stop();
            Debug($"ProcessImage: CaptureScreen: {stopwatchScreen.ElapsedMilliseconds}; {stopwatchScreen.ElapsedTicks}");
#endif

#if DEBUG_STOPWATCH
            var stopwatchProcess = Stopwatch.StartNew();
#endif
            var result = ImageAnalyzer.ProcessImage(info.Item1, info.Item2, flag | ImageProcessing.Mana | ImageProcessing.Health);
            Result = result;
#if DEBUG_STOPWATCH
            stopwatchProcess.Stop();
            Debug($"ProcessImage: ProcessImage: {stopwatchProcess.ElapsedMilliseconds}; {stopwatchProcess.ElapsedTicks}");
#endif

            return result;
        }

        private bool Routine(ImageDescription result)
        {
            if (result.IsImageProcessed)
            {
                if (result.Health <= Config.Tp.Threshold)
                {
                    Debug($"TP = {result.Health} <= {Config.Tp.Threshold}");
                    CurrentState = State.TP;
                    return true;
                }

                if(result.Health <= Config.HpSkillThreshold)
                {
                    Debug($"Health Skill = {result.Health} <= {Config.HpSkillThreshold}");
                    ProcessHP();
                }

                if (result.Health < Config.Health.Threshold)
                {
                    Debug($"Health = {result.Health} <= {Config.Health.Threshold}");
                    if(!ProcessHP())
                    {
                        Input.SendKey(Config.Health.Key);
                    }
                }
            }

            return false;
        }

        private bool FindMonster()
        {
#if DEBUG_STOPWATCH
            var stopwatch = Stopwatch.StartNew();
#endif

            var result = ProcessImage(ImageProcessing.Cursor);

#if DEBUG_STOPWATCH
            stopwatch.Stop();
            Debug($"FindMonster: {stopwatch.ElapsedMilliseconds}; {stopwatch.ElapsedTicks}");
#endif
            if (!result.IsImageProcessed)
            {
                Debug("Image isn't processed");
                return false;
            }

            if (Routine(result))
            {
                CurrentState = State.TP;
                return true;
            }

            if (result.Cursor == CursorType.Attack)
            {
                Input.SendLeftRightClick(250);

                result = ProcessImage(ImageProcessing.AttackName);
                if (!string.IsNullOrEmpty(result.AttackObjectName))
                {
                    if (result.AttackObjectName.Contains("Тотем жизни"))
                    {
                        Debug("Тотем жизни!");
                        Input.SendRightLeftClick(250);
                        return false;
                    }
                    else
                    {
                        Debug("Не Тотем жизни!");
                        CurrentState = State.Kill;
                        return true;
                    }
                }
                else
                {
                    Debug("Find monster kill!");
                    CurrentState = State.Kill;
                    return true;
                }
            }

            return false;
        }

        private void MoveMouseTo(int x, int y)
        {
            Input.MoveMouseTo((int)x, (int)y, true);
        }

        private bool KillMonster()
        {
            var popupOpen = true;
            var startTime = DateTime.Now;

            do
            {
                if (DateTime.Now - startTime > Config.MaxAttackTime)
                {
                    return true;
                }

                if(ExitFlag)
                {
                    return true;
                }

                var result = ProcessImage(ImageProcessing.AttackWindow);
                if (Routine(result))
                {
                    return false;
                }

                ProcessAttackSkills();
                popupOpen = result.IsAttackWindowOpen;
            }
            while (popupOpen);

            return true;
        }

        private void Take()
        {
            if(Config.IsTakeEnabled)
            {
                for(var i = 0; i< 6; i++)
                {
                    Debug("Take");
                    Input.SendKey(Interceptor.Keys.E);
                    Thread.Sleep(800);
                }
            }
        }


        [Conditional("DEBUG_NO")]
        public void Debug(string message, params object[] args)
        {
            Console.WriteLine(string.Format(message, args));
        }

    }
}
