#region License

//
// PlasticMaterialTest.cs
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
    public sealed class PlasticMaterialTest : IMaterialTest
    {
        #region Infrastructure

        protected override IMaterial CreateTestMaterial()
        {
            return new PlasticMaterial();
        }

        #endregion Infrastructure

        #region Cloning

        [TestMethod]
        public override void IsEquivalentToTest()
        {
            PlasticMaterial target = new PlasticMaterial();

            Assert.IsTrue(target.IsEquivalentTo(new PlasticMaterial()));
            Assert.IsFalse(target.IsEquivalentTo(new RubberMaterial()));
            Assert.IsFalse(target.IsEquivalentTo(null));
        }

        #endregion Cloning

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            PlasticMaterial target = new PlasticMaterial();
            Assert.AreEqual(String.Empty, target.ToCode(new StringBuilder()).ToString());

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Constructor

        [TestMethod]
        public void PlasticMaterialConstructorTest()
        {
            // default ctor
            PlasticMaterial target = new PlasticMaterial();

            // code ctor
            target = new PlasticMaterial("");
            target = new PlasticMaterial(null);
            target = new PlasticMaterial("  ");

            // invalid code
            try
            {
                target = new PlasticMaterial("METAL");
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
            PlasticMaterial target = new PlasticMaterial();
            Assert.IsNotNull(target.Description);
            Assert.AreEqual(Resources.Material_Plastic, target.Description);
        }

        #endregion Self-description
    }
}
