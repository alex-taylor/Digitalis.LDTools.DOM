#region License

//
// LDColourTest.cs
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
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    [TestClass]
    public sealed class LDColourTest : IColourTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDColour); } }

        protected override IColour CreateTestColour()
        {
            return new LDColour();
        }

        protected override IColour CreateTestDirectColour()
        {
            return new LDColour(0x2123456);
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IColour colour = CreateTestColour())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(colour.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.ColourDefinition, typeNameAttr.Description);
                Assert.AreEqual(Resources.ColourDefinition, colour.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(colour.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNotNull(elementFlagsAttr);
                Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

                ElementCategoryAttribute categoryAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementCategoryAttribute)) as ElementCategoryAttribute;
                Assert.IsNotNull(categoryAttr);
                Assert.AreEqual(Resources.ElementCategory_MetaCommand, categoryAttr.Description);

                Assert.AreEqual(String.Empty, colour.ExtendedDescription);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public void AnalyticsValidTest()
        {
            LDColour colour = new LDColour();
            ICollection<IProblemDescriptor> problems;

            colour.Name = "A_Valid_Name";
            colour.EdgeCode = Palette.MainColour;
            Assert.IsFalse(colour.IsNameInvalid);
            Assert.IsFalse(colour.IsEdgeInvalid);

            Assert.IsFalse(colour.HasProblems(CodeStandards.Full));
            problems = colour.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(colour.HasProblems(CodeStandards.PartsLibrary));
            problems = colour.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(colour.HasProblems(CodeStandards.OfficialModelRepository));
            problems = colour.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);
        }

        [TestMethod]
        public void AnalyticsInvalidNameCharsTest()
        {
            LDColour colour = new LDColour();
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            colour.Name = "An Invalid Name";
            colour.EdgeCode = Palette.MainColour;
            Assert.IsTrue(colour.IsNameInvalid);
            Assert.IsFalse(colour.IsEdgeInvalid);

            // mode checks
            Assert.IsTrue(colour.HasProblems(CodeStandards.Full));
            problems = colour.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDColour.Problem_InvalidName, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(colour, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(colour.HasProblems(CodeStandards.PartsLibrary));
            problems = colour.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDColour.Problem_InvalidName, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(colour, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(colour.HasProblems(CodeStandards.OfficialModelRepository));
            problems = colour.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDColour.Problem_InvalidName, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(colour, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsInvalidNameStartTest()
        {
            LDColour colour = new LDColour();
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            colour.Name = "0invalid";
            colour.EdgeCode = Palette.MainColour;
            Assert.IsTrue(colour.IsNameInvalid);
            Assert.IsFalse(colour.IsEdgeInvalid);

            // mode checks
            Assert.IsTrue(colour.HasProblems(CodeStandards.Full));
            problems = colour.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDColour.Problem_InvalidName, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(colour, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(colour.HasProblems(CodeStandards.PartsLibrary));
            problems = colour.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDColour.Problem_InvalidName, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(colour, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(colour.HasProblems(CodeStandards.OfficialModelRepository));
            problems = colour.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDColour.Problem_InvalidName, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(colour, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsInvalidNameDirectColoursTest()
        {
            LDColour colour = new LDColour(0x2FFFFFF);
            ICollection<IProblemDescriptor> problems;

            Assert.AreEqual("#2FFFFFF", colour.Name);
            Assert.IsFalse(colour.IsNameInvalid);
            Assert.IsFalse(colour.IsEdgeInvalid);

            // mode checks
            Assert.IsFalse(colour.HasProblems(CodeStandards.Full));
            problems = colour.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(colour.HasProblems(CodeStandards.PartsLibrary));
            problems = colour.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(colour.HasProblems(CodeStandards.OfficialModelRepository));
            problems = colour.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);
        }

        [TestMethod]
        public void AnalyticsInvalidEdgeTest()
        {
            LDColour colour = new LDColour();
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            colour.Name = "Name";
            colour.EdgeCode = 1000;
            Assert.IsFalse(colour.IsNameInvalid);
            Assert.IsTrue(colour.IsEdgeInvalid);

            // mode checks
            Assert.IsTrue(colour.HasProblems(CodeStandards.Full));
            problems = colour.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDColour.Problem_InvalidEdge, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(colour, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(colour.HasProblems(CodeStandards.PartsLibrary));
            problems = colour.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDColour.Problem_InvalidEdge, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(colour, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(colour.HasProblems(CodeStandards.OfficialModelRepository));
            problems = colour.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDColour.Problem_InvalidEdge, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(colour, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            // the problem should clear once we add a matching LDColour
            LDPage page = new LDPage();
            LDStep step = new LDStep();
            page.Add(step);
            step.Add(new LDColour("0 !COLOUR Edge CODE 1000 VALUE #123456 EDGE 0"));
            step.Add(colour);
            Assert.IsFalse(colour.IsEdgeInvalid);
            Assert.IsFalse(colour.HasProblems(CodeStandards.OfficialModelRepository));
            Assert.AreEqual(0, colour.Analyse(CodeStandards.OfficialModelRepository).Count());
        }

        #endregion Analytics

        #region Constructor

        [TestMethod]
        public void LDrawColourConstructorTest()
        {
            string name                   = "Name";
            uint code                     = 0;
            Color value                   = Color.Black;
            Color edgeValue               = Color.White;
            byte luminance                = 0;

            LDColour colour = new LDColour(name, code, value, edgeValue, luminance, new PlasticMaterial());

            Assert.AreEqual(name, colour.Name);
            Assert.AreEqual(code, colour.Code);
            Assert.AreEqual(value, colour.Value);
            Assert.AreEqual(edgeValue.ToArgb(), colour.EdgeValue.ToArgb());
            Assert.AreEqual(LDColour.ConvertRGBToDirectColour(edgeValue), colour.EdgeCode);
            Assert.AreEqual(luminance, colour.Luminance);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));
        }

        [TestMethod]
        public void LDrawColourConstructorTest1()
        {
            string name                   = "Name";
            uint code                     = 0;
            Color value                   = Color.Black;
            uint edgeCode                 = 1;
            byte luminance                = 0;

            LDColour colour = new LDColour(name, code, value, edgeCode, luminance, new PlasticMaterial());

            Assert.AreEqual(name, colour.Name);
            Assert.AreEqual(code, colour.Code);
            Assert.AreEqual(value, colour.Value);
            Assert.AreEqual(edgeCode, colour.EdgeCode);
            Assert.AreEqual(luminance, colour.Luminance);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));
        }

        [TestMethod]
        public void LDrawColourConstructorTest2()
        {
            // opaque 24-bit
            uint directColourValue = 0x2FF00FF;
            LDColour colour = new LDColour(directColourValue);
            Assert.AreEqual(LDColour.ColourValueToCode(directColourValue), colour.Name);
            Assert.AreEqual(directColourValue, colour.Code);
            Assert.AreEqual(LDColour.ConvertDirectColourToRGB(directColourValue), colour.Value);
            Assert.AreEqual(LDColour.ConvertRGBToDirectColour(Color.Black), colour.EdgeCode);
            Assert.AreEqual(Color.Black.ToArgb(), colour.EdgeValue.ToArgb());
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsFalse(colour.IsTransparent);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));

            // transparent 24-bit
            directColourValue = 0x3FF00FF;
            colour = new LDColour(directColourValue);
            Assert.AreEqual(LDColour.ColourValueToCode(directColourValue), colour.Name);
            Assert.AreEqual(directColourValue, colour.Code);
            Assert.AreEqual(LDColour.ConvertDirectColourToRGB(directColourValue), colour.Value);
            Assert.AreEqual(LDColour.ConvertRGBToDirectColour(Color.Black), colour.EdgeCode);
            Assert.AreEqual(Color.Black.ToArgb(), colour.EdgeValue.ToArgb());
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsTrue(colour.IsTransparent);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));

            // opaque 12-bit dithered (blended)
            directColourValue = 0x4FF00FF;
            uint expected = 0x27FFF7F;
            colour = new LDColour(directColourValue);
            Assert.AreEqual(LDColour.ColourValueToCode(expected), colour.Name);
            Assert.AreEqual(expected, colour.Code);
            Assert.AreEqual(LDColour.ConvertDirectColourToRGB(expected), colour.Value);
            Assert.AreEqual(LDColour.ConvertRGBToDirectColour(Color.Black), colour.EdgeCode);
            Assert.AreEqual(Color.Black.ToArgb(), colour.EdgeValue.ToArgb());
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsFalse(colour.IsTransparent);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));

            // transparent 12-bit dithered (blended)
            directColourValue = 0x5FF0000;
            expected = 0x3FFFF00;
            colour = new LDColour(directColourValue);
            Assert.AreEqual(LDColour.ColourValueToCode(expected), colour.Name);
            Assert.AreEqual(expected, colour.Code);
            Assert.AreEqual(LDColour.ConvertDirectColourToRGB(expected), colour.Value);
            Assert.AreEqual(LDColour.ConvertRGBToDirectColour(Color.Black), colour.EdgeCode);
            Assert.AreEqual(Color.Black.ToArgb(), colour.EdgeValue.ToArgb());
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsTrue(colour.IsTransparent);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));

            // transparent 12-bit dithered (blended), alternative form
            directColourValue = 0x6000FF0;
            expected = 0x3FFFF00;
            colour = new LDColour(directColourValue);
            Assert.AreEqual(LDColour.ColourValueToCode(expected), colour.Name);
            Assert.AreEqual(expected, colour.Code);
            Assert.AreEqual(LDColour.ConvertDirectColourToRGB(expected), colour.Value);
            Assert.AreEqual(LDColour.ConvertRGBToDirectColour(Color.Black), colour.EdgeCode);
            Assert.AreEqual(Color.Black.ToArgb(), colour.EdgeValue.ToArgb());
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsTrue(colour.IsTransparent);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));
        }

        [TestMethod]
        public void LDrawColourConstructorTest3()
        {
            LDColour colour = new LDColour();

            Assert.AreEqual("<unknown>", colour.Name);
            Assert.AreEqual(Palette.MainColour, colour.Code);
            Assert.AreEqual(Color.Black.ToArgb(), colour.Value.ToArgb());
            Assert.AreEqual(Palette.EdgeColour, colour.EdgeCode);
            Assert.AreEqual(Palette.SystemPalette[Palette.MainColour].EdgeValue, colour.EdgeValue);
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));
        }

        [TestMethod]
        public void LDrawColourConstructorTest4()
        {
            string code = "0 !COLOUR Black CODE 0 VALUE #05131D EDGE #595959";
            LDColour colour = new LDColour(code);
            Assert.AreEqual("Black", colour.Name);
            Assert.AreEqual(0U, colour.Code);
            Assert.AreEqual(Color.FromArgb(0x05, 0x13, 0x1D), colour.Value);
            Assert.AreEqual(LDColour.ConvertRGBToDirectColour(Color.FromArgb(0x59, 0x59, 0x59)), colour.EdgeCode);
            Assert.AreEqual(Color.FromArgb(0x59, 0x59, 0x59), colour.EdgeValue);
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));

            code = "0 !COLOUR Black CODE 0 VALUE #05131D EDGE #595959 CHROME";
            colour = new LDColour(code);
            Assert.AreEqual("Black", colour.Name);
            Assert.AreEqual(0U, colour.Code);
            Assert.AreEqual(Color.FromArgb(0x05, 0x13, 0x1D), colour.Value);
            Assert.AreEqual(LDColour.ConvertRGBToDirectColour(Color.FromArgb(0x59, 0x59, 0x59)), colour.EdgeCode);
            Assert.AreEqual(Color.FromArgb(0x59, 0x59, 0x59), colour.EdgeValue);
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(ChromeMaterial));

            code = "0 !COLOUR Black CODE 0 VALUE #05131D EDGE #595959 MATERIAL SPECKLE VALUE #595959 FRACTION 0.4 MINSIZE 1 MAXSIZE 3";
            colour = new LDColour(code);
            Assert.AreEqual("Black", colour.Name);
            Assert.AreEqual(0U, colour.Code);
            Assert.AreEqual(Color.FromArgb(0x05, 0x13, 0x1D), colour.Value);
            Assert.AreEqual(LDColour.ConvertRGBToDirectColour(Color.FromArgb(0x59, 0x59, 0x59)), colour.EdgeCode);
            Assert.AreEqual(Color.FromArgb(0x59, 0x59, 0x59), colour.EdgeValue);
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(SpeckleMaterial));
            SpeckleMaterial material = (SpeckleMaterial)colour.Material;
            Assert.AreEqual(Color.FromArgb(0xFF, 0x59, 0x59, 0x59), material.Value);
            Assert.AreEqual(0.4, material.Fraction);
            Assert.AreEqual(1U, material.MinSize);
            Assert.AreEqual(3U, material.MaxSize);

            code = "0 COLOR 64 DkRed 0 123 46 47 255 123 46 47 255";
            colour = new LDColour(code);
            Assert.AreEqual("DkRed", colour.Name);
            Assert.AreEqual(64U, colour.Code);
            Assert.AreEqual(Color.FromArgb(123, 46, 47), colour.Value);
            Assert.AreEqual(0U, colour.EdgeCode);
            Assert.AreEqual(Palette.SystemPalette[0].Value, colour.EdgeValue);
            Assert.AreEqual(0, colour.Luminance);
            Assert.IsNotNull(colour.Material);
            Assert.IsInstanceOfType(colour.Material, typeof(PlasticMaterial));
        }

        #endregion Constructor

        #region Parser

        [TestMethod]
        public void ParserTest()
        {
            bool documentModified;

            string code = "0 title\r\n" +
                          "0 Name: name.dat\r\n" +
                          "\r\n" +
                          "0 !COLOUR Black CODE 0 VALUE #05131D EDGE #595959\r\n" +
                          "0 COLOR 64 DkRed 0 123 46 47 255 123 46 47 255\r\n" +
                          "0 COLOUR 64 DkRed 0 123 46 47 255 123 46 47 255\r\n" +
                          "0 !COLOUR\r\n" +
                          "0 COLOR DEFINITIONS\r\n" +
                          "0 COLOUR\r\n";

            IDocument doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            IPage page    = doc[0];
            IStep step    = page[0];
            Assert.AreEqual(6, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDColour));
            Assert.IsInstanceOfType(step[1], typeof(LDColour));
            Assert.IsInstanceOfType(step[2], typeof(LDColour));
            Assert.IsInstanceOfType(step[3], typeof(IComment));
            Assert.IsInstanceOfType(step[4], typeof(IComment));
            Assert.IsInstanceOfType(step[5], typeof(IComment));
        }

        #endregion Parser

        #region Utility methods

        [TestMethod]
        public void ColourValueToCodeTest()
        {
            uint colourValue = 0;
            string expected = "0";
            string actual;

            actual = LDColour.ColourValueToCode(colourValue);
            Assert.AreEqual(expected, actual);

            colourValue = 0x2FF00FF;
            expected = "#2FF00FF";
            actual = LDColour.ColourValueToCode(colourValue);
            Assert.AreEqual(expected, actual);

            colourValue = 0x3FF00FF;
            expected = "#3FF00FF";
            actual = LDColour.ColourValueToCode(colourValue);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConvertDirectColourToRGBTest()
        {
            uint directColourValue = 0x2FF00FF;
            Color? expected = Color.FromArgb(255, 0, 255);
            Color? actual;

            actual = LDColour.ConvertDirectColourToRGB(directColourValue);
            Assert.AreEqual(expected, actual);

            directColourValue = 0x3FF00FF;
            expected = Color.FromArgb(127, 255, 0, 255);
            actual = LDColour.ConvertDirectColourToRGB(directColourValue);
            Assert.AreEqual(expected, actual);

            directColourValue = 0;
            actual = LDColour.ConvertDirectColourToRGB(directColourValue);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ConvertRGBToDirectColourTest()
        {
            Color color = Color.FromArgb(255, 0, 0);
            uint expected = 0x2FF0000;
            uint actual;
            actual = LDColour.ConvertRGBToDirectColour(color);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsDirectColourTest()
        {
            Assert.IsTrue(LDColour.IsDirectColour(0x2FF00FF));
            Assert.IsTrue(LDColour.IsDirectColour(0x3FF00FF));
            Assert.IsTrue(LDColour.IsDirectColour(0x4FF00FF));
            Assert.IsTrue(LDColour.IsDirectColour(0x5FF00FF));
            Assert.IsFalse(LDColour.IsDirectColour(255));
        }

        [TestMethod]
        public void IsOpaqueDirectColourTest()
        {
            Assert.IsTrue(LDColour.IsOpaqueDirectColour(0x2FF00FF));
            Assert.IsFalse(LDColour.IsOpaqueDirectColour(0x3FF00FF));
            Assert.IsTrue(LDColour.IsOpaqueDirectColour(0x4FF00FF));
            Assert.IsFalse(LDColour.IsOpaqueDirectColour(0x5FF00FF));
        }

        [TestMethod]
        public void IsTransparentDirectColourTest()
        {
            Assert.IsFalse(LDColour.IsTransparentDirectColour(0x2FF00FF));
            Assert.IsTrue(LDColour.IsTransparentDirectColour(0x3FF00FF));
            Assert.IsFalse(LDColour.IsTransparentDirectColour(0x4FF00FF));
            Assert.IsTrue(LDColour.IsTransparentDirectColour(0x5FF00FF));
        }

        #endregion Utility methods
    }
}
