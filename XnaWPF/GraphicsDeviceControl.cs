﻿namespace XnaWPF
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using Microsoft.Xna.Framework.Graphics;

    public class GraphicsDeviceControl : HwndHost
    {
        #region Properties

        public GraphicsDevice GraphicsDevice { get { return graphicsService.GraphicsDevice; } }
        public ServiceContainer Services { get { return services; } }

        #endregion

        #region Fields

        private const string windowClass = "GraphicsDeviceControlHostWindowClass";
        private IntPtr hWnd;

        private GraphicsDeviceService graphicsService;
        ServiceContainer services = new ServiceContainer();

        private bool applicationHasFocus = false;
        private bool mouseInWindow = false;
        private bool isMouseCaptured = false;
        private int capturedMouseX;
        private int capturedMouseY;
        private int capturedMouseClientX;
        private int capturedMouseClientY;
        private HwndMouseState mouseState = new HwndMouseState();

        #endregion

        #region Events

        public event EventHandler<GraphicsDeviceEventArgs> LoadContent;
        public event EventHandler<GraphicsDeviceEventArgs> Draw;

        // TODO: X1 & X2 is not counted in MouseDown/Up

        public event EventHandler<HwndMouseEventArgs> HwndMouseDown;
        public event EventHandler<HwndMouseEventArgs> HwndMouseUp;
        public event EventHandler<HwndMouseEventArgs> HwndMouseDblClick;
        public event EventHandler<HwndMouseEventArgs> HwndMouseMove;
        public event EventHandler<HwndMouseEventArgs> HwndMouseEnter;
        public event EventHandler<HwndMouseEventArgs> HwndMouseLeave;

        public event EventHandler<HwndMouseEventArgs> HwndLButtonDown;
        public event EventHandler<HwndMouseEventArgs> HwndLButtonUp;
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDblClick;
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDown;
        public event EventHandler<HwndMouseEventArgs> HwndRButtonUp;
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDblClick;
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDown;
        public event EventHandler<HwndMouseEventArgs> HwndMButtonUp;
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDblClick;
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDown;
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonUp;
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDblClick;
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDown;
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonUp;
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDblClick;

        #endregion

        #region Construction and Disposal

        public GraphicsDeviceControl()
        {
            Loaded += new RoutedEventHandler(XnaWindowHost_Loaded);
            SizeChanged += new SizeChangedEventHandler(XnaWindowHost_SizeChanged);

            Application.Current.Activated += new EventHandler(Current_Activated);
            Application.Current.Deactivated += new EventHandler(Current_Deactivated);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        protected override void Dispose(bool disposing)
        {
            if (graphicsService != null)
            {
                graphicsService.Release(disposing);
                graphicsService = null;
            }

            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            base.Dispose(disposing);
        }

        #endregion

        #region Public Methods

        public new void CaptureMouse()
        {
            if (isMouseCaptured)
                return;

            NativeMethods.ShowCursor(false);
            isMouseCaptured = true;

            NativeMethods.POINT p = new NativeMethods.POINT();
            NativeMethods.GetCursorPos(ref p);
            capturedMouseX = p.X;
            capturedMouseY = p.Y;

            NativeMethods.ScreenToClient(hWnd, ref p);
            capturedMouseClientX = p.X;
            capturedMouseClientY = p.Y;
        }

        public new void ReleaseMouseCapture()
        {
            if (!isMouseCaptured)
                return;

            NativeMethods.ShowCursor(true);
            isMouseCaptured = false;
        }

        #endregion

        #region Graphics Device Control Implementation

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isMouseCaptured &&
                (int)mouseState.Position.X != capturedMouseX &&
                (int)mouseState.Position.Y != capturedMouseY)
            {
                NativeMethods.SetCursorPos(capturedMouseX, capturedMouseY);
                mouseState.Position = mouseState.PreviousPosition = new Point(capturedMouseClientX, capturedMouseClientY);
            }

            if (graphicsService == null) return;

            int width = (int)ActualWidth;
            int height = (int)ActualHeight;

            if (width < 1 || height < 1) return;

            Viewport viewport = new Viewport(0, 0, width, height);
            graphicsService.GraphicsDevice.Viewport = viewport;

            if (Draw != null)
                Draw(this, new GraphicsDeviceEventArgs(graphicsService.GraphicsDevice));

            graphicsService.GraphicsDevice.Present(viewport.Bounds, null, hWnd);
        }

        void XnaWindowHost_Loaded(object sender, RoutedEventArgs e)
        {
            if (graphicsService == null)
            {
                graphicsService = GraphicsDeviceService.AddRef(hWnd, (int)ActualWidth, (int)ActualHeight);
                services.AddService<IGraphicsDeviceService>(graphicsService);

                if (LoadContent != null)
                    LoadContent(this, new GraphicsDeviceEventArgs(graphicsService.GraphicsDevice));
            }
        }

        void XnaWindowHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (graphicsService != null)
                graphicsService.ResetDevice((int)ActualWidth, (int)ActualHeight);
        }

        void Current_Activated(object sender, EventArgs e)
        {
            applicationHasFocus = true;
        }

        void Current_Deactivated(object sender, EventArgs e)
        {
            applicationHasFocus = false;
            ResetMouseState();

            if (mouseInWindow)
            {
                mouseInWindow = false;
                if (HwndMouseLeave != null)
                    HwndMouseLeave(this, new HwndMouseEventArgs(mouseState));
            }

            ReleaseMouseCapture();
        }

        private void ResetMouseState()
        {
            // We need to invoke events for any buttons that were pressed
            bool fireL = mouseState.LeftButton == MouseButtonState.Pressed;
            bool fireM = mouseState.MiddleButton == MouseButtonState.Pressed;
            bool fireR = mouseState.RightButton == MouseButtonState.Pressed;
            bool fireX1 = mouseState.X1Button == MouseButtonState.Pressed;
            bool fireX2 = mouseState.X2Button == MouseButtonState.Pressed;

            // Update the state of all of the buttons
            mouseState.LeftButton = MouseButtonState.Released;
            mouseState.MiddleButton = MouseButtonState.Released;
            mouseState.RightButton = MouseButtonState.Released;
            mouseState.X1Button = MouseButtonState.Released;
            mouseState.X2Button = MouseButtonState.Released;

            // Fire any events
            HwndMouseEventArgs args = new HwndMouseEventArgs(mouseState);
            if (fireL && HwndLButtonUp != null)
                HwndLButtonUp(this, args);
            if (fireM && HwndMButtonUp != null)
                HwndMButtonUp(this, args);
            if (fireR && HwndRButtonUp != null)
                HwndRButtonUp(this, args);
            if (fireX1 && HwndX1ButtonUp != null)
                HwndX1ButtonUp(this, args);
            if (fireX2 && HwndX2ButtonUp != null)
                HwndX2ButtonUp(this, args);
            // The mouse is no longer considered to be in our window
            mouseInWindow = false;
        }

        #endregion

        #region HWND Management

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            hWnd = CreateHostWindow(hwndParent.Handle);
            return new HandleRef(this, hWnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            NativeMethods.DestroyWindow(hwnd.Handle);
            hWnd = IntPtr.Zero;
        }

        private IntPtr CreateHostWindow(IntPtr hWndParent)
        {
            RegisterWindowClass();
            return NativeMethods.CreateWindowEx(0, windowClass, "",
               NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE,
               0, 0, (int)Width, (int)Height, hWndParent, IntPtr.Zero, IntPtr.Zero, 0);
        }

        private void RegisterWindowClass()
        {
            NativeMethods.WNDCLASSEX wndClass = new NativeMethods.WNDCLASSEX();
            wndClass.cbSize = (uint)Marshal.SizeOf(wndClass);
            wndClass.hInstance = NativeMethods.GetModuleHandle(null);
            wndClass.lpfnWndProc = NativeMethods.DefaultWindowProc;
            wndClass.lpszClassName = windowClass;
            wndClass.hCursor = NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW);

            NativeMethods.RegisterClassEx(ref wndClass);
        }

        #endregion

        #region WndProc Implementation

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HSCROLL ||
                msg == NativeMethods.WM_VSCROLL ||
                msg == NativeMethods.WM_MOUSEWHEEL)
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }

            switch (msg)
            {
                #region MouseDown
                case NativeMethods.WM_LBUTTONDOWN:
                    mouseState.LeftButton = MouseButtonState.Pressed;
                    if (HwndMouseDown != null)
                        HwndMouseDown(this, new HwndMouseEventArgs(mouseState));
                    if (HwndLButtonDown != null)
                        HwndLButtonDown(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_RBUTTONDOWN:
                    mouseState.RightButton = MouseButtonState.Pressed;
                    if (HwndMouseDown != null)
                        HwndMouseDown(this, new HwndMouseEventArgs(mouseState));
                    if (HwndRButtonDown != null)
                        HwndRButtonDown(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_MBUTTONDOWN:
                    mouseState.MiddleButton = MouseButtonState.Pressed;
                    if (HwndMouseDown != null)
                        HwndMouseDown(this, new HwndMouseEventArgs(mouseState));
                    if (HwndMButtonDown != null)
                        HwndMButtonDown(this, new HwndMouseEventArgs(mouseState));
                    break;
                #endregion

                #region MouseUp
                case NativeMethods.WM_LBUTTONUP:
                    mouseState.LeftButton = MouseButtonState.Released;
                    if (HwndMouseUp != null)
                        HwndMouseUp(this, new HwndMouseEventArgs(mouseState));
                    if (HwndLButtonUp != null)
                        HwndLButtonUp(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_RBUTTONUP:
                    mouseState.RightButton = MouseButtonState.Released;
                    if (HwndMouseUp != null)
                        HwndMouseUp(this, new HwndMouseEventArgs(mouseState));
                    if (HwndRButtonUp != null)
                        HwndRButtonUp(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_MBUTTONUP:
                    mouseState.MiddleButton = MouseButtonState.Released;
                    if (HwndMouseUp != null)
                        HwndMouseUp(this, new HwndMouseEventArgs(mouseState));
                    if (HwndMButtonUp != null)
                        HwndMButtonUp(this, new HwndMouseEventArgs(mouseState));
                    break;
                #endregion

                #region MouseDblClick
                case NativeMethods.WM_LBUTTONDBLCLK:
                    if (HwndMouseDblClick != null)
                        HwndMouseDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Left));
                    if (HwndLButtonDblClick != null)
                        HwndLButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Left));
                    break;
                case NativeMethods.WM_RBUTTONDBLCLK:
                    if (HwndMouseDblClick != null)
                        HwndMouseDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Right));
                    if (HwndRButtonDblClick != null)
                        HwndRButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Right));
                    break;
                case NativeMethods.WM_MBUTTONDBLCLK:
                    if (HwndMouseDblClick != null)
                        HwndMouseDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Middle));
                    if (HwndMButtonDblClick != null)
                        HwndMButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Middle));
                    break;
                #endregion

                #region X1 & X2
                case NativeMethods.WM_XBUTTONDOWN:
                    if (((int)wParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        mouseState.X1Button = MouseButtonState.Pressed;
                        if (HwndX1ButtonDown != null)
                            HwndX1ButtonDown(this, new HwndMouseEventArgs(mouseState));
                    }
                    else if (((int)wParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        mouseState.X2Button = MouseButtonState.Pressed;
                        if (HwndX2ButtonDown != null)
                            HwndX2ButtonDown(this, new HwndMouseEventArgs(mouseState));
                    }
                    break;
                case NativeMethods.WM_XBUTTONUP:
                    if (((int)wParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        mouseState.X1Button = MouseButtonState.Released;
                        if (HwndX1ButtonUp != null)
                            HwndX1ButtonUp(this, new HwndMouseEventArgs(mouseState));
                    }
                    else if (((int)wParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        mouseState.X2Button = MouseButtonState.Released;
                        if (HwndX2ButtonUp != null)
                            HwndX2ButtonUp(this, new HwndMouseEventArgs(mouseState));
                    }
                    break;
                case NativeMethods.WM_XBUTTONDBLCLK:
                    if (((int)wParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        if (HwndX1ButtonDblClick != null)
                            HwndX1ButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.XButton1));
                    }
                    else if (((int)wParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        if (HwndX2ButtonDblClick != null)
                            HwndX2ButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.XButton2));
                    }
                    break;
                #endregion

                #region Mouse Move etc
                case NativeMethods.WM_MOUSEMOVE:
                    if (!applicationHasFocus)
                        break;

                    mouseState.PreviousPosition = mouseState.Position;
                    mouseState.Position = new Point(
                        NativeMethods.GetXLParam((int)lParam),
                        NativeMethods.GetYLParam((int)lParam));

                    if (!mouseInWindow)
                    {
                        mouseInWindow = true;
                        mouseState.PreviousPosition = mouseState.Position;

                        if (HwndMouseEnter != null)
                            HwndMouseEnter(this, new HwndMouseEventArgs(mouseState));

                        NativeMethods.TRACKMOUSEEVENT tme = new NativeMethods.TRACKMOUSEEVENT();
                        tme.cbSize = Marshal.SizeOf(typeof(NativeMethods.TRACKMOUSEEVENT));
                        tme.dwFlags = NativeMethods.TME_LEAVE;
                        tme.hWnd = hwnd;
                        NativeMethods.TrackMouseEvent(ref tme);
                    }

                    if (mouseState.Position != mouseState.PreviousPosition)
                    {
                        if (HwndMouseMove != null)
                            HwndMouseMove(this, new HwndMouseEventArgs(mouseState));
                    }

                    break;
                case NativeMethods.WM_MOUSELEAVE:
                    if (isMouseCaptured)
                        break;

                    ResetMouseState();

                    if (HwndMouseLeave != null)
                        HwndMouseLeave(this, new HwndMouseEventArgs(mouseState));
                    break;
                #endregion

                default:
                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        #endregion
    }
}