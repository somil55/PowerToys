﻿using PowerLauncher.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
using Wox.Core.Plugin;
using Wox.Infrastructure;
using Wox.Infrastructure.Image;
using Wox.Infrastructure.Logger;
using Wox.Plugin;

namespace PowerLauncher.ViewModel
{
    public class ResultViewModel : BaseModel
    {
        public enum ActivationType
        {
            Selection,
            Hover
        };

        public ObservableCollection<ContextMenuItemViewModel> ContextMenuItems { get; set; } = new ObservableCollection<ContextMenuItemViewModel>();

        public ICommand ActivateContextButtonsHoverCommand { get; set; }
        public ICommand ActivateContextButtonsSelectionCommand { get; set; }
        public ICommand DeactivateContextButtonsHoverCommand { get; set; }

        public ICommand DeactivateContextButtonsSelectionCommand { get; set; }

        public bool IsSelected { get; set; }

        public bool IsHovered { get; set; }

        public bool AreContextButtonsActive { get; set; }

        public int ContextMenuSelectedIndex { get; set; }

        const int NoSelectionIndex = -1;

        public ResultViewModel(Result result)
        {
            if (result != null)
            {
                Result = result;
            }
            
            ContextMenuSelectedIndex = NoSelectionIndex;
            LoadContextMenu();
            
            ActivateContextButtonsHoverCommand = new RelayCommand(ActivateContextButtonsHoverAction);
            ActivateContextButtonsSelectionCommand = new RelayCommand(ActivateContextButtonsSelectionAction);
            DeactivateContextButtonsHoverCommand = new RelayCommand(DeactivateContextButtonsHoverAction);
            DeactivateContextButtonsSelectionCommand = new RelayCommand(DeactivateContextButtonsSelectionAction);
        }

        private void ActivateContextButtonsHoverAction(object sender)
        {
            ActivateContextButtons(ActivationType.Hover);
        }

        private void ActivateContextButtonsSelectionAction(object sender)
        {
            ActivateContextButtons(ActivationType.Selection);
        }
        public void ActivateContextButtons(ActivationType activationType)
        {
            // Result does not contain any context menu items - we don't need to show the context menu ListView at all.
            if (ContextMenuItems.Count > 0)
            {
                AreContextButtonsActive = true;
            }
            else
            {
                AreContextButtonsActive = false;
            }

            if (activationType == ActivationType.Selection)
            {
                IsSelected = true;
                EnableContextMenuAcceleratorKeys();
            }
            else if (activationType == ActivationType.Hover)
            {
                IsHovered = true;
            }
        }


        private void DeactivateContextButtonsHoverAction(object sender)
        {
            DeactivateContextButtons(ActivationType.Hover);
        }

        private void DeactivateContextButtonsSelectionAction(object sender)
        {
            DeactivateContextButtons(ActivationType.Selection);
        }

        public void DeactivateContextButtons(ActivationType activationType)
        {
            if (activationType == ActivationType.Selection)
            {
                IsSelected = false;
                DisableContextMenuAcceleratorkeys();
            }
            else if (activationType == ActivationType.Hover)
            {
                IsHovered = false;
            }

            // Result does not contain any context menu items - we don't need to show the context menu ListView at all.
            if (ContextMenuItems?.Count > 0)
            {
                AreContextButtonsActive = IsSelected || IsHovered;
            }
            else
            {
                AreContextButtonsActive = false;
            }
        }


        public void LoadContextMenu()
        {
            var results = PluginManager.GetContextMenusForPlugin(Result);
            ContextMenuItems.Clear();
            foreach (var r in results)
            {
                ContextMenuItems.Add(new ContextMenuItemViewModel()
                {
                    PluginName = r.PluginName,
                    Title = r.Title,
                    Glyph = r.Glyph,
                    FontFamily = r.FontFamily,
                    AcceleratorKey = r.AcceleratorKey,
                    AcceleratorModifiers = r.AcceleratorModifiers,
                    Command = new RelayCommand(_ =>
                    {
                        bool hideWindow = r.Action != null && r.Action(new ActionContext
                        {
                            SpecialKeyState = KeyboardHelper.CheckModifiers()
                        });

                        if (hideWindow)
                        {
                            //TODO - Do we hide the window
                            // MainWindowVisibility = Visibility.Collapsed;
                        }
                    })
                });
            }
        }

        private void EnableContextMenuAcceleratorKeys()
        {
            foreach (var i in ContextMenuItems)
            {
                i.IsAcceleratorKeyEnabled = true;
            }
        }

        private void DisableContextMenuAcceleratorkeys()
        {
            foreach (var i in ContextMenuItems)
            {
                i.IsAcceleratorKeyEnabled = false;
            }
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
                        imagePath = ImageLoader.ErrorIconPath;
                    }
                }

                // will get here either when icoPath has value\icon delegate is null\when had exception in delegate
                return ImageLoader.Load(imagePath);
            }
        }

        //Returns false if we've already reached the last item.
        public bool SelectNextContextButton()
        {
            if (ContextMenuSelectedIndex == (ContextMenuItems.Count - 1))
            {
                ContextMenuSelectedIndex = NoSelectionIndex;
                return false;
            }

            ContextMenuSelectedIndex++;
            return true;
        }

        //Returns false if we've already reached the first item.
        public bool SelectPrevContextButton()
        {
            if (ContextMenuSelectedIndex == NoSelectionIndex)
            {
                return false;
            }

            ContextMenuSelectedIndex--;
            return true;
        }

        public void SelectLastContextButton()
        {
            ContextMenuSelectedIndex = ContextMenuItems.Count - 1;
        }

        public bool HasSelectedContextButton()
        {
            var isContextSelected = (ContextMenuSelectedIndex != NoSelectionIndex);
            return isContextSelected;
        }

        /// <summary>
        ///  Triggers the action on the selected context button
        /// </summary>
        /// <returns>False if there is nothing selected, otherwise true</returns>
        public bool ExecuteSelectedContextButton()
        {
            if (HasSelectedContextButton())
            {
                ContextMenuItems[ContextMenuSelectedIndex].Command.Execute(null);
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
            var display = String.IsNullOrEmpty(Result.QueryTextDisplay) ? Result.Title : Result.QueryTextDisplay;
            return display;
        }
    }
}
