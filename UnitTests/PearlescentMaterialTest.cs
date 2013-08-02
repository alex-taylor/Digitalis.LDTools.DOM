#region License

//
// PearlescentMaterialTest.cs
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
    public sealed class PearlescentMaterialTest : IMaterialTest
    {
        #region Infrastructure

        protected override Digitalis.LDTools.DOM.API.IMaterial CreateTestMaterial()
        {
            return new PearlescentMaterial();
        }

        #endregion Infrastructure

        #region Cloning

        [TestMethod]
        public override void IsEquivalentToTest()
        {
            PearlescentMaterial target = new PearlescentMaterial();

            Assert.IsTrue(target.IsEquivalentTo(new PearlescentMaterial()));
            Assert.IsFalse(target.IsEquivalentTo(new PlasticMaterial()));
            Assert.IsFalse(target.IsEquivalentTo(null));
        }

        #endregion Cloning

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            PearlescentMaterial target = new PearlescentMaterial();
            Assert.AreEqual("PEARLESCENT", target.ToCode(new StringBuilder()).ToString());

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Constructor

        [TestMethod]
        public void PearlescentMaterialConstructorTest()
        {
            // default ctor
            PearlescentMaterial target = new PearlescentMaterial();

            // code ctor
            target = new PearlescentMaterial("PEARLESCENT");

            // invalid code
            try
            {
                target = new PearlescentMaterial("METAL");
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
            PearlescentMaterial target = new PearlescentMaterial();
            Assert.IsNotNull(target.Description);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.Material_Pearlescent, target.Description);
        }

        #endregion Self-description
    }
}
