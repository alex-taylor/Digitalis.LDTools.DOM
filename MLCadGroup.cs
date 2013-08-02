#region License

//
// MLCadGroup.cs
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
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [TypeName(typeof(Resources), "Group")]
    [DefaultIcon(typeof(Resources), "GroupIcon")]
    [ElementFlags(ElementFlags.HasEditor | ElementFlags.TopLevelElement)]
    public sealed class MLCadGroup : Element, IGroup
    {
        #region Attributes

        ///// <inheritdoc />
        ///// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/> is
        /////     <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see> or contains locked elements.</exception>
        ///// <remarks>
        ///// <para>
        ///// <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/>s do not support this property directly; instead, setting it will cause the property to be set on each
        ///// member of the group. The returned value will be <c>true</c> if any member of the group is ghosted and <c>false</c> otherwise.
        ///// </para>
        ///// </remarks>
        //public bool IsGhosted
        //{
        //    get { return (null != (from n in this where n is IGraphic && (n as IGraphic).IsGhosted select n).Take(1).FirstOrDefault()); }
        //    set
        //    {
        //        if (IsLocked)
        //            throw new ElementLockedException();

        //        if (0 != Count)
        //        {
        //            foreach (IGroupable element in this)
        //            {
        //                if (element.IsLocked)
        //                    throw new ElementLockedException();
        //            }

        //            foreach (IGraphic graphic in from n in this where n is IGraphic select n as IGraphic)
        //            {
        //                graphic.IsGhosted = value;
        //            }
        //        }
        //    }
        //}

        ///// <inheritdoc />
        ///// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/> is
        /////     <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see> or contains locked elements.</exception>
        ///// <remarks>
        ///// <para>
        ///// <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/>s do not support this property directly; instead, setting it will cause the property to be set on each
        ///// member of the group. The returned value will be <c>true</c> if any member of the group is visible and <c>false</c> otherwise.
        ///// </para>
        ///// </remarks>
        //public bool IsVisible
        //{
        //    get { return (null != (from n in this where n is IGraphic && (n as IGraphic).IsVisible select n).Take(1).FirstOrDefault()); }
        //    set
        //    {
        //        if (IsLocked)
        //            throw new ElementLockedException();

        //        if (0 != Count)
        //        {
        //            foreach (IGroupable element in this)
        //            {
        //                if (element.IsLocked)
        //                    throw new ElementLockedException();
        //            }

        //            foreach (IGraphic graphic in from n in this where n is IGraphic select n as IGraphic)
        //            {
        //                graphic.IsVisible = value;
        //            }
        //        }
        //    }
        //}

        #endregion Attributes

        #region Cloning

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Copying an <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/> does not alter the <see cref="P:Digitalis.LDTools.DOM.API.IElement.Group"/> properties of the
        /// <see cref="T:Digitalis.LDTools.DOM.API.IElement"/>s attached to the <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/>, so the copy will have a
        /// <see cref="P:System.Collections.IList{T}.Count"/> of zero.
        /// </para>
        /// </remarks>
        protected override void InitializeObject(IDOMObject obj)
        {
            ((IGroup)obj).Name = Name;
            base.InitializeObject(obj);
        }

        #endregion Cloning

        #region Code-generation

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// In the <see cref="Digitalis.LDTools.DOM.API.CodeStandards.Full"/> and <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> modes the
        /// <i>GROUP</i> meta-command is appended for non-empty <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/>s.
        /// In <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/> mode no code is appended.
        /// </para>
        /// </remarks>
        public override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint colour, ref Matrix4d transform, WindingDirection winding)
        {
            sb = base.ToCode(sb, codeFormat, colour, ref transform, winding);

            if (CodeStandards.OfficialModelRepository == codeFormat && null != Page && PageType.Model != Page.PageType)
                codeFormat = CodeStandards.PartsLibrary;

            if (0 == Count || CodeStandards.PartsLibrary == codeFormat)
                return sb;

            StringBuilder elementCode = new StringBuilder();

            foreach (IGroupable element in this)
            {
                element.ToCode(elementCode, codeFormat, colour, ref transform, winding);
            }

            // the generated code includes the "0 MLCAD BTG" lines, so we must divide by 2 to get the number of elements for the group
            int elements = System.Text.RegularExpressions.Regex.Matches(elementCode.ToString(), @"^.*\S.*\rn?$", System.Text.RegularExpressions.RegexOptions.Multiline).Count / 2;

            return sb.AppendFormat("0 GROUP {0} {1}{2}", elements, Name, LineTerminator);
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MLCadGroup"/> class with default values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="P:Digitalis.LDTools.DOM.API.IGroup.Name"/> will be set to <c>"Untitled"</c>.
        /// </para>
        /// </remarks>
        public MLCadGroup()
            : this(Resources.Untitled)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MLCadGroup"/> class with the specified values.
        /// </summary>
        /// <param name="name">The <see cref="P:Digitalis.LDTools.DOM.API.IGroup.Name"/> of the group.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="name"/> was <c>null</c> or empty.</exception>
        public MLCadGroup(string name)
            : base()
        {
            Name = name;

            _name.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != NameChanged)
                    NameChanged(this, e);

                OnChanged(this, "NameChanged", e);
            };
        }

        #endregion Constructor

        #region Content-management

        private IEnumerable<IGroupable> GetMembers()
        {
            IPage page = Page;

            if (null != page)
                return from n in page.Elements where n is IGroupable && ((IGroupable)n).Group == this select (IGroupable)n;

            IStep step = Step;

            if (null != step)
                return from n in step where n is IGroupable && ((IGroupable)n).Group == this select (IGroupable)n;

            return null;
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                IEnumerable<IElement> members = GetMembers();

                if (null == members)
                    return 0;

                return members.Count();
            }
        }

        /// <inheritdoc />
        public bool Contains(IGroupable element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (element.IsDisposed)
                return false;

            return (element.Group == this);
        }

        /// <inheritdoc />
        public void Add(IGroupable element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null == element)
                throw new ArgumentNullException();

            element.Group = this;
        }

        /// <inheritdoc />
        public bool Remove(IGroupable element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null == element)
                throw new ArgumentNullException();

            if (!Contains(element))
                return false;

            element.Group = null;
            return true;
        }

        /// <summary>
        /// Infrastructure. This method is not supported by <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="T:Digtialis.LDTools.DOM.API.IElement"/>s are removed from an <see cref="T:Digitalis.LDTools.DOM.API.ICadGroup"/> by setting their
        /// <see cref="P:Digitalis.LDTools.DOM.API.IElement.Group"/> property.
        /// </para>
        /// </remarks>
        public void Clear()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            foreach (IGroupable element in this)
            {
                element.Group = null;
            }
        }

        /// <inheritdoc />
        public void CopyTo(IGroupable[] array, int arrayIndex)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null == array)
                throw new ArgumentNullException();

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException();

            if (array.Length - arrayIndex < Count)
                throw new ArgumentException();

            foreach (IGroupable element in GetMembers())
            {
                array[arrayIndex++] = element;
            }
        }

        /// <inheritdoc />
        public IEnumerator<IGroupable> GetEnumerator()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            IEnumerable<IGroupable> members = GetMembers();

            if (null == members)
                throw new InvalidOperationException();

            return members.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Content-management

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.MLCadGroupEditor", typeof(MLCadGroup));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="MLCadGroup"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override bool HasEditor
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return (null != EditorFactory);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="MLCadGroup"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override IElementEditor GetEditor()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != EditorFactory)
                return EditorFactory.Invoke(new object[] { this }) as IElementEditor;

            return null;
        }

        #endregion Editor

        #region Geometry

        /// <inheritdoc />
        public Box3d BoundingBox
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                Box3d box = new Box3d();

                if (0 == Count)
                    return box;

                IGeometric g;
                bool first = true;

                foreach (IGroupable element in this)
                {
                    g = element as IGeometric;

                    if (null != g)
                    {
                        if (first)
                        {
                            box = g.BoundingBox;
                            first = false;
                        }
                        else
                        {
                            box.Union(g.BoundingBox);
                        }
                    }
                }

                return box;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// This is the centre of <see cref="P:Digitalis.LDTools.DOM.API.IGeometric.BoundingBox"/>.
        /// </para>
        /// </remarks>
        public Vector3d Origin
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return BoundingBox.Centre;
            }
        }

        /// <inheritdoc />
        public CullingMode WindingMode { get { return this.GetWindingMode(Page, Parent); } }

        /// <inheritdoc />
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see> or contains locked elements.</exception>
        public void Transform(ref Matrix4d transform)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsLocked)
                throw new ElementLockedException();

            foreach (IGroupable element in this)
            {
                if (element.IsLocked)
                    throw new ElementLockedException();
            }

            IGeometric graphic;

            foreach (IGroupable element in this)
            {
                graphic = element as IGeometric;

                if (null != graphic)
                    graphic.Transform(ref transform);
            }
        }

        /// <inheritdoc />
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The <see cref="MLCadGroup"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see> or contains locked elements.</exception>
        public void ReverseWinding()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsLocked)
                throw new ElementLockedException();

            foreach (IElement element in this)
            {
                if (element.IsLocked)
                    throw new ElementLockedException();
            }

            IGeometric graphic;

            foreach (IGroupable element in this)
            {
                graphic = element as IGeometric;

                if (null != graphic)
                    graphic.ReverseWinding();
            }
        }

        #endregion Geometry

        #region Properties

        /// <inheritdoc />
        public string Name
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _name.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();

                if (_name.Value == value)
                    return;

                IPage page = Page;

                if (!CheckGroupNameIsUnique(value))
                    throw new DuplicateNameException(value);

                _name.Value = value;
            }
        }
        private UndoableProperty<string> _name = new UndoableProperty<string>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> NameChanged;

        private bool CheckGroupNameIsUnique(string name)
        {
            IPage page = Page;

            if (null == page)
                return true;

            IGroup group;

            foreach (IStep step in page)
            {
                foreach (IElement element in step)
                {
                    group = element as IGroup;

                    if (null != group && group.Name == name)
                        return false;
                }
            }

            return true;
        }

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override DOMObjectType ObjectType { get { return DOMObjectType.Group; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.GroupIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.Group; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="T:Digitalis.LDTools.DOM.API.IGroup.Name"/>.
        /// </para>
        /// </remarks>
        public override string Description { get { return Name; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="F:String.Empty"/>
        /// </para>
        /// </remarks>
        public override string ExtendedDescription { get { return String.Empty; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/> is not a state-element.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/> is a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return true; } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return false; } }

        #endregion Self-description
    }
}
