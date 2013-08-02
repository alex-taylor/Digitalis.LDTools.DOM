#region License

//
// ICommentTest.cs
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
    public abstract class ICommentTest : IGroupableTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IComment); } }

        protected sealed override IGroupable CreateTestGroupable()
        {
            return CreateTestComment();
        }

        protected abstract IComment CreateTestComment();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IComment comment = CreateTestComment();

            Assert.AreEqual(DOMObjectType.Comment, comment.ObjectType);
            Assert.IsFalse(comment.IsTopLevelElement);
            Assert.IsFalse(comment.IsStateElement);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            IComment comment = CreateTestComment();
            comment.Text = "text";
            return comment;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            // Text should be preserved
            Assert.AreEqual(((IComment)original).Text, ((IComment)copy).Text);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            IComment comment = CreateTestComment();
            StringBuilder code;
            string text = "new text";
            string expected = "0 " + text + "\r\n";

            if (comment.IsImmutable)
            {
                throw new NotImplementedException("ICommentTest.ToCodeTest() not implemented for immutable objects");
            }
            else
            {
                comment.Text = text;
                code = Utils.PreProcessCode(comment.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(comment.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(comment.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
            }

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Properties

        [TestMethod]
        public void TextTest()
        {
            IComment comment    = CreateTestComment();
            string defaultValue = null;
            string newValue     = "new text";

            PropertyValueTest(comment,
                              defaultValue,
                              newValue,
                              delegate(IComment obj) { return obj.Text; },
                              delegate(IComment obj, string value) { obj.Text = value; },
                              PropertyValueFlags.None);

            comment = CreateTestComment();

            if (!comment.IsImmutable)
            {
                // null and whitespace are permitted; the latter should be converted to null
                comment.Text = null;
                Assert.IsNull(comment.Text);
                comment.Text = "  \t";
                Assert.IsNull(comment.Text);

                // trailing whitespace should be removed....
                comment.Text = "line with trailing whitespace   ";
                Assert.AreEqual("line with trailing whitespace", comment.Text);

                // ...but leading should not
                comment.Text = "   line with leading whitespace";
                Assert.AreEqual("   line with leading whitespace", comment.Text);

                // CR/LF should be removed
                comment.Text = "line with newlines\r\nsecond line";
                Assert.AreEqual("line with newlinessecond line", comment.Text);
            }
        }

        [TestMethod]
        public void TextChangedTest()
        {
            IComment comment   = CreateTestComment();
            string valueToSet  = "new text";

            PropertyChangedTest(comment,
                                "TextChanged",
                                valueToSet,
                                delegate(IComment obj, PropertyChangedEventHandler<string> handler) { obj.TextChanged += handler; },
                                delegate(IComment obj) { return obj.Text; },
                                delegate(IComment obj, string value) { obj.Text = value; });
        }

        [TestMethod]
        public void IsEmptyTest()
        {
            IComment comment = CreateTestComment();
            Assert.IsTrue(comment.IsEmpty);
            comment.Text = "text";
            Assert.IsFalse(comment.IsEmpty);
            comment.Text = "//";
            Assert.IsTrue(comment.IsEmpty);
        }

        #endregion Properties
    }
}
