#region License

//
// IStepTest.cs
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
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IStepTest : IPageElementTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IStep); } }

        protected sealed override IPageElement CreateTestPageElement()
        {
            return CreateTestStep();
        }

        protected sealed override IPageElement CreateTestPageElementWithDocumentTree()
        {
            IDocument document = new LDDocument();
            IPage page = new LDPage();
            IStep step = CreateTestStep();
            document.Add(page);
            page.Add(step);

            return step;
        }

        protected sealed override IPageElement CreateTestPageElementWithLockedAncestor()
        {
            return null;
        }

        protected abstract IStep CreateTestStep();

        // following the algorithm in the MLCad specs...note that the Y and Z angles have to be inverted, as MLCad does its calculations in
        // OpenGL coordinate-space rather than LDraw
        private static double x = 10.0 * Math.PI / 180.0;
        private static double y = -20.0 * Math.PI / 180.0;
        private static double z = -30.0 * Math.PI / 180.0;

        private static double s1 = Math.Sin(x);
        private static double s2 = Math.Sin(y);
        private static double s3 = Math.Sin(z);

        private static double c1 = Math.Cos(x);
        private static double c2 = Math.Cos(y);
        private static double c3 = Math.Cos(z);

        private static double a = c2 * c3;
        private static double b = -c2 * s3;
        private static double c = s2;
        private static double d = (c1 * s3) + (s1 * s2 * c3);
        private static double e = (c1 * c3) - (s1 * s2 * s3);
        private static double f = -s1 * c2;
        private static double g = (s1 * s3) - (c1 * s2 * c3);
        private static double h = (s1 * c3) + (c1 * s2 * s3);
        private static double i = c1 * c2;

        protected static Matrix4d TestMatrix = new Matrix4d(new Vector4d(a, d, g, 0), new Vector4d(b, e, h, 0), new Vector4d(c, f, i, 0), Vector4d.UnitW);

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IStep step = CreateTestStep();
            Assert.AreEqual(DOMObjectType.Step, step.ObjectType);
            Assert.IsTrue(step.AllowsTopLevelElements);

            if (step.IsImmutable)
                Assert.IsTrue(step.IsReadOnly);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Change-notification

        [TestMethod]
        public void ChangedTest()
        {
            IElementCollectionTest.ChangedTest(CreateTestStep());
        }

        #endregion Change-notification

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            IStep step = CreateTestStep();

            if (!step.IsImmutable && !step.IsReadOnly)
            {
                step.Mode = StepMode.Additive;
                step.X    = 10;
                step.Y    = 20;
                step.Z    = 30;

                IElementCollectionTest.PrepareForCloning(step);
            }

            return step;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IStep first  = (IStep)original;
            IStep second = (IStep)copy;

            Assert.AreEqual(first.Mode, second.Mode);
            Assert.AreEqual(first.X, second.X);
            Assert.AreEqual(first.Y, second.Y);
            Assert.AreEqual(first.Z, second.Z);
            IElementCollectionTest.CompareClonedObjects(first, second);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            IStep step = CreateTestStep();

            if (!step.IsImmutable && !step.IsReadOnly)
            {
                // this case should be optimised to '0 STEP'
                step.Mode = StepMode.Additive;
                step.X    = 0.0;
                step.Y    = 0.0;
                step.Z    = 0.0;
                Assert.AreEqual("0 STEP\r\n", step.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual(String.Empty, step.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 STEP\r\n", step.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());

                step.Mode = StepMode.Relative;
                step.X    = 10.0;
                step.Y    = 20.0;
                step.Z    = 30.5;
                Assert.AreEqual("0 ROTSTEP 10 20 30.5 REL\r\n", step.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual(String.Empty, step.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 ROTSTEP 10 20 30.5 REL\r\n", step.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());

                step.Mode = StepMode.Absolute;
                Assert.AreEqual("0 ROTSTEP 10 20 30.5 ABS\r\n", step.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual(String.Empty, step.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 ROTSTEP 10 20 30.5 ABS\r\n", step.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());

                step.Mode = StepMode.Additive;
                Assert.AreEqual("0 ROTSTEP 10 20 30.5 ADD\r\n", step.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual(String.Empty, step.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 ROTSTEP 10 20 30.5 ADD\r\n", step.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());

                step.Mode = StepMode.Reset;
                Assert.AreEqual("0 ROTSTEP END\r\n", step.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual(String.Empty, step.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 ROTSTEP END\r\n", step.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());

                // this case should be optimised to '0 ROTSTEP END'
                step.Mode = StepMode.Relative;
                step.X    = 0.0;
                step.Y    = 0.0;
                step.Z    = 0.0;
                Assert.AreEqual("0 ROTSTEP END\r\n", step.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual(String.Empty, step.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 ROTSTEP END\r\n", step.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());

                // add some elements
                step.Mode = StepMode.Additive;
                step.Add(new LDComment("0 comment"));
                step.Add(new LDLine("2 24 0 0 0 1 1 1"));
                Assert.AreEqual("0 comment\r\n2 24 0 0 0 1 1 1\r\n0 STEP\r\n", step.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 comment\r\n2 24 0 0 0 1 1 1\r\n", step.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 comment\r\n2 24 0 0 0 1 1 1\r\n0 STEP\r\n", step.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());

                // attach to a page : the last STEP should be suppressed
                IPage page = new LDPage();
                page.Add(step);
                page.PageType = PageType.Model;
                IStep step2   = CreateTestStep();
                step2.Add(new LDQuadrilateral("4 16 0 0 0 1 1 1 2 2 2 3 3 3"));
                Assert.AreEqual("4 16 0 0 0 1 1 1 2 2 2 3 3 3\r\n0 STEP\r\n", step2.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("4 16 0 0 0 1 1 1 2 2 2 3 3 3\r\n", step2.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("4 16 0 0 0 1 1 1 2 2 2 3 3 3\r\n0 STEP\r\n", step2.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                page.Add(step2);
                Assert.AreEqual("0 comment\r\n2 24 0 0 0 1 1 1\r\n0 STEP\r\n", step.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 comment\r\n2 24 0 0 0 1 1 1\r\n", step.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("0 comment\r\n2 24 0 0 0 1 1 1\r\n0 STEP\r\n", step.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("4 16 0 0 0 1 1 1 2 2 2 3 3 3\r\n", step2.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("4 16 0 0 0 1 1 1 2 2 2 3 3 3\r\n", step2.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
                Assert.AreEqual("4 16 0 0 0 1 1 1 2 2 2 3 3 3\r\n", step2.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString());
            }

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Collection-management

        [TestMethod]
        public void ItemsAddedTest()
        {
            IElementCollectionTest.ItemsAddedTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void ItemsRemovedTest()
        {
            IElementCollectionTest.ItemsRemovedTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void ItemsReplacedTest()
        {
            IElementCollectionTest.ItemsReplacedTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void CollectionClearedTest()
        {
            IElementCollectionTest.CollectionClearedTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void CanInsertTest()
        {
            IStep step = CreateTestStep();
            IElementCollectionTest.CanInsertTest(step, null, new LDLine());
        }

        [TestMethod]
        public void CanReplaceTest()
        {
            IStep step = CreateTestStep();
            IElementCollectionTest.CanReplaceTest(step, null, new LDLine());
        }

        [TestMethod]
        public void ContainsColourElementsTest()
        {
            IElementCollectionTest.ContainsColourElementsTest(CreateTestStep());
        }

        [TestMethod]
        public void ContainsBFCFlagElementsTest()
        {
            IElementCollectionTest.ContainsBFCFlagElementsTest(CreateTestStep());
        }

        [TestMethod]
        public void HasLockedDescendantsTest()
        {
            IElementCollectionTest.HasLockedDescendantsTest(CreateTestStep());
        }

        [TestMethod]
        public void IsReadOnlyTest()
        {
            IElementCollectionTest.IsReadOnlyTest(CreateTestStep());
        }

        [TestMethod]
        public void CountTest()
        {
            IElementCollectionTest.CountTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void IndexOfTest()
        {
            IElementCollectionTest.IndexOfTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void ContainsTest()
        {
            IElementCollectionTest.ContainsTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void IndexerTest()
        {
            IElementCollectionTest.IndexerTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void AddTest()
        {
            IElementCollectionTest.AddTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void InsertTest()
        {
            IElementCollectionTest.InsertTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void RemoveTest()
        {
            IElementCollectionTest.RemoveTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void RemoveAtTest()
        {
            IElementCollectionTest.RemoveAtTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void ClearTest()
        {
            IElementCollectionTest.ClearTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void CopyToTest()
        {
            IElementCollectionTest.CopyToTest(CreateTestStep(), new LDLine());
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            IElementCollectionTest.GetEnumeratorTest(CreateTestStep(), new LDLine());
        }

        #endregion Collection-management

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            // disposing of a step will remove it from its Page and dispose of its descendants
            IStep step = (IStep)CreateTestPageElementWithDocumentTree();
            IPage page = step.Page;
            ILine line = new LDLine();
            step.Add(line);

            Assert.AreSame(page, step.Page);
            Assert.IsTrue(page.Contains(step));
            Assert.AreSame(step, line.Parent);
            step.Dispose();
            Assert.IsTrue(line.IsDisposed);
            Assert.IsNull(step.Page);
            Assert.IsFalse(page.Contains(step));

            // a locked step can be disposed of
            step          = (IStep)CreateTestPageElementWithDocumentTree();
            page          = step.Page;
            step.IsLocked = true;
            Assert.AreSame(page, step.Page);
            Assert.IsTrue(page.Contains(step));
            step.Dispose();
            Assert.IsTrue(step.IsDisposed);

            // TODO: test for when the page is immutable or read-only

            // a step in a frozen document-tree cannot be disposed of
            step = (IStep)CreateTestPageElementWithDocumentTree();
            page = step.Page;
            step.Freeze();
            Assert.IsTrue(step.IsFrozen);

            try
            {
                step.Dispose();
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.IsTrue(page.Contains(step));
                Assert.AreSame(page, step.Page);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            // but the head of the tree can
            IDocument document = step.Document;
            document.Dispose();
            Assert.IsTrue(step.IsDisposed);
            Assert.IsTrue(page.IsDisposed);
            Assert.IsTrue(document.IsDisposed);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Document-tree

        [TestMethod]
        public override void PathToDocumentChangedTest()
        {
            IElementCollectionTest.PathToDocumentChangedTest(CreateTestStep(), null);
            base.PathToDocumentChangedTest();
        }

        [TestMethod]
        public override void PageTest()
        {
            IStep step         = CreateTestStep();
            IPage defaultValue = null;
            IPage newValue     = new LDPage();

            PropertyValueTest(step,
                              defaultValue,
                              newValue,
                              delegate(IStep obj) { return obj.Page; },
                              delegate(IStep obj, IPage value) { obj.Page = value; },
                              PropertyValueFlags.SettableWhenLocked | PropertyValueFlags.NotDisposable);

            // the step cannot be added to a frozen IPage
            step = CreateTestStep();
            IPage page = new LDPage();
            page.Freeze();
            Assert.IsTrue(page.IsFrozen);

            try
            {
                step.Page = page;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.IsNull(step.Parent);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            // TODO: the step cannot be added to an immutable IPage
            //step = CreateTestStep();
            //page = new LDPage();
            //page.OverrideIsImmutable = true;
            //Assert.IsTrue(page.IsImmutable);

            //try
            //{
            //    step.Page = page;
            //    Assert.Fail();
            //}
            //catch (NotSupportedException)
            //{
            //    Assert.IsNull(step.Parent);
            //}
            //catch (Exception e)
            //{
            //    Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            //}

            // TODO: the step cannot be added to a read-only IPage
            //step = CreateTestStep();
            //page = new LDPage();
            //page.OverrideIsReadOnly = true;

            //try
            //{
            //    step.Page = page;
            //    Assert.Fail();
            //}
            //catch (NotSupportedException)
            //{
            //    Assert.IsNull(step.Parent);
            //}
            //catch (Exception e)
            //{
            //    Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            //}

            // TODO: check for InvalidOperationException when page.CanInsert() fails its checks

            // Page cannot be set if it already has a value
            step = CreateTestStep();
            step.Page = new LDPage();
            IPage oldPage = step.Page;

            try
            {
                step.Page = new LDPage();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreSame(oldPage, step.Page);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }
        }

        [TestMethod]
        public void PageChangedTest()
        {
            IStep step = (IStep)CreateTestPageElementWithDocumentTree();

            if (!step.IsImmutable)
            {
                IPage page            = step.Page;
                bool eventSeen        = false;
                bool genericEventSeen = false;
                bool addEventSeen     = false;
                bool removeEventSeen  = false;
                bool disposing        = false;

                Assert.IsNotNull(page);

                step.PageChanged += delegate(object sender, PropertyChangedEventArgs<IPage> e)
                {
                    if (disposing)
                        return;

                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(step, sender);

                    if (null == e.NewValue)
                        Assert.AreSame(page, e.OldValue);
                    else
                        Assert.AreSame(page, e.NewValue);
                };

                step.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    if (disposing)
                        return;

                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(step, sender);
                    Assert.AreEqual("PageChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IPage>));

                    PropertyChangedEventArgs<IPage> args = (PropertyChangedEventArgs<IPage>)e.Parameters;

                    if (null == args.NewValue)
                        Assert.AreSame(page, args.OldValue);
                    else
                        Assert.AreSame(page, args.NewValue);
                };

                page.ItemsRemoved += delegate(object sender, UndoableListChangedEventArgs<IStep> e)
                {
                    if (disposing)
                        return;

                    Assert.IsFalse(removeEventSeen);
                    removeEventSeen = true;
                    Assert.AreSame(page, sender);
                };

                page.ItemsAdded += delegate(object sender, UndoableListChangedEventArgs<IStep> e)
                {
                    Assert.IsFalse(addEventSeen);
                    addEventSeen = true;
                    Assert.AreSame(page, sender);
                };

                step.Page = null;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.IsTrue(removeEventSeen);

                eventSeen = false;
                genericEventSeen = false;
                step.Page = page;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.IsTrue(addEventSeen);

                disposing = true;
            }
        }

        [TestMethod]
        public override void StepTest()
        {
            IStep step = CreateTestStep();
            Assert.IsNull(step.Step);
        }

        #endregion Document-tree

        #region Freezing

        [TestMethod]
        public override void IsFrozenTest()
        {
            IStep step = CreateTestStep();
            ILine line = new LDLine();
            step.Add(line);

            Assert.IsFalse(step.IsFrozen);
            Assert.IsFalse(line.IsFrozen);
            step.Freeze();
            Assert.IsTrue(step.IsFrozen);
            Assert.IsTrue(line.IsFrozen);

            base.IsFrozenTest();
        }

        #endregion Freezing

        #region Geometry

        [TestMethod]
        public void BoundingBoxTest()
        {
            IElementCollectionTest.BoundingBoxTest(CreateTestStep());
        }

        [TestMethod]
        public void OriginTest()
        {
            IElementCollectionTest.OriginTest(CreateTestStep());
        }

        [TestMethod]
        public void WindingModeTest()
        {
            IGeometricTest.WindingModeTest(CreateTestStep(), null);
        }

        [TestMethod]
        public void TransformTest()
        {
            IElementCollectionTest.TransformTest(CreateTestStep());
        }

        [TestMethod]
        public void ReverseWindingTest()
        {
            IElementCollectionTest.ReverseWindingTest(CreateTestStep());
        }

        #endregion Geometry

        #region Properties

        [TestMethod]
        public void ModeTest()
        {
            IStep step            = CreateTestStep();
            StepMode defaultValue = StepMode.Additive;
            StepMode newValue     = StepMode.Absolute;

            PropertyValueTest(step,
                              defaultValue,
                              newValue,
                              delegate(IStep obj) { return obj.Mode; },
                              delegate(IStep obj, StepMode value) { obj.Mode = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void ModeChangedTest()
        {
            IStep step          = CreateTestStep();
            StepMode valueToSet = StepMode.Absolute;

            PropertyChangedTest(step,
                                "ModeChanged",
                                valueToSet,
                                delegate(IStep obj, PropertyChangedEventHandler<StepMode> handler) { obj.ModeChanged += handler; },
                                delegate(IStep obj) { return obj.Mode; },
                                delegate(IStep obj, StepMode value) { obj.Mode = value; });
        }

        [TestMethod]
        public void XTest()
        {
            IStep step          = CreateTestStep();
            double defaultValue = 0.0;
            double newValue     = 45.0;

            PropertyValueTest(step,
                              defaultValue,
                              newValue,
                              delegate(IStep obj) { return obj.X; },
                              delegate(IStep obj, double value) { obj.X = value; },
                              PropertyValueFlags.None);

            // range-checks
            step = CreateTestStep();

            if (!step.IsImmutable && !step.IsReadOnly)
            {
                try
                {
                    step.X = -361.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(defaultValue, step.X);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                try
                {
                    step.X = 361.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(defaultValue, step.X);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void XChangedTest()
        {
            IStep step        = CreateTestStep();
            double valueToSet = 45.0;

            PropertyChangedTest(step,
                                "XChanged",
                                valueToSet,
                                delegate(IStep obj, PropertyChangedEventHandler<double> handler) { obj.XChanged += handler; },
                                delegate(IStep obj) { return obj.X; },
                                delegate(IStep obj, double value) { obj.X = value; });
        }

        [TestMethod]
        public void YTest()
        {
            IStep step          = CreateTestStep();
            double defaultValue = 0.0;
            double newValue     = 45.0;

            PropertyValueTest(step,
                              defaultValue,
                              newValue,
                              delegate(IStep obj) { return obj.Y; },
                              delegate(IStep obj, double value) { obj.Y = value; },
                              PropertyValueFlags.None);

            // range-checks
            step = CreateTestStep();

            if (!step.IsImmutable && !step.IsReadOnly)
            {
                try
                {
                    step.Y = -361.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(defaultValue, step.Y);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                try
                {
                    step.Y = 361.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(defaultValue, step.Y);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void YChangedTest()
        {
            IStep step        = CreateTestStep();
            double valueToSet = 45.0;

            PropertyChangedTest(step,
                                "YChanged",
                                valueToSet,
                                delegate(IStep obj, PropertyChangedEventHandler<double> handler) { obj.YChanged += handler; },
                                delegate(IStep obj) { return obj.Y; },
                                delegate(IStep obj, double value) { obj.Y = value; });
        }

        [TestMethod]
        public void ZTest()
        {
            IStep step          = CreateTestStep();
            double defaultValue = 0.0;
            double newValue     = 45.0;

            PropertyValueTest(step,
                              defaultValue,
                              newValue,
                              delegate(IStep obj) { return obj.Z; },
                              delegate(IStep obj, double value) { obj.Z = value; },
                              PropertyValueFlags.None);

            // range-checks
            step = CreateTestStep();

            if (!step.IsImmutable && !step.IsReadOnly)
            {
                try
                {
                    step.Z = -361.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(defaultValue, step.Z);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                try
                {
                    step.Z = 361.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(defaultValue, step.Z);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void ZChangedTest()
        {
            IStep step        = CreateTestStep();
            double valueToSet = 45.0;

            PropertyChangedTest(step,
                                "ZChanged",
                                valueToSet,
                                delegate(IStep obj, PropertyChangedEventHandler<double> handler) { obj.ZChanged += handler; },
                                delegate(IStep obj) { return obj.Z; },
                                delegate(IStep obj, double value) { obj.Z = value; });
        }

        [TestMethod]
        public void StepTransformTest()
        {
            IStep step = CreateTestStep();

            if (!step.IsImmutable && !step.IsReadOnly)
            {
                step.X = 10.0;
                step.Y = 20.0;
                step.Z = 30.0;
                Assert.AreEqual(TestMatrix, step.StepTransform);
            }
        }

        #endregion Properties

        #region View-transform

        [TestMethod]
        public void GetViewTransformTest()
        {
            IStep step1 = CreateTestStep();

            if (!step1.IsImmutable && !step1.IsReadOnly)
            {
                step1.Mode = StepMode.Relative;
                step1.X = 10.0;
                step1.Y = 20.0;
                step1.Z = 30.0;

                IStep step2 = CreateTestStep();
                step2.Mode = StepMode.Additive;
                step2.X = 10.0;
                step2.Y = 20.0;
                step2.Z = 30.0;

                IPage page = new LDPage();
                page.Add(step1);
                page.Add(step2);

                Matrix4d initialTransform = Matrix4d.CreateFromAxisAngle(new Vector3d(1, 2, 3), 45 / 180 * Math.PI);
                Matrix4d viewTransform;
                bool isAbsolute = false;

                Assert.AreEqual(TestMatrix, step1.StepTransform);
                step1.GetViewTransform(ref initialTransform, out viewTransform, ref isAbsolute);
                Assert.AreEqual(initialTransform * TestMatrix, viewTransform);
                Assert.IsFalse(isAbsolute);
                Assert.AreEqual(TestMatrix, step2.StepTransform);
                step2.GetViewTransform(ref initialTransform, out viewTransform, ref isAbsolute);
                Assert.AreEqual(initialTransform * TestMatrix * TestMatrix, viewTransform);
                Assert.IsFalse(isAbsolute);

                step2.Mode = StepMode.Relative;
                step2.GetViewTransform(ref initialTransform, out viewTransform, ref isAbsolute);
                Assert.AreEqual(initialTransform * TestMatrix, viewTransform);
                Assert.IsFalse(isAbsolute);

                step2.Mode = StepMode.Absolute;
                step2.GetViewTransform(ref initialTransform, out viewTransform, ref isAbsolute);
                Assert.AreEqual(TestMatrix, viewTransform);
                Assert.IsTrue(isAbsolute);

                step2.Mode = StepMode.Reset;
                step2.GetViewTransform(ref initialTransform, out viewTransform, ref isAbsolute);
                Assert.AreEqual(initialTransform, viewTransform);
                Assert.IsFalse(isAbsolute);
            }
        }

        #endregion View-transform
    }
}
