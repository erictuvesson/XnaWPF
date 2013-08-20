namespace XnaWPF
{
    using System;

    public class MouseEventArgs : EventArgs
    {
        public Microsoft.Xna.Framework.Input.MouseState MouseState { get; private set; }

        public MouseEventArgs(Microsoft.Xna.Framework.Input.MouseState mouseState)
        {
            this.MouseState = mouseState;
        }
    }
}
