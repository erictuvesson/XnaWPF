namespace XnaWPF
{
    using System;
    using Microsoft.Xna.Framework.Graphics;

    public class GraphicsDeviceEventArgs : EventArgs
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public GraphicsDeviceEventArgs(GraphicsDevice device)
        {
            GraphicsDevice = device;
        }
    }
}
