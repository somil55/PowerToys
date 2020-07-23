﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Wox.Plugin;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace Microsoft.Plugin.Folder
{

    public partial class FileSystemSettings
    {
        private IPublicAPI woxAPI;
        private FolderSettings _settings;

        public FileSystemSettings(IPublicAPI woxAPI, FolderSettings settings)
        {
            this.woxAPI = woxAPI;
            InitializeComponent();
            _settings = settings ?? throw new ArgumentNullException(paramName:nameof(settings));
            lbxFolders.ItemsSource = _settings.FolderLinks;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedFolder = lbxFolders.SelectedItem as FolderLink;
            if (selectedFolder != null)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, woxAPI.GetTranslation("wox_plugin_folder_delete_folder_link"), selectedFolder.Path);

                if (MessageBox.Show(msg, string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _settings.FolderLinks.Remove(selectedFolder);
                    lbxFolders.Items.Refresh();
                }
            }
            else
            {
                string warning = woxAPI.GetTranslation("wox_plugin_folder_select_folder_link_warning");
                MessageBox.Show(warning);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedFolder = lbxFolders.SelectedItem as FolderLink;
            if (selectedFolder != null)
            {
                using (var folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.SelectedPath = selectedFolder.Path;
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        var link = _settings.FolderLinks.First(x => x.Path == selectedFolder.Path);
                        link.Path = folderBrowserDialog.SelectedPath;
                    }

                    lbxFolders.Items.Refresh();
                }
            }
            else
            {
                string warning = woxAPI.GetTranslation("wox_plugin_folder_select_folder_link_warning");
                MessageBox.Show(warning);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    var newFolder = new FolderLink
                    {
                        Path = folderBrowserDialog.SelectedPath
                    };

                    _settings.FolderLinks.Add(newFolder);
                }

                lbxFolders.Items.Refresh();
            }
        }

        private void lbxFolders_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Any())
            {
                foreach (string s in files)
                {
                    if (Directory.Exists(s))
                    {
                        var newFolder = new FolderLink
                        {
                            Path = s
                        };

                        _settings.FolderLinks.Add(newFolder);
                    }

                    lbxFolders.Items.Refresh();
                }
            }
        }

        private void lbxFolders_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
    }
}
