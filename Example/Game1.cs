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
            // Key.Left and Key.Right, changes focus
            // All keyboard events are working!

            HwndMouseEnter += (s, e) => System.Diagnostics.Debug.WriteLine("MouseEnter");
            HwndMouseLeave += (s, e) => System.Diagnostics.Debug.WriteLine("MouseLeave");

            MouseEnter += (s, e) => System.Diagnostics.Debug.WriteLine("MouseEnterTest");
            MouseLeave += (s, e) => System.Diagnostics.Debug.WriteLine("MouseLeaveTest");
        }

        protected override void TestMouseButtonDown(MouseButton button)
        {
            if (!this.IsFocused) Focus();
            System.Diagnostics.Debug.WriteLine("TestMouseButtonDown");
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
            // This would just get the global key events... unless it turns all keys to false when not focusing, or should I just return null if control is not focused? :/
            // var mouseState = GetMouseState();
            // var keyState = GetKeyboardState();
        }

        protected override void Draw(float elapsedTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, Position, Color);
            spriteBatch.End();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
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
