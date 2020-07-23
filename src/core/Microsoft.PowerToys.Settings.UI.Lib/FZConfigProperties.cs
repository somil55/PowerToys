﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.PowerToys.Settings.UI.Lib
{
    public class FZConfigProperties
    {
        public static readonly HotkeySettings DefaultHotkeyValue = new HotkeySettings(true, false, false, false, 0xc0);

        public FZConfigProperties()
        {
            FancyzonesShiftDrag = new BoolProperty(ConfigDefaults.DefaultFancyzonesShiftDrag);
            FancyzonesOverrideSnapHotkeys = new BoolProperty();
            FancyzonesMouseSwitch = new BoolProperty();
            FancyzonesMoveWindowsAcrossMonitors = new BoolProperty();
            FancyzonesDisplayChangeMoveWindows = new BoolProperty();
            FancyzonesZoneSetChangeMoveWindows = new BoolProperty();
            FancyzonesAppLastZoneMoveWindows = new BoolProperty();
            FancyzonesOpenWindowOnActiveMonitor = new BoolProperty();
            FancyzonesRestoreSize = new BoolProperty();
            UseCursorposEditorStartupscreen = new BoolProperty(ConfigDefaults.DefaultUseCursorposEditorStartupscreen);
            FancyzonesShowOnAllMonitors = new BoolProperty();
            FancyzonesZoneHighlightColor = new StringProperty(ConfigDefaults.DefaultFancyZonesZoneHighlightColor);
            FancyzonesHighlightOpacity = new IntProperty(50);
            FancyzonesEditorHotkey = new KeyboardKeysProperty(DefaultHotkeyValue);
            FancyzonesMakeDraggedWindowTransparent = new BoolProperty();
            FancyzonesExcludedApps = new StringProperty();
            FancyzonesInActiveColor = new StringProperty(ConfigDefaults.DefaultFancyZonesInActiveColor);
            FancyzonesBorderColor = new StringProperty(ConfigDefaults.DefaultFancyzonesBorderColor);
        }

        [JsonPropertyName("fancyzones_shiftDrag")]
        public BoolProperty FancyzonesShiftDrag { get; set; }

        [JsonPropertyName("fancyzones_mouseSwitch")]
        public BoolProperty FancyzonesMouseSwitch { get; set; }

        [JsonPropertyName("fancyzones_overrideSnapHotkeys")]
        public BoolProperty FancyzonesOverrideSnapHotkeys { get; set; }

        [JsonPropertyName("fancyzones_moveWindowAcrossMonitors")]
        public BoolProperty FancyzonesMoveWindowsAcrossMonitors { get; set; }

        [JsonPropertyName("fancyzones_displayChange_moveWindows")]
        public BoolProperty FancyzonesDisplayChangeMoveWindows { get; set; }

        [JsonPropertyName("fancyzones_zoneSetChange_moveWindows")]
        public BoolProperty FancyzonesZoneSetChangeMoveWindows { get; set; }

        [JsonPropertyName("fancyzones_appLastZone_moveWindows")]
        public BoolProperty FancyzonesAppLastZoneMoveWindows { get; set; }

        [JsonPropertyName("fancyzones_openWindowOnActiveMonitor")]
        public BoolProperty FancyzonesOpenWindowOnActiveMonitor { get; set; }

        [JsonPropertyName("fancyzones_restoreSize")]
        public BoolProperty FancyzonesRestoreSize { get; set; }

        [JsonPropertyName("use_cursorpos_editor_startupscreen")]
        public BoolProperty UseCursorposEditorStartupscreen { get; set; }

        [JsonPropertyName("fancyzones_show_on_all_monitors")]
        public BoolProperty FancyzonesShowOnAllMonitors { get; set; }

        [JsonPropertyName("fancyzones_makeDraggedWindowTransparent")]
        public BoolProperty FancyzonesMakeDraggedWindowTransparent { get; set; }

        [JsonPropertyName("fancyzones_zoneHighlightColor")]
        public StringProperty FancyzonesZoneHighlightColor { get; set; }

        [JsonPropertyName("fancyzones_highlight_opacity")]
        public IntProperty FancyzonesHighlightOpacity { get; set; }

        [JsonPropertyName("fancyzones_editor_hotkey")]
        public KeyboardKeysProperty FancyzonesEditorHotkey { get; set; }

        [JsonPropertyName("fancyzones_excluded_apps")]
        public StringProperty FancyzonesExcludedApps { get; set; }

        [JsonPropertyName("fancyzones_zoneBorderColor")]
        public StringProperty FancyzonesBorderColor { get; set; }

        [JsonPropertyName("fancyzones_zoneColor")]
        public StringProperty FancyzonesInActiveColor { get; set; }

        // converts the current to a json string.
        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
