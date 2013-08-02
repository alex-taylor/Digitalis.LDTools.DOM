#region License

//
// MatteMetallicMaterialTest.cs
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
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    [TestClass]
    public sealed class MatteMetallicMaterialTest : IMaterialTest
    {
        #region Infrastructure

        protected override IMaterial CreateTestMaterial()
        {
            return new MatteMetallicMaterial();
        }

        #endregion Infrastructure

        #region Cloning

        [TestMethod]
        public override void IsEquivalentToTest()
        {
            MatteMetallicMaterial target = new MatteMetallicMaterial();

            Assert.IsTrue(target.IsEquivalentTo(new MatteMetallicMaterial()));
            Assert.IsFalse(target.IsEquivalentTo(new PlasticMaterial()));
            Assert.IsFalse(target.IsEquivalentTo(null));
        }

        #endregion Cloning

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            MatteMetallicMaterial target = new MatteMetallicMaterial();
            Assert.AreEqual("MATTE_METALLIC", target.ToCode(new StringBuilder()).ToString());

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Constructor

        [TestMethod]
        public void MatteMetallicMaterialConstructorTest()
        {
            // default ctor
            MatteMetallicMaterial target = new MatteMetallicMaterial();

            // code ctor
            target = new MatteMetallicMaterial("MATTE_METALLIC");

            // invalid code
            try
            {
                target = new MatteMetallicMaterial("METAL");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }
        }

        #endregion Constructor

        #region Self-description

        [TestMethod]
        public void DescriptionTest()
        {
            MatteMetallicMaterial target = new MatteMetallicMaterial();
            Assert.IsNotNull(target.Description);
            Assert.AreEqual(Resources.Material_MatteMetallic, target.Description);
        }

        #endregion Self-description
    }
}
