#define IMGUI_DEBUG_WINDOW
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;

namespace R2Bot
{
    internal class ImageDescription
    {
        public ImageProcessing ImageTask { get; set; } = ImageProcessing.None;
        public CursorType Cursor { get; set; } = CursorType.None;
        public bool IsImageProcessed { get; set; } = false;
        public bool IsAttackWindowOpen { get; set; }
        public string AttackObjectName { get; set; }
        public float Health { get; set; }
        public float Mana { get; set; }
    }

    [Flags]
    internal enum ImageProcessing
    {
        None = 1 << 0,
        Cursor = 1 << 1,
        Health = 1 << 2,
        Mana = 1 << 3,
        AttackWindow = 1 << 4,
        AttackName = 1 << 5
    }

    internal enum CursorType
    {
        None,
        Normal,
        Attack,
        Take,
        NoAttack
    }


    internal class ImageAnalyzer
    {
        public const string DefaultPath = ".\\";
        private const string FilenameFormat = "{0}_x={1}_y={2}.png";


        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;

        private Tesseract Tesseract { get; }
        private const string TesseractDefaultFolder = "./tessdata";
        private Rectangle AttackWindowRectangle { get; } = new Rectangle(856, 929, 208, 43);
        private Rectangle AttackWindowTopEdgeRectangle { get; } = new Rectangle(0, 0, 208, 2);
        private Rectangle AttackWindowBottomEdgeRectangle { get; } = new Rectangle(0, 41, 208, 43);
        private Rectangle HealthRectangle { get; set; } = new Rectangle(598, 1032, 276, 2);
        private Rectangle ManaRectangle { get; set; } = new Rectangle(595, 1058, 276, 2);
        private Gray Threshold128Gray { get; } = new Gray(128);
        private Gray Threshold255Gray { get; } = new Gray(255);
        private const double AttackWindowColor = 140;
        private const double Epsilon = 1;
        private Rectangle AttackObjectNameRectangle { get; } = new(858, 940, 206, 20);

        public ImageAnalyzer()
        {
            if (!Directory.Exists(TesseractDefaultFolder))
            {
                Directory.CreateDirectory(TesseractDefaultFolder);
            }

            if (!File.Exists(TesseractDefaultFolder + Path.DirectorySeparatorChar + "rus.traineddata"))
            {
                TesseractDownloadLangFile("./tessdata", "rus");
            }

            Tesseract = new Tesseract("./tessdata/", "rus", OcrEngineMode.Default);
        }

        public ImageDescription ProcessImage(Bitmap bitmap, Point pointer, ImageProcessing task)
        {
            var result = new ImageDescription
            {
                ImageTask = task
            };

            try
            {
                if (task.HasFlag(ImageProcessing.None))
                {
                    return result;
                }

                ConvertToImage(bitmap, out var bgra, out var gray);
                result.IsImageProcessed = true;

                if (task.HasFlag(ImageProcessing.Cursor))
                {
                   result.Cursor = GetCursorType(gray, pointer);
                }

                if (task.HasFlag(ImageProcessing.Health))
                {
                    result.Health = GetHealthInfo(bgra);
                    Debug("Health is {0}", result.Health);

                }

                if (task.HasFlag(ImageProcessing.Mana))
                {
                    result.Mana = GetManaInfo(bgra);
                    Debug("Mana is {0}", result.Mana);
                }

                if (task.HasFlag(ImageProcessing.AttackWindow) || task.HasFlag(ImageProcessing.AttackName))
                {
                    var isAttackWindowOpen = IsAttackWindowOpen(gray);
                    result.IsAttackWindowOpen = isAttackWindowOpen;
                    if (isAttackWindowOpen)
                    {
                        var name = TryGetAttackObjectName(gray);
                        result.AttackObjectName = name;
                    }
                    else
                    {
                        result.AttackObjectName = string.Empty;
                    }
                }
            }
            catch(Exception exception)
            {
                Debug( exception.Message);
            }

            return result;
        }

        private Rectangle CursorRect { get; } = new Rectangle(6, 6, 5, 5);
        private CursorType GetCursorType(Image<Gray, byte> image, Point pointer)
        {
            var rect = new Rectangle(pointer.X + CursorRect.X, pointer.Y + CursorRect.Y, CursorRect.Width,
                CursorRect.Height);
            var cursorArea = image.GetSubRect(rect);

#if IMGUI_DEBUG_WINDOW
            CvInvoke.NamedWindow("cursor area");
            CvInvoke.Imshow("cursor area", cursorArea);
#endif





            return CursorType.None;
        }

        private bool IsAttackWindowOpen(Image<Gray, byte> image)
        {
            var attack = image.GetSubRect(AttackWindowRectangle);
#if IMGUI_DEBUG_WINDOW
            CvInvoke.NamedWindow("attack window");
            CvInvoke.Imshow("attack window", attack);
#endif
            var topEdge = attack.GetSubRect(AttackWindowTopEdgeRectangle);
#if IMGUI_DEBUG_WINDOW
            CvInvoke.NamedWindow("attack window top edge");
            CvInvoke.Imshow("attack window top edge", topEdge);
#endif
            var topAverage = topEdge.GetAverage();
            Debug($"attack window top average = {topAverage.Intensity}");
            if (IsEqual(topAverage.Intensity, AttackWindowColor, Epsilon))
            {
                var bottomEdge = attack.GetSubRect(AttackWindowTopEdgeRectangle);
#if IMGUI_DEBUG_WINDOW
                CvInvoke.NamedWindow("attack window bottom edge");
                CvInvoke.Imshow("attack window bottom edge", bottomEdge);
    #endif
                var bottomAverage = bottomEdge.GetAverage();
                Debug($"attack window bottom average = {bottomAverage.Intensity}");
                if (IsEqual(bottomAverage.Intensity, AttackWindowColor, Epsilon))
                {
                    Debug("This is Attack window");
                    return true;
                }
            }
            Debug($"Not attack window");
            return false;
        }

