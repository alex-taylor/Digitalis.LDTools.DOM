#region License

//
// LDTexmapTest.cs
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

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    [TestClass]
    public sealed class LDTexmapTest : ITexmapTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDTexmap); } }

        protected override ITexmap CreateTestTexmap()
        {
            return new LDTexmap();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (ITexmap texmap = CreateTestTexmap())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(texmap.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.Texmap, typeNameAttr.Description);
                Assert.AreEqual(Resources.Texmap, texmap.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(texmap.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNotNull(elementFlagsAttr);
                Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

                Assert.AreEqual(String.Empty, texmap.ExtendedDescription);

                Assert.IsFalse(texmap.IsImmutable);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public void AnalyticsValidTexmapTest()
        {
            LDTexmap target;
            ICollection<IProblemDescriptor> problems;

            target = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 indy_face.png");
            target.SharedGeometry.Add(new LDLine());
            Assert.IsFalse(target.IsTextureMissing);
            Assert.IsFalse(target.IsGlossmapMissing);
            Assert.IsFalse(target.IsGeometryMissing);
            Assert.IsFalse(target.IsTextureTooLong);
            Assert.IsFalse(target.IsTextureInvalidChars);
            Assert.IsFalse(target.IsGlossmapInvalidChars);
            Assert.IsFalse(target.IsGlossmapTooLong);

            // mode-checks
            Assert.IsFalse(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);
        }

        [TestMethod]
        public void AnalyticsColocatedTest()
        {
            LDTexmap target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 0 0 0 0 0 0 indy_face.png");
            target.SharedGeometry.Add(new LDLine());
            Assert.IsFalse(target.IsTextureMissing);
            Assert.IsFalse(target.IsGlossmapMissing);
            Assert.IsFalse(target.IsGeometryMissing);
            Assert.IsFalse(target.IsTextureTooLong);
            Assert.IsFalse(target.IsTextureInvalidChars);
            Assert.IsFalse(target.IsGlossmapInvalidChars);
            Assert.IsFalse(target.IsGlossmapTooLong);
            Assert.IsTrue(target.IsColocated);

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(Graphic.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(Graphic.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(Graphic.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsMissingGeometryTest()
        {
            LDTexmap target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 indy_face.png");
            Assert.IsFalse(target.IsTextureMissing);
            Assert.IsFalse(target.IsGlossmapMissing);
            Assert.IsTrue(target.IsGeometryMissing);
            Assert.IsFalse(target.IsTextureTooLong);
            Assert.IsFalse(target.IsTextureInvalidChars);
            Assert.IsFalse(target.IsGlossmapInvalidChars);
            Assert.IsFalse(target.IsGlossmapTooLong);

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTexmap.Problem_NoGeometry, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTexmap.Problem_NoGeometry, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTexmap.Problem_NoGeometry, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsMissingTextureTest()
        {
            LDTexmap target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 foo.png");
            target.SharedGeometry.Add(new LDLine());
            Assert.IsTrue(target.IsTextureMissing);
            Assert.IsFalse(target.IsGlossmapMissing);
            Assert.IsFalse(target.IsGeometryMissing);
            Assert.IsFalse(target.IsTextureTooLong);
            Assert.IsFalse(target.IsTextureInvalidChars);
            Assert.IsFalse(target.IsGlossmapInvalidChars);
            Assert.IsFalse(target.IsGlossmapTooLong);

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTexmap.Problem_NoTexture, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTexmap.Problem_NoTexture, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTexmap.Problem_NoTexture, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsMissingGlossmapTest()
        {
            LDTexmap target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 indy_face.png GLOSSMAP glossmap.png");
            target.SharedGeometry.Add(new LDLine());
            Assert.IsFalse(target.IsTextureMissing);
            Assert.IsTrue(target.IsGlossmapMissing);
            Assert.IsFalse(target.IsGeometryMissing);
            Assert.IsFalse(target.IsTextureTooLong);
            Assert.IsFalse(target.IsTextureInvalidChars);
            Assert.IsFalse(target.IsGlossmapInvalidChars);
            Assert.IsFalse(target.IsGlossmapTooLong);

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTexmap.Problem_NoGlossmap, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTexmap.Problem_NoGlossmap, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTexmap.Problem_NoGlossmap, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsTextureInvalidCharsTest()
        {
            LDTexmap target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 [indy_face].png");
            target.SharedGeometry.Add(new LDLine());
            Assert.IsTrue(target.IsTextureMissing);
            Assert.IsFalse(target.IsGlossmapMissing);
            Assert.IsFalse(target.IsGeometryMissing);
            Assert.IsFalse(target.IsTextureTooLong);
            Assert.IsTrue(target.IsTextureInvalidChars);
            Assert.IsFalse(target.IsGlossmapInvalidChars);
            Assert.IsFalse(target.IsGlossmapTooLong);

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);     // file-not-found

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(2, problems.Count);
            problem = problems.Last();
            Assert.AreEqual(LDTexmap.Problem_TextureInvalidChars, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(2, problems.Count);
            problem = problems.Last();
            Assert.AreEqual(LDTexmap.Problem_TextureInvalidChars, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsGlossmapInvalidCharsTest()
        {
            LDTexmap target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 indy_face.png GLOSSMAP [].png");
            target.SharedGeometry.Add(new LDLine());
            Assert.IsFalse(target.IsTextureMissing);
            Assert.IsTrue(target.IsGlossmapMissing);
            Assert.IsFalse(target.IsGeometryMissing);
            Assert.IsFalse(target.IsTextureTooLong);
            Assert.IsFalse(target.IsTextureInvalidChars);
            Assert.IsTrue(target.IsGlossmapInvalidChars);
            Assert.IsFalse(target.IsGlossmapTooLong);

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);     // file-not-found

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(2, problems.Count);
            problem = problems.Last();
            Assert.AreEqual(LDTexmap.Problem_GlossmapInvalidChars, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(2, problems.Count);
            problem = problems.Last();
            Assert.AreEqual(LDTexmap.Problem_GlossmapInvalidChars, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsTextureTooLongTest()
        {
            LDTexmap target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 abcdefghijklmnopqrstuvwxyz.png");
            target.SharedGeometry.Add(new LDLine());
            Assert.IsTrue(target.IsTextureMissing);
            Assert.IsFalse(target.IsGlossmapMissing);
            Assert.IsFalse(target.IsGeometryMissing);
            Assert.IsTrue(target.IsTextureTooLong);
            Assert.IsFalse(target.IsTextureInvalidChars);
            Assert.IsFalse(target.IsGlossmapInvalidChars);
            Assert.IsFalse(target.IsGlossmapTooLong);

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);     // file-not-found

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(2, problems.Count);
            problem = problems.Last();
            Assert.AreEqual(LDTexmap.Problem_TextureTooLong, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(2, problems.Count);
            problem = problems.Last();
            Assert.AreEqual(LDTexmap.Problem_TextureTooLong, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsGlossmapTooLongTest()
        {
            LDTexmap target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 indy_face.png GLOSSMAP abcdefghijklmnopqrstuvwxyz.png");
            target.SharedGeometry.Add(new LDLine());
            Assert.IsFalse(target.IsTextureMissing);
            Assert.IsTrue(target.IsGlossmapMissing);
            Assert.IsFalse(target.IsGeometryMissing);
            Assert.IsFalse(target.IsTextureTooLong);
            Assert.IsFalse(target.IsTextureInvalidChars);
            Assert.IsFalse(target.IsGlossmapInvalidChars);
            Assert.IsTrue(target.IsGlossmapTooLong);

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);     // file-not-found

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(2, problems.Count);
            problem = problems.Last();
            Assert.AreEqual(LDTexmap.Problem_GlossmapTooLong, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(2, problems.Count);
            problem = problems.Last();
            Assert.AreEqual(LDTexmap.Problem_GlossmapTooLong, problem.Guid);
            Assert.AreEqual(target, problem.Element);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        #endregion Analytics

        #region Constructor

        [TestMethod]
        public void LDTexmapConstructorTest()
        {
            LDTexmap target = new LDTexmap();

            Assert.AreEqual(DOMObjectType.Texmap, target.ObjectType);
            Assert.IsNotNull(target.Icon);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.Texmap, target.TypeName);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.Undefined, target.Description);
            Assert.AreEqual(String.Empty, target.ExtendedDescription);
            Assert.AreEqual(0, target.Count);
            Assert.IsTrue(target.IsReadOnly);
            Assert.IsFalse(target.ColourValueEnabled);
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(3, target.Coordinates.Count());
            Assert.AreEqual(Vector3d.Zero, target.Point1);
            Assert.AreEqual(Vector3d.Zero, target.Point2);
            Assert.AreEqual(Vector3d.Zero, target.Point3);
            Assert.AreEqual(360.0, target.HorizontalExtent);
            Assert.AreEqual(360.0, target.VerticalExtent);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.Undefined, target.Texture);
            Assert.IsNull(target.TexturePath);
            Assert.IsNull(target.Glossmap);
            Assert.IsNull(target.GlossmapPath);

            Assert.IsNotNull(target.TextureGeometry);
            Assert.AreEqual(0, target.TextureGeometry.Count);

            //Assert.IsFalse(target.TextureGeometry.IsReadOnly);
            Assert.IsNotNull(target.TextureGeometry.Icon);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.Geometry_Texture, target.TextureGeometry.TypeName);
            Assert.AreEqual(String.Empty, target.TextureGeometry.Description);
            Assert.AreEqual(String.Empty, target.TextureGeometry.ExtendedDescription);

            Assert.IsNotNull(target.SharedGeometry);
            Assert.AreEqual(0, target.SharedGeometry.Count);

            //Assert.IsFalse(target.SharedGeometry.IsReadOnly);
            Assert.IsNotNull(target.SharedGeometry.Icon);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.Geometry_Shared, target.SharedGeometry.TypeName);
            Assert.AreEqual(String.Empty, target.SharedGeometry.Description);
            Assert.AreEqual(String.Empty, target.SharedGeometry.ExtendedDescription);

            Assert.IsNotNull(target.FallbackGeometry);
            Assert.AreEqual(0, target.FallbackGeometry.Count);

            //Assert.IsFalse(target.FallbackGeometry.IsReadOnly);
            Assert.IsNotNull(target.FallbackGeometry.Icon);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.Geometry_Fallback, target.FallbackGeometry.TypeName);
            Assert.AreEqual(String.Empty, target.FallbackGeometry.Description);
            Assert.AreEqual(String.Empty, target.FallbackGeometry.ExtendedDescription);
        }

        [TestMethod]
        public void LDTexmapConstructorTest1()
        {
            LDTexmap target;

            target = new LDTexmap("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png GLOSSMAP glossmap.png");
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual("texture.png", target.Texture);
            Assert.AreEqual("glossmap.png", target.Glossmap);

            target = new LDTexmap("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 \"long texture with no glossmap name.png\" GLOSSMAP \"long glossmap name.png\"");
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual("long texture with no glossmap name.png", target.Texture);
            Assert.AreEqual("long glossmap name.png", target.Glossmap);

            target = new LDTexmap("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png");
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual("texture.png", target.Texture);
            Assert.IsNull(target.Glossmap);

            target = new LDTexmap("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 \"long texture name.png\"");
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual("long texture name.png", target.Texture);
            Assert.IsNull(target.Glossmap);

            target = new LDTexmap("0 !TEXMAP START CYLINDRICAL 1 2 3 4 5 6 7 8 9 90.0 texture.png");
            Assert.AreEqual(TexmapProjection.Cylindrical, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual(90.0, target.HorizontalExtent);
            Assert.AreEqual("texture.png", target.Texture);
            Assert.IsNull(target.Glossmap);

            target = new LDTexmap("0 !TEXMAP START SPHERICAL 1 2 3 4 5 6 7 8 9 90.0 45.0 texture.png");
            Assert.AreEqual(TexmapProjection.Spherical, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual(90.0, target.HorizontalExtent);
            Assert.AreEqual(45.0, target.VerticalExtent);
            Assert.AreEqual("texture.png", target.Texture);
            Assert.IsNull(target.Glossmap);

            target = new LDTexmap(@"0 !TEXMAP NEXT PLANAR 1 2 3 4 5 6 7 8 9 s\texture.png");
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual(@"s\texture.png", target.Texture);
            Assert.IsNull(target.Glossmap);
        }

        #endregion Constructor

        #region Parser

        [TestMethod]
        public void ParserTest()
        {
            // TODO: LOCKNEXT, LOCKGEOM

            // test the LDPage parser loads up geometry correctly
            IDocument doc;
            IPage page;
            IStep step;
            ITexmap target;
            IDOMObject element;
            bool documentModified;

            // 1. simple geom-1 texmap, no fallback
            string code = "0 Title\r\n" +
                          "0 Name: name.dat\r\n" +
                          "0 !LDRAW_ORG Part\r\n" +
                          "\r\n" +
                          "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                          "0 !: 0 BFC INVERTNEXT\r\n" +
                          "0 !: 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                          "0 !: 2 24 0 0 0 1 1 1\r\n" +
                          "0 !: 0 // comment\r\n" +
                          "0 !TEXMAP END\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(1, page.Count);
            step = page[0] as LDStep;
            target = step[0] as LDTexmap;
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual("texture.png", target.Texture);
            Assert.AreEqual(3, target.TextureGeometry.Count);
            element = target.TextureGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.IsTrue((element as LDReference).Invert);
            element = target.TextureGeometry[1];
            Assert.IsInstanceOfType(element, typeof(LDLine));
            element = target.TextureGeometry[2];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual(0, target.SharedGeometry.Count);
            Assert.AreEqual(0, target.FallbackGeometry.Count);

            // 2. simple geom-1 texmap with fallback
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 !: 0 BFC INVERTNEXT\r\n" +
                   "0 !: 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "0 !: 2 24 0 0 0 1 1 1\r\n" +
                   "0 !: 0 // comment\r\n" +
                   "0 !TEXMAP FALLBACK\r\n" +
                   "1 16 0 0 0 1 0 0 0 0 1 0 0 1 3002.dat\r\n" +
                   "0 !TEXMAP END\r\n" +
                   "0 // comment\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(2, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            Assert.IsInstanceOfType(step[1], typeof(LDComment));
            target = step[0] as LDTexmap;
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual("texture.png", target.Texture);
            Assert.AreEqual(3, target.TextureGeometry.Count);
            element = target.TextureGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.IsTrue((element as LDReference).Invert);
            Assert.AreEqual("3001.dat", (element as LDReference).TargetName);
            element = target.TextureGeometry[1];
            Assert.IsInstanceOfType(element, typeof(LDLine));
            element = target.TextureGeometry[2];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual(0, target.SharedGeometry.Count);
            Assert.AreEqual(1, target.FallbackGeometry.Count);
            element = target.FallbackGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.AreEqual("3002.dat", (element as LDReference).TargetName);

            // 3. simple geom-2 texmap
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 BFC INVERTNEXT\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "2 24 0 0 0 1 1 1\r\n" +
                   "0 // comment\r\n" +
                   "0 !TEXMAP END\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            target = step[0] as LDTexmap;
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual("texture.png", target.Texture);
            Assert.AreEqual(0, target.TextureGeometry.Count);
            Assert.AreEqual(3, target.SharedGeometry.Count);
            element = target.SharedGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.IsTrue((element as LDReference).Invert);
            Assert.AreEqual("3001.dat", (element as LDReference).TargetName);
            element = target.SharedGeometry[1];
            Assert.IsInstanceOfType(element, typeof(LDLine));
            element = target.SharedGeometry[2];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual(0, target.FallbackGeometry.Count);

            // 4. single-line texmap
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP NEXT PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            target = step[0] as LDTexmap;
            Assert.AreEqual(TexmapProjection.Planar, target.Projection);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Point1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Point2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Point3);
            Assert.AreEqual("texture.png", target.Texture);
            Assert.AreEqual(0, target.TextureGeometry.Count);
            Assert.AreEqual(1, target.SharedGeometry.Count);
            element = target.SharedGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.AreEqual("3001.dat", (element as LDReference).TargetName);
            Assert.AreEqual(0, target.FallbackGeometry.Count);

            // 5. single-line texmap with invalid content
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP NEXT PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 BFC INVERTNEXT\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(2, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDComment));
            Assert.AreEqual("// !TEXMAP NEXT PLANAR 1 2 3 4 5 6 7 8 9 texture.png", (step[0] as LDComment).Text);
            Assert.IsInstanceOfType(step[1], typeof(LDReference));
            Assert.IsTrue((step[1] as LDReference).Invert);

            // 5. implicit end-of-block (STEP)
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 BFC INVERTNEXT\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "0 STEP\r\n" +
                   "2 24 0 0 0 1 1 1\r\n" +
                   "0 // comment\r\n" +
                   "0 !TEXMAP END\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            target = step[0] as LDTexmap;
            Assert.AreEqual(0, target.TextureGeometry.Count);
            Assert.AreEqual(1, target.SharedGeometry.Count);
            element = target.SharedGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.AreEqual(0, target.FallbackGeometry.Count);
            step = page[1] as LDStep;
            Assert.AreEqual(3, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDLine));
            Assert.IsInstanceOfType(step[1], typeof(LDComment));
            Assert.IsInstanceOfType(step[2], typeof(LDComment));
            Assert.AreEqual("// !TEXMAP END", (step[2] as LDComment).Text);

            // 5. implicit end-of-block (NOFILE)
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 BFC INVERTNEXT\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "0 NOFILE\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            target = step[0] as LDTexmap;
            Assert.AreEqual(0, target.TextureGeometry.Count);
            Assert.AreEqual(1, target.SharedGeometry.Count);
            element = target.SharedGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.AreEqual(0, target.FallbackGeometry.Count);

            // 6. implicit end-of-block (eof)
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 BFC INVERTNEXT\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            target = step[0] as LDTexmap;
            Assert.AreEqual(0, target.TextureGeometry.Count);
            Assert.AreEqual(1, target.SharedGeometry.Count);
            element = target.SharedGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.AreEqual(0, target.FallbackGeometry.Count);

            // 7. implicit end-of-block (TEXMAP START)
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 BFC INVERTNEXT\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "2 24 0 0 0 1 1 1\r\n" +
                   "0 // comment\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 !: 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3002.dat\r\n" +
                   "0 !TEXMAP END\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(2, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            target = step[0] as LDTexmap;
            Assert.AreEqual(0, target.TextureGeometry.Count);
            Assert.AreEqual(3, target.SharedGeometry.Count);
            element = target.SharedGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.AreEqual("3001.dat", (element as LDReference).TargetName);
            element = target.SharedGeometry[1];
            Assert.IsInstanceOfType(element, typeof(LDLine));
            element = target.SharedGeometry[2];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual(0, target.FallbackGeometry.Count);
            Assert.IsInstanceOfType(step[1], typeof(LDTexmap));
            target = step[1] as LDTexmap;
            Assert.AreEqual(1, target.TextureGeometry.Count);
            element = target.TextureGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.AreEqual("3002.dat", (element as LDReference).TargetName);
            Assert.AreEqual(0, target.SharedGeometry.Count);
            Assert.AreEqual(0, target.FallbackGeometry.Count);

            // 8. implicit end-of-block (TEXMAP NEXT)
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 BFC INVERTNEXT\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "2 24 0 0 0 1 1 1\r\n" +
                   "0 // comment\r\n" +
                   "0 !TEXMAP NEXT PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3002.dat\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(2, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            target = step[0] as LDTexmap;
            Assert.AreEqual(0, target.TextureGeometry.Count);
            Assert.AreEqual(3, target.SharedGeometry.Count);
            element = target.SharedGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.AreEqual("3001.dat", (element as LDReference).TargetName);
            element = target.SharedGeometry[1];
            Assert.IsInstanceOfType(element, typeof(LDLine));
            element = target.SharedGeometry[2];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual(0, target.FallbackGeometry.Count);
            Assert.IsInstanceOfType(step[1], typeof(LDTexmap));
            target = step[1] as LDTexmap;
            Assert.AreEqual(0, target.TextureGeometry.Count);
            Assert.AreEqual(1, target.SharedGeometry.Count);
            element = target.SharedGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            Assert.AreEqual("3002.dat", (element as LDReference).TargetName);
            Assert.AreEqual(0, target.FallbackGeometry.Count);

            // 9. orphaned meta-commands
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP END\r\n" +
                   "0 !TEXMAP FALLBACK\r\n" +
                   "0 !: 0 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(3, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDComment));
            Assert.IsInstanceOfType(step[1], typeof(LDComment));
            Assert.IsInstanceOfType(step[2], typeof(LDComment));
            Assert.AreEqual("// !TEXMAP END", (step[0] as LDComment).Text);
            Assert.AreEqual("// !TEXMAP FALLBACK", (step[1] as LDComment).Text);
            Assert.AreEqual("// !: 0 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat", (step[2] as LDComment).Text);

            // 10. MLCadGroup code inside a texmap (geom1) : not supported, so the group meta-commands should become comments
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 !: 0 MLCAD BTG group\r\n" +
                   "0 !: 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "0 !: 0 GROUP 1 group\r\n" +
                   "0 !TEXMAP END\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            target = step[0] as LDTexmap;
            Assert.AreEqual(3, target.TextureGeometry.Count);
            element = target.TextureGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual("// MLCAD BTG group", (element as LDComment).Text);
            element = target.TextureGeometry[1];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            element = target.TextureGeometry[2];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual("// GROUP 1 group", (element as LDComment).Text);

            // 11. MLCadGroup code inside a texmap (geom2) : not supported, so the group meta-commands should become comments
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 MLCAD BTG group\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "0 GROUP 1 group\r\n" +
                   "0 !TEXMAP END\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            target = step[0] as LDTexmap;
            Assert.AreEqual(0, target.TextureGeometry.Count);
            Assert.AreEqual(3, target.SharedGeometry.Count);
            element = target.SharedGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual("// MLCAD BTG group", (element as LDComment).Text);
            element = target.SharedGeometry[1];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            element = target.SharedGeometry[2];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual("// GROUP 1 group", (element as LDComment).Text);

            // 12. MLCadGroup code inside a texmap (geom3) : not supported, so the group meta-commands should become comments
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 !: 0 // comment\r\n" +
                   "0 !TEXMAP FALLBACK\r\n" +
                   "0 MLCAD BTG group\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "0 GROUP 1 group\r\n" +
                   "0 !TEXMAP END\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            target = step[0] as LDTexmap;
            Assert.AreEqual(1, target.TextureGeometry.Count);
            Assert.IsInstanceOfType(target.TextureGeometry[0], typeof(LDComment));
            Assert.AreEqual(0, target.SharedGeometry.Count);
            Assert.AreEqual(3, target.FallbackGeometry.Count);
            element = target.FallbackGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual("// MLCAD BTG group", (element as LDComment).Text);
            element = target.FallbackGeometry[1];
            Assert.IsInstanceOfType(element, typeof(LDReference));
            element = target.FallbackGeometry[2];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual("// GROUP 1 group", (element as LDComment).Text);

            // 13. texmap inside an MLCadGroup
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 MLCAD BTG group\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 MLCAD BTG group\r\n" +
                   "0 !: 0 // comment\r\n" +
                   "0 MLCAD BTG group\r\n" +
                   "0 !TEXMAP FALLBACK\r\n" +
                   "0 MLCAD BTG group\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "0 MLCAD BTG group\r\n" +
                   "0 !TEXMAP END\r\n" +
                   "\r\n" +
                   "0 GROUP 1 group\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            step = page[0] as LDStep;
            Assert.AreEqual(2, step.Count);
            Assert.IsInstanceOfType(step[0], typeof(LDTexmap));
            Assert.IsInstanceOfType(step[1], typeof(MLCadGroup));
            Assert.AreEqual(1, (step[1] as MLCadGroup).Count);
            target = (step[1] as MLCadGroup).ElementAt(0) as LDTexmap;
            Assert.AreEqual(1, target.TextureGeometry.Count);
            element = target.TextureGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDComment));
            Assert.AreEqual(0, target.SharedGeometry.Count);
            Assert.AreEqual(1, target.FallbackGeometry.Count);
            element = target.FallbackGeometry[0];
            Assert.IsInstanceOfType(element, typeof(LDReference));

            // 14. circular-dependency from inside texmap
            code = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Part\r\n" +
                   "\r\n" +
                   "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                   "0 !: 0 // comment\r\n" +
                   "0 !TEXMAP FALLBACK\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 name.dat\r\n" +
                   "0 !TEXMAP END\r\n";

            try
            {
                doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
                Assert.Fail();
            }
            catch (CircularReferenceException)
            {
            }
        }

        #endregion Parser
    }
}
