#region License

//
// LDCommentTest.cs
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
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using System.Collections.Generic;
    using Digitalis.LDTools.DOM.API.Analytics;

    #endregion Usings

    [TestClass]
    public sealed class LDCommentTest : ICommentTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDComment); } }

        protected override IComment CreateTestComment()
        {
            return new LDComment();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IComment comment = CreateTestComment())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(comment.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.Comment, typeNameAttr.Description);
                Assert.AreEqual(Resources.Comment, comment.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(comment.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNotNull(elementFlagsAttr);
                Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

                Assert.AreEqual(String.Empty, comment.ExtendedDescription);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public void AnalyticsTest()
        {
            LDComment comment;
            IEnumerable<IProblemDescriptor> problems;
            IProblemDescriptor problem;
            IEnumerable<IFixDescriptor> fixes;
            IFixDescriptor fix;

            comment = new LDComment("0 // some comment");
            Assert.IsFalse(comment.IsMissingSlashes);
            Assert.AreEqual(0, comment.Analyse(CodeStandards.OfficialModelRepository).Count());

            comment.Text = "some comment";
            Assert.IsTrue(comment.IsMissingSlashes);

            // mode-checks
            Assert.IsTrue(comment.HasProblems(CodeStandards.OfficialModelRepository));
            problems = comment.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(comment, problem.Element);
            Assert.AreEqual(LDComment.Problem_MissingSlashes, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(comment.HasProblems(CodeStandards.PartsLibrary));
            problems = comment.Analyse(CodeStandards.PartsLibrary);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(comment, problem.Element);
            Assert.AreEqual(LDComment.Problem_MissingSlashes, problem.Guid);
            Assert.AreEqual(Severity.Error, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            Assert.IsTrue(comment.HasProblems(CodeStandards.Full));
            problems = comment.Analyse(CodeStandards.Full);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(comment, problem.Element);
            Assert.AreEqual(LDComment.Problem_MissingSlashes, problem.Guid);
            Assert.AreEqual(Severity.Warning, problem.Severity);
            Assert.IsNotNull(problem.Description);
            Assert.IsNotNull(problem.Fixes);

            // test the fix
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDComment.Fix_AddSlashes, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("// some comment", comment.Text);

            comment.Text = "!META_COMMAND";
            Assert.IsFalse(comment.IsMissingSlashes);
            Assert.AreEqual(0, comment.Analyse(CodeStandards.OfficialModelRepository).Count());

            comment.Text = "!Not_a_META_COMMAND";
            Assert.IsTrue(comment.IsMissingSlashes);
            problems = comment.Analyse(CodeStandards.OfficialModelRepository);
            Assert.AreEqual(1, problems.Count());
            problem = problems.First();
            Assert.AreEqual(LDComment.Problem_MissingSlashes, problem.Guid);
            fixes = problem.Fixes;
            Assert.AreEqual(1, fixes.Count());
            fix = fixes.First();
            Assert.AreEqual(LDComment.Fix_AddSlashes, fix.Guid);
            Assert.IsTrue(fix.IsIntraElement);
            Assert.IsTrue(fix.Apply());
            Assert.AreEqual("// !Not_a_META_COMMAND", comment.Text);

            comment.Text = "META_COMMAND";
            Assert.IsFalse(comment.IsMissingSlashes);
            Assert.AreEqual(0, comment.Analyse(CodeStandards.OfficialModelRepository).Count());
        }

        #endregion Analytics

        #region Constructor

        [TestMethod]
        public void LDCommentConstructorTest()
        {
            LDComment comment = new LDComment();
            Assert.IsNull(comment.Text);
        }

        [TestMethod]
        public void LDCommentConstructorTest2()
        {
            LDComment comment = new LDComment("0 some comment");
            Assert.AreEqual("some comment", comment.Text);

            comment = new LDComment("0 // some comment");
            Assert.AreEqual("// some comment", comment.Text);

            comment = new LDComment("0 comment with trailing whitespace     ");
            Assert.AreEqual("comment with trailing whitespace", comment.Text);

            comment = new LDComment("0     comment with leading whitespace");
            Assert.AreEqual("    comment with leading whitespace", comment.Text);

            comment = new LDComment("0 !META_COMMAND");
            Assert.AreEqual("!META_COMMAND", comment.Text);

            comment = new LDComment("0    !META_COMMAND");
            Assert.AreEqual("!META_COMMAND", comment.Text);

            comment = new LDComment("0    META_COMMAND");
            Assert.AreEqual("META_COMMAND", comment.Text);

            try
            {
                comment = new LDComment("foo");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }
        }

        #endregion Constructor
    }
}
