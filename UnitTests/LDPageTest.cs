#region License

//
// LDPageTest.cs
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

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.LDTools.Library;


    #endregion Usings

    [TestClass]
    public class LDPageTest : IPageTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDPage); } }

        protected override IPage CreateTestPage()
        {
            return new LDPage();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IPage page = CreateTestPage();
            Assert.IsTrue(TestClassType.IsSealed);
            Assert.IsFalse(page.IsImmutable);

            TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
            Assert.IsNotNull(typeNameAttr);
            Assert.AreEqual(Resources.Page, typeNameAttr.Description);
            Assert.AreEqual(Resources.Page, page.TypeName);

            DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
            Assert.IsNotNull(defaultIconAttr);
            Assert.IsNotNull(defaultIconAttr.Icon);
            Assert.IsNotNull(page.Icon);

            ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
            Assert.IsNotNull(elementFlagsAttr);
            Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

            Assert.AreEqual(String.Empty, page.ExtendedDescription);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public void AnalyticsNameFormatModelsTest()
        {
            LDPage target = new LDPage();
            ICollection<IProblemDescriptor> problems;

            target.PageType = PageType.Model;
            target.User = "user";

            // only [-A-Za-z0-9_] are permitted for non-models; models can use anything
            target.Name = "x[=]";
            Assert.AreEqual(LDPage.NameFormatProblem.None, LDPage.NameFormatProblem.Invalid_Chars & target.IsNameFormatInvalid);

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
        public void AnalyticsNameFormatInvalidCharsTest()
        {
            LDPage target = new LDPage();
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target.User = "user";
            target.PageType = PageType.Part;
            target.Category = Category.Brick;
            target.BFC = CullingMode.CertifiedCounterClockwise;

            // only [-A-Za-z0-9_] are permitted for non-models
            target.Name = "xABCabc012-_";
            Assert.AreEqual(LDPage.NameFormatProblem.None, LDPage.NameFormatProblem.Invalid_Chars & target.IsNameFormatInvalid);
            target.Name = "12345A";
            Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);

            target.Name = "x[=]";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_Chars, LDPage.NameFormatProblem.Invalid_Chars & target.IsNameFormatInvalid);

            // mode checks
            Assert.IsFalse(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsNameFormatInvalidCharsTest2()
        {
            LDDocument doc = new LDDocument();
            LDPage target = new LDPage();
            ICollection<IProblemDescriptor> problems;

            doc.Add(new LDPage(PageType.Model, "model.ldr", "model"));

            target.User = "user";
            target.PageType = PageType.Part;
            target.Category = Category.Brick;
            target.BFC = CullingMode.CertifiedCounterClockwise;
            doc.Add(target);

            // if the page is in a Model document, all chars are permitted
            target.Name = "xABCabc012-_";
            Assert.AreEqual(LDPage.NameFormatProblem.None, LDPage.NameFormatProblem.Invalid_Chars & target.IsNameFormatInvalid);
            target.Name = "12345A";
            Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
            target.Name = "x[=]";
            Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);

            // mode checks
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
        public void AnalyticsNameLengthTest()
        {
            LDPage target = new LDPage();
            ICollection<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            // length: max of 21 chars for non-models
            target.User = "user";
            target.PageType = PageType.Model;
            target.BFC = CullingMode.NotSet;
            target.Name = "012345678901234567890123456789";
            Assert.IsFalse(target.IsNameTooLong);

            Assert.IsFalse(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(0, problems.Count);

            Assert.IsFalse(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count);

            target.PageType = PageType.Part;
            target.BFC = CullingMode.CertifiedCounterClockwise;
            target.Category = Category.Animal;
            Assert.IsTrue(target.IsNameTooLong);

            // mode checks
            Assert.IsFalse(target.HasProblems(CodeStandards.Full));
            problems = target.Analyse(CodeStandards.Full);
            Assert.AreEqual(0, problems.Count);

            Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
            problems = target.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_NameTooLong, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count);
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_NameTooLong, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.AreEqual(target, problem.Element);
            Assert.IsNotNull(problem.Description);
            Assert.IsNull(problem.Fixes);

            target.Name = "012345678901234567890";
            Assert.IsFalse(target.IsNameTooLong);

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
        public void AnalyticsNameFormatTest()
        {
            LDPage target = new LDPage();
            ICollection<IProblemDescriptor> problems;
            IEnumerable<IFixDescriptor> fixes;
            IProblemDescriptor problem;
            IFixDescriptor fix;

            target.PageType = PageType.Part;
            target.Category = Category.Animal;

            // Part/Shortcut/Shortcut_Physical_Colour: [prefix]nnnnn[suffix][pattern-marker][shortcut-marker]
            PageType[] types = new PageType[] { PageType.Part, PageType.Shortcut, PageType.Shortcut_Physical_Colour };

            foreach (PageType type in types)
            {
                target.PageType = type;
                target.Name = "12345";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345a";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345p";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345pp01";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345ap01";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "u12345ap01";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "s12345ap01";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "x12345ap01";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345pab";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "xabcde";
                Assert.AreEqual(LDPage.NameFormatProblem.Unrecognised, target.IsNameFormatInvalid);
                target.Name = "a12345";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_Prefix, target.IsNameFormatInvalid);
                target.Name = "12345-";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_Suffix, target.IsNameFormatInvalid);
                target.Name = "12a345";
                Assert.AreEqual(LDPage.NameFormatProblem.Unrecognised, target.IsNameFormatInvalid);
                target.Name = "12345pti";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345ptl";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345pto";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345ptp";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345pai";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker, target.IsNameFormatInvalid);
                target.Name = "12345pal";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker, target.IsNameFormatInvalid);
                target.Name = "12345pao";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker, target.IsNameFormatInvalid);
                target.Name = "12345pap";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker, target.IsNameFormatInvalid);
                target.Name = "12345pi0";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker, target.IsNameFormatInvalid);
                target.Name = "12345pl0";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker, target.IsNameFormatInvalid);
                target.Name = "12345po0";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker, target.IsNameFormatInvalid);
                target.Name = "12345c01";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345d01";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345s01";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_SubpartMarker, target.IsNameFormatInvalid);
                target.Name = "12345p01s01";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_SubpartMarker, target.IsNameFormatInvalid);
                target.Name = "12345cab";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_ShortcutMarker, target.IsNameFormatInvalid);
                target.Name = "3024ptc2";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "30361dps1";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "3626cpb9";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "4616559cc01";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345p01p01";
                Assert.AreEqual(LDPage.NameFormatProblem.Duplicate_PatternMarker, target.IsNameFormatInvalid);
                target.Name = "12345c01c01";
                Assert.AreEqual(LDPage.NameFormatProblem.Duplicate_ShortcutMarker, target.IsNameFormatInvalid);
                target.Name = "12345c01d01";
                Assert.AreEqual(LDPage.NameFormatProblem.Duplicate_ShortcutMarker, target.IsNameFormatInvalid);
                target.Name = "12345c01p01";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_MarkerSequence, target.IsNameFormatInvalid);
                target.Name = "12345d01p01";
                Assert.AreEqual(LDPage.NameFormatProblem.Invalid_MarkerSequence, target.IsNameFormatInvalid);
                target.Name = "12345c01p01c02";
                Assert.AreEqual(LDPage.NameFormatProblem.Duplicate_ShortcutMarker, target.IsNameFormatInvalid);

                // valid
                target.User = "user";
                target.Name = "12345";
                target.Title = (PageType.Shortcut_Physical_Colour == type) ? "_title [0]" : "title";
                target.BFC = CullingMode.Disabled;
                Assert.IsFalse(target.HasProblems(CodeStandards.OfficialModelRepository));
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(0, problems.Count());

                // unrecognisable
                target.Name = "xabcde";
                Assert.IsFalse(target.HasProblems(CodeStandards.Full));
                problems = target.Analyse(CodeStandards.Full);
                Assert.AreEqual(0, problems.Count);

                Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
                problems = target.Analyse(CodeStandards.PartsLibrary);
                Assert.AreEqual(1, problems.Count);
                problem = problems.First();
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.AreEqual(Severity.Error, problem.Severity);
                Assert.AreEqual(target, problem.Element);
                Assert.IsNotNull(problem.Description);
                Assert.IsNull(problem.Fixes);

                Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count);
                problem = problems.First();
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.AreEqual(Severity.Error, problem.Severity);
                Assert.AreEqual(target, problem.Element);
                Assert.IsNotNull(problem.Description);
                Assert.IsNull(problem.Fixes);

                // invalid prefix
                target.Name = "a12345";
                Assert.IsFalse(target.HasProblems(CodeStandards.Full));
                problems = target.Analyse(CodeStandards.Full);
                Assert.AreEqual(0, problems.Count);

                Assert.IsTrue(target.HasProblems(CodeStandards.PartsLibrary));
                problems = target.Analyse(CodeStandards.PartsLibrary);
                Assert.AreEqual(1, problems.Count);
                problem = problems.First();
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.AreEqual(Severity.Error, problem.Severity);
                Assert.AreEqual(target, problem.Element);
                Assert.IsNotNull(problem.Description);
                Assert.IsTrue(problem.Description.Contains("'a'"));
                Assert.IsNotNull(problem.Fixes);

                Assert.IsTrue(target.HasProblems(CodeStandards.OfficialModelRepository));
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count);
                problem = problems.First();
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.AreEqual(Severity.Error, problem.Severity);
                Assert.AreEqual(target, problem.Element);
                Assert.IsNotNull(problem.Description);
                Assert.IsTrue(problem.Description.Contains("'a'"));
                Assert.IsNotNull(problem.Fixes);

                fixes = problem.Fixes;
                Assert.AreEqual(4, fixes.Count());
                fix = fixes.ElementAt(1);
                Assert.AreEqual(LDPage.Fix_NameChangePrefix, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Instruction.Contains("'u'"));
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("u12345", target.Name);
                fix = fixes.ElementAt(2);
                Assert.AreEqual(LDPage.Fix_NameChangePrefix, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Instruction.Contains("'x'"));
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("x12345", target.Name);
                fix = fixes.ElementAt(3);
                Assert.AreEqual(LDPage.Fix_NameChangePrefix, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Instruction.Contains("'s'"));
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("s12345", target.Name);
                fix = fixes.ElementAt(0);
                Assert.AreEqual(LDPage.Fix_NameRemovePrefix, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("12345", target.Name);

                // invalid suffix
                target.Name = "12345-";
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count());
                problem = problems.First();
                Assert.IsTrue(problem.Description.Contains("'-'"));
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.IsNotNull(problem.Fixes);
                fixes = problem.Fixes;
                Assert.AreEqual(1, fixes.Count());
                fix = fixes.ElementAt(0);
                Assert.AreEqual(LDPage.Fix_NameRemoveSuffix, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("12345", target.Name);

                // invalid marker sequence
                target.Name = "12345c01p01";
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count());
                problem = problems.First();
                Assert.IsTrue(problem.Description.Contains("'c01p01'"));
                Assert.IsTrue(problem.Description.Contains("'p01c01'"));
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.IsNotNull(problem.Fixes);
                fixes = problem.Fixes;
                Assert.AreEqual(1, fixes.Count());
                fix = fixes.ElementAt(0);
                Assert.AreEqual(LDPage.Fix_NameSwapMarkers, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("12345p01c01", target.Name);

                // unneeded subpart marker
                target.Name = "12345s01";
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count());
                problem = problems.First();
                Assert.IsTrue(problem.Description.Contains("'s01'"));
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.IsNotNull(problem.Fixes);
                fixes = problem.Fixes;
                Assert.AreEqual(1, fixes.Count());
                fix = fixes.ElementAt(0);
                Assert.AreEqual(LDPage.Fix_NameRemoveMarker, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Instruction.Contains("'s01'"));
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("12345", target.Name);

                // invalid pattern marker
                target.Name = "12345pl1";
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count());
                problem = problems.First();
                Assert.IsTrue(problem.Description.Contains("'pl1'"));
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.IsNotNull(problem.Fixes);
                fixes = problem.Fixes;
                Assert.AreEqual(1, fixes.Count());
                fix = fixes.ElementAt(0);
                Assert.AreEqual(LDPage.Fix_NameRemoveMarker, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Instruction.Contains("'pl1'"));
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("12345", target.Name);

                // invalid shortcut marker
                target.Name = "12345cp1";
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count());
                problem = problems.First();
                Assert.IsTrue(problem.Description.Contains("'cp1'"));
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.IsNotNull(problem.Fixes);
                fixes = problem.Fixes;
                Assert.AreEqual(1, fixes.Count());
                fix = fixes.ElementAt(0);
                Assert.AreEqual(LDPage.Fix_NameRemoveMarker, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Instruction.Contains("'cp1'"));
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("12345", target.Name);

                // duplicate pattern marker
                target.Name = "12345p01p02";
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count());
                problem = problems.First();
                Assert.IsTrue(problem.Description.Contains("'p01p02'"));
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.IsNull(problem.Fixes);

                // duplicate shortcut marker
                target.Name = "12345c01d02";
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count());
                problem = problems.First();
                Assert.IsTrue(problem.Description.Contains("'c01d02'"));
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.IsNull(problem.Fixes);
            }

            // Part_Alias: nnnnn[suffix]
            target.PageType = PageType.Part_Alias;
            target.Title = "Title";
            target.Name = "12345";
            Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
            target.Name = "12345a";
            Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
            target.Name = "12345-";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_Suffix, target.IsNameFormatInvalid);
            target.Name = "a12345";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_Prefix, target.IsNameFormatInvalid);
            target.Name = "x12345";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_Prefix, target.IsNameFormatInvalid);
            target.Name = "12345p01";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_PatternMarker, target.IsNameFormatInvalid);
            target.Name = "12345c01";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_ShortcutMarker, target.IsNameFormatInvalid);
            target.Name = "12345d01";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_ShortcutMarker, target.IsNameFormatInvalid);
            target.Name = "12345s01";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345s01s01";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_SubpartMarker, target.IsNameFormatInvalid);

            // unneeded pattern-marker
            target.Name = "12345p01";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.IsTrue(problem.Description.Contains("'p01'"));
            Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
            Assert.IsNotNull(problem.Fixes);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.ElementAt(0);
            Assert.AreEqual(LDPage.Fix_NameRemoveMarker, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Instruction.Contains("'p01'"));
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("12345", target.Name);

            // unneeded shortcut-marker
            target.Name = "12345c01";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.IsTrue(problem.Description.Contains("'c01'"));
            Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
            Assert.IsNotNull(problem.Fixes);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.ElementAt(0);
            Assert.AreEqual(LDPage.Fix_NameRemoveMarker, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Instruction.Contains("'c01'"));
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("12345", target.Name);

            // Part_Physical_Colour/Shortcut_Alias: nnnnn
            types = new PageType[] { PageType.Shortcut_Alias, PageType.Part_Physical_Colour };
            foreach (PageType type in types)
            {
                target.PageType = type;
                target.Name = "12345";
                Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
                target.Name = "12345a";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_Suffix, target.IsNameFormatInvalid);
                target.Name = "12345-";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_Suffix, target.IsNameFormatInvalid);
                target.Name = "a12345";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_Prefix, target.IsNameFormatInvalid);
                target.Name = "x12345";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_Prefix, target.IsNameFormatInvalid);
                target.Name = "12345p01";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_PatternMarker, target.IsNameFormatInvalid);
                target.Name = "12345c01";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_ShortcutMarker, target.IsNameFormatInvalid);
                target.Name = "12345d01";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_ShortcutMarker, target.IsNameFormatInvalid);
                target.Name = "12345s01";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_SubpartMarker, target.IsNameFormatInvalid);
                target.Name = "12345s01s01";
                Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_SubpartMarker, target.IsNameFormatInvalid);

                // unneeded prefix
                target.Name = "x12345";
                target.Title = (PageType.Part_Physical_Colour == type) ? "_title [0]" : "title";
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count());
                problem = problems.First();
                Assert.IsTrue(problem.Description.Contains("'x'"));
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.IsNotNull(problem.Fixes);
                fixes = problem.Fixes;
                Assert.AreEqual(1, fixes.Count());
                fix = fixes.ElementAt(0);
                Assert.AreEqual(LDPage.Fix_NameRemovePrefix, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("12345", target.Name);

                // unneeded suffix
                target.Name = "12345a";
                problems = target.Analyse(CodeStandards.OfficialModelRepository);
                Assert.AreEqual(1, problems.Count());
                problem = problems.First();
                Assert.IsTrue(problem.Description.Contains("'a'"));
                Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
                Assert.IsNotNull(problem.Fixes);
                fixes = problem.Fixes;
                Assert.AreEqual(1, fixes.Count());
                fix = fixes.ElementAt(0);
                Assert.AreEqual(LDPage.Fix_NameRemoveSuffix, fix.Guid);
                Assert.IsTrue(fix.IsIntraElement);
                Assert.IsTrue(fix.Apply());
                Assert.AreEqual("12345", target.Name);
            }

            // Subpart: [prefix]nnnnn[suffix][pattern-marker]subpart-marker
            target.PageType = PageType.Subpart;
            target.Name = "12345";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345a";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345p";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345pp01";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345ap01";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "u12345ap01";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "s12345ap01";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "x12345ap01";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345pab";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "xabcde";
            Assert.AreEqual(LDPage.NameFormatProblem.Unrecognised, target.IsNameFormatInvalid);
            target.Name = "a12345";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_Prefix | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345-";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_Suffix | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12a345";
            Assert.AreEqual(LDPage.NameFormatProblem.Unrecognised, target.IsNameFormatInvalid);
            target.Name = "12345pti";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345ptl";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345pto";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345ptp";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345pai";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345pal";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345pao";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345pap";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345pi0";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345pl0";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345po0";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_PatternMarker | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345c01";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_ShortcutMarker | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345d01";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_ShortcutMarker | LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345s01";
            Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
            target.Name = "12345p01s01";
            Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
            target.Name = "3024ptc2";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "30361dps1";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "3626cpb9";
            Assert.AreEqual(LDPage.NameFormatProblem.Missing_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "2865s04a";
            Assert.AreEqual(LDPage.NameFormatProblem.Unrecognised, target.IsNameFormatInvalid);
            target.Name = "30361ds01";
            Assert.AreEqual(LDPage.NameFormatProblem.None, target.IsNameFormatInvalid);
            target.Name = "12345p01p01s01";
            Assert.AreEqual(LDPage.NameFormatProblem.Duplicate_PatternMarker, target.IsNameFormatInvalid);
            target.Name = "12345s01s01";
            Assert.AreEqual(LDPage.NameFormatProblem.Duplicate_SubpartMarker, target.IsNameFormatInvalid);
            target.Name = "12345c01s01";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_ShortcutMarker, target.IsNameFormatInvalid);
            target.Name = "12345d01s01";
            Assert.AreEqual(LDPage.NameFormatProblem.Unneeded_ShortcutMarker, target.IsNameFormatInvalid);
            target.Name = "12345s01p01";
            Assert.AreEqual(LDPage.NameFormatProblem.Invalid_MarkerSequence, target.IsNameFormatInvalid);
            target.Name = "12345s01p01s01";
            Assert.AreEqual(LDPage.NameFormatProblem.Duplicate_SubpartMarker, target.IsNameFormatInvalid);

            // invalid marker sequence
            target.Name = "12345s01p01";
            target.Title = "~title";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.IsTrue(problem.Description.Contains("'s01p01'"));
            Assert.IsTrue(problem.Description.Contains("'p01s01'"));
            Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
            Assert.IsNotNull(problem.Fixes);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.ElementAt(0);
            Assert.AreEqual(LDPage.Fix_NameSwapMarkers, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("12345p01s01", target.Name);

            // missing subpart-marker
            target.Name = "12345";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
            Assert.IsNull(problem.Fixes);

            // invalid subpart-marker
            target.Name = "12345sa1";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.IsTrue(problem.Description.Contains("'sa1'"));
            Assert.AreEqual(LDPage.Problem_NameFormatInvalid, problem.Guid);
            Assert.IsNotNull(problem.Fixes);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.ElementAt(0);
            Assert.AreEqual(LDPage.Fix_NameRemoveMarker, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Instruction.Contains("'sa1'"));
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("12345", target.Name);
        }

        [TestMethod]
        public void AnalyticsTitleTest()
        {
            LDPage target = new LDPage();
            IEnumerable<IProblemDescriptor> problems;
            IEnumerable<IFixDescriptor> fixes;
            IProblemDescriptor problem;
            IFixDescriptor fix;

            LDStep step = new LDStep();
            target.Add(step);

            target.User = "user";
            target.Name = "12345";

            // length: old 64 chars for non-models limit now raised
            target.PageType = PageType.Model;
            target.BFC = CullingMode.NotSet;
            target.Title = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());
            target.PageType = PageType.Part;
            target.Category = Category.Brick;
            target.BFC = CullingMode.Disabled;
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count());

            // leading underscore
            target.Title = "_title";
            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Part_Physical_Colour == type || PageType.Shortcut_Physical_Colour == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.None | LDPage.TitleFormatProblem.Missing_ColourValue, target.IsTitleFormatInvalid);
                else if (PageType.Subpart == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Underscore | LDPage.TitleFormatProblem.Missing_Tilde, target.IsTitleFormatInvalid);
                else if (PageType.Model == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
                else
                    Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Underscore, target.IsTitleFormatInvalid);
            }

            target.PageType = PageType.Part;
            target.Title = "_title";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_TitleFormatInvalid, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_TitleRemoveUnderscore, fix.Guid);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("title", target.Title);

            // missing underscore
            target.Title = "title";
            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Part_Physical_Colour == type || PageType.Shortcut_Physical_Colour == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_Underscore | LDPage.TitleFormatProblem.Missing_ColourValue, target.IsTitleFormatInvalid);
                else if (PageType.Subpart == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_Tilde, target.IsTitleFormatInvalid);
                else
                    Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
            }

            target.PageType = PageType.Part_Physical_Colour;
            target.Title = "title [0]";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_TitleFormatInvalid, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_TitleAddUnderscore, fix.Guid);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("_title [0]", target.Title);

            // leading tilde
            target.Title = "~title";
            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Part_Physical_Colour == type || PageType.Shortcut_Physical_Colour == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_Underscore | LDPage.TitleFormatProblem.Unneeded_Tilde | LDPage.TitleFormatProblem.Missing_ColourValue, target.IsTitleFormatInvalid);
                else
                    Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
            }

            target.PageType = PageType.Part_Physical_Colour;
            target.Title = "~title";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_TitleFormatInvalid, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreNotEqual(0, fixes.Count());

            foreach (IFixDescriptor f in fixes)
            {
                if (LDPage.Fix_TitleRemoveTilde == f.Guid)
                {
                    Assert.IsTrue(f.Apply());
                    Assert.AreEqual("title", target.Title);
                }
            }

            // missing tilde
            target.Title = "title";
            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Subpart == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_Tilde, target.IsTitleFormatInvalid);
                else if (PageType.Part_Physical_Colour == type || PageType.Shortcut_Physical_Colour == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_Underscore | LDPage.TitleFormatProblem.Missing_ColourValue, target.IsTitleFormatInvalid);
                else
                    Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
            }

            target.PageType = PageType.Subpart;
            target.Title = "title";
            target.Name = "12345s01";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_TitleFormatInvalid, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_TitleAddTilde, fix.Guid);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("~title", target.Title);
            target.Name = "12345";

            // trailing colour-value
            target.Title = "_title [0]";
            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Part_Physical_Colour == type || PageType.Shortcut_Physical_Colour == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
                else if (PageType.Subpart == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_Tilde | LDPage.TitleFormatProblem.Unneeded_Underscore | LDPage.TitleFormatProblem.Unneeded_ColourValue, target.IsTitleFormatInvalid);
                else if (PageType.Model == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
                else
                    Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_ColourValue | LDPage.TitleFormatProblem.Unneeded_Underscore, target.IsTitleFormatInvalid);
            }

            target.PageType = PageType.Part;
            target.Title = "title [0]";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_TitleFormatInvalid, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_TitleRemoveColourValue, fix.Guid);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("title", target.Title);

            // missing colour-value
            target.Title = "_title";
            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Part_Physical_Colour == type || PageType.Shortcut_Physical_Colour == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_ColourValue, target.IsTitleFormatInvalid);
                else if (PageType.Subpart == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_Tilde | LDPage.TitleFormatProblem.Unneeded_Underscore, target.IsTitleFormatInvalid);
                else if (PageType.Model == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
                else
                    Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Underscore, target.IsTitleFormatInvalid);
            }

            target.Title = "_title [foo]";
            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Part_Physical_Colour == type || PageType.Shortcut_Physical_Colour == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_ColourValue, target.IsTitleFormatInvalid);
                else if (PageType.Subpart == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.Missing_Tilde | LDPage.TitleFormatProblem.Unneeded_Underscore, target.IsTitleFormatInvalid);
                else if (PageType.Model == type)
                    Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
                else
                    Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Underscore, target.IsTitleFormatInvalid);
            }

            target.PageType = PageType.Part_Physical_Colour;
            target.Title = "_title";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_TitleFormatInvalid, problem.Guid);
            Assert.IsNull(problem.Fixes);
            step.Add(new LDReference("1 4 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat", false));
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_TitleFormatInvalid, problem.Guid);
            Assert.IsNotNull(problem.Fixes);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_TitleAddColourValue, fix.Guid);
            Assert.IsTrue(fix.Instruction.Contains("'[4]'"));
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("_title [4]", target.Title);

            // missing whitespace
            target.PageType = PageType.Part;
            target.Title = "Title  1 x  2L x  3";
            Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
            target.Title = "Title 1 x  2L x  3";
            Assert.AreEqual(LDPage.TitleFormatProblem.Missing_SpaceBeforeNumber, target.IsTitleFormatInvalid);
            target.Title = "Title  1 x 2L x  3";
            Assert.AreEqual(LDPage.TitleFormatProblem.Missing_SpaceBeforeNumber, target.IsTitleFormatInvalid);
            target.Title = "Title  1 x  2L x 3";
            Assert.AreEqual(LDPage.TitleFormatProblem.Missing_SpaceBeforeNumber, target.IsTitleFormatInvalid);
            target.Title = "1 title";
            Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
            target.Title = "1 title 1";
            Assert.AreEqual(LDPage.TitleFormatProblem.Missing_SpaceBeforeNumber, target.IsTitleFormatInvalid);
            target.PageType = PageType.Model;
            Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);

            target.PageType = PageType.Part;
            target.Title = "Title 1 x  2L x  3";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_TitleFormatInvalid, problem.Guid);
            Assert.IsNotNull(problem.Fixes);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_TitleCorrectSpacing, fix.Guid);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("Title  1 x  2L x  3", target.Title);

            // excess whitespace
            target.PageType = PageType.Part;
            target.Title = "Title 12 x 12 foo";
            Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);
            target.Title = "Title  12 x 12 foo";
            Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Spaces, target.IsTitleFormatInvalid);
            target.Title = "Title  12 x 12  foo";
            Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Spaces, target.IsTitleFormatInvalid);
            target.Title = "Title 12 x 12  foo";
            Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Spaces, target.IsTitleFormatInvalid);
            target.Title = "Title 12 x  12 foo";
            Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Spaces, target.IsTitleFormatInvalid);
            target.Title = "Title 12 x 12        foo";
            Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Spaces, target.IsTitleFormatInvalid);
            target.Title = "Title 12 x 12 \tfoo";
            Assert.AreEqual(LDPage.TitleFormatProblem.Unneeded_Spaces, target.IsTitleFormatInvalid);
            target.PageType = PageType.Model;
            Assert.AreEqual(LDPage.TitleFormatProblem.None, target.IsTitleFormatInvalid);

            target.PageType = PageType.Part;
            target.Title = "Title  1 x  20 x  3";
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_TitleFormatInvalid, problem.Guid);
            Assert.IsNotNull(problem.Fixes);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_TitleCorrectSpacing, fix.Guid);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("Title  1 x 20 x  3", target.Title);
        }

        [TestMethod]
        public void AnalyticsKeywordsTest()
        {
            LDPage target = new LDPage();
            IEnumerable<IProblemDescriptor> problems;
            IEnumerable<IFixDescriptor> fixes;
            IProblemDescriptor problem;
            IFixDescriptor fix;
            IEnumerable<string> keywords;

            target.User = "user";
            target.PageType = PageType.Model;
            target.Title = "Title";
            Assert.IsFalse(target.HasKeywordDuplicates);
            Assert.IsFalse(target.HasKeywordInTitle);
            Assert.AreEqual(0, target.KeywordDuplicates.Length);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());

            // unique keywords
            target.Keywords = new string[] { "keyword1", "keyword2", "keyword3" };
            Assert.IsFalse(target.HasKeywordDuplicates);
            Assert.IsFalse(target.HasKeywordInTitle);
            Assert.AreEqual(0, target.KeywordDuplicates.Length);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());

            // unique keywords, but with one that appears in the title (case-insensitive)
            target.Keywords = new string[] { "keyword1", "keyword2", "title" };
            Assert.IsFalse(target.HasKeywordDuplicates);
            Assert.IsTrue(target.HasKeywordInTitle);
            Assert.AreEqual(1, target.KeywordDuplicates.Length);
            Assert.AreEqual(2U, target.KeywordDuplicates[0]);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_KeywordInTitle, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_KeywordRemoveUnneededEntries, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            keywords = target.Keywords;
            Assert.AreEqual(2, keywords.Count());
            Assert.IsFalse(keywords.Contains("title"));
            Assert.IsFalse(target.HasKeywordInTitle);

            // multi-word keywords
            target.Keywords = new string[] { "keyword 1", "keyword 2", "keyword 3" };
            Assert.IsFalse(target.HasKeywordDuplicates);
            Assert.IsFalse(target.HasKeywordInTitle);
            Assert.AreEqual(0, target.KeywordDuplicates.Length);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());

            // multi-word keyword that appears in the title
            target.Title = "Long Title";
            target.Keywords = new string[] { "keyword 1", "keyword 2", "long title" };
            Assert.IsFalse(target.HasKeywordDuplicates);
            Assert.IsTrue(target.HasKeywordInTitle);
            Assert.AreEqual(1, target.KeywordDuplicates.Length);
            Assert.AreEqual(2U, target.KeywordDuplicates[0]);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_KeywordInTitle, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_KeywordRemoveUnneededEntries, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            keywords = target.Keywords;
            Assert.AreEqual(2, keywords.Count());
            Assert.IsFalse(keywords.Contains("long title"));
            Assert.IsFalse(target.HasKeywordInTitle);

            // duplicate keywords
            target.Keywords = new string[] { "keyword 1", "keyword 1", "keyword 2" };
            Assert.IsTrue(target.HasKeywordDuplicates);
            Assert.IsFalse(target.HasKeywordInTitle);
            Assert.AreEqual(1, target.KeywordDuplicates.Length);
            Assert.AreEqual(1U, target.KeywordDuplicates[0]);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_KeywordDuplicates, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_KeywordRemoveUnneededEntries, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            keywords = target.Keywords;
            Assert.AreEqual(2, keywords.Count());
            Assert.AreEqual("keyword 1", keywords.ElementAt(0));
            Assert.AreEqual("keyword 2", keywords.ElementAt(1));
            Assert.IsFalse(target.HasKeywordDuplicates);

            // duplicate keywords with one in the title
            target.Title = "Title";
            target.Keywords = new string[] { "keyword 1", "title", "title" };
            Assert.IsTrue(target.HasKeywordDuplicates);
            Assert.IsTrue(target.HasKeywordInTitle);
            Assert.AreEqual(2, target.KeywordDuplicates.Length);
            Assert.AreEqual(1U, target.KeywordDuplicates[0]);
            Assert.AreEqual(2U, target.KeywordDuplicates[1]);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(2, problems.Count());
            problem = problems.ElementAt(0);
            Assert.AreEqual(LDPage.Problem_KeywordDuplicates, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_KeywordRemoveUnneededEntries, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            keywords = target.Keywords;
            Assert.AreEqual(2, keywords.Count());
            Assert.AreEqual("keyword 1", keywords.ElementAt(0));
            Assert.AreEqual("title", keywords.ElementAt(1));
            Assert.IsFalse(target.HasKeywordDuplicates);
            Assert.IsTrue(target.HasKeywordInTitle);
            problem = problems.ElementAt(1);
            Assert.AreEqual(LDPage.Problem_KeywordInTitle, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_KeywordRemoveUnneededEntries, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            keywords = target.Keywords;
            Assert.AreEqual(1, keywords.Count());
            Assert.AreEqual("keyword 1", keywords.ElementAt(0));
            Assert.IsFalse(target.HasKeywordDuplicates);
            Assert.IsFalse(target.HasKeywordInTitle);

            target.Keywords = new string[] { "keyword 1", "title", "title" };
            Assert.IsTrue(target.HasKeywordDuplicates);
            Assert.IsTrue(target.HasKeywordInTitle);
            target.PageType = PageType.Primitive;
            Assert.IsFalse(target.HasKeywordDuplicates);
            Assert.IsFalse(target.HasKeywordInTitle);
        }

        [TestMethod]
        public void AnalyticsBFCTest()
        {
            LDPage target = new LDPage();
            IEnumerable<IProblemDescriptor> problems;
            IEnumerable<IFixDescriptor> fixes;
            IProblemDescriptor problem;
            IFixDescriptor fix;

            target.User = "user";
            target.PageType = PageType.Part;
            target.Name = "12345";
            target.Category = Category.Brick;
            target.BFC = CullingMode.Disabled;
            Assert.IsFalse(target.IsBFCMissing);
            Assert.IsFalse(target.IsBFCInvalid);
            target.BFC = CullingMode.CertifiedClockwise;
            Assert.IsFalse(target.IsBFCMissing);
            Assert.IsFalse(target.IsBFCInvalid);
            target.BFC = CullingMode.CertifiedCounterClockwise;
            Assert.IsFalse(target.IsBFCMissing);
            Assert.IsFalse(target.IsBFCInvalid);
            target.BFC = CullingMode.NotSet;
            Assert.IsTrue(target.IsBFCMissing);
            Assert.IsFalse(target.IsBFCInvalid);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_BFCMissing, problem.Guid);
            Assert.IsNotNull(problem.Fixes);
            fixes = problem.Fixes;
            Assert.AreEqual(3, fixes.Count());
            fix = fixes.ElementAt(0);
            Assert.AreEqual(LDPage.Fix_BFCSetCounterClockwise, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.IsBFCMissing);
            Assert.IsFalse(target.IsBFCInvalid);
            Assert.AreEqual(CullingMode.CertifiedCounterClockwise, target.BFC);
            fix = fixes.ElementAt(1);
            Assert.AreEqual(LDPage.Fix_BFCSetClockwise, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.IsBFCMissing);
            Assert.IsFalse(target.IsBFCInvalid);
            Assert.AreEqual(CullingMode.CertifiedClockwise, target.BFC);
            fix = fixes.ElementAt(2);
            Assert.AreEqual(LDPage.Fix_BFCSetUncertified, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.IsBFCMissing);
            Assert.IsFalse(target.IsBFCInvalid);
            Assert.AreEqual(CullingMode.Disabled, target.BFC);

            target.PageType = PageType.Model;
            target.BFC = CullingMode.NotSet;
            Assert.IsFalse(target.IsBFCMissing);
            Assert.IsFalse(target.IsBFCInvalid);

            target.PageType = PageType.Primitive;
            target.BFC = CullingMode.CertifiedClockwise;
            Assert.IsFalse(target.IsBFCMissing);
            Assert.IsTrue(target.IsBFCInvalid);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_BFCInvalid, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_BFCSetCounterClockwise, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.IsBFCInvalid);
            Assert.AreEqual(CullingMode.CertifiedCounterClockwise, target.BFC);
        }

        [TestMethod]
        public void AnalyticsLicenseTest()
        {
            LDPage target = new LDPage();
            IEnumerable<IProblemDescriptor> problems;
            IEnumerable<IFixDescriptor> fixes;
            IProblemDescriptor problem;
            IFixDescriptor fix;

            target.User = "user";
            target.PageType = PageType.Part;
            target.Name = "12345";
            target.Category = Category.Brick;
            target.BFC = CullingMode.CertifiedCounterClockwise;
            target.License = License.CCAL2;
            Assert.IsFalse(target.IsLicenseInvalid);
            target.License = License.NonCCAL;
            Assert.IsTrue(target.IsLicenseInvalid);
            target.License = License.None;
            Assert.IsTrue(target.IsLicenseInvalid);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_LicenseInvalid, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_LicenseSetCCAL2, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.IsFalse(target.IsLicenseInvalid);
            Assert.AreEqual(License.CCAL2, target.License);
        }

        [TestMethod]
        public void AnalyticsCategoryMissingTest()
        {
            LDPage target = new LDPage();
            IEnumerable<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target.User = "user";
            target.PageType = PageType.Part;
            target.Name = "12345";
            target.BFC = CullingMode.CertifiedCounterClockwise;
            target.Title = "Brick  1 x  1";
            Assert.IsFalse(target.IsCategoryMissing);
            Assert.AreEqual(Category.Brick, target.Category);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());

            target.Title = "Foo";
            Assert.IsTrue(target.IsCategoryMissing);
            Assert.AreEqual(Category.Unknown, target.Category);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_CategoryMissing, problem.Guid);
            Assert.IsNull(problem.Fixes);
            target.Category = Category.Tyre;
            Assert.IsFalse(target.IsCategoryMissing);
            Assert.AreEqual(Category.Tyre, target.Category);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());

            target.PageType = PageType.Model;
            target.BFC = CullingMode.NotSet;
            Assert.IsFalse(target.IsCategoryMissing);
            Assert.AreEqual(Category.Unknown, target.Category);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());

            target.PageType = PageType.Primitive;
            target.BFC = CullingMode.CertifiedCounterClockwise;
            Assert.IsFalse(target.IsCategoryMissing);
            Assert.AreEqual(Category.Primitive_Unknown, target.Category);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());
        }

        [TestMethod]
        public void AnalyticsCategoryMismatchTest()
        {
            LDPage target = new LDPage();
            IEnumerable<IProblemDescriptor> problems;
            IEnumerable<IFixDescriptor> fixes;
            IProblemDescriptor problem;
            IFixDescriptor fix;

            target.PageType = PageType.Part;
            target.Name = "12345";
            target.User = "user";
            target.BFC = CullingMode.CertifiedCounterClockwise;
            target.Title = "Brick  1 x  1";
            Assert.IsFalse(target.IsCategoryMismatch);
            Assert.AreEqual(Category.Brick, target.Category);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count());

            target.Category = Category.Gate;
            Assert.IsTrue(target.IsCategoryMismatch);
            Assert.AreEqual(Category.Gate, target.Category);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_CategoryMismatch, problem.Guid);
            Assert.IsNotNull(problem.Fixes);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDPage.Fix_CategoryClear, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsNotNull(fix.Action);
            Assert.IsNotNull(fix.Instruction);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(Category.Brick, target.Category);
        }

        [TestMethod]
        public void AnalyticsAuthorMissingTest()
        {
            LDPage target = new LDPage();
            IEnumerable<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target.PageType = PageType.Part;
            target.Name = "12345";
            target.User = "user";
            target.BFC = CullingMode.CertifiedCounterClockwise;
            target.Title = "Brick  1 x  1";
            target.Author = "author";
            Assert.IsFalse(target.IsAuthorMissing);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count());

            target.Author = null;
            Assert.IsTrue(target.IsAuthorMissing);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_AuthorMissing, problem.Guid);
            Assert.IsNull(problem.Fixes);
        }

        [TestMethod]
        public void AnalyticsUserMissingTest()
        {
            LDPage target = new LDPage();
            IEnumerable<IProblemDescriptor> problems;
            IProblemDescriptor problem;

            target.PageType = PageType.Part;
            target.Name = "12345";
            target.User = "user";
            target.BFC = CullingMode.CertifiedCounterClockwise;
            target.Title = "Brick  1 x  1";
            target.Author = "author";
            Assert.IsFalse(target.IsUserMissing);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count());

            target.User = null;
            Assert.IsTrue(target.IsUserMissing);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_UserMissing, problem.Guid);
            Assert.IsNull(problem.Fixes);

            // exception: if the Author is 'James Jessiman' then there never was and never will be a User
            target.Author = "James Jessiman";
            Assert.IsFalse(target.IsUserMissing);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(0, problems.Count());
        }

        //[TestMethod]
        public void AnalyticsOriginTest()
        {
            LDPage target = new LDPage();
            IEnumerable<IProblemDescriptor> problems;
            IEnumerable<IFixDescriptor> fixes;
            IProblemDescriptor problem;
            IFixDescriptor fix;

            target.User = "user";
            target.PageType = PageType.Part;
            target.Name = "12345";
            target.BFC = CullingMode.CertifiedCounterClockwise;
            target.Category = Category.Tyre;
            Assert.IsFalse(target.IsOriginOutsideBoundingBox);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());

            LDStep step = new LDStep();
            target.Add(step);

            // quad starting at the origin
            step.Add(new LDQuadrilateral("4 16 0 0 0 10 0 0 10 10 0 0 10 0"));
            Assert.IsFalse(target.IsOriginOutsideBoundingBox);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());
            step.Clear();

            // quad centred on the origin
            step.Add(new LDQuadrilateral("4 16 -10 -10 0 10 -10 0 10 10 0 -10 10 0"));
            Assert.IsFalse(target.IsOriginOutsideBoundingBox);
            Assert.AreEqual(0, target.Analyse(CodeStandards.OfficialModelRepository).Count());
            step.Clear();

            // quad away from the origin
            step.Add(new LDQuadrilateral("4 16 10 10 0 20 10 0 20 30 0 10 30 0"));
            Assert.AreEqual(new Box3d(10, 10, 0, 20, 30, 0), target.BoundingBox);
            Assert.IsTrue(target.IsOriginOutsideBoundingBox);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_OriginOutsideBoundingBox, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(3, fixes.Count());
            fix = fixes.ElementAt(0);
            Assert.AreEqual(LDPage.Fix_OriginCentreTop, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(new Box3d(-5, 0, 0, 5, 20, 0), target.BoundingBox);
            step.Clear();
            step.Add(new LDQuadrilateral("4 16 10 10 0 20 10 0 20 30 0 10 30 0"));
            Assert.AreEqual(new Box3d(10, 10, 0, 20, 30, 0), target.BoundingBox);
            Assert.IsTrue(target.IsOriginOutsideBoundingBox);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_OriginOutsideBoundingBox, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(3, fixes.Count());
            fix = fixes.ElementAt(1);
            Assert.AreEqual(LDPage.Fix_OriginCentreBottom, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(new Box3d(-5, -20, 0, 5, 0, 0), target.BoundingBox);
            step.Clear();
            step.Add(new LDQuadrilateral("4 16 10 10 0 20 10 0 20 30 0 10 30 0"));
            Assert.AreEqual(new Box3d(10, 10, 0, 20, 30, 0), target.BoundingBox);
            Assert.IsTrue(target.IsOriginOutsideBoundingBox);
            problems = target.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDPage.Problem_OriginOutsideBoundingBox, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(3, fixes.Count());
            fix = fixes.ElementAt(2);
            Assert.AreEqual(LDPage.Fix_OriginCentreBoundingBox, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual(new Box3d(-5, -10, 0, 5, 10, 0), target.BoundingBox);
            step.Clear();
        }

        #endregion Analytics

        #region Constructor

        [TestMethod]
        public void LDPageConstructorTest()
        {
            PageType type = PageType.Part;
            string name = "name";
            string title = "title";
            LDPage target = new LDPage(type, name, title);
            Assert.AreEqual(DOMObjectType.Page, target.ObjectType);
            Assert.AreEqual(type, target.PageType);
            Assert.AreEqual(name, target.Name);
            Assert.AreEqual(title, target.Title);
            Assert.AreEqual(CullingMode.CertifiedCounterClockwise, target.BFC);
        }

        [TestMethod]
        public void LDPageConstructorTest1()
        {
            LDPage target = new LDPage();
            Assert.AreEqual(PageType.Model, target.PageType);
            Assert.AreEqual("Untitled", target.Name);
            Assert.AreEqual("New Model", target.Title);
            Assert.AreEqual(CullingMode.NotSet, target.BFC);
        }

        [TestMethod]
        public void LDPageConstructorTest2()
        {
            PageType type = PageType.Primitive;
            LDPage target = new LDPage(type);
            Assert.AreEqual(type, target.PageType);
            Assert.AreEqual("Untitled", target.Name);
            Assert.AreEqual("New Primitive", target.Title);
            Assert.AreEqual(Digitalis.LDTools.DOM.Configuration.Author, target.Author);
            Assert.AreEqual(Digitalis.LDTools.DOM.Configuration.Username, target.User);
            Assert.AreEqual(License.CCAL2, target.License);
            Assert.AreEqual(CullingMode.CertifiedCounterClockwise, target.BFC);

            List<LDHistory> history = new List<LDHistory>(target.History);
            Assert.AreEqual(1, history.Count);
            Assert.AreEqual(Environment.UserName, history[0].Name);
            Assert.IsTrue(history[0].IsRealName);
        }

        #endregion Constructor

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            LDPage page         = new LDPage();
            LDDocument document = new LDDocument();

            Assert.AreEqual(0, page.ChangedSubscribers);
            document.Add(page);
            Assert.AreEqual(1, page.ChangedSubscribers);
            page.Dispose();
            Assert.AreEqual(0, page.ChangedSubscribers);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Document-tree

        [TestMethod]
        public override void DocumentTest()
        {
            LDPage page         = new LDPage();
            LDDocument document = new LDDocument();

            Assert.AreEqual(0, page.ChangedSubscribers);
            ((IPage)page).Document = document;
            Assert.AreEqual(1, page.ChangedSubscribers);
            ((IPage)page).Document = null;
            Assert.AreEqual(0, page.ChangedSubscribers);

            base.DocumentTest();
        }

        #endregion Document-tree

        #region Parser

        [TestMethod]
        public void ParserTest_Repository()
        {
            // load every document in the repository and make sure none of them cause exceptions
            string resultsFile = @"C:\parserresults.txt";
            bool passed = true;
            int count = 0;

            if (null == LibraryManager.Cache)
                LibraryManager.Load(delegate(int progress, string status, string partname) { return true; }, false);

            using (TextWriter writer = new StreamWriter(resultsFile))
            {
                foreach (IndexCard card in LibraryManager.Cache)
                {
                    string name = card.Filepath;

                    try
                    {
                        LDDocument doc = new LDDocument(name, ParseFlags.None);
                        doc.Dispose();
                    }
                    catch (Exception e)
                    {
                        writer.WriteLine(name + "\r\n------------------------------------------------------------\r\n" + e + "\r\n\r\n");
                        passed = false;
                    }

                    count++;
                }
            }

            if (passed)
                File.Delete(resultsFile);

            int loaded = LibraryManager.Cache.Count;
            LibraryManager.Unload();

            Assert.AreEqual(loaded, count);
            Assert.IsTrue(passed);
        }

        [TestMethod]
        public void SetTypeFromFilePathTest()
        {
            LDPage_Accessor target = new LDPage_Accessor();

            Assert.AreEqual(PageType.Model, target.PageType);

            // filepath prefixes
            target.SetTypeFromFilePath(@"s\name.dat", "name.dat", false);
            Assert.AreEqual(PageType.Subpart, target.PageType);
            target.SetTypeFromFilePath(@"48\name.dat", "name.dat", false);
            Assert.AreEqual(PageType.HiresPrimitive, target.PageType);

            // name prefixes
            target.SetTypeFromFilePath("name.dat", @"s\name.dat", false);
            Assert.AreEqual(PageType.Subpart, target.PageType);
            target.SetTypeFromFilePath("name.dat", @"48\name.dat", false);
            Assert.AreEqual(PageType.HiresPrimitive, target.PageType);

            // filepath prefixes take precedence over the name
            target.SetTypeFromFilePath(@"s\name.dat", @"48\name.dat", false);
            Assert.AreEqual(PageType.Subpart, target.PageType);
            target.SetTypeFromFilePath(@"48\name.dat", @"s\name.dat", false);
            Assert.AreEqual(PageType.HiresPrimitive, target.PageType);

            // filepath suffixes
            target.SetTypeFromFilePath("name.dat", "name.dat", false);
            Assert.AreEqual(PageType.Part, target.PageType);
            target.SetTypeFromFilePath("name.ldr", "name.dat", false);
            Assert.AreEqual(PageType.Model, target.PageType);

            // invalid filepath suffix, so check the name
            target.SetTypeFromFilePath("name.foo", "name.dat", false);
            Assert.AreEqual(PageType.Part, target.PageType);
            target.SetTypeFromFilePath("name.foo", "name.ldr", false);
            Assert.AreEqual(PageType.Model, target.PageType);
        }

        [TestMethod]
        public void ParserTest_Main()
        {
            // basic tests using strings
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            // document with no header
            document = "1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n";
            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, doc.Count);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDReference));

            document = "0 BFC INVERTNEXT\r\n" +
                       "1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n";
            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, doc.Count);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDReference));
            Assert.IsTrue((page.Elements[0] as LDReference).Invert);

            document = "0 // comment\r\n" +
                       "1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n";
            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, doc.Count);
            page = doc[0];
            Assert.AreEqual(2, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            Assert.IsInstanceOfType(page.Elements[1], typeof(LDReference));

            document = "0 STEP\r\n" +
                       "1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n";
            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, doc.Count);
            page = doc[0];
            Assert.AreEqual(0, page[0].Count);
            Assert.AreEqual(1, page[1].Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDReference));

            // single-page document with valid Official Part header
            document = "0 Part Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "0 Author: authorname [username]\r\n" +
                       "0 !LDRAW_ORG Part ORIGINAL\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "0 !HELP more help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Brick\r\n" +
                       "0 !KEYWORDS keyword 1, keyword 2\r\n" +
                       "0 !KEYWORDS keyword 3, keyword 4\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-27 [username] description of change\r\n" +
                       "0 !HISTORY 2012-08-31 [username] description of change\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +
                       "0 ROTATION CONFIG -3 1\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, doc.Count);
            page = doc[0];
            Assert.AreEqual("Part Name.dat", page.TargetName);
            Assert.AreEqual("Part Title", page.Title);
            Assert.AreEqual("Part Name", page.Name);
            Assert.AreEqual("authorname", page.Author);
            Assert.AreEqual("username", page.User);
            Assert.AreEqual(PageType.Part, page.PageType);
            Assert.IsNotNull(page.Update);
            Assert.AreEqual(License.CCAL2, page.License);
            Assert.AreEqual("help text\r\nmore help text", page.Help);
            Assert.AreEqual(CullingMode.CertifiedCounterClockwise, page.BFC);
            Assert.AreEqual(Category.Brick, page.Category);
            Assert.AreEqual(4, page.Keywords.Count());
            Assert.AreEqual("keyword 1", page.Keywords.First());
            Assert.AreEqual("keyword 2", page.Keywords.ElementAt(1));
            Assert.AreEqual("keyword 3", page.Keywords.ElementAt(2));
            Assert.AreEqual("keyword 4", page.Keywords.ElementAt(3));
            Assert.AreEqual(1U, page.DefaultColour);
            Assert.AreEqual(2, page.History.Count());
            Assert.AreEqual(1, page.RotationConfig.Count());
            Assert.AreEqual(MLCadRotationConfig.Type.WorldOrigin, page.RotationPoint);
            Assert.IsTrue(page.RotationPointVisible);
            Assert.IsTrue(page.InlineOnPublish);

            // add some elements
            document += "\r\n" +

                        // the six basic elements, including the empty comment and the blank line
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "2 24 0 0 0 10 10 10\r\n" +
                        "3 16 0 0 0 10 10 10 20 20 20\r\n" +
                        "4 16 0 0 0 10 10 10 20 20 20 30 30 30\r\n" +
                        "5 24 0 0 0 10 10 10 20 20 20 30 30 30\r\n" +
                        "0 comment\r\n" +
                        "0\r\n" +
                        "\r\n" +

                        // total: 7

                        // meta-commands
                        "0 WRITE message\r\n" +
                        "0 SAVE\r\n" +

                        // total: 2

                        "0 STEP\r\n" +

                        "0 PRINT\r\n" +
                        "0 PAUSE\r\n" +
                        "0 CLEAR\r\n" +
                        "0 !COLOUR Black CODE 0 VALUE #212121 EDGE #595959\r\n" +
                        "0 COLOUR 0 Black 0 33 33 33 255 33 33 33 255\r\n" +
                        "0 COLOR 0 Black 0 33 33 33 255 33 33 33 255\r\n" +

                        // total: 6

                        // BFC flags
                        "0 BFC CCW\r\n" +

                        // total: 1

                        // single attributes
                        "0 BFC INVERTNEXT\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 BFC CERTIFY INVERTNEXT\r\n" +                    // handle the MLCad bug
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 BFC INVERTNEXT\r\n" +                            // verify that the INVERTNEXT isn't lost if there are blank lines after it
                        "\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 BFC INVERTNEXT\r\n" +                            // verify that the INVERTNEXT isn't lost if there are comments after it
                        "0 comment\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 GHOST 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 MLCAD HIDE 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +

                        // total: 8

                        // multiple attributes
                        "0 BFC INVERTNEXT\r\n" +                            // verify that the INVERTNEXT isn't lost if there are other meta-commands after it
                        "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 BFC INVERTNEXT\r\n" +
                        "0 GHOST 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 BFC INVERTNEXT\r\n" +
                        "0 MLCAD HIDE 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 BFC INVERTNEXT\r\n" +
                        "0 GHOST 0 MLCAD HIDE 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                        "0 BFC INVERTNEXT\r\n" +
                        "0 GHOST 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                        "0 BFC INVERTNEXT\r\n" +
                        "0 GHOST 0 MLCAD HIDE 1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                        "0 BFC INVERTNEXT\r\n" +
                        "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +           // this order shouldn't happen, but make sure it works just in case
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +

                        // total: 7

                        // groups
                        "0 GROUP 2 group name\r\n" +
                        "0 MLCAD BTG group name\r\n" +
                        "0 comment\r\n" +
                        "0 MLCAD BTG group name\r\n" +
                        "0 comment\r\n" +
                        "0 GROUP 2 Group name\r\n" +
                        "0 MLCAD BTG Group name\r\n" +
                        "0 Comment\r\n" +
                        "0 MLCAD BTG Group name\r\n" +
                        "0 Comment\r\n" +

                        // total: 6

                        // verify that attributes are correctly cleared if followed by a graphic other than a reference
                        "0 BFC INVERTNEXT\r\n" +
                        "2 24 0 0 0 10 10 10\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +       // this should not be inverted
                        "0 BFC INVERTNEXT\r\n" +
                        "3 16 0 0 0 10 10 10 20 20 20\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +       // this should not be inverted
                        "0 BFC INVERTNEXT\r\n" +
                        "4 16 0 0 0 10 10 10 20 20 20 30 30 30\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +       // this should not be inverted
                        "0 BFC INVERTNEXT\r\n" +
                        "5 24 0 0 0 10 10 10 20 20 20 30 30 30\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +       // this should not be inverted
                        "0 BFC INVERTNEXT\r\n" +
                        "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                        "0 MLCAD BTG group 3\r\n" +
                        "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +       // this should not be locked or inverted as the attributes are not members of the group
                        "0 GROUP 1 group 3\r\n" +

                        // total: 10

                        // duplicate group-name
                        "0 GROUP 1 group name\r\n" +
                        "0 MLCAD BTG group name\r\n" +
                        "0 comment\r\n" +

                        // total: 1 (because 'group name' is a duplicate so should be merged)

                        // missing GROUP definition
                        "0 MLCAD BTG missing group\r\n" +
                        "0 comment\r\n" +

                        // total: 1 (because 'missing group' should be auto-generated at the end of the document)

                        "\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, doc.Count);
            page = doc[0];
            Assert.AreEqual(2, page.Count);
            Assert.IsInstanceOfType(page[0], typeof(LDStep));
            Assert.IsInstanceOfType(page[1], typeof(LDStep));

            IStep step = page[0];
            Assert.AreEqual(9, step.Count);

            Assert.IsInstanceOfType(step[0], typeof(LDReference));
            Assert.IsInstanceOfType(step[1], typeof(LDLine));
            Assert.IsInstanceOfType(step[2], typeof(LDTriangle));
            Assert.IsInstanceOfType(step[3], typeof(LDQuadrilateral));
            Assert.IsInstanceOfType(step[4], typeof(LDOptionalLine));
            Assert.IsInstanceOfType(step[5], typeof(LDComment));
            Assert.IsInstanceOfType(step[6], typeof(LDComment));

            Assert.IsInstanceOfType(step[7], typeof(LDWrite));
            Assert.IsInstanceOfType(step[8], typeof(LDSave));

            step = page[1];
            Assert.AreEqual(41, step.Count);

            Assert.IsInstanceOfType(step[0], typeof(LDWrite));     // PRINT
            Assert.IsInstanceOfType(step[1], typeof(LDPause));
            Assert.IsInstanceOfType(step[2], typeof(LDClear));
            Assert.IsInstanceOfType(step[3], typeof(LDColour));
            Assert.IsInstanceOfType(step[4], typeof(LDColour));
            Assert.IsInstanceOfType(step[5], typeof(LDColour));

            Assert.IsInstanceOfType(step[6], typeof(LDBFCFlag));

            Assert.IsInstanceOfType(step[7], typeof(LDReference));
            Assert.IsTrue((step[7] as LDReference).Invert);
            Assert.IsInstanceOfType(step[8], typeof(LDReference));
            Assert.IsTrue((step[8] as LDReference).Invert);        // MLCad bug
            Assert.IsInstanceOfType(step[9], typeof(LDReference));
            Assert.IsTrue((step[9] as LDReference).Invert);        // blank line after INVERTNEXT
            Assert.IsInstanceOfType(step[10], typeof(LDComment));
            Assert.IsInstanceOfType(step[11], typeof(LDReference));
            Assert.IsTrue((step[11] as LDReference).Invert);        // comment after INVERTNEXT
            Assert.IsInstanceOfType(step[12], typeof(LDReference));
            Assert.IsFalse((step[12] as LDReference).Invert);
            Assert.IsTrue(step[12].IsLocked);
            Assert.IsInstanceOfType(step[13], typeof(LDReference));
            Assert.IsFalse(step[13].IsLocked);
            Assert.IsTrue((step[13] as LDReference).IsGhosted);
            Assert.IsInstanceOfType(step[14], typeof(LDReference));
            Assert.IsFalse((step[14] as LDReference).IsVisible);

            Assert.IsInstanceOfType(step[15], typeof(LDReference));
            Assert.IsTrue((step[15] as LDReference).Invert);
            Assert.IsTrue(step[15].IsLocked);
            Assert.IsInstanceOfType(step[16], typeof(LDReference));
            Assert.IsTrue((step[16] as LDReference).Invert);
            Assert.IsFalse(step[16].IsLocked);
            Assert.IsTrue((step[16] as LDReference).IsGhosted);
            Assert.IsInstanceOfType(step[17], typeof(LDReference));
            Assert.IsTrue((step[17] as LDReference).Invert);
            Assert.IsFalse((step[17] as LDReference).IsVisible);
            Assert.IsInstanceOfType(step[18], typeof(LDReference));
            Assert.IsTrue((step[18] as LDReference).Invert);
            Assert.IsTrue((step[18] as LDReference).IsGhosted);
            Assert.IsFalse((step[18] as LDReference).IsVisible);
            Assert.IsInstanceOfType(step[19], typeof(LDReference));
            Assert.IsTrue((step[19] as LDReference).Invert);
            Assert.IsTrue((step[19] as LDReference).IsGhosted);
            Assert.IsTrue(step[19].IsLocked);
            Assert.IsInstanceOfType(step[20], typeof(LDReference));
            Assert.IsTrue((step[20] as LDReference).Invert);
            Assert.IsTrue((step[20] as LDReference).IsGhosted);
            Assert.IsFalse((step[20] as LDReference).IsVisible);
            Assert.IsTrue(step[20].IsLocked);
            Assert.IsInstanceOfType(step[21], typeof(LDReference));
            Assert.IsTrue((step[21] as LDReference).Invert);            // invert & locked in the wrong order
            Assert.IsTrue(step[21].IsLocked);

            Assert.IsInstanceOfType(step[22], typeof(MLCadGroup));
            Assert.AreEqual(3, (step[22] as MLCadGroup).Count);         // should include members of both instances of 'group name'
            Assert.IsInstanceOfType(step[23], typeof(LDComment));
            Assert.IsInstanceOfType(step[24], typeof(LDComment));
            Assert.IsInstanceOfType(step[25], typeof(MLCadGroup));
            Assert.AreEqual(2, (step[25] as MLCadGroup).Count);
            Assert.IsInstanceOfType(step[26], typeof(LDComment));
            Assert.IsInstanceOfType(step[27], typeof(LDComment));

            Assert.IsInstanceOfType(step[28], typeof(LDLine));
            Assert.IsInstanceOfType(step[29], typeof(LDReference));
            Assert.IsFalse((step[29] as LDReference).Invert);
            Assert.IsInstanceOfType(step[30], typeof(LDTriangle));
            Assert.IsInstanceOfType(step[31], typeof(LDReference));
            Assert.IsFalse((step[31] as LDReference).Invert);
            Assert.IsInstanceOfType(step[32], typeof(LDQuadrilateral));
            Assert.IsInstanceOfType(step[33], typeof(LDReference));
            Assert.IsFalse((step[33] as LDReference).Invert);
            Assert.IsInstanceOfType(step[34], typeof(LDOptionalLine));
            Assert.IsInstanceOfType(step[35], typeof(LDReference));
            Assert.IsFalse((step[35] as LDReference).Invert);
            Assert.IsInstanceOfType(step[36], typeof(LDReference));
            Assert.IsFalse((step[36] as LDReference).Invert);
            Assert.IsFalse((step[36] as LDReference).IsLocked);
            Assert.IsInstanceOfType(step[37], typeof(MLCadGroup));

            Assert.IsInstanceOfType(step[38], typeof(LDComment));

            Assert.IsInstanceOfType(step[39], typeof(LDComment));

            Assert.IsInstanceOfType(step[step.Count - 1], typeof(MLCadGroup));      // this one should always be at the end of the step as it's an auto-generated group
        }

        [TestMethod]
        public void ParserTest_PartsCategories()
        {
            // check categories parse correctly
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            foreach (Category category in Enum.GetValues(typeof(Category)))
            {
                if (Category.Primitive_Unknown == category)
                    break;

                document = "0 Part Title\r\n" +
                           "0 Name: Part Name.dat\r\n" +
                           "0 !CATEGORY " + Digitalis.LDTools.DOM.LDTranslationCatalog.GetCategory(category) + "\r\n";

                doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
                page = doc[0];
                Assert.AreEqual(category, page.Category);
            }
        }

        [TestMethod]
        public void PrimitivesAutoCategoriesTest()
        {
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            document = "0 Title\r\n" +
                       "0 Name: 4-4ring4.dat\r\n" +
                       "0 !LDRAW_ORG Primitive\r\n";
            doc = new LDDocument(new StringReader(document), "4-4ring4.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(PageType.Primitive, page.PageType);
            Assert.AreEqual(Category.Primitive_Ring, page.Category);
        }

        [TestMethod]
        public void PartsAutoCategoriesTest()
        {
            // if no !CATEGORY line is present, the page should try and figure out the category from the first word of the Title
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                string name = "Part Name." + ((PageType.Model == type) ? "ldr" : "dat");
                string filePath;
                string typeName;

                switch (type)
                {
                    case PageType.Part_Physical_Colour:
                        typeName = "Part Physical_Colour";
                        break;

                    case PageType.Shortcut_Physical_Colour:
                        typeName = "Shortcut Physical_Colour";
                        break;

                    case PageType.Part_Alias:
                        typeName = "Part Alias";
                        break;

                    case PageType.Shortcut_Alias:
                        typeName = "Shortcut Alias";
                        break;

                    case PageType.HiresPrimitive:
                        typeName = "48_Primitive";
                        break;

                    default:
                        typeName = type.ToString();
                        break;
                }

                foreach (Category category in Enum.GetValues(typeof(Category)))
                {
                    if (Category.Primitive_Unknown == category)
                        break;

                    if (PageType.Subpart == type)
                        filePath = @"s\" + name;
                    else if (PageType.HiresPrimitive == type)
                        filePath = @"48\" + name;
                    else
                        filePath = name;

                    // note that this only works correctly if the locale is English, as GetCategory() returns localized strings
                    document = "0 " + Digitalis.LDTools.DOM.LDTranslationCatalog.GetCategory(category).Split()[0] + " Part Title\r\n" +
                               "0 Name: " + filePath + "\r\n" +
                               "0 !LDRAW_ORG " + typeName + "\r\n";

                    doc = new LDDocument(new StringReader(document), filePath, null, ParseFlags.None, out documentModified);
                    page = doc[0];
                    Assert.AreEqual(type, page.PageType);

                    switch (type)
                    {
                        case PageType.Model:
                            Assert.AreEqual(Category.Unknown, page.Category);
                            break;

                        case PageType.Primitive:
                        case PageType.HiresPrimitive:

                            // skip these
                            break;

                        default:
                            switch (category)
                            {
                                case Category.FigureAccessory:
                                    Assert.AreEqual(Category.Figure, page.Category);
                                    break;

                                case Category.MinifigAccessory:
                                case Category.MinifigFootwear:
                                case Category.MinifigHeadwear:
                                case Category.MinifigHipwear:
                                case Category.MinifigNeckwear:
                                    Assert.AreEqual(Category.Minifig, page.Category);
                                    break;

                                default:
                                    Assert.AreEqual(category, page.Category);
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void ParserTest_PageTermination()
        {
            // parser for '0 FILE' / '0 NOFILE' termination
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            // valid mpd structure with NOFILE
            document = "0 FILE page1.dat\r\n" +
                       "0 Page 1\r\n" +
                       "0 Name: page1.dat\r\n" +
                       "0 comment\r\n" +
                       "0 NOFILE\r\n" +
                       "0 this should be skipped\r\n" +
                       "as should this\r\n" +
                       "0 FILE page2.dat\r\n" +
                       "0 Page 2\r\n" +
                       "0 Name: page2.dat\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.mpd", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(2, doc.Count);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            page = doc[1];
            Assert.AreEqual(0, page.Elements.Count);

            // valid mpd structure without NOFILE
            document = "0 FILE page1.dat\r\n" +
                       "0 Page 1\r\n" +
                       "0 Name: page1.dat\r\n" +
                       "0 comment\r\n" +
                       "0 this should not be skipped\r\n" +
                       "0 FILE page2.dat\r\n" +
                       "0 Page 2\r\n" +
                       "0 Name: page2.dat\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.mpd", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(2, doc.Count);
            page = doc[0];
            Assert.AreEqual(2, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            page = doc[1];
            Assert.AreEqual(0, page.Count);

            // valid spd structure
            document = "0 Page 1\r\n" +
                       "0 Name: page1.dat\r\n" +
                       "0 comment\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, doc.Count);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);

            // FILE without a filename - should be treated as the page's title
            document = "0 FILE\r\n" +
                       "0 Page 1\r\n" +     // this should become a comment
                       "0 Name: page1.dat\r\n" +
                       "0 comment\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.mpd", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, doc.Count);
            page = doc[0];
            Assert.AreEqual("FILE", page.Title);
            Assert.AreEqual(2, page.Elements.Count);

            // NOFILE with extra stuff - should be treated as a comment
            document = "0 FILE page1.dat\r\n" +
                       "0 Page 1\r\n" +
                       "0 Name: page1.dat\r\n" +
                       "0 comment\r\n" +
                       "0 NOFILE more text\r\n" +
                       "0 FILE page2.dat\r\n" +
                       "0 Page 2\r\n" +
                       "0 Name: page2.dat\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(2, doc.Count);
            page = doc[0];
            Assert.AreEqual(2, page.Elements.Count);
            page = doc[1];
            Assert.AreEqual(0, page.Count);

            // invalid mpd structure (missing first FILE) - should use the filename instead (but with the correct .dat extension)
            document = "0 Page 1\r\n" +
                       "0 Name: page1.dat\r\n" +
                       "0 comment\r\n" +
                       "0 FILE page2.dat\r\n" +
                       "0 Page 2\r\n" +
                       "0 Name: page2.dat\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.mpd", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(2, doc.Count);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);
            Assert.AreEqual("Part Name.dat", page.TargetName);
            page = doc[1];
            Assert.AreEqual(0, page.Count);
            Assert.AreEqual("page2.dat", page.TargetName);

            // invalid spd structure (ends with NOFILE) - should terminate normally
            document = "0 Page 1\r\n" +
                       "0 Name: page1.dat\r\n" +
                       "0 comment\r\n" +
                       "0 NOFILE\r\n" +
                       "0 Page 2\r\n" +
                       "0 Name: page2.dat\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, doc.Count);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);
        }

        [TestMethod]
        public void ParserTest_TrailingComments()
        {
            // parser should strip trailing empty comments
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            document = "0 Part Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                       "0 comment\r\n" +
                       "0\r\n" +
                       "0 comment\r\n" +

                       // these three should be stripped off
                       "0 \r\n" +
                       "0 //\r\n" +
                       "0\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(4, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDReference));
            Assert.IsInstanceOfType(page.Elements[1], typeof(LDComment));
            Assert.IsInstanceOfType(page.Elements[2], typeof(LDComment));
            Assert.IsInstanceOfType(page.Elements[3], typeof(LDComment));
        }

        [TestMethod]
        public void ParserTest_MetacommandTitles()
        {
            // parser should cope with Titles which look like a meta-command
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            document = "0 GHOST Title\r\n" +
                       "0 Name: Part Name.dat\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual("GHOST Title", page.Title);

            document = "0 MLCAD Title\r\n" +
                       "0 Name: Part Name.dat\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual("MLCAD Title", page.Title);

            document = "0 MLCAD HIDE Title\r\n" +
                       "0 Name: Part Name.dat\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual("MLCAD HIDE Title", page.Title);
        }

        [TestMethod]
        public void ParserTest_MetacommandComments()
        {
            // parser should cope with comments which look like a meta-command but aren't
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            document = "0 Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "0 GHOST comment\r\n";           // malformedMLCad command, should be deactivated

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            Assert.AreEqual("// GHOST comment", (page.Elements[0] as LDComment).Text);

            document = "0 Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "0 MLCAD comment\r\n";           // malformed MLCad command, should be deactivated

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            Assert.AreEqual("// MLCAD comment", (page.Elements[0] as LDComment).Text);

            document = "0 Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "0 MLCAD HIDE comment\r\n";      // malformed MLCad command, should be deactivated

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            Assert.AreEqual("// MLCAD HIDE comment", (page.Elements[0] as LDComment).Text);

            document = "0 Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "0 Name: blah\r\n";              // duplicate 'Name:' command, should be deactivated

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            Assert.AreEqual("// Name: blah", (page.Elements[0] as LDComment).Text);

            document = "0 Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "0 !LDRAW_ORG Part\r\n" +
                       "0 Model description\r\n";       // a comment, should not be deactivated as 'model' is a common word

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(PageType.Part, page.PageType);
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            Assert.AreEqual("Model description", (page.Elements[0] as LDComment).Text);

            document = "0 Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "0 !LDRAW_ORG Part\r\n" +
                       "0 Colour\r\n";                  // a comment, should not be deactivated as 'colour' is a common word

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(PageType.Part, page.PageType);
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            Assert.AreEqual("Colour", (page.Elements[0] as LDComment).Text);

            document = "0 Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "0 !LDRAW_ORG Part\r\n" +
                       "0 Colour some more text\r\n";                  // a comment, should not be deactivated as 'colour' is a common word

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(PageType.Part, page.PageType);
            Assert.AreEqual(1, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            Assert.AreEqual("Colour some more text", (page.Elements[0] as LDComment).Text);
        }

        [TestMethod]
        public void ParserTest_NameAndType()
        {
            // parser for Name and Type
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            // valid document
            document = "0 Title\r\n" +
                       "0 Name: Name.dat\r\n" +
                       "0 !LDRAW_ORG Part\r\n";

            doc = new LDDocument(new StringReader(document), "Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual("Name.dat", page.TargetName);
            Assert.AreEqual("Name", page.Name);
            Assert.AreEqual(PageType.Part, page.PageType);

            // mismatched Name/Type: should resolve as a Model
            document = "0 Title\r\n" +
                       "0 Name: Name.dat\r\n" +
                       "0 !LDRAW_ORG Model\r\n";

            doc = new LDDocument(new StringReader(document), "Name.ldr", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual("Name.ldr", page.TargetName);
            Assert.AreEqual("Name", page.Name);
            Assert.AreEqual(PageType.Model, page.PageType);

            // mismatched Name/Type: should resolve as a Model
            document = "0 Title\r\n" +
                       "0 Name: Name.ldr\r\n" +
                       "0 !LDRAW_ORG Part\r\n";

            doc = new LDDocument(new StringReader(document), "Name.ldr", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual("Name.ldr", page.TargetName);
            Assert.AreEqual("Name", page.Name);
            Assert.AreEqual(PageType.Model, page.PageType);

            // mismatched Name/Type: should resolve as a Subpart
            document = "0 Title\r\n" +
                       "0 Name: s\\Name.dat\r\n" +
                       "0 !LDRAW_ORG Part\r\n";

            doc = new LDDocument(new StringReader(document), "Name.ldr", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(@"s\Name.dat", page.TargetName);
            Assert.AreEqual("Name", page.Name);
            Assert.AreEqual(PageType.Subpart, page.PageType);

            // mismatched Name/Type: should resolve as a HiresPrimitive
            document = "0 Title\r\n" +
                       "0 Name: 48\\Name.dat\r\n" +
                       "0 !LDRAW_ORG Part\r\n";

            doc = new LDDocument(new StringReader(document), "Name.ldr", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(@"48\Name.dat", page.TargetName);
            Assert.AreEqual("Name", page.Name);
            Assert.AreEqual(PageType.HiresPrimitive, page.PageType);
        }

        [TestMethod]
        public void ParserTest_NameAndType_Prefixes()
        {
            string document;
            IPage page;
            IDocument doc;
            bool documentModified;

            // Type which requires a prefix
            document = "0 Title\r\n" +
                       "0 Name: Name.dat\r\n" +
                       "0 !LDRAW_ORG Subpart\r\n";

            doc = new LDDocument(new StringReader(document), "Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(@"s\Name.dat", page.TargetName);
            Assert.AreEqual("Name", page.Name);
            Assert.AreEqual(PageType.Subpart, page.PageType);

            // Type which requires a prefix
            document = "0 Title\r\n" +
                       "0 Name: Name.dat\r\n" +
                       "0 !LDRAW_ORG 48_Primitive\r\n";

            doc = new LDDocument(new StringReader(document), "Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(@"48\Name.dat", page.TargetName);
            Assert.AreEqual("Name", page.Name);
            Assert.AreEqual(PageType.HiresPrimitive, page.PageType);
        }

        [TestMethod]
        public void ParseTypeLineTest_UnofficialFormats()
        {
            LDPage_Accessor target = new LDPage_Accessor();
            string line;
            string[] fields;

            // these two are typically found in user-generated files
            line = "0 Model";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Model, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 Unofficial Model";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Model, target.PageType);
            Assert.IsNull(target.Update);

            // these are the non-standard formats currently known to exist in the parts-library, so we should handle them
            line = "0 Unofficial Part";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 Unofficial Shortcut";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Shortcut, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 Unofficial LDraw sub-part";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Subpart, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 Unofficial subfile";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Subpart, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 Unofficial Sub-Part";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Subpart, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 Unofficial Element";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 Unofficial LCad Part";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 Official LCad update 99-04";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNotNull(target.Update);
            Assert.AreEqual(1999U, ((LDUpdate)target.Update).Year);
            Assert.AreEqual(4U, ((LDUpdate)target.Update).Release);

            line = "0 Official LCad Part 2000-01";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNotNull(target.Update);
            Assert.AreEqual(2000U, ((LDUpdate)target.Update).Year);
            Assert.AreEqual(1U, ((LDUpdate)target.Update).Release);

            line = "0 Official LCad Part - 2000-01 UPDATE";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNotNull(target.Update);
            Assert.AreEqual(2000U, ((LDUpdate)target.Update).Year);
            Assert.AreEqual(1U, ((LDUpdate)target.Update).Release);

            line = "0 Official LCad Subpart - 2000-01 UPDATE";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Subpart, target.PageType);
            Assert.IsNotNull(target.Update);
            Assert.AreEqual(2000U, ((LDUpdate)target.Update).Year);
            Assert.AreEqual(1U, ((LDUpdate)target.Update).Release);
        }

        [TestMethod]
        public void ParseTypeLineTest_OfficialFormat()
        {
            LDPage_Accessor target = new LDPage_Accessor();
            string line;
            string[] fields;

            // the official format: // 0 !LDRAW_ORG [Unofficial_]<type> [<qualifier>] [ORIGINAL|UPDATE YYYY-RR]
            line = "0 !LDRAW_ORG Model";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Model, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Part";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Subpart";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Subpart, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Primitive";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Primitive, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG 48_Primitive";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.HiresPrimitive, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Shortcut";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Shortcut, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Part Alias";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part_Alias, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Part Physical_Colour";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part_Physical_Colour, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Shortcut Alias";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Shortcut_Alias, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Shortcut Physical_Colour";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Shortcut_Physical_Colour, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Unofficial_Part";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNull(target.Update);

            line = "0 !LDRAW_ORG Part ORIGINAL";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNotNull(target.Update);
            Assert.AreEqual(0U, ((LDUpdate)target.Update).Year);
            Assert.AreEqual(0U, ((LDUpdate)target.Update).Release);

            line = "0 !LDRAW_ORG Part UPDATE 2012-01";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNotNull(target.Update);
            Assert.AreEqual(2012U, ((LDUpdate)target.Update).Year);
            Assert.AreEqual(1U, ((LDUpdate)target.Update).Release);

            line = "0 !LDRAW_ORG Part 2012-01 UPDATE";
            fields = line.Split(LDPage_Accessor.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(target.ParseTypeLine(fields));
            Assert.AreEqual(PageType.Part, target.PageType);
            Assert.IsNotNull(target.Update);
            Assert.AreEqual(2012U, ((LDUpdate)target.Update).Year);
            Assert.AreEqual(1U, ((LDUpdate)target.Update).Release);
        }

        private class RepositoryParams
        {
            public LDPage.NameFormatProblem IsNameFormatInvalid;

            public RepositoryParams(LDPage.NameFormatProblem isNameFormatInvalid)
            {
                IsNameFormatInvalid = isNameFormatInvalid;
            }
        }

        [TestMethod]
        public void ParserTest_Groups()
        {
            // parser should handle grouped elements with multiple attributes
            string document;
            IPage page;
            IDocument doc;
            LDReference r;
            MLCadGroup group;
            bool documentModified;

            document = "0 Part Title\r\n" +
                       "0 Name: Part Name.dat\r\n" +
                       "0 MLCAD BTG group\r\n" +
                       "0 BFC INVERTNEXT\r\n" +
                       "0 MLCAD BTG group\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                       "0 MLCAD BTG group\r\n" +
                       "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                       "0 MLCAD BTG group\r\n" +
                       "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                       "0 MLCAD BTG group\r\n" +
                       "0 comment\r\n" +
                       "0 GROUP 3 group\r\n";

            doc = new LDDocument(new StringReader(document), "Part Name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(4, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDReference));
            Assert.IsInstanceOfType(page.Elements[1], typeof(LDReference));
            Assert.IsInstanceOfType(page.Elements[2], typeof(LDComment));
            Assert.IsInstanceOfType(page.Elements[3], typeof(MLCadGroup));
            group = page.Elements[3] as MLCadGroup;
            Assert.AreEqual(3, group.Count);
            Assert.IsFalse(group.IsLocked);
            Assert.IsInstanceOfType(group.ElementAt(0), typeof(LDReference));
            r = group.ElementAt(0) as LDReference;
            Assert.IsTrue(r.Invert);
            Assert.IsTrue(r.IsLocked);
            Assert.IsInstanceOfType(group.ElementAt(1), typeof(LDReference));
            r = group.ElementAt(1) as LDReference;
            Assert.IsFalse(r.Invert);
            Assert.IsFalse(r.IsLocked);
            Assert.IsInstanceOfType(group.ElementAt(2), typeof(LDComment));
            Assert.IsFalse(group.ElementAt(2).IsLocked);
        }

        [TestMethod]
        public void AllowsCategoryTest()
        {
            LDPage_Accessor target = new LDPage_Accessor();

            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Primitive == type || PageType.HiresPrimitive == type || PageType.Model == type)
                    Assert.IsFalse(LDPage.AllowsCategory(type));
                else
                    Assert.IsTrue(LDPage.AllowsCategory(type));
            }
        }

        [TestMethod]
        public void AllowsThemeTest()
        {
            LDPage_Accessor target = new LDPage_Accessor();

            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Model == type)
                    Assert.IsTrue(LDPage.AllowsTheme(type));
                else
                    Assert.IsFalse(LDPage.AllowsTheme(type));
            }
        }

        [TestMethod]
        public void AllowsKeywordsTest()
        {
            LDPage_Accessor target = new LDPage_Accessor();

            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Primitive == type || PageType.HiresPrimitive == type)
                    Assert.IsFalse(LDPage.AllowsKeywords(type));
                else
                    Assert.IsTrue(LDPage.AllowsKeywords(type));
            }
        }

        [TestMethod]
        public void AllowsDefaultColourTest()
        {
            LDPage_Accessor target = new LDPage_Accessor();

            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                target.PageType = type;

                if (PageType.Primitive == type || PageType.HiresPrimitive == type || PageType.Model == type)
                    Assert.IsFalse(LDPage.AllowsDefaultColour(type));
                else
                    Assert.IsTrue(LDPage.AllowsDefaultColour(type));
            }
        }

        #endregion Parser

        #region Elements

        [TestMethod]
        public void ElementsAccessorTest()
        {
            IDocument doc;
            IPage page;
            bool documentModified;
            string code;

            code = "0 title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "0 !LDRAW_ORG Model\r\n" +
                   "\r\n" +
                   "0 // comment\r\n " +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat\r\n" +
                   "0 STEP\r\n" +
                   "2 24 0 0 0 0 0 0\r\n";

            doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            page = doc[0];
            Assert.AreEqual(3, page.Elements.Count);

            // enumerator
            int count = 0;

            foreach (IElement element in page.Elements)
            {
                count++;
            }

            Assert.AreEqual(3, count);

            // indexer
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDComment));
            Assert.AreEqual(page[0], page.Elements[0].Step);
            Assert.IsInstanceOfType(page.Elements[1], typeof(LDReference));
            Assert.AreEqual(page[0], page.Elements[1].Step);
            Assert.IsInstanceOfType(page.Elements[2], typeof(LDLine));
            Assert.AreEqual(page[1], page.Elements[2].Step);
            page.Elements[0] = new LDTriangle();
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDTriangle));
            Assert.AreEqual(page[0], page.Elements[0].Step);

            // Add()
            page.Elements.Add(new LDBFCFlag());
            Assert.AreEqual(4, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[3], typeof(LDBFCFlag));
            Assert.AreEqual(page[1], page.Elements[3].Step);

            // Insert() at head
            page.Elements.Insert(0, new LDClear());
            Assert.AreEqual(5, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[0], typeof(LDClear));
            Assert.AreEqual(page[0], page.Elements[0].Step);

            // Insert() at tail
            page.Elements.Insert(5, new LDPause());
            Assert.AreEqual(6, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[5], typeof(LDPause));
            Assert.AreEqual(page[1], page.Elements[5].Step);

            // Insert() at boundary
            page.Elements.Insert(3, new LDSave());
            Assert.AreEqual(7, page.Elements.Count);
            Assert.IsInstanceOfType(page.Elements[3], typeof(LDSave));
            Assert.AreEqual(page[0], page.Elements[3].Step);

            // Remove(), Contains() and IndexOf()
            IElement el = page.Elements[5];
            Assert.IsTrue(page.Elements.Contains(el));
            Assert.AreEqual(5, page.Elements.IndexOf(el));
            page.Elements.Remove(el);
            Assert.IsFalse(page.Elements.Contains(el));
            Assert.AreEqual(-1, page.Elements.IndexOf(el));

            // RemoveAt()
            el = page.Elements[4];
            Assert.IsTrue(page.Elements.Contains(el));
            Assert.AreEqual(4, page.Elements.IndexOf(el));
            page.Elements.RemoveAt(4);
            Assert.IsFalse(page.Elements.Contains(el));
            Assert.AreEqual(-1, page.Elements.IndexOf(el));

            // Clear()
            page.Elements.Clear();
            Assert.AreEqual(0, page.Elements.Count);
        }

        #endregion Elements
    }
}
