#region License

//
// IDocumentElementTest.cs
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

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;

    #endregion Usings

    [TestClass]
    public abstract class IDocumentElementTest : IDOMObjectTest
    {
        #region Infrastructure

        protected override Type InterfaceType { get { return typeof(IDocumentElement); } }

        protected sealed override IDOMObject CreateTestObject()
        {
            return CreateTestDocumentElement();
        }

        protected sealed override IDOMObject CreateTestObjectWithFrozenAncestor()
        {
            IDocumentElement element = CreateTestDocumentElementWithDocument();
            element.Document.Freeze();
            return element;
        }

        protected sealed override IDOMObject CreateTestObjectWithDocumentTree()
        {
            return CreateTestDocumentElementWithDocument();
        }

        protected abstract IDocumentElement CreateTestDocumentElement();

        protected abstract IDocumentElement CreateTestDocumentElementWithDocument();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IDocumentElement element = CreateTestDocumentElement();
            Image icon               = element.Icon;

            if (null != icon)
            {
                Assert.AreEqual(16, icon.Width, TestClassType.FullName + ".Icon.Width must be 16");
                Assert.AreEqual(16, icon.Height, TestClassType.FullName + ".Icon.Height must be 16");
                Assert.AreEqual(96, Math.Round(icon.HorizontalResolution), TestClassType.FullName + ".Icon.HorizontalResolution must be 96");
                Assert.AreEqual(96, Math.Round(icon.VerticalResolution), TestClassType.FullName + ".Icon.VerticalResolution must be 96");
            }

            Assert.IsFalse(String.IsNullOrWhiteSpace(element.TypeName), TestClassType.FullName + ".TypeName must be set");

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public virtual void HasProblemsTest()
        {
            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                IDocumentElement element = CreateTestDocumentElement();

                Utils.DisposalAccessTest(element, delegate() { bool hasProblems = element.HasProblems(codeFormat); });
            }
        }

        [TestMethod]
        public virtual void AnalyseTest()
        {
            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                IDocumentElement element                 = CreateTestDocumentElement();
                ICollection<IProblemDescriptor> problems = element.Analyse(codeFormat);

                Assert.IsNotNull(problems);

                if (element.HasProblems(codeFormat))
                    Assert.AreNotEqual(0, problems.Count);
                else
                    Assert.AreEqual(0, problems.Count);

                Utils.DisposalAccessTest(element, delegate() { element.Analyse(codeFormat); });
            }
        }

        #endregion Analytics

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            return CreateTestDocumentElementWithDocument();
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IDocumentElement first  = (IDocumentElement)original;
            IDocumentElement second = (IDocumentElement)copy;

            if (null != first.Icon)
                Assert.IsNotNull(second.Icon);
            else
                Assert.IsNull(second.Icon);

            Assert.AreEqual(first.TypeName, second.TypeName);
            Assert.AreEqual(first.Description, second.Description);
            Assert.AreEqual(first.ExtendedDescription, second.ExtendedDescription);

            Assert.IsNull(second.Document, "Upstream links should not be preserved when cloning/serializing a " + TestClassType.FullName);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Document-tree

        [TestMethod]
        public virtual void DocumentTest()
        {
            IDocumentElement element = CreateTestDocumentElement();
            Assert.IsNull(element.Document);

            element = CreateTestDocumentElementWithDocument();
            Assert.IsNotNull(element.Document);

            Utils.DisposalAccessTest(element, delegate() { IDocument document = element.Document; });
        }

        #endregion Document-tree

        #region Editor

        [TestMethod]
        public virtual void HasEditorTest()
        {
            IDocumentElement element = CreateTestDocumentElement();

            if (element.IsImmutable)
                Assert.IsFalse(element.HasEditor);

            element.Freeze();
            Assert.IsFalse(element.HasEditor);

            Utils.DisposalAccessTest(element, delegate() { bool hasEditor = element.HasEditor; });
        }

        [TestMethod]
        public virtual void GetEditorTest()
        {
            IDocumentElement element = CreateTestDocumentElement();

            if (element.IsImmutable)
                Assert.IsNull(element.GetEditor());

            element.Freeze();
            Assert.IsNull(element.GetEditor());

            Utils.DisposalAccessTest(element, delegate() { IElementEditor editor = element.GetEditor(); });
        }

        #endregion Editor

        #region Freezing

        [TestMethod]
        public override void IsFrozenTest()
        {
            // freezing the containing IDocument should cause the element to report itself as frozen
            IDocumentElement element = CreateTestDocumentElementWithDocument();
            Assert.IsFalse(element.IsFrozen);
            element.Document.Freeze();
            Assert.IsTrue(element.IsFrozen);

            // freezing the element should freeze the entire tree
            element = CreateTestDocumentElementWithDocument();
            Assert.IsFalse(element.IsFrozen);
            element.Freeze();
            Assert.IsTrue(element.IsFrozen);
            Assert.IsTrue(element.Document.IsFrozen);

            base.IsFrozenTest();
        }

        #endregion Freezing
    }
}
