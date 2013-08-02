#region License

//
// CustomElement.cs
//
// Copyright (C) 2009-2012 Alex Taylor.  All Rights Reserved.
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

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public abstract class CompositeElement : Graphic, ICompositeElement
    {
        #region Collection-management

        /// <inheritdoc />
        public abstract event UndoableListChangedEventHandler<IElement> ItemsAdded;

        /// <inheritdoc />
        public abstract event UndoableListChangedEventHandler<IElement> ItemsRemoved;

        /// <inheritdoc />
        public abstract event UndoableListReplacedEventHandler<IElement> ItemsReplaced;

        /// <inheritdoc />
        public abstract event UndoableListChangedEventHandler<IElement> CollectionCleared;

        /// <inheritdoc />
        public abstract bool AllowsTopLevelElements { get; }

        /// <inheritdoc />
        public abstract InsertCheckResult CanInsert(IElement element, InsertCheckFlags flags);

        /// <inheritdoc />
        public abstract InsertCheckResult CanReplace(IElement elementToInsert, IElement elementToReplace, InsertCheckFlags flags);

        /// <inheritdoc />
        public abstract bool ContainsColourElements { get; }

        /// <inheritdoc />
        public abstract bool ContainsBFCFlagElements { get; }

        /// <inheritdoc />
        public abstract bool HasLockedDescendants { get; }

        /// <inheritdoc />
        public abstract bool IsReadOnly { get; }

        /// <inheritdoc />
        public abstract int Count { get; }

        /// <inheritdoc />
        public abstract int IndexOf(IElement item);

        /// <inheritdoc />
        public abstract bool Contains(IElement item);

        /// <inheritdoc />
        public abstract IElement this[int index] { get; set; }

        /// <inheritdoc />
        public abstract void Add(IElement item);

        /// <inheritdoc />
        public abstract void Insert(int index, IElement item);

        /// <inheritdoc />
        public abstract bool Remove(IElement item);

        /// <inheritdoc />
        public abstract void RemoveAt(int index);

        /// <inheritdoc />
        public abstract void Clear();

        /// <inheritdoc />
        public abstract void CopyTo(IElement[] array, int arrayIndex);

        /// <inheritdoc />
        public abstract IEnumerator<IElement> GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Collection-management
    }
}
