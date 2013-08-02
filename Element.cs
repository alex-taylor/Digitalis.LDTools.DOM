#region License

//
// Element.cs
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

    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Abstract implementation of <see cref="Digitalis.LDTools.DOM.API.IElement"/>.
    /// </summary>
    [Serializable]
    public abstract class Element : PageElement, IElement
    {
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
        /// <exception cref="System.NotSupportedException">The <see cref="Element"/>'s <see cref="Parent"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsImmutable">immutable</see> or
        ///     <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>.</exception>
        /// <exception cref="Digitalis.LDTools.DOM.API.ObjectFrozenException">The <see cref="Element"/>'s <see cref="Parent"/>
        ///     is <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="Digitalis.LDTools.DOM.API.ElementLockedException">The <see cref="Element"/>'s <see cref="Parent"/>
        ///     is <see cref="Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before carrying out their own
        /// disposal operations.
        /// </note>
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IElementCollection parent = Parent;

                // unless the parent is also being disposed of, it may not allow us to be removed from it
                if (null != parent && !parent.IsDisposing)
                {
                    if (parent.IsImmutable || parent.IsReadOnly)
                        throw new NotSupportedException();

                    if (parent.IsFrozen)
                        throw new ObjectFrozenException();

                    if (parent.IsLocked)
                        throw new ElementLockedException();

                    Parent = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion Disposal

        #region Document-tree

        /// <inheritdoc />
        public override IStep Step
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                IElementCollection parent = Parent;

                if (null != parent)
                {
                    if (DOMObjectType.Step == parent.ObjectType)
                        return (IStep)parent;

                    return parent.Step;
                }

                return null;
            }
        }

        /// <inheritdoc />
        public virtual IElementCollection Parent
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _parent;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != Parent && null != value)
                    throw new InvalidOperationException("Cannot set Parent: " + InsertCheckResult.AlreadyMember);

                if (value == Parent)
                    return;

                if (null == value)
                {
                    // removing
                    if (Parent.Contains(this))
                    {
                        // being set directly
                        Parent.Remove(this);
                    }
                    else
                    {
                        // being set by a call to IElementCollection.Remove()
                        PropertyChangedEventArgs<IElementCollection> args = new PropertyChangedEventArgs<IElementCollection>(Parent, null);

                        _parent = null;
                        OnParentChanged(args);
                    }
                }
                else
                {
                    // adding
                    if (value.Contains(this))
                    {
                        // being set by a call to IElementCollection.Insert()
                        PropertyChangedEventArgs<IElementCollection> args = new PropertyChangedEventArgs<IElementCollection>(Parent, value);

                        _parent = value;
                        OnParentChanged(args);
                    }
                    else
                    {
                        // being set directly; this will do the CanInsert() checks
                        value.Add(this);
                    }
                }
            }
        }

        [NonSerialized]
        private IElementCollection _parent;

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<IElementCollection> ParentChanged;

        /// <summary>
        /// Raises the <see cref="IElement.ParentChanged"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="Element"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before returning.
        /// </note>
        /// </remarks>
        protected virtual void OnParentChanged(PropertyChangedEventArgs<IElementCollection> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != e.OldValue)
                e.OldValue.PathToDocumentChanged -= OnPathToDocumentChanged;

            if (null != e.NewValue)
                e.NewValue.PathToDocumentChanged += OnPathToDocumentChanged;

            if (null != ParentChanged)
                ParentChanged(this, e);

            OnChanged(this, "ParentChanged", e);
            OnPathToDocumentChanged(this, EventArgs.Empty);
        }

        private void OnPathToDocumentChanged(object sender, EventArgs e)
        {
            OnPathToDocumentChanged(e);
        }

        #endregion Document-tree

        #region Freezing

        /// <inheritdoc />
        public override bool IsFrozen
        {
            get
            {
                if (base.IsFrozen)
                    return true;

                IElementCollection parent = Parent;

                if (null != parent)
                    return parent.IsFrozen;

                return false;
            }
        }

        /// <inheritdoc />
        protected override void OnFreezing(EventArgs e)
        {
            IElementCollection parent = Parent;

            if (null != parent)
                parent.Freeze();

            base.OnFreezing(e);
        }

        #endregion Freezing

        #region Locking

        /// <inheritdoc />
        public sealed override bool IsLocked
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
            set { base.IsLocked = value; }
        }

        #endregion Locking

        #region Self-description

        /// <inheritdoc />
        public abstract bool IsStateElement { get; }

        /// <inheritdoc />
        public abstract bool IsTopLevelElement { get; }

        #endregion Self-description
    }
}
