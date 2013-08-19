using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XnaWPF;
using XnaWPF.Content;

namespace Example
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Slider1.Value = 255;
            Slider2.Value = 255;
            Slider3.Value = 255;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // You can do this by binding or what you want. Just doing it like this for the simplicity.
            XnaHost.Color.R = (byte)Slider1.Value;
            XnaHost.Color.G = (byte)Slider2.Value;
            XnaHost.Color.B = (byte)Slider3.Value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // You should really pre-create this,
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "";
            dlg.DefaultExt = ".png";
            dlg.Filter = "Textures|*.bmp;*.dds;*.dib;*.hdr;*.jpg;*.pfm;*.png;*.ppm;*.tga";

            if (dlg.ShowDialog() == true)
            {
                XnaHost.ChangeTexture(dlg.FileName);
            }
        }
    }
}
