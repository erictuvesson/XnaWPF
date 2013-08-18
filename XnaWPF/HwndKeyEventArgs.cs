namespace XnaWPF
{
    using System;
    using System.Windows.Input;
    using System.Windows;

    public class HwndKeyEventArgs : EventArgs
    {
        public Key Key;
        public ModifierKeys Modifier;

        public HwndKeyEventArgs(Key key, ModifierKeys modifier)
        {
            this.Key = key;
            this.Modifier = modifier;
        }
    }
}
