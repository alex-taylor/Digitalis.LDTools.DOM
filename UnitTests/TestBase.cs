#region License

//
// TestBase.cs
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

    #endregion Usings

    [TestClass]
    public abstract class TestBase
    {
        #region Infrastructure

        protected abstract Type TestClassType { get; }

        protected abstract Type InterfaceType { get; }

        #endregion Infrastructure

        #region Type Definition

        [TestMethod()]
        public virtual void DefinitionTest()
        {
            Assert.IsTrue(InterfaceType.IsAssignableFrom(TestClassType));
        }

        #endregion Type Definition
    }
}
