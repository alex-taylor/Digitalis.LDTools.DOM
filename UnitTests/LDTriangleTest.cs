#region License

//
// LDTriangleTest.cs
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
    public sealed class LDTriangleTest : ITriangleTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDTriangle); } }

        protected override ITriangle CreateTestTriangle()
        {
            return new LDTriangle();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (ITriangle triangle = CreateTestTriangle())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(triangle.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.Triangle, typeNameAttr.Description);
                Assert.AreEqual(Resources.Triangle, triangle.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(triangle.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNotNull(elementFlagsAttr);
                Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

                Assert.AreEqual(String.Empty, triangle.ExtendedDescription);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public void AnalyseValidTriangleTest()
        {
            ICollection<IProblemDescriptor> problems;
            LDTriangle target = new LDTriangle("3 16 1 0 0 2 0 0 1 1 0");

            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColinear);
            Assert.IsFalse(target.IsColourInvalid);

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
        public void AnalyseInvalidColourTest()
        {
            ICollection<IProblemDescriptor> problems;
            LDTriangle target;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDTriangle("3 24 1 0 0 2 0 0 1 1 0");
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColinear);
            Assert.IsTrue(target.IsColourInvalid);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDTriangle.Fix_ColourInvalid_SetToMainColour, fix.Guid);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.AreNotEqual(Palette.MainColour, target.ColourValue);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(Palette.MainColour, target.ColourValue);
        }

        [TestMethod]
        public void AnalyseColocatedTriangleTest()
        {
            ICollection<IProblemDescriptor> problems;
            LDTriangle target;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;
            LDStep step = new LDStep();

            target = new LDTriangle("3 16 1 0 0 1 0 0 1 1 0");
            step.Add(target);
            Assert.IsTrue(target.IsColocated);
            Assert.IsTrue(target.IsColinear);
            Assert.IsFalse(target.IsColourInvalid);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDTriangle.Fix_CoordinatesColocated_DeleteTriangle, fix.Guid);
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
        public void AnalyseColinearTriangleTest()
        {
            ICollection<IProblemDescriptor> problems;
            LDTriangle target;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;
            LDStep step = new LDStep();

            target = new LDTriangle("3 16 1 0 0 2 0 0 3 0 0");
            step.Add(target);
            Assert.IsFalse(target.IsColocated);
            Assert.IsTrue(target.IsColinear);
            Assert.IsFalse(target.IsColourInvalid);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_VerticesColinear, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_VerticesColinear, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_VerticesColinear, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDTriangle.Fix_VerticesColinear_DeleteTriangle, fix.Guid);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsFalse(fix.IsIntraElement);
            LDPage page = new LDPage();
            page.Add(step);
            Assert.IsTrue(step.Contains(target));
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(step.Contains(target));
        }

        #endregion Analytics

        #region Constructor

        [TestMethod]
        public void LDTriangleConstructorTest()
        {
            uint colour = 1;
            IEnumerable<Vector3d> vertices = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            LDTriangle target = new LDTriangle(colour, vertices);
            Assert.AreEqual(DOMObjectType.Triangle, target.ObjectType);
            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(Vector3d.UnitX, target.Vertex1);
            Assert.AreEqual(Vector3d.UnitY, target.Vertex2);
            Assert.AreEqual(Vector3d.UnitZ, target.Vertex3);
        }

        [TestMethod]
        public void LDTriangleConstructorTest1()
        {
            LDTriangle target = new LDTriangle();
            Assert.AreEqual(Palette.MainColour, target.ColourValue);
            Assert.AreEqual(Vector3d.Zero, target.Vertex1);
            Assert.AreEqual(Vector3d.Zero, target.Vertex2);
            Assert.AreEqual(Vector3d.Zero, target.Vertex3);
        }

        [TestMethod]
        public void LDTriangleConstructorTest2()
        {
            uint colour = 1;
            Vector3d vertex1 = Vector3d.UnitX;
            Vector3d vertex2 = Vector3d.UnitY;
            Vector3d vertex3 = Vector3d.UnitZ;
            LDTriangle target = new LDTriangle(colour, ref vertex1, ref vertex2, ref vertex3);

            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(vertex1, target.Vertex1);
            Assert.AreEqual(vertex2, target.Vertex2);
            Assert.AreEqual(vertex3, target.Vertex3);
        }

        [TestMethod]
        public void LDTriangleConstructorTest3()
        {
            uint colour = 1;
            Vector3d vertex1 = Vector3d.UnitX;
            Vector3d vertex2 = Vector3d.UnitY;
            Vector3d vertex3 = Vector3d.UnitZ;
            LDTriangle target = new LDTriangle(colour, vertex1, vertex2, vertex3);

            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(vertex1, target.Vertex1);
            Assert.AreEqual(vertex2, target.Vertex2);
            Assert.AreEqual(vertex3, target.Vertex3);
        }

        [TestMethod]
        public void LDTriangleConstructorTest4()
        {
            LDTriangle target = new LDTriangle("3 16 1 2 3 4 5 6 7 8 9");
            Assert.AreEqual(16U, target.ColourValue);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Vertex1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Vertex2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Vertex3);
        }

        #endregion Constructor
    }
}
