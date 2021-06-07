using System;
using System.Runtime.InteropServices;

namespace toolstray
{
    /// <summary>
    /// 全角半角切り替え
    /// </summary>
    public class ImeSwitch : IShortcut
    {

        [DllImport("User32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("imm32.dll")]
        static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool GetGUIThreadInfo(uint dwthreadid, ref GUITHREADINFO lpguithreadinfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public System.Drawing.Rectangle rcCaret;
        }

        const int WM_IME_CONTROL = 0x283;
        const int IMC_GETOPENSTATUS = 5;
        const int IMC_SETOPENSTATUS = 6;

        /// <summary>
        /// 実行
        /// </summary>
        public void Run()
        {
            //IME状態の取得
            GUITHREADINFO gti = new GUITHREADINFO();
            gti.cbSize = Marshal.SizeOf(gti);

            if (!GetGUIThreadInfo(0, ref gti))
            {
                Console.WriteLine("GetGUIThreadInfo failed");
                throw new System.ComponentModel.Win32Exception();
            }
            IntPtr imwd = ImmGetDefaultIMEWnd(gti.hwndFocus);

            bool imeEnabled = SendMessage(imwd, WM_IME_CONTROL, (IntPtr)IMC_GETOPENSTATUS, IntPtr.Zero) != 0;

            // 切り替え
            SendMessage(imwd, WM_IME_CONTROL, (IntPtr)IMC_SETOPENSTATUS, imeEnabled ? IntPtr.Zero : new IntPtr(1));
        }
    }
}
