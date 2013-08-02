#region License

//
// PageElementTest.cs
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
    public sealed class PageElementTest : IPageElementTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(PageElement); } }

        protected override IPageElement CreateTestPageElement()
        {
            return new MockPageElement();
        }

        protected override IPageElement CreateTestPageElementWithDocumentTree()
        {
            return new MockPageElement(MocksFactory.CreateMockDocument(), MocksFactory.CreateMockPage(), MocksFactory.CreateMockStep());
        }

        protected override IPageElement CreateTestPageElementWithLockedAncestor()
        {
            IStep step = MocksFactory.CreateMockStep();
            step.IsLocked = true;
            return new MockPageElement(null, null, step);
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

        #region Locking

        [TestMethod]
        public void OnIsLockedChangedTest()
        {
            PageElement_Accessor element = new PageElement_Accessor(new PrivateObject(new MockPageElement()));
            bool eventSeen = false;

            element.add_IsLockedChanged(delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            element.OnIsLockedChanged(new PropertyChangedEventArgs<bool>(true, false));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            element.Dispose();

            try
            {
                element.OnIsLockedChanged(new PropertyChangedEventArgs<bool>(true, false));
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

        #endregion Locking
    }
}
