using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XnaWPF.Content;

namespace Example
{
    public class Game1 : XnaWPF.GraphicsDeviceControl
    {
        public Color Color = Color.White;
        public Vector2 Position = new Vector2();

        ContentBuilder contentBuilder;
        ContentManager contentManager;

        SpriteBatch spriteBatch;
        Texture2D texture;

        public Game1()
        {

        }

        protected override void LoadContent()
        {
            contentBuilder = new ContentBuilder();
            contentManager = new ContentManager(Services, contentBuilder.OutputDirectory);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            try
            {
                texture = contentBuilder.Load<Texture2D>(contentManager, Environment.CurrentDirectory + "\\XnaLogo.png");
            }
            catch (Exception error)
            {
                System.Windows.MessageBox.Show(error.Message);
            }
        }

        protected override void Update(float elapsedTime)
        {

        }

        protected override void Draw(float elapsedTime)
        {
            // Should I make this compatible?
            // var keyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            // var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, Position, Color);
            spriteBatch.End();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            // Key.Left and Key.Right, changes focus
            float speed = 10;

            if (e.Key == Key.W)
                Position.Y -= speed;
            else if (e.Key == Key.S)
                Position.Y += speed;

            if (e.Key == Key.A)
                Position.X -= speed;
            else if (e.Key == Key.D)
                Position.X += speed;

            base.OnKeyDown(e);
        }

        public void ChangeTexture(string filePath)
        {
            try
            {
                texture = contentBuilder.Load<Texture2D>(contentManager, filePath);
            }
            catch (Exception error)
            {
                System.Windows.MessageBox.Show(error.Message);
            }
        }
    }
}
