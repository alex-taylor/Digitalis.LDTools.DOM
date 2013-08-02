#region License

//
// ITexmapGeometryTest.cs
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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class ITexmapGeometryTest : IPageElementTest
    {
        #region Infrastructure

        protected override Type InterfaceType { get { return typeof(ITexmapGeometry); } }

        protected sealed override IPageElement CreateTestPageElement()
        {
            return CreateTestTexmapGeometry();
        }

        protected sealed override IPageElement CreateTestPageElementWithDocumentTree()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            IDocument document       = MocksFactory.CreateMockDocument();
            IPage page               = MocksFactory.CreateMockPage();
            IStep step               = MocksFactory.CreateMockStep();

            document.Add(page);
            page.Add(step);
            step.Add(geometry.Texmap);
            return geometry;
        }

        protected sealed override IPageElement CreateTestPageElementWithLockedAncestor()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            geometry.Texmap.IsLocked = true;
            return geometry;
        }

        protected abstract ITexmapGeometry CreateTestTexmapGeometry();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            Assert.AreEqual(DOMObjectType.Collection, geometry.ObjectType);
            Assert.IsFalse(geometry.AllowsTopLevelElements);
            Assert.AreSame(geometry.Texmap, geometry.Parent);
            Assert.IsTrue(Enum.IsDefined(typeof(TexmapGeometryType), geometry.GeometryType));

            if (geometry.IsImmutable)
                Assert.IsTrue(geometry.IsReadOnly);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Change-notification

        [TestMethod]
        public void ChangedTest()
        {
            IElementCollectionTest.ChangedTest(CreateTestTexmapGeometry());
        }

        #endregion Change-notification

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            IElementCollectionTest.PrepareForCloning(geometry);
            return geometry;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            ITexmapGeometry first  = (ITexmapGeometry)original;
            ITexmapGeometry second = (ITexmapGeometry)copy;

            // upstream links should not be preserved
            Assert.IsNull(second.Texmap);

            Assert.AreEqual(first.GeometryType, second.GeometryType);

            IElementCollectionTest.CompareClonedObjects(first, second);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            StringBuilder code;
            string prefix = "";

            if (TexmapGeometryType.Texture == geometry.GeometryType)
                prefix = "0 !: ";

            IElementCollectionTest.ToCodeTest(geometry, geometry.Texmap, prefix);

            geometry = CreateTestTexmapGeometry();

            if (!geometry.IsImmutable && !geometry.IsReadOnly)
            {
                ILine line = MocksFactory.CreateMockLine();
                geometry.Add(line);

                const string lockGeomCode = "0 !DIGITALIS_LDTOOLS_DOM LOCKGEOM\r\n";
                string lineCode           = Utils.PreProcessCode(line.ToCode(new StringBuilder(), CodeStandards.Full, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal)).ToString();
                string lockNextCode       = "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n";

                lineCode     = prefix + lineCode;
                lockNextCode = prefix + lockNextCode;

                // unlocked output
                foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
                {
                    code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(lineCode, code.ToString());
                }

                // with the line locked, the extra code should show up in Full and OMR modes only, as LOCKNEXT is not permitted in the PartsLibrary
                line.IsLocked = true;
                code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lockNextCode + lineCode, code.ToString());
                code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lockNextCode + lineCode, code.ToString());
                code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lineCode, code.ToString());

                // locking the containing ITexmap should have no effect
                geometry.Texmap.IsLocked = true;
                code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lockNextCode + lineCode, code.ToString());
                code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lockNextCode + lineCode, code.ToString());
                code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lineCode, code.ToString());
                geometry.Texmap.IsLocked = false;

                // but locking the geometry itself should add the LOCKGEOM in Full/OMR modes as before
                geometry.IsLocked = true;
                code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lockGeomCode + lockNextCode + lineCode, code.ToString());
                code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lockGeomCode + lockNextCode + lineCode, code.ToString());
                code = Utils.PreProcessCode(geometry.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(lineCode, code.ToString());
            }

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Collection-management

        [TestMethod]
        public void ItemsAddedTest()
        {
            IElementCollectionTest.ItemsAddedTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void ItemsRemovedTest()
        {
            IElementCollectionTest.ItemsRemovedTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void ItemsReplacedTest()
        {
            IElementCollectionTest.ItemsReplacedTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void CollectionClearedTest()
        {
            IElementCollectionTest.CollectionClearedTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void CanInsertTest()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            IElementCollectionTest.CanInsertTest(geometry, geometry.Texmap, MocksFactory.CreateMockLine());

            geometry = CreateTestTexmapGeometry();

            if (!geometry.IsImmutable && !geometry.IsReadOnly)
            {
                // it should not be possible to add our own Texmap
                Assert.AreEqual(InsertCheckResult.NotSupported, geometry.CanInsert(geometry.Texmap, InsertCheckFlags.None));

                // other ITexmaps are not allowed either
                Assert.AreEqual(InsertCheckResult.NotSupported, geometry.CanInsert(MocksFactory.CreateMockTexmap(), InsertCheckFlags.None));
            }
        }

        [TestMethod]
        public void CanReplaceTest()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            IElementCollectionTest.CanReplaceTest(geometry, geometry.Texmap, MocksFactory.CreateMockLine());

            geometry = CreateTestTexmapGeometry();

            if (!geometry.IsImmutable && !geometry.IsReadOnly)
            {
                geometry.Add(MocksFactory.CreateMockLine());

                // it should not be possible to add our own Texmap
                Assert.AreEqual(InsertCheckResult.NotSupported, geometry.CanReplace(geometry.Texmap, geometry[0], InsertCheckFlags.None));

                // other ITexmaps are not allowed either
                Assert.AreEqual(InsertCheckResult.NotSupported, geometry.CanReplace(MocksFactory.CreateMockTexmap(), geometry[0], InsertCheckFlags.None));
            }
        }

        [TestMethod]
        public void ContainsColourElementsTest()
        {
            IElementCollectionTest.ContainsColourElementsTest(CreateTestTexmapGeometry());
        }

        [TestMethod]
        public void ContainsBFCFlagElementsTest()
        {
            IElementCollectionTest.ContainsBFCFlagElementsTest(CreateTestTexmapGeometry());
        }

        [TestMethod]
        public void HasLockedDescendantsTest()
        {
            IElementCollectionTest.HasLockedDescendantsTest(CreateTestTexmapGeometry());
        }

        [TestMethod]
        public void IsReadOnlyTest()
        {
            IElementCollectionTest.IsReadOnlyTest(CreateTestTexmapGeometry());
        }

        [TestMethod]
        public void CountTest()
        {
            IElementCollectionTest.CountTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void IndexOfTest()
        {
            IElementCollectionTest.IndexOfTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void ContainsTest()
        {
            IElementCollectionTest.ContainsTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void IndexerTest()
        {
            IElementCollectionTest.IndexerTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void AddTest()
        {
            IElementCollectionTest.AddTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void InsertTest()
        {
            IElementCollectionTest.InsertTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void RemoveTest()
        {
            IElementCollectionTest.RemoveTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void RemoveAtTest()
        {
            IElementCollectionTest.RemoveAtTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void ClearTest()
        {
            IElementCollectionTest.ClearTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void CopyToTest()
        {
            IElementCollectionTest.CopyToTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            IElementCollectionTest.GetEnumeratorTest(CreateTestTexmapGeometry(), MocksFactory.CreateMockLine());
        }

        #endregion Collection-management

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            // disposing of an ITexmapGeometry that is attached to an ITexmap is not possible
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            ITexmap texmap = geometry.Texmap;

            try
            {
                geometry.Dispose();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreSame(texmap, geometry.Texmap);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            // but it is possible to dispose of a 'loose' one
            ITexmapGeometry clone = (ITexmapGeometry)geometry.Clone();
            Assert.IsNull(clone.Texmap);
            clone.Dispose();

            // and disposing of the containing ITexmap shouldn't cause any problems either
            geometry.Texmap.Dispose();
        }

        #endregion Disposal

        #region Document-tree

        [TestMethod]
        public override void PathToDocumentChangedTest()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();
            IElementCollectionTest.PathToDocumentChangedTest(geometry, geometry.Texmap);
        }

        #endregion Document-tree

        #region Geometry

        [TestMethod]
        public void BoundingBoxTest()
        {
            IElementCollectionTest.BoundingBoxTest(CreateTestTexmapGeometry());
        }

        [TestMethod]
        public void OriginTest()
        {
            IElementCollectionTest.OriginTest(CreateTestTexmapGeometry());
        }

        [TestMethod]
        public void WindingModeTest()
        {
            ITexmapGeometry geometry = CreateTestTexmapGeometry();

            if (null == geometry.Texmap)
                Assert.AreEqual(CullingMode.Disabled, geometry.WindingMode);
            else
                Assert.AreEqual(geometry.Texmap.WindingMode, geometry.WindingMode);
        }

        [TestMethod]
        public void TransformTest()
        {
            IElementCollectionTest.TransformTest(CreateTestTexmapGeometry());
        }

        [TestMethod]
        public void ReverseWindingTest()
        {
            IElementCollectionTest.ReverseWindingTest(CreateTestTexmapGeometry());
        }

        #endregion Geometry
    }
}
