#region License

//
// MLCadGroupTest.cs
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
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using System.Reflection;

    #endregion Usings

    [TestClass]
    public sealed class MLCadGroupTest : IGroupTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(MLCadGroup); } }

        protected override IGroup CreateTestGroup()
        {
            return new MLCadGroup();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IGroup group = CreateTestGroup())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(group.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.Group, typeNameAttr.Description);
                Assert.AreEqual(Resources.Group, group.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(group.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNotNull(elementFlagsAttr);
                Assert.AreEqual(ElementFlags.HasEditor | ElementFlags.TopLevelElement, elementFlagsAttr.Flags);

                Assert.AreEqual(String.Empty, group.ExtendedDescription);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Constructor

        [TestMethod]
        public void MLCadGroupConstructorTest()
        {
            using (IGroup group = new MLCadGroup())
            {
                Assert.AreEqual(Resources.Untitled, group.Description);
                Assert.AreEqual(Resources.Untitled, group.Name);
                Assert.AreEqual(0, group.Count);
                Assert.AreEqual(new Box3d(), group.BoundingBox);
                Assert.AreEqual(Vector3d.Zero, group.Origin);
            }
        }

        [TestMethod]
        public void MLCadGroupConstructorTest1()
        {
            using (IGroup group = new MLCadGroup("name"))
            {
                Assert.AreEqual("name", group.Description);
                Assert.AreEqual("name", group.Name);
                Assert.AreEqual(0, group.Count);
                Assert.AreEqual(new Box3d(), group.BoundingBox);
                Assert.AreEqual(Vector3d.Zero, group.Origin);
            }
        }

        #endregion Constructor

        #region Parser

        [TestMethod]
        public void ParserTest()
        {
            IDocument doc;
            IPage page;
            IGroup group;
            IComment comment;
            IReference r;
            string code;
            bool documentModified;

            // single group
            code = "0 title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "\r\n" +
                   "0 MLCAD BTG group1\r\n" +
                   "0 comment\r\n" +
                   "0 GROUP 1 group1\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(1, page.Count);
            Assert.IsInstanceOfType(page[0], typeof(IStep));
            Assert.AreEqual(2, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(IComment));
            comment = (IComment)page.Elements[0];
            Assert.AreEqual("comment", comment.Text);
            Assert.IsInstanceOfType(page.Elements[1], typeof(MLCadGroup));
            group = (IGroup)page.Elements[1];
            Assert.AreEqual(1, group.Count);
            Assert.AreEqual("group1", group.Name);
            Assert.IsInstanceOfType(group.ElementAt(0), typeof(IComment));
            Assert.AreEqual(group, comment.Group);
            Assert.IsTrue(group.Contains(comment));

            // two groups, sequential
            code = "0 title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "\r\n" +
                   "0 MLCAD BTG group1\r\n" +
                   "0 comment\r\n" +
                   "0 GROUP 1 group1\r\n" +
                   "0 MLCAD BTG group2\r\n" +
                   "0 comment2\r\n" +
                   "0 GROUP 1 group2\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(1, page.Count);
            Assert.IsInstanceOfType(page[0], typeof(IStep));
            Assert.AreEqual(4, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(IComment));
            comment = page.Elements[0] as IComment;
            Assert.AreEqual("comment", comment.Text);
            group = page.Elements[1] as MLCadGroup;
            Assert.AreEqual("group1", group.Name);
            Assert.AreEqual(1, group.Count);
            Assert.IsInstanceOfType(group.ElementAt(0), typeof(IComment));
            comment = group.ElementAt(0) as IComment;
            Assert.AreEqual("comment", comment.Text);
            Assert.AreEqual(group, comment.Group);
            Assert.IsInstanceOfType(page.Elements[2], typeof(IComment));
            comment = page.Elements[2] as IComment;
            Assert.AreEqual("comment2", comment.Text);
            Assert.IsInstanceOfType(page.Elements[3], typeof(MLCadGroup));
            group = page.Elements[3] as MLCadGroup;
            Assert.AreEqual(1, group.Count);
            Assert.AreEqual("group2", group.Name);
            Assert.IsInstanceOfType(group.ElementAt(0), typeof(IComment));
            comment = group.ElementAt(0) as IComment;
            Assert.AreEqual("comment2", comment.Text);
            Assert.AreEqual(group, comment.Group);

            // two groups, interleaved
            code = "0 title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "\r\n" +
                   "0 MLCAD BTG group1\r\n" +
                   "0 comment\r\n" +
                   "0 MLCAD BTG group2\r\n" +
                   "0 comment2\r\n" +
                   "0 MLCAD BTG group1\r\n" +
                   "0 comment3\r\n" +
                   "0 GROUP 1 group1\r\n" +
                   "0 GROUP 1 group2\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(1, page.Count);
            Assert.IsInstanceOfType(page[0], typeof(IStep));
            Assert.AreEqual(5, page.Elements.Count);
            comment = page.Elements[0] as IComment;
            Assert.AreEqual("comment", comment.Text);
            comment = page.Elements[1] as IComment;
            Assert.AreEqual("comment2", comment.Text);
            comment = page.Elements[2] as IComment;
            Assert.AreEqual("comment3", comment.Text);
            group = page.Elements[3] as MLCadGroup;
            Assert.AreEqual("group1", group.Name);
            Assert.AreEqual(2, group.Count);
            comment = group.ElementAt(0) as IComment;
            Assert.AreEqual("comment", comment.Text);
            comment = group.ElementAt(1) as IComment;
            Assert.AreEqual("comment3", comment.Text);
            group = page.Elements[4] as MLCadGroup;
            Assert.AreEqual("group2", group.Name);
            Assert.AreEqual(1, group.Count);
            comment = group.ElementAt(0) as IComment;
            Assert.AreEqual("comment2", comment.Text);

            // two groups, interleaved with attributes
            code = "0 title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "\r\n" +
                   "0 MLCAD BTG group1\r\n" +
                   "0 BFC INVERTNEXT\r\n" +
                   "0 MLCAD BTG group2\r\n" +
                   "0 comment2\r\n" +
                   "0 MLCAD BTG group1\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "0 GROUP 1 group1\r\n" +
                   "0 GROUP 1 group2\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(1, page.Count);
            Assert.IsInstanceOfType(page[0], typeof(IStep));
            Assert.AreEqual(4, page.Elements.Count);
            comment = page.Elements[0] as IComment;
            Assert.AreEqual("comment2", comment.Text);
            r = page.Elements[1] as IReference;
            Assert.IsTrue(r.Invert);
            Assert.IsInstanceOfType(page.Elements[2], typeof(MLCadGroup));
            group = page.Elements[2] as MLCadGroup;
            Assert.AreEqual(1, group.Count);
            Assert.AreEqual("group1", group.Name);
            Assert.IsInstanceOfType(group.ElementAt(0), typeof(IReference));
            Assert.IsInstanceOfType(page.Elements[3], typeof(MLCadGroup));
            group = page.Elements[3] as MLCadGroup;
            Assert.AreEqual(1, group.Count);
            Assert.AreEqual("group2", group.Name);
            comment = group.ElementAt(0) as IComment;
            Assert.AreEqual("comment2", comment.Text);

            // group with members in multiple steps
            code = "0 title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "\r\n" +
                   "0 MLCAD BTG group1\r\n" +
                   "0 comment\r\n" +
                   "0 STEP\r\n" +
                   "0 MLCAD BTG group1\r\n" +
                   "0 comment2\r\n" +
                   "0 STEP\r\n" +
                   "0 GROUP 1 group1\r\n" +
                   "0 STEP\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(3, page.Count);
            Assert.AreEqual(3, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(IComment));
            Assert.IsInstanceOfType(page.Elements[1], typeof(IComment));
            Assert.IsInstanceOfType(page.Elements[2], typeof(MLCadGroup));
            group = page.Elements[2] as MLCadGroup;
            Assert.AreEqual(2, group.Count);
            Assert.AreSame(group, ((IGroupable)page.Elements[0]).Group);
            Assert.AreSame(group, ((IGroupable)page.Elements[1]).Group);
        }

        #endregion Parser
    }
}
