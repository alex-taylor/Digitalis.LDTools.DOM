#region License

//
// IReferenceTest.cs
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
    using System.Drawing;
    using System.IO;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IReferenceTest : IGraphicTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IReference); } }

        protected sealed override IGraphic CreateTestGraphic()
        {
            return CreateTestReference();
        }

        protected sealed override IGraphic CreateTestGraphicWithCoordinates()
        {
            return null;
        }

        protected sealed override IGraphic CreateTestGraphicWithColour()
        {
            return CreateTestReference();
        }

        protected sealed override IGraphic CreateTestGraphicWithNoColour()
        {
            return null;
        }

        protected abstract IReference CreateTestReference();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IReference reference = CreateTestReference())
            {
                Assert.AreEqual(DOMObjectType.Reference, reference.ObjectType);
                Assert.IsFalse(reference.IsStateElement);
                Assert.IsFalse(reference.IsTopLevelElement);
                Assert.AreEqual(Palette.MainColour, reference.OverrideableColourValue);
                Assert.IsTrue(reference.ColourValueEnabled);
                Assert.AreEqual(0U, reference.CoordinatesCount);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public sealed override void IsDuplicateOfTest()
        {
            // identical elements
            using (IReference reference = CreateTestReference())
            {
                using (IReference reference2 = (IReference)reference.Clone())
                {
                    Assert.IsTrue(reference.IsDuplicateOf(reference2));
                    Assert.IsTrue(reference2.IsDuplicateOf(reference));
                }

                // different ColourValue
                using (IReference reference2 = CreateTestReference())
                {
                    reference2.ColourValue = 1U;
                    Assert.IsFalse(reference.IsDuplicateOf(reference2));
                    Assert.IsFalse(reference2.IsDuplicateOf(reference));
                }

                // different Matrix
                using (IReference reference2 = CreateTestReference())
                {
                    reference2.Matrix = Matrix4d.Scale(2.0);
                    Assert.IsFalse(reference.IsDuplicateOf(reference2));
                    Assert.IsFalse(reference2.IsDuplicateOf(reference));
                }

                // different TargetName
                using (IReference reference2 = CreateTestReference())
                {
                    reference2.TargetName = "target";
                    Assert.IsFalse(reference.IsDuplicateOf(reference2));
                    Assert.IsFalse(reference2.IsDuplicateOf(reference));
                }

                // different Invert should be ignored
                using (IReference reference2 = CreateTestReference())
                {
                    reference2.Invert = reference.Invert;
                    Assert.IsTrue(reference.IsDuplicateOf(reference2));
                    Assert.IsTrue(reference2.IsDuplicateOf(reference));
                }

                // other properties should be ignored
                using (IReference reference2 = CreateTestReference())
                {
                    reference2.IsLocked = reference.IsLocked;
                    Assert.IsTrue(reference.IsDuplicateOf(reference2));
                    Assert.IsTrue(reference2.IsDuplicateOf(reference));
                }
            }
        }

        #endregion Analytics

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            IReference reference    = CreateTestReference();
            reference.Matrix        = Matrix4d.Scale(2.0);
            reference.TargetName    = "3001.dat";
            reference.Invert        = true;
            reference.TargetContext = MocksFactory.CreateMockDocument();

            IPage target = reference.Target;
            Assert.IsNotNull(target);
            Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);

            return reference;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IReference first  = (IReference)original;
            IReference second = (IReference)copy;

            Assert.AreEqual(first.Matrix, second.Matrix);
            Assert.AreEqual(first.Invert, second.Invert);
            Assert.AreEqual(first.TargetName, second.TargetName);

            // these should not be preserved
            Assert.IsNull(second.TargetContext);
            Assert.AreEqual(TargetStatus.Unresolved, second.TargetStatus);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            IReference reference;
            StringBuilder code;

            // 1. reference with an overrideable colour
            using (reference = CreateTestReference())
            {
                Matrix4d transform = Matrix4d.Scale(2, 3, 4);

                reference.Matrix     = new Matrix4d(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 10, 10, 10, 1);
                reference.TargetName = "test.dat";

                foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
                {
                    // normal winding
                    code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual("1 16 10 10 10 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());

                    // reverse winding: should not change
                    code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Reversed));
                    Assert.AreEqual("1 16 10 10 10 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());

                    // transform
                    code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref transform, WindingDirection.Normal));
                    Assert.AreEqual("1 16 20 30 40 2 0 0 0 3 0 0 0 4 test.dat\r\n", code.ToString());

                    // override the colour with an index
                    code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), codeFormat, 10, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual("1 10 10 10 10 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());

                    // override the colour with an opaque direct colour
                    code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), codeFormat, 0x2FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual("1 #2FF00FF 10 10 10 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());

                    // override the colour with a transparent direct colour
                    code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), codeFormat, 0x3FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual("1 #3FF00FF 10 10 10 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());

                    // override the colour with EdgeColour
                    code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), codeFormat, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual("1 24 10 10 10 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());
                }
            }

            // 2. reference with a fixed colour
            using (reference = CreateTestReference())
            {
                reference.ColourValue = 1U;
                reference.TargetName  = "test.dat";

                foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
                {
                    // overriding the colour will have no effect
                    code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), codeFormat, 10, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual("1 1 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());
                }
            }

            // 3. reference with a fixed colour from a local-palette, which should be converted to a direct-colour in PartsLibrary mode
            using (reference = CreateTestReference())
            {
                IColour localColour  = MocksFactory.CreateMockColour();
                localColour.Code     = 100U;
                localColour.Value    = Color.Red;
                localColour.EdgeCode = 0x2000000;

                IPage page = MocksFactory.CreateMockPage();
                IStep step = MocksFactory.CreateMockStep();
                page.Add(step);
                step.Add(localColour);

                reference.ColourValue = 100U;
                reference.TargetName  = "test.dat";
                step.Add(reference);

                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 100 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 #2FF0000 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 100 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n", code.ToString());
            }

            // 4: inlining
            using (IDocument doc = MocksFactory.CreateMockDocument())
            {
                IPage page1           = MocksFactory.CreateMockPage();
                IStep step1           = MocksFactory.CreateMockStep();
                page1.Name            = "page1";
                page1.PageType        = PageType.Model;
                page1.InlineOnPublish = false;
                doc.Add(page1);
                page1.Add(step1);

                IPage page2           = MocksFactory.CreateMockPage();
                IStep step2           = MocksFactory.CreateMockStep();
                page2.Name            = "page2";
                page2.PageType        = PageType.Part;
                page2.InlineOnPublish = true;
                doc.Add(page2);
                page2.Add(step2);

                IPage page3           = MocksFactory.CreateMockPage();
                IStep step3           = MocksFactory.CreateMockStep();
                page3.Name            = "page3";
                page3.PageType        = PageType.Part;
                page3.InlineOnPublish = false;
                doc.Add(page3);
                page3.Add(step3);

                reference             = CreateTestReference();
                reference.ColourValue = 1;
                reference.Invert      = false;
                reference.TargetName  = "page2.dat";
                reference.Matrix      = new Matrix4d(2, 0, 0, 0, 0, 3, 0, 0, 0, 0, 4, 0, 1, 2, 3, 1);
                step1.Add(reference);

                IReference reference2  = CreateTestReference();
                reference2.ColourValue = Palette.MainColour;
                reference2.Invert      = false;
                reference2.TargetName  = "page3.dat";
                reference2.Matrix      = new Matrix4d(2, 0, 0, 0, 0, 3, 0, 0, 0, 0, 4, 0, 5, 6, 7, 1);

                IReference reference3  = CreateTestReference();
                reference3.ColourValue = 2;
                reference3.Invert      = true;
                reference3.TargetName  = "3001.dat";

                step2.Add(reference2);
                step2.Add(reference3);

                ILine line   = MocksFactory.CreateMockLine();
                line.Vertex1 = Vector3d.UnitX;
                line.Vertex2 = Vector3d.UnitY;
                step2.Add(line);

                // source and target pages both BFC'd, same direction: the inlining should just output the original code
                page1.BFC = CullingMode.CertifiedCounterClockwise;
                page2.BFC = CullingMode.CertifiedCounterClockwise;
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 11 20 31 4 0 0 0 9 0 0 0 16 page3.dat\r\n" +
                                "0 BFC INVERTNEXT\r\n" +
                                "1 2 1 2 3 2 0 0 0 3 0 0 0 4 3001.dat\r\n" +
                                "2 #2333333 3 2 3 1 5 3\r\n",
                                code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());

                // source and target pages both BFC'd, opposite directions: the inlining should reverse the winding of the ILine
                page1.BFC = CullingMode.CertifiedCounterClockwise;
                page2.BFC = CullingMode.CertifiedClockwise;
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 11 20 31 4 0 0 0 9 0 0 0 16 page3.dat\r\n" +
                                "0 BFC INVERTNEXT\r\n" +
                                "1 2 1 2 3 2 0 0 0 3 0 0 0 4 3001.dat\r\n" +
                                "2 #2333333 1 5 3 3 2 3\r\n",
                                code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());

                // source BFC'd, target not: the inlining should disable BFC
                page1.BFC = CullingMode.CertifiedCounterClockwise;
                page2.BFC = CullingMode.Disabled;
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 BFC NOCLIP\r\n" +
                                "1 1 11 20 31 4 0 0 0 9 0 0 0 16 page3.dat\r\n" +
                                "1 2 1 2 3 2 0 0 0 3 0 0 0 4 3001.dat\r\n" +
                                "2 #2333333 3 2 3 1 5 3\r\n",
                                code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());

                // target BFC'd, source not: the inlining should drop any BFC statements from the inlined page
                page1.BFC = CullingMode.Disabled;
                page2.BFC = CullingMode.CertifiedCounterClockwise;
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 11 20 31 4 0 0 0 9 0 0 0 16 page3.dat\r\n" +
                                "1 2 1 2 3 2 0 0 0 3 0 0 0 4 3001.dat\r\n" +
                                "2 #2333333 3 2 3 1 5 3\r\n",
                                code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());

                // attributes: GHOST/HIDE not allowed in PartsLibrary mode, so the inlining should drop the code entirely
                reference.IsGhosted = true;
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 GHOST 1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(String.Empty, code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 GHOST 1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                reference.IsGhosted = false;

                reference.IsVisible = false;
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 MLCAD HIDE 1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(String.Empty, code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 MLCAD HIDE 1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                reference.IsVisible = true;

                // attributes: LOCKNEXT not allowed in PartsLibrary mode, so the inlining should omit it
                reference.IsLocked = true;
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                                "1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n",
                                code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 11 20 31 4 0 0 0 9 0 0 0 16 page3.dat\r\n" +
                                "1 2 1 2 3 2 0 0 0 3 0 0 0 4 3001.dat\r\n" +
                                "2 #2333333 3 2 3 1 5 3\r\n",
                                code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                                "1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n",
                                code.ToString());
                reference.IsLocked = false;

                // attributes: hide one of the members of the inlined page
                reference2.IsVisible = false;
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 2 1 2 3 2 0 0 0 3 0 0 0 4 3001.dat\r\n" +
                                "2 #2333333 3 2 3 1 5 3\r\n",
                                code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                reference2.IsVisible = true;

                // ensure that BFC is renabled when there are more elements after 'reference'
                page1.BFC        = CullingMode.CertifiedCounterClockwise;
                page2.BFC        = CullingMode.Disabled;
                IComment comment = MocksFactory.CreateMockComment();
                comment.Text     = "comment";
                step1.Add(comment);
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 BFC NOCLIP\r\n" +
                                "1 1 11 20 31 4 0 0 0 9 0 0 0 16 page3.dat\r\n" +
                                "1 2 1 2 3 2 0 0 0 3 0 0 0 4 3001.dat\r\n" +
                                "2 #2333333 3 2 3 1 5 3\r\n" +
                                "0 BFC CLIP\r\n",
                                code.ToString());
                code = Utils.PreProcessCode(reference.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("1 1 1 2 3 2 0 0 0 3 0 0 0 4 page2.dat\r\n", code.ToString());
            }

            // 5. attributes test
            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Geometry

        [TestMethod]
        public sealed override void BoundingBoxTest()
        {
            IReference reference = CreateTestReference();
            reference.TargetName = "3001.dat";
            Assert.IsNotNull(reference.Target);
            Assert.AreEqual(reference.Target.BoundingBox, reference.BoundingBox);

            reference.TargetName = "missingfile";
            Assert.IsNull(reference.Target);
            Assert.AreEqual(new Box3d(), reference.BoundingBox);

            Utils.DisposalAccessTest(reference, delegate() { Box3d bounds = reference.BoundingBox; });
        }

        [TestMethod]
        public sealed override void OriginTest()
        {
            IReference reference = CreateTestReference();
            reference.Matrix     = new Matrix4d(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1);
            Assert.AreEqual(Vector3d.UnitX, reference.Origin);

            Utils.DisposalAccessTest(reference, delegate() { Vector3d origin = reference.Origin; });
        }

        [TestMethod]
        public sealed override void TransformTest()
        {
            IReference reference = CreateTestReference();
            reference.Matrix     = new Matrix4d(1, 2, 3, 0, 4, 5, 6, 0, 7, 8, 9, 0, 10, 11, 12, 1);
            Matrix4d matrix      = new Matrix4d(1, 2, 3, 0, 4, 5, 6, 0, 7, 8, 9, 0, 10, 11, 12, 1);
            reference.Transform(ref matrix);
            Assert.AreEqual(matrix * matrix, reference.Matrix);

            Utils.DisposalAccessTest(reference, delegate() { reference.Transform(ref matrix); });
        }

        [TestMethod]
        public sealed override void ReverseWindingTest()
        {
            IReference reference = CreateTestReference();
            reference.Matrix     = new Matrix4d(1, 2, 3, 0, 4, 5, 6, 0, 7, 8, 9, 0, 10, 11, 12, 1);
            reference.TargetName = "3001.dat";
            Matrix4d matrix      = new Matrix4d(1, 2, 3, 0, 4, 5, 6, 0, 7, 8, 9, 0, 10, 11, 12, 1);

            // if the target is 3D then the Invert flag should be toggled
            Assert.IsNotNull(reference.Target);
            reference.ReverseWinding();
            Assert.AreEqual(matrix, reference.Matrix);
            Assert.IsTrue(reference.Invert);

            // if the target is 2D then the matrix should be flipped
            reference.Invert     = false;
            reference.TargetName = "4-4disc.dat";
            Assert.IsNotNull(reference.Target);
            reference.ReverseWinding();
            matrix = new Matrix4d(1, 2, 3, 0, -4, -5, -6, 0, 7, 8, 9, 0, 10, 11, 12, 1);
            Assert.AreEqual(matrix, reference.Matrix);
            Assert.IsFalse(reference.Invert);

            Utils.DisposalAccessTest(reference, delegate() { reference.ReverseWinding(); });
        }

        #endregion Geometry

        #region Properties

        [TestMethod]
        public void MatrixTest()
        {
            using (IReference reference = CreateTestReference())
            {
                Matrix4d defaultValue = Matrix4d.Identity;
                Matrix4d newValue     = Matrix4d.CreateRotationX(45.0);

                PropertyValueTest(reference,
                                  defaultValue,
                                  newValue,
                                  delegate(IReference obj) { return obj.Matrix; },
                                  delegate(IReference obj, Matrix4d value) { obj.Matrix = value; },
                                  PropertyValueFlags.None);
            }

            // the fourth column must be Vector3d.UnitW
            using (IReference reference = CreateTestReference())
            {
                if (!reference.IsImmutable)
                {
                    try
                    {
                        reference.Matrix = new Matrix4d(1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0);
                        Assert.Fail();
                    }
                    catch (ArgumentException)
                    {
                        Assert.AreEqual(Matrix4d.Identity, reference.Matrix);
                    }
                    catch (Exception e)
                    {
                        Assert.AreEqual(typeof(ArgumentException), e.GetType());
                    }
                }
            }
        }

        [TestMethod]
        public void MatrixChangedTest()
        {
            using (IReference reference = CreateTestReference())
            {
                Matrix4d valueToSet = Matrix4d.CreateRotationX(45.0);

                PropertyChangedTest(reference,
                                    "MatrixChanged",
                                    valueToSet,
                                    delegate(IReference obj, PropertyChangedEventHandler<Matrix4d> handler) { obj.MatrixChanged += handler; },
                                    delegate(IReference obj) { return obj.Matrix; },
                                    delegate(IReference obj, Matrix4d value) { obj.Matrix = value; });
            }
        }

        [TestMethod]
        public void InvertTest()
        {
            using (IReference reference = CreateTestReference())
            {
                bool defaultValue = false;
                bool newValue     = true;

                PropertyValueTest(reference,
                                  defaultValue,
                                  newValue,
                                  delegate(IReference obj) { return obj.Invert; },
                                  delegate(IReference obj, bool value) { obj.Invert = value; },
                                  PropertyValueFlags.None);
            }

            // if the target is 2D then this will change Matrix instead
            using (IReference reference = CreateTestReference())
            {
                if (!reference.IsImmutable)
                {
                    reference.TargetName = "4-4disc.dat";

                    // the target must be resolved for this to work
                    Assert.IsNotNull(reference.Target);
                    Assert.IsFalse(reference.Invert);
                    Assert.AreEqual(Matrix4d.Identity, reference.Matrix);
                    reference.Invert = true;
                    Assert.IsFalse(reference.Invert);
                    Assert.AreEqual(new Matrix4d(Vector4d.UnitX, -Vector4d.UnitY, Vector4d.UnitZ, Vector4d.UnitW), reference.Matrix);
                }
            }
        }

        [TestMethod]
        public void InvertChangedTest()
        {
            using (IReference reference = CreateTestReference())
            {
                bool valueToSet = true;

                PropertyChangedTest(reference,
                                    "InvertChanged",
                                    valueToSet,
                                    delegate(IReference obj, PropertyChangedEventHandler<bool> handler) { obj.InvertChanged += handler; },
                                    delegate(IReference obj) { return obj.Invert; },
                                    delegate(IReference obj, bool value) { obj.Invert = value; });
            }
        }

        #endregion Properties

        #region Target-management

        [TestMethod]
        public void TargetNameTest()
        {
            using (IReference reference = CreateTestReference())
            {
                string defaultValue = "Undefined";
                string newValue     = "target";

                PropertyValueTest(reference,
                                  defaultValue,
                                  newValue,
                                  delegate(IReference obj) { return obj.TargetName; },
                                  delegate(IReference obj, string value) { obj.TargetName = value; },
                                  PropertyValueFlags.None);
            }

            // TargetName may not be null/empty/whitespace
            using (IReference reference = CreateTestReference())
            {
                if (!reference.IsImmutable)
                {
                    string oldValue = reference.TargetName;

                    try
                    {
                        reference.TargetName = null;
                        Assert.Fail();
                    }
                    catch (ArgumentNullException)
                    {
                        Assert.AreEqual(oldValue, reference.TargetName);
                    }
                    catch (Exception e)
                    {
                        Assert.AreEqual(typeof(ArgumentException), e.GetType());
                    }

                    try
                    {
                        reference.TargetName = "";
                        Assert.Fail();
                    }
                    catch (ArgumentNullException)
                    {
                        Assert.AreEqual(oldValue, reference.TargetName);
                    }
                    catch (Exception e)
                    {
                        Assert.AreEqual(typeof(ArgumentException), e.GetType());
                    }

                    try
                    {
                        reference.TargetName = "  ";
                        Assert.Fail();
                    }
                    catch (ArgumentNullException)
                    {
                        Assert.AreEqual(oldValue, reference.TargetName);
                    }
                    catch (Exception e)
                    {
                        Assert.AreEqual(typeof(ArgumentException), e.GetType());
                    }
                }
            }
        }

        [TestMethod]
        public void TargetNameChangedTest()
        {
            IReference reference = CreateTestReference();
            string valueToSet    = "target";

            PropertyChangedTest(reference,
                                "TargetNameChanged",
                                valueToSet,
                                delegate(IReference obj, PropertyChangedEventHandler<string> handler) { obj.TargetNameChanged += handler; },
                                delegate(IReference obj) { return obj.TargetName; },
                                delegate(IReference obj, string value) { obj.TargetName = value; });
        }

        [TestMethod]
        public void TargetContextTest()
        {
            IDocument doc = MocksFactory.CreateMockDocument();
            IPage page    = MocksFactory.CreateMockPage();
            IStep step    = MocksFactory.CreateMockStep();
            page.Add(step);
            page.Name     = "test";
            page.PageType = PageType.Model;
            doc.Add(page);

            IPage page2    = MocksFactory.CreateMockPage();
            page2.Name     = "test2";
            page2.PageType = PageType.Model;
            doc.Add(page2);

            using (IReference reference = CreateTestReference())
            {
                reference.TargetName = page2.TargetName;
                Assert.IsNull(reference.Target);
                step.Add(reference);
                Assert.AreSame(page2, reference.Target);

                using (IReference clone = (IReference)reference.Clone())
                {
                    Assert.IsNull(clone.Document);
                    Assert.IsNull(clone.Target);
                    clone.TargetContext = doc;
                    Assert.AreSame(page2, clone.Target);
                }
            }
        }

        [TestMethod]
        public void TargetStatusTest()
        {
            using (IReference reference = CreateTestReference())
            {
                IPage target;

                reference.TargetName = "3001.dat";

                // basic resolve/clear behaviour
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                reference.ClearTarget();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // changing TargetName should cause a status-change
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                reference.TargetName = "3005.dat";
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // try for a non-existent file
                reference.TargetName = "foo.dat";
                target               = reference.Target;
                Assert.IsNull(target);
                Assert.AreEqual(TargetStatus.Missing, reference.TargetStatus);

                // try an extant but invalid file
                // TODO: need a distributable file here
                reference.TargetName = @"C:\LDraw\parts.lst";
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNull(target);
                Assert.AreEqual(TargetStatus.Unloadable, reference.TargetStatus);

                // and a dodgy file
                // TODO: need a distributable file here
                reference.TargetName = @"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.LDrawDocument.CircularReferences.mpd";
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNull(target);
                Assert.AreEqual(TargetStatus.CircularReference, reference.TargetStatus);

                // try for an internal circular-ref
                reference.TargetName = "untitled.ldr";

                IDocument doc  = MocksFactory.CreateMockDocument();
                IPage page1    = MocksFactory.CreateMockPage();
                page1.PageType = PageType.Part;
                page1.Name     = "page1";
                doc.Add(page1);

                IPage page2    = MocksFactory.CreateMockPage();
                page2.PageType = PageType.Part;
                page2.Name     = "page2";
                doc.Add(page2);
                doc[1].Add(MocksFactory.CreateMockStep());

                IReference reference2 = CreateTestReference();
                reference2.TargetName = page1.TargetName;
                doc[1][0].Add(reference2);
                doc[0].Add(MocksFactory.CreateMockStep());
                doc[0][0].Add(reference);

                reference.TargetName = page2.TargetName;
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNull(target);
                Assert.AreEqual(TargetStatus.CircularReference, reference.TargetStatus);
            }
        }

        [TestMethod]
        public void TargetTest()
        {
            // library target: primitive
            using (IReference reference = CreateTestReference())
            {
                reference.TargetName = "4-4disc.dat";
                IPage t              = reference.Target;
                Assert.AreEqual(reference.TargetName, t.TargetName);
                Assert.IsTrue(t.IsFrozen);
            }

            // library target: hires primitive
            using (IReference reference = CreateTestReference())
            {
                reference.TargetName = @"48\4-4disc.dat";
                IPage t              = reference.Target;
                Assert.AreEqual(reference.TargetName, t.TargetName);
                Assert.IsTrue(t.IsFrozen);
            }

            // library target: part
            using (IReference reference = CreateTestReference())
            {
                reference.TargetName = "3001.dat";
                IPage t              = reference.Target;
                Assert.AreEqual(reference.TargetName, t.TargetName);
                Assert.IsTrue(t.IsFrozen);
            }

            // library target: subpart
            using (IReference reference = CreateTestReference())
            {
                reference.TargetName = @"s\10s01.dat";
                IPage t = reference.Target;
                Assert.AreEqual(reference.TargetName, t.TargetName);
                Assert.IsTrue(t.IsFrozen);
            }

            // library target: model
            using (IReference reference = CreateTestReference())
            {
                // TODO: need a distributable file here
                reference.TargetName = "A-Wing.mpd";
                IPage t = reference.Target;

                // note that the Target's TargetName is the name of its first page, not the file
                Assert.AreEqual("Untitled.ldr", t.TargetName);
                Assert.IsTrue(t.IsFrozen);
            }

            // filepath
            using (IReference reference = CreateTestReference())
            {
                // TODO: need a distributable file here
                reference.TargetName = @"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.MLCadGroup.ldr";
                IPage t = reference.Target;
                Assert.AreEqual("Test.MLCadGroup.ldr", t.TargetName);
                Assert.IsTrue(t.IsFrozen);
            }

            // local target
            IDocument doc = MocksFactory.CreateMockDocument();
            IPage page    = MocksFactory.CreateMockPage();
            IStep step    = MocksFactory.CreateMockStep();
            page.Add(step);
            page.Name     = "test";
            page.PageType = PageType.Model;
            doc.Add(page);

            IPage page2    = MocksFactory.CreateMockPage();
            page2.Name     = "test2";
            page2.PageType = PageType.Model;
            doc.Add(page2);

            using (IReference reference = CreateTestReference())
            {
                reference.TargetName = page2.TargetName;
                step.Add(reference);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
            }

            // invalid target
            using (IReference reference = CreateTestReference())
            {
                // TODO: need a distributable file here
                reference.TargetName = @"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.LDrawDocument.CircularReferences.mpd";
                Assert.IsNull(reference.Target);
            }
        }

        [TestMethod]
        public void TargetChangedTest()
        {
            IDocument doc = MocksFactory.CreateMockDocument();
            IPage page    = MocksFactory.CreateMockPage();
            page.Name     = "test";
            page.PageType = PageType.Model;
            doc.Add(page);

            IPage page2    = MocksFactory.CreateMockPage();
            page2.Name     = "test2";
            page2.PageType = PageType.Model;
            doc.Add(page2);

            using (IReference reference = CreateTestReference())
            {
                reference.TargetName = page2.TargetName;

                IStep step = MocksFactory.CreateMockStep();
                page.Add(step);
                step.Add(reference);

                bool eventSeen        = false;
                bool genericEventSeen = false;

                reference.TargetChanged += delegate(object sender, EventArgs e)
                {
                    eventSeen = true;
                    Assert.AreSame(reference, sender);
                };

                reference.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    if ("TargetChanged" == e.Operation)
                    {
                        genericEventSeen = true;
                        Assert.AreSame(reference, sender);
                    }
                };

                // resolve the target
                Assert.IsNotNull(reference.Target);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;

                // add an element to the page
                IComment comment = MocksFactory.CreateMockComment();
                IStep step2      = MocksFactory.CreateMockStep();
                page2.Add(step2);
                step2.Add(comment);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;

                // change a property of the element in the page
                comment.Text = "text";
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;

                // change the page's Name
                page2.Name = "foobar";
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.AreEqual("foobar.ldr", reference.TargetName);
                eventSeen        = false;
                genericEventSeen = false;

                // change the page's PageType
                page2.PageType = PageType.Part;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.AreEqual("foobar.dat", reference.TargetName);
                eventSeen        = false;
                genericEventSeen = false;

                // change some other property of the page
                page2.Title = "new title";
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;

                // replace an element
                ILine line = MocksFactory.CreateMockLine();
                step2[0]   = line;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;

                // remove an element
                step2.Remove(line);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;

                // clear the page
                step2.Add(line);
                step2.Add(comment);
                step2.Clear();
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;

                // and finally remove the page
                doc.Remove(page2);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;

                // add a local target which overrides a valid library target
                reference.TargetName = "3001.dat";
                IPage target         = reference.Target;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;
                page2.Name       = "3001";
                page2.PageType   = PageType.Part;
                doc.Add(page2);
                target = reference.Target;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.IsNotNull(target);
                Assert.AreSame(target, page2);
                eventSeen        = false;
                genericEventSeen = false;

                // a missing target should resolve if the page is renamed
                reference.TargetName = "foo.dat";
                Assert.IsNull(reference.Target);
                page2.Name = "foo";
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
            }
        }

        [TestMethod]
        public void ClearTargetTest()
        {
            // each of the following should cause Target to clear
            using (IReference reference = CreateTestReference())
            {
                reference.TargetName = "3001.dat";

                // manual clear
                Assert.IsNotNull(reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                reference.ClearTarget();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                IDocument doc = MocksFactory.CreateMockDocument();
                IPage page    = MocksFactory.CreateMockPage();
                IStep step    = MocksFactory.CreateMockStep();

                page.Add(step);
                step.Add(reference);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // add page to document
                Assert.IsNotNull(reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                doc.Add(page);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // remove page from document
                Assert.IsNotNull(reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                doc.Remove(page);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // remove from step
                Assert.IsNotNull(reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                step.Remove(reference);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // add to step
                Assert.IsNotNull(reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                step.Add(reference);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // remove step from page
                Assert.IsNotNull(reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                page.Remove(step);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // add step to page
                Assert.IsNotNull(reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                page.Add(step);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // resolve to local target and then remove it
                IPage localTarget    = MocksFactory.CreateMockPage();
                localTarget.PageType = PageType.Model;
                localTarget.Name     = "localtarget";
                doc.Add(localTarget);
                doc.Add(page);
                reference.TargetName = localTarget.TargetName;
                Assert.IsNotNull(reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                Assert.AreSame(localTarget, reference.Target);
                doc.Remove(localTarget);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
            }
        }

        [TestMethod]
        public void TargetAutoUpdateOnRenamedPageTest()
        {
            using (IDocument document = MocksFactory.CreateMockDocument())
            {
                UndoStack undoStack = new UndoStack();

                IPage page1    = MocksFactory.CreateMockPage();
                page1.PageType = PageType.Part;
                page1.Name     = "page1";
                document.Add(page1);

                IStep step1 = MocksFactory.CreateMockStep();
                page1.Add(step1);

                IPage page2    = MocksFactory.CreateMockPage();
                page2.PageType = PageType.Part;
                page2.Name     = "page2";
                document.Add(page2);

                IStep step2 = MocksFactory.CreateMockStep();
                page2.Add(step2);

                IReference reference = CreateTestReference();
                reference.TargetName = page2.TargetName;
                step1.Add(reference);

                int eventSeen = 0;

                reference.TargetChanged += delegate(object sender, EventArgs e)
                {
                    eventSeen++;
                };

                // 1. if a local-target which matches TargetName is renamed, TargetName should update and TargetChanged should be raised

                // 1a. the reference has resolved to the local-target
                Assert.IsNotNull(reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                Assert.AreSame(page2, reference.Target);

                eventSeen  = 0;
                page2.Name = "newname";
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                Assert.AreSame(page2, reference.Target);

                eventSeen      = 0;
                page2.PageType = PageType.Model;
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                Assert.AreSame(page2, reference.Target);

                // should also work if the reference is locked
                reference.IsLocked = true;
                page2.Name         = "name";
                reference.IsLocked = false;
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);

                // undo/redo
                undoStack.StartCommand("command");
                page2.Name = "oldname";
                undoStack.EndCommand();
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                Assert.AreSame(page2, reference.Target);
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                Assert.AreSame(page2, reference.Target);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                Assert.AreSame(page2, reference.Target);

                // 1b. the reference has not yet resolved
                reference.ClearTarget();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                eventSeen  = 0;
                page2.Name = "newname2";
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                eventSeen      = 0;
                page2.PageType = PageType.Part;
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // should also work if the reference is locked
                reference.ClearTarget();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                reference.IsLocked = true;
                page2.Name         = "name";
                reference.IsLocked = false;
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);

                // undo/redo
                reference.ClearTarget();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                undoStack.StartCommand("command");
                page2.Name = "oldname";
                undoStack.EndCommand();
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // 2. if a local-target is renamed to match TargetName, TargetChanged should be raised

                // 2a. the reference has resolved to a target elsewhere
                reference.TargetName = "3001.dat";
                Assert.IsNotNull(reference.Target);
                Assert.AreNotSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);

                eventSeen  = 0;
                page2.Name = "3001";
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);

                // should also work if the reference is locked
                page2.Name           = "oldname";
                page2.PageType       = PageType.Part;
                reference.TargetName = "3001.dat";

                reference.IsLocked   = true;
                page2.Name           = "3001";
                reference.IsLocked   = false;
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);

                // undo/redo
                page2.Name           = "oldname";
                page2.PageType       = PageType.Part;
                reference.TargetName = "3001.dat";
                Assert.IsNotNull(reference.Target);
                Assert.AreNotSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                undoStack.StartCommand("command");
                page2.Name = "3001";
                undoStack.EndCommand();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreNotSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);

                // 2b. the reference has not yet resolved
                reference.ClearTarget();
                reference.TargetName = "foobar.dat";
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);

                eventSeen  = 0;
                page2.Name = "foobar";
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);

                // should also work if the reference is locked
                page2.Name     = "oldname";
                page2.PageType = PageType.Part;
                reference.ClearTarget();
                reference.TargetName = "foobar.dat";

                reference.IsLocked = true;
                page2.Name         = "foobar";
                reference.IsLocked = false;
                Assert.AreEqual(page2.TargetName, reference.TargetName);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);

                // undo/redo
                page2.Name     = "oldname";
                page2.PageType = PageType.Part;
                reference.ClearTarget();
                reference.TargetName = "foobar.dat";
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);
                undoStack.StartCommand("command");
                page2.Name = "foobar";
                undoStack.EndCommand();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
            }
        }

        [TestMethod]
        public void TargetAutoUpdateOnAddedOrRemovedPageTest()
        {
            using (IDocument document = MocksFactory.CreateMockDocument())
            {
                UndoStack undoStack = new UndoStack();

                IPage page1    = MocksFactory.CreateMockPage();
                page1.PageType = PageType.Part;
                page1.Name     = "page1";
                document.Add(page1);

                IStep step1 = MocksFactory.CreateMockStep();
                page1.Add(step1);

                IPage page2    = MocksFactory.CreateMockPage();
                page2.PageType = PageType.Part;
                page2.Name     = "page2";

                IPage page3    = MocksFactory.CreateMockPage();
                page3.PageType = PageType.Part;
                page3.Name     = "page3";

                IStep step2 = MocksFactory.CreateMockStep();
                page2.Add(step2);

                IReference reference = CreateTestReference();
                step1.Add(reference);
                reference.TargetName = page2.TargetName;

                int eventSeen = 0;

                reference.TargetChanged += delegate(object sender, EventArgs e)
                {
                    eventSeen++;
                };

                // 1. if a local-target which matches TargetName is added, TargetChanged should be raised

                // 1a. the reference has not yet resolved to anything
                Assert.IsNull(reference.Target);
                Assert.AreEqual(TargetStatus.Missing, reference.TargetStatus);

                eventSeen = 0;
                document.Add(page2);
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);

                document.Remove(page2);

                // undo/redo
                undoStack.StartCommand("command");
                document.Add(page2);
                undoStack.EndCommand();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);

                document.Remove(page2);

                // 1b. the reference is resolved to an external-target
                reference.TargetName = "3001.dat";
                page2.Name           = "3001";
                Assert.AreEqual(reference.TargetName, page2.TargetName);
                Assert.IsNotNull(reference.Target);
                Assert.AreNotSame(page2, reference.Target);

                eventSeen = 0;
                document.Add(page2);
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);

                document.Remove(page2);
                Assert.IsNotNull(reference.Target);
                Assert.AreNotSame(page2, reference.Target);

                // undo/redo
                undoStack.StartCommand("command");
                document.Add(page2);
                undoStack.EndCommand();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreNotSame(page2, reference.Target);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);

                // 2. a local-target which matches TargetName is removed

                // 2a. the reference has not yet resolved to anything: TargetChanged should not be raised as there's no change to the reference
                reference.TargetName = "page2.dat";
                page2.Name           = "page2";
                reference.ClearTarget();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.AreEqual(reference.TargetName, page2.TargetName);

                eventSeen = 0;
                document.Remove(page2);
                Assert.AreEqual(0, eventSeen);
                Assert.IsNull(reference.Target);
                Assert.AreEqual(TargetStatus.Missing, reference.TargetStatus);

                document.Add(page2);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // undo/redo
                undoStack.StartCommand("command");
                document.Remove(page2);
                undoStack.EndCommand();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);
                Assert.AreEqual(TargetStatus.Missing, reference.TargetStatus);
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                reference.ClearTarget();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(0, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);
                Assert.AreEqual(TargetStatus.Missing, reference.TargetStatus);

                document.Add(page2);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // 2b. the reference is resolved to the local-target being removed: TargetChanged should be raised
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);

                eventSeen = 0;
                document.Remove(page2);
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);

                document.Add(page2);
                reference.ClearTarget();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                // undo/redo
                undoStack.StartCommand("command");
                document.Remove(page2);
                undoStack.EndCommand();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page2, reference.Target);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);

                // 3. if a local-target which does not match TargetName is replaced with one which does, TargetChanged should be raised
                document.Add(page2);
                page2.Name           = "foobar";
                reference.TargetName = page3.TargetName;
                reference.ClearTarget();
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);

                eventSeen   = 0;
                document[1] = page3;
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page3, reference.Target);

                // 4. if a local-target which matches TargetName is replaced with one which does not, TargetChanged should be raised if Target was resolved
                eventSeen   = 0;
                document[1] = page2;
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);

                document[1] = page3;
                reference.ClearTarget();

                eventSeen   = 0;
                document[1] = page2;
                Assert.AreEqual(0, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);

                // 5. if a local-target which matches TargetName is replaced with another, TargetChanged should be raised
                page3.Name           = page2.Name;
                page3.PageType       = page2.PageType;
                reference.TargetName = page2.TargetName;

                eventSeen   = 0;
                document[1] = page3;
                Assert.AreEqual(1, eventSeen);
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNotNull(reference.Target);
                Assert.AreSame(page3, reference.Target);

                // 6. if a local-target which does not match TargetName is replaced with another, TargetChanged should not be raised
                reference.TargetName = "missing.dat";
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                Assert.IsNull(reference.Target);

                page2.Name = "page2";
                Assert.AreNotEqual(page2.TargetName, page3.TargetName);
                eventSeen   = 0;
                document[1] = page2;
                Assert.AreEqual(0, eventSeen);
            }
        }

        [TestMethod]
        public void TargetAutoUpdateOnDocumentPathChangedTest()
        {
            using (IDocument document = MocksFactory.CreateMockDocument())
            {
                UndoStack undoStack = new UndoStack();

                IPage page1    = MocksFactory.CreateMockPage();
                page1.PageType = PageType.Part;
                page1.Name     = "page1";
                document.Add(page1);

                IStep step1 = MocksFactory.CreateMockStep();
                page1.Add(step1);

                IPage page2    = MocksFactory.CreateMockPage();
                page2.PageType = PageType.Part;
                page2.Name     = "page2";
                document.Add(page2);

                IStep step2 = MocksFactory.CreateMockStep();
                page2.Add(step2);

                IReference reference = CreateTestReference();
                reference.TargetName = page2.TargetName;

                int eventSeen = 0;

                reference.TargetChanged += delegate(object sender, EventArgs e)
                {
                    eventSeen++;
                };

                // 1. the reference is added to a document which contains a page with a matching TargetName
                step1.Add(reference);
                Assert.AreEqual(1, eventSeen);

                step1.Remove(reference);

                // undo/redo
                undoStack.StartCommand("command");
                step1.Add(reference);
                undoStack.EndCommand();
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(0, eventSeen);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(1, eventSeen);

                step1.Remove(reference);

                // 2. the reference is added to a document which does not contain a page with a matching TargetName
                reference.TargetName = "foo.dat";
                eventSeen = 0;
                step1.Add(reference);
                Assert.AreEqual(0, eventSeen);

                step1.Remove(reference);

                // undo/redo
                undoStack.StartCommand("command");
                step1.Add(reference);
                undoStack.EndCommand();
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(0, eventSeen);
                undoStack.Redo();
                Assert.AreEqual(0, eventSeen);

                // 3. the reference is removed from a document which contains a page with a matching TargetName
                reference.TargetName = page2.TargetName;
                eventSeen = 0;
                step1.Remove(reference);
                Assert.AreEqual(0, eventSeen);

                step1.Add(reference);

                //undo/redo
                undoStack.StartCommand("command");
                step1.Remove(reference);
                undoStack.EndCommand();
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(1, eventSeen);
                eventSeen = 0;
                undoStack.Redo();
                Assert.AreEqual(0, eventSeen);

                // 4. the reference is removed from a document which does not contain a page with a matching TargetName
                reference.TargetName = "foo.dat";
                eventSeen = 0;
                step1.Remove(reference);
                Assert.AreEqual(0, eventSeen);

                step1.Add(reference);

                //undo/redo
                undoStack.StartCommand("command");
                step1.Remove(reference);
                undoStack.EndCommand();
                eventSeen = 0;
                undoStack.Undo();
                Assert.AreEqual(0, eventSeen);
                undoStack.Redo();
                Assert.AreEqual(0, eventSeen);
            }
        }

        #endregion Target-management
    }
}