        private string TryGetAttackObjectName(Image<Gray, byte> image)
        {
#if DEBUG_STOPWATCH
            var stopwatch = Stopwatch.StartNew();
#endif
            var attackObjectImage = image.GetSubRect(AttackObjectNameRectangle);
            var binaryAttackObjectImage = attackObjectImage.ThresholdBinary(Threshold128Gray, Threshold255Gray);

#if IMGUI_DEBUG_WINDOW
            CvInvoke.NamedWindow("binary Attack Object Image");
            CvInvoke.Imshow("binary Attack Object Image", binaryAttackObjectImage);
#endif

            Tesseract.SetImage(binaryAttackObjectImage);
            if (Tesseract.Recognize() != -1)
            {
                var result = Tesseract.GetUTF8Text();
                result = result.Replace(".", string.Empty);
                result = result.Replace("/", string.Empty);
                result = result.Replace("\\", string.Empty);
                result = result.Replace("|", string.Empty);

                Debug($"Tesseract:: Result is {result}");
#if DEBUG_STOPWATCH
                stopwatch.Stop();
                Debug($"Stopwatch {stopwatch.ElapsedMilliseconds}ms or {stopwatch.ElapsedTicks}ticks");
#endif
                return result;
            }
            else
            {
                Debug("Tesseract:: Cannot recognise text");
            }

#if DEBUG_STOPWATCH
            stopwatch.Stop();
            Debug($"Stopwatch {stopwatch.ElapsedMilliseconds}ms or {stopwatch.ElapsedTicks}ticks");
#endif
            return string.Empty;
        }

        private float GetManaInfo(Image<Bgra, byte> image)
        {
            return GetAvgInfo(image, ManaRectangle, 0);
        }

        private float GetHealthInfo(Image<Bgra, byte> image)
        {
            return GetAvgInfo(image, HealthRectangle, 2);
        }

        private float GetAvgInfo(Image<Bgra, byte> image, Rectangle rect, int channel)
        {
#if DEBUG_STOPWATCH
            var stopwatch = Stopwatch.StartNew();
#endif

            var searchArea = image.GetSubRect(rect);
#if IMGUI_DEBUG_WINDOW
            CvInvoke.NamedWindow("search area");
            CvInvoke.Imshow("search area", searchArea);
#endif
            var channels = searchArea.Split();
            var channelAverage = channels[channel].ThresholdBinary(Threshold128Gray, Threshold255Gray).GetAverage();

            var result = (float)channelAverage.Intensity / 255;

#if DEBUG_STOPWATCH
            stopwatch.Stop();
            Debug($"Stopwatch {stopwatch.ElapsedMilliseconds}ms or {stopwatch.ElapsedTicks}ticks");
#endif
            return result;
        }

        private static void ConvertToImage(Bitmap bitmap, out Image<Bgra, byte> bgra, out Image<Gray, byte> gray)
        {
            bgra = bitmap.ToImage<Bgra, byte>();
#if IMGUI_DEBUG_WINDOW
            CvInvoke.NamedWindow("bgra");
            CvInvoke.Imshow("bgra", bgra);
#endif
            gray = new Image<Gray, byte>(bgra.Size);
            CvInvoke.CvtColor(bgra, gray, ColorConversion.Bgra2Gray);
#if IMGUI_DEBUG_WINDOW
            CvInvoke.NamedWindow("gray");
            CvInvoke.Imshow("gray", gray);
#endif
        }

        public static void SaveImage(Bitmap bitmap, Point point, string name)
        {
            bitmap.Save(DefaultPath + string.Format(FilenameFormat, name, point.X, point.Y), ImageFormat.Png);
        }

        public static (Bitmap, Point) LoadImage(string name)
        {
            var image = Image.FromFile(name);
            var bitmap = new Bitmap(image);
            return (bitmap, new Point());
        }

        protected void TesseractDownloadLangFile(string folder, string lang)
        {
            var folderName = folder;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            var dest = Path.Combine(folderName, string.Format("{0}.traineddata", lang));
            if (!File.Exists(dest))
            {
                using (var webclient = new System.Net.WebClient())
                {
                    var source = Emgu.CV.OCR.Tesseract.GetLangFileUrl(lang);

                    Debug(string.Format("Downloading file from '{0}' to '{1}'", source, dest));
                    webclient.DownloadFile(source, dest);
                    Debug("Download completed");
                }
            }
        }
        public static (Bitmap, Point) CaptureScreen()
        {
            Bitmap result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);
            Point pointer = new Point();

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

                    CURSORINFO pci;
                    pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

                    if (GetCursorInfo(out pci))
                    {
                        if (pci.flags == CURSOR_SHOWING)
                        {
                            DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                            g.ReleaseHdc();
                            pointer.X = pci.ptScreenPos.x;
                            pointer.Y = pci.ptScreenPos.y;
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }

            return (result, pointer);
        }

        public static void SaveCursor(Bitmap bitmap, Point point, string filename)
        {
            ConvertToImage(bitmap, out var bgra, out var gray);
            bgra.Save(string.Format(FilenameFormat, filename));
            gray.Save(string.Format(FilenameFormat, filename));
        }

        public bool IsEqual(double one, double two, double epsilon)
        {
            return Math.Abs(one - two) < epsilon;
        }
        public bool IsEqual(float one, float two, float epsilon)
        {
            return MathF.Abs(one - two) < epsilon;
        }

        [Conditional("DEBUG")]
        public void Debug(string message, params object[] args)
        {
            Console.WriteLine(string.Format(message, args));
        }
    }
}
