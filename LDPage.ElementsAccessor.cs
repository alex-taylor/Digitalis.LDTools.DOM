#region License

//
// LDPage.ElementsAccessor.cs
//
// Copyright (C) 2009-2013 Alex Taylor.  All Rights Reserved.
//
// This file is part of Digitalis.LDTools.DOM.dll
//
// Digitalis.LDTools.DOM.dll is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Digitalis.LDTools.DOM.dll is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Digitalis.LDTools.DOM.dll.  If not, see <http://www.gnu.org/licenses/>.
//

#endregion License

namespace Digitalis.LDTools.DOM
{
    #region Usings

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.UndoSystem;

    #endregion Usings

    [Serializable]
    internal class ElementAccessor : IDOMObjectCollection<IElement>
    {
        #region Internals

        private LDPage _page;

        private IStep GetStep(ref int elementIndex)
        {
            if (elementIndex < 0 || elementIndex >= Count)
                throw new ArgumentOutOfRangeException();

            foreach (IStep step in _page)
            {
                if (elementIndex < step.Count)
                    return step;

                elementIndex -= step.Count;
            }

            return null;
        }

        private IStep GetStepForInsert(ref int elementIndex)
        {
            if (elementIndex < 0 || elementIndex > Count)
                throw new ArgumentOutOfRangeException();

            foreach (IStep step in _page)
            {
                if (elementIndex <= step.Count)
                    return step;

                elementIndex -= step.Count;
            }

            return null;
        }

        #endregion Internals

        #region Collection-management

        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IElement> ItemsAdded;

        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IElement> ItemsRemoved;

        [field: NonSerialized]
        public event UndoableListReplacedEventHandler<IElement> ItemsReplaced;

        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IElement> CollectionCleared;

        public InsertCheckResult CanInsert(IElement element, InsertCheckFlags flags)
        {
            if (0 == _page.Count)
                return InsertCheckResult.NotSupported;

            return _page[0].CanInsert(element, flags);
        }

        public InsertCheckResult CanReplace(IElement elementToInsert, IElement elementToReplace, InsertCheckFlags flags)
        {
            if (0 == _page.Count)
                return InsertCheckResult.NotSupported;

            return _page[0].CanReplace(elementToInsert, elementToReplace, flags);
        }

        public bool IsReadOnly { get { return false; } }

        public int Count
        {
            get
            {
                int count = 0;

                foreach (IStep step in _page)
                {
                    count += step.Count;
                }

                return count;
            }
        }

        public int IndexOf(IElement element)
        {
            int idx = 0;

            foreach (IStep step in _page)
            {
                if (step.Contains(element))
                {
                    idx += step.IndexOf(element);
                    return idx;
                }

                idx += step.Count;
            }

            return -1;
        }

        public bool Contains(IElement element)
        {
            return (-1 != IndexOf(element));
        }

        public IElement this[int index]
        {
            get
            {
                IStep step = GetStep(ref index);
                return step[index];
            }
            set
            {
                IStep step = GetStep(ref index);
                step[index] = value;
            }
        }

        public void Add(IElement element)
        {
            IStep step = _page[_page.Count - 1];
            step.Add(element);
        }

        public void Insert(int index, IElement element)
        {
            IStep step = GetStepForInsert(ref index);

            if (null == step)
                throw new ArgumentOutOfRangeException();

            step.Insert(index, element);
        }

