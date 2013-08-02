#region License

//
// ElementCollection.cs
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
    using System.Runtime.Serialization;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Abstract implementation of <see cref="Digitalis.LDTools.DOM.API.IElementCollection"/>.
    /// </summary>
    [Serializable]
    public abstract class ElementCollection : PageElement, IElementCollection
    {
        #region Change-notification

        /// <inheritdoc />
        protected override void OnChanged(IDOMObject source, string operation, EventArgs parameters)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            _boundsDirty = true;
            base.OnChanged(source, operation, parameters);
        }

        private void OnElementChanged(IDOMObject sender, ObjectChangedEventArgs e)
        {
            _boundsDirty = true;
            base.OnChanged(e);
        }

        #endregion Change-notification

        #region Cloning and Serialization

        private List<IElement> _serializedContents;

        [OnSerializing]
        private void OnSerializing(StreamingContext sc)
        {
            _serializedContents = new List<IElement>(this);
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext sc)
        {
            Initialize();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext sc)
        {
            foreach (IElement element in _serializedContents)
            {
                AddUnchecked(element);
            }

            _serializedContents.Clear();
            _serializedContents = null;
        }

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            IElementCollection collection = (IElementCollection)obj;

            foreach (IElement element in this)
            {
                collection.Add((IElement)element.Clone());
            }

            base.InitializeObject(obj);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        private uint GetEdgeColour(IGraphic line, uint overrideColour, CodeStandards codeFormat)
        {
            if (Palette.MainColour == overrideColour || Palette.EdgeColour == overrideColour)
                return Palette.EdgeColour;

            IColour colour = line.GetColour(overrideColour);

            if (Palette.MainColour == colour.Code || Palette.EdgeColour == colour.Code)
                return Palette.EdgeColour;

            // in 'PartsLibrary' mode we need to convert local-palette colours to Direct Colours as the IColour will be excluded from the
            // output code
            if (CodeStandards.PartsLibrary == codeFormat && !colour.IsSystemPaletteColour && LDColour.IsDirectColour(colour.EdgeCode))
                return LDColour.ConvertRGBToDirectColour(colour.EdgeValue);

            return colour.EdgeCode;
        }

        /// <inheritdoc />
        /// <remarks>
        /// To improve readability of the code, a blank line is inserted before each block of
        /// <see cref="Digitalis.LDTools.DOM.API.IComment"/>s.
        /// </remarks>
        public override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            IElement last = null;
            int size;

            if (CodeStandards.PartsLibrary != codeFormat)
            {
                foreach (IElement el in this)
                {
                    // insert an empty line before each block of comments
                    if (DOMObjectType.Comment == el.ObjectType && null != last && DOMObjectType.Comment != last.ObjectType)
                        sb.Append(LineTerminator);

                    size = sb.Length;

                    if (DOMObjectType.Line == el.ObjectType || DOMObjectType.OptionalLine == el.ObjectType)
                        sb = el.ToCode(sb, codeFormat, GetEdgeColour((IGraphic)el, overrideColour, codeFormat), ref transform, winding);
                    else
                        sb = el.ToCode(sb, codeFormat, overrideColour, ref transform, winding);

                    if (size != sb.Length)
                        last = el;
                }
            }
            else
            {
                CullingMode windingMode = WindingMode;

                if (CullingMode.Disabled == windingMode || WindingDirection.None == winding)
                {
                    foreach (IElement el in this)
                    {
                        if (el is IBFCFlag)
                            continue;

                        // insert an empty line before each block of comments
                        if (DOMObjectType.Comment == el.ObjectType && null != last && DOMObjectType.Comment != last.ObjectType)
                            sb.Append(LineTerminator);

                        size = sb.Length;

                        if (DOMObjectType.Line == el.ObjectType || DOMObjectType.OptionalLine == el.ObjectType)
                            sb = el.ToCode(sb, codeFormat, GetEdgeColour((IGraphic)el, overrideColour, codeFormat), ref transform, winding);
                        else
                            sb = el.ToCode(sb, codeFormat, overrideColour, ref transform, winding);

                        if (size != sb.Length)
                            last = el;
                    }
                }
                else
                {
                    // see if we can reduce the amount of LDraw code generated - if a BFCFlag object is just changing the winding-direction,
                    // we can remove it and just invert 'winding' to create the same effect
                    bool bfcClockwise = (CullingMode.CertifiedClockwise == windingMode);
                    bool bfcEnabled   = (bfcClockwise || CullingMode.CertifiedCounterClockwise == windingMode);

                    foreach (IElement el in this)
                    {
                        IBFCFlag bfcFlag = el as IBFCFlag;

                        if (null != bfcFlag)
                        {
                            size = sb.Length;

                            switch (bfcFlag.Flag)
                            {
                                case BFCFlag.SetWindingModeClockwise:
                                    if (!bfcClockwise)
                                    {
                                        bfcClockwise = true;

                                        if (WindingDirection.Normal == winding)
                                            winding = WindingDirection.Reversed;
                                        else
                                            winding = WindingDirection.Normal;
                                    }
                                    break;

                                case BFCFlag.SetWindingModeCounterClockwise:
                                    if (bfcClockwise)
                                    {
                                        bfcClockwise = false;

                                        if (WindingDirection.Normal == winding)
                                            winding = WindingDirection.Reversed;
                                        else
                                            winding = WindingDirection.Normal;
                                    }
                                    break;

                                case BFCFlag.EnableBackFaceCulling:
                                    if (!bfcEnabled)
                                    {
                                        bfcEnabled = true;
                                        sb = bfcFlag.ToCode(sb, codeFormat, overrideColour, ref transform, winding);
                                    }
                                    break;

                                case BFCFlag.DisableBackFaceCulling:
                                    if (bfcEnabled)
                                    {
                                        bfcEnabled = false;
                                        sb = bfcFlag.ToCode(sb, codeFormat, overrideColour, ref transform, winding);
                                    }
                                    break;

                                case BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise:
                                    if (!bfcEnabled)
                                    {
                                        bfcEnabled = true;
                                        sb = new LDBFCFlag(BFCFlag.EnableBackFaceCulling).ToCode(sb, codeFormat, overrideColour, ref transform, winding);
                                    }

                                    if (!bfcClockwise)
                                    {
                                        bfcClockwise = true;

                                        if (WindingDirection.Normal == winding)
                                            winding = WindingDirection.Reversed;
                                        else
                                            winding = WindingDirection.Normal;
                                    }
                                    break;

                                case BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise:
                                    if (!bfcEnabled)
                                    {
                                        bfcEnabled = true;
                                        sb = new LDBFCFlag(BFCFlag.EnableBackFaceCulling).ToCode(sb, codeFormat, overrideColour, ref transform, winding);
                                    }

                                    if (bfcClockwise)
                                    {
                                        bfcClockwise = false;

                                        if (WindingDirection.Normal == winding)
                                            winding = WindingDirection.Reversed;
                                        else
                                            winding = WindingDirection.Normal;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            // insert an empty line before each block of comments
                            if (DOMObjectType.Comment == el.ObjectType && null != last && DOMObjectType.Comment != last.ObjectType)
                                sb.Append(LineTerminator);

                            size = sb.Length;

                            if (DOMObjectType.Line == el.ObjectType || DOMObjectType.OptionalLine == el.ObjectType)
                                sb = el.ToCode(sb, codeFormat, GetEdgeColour((IGraphic)el, overrideColour, codeFormat), ref transform, winding);
                            else
                                sb = el.ToCode(sb, codeFormat, overrideColour, ref transform, winding);
                        }

                        if (size != sb.Length)
                            last = el;
                    }
                }
            }

            return sb;
        }

        #endregion Code-generation

        #region Collection-management

        [NonSerialized]
        private UndoableList<IElement> _elements;

        private void Initialize()
        {
            _elements                = new UndoableList<IElement>();
            _elements.ItemsAdded    += delegate(object sender, UndoableListChangedEventArgs<IElement> e) { OnItemsAdded(e); };
            _elements.ItemsRemoved  += delegate(object sender, UndoableListChangedEventArgs<IElement> e) { OnItemsRemoved(e); };
            _elements.ItemsReplaced += delegate(object sender, UndoableListReplacedEventArgs<IElement> e) { OnItemsReplaced(e); };
            _elements.ListCleared   += delegate(object sender, UndoableListChangedEventArgs<IElement> e) { OnCollectionCleared(e); };
        }

        // set during a call to AddUnchecked
        [field: NonSerialized]
        private bool _skipPermissionChecks;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IElement> ItemsAdded;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IElement> ItemsRemoved;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListReplacedEventHandler<IElement> ItemsReplaced;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IElement> CollectionCleared;

        /// <summary>
        /// Raises the <see cref="ItemsAdded"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ElementCollection"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void OnItemsAdded(UndoableListChangedEventArgs<IElement> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (!_skipPermissionChecks)
            {
                if (IsFrozen)
                    throw new ObjectFrozenException("Cannot insert: collection is frozen");

                if (IsLocked)
                    throw new ElementLockedException("Cannot insert: collection is locked");

                foreach (IElement element in e.Items)
                {
                    if (null == element)
                        throw new ArgumentNullException("Cannot insert a null value");

                    if (element.IsFrozen)
                        throw new ObjectFrozenException("Cannot insert: element is frozen");

                    InsertCheckResult result = CanInsert(element, InsertCheckFlags.None);

                    if (InsertCheckResult.CanInsert != result)
                        throw new InvalidOperationException("Cannot insert: " + result);
                }
            }

            foreach (IElement el in e.Items)
            {
                if (el is IColour)
                    _numColourElements++;
                else if (el is IBFCFlag)
                    _numBFCFlagElements++;

                el.Changed += OnElementChanged;
                el.Parent = this;
            }

            _boundsDirty = true;

            if (null != ItemsAdded)
                ItemsAdded(this, e);

            OnChanged(this, "ItemsAdded", e);
        }

        /// <summary>
        /// Raises the <see cref="ItemsRemoved"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ElementCollection"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void OnItemsRemoved(UndoableListChangedEventArgs<IElement> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsFrozen)
                throw new ObjectFrozenException("Cannot remove: collection is frozen");

            if (IsLocked)
                throw new ElementLockedException("Cannot remove: collection is locked");

            foreach (IElement el in e.Items)
            {
                if (el is IColour)
                    _numColourElements--;
                else if (el is IBFCFlag)
                    _numBFCFlagElements--;

                el.Parent = null;
                el.Changed -= OnElementChanged;
            }

            _boundsDirty = true;

            if (null != ItemsRemoved)
                ItemsRemoved(this, e);

            OnChanged(this, "ItemsRemoved", e);
        }

        /// <summary>
        /// Raises the <see cref="ItemsReplaced"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ElementCollection"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void OnItemsReplaced(UndoableListReplacedEventArgs<IElement> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsFrozen)
                throw new ObjectFrozenException("Cannot replace: collection is frozen");

            if (IsLocked)
                throw new ElementLockedException("Cannot replace: collection is locked");

            foreach (IElement element in e.ItemsAdded.Items)
            {
                if (null == element)
                    throw new ArgumentNullException("Cannot replace with a null value");

                if (element.IsFrozen)
                    throw new ObjectFrozenException("Cannot replace: element is frozen");

                InsertCheckResult result = CanInsert(element, InsertCheckFlags.None);

                if (InsertCheckResult.CanInsert != result)
                    throw new InvalidOperationException("Cannot replace: " + result);
            }

            foreach (IElement el in e.ItemsAdded.Items)
            {
                if (el is IColour)
                    _numColourElements++;
                else if (el is IBFCFlag)
                    _numBFCFlagElements++;

                el.Changed += OnElementChanged;
                el.Parent = this;
            }

            foreach (IElement el in e.ItemsRemoved.Items)
            {
                if (el is IColour)
                    _numColourElements--;
                else if (el is IBFCFlag)
                    _numBFCFlagElements--;

                el.Parent = null;
                el.Changed -= OnElementChanged;
            }

            _boundsDirty = true;

            if (null != ItemsReplaced)
                ItemsReplaced(this, e);

            OnChanged(this, "ItemsReplaced", e);
        }

        /// <summary>
        /// Raises the <see cref="CollectionCleared"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ElementCollection"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void OnCollectionCleared(UndoableListChangedEventArgs<IElement> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsFrozen)
                throw new ObjectFrozenException("Cannot clear: collection is frozen");

            if (IsLocked)
                throw new ElementLockedException("Cannot clear: collection is locked");

            _numColourElements  = 0;
            _numBFCFlagElements = 0;
            _boundsDirty        = true;

            foreach (IElement el in e.Items)
            {
                el.Parent = null;
                el.Changed -= OnElementChanged;
            }

            if (null != CollectionCleared)
                CollectionCleared(this, e);

            OnChanged(this, "CollectionCleared", e);
        }

        /// <inheritdoc />
        public abstract bool AllowsTopLevelElements { get; }

        /// <inheritdoc />
        public virtual InsertCheckResult CanInsert(IElement element, InsertCheckFlags flags)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return this.CanReplaceElement(element, null, flags);
        }

        /// <inheritdoc />
        public virtual InsertCheckResult CanReplace(IElement elementToInsert, IElement elementToReplace, InsertCheckFlags flags)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return this.CanReplaceElement(elementToInsert, elementToReplace, flags);
        }

        /// <inheritdoc />
        public bool ContainsColourElements
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return (_numColourElements > 0);
            }
        }
        private int _numColourElements;

        /// <inheritdoc />
        public bool ContainsBFCFlagElements
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return (_numBFCFlagElements > 0);
            }
        }
        private int _numBFCFlagElements;

        /// <inheritdoc />
        public bool HasLockedDescendants
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                foreach (IElement e in this)
                {
                    if (e.IsLocked)
                        return true;

                    IElementCollection collection = e as IElementCollection;

                    if (null != collection && collection.HasLockedDescendants)
                        return true;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public virtual int Count
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _elements.Count;
            }
        }

        /// <inheritdoc />
        public virtual int IndexOf(IElement element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return _elements.IndexOf(element);
        }

        /// <inheritdoc />
        public virtual bool Contains(IElement element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return _elements.Contains(element);
        }

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: in order to allow subclasses to implement a collection which is publically read-only but
        /// privately writeable, this does not check the value of
        /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/>.
        /// If you need this check, you must override the setter.
        /// </note>
        /// </remarks>
        public virtual IElement this[int index]
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _elements[index];
            }
            set
            {
                _elements[index] = value;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: in order to allow subclasses to implement a collection which is publically read-only but
        /// privately writeable, this does not check the value of
        /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/>.
        /// If you need this check, you must override this method.
        /// </note>
        /// </remarks>
        public virtual void Add(IElement element)
        {
            Insert(Count, element);
        }

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: in order to allow subclasses to implement a collection which is publically read-only but
        /// privately writeable, this does not check the value of
        /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/>.
        /// If you need this check, you must override this method.
        /// </note>
        /// </remarks>
        public virtual void Insert(int index, IElement element)
        {
            _elements.Insert(index, element);
        }

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: in order to allow subclasses to implement a collection which is publically read-only but
        /// privately writeable, this does not check the value of
        /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/>.
        /// If you need this check, you must override this method.
        /// </note>
        /// </remarks>
        public virtual bool Remove(IElement element)
        {
            return _elements.Remove(element);
        }

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: in order to allow subclasses to implement a collection which is publically read-only but
        /// privately writeable, this does not check the value of
        /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/>.
        /// If you need this check, you must override this method.
        /// </note>
        /// </remarks>
        public virtual void RemoveAt(int index)
        {
            _elements.RemoveAt(index);
        }

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: in order to allow subclasses to implement a collection which is publically read-only but
        /// privately writeable, this does not check the value of
        /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/>.
        /// If you need this check, you must override this method.
        /// </note>
        /// </remarks>
        public virtual void Clear()
        {
            _elements.Clear();
        }

        /// <inheritdoc />
        public virtual void CopyTo(IElement[] array, int arrayIndex)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            _elements.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public virtual IEnumerator<IElement> GetEnumerator()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return _elements.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // used by LDPage's parser: we cannot run through the regular CanInsertElement() checks because the document-tree hasn't been fully constructed yet
        internal void AddUnchecked(IElement element)
        {
            _skipPermissionChecks = true;
            _elements.Add(element);
            _skipPermissionChecks = false;
        }

        #endregion Collection-management

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementCollection"/> class with default values.
        /// </summary>
        protected ElementCollection()
        {
            Initialize();
        }

        #endregion Constructor

        #region Disposal

        /// <inheritdoc />
        public override bool IsDisposing
        {
            get
            {
                IElementCollection parent = Parent;

                if (null != parent && parent.IsDisposing)
                    return true;

                return base.IsDisposing;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing && null != Parent && Parent.IsFrozen)
                throw new ObjectFrozenException();

            base.Dispose(disposing);

            if (disposing)
            {
                foreach (IElement element in this)
                {
                    element.Dispose();
                }
            }
        }

        #endregion Disposal

        #region Document-tree

        /// <inheritdoc />
        public abstract IElementCollection Parent { get; }

        #endregion Document-tree

        #region Geometry

        private void AddToBoundingBox(IElementCollection collection, ref Box3d box, ref bool set)
        {
            IElementCollection c;
            IGeometric         geometric;

            foreach (IElement e in collection)
            {
                c = e as IElementCollection;

                if (null != c)
                {
                    AddToBoundingBox(c, ref box, ref set);
                    continue;
                }

                geometric = e as IGeometric;

                if (null != geometric)
                {
                    if (!set)
                    {
                        box = geometric.BoundingBox;
                        set = true;
                    }
                    else
                    {
                        box.Union(geometric.BoundingBox);
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual Box3d BoundingBox
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (_boundsDirty)
                {
                    bool set = false;

                    _bounds = new Box3d();
                    AddToBoundingBox(this, ref _bounds, ref set);
                    _boundsDirty = false;
                }

                return _bounds;
            }
        }
        private Box3d _bounds;
        private bool  _boundsDirty = true;

        /// <inheritdoc />
        public virtual Vector3d Origin
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return BoundingBox.Centre;
            }
        }

        /// <inheritdoc />
        public virtual CullingMode WindingMode
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return this.GetWindingMode(Page, Parent);
            }
        }

        /// <inheritdoc />
        public virtual void Transform(ref Matrix4d transform)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsLocked || HasLockedDescendants)
                throw new ElementLockedException();

            _boundsDirty = true;

            IGeometric geometric;

            foreach (IElement element in this)
            {
                geometric = element as IGeometric;

                if (null != geometric)
                    geometric.Transform(ref transform);
            }
        }

        /// <inheritdoc />
        public virtual void ReverseWinding()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsLocked || HasLockedDescendants)
                throw new ElementLockedException();

            IGeometric geometric;

            foreach (IElement element in this)
            {
                geometric = element as IGeometric;

                if (null != geometric)
                    geometric.ReverseWinding();
            }
        }

        #endregion Geometry

        #region Locking

        /// <inheritdoc />
        public override bool IsLocked
        {
            get
            {
                if (base.IsLocked)
                    return true;

                IElementCollection parent = Parent;

                if (null != parent)
                    return parent.IsLocked;

                return false;
            }
            set
            {
                base.IsLocked = value;
            }
        }

        #endregion Locking

        #region Self-description

        /// <inheritdoc />
        public abstract bool IsReadOnly { get; }

        #endregion Self-description
    }
}
