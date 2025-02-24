//#define IMGUI_DEBUG_WINDOW
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
using Emgu.CV.Quality;
using Emgu.CV.Structure;
using Emgu.CV.Util;

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

    public class ImageAnalyzerConfig
    {
        public Rectangle HealthRectangle = new Rectangle(598, 1032, 276, 2);
        public Rectangle ManaRectangle = new Rectangle(595, 1058, 276, 2);

        public Rectangle AttackWindowRectangle = new Rectangle(856, 929, 208, 43);
        public Rectangle AttackWindowTopEdgeRectangle = new Rectangle(0, 0, 208, 2);
        public Rectangle AttackWindowBottomEdgeRectangle = new Rectangle(0, 41, 208, 2);
        public Rectangle AttackObjectNameRectangle = new(858, 940, 206, 20);
        public double AttackWindowColor = 140;

        public Rectangle CursorRect = new Rectangle(7, 7, 2, 2);
        public Image<Bgra, byte> NormalImageBgra;
        public Image<Bgra, byte> AttackImageBgra;
        public Image<Bgra, byte> NoAttackImageBgra;
        public Image<Bgra, byte> TakeImageBgra;

        public ImageAnalyzerConfig()
        {
            NormalImageBgra ??= new(CursorRect.Size);
        }
    }

    internal class ImageAnalyzer
    {
        public const string DefaultPath = ".\\";
        private const string FilenameFormat = "{0}_x={1}_y={2}.png";
        private const string CursorFilenameFormat = "{0}_{1}_x={2}_y={3}.png";


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

        private double Debug_AttackWindowTopColor = 0;
        private double Debug_AttackWindowBottomColor = 0;
        private Tesseract Tesseract { get; }
        private const string TesseractDefaultFolder = "./tessdata";
        private Rectangle AttackWindowRectangle { get; } = new Rectangle(856, 929, 208, 43);
        private Rectangle AttackWindowTopEdgeRectangle { get; } = new Rectangle(0, 0, 208, 2);
        private Rectangle AttackWindowBottomEdgeRectangle { get; } = new Rectangle(0, 41, 208, 2);
        private Rectangle HealthRectangle { get; set; } = new Rectangle(598, 1032, 276, 2);
        private Rectangle ManaRectangle { get; set; } = new Rectangle(595, 1058, 276, 2);
        private Gray Threshold128Gray { get; } = new Gray(128);
        private Gray Threshold255Gray { get; } = new Gray(255);
        private double AttackWindowColor = 140;
        private const double Epsilon = 2;
        private Rectangle AttackObjectNameRectangle { get; } = new(858, 940, 206, 20);
        private static Rectangle CursorRect { get; set;  } = new Rectangle(7, 7, 2, 2);

        private Image<Bgra, byte> NormalImageBgra { get; } = new Image<Bgra, byte>(CursorRect.Size);
        private Image<Bgra, byte> AttackImageBgra { get; } = new Image<Bgra, byte>(CursorRect.Size);
        private Image<Bgra, byte> NoAttackImageBgra { get; } = new Image<Bgra, byte>(CursorRect.Size);
        private Image<Bgra, byte> TakeImageBgra { get; } = new Image<Bgra, byte>(CursorRect.Size);


        public ImageAnalyzer(ImageAnalyzerConfig config)
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


            if (config != null)
            {
                HealthRectangle = config.HealthRectangle;
                ManaRectangle = config.ManaRectangle;

                AttackWindowRectangle = config.AttackWindowRectangle;
                AttackWindowTopEdgeRectangle = config.AttackWindowTopEdgeRectangle;
                AttackWindowBottomEdgeRectangle = config.AttackWindowBottomEdgeRectangle;
                AttackObjectNameRectangle = config.AttackObjectNameRectangle;
                AttackWindowColor = config.AttackWindowColor;

                CursorRect = config.CursorRect;
                NormalImageBgra = config.NormalImageBgra ?? new Image<Bgra, byte>(CursorRect.Size);
                AttackImageBgra = config.AttackImageBgra?? new Image<Bgra, byte>(CursorRect.Size);
                NoAttackImageBgra = config.NoAttackImageBgra?? new Image<Bgra, byte>(CursorRect.Size);
                TakeImageBgra = config.TakeImageBgra?? new Image<Bgra, byte>(CursorRect.Size);
            }
        }

        ~ImageAnalyzer()
        {
            Tesseract?.Dispose();
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

                using (var bgra = bitmap.ToImage<Bgra, byte>())
                {
    #if IMGUI_DEBUG_WINDOW
                    CvInvoke.NamedWindow("bgra");
                    CvInvoke.Imshow("bgra", bgra);
    #endif

                    if (task.HasFlag(ImageProcessing.Cursor))
                    {
                       result.Cursor = GetCursorType(bgra, pointer);
                       Debug("Cursor is {0}", result.Cursor);
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
                        var isAttackWindowOpen = IsAttackWindowOpen(bgra);
                        result.IsAttackWindowOpen = isAttackWindowOpen;
                        if (isAttackWindowOpen)
                        {
                            var name = TryGetAttackObjectName(bgra);
                            result.AttackObjectName = name;
                        }
                        else
                        {
                            result.AttackObjectName = string.Empty;
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Debug( exception.Message + exception.StackTrace);
                result.IsImageProcessed = false;
            }
            finally{
                result.IsImageProcessed = true;
            }

            return result;
        }


        private CursorType GetCursorType(Image<Bgra, byte> image, Point pointer)
        {
            var rect = new Rectangle(pointer.X + CursorRect.X, pointer.Y + CursorRect.Y, CursorRect.Width,
                CursorRect.Height);
            using (var cursorArea = image.GetSubRect(rect))
            {
    #if IMGUI_DEBUG_WINDOW
                CvInvoke.NamedWindow("cursor area");
                CvInvoke.Imshow("cursor area", cursorArea);
    #endif
                if(DiffImages(cursorArea, NormalImageBgra, Epsilon))
                {
                    return CursorType.Normal;
                }

                if (DiffImages(cursorArea, AttackImageBgra, Epsilon))
                {
                    return CursorType.Attack;
                }

                if (DiffImages(cursorArea, NoAttackImageBgra, Epsilon))
                {
                    return CursorType.NoAttack;
                }

                if (DiffImages(cursorArea, TakeImageBgra, Epsilon))
                {
                    return CursorType.Take;
                }

                return CursorType.None;
            }
        }

        private bool DiffImages(Image<Bgra, byte> image1, Image<Bgra, byte> image2, double epsilon)
        {
            using (var diff = image1.AbsDiff(image2))
            {
                var averageColor = diff.GetAverage();
                var average = (averageColor.Red + averageColor.Green + averageColor.Blue) / 3.0;
                return average < epsilon;
            }
        }

        private bool IsAttackWindowOpen(Image<Bgra, byte> image)
        {
            using(var subRect = image.GetSubRect(AttackWindowRectangle))
            using (var attack = new Image<Gray, byte>(subRect.Size))
            {
                CvInvoke.CvtColor(subRect, attack, ColorConversion.Bgra2Gray);
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
                Debug_AttackWindowTopColor = topAverage.Intensity;
                Debug($"attack window top average = {topAverage}");
                if (IsEqual(topAverage.Intensity, AttackWindowColor, Epsilon))
                {
                    var bottomEdge = attack.GetSubRect(AttackWindowBottomEdgeRectangle);
    #if IMGUI_DEBUG_WINDOW
                    CvInvoke.NamedWindow("attack window bottom edge");
                    CvInvoke.Imshow("attack window bottom edge", bottomEdge);
    #endif
                    var bottomAverage = bottomEdge.GetAverage();
                    Debug_AttackWindowBottomColor = bottomAverage.Intensity;
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
        }

        private string TryGetAttackObjectName(Image<Bgra, byte> bgra)
        {
            using (var gray = new Image<Gray, byte>(bgra.Size))
            {

                CvInvoke.CvtColor(bgra, gray, ColorConversion.Bgra2Gray);
#if IMGUI_DEBUG_WINDOW
                CvInvoke.NamedWindow("gray");
                CvInvoke.Imshow("gray", gray);
#endif
#if DEBUG_STOPWATCH
                var stopwatch = Stopwatch.StartNew();
#endif
                using(var attackObjectImage = gray.GetSubRect(AttackObjectNameRectangle))
                using (var binaryAttackObjectImage =
                       attackObjectImage.ThresholdBinary(Threshold128Gray, Threshold255Gray))
                {


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
            }
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

            using (var searchArea = image.GetSubRect(rect))
            {
    #if IMGUI_DEBUG_WINDOW
                CvInvoke.NamedWindow("search area");
                CvInvoke.Imshow("search area", searchArea);
    #endif
                var channels = searchArea.Split();
                using (var channelThresholdBinary =
                       channels[channel].ThresholdBinary(Threshold128Gray, Threshold255Gray))
                {
                      var channelAverage = channelThresholdBinary.GetAverage();
                    var result = (float)channelAverage.Intensity / 255;

        #if DEBUG_STOPWATCH
                    stopwatch.Stop();
                    Debug($"Stopwatch {stopwatch.ElapsedMilliseconds}ms or {stopwatch.ElapsedTicks}ticks");
        #endif
                    return result;
                }
            }
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

        public Image<Bgra, byte> GetCursorData(Bitmap bitmap, Point pointer)
        {
            using (var bgra = bitmap.ToImage<Bgra, byte>())
            {
                
            var rect = new Rectangle(pointer.X + CursorRect.X, pointer.Y + CursorRect.Y, CursorRect.Width,
                CursorRect.Height);
                using (var cursorArea = bgra.GetSubRect(rect))
                {
                    return cursorArea.Clone();
                }
            }
        }

        public string DrawDebug(Bitmap bitmap, Point pointer, ImageProcessing task)
        {
            using (var bgra = bitmap.ToImage<Bgra, byte>())
            {
                var thinkness = 1;
              
                CvInvoke.Rectangle(bgra, HealthRectangle, new MCvScalar(255, 0, 255, 128), thinkness);
                CvInvoke.Rectangle(bgra, ManaRectangle, new MCvScalar(255, 255, 0, 128), thinkness);
                CvInvoke.Rectangle(bgra, AttackWindowRectangle, new MCvScalar(0, 255, 255, 128), thinkness);

                var topBorder = new Rectangle(
                    AttackWindowRectangle.X + AttackWindowTopEdgeRectangle.X,
                    AttackWindowRectangle.Y + AttackWindowTopEdgeRectangle.Y,
                    AttackWindowTopEdgeRectangle.Width, AttackWindowTopEdgeRectangle.Height);

                CvInvoke.Rectangle(bgra, topBorder, new MCvScalar(0, 0, 255, 128), thinkness);

                var bottomBorder = new Rectangle(
                    AttackWindowRectangle.X + AttackWindowBottomEdgeRectangle.X,
                    AttackWindowRectangle.Y + AttackWindowBottomEdgeRectangle.Y,
                    AttackWindowBottomEdgeRectangle.Width, AttackWindowBottomEdgeRectangle.Height);

                CvInvoke.Rectangle(bgra, bottomBorder, new MCvScalar(0, 0, 255, 128), thinkness);

                var cursorRect = new Rectangle(pointer.X + CursorRect.X, pointer.Y + CursorRect.Y,
                    CursorRect.Width, CursorRect.Height);

                CvInvoke.Rectangle(bgra, cursorRect, new MCvScalar(0, 255, 0), thinkness);

                var result = ProcessImage(bitmap, pointer, ImageProcessing.Cursor | ImageProcessing.AttackWindow | ImageProcessing.Health | ImageProcessing.Mana);
                if(result.IsImageProcessed)
                {
                    CvInvoke.PutText(bgra, result.Cursor.ToString(), new Point(100, 100),
                        FontFace.HersheySimplex, 2, new MCvScalar(0, 255, 0), 8);
                }

                CvInvoke.NamedWindow("bgra");
                CvInvoke.Imshow("bgra", bgra);
                bgra.Save("test_client.png");
                return $"Cursor: {result.Cursor.ToString()}; IsAttackWindowOpen: {result.IsAttackWindowOpen}; AttackWindowTopColor: {Debug_AttackWindowTopColor}; AttackWindowBottomColor: {Debug_AttackWindowBottomColor};\nHealth: {result.Health}; Mana: {result.Mana};";
            }
        }

        public bool IsEqual(double one, double two, double epsilon)
        {
            return Math.Abs(one - two) < epsilon;
        }
        public bool IsEqual(float one, float two, float epsilon)
        {
            return MathF.Abs(one - two) < epsilon;
        }

        public void Debug(string message, params object[] args)
        {
            Console.WriteLine(string.Format(message, args));
        }
    }
}