        public bool Remove(IElement element)
        {
            foreach (IStep step in _page)
            {
                if (step.Contains(element))
                {
                    step.Remove(element);
                    return true;
                }
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            IStep step = GetStep(ref index);
            step.RemoveAt(index);
        }

        public void Clear()
        {
            foreach (IStep step in _page)
            {
                step.Clear();
            }
        }

        public void CopyTo(IElement[] array, int arrayIndex)
        {
            if (null == array)
                throw new ArgumentNullException();

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException();

            if (array.Length - arrayIndex < Count)
                throw new ArgumentException();

            foreach (IStep step in _page)
            {
                step.CopyTo(array, arrayIndex);
                arrayIndex += step.Count;
            }
        }

        public IEnumerator<IElement> GetEnumerator()
        {
            foreach (IStep step in _page)
            {
                foreach (IElement element in step)
                {
                    yield return element;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Collection-management

        #region Constructor

        public ElementAccessor(LDPage page)
        {
            _page = page;

            page.ItemsAdded += delegate(object sender, UndoableListChangedEventArgs<IStep> e)
            {
                foreach (IStep step in e.Items)
                {
                    step.ItemsAdded        += OnElementsAdded;
                    step.ItemsRemoved      += OnElementsRemoved;
                    step.ItemsReplaced     += OnElementsReplaced;
                    step.CollectionCleared += OnStepCleared;

                    if (0 != step.Count && null != ItemsAdded)
                        ItemsAdded(this, new UndoableListChangedEventArgs<IElement>(step, 0, step.Count));
                }
            };

            page.ItemsRemoved += delegate(object sender, UndoableListChangedEventArgs<IStep> e)
            {
                foreach (IStep step in e.Items)
                {
                    step.ItemsAdded        -= OnElementsAdded;
                    step.ItemsRemoved      -= OnElementsRemoved;
                    step.ItemsReplaced     -= OnElementsReplaced;
                    step.CollectionCleared -= OnStepCleared;

                    if (0 != step.Count && null != ItemsRemoved)
                        ItemsRemoved(this, new UndoableListChangedEventArgs<IElement>(step, 0, step.Count));
                }
            };

            page.ItemsReplaced += delegate(object sender, UndoableListReplacedEventArgs<IStep> e)
            {
                List<IElement> elementsRemoved = new List<IElement>();
                List<IElement> elementsAdded   = new List<IElement>();

                foreach (IStep step in e.ItemsRemoved.Items)
                {
                    step.ItemsAdded        -= OnElementsAdded;
                    step.ItemsRemoved      -= OnElementsRemoved;
                    step.ItemsReplaced     -= OnElementsReplaced;
                    step.CollectionCleared -= OnStepCleared;

                    elementsRemoved.AddRange(step);
                }

                foreach (IStep step in e.ItemsAdded.Items)
                {
                    step.ItemsAdded        += OnElementsAdded;
                    step.ItemsRemoved      += OnElementsRemoved;
                    step.ItemsReplaced     += OnElementsReplaced;
                    step.CollectionCleared += OnStepCleared;

                    elementsAdded.AddRange(step);
                }

                if ((0 != elementsRemoved.Count || 0 != elementsAdded.Count) && null != ItemsReplaced)
                {
                    ItemsReplaced(this, new UndoableListReplacedEventArgs<IElement>(new UndoableListChangedEventArgs<IElement>(elementsAdded, 0, elementsAdded.Count),
                                                                                       new UndoableListChangedEventArgs<IElement>(elementsRemoved, 0, elementsRemoved.Count)));
                }
            };

            page.CollectionCleared += delegate(object sender, UndoableListChangedEventArgs<IStep> e)
            {
                List<IElement> elements = new List<IElement>();

                foreach (IStep step in e.Items)
                {
                    step.ItemsAdded        -= OnElementsAdded;
                    step.ItemsRemoved      -= OnElementsRemoved;
                    step.ItemsReplaced     -= OnElementsReplaced;
                    step.CollectionCleared -= OnStepCleared;

                    elements.AddRange(step);
                }

                if (0 != elements.Count && null != CollectionCleared)
                    CollectionCleared(this, new UndoableListChangedEventArgs<IElement>(elements, 0, elements.Count));
            };
        }

        private void OnElementsAdded(object sender, UndoableListChangedEventArgs<IElement> e)
        {
            if (null != ItemsAdded)
                ItemsAdded(this, e);
        }

        private void OnElementsRemoved(object sender, UndoableListChangedEventArgs<IElement> e)
        {
            if (null != ItemsRemoved)
                ItemsRemoved(this, e);
        }

        private void OnElementsReplaced(object sender, UndoableListReplacedEventArgs<IElement> e)
        {
            if (null != ItemsReplaced)
                ItemsReplaced(this, e);
        }

        private void OnStepCleared(object sender, UndoableListChangedEventArgs<IElement> e)
        {
            if (0 == Count && null != CollectionCleared)
                CollectionCleared(this, e);
        }

        #endregion Constructor
    }
}
