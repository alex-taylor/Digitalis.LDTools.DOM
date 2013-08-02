#region License

//
// IPage.ElementsTest.cs
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
    public abstract class IPageElementsTest
    {
        #region Infrastructure

        protected abstract IPage CreateTestPage();

        #endregion Infrastructure

        #region Collection-management

        [TestMethod]
        public void ItemsAddedTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            page.Add(new LDStep());
            IDOMObjectCollectionTest.ItemsAddedTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;

            bool eventSeen = false;

            elements.ItemsAdded += delegate(object sender, UndoableListChangedEventArgs<IElement> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(elements, sender);
                Assert.AreEqual(1, e.Count);
            };

            // add an empty step
            page.Add(new LDStep());
            Assert.IsFalse(eventSeen);

            // add a step with elements
            IStep step = new LDStep();
            step.Add(new LDLine());
            page.Add(step);
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            // add elements to a step
            step.Add(new LDLine());
            Assert.IsTrue(eventSeen);
        }

        [TestMethod]
        public void ItemsRemovedTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            page.Add(new LDStep());
            IDOMObjectCollectionTest.ItemsRemovedTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep emptyStep = new LDStep();
            page.Add(emptyStep);
            IStep stepWithElement = new LDStep();
            stepWithElement.Add(new LDLine());
            page.Add(stepWithElement);
            IStep stepWithElements = new LDStep();
            stepWithElements.Add(new LDLine());
            stepWithElements.Add(new LDLine());
            page.Add(stepWithElements);

            bool eventSeen = false;

            elements.ItemsRemoved += delegate(object sender, UndoableListChangedEventArgs<IElement> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(elements, sender);
                Assert.AreEqual(1, e.Count);
            };

            // remove an empty step
            page.Remove(emptyStep);
            Assert.IsFalse(eventSeen);

            // remove a step with elements
            page.Remove(stepWithElement);
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            // remove elements from a step
            stepWithElements.RemoveAt(0);
            Assert.IsTrue(eventSeen);
        }

        [TestMethod]
        public void ItemsReplacedTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            page.Add(new LDStep());
            IDOMObjectCollectionTest.ItemsReplacedTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep step = new LDStep();
            step.Add(new LDLine());
            page.Add(step);

            bool eventSeen = false;

            elements.ItemsReplaced += delegate(object sender, UndoableListReplacedEventArgs<IElement> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(elements, sender);
                Assert.AreEqual(1, e.ItemsAdded.Count);
                Assert.AreEqual(1, e.ItemsRemoved.Count);
            };

            // replace element in a step
            step[0] = new LDLine();
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            // replace entire step
            step = new LDStep();
            step.Add(new LDLine());
            page[0] = step;
            Assert.IsTrue(eventSeen);
        }

        [TestMethod]
        public void CollectionClearedTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            page.Add(new LDStep());
            IDOMObjectCollectionTest.CollectionClearedTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;

            bool eventSeen = false;

            elements.CollectionCleared += delegate(object sender, UndoableListChangedEventArgs<IElement> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(elements, sender);
                Assert.AreEqual(1, e.Count);
            };

            // clear a page with no elements
            IStep step = new LDStep();
            page.Add(step);
            page.Clear();
            Assert.IsFalse(eventSeen);

            // clear a page with elements
            step.Add(new LDLine());
            page.Add(step);
            page.Clear();
            Assert.IsTrue(eventSeen);
        }

        [TestMethod]
        public void CanInsertTest()
        {
            IPage page1 = CreateTestPage();
            IDOMObjectCollection<IElement> elements1 = page1.Elements;
            IPage page2 = CreateTestPage();
            IDOMObjectCollection<IElement> elements2 = page2.Elements;
            Assert.AreEqual(InsertCheckResult.NotSupported, elements1.CanInsert(new LDLine(), InsertCheckFlags.None));
            page1.Add(new LDStep());
            page2.Add(new LDStep());
            IDOMObjectCollectionTest.CanInsertTest(elements1, elements2, new LDLine());
        }

        [TestMethod]
        public void CanReplaceTest()
        {
            IPage page1 = CreateTestPage();
            IDOMObjectCollection<IElement> elements1 = page1.Elements;
            IPage page2 = CreateTestPage();
            IDOMObjectCollection<IElement> elements2 = page2.Elements;
            Assert.AreEqual(InsertCheckResult.NotSupported, elements1.CanReplace(new LDLine(), new LDLine(), InsertCheckFlags.None));
            page1.Add(new LDStep());
            page2.Add(new LDStep());
            IDOMObjectCollectionTest.CanReplaceTest(elements1, elements2, new LDLine(), new LDLine());
        }

        [TestMethod]
        public void CountTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            Assert.AreEqual(0, elements.Count);
            page.Add(new LDStep());
            IDOMObjectCollectionTest.CountTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep step1 = new LDStep();
            IStep step2 = new LDStep();
            page.Add(step1);
            page.Add(step2);
            step1.Add(new LDLine());
            step2.Add(new LDLine());
            Assert.AreEqual(step1.Count + step2.Count, elements.Count);
        }

        [TestMethod]
        public void IndexOfTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            ILine line = new LDLine();
            Assert.AreEqual(-1, elements.IndexOf(line));
            page.Add(new LDStep());
            elements.Add(line);
            Assert.AreEqual(0, elements.IndexOf(line));
            IDOMObjectCollectionTest.IndexOfTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep step1 = new LDStep();
            IStep step2 = new LDStep();
            page.Add(step1);
            page.Add(step2);
            step1.Add(new LDLine());
            step2.Add(new LDLine());
            Assert.AreEqual(0, elements.IndexOf(step1[0]));
            Assert.AreEqual(1, elements.IndexOf(step2[0]));
        }

        [TestMethod]
        public void ContainsTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            ILine line = new LDLine();
            Assert.IsFalse(elements.Contains(line));
            page.Add(new LDStep());
            elements.Add(line);
            Assert.IsTrue(elements.Contains(line));
            IDOMObjectCollectionTest.ContainsTest(elements, new LDLine());
        }

        [TestMethod]
        public void IndexerTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;

            // no steps
            try
            {
                elements[0] = new LDLine();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            page.Add(new LDStep());

            // invalid index
            try
            {
                elements[10] = new LDLine();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            IDOMObjectCollectionTest.IndexerTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep step1 = new LDStep();
            IStep step2 = new LDStep();
            page.Add(step1);
            page.Add(step2);
            step1.Add(new LDLine());
            step2.Add(new LDLine());
            Assert.AreSame(step1[0], elements[0]);
            Assert.AreSame(step2[0], elements[1]);
        }

        [TestMethod]
        public void AddTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;

            try
            {
                elements.Add(new LDLine());
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            page.Add(new LDStep());
            IDOMObjectCollectionTest.AddTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep step = new LDStep();
            page.Add(step);
            elements.Add(new LDLine());
            Assert.AreSame(elements[0], step[0]);
        }

        [TestMethod]
        public void InsertTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;

            // no steps
            try
            {
                elements.Insert(0, new LDLine());
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            page.Add(new LDStep());

            // invalid index
            try
            {
                elements.Insert(10, new LDLine());
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            IDOMObjectCollectionTest.InsertTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep step = new LDStep();
            page.Add(step);
            elements.Insert(0, new LDLine());
            Assert.AreSame(elements[0], step[0]);
        }

        [TestMethod]
        public void RemoveTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            Assert.IsFalse(elements.Remove(null));
            page.Add(new LDStep());
            IDOMObjectCollectionTest.RemoveTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep step = new LDStep();
            page.Add(step);
            ILine line = new LDLine();
            step.Add(line);
            Assert.IsTrue(elements.Contains(line));
            elements.Remove(line);
            Assert.IsFalse(step.Contains(line));
        }

        [TestMethod]
        public void RemoveAtTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;

            // no steps
            try
            {
                elements.RemoveAt(0);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            page.Add(new LDStep());

            // invalid index
            try
            {
                elements.RemoveAt(10);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            IDOMObjectCollectionTest.RemoveAtTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep step = new LDStep();
            page.Add(step);
            ILine line = new LDLine();
            step.Add(line);
            Assert.IsTrue(elements.Contains(line));
            elements.RemoveAt(0);
            Assert.IsFalse(step.Contains(line));
        }

        [TestMethod]
        public void ClearTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            elements.Clear();
            page.Add(new LDStep());
            IDOMObjectCollectionTest.ClearTest(elements, new LDLine());

            page = CreateTestPage();
            elements = page.Elements;
            IStep step = new LDStep();
            page.Add(step);
            step.Add(new LDLine());
            Assert.AreEqual(1, elements.Count);
            elements.Clear();
            Assert.AreEqual(0, step.Count);
        }

        [TestMethod]
        public void CopyToTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            elements.CopyTo(new IElement[1], 0);
            page.Add(new LDStep());
            IDOMObjectCollectionTest.CopyToTest(elements, new LDLine());
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            IPage page = CreateTestPage();
            IDOMObjectCollection<IElement> elements = page.Elements;
            int count = 0;

            foreach (IElement element in elements)
            {
                count++;
            }

            Assert.AreEqual(0, count);
            page.Add(new LDStep());
            IDOMObjectCollectionTest.GetEnumeratorTest(elements, new LDLine());
        }

        #endregion Collection-management
    }
}
