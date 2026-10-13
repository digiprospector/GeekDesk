using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;

namespace GeekDesk.Util
{
    class MouseUtil
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        /// <summary>
        /// 获取鼠标坐标
        /// </summary>
        /// <returns></returns>
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }


        /// <summary>
        /// 获取鼠标坐标并转换为 WPF 逻辑坐标(DIP)
        /// GetCursorPos 返回的是物理像素，在高 DPI 缩放下需要除以缩放比
        /// </summary>
        public static Point GetMousePositionDip()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            HwndSource source = null;
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                source = PresentationSource.FromVisual(Application.Current.MainWindow) as HwndSource;
            }

            double scaleX = 1.0;
            double scaleY = 1.0;
            if (source?.CompositionTarget != null)
            {
                scaleX = source.CompositionTarget.TransformToDevice.M11;
                scaleY = source.CompositionTarget.TransformToDevice.M22;
            }

            return new Point(w32Mouse.X / scaleX, w32Mouse.Y / scaleY);
        }


        public static Point GetMousePosition(Visual relativeTo)
        {
            Win32Point mouse = new Win32Point();
            GetCursorPos(ref mouse);

            // Using PointFromScreen instead of Dan Crevier's code (commented out below)
            // is a bug fix created by William J. Roberts.  Read his comments about the fix
            // here: http://www.codeproject.com/useritems/ListViewDragDropManager.asp?msg=1911611#xx1911611xx
            return relativeTo.PointFromScreen(new Point((double)mouse.X, (double)mouse.Y));

            #region Commented Out
            //System.Windows.Interop.HwndSource presentationSource =
            //    (System.Windows.Interop.HwndSource)PresentationSource.FromVisual( relativeTo );
            //ScreenToClient( presentationSource.Handle, ref mouse );
            //GeneralTransform transform = relativeTo.TransformToAncestor( presentationSource.RootVisual );
            //Point offset = transform.Transform( new Point( 0, 0 ) );
            //return new Point( mouse.X - offset.X, mouse.Y - offset.Y );
            #endregion // Commented Out
        }

    }
}
