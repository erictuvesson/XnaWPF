namespace XnaWPF
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Microsoft.Xna.Framework.Input;

    // We could just send those structures instead of Xna's but what a heck

    internal struct KeyboardState
    {
        internal Dictionary<Keys, bool> keysDown;

        public KeyboardState(params Keys[] keys) // TODO: Apply those keys? I am never going to use it tho
        {
            keysDown = new Dictionary<Keys, bool>();
        }

        public Microsoft.Xna.Framework.Input.KeyboardState GetXnaState()
        {
            return new Microsoft.Xna.Framework.Input.KeyboardState((from k in keysDown.ToList() where k.Value select k.Key).ToArray());
        }
    }

    internal struct MouseState
    {
        public ButtonState LeftButton { get; internal set; }
        public ButtonState MiddleButton { get; internal set; }
        public ButtonState RightButton { get; internal set; }
        public ButtonState XButton1 { get; internal set; }
        public ButtonState XButton2 { get; internal set; }
        public int ScrollWheelValue { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }

        public Microsoft.Xna.Framework.Input.MouseState GetXnaState()
        {
            return new Microsoft.Xna.Framework.Input.MouseState(X, Y, ScrollWheelValue, LeftButton, MiddleButton, RightButton, XButton1, XButton2);
        }
    }
}
