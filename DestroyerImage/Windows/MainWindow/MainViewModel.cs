using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Win32;
using DestroyerImage.Windows.Load;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace DestroyerImage
{
    class MainViewModel : INotifyPropertyChanged
    {
        private ProgressBar proggress;
        private ImageDestroyer imageTool;
        private Bitmap[] destroyGIF = new Bitmap[0];
        private Bitmap[] repairGIF = new Bitmap[0];

        private static System.Windows.Controls.Image destroyDisplay;
        private static System.Windows.Controls.Image repairDisplay;

        private static volatile int delayBetweenDestroyFrames = 50;
        public string DelayBetweenDestroyFrames_string
        {
            get { return delayBetweenDestroyFrames.ToString(); }
            set
            {
                try
                {
                    delayBetweenDestroyFrames = int.Parse(value);
                    OnPropertyChanged("DelayBetweenDestroyFrames_string");
                }
                catch { }
            }
        }

        private static volatile int delayBetweenRepairFrames = 50;
        public string DelayBetweenRepairFrames_string
        {
            get { return delayBetweenRepairFrames.ToString(); }
            set
            {
                try
                {
                    delayBetweenRepairFrames = int.Parse(value);
                    OnPropertyChanged("DelayBetweenRepairFrames_string");
                }
                catch { }
            }
        }

        private bool enableWindow = true;
        public bool EnableWindow { get { return enableWindow; } set { enableWindow = value; OnPropertyChanged("EnableWindow"); } }

        private static int quantDestroyFrames = 0;
        public int QuantDestroyFrames
        {
            get { return quantDestroyFrames; }
            set
            {
                quantDestroyFrames = value;
                OnPropertyChanged("QuantDestroyFrames");
            }
        }

        private static int currentDestroyFrame = 0;
        public int CurrentDestroyFrame
        {
            get { return currentDestroyFrame; }
            set
            {
                currentDestroyFrame = value;
                OnPropertyChanged("CurrentDestroyFrame");
            }
        }

        private static int quantRepairFrames = 0;
        public int QuantRepairFrames
        {
            get { return quantRepairFrames; }
            set
            {
                quantRepairFrames = value;
                OnPropertyChanged("QuantRepairFrames");
            }
        }

        private static int currentRepairFrame = 0;
        public int CurrentRepairFrame
        {
            get { return currentRepairFrame; }
            set
            {
                currentRepairFrame = value;
                OnPropertyChanged("CurrentRepairFrame");
            }
        }

        private static Range tailRadiusShiftOutside_rndRange = new Range(5, 10);
        public string TailRadiusShiftOutside_rndRange_string
        {
            get { return tailRadiusShiftOutside_rndRange.ToString(); }
            set 
            {
                if (Range.CanParse(value))
                    tailRadiusShiftOutside_rndRange = new Range(value);
                OnPropertyChanged("TailRadiusShiftOutside_rndRange_string");
            }
        }

        private static Range tailRadiusShiftInside_rndRange = new Range(0, 0);
        public string TailRadiusShiftInside_rndRange_string
        {
            get { return tailRadiusShiftInside_rndRange.ToString(); }
            set
            {
                if (Range.CanParse(value))
                    tailRadiusShiftInside_rndRange = new Range(value);
                OnPropertyChanged("TailRadiusShiftInside_rndRange_string");
            }
        }

        private static Range AddRadiusToClearTail_rndRange = new Range(15, 15);
        public string AddRadiusToClearTail_rndRange_string
        {
            get { return AddRadiusToClearTail_rndRange.ToString(); }
            set
            {
                if (Range.CanParse(value))
                    AddRadiusToClearTail_rndRange = new Range(value);
                OnPropertyChanged("AddRadiusToClearTail_rndRange_string");
            }
        }

        private static Range AddRadiusToRemovePixels_rndRange = new Range(0, 0);
        public string AddRadiusToRemovePixels_rndRange_string
        {
            get { return AddRadiusToRemovePixels_rndRange.ToString(); }
            set
            {
                if (Range.CanParse(value))
                    AddRadiusToRemovePixels_rndRange = new Range(value);
                OnPropertyChanged("AddRadiusToRemovePixels_rndRange_string");
            }
        }

        private static volatile int MaxThreads = 1;
        public string MaxThreads_string
        {
            get { return MaxThreads.ToString(); }
            set
            {
                try
                {
                    MaxThreads = int.Parse(value);
                    if (MaxThreads <= 0)
                        MaxThreads = 1;
                    OnPropertyChanged("MaxThreads_string");
                }
                catch { }
            }
        }

        private static volatile int RadiusShift = -2;
        public string RadiusShift_string
        {
            get { return RadiusShift.ToString(); }
            set
            {
                try
                {
                    RadiusShift = int.Parse(value);
                    if (RadiusShift > 0)
                        RadiusShift *= -1;
                    if (RadiusShift == 0)
                        RadiusShift = -1;
                    OnPropertyChanged("RadiusShift_string");
                }
                catch { }
            }
        }

        private static volatile int maxTailCopies = 1; 
        public string MaxTailCopies
        {
            get { return maxTailCopies.ToString(); }
            set
            {
                try
                {
                    maxTailCopies = int.Parse(value);
                    if (maxTailCopies < 0)
                        RadiusShift = 0;
                    OnPropertyChanged("MaxTailCopies");
                }
                catch { }
            }
        }

        private static volatile int ChanceToSecondTail = 40; 
        public string ChanceToSecondTail_string
        {
            get { return ChanceToSecondTail.ToString(); }
            set
            {
                try
                {
                    ChanceToSecondTail = int.Parse(value);
                    if (ChanceToSecondTail < 0)
                        ChanceToSecondTail = 0;
                    if (ChanceToSecondTail > 100)
                        ChanceToSecondTail = 100;
                    OnPropertyChanged("ChanceToSecondTail_string");
                }
                catch { }
            }
        }

        private static volatile int chanceToRemoveAddPixels = 0;
        public string ChanceToRemoveAddPixels_string
        {
            get { return chanceToRemoveAddPixels.ToString(); }
            set
            {
                try
                {
                    chanceToRemoveAddPixels = int.Parse(value);
                    if (chanceToRemoveAddPixels < 0)
                        chanceToRemoveAddPixels = 0;
                    if (chanceToRemoveAddPixels > 100)
                        chanceToRemoveAddPixels = 100;
                    OnPropertyChanged("ChanceToRemoveAddPixels_string");
                }
                catch { }
            }
        }

        private static bool Tail = false;
        public bool Tail_
        {
            get { return Tail; }
            set
            {
                Tail = value;
                OnPropertyChanged("Tail_");
            }
        }

        private static double percentEmptyPixelsAtTheEnd = 1; 
        public string PercentEmptyPixelsAtTheEnd
        {
            get { return (percentEmptyPixelsAtTheEnd*100).ToString(); }
            set
            {
                try 
                { 
                    percentEmptyPixelsAtTheEnd = Math.Min(Math.Max(double.Parse(value), 0), 100)/100.0; 
                    OnPropertyChanged("PercentEmptyPixelsAtTheEnd"); 
                }
                catch { }
            }
        }

        private string imagePath = "";
        public string ImagePath
        {
            get { return imagePath; }
            set 
            { 
                imagePath = value;
                imageTool = new ImageDestroyer((Bitmap)System.Drawing.Image.FromFile(imagePath));
                OnPropertyChanged("ImagePath");
            }
        }

        private string savePath= "";
        public string SavePath
        {
            get { return savePath; }
            set
            {
                savePath = value;
                OnPropertyChanged("SavePath");
            }
        }

        private string firstFileName;
        public string FirstFileName
        {
            get { return firstFileName; }
            set
            {
                firstFileName = value;
                OnPropertyChanged("SavePath");
            }
        }

        private RelayCommand openImageCommand;
        public RelayCommand OpenImageCommand
        {
            get
            {
                return openImageCommand ??
                  (openImageCommand = new RelayCommand(obj =>
                  {
                      OpenFileDialog ofd = new OpenFileDialog();
                      ofd.Filter = "Файлы изображений (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png";
                      if (ofd.ShowDialog().Value)
                      {
                          ImagePath = ofd.FileName;
                      }
                  }));
            }
        }

        private RelayCommand selectSavePathCommand;
        public RelayCommand SelectSavePathCommand
        {
            get
            {
                return selectSavePathCommand ??
                  (selectSavePathCommand = new RelayCommand(obj =>
                  {
                      System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                      if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                      {
                          SavePath = fbd.SelectedPath;
                      }
                  }));
            }
        }

        private RelayCommand generateDestroyGIF;
        public RelayCommand GenerateDestroyGIF
        {
            get
            {
                return generateDestroyGIF ??
                  (generateDestroyGIF = new RelayCommand(obj =>
                  {
                      if (ImagePath.Length == 0)
                      {
                          MessageBox.Show("Выберите картинку!", "Ошибка");
                          return;
                      }
                      proggress = new ProgressBar();
                      LoadWindow lw = new LoadWindow();
                      lw.Show();
                      EnableWindow = false;
                      new Thread(() => 
                      { 
                          destroyGIF = imageTool.GetDestroyedGIF(lw);
                          lw.Dispatcher.Invoke(() =>
                          {
                              lw.CanClose = true;
                              lw.Close();
                          });
                          EnableWindow = true;
                          destroyDisplay.Dispatcher.Invoke(()=> { destroyDisplay.Source = BitmapToImageSource(destroyGIF[0]); });
                          QuantDestroyFrames = destroyGIF.Length;
                          CurrentDestroyFrame = 0;
                      }).Start();
                      
                  }));
            }
        }

        bool destroyIsPlaying = false;
        private RelayCommand playDestroyGIF;
        public RelayCommand PlayDestroyGIF
        {
            get
            {
                return playDestroyGIF ??
                  (playDestroyGIF = new RelayCommand(obj =>
                  {
                      new Thread(()=> 
                      {
                          destroyIsPlaying = true;
                          CurrentDestroyFrame = 0;
                          foreach (var frame in destroyGIF)
                          {
                              destroyDisplay.Dispatcher.Invoke(() =>
                              {
                                  destroyDisplay.Source = BitmapToImageSource(frame);
                              });
                              CurrentDestroyFrame++;
                              Thread.Sleep(delayBetweenDestroyFrames);
                          }
                          destroyIsPlaying = false;
                          destroyDisplay.Dispatcher.Invoke(() =>
                          {
                              destroyDisplay.Source = BitmapToImageSource(destroyGIF[0]);
                          });
                          
                      }).Start();
                  }, obj=> !destroyIsPlaying)
                );
            }
        }

        private RelayCommand saveDestroyCommand; 
        public RelayCommand SaveDestroyCommand
        {
            get
            {
                return saveDestroyCommand ??
                  (saveDestroyCommand = new RelayCommand(obj =>
                  {
                      if (SavePath.Length == 0)
                      {
                          MessageBox.Show("Выберите папку сохранения!", "Ошибка");
                          return;
                      }
                      for (int i = 0; i < destroyGIF.Length; i++)
                      {
                          destroyGIF[i].Save(savePath+@"\" + FirstFileName + i + ".png");
                      }
                      MessageBox.Show("Сохранено", "Успех");
                  }));
            }
        }

        private RelayCommand generateRepairGIF;
        public RelayCommand GenerateRepairGIF
        {
            get
            {
                return generateRepairGIF ??
                  (generateRepairGIF = new RelayCommand(obj =>
                  {
                      if (ImagePath.Length == 0)
                      {
                          MessageBox.Show("Выберите картинку!", "Ошибка");
                          return;
                      }
                      proggress = new ProgressBar();
                      LoadWindow lw = new LoadWindow();
                      lw.Show();
                      EnableWindow = false;
                      new Thread(() =>
                      {
                          repairGIF = imageTool.GetRepairedGIF(lw);
                          lw.Dispatcher.Invoke(() =>
                          {
                              lw.CanClose = true;
                              lw.Close();
                          });
                          EnableWindow = true;
                          repairDisplay.Dispatcher.Invoke(() => { repairDisplay.Source = BitmapToImageSource(repairGIF[0]); });
                          QuantRepairFrames = repairGIF.Length;
                          CurrentRepairFrame = 0;
                      }).Start();

                  }));
            }
        }

        bool repairIsPlaying = false;
        private RelayCommand playRepairGIF;
        public RelayCommand PlayRepairGIF
        {
            get
            {
                return playRepairGIF ??
                  (playRepairGIF = new RelayCommand(obj =>
                  {
                      new Thread(() =>
                      {
                          repairIsPlaying = true;
                          CurrentRepairFrame = 0;
                          foreach (var frame in repairGIF)
                          {
                              repairDisplay.Dispatcher.Invoke(() =>
                              {
                                  repairDisplay.Source = BitmapToImageSource(frame);
                              });
                              CurrentRepairFrame++;
                              Thread.Sleep(delayBetweenRepairFrames);
                          }
                          repairIsPlaying = false;
                          repairDisplay.Dispatcher.Invoke(() =>
                          {
                              repairDisplay.Source = BitmapToImageSource(repairGIF[0]);
                          });
                      }).Start();
                  }));
            }
        }

        private RelayCommand saveRepairCommand;
        public RelayCommand SaveRepairCommand
        {
            get
            {
                return saveRepairCommand ??
                  (saveRepairCommand = new RelayCommand(obj =>
                  {
                      if (SavePath.Length == 0)
                      {
                          MessageBox.Show("Выберите папку сохранения!", "Ошибка");
                          return;
                      }
                      for (int i = 0; i < repairGIF.Length; i++)
                      {
                          repairGIF[i].Save(savePath + @"\" + FirstFileName + i + ".png");
                      }
                      MessageBox.Show("Сохранено", "Успех");
                  }));
            }
        }

        static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public MainViewModel(System.Windows.Controls.Image destDisp, System.Windows.Controls.Image repDisp)
        {
            destroyDisplay = destDisp;
            repairDisplay = repDisp;
        }

        

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }


        public struct Range
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

            public Range(string text)
            {
                string[] nums = text.Split(':');
                if (nums.Length != 2)
                    throw new Exception();
                try
                {
                    min = int.Parse(nums[0]);
                    max = int.Parse(nums[1]);
                    if (min > max)
                    {
                        int buffer = min;
                        min = max;
                        max = buffer;
                    }
                }
                catch 
                { 
                    min = 0;
                    max = 0;  
                    throw new Exception(); 
                }
            }

            public static bool CanParse(string s)
            {
                string[] nums = s.Split(':');
                if (nums.Length != 2)
                    return false;
                try
                {
                    int.Parse(nums[0]);
                    int.Parse(nums[1]);
                }
                catch
                {
                    return false;
                }
                return true;
            }

            public override string ToString()
            {
                return min + ":" + max;
            }
        }

        class ImageDestroyer
        {
            

            Bitmap image;
            System.Drawing.Point center;

            public ImageDestroyer(Bitmap img)
            {
                image = (Bitmap)img.Clone();
                center = new System.Drawing.Point(image.Width / 2, image.Height / 2);
            }

            public Bitmap[] GetRepairedGIF(LoadWindow lw)
            {
                HashSet<Thread> threads = new HashSet<Thread>();
                var buffer = new List<Bitmap>();
                int startRadius = (int)Math.Sqrt((center.X * center.X) +(center.Y* center.Y));
                lw.PgMax = Math.Abs(startRadius / RadiusShift);
                lw.PgValue = 0;
                while (startRadius > 0)
                {
                    var th = new Thread(param =>
                    {
                        ThreadParams pars = (ThreadParams)param;
                        TailInfo tailInfo = new TailInfo() { InsideShifts = tailRadiusShiftInside_rndRange, OutsideShifts = tailRadiusShiftOutside_rndRange };
                        Bitmap r = new Bitmap(pars.size.Width, pars.size.Height); 
                        GetRepairedGIF(r, pars.radius, tailInfo);
                        while (buffer.Count != pars.ID)
                        {
                            Thread.Sleep(10);
                        }
                        lock (buffer)
                        {
                            buffer.Add(r);
                            lw.Dispatcher.Invoke(() =>
                            {
                                lw.PgValue++;
                            });
                        }
                    });
                    ThreadParams tp;
                    tp.ID = threads.Count;
                    tp.img = null;
                    tp.radius = startRadius;
                    lock (image)
                        tp.size = new System.Drawing.Size(image.Width, image.Height);
                    th.Start(tp);

                    threads.Add(th);
                    startRadius += RadiusShift;
                    //th.Join();
                    while (threads.Count(t => t.ThreadState == ThreadState.Running) >= MaxThreads)
                        Thread.Sleep(50);
                }
                foreach (var th in threads)
                    th.Join();
                return buffer.ToArray();
            }


            private void GetRepairedGIF(Bitmap source, int radius, TailInfo tailInfo)
            {
                Random rnd = new Random();
                List<System.Drawing.Point> pointsForTail = null;
                if (Tail)
                    pointsForTail = GetPointsForTail(source, radius - tailInfo.InsideShifts.max, radius + tailInfo.OutsideShifts.max);
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        System.Drawing.Color orig;
                        lock(image) { orig = image.GetPixel(x, y); }
                        int relativeToCenterX = x - center.X;
                        int relativeToCenterY = y - center.Y;
                        int currentRadius = (relativeToCenterX * relativeToCenterX) + (relativeToCenterY * relativeToCenterY);



                        if (currentRadius >= radius * radius)
                            source.SetPixel(x, y, orig);


                        if (Tail)
                        {
                            if (pointsForTail.Count > 0)
                            {
                                int count = 0;
                                while (count++ < maxTailCopies && rnd.Next(0, 101) <= ChanceToSecondTail)
                                {
                                    System.Drawing.Point p;
                                    int attemptCount = 0;
                                    do
                                    {
                                        p = pointsForTail[rnd.Next(0, pointsForTail.Count)];
                                    } while (source.GetPixel(p.X, p.Y) != System.Drawing.Color.FromArgb(0, 0, 0, 0) && attemptCount++ < 3);
                                    source.SetPixel(p.X, p.Y, orig);
                                }
                            }
                        }

                        int rndAddRadius2 = rnd.Next(AddRadiusToRemovePixels_rndRange.min, AddRadiusToRemovePixels_rndRange.max);
                        if (currentRadius >= (radius + rndAddRadius2) * (radius + rndAddRadius2))
                        {
                            if (System.Drawing.Color.FromArgb(0, 0, 0, 0) == source.GetPixel(x, y))
                            {
                                if (!(rnd.Next(0, 101) <= chanceToRemoveAddPixels))
                                    source.SetPixel(x, y, orig);
                            }
                        }
                    }
                }
            }

            public Bitmap[] GetDestroyedGIF(LoadWindow lw)
            {
                HashSet<Thread> threads = new HashSet<Thread>();
                var buffer = new List<Bitmap>();
                int startRadius = (int)Math.Sqrt((center.X * center.X) + (center.Y * center.Y));
                lw.PgMax = Math.Abs(startRadius / RadiusShift);
                lw.PgValue = 0;
                while (startRadius > 0)
                {
                    var th = new Thread(param =>
                    {
                        ThreadParams pars = (ThreadParams)param;
                        TailInfo tailInfo = new TailInfo() { InsideShifts = tailRadiusShiftInside_rndRange, OutsideShifts = tailRadiusShiftOutside_rndRange };
                        GetDestroyedGIF(pars.img, pars.radius, tailInfo);
                        while (buffer.Count != pars.ID)
                        {
                            Thread.Sleep(10);
                        }
                        lock(buffer) 
                        {
                            buffer.Add(pars.img); 
                            lw.Dispatcher.Invoke(() =>
                            {
                                lw.PgValue++;
                            });
                        }
                    });
                    ThreadParams tp;
                    tp.ID = threads.Count;
                    tp.img = (Bitmap)image.Clone();
                    tp.radius = startRadius;
                    tp.size = new System.Drawing.Size(image.Width, image.Height);
                    th.Start(tp);
                    
                    threads.Add(th);
                    startRadius += RadiusShift;
                    //th.Join();
                    while (threads.Count(t => t.ThreadState == ThreadState.Running) >= MaxThreads)
                        Thread.Sleep(50);
                }
                foreach (var th in threads)
                    th.Join();
                return buffer.ToArray();
            }

            struct ThreadParams
            {
                public int ID;
                public Bitmap img;
                public int radius;
                public System.Drawing.Size size;
            }

            struct TailInfo
            {
                public Range OutsideShifts;
                public Range InsideShifts;
            }

            private void GetDestroyedGIF(Bitmap source, int radius, TailInfo tailInfo)
            {
                Random rnd = new Random();
                List<System.Drawing.Point> pointsForTail = null;
                if (Tail)
                    pointsForTail = GetPointsForTail(source, radius - tailInfo.InsideShifts.max, radius + tailInfo.OutsideShifts.max);
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        System.Drawing.Color buffer = source.GetPixel(x, y);
                        int relativeToCenterX = x - center.X;
                        int relativeToCenterY = y - center.Y;
                        int currentRadius = (relativeToCenterX * relativeToCenterX) + (relativeToCenterY * relativeToCenterY);

                        int rndAddRadius2 = rnd.Next(AddRadiusToRemovePixels_rndRange.min, AddRadiusToRemovePixels_rndRange.max);

                        
                        if (currentRadius >= radius * radius)
                            source.SetPixel(x, y, System.Drawing.Color.FromArgb(0, 0, 0, 0));

                        if (Tail)
                        {
                            if (pointsForTail.Count > 0)
                            {
                                int count = 0;
                                while (count++ < maxTailCopies && rnd.Next(0, 101) <= ChanceToSecondTail)
                                {
                                    System.Drawing.Point p;
                                    int attemptCount = 0;
                                    do
                                    {
                                        p = pointsForTail[rnd.Next(0, pointsForTail.Count)];
                                    } while (source.GetPixel(p.X, p.Y) != System.Drawing.Color.FromArgb(0, 0, 0, 0) && attemptCount++ < 3);
                                    source.SetPixel(p.X, p.Y, buffer);
                                }
                            }
                        }

                        if (currentRadius >= (radius + rndAddRadius2) * (radius + rndAddRadius2))
                        {
                            if (buffer == source.GetPixel(x, y))
                            {
                                if(!(rnd.Next(0, 101) <= chanceToRemoveAddPixels))
                                    source.SetPixel(x, y, System.Drawing.Color.FromArgb(0, 0, 0, 0));
                            }
                        }
                    }
                }
            }

            private List<System.Drawing.Point> GetPointsForTail(Bitmap source, int inputRadius, int outputRadius)
            {
                List<System.Drawing.Point> pointsForTail = new List<System.Drawing.Point>();
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        int relativeToCenterX = x - center.X;
                        int relativeToCenterY = y - center.Y;
                        int currentRadius = (relativeToCenterX * relativeToCenterX) + (relativeToCenterY * relativeToCenterY);
                        if(currentRadius<= outputRadius* outputRadius && currentRadius>= inputRadius* inputRadius)
                                pointsForTail.Add(new System.Drawing.Point(x, y));
                    }
                }
                MixList(pointsForTail);
                return pointsForTail;
            }

            private void MixList(IList l)
            {
                Random rnd = new Random();
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
}
