#region License

//
// GraphicTest.cs
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
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public sealed class GraphicTest : IGraphicTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(Graphic); } }

        protected override IGraphic CreateTestGraphic()
        {
            return new MockGraphic();
        }

        protected override IGraphic CreateTestGraphicWithCoordinates()
        {
            IGraphic graphic = new MockGraphic();
            graphic.Coordinates = GenerateCoordinates(graphic);
            return graphic;
        }

        protected override IGraphic CreateTestGraphicWithColour()
        {
            return new MockGraphic();
        }

        protected override IGraphic CreateTestGraphicWithNoColour()
        {
            return new MockGraphic(true);
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

        #region Analytics

        [TestMethod]
        public override void HasProblemsTest()
        {
            IGraphic graphic = CreateTestGraphic();

            // colocated vertices
            graphic.Coordinates = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitX, Vector3d.UnitZ };
            Assert.IsTrue(graphic.HasProblems(CodeStandards.Full));
            Assert.IsTrue(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            Assert.IsTrue(graphic.HasProblems(CodeStandards.PartsLibrary));

            graphic.Coordinates = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            Assert.IsFalse(graphic.HasProblems(CodeStandards.Full));
            Assert.IsFalse(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            Assert.IsFalse(graphic.HasProblems(CodeStandards.PartsLibrary));

            // invalid colour-value: index out-of-range
            graphic.ColourValue = 1024U;
            Assert.IsTrue(graphic.HasProblems(CodeStandards.Full));
            Assert.IsTrue(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            Assert.IsTrue(graphic.HasProblems(CodeStandards.PartsLibrary));

            // invalid colour-value: transparent Direct Colour
            graphic.ColourValue = LDColour.DirectColourTransparent | 0xFFFFFF;
            Assert.IsTrue(graphic.HasProblems(CodeStandards.Full));
            Assert.IsTrue(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            Assert.IsTrue(graphic.HasProblems(CodeStandards.PartsLibrary));

            graphic.ColourValue = Palette.MainColour;
            Assert.IsFalse(graphic.HasProblems(CodeStandards.Full));
            Assert.IsFalse(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            Assert.IsFalse(graphic.HasProblems(CodeStandards.PartsLibrary));
        }

        [TestMethod]
        public void AnalyticsTest_IsColourValid()
        {
            Graphic graphic = new MockGraphic();
            ICollection<IProblemDescriptor> problems;

            graphic.Coordinates = GenerateCoordinates(graphic);

            // valid colour: system-palette index
            graphic.ColourValue = Palette.MainColour;
            Assert.IsFalse(graphic.IsColourInvalid);
            Assert.IsFalse(graphic.IsColocated);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.Full));
            problems = graphic.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.PartsLibrary));
            problems = graphic.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            problems = graphic.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);

            // valid colour: local-palette index
            IColour colour = MocksFactory.CreateMockColour();
            colour.Code = 2000;
            IElementCollection collection = MocksFactory.CreateMockElementCollection();
            collection.Add(colour);
            collection.Add(graphic);
            graphic.ColourValue = colour.Code;
            Assert.IsFalse(graphic.IsColourInvalid);
            Assert.IsFalse(graphic.IsColocated);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.Full));
            problems = graphic.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.PartsLibrary));
            problems = graphic.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            problems = graphic.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);

            // valid colour: opaque Direct Colour
            graphic.ColourValue = 0x2FF0000;
            Assert.IsFalse(graphic.IsColourInvalid);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.Full));
            problems = graphic.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.PartsLibrary));
            problems = graphic.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            problems = graphic.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);

            // semi-valid colour: transparent Direct Colour - not allowed in OMR or PartsLibrary mode
            graphic.ColourValue = 0x3FF0000;
            Assert.IsTrue(graphic.IsColourInvalid);
            Assert.IsTrue(graphic.HasProblems(CodeStandards.Full));
            problems = graphic.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            CheckInvalidColourProblem(problems.First(), graphic, CodeStandards.Full);
            Assert.IsTrue(graphic.HasProblems(CodeStandards.PartsLibrary));
            problems = graphic.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            CheckInvalidColourProblem(problems.First(), graphic, CodeStandards.PartsLibrary);
            Assert.IsTrue(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            problems = graphic.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            CheckInvalidColourProblem(problems.First(), graphic, CodeStandards.OfficialModelRepository);

            // invalid colour: out-of-range index - not allowed in OMR or PartsLibrary mode
            graphic.ColourValue = 1024U;
            Assert.IsTrue(graphic.IsColourInvalid);
            Assert.IsTrue(graphic.HasProblems(CodeStandards.Full));
            problems = graphic.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            CheckInvalidColourProblem(problems.First(), graphic, CodeStandards.Full);
            Assert.IsTrue(graphic.HasProblems(CodeStandards.PartsLibrary));
            problems = graphic.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            CheckInvalidColourProblem(problems.First(), graphic, CodeStandards.PartsLibrary);
            Assert.IsTrue(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            problems = graphic.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            CheckInvalidColourProblem(problems.First(), graphic, CodeStandards.OfficialModelRepository);
        }

        private void CheckInvalidColourProblem(IProblemDescriptor problem, IGraphic graphic, CodeStandards codeFormat)
        {
            Assert.AreSame(graphic, problem.Element);
            Assert.AreEqual(Graphic.Problem_ColourInvalid, problem.Guid);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            switch (codeFormat)
            {
                case CodeStandards.Full:
                    Assert.AreEqual(Severity.Information, problem.Severity);
                    break;

                case CodeStandards.OfficialModelRepository:
                    Assert.AreEqual(Severity.Error, problem.Severity);
                    break;

                case CodeStandards.PartsLibrary:
                    Assert.AreEqual(Severity.Error, problem.Severity);
                    break;
            }
        }

        [TestMethod]
        public void AnalyticsTest_IsColocated()
        {
            Graphic graphic = new MockGraphic();
            ICollection<IProblemDescriptor> problems;

            // all coordinates are unique
            graphic.Coordinates = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            Assert.IsFalse(graphic.IsColocated);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.Full));
            problems = graphic.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.PartsLibrary));
            problems = graphic.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);
            Assert.IsFalse(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            problems = graphic.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);

            // two coordinates co-located, but Graphic does not return IProblemDescriptors for this condition
            graphic.Coordinates = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitX, Vector3d.UnitZ };
            Assert.IsTrue(graphic.IsColocated);
            Assert.IsTrue(graphic.HasProblems(CodeStandards.Full));
            problems = graphic.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);
            Assert.IsTrue(graphic.HasProblems(CodeStandards.PartsLibrary));
            problems = graphic.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);
            Assert.IsTrue(graphic.HasProblems(CodeStandards.OfficialModelRepository));
            problems = graphic.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);
        }

        [TestMethod]
        public override void IsDuplicateOfTest()
        {
            IGraphic first  = new MockGraphic();
            IGraphic second = new MockGraphic();

            // different colour-values
            first.ColourValue  = 1U;
            second.ColourValue = 2U;
            Assert.IsFalse(first.IsDuplicateOf(second));
            Assert.IsFalse(second.IsDuplicateOf(first));
            second.ColourValue = 1U;

            // different coordinates
            first.Coordinates = GenerateCoordinates(first);
            second.Coordinates = GenerateCoordinates(second);
            Assert.IsFalse(first.IsDuplicateOf(second));
            Assert.IsFalse(second.IsDuplicateOf(first));

            // identical colours and coordinates
            second.Coordinates = first.Coordinates;
            Assert.IsTrue(first.IsDuplicateOf(second));
            Assert.IsTrue(second.IsDuplicateOf(first));

            // same coordinates in different order
            Random rnd = new Random();
            second.Coordinates = first.Coordinates.OrderBy<Vector3d, int>(item => rnd.Next());
            Assert.IsTrue(first.IsDuplicateOf(second));
            Assert.IsTrue(second.IsDuplicateOf(first));
        }

        [TestMethod]
        public void ColocatedTest()
        {
            Graphic graphic = new MockGraphic();

            graphic.Coordinates = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitX, Vector3d.UnitZ };
            uint[] expected = new uint[] { 0, 1 };
            uint[] actual = graphic.ColocatedCoordinates;
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(expected[0], actual[0]);
            Assert.AreEqual(expected[1], actual[1]);

            graphic.Coordinates = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            expected = new uint[0];
            actual = graphic.ColocatedCoordinates;
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void CheckVertexIsUniqueTest()
        {
            Graphic_Accessor graphic = new Graphic_Accessor(new PrivateObject(new MockGraphic(), new PrivateType(typeof(Graphic))));

            graphic.Coordinates = new Vector3d[] { new Vector3d(), new Vector3d(), new Vector3d() };

            for (uint idx = 0; idx < graphic.CoordinatesCount; idx++)
            {
                Assert.IsFalse(graphic.CheckVertexIsUnique(idx));
            }

            graphic.Coordinates = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };

            for (uint idx = 0; idx < graphic.CoordinatesCount; idx++)
            {
                Assert.IsTrue(graphic.CheckVertexIsUnique(idx));
            }
        }

        #endregion Analytics

        #region Attributes

        [TestMethod]
        public void OnIsVisibleChangedTest()
        {
            Graphic_Accessor graphic = new Graphic_Accessor(new PrivateObject(new MockGraphic()));
            bool eventSeen           = false;

            graphic.add_IsVisibleChanged(delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            graphic.OnIsVisibleChanged(new PropertyChangedEventArgs<bool>(true, false));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            graphic.Dispose();

            try
            {
                graphic.OnIsVisibleChanged(new PropertyChangedEventArgs<bool>(true, false));
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

        [TestMethod]
        public void OnIsGhostedChangedTest()
        {
            Graphic_Accessor graphic = new Graphic_Accessor(new PrivateObject(new MockGraphic()));
            bool eventSeen           = false;

            graphic.add_IsGhostedChanged(delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            graphic.OnIsGhostedChanged(new PropertyChangedEventArgs<bool>(false, true));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            graphic.Dispose();

            try
            {
                graphic.OnIsGhostedChanged(new PropertyChangedEventArgs<bool>(false, true));
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

        #endregion Attributes

        #region Colour

        [TestMethod]
        public void OnColourValueChangedTest()
        {
            Graphic_Accessor graphic = new Graphic_Accessor(new PrivateObject(new MockGraphic()));
            bool eventSeen = false;

            graphic.add_ColourValueChanged(delegate(object sender, PropertyChangedEventArgs<uint> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            graphic.OnColourValueChanged(new PropertyChangedEventArgs<uint>(0U, 1U));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            graphic.Dispose();

            try
            {
                graphic.OnColourValueChanged(new PropertyChangedEventArgs<uint>(0U, 1U));
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

        [TestMethod]
        public void SetColourValueTest()
        {
            Graphic_Accessor graphic = new Graphic_Accessor(new PrivateObject(new MockGraphic()));
            string code              = "1";

            // 1: base-10
            graphic.SetColourValue(code);
            Assert.AreEqual(code, graphic.ColourValue.ToString());

            // 2: base-16
            code = "0x2FF00FF";
            graphic.SetColourValue(code);
            Assert.AreEqual(code, "0x" + graphic.ColourValue.ToString("X7"));
            code = "0X2FF00FF";
            graphic.SetColourValue(code);
            Assert.AreEqual(code, "0X" + graphic.ColourValue.ToString("X7"));

            // 3: base-16
            code = "#2FF00FF";
            graphic.SetColourValue(code);
            Assert.AreEqual(code, "#" + graphic.ColourValue.ToString("X7"));

            // 4: invalid
            try
            {
                code = "2FF00FF";
                graphic.SetColourValue(code);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }
        }

        #endregion Colour

        #region Constructor

        [TestMethod]
        public void GraphicConstructorTest()
        {
            Graphic graphic = new MockGraphic();

            Assert.AreEqual(graphic.OverrideableColourValue, graphic.ColourValue);

            foreach (Vector3d coordinate in graphic.Coordinates)
            {
                Assert.AreEqual(Vector3d.Zero, coordinate);
            }
        }

        [TestMethod]
        public void GraphicConstructorTest1()
        {
            const uint colourValue = 1U;
            Vector3d[] coordinates = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            Graphic graphic = new MockGraphic(colourValue, coordinates);
            int i = 0;

            Assert.AreEqual(colourValue, graphic.ColourValue);

            foreach (Vector3d coordinate in graphic.Coordinates)
            {
                Assert.AreEqual(coordinates[i++], coordinate);
            }

            coordinates = new Vector3d[1];

            try
            {
                graphic = new MockGraphic(colourValue, coordinates);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentException), e.GetType());
            }
        }

        [TestMethod]
        public void GraphicConstructorTest2()
        {
            const uint colourValue = 1U;
            Graphic graphic = new MockGraphic(colourValue);

            Assert.AreEqual(colourValue, graphic.ColourValue);

            foreach (Vector3d coordinate in graphic.Coordinates)
            {
                Assert.AreEqual(Vector3d.Zero, coordinate);
            }
        }

        #endregion Constructor

        #region Coordinates

        [TestMethod]
        public void OnCoordinatesChangedTest()
        {
            Graphic_Accessor graphic = new Graphic_Accessor(new PrivateObject(new MockGraphic()));
            bool eventSeen = false;

            graphic.add_CoordinatesChanged(delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
            });

            graphic.OnCoordinatesChanged(new PropertyChangedEventArgs<IEnumerable<Vector3d>>(null, null));
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            graphic.Dispose();

            try
            {
                graphic.OnCoordinatesChanged(new PropertyChangedEventArgs<IEnumerable<Vector3d>>(null, null));
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

        [TestMethod]
        public void CoordinatesArrayTest()
        {
            Graphic_Accessor graphic = new Graphic_Accessor(new PrivateObject(new MockGraphic()));
            Vector3d[] expected      = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            Vector3d[] actual;

            graphic.Coordinates = expected;
            actual             = graphic.CoordinatesArray;

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        #endregion Coordinates
    }
}
