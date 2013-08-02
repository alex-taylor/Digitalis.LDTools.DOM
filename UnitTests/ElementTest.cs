#region License

//
// ElementTest.cs
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
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public sealed class ElementTest : IElementTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(Element); } }

        protected override IElement CreateTestElement()
        {
            return new MockElement();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            Assert.IsTrue(TestClassType.IsAbstract);
            Assert.IsFalse(TestClassType.IsSealed);
            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            MockElement element = new MockElement();
            LDStep step         = new LDStep();

            // Element should subscribe to its parent's PathToDocumentChanged event
            Assert.AreEqual(0, step.PathToDocumentChangedSubscribers);
            step.Add(element);
            Assert.AreEqual(1, step.PathToDocumentChangedSubscribers);
            element.Dispose();
            Assert.AreEqual(0, step.PathToDocumentChangedSubscribers);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Document-tree

        [TestMethod]
        public void OnParentChangedTest()
        {
            Element_Accessor element = new Element_Accessor(new PrivateObject(new MockElement()));
            bool eventSeen = false;

            element.add_ParentChanged(delegate(object sender, PropertyChangedEventArgs<IElementCollection> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            element.OnParentChanged(new PropertyChangedEventArgs<IElementCollection>(null, null));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            element.Dispose();

            try
            {
                element.OnParentChanged(new PropertyChangedEventArgs<IElementCollection>(null, null));
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

        #region Freezing

        [TestMethod]
        public override void IsFrozenTest()
        {
            // freezing the element should freeze the entire tree
            IElement element = (IElement)CreateTestObjectWithDocumentTree();
            Assert.IsFalse(element.IsFrozen);
            Assert.IsFalse(element.Parent.IsFrozen);
            Assert.IsFalse(element.Step.IsFrozen);
            Assert.IsFalse(element.Page.IsFrozen);
            Assert.IsFalse(element.Document.IsFrozen);
            element.Freeze();
            Assert.IsTrue(element.IsFrozen);
            Assert.IsTrue(element.Parent.IsFrozen);
            Assert.IsTrue(element.Step.IsFrozen);
            Assert.IsTrue(element.Page.IsFrozen);
            Assert.IsTrue(element.Document.IsFrozen);

            base.IsFrozenTest();
        }

        #endregion Freezing
    }
}
