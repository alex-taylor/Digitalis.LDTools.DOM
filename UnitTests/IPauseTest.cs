#region License

//
// IPauseTest.cs
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
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    [TestClass]
    public abstract class IPauseTest : IMetaCommandTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IPause); } }

        protected sealed override IMetaCommand CreateTestMetaCommand()
        {
            return CreateTestPause();
        }

        protected abstract IPause CreateTestPause();

        protected sealed override string[] SyntaxExamples
        {
            get
            {
                return new string[]
                {
                    "0 PAUSE\r\n"
                };
            }
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IPause pause = CreateTestPause();

            Assert.AreEqual(DOMObjectType.MetaCommand, pause.ObjectType);
            Assert.IsFalse(pause.IsTopLevelElement);
            Assert.IsFalse(pause.IsStateElement);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            IPause pause = CreateTestPause();
            StringBuilder code;
            string expected = "0 PAUSE\r\n";

            // PAUSE not allowed in PartsLibrary mode
            code = Utils.PreProcessCode(pause.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected, code.ToString());
            code = Utils.PreProcessCode(pause.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected, code.ToString());
            code = Utils.PreProcessCode(pause.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(String.Empty, code.ToString());

            base.ToCodeTest();
        }

        #endregion Code-generation
    }
}
