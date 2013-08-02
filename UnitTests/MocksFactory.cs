#region License

//
// MocksFactory.cs
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

    #endregion Usings

    public static class MocksFactory
    {
        #region Abstract types

        public static IDOMObject CreateMockDOMObject() { return new MockDOMObject(); }
        public static IDocumentElement CreateMockDocumentElement() { return new MockDocumentElement(); }
        public static IPageElement CreateMockPageElement() { return new MockPageElement(); }
        public static IElement CreateMockElement() { return new MockElement(); }
        public static IGroupable CreateMockGroupable() { return new MockGroupable(); }
        public static IGraphic CreateMockGraphic() { return new MockGraphic(); }
        public static IElementCollection CreateMockElementCollection() { return new MockElementCollection(); }

        #endregion Abstract types

        #region Structural types

        public static IDocument CreateMockDocument() { return new LDDocument(); }
        public static IPage CreateMockPage() { return new LDPage(); }
        public static IStep CreateMockStep() { return new LDStep(); }
        public static IGroup CreateMockGroup() { return new MLCadGroup(); }

        #endregion Structural types

        #region Comment

        public static IComment CreateMockComment() { return new LDComment(); }

        #endregion Comment

        #region Meta-commands

        public static IBFCFlag CreateMockBFCFlag() { return new LDBFCFlag(); }
        public static IClear CreateMockClear() { return new LDClear(); }
        public static IColour CreateMockColour() { return new LDColour(); }
        public static IPause CreateMockPause() { return new LDPause(); }
        public static ISave CreateMockSave() { return new LDSave(); }
        public static IWrite CreateMockWrite() { return new LDWrite(); }

        #endregion Meta-commands

        #region Graphic types

        public static IReference CreateMockReference() { return new LDReference(); }
        public static ILine CreateMockLine() { return new LDLine(); }
        public static ITriangle CreateMockTriangle() { return new LDTriangle(); }
        public static IQuadrilateral CreateMockQuadrilateral() { return new LDQuadrilateral(); }
        public static IOptionalLine CreateMockOptionalLine() { return new LDOptionalLine(); }
        public static ITexmap CreateMockTexmap() { return new LDTexmap(); }

        #endregion Graphic types
    }
}
