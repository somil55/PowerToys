// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1060:Move pinvokes to native methods class", Justification = "<Pending>", Scope = "type", Target = "~T:PowerLauncher.Helper.WindowsInteropHelper")]
[assembly: SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>", Scope = "member", Target = "~M:PowerLauncher.Helper.SingleInstance`1.Cleanup")]
[assembly: SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>", Scope = "member", Target = "~M:PowerLauncher.Helper.SingleInstance`1.InitializeAsFirstInstance(System.String)~System.Boolean")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>", Scope = "member", Target = "~P:PowerLauncher.ViewModel.ResultViewModel.ContextMenuItems")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~P:PowerLauncher.ViewModel.ResultViewModel.Image")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~P:PowerLauncher.ViewModel.SettingWindowViewModel.ActivatedTimes")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:PowerLauncher.ViewModel.MainViewModel.SetHotkey(Wox.Infrastructure.Hotkey.HotkeyModel,interop.HotkeyCallback)")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:PowerLauncher.ViewModel.MainViewModel.InitializeKeyCommands")]
[assembly: SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "<Pending>", Scope = "member", Target = "~M:PowerLauncher.ViewModel.MainViewModel.QueryHistory")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:PowerLauncher.ViewModel.MainViewModel.QueryHistory")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:PowerLauncher.ViewModel.MainViewModel.SetHotkey(Wox.Infrastructure.Hotkey.HotkeyModel,interop.HotkeyCallback)")]
[assembly: SuppressMessage("Design", "CA1041:Provide ObsoleteAttribute message", Justification = "<Pending>", Scope = "member", Target = "~M:Wox.PublicAPIInstance.HideApp")]
[assembly: SuppressMessage("Design", "CA1041:Provide ObsoleteAttribute message", Justification = "<Pending>", Scope = "member", Target = "~M:Wox.PublicAPIInstance.ShowApp")]
[assembly: SuppressMessage("Design", "CA1041:Provide ObsoleteAttribute message", Justification = "<Pending>", Scope = "member", Target = "~M:Wox.PublicAPIInstance.CloseApp")]
[assembly: SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "<Pending>", Scope = "type", Target = "~T:PowerLauncher.Helper.WindowsInteropHelper.INPUTTYPE")]
[assembly: SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible", Justification = "<Pending>", Scope = "member", Target = "~M:PowerLauncher.Helper.WindowsInteropHelper.SendInput(System.UInt32,PowerLauncher.Helper.WindowsInteropHelper.INPUT[],System.Int32)~System.UInt32")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>", Scope = "type", Target = "~T:PowerLauncher.Helper.WindowsInteropHelper.INPUT")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:PowerLauncher.Helper.WindowsInteropHelper.INPUT.type")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:PowerLauncher.Helper.WindowsInteropHelper.INPUT.data")]
[assembly: SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:PowerLauncher.Helper.WindowsInteropHelper.INPUT")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>", Scope = "type", Target = "~T:PowerLauncher.Helper.WindowsInteropHelper.InputUnion")]
[assembly: SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:PowerLauncher.Helper.WindowsInteropHelper.InputUnion")]
