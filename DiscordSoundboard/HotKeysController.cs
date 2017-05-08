using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DiscordSoundboard
{
    public class HotKeysController
    {
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private readonly Window _window;
        private HwndSource _source;
        private IntPtr _hWnd;
        private const int WM_HOTKEY = 0x0312;

        // TODO: Rework to dictionary
        private const int HotkeyId = 9000;

        public HotKeysController(Window window)
        {
            _window = window;
        }

        public void OnSourceInitialized(EventArgs e)
        {
            var helper = new WindowInteropHelper(_window);
            _hWnd = helper.Handle;
            _source = HwndSource.FromHwnd(_hWnd);
            if (_source != null)
            {
                _source.AddHook(HwndHook);
                RegisterHotKey();
            }
            else
            {
                MessageBox.Show("Adding HotKeys hook error");
                return;
            }
        }

        public void OnClosed(EventArgs e)
        {
            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
                _source = null;
                UnregisterHotKey();
            }
        }

        private void RegisterHotKey()
        {
            // https://msdn.microsoft.com/pl-pl/library/windows/desktop/dd375731(v=vs.85).aspx
            const uint VK_F10 = 0x79;
            //const uint MOD_CTRL = 0x0002;
            if (!RegisterHotKey(_hWnd, HotkeyId, 0, VK_F10))
            {
                // handle error
                MessageBox.Show(_window, "Jebło RegisterHotKey");
            }
        }

        private void UnregisterHotKey()
        { 
            UnregisterHotKey(_hWnd, HotkeyId);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HotkeyId:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            // do stuff
            MessageBox.Show("test");
        }
    }
}
