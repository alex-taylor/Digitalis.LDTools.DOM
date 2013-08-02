#region License

//
// LDLineTest.cs
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
    public sealed class LDLineTest : ILineTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDLine); } }

        protected override ILine CreateTestLine()
        {
            return new LDLine();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (ILine line = CreateTestLine())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(line.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.Line, typeNameAttr.Description);
                Assert.AreEqual(Resources.Line, line.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(line.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNotNull(elementFlagsAttr);
                Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

                Assert.AreEqual(String.Empty, line.ExtendedDescription);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public void AnalyseValidLineTest()
        {
            ICollection<IProblemDescriptor> problems;

            LDLine target = new LDLine("2 1 1 0 0 2 0 0");
            Assert.IsFalse(target.IsColocated);
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
        public void AnalyseColocatedLineTest()
        {
            LDLine target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;
            LDStep step = new LDStep();

            target = new LDLine("2 1 1 0 0 1 0 0");
            step.Add(target);
            Assert.IsTrue(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDLine.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDLine.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDLine.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDLine.Fix_CoordinatesColocated_DeleteLine, fix.Guid);
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
        public void AnalyseInvalidColourTest()
        {
            LDLine target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDLine("2 16 1 0 0 2 0 0");
            Assert.IsTrue(target.IsColourInvalid);
            Assert.IsFalse(target.IsColocated);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDTriangle.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDLine.Fix_ColourInvalid_SetToEdgeColour, fix.Guid);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.AreNotEqual(Palette.EdgeColour, target.ColourValue);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(Palette.EdgeColour, target.ColourValue);
        }

        #endregion Analytics

        #region Constructor

        [TestMethod]
        public void LDLineConstructorTest()
        {
            LDLine target = new LDLine();
            Assert.AreEqual(DOMObjectType.Line, target.ObjectType);
            Assert.AreEqual(Palette.EdgeColour, target.ColourValue);
            Assert.AreEqual(Vector3d.Zero, target.Vertex1);
            Assert.AreEqual(Vector3d.Zero, target.Vertex2);
        }

        [TestMethod]
        public void LDLineConstructorTest1()
        {
            uint colour = 1;
            IEnumerable<Vector3d> vertices = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY };
            LDLine target = new LDLine(colour, vertices);

            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(Vector3d.UnitX, target.Vertex1);
            Assert.AreEqual(Vector3d.UnitY, target.Vertex2);
        }

        [TestMethod]
        public void LDLineConstructorTest2()
        {
            uint colour = 1;
            Vector3d vertex1 = Vector3d.UnitX;
            Vector3d vertex2 = Vector3d.UnitY;
            LDLine target = new LDLine(colour, ref vertex1, ref vertex2);

            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(vertex1, target.Vertex1);
            Assert.AreEqual(vertex2, target.Vertex2);
        }

        [TestMethod]
        public void LDLineConstructorTest3()
        {
            uint colour = 1;
            Vector3d vertex1 = Vector3d.UnitX;
            Vector3d vertex2 = Vector3d.UnitY;
            LDLine target = new LDLine(colour, vertex1, vertex2);

            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(vertex1, target.Vertex1);
            Assert.AreEqual(vertex2, target.Vertex2);
        }

        [TestMethod]
        public void LDLineConstructorTest4()
        {
            string code = "2 1 1 0 0 2 0 0";
            LDLine target = new LDLine(code);

            Assert.AreEqual(1U, target.ColourValue);
            Assert.AreEqual(new Vector3d(1, 0, 0), target.Vertex1);
            Assert.AreEqual(new Vector3d(2, 0, 0), target.Vertex2);

            target = new LDLine("2 0x2FF00FF 1 0 0 2 0 0");
            Assert.AreEqual(0x2FF00FFU, target.ColourValue);
        }

        #endregion Constructor
    }
}
