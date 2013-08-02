#region License

//
// IGroupableTest.cs
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
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IGroupableTest : IElementTest
    {
        #region Infrastructure

        protected override Type InterfaceType { get { return typeof(IGroupable); } }

        protected sealed override IElement CreateTestElement()
        {
            return CreateTestGroupable();
        }

        protected abstract IGroupable CreateTestGroupable();

        protected IGroupable CreateTestGroupableWithGroup()
        {
            IGroupable element = (IGroupable)CreateTestPageElementWithDocumentTree();

            if (element.IsGroupable)
            {
                IGroup group = MocksFactory.CreateMockGroup();
                element.Step.Add(group);
                element.Group = group;
                return element;
            }

            return null;
        }

        protected IGroupable CreateTestGroupableWithGroupInSeparateStep()
        {
            IGroupable element = (IGroupable)CreateTestPageElementWithDocumentTree();

            if (element.IsGroupable)
            {
                IStep step   = MocksFactory.CreateMockStep();
                IGroup group = MocksFactory.CreateMockGroup();
                step.Add(group);
                element.Page.Add(step);
                element.Group = group;
                return element;
            }

            return null;
        }

        #endregion Infrastructure

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            return CreateTestGroupableWithGroup();
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            // upstream links should not be preserved
            Assert.IsNull(((IGroupable)copy).Group, "Upstream links should not be preserved when cloning/serializing a " + TestClassType.FullName);

            base.CompareCopiedObjects(original, copy);
        }

        [TestMethod]
        public void CloneGroupableTest()
        {
            GroupRelinkTest(delegate(IDOMObject obj) { return obj.Clone(); });
        }

        [TestMethod]
        public void SerializeGroupableTest()
        {
            // TODO: tests for other types of serialization

            GroupRelinkTest(delegate(IDOMObject obj)
            {
                using (Stream stream = Utils.SerializeBinary(obj))
                {
                    return (IDOMObject)Utils.DeserializeBinary(stream);
                }
            });
        }

        private delegate IDOMObject Copy(IDOMObject obj);

        private void GroupRelinkTest(Copy copy)
        {
            IGroupable groupable      = CreateTestGroupableWithGroup();
            IGroup group              = groupable.Group;
            IElementCollection parent = groupable.Parent;
            IPage page                = groupable.Page;
            IDocument document        = groupable.Document;
            int count                 = group.Count;

            IGroupable groupableClone;
            IGroup groupClone;

            // first copy the immediate parent
            IElementCollection parentClone = (IElementCollection)copy(parent);
            groupClone                     = (from n in parentClone where n is IGroup select n as IGroup).FirstOrDefault();
            groupableClone                 = (from n in parentClone where n is IGroupable select n as IGroupable).FirstOrDefault();

            Assert.AreNotSame(parent, parentClone);
            Assert.AreNotSame(group, groupClone);
            Assert.AreNotSame(groupable, groupableClone);

            Assert.AreEqual(count, groupClone.Count);
            Assert.IsTrue(groupClone.Contains(groupableClone));
            Assert.IsNotNull(groupableClone.Group);
            Assert.AreSame(groupClone, groupableClone.Group);

            // then try copying the page
            IPage pageClone = (IPage)copy(page);
            groupClone      = (from n in pageClone.Elements where n is IGroup select n as IGroup).FirstOrDefault();
            groupableClone  = (from n in pageClone.Elements where n is IGroupable select n as IGroupable).FirstOrDefault();

            Assert.AreNotSame(parent, parentClone);
            Assert.AreNotSame(group, groupClone);
            Assert.AreNotSame(groupable, groupableClone);

            Assert.AreEqual(count, groupClone.Count);
            Assert.IsTrue(groupClone.Contains(groupableClone));
            Assert.IsNotNull(groupableClone.Group);
            Assert.AreSame(groupClone, groupableClone.Group);

            // and copy the document for good measure
            IDocument documentClone = (IDocument)copy(document);
            groupClone              = (from n in documentClone[0].Elements where n is IGroup select n as IGroup).FirstOrDefault();
            groupableClone          = (from n in documentClone[0].Elements where n is IGroupable select n as IGroupable).FirstOrDefault();

            Assert.AreNotSame(parent, parentClone);
            Assert.AreNotSame(group, groupClone);
            Assert.AreNotSame(groupable, groupableClone);

            Assert.AreEqual(count, groupClone.Count);
            Assert.IsTrue(groupClone.Contains(groupableClone));
            Assert.IsNotNull(groupableClone.Group);
            Assert.AreSame(groupClone, groupableClone.Group);

            // and finally try with the IGroup and IGroupable in separate ISteps
            groupable = CreateTestGroupableWithGroupInSeparateStep();
            group     = groupable.Group;
            page      = groupable.Page;
            count     = group.Count;

            pageClone      = (IPage)copy(page);
            groupClone     = (from n in pageClone.Elements where n is IGroup select n as IGroup).FirstOrDefault();
            groupableClone = (from n in pageClone.Elements where n is IGroupable select n as IGroupable).FirstOrDefault();

            Assert.AreNotSame(page, pageClone);
            Assert.AreNotSame(group, groupClone);
            Assert.AreNotSame(groupable, groupableClone);

            Assert.AreEqual(count, groupClone.Count);
            Assert.IsTrue(groupClone.Contains(groupableClone));
            Assert.IsNotNull(groupableClone.Group);
            Assert.AreSame(groupClone, groupableClone.Group);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            IGroupable groupable = CreateTestGroupable();
            StringBuilder code;
            string groupCode = "0 MLCAD BTG ";

            if (groupable.IsImmutable)
            {
                if (null == groupable.Group)
                {
                    code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsFalse(code.ToString().StartsWith(groupCode), code.ToString());
                    code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsFalse(code.ToString().StartsWith(groupCode), code.ToString());
                    code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsFalse(code.ToString().StartsWith(groupCode), code.ToString());
                }
                else
                {
                    // MLCAD BTG is not allowed in PartsLibrary mode
                    code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsTrue(code.ToString().StartsWith(groupCode), code.ToString());
                    code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsFalse(code.ToString().StartsWith(groupCode), code.ToString());
                    code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsTrue(code.ToString().StartsWith(groupCode), code.ToString());
                }
            }
            else
            {
                // ungrouped
                code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.IsFalse(code.ToString().StartsWith(groupCode), code.ToString());
                code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.IsFalse(code.ToString().StartsWith(groupCode), code.ToString());
                code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.IsFalse(code.ToString().StartsWith(groupCode), code.ToString());
            }

            // MLCAD BTG is not allowed in PartsLibrary mode
            groupable = CreateTestGroupableWithGroup();
            groupCode = "0 MLCAD BTG " + groupable.Group.Name + "\r\n";

            code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.IsTrue(code.ToString().StartsWith(groupCode), code.ToString());
            code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.IsFalse(code.ToString().StartsWith(groupCode), code.ToString());
            code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.IsTrue(code.ToString().StartsWith(groupCode), code.ToString());

            // the LOCKNEXT attribute should also be grouped
            groupable.IsLocked = true;
            groupCode += "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n0 MLCAD BTG " + groupable.Group.Name + "\r\n";
            code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.IsTrue(code.ToString().StartsWith(groupCode), code.ToString());
            code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.IsFalse(code.ToString().StartsWith(groupCode), code.ToString());
            code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.IsTrue(code.ToString().StartsWith(groupCode), code.ToString());

            // multiple lines should be grouped individually
            groupable = CreateTestGroupableWithGroup();
            groupCode = "0 MLCAD BTG " + groupable.Group.Name;

            code = Utils.PreProcessCode(groupable.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            string[] lines = code.ToString().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length > 2)
            {
                Assert.AreEqual(0, lines.Length % 2);

                for (int i = 0; i < lines.Length; i += 2)
                {
                    Assert.AreEqual(groupCode, lines[i]);
                }
            }

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            IGroupable groupable = CreateTestGroupableWithGroup();
            IGroup group         = groupable.Group;

            Assert.IsTrue(group.Contains(groupable));
            groupable.Dispose();
            Assert.IsFalse(group.Contains(groupable));

            // if the IGroup is locked, the dispose will fail
            groupable      = CreateTestGroupableWithGroup();
            group          = groupable.Group;
            group.IsLocked = true;

            try
            {
                groupable.Dispose();
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.IsTrue(group.Contains(groupable));
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            // TODO: cannot dispose an IGroupable if its IGroup is immutable or read-only

            // disposing of the group should not dispose the groupable
            groupable = CreateTestGroupableWithGroup();
            group     = groupable.Group;
            group.Dispose();
            Assert.IsFalse(groupable.IsDisposed);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Grouping

        [TestMethod]
        public virtual void IsGroupableTest()
        {
            // 'loose' groupables may not be grouped
            IGroupable groupable = CreateTestGroupable();
            Assert.IsFalse(groupable.IsGroupable);

            // a groupable may only be attached to one group at a time
            groupable = CreateTestGroupableWithGroup();
            Assert.IsFalse(groupable.IsGroupable);

            // the groupable must be a descendant of a Page or a Step
            groupable = (IGroupable)CreateTestObjectWithDocumentTree();
            Assert.IsNotNull(groupable.Page);
            Assert.IsNotNull(groupable.Step);
            Assert.IsTrue(groupable.IsGroupable);

            // the groupable must be a direct child of a Step
            groupable = (IGroupable)CreateTestElementWithGrandparent();
            Assert.IsNotNull(groupable.Page);
            Assert.IsNotNull(groupable.Step);
            Assert.IsNotNull(groupable.Parent);
            Assert.AreNotSame(groupable.Step, groupable.Parent);
            Assert.IsFalse(groupable.IsGroupable);

            // a locked groupable may be grouped
            groupable = (IGroupable)CreateTestObjectWithDocumentTree();

            if (!groupable.IsImmutable)
            {
                groupable.IsLocked = true;
                Assert.IsTrue(groupable.IsGroupable);
            }

            // an immutable groupable may be grouped
            groupable = (IGroupable)CreateTestObjectWithDocumentTree();

            if (groupable.IsImmutable)
                Assert.IsTrue(groupable.IsGroupable);

            // a frozen groupable may not be grouped
            groupable = (IGroupable)CreateTestObjectWithDocumentTree();
            groupable.Freeze();
            Assert.IsTrue(groupable.IsFrozen);
            Assert.IsFalse(groupable.IsGroupable);
        }

        [TestMethod]
        public virtual void GroupTest()
        {
            IGroupable groupable = (IGroupable)CreateTestElementWithStep();
            IGroup defaultValue  = null;
            IGroup newValue      = MocksFactory.CreateMockGroup();

            groupable.Step.Add(newValue);

            PropertyValueTest(groupable,
                              defaultValue,
                              newValue,
                              delegate(IGroupable obj) { return obj.Group; },
                              delegate(IGroupable obj, IGroup value) { obj.Group = value; },
                              PropertyValueFlags.SettableWhenLocked);

            // Group cannot be set if it already has a value
            groupable = CreateTestGroupableWithGroup();

            if (!groupable.IsImmutable)
            {
                IGroup group = groupable.Group;

                try
                {
                    groupable.Group = (IGroup)group.Clone();
                    Assert.Fail();
                }
                catch (InvalidOperationException)
                {
                    Assert.AreSame(group, groupable.Group);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
                }
            }

            // Group cannot be set if the IGroup is locked
            groupable = CreateTestGroupableWithGroup();

            if (!groupable.IsImmutable)
            {
                IGroup group = groupable.Group;
                group.Remove(groupable);
                group.IsLocked = true;

                try
                {
                    groupable.Group = group;
                    Assert.Fail();
                }
                catch (ElementLockedException)
                {
                    Assert.IsNull(groupable.Group);
                    Assert.IsFalse(group.Contains(groupable));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }
            }

            // Group should clear itself if the groupable is removed from the document-tree...
            groupable = CreateTestGroupableWithGroup();

            if (!groupable.IsImmutable)
            {
                UndoStack undoStack = new UndoStack();
                IGroup group = groupable.Group;
                Assert.IsNotNull(group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.StartCommand("command");
                groupable.Parent = null;
                undoStack.EndCommand();
                Assert.IsNull(groupable.Group);
                Assert.IsFalse(group.Contains(groupable));

                undoStack.Undo();
                Assert.AreSame(group, groupable.Group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.Redo();
                Assert.IsNull(groupable.Group);
                Assert.IsFalse(group.Contains(groupable));
            }

            // ...or if the group is removed...
            groupable = CreateTestGroupableWithGroup();

            if (!groupable.IsImmutable)
            {
                UndoStack undoStack = new UndoStack();
                IGroup group = groupable.Group;
                Assert.IsNotNull(group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.StartCommand("command");
                group.Parent = null;
                undoStack.EndCommand();
                Assert.IsNull(groupable.Group);
                Assert.IsFalse(group.Contains(groupable));

                undoStack.Undo();
                Assert.AreSame(group, groupable.Group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.Redo();
                Assert.IsNull(groupable.Group);
                Assert.IsFalse(group.Contains(groupable));
            }

            // ...or if the step containing the group is removed...
            groupable = CreateTestGroupableWithGroupInSeparateStep();

            if (!groupable.IsImmutable)
            {
                UndoStack undoStack = new UndoStack();
                IGroup group = groupable.Group;
                Assert.IsNotNull(group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.StartCommand("command");
                group.Step.Page = null;
                undoStack.EndCommand();
                Assert.IsNull(groupable.Group);
                Assert.IsFalse(group.Contains(groupable));

                undoStack.Undo();
                Assert.AreSame(group, groupable.Group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.Redo();
                Assert.IsNull(groupable.Group);
                Assert.IsFalse(group.Contains(groupable));
            }

            // ...or if the step containing the element is removed...
            groupable = CreateTestGroupableWithGroupInSeparateStep();

            if (!groupable.IsImmutable)
            {
                UndoStack undoStack = new UndoStack();
                IGroup group = groupable.Group;
                Assert.IsNotNull(group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.StartCommand("command");
                groupable.Step.Page = null;
                undoStack.EndCommand();
                Assert.IsNull(groupable.Group);
                Assert.IsFalse(group.Contains(groupable));

                undoStack.Undo();
                Assert.AreSame(group, groupable.Group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.Redo();
                Assert.IsNull(groupable.Group);
                Assert.IsFalse(group.Contains(groupable));
            }

            // ...but removing the step which contains both the element and the group should be fine
            groupable = CreateTestGroupableWithGroup();

            if (!groupable.IsImmutable)
            {
                UndoStack undoStack = new UndoStack();
                IGroup group = groupable.Group;
                Assert.IsNotNull(group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.StartCommand("command");
                groupable.Step.Page = null;
                undoStack.EndCommand();
                Assert.AreSame(group, groupable.Group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.Undo();
                Assert.AreSame(group, groupable.Group);
                Assert.IsTrue(group.Contains(groupable));

                undoStack.Redo();
                Assert.AreSame(group, groupable.Group);
                Assert.IsTrue(group.Contains(groupable));
            }
        }

        [TestMethod]
        public virtual void GroupChangedTest()
        {
            IGroupable groupable = CreateTestGroupableWithGroup();
            IGroup valueToSet    = null;

            PropertyChangedTest(groupable,
                                "GroupChanged",
                                valueToSet,
                                delegate(IGroupable obj, PropertyChangedEventHandler<IGroup> handler) { obj.GroupChanged += handler; },
                                delegate(IGroupable obj) { return obj.Group; },
                                delegate(IGroupable obj, IGroup value) { obj.Group = value; });
        }

        #endregion Grouping
    }
}
