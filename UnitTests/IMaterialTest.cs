#region License

//
// IMaterialTest.cs
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
    using System.IO;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    [TestClass]
    public abstract class IMaterialTest
    {
        #region Infrastructure

        protected abstract IMaterial CreateTestMaterial();

        #endregion Infrastructure

        #region Cloning

        [TestMethod]
        public virtual void CloneTest()
        {
            IMaterial material = CreateTestMaterial();
            IColour colour = new LDColour();
            colour.Material = material;

            Assert.IsNotNull(material.Colour);
            colour.Freeze();
            Assert.IsTrue(material.IsFrozen);
            Assert.IsFalse(material.IsLocked);

            IMaterial clone = material.Clone();
            Assert.IsInstanceOfType(clone, material.GetType());
            Assert.AreNotSame(material, clone);
            Assert.IsNull(clone.Colour);
            Assert.IsFalse(clone.IsLocked);
            Assert.IsFalse(clone.IsFrozen);
        }

        [TestMethod]
        public abstract void IsEquivalentToTest();

        #endregion Cloning

        #region Code-generation

        [TestMethod]
        public virtual void ToCodeTest()
        {
            IMaterial material = CreateTestMaterial();
            StringBuilder sb = new StringBuilder();

            Assert.AreSame(sb, material.ToCode(sb));
        }

        #endregion Code-generation

        #region Freezing

        [TestMethod]
        public void IsFrozenTest()
        {
            IMaterial material = CreateTestMaterial();
            IColour colour = new LDColour();

            Assert.IsFalse(material.IsFrozen);

            IMaterial oldMaterial = colour.Material;
            Assert.IsFalse(oldMaterial.IsFrozen);

            colour.Material = material;
            colour.Freeze();
            Assert.IsTrue(material.IsFrozen);
            Assert.IsFalse(oldMaterial.IsFrozen);
        }

        #endregion Freezing

        #region Locking

        [TestMethod]
        public void IsLockedTest()
        {
            IMaterial material = CreateTestMaterial();
            IColour colour = new LDColour();

            Assert.IsFalse(material.IsLocked);

            IMaterial oldMaterial = colour.Material;
            Assert.IsFalse(oldMaterial.IsLocked);

            colour.Material = material;
            colour.IsLocked = true;
            Assert.IsTrue(material.IsLocked);
            Assert.IsFalse(oldMaterial.IsLocked);

            colour.IsLocked = false;
            Assert.IsFalse(material.IsLocked);
        }

        #endregion Locking

        #region Properties

        [TestMethod]
        public void ColourTest()
        {
            IMaterial material = CreateTestMaterial();
            IColour colour = new LDColour();
            IMaterial oldMaterial = colour.Material;

            Assert.IsNull(material.Colour);
            Assert.AreEqual(colour, oldMaterial.Colour);

            material.Colour = colour;
            Assert.AreEqual(colour, material.Colour);
            Assert.IsNull(oldMaterial.Colour);
            oldMaterial.Colour = colour;

            // Colour cannot be set if the material is locked
            colour.IsLocked = true;

            try
            {
                material.Colour = colour;
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.IsNull(material.Colour);
                Assert.AreEqual(colour, oldMaterial.Colour);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            // Colour cannot be set if the material is frozen
            colour.Freeze();

            try
            {
                material.Colour = colour;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.IsNull(material.Colour);
                Assert.AreEqual(colour, oldMaterial.Colour);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }
        }

        #endregion Properties

        #region Serialization

        [TestMethod]
        public virtual void SerializeTest()
        {
            IMaterial material = CreateTestMaterial();

            using (Stream stream = Utils.SerializeBinary(material))
            {
                IMaterial result = (IMaterial)Utils.DeserializeBinary(stream);

                Assert.AreNotSame(material, result);

                // these should not be preserved unless the containing Colour is serialized too
                Assert.IsFalse(result.IsFrozen);
                Assert.IsFalse(result.IsLocked);
                Assert.IsNull(result.Colour);
            }
        }

        #endregion Serialization
    }
}
