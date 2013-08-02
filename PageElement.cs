#region License

//
// PageElement.cs
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
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Abstract implementation of <see cref="Digitalis.LDTools.DOM.API.IPageElement"/>.
    /// </summary>
    [Serializable]
    public abstract class PageElement : DocumentElement, IPageElement
    {
        #region Cloning and Serialization

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            ((IPageElement)obj).IsLocked = IsLocked;
            base.InitializeObject(obj);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: if <see cref="Digitalis.LDTools.DOM.API.IPageElement.IsLocalLock"/> is <c>true</c> and
        /// <paramref name="codeFormat"/> is <see cref="Digitalis.LDTools.DOM.API.CodeStandards.Full"/> or
        /// <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/>, this will append the
        /// <i>!DIGITALIS_LDTOOLS_DOM LOCKNEXT</i> meta-command to <paramref name="sb"/>. It does not support the
        /// <i>!DIGITALIS_LDTOOLS_DOM LOCKGEOM</i> meta-command, so implementations of
        /// <see cref="Digitalis.LDTools.DOM.API.ITexmapGeometry"/> must output this themselves.
        /// </note>
        /// </remarks>
        public override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (CodeStandards.PartsLibrary != codeFormat && IsLocalLock)
                sb.Append("0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n");

            return sb;
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PageElement"/> class.
        /// </summary>
        protected PageElement()
        {
            _isLocked.ValueChanged += delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsImmutable)
                    throw new NotSupportedException();

                OnIsLockedChanged(e);
            };
        }

        #endregion Constructor

        #region Disposal

        /// <inheritdoc />
        public override bool IsDisposing
        {
            get
            {
                IPage page = Page;

                if (null != page && page.IsDisposing)
                    return true;

                IStep step = Step;

                if (null != step && step.IsDisposing)
                    return true;

                return base.IsDisposing;
            }
        }

        #endregion Disposal

        #region Document-tree

        /// <inheritdoc />
        public override IDocument Document
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                IPage page = Page;

                if (null != page)
                    return page.Document;

                return null;
            }
        }

        /// <inheritdoc />
        public virtual IPage Page
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                IStep step = Step;

                if (null != step)
                    return step.Page;

                return null;
            }
        }

        /// <inheritdoc />
        public abstract IStep Step { get; }

        #endregion Document-tree

        #region Freezing

        /// <inheritdoc />
        public override bool IsFrozen
        {
            get
            {
                if (base.IsFrozen)
                    return true;

                IStep step = Step;

                if (null != step && step.IsFrozen)
                    return true;

                IPage page = Page;

                if (null != page)
                    return page.IsFrozen;

                return false;
            }
        }

        /// <inheritdoc />
        protected override void OnFreezing(EventArgs e)
        {
            IPage page = Page;

            if (null != page)
                page.Freeze();

            base.OnFreezing(e);
        }

        #endregion Freezing

        #region Locking

        /// <inheritdoc />
        public virtual bool IsLocked
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsDisposing)
                    return false;

                if (IsLocalLock)
                    return true;

                if (null != Step)
                    return Step.IsLocked;

                return false;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsImmutable)
                    throw new NotSupportedException();

                // if the lock was inherited then it cannot be set explicitly
                if (IsLocked && !IsLocalLock)
                    throw new ElementLockedException();

                if (_isLocked.Value != value)
                    _isLocked.Value = value;
            }
        }
        private UndoableProperty<bool> _isLocked = new UndoableProperty<bool>(false);

        /// <inheritdoc />
        [field:NonSerialized]
        public virtual event PropertyChangedEventHandler<bool> IsLockedChanged;

        /// <summary>
        /// Raises the <see cref="IsLockedChanged"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="PageElement"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before returning.
        /// </note>
        /// </remarks>
        protected virtual void OnIsLockedChanged(PropertyChangedEventArgs<bool> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != IsLockedChanged)
                IsLockedChanged(this, e);

            OnChanged(this, "IsLockedChanged", e);
        }

        /// <inheritdoc />
        public bool IsLocalLock
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _isLocked.Value;
            }
        }

        #endregion Locking
    }
}
