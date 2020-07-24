using PowerLauncher.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PowerLauncher.Helper
{
    public class ResultCollection : ObservableCollection<ResultViewModel>
    {

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs _)
        {
            //if (!_suppressNotification)
            //    base.OnCollectionChanged(e);
        }

        public void RemoveAll(Predicate<ResultViewModel> predicate)
        {
            CheckReentrancy();
            List<ResultViewModel> removedItems = new List<ResultViewModel>();
            for (int i = Count - 1; i >= 0; i--)
            {
                if (predicate(this[i]))
                {
                    removedItems.Add(this[i]);
                    RemoveAt(i);
                }
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
        }

        /// <summary>
        /// Update the results collection with new results, try to keep identical results
        /// </summary>
        /// <param name="newItems"></param>
        public void Update(List<ResultViewModel> newItems)
        {
            if (newItems == null)
            {
                throw new ArgumentNullException(nameof(newItems));
            }


            int newCount = newItems.Count;
            int oldCount = Items.Count;
            int location = newCount > oldCount ? oldCount : newCount;

            for (int i = 0; i < location; i++)
            {
                ResultViewModel oldResult = this[i];
                ResultViewModel newResult = newItems[i];
                if (!oldResult.Equals(newResult))
                { // result is not the same update it in the current index
                    this[i] = newResult;
                }
                else if (oldResult.Result.Score != newResult.Result.Score)
                {
                    this[i].Result.Score = newResult.Result.Score;
                }
            }

            if (newCount >= oldCount)
            {
                for (int i = oldCount; i < newCount; i++)
                {
                    Add(newItems[i]);
                }
            }
            else
            {
                for (int i = oldCount - 1; i >= newCount; i--)
                {
                    RemoveAt(i);
                }
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
