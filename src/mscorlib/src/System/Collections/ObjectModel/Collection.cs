// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// 

namespace System.Collections.ObjectModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    [DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]    
    public class Collection<T>: IList<T>, IList, IReadOnlyList<T>
    {
        IList<T> items;
        [NonSerialized]
        private Object _syncRoot;

        public Collection() {
            items = new List<T>();
        }

        public Collection(IList<T> list) {
            if (list == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.list);
            }
            items = list;
        }

        public int Count {
            get { return items.Count; }
        }

        protected IList<T> Items {
            get { return items; }
        }

        public T this[int index] {
            get { return items[index]; }
            set {
                CheckReadOnly();
                CheckIndex(index);

                SetItem(index, value);
            }
        }

        public void Add(T item) {
            CheckReadOnly();
            
            int index = items.Count;
            InsertItem(index, item);
        }

        public void AddRange(IEnumerable<T> collection) {
            CheckReadOnly();
            CheckNull(collection, ExceptionArgument.collection);

            InsertItemRange(items.Count, collection);
        }

        public void Clear() {
            CheckReadOnly();

            ClearItems();
        }

        public void CopyTo(T[] array, int index) {
            items.CopyTo(array, index);
        }

        public bool Contains(T item) {
            return items.Contains(item);
        }

        public IEnumerator<T> GetEnumerator() {
            return items.GetEnumerator();
        }

        public int IndexOf(T item) {
            return items.IndexOf(item);
        }

        public void Insert(int index, T item) {
            CheckReadOnly();
            CheckIndex(index, ExceptionResource.ArgumentOutOfRange_ListInsert);

            InsertItem(index, item);
        }

        public void InsertRange(int index, IEnumerable<T> collection) {
            CheckReadOnly();
            CheckIndex(index, ExceptionResource.ArgumentOutOfRange_ListInsert);
            CheckNull(collection, ExceptionArgument.collection);

            InsertItemRange(index, collection);
        }

        public bool Remove(T item) {
            CheckReadOnly();

            int index = items.IndexOf(item);
            if (index < 0) return false;
            RemoveItem(index);
            return true;
        }

        //public void RemoveAll(Predicate<T> match) {
        //    CheckReadOnly();
        //    CheckNull(match, ExceptionArgument.match);

        //    RemoveItemRange(match);
        //}

        public void RemoveAt(int index) {
            CheckReadOnly();
            CheckIndex(index);

            RemoveItem(index);
        }

        public void RemoveRange(int index, int count)
        {
            CheckReadOnly();
            CheckIndex(index);
            CheckCount(count, index);

            RemoveItemRange(index, count);
        }

        public void ReplaceRange(int index, int count, IEnumerable<T> collection)
        {
            CheckReadOnly();
            CheckIndex(index);
            CheckCount(count, index);

            RemoveItemRange(index, count);
            InsertItemRange(index, collection);
        }

        protected virtual void ClearItems() {
            items.Clear();
        }

        protected virtual void InsertItem(int index, T item) {
            items.Insert(index, item);
        }

        protected virtual void InsertItemRange(int index, IEnumerable<T> collection) {
            List<T> list = Items as List<T>;
            if (list != null) {
                // If items is List<T>, use List<T>'s optimized InsertRange.
                list.InsertRange(index, collection);
            }
            else {
                // Otherwise, fallback to inserting each item individually.
                foreach (T item in collection) {
                    items.Insert(index++, item);
                }
            }
        }

        protected virtual void RemoveItem(int index) {
            items.RemoveAt(index);
        }

        protected virtual void RemoveItemRange(int index, int count) {
            List<T> list = Items as List<T>;
            if (list != null) {
                // If items is List<T>, use List<T>'s optimized RemoveRange.
                list.RemoveRange(index, count);
            }
            else {
                // Otherwise, fallback to removing each item individually.
                for (int i = index; i <= count - 1; i++) {
                    items.RemoveAt(index);
                }
            }
        }

        //// NOTE: Removed for now per https://github.com/dotnet/corefx/issues/10752#issuecomment-252998145, but
        //// leaving implementation for future discussion.
        //protected virtual void RemoveItemRange(Predicate<T> match) {
        //    List<T> list = Items as List<T>;
        //    if (list != null) {
        //        // If items is List<T>, use List<T>'s optimized RemoveAll.
        //        list.RemoveAll(match);
        //    }
        //    else {
        //        // Otherwise, fallback to removing each item individually.
        //        foreach (var item in list.FindAll(match)) {
        //            list.Remove(item);
        //        }
        //    }
        //}

        protected virtual void SetItem(int index, T item) {
            items[index] = item;
        }
        
        bool ICollection<T>.IsReadOnly {
            get { 
                return items.IsReadOnly; 
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)items).GetEnumerator();
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot { 
            get {                 
                if( _syncRoot == null) {
                    ICollection c = items as ICollection;
                    if( c != null) {
                        _syncRoot = c.SyncRoot;
                    }
                    else {
                        System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);    
                    }
                }
                return _syncRoot;                 
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            if (array == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }
            
            if (array.Rank != 1) {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
            }

            if( array.GetLowerBound(0) != 0 ) {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
            }
            
            if (index < 0 ) {
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            }

            if (array.Length - index < Count) {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }

            T[] tArray = array as T[];
            if (tArray  != null) {
                items.CopyTo(tArray , index);
            }
            else {
                //
                // Catch the obvious case assignment will fail.
                // We can found all possible problems by doing the check though.
                // For example, if the element type of the Array is derived from T,
                // we can't figure out if we can successfully copy the element beforehand.
                //
                Type targetType = array.GetType().GetElementType(); 
                Type sourceType = typeof(T);
                if(!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) {
                    ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                }

                //
                // We can't cast array of value type to object[], so we don't support 
                // widening of primitive types here.
                //
                object[] objects = array as object[];
                if( objects == null) {
                    ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                }

                int count = items.Count;
                try {
                    for (int i = 0; i < count; i++) {
                        objects[index++] = items[i];
                    }
                }
                catch(ArrayTypeMismatchException) {
                    ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
                }
            }            
        }

        object IList.this[int index] {
            get { return items[index]; }
            set {
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

                try { 
                    this[index] = (T)value;               
                }
                catch (InvalidCastException) { 
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));            
                }

            }
        }

        bool IList.IsReadOnly {
            get { 
                return items.IsReadOnly;
            }
        }

        bool IList.IsFixedSize {
            get { 
                // There is no IList<T>.IsFixedSize, so we must assume that only
                // readonly collections are fixed size, if our internal item 
                // collection does not implement IList.  Note that Array implements
                // IList, and therefore T[] and U[] will be fixed-size.
                IList list = items as IList;
                if(list != null)
                {
                    return list.IsFixedSize;
                }
                return items.IsReadOnly;
            }
        }

        int IList.Add(object value) {
            CheckReadOnly();
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
            
            try { 
                Add((T)value);
            }
            catch (InvalidCastException) { 
                ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));            
            }   
            
            return this.Count - 1;
        }

        bool IList.Contains(object value) {
            if(IsCompatibleObject(value)) {
                return Contains((T) value);
            }
            return false;
        }

        int IList.IndexOf(object value) {  
            if(IsCompatibleObject(value)) {
                return IndexOf((T)value);
            }             
            return -1;
        }

        void IList.Insert(int index, object value) {
            CheckReadOnly();
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
            
            try { 
                Insert(index, (T)value);
            }
            catch (InvalidCastException) { 
                ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));            
            } 

        }

        void IList.Remove(object value) {
            CheckReadOnly();

            if(IsCompatibleObject(value)) {
                Remove((T) value);
            }             
        }

        private void CheckCount(int count, int index) {
            if (count < 0) {
                ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
            }
            if (index + count > items.Count) {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);
            }
        }

        private void CheckIndex(int index){
            if (index < 0 || index >= items.Count) {
                ThrowHelper.ThrowArgumentOutOfRange_IndexException();
            }
        }

        private void CheckIndex(int index, ExceptionResource resource) {
            if (index < 0 || index > items.Count) {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, resource);
            }
        }

        private void CheckNull(object thingToCheck, ExceptionArgument argument) {
            if (thingToCheck == null) {
                ThrowHelper.ThrowArgumentNullException(argument);
            }
        }

        private void CheckReadOnly() {
            if (items.IsReadOnly) {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
        }

        private static bool IsCompatibleObject(object value) {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            return ((value is T) || (value == null && default(T) == null));
        }
    }
}
