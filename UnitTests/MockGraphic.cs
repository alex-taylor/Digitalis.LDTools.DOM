#region License

//
// MockGraphic.cs
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
    using System.Collections.Generic;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    [Serializable]
    public class MockGraphic : Graphic
    {
        public MockGraphic()
        {
        }

        public MockGraphic(bool disableColourValue)
        {
            _colourValueEnabled = false;
        }

        public MockGraphic(uint colourValue)
            : base(colourValue)
        {
        }

        public MockGraphic(uint colourValue, IEnumerable<Vector3d> coordinates)
            : base(colourValue, coordinates)
        {
        }

        public override uint OverrideableColourValue
        {
            get { return Palette.MainColour; }
        }

        public override bool ColourValueEnabled
        {
            get { return _colourValueEnabled; }
        }
        private bool _colourValueEnabled = true;

        public override uint CoordinatesCount
        {
            get { return 3; }
        }

        public override Vector3d Origin
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                throw new NotImplementedException();
            }
        }

        public override void ReverseWinding()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsFrozen)
                throw new ObjectFrozenException();

            if (IsLocked)
                throw new ElementLockedException();

            throw new NotImplementedException();
        }

        public override bool IsStateElement
        {
            get { return true; }
        }

        public override bool IsTopLevelElement
        {
            get { return false; }
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
            get { return DOMObjectType.Line; }
        }

        public override bool IsImmutable { get { return false; } }

        protected override StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            base.GenerateCode(sb, codeFormat, overrideColour, ref transform, winding);

            if (CodeStandards.PartsLibrary != codeFormat || (IsVisible && !IsGhosted))
                sb.Append("graphic\r\n");

            return sb;
        }
    }
}
