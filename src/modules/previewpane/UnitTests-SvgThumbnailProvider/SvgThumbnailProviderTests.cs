﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.ComInterlop;
using SvgThumbnailProvider;

namespace SvgThumbnailProviderUnitTests
{
    [TestClass]
    public class SvgThumbnailProviderTests
    {
        [TestMethod]
        public void LoadSimpleSVG_ShouldReturnNonNullBitmap()
        {
            var svgBuilder = new StringBuilder();
            svgBuilder.AppendLine("<svg viewBox=\"0 0 100 100\" xmlns=\"http://www.w3.org/2000/svg\">");
            svgBuilder.AppendLine("\t<circle cx=\"50\" cy=\"50\" r=\"50\">");
            svgBuilder.AppendLine("\t</circle>");
            svgBuilder.AppendLine("</svg>");

            Bitmap thumbnail = SvgThumbnailProvider.SvgThumbnailProvider.GetThumbnail(svgBuilder.ToString(), 256);
            Assert.IsTrue(thumbnail != null);
        }

        [TestMethod]
        public void CheckBlockedElements_ShouldReturnNullBitmap_IfBlockedElementsIsPresentInNestedLevel()
        {
            var svgBuilder = new StringBuilder();
            svgBuilder.AppendLine("<svg viewBox=\"0 0 100 100\" xmlns=\"http://www.w3.org/2000/svg\">");
            svgBuilder.AppendLine("\t<circle cx=\"50\" cy=\"50\" r=\"50\">");
            svgBuilder.AppendLine("\t\t<script>alert(\"valid-message\")</script>");
            svgBuilder.AppendLine("\t</circle>");
            svgBuilder.AppendLine("</svg>");

            Bitmap thumbnail = SvgThumbnailProvider.SvgThumbnailProvider.GetThumbnail(svgBuilder.ToString(), 256);
            Assert.IsTrue(thumbnail == null);
        }

        [TestMethod]
        public void CheckNoSvg_ShouldReturnNullBitmap()
        {
            var svgBuilder = new StringBuilder();
            svgBuilder.AppendLine("<p>foo</p>");

            Bitmap thumbnail = SvgThumbnailProvider.SvgThumbnailProvider.GetThumbnail(svgBuilder.ToString(), 256);
            Assert.IsTrue(thumbnail == null);
        }

        [TestMethod]
        public void CheckNoSvgEmptyString_ShouldReturnNullBitmap()
        {
            Bitmap thumbnail = SvgThumbnailProvider.SvgThumbnailProvider.GetThumbnail("", 256);
            Assert.IsTrue(thumbnail == null);
        }

        [TestMethod]
        public void CheckNoSvgNullString_ShouldReturnNullBitmap()
        {
            Bitmap thumbnail = SvgThumbnailProvider.SvgThumbnailProvider.GetThumbnail(null, 256);
            Assert.IsTrue(thumbnail == null);
        }

        [TestMethod]
        public void CheckZeroSizedThumbnail_ShouldReturnNullBitmap()
        {
            string content = "<svg></svg>";
            Bitmap thumbnail = SvgThumbnailProvider.SvgThumbnailProvider.GetThumbnail(content, 0);
            Assert.IsTrue(thumbnail == null);
        }

        [TestMethod]
        public void CheckBlockedElements_ShouldReturnBitmap_HTMLWrapped()
        {
            var svgBuilder = new StringBuilder();
            svgBuilder.AppendLine("<html>");
            svgBuilder.AppendLine("<head>");
            svgBuilder.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\">");
            svgBuilder.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html\" charset=\"utf-8\">");
            svgBuilder.AppendLine("</head>");
            svgBuilder.AppendLine("<body>");
            svgBuilder.AppendLine("<svg viewBox=\"0 0 100 100\" xmlns=\"http://www.w3.org/2000/svg\">");
            svgBuilder.AppendLine("<circle cx=\"50\" cy=\"50\" r=\"50\">");
            svgBuilder.AppendLine("</circle>");
            svgBuilder.AppendLine("</svg>");
            svgBuilder.AppendLine("</body>");
            svgBuilder.AppendLine("</html>");

            Bitmap thumbnail = SvgThumbnailProvider.SvgThumbnailProvider.GetThumbnail(svgBuilder.ToString(), 256);
            Assert.IsTrue(thumbnail != null);
        }

        [TestMethod]
        public void GetThumbnail_ValidStreamSVG()
        {
            var svgBuilder = new StringBuilder();
            svgBuilder.AppendLine("<svg viewBox=\"0 0 100 100\" xmlns=\"http://www.w3.org/2000/svg\">");
            svgBuilder.AppendLine("<circle cx=\"50\" cy=\"50\" r=\"50\">");
            svgBuilder.AppendLine("</circle>");
            svgBuilder.AppendLine("</svg>");

            SvgThumbnailProvider.SvgThumbnailProvider provider = new SvgThumbnailProvider.SvgThumbnailProvider();

            provider.Initialize(GetMockStream(svgBuilder.ToString()), 0);

            IntPtr bitmap;
            WTS_ALPHATYPE alphaType;
            provider.GetThumbnail(256, out bitmap, out alphaType);

            Assert.IsTrue(bitmap != IntPtr.Zero);
            Assert.IsTrue(alphaType == WTS_ALPHATYPE.WTSAT_RGB);
        }

        [TestMethod]
        public void GetThumbnail_ValidStreamHTML()
        {
            var svgBuilder = new StringBuilder();
            svgBuilder.AppendLine("<html>");
            svgBuilder.AppendLine("<head>");
            svgBuilder.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\">");
            svgBuilder.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html\" charset=\"utf-8\">");
            svgBuilder.AppendLine("</head>");
            svgBuilder.AppendLine("<body>");
            svgBuilder.AppendLine("<svg viewBox=\"0 0 100 100\" xmlns=\"http://www.w3.org/2000/svg\">");
            svgBuilder.AppendLine("<circle cx=\"50\" cy=\"50\" r=\"50\">");
            svgBuilder.AppendLine("</circle>");
            svgBuilder.AppendLine("</svg>");
            svgBuilder.AppendLine("</body>");
            svgBuilder.AppendLine("</html>");

            SvgThumbnailProvider.SvgThumbnailProvider provider = new SvgThumbnailProvider.SvgThumbnailProvider();

            provider.Initialize(GetMockStream(svgBuilder.ToString()), 0);

            IntPtr bitmap;
            WTS_ALPHATYPE alphaType;
            provider.GetThumbnail(256, out bitmap, out alphaType);

            Assert.IsTrue(bitmap != IntPtr.Zero);
            Assert.IsTrue(alphaType == WTS_ALPHATYPE.WTSAT_RGB);
        }

        private IStream GetMockStream(string streamData)
        {
            var mockStream = new Mock<IStream>();
            var streamBytes = Encoding.UTF8.GetBytes(streamData);

            var streamMock = new Mock<IStream>();
            var firstCall = true;
            streamMock
                .Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<IntPtr>()))
                .Callback<byte[], int, IntPtr>((buffer, countToRead, bytesReadPtr) =>
                {
                    if (firstCall)
                    {
                        Array.Copy(streamBytes, 0, buffer, 0, streamBytes.Length);
                        Marshal.WriteInt32(bytesReadPtr, streamBytes.Length);
                        firstCall = false;
                    }
                    else
                    {
                        Marshal.WriteInt32(bytesReadPtr, 0);
                    }
                });

            return streamMock.Object;
        }
    }
}
