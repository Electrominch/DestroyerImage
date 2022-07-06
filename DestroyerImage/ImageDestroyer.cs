using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;

namespace DestroyerImage
{
    struct Range
    {
        public readonly int min;
        public readonly int max;
        public Range(int min, int max)
        {
            if (min <= max)
            {
                this.min = min;
                this.max = max;
            }
            else
            {
                this.min = max;
                this.max = min;
            }
        }
    }

    class ImageDestroyer
    {
        static Random rnd = new Random();
        public static Range TailRadiusShift_rndRange = new Range(1, 7);
        public static Range AddRadiusToClearTail_rndRange = new Range(15, 15);
        public static Range AddRadiusToRemovePixels_rndRange = new Range(-30, 0);
        public static int RadiusShift = -2;
        public static int ChanceToSecondTail = 40;
        public static bool Tail = false;
        private static double percentEmptyPixelsAtTheEnd = 1;
        public static double PercentEmptyPixelsAtTheEnd
        {
            get { return percentEmptyPixelsAtTheEnd; }
            set
            {
                percentEmptyPixelsAtTheEnd = Math.Min(Math.Max(value, 0), 1);
            }
        }

        Bitmap image;
        Point center;

        public ImageDestroyer(Bitmap img)
        {
            image = (Bitmap)img.Clone();
            center = new Point(image.Width / 2, image.Height / 2);
        }

        public Bitmap[] GetRepairedGIF()
        {
            var buffer = new List<Bitmap>();
            GetRepairedGIF(new Bitmap(image.Width, image.Height), buffer, Math.Min(image.Width / 2, image.Height / 2), image);
            return buffer.ToArray();
        }


        private void GetRepairedGIF(Bitmap sourse, List<Bitmap> output, int radius, Bitmap imageToRepair)
        {
            List<Point> pointsForTail = GetPointsForTail(imageToRepair, radius);
            for (int y = 0; y < imageToRepair.Height; y++)
            {
                for (int x = 0; x < imageToRepair.Width; x++)
                {
                    int relativeToCenterX = x - center.X;
                    int relativeToCenterY = y - center.Y;
                    int currentRadius = (relativeToCenterX * relativeToCenterX) + (relativeToCenterY * relativeToCenterY);

                    int AddRadiusToClear = rnd.Next(AddRadiusToClearTail_rndRange.min, AddRadiusToClearTail_rndRange.max);
                    int rndAddRadius = rnd.Next(AddRadiusToRemovePixels_rndRange.min, AddRadiusToRemovePixels_rndRange.max);

                    if (currentRadius >= (radius + AddRadiusToClear) * (radius + AddRadiusToClear))
                        sourse.SetPixel(x, y, imageToRepair.GetPixel(x, y));
                    else if (currentRadius >= (radius + rndAddRadius) * (radius + rndAddRadius))
                    {
                        if (Tail)
                        {
                            do
                            {
                                var p = pointsForTail[rnd.Next(0, pointsForTail.Count)];
                                sourse.SetPixel(p.X, p.Y, Color.FromArgb(0, 0, 0, 0));
                            } while (rnd.Next(0, 101) < ChanceToSecondTail);
                        }
                        sourse.SetPixel(x, y, imageToRepair.GetPixel(x, y));
                    }
                }
            }
            output.Add((Bitmap)sourse.Clone());
            if (radius <= 0)
                return;
            GetRepairedGIF(sourse, output, radius + RadiusShift, imageToRepair);
        }

        public Bitmap[] GetDestroyedGIF()
        {
            var buffer = new List<Bitmap>();
            GetDestroyedGIF((Bitmap)image.Clone(), buffer, Math.Min(image.Width / 2, image.Height / 2));
            return buffer.ToArray();
        }


        private void GetDestroyedGIF(Bitmap source, List<Bitmap> output, int radius)
        {
            List<Point> pointsForTail = GetPointsForTail(source, radius);

            int countEmptyPixels = 0;
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    int relativeToCenterX = x - center.X;
                    int relativeToCenterY = y - center.Y;
                    int currentRadius = (relativeToCenterX * relativeToCenterX) + (relativeToCenterY * relativeToCenterY);

                    int AddRadiusToClear = rnd.Next(AddRadiusToClearTail_rndRange.min, AddRadiusToClearTail_rndRange.max);
                    int rndAddRadius2 = rnd.Next(AddRadiusToRemovePixels_rndRange.min, AddRadiusToRemovePixels_rndRange.max);

                    if (currentRadius >= (radius + AddRadiusToClear) * (radius + AddRadiusToClear))
                        source.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    else if ((relativeToCenterX * relativeToCenterX) + (relativeToCenterY * relativeToCenterY) >= (radius + rndAddRadius2) * (radius + rndAddRadius2))
                    {
                        Color buffer = source.GetPixel(x, y);
                        if (Tail)
                        {
                            do
                            {
                                var p = pointsForTail[rnd.Next(0, pointsForTail.Count)];
                                source.SetPixel(p.X, p.Y, buffer);
                            } while (rnd.Next(0, 101) < ChanceToSecondTail);
                        }
                        source.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }
                    if (source.GetPixel(x, y).ToArgb() == 0)
                        countEmptyPixels++;
                }
            }
            output.Add((Bitmap)source.Clone());
            if (countEmptyPixels * 1.0 / (source.Width * source.Height) >= percentEmptyPixelsAtTheEnd)
                return;
            if (radius <= 0)
                return;
            GetDestroyedGIF(source, output, radius + RadiusShift);
        }

        private List<Point> GetPointsForTail(Bitmap source, int sourceRadius)
        {
            int sourceRadiusSqr = sourceRadius * sourceRadius;
            List<Point> pointsForTail = new List<Point>();
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    int relativeToCenterX = x - center.X;
                    int relativeToCenterY = y - center.Y;
                    int currentRadius = (relativeToCenterX * relativeToCenterX) + (relativeToCenterY * relativeToCenterY);
                    int rndAddRadius = rnd.Next(TailRadiusShift_rndRange.min, TailRadiusShift_rndRange.max);//определяем, насколько дальше может быть точка
                    if (currentRadius >= sourceRadiusSqr)//чтобы было дальше изначального радиуса
                        if (currentRadius <= (sourceRadius + rndAddRadius) * (sourceRadius + rndAddRadius))//чтобы было не дальше, чем новый радиус
                            pointsForTail.Add(new Point(x, y));
                }
            }
            MixList(pointsForTail);
            return pointsForTail;
        }

        private void MixList(IList l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                int j = rnd.Next(l.Count);
                // обменять значения data[j] и data[i]
                var temp = l[j];
                l[j] = l[i];
                l[i] = temp;
            }
        }
    }
}
