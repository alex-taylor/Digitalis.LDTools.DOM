#region License

//
// IPageElementTest.cs
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
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IPageElementTest : IDocumentElementTest
    {
        #region Infrastructure

        protected override Type InterfaceType { get { return typeof(IPageElement); } }

        protected sealed override IDocumentElement CreateTestDocumentElement()
        {
            return CreateTestPageElement();
        }

        protected sealed override IDocumentElement CreateTestDocumentElementWithDocument()
        {
            return CreateTestPageElementWithDocumentTree();
        }

        protected abstract IPageElement CreateTestPageElement();

        protected abstract IPageElement CreateTestPageElementWithDocumentTree();

        protected abstract IPageElement CreateTestPageElementWithLockedAncestor();

        #endregion Infrastructure

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            IPageElement element = CreateTestPageElementWithDocumentTree();

            if (!element.IsImmutable)
                element.IsLocked = true;

            return element;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IPageElement first  = (IPageElement)original;
            IPageElement second = (IPageElement)copy;

            // upstream links should not be preserved
            Assert.IsNull(second.Page, "Upstream links should not be preserved when cloning/serializing a " + TestClassType.FullName);
            Assert.IsNull(second.Step, "Upstream links should not be preserved when cloning/serializing a " + TestClassType.FullName);

            // IsLocked should be preserved
            Assert.AreEqual(first.IsLocked, second.IsLocked);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            IPageElement element = CreateTestPageElement();
            StringBuilder expected;
            StringBuilder code;
            string lockCode;

            if (element is ITexmapGeometry)
                lockCode = "0 !DIGITALIS_LDTOOLS_DOM LOCKGEOM\r\n";
            else
                lockCode = "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n";

            if (element.IsImmutable)
            {
                if (element.IsLocalLock)
                {
                    // IsLocked is not allowed in PartsLibrary mode
                    code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsTrue(code.ToString().StartsWith(lockCode), code.ToString());
                    code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsFalse(code.ToString().StartsWith(lockCode), code.ToString());
                    code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsTrue(code.ToString().StartsWith(lockCode), code.ToString());
                }
                else
                {
                    code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsFalse(code.ToString().StartsWith(lockCode), code.ToString());
                    code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsFalse(code.ToString().StartsWith(lockCode), code.ToString());
                    code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsFalse(code.ToString().StartsWith(lockCode), code.ToString());
                }
            }
            else
            {
                // unlocked
                code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.IsFalse(code.ToString().StartsWith(lockCode), code.ToString());
                code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.IsFalse(code.ToString().StartsWith(lockCode), code.ToString());
                code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.IsFalse(code.ToString().StartsWith(lockCode), code.ToString());

                // IsLocked is not allowed in PartsLibrary mode
                expected         = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                element.IsLocked = true;
                code             = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lockCode + expected.ToString(), code.ToString());
                element.IsLocked = false;

                expected         = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                element.IsLocked = true;
                code             = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected.ToString(), code.ToString());
                element.IsLocked = false;

                expected         = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                element.IsLocked = true;
                code             = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lockCode + expected.ToString(), code.ToString());
                element.IsLocked = false;
            }

            // the LOCKNEXT/LOCKGEOM should be dependent on IsLocalLock and not on IsLocked
            element = CreateTestPageElementWithLockedAncestor();

            if (null != element)
            {
                Assert.IsTrue(element.IsLocked);
                Assert.IsFalse(element.IsLocalLock);
                code = Utils.PreProcessCode(element.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.IsFalse(code.ToString().StartsWith(lockCode), code.ToString());
            }

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Document-tree

        [TestMethod]
        public virtual void PageTest()
        {
            IPageElement element = CreateTestPageElement();
            Assert.IsNull(element.Page);

            element = CreateTestPageElementWithDocumentTree();
            Assert.IsNotNull(element.Page);

            Utils.DisposalAccessTest(element, delegate() { IPage page = element.Page; });
        }

        [TestMethod]
        public virtual void StepTest()
        {
            IPageElement element = CreateTestPageElement();
            Assert.IsNull(element.Step);

            element = CreateTestPageElementWithDocumentTree();
            Assert.IsNotNull(element.Step);

            Utils.DisposalAccessTest(element, delegate() { IStep step = element.Step; });
        }

        [TestMethod]
        public virtual void PathToDocumentChangedTest()
        {
            IPageElement element = CreateTestPageElementWithDocumentTree();
            bool eventSeen       = false;

            element.PathToDocumentChanged += delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(element, sender);
            };

            IPage page = element.Page;
            Assert.IsNotNull(page);

            IDocument doc = page.Document;
            Assert.IsNotNull(doc);

            // clearing the page's document should trigger a path-change
            page.Document = null;
            Assert.IsNull(element.Document);
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            // as should setting it again
            page.Document = doc;
            Assert.IsNotNull(element.Document);
            Assert.IsTrue(eventSeen);
        }

        #endregion Document-tree

        #region Freezing

        [TestMethod]
        public override void IsFrozenTest()
        {
            // freezing the element should freeze the entire tree
            IPageElement element = CreateTestPageElementWithDocumentTree();
            Assert.IsFalse(element.IsFrozen);

            if (null != element.Step)
                Assert.IsFalse(element.Step.IsFrozen);

            Assert.IsFalse(element.Page.IsFrozen);
            Assert.IsFalse(element.Document.IsFrozen);
            element.Freeze();
            Assert.IsTrue(element.IsFrozen);

            if (null != element.Step)
                Assert.IsTrue(element.Step.IsFrozen);

            Assert.IsTrue(element.Page.IsFrozen);
            Assert.IsTrue(element.Document.IsFrozen);

            base.IsFrozenTest();
        }

        #endregion Freezing

        #region Locking

        [TestMethod]
        public void IsLockedTest()
        {
            IPageElement element     = CreateTestPageElement();
            PropertyValueFlags flags = PropertyValueFlags.SettableWhenLocked;
            bool defaultValue        = false;
            bool newValue            = true;

            if (element is ITexmapGeometry)
                flags |= PropertyValueFlags.NotDisposable;

            PropertyValueTest(element,
                              defaultValue,
                              newValue,
                              delegate(IPageElement obj) { return obj.IsLocked; },
                              delegate(IPageElement obj, bool value) { obj.IsLocked = value; },
                              flags);

            // a locked element may be frozen
            element = CreateTestPageElement();
            element.IsLocked = true;
            element.Freeze();
            Assert.IsTrue(element.IsFrozen);
            Assert.IsTrue(element.IsLocked);

            // an element is implicitly locked if it has a locked ancestor
            element = CreateTestPageElementWithLockedAncestor();

            if (null != element)
            {
                Assert.IsTrue(element.IsLocked);
                Assert.IsFalse(element.IsLocalLock);

                // it should not be possible to unlock it
                try
                {
                    element.IsLocked = false;
                    Assert.Fail();
                }
                catch (ElementLockedException)
                {
                    Assert.IsTrue(element.IsLocked);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                // nor to set an explicit lock
                try
                {
                    element.IsLocked = true;
                    Assert.Fail();
                }
                catch (ElementLockedException)
                {
                    Assert.IsTrue(element.IsLocked);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void IsLockedChangedTest()
        {
            IPageElement element = CreateTestPageElement();
            bool valueToSet      = true;

            PropertyChangedTest(element,
                                "IsLockedChanged",
                                valueToSet,
                                delegate(IPageElement obj, PropertyChangedEventHandler<bool> handler) { obj.IsLockedChanged += handler; },
                                delegate(IPageElement obj) { return obj.IsLocked; },
                                delegate(IPageElement obj, bool value) { obj.IsLocked = value; });
        }

        [TestMethod]
        public void IsLocalLockTest()
        {
            IPageElement element = CreateTestPageElement();
            Assert.IsFalse(element.IsLocked);
            Assert.IsFalse(element.IsLocalLock);
            element.IsLocked = true;
            Assert.IsTrue(element.IsLocked);
            Assert.IsTrue(element.IsLocalLock);

            Utils.DisposalAccessTest(element, delegate() { bool isLocalLock = element.IsLocalLock; });

            element = CreateTestPageElementWithLockedAncestor();

            if (null != element)
            {
                Assert.IsTrue(element.IsLocked);
                Assert.IsFalse(element.IsLocalLock);
            }
        }

        #endregion Locking

        #region Properties

        protected sealed override void PropertyValueTest<C, T>(C obj, T defaultValue, T newValue, PropertyValueGetter<C, T> getter, PropertyValueSetter<C, T> setter, PropertyValueComparer<C, T> comparer, PropertyValueFlags flags)
        {
            IPageElement element = (IPageElement)obj;

            if (!element.IsImmutable)
            {
                T oldValue = getter(obj);

                if (null == comparer)
                    comparer = delegate(C obj2, T expectedValue) { Assert.AreEqual(expectedValue, getter(obj2)); };

                if (0 == (PropertyValueFlags.SettableWhenLocked & flags))
                {
                    // setting the property while the element is locked is not permitted
                    try
                    {
                        element.IsLocked = true;
                        setter(obj, newValue);
                        Assert.Fail();
                    }
                    catch (ElementLockedException)
                    {
                        comparer(obj, oldValue);
                        element.IsLocked = false;
                    }
                    catch (Exception e)
                    {
                        Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                    }

                    // it should not be possible to undo the command once the element is locked
                    UndoStack undoStack = new UndoStack();
                    undoStack.StartCommand("command");
                    setter(obj, newValue);
                    undoStack.EndCommand();
                    element.IsLocked = true;

                    try
                    {
                        undoStack.Undo();
                        Assert.Fail();
                    }
                    catch (ElementLockedException)
                    {
                        comparer(obj, newValue);
                        element.IsLocked = false;
                    }
                    catch (Exception e)
                    {
                        Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                    }
                }
                else
                {
                    // setting the property while the element is locked is permitted
                    element.IsLocked = true;
                    setter(obj, newValue);
                    element.IsLocked = false;
                }

                setter(obj, defaultValue);
            }

            base.PropertyValueTest(obj, defaultValue, newValue, getter, setter, comparer, flags);
        }

        #endregion Properties
    }
}
