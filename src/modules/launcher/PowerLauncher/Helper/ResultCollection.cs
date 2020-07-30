﻿using PowerLauncher.ViewModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PowerLauncher.Helper
{
    public class ResultCollection : List<ResultViewModel>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private int CompareResultViewModel(ResultViewModel c1, ResultViewModel c2)
        {
            if (c1.Result.Score > c2.Result.Score)
            {
                return -1;
            }
            else if (c1.Result.Score == c2.Result.Score)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// sort the list in descending order of score
        /// </summary>
        public new void Sort()
        {
            base.Sort(CompareResultViewModel);
        }

        /// <summary>
        /// Notify change in the List view items
        /// </summary>
        /// <param name="e">The event argument.</param>
        public void NotifyChanges()
        {
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

    }
}