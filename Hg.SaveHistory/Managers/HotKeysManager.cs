using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Hg.SaveHistory.Types;
using Hg.SaveHistory.Utilities;

// ReSharper disable InconsistentNaming

namespace Hg.SaveHistory.Managers
{
    // Lots of code is from:
    // https://www.pinvoke.net
    public class HotKeysManager : IDisposable
    {
        #region

        internal delegate IntPtr HookProc(int code, uint wParam, IntPtr lParam);

        // From MSDN: https://docs.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-tagkbdllhookstruct
        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal struct KbDllHookStruct
        {
            public uint vkCode { get; set; }
            public uint scanCode { get; set; }
            public uint flags { get; set; }
            public uint time { get; set; }
            public IntPtr dwExtraInfo { get; set; }
        }

        #endregion

        #region

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        public List<HotKeyToAction> HotKeys;

        private readonly HookProc hookProc;

        private bool alt;
        private bool ctrl;
        private bool hooked;

        private IntPtr hookPtr;
        private bool shift;

        public event HotKeyEventHandler KeyDown;
        public event HotKeyEventHandler KeyUp;

        #endregion

        #region

        public HotKeysManager()
        {
            HotKeys = new List<HotKeyToAction>();
            hookProc = HookProcCallback;
            ctrl = false;
            alt = false;
            shift = false;
        }

        public void Dispose()
        {
            UnHook();
            GC.SuppressFinalize(this);
        }

        public string GetCurrentStates()
        {
            return $"Ctrl: {ctrl}, Alt: {alt}, Shift: {shift}";
        }

        public bool Hook()
        {
            if (hooked)
            {
                UnHook();
                Thread.Sleep(250);
            }

            try
            {
                using (var process = Process.GetCurrentProcess())
                {
                    using (var module = process.MainModule)
                    {
                        if (module == null)
                        {
                            return false;
                        }

                        var hModule = GetModuleHandle(module.ModuleName);
                        hookPtr = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, hModule, 0);
                        hooked = true;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return false;
        }

        public void UnHook()
        {
            if (!hooked)
            {
                return;
            }

            UnhookWindowsHookEx(hookPtr);
            hooked = false;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hookId, int nCode, uint wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, HookProc hookProc, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hookId);

        private IntPtr HookProcCallback(int code, uint wParam, IntPtr lParam)
        {
            if (code >= 0)
            {
                KbDllHookStruct kbDllHookStruct =
                    (KbDllHookStruct)Marshal.PtrToStructure(lParam, typeof(KbDllHookStruct));

                Keys key = (Keys)kbDllHookStruct.vkCode;

                KeyEventArgs keyEventArgs = new KeyEventArgs(key);

                if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
                {
                    OnKeyDown(this, keyEventArgs);
                }
                else if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
                {
                    OnKeyUp(this, keyEventArgs);
                }

                if (keyEventArgs.Handled)
                {
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyCode;

            if (key == Keys.LControlKey || key == Keys.RControlKey)
            {
                ctrl = true;
            }

            if (key == Keys.LMenu || key == Keys.RMenu)
            {
                alt = true;
            }

            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
            {
                shift = true;
            }

            foreach (HotKeyToAction hotKeyToAction in HotKeys)
            {
                if (hotKeyToAction.HotKey.Match(key, ctrl, alt, shift))
                {
                    KeyDown?.Invoke(sender, e, hotKeyToAction);
                    return;
                }
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyCode;

            if (key == Keys.LControlKey || key == Keys.RControlKey)
            {
                ctrl = false;
            }

            if (key == Keys.LMenu || key == Keys.RMenu)
            {
                alt = false;
            }

            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
            {
                shift = false;
            }

            foreach (HotKeyToAction hotKeyToAction in HotKeys)
            {
                if (hotKeyToAction.HotKey.Match(key, ctrl, alt, shift))
                {
                    KeyUp?.Invoke(sender, e, hotKeyToAction);
                    return;
                }
            }
        }

        ~HotKeysManager()
        {
            UnHook();
        }

        #endregion
    }
}