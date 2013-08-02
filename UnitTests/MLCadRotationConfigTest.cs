﻿#region License

//
// MLCadRotationConfigTest.cs
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
    public sealed class MLCadRotationConfigTest
    {
        #region Definition Test

        [TestMethod]
        public void DefinitionTest()
        {
            Assert.IsTrue(typeof(MLCadRotationConfig).IsSealed);
            Assert.IsTrue(typeof(MLCadRotationConfig).IsSerializable);
        }

        #endregion Definition Test

        #region Serialization

        [TestMethod]
        public void SerializeTest()
        {
            //MLCadRotationConfig target = new MLCadRotationConfig();

            // TODO: SerializeTest()
        }

        #endregion Serialization
    }
}