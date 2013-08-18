namespace XnaWPF
{
    using System;
    using System.Windows.Input;
    using System.Windows;

    public class HwndKeyEventArgs : EventArgs
    {
        public Key Key;
        //public Key Modifier;

        public HwndKeyEventArgs(Key key)
        {
            this.Key = key;
        }
    }
}
