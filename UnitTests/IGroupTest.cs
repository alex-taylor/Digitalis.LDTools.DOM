#region License

//
// IGroupTest.cs
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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IGroupTest : IElementTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IGroup); } }

        protected sealed override IElement CreateTestElement()
        {
            return CreateTestGroup();
        }

        protected abstract IGroup CreateTestGroup();

        private IGroup CreateTestGroupWithMembers()
        {
            IGroup group = CreateTestGroup();
            IStep step   = MocksFactory.CreateMockStep();

            step.Add(group);

            IGroupable element = MocksFactory.CreateMockComment();
            step.Add(element);
            element.Group = group;

            return group;
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IGroup group = CreateTestGroup();
            Assert.IsTrue(group.IsTopLevelElement);
            Assert.IsFalse(group.IsStateElement);
            Assert.IsFalse(group.IsReadOnly);
            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            return CreateTestGroupWithMembers();
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IGroup first  = (IGroup)original;
            IGroup second = (IGroup)copy;

            // Name should be preserved
            Assert.AreEqual(first.Name, second.Name);

            // Count should become zero as group members are not serialized
            Assert.AreEqual(0, second.Count);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            StringBuilder code;

            // empty group
            IGroup group = CreateTestGroup();
            code = Utils.PreProcessCode(group.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(String.Empty, code.ToString());
            code = Utils.PreProcessCode(group.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(String.Empty, code.ToString());
            code = Utils.PreProcessCode(group.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(String.Empty, code.ToString());

            group = CreateTestGroupWithMembers();
            string groupCode = "0 GROUP " + group.Count + " " + group.Name + "\r\n";

            // GROUP not allowed in PartsLibrary mode
            code = Utils.PreProcessCode(group.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(groupCode, code.ToString());
            code = Utils.PreProcessCode(group.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(String.Empty, code.ToString());
            code = Utils.PreProcessCode(group.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(groupCode, code.ToString());

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Collection-management

        [TestMethod]
        public void CountTest()
        {
            IGroup group = CreateTestGroup();
            IStep step   = MocksFactory.CreateMockStep();

            step.Add(group);
            Assert.AreEqual(0, group.Count);

            IComment comment = MocksFactory.CreateMockComment();
            step.Add(comment);
            comment.Group = group;
            Assert.AreEqual(1, group.Count);

            ILine line = MocksFactory.CreateMockLine();
            step.Add(line);
            line.Group = group;
            Assert.AreEqual(2, group.Count);

            line.Group = null;
            Assert.AreEqual(1, group.Count);

            step.Remove(comment);
            Assert.AreEqual(0, group.Count);

            Utils.DisposalAccessTest(group, delegate() { int count = group.Count; });
        }

        [TestMethod]
        public void ContainsTest()
        {
            IGroup group = CreateTestGroup();
            IPage page   = MocksFactory.CreateMockPage();
            IStep step   = MocksFactory.CreateMockStep();

            page.Add(step);
            step.Add(group);

            IComment comment = MocksFactory.CreateMockComment();
            step.Add(comment);
            comment.Group = group;
            Assert.IsTrue(group.Contains(comment));

            ILine line = MocksFactory.CreateMockLine();
            step.Add(line);
            line.Group = group;
            Assert.IsTrue(group.Contains(line));

            line.Group = null;
            Assert.IsFalse(group.Contains(line));

            step.Remove(comment);
            Assert.IsFalse(group.Contains(comment));

            step.Add(comment);
            comment.Group = group;
            Assert.IsTrue(group.Contains(comment));

            // removing the group should break the links to its members
            step.Remove(group);
            Assert.IsFalse(group.Contains(comment));
            step.Add(group);
            comment.Group = group;

            // removing the step which contains both group and comment should not break the link between them
            page.Remove(step);
            Assert.IsTrue(step.Contains(group));
            Assert.IsTrue(group.Contains(comment));
            page.Add(step);

            step.Remove(comment);
            Assert.IsFalse(group.Contains(comment));

            // but putting the group and comment in different steps should break the link when either step is removed
            step = MocksFactory.CreateMockStep();
            step.Add(comment);
            Assert.IsFalse(group.Contains(comment));
            page.Add(step);
            comment.Group = group;
            Assert.IsTrue(group.Contains(comment));
            page.Remove(step);
            Assert.IsFalse(group.Contains(comment));
            page.Add(step);
            comment.Group = group;
            Assert.IsTrue(group.Contains(comment));
            step = page[0];
            page.Remove(step);
            Assert.IsFalse(group.Contains(comment));

            // re-adding the group should not reinstate the link
            page.Add(step);
            Assert.IsFalse(group.Contains(comment));

            // check the undo-system functions
            comment.Group = group;
            Assert.IsTrue(group.Contains(comment));
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            page.Remove(step);
            undoStack.EndCommand();
            Assert.IsFalse(group.Contains(comment));
            undoStack.Undo();
            Assert.IsTrue(group.Contains(comment));
            undoStack.Redo();
            Assert.IsFalse(group.Contains(comment));

            Utils.DisposalAccessTest(group, delegate() { bool contains = group.Contains(comment); });
        }

        [TestMethod]
        public void CopyToArrayTest()
        {
            IGroup group       = CreateTestGroup();
            IPage page         = MocksFactory.CreateMockPage();
            IStep step         = MocksFactory.CreateMockStep();
            IGroupable[] array = new IGroupable[2];

            page.Add(step);
            step.Add(group);

            try
            {
                group.CopyTo(null, 0);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentNullException), e.GetType());
            }

            try
            {
                group.CopyTo(array, -1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            IComment comment = MocksFactory.CreateMockComment();
            step.Add(comment);
            comment.Group = group;

            ILine line = MocksFactory.CreateMockLine();
            step.Add(line);
            line.Group = group;

            group.CopyTo(array, 0);
            Assert.AreSame(comment, array[0]);
            Assert.AreSame(line, array[1]);

            // array too small
            try
            {
                group.CopyTo(array, 1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentException), e.GetType());
            }

            Utils.DisposalAccessTest(group, delegate() { group.CopyTo(array, 0); });
        }

        [TestMethod]
        public void AddTest()
        {
            IGroup group     = CreateTestGroup();
            IStep step       = MocksFactory.CreateMockStep();
            IComment comment = MocksFactory.CreateMockComment();

            step.Add(group);
            step.Add(comment);

            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            group.Add(comment);
            undoStack.EndCommand();
            Assert.AreEqual(1, group.Count);
            Assert.AreSame(group, comment.Group);
            Assert.IsTrue(group.Contains(comment));

            undoStack.Undo();
            Assert.AreEqual(0, group.Count);
            Assert.IsNull(comment.Group);
            Assert.IsFalse(group.Contains(comment));

            undoStack.Redo();
            Assert.AreEqual(1, group.Count);
            Assert.AreSame(group, comment.Group);
            Assert.IsTrue(group.Contains(comment));

            try
            {
                group.Add(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentNullException), e.GetType());
            }

            // cannot add an IElement which is already a member of an IGroup
            try
            {
                group.Add(comment);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            Utils.DisposalAccessTest(group, delegate() { group.Add(comment); });
        }

        [TestMethod]
        public void RemoveTest()
        {
            IGroup group       = CreateTestGroupWithMembers();
            IGroupable element = group.First();
            int count          = group.Count;

            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            Assert.IsTrue(group.Remove(element));
            undoStack.EndCommand();
            Assert.AreEqual(count - 1, group.Count);
            Assert.IsNull(element.Group);
            Assert.IsFalse(group.Contains(element));

            undoStack.Undo();
            Assert.AreEqual(count, group.Count);
            Assert.AreSame(group, element.Group);
            Assert.IsTrue(group.Contains(element));

            undoStack.Redo();
            Assert.AreEqual(count - 1, group.Count);
            Assert.IsNull(element.Group);
            Assert.IsFalse(group.Contains(element));

            try
            {
                group.Remove(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentNullException), e.GetType());
            }

            // cannot remove an IElement which is not a member of the IGroup
            Assert.IsFalse(group.Remove(element));

            Utils.DisposalAccessTest(group, delegate() { group.Remove(element); });
        }

        [TestMethod]
        public void ClearTest()
        {
            IGroup group = CreateTestGroupWithMembers();
            int count    = group.Count;

            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            group.Clear();
            undoStack.EndCommand();
            Assert.AreEqual(0, group.Count);

            undoStack.Undo();
            Assert.AreEqual(count, group.Count);

            undoStack.Redo();
            Assert.AreEqual(0, group.Count);

            Utils.DisposalAccessTest(group, delegate() { group.Clear(); });
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            IGroup group = CreateTestGroupWithMembers();
            int count = 0;

            foreach (IGroupable element in group)
            {
                count++;
            }

            Assert.AreEqual(group.Count, count);

            Utils.DisposalAccessTest(group, delegate() { foreach (IGroupable g in group) { } });
        }

        #endregion Collection-management

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            IGroup group = CreateTestGroupWithMembers();
            List<IGroupable> members = new List<IGroupable>(group);

            Assert.AreEqual(group.Count, members.Count);

            foreach (IGroupable element in members)
            {
                Assert.AreSame(group, element.Group);
            }

            group.Dispose();

            foreach (IGroupable element in members)
            {
                Assert.IsNull(element.Group);
            }

            base.DisposeTest();
        }

        #endregion Disposal

        #region Geometry

        [TestMethod]
        public void BoundingBoxTest()
        {
            IGroup group = CreateTestGroup();
            IStep step   = MocksFactory.CreateMockStep();

            step.Add(group);

            // empty group
            Assert.AreEqual(new Box3d(), group.BoundingBox);

            // add some geometry
            ITriangle triangle = MocksFactory.CreateMockTriangle();
            triangle.Vertex1   = new Vector3d(10, 0, 0);
            triangle.Vertex2   = new Vector3d(20, 0, 0);
            triangle.Vertex3   = new Vector3d(20, 10, 0);
            step.Add(triangle);
            triangle.Group = group;
            Assert.AreSame(group, triangle.Group);
            Assert.AreEqual(1, group.Count);
            Assert.AreEqual(new Box3d(10, 0, 0, 20, 10, 0), group.BoundingBox);
            Assert.AreEqual(step.BoundingBox, group.BoundingBox);

            Utils.DisposalAccessTest(group, delegate() { Box3d box = group.BoundingBox; });
        }

        [TestMethod]
        public void OriginTest()
        {
            IGroup group = CreateTestGroup();
            IStep step   = MocksFactory.CreateMockStep();

            step.Add(group);

            // empty group
            Assert.AreEqual(Vector3d.Zero, group.Origin);

            // add some geometry
            ITriangle triangle = MocksFactory.CreateMockTriangle();
            triangle.Vertex1   = new Vector3d(10, 0, 0);
            triangle.Vertex2   = new Vector3d(20, 0, 0);
            triangle.Vertex3   = new Vector3d(20, 10, 0);
            step.Add(triangle);
            triangle.Group = group;
            Assert.AreEqual(new Vector3d(15, 5, 0), group.Origin);

            Utils.DisposalAccessTest(group, delegate() { Vector3d origin = group.Origin; });
        }

        [TestMethod]
        public void TransformTest()
        {
            IGroup group = CreateTestGroup();
            IStep step   = MocksFactory.CreateMockStep();

            step.Add(group);

            ITriangle triangle = MocksFactory.CreateMockTriangle();
            triangle.Vertex1   = new Vector3d(10, 0, 0);
            triangle.Vertex2   = new Vector3d(20, 0, 0);
            triangle.Vertex3   = new Vector3d(20, 10, 0);
            step.Add(triangle);
            triangle.Group = group;

            Matrix4d transform = Matrix4d.Scale(5);
            group.Transform(ref transform);
            Assert.AreEqual(new Box3d(50, 0, 0, 100, 50, 0), group.BoundingBox);
            Assert.AreEqual(new Vector3d(50, 0, 0), triangle.Vertex1);
            Assert.AreEqual(new Vector3d(100, 0, 0), triangle.Vertex2);
            Assert.AreEqual(new Vector3d(100, 50, 0), triangle.Vertex3);

            // cannot transform a locked group
            group.IsLocked = true;

            try
            {
                group.Transform(ref transform);
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(new Box3d(50, 0, 0, 100, 50, 0), group.BoundingBox);
                Assert.AreEqual(new Vector3d(50, 0, 0), triangle.Vertex1);
                Assert.AreEqual(new Vector3d(100, 0, 0), triangle.Vertex2);
                Assert.AreEqual(new Vector3d(100, 50, 0), triangle.Vertex3);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            group.IsLocked = false;

            // cannot transform a group with locked elements
            triangle.IsLocked = true;

            try
            {
                group.Transform(ref transform);
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(new Box3d(50, 0, 0, 100, 50, 0), group.BoundingBox);
                Assert.AreEqual(new Vector3d(50, 0, 0), triangle.Vertex1);
                Assert.AreEqual(new Vector3d(100, 0, 0), triangle.Vertex2);
                Assert.AreEqual(new Vector3d(100, 50, 0), triangle.Vertex3);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            // cannot transform a frozen group
            step.Freeze();

            try
            {
                group.Transform(ref transform);
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.AreEqual(new Box3d(50, 0, 0, 100, 50, 0), group.BoundingBox);
                Assert.AreEqual(new Vector3d(50, 0, 0), triangle.Vertex1);
                Assert.AreEqual(new Vector3d(100, 0, 0), triangle.Vertex2);
                Assert.AreEqual(new Vector3d(100, 50, 0), triangle.Vertex3);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            group = CreateTestGroup();
            Utils.DisposalAccessTest(group, delegate() { group.Transform(ref transform); });
        }

        [TestMethod]
        public void ReverseWindingTest()
        {
            IGroup group = CreateTestGroup();
            IStep step   = MocksFactory.CreateMockStep();

            step.Add(group);

            ITriangle triangle = MocksFactory.CreateMockTriangle();
            triangle.Vertex1   = new Vector3d(10, 0, 0);
            triangle.Vertex2   = new Vector3d(20, 0, 0);
            triangle.Vertex3   = new Vector3d(20, 10, 0);
            step.Add(triangle);
            triangle.Group = group;

            group.ReverseWinding();
            Assert.AreEqual(new Box3d(10, 0, 0, 20, 10, 0), group.BoundingBox);
            Assert.AreEqual(new Vector3d(20, 10, 0), triangle.Vertex1);
            Assert.AreEqual(new Vector3d(20, 0, 0), triangle.Vertex2);
            Assert.AreEqual(new Vector3d(10, 0, 0), triangle.Vertex3);

            // cannot transform a locked group
            group.IsLocked = true;

            try
            {
                group.ReverseWinding();
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(new Box3d(10, 0, 0, 20, 10, 0), group.BoundingBox);
                Assert.AreEqual(new Vector3d(20, 10, 0), triangle.Vertex1);
                Assert.AreEqual(new Vector3d(20, 0, 0), triangle.Vertex2);
                Assert.AreEqual(new Vector3d(10, 0, 0), triangle.Vertex3);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            group.IsLocked = false;

            // cannot transform a group with locked elements
            triangle.IsLocked = true;

            try
            {
                group.ReverseWinding();
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(new Box3d(10, 0, 0, 20, 10, 0), group.BoundingBox);
                Assert.AreEqual(new Vector3d(20, 10, 0), triangle.Vertex1);
                Assert.AreEqual(new Vector3d(20, 0, 0), triangle.Vertex2);
                Assert.AreEqual(new Vector3d(10, 0, 0), triangle.Vertex3);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            // cannot transform a frozen group
            step.Freeze();

            try
            {
                group.ReverseWinding();
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.AreEqual(new Box3d(10, 0, 0, 20, 10, 0), group.BoundingBox);
                Assert.AreEqual(new Vector3d(20, 10, 0), triangle.Vertex1);
                Assert.AreEqual(new Vector3d(20, 0, 0), triangle.Vertex2);
                Assert.AreEqual(new Vector3d(10, 0, 0), triangle.Vertex3);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            group = CreateTestGroup();
            Utils.DisposalAccessTest(group, delegate() { group.ReverseWinding(); });
        }

        #endregion Geometry

        #region Properties

        [TestMethod]
        public void NameTest()
        {
            IGroup group        = CreateTestGroup();
            string defaultValue = Resources.Untitled;
            string newValue     = "name";

            PropertyValueTest(group,
                                defaultValue,
                                newValue,
                                delegate(IGroup obj) { return obj.Name; },
                                delegate(IGroup obj, string value) { obj.Name = value; },
                                PropertyValueFlags.None);

            // Name must be unique within the page
            group      = CreateTestGroup();
            IPage page = MocksFactory.CreateMockPage();
            IStep step = MocksFactory.CreateMockStep();

            page.Add(step);
            step.Add(group);

            IGroup group2 = CreateTestGroup();
            group2.Name   = "group2";
            step.Add(group2);

            try
            {
                group2.Name = group.Name;
                Assert.Fail();
            }
            catch (DuplicateNameException)
            {
                Assert.AreEqual("group2", group2.Name);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(DuplicateNameException), e.GetType());
            }

            // but names are case-sensitive
            group2.Name = group.Name.ToUpper();

            // finally, removing group2 from the page again should allow the name to be used
            step.Remove(group2);
            group2.Name = group.Name;
        }

        [TestMethod]
        public void NameChangedTest()
        {
            IGroup group = CreateTestGroup();
            string valueToSet = "name";

            PropertyChangedTest(group,
                                "NameChanged",
                                valueToSet,
                                delegate(IGroup obj, PropertyChangedEventHandler<string> handler) { obj.NameChanged += handler; },
                                delegate(IGroup obj) { return obj.Name; },
                                delegate(IGroup obj, string value) { obj.Name = value; });
        }

        #endregion Properties
    }
}
