#region License

//
// GroupableTest.cs
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
    public sealed class GroupableTest : IGroupableTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(Groupable); } }

        protected override IGroupable CreateTestGroupable()
        {
            return new MockGroupable();
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
            LDStep step             = new LDStep();
            MockGroupable groupable = new MockGroupable();
            MLCadGroup group        = new MLCadGroup();

            step.Add(group);
            Assert.AreEqual(1, step.PathToDocumentChangedSubscribers);
            Assert.AreEqual(0, group.PathToDocumentChangedSubscribers);

            step.Add(groupable);
            groupable.Group = group;

            // the Groupable should subscribe to its step's PathToDocumentChanged event...
            Assert.AreEqual(3, step.PathToDocumentChangedSubscribers);  // adds two subscribers: Groupable and its superclass Element
            // ...and to its group's PathToDocumentChanged event
            Assert.AreEqual(1, group.PathToDocumentChangedSubscribers);

            groupable.Dispose();
            Assert.AreEqual(1, step.PathToDocumentChangedSubscribers);
            Assert.AreEqual(0, group.PathToDocumentChangedSubscribers);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Grouping

        [TestMethod]
        public override void GroupTest()
        {
            IGroupable groupable = CreateTestGroupable();
            IStep step           = new LDStep();
            MLCadGroup group     = new MLCadGroup();

            step.Add(groupable);
            step.Add(group);

            Assert.AreEqual(0, group.PathToDocumentChangedSubscribers);
            groupable.Group = group;
            Assert.AreEqual(1, group.PathToDocumentChangedSubscribers);
            groupable.Group = null;
            Assert.AreEqual(0, group.PathToDocumentChangedSubscribers);

            base.GroupTest();
        }

        #endregion Grouping
    }
}
