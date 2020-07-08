using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Wox.Infrastructure.Logger;
using Wox.Plugin;
using Microsoft.Plugin.Indexer.SearchHelper;
using System.Windows.Input;
using System.Reflection;
using Wox.Infrastructure;

namespace Microsoft.Plugin.Indexer
{
    internal class ContextMenuLoader : IContextMenu
    {
        private readonly PluginInitContext _context;

        public enum ResultType
        {
            Folder,
            File
        }

        public ContextMenuLoader(PluginInitContext context)
        {
            _context = context;
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            var contextMenus = new List<ContextMenuResult>();
            if (selectedResult.ContextData is SearchResult record)
            {
                ResultType type = Path.HasExtension(record.Path) ? ResultType.File : ResultType.Folder;

                if (type == ResultType.File)
                {
                    contextMenus.Add(CreateOpenContainingFolderResult(record));
                }

                contextMenus.Add(new ContextMenuResult
                {
                    PluginName = Assembly.GetExecutingAssembly().GetName().Name,
                    Title = _context.API.GetTranslation("Microsoft_plugin_indexer_copy_path"),
                    Glyph = "\xE8C8",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.C, 
                    AcceleratorModifiers = ModifierKeys.Control,

                    Action = (context) =>
                    {
                        try
                        {
                            Clipboard.SetText(record.Path);
                            return true;
                        }
                        catch (Exception e)
                        {
                            var message = "Fail to set text in clipboard";
                            LogException(message, e);
                            _context.API.ShowMsg(message);
                            return false;
                        }
                    }
                });
                contextMenus.Add(new ContextMenuResult
                {
                    PluginName = Assembly.GetExecutingAssembly().GetName().Name,
                    Title = _context.API.GetTranslation("Microsoft_plugin_indexer_open_in_console"),
                    Glyph = "\xE756",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.C,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,

                    Action = (context) =>
                    {
                        try
                        {
                            if (type == ResultType.File)
                            {
                                Helper.OpenInConsole(Path.GetDirectoryName(record.Path));
                            }
                            else
                            {
                                Helper.OpenInConsole(record.Path);
                            }

                            return true;
                        }
                        catch (Exception e)
                        {
                            Log.Exception($"|Microsoft.Plugin.Indexer.ContextMenuLoader.LoadContextMenus| Failed to open {record.Path} in console, {e.Message}", e);
                            return false;
                        }
                    }
                });
            }

            return contextMenus;
        }

        private ContextMenuResult CreateOpenContainingFolderResult(SearchResult record)
        {
            return new ContextMenuResult
            {
                PluginName = Assembly.GetExecutingAssembly().GetName().Name,
                Title = _context.API.GetTranslation("Microsoft_plugin_indexer_open_containing_folder"),
                Glyph = "\xE838",
                FontFamily = "Segoe MDL2 Assets",
                AcceleratorKey = Key.E,
                AcceleratorModifiers = (ModifierKeys.Control | ModifierKeys.Shift),
                Action = _ =>
                {
                    try
                    {
                        Process.Start("explorer.exe", $" /select,\"{record.Path}\"");
                    }
                    catch(Exception e)
                    {
                        var message = $"Fail to open file at {record.Path}";
                        LogException(message, e);
                        _context.API.ShowMsg(message);
                        return false;
                    }

                    return true;
                },
            };
        }

        public void LogException(string message, Exception e)
        {
            Log.Exception($"|Microsoft.Plugin.Folder.ContextMenu|{message}", e);
        }
    }

}