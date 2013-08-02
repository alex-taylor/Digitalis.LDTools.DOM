#region License

//
// IColourTest.cs
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
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IColourTest : IMetaCommandTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IColour); } }

        protected sealed override IMetaCommand CreateTestMetaCommand()
        {
            return CreateTestColour();
        }

        protected abstract IColour CreateTestColour();

        protected abstract IColour CreateTestDirectColour();

        protected sealed override string[] SyntaxExamples
        {
            get
            {
                return new string[]
                {
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE 0\r\n",
                    "0 !COLOUR name CODE 0 VALUE 0x123456 EDGE 0\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456 ALPHA 128\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456 ALPHA 128 LUMINANCE 100\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456 ALPHA 128 LUMINANCE 100 CHROME\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456 ALPHA 128 LUMINANCE 100 PEARLESCENT\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456 ALPHA 128 LUMINANCE 100 RUBBER\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456 ALPHA 128 LUMINANCE 100 MATTE_METALLIC\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456 ALPHA 128 LUMINANCE 100 METAL\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456 ALPHA 128 LUMINANCE 100 MATERIAL GLITTER VALUE #123456\r\n",
                    "0 !COLOUR name CODE 0 VALUE #123456 EDGE #123456 ALPHA 128 LUMINANCE 100 MATERIAL SPECKLE VALUE #123456\r\n",
                    "0 COLOUR 0 name 0 255 255 255 255 255 255 255 255\r\n",
                    "0 COLOR 0 name 0 255 255 255 255 255 255 255 255\r\n"
                };
            }
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IColour colour = CreateTestColour();

            Assert.AreEqual(DOMObjectType.MetaCommand, colour.ObjectType);
            Assert.IsFalse(colour.IsTopLevelElement);
            Assert.IsTrue(colour.IsStateElement);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Cloning and Serialization

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IColour first  = (IColour)original;
            IColour second = (IColour)copy;

            // properties should be preserved
            Assert.AreEqual(first.Name, second.Name);
            Assert.AreEqual(first.Code, second.Code);
            Assert.AreEqual(first.EdgeCode, second.EdgeCode);
            Assert.AreEqual(first.EdgeValue, second.EdgeValue);
            Assert.AreEqual(first.Value, second.Value);
            Assert.AreEqual(first.Luminance, second.Luminance);
            Assert.AreEqual(first.IsTransparent, second.IsTransparent);
            Assert.AreNotSame(first.Material, second.Material);
            Assert.AreEqual(first.Material.GetType(), second.Material.GetType());
            Assert.IsTrue(first.Material.IsEquivalentTo(second.Material));

            // by definition, the copy cannot be a member of the system-palette
            Assert.IsFalse(second.IsSystemPaletteColour);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            IColour colour = CreateTestColour();
            StringBuilder code;
            string expected = "0 !COLOUR Black CODE 0 VALUE #05131D EDGE #595959\r\n";

            if (colour.IsImmutable)
            {
                throw new NotImplementedException("IColourTest.ToCodeTest() not implemented for immutable objects");
            }
            else
            {
                colour.Name      = "Black";
                colour.Code      = 0;
                colour.Value     = Color.FromArgb(0xFF, 0x05, 0x13, 0x1D);
                colour.EdgeCode  = 0x2595959;
                colour.Luminance = 0;
                colour.Material  = new PlasticMaterial();

                // !COLOUR not allowed in PartsLibrary mode
                code = Utils.PreProcessCode(colour.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(colour.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(colour.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(String.Empty, code.ToString());
            }

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Properties

        [TestMethod]
        public void NameTest()
        {
            IColour colour      = CreateTestColour();
            string defaultValue = "<unknown>";
            string newValue     = "new name";

            PropertyValueTest(colour,
                              defaultValue,
                              newValue,
                              delegate(IColour obj) { return obj.Name; },
                              delegate(IColour obj, string value) { obj.Name = value; },
                              PropertyValueFlags.None);

            // if Code is a Direct Colours value, then Name cannot be set and will return the Code as a string
            colour = CreateTestDirectColour();

            if (!colour.IsImmutable)
            {
                colour.Name = "foo";
                Assert.AreEqual(String.Format("#{0:X7}", colour.Code), colour.Name);
            }
        }

        [TestMethod]
        public void NameChangedTest()
        {
            IColour colour    = CreateTestColour();
            string valueToSet = "new name";

            PropertyChangedTest(colour,
                                "NameChanged",
                                valueToSet,
                                delegate(IColour obj, PropertyChangedEventHandler<string> handler) { obj.NameChanged += handler; },
                                delegate(IColour obj) { return obj.Name; },
                                delegate(IColour obj, string value) { obj.Name = value; });
        }

        [TestMethod]
        public void CodeTest()
        {
            IColour colour    = CreateTestColour();
            uint defaultValue = Palette.MainColour;
            uint newValue     = 1U;

            PropertyValueTest(colour,
                              defaultValue,
                              newValue,
                              delegate(IColour obj) { return obj.Code; },
                              delegate(IColour obj, uint value) { obj.Code = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void CodeChangedTest()
        {
            IColour colour  = CreateTestColour();
            uint valueToSet = 1U;

            PropertyChangedTest(colour,
                                "CodeChanged",
                                valueToSet,
                                delegate(IColour obj, PropertyChangedEventHandler<uint> handler) { obj.CodeChanged += handler; },
                                delegate(IColour obj) { return obj.Code; },
                                delegate(IColour obj, uint value) { obj.Code = value; });
        }

        [TestMethod]
        public void EdgeCodeTest()
        {
            IColour colour    = CreateTestColour();
            uint defaultValue = Palette.EdgeColour;
            uint newValue     = 1U;

            PropertyValueTest(colour,
                              defaultValue,
                              newValue,
                              delegate(IColour obj) { return obj.EdgeCode; },
                              delegate(IColour obj, uint value) { obj.EdgeCode = value; },
                              PropertyValueFlags.None);

            // transparent Direct Colours are not supported
            colour = CreateTestColour();

            if (!colour.IsImmutable)
            {
                try
                {
                    colour.EdgeCode = 0x3123456;
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void EdgeCodeChangedTest()
        {
            IColour colour  = CreateTestColour();
            uint valueToSet = 1U;

            PropertyChangedTest(colour,
                                "EdgeCodeChanged",
                                valueToSet,
                                delegate(IColour obj, PropertyChangedEventHandler<uint> handler) { obj.EdgeCodeChanged += handler; },
                                delegate(IColour obj) { return obj.EdgeCode; },
                                delegate(IColour obj, uint value) { obj.EdgeCode = value; });
        }

        [TestMethod]
        public void ValueTest()
        {
            IColour colour     = CreateTestColour();
            Color defaultValue = Color.Black;
            Color newValue     = Color.Red;

            PropertyValueTest(colour,
                              defaultValue,
                              newValue,
                              delegate(IColour obj) { return obj.Value; },
                              delegate(IColour obj, Color value) { obj.Value = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void ValueChangedTest()
        {
            IColour colour   = CreateTestColour();
            Color valueToSet = Color.Red;

            PropertyChangedTest(colour,
                                "ValueChanged",
                                valueToSet,
                                delegate(IColour obj, PropertyChangedEventHandler<Color> handler) { obj.ValueChanged += handler; },
                                delegate(IColour obj) { return obj.Value; },
                                delegate(IColour obj, Color value) { obj.Value = value; });
        }

        [TestMethod]
        public void EdgeValueTest()
        {
            IColour colour = CreateTestColour();

            if (!colour.IsImmutable)
            {
                colour.EdgeCode = 0x2595959;
                Assert.AreEqual(Color.FromArgb(0xFF, 0x59, 0x59, 0x59), colour.EdgeValue);
            }
        }

        [TestMethod]
        public void LuminanceTest()
        {
            IColour colour    = CreateTestColour();
            byte defaultValue = 0;
            byte newValue     = 128;

            PropertyValueTest(colour,
                              defaultValue,
                              newValue,
                              delegate(IColour obj) { return obj.Luminance; },
                              delegate(IColour obj, byte value) { obj.Luminance = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void LuminanceChangedTest()
        {
            IColour colour  = CreateTestColour();
            byte valueToSet = 128;

            PropertyChangedTest(colour,
                                "LuminanceChanged",
                                valueToSet,
                                delegate(IColour obj, PropertyChangedEventHandler<byte> handler) { obj.LuminanceChanged += handler; },
                                delegate(IColour obj) { return obj.Luminance; },
                                delegate(IColour obj, byte value) { obj.Luminance = value; });
        }

        [TestMethod]
        public void MaterialTest()
        {
            IColour colour         = CreateTestColour();
            IMaterial defaultValue = new PlasticMaterial();
            IMaterial newValue     = new ChromeMaterial();

            PropertyValueTest(colour,
                              defaultValue,
                              newValue,
                              delegate(IColour obj) { return obj.Material; },
                              delegate(IColour obj, IMaterial value) { obj.Material = value; },
                              delegate(IColour obj, IMaterial expectedValue) { Assert.IsTrue(expectedValue.IsEquivalentTo(obj.Material)); },
                              PropertyValueFlags.None);

            colour = CreateTestColour();

            if (!colour.IsImmutable)
            {
                // a material cannot be added to more than one IColour at a time
                IColour colour2 = CreateTestColour();

                try
                {
                    colour2.Material = colour.Material;
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
                }

                // removing a material should disconnect it completely
                colour = CreateTestColour();
                IMaterial material = colour.Material;
                colour.Material = new PlasticMaterial();
                Assert.IsNull(material.Colour);
            }
        }

        [TestMethod]
        public void MaterialChangedTest()
        {
            IColour colour       = CreateTestColour();
            IMaterial valueToSet = new ChromeMaterial();

            PropertyChangedTest(colour,
                                "MaterialChanged",
                                valueToSet,
                                delegate(IColour obj, PropertyChangedEventHandler<IMaterial> handler) { obj.MaterialChanged += handler; },
                                delegate(IColour obj) { return obj.Material; },
                                delegate(IColour obj, IMaterial value) { obj.Material = value; });

            // changing a material's properties should produce events from the IColour
            colour = CreateTestColour();

            if (!colour.IsImmutable)
            {
                bool eventSeen        = false;
                bool genericEventSeen = false;

                colour = CreateTestColour();

                colour.MaterialChanged += delegate(object sender, PropertyChangedEventArgs<IMaterial> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                };

                colour.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(colour, sender);
                    Assert.AreEqual("MaterialChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(EventArgs));
                };

                GlitterMaterial glitter = new GlitterMaterial();
                IMaterial oldMaterial   = colour.Material;
                IMaterial newMaterial   = glitter;
                colour.Material         = glitter;
                eventSeen               = false;
                genericEventSeen        = false;
                glitter.Fraction        = 0.1;
                Assert.AreEqual(0.1, glitter.Fraction);
                Assert.IsFalse(eventSeen);
                Assert.IsTrue(genericEventSeen);
            }
        }

        [TestMethod]
        public void IsTransparentTest()
        {
            IColour colour = CreateTestColour();
            Assert.IsFalse(colour.IsTransparent);

            if (!colour.IsImmutable)
            {
                colour.Value = Color.FromArgb(128, 255, 255, 255);
                Assert.IsTrue(colour.IsTransparent);
            }
        }

        [TestMethod]
        public void IsSystemPaletteColourTest()
        {
            IColour colour = CreateTestColour();
            Assert.IsFalse(colour.IsSystemPaletteColour);

            colour = Palette.SystemPalette[Palette.MainColour];
            Assert.IsTrue(colour.IsSystemPaletteColour);

            IColour clone = (IColour)colour.Clone();
            Assert.IsFalse(clone.IsSystemPaletteColour);
        }

        #endregion Properties
    }
}
