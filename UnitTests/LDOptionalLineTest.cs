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
    public sealed class LDOptionalLineTest : IOptionalLineTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDOptionalLine); } }

        protected override IOptionalLine CreateTestOptionalLine()
        {
            return new LDOptionalLine();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IOptionalLine line = CreateTestOptionalLine())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(line.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.OptionalLine, typeNameAttr.Description);
                Assert.AreEqual(Resources.OptionalLine, line.TypeName);

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

        /// <summary>
        ///A test for Analyse
        ///</summary>
        [TestMethod]
        public void AnalyseValidOptionalLineTest()
        {
            ICollection<IProblemDescriptor> problems;

            LDOptionalLine target = new LDOptionalLine("5 24 1 2 3 4 5 6 7 8 9 10 11 12");
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
        public void AnalyseColocatedVerticesOptionalLineTest()
        {
            LDOptionalLine target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;
            LDStep step = new LDStep();

            target = new LDOptionalLine("5 24 1 2 3 1 2 3 7 8 9 10 11 12");
            step.Add(target);
            Assert.IsTrue(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDOptionalLine.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDOptionalLine.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDOptionalLine.Problem_CoordinatesColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDOptionalLine.Fix_CoordinatesColocated_DeleteLine, fix.Guid);
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
        public void AnalyseColocatedControlPointsOptionalLineTest()
        {
            LDOptionalLine target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDOptionalLine("5 24 1 2 3 4 5 6 7 8 9 7 8 9");
            Assert.IsTrue(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDOptionalLine.Problem_ControlPointsColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDOptionalLine.Problem_ControlPointsColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDOptionalLine.Problem_ControlPointsColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyseColocatedVerticesAndControlPointsOptionalLineTest()
        {
            LDOptionalLine target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDOptionalLine("5 24 1 2 3 4 5 6 1 2 3 10 11 12");
            Assert.IsTrue(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDOptionalLine.Problem_VerticesAndControlPointsColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDOptionalLine.Problem_VerticesAndControlPointsColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDOptionalLine.Problem_VerticesAndControlPointsColocated, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyseInvalidColourOptionalLineTest()
        {
            LDOptionalLine target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            // bad colour
            target = new LDOptionalLine("5 16 1 2 3 4 5 6 7 8 9 10 11 12");
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
            Assert.AreEqual(LDOptionalLine.Fix_ColourInvalid_SetToEdgeColour, fix.Guid);
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
        public void LDOptionalLineConstructorTest()
        {
            uint colour = 1;
            Vector3d vertex1 = Vector3d.UnitX;
            Vector3d vertex2 = Vector3d.UnitY;
            Vector3d control1 = Vector3d.UnitZ;
            Vector3d control2 = Vector3d.Zero;
            LDOptionalLine target = new LDOptionalLine(colour, vertex1, vertex2, control1, control2);
            Assert.AreEqual(DOMObjectType.OptionalLine, target.ObjectType);
            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(vertex1, target.Vertex1);
            Assert.AreEqual(vertex2, target.Vertex2);
            Assert.AreEqual(control1, target.Control1);
            Assert.AreEqual(control2, target.Control2);
        }

        [TestMethod]
        public void LDOptionalLineConstructorTest1()
        {
            LDOptionalLine target = new LDOptionalLine("5 24 1 2 3 4 5 6 7 8 9 10 11 12");

            Assert.AreEqual(24U, target.ColourValue);
            Assert.AreEqual(new Vector3d(1, 2, 3), target.Vertex1);
            Assert.AreEqual(new Vector3d(4, 5, 6), target.Vertex2);
            Assert.AreEqual(new Vector3d(7, 8, 9), target.Control1);
            Assert.AreEqual(new Vector3d(10, 11, 12), target.Control2);

            target = new LDOptionalLine("5 #2FF00FF 1 2 3 4 5 6 7 8 9 10 11 12");
            Assert.AreEqual(0x2FF00FFU, target.ColourValue);
        }

        [TestMethod]
        public void LDOptionalLineConstructorTest2()
        {
            uint colour = 1;
            IEnumerable<Vector3d> vertices = new Vector3d[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ, Vector3d.Zero };
            LDOptionalLine target = new LDOptionalLine(colour, vertices);

            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(Vector3d.UnitX, target.Vertex1);
            Assert.AreEqual(Vector3d.UnitY, target.Vertex2);
            Assert.AreEqual(Vector3d.UnitZ, target.Control1);
            Assert.AreEqual(Vector3d.Zero, target.Control2);
        }

        [TestMethod]
        public void LDOptionalLineConstructorTest3()
        {
            uint colour = 1;
            Vector3d vertex1 = Vector3d.UnitX;
            Vector3d vertex2 = Vector3d.UnitY;
            Vector3d control1 = Vector3d.UnitZ;
            Vector3d control2 = Vector3d.Zero;
            LDOptionalLine target = new LDOptionalLine(colour, ref vertex1, ref vertex2, ref control1, ref control2);
            Assert.AreEqual(colour, target.ColourValue);
            Assert.AreEqual(vertex1, target.Vertex1);
            Assert.AreEqual(vertex2, target.Vertex2);
            Assert.AreEqual(control1, target.Control1);
            Assert.AreEqual(control2, target.Control2);
        }

        [TestMethod]
        public void LDOptionalLineConstructorTest4()
        {
            LDOptionalLine target = new LDOptionalLine();
            Assert.AreEqual(Palette.EdgeColour, target.ColourValue);
            Assert.AreEqual(Vector3d.Zero, target.Vertex1);
            Assert.AreEqual(Vector3d.Zero, target.Vertex2);
            Assert.AreEqual(Vector3d.Zero, target.Control1);
            Assert.AreEqual(Vector3d.Zero, target.Control2);
        }

        #endregion Constructor
    }
}
