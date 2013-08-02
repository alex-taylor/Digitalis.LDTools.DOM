#region License

//
// MockGroupable.cs
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
    public class MockGroupable : Groupable
    {
        public override bool IsStateElement { get { return false; } }

        public override bool IsTopLevelElement { get { return true; } }

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

        public override Image Icon { get { return null; } }

        public override string TypeName { get { return "type name"; } }

        public override string Description { get { return "description"; } }

        public override string ExtendedDescription { get { return "extended description"; } }

        public override DOMObjectType ObjectType { get { return DOMObjectType.MetaCommand; } }

        public override bool IsImmutable { get { return false; } }

        protected override StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            base.GenerateCode(sb, codeFormat, overrideColour, ref transform, winding);
            sb.Append("line1\r\nline2\r\n");
            return sb;
        }
    }
}
