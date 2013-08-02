#region License

//
// LDQuadrilateralTest.cs
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
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    [TestClass]
    public sealed class LDQuadrilateralTest : IQuadrilateralTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDQuadrilateral); } }

        protected override IQuadrilateral CreateTestQuadrilateral()
        {
            return new LDQuadrilateral();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IQuadrilateral quadrilateral = CreateTestQuadrilateral())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(quadrilateral.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.Quadrilateral, typeNameAttr.Description);
                Assert.AreEqual(Resources.Quadrilateral, quadrilateral.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(quadrilateral.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNotNull(elementFlagsAttr);
                Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

                Assert.AreEqual(String.Empty, quadrilateral.ExtendedDescription);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public void AnalyseValidQuadTest()
        {
            ICollection<IProblemDescriptor> problems;
            LDQuadrilateral target = new LDQuadrilateral("4 16 0 0 0 10 0 0 10 10 0 0 10 0");

            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColinear);
            Assert.IsFalse(target.IsBowtie);
            Assert.IsFalse(target.IsConcave);
            Assert.IsFalse(target.IsWarped);
            Assert.AreEqual(0.0, target.Warp);

            Assert.IsFalse(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);
        }

        [TestMethod]
        public void AnalyseColocatedQuadTest()
        {
            LDQuadrilateral target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;
            LDStep step = new LDStep();

            target = new LDQuadrilateral("4 16 0 0 0 0 0 0 10 10 0 0 10 0");
            step.Add(target);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsTrue(target.IsColocated);
            Assert.IsTrue(target.IsColinear);
            Assert.IsFalse(target.IsBowtie);
            Assert.IsFalse(target.IsConcave);
            Assert.IsFalse(target.IsWarped);
            Assert.AreEqual(0.0, target.Warp);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDQuadrilateral.Fix_CoordinatesColocated_DeleteQuadrilateral, fix.Guid);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsFalse(fix.IsIntraElement);
            LDPage page = new LDPage();
            page.Add(step);
            Assert.IsTrue(step.Contains(target));
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(step.Contains(target));
        }

        [TestMethod]
        public void AnalyseQuadDegeneratedToTriangleTest()
        {
            LDQuadrilateral target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;
            LDStep step = new LDStep();

            target = new LDQuadrilateral("4 16 0 0 0 10 0 0 20 0 0 0 10 0");
            step.Add(target);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);
            Assert.IsTrue(target.IsColinear);
            Assert.IsFalse(target.IsBowtie);
            Assert.IsFalse(target.IsConcave);
            Assert.IsFalse(target.IsWarped);
            Assert.AreEqual(0.0, target.Warp);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_VerticesColinear, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_VerticesColinear, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_VerticesColinear, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDQuadrilateral.Fix_VerticesColinear_DeleteQuadrilateral, fix.Guid);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsFalse(fix.IsIntraElement);
            LDPage page = new LDPage();
            page.Add(step);
            Assert.IsTrue(step.Contains(target));
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(step.Contains(target));
        }

        [TestMethod]
        public void AnalyseQuadDegeneratedToLineTest()
        {
            LDQuadrilateral target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;
            LDStep step = new LDStep();

            target = new LDQuadrilateral("4 16 0 0 0 10 0 0 20 0 0 30 0 0");
            step.Add(target);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);
            Assert.IsTrue(target.IsColinear);
            Assert.IsFalse(target.IsBowtie);
            Assert.IsFalse(target.IsConcave);
            Assert.IsFalse(target.IsWarped);
            Assert.AreEqual(0.0, target.Warp);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_VerticesColinear, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_VerticesColinear, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_VerticesColinear, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDQuadrilateral.Fix_VerticesColinear_DeleteQuadrilateral, fix.Guid);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsFalse(fix.IsIntraElement);
            LDPage page = new LDPage();
            page.Add(step);
            Assert.IsTrue(step.Contains(target));
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(step.Contains(target));
        }

        [TestMethod]
        public void AnalyseBowtiedQuadTest()
        {
            LDQuadrilateral target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDQuadrilateral("4 16 0 0 0 10 10 0 10 0 0 0 10 0");
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColinear);
            Assert.IsTrue(target.IsBowtie);
            Assert.IsFalse(target.IsConcave);
            Assert.IsFalse(target.IsWarped);
            Assert.AreEqual(0.0, target.Warp);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Bowtie, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Bowtie, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Bowtie, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDQuadrilateral.Fix_Bowtie, fix.Guid);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.IsBowtie);
        }

        [TestMethod]
        public void AnalyseConcaveQuadTest()
        {
            LDQuadrilateral target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDQuadrilateral("4 16 0 0 0 10 10 0 20 0 0 10 5 0");
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColinear);
            Assert.IsFalse(target.IsBowtie);
            Assert.IsTrue(target.IsConcave);
            Assert.IsFalse(target.IsWarped);
            Assert.AreEqual(0.0, target.Warp);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Concave, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Concave, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Concave, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyseMaxWarpedQuadTest()
        {
            LDQuadrilateral target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDQuadrilateral("4 16 0 0 0 10 0 0 10 10 0 0 10 10");
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColinear);
            Assert.IsFalse(target.IsBowtie);
            Assert.IsFalse(target.IsConcave);
            Assert.IsTrue(target.IsWarped);
            Assert.AreNotEqual(0.0, target.Warp);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Warped, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Warped, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Warped, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyseMedWarpedQuadTest()
        {
            LDQuadrilateral target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDQuadrilateral("4 16 0 0 0 10 0 0 10 10 0 0 10 .25");
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColinear);
            Assert.IsFalse(target.IsBowtie);
            Assert.IsFalse(target.IsConcave);
            Assert.IsTrue(target.IsWarped);
            Assert.AreNotEqual(0.0, target.Warp);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Warped, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Warped, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Warped, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyseMinWarpedQuadTest()
        {
            LDQuadrilateral target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDQuadrilateral("4 16 0 0 0 10 0 0 10 10 0 0 10 0.1");
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColinear);
            Assert.IsFalse(target.IsBowtie);
            Assert.IsFalse(target.IsConcave);
            Assert.IsTrue(target.IsWarped);
            Assert.AreNotEqual(0.0, target.Warp);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Warped, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Warped, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_Warped, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyseInvalidColourTest()
        {
            LDQuadrilateral target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDQuadrilateral("4 24 0 0 0 10 0 0 10 10 0 0 10 0");
            Assert.IsTrue(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColinear);
            Assert.IsFalse(target.IsBowtie);
            Assert.IsFalse(target.IsConcave);
            Assert.IsFalse(target.IsWarped);
            Assert.AreEqual(0.0, target.Warp);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDQuadrilateral.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDQuadrilateral.Fix_ColourInvalid_SetToMainColour, fix.Guid);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.AreNotEqual(Palette.MainColour, target.ColourValue);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(Palette.MainColour, target.ColourValue);
        }

        #endregion Analytics

        #region Constructor

        [TestMethod]
        public void LDQuadrilateralConstructorTest()
        {
            uint colour = 1;
            IEnumerable<Vector3d> vertices = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ, Vector3d.Zero };
            LDQuadrilateral target = new LDQuadrilateral(colour, vertices);

            Assert.AreEqual(DOMObjectType.Quadrilateral, target.ObjectType);
            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(Vector3d.UnitX, target.Vertex1);
            Assert.AreEqual(Vector3d.UnitY, target.Vertex2);
            Assert.AreEqual(Vector3d.UnitZ, target.Vertex3);
            Assert.AreEqual(Vector3d.Zero, target.Vertex4);
        }

        [TestMethod]
        public void LDQuadrilateralConstructorTest1()
        {
            LDQuadrilateral target = new LDQuadrilateral();
            Assert.AreEqual(Palette.MainColour, target.ColourValue);
            Assert.AreEqual(Vector3d.Zero, target.Vertex1);
            Assert.AreEqual(Vector3d.Zero, target.Vertex2);
            Assert.AreEqual(Vector3d.Zero, target.Vertex3);
            Assert.AreEqual(Vector3d.Zero, target.Vertex4);
        }

        [TestMethod]
        public void LDQuadrilateralConstructorTest2()
        {
            uint colour = 1;
            Vector3d vertex1 = Vector3d.UnitX;
            Vector3d vertex2 = Vector3d.UnitY;
            Vector3d vertex3 = Vector3d.UnitZ;
            Vector3d vertex4 = Vector3d.Zero;
            LDQuadrilateral target = new LDQuadrilateral(colour, vertex1, vertex2, vertex3, vertex4);

            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(vertex1, target.Vertex1);
            Assert.AreEqual(vertex2, target.Vertex2);
            Assert.AreEqual(vertex3, target.Vertex3);
            Assert.AreEqual(vertex4, target.Vertex4);
        }

        [TestMethod]
        public void LDQuadrilateralConstructorTest3()
        {
            uint colour = 1;
            Vector3d vertex1 = Vector3d.UnitX;
            Vector3d vertex2 = Vector3d.UnitY;
            Vector3d vertex3 = Vector3d.UnitZ;
            Vector3d vertex4 = Vector3d.Zero;
            LDQuadrilateral target = new LDQuadrilateral(colour, ref vertex1, ref vertex2, ref vertex3, ref vertex4);

            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(vertex1, target.Vertex1);
            Assert.AreEqual(vertex2, target.Vertex2);
            Assert.AreEqual(vertex3, target.Vertex3);
            Assert.AreEqual(vertex4, target.Vertex4);
        }

        [TestMethod]
        public void LDQuadrilateralConstructorTest4()
        {
            LDQuadrilateral target = new LDQuadrilateral("4 16 0 0 0 10 0 0 10 10 0 0 10 0");

            Assert.AreEqual(16U, target.ColourValue);
            Assert.AreEqual(new Vector3d(0, 0, 0), target.Vertex1);
            Assert.AreEqual(new Vector3d(10, 0, 0), target.Vertex2);
            Assert.AreEqual(new Vector3d(10, 10, 0), target.Vertex3);
            Assert.AreEqual(new Vector3d(0, 10, 0), target.Vertex4);
        }

        #endregion Constructor
    }
}
