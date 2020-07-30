using PowerLauncher.ViewModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PowerLauncher.Helper
{
    public class ResultCollection : List<ResultViewModel>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Notify change in the List
        /// </summary>
        /// <param name="e">The event argument.</param>
        public void NotifyChanges()
        {
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}