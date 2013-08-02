#region License

//
// MockElementCollection.cs
//
// Copyright (C) 2009-2013 Alex Taylor.  All Rights Reserved.
//
// This file is part of Digitalis.LDTools.DOM.UnitTests.dll
//
// Digitalis.LDTools.DOM.UnitTests.dll is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Digitalis.LDTools.DOM.UnitTests.dll is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Digitalis.LDTools.DOM.UnitTests.dll.  If not, see <http://www.gnu.org/licenses/>.
//

#endregion License

namespace UnitTests
{
    #region Usings

    using System;
    using System.Drawing;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [Serializable]
    public class MockElementCollection : ElementCollection, IElement
    {
        public MockElementCollection()
        {
            // shut the compiler up
            if (null != GroupChanged) { }
        }

        public override bool AllowsTopLevelElements
        {
            get { return true; }
        }

        public override IElementCollection Parent
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _parent;
            }
        }

        IElementCollection IElement.Parent
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

        public override bool HasEditor
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return false;
            }
        }

        public override IElementEditor GetEditor()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return null;
        }

        public override Image Icon
        {
            get { return null; }
        }

        public override string TypeName
        {
            get { return "MockElementCollection"; }
        }

        public override string Description
        {
            get { return null; }
        }

        public override string ExtendedDescription
        {
            get { return null; }
        }

        public override DOMObjectType ObjectType
        {
            get { return DOMObjectType.Collection; }
        }

        public override bool IsImmutable { get { return OverrideIsImmutable; } }
        public bool OverrideIsImmutable;

        [field:NonSerialized]
        public event PropertyChangedEventHandler<IElementCollection> ParentChanged;

        public bool IsGroupable
        {
            get { throw new NotImplementedException(); }
        }

        public IGroup Group
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        [field:NonSerialized]
        public event PropertyChangedEventHandler<IGroup> GroupChanged;

        public bool IsStateElement
        {
            get { return false; }
        }

        public bool IsTopLevelElement
        {
            get { return false; }
        }

        public override bool IsReadOnly { get { return OverrideIsReadOnly; } }
        public bool OverrideIsReadOnly;

        public override void Add(IElement element)
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsImmutable || IsReadOnly)
                throw new NotSupportedException();

            if (IsLocked)
                throw new ElementLockedException();

            base.Add(element);
        }

        public override void Insert(int index, IElement element)
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsImmutable || IsReadOnly)
                throw new NotSupportedException();

            if (IsLocked)
                throw new ElementLockedException();

            base.Insert(index, element);
        }

        public override bool Remove(IElement element)
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsImmutable || IsReadOnly)
                throw new NotSupportedException();

            if (IsLocked)
                throw new ElementLockedException();

            return base.Remove(element);
        }

        public override void RemoveAt(int index)
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsImmutable || IsReadOnly)
                throw new NotSupportedException();

            if (IsLocked)
                throw new ElementLockedException();

            base.RemoveAt(index);
        }

        public override void Clear()
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsImmutable || IsReadOnly)
                throw new NotSupportedException();

            if (IsLocked)
                throw new ElementLockedException();

            base.Clear();
        }
    }
}
