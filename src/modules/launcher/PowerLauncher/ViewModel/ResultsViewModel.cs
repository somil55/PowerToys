﻿#define DEBUG
using PowerLauncher.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Wox.Infrastructure.UserSettings;
using Wox.Plugin;

namespace PowerLauncher.ViewModel
{
    public class ResultsViewModel : BaseModel
    {
        #region Private Fields

        public ResultCollection Results { get; }

        private readonly object _collectionLock = new object();
        private readonly Settings _settings;
        // private int MaxResults => _settings?.MaxResultsToShow ?? 6;

        public ResultsViewModel()
        {
            Results = new ResultCollection();
            BindingOperations.EnableCollectionSynchronization(Results, _collectionLock);
        }
        public ResultsViewModel(Settings settings) : this()
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_settings.MaxResultsToShow))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnPropertyChanged(nameof(MaxHeight));
                    });
                }
            };
        }

        #endregion

        #region Properties

        public int MaxHeight
        {
            get
            {
                return _settings.MaxResultsToShow * 75;
            }
        }
        public int SelectedIndex { get; set; }

        private ResultViewModel _selectedItem;
        public ResultViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                //value can be null when selecting an item in a virtualized list
                if (value != null)
                {
                    if (_selectedItem != null)
                    {
                        _selectedItem.DeactivateContextButtons(ResultViewModel.ActivationType.Selection);
                    }

                    _selectedItem = value;
                    _selectedItem.ActivateContextButtons(ResultViewModel.ActivationType.Selection);
                }
                else
                {
                    _selectedItem = value;
                }
            }
        }



        public Thickness Margin { get; set; }
        public Visibility Visibility { get; set; } = Visibility.Hidden;

        #endregion

        #region Private Methods

        private static int InsertIndexOf(int newScore, IList<ResultViewModel> list)
        {
            int index = 0;
            for (; index < list.Count; index++)
            {
                var result = list[index];
                if (newScore > result.Result.Score)
                {
                    break;
                }
            }
            return index;
        }

        private int NewIndex(int i)
        {
            var n = Results.Count;
            if (n > 0)
            {
                i = (n + i) % n;
                return i;
            }
            else
            {
                // SelectedIndex returns -1 if selection is empty.
                return -1;
            }
        }


        #endregion

        #region Public Methods

        public void SelectNextResult()
        {
            SelectedIndex = NewIndex(SelectedIndex + 1);
        }

        public void SelectPrevResult()
        {
            SelectedIndex = NewIndex(SelectedIndex - 1);
        }

        public void SelectNextPage()
        {
            SelectedIndex = NewIndex(SelectedIndex + _settings.MaxResultsToShow);
        }

        public void SelectPrevPage()
        {
            SelectedIndex = NewIndex(SelectedIndex - _settings.MaxResultsToShow);
        }

        public void SelectFirstResult()
        {
            SelectedIndex = NewIndex(0);
        }

        public void Clear()
        {
            Results.Clear();
        }

        public void RemoveResultsExcept(PluginMetadata metadata)
        {
            Results.RemoveAll(r => r.Result.PluginID != metadata.ID);
        }

        public void RemoveResultsFor(PluginMetadata metadata)
        {
            Results.RemoveAll(r => r.Result.PluginID == metadata.ID);
        }

        public void SelectNextTabItem()
        {
            //Do nothing if there is no selected item or we've selected the next context button
            if (!SelectedItem?.SelectNextContextButton() ?? true)
            {
                SelectNextResult();
            }
        }

        public void SelectPrevTabItem()
        {
            //Do nothing if there is no selected item or we've selected the previous context button
            if (!SelectedItem?.SelectPrevContextButton() ?? true)
            {
                //Tabbing backwards should highlight the last item of the previous row
                SelectPrevResult();
                SelectedItem.SelectLastContextButton();
            }
        }

        public void SelectNextContextMenuItem()
        {
            if(SelectedItem != null)
            {
                if(!SelectedItem.SelectNextContextButton())
                {
                    SelectedItem.SelectLastContextButton();
                }
            }
        }

        public void SelectPreviousContextMenuItem()
        {
            if (SelectedItem != null)
            {
                SelectedItem.SelectPrevContextButton();
            }
        }

        public bool IsContextMenuItemSelected()
        {
            if (SelectedItem != null && SelectedItem.ContextMenuSelectedIndex != ResultViewModel.NoSelectionIndex)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Add new results to ResultCollection
        /// </summary>
        public void AddResults(List<Result> newRawResults, string resultId)
        {
            Results.RemoveAll(r => r.Result.PluginID == resultId);
            var newResults = newRawResults.Select(r => new ResultViewModel(r)).ToList();
            Results.AddRange(newResults);
        }
        #endregion

        #region FormattedText Dependency Property
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
            "FormattedText",
            typeof(Inline),
            typeof(ResultsViewModel),
            new PropertyMetadata(null, FormattedTextPropertyChanged));

        public static void SetFormattedText(DependencyObject textBlock, IList<int> value)
        {
            if (textBlock != null)
            {
                textBlock.SetValue(FormattedTextProperty, value);
            }
        }

        public static Inline GetFormattedText(DependencyObject textBlock)
        {
            return (Inline)textBlock?.GetValue(FormattedTextProperty);
        }

        private static void FormattedTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = d as TextBlock;
            if (textBlock == null) return;

            var inline = (Inline)e.NewValue;

            textBlock.Inlines.Clear();
            if (inline == null) return;

            textBlock.Inlines.Add(inline);
        }
        #endregion


    }
}