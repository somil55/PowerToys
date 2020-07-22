﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.PowerToys.Settings.UI.Lib
{
    public class ColorPickerSettings : BasePTModuleSettings
    {
        public const string ModuleName = "ColorPicker";

        [JsonPropertyName("properties")]
        public ColorPickerProperties Properties { get; set; }

        public ColorPickerSettings()
        {
            Properties = new ColorPickerProperties();
            Version = "1";
            Name = ModuleName;
        }

        public virtual void Save()
        {
            // Save settings to file
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            SettingsUtils.SaveSettings(JsonSerializer.Serialize(this, options), ModuleName);
        }
    }
}
