#region License

//
// LDPauseTest.cs
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
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    [TestClass]
    public sealed class LDPauseTest : IPauseTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDPause); } }

        protected override IPause CreateTestPause()
        {
            return new LDPause();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IPause pause = CreateTestPause())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(pause.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.Pause, typeNameAttr.Description);
                Assert.AreEqual(Resources.Pause, pause.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(pause.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNull(elementFlagsAttr);

                ElementCategoryAttribute categoryAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementCategoryAttribute)) as ElementCategoryAttribute;
                Assert.IsNotNull(categoryAttr);
                Assert.AreEqual(Resources.ElementCategory_MetaCommand, categoryAttr.Description);

                Assert.AreEqual(String.Empty, pause.ExtendedDescription);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Parser

        [TestMethod]
        public void ParserTest()
        {
            bool documentModified;

            string code = "0 title\r\n" +
                          "0 Name: name.dat\r\n" +
                          "\r\n" +
                          "0 PAUSE\r\n" +
                          "0 PAUSE THIS\r\n";

            IDocument doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            IPage page    = doc[0];
            IStep step    = page[0];
            Assert.AreEqual(2, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDPause));
            Assert.IsInstanceOfType(step[1], typeof(IComment));
        }

        #endregion Parser
    }
}
