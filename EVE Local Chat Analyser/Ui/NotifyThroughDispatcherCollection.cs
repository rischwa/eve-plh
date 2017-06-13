#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

#endregion

namespace EveLocalChatAnalyser.Ui
{
    public class NotifyThroughDispatcherCollection<T> : List<T>, INotifyCollectionChanged where T : IDisposable
    {
        public bool IsReadOnly
        {
            get { return false; }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public new void Add(T item)
        {
            base.Add(item);
            if (CollectionChanged != null)
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(() =>
                          CollectionChanged(this,
                                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item))));
            }
        }

        public new void Clear()
        {
            base.Clear();
            if (CollectionChanged != null)
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(() =>
                          CollectionChanged(this,
                                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))));
            }
        }

        public new bool Remove(T item)
        {
            var isRemoved = base.Remove(item);
            if (isRemoved && CollectionChanged != null)
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(() =>
                          CollectionChanged(this,
                                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                                                                                 item))));
            }
            return isRemoved;
        }

        public void SetContent(IEnumerable<T> characters)
        {
            foreach (var curChar in this)
            {
                curChar.Dispose();
            }
            base.Clear();
            AddRange(characters.ToList());
            if (CollectionChanged != null)
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(() =>
                          CollectionChanged(this,
                                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))));
            }
        }
    }
}