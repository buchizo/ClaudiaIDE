using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClaudiaIDE.Helpers
{
    internal class GaussianBlur
    {
        public BitmapSource Result { get => result; }
        private readonly BitmapSource result;
        private readonly int width;
        private readonly int height;
        private readonly int bytesPerPixel;
        private readonly int stride;
        private readonly byte[] srcBytes;
        private readonly byte[] red;
        private readonly byte[] green;
        private readonly byte[] blue;
        private readonly byte[] alpha;

        public GaussianBlur(BitmapSource image, int radius)
        {
            height = image.PixelHeight;
            width = image.PixelWidth;
            bytesPerPixel = (image.Format.BitsPerPixel + 7) / 8;
            stride = width * bytesPerPixel;

            srcBytes = new byte[width * stride];
            image.CopyPixels(srcBytes, stride, 0);

            red = new byte[width * height];
            green = new byte[width * height];
            blue = new byte[width * height];
            alpha = new byte[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    int i = x + y * width;
                    var offset = GetPixelOffset(x, y);

                    red[i] = srcBytes[offset];
                    green[i] = srcBytes[offset + 1];
                    blue[i] = srcBytes[offset + 2];
                    alpha[i] = srcBytes[offset + 3];
                }
            }

            byte[] srcResult = new byte[width * stride];
            byte[] nRed = new byte[width * height];
            byte[] nGreen = new byte[width * height];
            byte[] nBlue = new byte[width * height];
            byte[] nAlpha = new byte[width * height];

            gaussBlur_4(red, nRed, radius);
            gaussBlur_4(green, nGreen, radius);
            gaussBlur_4(blue, nBlue, radius);
            gaussBlur_4(alpha, nAlpha, radius);

            for (var y = 0; y < height; y++)
            {
                for(var x = 0; x < width; x++)
                {
                    int i = x + y * width;
                    nRed[i] = Math.Max((byte)0, Math.Min(nRed[i], (byte)0xff));
                    nGreen[i] = Math.Max((byte)0, Math.Min(nGreen[i], (byte)0xff));
                    nBlue[i] = Math.Max((byte)0, Math.Min(nBlue[i], (byte)0xff));
                    nAlpha[i] = Math.Max((byte)0, Math.Min(nAlpha[i], (byte)0xff));

                    int offset = GetPixelOffset(x, y);
                    srcResult[offset] = nRed[i];
                    srcResult[offset + 1] = nGreen[i];
                    srcResult[offset + 2] = nBlue[i];
                    srcResult[offset + 3] = nAlpha[i];
                }
            }

            BitmapSource _res = BitmapSource.Create(width,
                                                    height,
                                                    image.DpiX,
                                                    image.DpiY,
                                                    PixelFormats.Pbgra32,
                                                    image.Palette,
                                                    srcResult,
                                                    stride);
            _res.Freeze();
            result = _res;
        }

        private int GetPixelOffset(int x, int y)
        {
            return bytesPerPixel * (x + y * width);
        }

        private void gaussBlur_4(byte[] source, byte[] dest, int r)
        {
            var bxs = boxesForGauss(r, 3);
            boxBlur_4(source, dest, width, height, (bxs[0] - 1) / 2);
            boxBlur_4(dest, source, width, height, (bxs[1] - 1) / 2);
            boxBlur_4(source, dest, width, height, (bxs[2] - 1) / 2);
        }

        private int[] boxesForGauss(int sigma, int n)
        {
            var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
            var wl = (int)Math.Floor(wIdeal);
            if (wl % 2 == 0) wl--;
            var wu = wl + 2;

            var mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            var m = Math.Round(mIdeal);

            var sizes = new List<int>();
            for (var i = 0; i < n; i++) sizes.Add(i < m ? wl : wu);
            return sizes.ToArray();
        }

        private void boxBlur_4(byte[] source, byte[] dest, int w, int h, int r)
        {
            for (var i = 0; i < source.Length; i++) dest[i] = source[i];
            boxBlurH_4(dest, source, w, h, r);
            boxBlurT_4(source, dest, w, h, r);
        }

        private void boxBlurH_4(byte[] source, byte[] dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            for(var i = 0; i < h; i++)
            {
                var ti = i * w;
                var li = ti;
                var ri = ti + r;
                var fv = source[ti];
                var lv = source[ti + w - 1];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri++] - fv;
                    dest[ti++] = (byte)Math.Round(val * iar);
                }
                for (var j = r + 1; j < w - r; j++)
                {
                    val += source[ri++] - source[li++];
                    dest[ti++] = (byte)Math.Round(val * iar);
                }
                for (var j = w - r; j < w; j++)
                {
                    val += lv - source[li++];
                    dest[ti++] = (byte)Math.Round(val * iar);
                }
            }
        }

        private void boxBlurT_4(byte[] source, byte[] dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            for (var i = 0; i < w; i++)
            {
                var ti = i;
                var li = ti;
                var ri = ti + r * w;
                var fv = source[ti];
                var lv = source[ti + w * (h - 1)];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j * w];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri] - fv;
                    dest[ti] = (byte)Math.Round(val * iar);
                    ri += w;
                    ti += w;
                }
                for (var j = r + 1; j < h - r; j++)
                {
                    val += source[ri] - source[li];
                    dest[ti] = (byte)Math.Round(val * iar);
                    li += w;
                    ri += w;
                    ti += w;
                }
                for (var j = h - r; j < h; j++)
                {
                    val += lv - source[li];
                    dest[ti] = (byte)Math.Round(val * iar);
                    li += w;
                    ti += w;
                }
            }
        }
    }
}
