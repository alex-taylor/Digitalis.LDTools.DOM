#region License

//
// ILineTest.cs
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
    public abstract class ILineTest : IGraphicTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(ILine); } }

        protected sealed override IGraphic CreateTestGraphic()
        {
            return CreateTestLine();
        }

        protected sealed override IGraphic CreateTestGraphicWithCoordinates()
        {
            return CreateTestLine();
        }

        protected sealed override IGraphic CreateTestGraphicWithColour()
        {
            return CreateTestLine();
        }

        protected sealed override IGraphic CreateTestGraphicWithNoColour()
        {
            return null;
        }

        protected abstract ILine CreateTestLine();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            ILine line = CreateTestLine();
            Assert.AreEqual(DOMObjectType.Line, line.ObjectType);
            Assert.IsFalse(line.IsStateElement);
            Assert.IsFalse(line.IsTopLevelElement);
            Assert.AreEqual(2U, line.CoordinatesCount);
            Assert.AreEqual(Palette.EdgeColour, line.OverrideableColourValue);
            Assert.IsTrue(line.ColourValueEnabled);
            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public sealed override void IsDuplicateOfTest()
        {
            // identical elements
            ILine line   = CreateTestLine();
            line.Vertex2 = new Vector3d(1, 1, 1);
            ILine line2  = (ILine)line.Clone();
            Assert.IsTrue(line.IsDuplicateOf(line2));
            Assert.IsTrue(line2.IsDuplicateOf(line));

            // different ColourValue
            line2.ColourValue = Palette.MainColour;
            Assert.IsFalse(line.IsDuplicateOf(line2));
            Assert.IsFalse(line2.IsDuplicateOf(line));

            // different vertices
            line2.ColourValue = line.ColourValue;
            line2.Vertex1     = new Vector3d(1, 2, 3);
            line2.Vertex2     = new Vector3d(4, 5, 6);
            Assert.IsFalse(line.IsDuplicateOf(line2));
            Assert.IsFalse(line2.IsDuplicateOf(line));

            // same vertices, but in the opposite order
            line2.Vertex1 = line.Vertex2;
            line2.Vertex2 = line.Vertex1;
            Assert.IsTrue(line.IsDuplicateOf(line2));
            Assert.IsTrue(line2.IsDuplicateOf(line));

            // other properties should be ignored
            line.IsLocked = !line2.IsLocked;
            Assert.IsTrue(line.IsDuplicateOf(line2));
            Assert.IsTrue(line2.IsDuplicateOf(line));

            // a line is not a duplicate of itself
            Assert.IsFalse(line.IsDuplicateOf(line));

            Utils.DisposalAccessTest(line, delegate() { bool isDuplicate = line.IsDuplicateOf(line2); });
        }

        #endregion Analytics

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            ILine line = CreateTestLine();
            StringBuilder code;

            line.Vertex1 = new Vector3d(1, 2, 3);
            line.Vertex2 = new Vector3d(4, 5, 6);

            // 1. line with an overrideable colour
            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                Matrix4d transform = Matrix4d.Scale(2, 3, 4);

                // normal winding
                code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), codeFormat, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("2 24 1 2 3 4 5 6\r\n", code.ToString());

                // reverse winding
                code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), codeFormat, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Reversed));
                Assert.AreEqual("2 24 4 5 6 1 2 3\r\n", code.ToString());

                // transform
                code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), codeFormat, Palette.EdgeColour, ref transform, WindingDirection.Normal));
                Assert.AreEqual("2 24 2 6 12 8 15 24\r\n", code.ToString());

                // override the colour with an index
                code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), codeFormat, 10, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("2 10 1 2 3 4 5 6\r\n", code.ToString());

                // override the colour with an opaque direct colour
                code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), codeFormat, 0x2FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("2 #2FF00FF 1 2 3 4 5 6\r\n", code.ToString());

                // override the colour with a transparent direct colour
                code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), codeFormat, 0x3FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("2 #3FF00FF 1 2 3 4 5 6\r\n", code.ToString());

                // override the colour with MainColour
                code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("2 16 1 2 3 4 5 6\r\n", code.ToString());
            }

            // 2. line with a fixed colour from the system-palette
            line.ColourValue = 1U;

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                // overriding the colour will have no effect
                code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), codeFormat, 10, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("2 1 1 2 3 4 5 6\r\n", code.ToString());
            }

            // 3. line with a fixed colour from a local-palette, which should be converted to a direct-colour in PartsLibrary mode
            IColour localColour  = MocksFactory.CreateMockColour();
            localColour.Code     = 100U;
            localColour.Value    = Color.Red;
            localColour.EdgeCode = 0x2000000;

            IPage page = MocksFactory.CreateMockPage();
            IStep step = MocksFactory.CreateMockStep();
            page.Add(step);
            step.Add(localColour);

            line.ColourValue = 100U;
            step.Add(line);

            code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), CodeStandards.Full, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("2 100 1 2 3 4 5 6\r\n", code.ToString());
            code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("2 #2FF0000 1 2 3 4 5 6\r\n", code.ToString());
            code = Utils.PreProcessCode(line.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("2 100 1 2 3 4 5 6\r\n", code.ToString());

            // 4. attributes test
            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Geometry

        [TestMethod]
        public sealed override void OriginTest()
        {
            ILine line = CreateTestLine();
            line.Vertex1 = new Vector3d(1, 0, 0);
            line.Vertex2 = new Vector3d(0, 1, 0);
            Assert.AreEqual(line.Vertex1, line.Origin);

            Utils.DisposalAccessTest(line, delegate() { Vector3d origin = line.Origin; });
        }

        [TestMethod]
        public sealed override void ReverseWindingTest()
        {
            ILine line = CreateTestLine();
            line.Vertex1 = new Vector3d(1, 0, 0);
            line.Vertex2 = new Vector3d(2, 0, 0);
            line.ReverseWinding();
            Assert.AreEqual(new Vector3d(2, 0, 0), line.Vertex1);
            Assert.AreEqual(new Vector3d(1, 0, 0), line.Vertex2);

            Utils.DisposalAccessTest(line, delegate() { line.ReverseWinding(); });
        }

        #endregion Geometry

        #region Properties

        [TestMethod]
        public void Vertex1Test()
        {
            ILine line            = CreateTestLine();
            Vector3d defaultValue = Vector3d.Zero;
            Vector3d newValue     = Vector3d.UnitX;

            PropertyValueTest(line,
                              defaultValue,
                              newValue,
                              delegate(ILine obj) { return obj.Vertex1; },
                              delegate(ILine obj, Vector3d value) { obj.Vertex1 = value; },
                              PropertyValueFlags.None);

            // Vertex1==Coordinates[0]
            line = CreateTestLine();

            if (!line.IsImmutable)
                line.Vertex1 = Vector3d.UnitX;

            Assert.AreEqual(line.Vertex1, line.Coordinates.ElementAt(0));
        }

        [TestMethod]
        public void Vertex1ChangedTest()
        {
            ILine line = CreateTestLine();

            if (!line.IsImmutable)
            {
                IEnumerable<Vector3d> oldValue = line.Coordinates;
                IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.UnitX, Vector3d.Zero };
                bool eventSeen                 = false;
                bool genericEventSeen          = false;

                line.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(line, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                };

                line.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(line, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                };

                line.Vertex1 = newValue.ElementAt(0);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen        = false;
                genericEventSeen = false;
                line.Vertex1     = newValue.ElementAt(0);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void Vertex2Test()
        {
            ILine line            = CreateTestLine();
            Vector3d defaultValue = Vector3d.Zero;
            Vector3d newValue     = Vector3d.UnitX;

            PropertyValueTest(line,
                              defaultValue,
                              newValue,
                              delegate(ILine obj) { return obj.Vertex2; },
                              delegate(ILine obj, Vector3d value) { obj.Vertex2 = value; },
                              PropertyValueFlags.None);

            // Vertex2==Coordinates[1]
            line = CreateTestLine();

            if (!line.IsImmutable)
                line.Vertex2 = Vector3d.UnitX;

            Assert.AreEqual(line.Vertex2, line.Coordinates.ElementAt(1));
        }

        [TestMethod]
        public void Vertex2ChangedTest()
        {
            ILine line = CreateTestLine();

            if (!line.IsImmutable)
            {
                IEnumerable<Vector3d> oldValue = line.Coordinates;
                IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.Zero, Vector3d.UnitX };
                bool eventSeen                 = false;
                bool genericEventSeen          = false;

                line.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(line, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                };

                line.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(line, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                };

                line.Vertex2 = newValue.ElementAt(1);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen        = false;
                genericEventSeen = false;
                line.Vertex2     = newValue.ElementAt(1);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        #endregion Properties
    }
}
