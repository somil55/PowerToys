﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wox.Infrastructure.Logger;
using Wox.Infrastructure.Storage;
using Wox.Plugin;

namespace Wox.Infrastructure.Image
{
    public static class ImageLoader
    {
        private static readonly ImageCache ImageCache = new ImageCache();
        private static BinaryStorage<Dictionary<string, int>> _storage;
        private static readonly ConcurrentDictionary<string, string> GuidToKey = new ConcurrentDictionary<string, string>();
        private static IImageHashGenerator _hashGenerator;
        public static string ErrorIconPath;
        public static string DefaultIconPath;

        private static readonly string[] ImageExtensions =
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".gif",
            ".bmp",
            ".tiff",
            ".ico"
        };


        public static void Initialize(Theme theme)
        {
            _storage = new BinaryStorage<Dictionary<string, int>>("Image");
            _hashGenerator = new ImageHashGenerator();
            ImageCache.SetUsageAsDictionary(_storage.TryLoad(new Dictionary<string, int>()));

            // Todo : Add error and default icon specific to each theme
            foreach (var icon in new[] { Constant.DefaultIcon, Constant.ErrorIcon })
            {
                ImageSource img = new BitmapImage(new Uri(icon));
                img.Freeze();
                ImageCache[icon] = img;
            }
            UpdateIconPath(theme);
            Task.Run(() =>
            {
                Stopwatch.Normal("|ImageLoader.Initialize|Preload images cost", () =>
                {
                    ImageCache.Usage.AsParallel().ForAll(x =>
                    {
                        Load(x.Key);
                    });
                });
                Log.Info($"|ImageLoader.Initialize|Number of preload images is <{ImageCache.Usage.Count}>, Images Number: {ImageCache.CacheSize()}, Unique Items {ImageCache.UniqueImagesInCache()}");
            });
        }

        public static void Save()
        {
            ImageCache.Cleanup();
            _storage.Save(ImageCache.GetUsageAsDictionary());
        }

        //Todo : Update it with icons specific to each theme.
        public static void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                ErrorIconPath = Constant.ErrorIcon;
                DefaultIconPath = Constant.DefaultIcon;
            }
            else
            {
                ErrorIconPath = Constant.ErrorIcon;
                DefaultIconPath = Constant.DefaultIcon;
            }
        }

        private class ImageResult
        {
            public ImageResult(ImageSource imageSource, ImageType imageType)
            {
                ImageSource = imageSource;
                ImageType = imageType;
            }

            public ImageType ImageType { get; }
            public ImageSource ImageSource { get; }
        }

        private enum ImageType
        {
            File,
            Folder,
            Data,
            ImageFile,
            Error,
            Cache
        }

        private static ImageResult LoadInternal(string path, bool loadFullImage = false)
        {
            ImageSource image;
            ImageType type = ImageType.Error;
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return new ImageResult(ImageCache[ErrorIconPath], ImageType.Error);
                }
                if (ImageCache.ContainsKey(path))
                {
                    return new ImageResult(ImageCache[path], ImageType.Cache);
                }

                if (path.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    var imageSource = new BitmapImage(new Uri(path));
                    imageSource.Freeze();
                    return new ImageResult(imageSource, ImageType.Data);
                }

                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(Constant.ProgramDirectory, "Images", Path.GetFileName(path));
                }

                if (Directory.Exists(path))
                {
                    /* Directories can also have thumbnails instead of shell icons.
                     * Generating thumbnails for a bunch of folders while scrolling through
                     * results from Everything makes a big impact on performance and 
                     * Wox responsibility. 
                     * - Solution: just load the icon
                     */
                    type = ImageType.Folder;
                    image = WindowsThumbnailProvider.GetThumbnail(path, Constant.ThumbnailSize,
                        Constant.ThumbnailSize, ThumbnailOptions.IconOnly);

                }
                else if (File.Exists(path))
                {
                    var extension = Path.GetExtension(path).ToLower();
                    if (ImageExtensions.Contains(extension))
                    {
                        type = ImageType.ImageFile;
                        if (loadFullImage)
                        {
                            image = LoadFullImage(path);
                        }
                        else
                        {
                            /* Although the documentation for GetImage on MSDN indicates that 
                             * if a thumbnail is available it will return one, this has proved to not
                             * be the case in many situations while testing. 
                             * - Solution: explicitly pass the ThumbnailOnly flag
                             */
                            image = WindowsThumbnailProvider.GetThumbnail(path, Constant.ThumbnailSize,
                                Constant.ThumbnailSize, ThumbnailOptions.ThumbnailOnly);
                        }
                    }
                    else
                    {
                        type = ImageType.File;
                        image = WindowsThumbnailProvider.GetThumbnail(path, Constant.ThumbnailSize,
                            Constant.ThumbnailSize, ThumbnailOptions.None);
                    }
                }
                else
                {
                    image = ImageCache[ErrorIconPath];
                    path = ErrorIconPath;
                }

                if (type != ImageType.Error)
                {
                    image.Freeze();
                }
            }
            catch (System.Exception e)
            {
                Log.Exception($"|ImageLoader.Load|Failed to get thumbnail for {path}", e);
                type = ImageType.Error;
                image = ImageCache[ErrorIconPath];
                ImageCache[path] = image;
            }
            return new ImageResult(image, type);
        }

        private static bool EnableImageHash = true;

        public static ImageSource Load(string path, bool loadFullImage = false)
        {
            var imageResult = LoadInternal(path, loadFullImage);

            var img = imageResult.ImageSource;
            if (imageResult.ImageType != ImageType.Error && imageResult.ImageType != ImageType.Cache)
            { // we need to get image hash
                string hash = EnableImageHash ? _hashGenerator.GetHashFromImage(img) : null;
                if (hash != null)
                {
                    int ImageCacheValue;
                    if (GuidToKey.TryGetValue(hash, out string key))
                    { // image already exists
                        if (ImageCache.Usage.TryGetValue(path, out ImageCacheValue))
                        {
                            img = ImageCache[key];
                        }
                    }
                    else
                    { // new guid
                        GuidToKey[hash] = path;
                    }
                }

                // update cache
                ImageCache[path] = img;
            }


            return img;
        }

        private static BitmapImage LoadFullImage(string path)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path);
            image.EndInit();
            return image;
        }
    }
}