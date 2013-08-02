#region License

//
// MockDocumentElement.cs
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
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    [Serializable]
    public class MockDocumentElement : DocumentElement
    {
        public MockDocumentElement()
        {
        }

        public MockDocumentElement(IDocument document)
        {
            _document = document;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && null != _document && _document.IsFrozen)
                throw new ObjectFrozenException();

            base.Dispose(disposing);
        }

        public override IDocument Document
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _document;
            }
        }
        [NonSerialized]
        private IDocument _document;

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

        public override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return sb;
        }

        public override DOMObjectType ObjectType
        {
            get { return DOMObjectType.Comment; }
        }

        public override bool IsImmutable { get { return false; } }
    }
}
