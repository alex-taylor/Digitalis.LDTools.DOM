#region License

//
// IQuadrilateralTest.cs
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
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IQuadrilateralTest : IGraphicTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IQuadrilateral); } }

        protected sealed override IGraphic CreateTestGraphic()
        {
            return CreateTestQuadrilateral();
        }

        protected sealed override IGraphic CreateTestGraphicWithCoordinates()
        {
            return CreateTestQuadrilateral();
        }

        protected sealed override IGraphic CreateTestGraphicWithColour()
        {
            return CreateTestQuadrilateral();
        }

        protected sealed override IGraphic CreateTestGraphicWithNoColour()
        {
            return null;
        }

        protected abstract IQuadrilateral CreateTestQuadrilateral();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            Assert.AreEqual(DOMObjectType.Quadrilateral, quadrilateral.ObjectType);
            Assert.IsFalse(quadrilateral.IsStateElement);
            Assert.IsFalse(quadrilateral.IsTopLevelElement);
            Assert.AreEqual(Palette.MainColour, quadrilateral.OverrideableColourValue);
            Assert.IsTrue(quadrilateral.ColourValueEnabled);
            Assert.AreEqual(4U, quadrilateral.CoordinatesCount);
            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public sealed override void IsDuplicateOfTest()
        {
            // identical elements
            IQuadrilateral quadrilateral  = CreateTestQuadrilateral();
            quadrilateral.Vertex2         = new Vector3d(1, 1, 1);
            quadrilateral.Vertex3         = new Vector3d(2, 2, 2);
            quadrilateral.Vertex4         = new Vector3d(3, 3, 3);
            IQuadrilateral quadrilateral2 = (IQuadrilateral)quadrilateral.Clone();
            Assert.IsTrue(quadrilateral.IsDuplicateOf(quadrilateral2));
            Assert.IsTrue(quadrilateral2.IsDuplicateOf(quadrilateral));

            // different ColourValue
            quadrilateral2.ColourValue = 1U;
            Assert.IsFalse(quadrilateral.IsDuplicateOf(quadrilateral2));
            Assert.IsFalse(quadrilateral2.IsDuplicateOf(quadrilateral));

            // different vertices
            quadrilateral2.ColourValue = quadrilateral.ColourValue;
            quadrilateral2.Vertex1     = new Vector3d(1, 2, 3);
            quadrilateral2.Vertex2     = new Vector3d(4, 5, 6);
            quadrilateral2.Vertex3     = new Vector3d(7, 8, 9);
            quadrilateral2.Vertex4     = new Vector3d(10, 11, 12);
            Assert.IsFalse(quadrilateral.IsDuplicateOf(quadrilateral2));
            Assert.IsFalse(quadrilateral2.IsDuplicateOf(quadrilateral));

            // same vertices, but in a different order
            quadrilateral2.Vertex1 = quadrilateral.Vertex2;
            quadrilateral2.Vertex2 = quadrilateral.Vertex3;
            quadrilateral2.Vertex3 = quadrilateral.Vertex4;
            quadrilateral2.Vertex4 = quadrilateral.Vertex1;
            Assert.IsTrue(quadrilateral.IsDuplicateOf(quadrilateral2));
            Assert.IsTrue(quadrilateral2.IsDuplicateOf(quadrilateral));

            // other properties should be ignored
            quadrilateral.IsLocked = !quadrilateral2.IsLocked;
            Assert.IsTrue(quadrilateral.IsDuplicateOf(quadrilateral2));
            Assert.IsTrue(quadrilateral2.IsDuplicateOf(quadrilateral));

            // a quadrilateral is not a duplicate of itself
            Assert.IsFalse(quadrilateral.IsDuplicateOf(quadrilateral));

            Utils.DisposalAccessTest(quadrilateral, delegate() { bool isDuplicate = quadrilateral.IsDuplicateOf(quadrilateral2); });
        }

        [TestMethod]
        public void IsBowtieTest()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            quadrilateral.Vertex2 = new Vector3d(10, 10, 0);
            quadrilateral.Vertex3 = new Vector3d(10, 0, 0);
            quadrilateral.Vertex4 = new Vector3d(0, 10, 0);
            Assert.IsTrue(quadrilateral.IsBowtie);

            quadrilateral.Vertex3 = new Vector3d(10, 10, 0);
            quadrilateral.Vertex4 = new Vector3d(0, 10, 0);
            Assert.IsFalse(quadrilateral.IsBowtie);

            Utils.DisposalAccessTest(quadrilateral, delegate() { bool isBowtie = quadrilateral.IsBowtie; });
        }

        #endregion Analytics

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            IQuadrilateral quadrilateral;
            StringBuilder code;

            quadrilateral         = CreateTestQuadrilateral();
            quadrilateral.Vertex1 = new Vector3d(1, 2, 3);
            quadrilateral.Vertex2 = new Vector3d(4, 5, 6);
            quadrilateral.Vertex3 = new Vector3d(7, 8, 9);
            quadrilateral.Vertex4 = new Vector3d(10, 11, 12);

            // 1. quadrilateral with an overrideable colour
            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                Matrix4d transform = Matrix4d.Scale(2, 3, 4);

                // normal winding
                code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n", code.ToString());

                // reverse winding
                code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Reversed));
                Assert.AreEqual("4 16 10 11 12 7 8 9 4 5 6 1 2 3\r\n", code.ToString());

                // transform
                code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref transform, WindingDirection.Normal));
                Assert.AreEqual("4 16 2 6 12 8 15 24 14 24 36 20 33 48\r\n", code.ToString());

                // override the colour with an index
                code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), codeFormat, 10, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("4 10 1 2 3 4 5 6 7 8 9 10 11 12\r\n", code.ToString());

                // override the colour with an opaque direct colour
                code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), codeFormat, 0x2FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("4 #2FF00FF 1 2 3 4 5 6 7 8 9 10 11 12\r\n", code.ToString());

                // override the colour with a transparent direct colour
                code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), codeFormat, 0x3FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("4 #3FF00FF 1 2 3 4 5 6 7 8 9 10 11 12\r\n", code.ToString());

                // override the colour with EdgeColour - should have no effect, as this is not a legal colour for a quad
                code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), CodeStandards.Full, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n", code.ToString());
            }

            // 2. quadrilateral with a fixed colour
            quadrilateral.ColourValue = 1U;

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                // overriding the colour will have no effect
                code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), codeFormat, 10, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("4 1 1 2 3 4 5 6 7 8 9 10 11 12\r\n", code.ToString());
            }

            // 3. quadrilateral with a fixed colour from a local-palette, which should be converted to a direct-colour in PartsLibrary mode
            IColour localColour  = MocksFactory.CreateMockColour();
            localColour.Code     = 100U;
            localColour.Value    = Color.Red;
            localColour.EdgeCode = 0x2000000;

            IPage page = MocksFactory.CreateMockPage();
            IStep step = MocksFactory.CreateMockStep();
            page.Add(step);
            step.Add(localColour);

            quadrilateral.ColourValue = 100U;
            step.Add(quadrilateral);

            code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("4 100 1 2 3 4 5 6 7 8 9 10 11 12\r\n", code.ToString());
            code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("4 #2FF0000 1 2 3 4 5 6 7 8 9 10 11 12\r\n", code.ToString());
            code = Utils.PreProcessCode(quadrilateral.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("4 100 1 2 3 4 5 6 7 8 9 10 11 12\r\n", code.ToString());

            // 4. attributes test
            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Geometry

        [TestMethod]
        public sealed override void OriginTest()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            quadrilateral.Vertex1        = new Vector3d(1, 0, 0);
            quadrilateral.Vertex2        = new Vector3d(0, 1, 0);
            quadrilateral.Vertex3        = new Vector3d(0, 0, 1);
            quadrilateral.Vertex4        = new Vector3d(1, 1, 1);
            Assert.AreEqual(quadrilateral.Vertex1, quadrilateral.Origin);

            Utils.DisposalAccessTest(quadrilateral, delegate() { Vector3d origin = quadrilateral.Origin; });
        }

        [TestMethod]
        public sealed override void ReverseWindingTest()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            quadrilateral.Vertex1        = new Vector3d(1, 0, 0);
            quadrilateral.Vertex2        = new Vector3d(0, 1, 0);
            quadrilateral.Vertex3        = new Vector3d(0, 0, 1);
            quadrilateral.Vertex4        = new Vector3d(1, 1, 1);
            quadrilateral.ReverseWinding();
            Assert.AreEqual(new Vector3d(1, 1, 1), quadrilateral.Vertex1);
            Assert.AreEqual(new Vector3d(0, 0, 1), quadrilateral.Vertex2);
            Assert.AreEqual(new Vector3d(0, 1, 0), quadrilateral.Vertex3);
            Assert.AreEqual(new Vector3d(1, 0, 0), quadrilateral.Vertex4);

            Utils.DisposalAccessTest(quadrilateral, delegate() { quadrilateral.ReverseWinding(); });
        }

        [TestMethod]
        public void RepairBowtieTest()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            quadrilateral.Vertex2        = new Vector3d(10, 10, 0);
            quadrilateral.Vertex3        = new Vector3d(10, 0, 0);
            quadrilateral.Vertex4        = new Vector3d(0, 10, 0);
            Assert.IsTrue(quadrilateral.IsBowtie);
            quadrilateral.RepairBowtie();
            Assert.IsFalse(quadrilateral.IsBowtie);
            Assert.AreEqual(Vector3d.Zero, quadrilateral.Vertex1);
            Assert.AreEqual(new Vector3d(10, 0, 0), quadrilateral.Vertex2);
            Assert.AreEqual(new Vector3d(10, 10, 0), quadrilateral.Vertex3);
            Assert.AreEqual(new Vector3d(0, 10, 0), quadrilateral.Vertex4);

            Utils.DisposalAccessTest(quadrilateral, delegate() { quadrilateral.RepairBowtie(); });
        }

        #endregion Geometry

        #region Properties

        [TestMethod]
        public void Vertex1Test()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            Vector3d defaultValue        = Vector3d.Zero;
            Vector3d newValue            = Vector3d.UnitX;

            PropertyValueTest(quadrilateral,
                              defaultValue,
                              newValue,
                              delegate(IQuadrilateral obj) { return obj.Vertex1; },
                              delegate(IQuadrilateral obj, Vector3d value) { obj.Vertex1 = value; },
                              PropertyValueFlags.None);

            // Vertex1==Coordinates[0]
            quadrilateral = CreateTestQuadrilateral();

            if (!quadrilateral.IsImmutable)
                quadrilateral.Vertex1 = Vector3d.UnitX;

            Assert.AreEqual(quadrilateral.Vertex1, quadrilateral.Coordinates.ElementAt(0));
        }

        [TestMethod]
        public void Vertex1ChangedTest()
        {
            IQuadrilateral quadrilateral   = CreateTestQuadrilateral();
            IEnumerable<Vector3d> oldValue = quadrilateral.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.UnitX, Vector3d.Zero, Vector3d.Zero, Vector3d.Zero };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!quadrilateral.IsImmutable)
            {
                quadrilateral.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(quadrilateral, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(oldValue.ElementAt(3), e.OldValue.ElementAt(3));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(3), e.NewValue.ElementAt(3));
                };

                quadrilateral.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(quadrilateral, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(oldValue.ElementAt(3), args.OldValue.ElementAt(3));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(3), args.NewValue.ElementAt(3));
                };

                quadrilateral.Vertex1 = newValue.ElementAt(0);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen             = false;
                genericEventSeen      = false;
                quadrilateral.Vertex1 = newValue.ElementAt(0);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void Vertex2Test()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            Vector3d defaultValue        = Vector3d.Zero;
            Vector3d newValue            = Vector3d.UnitX;

            PropertyValueTest(quadrilateral,
                              defaultValue,
                              newValue,
                              delegate(IQuadrilateral obj) { return obj.Vertex2; },
                              delegate(IQuadrilateral obj, Vector3d value) { obj.Vertex2 = value; },
                              PropertyValueFlags.None);

            // Vertex2==Coordinates[1]
            quadrilateral = CreateTestQuadrilateral();

            if (!quadrilateral.IsImmutable)
                quadrilateral.Vertex2 = Vector3d.UnitX;

            Assert.AreEqual(quadrilateral.Vertex2, quadrilateral.Coordinates.ElementAt(1));
        }

        [TestMethod]
        public void Vertex2ChangedTest()
        {
            IQuadrilateral quadrilateral   = CreateTestQuadrilateral();
            IEnumerable<Vector3d> oldValue = quadrilateral.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.Zero, Vector3d.UnitX, Vector3d.Zero, Vector3d.Zero };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!quadrilateral.IsImmutable)
            {
                quadrilateral.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(quadrilateral, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(oldValue.ElementAt(3), e.OldValue.ElementAt(3));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(3), e.NewValue.ElementAt(3));
                };

                quadrilateral.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(quadrilateral, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(oldValue.ElementAt(3), args.OldValue.ElementAt(3));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(3), args.NewValue.ElementAt(3));
                };

                quadrilateral.Vertex2 = newValue.ElementAt(1);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen             = false;
                genericEventSeen      = false;
                quadrilateral.Vertex2 = newValue.ElementAt(1);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void Vertex3Test()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            Vector3d defaultValue        = Vector3d.Zero;
            Vector3d newValue            = Vector3d.UnitX;

            PropertyValueTest(quadrilateral,
                              defaultValue,
                              newValue,
                              delegate(IQuadrilateral obj) { return obj.Vertex3; },
                              delegate(IQuadrilateral obj, Vector3d value) { obj.Vertex3 = value; },
                              PropertyValueFlags.None);

            // Vertex3=Coordinates[2]
            quadrilateral = CreateTestQuadrilateral();

            if (!quadrilateral.IsImmutable)
                quadrilateral.Vertex3 = Vector3d.UnitX;

            Assert.AreEqual(quadrilateral.Vertex3, quadrilateral.Coordinates.ElementAt(2));
        }

        [TestMethod]
        public void Vertex3ChangedTest()
        {
            IQuadrilateral quadrilateral   = CreateTestQuadrilateral();
            IEnumerable<Vector3d> oldValue = quadrilateral.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.Zero, Vector3d.Zero, Vector3d.UnitX, Vector3d.Zero };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!quadrilateral.IsImmutable)
            {
                quadrilateral.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(quadrilateral, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(oldValue.ElementAt(3), e.OldValue.ElementAt(3));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(3), e.NewValue.ElementAt(3));
                };

                quadrilateral.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(quadrilateral, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(oldValue.ElementAt(3), args.OldValue.ElementAt(3));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(3), args.NewValue.ElementAt(3));
                };

                quadrilateral.Vertex3 = newValue.ElementAt(2);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen             = false;
                genericEventSeen      = false;
                quadrilateral.Vertex3 = newValue.ElementAt(2);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void Vertex4Test()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            Vector3d defaultValue        = Vector3d.Zero;
            Vector3d newValue            = Vector3d.UnitX;

            PropertyValueTest(quadrilateral,
                              defaultValue,
                              newValue,
                              delegate(IQuadrilateral obj) { return obj.Vertex4; },
                              delegate(IQuadrilateral obj, Vector3d value) { obj.Vertex4 = value; },
                              PropertyValueFlags.None);

            // Vertex4==Coordinates[3]
            quadrilateral = CreateTestQuadrilateral();

            if (!quadrilateral.IsImmutable)
                quadrilateral.Vertex4 = Vector3d.UnitX;

            Assert.AreEqual(quadrilateral.Vertex4, quadrilateral.Coordinates.ElementAt(3));
        }

        [TestMethod]
        public void Vertex4ChangedTest()
        {
            IQuadrilateral quadrilateral   = CreateTestQuadrilateral();
            IEnumerable<Vector3d> oldValue = quadrilateral.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.Zero, Vector3d.Zero, Vector3d.Zero, Vector3d.UnitX };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!quadrilateral.IsImmutable)
            {
                quadrilateral.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(quadrilateral, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(oldValue.ElementAt(3), e.OldValue.ElementAt(3));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(3), e.NewValue.ElementAt(3));
                };

                quadrilateral.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(quadrilateral, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(oldValue.ElementAt(3), args.OldValue.ElementAt(3));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(3), args.NewValue.ElementAt(3));
                };

                quadrilateral.Vertex4 = newValue.ElementAt(3);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen             = false;
                genericEventSeen      = false;
                quadrilateral.Vertex4 = newValue.ElementAt(3);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void NormalTest()
        {
            IQuadrilateral quadrilateral = CreateTestQuadrilateral();
            quadrilateral.Vertex2 = new Vector3d(10, 0, 0);
            quadrilateral.Vertex3 = new Vector3d(10, 10, 0);
            quadrilateral.Vertex4 = new Vector3d(0, 10, 0);
            Assert.AreEqual(new Vector3d(0, 0, 1), quadrilateral.Normal);

            Utils.DisposalAccessTest(quadrilateral, delegate() { Vector3d normal = quadrilateral.Normal; });
        }

        #endregion Properties
    }
}
