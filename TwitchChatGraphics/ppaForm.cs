// Copyright (c) 2016, Henrik Barestrand, All rights reserved.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using TwitchBot;

namespace ChatGraphics
{
    class ppaForm : Form
    {
        private Thread drawer;

        public ppaForm()
        {
            TopMost = true;
            ShowInTaskbar = true;
            Location = new Point(-450, 60); // app start position
            FormBorderStyle = FormBorderStyle.None; // This form should not have a border or else Windows will clip it.
            Text = "TwitchChat";

            SetBitmap(new Bitmap(Program.getPath("res/background.png")));

            drawer = new Thread(new ThreadStart(TwitchdataThread));
            drawer.SetApartmentState(ApartmentState.STA);
            drawer.Start();

            #region BackgroundworkerExample
            /*
                BackgroundWorker bw = new BackgroundWorker();

                // this allows our worker to report progress during work
                bw.WorkerReportsProgress = true;

                // what to do in the background thread
                bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    BackgroundWorker b = o as BackgroundWorker;

                    // do some simple processing for 10 seconds
                    for (int i = 1; i <= 10; i++)
                    {
                        // report the progress in percent
                        b.ReportProgress(i * 10);
                        Thread.Sleep(1000);
                    }

                });

                // what to do when progress changed (update the progress bar for example)
                bw.ProgressChanged += new ProgressChangedEventHandler(
                delegate (object o, ProgressChangedEventArgs args)
                {
                    //Location = new Point(args.ProgressPercentage * 10, 100);
                    SetBitmap(bitmap1, 255, 0.5f+args.ProgressPercentage*0.05f, new Point(0,0)); //label1.Text = string.Format("{0}% Completed", args.ProgressPercentage);
                });

                // what to do when worker completes its task (notify the user)
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate (object o, RunWorkerCompletedEventArgs args)
                {
                    Location = new Point(100, 100);
                    SetBitmap(bitmap1, 255, 1, new Point(0, 0)); //label1.Text = "Finished!";
                });

                bw.RunWorkerAsync();
                */
            #endregion
        }

        private void TwitchdataThread()
        {
            TwitchData data = new TwitchData();

            while (true)
            {
                Bitmap canvas = data.Run();

                // mainthread code
                BeginInvoke((Action)delegate
                {
                    SetBitmap(canvas);
                    canvas.Dispose();
                });
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        // Let Windows drag this form for us
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0084) // WM_NCHITTEST
            {
                m.Result = (IntPtr)2;   // HTCLIENT
                return;
            }

            base.WndProc(ref m);
        }
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // This form has to have the WS_EX_LAYERED extended style
                return cp;
            }
        }

        /// <para>Changes the current bitmap with a custom opacity level.  Here is where all happens!</para>
        public void SetBitmap(Bitmap bitmap, int px = 0, byte opacity = 255)
        {
            float scale;
            if (px != 0)
                scale = (float)px / bitmap.Width;
            else
                scale = 1;

            Bitmap map = new Bitmap(bitmap, new Size((int)(bitmap.Width * scale), (int)(bitmap.Height * scale))); // scale bitmap
            bitmap.Dispose();

            if (map.PixelFormat != PixelFormat.Format32bppArgb)
                throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

            // The ideia of this is very simple,
            // 1. Create a compatible DC with screen;
            // 2. Select the bitmap with 32bpp with alpha-channel in the compatible DC;
            // 3. Call the UpdateLayeredWindow.

            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;
            IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
            IntPtr memDc = Win32.CreateCompatibleDC(screenDc);

            try
            {
                hBitmap = map.GetHbitmap(Color.FromArgb(0));  // grab a GDI handle from this GDI+ bitmap
                oldBitmap = Win32.SelectObject(memDc, hBitmap);

                Win32.Size size = new Win32.Size(map.Width, map.Height);
                Win32.Point pointSource = new Win32.Point(0, 0);
                Win32.Point topPos = new Win32.Point(Left, Top); // image position in form
                Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION();
                blend.BlendOp = Win32.AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = opacity;
                blend.AlphaFormat = Win32.AC_SRC_ALPHA;

                Win32.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);

            }
            finally
            {
                Win32.ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDc, oldBitmap);
                    Win32.DeleteObject(hBitmap);
                }
                Win32.DeleteDC(memDc);
            }
            map.Dispose();
        }

        public static void TextArea(ref Bitmap textBack, Bitmap destBitmap, string text, int textAreaHeight, Point pixeledPos)
        {
            float emSize = textAreaHeight * .62f;

            Font stringFont = new Font("Calibri", emSize, FontStyle.Bold);

            Graphics g = Graphics.FromImage(destBitmap);
            SizeF stringSize = g.MeasureString(text, stringFont);

            Size textureSize = new Size( // adapt texture size
                (int)(stringSize.Width * 0.99f),
                (int)(stringSize.Height * 0.9f)
                );
            PointF textPos = new PointF( // adjusted text in texture
                pixeledPos.X + textureSize.Width / 2 - stringSize.Width * 0.482f,
                pixeledPos.Y + textureSize.Height / 2 - stringSize.Height * 0.46f
                );

            // superpose textBackground
            g.CompositingMode = CompositingMode.SourceOver;
            g.DrawImage(textBack, new Rectangle(pixeledPos, textureSize), entireRegion(textBack), GraphicsUnit.Pixel);

            // superpose text
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit; //makes text good looking
            g.DrawString(text, stringFont, Brushes.White, textPos);
            g.Flush();
            g.Dispose();
        }

        public static Rectangle entireRegion(Bitmap bitmap)
        {
            return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        }
        
        public static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Point pixeledPos, Size pixeledSize, bool alignRight)
        {
            if (alignRight)
            {
                pixeledPos.X -= pixeledSize.Width;
            }

            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.CompositingMode = CompositingMode.SourceOver;
                grD.DrawImage(srcBitmap, new Rectangle(pixeledPos, pixeledSize), srcRegion, GraphicsUnit.Pixel);
                //grD.Dispose();
            }
        }
        
    }
}
