#region License

//
// DocumentElementTest.cs
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
    using System.Drawing;
    using System.Reflection;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;

    #endregion Usings

    [TestClass]
    public sealed class DocumentElementTest : IDocumentElementTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(DocumentElement); } }

        protected override IDocumentElement CreateTestDocumentElement()
        {
            return new MockDocumentElement();
        }

        protected override IDocumentElement CreateTestDocumentElementWithDocument()
        {
            return new MockDocumentElement(MocksFactory.CreateMockDocument());
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

        #region Document-tree

        [TestMethod]
        public void OnPathToDocumentChangedTest()
        {
            DocumentElement_Accessor element = new DocumentElement_Accessor(new PrivateObject(new MockDocumentElement()));
            bool eventSeen = false;

            element.add_PathToDocumentChanged(delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            element.OnPathToDocumentChanged(EventArgs.Empty);
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            element.Dispose();

            try
            {
                element.OnPathToDocumentChanged(EventArgs.Empty);
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
    }
}
