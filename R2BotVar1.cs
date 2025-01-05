using Interceptor;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace R2Bot
{
    internal class R2BotVar1
    {
        private Thread BotThread { get; set; }
        private bool ExitFlag { get; set; } = false;

        private enum State
        {
            None,
            Search,
            Kill,
            Take,
            Wait,
            TP,
            Exit
        }

        private State CurrentState { get; set; } = State.None;
        private Input Input { get; }
        private ImageAnalyzer ImageAnalyzer { get; } = new ImageAnalyzer();

        public R2BotVar1(Input input)
        {
            Input = input;
        }

        public void Start()
        {
            if (BotThread != null)
            {
                return;
            }

            BotThread = new Thread(Run);
            BotThread.Start();

        }

        public void Exit()
        {
            ExitFlag = true;
        }

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
                            Search();
                            break;
                        }

                    case State.Kill:
                        {
                            KillMonster();
                            break;
                        }

                    case State.Take:
                        {
                            Take();
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
                    if(FindMonster(ImageAnalyzer.CaptureScreen()))
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
                    if(FindMonster(ImageAnalyzer.CaptureScreen()))
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

        private void MoveMouseTo(int x, int y)
        {
            Input.MoveMouseTo((int)x, (int)y, true);
            Thread.Sleep(10);
        }

        private void KillMonster()
        {
            var popupOpen = true;
            Input.SendKey(Interceptor.Keys.One);

            do
            {
                var (image, x, y) = ImageAnalyzer.CaptureScreen();
                var popupColor = Color.FromArgb(255, 165, 138, 82);
                var color1 = image.GetPixel(960, 929);
                var color2 = image.GetPixel(960, 930);
                //image.Save("D:\\popup.png", ImageFormat.Png);
                if(color1  == color2 && color1 == popupColor)
                {
                    Thread.Sleep(250);
                }
                else
                {
                    popupOpen = false;
                    CurrentState = State.Take;
                }
            }
            while (popupOpen);
        }

        private void Take()
        {
            for(var i = 0; i< 10; i++)
            {
                Input.SendKey(Interceptor.Keys.E);
                Thread.Sleep(50);
            }

            CurrentState = State.Search;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> false = not found, true = found</returns>
        private bool FindMonster((Bitmap image, int x, int y) input)
        {
            var cursor_x = input.x + 7;
            var cursor_y = input.y + 7;
            if(input.image == null)
            {
                CurrentState = State.Exit;
                Exit();
                return true;
            }


            var cursor_color = input.image.GetPixel(cursor_x, cursor_y);
            //var normalCursor = Color.FromArgb(255, 58, 19, 3);
            var normalCursor = Color.FromArgb(255, 136, 85, 12);
            var notAttackCursor = Color.FromArgb(255,218, 23, 3);
            var attackCursor = Color.FromArgb(255,242, 242, 236);
            if(normalCursor != cursor_color && notAttackCursor != cursor_color)
            {
               //input.image.Save("D:\\attack.png", ImageFormat.Png);
                Input.SendLeftRightClick(250);
                CurrentState = State.Kill;
                return true;
            }
            else
            {
               //input.image.Save("D:\\normal.png", ImageFormat.Png);
            }

            return false;
        }


        //private void TestMovement()
        //{
        //    var max = 65536;
        //    var border_min = 256;
        //    var border_max = max - border_min;
        //    var step = max / 50;

        //    for(int i = 0; i < max; i += step)
        //    {
        //        Update(i, border_min); 
        //    }

        //    Input.SendKey(Interceptor.Keys.One);

        //    for(int i = 0; i < max; i += step)
        //    {
        //        Update(border_max, i); 
        //    }

        //    Input.SendKey(Interceptor.Keys.Two);

        //    for(int i = max; i > 0; i -= step)
        //    {
        //        Update(i, border_max); 
        //    }

        //    Input.SendKey(Interceptor.Keys.Three);

        //    for(int i = max; i > 0; i -= step)
        //    {
        //        Update(border_min, i); 
        //    }
        //    Input.SendKey(Interceptor.Keys.Four);
        //    Input.SendKey(Interceptor.Keys.Q);
        //    Input.SendKey(Interceptor.Keys.Eight);
        //}

        //private void TestImage()
        //{
        //    var (image, x, y) = CaptureScreen();
        //    image.Save("D:\\test.png", ImageFormat.Png);
        //    x += 1;
        //    y += 1;
        //    var color = image.GetPixel(x, y); // (255,1,0,1)
        //    var usualCursor = Color.FromArgb(255, 198, 173, 104);
        //    var attackCursor = Color.FromArgb(255,242, 242, 236);
        //    if(color == attackCursor)
        //    {
        //        int i = 0;
        //        Input.SendLeftRightClick(200);
        //    }

        //}



    }
}
