using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Baldini.Controls.Calendar
{
    class NativeMethods
    {
        #region pinvokes
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, IntPtr wParam, IntPtr lParam);


        public static void SendMessageA(System.Windows.Forms.Control ctl, UInt32 msg, IntPtr wParam, IntPtr lParam)
        {
            SendMessage(ctl.Handle, msg, wParam, lParam);
        }

        #endregion
    }
}
