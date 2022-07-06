using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DestroyerImage.Windows.Load
{
    /// <summary>
    /// Логика взаимодействия для LoadWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        ProgressBar pg = new ProgressBar();
        public bool CanClose { get; set; }
        public double PgValue
        {
            get { return pg.Value; }
            set { this.Dispatcher.Invoke(() => { pg.Value = value; pg.UpdateLayout(); }); }
        }

        public double PgMax
        {
            get { return pg.Maximum; }
            set { this.Dispatcher.Invoke(() => { pg.Maximum = value; pg.UpdateLayout(); }); }
        }

        public LoadWindow(int maxValue=0)
        {
            InitializeComponent();
            
            pg.Maximum = maxValue;
            pg.Margin = new Thickness(10);
            this.Content = pg;
        }

        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!CanClose)
                e.Cancel = true;
        }
    }
}
