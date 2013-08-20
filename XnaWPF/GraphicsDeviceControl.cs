namespace XnaWPF
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class GraphicsDeviceControl : HwndHost
    {
        #region Properties

        public GraphicsDevice GraphicsDevice { get { return graphicsService.GraphicsDevice; } }
        public ServiceContainer Services { get { return services; } }

        public bool HwndIsFocused
        {
            get { return (NativeMethods.GetFocus() == hWnd); }
        }

        #endregion

        #region Fields

        private const string windowClass = "GraphicsDeviceControlHostWindowClass";
        private IntPtr hWnd;

        private GraphicsDeviceService graphicsService;
        private ServiceContainer services = new ServiceContainer();

        private bool applicationHasFocus = false;
        private bool mouseInWindow = false;
        private bool isMouseCaptured = false;
        private int capturedMouseX;
        private int capturedMouseY;
        private int capturedMouseClientX;
        private int capturedMouseClientY;

        private Stopwatch timer = new Stopwatch();

        private MouseState mouseState;
        private KeyboardState keyboardState;

        #endregion

        #region Events

        public event EventHandler<GraphicsDeviceEventArgs> OnLoadContent;
        public event EventHandler<GraphicsDeviceEventArgs> OnDraw;

        public event EventHandler<MouseEventArgs> HwndMouseMove;
        public event EventHandler<MouseEventArgs> HwndMouseEnter;
        public event EventHandler<MouseEventArgs> HwndMouseLeave;

        #endregion

        #region Construction and Disposal

        public GraphicsDeviceControl()
        {
            mouseState = new MouseState();
            keyboardState = new KeyboardState();

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

        [Obsolete()]
        public Microsoft.Xna.Framework.Input.MouseState GetMouseState()
        {
            return mouseState.GetXnaState();
        }

        [Obsolete()]
        public KeyboardState GetKeyboardState()
        {
            return keyboardState;
        }

        #endregion

        #region Virtual Methods

        protected virtual void LoadContent()
        {

        }

        protected virtual void Update(float elapsedTime)
        {

        }

        protected virtual void Draw(float elapsedTime)
        {

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        #endregion

        #region Graphics Device Control Implementation

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isMouseCaptured &&
                mouseState.X != capturedMouseX &&
                mouseState.Y != capturedMouseY)
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
            
            Update((float)timer.Elapsed.TotalSeconds);
            Draw((float)timer.Elapsed.TotalSeconds);
            if (OnDraw != null)
                OnDraw(this, new GraphicsDeviceEventArgs(graphicsService.GraphicsDevice));

            graphicsService.GraphicsDevice.Present(viewport.Bounds, null, hWnd);
            timer.Restart();
        }

        void XnaWindowHost_Loaded(object sender, RoutedEventArgs e)
        {
            if (graphicsService == null)
            {
                graphicsService = GraphicsDeviceService.AddRef(hWnd, (int)ActualWidth, (int)ActualHeight);
                services.AddService<IGraphicsDeviceService>(graphicsService);

                LoadContent();
                if (OnLoadContent != null)
                    OnLoadContent(this, new GraphicsDeviceEventArgs(graphicsService.GraphicsDevice));
                timer.Start();
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
                    HwndMouseLeave(this, new MouseEventArgs(mouseState.GetXnaState()));
            }

            ReleaseMouseCapture();
        }

        private void ResetMouseState()
        {
            bool fireL = (mouseState.LeftButton == ButtonState.Pressed);
            bool fireM = (mouseState.MiddleButton == ButtonState.Pressed);
            bool fireR = (mouseState.RightButton == ButtonState.Pressed);
            bool fireX1 = (mouseState.XButton1 == ButtonState.Pressed);
            bool fireX2 = (mouseState.XButton2 == ButtonState.Pressed);

            mouseState.LeftButton = ButtonState.Released;
            mouseState.MiddleButton = ButtonState.Released;
            mouseState.RightButton = ButtonState.Released;
            mouseState.XButton1 = ButtonState.Released;
            mouseState.XButton2 = ButtonState.Released;

            // TODO: Trigger events
            mouseInWindow = false;
        }

        #endregion

        #region HWND Management

        protected override bool TabIntoCore(TraversalRequest request)
        {
            return (NativeMethods.SetFocus(hWnd) != hWnd);
        }

        protected override bool HasFocusWithinCore()
        {
            return HwndIsFocused;
        }

        protected override bool TranslateAcceleratorCore(ref MSG msg, ModifierKeys modifiers)
        {
            if (msg.message == NativeMethods.WM_KEYDOWN)
            {
                var key = (Key)KeyInterop.KeyFromVirtualKey(msg.wParam.ToInt32());
                keyboardState.keysDown[key] = true;
            }
            else if (msg.message == NativeMethods.WM_KEYDOWN)
            {
                var key = (Key)KeyInterop.KeyFromVirtualKey(msg.wParam.ToInt32());
                keyboardState.keysDown[key] = false;
            }

            if (msg.message == NativeMethods.WM_XBUTTONDOWN)
            {
                if (((int)msg.wParam & NativeMethods.MK_XBUTTON1) != 0)
                {
                    mouseState.XButton1 = ButtonState.Released;
                    //if (HwndMouseX1ButtonUp != null)
                    //    HwndMouseX1ButtonUp(this, new HwndMouseEventArgs(mouseState));
                }
                else if (((int)msg.wParam & NativeMethods.MK_XBUTTON2) != 0)
                {
                    mouseState.XButton2 = ButtonState.Released;
                    //if (HwndMouseX2ButtonUp != null)
                    //    HwndMouseX2ButtonUp(this, new HwndMouseEventArgs(mouseState));
                }
            }
            return base.TranslateAcceleratorCore(ref msg, modifiers);
        }

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
            var flags = NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE | NativeMethods.WS_TABSTOP;

            if (this.AllowDrop) flags |= NativeMethods.WS_EX_ACCEPTFILES;

            RegisterWindowClass();
            return NativeMethods.CreateWindowEx(0, windowClass, "",
               flags, 0, 0, (int)Width, (int)Height, hWndParent, IntPtr.Zero, IntPtr.Zero, 0);
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

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
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
                            HwndMouseEnter(this, new MouseEventArgs(mouseState.GetXnaState()));

                        NativeMethods.TRACKMOUSEEVENT tme = new NativeMethods.TRACKMOUSEEVENT();
                        tme.cbSize = Marshal.SizeOf(typeof(NativeMethods.TRACKMOUSEEVENT));
                        tme.dwFlags = NativeMethods.TME_LEAVE;
                        tme.hWnd = hwnd;
                        NativeMethods.TrackMouseEvent(ref tme);
                    }

                    if (mouseState.Position != mouseState.PreviousPosition)
                    {
                        if (HwndMouseMove != null)
                            HwndMouseMove(this, new MouseEventArgs(mouseState.GetXnaState()));
                    }

                    break;
                case NativeMethods.WM_MOUSELEAVE:
                    if (isMouseCaptured)
                        break;

                    ResetMouseState();

                    if (HwndMouseLeave != null)
                        HwndMouseLeave(this, new MouseEventArgs(mouseState.GetXnaState()));
                    break;

                case NativeMethods.WM_LBUTTONDOWN: TestMouseButtonDown(MouseButton.Left); break;
                case NativeMethods.WM_MBUTTONDOWN: TestMouseButtonDown(MouseButton.Middle); break;
                case NativeMethods.WM_RBUTTONDOWN: TestMouseButtonDown(MouseButton.Right); break;
                case NativeMethods.WM_LBUTTONUP: TestMouseButtonUp(MouseButton.Left);  break;
                case NativeMethods.WM_MBUTTONUP: TestMouseButtonUp(MouseButton.Middle); break;
                case NativeMethods.WM_RBUTTONUP: TestMouseButtonUp(MouseButton.Right); break;

                default:
                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        protected virtual void TestMouseButtonDown(MouseButton button) { }
        protected virtual void TestMouseButtonUp(MouseButton button) { }
    }
}
