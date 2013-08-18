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
        }

        ContentBuilder contentBuilder;
        ContentManager contentManager;

        SpriteBatch spriteBatch;
        Texture2D texture;
        Vector2 position;

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

            position = new Vector2();
        }

        void XnaHost_Draw(object sender, XnaWPF.GraphicsDeviceEventArgs e)
        {
            this.Title = XnaHost.IsFocused.ToString();
            e.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y, 200, 100), Color.White);
            spriteBatch.End();
        }

        private void XnaHost_HwndKeyDown(object sender, HwndKeyEventArgs e)
        {
            // Key.Left and Key.Right, changes focus

            float speed = 10;

            if (e.Key == Key.W)
                position.Y -= speed;
            else if (e.Key == Key.S)
                position.Y += speed;
            else if (e.Key == Key.A)
                position.X -= speed;
            else if (e.Key == Key.D)
                position.X += speed;
        }
    }
}
