#region License

//
// DocumentElement.cs
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
    using System.Drawing;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;

    #endregion Usings

    /// <summary>
    /// Abstract implementation of <see cref="Digitalis.LDTools.DOM.API.IDocumentElement"/>.
    /// </summary>
    [Serializable]
    public abstract class DocumentElement : DOMObject, IDocumentElement
    {
        #region Analytics

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: if the subclass is capable of detecting problems with the values input by the user - for
        /// example, violations of the LDraw specification, or combinations of settings which are mutually exclusive - it should
        /// override both this and <see cref="Analyse"/>. The value returned should be OR'd with the value returned by the
        /// superclass.
        /// </note>
        /// </remarks>
        public virtual bool HasProblems(CodeStandards mode)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return false;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: if the subclass is capable of detecting problems with the values input by the user - for
        /// example, violations of the LDraw specification, or combinations of settings which are mutually exclusive - it should
        /// override both this and <see cref="HasProblems"/>. For each problem detected, an
        /// <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/> should be added to the collection returned by
        /// the superclass.
        /// </note>
        /// </remarks>
        public virtual ICollection<IProblemDescriptor> Analyse(CodeStandards mode)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return new List<IProblemDescriptor>();
        }

        #endregion Analytics

        #region Disposal

        /// <inheritdoc />
        public override bool IsDisposing
        {
            get
            {
                IDocument document = Document;

                if (null != document && document.IsDisposing)
                    return true;

                return base.IsDisposing;
            }
        }

        #endregion Disposal

        #region Document-tree

        /// <inheritdoc />
        public abstract IDocument Document { get; }

        /// <inheritdoc />
        [field:NonSerialized]
        public event EventHandler PathToDocumentChanged;

        /// <summary>
        /// Raises the <see cref="PathToDocumentChanged"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="DocumentElement"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before returning.
        /// </note>
        /// </remarks>
        protected virtual void OnPathToDocumentChanged(EventArgs e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != PathToDocumentChanged)
                PathToDocumentChanged(this, e);
        }

        // used by the unit-tests
        internal int PathToDocumentChangedSubscribers
        {
            get
            {
                if (null == PathToDocumentChanged)
                    return 0;

                return PathToDocumentChanged.GetInvocationList().Length;
            }
        }

        #endregion Document-tree

        #region Editor

        /// <inheritdoc />
        public abstract bool HasEditor { get; }

        /// <inheritdoc />
        public abstract IElementEditor GetEditor();

        #endregion Editor

        #region Freezing

        /// <inheritdoc />
        public override bool IsFrozen
        {
            get
            {
                if (base.IsFrozen)
                    return true;

                IDocument document = Document;

                if (null != document)
                    return document.IsFrozen;

                return false;
            }
        }

        /// <inheritdoc />
        protected override void OnFreezing(EventArgs e)
        {
            IDocument document = Document;

            if (null != document)
                document.Freeze();

            base.OnFreezing(e);
        }

        #endregion Freezing

        #region Self-description

        /// <inheritdoc />
        public abstract Image Icon { get; }

        /// <inheritdoc />
        public abstract string TypeName { get; }

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public abstract string ExtendedDescription { get; }

        #endregion Self-description
    }
}
