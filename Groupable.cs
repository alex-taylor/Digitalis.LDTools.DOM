#region License

//
// Groupable.cs
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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Abstract implementation of <see cref="Digitalis.LDTools.DOM.API.IGroupable"/>.
    /// </summary>
    [Serializable]
    public abstract class Groupable : Element, IGroupable
    {
        #region Cloning and Serialization

        // used during cloning and serialization to allow us to re-link to our group
        private string _groupName;

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            if (null != Group)
            {
                Groupable copy = (Groupable)obj;
                copy._groupName = Group.Name;
            }

            base.InitializeObject(obj);
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext sc)
        {
            if (null != Group)
                _groupName = Group.Name;
        }

        #endregion Cloning and Serialization

        #region Code-generation

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: <see cref="Groupable"/> provides the implementation of this method in order to handle
        /// code-output for the <see cref="Group"/> property. Subclasses must implement <see cref="GenerateCode"/> to provide
        /// their own output.
        /// </note>
        /// </remarks>
        public sealed override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if (CodeStandards.OfficialModelRepository == codeFormat && null != Page && PageType.Model != Page.PageType)
                codeFormat = CodeStandards.PartsLibrary;

            if (CodeStandards.Full == codeFormat || CodeStandards.OfficialModelRepository == codeFormat)
            {
                if (null != Group)
                {
                    string btg = "0 MLCAD BTG " + Group.Name + LineTerminator;

                    if (IsLocalLock)
                        sb.Append(btg);

                    base.ToCode(sb, codeFormat, overrideColour, ref transform, winding);

                    StringBuilder code = GenerateCode(new StringBuilder(), codeFormat, overrideColour, ref transform, winding);

                    if (code.Length > 0)
                    {
                        int i = 0;

                        while (i < code.Length - LineTerminator.Length)
                        {
                            if ('\n' == code[i++] && '\r' != code[i])
                            {
                                code.Insert(i, btg);
                                i += btg.Length;
                            }
                        }

                        code.Insert(0, btg);
                        sb.Append(code);
                    }

                    return sb;
                }
            }

            base.ToCode(sb, codeFormat, overrideColour, ref transform, winding);
            return GenerateCode(sb, codeFormat, overrideColour, ref transform, winding);
        }

        /// <summary>
        /// Returns the <see cref="Groupable"/> as LDraw code.
        /// </summary>
        /// <param name="sb">A <see cref="System.Text.StringBuilder"/> to which the LDraw code will be appended.</param>
        /// <param name="codeFormat">The format required for the returned code.</param>
        /// <param name="overrideColour">The colour-value to be used to override
        ///     <see cref="Digitalis.LDTools.DOM.API.IGraphic.OverrideableColourValue"/> in any
        ///     <see cref="Digitalis.LDTools.DOM.API.IGraphic"/>s in the returned code.</param>
        /// <param name="transform">The transform to be applied to any <see cref="Digitalis.LDTools.DOM.API.IGeometric"/>s in
        ///     the returned code.</param>
        /// <param name="winding">The winding direction to be used by any <see cref="Digitalis.LDTools.DOM.API.IGeometric"/>s in
        ///     the returned code.</param>
        /// <returns>A reference to <paramref name="sb"/> after the append operation has completed.</returns>
        /// <remarks>
        /// <note>
        /// Note to implementors: this method is called by <see cref="ToCode"/> to obtain the LDraw code which represents the
        /// <see cref="Groupable"/>. Subclasses which override this method must call their superclass before returning.
        /// </note>
        /// </remarks>
        protected virtual StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            return sb;
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Groupable"/> class.
        /// </summary>
        protected Groupable()
        {
            _group.ValueChanged += delegate(object sender, PropertyChangedEventArgs<IGroup> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if ((null != e.NewValue && (e.NewValue.IsImmutable || e.NewValue.IsReadOnly)) || (null != e.OldValue && (e.OldValue.IsImmutable || e.OldValue.IsReadOnly)))
                    throw new NotSupportedException();

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if ((null != e.NewValue && e.NewValue.IsLocked) || (null != e.OldValue && e.OldValue.IsLocked))
                    throw new ElementLockedException();

                if (null != e.OldValue)
                    e.OldValue.PathToDocumentChanged -= OnGroupPathToDocumentChanged;

                if (null != e.NewValue)
                    e.NewValue.PathToDocumentChanged += OnGroupPathToDocumentChanged;

                OnGroupChanged(e);
            };
        }

        #endregion Constructor

        #region Disposal

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IElementCollection parent = Parent;

                if (null == parent || !parent.IsDisposing)
                    Group = null;
            }

            base.Dispose(disposing);
        }

        #endregion Disposal

        #region Document-tree

        /// <inheritdoc />
        protected override void OnParentChanged(PropertyChangedEventArgs<IElementCollection> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != e.OldValue)
                e.OldValue.PathToDocumentChanged -= OnGroupPathToDocumentChanged;

            if (null != e.NewValue)
                e.NewValue.PathToDocumentChanged += OnGroupPathToDocumentChanged;

            OnGroupPathToDocumentChanged(this, EventArgs.Empty);
            base.OnParentChanged(e);
        }

        /// <inheritdoc />
        protected override void OnPathToDocumentChanged(EventArgs e)
        {
            if (null != _groupName)
            {
                IPage page = Page;

                if (null != page)
                {
                    if (FindGroup(page.Elements))
                        return;

                    page.Changed += FindGroup;
                }

                IStep step = Step;

                if (null != step)
                {
                    if (FindGroup(step))
                        return;

                    step.ItemsAdded += OnElementsAdded;
                }
            }

            base.OnPathToDocumentChanged(e);
        }

        private void FindGroup(IDOMObject sender, ObjectChangedEventArgs e)
        {
            if (sender is IPage)
                FindGroup(((IPage)sender).Elements);
        }

        private bool FindGroup(IEnumerable<IElement> collection)
        {
            IGroup group;

            foreach (IElement element in collection)
            {
                group = element as IGroup;

                if (null != group && group.Name == _groupName)
                {
                    if (null != Step)
                        Step.ItemsAdded -= OnElementsAdded;

                    if (null != Page)
                        Page.Changed -= FindGroup;

                    _group.Value = group;
                    _groupName   = null;
                    return true;
                }
            }

            return false;
        }

        private void OnElementsAdded(object sender, UndoableListChangedEventArgs<IElement> e)
        {
            IStep step = Step;

            if (null != step)
                FindGroup(step);
        }

        #endregion Document-tree

        #region Grouping

        /// <inheritdoc />
        public bool IsGroupable
        {
            get
            {
                // 1. Frozen elements cannot be grouped.
                if (IsFrozen)
                    return false;

                // 2. The element must be a direct child of a Step.
                if (null != Parent && DOMObjectType.Step != Parent.ObjectType)
                    return false;

                // 3. The element must not be a member of a Group already, and must not be a Group itself
                if (null != Group || DOMObjectType.Group == ObjectType)
                    return false;

                // 4. The element must be a member of a document-tree
                if (null == Parent)
                    return false;

                return true;
            }
        }

        /// <inheritdoc />
        public IGroup Group
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _group.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != value)
                {
                    if (!IsGroupable)
                        throw new InvalidOperationException();

                    if (Page != value.Page && Step != value.Step)
                        throw new InvalidOperationException("The Element and Group must be descendants of the same Page or Step");
                }

                if (_group.Value != value)
                    _group.Value = value;
            }
        }
        private UndoableProperty<IGroup> _group = new UndoableProperty<IGroup>(null, PropertyFlags.DoNotSerializeValue);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<IGroup> GroupChanged;

        /// <summary>
        /// Raises the <see cref="GroupChanged"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="Groupable"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before returning.
        /// </note>
        /// </remarks>
        protected virtual void OnGroupChanged(PropertyChangedEventArgs<IGroup> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != GroupChanged)
                GroupChanged(this, e);

            OnChanged(this, "GroupChanged", e);
        }

        private void OnGroupPathToDocumentChanged(object sender, EventArgs e)
        {
            // disconnect from our Group if its path has changed such that it is no longer accessible to us
            if (null != Group && (null == Page || Group.Page != Page) && (null == Step || Group.Step != Step))
                Group = null;
        }

        #endregion Grouping
    }
}
