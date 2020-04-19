﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using Wox.Core.Plugin;
using Wox.Infrastructure;
using Wox.Infrastructure.Hotkey;
using Wox.Infrastructure.Image;
using Wox.Infrastructure.Logger;
using Wox.Plugin;
using Windows.UI.Xaml.Media;


namespace Wox.ViewModel
{
    public class ResultViewModel : BaseModel
    {
        public List<ContextMenuItemViewModel> ContextMenuItems { get; set; }

        public ICommand LoadContextMenuCommand { get; set; }

        public bool IsSelected { get; set; }

        public int SelectedIndex { get; set; }

        const int NoSelectionIndex = -1;

        public ResultViewModel(Result result)
        {
            if (result != null)
            {
                Result = result;
            }
            SelectedIndex = NoSelectionIndex;
            LoadContextMenuCommand = new RelayCommand(LoadContextMenu);
        }
        public void LoadContextMenu(object sender=null)
        {
            var results = PluginManager.GetContextMenusForPlugin(Result);
            var newItems = new List<ContextMenuItemViewModel>();
            foreach (var r in results)
            {
                newItems.Add(new ContextMenuItemViewModel
                {
                    Title = r.Title,
                    Glyph = r.Glyph,
                    FontFamily = r.FontFamily,
                    Command = new RelayCommand(_ =>
                    {
                        bool hideWindow = r.Action != null && r.Action(new ActionContext
                        {
                            SpecialKeyState = GlobalHotkey.Instance.CheckModifiers()
                        });

                        if (hideWindow)
                        {
                            //TODO - Do we hide the window
                            // MainWindowVisibility = Visibility.Collapsed;
                        }
                    })
                });
            }

            ContextMenuItems = newItems;
        }

        public ImageSource Image
        {
            get
            {
                var imagePath = Result.IcoPath;
                if (string.IsNullOrEmpty(imagePath) && Result.Icon != null)
                {
                    try
                    {
                        return Result.Icon();
                    }
                    catch (Exception e)
                    {
                        Log.Exception($"|ResultViewModel.Image|IcoPath is empty and exception when calling Icon() for result <{Result.Title}> of plugin <{Result.PluginDirectory}>", e);
                        imagePath = Constant.ErrorIcon;
                    }
                }
                
                // will get here either when icoPath has value\icon delegate is null\when had exception in delegate
                return ImageLoader.Load(imagePath);
            }
        }

        //Returns false if we've already reached the last item.
        public bool SelectNextContextButton()
        {
            if(SelectedIndex == (ContextMenuItems.Count -1))
            {
                SelectedIndex = NoSelectionIndex;
                return false; 
            }

            SelectedIndex++;
            return true;
        }

        //Returns false if we've already reached the first item.
        public bool SelectPrevContextButton()
        {
            if (SelectedIndex == NoSelectionIndex)
            {
                return false;
            }

            SelectedIndex--;
            return true;
        }

        public void SelectLastContextButton()
        {
            SelectedIndex = ContextMenuItems.Count - 1;
        }

        public bool HasSelectedContextButton()
        {
            var isContextSelected = (SelectedIndex != NoSelectionIndex);
            return isContextSelected;
        }

        /// <summary>
        ///  Triggers the action on the selected context button
        /// </summary>
        /// <returns>False if there is nothing selected, oherwise true</returns>
        public bool ExecuteSelectedContextButton()
        {
            if (HasSelectedContextButton())
            {
                ContextMenuItems[SelectedIndex].Command.Execute(null);
                return true;
            }

            return false;
        }

        public Result Result { get; }

        public override bool Equals(object obj)
        {
            var r = obj as ResultViewModel;
            if (r != null)
            {
                return Result.Equals(r.Result);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Result.GetHashCode();
        }

        public override string ToString()
        {
            return Result.Title.ToString();
        }
    }
}
