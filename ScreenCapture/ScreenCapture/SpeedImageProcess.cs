using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ScreenCapture
{
    class SpeedImageProcess
    {
        private Bitmap destImage;
        private Rectangle destRect;


        public SpeedImageProcess()
        {
            destImage = new Bitmap(120, 76);
            destRect = new Rectangle(0, 0, 120, 76);
        }

        // 照片去背轉黑白
        public Bitmap ToBlackWhite(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            try
            {
                byte newColor = 0;
                BitmapData srcData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                unsafe
                {
                    byte* p = (byte*)srcData.Scan0.ToPointer();
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            newColor = (byte)((float)p[0] * 0.114f + (float)p[1] * 0.587f + (float)p[2] * 0.299f);
                            if (newColor > 200)
                            {
                                p[0] = 255;
                                p[1] = 255;
                                p[2] = 255;
                            }
                            else
                            {
                                p[0] = 0;
                                p[1] = 0;
                                p[2] = 0;
                            }

                            p += 3;
                        }
                        p += srcData.Stride - w * 3;
                    }
                    bmp.UnlockBits(srcData);
                    return bmp;
                }
            }
            catch
            {
                return null;
            }
        }

        // 將圖片轉換成負片效果
        public Bitmap NegativePicture(Bitmap image)
        {
            int w = image.Width;
            int h = image.Height;
            BitmapData srcData = image.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            image.UnlockBits(srcData);
            int cChannels = 3;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int current = y * srcData.Stride + x * 4;
                    for (int c = 0; c < cChannels; c++)
                    {
                        result[current + c] = (byte)(255 - buffer[current + c]);
                    }
                    result[current + 3] = 255;
                }
            }

            Bitmap resImg = new Bitmap(w, h);
            BitmapData resData = resImg.LockBits(new Rectangle(0, 0, w, h),
            ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, resData.Scan0, bytes);
            resImg.UnlockBits(resData);
            return resImg;
        }

        // 改變圖片size
        public Bitmap ResizeImage(Image image, int width, int height)
        {
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
