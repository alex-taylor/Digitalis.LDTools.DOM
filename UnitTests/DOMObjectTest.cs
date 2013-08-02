#region License

//
// DOMObjectTest.cs
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

    #endregion Usings

    [TestClass]
    public sealed class DOMObjectTest : IDOMObjectTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(DOMObject); } }

        protected override IDOMObject CreateTestObject()
        {
            return new MockDOMObject();
        }

        protected override IDOMObject CreateTestObjectWithDocumentTree()
        {
            return new MockDOMObject(true, false);
        }

        protected override IDOMObject CreateTestObjectWithFrozenAncestor()
        {
            return new MockDOMObject(true, true);
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

        #region Change-notification

        [TestMethod]
        public void OnChangedTest()
        {
            MockDOMObject obj = new MockDOMObject();
            bool eventSeen    = false;

            obj.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.AreSame(obj, sender);
                Assert.AreEqual("TestEvent", e.Operation);
                Assert.AreSame(EventArgs.Empty, e.Parameters);
                eventSeen = true;
            };

            obj.OnChanged();
            Assert.IsTrue(eventSeen);

            obj.Dispose();

            try
            {
                obj.OnChanged();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectDisposedException), e.GetType());
            }
        }

        #endregion Change-notification

        #region Cloning

        [TestMethod]
        public void CloneCallsInitializeObjectTest()
        {
            MockDOMObject target = new MockDOMObject();
            Assert.IsFalse(target.InitializeElementCalled);

            MockDOMObject clone = (MockDOMObject)target.Clone();
            Assert.IsTrue(target.InitializeElementCalled);
            Assert.IsFalse(clone.InitializeElementCalled);
        }

        #endregion Cloning

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            MockDOMObject obj = new MockDOMObject();
            bool isDisposing  = false;

            obj.OnDisposing += delegate(object sender, EventArgs e)
            {
                isDisposing = obj.IsDisposing;
            };

            Assert.IsFalse(obj.DisposeCalled);
            obj.Dispose();
            Assert.IsTrue(obj.DisposeCalled);
            Assert.IsTrue(isDisposing);
            Assert.IsFalse(obj.IsDisposing);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Freezing

        [TestMethod]
        public void OnFreezingTest()
        {
            MockDOMObject obj = new MockDOMObject();

            Assert.IsFalse(obj.OnFreezingCalled);
            obj.Freeze();
            Assert.IsTrue(obj.OnFreezingCalled);
        }

        [TestMethod]
        public void OnFrozenTest()
        {
            MockDOMObject obj = new MockDOMObject();

            Assert.IsFalse(obj.OnFrozenCalled);
            obj.Freeze();
            Assert.IsTrue(obj.OnFrozenCalled);
        }

        #endregion Freezing
    }
}
