#region License

//
// IElementTest.cs
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

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IElementTest : IPageElementTest
    {
        #region Infrastructure

        protected override Type InterfaceType { get { return typeof(IElement); } }

        protected sealed override IPageElement CreateTestPageElement()
        {
            return CreateTestElement();
        }

        protected abstract IElement CreateTestElement();

        protected IElement CreateTestElementWithParent()
        {
            IElement element = CreateTestElement();

            if (null == element.Parent)
            {
                IElementCollection parent = MocksFactory.CreateMockElementCollection();
                parent.Add(element);
            }

            return element;
        }

        protected virtual IElement CreateTestElementWithStep()
        {
            IElement element          = CreateTestElement();
            IElementCollection parent = element.Parent;
            IStep step                = MocksFactory.CreateMockStep();

            if (null == parent)
            {
                step.Add(element);
            }
            else
            {
                while (null != parent)
                {
                    if (null == parent.Parent && parent is IElement)
                    {
                        step.Add(parent as IElement);
                        break;
                    }

                    parent = parent.Parent;
                }
            }

            return element;
        }

        protected IElement CreateTestElementWithGrandparent()
        {
            IElement element = CreateTestElementWithParent();
            IStep step       = MocksFactory.CreateMockStep();
            step.Add((IElement)element.Parent);
            IPage page = MocksFactory.CreateMockPage();
            page.Add(step);
            return element;
        }

        protected sealed override IPageElement CreateTestPageElementWithDocumentTree()
        {
            IElement element = CreateTestElementWithStep();
            IPage page       = MocksFactory.CreateMockPage();
            IDocument doc    = MocksFactory.CreateMockDocument();
            doc.Add(page);
            page.Add(element.Step);
            return element;
        }

        protected sealed override IPageElement CreateTestPageElementWithLockedAncestor()
        {
            IElement element      = CreateTestElementWithStep();
            element.Step.IsLocked = true;
            return element;
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IElement element                       = CreateTestElement();
            ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;

            if (null != elementFlagsAttr)
            {
                bool hasTopLevelFlag = (0 != (ElementFlags.TopLevelElement & elementFlagsAttr.Flags));
                Assert.AreEqual(element.IsTopLevelElement, hasTopLevelFlag, TestClassType.FullName + ".IsTopLevelElement and ElementFlagsAttribute must have the same value");
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            return CreateTestElementWithParent();
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IElement first  = (IElement)original;
            IElement second = (IElement)copy;

            // upstream links should not be preserved
            Assert.IsNull(second.Parent, "Upstream links should not be preserved when cloning/serializing a " + TestClassType.FullName);

            // the original element's parent should not see 'Changed' events from the copy
            IDOMObject parent = null;
            bool treeEventSeen = false;

            if (null != first.Document)
            {
                parent = first.Document;

                first.Document.DocumentTreeChanged += delegate(IDocument sender, DocumentTreeChangedEventArgs e)
                {
                    Assert.IsFalse(treeEventSeen);
                    treeEventSeen = true;
                };
            }
            else if (null != first.Page)
            {
                parent = first.Page;

                first.Page.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(treeEventSeen);
                    treeEventSeen = true;
                };
            }
            else if (null != first.Parent)
            {
                parent = first.Parent;

                first.Parent.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(treeEventSeen);
                    treeEventSeen = true;
                };
            }

            second.IsLocked = !second.IsLocked;
            Assert.IsFalse(treeEventSeen);
            treeEventSeen = false;
            second.IsLocked = !second.IsLocked;

            // and the copy should not see PathToDocumentChanged events from the original's parent
            if (null != parent)
            {
                bool pathEventSeen = false;

                second.PathToDocumentChanged += delegate(object sender, EventArgs e)
                {
                    Assert.IsFalse(pathEventSeen);
                    pathEventSeen = true;
                };

                switch (parent.ObjectType)
                {
                    case DOMObjectType.Page:
                        IDocument document = MocksFactory.CreateMockDocument();
                        document.Add((IPage)parent);
                        break;

                    case DOMObjectType.Step:
                        IPage page = MocksFactory.CreateMockPage();
                        page.Add((IStep)parent);
                        break;

                    default:
                        if (parent is IElement)
                        {
                            IStep step = MocksFactory.CreateMockStep();
                            step.Add((IElement)parent);
                        }
                        break;
                }

                Assert.IsFalse(pathEventSeen);
            }

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            // TODO: test for when the parent is immutable or read-only

            IElement element          = CreateTestElementWithParent();
            IElementCollection parent = element.Parent;

            // disposing of an element will remove it from its Parent
            Assert.IsTrue(parent.Contains(element));
            element.Dispose();
            Assert.IsFalse(parent.Contains(element));

            // a locked element can be disposed of
            element          = CreateTestElementWithParent();
            parent           = element.Parent;
            element.IsLocked = true;
            element.Dispose();
            Assert.IsFalse(parent.Contains(element));

            // but one with a locked parent cannot
            element         = CreateTestElementWithParent();
            parent          = element.Parent;
            parent.IsLocked = true;

            try
            {
                element.Dispose();
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.IsTrue(parent.Contains(element));
                Assert.AreSame(parent, element.Parent);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            // the parent can be disposed, though
            parent.Dispose();
            Assert.IsTrue(parent.IsDisposed);
            Assert.IsTrue(element.IsDisposed);

            // an element in a frozen document-tree cannot be disposed
            element = CreateTestElementWithParent();
            parent  = element.Parent;
            element.Freeze();
            Assert.IsTrue(element.IsFrozen);

            try
            {
                element.Dispose();
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.IsTrue(parent.Contains(element));
                Assert.AreSame(parent, element.Parent);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            // but the head of the tree can
            parent.Dispose();
            Assert.IsTrue(parent.IsDisposed);
            Assert.IsTrue(element.IsDisposed);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Document-tree

        [TestMethod]
        public virtual void ParentTest()
        {
            IElement element                = CreateTestElement();
            IElementCollection defaultValue = null;
            IElementCollection newValue     = new MockElementCollection();

            PropertyValueTest(element,
                              defaultValue,
                              newValue,
                              delegate(IElement obj) { return obj.Parent; },
                              delegate(IElement obj, IElementCollection value) { obj.Parent = value; },
                              PropertyValueFlags.SettableWhenLocked);

            // test the event-propagation links
            element = CreateTestElement();
            MockElementCollection parent = new MockElementCollection();
            IElementCollectionTest.CheckCollectionEvents(parent, element);

            // the element cannot be added to a frozen IElementCollection
            element = CreateTestElement();
            parent  = new MockElementCollection();
            parent.Freeze();
            Assert.IsTrue(parent.IsFrozen);

            try
            {
                element.Parent = parent;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.IsNull(element.Parent);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            // the element cannot be added to an immutable IElementCollection
            element                    = CreateTestElement();
            parent                     = new MockElementCollection();
            parent.OverrideIsImmutable = true;
            Assert.IsTrue(parent.IsImmutable);

            try
            {
                element.Parent = parent;
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
                Assert.IsNull(element.Parent);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            }

            // the element cannot be added to a read-only IElementCollection
            element                   = CreateTestElement();
            parent                    = new MockElementCollection();
            parent.OverrideIsReadOnly = true;
            Assert.IsTrue(parent.IsReadOnly);

            try
            {
                element.Parent = parent;
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
                Assert.IsNull(element.Parent);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            }

            // the element cannot be added to a locked IElementCollection
            element         = CreateTestElement();
            parent          = new MockElementCollection();
            parent.IsLocked = true;
            Assert.IsTrue(parent.IsLocked);

            try
            {
                element.Parent = parent;
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.IsNull(element.Parent);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            // TODO: check for InvalidOperationException when parent.CanInsert() fails its checks

            // if the element is a child of a Step, Parent should return the same value
            element = CreateTestElementWithStep();
            Assert.IsNotNull(element.Parent);
            Assert.IsNotNull(element.Step);

            if (element.Step.Contains(element))
                Assert.AreSame(element.Step, element.Parent);

            // Parent cannot be set if it already has a value
            IElementCollection oldParent = element.Parent;

            try
            {
                element.Parent = new MockElementCollection();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreSame(oldParent, element.Parent);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }
        }

        [TestMethod]
        public virtual void ParentChangedTest()
        {
            IElement element = CreateTestElementWithParent();

            if (!element.IsImmutable)
            {
                IElementCollection parent = element.Parent;
                bool eventSeen            = false;
                bool genericEventSeen     = false;
                bool addEventSeen         = false;
                bool removeEventSeen      = false;

                Assert.IsNotNull(parent);

                element.ParentChanged += delegate(object sender, PropertyChangedEventArgs<IElementCollection> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(element, sender);

                    if (null == e.NewValue)
                        Assert.AreSame(parent, e.OldValue);
                    else
                        Assert.AreSame(parent, e.NewValue);
                };

                element.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(element, sender);
                    Assert.AreEqual("ParentChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IElementCollection>));

                    PropertyChangedEventArgs<IElementCollection> args = (PropertyChangedEventArgs<IElementCollection>)e.Parameters;

                    if (null == args.NewValue)
                        Assert.AreSame(parent, args.OldValue);
                    else
                        Assert.AreSame(parent, args.NewValue);
                };

                parent.ItemsRemoved += delegate(object sender, UndoableListChangedEventArgs<IElement> e)
                {
                    Assert.IsFalse(removeEventSeen);
                    removeEventSeen = true;
                    Assert.AreSame(parent, sender);
                };

                parent.ItemsAdded += delegate(object sender, UndoableListChangedEventArgs<IElement> e)
                {
                    Assert.IsFalse(addEventSeen);
                    addEventSeen = true;
                    Assert.AreSame(parent, sender);
                };

                element.Parent = null;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.IsTrue(removeEventSeen);

                eventSeen        = false;
                genericEventSeen = false;
                element.Parent   = parent;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.IsTrue(addEventSeen);
            }
        }

        [TestMethod]
        public override void PathToDocumentChangedTest()
        {
            // setting the element's parent should trigger a path-change
            IElement element = CreateTestElementWithParent();

            if (!element.IsImmutable)
            {
                bool eventSeen = false;

                element.PathToDocumentChanged += delegate(object sender, EventArgs e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(element, sender);
                };

                IElementCollection parent = element.Parent;
                Assert.IsNotNull(parent);
                element.Parent = null;
                Assert.IsNull(element.Parent);
                Assert.IsTrue(eventSeen);
                eventSeen = false;
                element.Parent = parent;
                Assert.IsNotNull(element.Parent);
                Assert.IsTrue(eventSeen);
            }

            // setting the element's page should trigger a path-change
            element = (IElement)CreateTestObjectWithDocumentTree();

            if (!element.IsImmutable)
            {
                bool eventSeen = false;

                element.PathToDocumentChanged += delegate(object sender, EventArgs e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(element, sender);
                };

                IStep step = element.Step;
                Assert.IsNotNull(step);
                IPage page = step.Page;
                Assert.IsNotNull(page);
                step.Page = null;
                Assert.IsNull(element.Page);
                Assert.IsTrue(eventSeen);
                eventSeen = false;
                step.Page = page;
                Assert.IsNotNull(element.Page);
                Assert.IsTrue(eventSeen);
            }

            base.PathToDocumentChangedTest();
        }

        #endregion Document-tree
    }
}
