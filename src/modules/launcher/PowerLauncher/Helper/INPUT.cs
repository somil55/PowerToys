using System.Runtime.InteropServices;

namespace PowerLauncher.Helper
{
    public static partial class WindowsInteropHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public INPUTTYPE type;
            public InputUnion data;

            public static int Size
            {
                get { return Marshal.SizeOf(typeof(INPUT)); }
            }
        }
    }
}