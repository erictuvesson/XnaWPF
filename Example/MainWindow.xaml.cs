using System;
using System.IO;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XnaWPF.Content;

namespace Example
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            XnaHost.LoadContent += XnaHost_LoadContent;
            XnaHost.Draw += XnaHost_Draw;
        }

        ContentBuilder contentBuilder;
        ContentManager contentManager;

        SpriteBatch spriteBatch;
        Texture2D texture;

        void XnaHost_LoadContent(object sender, XnaWPF.GraphicsDeviceEventArgs e)
        {
            contentBuilder = new ContentBuilder();
            contentManager = new ContentManager(XnaHost.Services, contentBuilder.OutputDirectory);

            spriteBatch = new SpriteBatch(e.GraphicsDevice);

            try
            {
                // http://msdn.microsoft.com/en-us/library/bb447762.aspx

                //contentBuilder.TryLoadTexture2D(contentManager, Environment.CurrentDirectory + "\\XnaLogo.png", out texture);
                texture = contentBuilder.Load<Texture2D>(contentManager, Environment.CurrentDirectory + "\\XnaLogo.png");

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        void XnaHost_Draw(object sender, XnaWPF.GraphicsDeviceEventArgs e)
        {
            e.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 200, 100), Color.White);
            spriteBatch.End();
        }
    }
}
