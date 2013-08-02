#region License

//
// ITriangleTest.cs
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
    public abstract class ITriangleTest : IGraphicTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(ITriangle); } }

        protected sealed override IGraphic CreateTestGraphic()
        {
            return CreateTestTriangle();
        }

        protected sealed override IGraphic CreateTestGraphicWithCoordinates()
        {
            return CreateTestTriangle();
        }

        protected sealed override IGraphic CreateTestGraphicWithColour()
        {
            return CreateTestTriangle();
        }

        protected sealed override IGraphic CreateTestGraphicWithNoColour()
        {
            return null;
        }

        protected abstract ITriangle CreateTestTriangle();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            ITriangle triangle = CreateTestTriangle();
            Assert.AreEqual(DOMObjectType.Triangle, triangle.ObjectType);
            Assert.IsFalse(triangle.IsStateElement);
            Assert.IsFalse(triangle.IsTopLevelElement);
            Assert.AreEqual(Palette.MainColour, triangle.OverrideableColourValue);
            Assert.IsTrue(triangle.ColourValueEnabled);
            Assert.AreEqual(3U, triangle.CoordinatesCount);
            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public sealed override void IsDuplicateOfTest()
        {
            // identical elements
            ITriangle triangle  = CreateTestTriangle();
            triangle.Vertex2    = new Vector3d(1, 1, 1);
            triangle.Vertex3    = new Vector3d(2, 2, 2);
            ITriangle triangle2 = (ITriangle)triangle.Clone();
            Assert.IsTrue(triangle.IsDuplicateOf(triangle2));
            Assert.IsTrue(triangle2.IsDuplicateOf(triangle));

            // different ColourValue
            triangle2.ColourValue = Palette.EdgeColour;
            Assert.IsFalse(triangle.IsDuplicateOf(triangle2));
            Assert.IsFalse(triangle2.IsDuplicateOf(triangle));

            // different vertices
            triangle2.ColourValue = triangle.ColourValue;
            triangle2.Vertex1     = new Vector3d(1, 2, 3);
            triangle2.Vertex2     = new Vector3d(4, 5, 6);
            triangle2.Vertex2     = new Vector3d(7, 8, 9);
            Assert.IsFalse(triangle.IsDuplicateOf(triangle2));
            Assert.IsFalse(triangle2.IsDuplicateOf(triangle));

            // same vertices, but in a different order
            triangle2.Vertex1 = triangle.Vertex2;
            triangle2.Vertex2 = triangle.Vertex3;
            triangle2.Vertex3 = triangle.Vertex1;
            Assert.IsTrue(triangle.IsDuplicateOf(triangle2));
            Assert.IsTrue(triangle2.IsDuplicateOf(triangle));

            // other properties should be ignored
            triangle.IsLocked = !triangle2.IsLocked;
            Assert.IsTrue(triangle.IsDuplicateOf(triangle2));
            Assert.IsTrue(triangle2.IsDuplicateOf(triangle));

            // a triangle is not a duplicate of itself
            Assert.IsFalse(triangle.IsDuplicateOf(triangle));

            Utils.DisposalAccessTest(triangle, delegate() { bool isDuplicate = triangle.IsDuplicateOf(triangle2); });
        }

        #endregion Analytics

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            ITriangle triangle;
            StringBuilder code;

            triangle = CreateTestTriangle();
            triangle.Vertex1 = new Vector3d(1, 2, 3);
            triangle.Vertex2 = new Vector3d(4, 5, 6);
            triangle.Vertex3 = new Vector3d(7, 8, 9);

            // 1. triangle with an overrideable colour
            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                Matrix4d transform = Matrix4d.Scale(2, 3, 4);

                // normal winding
                code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("3 16 1 2 3 4 5 6 7 8 9\r\n", code.ToString());

                // reverse winding
                code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Reversed));
                Assert.AreEqual("3 16 7 8 9 4 5 6 1 2 3\r\n", code.ToString());

                // transform
                code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref transform, WindingDirection.Normal));
                Assert.AreEqual("3 16 2 6 12 8 15 24 14 24 36\r\n", code.ToString());

                // override the colour with an index
                code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), codeFormat, 10, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("3 10 1 2 3 4 5 6 7 8 9\r\n", code.ToString());

                // override the colour with an opaque direct colour
                code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), codeFormat, 0x2FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("3 #2FF00FF 1 2 3 4 5 6 7 8 9\r\n", code.ToString());

                // override the colour with a transparent direct colour
                code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), codeFormat, 0x3FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("3 #3FF00FF 1 2 3 4 5 6 7 8 9\r\n", code.ToString());

                // override the colour with EdgeColour - should have no effect, as this is not a legal colour for a triangle
                code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), codeFormat, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("3 16 1 2 3 4 5 6 7 8 9\r\n", code.ToString());
            }

            // 2. triangle with a fixed colour
            triangle.ColourValue = 1U;

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                // overriding the colour will have no effect
                code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), codeFormat, 10, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("3 1 1 2 3 4 5 6 7 8 9\r\n", code.ToString());
            }

            // 3. triangle with a fixed colour from a local-palette, which should be converted to a direct-colour in PartsLibrary mode
            IColour localColour  = MocksFactory.CreateMockColour();
            localColour.Code     = 100U;
            localColour.Value    = Color.Red;
            localColour.EdgeCode = 0x2000000;

            IPage page = MocksFactory.CreateMockPage();
            IStep step = MocksFactory.CreateMockStep();
            page.Add(step);
            step.Add(localColour);

            triangle.ColourValue = 100U;
            step.Add(triangle);

            code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("3 100 1 2 3 4 5 6 7 8 9\r\n", code.ToString());
            code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("3 #2FF0000 1 2 3 4 5 6 7 8 9\r\n", code.ToString());
            code = Utils.PreProcessCode(triangle.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("3 100 1 2 3 4 5 6 7 8 9\r\n", code.ToString());

            // 4. attributes test
            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Geometry

        [TestMethod]
        public sealed override void OriginTest()
        {
            ITriangle triangle = CreateTestTriangle();
            triangle.Vertex1 = new Vector3d(1, 0, 0);
            triangle.Vertex2 = new Vector3d(0, 1, 0);
            triangle.Vertex3 = new Vector3d(0, 0, 1);
            Assert.AreEqual(triangle.Vertex1, triangle.Origin);

            Utils.DisposalAccessTest(triangle, delegate() { Vector3d origin = triangle.Origin; });

        }

        [TestMethod]
        public sealed override void ReverseWindingTest()
        {
            ITriangle triangle = CreateTestTriangle();
            triangle.Vertex1 = new Vector3d(1, 0, 0);
            triangle.Vertex2 = new Vector3d(0, 1, 0);
            triangle.Vertex3 = new Vector3d(0, 0, 1);
            triangle.ReverseWinding();
            Assert.AreEqual(new Vector3d(0, 0, 1), triangle.Vertex1);
            Assert.AreEqual(new Vector3d(0, 1, 0), triangle.Vertex2);
            Assert.AreEqual(new Vector3d(1, 0, 0), triangle.Vertex3);

            Utils.DisposalAccessTest(triangle, delegate() { triangle.ReverseWinding(); });
        }

        #endregion Geometry

        #region Properties

        [TestMethod]
        public void Vertex1Test()
        {
            ITriangle triangle    = CreateTestTriangle();
            Vector3d defaultValue = Vector3d.Zero;
            Vector3d newValue     = Vector3d.UnitX;

            PropertyValueTest(triangle,
                              defaultValue,
                              newValue,
                              delegate(ITriangle obj) { return obj.Vertex1; },
                              delegate(ITriangle obj, Vector3d value) { obj.Vertex1 = value; },
                              PropertyValueFlags.None);

            // Vertex1==Coordinates[0]
            triangle = CreateTestTriangle();

            if (!triangle.IsImmutable)
                triangle.Vertex1 = Vector3d.UnitX;

            Assert.AreEqual(triangle.Vertex1, triangle.Coordinates.ElementAt(0));
        }

        [TestMethod]
        public void Vertex1ChangedTest()
        {
            ITriangle triangle             = CreateTestTriangle();
            IEnumerable<Vector3d> oldValue = triangle.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.UnitX, Vector3d.Zero, Vector3d.Zero };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!triangle.IsImmutable)
            {
                triangle.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(triangle, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                };

                triangle.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(triangle, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                };

                triangle.Vertex1 = newValue.ElementAt(0);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen = false;
                genericEventSeen = false;
                triangle.Vertex1 = newValue.ElementAt(0);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void Vertex2Test()
        {
            ITriangle triangle    = CreateTestTriangle();
            Vector3d defaultValue = Vector3d.Zero;
            Vector3d newValue     = Vector3d.UnitX;

            PropertyValueTest(triangle,
                              defaultValue,
                              newValue,
                              delegate(ITriangle obj) { return obj.Vertex2; },
                              delegate(ITriangle obj, Vector3d value) { obj.Vertex2 = value; },
                              PropertyValueFlags.None);

            // Vertex2==Coordinates[1]
            triangle = CreateTestTriangle();

            if (!triangle.IsImmutable)
                triangle.Vertex2 = Vector3d.UnitX;

            Assert.AreEqual(triangle.Vertex2, triangle.Coordinates.ElementAt(1));
        }

        [TestMethod]
        public void Vertex2ChangedTest()
        {
            ITriangle triangle             = CreateTestTriangle();
            IEnumerable<Vector3d> oldValue = triangle.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.Zero, Vector3d.UnitX, Vector3d.Zero };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!triangle.IsImmutable)
            {
                triangle.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(triangle, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                };

                triangle.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(triangle, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                };

                triangle.Vertex2 = newValue.ElementAt(1);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen = false;
                genericEventSeen = false;
                triangle.Vertex2 = newValue.ElementAt(1);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void Vertex3Test()
        {
            ITriangle triangle    = CreateTestTriangle();
            Vector3d defaultValue = Vector3d.Zero;
            Vector3d newValue     = Vector3d.UnitX;

            PropertyValueTest(triangle,
                              defaultValue,
                              newValue,
                              delegate(ITriangle obj) { return obj.Vertex3; },
                              delegate(ITriangle obj, Vector3d value) { obj.Vertex3 = value; },
                              PropertyValueFlags.None);

            // Vertex3==Coordinates[2]
            triangle = CreateTestTriangle();

            if (!triangle.IsImmutable)
                triangle.Vertex3 = Vector3d.UnitX;

            Assert.AreEqual(triangle.Vertex3, triangle.Coordinates.ElementAt(2));
        }

        [TestMethod]
        public void Vertex3ChangedTest()
        {
            ITriangle triangle             = CreateTestTriangle();
            IEnumerable<Vector3d> oldValue = triangle.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.Zero, Vector3d.Zero, Vector3d.UnitX };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!triangle.IsImmutable)
            {
                triangle.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(triangle, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                };

                triangle.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(triangle, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                };

                triangle.Vertex3 = newValue.ElementAt(2);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen        = false;
                genericEventSeen = false;
                triangle.Vertex3 = newValue.ElementAt(2);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void NormalTest()
        {
            ITriangle triangle = CreateTestTriangle();
            triangle.Vertex2 = new Vector3d(2, 0, 0);
            triangle.Vertex3 = new Vector3d(1, 1, 0);
            Assert.AreEqual(new Vector3d(0, 0, 1), triangle.Normal);

            Utils.DisposalAccessTest(triangle, delegate() { Vector3d normal = triangle.Normal; });
        }

        #endregion Properties
    }
}
