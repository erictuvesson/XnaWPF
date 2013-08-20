namespace XnaWPF
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Microsoft.Xna.Framework.Input;

    // Struct
    public class KeyboardState
    {
        internal Dictionary<Key, bool> keysDown;

        public bool this[Key key] 
        {
            get { return IsKeyDown(key); } 
        }

        public KeyboardState()
        {
            keysDown = new Dictionary<Key, bool>();
        }

        public Key[] GetPressedKeys()
        {
            return (from k in keysDown where k.Value select k.Key).ToArray();
        }

        public bool IsKeyDown(Key key)
        {
            if (!keysDown.ContainsKey(key)) 
                return false;
            return keysDown[key];
        }

        public bool IsKeyUp(Key key)
        {
            return !IsKeyDown(key);
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
        public int PrevX { get; internal set; }
        public int PrevY { get; internal set; }

        public System.Windows.Point Position
        {
            get { return new System.Windows.Point(X, Y); }
            set
            {
                X = (int)value.X;
                Y = (int)value.Y;
            }
        }
        public System.Windows.Point PreviousPosition
        {
            get { return new System.Windows.Point(PrevX, PrevY); }
            set
            {
                PrevX = (int)value.X;
                PrevY = (int)value.Y;
            }
        }

        public Microsoft.Xna.Framework.Input.MouseState GetXnaState()
        {
            return new Microsoft.Xna.Framework.Input.MouseState(X, Y, ScrollWheelValue, LeftButton, MiddleButton, RightButton, XButton1, XButton2);
        }
    }
}
