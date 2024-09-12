using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace System.Collections.ObjectModel
{
    /// <summary>
    /// Represents a collection of objects that maintains an observable order, 
    /// where each object implements the IOrdered interface.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection, which must implement IOrdered.</typeparam>
    [Serializable]
    public class OrderedObservableCollection<T> : ObservableCollection<T> where T : IOrdered
    {
        private const int Exponent = 10000;
        private const int ExponentChunk = 100;

        /// <summary>
        /// Initializes a new instance of OrderedObservableCollection that is empty and has default initial capacity.
        /// </summary>
        public OrderedObservableCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the OrderedObservableCollection class that contains
        /// elements copied from the specified collection and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the OrderedObservableCollection in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> collection is a null reference </exception>
        public OrderedObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OrderedObservableCollection class
        /// that contains elements copied from the specified list
        /// </summary>
        /// <param name="list">The list whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the OrderedObservableCollection in the
        /// same order they are read by the enumerator of the list.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> list is a null reference </exception>
        public OrderedObservableCollection(List<T> list) : base(list)
        {
        }

        /*protected override void InsertItem(int index, T item)
        {
            Debug.Assert(index == Items.Count, "Implement insering");
            bool isAddedToEnd = index == Items.Count;
            base.InsertItem(index, item);

            if (isAddedToEnd)
            {
                if (index > 0)
                {
                    T previousItem = this[index - 1];
                    int order = previousItem.Order + Exponent;
                    order -= order % ExponentChunk;
                    item.Order = order;
                }
            }
        }*/

        /// <summary>
        /// Moves the item at the specified index to a new location within the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        /// <remarks>
        /// This method overrides the MoveItem method to ensure proper reentrancy checking
        /// and to handle any additional logic necessary for moving items within the ordered collection.
        /// </remarks>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            T removedItem = this[oldIndex];
            T replacedItem = this[newIndex];

            if (newIndex > oldIndex)
            {
                if (newIndex == oldIndex + 1)//moved to the next
                {
                    SwapOrders(removedItem, replacedItem);
                }
                else if (newIndex == Count - 1)//moved to the end
                {
                    Debug.Assert(Count > 2);
                    int order = replacedItem.Order + Exponent;
                    order -= order % ExponentChunk;
                    removedItem.Order = order;
                }
                else//moved to something else
                {
                    throw new NotImplementedException();
                }
            }
            else if (newIndex < oldIndex)
            {
                if (newIndex == oldIndex - 1)//moved to the previous
                {
                    SwapOrders(removedItem, replacedItem);
                }
                else if (newIndex == 0)//moved to the begin
                {
                    Debug.Assert(Count > 2);
                    int newIndexOrder = this[newIndex].Order;
                    int order = newIndexOrder > Exponent ? newIndexOrder - Exponent : newIndexOrder - ExponentChunk;
                    order -= order % ExponentChunk;//TODO check negative?
                    removedItem.Order = order;
                }
                else//moved to something else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }

        /// <summary>
        /// Normalizes the Order properties of items in the collection to ensure they are unique and correctly sequenced.
        /// </summary>
        /// <remarks>
        /// This method adjusts the Order of each item to maintain a consistent sequence,
        /// especially when the initial orders are not properly set or when there are duplicate orders.
        /// It considers whether the orders use a base exponent value and adjusts accordingly.
        /// </remarks>
        public void NormalizeOrders()
        {
            if (Count <= 1)
            {
                return;
            }

            T firstItem = this[0];
            T lastItem = this[Count - 1];

            /*if (firstItem.Order == lastItem.Order && firstItem.Order < Exponent)
            {
                for (int i = 0; i < Count; i++)
                {
                    T currentItem = this[i];
                    currentItem.Order = (i + 1) * Exponent;
                }
                return;
            }*/

            bool hasExponent = lastItem.Order >= Exponent;
            if (!hasExponent && firstItem.Order < ExponentChunk)
            {
                firstItem.Order = Exponent;
            }

            int previousOrder = firstItem.Order;
            for (int i = 1; i < Count; i++)
            {
                T currentItem = this[i];
                if (currentItem.Order <= previousOrder)
                {
                    int order;
                    if (!hasExponent)
                    {
                        if (i < Count - 1 || previousOrder < Exponent)
                        {
                            order = (i + 1) * Exponent;
                        }
                        else
                        {
                            order = previousOrder - previousOrder % ExponentChunk + Exponent;
                        }
                    }
                    else
                    {
                        if (i < Count - 1)
                        {
                            order = previousOrder + ExponentChunk;
                            order -= order % ExponentChunk;
                        }
                        else
                        {
                            order = previousOrder - previousOrder % ExponentChunk + Exponent;
                        }
                    }
                    currentItem.Order = order;
                }
                Debug.Assert(currentItem.Order > previousOrder);
                previousOrder = currentItem.Order;
            }

#if DEBUG
            var hash = new HashSet<int>();
            for (int i = 0; i < Count; i++)
            {
                Debug.Assert(hash.Add(this[i].Order));
                T currentItem = this[i];
            }
#endif
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise a PropertyChanged event for the Indexer property
        /// </summary>
        private void OnIndexerPropertyChanged() => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);


        /// <summary>
        /// Swaps the Order properties of two specified items.
        /// </summary>
        /// <param name="item1">The first item whose Order will be swapped.</param>
        /// <param name="item2">The second item whose Order will be swapped.</param>
        /// <remarks>
        /// This method uses tuple deconstruction to efficiently swap the Order properties of the two items.
        /// </remarks>
        private static void SwapOrders(T item1, T item2)
        {
            (item1.Order, item2.Order) = (item2.Order, item1.Order);
        }
    }

    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
        internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
    }
}
