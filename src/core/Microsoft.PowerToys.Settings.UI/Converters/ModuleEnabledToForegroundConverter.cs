﻿using Microsoft.PowerToys.Settings.UI.Lib;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Microsoft.PowerToys.Settings.UI.Converters
{
    public sealed class ModuleEnabledToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isEnabled = (bool)value;
            GeneralSettings generalSettings = SettingsUtils.GetSettings<GeneralSettings>(string.Empty);

            var defaultTheme = new Windows.UI.ViewManagement.UISettings();
            var uiTheme = defaultTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();

            string selectedTheme = generalSettings.Theme.ToLower();

            if (selectedTheme == "dark" || (selectedTheme == "system" && uiTheme == "#FF000000"))
            {
                // DARK
                if (isEnabled)
                {
                    return (SolidColorBrush)Application.Current.Resources["DarkForegroundBrush"];
                }
                else
                {
                    return (SolidColorBrush)Application.Current.Resources["DarkForegroundDisabledBrush"];
                }
            }
            else
            {
                // LIGHT
                if (isEnabled)
                {
                    return (SolidColorBrush)Application.Current.Resources["LightForegroundBrush"];
                }
                else
                {
                    return (SolidColorBrush)Application.Current.Resources["LightForegroundDisabledBrush"];
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
