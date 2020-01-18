// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace Common
{
    /// <summary>
    /// This is test custom control to test the implementation.
    /// </summary>
    public class CustomPreviewHandlerControl : FormPreviewHandlerControl
    {
        /// <summary>
        /// Start the preview on the Control.
        /// </summary>
        /// <param name="dataSource">Path to the file.</param>
        public override void DoPreview(IStream dataSource)
        {
            string filename;
            if (dataSource is FileStream)
            {
                filename = ((FileStream)dataSource).Name;
            }

            base.DoPreview(dataSource);
            this.InvokeOnControlThread(() =>
            {
                string filePath = ((FileStream)dataSource).Name;
                WebBrowser browser = new WebBrowser
                {
                    DocumentText = "Test v7.212 " + filePath,
                };
                browser.Navigate(filePath);
                browser.Dock = DockStyle.Fill;
                browser.IsWebBrowserContextMenuEnabled = false;
                this.Controls.Add(browser);
                base.DoPreview(dataSource);
            });
        }
    }
}
