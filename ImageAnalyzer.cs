using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.OCR;

namespace R2Bot
{
    internal class ImageAnalyzer
    {
        public const string DefaultPath = "D:\\r2\\";
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

        public void ProcessImage(Bitmap bitmap, Point pointer)
        {

        }

        public void SaveImage((Bitmap bitmap, int x, int y) info, string name)
        {
            SaveImage(@info, name);
        }

        public void SaveImage(Bitmap bitmap, Point point, string name)
        {
            bitmap.Save(DefaultPath + string.Format(FilenameFormat, name, point.X, point.Y), ImageFormat.Png);
        }

        public (Bitmap, Point) LoadImage(string name)
        {
            var image = Image.FromFile(name);
            var bitmap = new Bitmap(image);
            return (bitmap, new Point());
        }

        protected void TesseractDownloadLangFile(string folder, string lang)
        {
            //String subfolderName = "tessdata";
            //String folderName = System.IO.Path.Combine(folder, subfolderName);
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

                    Console.WriteLine(string.Format("Downloading file from '{0}' to '{1}'", source, dest));
                    webclient.DownloadFile(source, dest);
                    Console.WriteLine("Download completed");
                }
            }
        }
        protected static (Bitmap, Point) CaptureScreen()
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
    }
}
