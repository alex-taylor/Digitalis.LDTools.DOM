#region License

//
// LDReferenceTest.cs
//
// Copyright (C) 2009-2012 Alex Taylor.  All Rights Reserved.
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
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Properties;

    using Configuration = Digitalis.LDTools.DOM.Configuration;

    #endregion Usings

    [TestClass]
    public sealed class LDReferenceTest : IReferenceTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDReference); } }

        protected override IReference CreateTestReference()
        {
            return new LDReference();
        }

        private void CheckTarget(IReference reference, IDocument externalTarget, IPage page)
        {
            IPage target = reference.Target;
            Assert.IsNotNull(target);
            Assert.AreNotSame(target, page);
            Assert.AreNotSame(target.Document, externalTarget);
            Assert.IsTrue(target.Document.IsFrozen);
            Assert.IsFalse(externalTarget.IsFrozen);
        }

        private const int EventTimeout = 750;

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IReference reference = CreateTestReference())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(reference.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.Reference, typeNameAttr.Description);
                Assert.AreEqual(Resources.Part, reference.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(reference.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNotNull(elementFlagsAttr);
                Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

                Assert.AreEqual(Resources.Undefined, reference.ExtendedDescription);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public void AnalyseValidRefTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;

            target = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            Assert.IsFalse(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsMissingTargetTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 missing.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsTrue(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMissing, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMissing, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMissing, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            // in PartsLibrary mode it should report an Error for non-Models
            LDPage page = new LDPage();
            LDStep step = new LDStep();
            page.Add(step);
            page.PageType = PageType.Part;
            step.Add(target);
            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMissing, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsUnreleasedTargetTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 53992c01.dat", false);
            Assert.IsNotNull(target.Target);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            Assert.IsTrue(target.IsTargetUnreleased);

            // local target should override the library one
            LDDocument doc = new LDDocument();
            LDPage page = new LDPage();
            LDStep step = new LDStep();
            step.Add(target);
            page.Add(step);
            doc.Add(page);
            doc.Add(new LDPage(PageType.Part, "53992c01", "title"));
            Assert.IsFalse(target.IsTargetUnreleased);
            doc.Remove(doc[1]);
            Assert.IsTrue(target.IsTargetUnreleased);

            // 'target' is a released Part undergoing further changes
            target.TargetName = @"s\4209s02.dat";
            Assert.IsFalse(target.IsTargetUnreleased);

            target.TargetName = "53992c01.dat";

            // mode-checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetUnreleased, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetUnreleased, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetUnreleased, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            // target pointing at a non-library file
            target.TargetName = "133.ldr";
            Assert.IsFalse(target.IsTargetUnreleased);

            Assert.IsFalse(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsMovedToTargetTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 121.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.MovedTo, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode-checks: no page
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // mode-checks: with Model page
            LDPage page = new LDPage();
            LDStep step = new LDStep();
            page.Add(step);
            page.PageType = PageType.Model;
            step.Add(target);
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // mode-checks: with non-Model page
            page.PageType = PageType.Part;
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetRedirect_FollowRedirect, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("3787.dat", target.TargetName);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsAliasTargetTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 30071.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.Alias, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode-checks: no page
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // mode-checks: with Model page
            LDPage page = new LDPage();
            LDStep step = new LDStep();
            page.Add(step);
            page.PageType = PageType.Model;
            step.Add(target);
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Information, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // mode-checks: with non-Model page
            page.PageType = PageType.Part;
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetRedirect, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetRedirect_FollowRedirect, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("3005.DAT", target.TargetName);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsLinearScalePartTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDReference("1 16 0 0 0 2.125 0 0 0 2.125 0 0 0 2.125 3005.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsTrue(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // in PartsLibrary mode it should report as an Error for non-Models
            LDPage page = new LDPage();
            LDStep step = new LDStep();
            page.PageType = PageType.Part;
            page.Add(step);
            step.Add(target);
            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetScaled_UnscaleMatrix, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.IsTargetScaled);
            Assert.AreEqual(Matrix4d.Identity, target.Matrix);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsNonlinearScalePartTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDReference("1 16 0 0 0 2.125 0 0 0 10 0 0 0 5 3005.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsTrue(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // in PartsLibrary mode it should report as an Error for non-Models
            LDPage page = new LDPage();
            LDStep step = new LDStep();
            page.Add(step);
            page.PageType = PageType.Part;
            step.Add(target);
            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetScaled_UnscaleMatrix, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.IsTargetScaled);
            Assert.AreEqual(Matrix4d.Identity, target.Matrix);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsMirrorPartTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDReference("1 16 0 0 0 -1 0 0 0 1 0 0 0 1 3005.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsTrue(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMirrored, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMirrored, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMirrored, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // in PartsLibrary mode it should report as an Error for non-Models
            LDPage page = new LDPage();
            LDStep step = new LDStep();
            page.Add(step);
            page.PageType = PageType.Part;
            step.Add(target);
            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMirrored, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetMirrored_UnmirrorMatrix, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.AreEqual(Matrix4d.Identity, target.Matrix);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsInvertedPartTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 3005.dat", true);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsTrue(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);
            Assert.IsTrue(target.Invert);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetInverted, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetInverted, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetInverted, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetInverted_ClearInvert, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.Invert);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsInvertedNonBFCPartTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDReference(@"1 16 0 0 0 1 0 0 0 1 0 0 0 1 s\466as01.dat", true);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsTrue(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);
            Assert.IsTrue(target.Invert);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetInverted, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetInverted, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetInverted, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetInverted_ClearInvert, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.Invert);
            Assert.IsFalse(target.HasProblems(CodeStandards.Full));

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsScalePrimitiveTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            // scaled target: Primitive disc - may not be scaled in the Y-direction
            target = new LDReference("1 16 0 0 0 2 0 0 0 2 0 0 0 2 4-4disc.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsTrue(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetScaled_UnscaleMatrix, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(new Matrix4d(2, 0, 0, 0, 0, 1, 0, 0, 0, 0, 2, 0, 0, 0, 0, 1), target.Matrix);
            Assert.IsFalse(target.IsTargetScaled);

            // scaled target: stud3 - can be scaled in the Y-direction only
            target = new LDReference("1 16 0 0 0 2 0 0 0 2 0 0 0 2 stud3.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsTrue(target.IsTargetScaled);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetScaled_UnscaleMatrix, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(new Matrix4d(1, 0, 0, 0, 0, 2, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1), target.Matrix);
            Assert.IsFalse(target.IsTargetScaled);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsMirrorPrimitiveTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            // mirrored target: stud3 - can be mirrored in the Y-direction only
            target = new LDReference("1 16 0 0 0 -1 0 0 0 1 0 0 0 1 stud3.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsTrue(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMirrored, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMirrored, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetMirrored, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_TargetMirrored_UnmirrorMatrix, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(new Matrix4d(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1), target.Matrix);
            Assert.IsFalse(target.IsTargetScaled);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsSingularMatrixTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            // singular matrix, no fix possible
            target = new LDReference("1 16 0 0 0 0 0 0 0 0 0 0 0 0 3005.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsTrue(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsTrue(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_MatrixSingular, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_MatrixSingular, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_MatrixSingular, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            // singular matrix, fixable
            target = new LDReference("1 16 0 0 0 1 0 0 0 0 0 0 0 1 3005.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsTrue(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsTrue(target.IsTargetScaled);
            Assert.AreEqual(1, target.MatrixZeroColumns.Length);
            Assert.AreEqual(1, target.MatrixZeroRows.Length);
            Assert.AreEqual(1U, target.MatrixZeroColumns[0]);
            Assert.AreEqual(1U, target.MatrixZeroRows[0]);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_MatrixSingular, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_MatrixSingular, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_MatrixSingular, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // apply the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_MatrixSingular_RepairMatrix, fix.Guid);
            Assert.AreEqual(true, fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(Matrix4d.Identity, target.Matrix);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsInvalidColourTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            target = new LDReference("1 24 0 0 0 1 0 0 0 1 0 0 0 1 3005.dat", false);
            Assert.IsFalse(target.IsColocated);
            Assert.IsTrue(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsFalse(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsFalse(target.IsTargetCircularReference);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_ColourInvalid, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // perform the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDReference.Fix_ColourInvalid_SetToMainColour, fix.Guid);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.AreNotEqual(Palette.MainColour, target.ColourValue);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(Palette.MainColour, target.ColourValue);

            target.Dispose();
        }

        [TestMethod]
        public void AnalyticsScaledDiscsTest()
        {
            LDDocument doc = new LDDocument(@"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.LDReference.Analytics.Primitives.ldr", ParseFlags.None);
            IPage page = doc[0];
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;
            bool errors = false;

            foreach (IElement element in page.Elements)
            {
                if (element is LDComment)
                {
                    errors = (element as LDComment).Text.Contains("***");
                }
                else
                {
                    if (errors)
                    {
                        Assert.IsTrue(element.HasProblems(CodeStandards.OfficialModelRepository));
                        problems = element.Analyse(CodeStandards.OfficialModelRepository);
                        Assert.AreEqual(1, problems.Count);
                        problem = problems.First();
                        Assert.AreEqual(LDReference.Problem_TargetScaled, problem.Guid);
                        Assert.AreEqual(Severity.Error, problem.Severity);
                        Assert.AreEqual(element, problem.Element);
                        Assert.IsNotNull(problem.Description);
                        Assert.IsNotNull(problem.Fixes);

                        // apply the fix
                        fixes = problem.Fixes;
                        Assert.AreEqual(1, fixes.Count());
                        fix = fixes.First();
                        Assert.AreEqual(LDReference.Fix_TargetScaled_UnscaleMatrix, fix.Guid);
                        Assert.IsNotNull(fix.Action);
                        Assert.IsNotNull(fix.Instruction);
                        Assert.IsTrue(fix.IsIntraElement);
                        Assert.IsTrue(fix.Apply());
                        Assert.IsFalse(element.HasProblems(CodeStandards.OfficialModelRepository));
                    }
                    else
                    {
                        Assert.IsFalse(element.HasProblems(CodeStandards.OfficialModelRepository));
                        problems = element.Analyse(CodeStandards.OfficialModelRepository);
                        Assert.AreEqual(0, problems.Count);
                    }
                }
            }
        }

        [TestMethod]
        public void AnalyticsCircularReferenceTest()
        {
            LDReference target;
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            LDPage page = new LDPage();

            page.Add(new LDStep());
            target = new LDReference();
            page[0].Add(target);

            target.TargetName = page.TargetName;

            Assert.IsFalse(target.IsColocated);
            Assert.IsFalse(target.IsColourInvalid);
            Assert.IsFalse(target.IsMatrixSingular);
            Assert.IsFalse(target.IsTargetInverted);
            Assert.IsFalse(target.IsTargetMirrored);
            Assert.IsTrue(target.IsTargetMissing);
            Assert.AreEqual(TargetRedirectType.NoRedirect, target.IsTargetRedirect);
            Assert.IsFalse(target.IsTargetScaled);
            Assert.IsTrue(target.IsTargetCircularReference);

            // mode checks
            Assert.IsTrue(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetCircularReference, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetCircularReference, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDReference.Problem_TargetCircularReference, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            target.Dispose();
        }

        #endregion Analytics

        #region Cache

        [TestMethod]
        public void CacheClearTest()
        {
            //LDReference target = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat", false);
            //IPage t = target.Target;
            //Assert.AreNotEqual(0, LDReference.CacheCount);

            //string ldbase = Digitalis.LDTools.DOM.Configuration.LDrawBase;

            //try
            //{
            //    LibraryManagerTest.SetupMinimalLibrary();
            //    Digitalis.LDTools.DOM.Configuration.LDrawBase = LibraryManagerTest.MinimalLibraryBase;
            //    Assert.AreEqual(0, LDReference.CacheCount);
            //}
            //finally
            //{
            //    target.Dispose();
            //    Digitalis.LDTools.DOM.Configuration.LDrawBase = ldbase;
            //    LibraryManagerTest.TeardownMinimalLibrary();
            //}
        }

        [TestMethod]
        public void CacheEventsTest()
        {
            //bool added = false;
            //bool removed = false;

            //LDReference.CacheEntryAdded += delegate(object sender, ReferenceCacheChangedEventArgs e)
            //{
            //    added = true;
            //};

            //LDReference.CacheEntryRemoved += delegate(object sender, ReferenceCacheChangedEventArgs e)
            //{
            //    removed = true;
            //};

            //LDReference r = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat");
            //IPage target = r.Target;
            //Assert.IsNotNull(target);
            //Assert.IsTrue(added);
            //added = false;
            //Assert.IsFalse(removed);

            //// release
            //r.TargetName = "3002.dat";
            //Assert.IsFalse(added);
            //Assert.IsTrue(removed);

            //r.Dispose();
        }

        [TestMethod]
        public void TargetKeyTest()
        {
            //LDReference target = new LDReference();

            //Assert.IsNull(target.Target);
            //Assert.AreEqual(TargetStatus.Missing, target.TargetStatus);
            //Assert.IsNull(target.TargetKey);

            //// library part
            //target.TargetName = "3001.dat";
            //Assert.IsNotNull(target.Target);
            //Assert.AreEqual(TargetStatus.Resolved, target.TargetStatus);
            //Assert.AreEqual("3001.dat", target.TargetKey);

            //// clear
            //target.ClearTarget();
            //Assert.AreEqual(TargetStatus.Unresolved, target.TargetStatus);
            //Assert.IsNull(target.TargetKey);

            //// absolute path
            //target.TargetName = @"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.MLCadGroup.ldr";
            //Assert.IsNotNull(target.Target);
            //Assert.AreEqual(TargetStatus.Resolved, target.TargetStatus);
            //Assert.AreEqual(target.TargetName.ToLower(), target.TargetKey);

            //// local fs target
            //target.TargetName = "page2.dat";

            //LDDocument doc = new LDDocument();
            //doc.Filepath = @"C:\targetkeytest.dat";
            //doc.Add(new LDPage());
            //doc[0].Add(new LDStep());
            //doc[0][0].Add(target);
            //doc.Add(new LDPage(PageType.Part, "page2", "title"));
            //doc.Save();

            //Assert.IsNotNull(target.Target);
            //Assert.AreEqual(doc[1], target.Target);
            //Assert.AreEqual(TargetStatus.Resolved, target.TargetStatus);
            //Assert.IsNull(target.TargetKey);

            //// local target
            //File.Copy(@"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.MLCadGroup.ldr", @"C:\target.dat", true);
            //target.TargetName = "target.dat";
            //Assert.IsNotNull(target.Target);
            //Assert.AreEqual(TargetStatus.Resolved, target.TargetStatus);
            //Assert.AreEqual(@"c:\target.dat", target.TargetKey);

            //File.Delete(@"C:\target.dat");
            //File.Delete(doc.Filepath);

            //target.Dispose();
        }

        #endregion Cache

        #region Constructor

        [TestMethod]
        public void LDReferenceConstructorTest()
        {
            LDReference target = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat", true);
            Assert.AreEqual(DOMObjectType.Reference, target.ObjectType);
            Assert.AreEqual(Palette.MainColour, target.ColourValue);
            Assert.AreEqual(Matrix4d.Identity, target.Matrix);
            Assert.AreEqual("test.dat", target.TargetName);
            Assert.IsTrue(target.Invert);
            target.Dispose();
        }

        [TestMethod]
        public void LDReferenceConstructorTest1()
        {
            LDReference target = new LDReference(Palette.MainColour, ref Matrix4d.Identity, "test.dat", true);
            Assert.AreEqual(Palette.MainColour, target.ColourValue);
            Assert.AreEqual(Matrix4d.Identity, target.Matrix);
            Assert.AreEqual("test.dat", target.TargetName);
            Assert.IsTrue(target.Invert);
            target.Dispose();
        }

        [TestMethod]
        public void LDReferenceConstructorTest2()
        {
            LDReference target = new LDReference();
            Assert.AreEqual(Palette.MainColour, target.ColourValue);
            Assert.AreEqual(Matrix4d.Identity, target.Matrix);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.Undefined, target.TargetName);
            Assert.IsFalse(target.Invert);
            target.Dispose();
        }

        #endregion Constructor

        #region Target-management

        [TestMethod]
        public void TargetAddedToLibraryTest()
        {
            string targetName = "does_not_exist.dat";
            string targetPath = Path.Combine(Configuration.LDrawBase, "parts", targetName);

            using (IReference reference = new LDReference(Palette.MainColour, ref Matrix4d.Identity, targetName, true))
            {
                try
                {
                    if (File.Exists(targetPath))
                        File.Delete(targetPath);

                    // verify that the target cannot be found
                    IPage target = reference.Target;
                    Assert.IsNull(target);

                    // make the target available in the library
                    AutoResetEvent ev = new AutoResetEvent(false);

                    reference.TargetChanged += delegate(object sender, EventArgs e)
                    {
                        ev.Set();
                    };

                    using (TextWriter writer = File.CreateText(targetPath))
                    {
                        writer.Write("0 title\r\n0 Name: " + targetName + "\r\n0 !LDRAW_ORG Part\r\n");
                    }

                    Assert.IsTrue(ev.WaitOne(EventTimeout));
                    target = reference.Target;
                    Assert.IsNotNull(target);
                    Assert.AreEqual(targetName, target.TargetName);

                    // remove it again
                    ev.Reset();
                    File.Delete(targetPath);
                    Assert.IsTrue(ev.WaitOne(EventTimeout));
                    target = reference.Target;
                    Assert.IsNull(target);
                }
                finally
                {
                    if (File.Exists(targetPath))
                        File.Delete(targetPath);
                }
            }
        }

        [TestMethod]
        public void ExternalTargetChangedTest()
        {
            const string path = @"C:\external_target.dat";

            using (IReference reference = new LDReference(Palette.MainColour, ref Matrix4d.Identity, path, false))
            {
                string newpath = @"C:\external_target_renamed.dat";

                if (File.Exists(path))
                    File.Delete(path);

                if (File.Exists(newpath))
                    File.Delete(newpath);

                // create the target
                IDocument externalTarget = new LDDocument();
                IPage page               = new LDPage(PageType.Part, "external_target.dat", "target");
                externalTarget.Add(page);
                externalTarget.Filepath = path;
                externalTarget.Save();

                try
                {
                    // try and resolve it
                    Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                    CheckTarget(reference, externalTarget, page);
                    Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);

                    AutoResetEvent ev = new AutoResetEvent(false);

                    reference.TargetChanged += delegate(object sender, EventArgs e)
                    {
                        ev.Set();
                    };

                    // modifying the file should trigger the event
                    IStep step = new LDStep();
                    page.Add(step);
                    step.Add(new LDLine());
                    externalTarget.Save();
                    Thread.Sleep(EventTimeout);
                    Assert.IsTrue(ev.WaitOne(EventTimeout));
                    ev.Reset();
                    Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                    CheckTarget(reference, externalTarget, page);
                    Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);

                    // renaming the file should trigger the event and update TargetName
                    File.Move(path, newpath);
                    Thread.Sleep(EventTimeout);
                    Assert.IsTrue(ev.WaitOne(EventTimeout));
                    ev.Reset();
                    Assert.AreEqual(newpath, reference.TargetName);
                    Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                    CheckTarget(reference, externalTarget, page);
                    Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);

                    // deleting the file should trigger the event
                    File.Delete(newpath);
                    Thread.Sleep(EventTimeout);
                    Assert.IsTrue(ev.WaitOne(EventTimeout));
                    Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                    Assert.IsNull(reference.Target);
                }
                finally
                {
                    if (File.Exists(path))
                        File.Delete(path);

                    if (File.Exists(newpath))
                        File.Delete(newpath);
                }
            }
        }

        // TODO: ExternalTargetOverrideTest() is unreliable
        //[TestMethod]
        public void ExternalTargetOverrideTest()
        {
            IReference reference = null;
            string srcPath       = Path.Combine(Configuration.LDrawBase, "parts", "3005.dat");
            string dstPath       = Path.Combine(Configuration.LDrawBase, "My Parts", "3005.dat");
            string localDstPath  = @"C:\3005.dat";
            string docPath       = @"C:\testdoc.ldr";

            try
            {
                if (File.Exists(dstPath))
                    File.Delete(dstPath);

                if (File.Exists(localDstPath))
                    File.Delete(localDstPath);

                if (File.Exists(docPath))
                    File.Delete(docPath);

                IDocument doc = new LDDocument();
                IPage page    = new LDPage();
                IStep step    = new LDStep();
                reference     = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 3005.dat");

                doc.Add(page);
                page.Add(step);
                step.Add(reference);
                doc.Filepath = docPath;
                doc.Save();

                // initial resolve to a library target
                IPage target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreEqual(srcPath, target.Document.Filepath);

                AutoResetEvent ev = new AutoResetEvent(false);

                reference.TargetChanged += delegate(object sender, EventArgs e)
                {
                    ev.Set();
                };

                // adding a local page with the same TargetName should replace the library target
                IPage overrideTarget = new LDPage(PageType.Part, "3005.dat", "title");
                doc.Add(overrideTarget);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreSame(overrideTarget, target);
                Assert.AreSame(doc, target.Document);
                ev.Reset();

                // there should not be a TargetChanged event if a file with the same name is updated in the library
                File.Copy(srcPath, dstPath, true);
                Thread.Sleep(EventTimeout);
                Assert.IsFalse(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                target = reference.Target;
                Assert.AreEqual(overrideTarget, target);
                Assert.AreEqual(doc, target.Document);
                ev.Reset();

                // and similarly, no event when the library file is removed again
                File.Delete(dstPath);
                Thread.Sleep(EventTimeout);
                Assert.IsFalse(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                target = reference.Target;
                Assert.AreEqual(overrideTarget, target);
                Assert.AreEqual(doc, target.Document);
                ev.Reset();

                // removing the local target should revert to the library
                doc.Remove(overrideTarget);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreNotSame(overrideTarget, target);
                ev.Reset();

                // 3005.dat comes from the official library, so adding a file to the 'My Parts' folder should override it
                File.Copy(srcPath, dstPath, true);
                Thread.Sleep(EventTimeout);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreEqual(dstPath, target.Document.Filepath);
                ev.Reset();

                // deleting it again should reinstate the original
                File.Delete(dstPath);
                Thread.Sleep(EventTimeout);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreEqual(srcPath, target.Document.Filepath);
                ev.Reset();

                // adding a file into the local filesystem should also override the library target
                File.Copy(srcPath, localDstPath, true);
                Thread.Sleep(EventTimeout);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreEqual(localDstPath, target.Document.Filepath);
                ev.Reset();

                // there should not be an event if a file with the same name is updated in the library
                File.Copy(srcPath, dstPath, true);
                Thread.Sleep(EventTimeout);
                Assert.IsFalse(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreEqual(localDstPath, target.Document.Filepath);
                ev.Reset();

                // and similarly, no event when the library file is removed again
                File.Delete(dstPath);
                Thread.Sleep(EventTimeout);
                Assert.IsFalse(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Resolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreEqual(localDstPath, target.Document.Filepath);
                ev.Reset();

                // deleting the local target should reinstate the library target
                File.Delete(localDstPath);
                Thread.Sleep(EventTimeout);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(TargetStatus.Unresolved, reference.TargetStatus);
                target = reference.Target;
                Assert.IsNotNull(target);
                Assert.AreEqual(srcPath, target.Document.Filepath);
            }
            finally
            {
                if (File.Exists(dstPath))
                    File.Delete(dstPath);

                if (File.Exists(localDstPath))
                    File.Delete(localDstPath);

                if (File.Exists(docPath))
                    File.Delete(docPath);
            }
        }

        #endregion Target-management
    }
}
