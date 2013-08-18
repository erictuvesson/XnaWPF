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
        ContentBuilder contentBuilder;
        ContentManager contentManager;

        SpriteBatch spriteBatch;
        Texture2D texture;
        Vector2 position;

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

            position = new Vector2();
        }

        protected override void Draw()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y, 200, 100), Color.White);
            spriteBatch.End();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            // Key.Left and Key.Right, changes focus
            float speed = 10;

            if (e.Key == Key.W)
                position.Y -= speed;
            else if (e.Key == Key.S)
                position.Y += speed;

            if (e.Key == Key.A)
                position.X -= speed;
            else if (e.Key == Key.D)
                position.X += speed;

            base.OnKeyDown(e);
        }
    }
}
