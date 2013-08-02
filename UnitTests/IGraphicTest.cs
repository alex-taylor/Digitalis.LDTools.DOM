#region License

//
// IGraphicTest.cs
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
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IGraphicTest : IGroupableTest
    {
        #region Infrastructure

        protected override Type InterfaceType { get { return typeof(IGraphic); } }

        protected sealed override IGroupable CreateTestGroupable()
        {
            IGraphic graphic = CreateTestGraphicWithCoordinates();

            if (null == graphic)
                graphic = CreateTestGraphic();

            return graphic;
        }

        protected abstract IGraphic CreateTestGraphic();

        protected abstract IGraphic CreateTestGraphicWithCoordinates();

        protected abstract IGraphic CreateTestGraphicWithColour();

        protected abstract IGraphic CreateTestGraphicWithNoColour();

        private static Random random = new Random(DateTime.Now.Millisecond);

        protected static Vector3d[] GenerateCoordinates(IGraphic graphic)
        {
            Vector3d[] coordinates = new Vector3d[graphic.CoordinatesCount];
            double x = 1.0;
            double y = 2.0;
            double z = 3.0;

            for (int i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = new Vector3d(x, y, z);
                x *= (double)random.Next();
                y *= (double)random.Next();
                z *= (double)random.Next();
            }

            return coordinates;
        }

        #endregion Infrastructure

        #region Analytics

        [TestMethod]
        public abstract void IsDuplicateOfTest();

        #endregion Analytics

        #region Attributes

        [TestMethod]
        public virtual void IsVisibleTest()
        {
            IGraphic graphic  = CreateTestGraphic();
            bool defaultValue = true;
            bool newValue     = false;

            PropertyValueTest(graphic,
                              defaultValue,
                              newValue,
                              delegate(IGraphic obj) { return obj.IsVisible; },
                              delegate(IGraphic obj, bool value) { obj.IsVisible = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public virtual void IsVisibleChangedTest()
        {
            IGraphic graphic = CreateTestGraphic();
            bool valueToSet  = false;

            PropertyChangedTest(graphic,
                                "IsVisibleChanged",
                                valueToSet,
                                delegate(IGraphic obj, PropertyChangedEventHandler<bool> handler) { obj.IsVisibleChanged += handler; },
                                delegate(IGraphic obj) { return obj.IsVisible; },
                                delegate(IGraphic obj, bool value) { obj.IsVisible = value; });
        }

        [TestMethod]
        public virtual void IsGhostedTest()
        {
            IGraphic graphic  = CreateTestGraphic();
            bool defaultValue = false;
            bool newValue     = true;

            PropertyValueTest(graphic,
                              defaultValue,
                              newValue,
                              delegate(IGraphic obj) { return obj.IsGhosted; },
                              delegate(IGraphic obj, bool value) { obj.IsGhosted = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public virtual void IsGhostedChangedTest()
        {
            IGraphic graphic = CreateTestGraphic();
            bool valueToSet  = true;

            PropertyChangedTest(graphic,
                                "IsGhostedChanged",
                                valueToSet,
                                delegate(IGraphic obj, PropertyChangedEventHandler<bool> handler) { obj.IsGhostedChanged += handler; },
                                delegate(IGraphic obj) { return obj.IsGhosted; },
                                delegate(IGraphic obj, bool value) { obj.IsGhosted = value; });
        }

        #endregion Attributes

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            IGraphic graphic = CreateTestGraphicWithCoordinates();

            if (null == graphic)
                graphic = CreateTestGraphicWithColour();

            if (null == graphic)
                graphic = CreateTestGraphic();

            return graphic;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IGraphic first  = (IGraphic)original;
            IGraphic second = (IGraphic)copy;

            // properties should be preserved
            Assert.AreEqual(first.IsVisible, second.IsVisible);
            Assert.AreEqual(first.IsGhosted, second.IsGhosted);
            Assert.AreEqual(first.ColourValueEnabled, second.ColourValueEnabled);

            if (first.ColourValueEnabled)
                Assert.AreEqual(first.ColourValue, second.ColourValue);

            Assert.AreEqual(first.CoordinatesCount, second.CoordinatesCount);

            IEnumerable<Vector3d> graphicCoords = first.Coordinates;
            IEnumerable<Vector3d> cloneCoords   = second.Coordinates;

            Assert.AreEqual(first.CoordinatesCount, (uint)graphicCoords.Count());
            Assert.AreEqual(second.CoordinatesCount, (uint)cloneCoords.Count());

            for (int i = 0; i < graphicCoords.Count(); i++)
            {
                Assert.AreEqual(graphicCoords.ElementAt(i), cloneCoords.ElementAt(i));
            }

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            IGraphic graphic = CreateTestGraphic();

            // these two don't implement the attributes directly, so we do not need to test for them here
            if (DOMObjectType.Texmap != graphic.ObjectType && DOMObjectType.CompositeElement != graphic.ObjectType)
            {
                StringBuilder code;
                string attributesCode = "";

                if (graphic.IsImmutable)
                {
                    if (graphic.IsGhosted)
                        attributesCode += "0 GHOST ";

                    if (!graphic.IsVisible)
                        attributesCode += "0 MLCAD HIDE ";

                    // attributes are not allowed in PartsLibrary mode
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsTrue(code.ToString().StartsWith(attributesCode), code.ToString());
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsFalse(code.ToString().StartsWith(attributesCode), code.ToString());
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.IsTrue(code.ToString().StartsWith(attributesCode), code.ToString());
                }
                else
                {
                    string cleanCode;

                    // attributes are not allowed in PartsLibrary mode, and if either is specified then no code should be generated
                    attributesCode = "0 GHOST 0 MLCAD HIDE ";

                    cleanCode = graphic.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                    graphic.IsVisible = false;
                    graphic.IsGhosted = true;
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(attributesCode + cleanCode, code.ToString());
                    graphic.IsVisible = true;
                    graphic.IsGhosted = false;

                    cleanCode = graphic.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                    graphic.IsVisible = false;
                    graphic.IsGhosted = true;
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(String.Empty, code.ToString());
                    graphic.IsVisible = true;
                    graphic.IsGhosted = false;

                    cleanCode = graphic.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                    graphic.IsVisible = false;
                    graphic.IsGhosted = true;
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(attributesCode + cleanCode, code.ToString());
                    graphic.IsVisible = true;
                    graphic.IsGhosted = false;

                    // Ghost only
                    attributesCode = "0 GHOST ";

                    cleanCode = graphic.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                    graphic.IsGhosted = true;
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(attributesCode + cleanCode, code.ToString());
                    graphic.IsGhosted = false;

                    cleanCode = graphic.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                    graphic.IsGhosted = true;
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(String.Empty, code.ToString());
                    graphic.IsGhosted = false;

                    cleanCode = graphic.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                    graphic.IsGhosted = true;
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(attributesCode + cleanCode, code.ToString());
                    graphic.IsGhosted = false;

                    // Hide only
                    attributesCode = "0 MLCAD HIDE ";

                    cleanCode = graphic.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                    graphic.IsVisible = false;
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(attributesCode + cleanCode, code.ToString());
                    graphic.IsVisible = true;

                    cleanCode = graphic.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                    graphic.IsVisible = false;
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(String.Empty, code.ToString());
                    graphic.IsVisible = true;

                    cleanCode = graphic.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                    graphic.IsVisible = false;
                    code = Utils.PreProcessCode(graphic.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    Assert.AreEqual(attributesCode + cleanCode, code.ToString());
                    graphic.IsVisible = true;
                }
            }

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Colour

        [TestMethod]
        public virtual void ColourValueTest()
        {
            IGraphic graphic = CreateTestGraphicWithColour();

            if (null != graphic)
            {
                uint defaultValue = graphic.OverrideableColourValue;
                uint newValue     = 1U;

                if (null != graphic)
                {
                    PropertyValueTest(graphic,
                                      defaultValue,
                                      newValue,
                                      delegate(IGraphic obj) { return obj.ColourValue; },
                                      delegate(IGraphic obj, uint value) { obj.ColourValue = value; },
                                      PropertyValueFlags.None);
                }
            }

            // setting ColourValue on an IGraphic which does not support it should have no effect
            graphic = CreateTestGraphicWithNoColour();

            if (null != graphic && !graphic.IsImmutable)
            {
                uint defaultValue = graphic.OverrideableColourValue;
                uint newValue     = 1U;

                Assert.IsFalse(graphic.ColourValueEnabled);
                Assert.AreEqual(defaultValue, graphic.ColourValue);
                graphic.ColourValue = newValue;
                Assert.AreEqual(defaultValue, graphic.ColourValue);
            }
        }

        [TestMethod]
        public virtual void ColourValueChangedTest()
        {
            IGraphic graphic = CreateTestGraphicWithColour();

            if (null != graphic)
            {
                uint valueToSet = 1U;

                PropertyChangedTest(graphic,
                                    "ColourValueChanged",
                                    valueToSet,
                                    delegate(IGraphic obj, PropertyChangedEventHandler<uint> handler) { obj.ColourValueChanged += handler; },
                                    delegate(IGraphic obj) { return obj.ColourValue; },
                                    delegate(IGraphic obj, uint value) { obj.ColourValue = value; });
            }
        }

        [TestMethod]
        public void GetColourTest()
        {
            IGraphic graphic = CreateTestGraphicWithColour();

            if (null == graphic)
                return;

            if (graphic.IsImmutable)
            {
                throw new NotImplementedException("IGraphicTest.GetColourTest() not implemented for read-only elements");
            }
            else
            {
                IColour actual;
                uint overrideColour;

                graphic.ColourValue = graphic.OverrideableColourValue;

                // unattached IGraphic: check we pick up the system-palette entry
                overrideColour      = 1U;
                graphic.ColourValue = overrideColour;
                actual              = graphic.GetColour(overrideColour);
                Assert.AreSame(Palette.SystemPalette[overrideColour], actual);
                Assert.IsTrue(actual.IsFrozen);

                // check for a Direct Colours value
                overrideColour      = 0x2FF00FF;
                graphic.ColourValue = overrideColour;
                actual              = graphic.GetColour(overrideColour);
                Assert.AreEqual(overrideColour, actual.Code);
                Assert.IsTrue(actual.IsFrozen);

                // check for a non-existent value: should return MainColour
                overrideColour      = 512;
                graphic.ColourValue = overrideColour;
                actual              = graphic.GetColour(overrideColour);
                Assert.AreSame(Palette.SystemPalette[Palette.MainColour], actual);

                // check that document-tree lookups work
                IPage page = new LDPage();
                IStep step = new LDStep();
                page.Add(step);
                overrideColour = 64U;
                IColour colour = new LDColour("0 COLOR " + overrideColour + " DkRed 0 123 46 47 255 123 46 47 255");
                step.Add(colour);
                graphic.ColourValue = overrideColour;
                step.Add(graphic);
                actual = graphic.GetColour(overrideColour);
                Assert.AreSame(colour, actual);
                Assert.AreNotSame(Palette.SystemPalette[overrideColour], actual);

                // placing the IGraphic after the IColour should return the palette-entry instead
                step.Remove(graphic);
                step.Insert(0, graphic);
                actual = graphic.GetColour(overrideColour);
                Assert.AreNotSame(colour, actual);
                Assert.AreSame(Palette.SystemPalette[overrideColour], actual);

                // the IGraphic should still function if placed in a later IStep
                step.Remove(graphic);
                step = new LDStep();
                step.Add(graphic);
                page.Add(step);
                actual = graphic.GetColour(overrideColour);
                Assert.AreSame(colour, actual);
                Assert.AreNotSame(Palette.SystemPalette[overrideColour], actual);
            }
        }

        #endregion Colour

        #region Coordinates

        [TestMethod]
        public void CoordinatesTest()
        {
            IGraphic graphic = CreateTestGraphicWithCoordinates();

            if (null != graphic)
            {
                Assert.AreNotEqual(0U, graphic.CoordinatesCount);

                IEnumerable<Vector3d> defaultValue = new Vector3d[graphic.CoordinatesCount];
                IEnumerable<Vector3d> newValue     = GenerateCoordinates(graphic);

                PropertyValueTest(graphic,
                                  defaultValue,
                                  newValue,
                                  delegate(IGraphic obj) { return obj.Coordinates; },
                                  delegate(IGraphic obj, IEnumerable<Vector3d> value) { obj.Coordinates = value; },
                                  delegate(IGraphic obj, IEnumerable<Vector3d> expectedValue)
                                  {
                                      IEnumerable<Vector3d> coordinates = obj.Coordinates;

                                      for (int i = 0; i < obj.CoordinatesCount; i++)
                                      {
                                          Assert.AreEqual(expectedValue.ElementAt(i), coordinates.ElementAt(i));
                                      }
                                  },
                                  PropertyValueFlags.None);
            }

            // when setting, the number of coordinates must match CoordinatesCount
            graphic = CreateTestGraphicWithCoordinates();

            if (null != graphic && !graphic.IsImmutable)
            {
                Assert.AreNotEqual(0U, graphic.CoordinatesCount);

                try
                {
                    graphic.Coordinates = new Vector3d[graphic.CoordinatesCount + 1];
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void CoordinatesChangedTest()
        {
            IGraphic graphic = CreateTestGraphicWithCoordinates();

            if (null != graphic)
            {
                Assert.AreNotEqual(0U, graphic.CoordinatesCount);

                IEnumerable<Vector3d> valueToSet = GenerateCoordinates(graphic);

                PropertyChangedTest(graphic,
                                    "CoordinatesChanged",
                                    valueToSet,
                                    delegate(IGraphic obj, PropertyChangedEventHandler<IEnumerable<Vector3d>> handler) { obj.CoordinatesChanged += handler; },
                                    delegate(IGraphic obj) { return obj.Coordinates; },
                                    delegate(IGraphic obj, IEnumerable<Vector3d> value) { obj.Coordinates = value; },
                                    delegate(IGraphic obj, IEnumerable<Vector3d> oldValue, IEnumerable<Vector3d> newValue, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                                    {
                                        for (int i = 0; i < obj.CoordinatesCount; i++)
                                        {
                                            Assert.AreEqual(oldValue.ElementAt(i), e.OldValue.ElementAt(i));
                                            Assert.AreEqual(newValue.ElementAt(i), e.NewValue.ElementAt(i));
                                        }
                                    });
            }
        }

        #endregion Coordinates

        #region Geometry

        [TestMethod]
        public virtual void BoundingBoxTest()
        {
            IGraphic graphic = CreateTestGraphicWithCoordinates();

            if (null != graphic)
            {
                Assert.AreNotEqual(0U, graphic.CoordinatesCount);

                Vector3d[] coords = GenerateCoordinates(graphic);

                Assert.IsTrue(coords.Length >= 2);
                graphic.Coordinates = coords;

                Box3d bounds = new Box3d(coords[0], coords[1]);

                foreach (Vector3d coord in coords)
                {
                    bounds.Union(coord);
                }

                Assert.AreEqual(bounds, graphic.BoundingBox);

                IGeometricTest.BoundingBoxTest(graphic, graphic);
            }
        }

        [TestMethod]
        public virtual void OriginTest()
        {
            IGraphic graphic = CreateTestGraphicWithCoordinates();

            if (null != graphic)
                IGeometricTest.OriginTest(graphic, graphic);
        }

        [TestMethod]
        public void WindingModeTest()
        {
            IGraphic graphic = CreateTestGraphicWithCoordinates();

            if (null != graphic)
                IGeometricTest.WindingModeTest(graphic, graphic);
        }

        [TestMethod]
        public virtual void TransformTest()
        {
            Matrix4d transform = new Matrix4d(1, 2, 3, 0, 4, 5, 6, 0, 7, 8, 9, 0, 10, 11, 12, 1);
            IGraphic graphic = CreateTestGraphicWithCoordinates();

            if (null != graphic)
            {
                Assert.AreNotEqual(0U, graphic.CoordinatesCount);

                Vector3d[] coords = GenerateCoordinates(graphic);

                graphic.Coordinates = coords;

                for (int i = 0; i < coords.Length; i++)
                {
                    coords[i] = Vector3d.Transform(coords[i], transform);
                }

                graphic.Transform(ref transform);

                for (int i = 0; i < coords.Length; i++)
                {
                    Assert.AreEqual(coords[i], graphic.Coordinates.ElementAt(i));
                }

                IGeometricTest.TransformTest(graphic, graphic, ref transform);
            }
        }

        [TestMethod]
        public virtual void ReverseWindingTest()
        {
            IGraphic graphic = CreateTestGraphicWithCoordinates();

            if (null != graphic)
                IGeometricTest.ReverseWindingTest(graphic, graphic);
        }

        #endregion Geometry
    }
}
