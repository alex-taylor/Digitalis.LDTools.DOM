#region License

//
// LDUpdateTest.cs
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

    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;

    #endregion Usings

    [TestClass]
    public class LDUpdateTest
    {
        #region Definition Test

        [TestMethod]
        public void DefinitionTest()
        {
            Assert.IsTrue(typeof(LDUpdate).IsSealed);
            Assert.IsTrue(typeof(LDUpdate).IsSerializable);
        }

        #endregion Definition Test

        #region Constructor

        [TestMethod]
        public void LDrawUpdateConstructorForOriginalPartsTest()
        {
            LDUpdate target = new LDUpdate();

            Assert.AreEqual(0U, target.Year);
            Assert.AreEqual(0U, target.Release);
            Assert.IsTrue(target.IsOriginal);
            Assert.AreEqual("ORIGINAL", target.ToCode());
        }

        [TestMethod]
        public void LDrawUpdateConstructorForUpdatePartsTest()
        {
            uint year = 2012;
            uint release = 1;
            LDUpdate target = new LDUpdate(year, release);

            Assert.AreEqual(year, target.Year);
            Assert.AreEqual(release, target.Release);
            Assert.IsFalse(target.IsOriginal);
            Assert.AreEqual("UPDATE 2012-01", target.ToCode());
        }

        [TestMethod]
        public void LDrawUpdateConstructorWithCodeTest()
        {
            string code = "UPDATE 2012-01";
            LDUpdate target = new LDUpdate(code);
            Assert.AreEqual(2012U, target.Year);
            Assert.AreEqual(1U, target.Release);
            Assert.IsFalse(target.IsOriginal);
            Assert.AreEqual(code, target.ToCode());

            code = "ORIGINAL";
            target = new LDUpdate(code);
            Assert.AreEqual(0U, target.Year);
            Assert.AreEqual(0U, target.Release);
            Assert.IsTrue(target.IsOriginal);
            Assert.AreEqual(code, target.ToCode());
        }

        #endregion Constructor

        #region Serialization

        [TestMethod]
        public void SerializeTest()
        {
            //LDUpdate target = new LDUpdate();

            // TODO: SerializeTest()
        }

        #endregion Serialization
    }
}
