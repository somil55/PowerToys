﻿using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Wox.Plugin;
using Microsoft.Plugin.Folder;

namespace Wox.Test.Plugins
{
    class FolderPluginTest
    {
        [Test]
        public void ContextMenuLoader_ReturnContextMenuForFolderWithOpenInConsole_WhenLoadContextMenusIsCalled()
        {
            // Arrange 
            var mock = new Mock<IPublicAPI>();
            mock.Setup(api => api.GetTranslation(It.IsAny<string>())).Returns(It.IsAny<string>());
            var pluginInitContext = new PluginInitContext() { API = mock.Object };
            var contextMenuLoader = new ContextMenuLoader(pluginInitContext);
            var searchResult = new SearchResult() { Type = ResultType.Folder, FullPath = "C:/DummyFolder" };
            var result = new Result() { ContextData = searchResult };

            // Act
            List<ContextMenuResult> contextMenuResults = contextMenuLoader.LoadContextMenus(result);

            // Assert
            Assert.AreEqual(contextMenuResults.Count, 2);
            mock.Verify(x => x.GetTranslation("Microsoft_plugin_folder_copy_path"), Times.Once());
            mock.Verify(x => x.GetTranslation("Microsoft_plugin_folder_open_in_console"), Times.Once());
        }

        [Test]
        public void ContextMenuLoader_ReturnContextMenuForFileWithOpenInConsole_WhenLoadContextMenusIsCalled()
        {
            // Arrange 
            var mock = new Mock<IPublicAPI>();
            mock.Setup(api => api.GetTranslation(It.IsAny<string>())).Returns(It.IsAny<string>());
            var pluginInitContext = new PluginInitContext() { API = mock.Object };
            var contextMenuLoader = new ContextMenuLoader(pluginInitContext);
            var searchResult = new SearchResult() { Type = ResultType.File, FullPath = "C:/DummyFile.cs" };
            var result = new Result() { ContextData = searchResult };

            // Act
            List<ContextMenuResult> contextMenuResults = contextMenuLoader.LoadContextMenus(result);

            // Assert
            Assert.AreEqual(contextMenuResults.Count, 3);
            mock.Verify(x => x.GetTranslation("Microsoft_plugin_folder_open_containing_folder"), Times.Once());
            mock.Verify(x => x.GetTranslation("Microsoft_plugin_folder_copy_path"), Times.Once());
            mock.Verify(x => x.GetTranslation("Microsoft_plugin_folder_open_in_console"), Times.Once());
        }
    }
}
