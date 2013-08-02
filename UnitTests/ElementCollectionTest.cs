#region License

//
// ElementCollectionTest.cs
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
    public class ElementCollectionTest : IPageElementTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(ElementCollection); } }

        protected override IPageElement CreateTestPageElement()
        {
            return new MockElementCollection();
        }

        protected override IPageElement CreateTestPageElementWithLockedAncestor()
        {
            IStep step                       = MocksFactory.CreateMockStep();
            MockElementCollection collection = new MockElementCollection();
            step.Add(collection);
            step.IsLocked = true;
            return collection;
        }

        protected override IPageElement CreateTestPageElementWithDocumentTree()
        {
            IDocument document               = MocksFactory.CreateMockDocument();
            IPage page                       = MocksFactory.CreateMockPage();
            IStep step                       = MocksFactory.CreateMockStep();
            MockElementCollection collection = new MockElementCollection();
            document.Add(page);
            page.Add(step);
            step.Add(collection);
            return collection;
        }

        #endregion Infrastructure

        #region Change-notification

        [TestMethod]
        public void CollectionTreeChangedTest()
        {
            IElementCollectionTest.ChangedTest(new MockElementCollection());
        }

        #endregion Change-notification

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            ElementCollection collection = new MockElementCollection();
            IElementCollectionTest.PrepareForCloning(collection);
            return collection;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            ElementCollection first  = (ElementCollection)original;
            ElementCollection second = (ElementCollection)copy;
            IElementCollectionTest.CompareClonedObjects(first, second);
            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            MockElementCollection collection = new MockElementCollection();
            IElementCollectionTest.ToCodeTest(collection, collection, String.Empty);

            collection = new MockElementCollection();

            // blank line before comment-blocks
            collection.Add(new LDLine("2 24 0 0 0 1 1 1"));
            collection.Add(new LDComment("0 comment 1"));
            collection.Add(new LDComment("0 comment 2"));
            collection.Add(new LDLine("2 24 2 2 2 3 3 3"));
            collection.Add(new LDComment("0 comment 3"));

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                string code = collection.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();

                Assert.AreEqual("2 24 0 0 0 1 1 1\r\n" +
                                "\r\n" +
                                "0 comment 1\r\n" +
                                "0 comment 2\r\n" +
                                "2 24 2 2 2 3 3 3\r\n" +
                                "\r\n" +
                                "0 comment 3\r\n",
                                code);
            }
        }

        #endregion Code-generation

        #region Collection-management

        [TestMethod]
        public void ItemsAddedTest()
        {
            IElementCollectionTest.ItemsAddedTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void ItemsRemovedTest()
        {
            IElementCollectionTest.ItemsRemovedTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void ItemsReplacedTest()
        {
            IElementCollectionTest.ItemsReplacedTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void CollectionClearedTest()
        {
            IElementCollectionTest.CollectionClearedTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void CanInsertTest()
        {
            MockElementCollection collection = new MockElementCollection();
            IElementCollectionTest.CanInsertTest(collection, collection, new LDLine());
        }

        [TestMethod]
        public void CanReplaceTest()
        {
            MockElementCollection collection = new MockElementCollection();
            IElementCollectionTest.CanReplaceTest(collection, collection, new LDLine());
        }

        [TestMethod]
        public void ContainsColourElementsTest()
        {
            IElementCollectionTest.ContainsColourElementsTest(new MockElementCollection());
        }

        [TestMethod]
        public void ContainsBFCFlagElementsTest()
        {
            IElementCollectionTest.ContainsBFCFlagElementsTest(new MockElementCollection());
        }

        [TestMethod]
        public void HasLockedDescendantsTest()
        {
            IElementCollectionTest.HasLockedDescendantsTest(new MockElementCollection());
        }

        [TestMethod]
        public void IsReadOnlyTest()
        {
            IElementCollectionTest.IsReadOnlyTest(new MockElementCollection());
        }

        [TestMethod]
        public void CountTest()
        {
            IElementCollectionTest.CountTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void IndexOfTest()
        {
            IElementCollectionTest.IndexOfTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void ContainsTest()
        {
            IElementCollectionTest.ContainsTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void IndexerTest()
        {
            IElementCollectionTest.IndexerTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void AddTest()
        {
            IElementCollectionTest.AddTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void InsertTest()
        {
            IElementCollectionTest.InsertTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void RemoveTest()
        {
            IElementCollectionTest.RemoveTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void RemoveAtTest()
        {
            IElementCollectionTest.RemoveAtTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void ClearTest()
        {
            IElementCollectionTest.ClearTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void CopyToTest()
        {
            IElementCollectionTest.CopyToTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            IElementCollectionTest.GetEnumeratorTest(new MockElementCollection(), new LDLine());
        }

        [TestMethod]
        public void OnItemsAddedTest()
        {
            ElementCollection_Accessor collection = new ElementCollection_Accessor(new PrivateObject(new MockElementCollection()));
            bool eventSeen                        = false;

            collection.add_ItemsAdded(delegate(object sender, UndoableListChangedEventArgs<IElement> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            collection.OnItemsAdded(new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            collection.Dispose();

            try
            {
                collection.OnItemsAdded(new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0));
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
                Assert.IsFalse(eventSeen);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectDisposedException), e.GetType());
            }
        }

        [TestMethod]
        public void OnItemsRemovedTest()
        {
            ElementCollection_Accessor collection = new ElementCollection_Accessor(new PrivateObject(new MockElementCollection()));
            bool eventSeen                        = false;

            collection.add_ItemsRemoved(delegate(object sender, UndoableListChangedEventArgs<IElement> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            collection.OnItemsRemoved(new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            collection.Dispose();

            try
            {
                collection.OnItemsRemoved(new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0));
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
                Assert.IsFalse(eventSeen);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectDisposedException), e.GetType());
            }
        }

        [TestMethod]
        public void OnItemsReplacedTest()
        {
            ElementCollection_Accessor collection = new ElementCollection_Accessor(new PrivateObject(new MockElementCollection()));
            bool eventSeen                        = false;

            collection.add_ItemsReplaced(delegate(object sender, UndoableListReplacedEventArgs<IElement> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            collection.OnItemsReplaced(new UndoableListReplacedEventArgs<IElement>(new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0), new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0)));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            collection.Dispose();

            try
            {
                collection.OnItemsReplaced(new UndoableListReplacedEventArgs<IElement>(new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0), new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0)));
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
                Assert.IsFalse(eventSeen);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectDisposedException), e.GetType());
            }
        }

        [TestMethod]
        public void OnCollectionClearedTest()
        {
            ElementCollection_Accessor collection = new ElementCollection_Accessor(new PrivateObject(new MockElementCollection()));
            bool eventSeen                        = false;

            collection.add_CollectionCleared(delegate(object sender, UndoableListChangedEventArgs<IElement> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            collection.OnCollectionCleared(new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            collection.Dispose();

            try
            {
                collection.OnCollectionCleared(new UndoableListChangedEventArgs<IElement>(new IElement[0], 0, 0));
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
                Assert.IsFalse(eventSeen);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectDisposedException), e.GetType());
            }
        }

        #endregion Collection-management

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            IElementCollectionTest.DisposeTest(new MockElementCollection());

            ElementCollection collection = new MockElementCollection();
            Element element              = new MockElement();

            // Elements should subscribe to the collection's PathToDocumentChanged event,
            // and the ElementCollection should subscribe to the element's Changed event
            Assert.AreEqual(0, collection.PathToDocumentChangedSubscribers);
            Assert.AreEqual(0, element.ChangedSubscribers);
            collection.Add(element);
            Assert.AreEqual(1, collection.PathToDocumentChangedSubscribers);
            Assert.AreEqual(1, element.ChangedSubscribers);

            // for efficiency, disposing of the collection will not disconnect anything
            collection.Dispose();
            Assert.AreEqual(1, collection.PathToDocumentChangedSubscribers);
            Assert.AreEqual(1, element.ChangedSubscribers);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Document-tree

        [TestMethod]
        public override void PathToDocumentChangedTest()
        {
            MockElementCollection collection = new MockElementCollection();
            IElementCollectionTest.PathToDocumentChangedTest(collection, collection);
            base.PathToDocumentChangedTest();
        }

        [TestMethod]
        public void OnChangedTest()
        {
            MockElementCollection collection = new MockElementCollection();
            ElementCollection_Accessor accessor = new ElementCollection_Accessor(new PrivateObject(collection));
            bool eventSeen                        = false;

            accessor.add_Changed(delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(collection, sender);
                Assert.AreSame(collection, e.Source);
                Assert.AreSame("operation", e.Operation);
                Assert.AreSame(EventArgs.Empty, e.Parameters);
            });

            accessor.OnChanged(collection, "operation", EventArgs.Empty);
            Assert.IsTrue(eventSeen);
            eventSeen     = false;

            accessor.Dispose();

            try
            {
                accessor.OnChanged(collection, "operation", EventArgs.Empty);
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
                Assert.IsFalse(eventSeen);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectDisposedException), e.GetType());
            }
        }

        #endregion Document-tree

        #region Geometry

        [TestMethod]
        public void BoundingBoxTest()
        {
            IElementCollectionTest.BoundingBoxTest(new MockElementCollection());
        }

        [TestMethod]
        public void OriginTest()
        {
            IElementCollectionTest.OriginTest(new MockElementCollection());
        }

        [TestMethod]
        public void WindingModeTest()
        {
            MockElementCollection collection = new MockElementCollection();
            IElementCollectionTest.WindingModeTest(collection, collection);
        }

        [TestMethod]
        public void TransformTest()
        {
            IElementCollectionTest.TransformTest(new MockElementCollection());
        }

        [TestMethod]
        public void ReverseWindingTest()
        {
            IElementCollectionTest.ReverseWindingTest(new MockElementCollection());
        }

        #endregion Geometry
    }
}
