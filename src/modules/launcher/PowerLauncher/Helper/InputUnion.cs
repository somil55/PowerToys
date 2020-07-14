using System.Runtime.InteropServices;

namespace PowerLauncher.Helper
{
    public static partial class WindowsInteropHelper
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;
            [FieldOffset(0)]
            internal KEYBDINPUT ki;
            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }
    }
}