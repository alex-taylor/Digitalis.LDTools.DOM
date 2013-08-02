#region License

//
// TexmapTextureGeometryTest.cs
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

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    [TestClass]
    public sealed class TexmapTextureGeometryTest : ITexmapGeometryTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return new LDTexmap().TextureGeometry.GetType(); } }

        protected override ITexmapGeometry CreateTestTexmapGeometry()
        {
            ITexmap texmap = new LDTexmap();
            return texmap.TextureGeometry;
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            Assert.AreEqual(TexmapGeometryType.Texture, geometry.GeometryType);
            Assert.IsFalse(geometry.IsImmutable);
            Assert.IsNotNull(geometry.Icon);
            Assert.AreEqual(Resources.Geometry_Texture, geometry.TypeName);
            Assert.AreEqual(String.Empty, geometry.Description);
            Assert.AreEqual(String.Empty, geometry.ExtendedDescription);

            base.DefinitionTest();
        }

        #endregion Definition Test
    }
}
