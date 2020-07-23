using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace PowerToysTests
{
    [TestClass]
    public class FancyZonesEditorOpeningTests : FancyZonesEditor
    {
        void RemoveSettingsFile()
        {
            File.Delete(_zoneSettingsPath);
        }

        void RemoveSettingsFolder()
        {
            Directory.Delete(_settingsFolderPath, true);
        }

        void CreateEmptySettingsFile()
        {
            string zoneSettings = "";
            File.WriteAllText(_zoneSettingsPath, zoneSettings);
        }

        void CreateDefaultSettingsFile()
        {
            string zoneSettings = "{\"app-zone-history\":[],\"devices\":[],\"custom-zone-sets\":[]}";
            File.WriteAllText(_zoneSettingsPath, zoneSettings);
        }

        void CreateValidSettingsFile()
        {
            string zoneSettings = "{\"app-zone-history\":[{\"app-path\":\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\Extensions\\TestPlatform\\testhost.exe\",\"zone-index\":3,\"device-id\":\"DELA026#5&10a58c63&0&UID16777488_1920_1200_{39B25DD2-130D-4B5D-8851-4791D66B1539}\",\"zoneset-uuid\":\"{D13ABB6D-7721-4176-9647-C8C0836D99CC}\"}],\"devices\":[{\"device-id\":\"DELA026#5&10a58c63&0&UID16777488_1920_1200_{39B25DD2-130D-4B5D-8851-4791D66B1539}\",\"active-zoneset\":{\"uuid\":\"{D13ABB6D-7721-4176-9647-C8C0836D99CC}\",\"type\":\"columns\"},\"editor-show-spacing\":true,\"editor-spacing\":16,\"editor-zone-count\":3}],\"custom-zone-sets\":[]}";
            File.WriteAllText(_zoneSettingsPath, zoneSettings);
        }

        void CreateValidSettingsFileWithUtf8()
        {
            string zoneSettings = "{\"app-zone-history\":[{\"app-path\":\"C:\\Program Files (x86)\\йцукен\\testhost.exe\",\"zone-index\":3,\"device-id\":\"DELA026#5&10a58c63&0&UID16777488_1920_1200_{39B25DD2-130D-4B5D-8851-4791D66B1539}\",\"zoneset-uuid\":\"{D13ABB6D-7721-4176-9647-C8C0836D99CC}\"}],\"devices\":[{\"device-id\":\"DELA026#5&10a58c63&0&UID16777488_1920_1200_{39B25DD2-130D-4B5D-8851-4791D66B1539}\",\"active-zoneset\":{\"uuid\":\"{D13ABB6D-7721-4176-9647-C8C0836D99CC}\",\"type\":\"columns\"},\"editor-show-spacing\":true,\"editor-spacing\":16,\"editor-zone-count\":3}],\"custom-zone-sets\":[]}";
            File.WriteAllText(_zoneSettingsPath, zoneSettings);
        }

        void CreateInvalidSettingsFile()
        {
            string zoneSettings = "{\"app-zone-history\":[{\"app-path\":\"C:\\Program Files (x86)\\Microsoft Visual Studio\\testhost.exe\",\"zone-index\":3,\"device-id\":\"wrong-device-id\",\"zoneset-uuid\":\"{D13ABB6D-invalid-uuid-C8C0836D99CC}\"}],\"devices\":[{\"device-id\":\"DELA026#5&10a58c63&0&UID16777488_1920_1200_{39B25DD2-130D-4B5D-8851-4791D66B1539}\",\"active-zoneset\":{\"uuid\":\"{D13ABB6D-7721-4176-9647-C8C0836D99CC}\",\"type\":\"columns\"},\"editor-show-spacing\":true,\"editor-spacing\":16,\"editor-zone-count\":3}],\"custom-zone-sets\":[]}";
            File.WriteAllText(_zoneSettingsPath, zoneSettings);
        }

        void CreateCroppedSettingsFile()
        {
            string zoneSettings = "{\"app-zone-history\":[],\"devices\":[],\"custom-zone-sets\":[{\"uuid\":\"{8BEC7183-C90E-4D41-AD1C-1AC2BC4760BA}\",\"name\":\"";
            File.WriteAllText(_zoneSettingsPath, zoneSettings);
        }

        void TestEditorOpened()
        {
            WindowsElement errorMessage = null;
            try
            {
                errorMessage = WaitElementByName("FancyZones Editor Exception Handler");
                if (errorMessage != null)
                {
                    errorMessage.FindElementByName("OK").Click();
                }
            }
            catch (OpenQA.Selenium.WebDriverException)
            {
                //no error message, it's ok
            }

            try
            {
                editorWindow = session.FindElementByXPath("//Window[@Name=\"FancyZones Editor\"]");
            }
            catch (OpenQA.Selenium.WebDriverException)
            {
            }

            Assert.IsNotNull(editorWindow);
            Assert.IsNull(errorMessage);
        }

        void OpenEditorBySettingsButton()
        {
            OpenSettings();
            OpenFancyZonesSettings();

            WindowsElement editorButton = session.FindElementByXPath("//Button[@Name=\"Edit zones\"]");
            Assert.IsNotNull(editorButton);

            editorButton.Click();
            TestEditorOpened();
        }

        void OpenEditorByHotkey()
        {
            new Actions(session).KeyDown(OpenQA.Selenium.Keys.Command).SendKeys("`").KeyUp(OpenQA.Selenium.Keys.Command).Perform();
            TestEditorOpened();
        }

        [TestMethod]
        public void OpenEditorBySettingsButtonNoSettings()
        {
            RemoveSettingsFile();
            OpenEditorBySettingsButton();
        }

        [TestMethod]
        public void OpenEditorBySettingsButtonNoSettingsFolder()
        {
            /*
            if (isPowerToysLaunched)
            {
                ExitPowerToys();
            }
            RemoveSettingsFolder();
            LaunchPowerToys();
            */

            RemoveSettingsFolder();
            OpenEditorBySettingsButton();
        }

        [TestMethod]
        public void OpenEditorBySettingsButtonEmptySettings()
        {
            CreateEmptySettingsFile();
            OpenEditorBySettingsButton();
        }

        [TestMethod]
        public void OpenEditorBySettingsButtonDefaultSettings()
        {
            CreateDefaultSettingsFile();
            OpenEditorBySettingsButton();
        }

        [TestMethod]
        public void OpenEditorBySettingsButtonValidSettings()
        {
            CreateValidSettingsFile();
            OpenEditorBySettingsButton();
        }

        [TestMethod]
        public void OpenEditorBySettingsButtonValidUtf8Settings()
        {
            CreateValidSettingsFileWithUtf8();
            OpenEditorBySettingsButton();
        }

        [TestMethod]
        public void OpenEditorBySettingsButtonInvalidSettings()
        {
            CreateInvalidSettingsFile();
            OpenEditorBySettingsButton();
        }

        [TestMethod]
        public void OpenEditorBySettingsButtonCroppedSettings()
        {
            CreateCroppedSettingsFile();
            OpenEditorBySettingsButton();
        }

        [TestMethod]
        public void OpenEditorByHotkeyNoSettings()
        {
            RemoveSettingsFile();
            OpenEditorByHotkey();
        }

        [TestMethod]
        public void OpenEditorByHotkeyNoSettingsFolder()
        {
            /*
            if (isPowerToysLaunched)
            {
                ExitPowerToys();
            }
            RemoveSettingsFolder();
            LaunchPowerToys();
            */
            RemoveSettingsFolder();
            OpenEditorByHotkey();
        }

        [TestMethod]
        public void OpenEditorByHotkeyEmptySettings()
        {
            CreateEmptySettingsFile();
            OpenEditorByHotkey();
        }

        [TestMethod]
        public void OpenEditorByHotkeyDefaultSettings()
        {
            CreateDefaultSettingsFile();
            OpenEditorByHotkey();
        }

        [TestMethod]
        public void OpenEditorByHotkeyValidSettings()
        {
            CreateValidSettingsFile();
            OpenEditorByHotkey();
        }

        [TestMethod]
        public void OpenEditorByHotkeyValidUtf8Settings()
        {
            CreateValidSettingsFileWithUtf8();
            OpenEditorByHotkey();
        }

        [TestMethod]
        public void OpenEditorByHotkeyInvalidSettings()
        {
            CreateInvalidSettingsFile();
            OpenEditorByHotkey();
        }

        [TestMethod]
        public void OpenEditorByHotkeyCroppedSettings()
        {
            CreateCroppedSettingsFile();
            OpenEditorByHotkey();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Setup(context, false);
            if (session == null)
                return;

            ResetDefaultFancyZonesSettings(true);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            ExitPowerToys();
            TearDown();
        }

        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanup()
        {
            CloseEditor();

            if (!Directory.Exists(_settingsFolderPath))
            {
                Directory.CreateDirectory(_settingsFolderPath);
            }
        }
    }
}