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
        public class Skill
        {
            public bool IsAttackSkill { get; set; }
            public Interceptor.Keys Key { get; set; }
            public TimeSpan Delay { get; set; }
            public DateTime? Timestamp { get; set; } = null;
            public Double Threshold { get; set; }
        }

        public Interceptor.Keys TpKey { get; set; }
        public double TpThreshold { get; set; }
        public Interceptor.Keys HpKey { get; set; }
        public double HpThreshold { get; set; }

        public double ManaThreshold { get; set; }

        public List<Skill> AllSkills { get; } = new List<Skill>();
    }

    internal class BotInternalConfiguration : BotConfiguration
    {
        public BotInternalConfiguration(BotConfiguration config)
        {
            Health = new Skill()
            {
                IsAttackSkill = false,
                Key = config.HpKey,
                Threshold = config.HpThreshold
            };

            Mana = new Skill()
            {
                IsAttackSkill = false,
                Threshold = config.ManaThreshold
            };

            Tp = new Skill()
            {
                IsAttackSkill = false,
                Key = config.TpKey,
                Threshold = config.TpThreshold
            };

            AttackSkill = AllSkills.Where(e => e.IsAttackSkill).ToList();
            NotAttackSkill = AllSkills.Where(e => !e.IsAttackSkill).ToList();
        }

        public Skill Health { get; }
        public Skill Mana { get; }
        public Skill Tp { get; }

        public List<Skill> AttackSkill { get; }
        public List<Skill> NotAttackSkill { get; }
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
            Exit
        }

        private State CurrentState { get; set; } = State.None;
        private Input Input { get; }
        private ImageAnalyzer ImageAnalyzer { get; } = new ImageAnalyzer();
        private BotInternalConfiguration Config { get; set; }

        public R2BotVar1(Input input)
        {
            Input = input;
        }

        public void Start(BotConfiguration config)
        {
            Debug("Bot started");
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
                            CurrentState = State.Search;
                            break;
                        }

                    case State.Search:
                        {
                            Debug("Search");
                            if (Search())
                            {
                                NumberOfFailedSearch++;
                            }
                            else
                            {
                                NumberOfFailedSearch = 0;
                            }

                            if (NumberOfFailedSearch > 10)
                            {
                                CurrentState = State.Move;
                            }
                            break;
                        }

                    case State.Kill:
                        {
                            Debug("Kill monster");
                            if (KillMonster())
                            {
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
                            Take();
                            CurrentState = State.Skills;
                            break;
                        }

                    case State.TP:
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Input.SendKey(Config.Tp.Key);
                        }
                        Exit();
                        CurrentState = State.Exit;
                        break;
                    }

                    case State.Skills:
                    {
                        ProcessSkills();
                        CurrentState = State.Search;
                        break;
                    }

                    case State.Wait:
                    {
                        break;
                    }

                    case State.Move:
                    {
                        Move();
                        CurrentState = State.Search;
                        break;
                    }

                    case State.Exit:
                    default: 
                        {
                            return;
                        }
                }
            }
        }

        private void Move()
        {
            Input.SendKeyWithDelay(Keys.D, 250);
        }

        private void ProcessSkills()
        {
            foreach (var skill in Config.NotAttackSkill)
            {
                if (skill.Timestamp == null)
                {
                    skill.Timestamp = DateTime.Now;
                    continue;
                }

                if (DateTime.Now - skill.Timestamp >= skill.Delay)
                {
                    Input.SendKey(skill.Key);
                    skill.Timestamp = DateTime.Now;
                    Thread.Sleep(500);
                }
            }
        }

        private void ProcessAttackSkills()
        {

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
            }
            return false;
        }

        private ImageDescription ProcessImage(ImageProcessing flag)
        {
            var info = ImageAnalyzer.CaptureScreen();
            var result = ImageAnalyzer.ProcessImage(info.Item1, info.Item2, flag | ImageProcessing.Mana | ImageProcessing.Health);


            return result;
        }

        private bool Routine(ImageDescription result)
        {
            if (result.IsImageProcessed)
            {
                if (result.Health <= Config.TpThreshold)
                {
                    CurrentState = State.TP;
                    return true;
                }

                if (result.Health < Config.HpThreshold)
                {
                    Input.SendKey(Config.HpKey);
                }
            }

            return false;
        }

        private bool FindMonster()
        {
            var result = ProcessImage(ImageProcessing.Cursor);

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
                        Input.SendRightLeftClick(250);
                        return false;
                    }
                    else
                    {
                        CurrentState = State.Kill;
                        return true;
                    }
                }
                else
                {
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
            Input.SendKey(Interceptor.Keys.One);

            do
            {
                var result = ProcessImage(ImageProcessing.AttackWindow);
                if (Routine(result))
                {
                    return false;
                }

                popupOpen = result.IsAttackWindowOpen;
            }
            while (popupOpen);

            return true;
        }

        private void Take()
        {
            for(var i = 0; i< 5; i++)
            {
                Input.SendKey(Interceptor.Keys.E);
                Thread.Sleep(250);
            }
        }


        [Conditional("DEBUG")]
        public void Debug(string message, params object[] args)
        {
            Console.WriteLine(string.Format(message, args));
        }

    }
}
