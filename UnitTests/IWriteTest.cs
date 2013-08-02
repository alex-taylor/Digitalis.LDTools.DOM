#region License

//
// IWriteTest.cs
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
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IWriteTest : IMetaCommandTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IWrite); } }

        protected sealed override IMetaCommand CreateTestMetaCommand()
        {
            return CreateTestWrite();
        }

        protected abstract IWrite CreateTestWrite();

        protected sealed override string[] SyntaxExamples
        {
            get
            {
                return new string[]
                {
                    "0 WRITE\r\n",
                    "0 WRITE message\r\n"
                };
            }
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IWrite write = CreateTestWrite();

            Assert.AreEqual(DOMObjectType.MetaCommand, write.ObjectType);
            Assert.IsFalse(write.IsTopLevelElement);
            Assert.IsFalse(write.IsStateElement);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            IWrite write = CreateTestWrite();
            write.Text = "write";
            return write;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            // Text should be preserved
            Assert.AreEqual(((IWrite)original).Text, ((IWrite)copy).Text);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            IWrite write = CreateTestWrite();
            StringBuilder code;
            string text = "new text";
            string expected = "0 WRITE " + text + "\r\n";

            if (write.IsImmutable)
            {
                throw new NotImplementedException("IWriteTest.ToCodeTest() not implemented for immutable objects");
            }
            else
            {
                // WRITE not allowed in PartsLibrary mode
                write.Text = text;
                code = Utils.PreProcessCode(write.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(write.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(write.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(String.Empty, code.ToString());
            }

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Properties

        [TestMethod]
        public void TextTest()
        {
            IWrite write        = CreateTestWrite();
            string defaultValue = null;
            string newValue     = "new text";

            PropertyValueTest(write,
                              defaultValue,
                              newValue,
                              delegate(IWrite obj) { return obj.Text; },
                              delegate(IWrite obj, string value) { obj.Text = value; },
                              PropertyValueFlags.None);

            write = CreateTestWrite();

            if (!write.IsImmutable)
            {
                // null and whitespace are permitted; the latter should be converted to null
                write.Text = null;
                Assert.IsNull(write.Text);
                write.Text = "  \t";
                Assert.IsNull(write.Text);

                // trailing whitespace should be removed....
                write.Text = "line with trailing whitespace   ";
                Assert.AreEqual("line with trailing whitespace", write.Text);

                // ...but leading should not
                write.Text = "   line with leading whitespace";
                Assert.AreEqual("   line with leading whitespace", write.Text);

                // CR/LF should be removed
                write.Text = "line with newlines\r\nsecond line";
                Assert.AreEqual("line with newlinessecond line", write.Text);
            }
        }

        [TestMethod]
        public void TextChangedTest()
        {
            IWrite write      = CreateTestWrite();
            string valueToSet = "new text";

            PropertyChangedTest(write,
                                "TextChanged",
                                valueToSet,
                                delegate(IWrite obj, PropertyChangedEventHandler<string> handler) { obj.TextChanged += handler; },
                                delegate(IWrite obj) { return obj.Text; },
                                delegate(IWrite obj, string value) { obj.Text = value; });
        }

        #endregion Properties
    }
}
