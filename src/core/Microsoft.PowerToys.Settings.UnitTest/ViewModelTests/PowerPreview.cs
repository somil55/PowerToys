﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Text.Json;
using Microsoft.PowerToys.Settings.UI.Lib;
using Microsoft.PowerToys.Settings.UI.ViewModels;
using Microsoft.PowerToys.Settings.UI.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ViewModelTests
{
    [TestClass]
    public class PowerPreview
    {
        public const string Module = "File Explorer";

        [TestInitialize]
        public void Setup()
        {
            // initialize creation of test settings file.
            GeneralSettings generalSettings = new GeneralSettings();
            PowerPreviewSettings powerpreview = new PowerPreviewSettings();

            SettingsUtils.SaveSettings(generalSettings.ToJsonString());
            SettingsUtils.SaveSettings(powerpreview.ToJsonString(), powerpreview.Name);
        }

        [TestCleanup]
        public void CleanUp()
        {
            // delete folder created.
            string generalSettings_file_name = string.Empty;
            if (SettingsUtils.SettingsFolderExists(generalSettings_file_name))
            {
                DeleteFolder(generalSettings_file_name);
            }

            if (SettingsUtils.SettingsFolderExists(Module))
            {
                DeleteFolder(Module);
            }
        }

        public void DeleteFolder(string powertoy)
        {
            Directory.Delete(Path.Combine(SettingsUtils.LocalApplicationDataFolder(), $"Microsoft\\PowerToys\\{powertoy}"), true);
        }

        [TestMethod]
        public void SVGRenderIsEnabled_ShouldPrevHandler_WhenSuccessful()
        {
            // arrange
            PowerPreviewViewModel viewModel = new PowerPreviewViewModel();

            // Assert
            ShellPage.DefaultSndMSGCallback = msg =>
            {
                SndModuleSettings<SndPowerPreviewSettings> snd = JsonSerializer.Deserialize<SndModuleSettings<SndPowerPreviewSettings>>(msg);
                Assert.IsTrue(snd.powertoys.FileExplorerPreviewSettings.Properties.EnableSvgPreview);
            };

            // act
            viewModel.SVGRenderIsEnabled = true;
        }

        [TestMethod]
        public void SVGThumbnailIsEnabled_ShouldPrevHandler_WhenSuccessful()
        {
            // arrange
            PowerPreviewViewModel viewModel = new PowerPreviewViewModel();

            // Assert
            ShellPage.DefaultSndMSGCallback = msg =>
            {
                SndModuleSettings<SndPowerPreviewSettings> snd = JsonSerializer.Deserialize<SndModuleSettings<SndPowerPreviewSettings>>(msg);
                Assert.IsTrue(snd.powertoys.FileExplorerPreviewSettings.Properties.EnableSvgThumbnail);
            };

            // act
            viewModel.SVGThumbnailIsEnabled = true;
        }

        [TestMethod]
        public void MDRenderIsEnabled_ShouldPrevHandler_WhenSuccessful()
        {
            // arrange
            PowerPreviewViewModel viewModel = new PowerPreviewViewModel();

            // Assert
            ShellPage.DefaultSndMSGCallback = msg =>
            {
                SndModuleSettings<SndPowerPreviewSettings> snd = JsonSerializer.Deserialize<SndModuleSettings<SndPowerPreviewSettings>>(msg);
                Assert.IsTrue(snd.powertoys.FileExplorerPreviewSettings.Properties.EnableMdPreview);
            };

            // act
            viewModel.MDRenderIsEnabled = true;
        }
    }
}
