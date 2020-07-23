﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace Microsoft.PowerToys.Settings.UI
{
    internal static class Interop
    {
        public static ICoreWindowInterop GetInterop(this Windows.UI.Core.CoreWindow @this)
        {
            var unkIntPtr = Marshal.GetIUnknownForObject(@this);
            try
            {
                var interopObj = Marshal.GetTypedObjectForIUnknown(unkIntPtr, typeof(ICoreWindowInterop)) as ICoreWindowInterop;
                return interopObj;
            }
            finally
            {
                Marshal.Release(unkIntPtr);
                unkIntPtr = System.IntPtr.Zero;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

        public const int SW_HIDE = 0;
    }
}
