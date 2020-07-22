﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.PowerToys.Settings.Telemetry;
using Microsoft.PowerToys.Telemetry;

namespace Microsoft.PowerToys.Settings.UI.Lib
{
    public class PowerPreviewProperties
    {
        private bool enableSvgPreview = true;

        [JsonPropertyName("svg-previewer-toggle-setting")]
        [JsonConverter(typeof(BoolPropertyJsonConverter))]
        public bool EnableSvgPreview
        {
            get => this.enableSvgPreview;
            set
            {
                if (value != this.enableSvgPreview)
                {
                    LogTelemetryEvent(value);
                    this.enableSvgPreview = value;
                }
            }
        }

        private bool enableSvgThumbnail = true;

        [JsonPropertyName("svg-thumbnail-toggle-setting")]
        [JsonConverter(typeof(BoolPropertyJsonConverter))]
        public bool EnableSvgThumbnail
        {
            get => this.enableSvgThumbnail;
            set
            {
                if (value != this.enableSvgThumbnail)
                {
                    LogTelemetryEvent(value);
                    this.enableSvgThumbnail = value;
                }
            }
        }

        private bool enableMdPreview = true;

        [JsonPropertyName("md-previewer-toggle-setting")]
        [JsonConverter(typeof(BoolPropertyJsonConverter))]
        public bool EnableMdPreview
        {
            get => this.enableMdPreview;
            set
            {
                if (value != this.enableMdPreview)
                {
                    LogTelemetryEvent(value);
                    this.enableMdPreview = value;
                }
            }
        }

        public PowerPreviewProperties()
        {

        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        private void LogTelemetryEvent(bool value, [CallerMemberName] string propertyName = null)
        {
            var dataEvent = new SettingsEnabledEvent()
            {
                Value = value,
                Name = propertyName,
            };
            PowerToysTelemetry.Log.WriteEvent(dataEvent);
        }
    }
}
