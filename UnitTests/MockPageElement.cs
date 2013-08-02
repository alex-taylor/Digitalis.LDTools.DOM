#region License

//
// MockPageElement.cs
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

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [Serializable]
    public class MockPageElement : PageElement
    {
        public MockPageElement()
        {
        }

        public MockPageElement(IDocument document, IPage page, IStep step)
        {
            if (null != document && null != page)
                document.Add(page);

            if (null != page && null != step)
                page.Add(step);

            if (null != page)
                page.PathToDocumentChanged += delegate(object sender, EventArgs e) { OnPathToDocumentChanged(e); };

            _step = step;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && null != _step && _step.IsFrozen)
                throw new ObjectFrozenException();

            base.Dispose(disposing);
        }

        public override IStep Step
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _step;
            }
        }

        [NonSerialized]
        private IStep _step;

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

        public override System.Drawing.Image Icon
        {
            get { return null; }
        }

        public override string TypeName
        {
            get { return "type name"; }
        }

        public override string Description
        {
            get { return "description"; }
        }

        public override string ExtendedDescription
        {
            get { return "extended description"; }
        }

        public override DOMObjectType ObjectType
        {
            get { return DOMObjectType.Comment; }
        }

        public override bool IsImmutable { get { return false; } }
    }
}
