using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NBD2.Util
{
    public class DeeplyObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Register(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Unregister(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (e.OldItems != null)
                    {
                        Unregister(e.OldItems);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Unregister(e.OldItems);
                    Register(e.NewItems);
                    break;
            }
        }

        private void Register(IEnumerable items)
        {
            foreach (T item in items)
            {
                item.PropertyChanged += ItemOnPropertyChanged;
            }
        }

        private void Unregister(IEnumerable items)
        {
            foreach (T item in items)
            {
                item.PropertyChanged -= ItemOnPropertyChanged;
            }
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemPropertyChanged?.Invoke(sender, e);
        }

        public event PropertyChangedEventHandler ItemPropertyChanged;
    }
}