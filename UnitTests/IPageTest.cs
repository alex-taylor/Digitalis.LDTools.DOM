#region License

//
// IPageTest.cs
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
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    using Configuration = Digitalis.LDTools.DOM.Configuration;

    #endregion Usings

    [TestClass]
    public abstract class IPageTest : IDocumentElementTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IPage); } }

        protected sealed override IDocumentElement  CreateTestDocumentElement()
        {
            return CreateTestPage();
        }

        protected sealed override IDocumentElement CreateTestDocumentElementWithDocument()
        {
            IDocument document = new LDDocument();
            IPage page = CreateTestPage();
            document.Add(page);
            return page;
        }

        protected abstract IPage CreateTestPage();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IPage page = CreateTestPage();
            Assert.AreEqual(DOMObjectType.Page, page.ObjectType);

            if (page.IsImmutable)
                Assert.IsTrue(page.IsReadOnly);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            IPage page                = CreateTestPage();
            page.Name                 = "name";
            page.PageType             = PageType.Part;
            page.Title                = "title";
            page.Category             = Category.Animal;
            page.BFC                  = CullingMode.CertifiedClockwise;
            page.Author               = "author";
            page.User                 = "user";
            page.Update               = new LDUpdate();
            page.Help                 = "help";
            page.InlineOnPublish      = true;
            page.License              = License.CCAL2;
            page.RotationPoint        = MLCadRotationConfig.Type.PartRotationPoint;
            page.RotationPointVisible = true;
            page.DefaultColour        = 1U;
            page.Keywords             = new string[] { "keyword" };
            page.History              = new LDHistory[] { new LDHistory(DateTime.Now, "description") };
            page.RotationConfig       = new MLCadRotationConfig[] { new MLCadRotationConfig(Vector3d.Zero, false, "Custom") };

            IStep step       = MocksFactory.CreateMockStep();
            IComment comment = MocksFactory.CreateMockComment();
            comment.Text     = "foobar";
            step.Add(comment);
            page.Add(step);

            IDocument document = MocksFactory.CreateMockDocument();
            document.Add(page);

            return page;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IPage first  = (IPage)original;
            IPage second = (IPage)copy;

            // properties should be preserved
            Assert.AreEqual(first.Name, second.Name);
            Assert.AreEqual(first.PageType, second.PageType);
            Assert.AreEqual(first.Title, second.Title);
            Assert.AreEqual(first.Category, second.Category);
            Assert.AreEqual(first.BFC, second.BFC);
            Assert.AreEqual(first.Author, second.Author);
            Assert.AreEqual(first.User, second.User);
            Assert.AreEqual(first.Update, second.Update);
            Assert.AreEqual(first.Help, second.Help);
            Assert.AreEqual(first.InlineOnPublish, second.InlineOnPublish);
            Assert.AreEqual(first.License, second.License);
            Assert.AreEqual(first.RotationPoint, second.RotationPoint);
            Assert.AreEqual(first.RotationPointVisible, second.RotationPointVisible);
            Assert.AreEqual(first.DefaultColour, second.DefaultColour);
            Assert.AreEqual(first.Theme, second.Theme);

            List<string> keywords = new List<string>(second.Keywords);
            Assert.AreEqual(1, keywords.Count);
            Assert.AreEqual("keyword", keywords[0]);

            List<LDHistory> history = new List<LDHistory>(second.History);
            Assert.AreEqual(1, history.Count);
            Assert.AreEqual(new LDHistory(DateTime.Now, "description"), history[0]);

            List<MLCadRotationConfig> config = new List<MLCadRotationConfig>(second.RotationConfig);
            Assert.AreEqual(1, config.Count);
            Assert.AreEqual(new MLCadRotationConfig(Vector3d.Zero, false, "Custom"), config[0]);

            // contents should be preserved
            Assert.AreEqual(first.Count, second.Count);
            Assert.AreNotSame(first[0], second[0]);
            Assert.AreSame(second, second[0].Page);
            Assert.IsInstanceOfType(second.Elements[0], typeof(IComment));
            Assert.AreEqual((first.Elements[0] as IComment).Text, (second.Elements[0] as IComment).Text);

            // upstream links should not be preserved
            Assert.IsNull(second.Document);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public void ToCodeTest_Steps()
        {
            // there should be a blank line between the header and the first element
            IPage page    = CreateTestPage();
            page.PageType = PageType.Part;
            page.Name     = "name";
            page.Title    = "title";
            page.Author   = null;
            page.License  = License.None;
            page.History  = null;
            page.BFC      = CullingMode.CertifiedCounterClockwise;

            IStep step = new LDStep();
            page.Add(step);
            step.Add(new LDLine());

            string actual;
            string expected = "0 title\r\n" +
                              "0 Name: name.dat\r\n" +
                              "0 !LDRAW_ORG Unofficial_Part\r\n" +
                              "\r\n" +
                              "0 BFC CERTIFY CCW\r\n" +
                              "\r\n" +
                              "2 24 0 0 0 0 0 0\r\n";

            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToCodeTest_Help()
        {
            // multi-line !HELP
            IPage page     = CreateTestPage();
            page.PageType  = PageType.Part;
            page.Name      = "name";
            page.Title     = "title";
            page.Author    = null;
            page.License   = License.None;
            page.History   = null;
            page.Help      = "help 1\r\nhelp 2\r\nhelp 3\r\n";
            page.BFC       = CullingMode.CertifiedCounterClockwise;

            string actual;
            string expected = "0 title\r\n" +
                              "0 Name: name.dat\r\n" +
                              "0 !LDRAW_ORG Unofficial_Part\r\n" +
                              "\r\n" +
                              "0 !HELP help 1\r\n" +
                              "0 !HELP help 2\r\n" +
                              "0 !HELP help 3\r\n" +
                              "\r\n" +
                              "0 BFC CERTIFY CCW\r\n";

            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToCodeTest_Keywords()
        {
            // multi-line keywords: keywords should wrap around to a new line after 60 characters
            IPage page     = CreateTestPage();
            page.PageType  = PageType.Part;
            page.Name      = "name";
            page.Title     = "title";
            page.Author    = null;
            page.License   = License.None;
            page.History   = null;
            page.BFC       = CullingMode.CertifiedCounterClockwise;

            List<string> keywords = new List<string>();

            for (int i = 0; i < 30; i++)
            {
                keywords.Add("keyword " + i);
            }

            page.Keywords = keywords;

            string actual;
            string expected = "0 title\r\n" +
                              "0 Name: name.dat\r\n" +
                              "0 !LDRAW_ORG Unofficial_Part\r\n" +
                              "\r\n" +
                              "0 BFC CERTIFY CCW\r\n" +
                              "\r\n" +
                              "0 !KEYWORDS keyword 0, keyword 1, keyword 2, keyword 3, keyword 4, keyword 5\r\n" +
                              "0 !KEYWORDS keyword 6, keyword 7, keyword 8, keyword 9, keyword 10, keyword 11\r\n" +
                              "0 !KEYWORDS keyword 12, keyword 13, keyword 14, keyword 15, keyword 16, keyword 17\r\n" +
                              "0 !KEYWORDS keyword 18, keyword 19, keyword 20, keyword 21, keyword 22, keyword 23\r\n" +
                              "0 !KEYWORDS keyword 24, keyword 25, keyword 26, keyword 27, keyword 28, keyword 29\r\n";

            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToCodeTest_Headers()
        {
            // test the full header output for each PageType, Official and Unofficial and in each code-format (Full, PartsLibrary, OMR)
            IPage page = CreateTestPage();
            string expected;
            string actual;

            // Unofficial Model header
            page.Title                = "Title String";
            page.Name                 = "Name String";
            page.PageType             = PageType.Model;
            page.Author               = Environment.UserName;
            page.User                 = "username";
            page.License              = License.CCAL2;
            page.Theme                = "theme name";
            page.Keywords             = new string[] { "keyword1", "keyword2", "long keyword" };
            page.History              = new LDHistory[] { new LDHistory("0 !HISTORY 2012-08-31 {Alex} more changes"), new LDHistory("0 !HISTORY 2012-08-26 [anathema] details of change") };
            page.BFC                  = CullingMode.CertifiedCounterClockwise;
            page.Help                 = "help text";
            page.RotationPoint        = MLCadRotationConfig.Type.WorldOrigin;
            page.RotationPointVisible = true;
            page.RotationConfig       = new MLCadRotationConfig[] { new MLCadRotationConfig(Vector3d.Zero, false, "Custom") };

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.ldr\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Model\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !THEME theme name\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +
                       "0 ROTATION CONFIG -3 1\r\n";
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.ldr\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Model\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !THEME theme name\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +
                       "0 ROTATION CONFIG -3 1\r\n";
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            try
            {
                // Models cannot be encoded in PartsLibrary mode
                actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentException), e.GetType());
            }

            // Official Model header
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.ldr\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Model UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !THEME theme name\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +
                       "0 ROTATION CONFIG -3 1\r\n";
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.ldr\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Model UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !THEME theme name\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +
                       "0 ROTATION CONFIG -3 1\r\n";
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            try
            {
                // Models cannot be encoded in PartsLibrary mode
                actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentException), e.GetType());
            }

            // Unofficial Part header
            page.PageType      = PageType.Part;
            page.Update        = null;
            page.Category      = Category.Bracket;
            page.DefaultColour = 1;

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Part\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Part\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Part\r\n" +         // UPDATE only included in PartsLibrary mode for Official Parts
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Official Part header
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Part UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Part UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Part UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Unofficial Subpart header
            page.PageType = PageType.Subpart;
            page.Update = null;

            expected = "0 Title String\r\n" +
                       "0 Name: s\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Subpart\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: s\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Subpart\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: s\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Subpart\r\n" +         // UPDATE only included in PartsLibrary mode for Official Subparts
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Official Subpart header
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: s\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Subpart UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: s\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Subpart UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: s\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Subpart UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Unofficial Shortcut
            page.PageType = PageType.Shortcut;
            page.Category = Category.Bracket;
            page.Update = null;

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Shortcut\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Shortcut\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Shortcut\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Official Shortcut
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Shortcut UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Shortcut UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Shortcut UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Unofficial Part Alias
            page.PageType = PageType.Part_Alias;
            page.Update = null;

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Part Alias\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Part Alias\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Part Alias\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Official Part Alias
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Part Alias UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Part Alias UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Part Alias UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Unofficial Shortcut Alias
            page.PageType = PageType.Shortcut_Alias;
            page.Update = null;

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Shortcut Alias\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Shortcut Alias\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Shortcut Alias\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Official Shortcut Alias
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Shortcut Alias UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Shortcut Alias UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Shortcut Alias UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Unofficial Part Physical_Colour
            page.PageType = PageType.Part_Physical_Colour;
            page.Update = null;

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Part Physical_Colour\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Part Physical_Colour\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Part Physical_Colour\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Official Part Physical_Colour
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Part Physical_Colour UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Part Physical_Colour UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Part Physical_Colour UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Unofficial Shortcut Physical_Colour
            page.PageType = PageType.Shortcut_Physical_Colour;
            page.Update = null;

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Shortcut Physical_Colour\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Shortcut Physical_Colour\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Shortcut Physical_Colour\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Official Shortcut Physical_Colour
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Shortcut Physical_Colour UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Shortcut Physical_Colour UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Shortcut Physical_Colour UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !CATEGORY Bracket\r\n" +
                       "0 !KEYWORDS keyword1, keyword2, long keyword\r\n" +
                       "\r\n" +
                       "0 !CMDLINE -c1\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Unofficial Primitive header
            page.PageType = PageType.Primitive;
            page.Update = null;

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Primitive\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Primitive\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_Primitive\r\n" +    // UPDATE only included in PartsLibrary mode for Official Subparts
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Official Primitive header
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Primitive UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Primitive UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Primitive UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Unofficial HiresPrimitive header
            page.PageType = PageType.HiresPrimitive;
            page.Update = null;

            expected = "0 Title String\r\n" +
                       "0 Name: 48\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_48_Primitive\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: 48\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_48_Primitive\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: 48\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG Unofficial_48_Primitive\r\n" +    // UPDATE only included in PartsLibrary mode for Official Subparts
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            // Official HiresPrimitive header
            page.Update = new LDUpdate(2012, 1);

            expected = "0 Title String\r\n" +
                       "0 Name: 48\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG 48_Primitive UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH\r\n" +     // Full mode only
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +       // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                // only allowed in Full and OMR modes
            page.InlineOnPublish = true;
            actual = page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: 48\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG 48_Primitive UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n" +
                       "\r\n" +
                       "0 ROTATION CENTER 0 0 0 0 \"Custom\"\r\n" +         // only allowed in Full and OMR modes
                       "0 ROTATION CONFIG -3 1\r\n";                        // only allowed in Full and OMR modes
            page.InlineOnPublish = false;
            actual = page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);

            expected = "0 Title String\r\n" +
                       "0 Name: 48\\Name String.dat\r\n" +
                       "0 Author: " + Environment.UserName + " [username]\r\n" +
                       "0 !LDRAW_ORG 48_Primitive UPDATE 2012-01\r\n" +
                       "0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt\r\n" +
                       "\r\n" +
                       "0 !HELP help text\r\n" +
                       "\r\n" +
                       "0 BFC CERTIFY CCW\r\n" +
                       "\r\n" +
                       "0 !HISTORY 2012-08-26 [anathema] details of change\r\n" +
                       "0 !HISTORY 2012-08-31 {Alex} more changes\r\n";
            actual = page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal).ToString();
            Assert.AreEqual(expected, actual);
        }

        #endregion Code-generation

        #region Collection-management

        [TestMethod]
        public void ItemsAddedTest()
        {
            IDOMObjectCollectionTest.ItemsAddedTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void ItemsRemovedTest()
        {
            IDOMObjectCollectionTest.ItemsRemovedTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void ItemsReplacedTest()
        {
            IDOMObjectCollectionTest.ItemsReplacedTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void CollectionClearedTest()
        {
            IDOMObjectCollectionTest.CollectionClearedTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void CanInsertTest()
        {
            IDOMObjectCollectionTest.CanInsertTest(CreateTestPage(), CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void CanReplaceTest()
        {
            IDOMObjectCollectionTest.CanReplaceTest(CreateTestPage(), CreateTestPage(), new LDStep(), new LDStep());
        }

        [TestMethod]
        public void HasLockedDescendantsTest()
        {
            IPage page = CreateTestPage();
            Assert.IsFalse(page.HasLockedDescendants);

            IStep step = new LDStep();
            page.Add(step);
            Assert.IsFalse(page.HasLockedDescendants);
            step.IsLocked = true;
            Assert.IsTrue(page.HasLockedDescendants);
            step.IsLocked = false;

            ILine line = new LDLine();
            step.Add(line);
            Assert.IsFalse(page.HasLockedDescendants);
            line.IsLocked = true;
            Assert.IsTrue(page.HasLockedDescendants);
        }

        [TestMethod]
        public void CountTest()
        {
            IDOMObjectCollectionTest.CountTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void IndexOfTest()
        {
            IDOMObjectCollectionTest.IndexOfTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void ContainsTest()
        {
            IDOMObjectCollectionTest.ContainsTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void IndexerTest()
        {
            IDOMObjectCollectionTest.IndexerTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void AddTest()
        {
            IDOMObjectCollectionTest.AddTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void InsertTest()
        {
            IDOMObjectCollectionTest.InsertTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void RemoveTest()
        {
            IDOMObjectCollectionTest.RemoveTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void RemoveAtTest()
        {
            IDOMObjectCollectionTest.RemoveAtTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void ClearTest()
        {
            IDOMObjectCollectionTest.ClearTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void CopyToTest()
        {
            IDOMObjectCollectionTest.CopyToTest(CreateTestPage(), new LDStep());
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            IDOMObjectCollectionTest.GetEnumeratorTest(CreateTestPage(), new LDStep());
        }

        #endregion Collection-management

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            // disposing of a page will remove it from its Document and dispose of its descendants
            IPage page         = (IPage)CreateTestDocumentElementWithDocument();
            IDocument document = page.Document;
            IStep step         = new LDStep();
            page.Add(step);

            Assert.AreSame(document, page.Document);
            Assert.IsTrue(document.Contains(page));
            Assert.AreSame(page, step.Page);
            page.Dispose();
            Assert.IsTrue(step.IsDisposed);
            Assert.IsNull(page.Document);
            Assert.IsFalse(document.Contains(page));

            // TODO: test for when the document is immutable or read-only

            // a page in a frozen document-tree cannot be disposed of
            page     = (IPage)CreateTestDocumentElementWithDocument();
            document = page.Document;
            document.Freeze();
            Assert.IsTrue(document.IsFrozen);

            try
            {
                page.Dispose();
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.IsTrue(document.Contains(page));
                Assert.AreSame(document, page.Document);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            // but the head of the tree can
            document.Dispose();
            Assert.IsTrue(document.IsDisposed);
            Assert.IsTrue(page.IsDisposed);
            Assert.IsTrue(step.IsDisposed);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Document-tree

        [TestMethod]
        public override void DocumentTest()
        {
            IPage page             = CreateTestPage();
            IDocument defaultValue = null;
            IDocument newValue     = new LDDocument();

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.Document; },
                              delegate(IPage obj, IDocument value) { obj.Document = value; },
                              PropertyValueFlags.SettableWhenLocked | PropertyValueFlags.NotDisposable);

            // the page cannot be added to a frozen IDocument
            page = CreateTestPage();
            IDocument document = new LDDocument();
            document.Freeze();
            Assert.IsTrue(document.IsFrozen);

            try
            {
                page.Document = document;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.IsNull(page.Document);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            // TODO: the page cannot be added to an immutable IDocument

            // TODO: the step cannot be added to a read-only IPage

            // TODO: check for InvalidOperationException when page.CanInsert() fails its checks

            // Document cannot be set if it already has a value
            page                  = CreateTestPage();
            page.Document         = new LDDocument();
            IDocument oldDocument = page.Document;

            try
            {
                page.Document = new LDDocument();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreSame(oldDocument, page.Document);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }
        }

        [TestMethod]
        public void DocumentChangedTest()
        {
            IPage page = (IPage)CreateTestDocumentElementWithDocument();

            if (!page.IsImmutable)
            {
                IDocument document    = page.Document;
                bool eventSeen        = false;
                bool genericEventSeen = false;
                bool addEventSeen     = false;
                bool removeEventSeen  = false;
                bool disposing        = false;

                Assert.IsNotNull(document);

                page.DocumentChanged += delegate(object sender, PropertyChangedEventArgs<IDocument> e)
                {
                    if (disposing)
                        return;

                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(page, sender);

                    if (null == e.NewValue)
                        Assert.AreSame(document, e.OldValue);
                    else
                        Assert.AreSame(document, e.NewValue);
                };

                page.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    if (disposing)
                        return;

                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(page, sender);
                    Assert.AreEqual("DocumentChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IDocument>));

                    PropertyChangedEventArgs<IDocument> args = (PropertyChangedEventArgs<IDocument>)e.Parameters;

                    if (null == args.NewValue)
                        Assert.AreSame(document, args.OldValue);
                    else
                        Assert.AreSame(document, args.NewValue);
                };

                document.ItemsRemoved += delegate(object sender, UndoableListChangedEventArgs<IPage> e)
                {
                    if (disposing)
                        return;

                    Assert.IsFalse(removeEventSeen);
                    removeEventSeen = true;
                    Assert.AreSame(document, sender);
                };

                document.ItemsAdded += delegate(object sender, UndoableListChangedEventArgs<IPage> e)
                {
                    Assert.IsFalse(addEventSeen);
                    addEventSeen = true;
                    Assert.AreSame(document, sender);
                };

                page.Document = null;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.IsTrue(removeEventSeen);

                eventSeen        = false;
                genericEventSeen = false;
                page.Document    = document;
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                Assert.IsTrue(addEventSeen);

                disposing = true;
            }
        }

        [TestMethod]
        public void PageTreeChangedTest()
        {
            IPage page = CreateTestPage();

            if (page.IsImmutable || page.IsReadOnly)
            {
                throw new NotImplementedException("IPageTest.PageTreeChangedTest() not implemented for immutable/read-only objects");
            }
            else
            {
                IStep step = new LDStep();
                ILine line = new LDLine();
                bool eventSeen = false;

                page.Add(step);
                step.Add(line);

                page.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(page, sender);
                    Assert.AreEqual("ColourValueChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<uint>));
                    Assert.AreSame(line, e.Source);

                    PropertyChangedEventArgs<uint> args = (PropertyChangedEventArgs<uint>)e.Parameters;
                    Assert.AreEqual(Palette.EdgeColour, args.OldValue);
                    Assert.AreEqual(1U, args.NewValue);
                };

                // events issued by children of the page should propagate up
                line.ColourValue = 1U;
                Assert.IsTrue(eventSeen);
            }
        }

        [TestMethod]
        public void PathToDocumentChangedTest()
        {
            IPage page     = CreateTestPage();
            bool eventSeen = false;

            page.PathToDocumentChanged += delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(page, sender);
            };

            page.Document = new LDDocument();
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            page.Document = null;
            Assert.IsTrue(eventSeen);
        }

        #endregion Document-tree

        #region Freezing

        [TestMethod]
        public override void IsFrozenTest()
        {
            IPage page = CreateTestPage();
            IStep step = new LDStep();
            page.Add(step);

            Assert.IsFalse(page.IsFrozen);
            Assert.IsFalse(step.IsFrozen);
            page.Freeze();
            Assert.IsTrue(page.IsFrozen);
            Assert.IsTrue(step.IsFrozen);

            base.IsFrozenTest();
        }

        #endregion Freezing

        #region Geometry

        private void CreateGeometry(IPage page)
        {
            Random random = new Random();
            double x1     = 1.0;
            double y1     = 2.0;
            double z1     = 3.0;
            double x2     = 4.0;
            double y2     = 5.0;
            double z2     = 6.0;

            for (int i = 0; i < 10; i++)
            {
                x1 *= random.NextDouble();
                y1 *= random.NextDouble();
                z1 *= random.NextDouble();
                x2 *= random.NextDouble();
                y2 *= random.NextDouble();
                z2 *= random.NextDouble();

                IStep step = new LDStep();
                step.Add(new LDLine(Palette.EdgeColour, new Vector3d(x1, y1, z1), new Vector3d(x2, y2, z2)));
                page.Add(step);
            }
        }

        [TestMethod]
        public void BoundingBoxTest()
        {
            IPage page = CreateTestPage();

            if (page.IsImmutable || page.IsReadOnly)
            {
                throw new NotImplementedException("IPageTest.BoundingBoxTest() not implemented for immutable pages");
            }
            else
            {
                Box3d expectedBounds;
                CreateGeometry(page);

                expectedBounds = ((IGraphic)page.Elements[0]).BoundingBox;

                for (int i = 1; i < page.Count; i++)
                {
                    expectedBounds.Union(((IGraphic)page.Elements[i]).BoundingBox);
                }

                Assert.AreEqual(expectedBounds, page.BoundingBox);
            }
        }

        [TestMethod]
        public void OriginTest()
        {
            IPage page = CreateTestPage();
            Assert.AreEqual(Vector3d.Zero, page.Origin);
        }

        [TestMethod]
        public void WindingModeTest()
        {
            IPage page = CreateTestPage();
            Assert.AreEqual(page.BFC, page.WindingMode);
        }

        [TestMethod]
        public void TransformTest()
        {
            IPage page = CreateTestPage();

            if (page.IsImmutable || page.IsReadOnly)
            {
                throw new NotImplementedException("IPageTest.TransformTest() not implemented for immutable pages");
            }
            else
            {
                CreateGeometry(page);

                Matrix4d transform = Matrix4d.Scale(1.0, 2.0, 3.0);
                IGraphic[] oldValues = new IGraphic[page.Count];
                IGraphic[] newValues = new IGraphic[page.Count];

                for (int i = 0; i < page.Elements.Count; i++)
                {
                    oldValues[i] = (IGraphic)page.Elements[i].Clone();
                    newValues[i] = (IGraphic)page.Elements[i].Clone();
                    newValues[i].Transform(ref transform);
                }

                for (int i = 0; i < page.Count; i++)
                {
                    Assert.IsTrue(oldValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                    Assert.IsFalse(newValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                }

                page.Transform(ref transform);

                for (int i = 0; i < page.Count; i++)
                {
                    Assert.IsFalse(oldValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                    Assert.IsTrue(newValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                }

                for (int i = 0; i < page.Count; i++)
                {
                    ((IGraphic)page.Elements[i]).Coordinates = oldValues[i].Coordinates;
                }

                // undo/redo
                UndoStack undoStack = new UndoStack();
                undoStack.StartCommand("command");
                page.Transform(ref transform);
                undoStack.EndCommand();
                for (int i = 0; i < page.Count; i++)
                {
                    Assert.IsFalse(oldValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                    Assert.IsTrue(newValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                }
                undoStack.Undo();
                for (int i = 0; i < page.Count; i++)
                {
                    Assert.IsTrue(oldValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                    Assert.IsFalse(newValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                }
                undoStack.Redo();
                for (int i = 0; i < page.Count; i++)
                {
                    Assert.IsFalse(oldValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                    Assert.IsTrue(newValues[i].IsDuplicateOf((IGraphic)page.Elements[i]));
                }

                // a page with locked descendants cannot be transformed
                page[0].IsLocked = true;

                try
                {
                    page.Transform(ref transform);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                // a frozen page cannot be transformed
                page.Freeze();

                try
                {
                    page.Transform(ref transform);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void ReverseWindingTest()
        {
            IPage page = CreateTestPage();

            if (page.IsImmutable || page.IsReadOnly)
            {
                throw new NotImplementedException("IPageTest.ReverseWindingTest() not implemented for immutable pages");
            }
            else
            {
                CreateGeometry(page);

                IGraphic[] oldValues = new IGraphic[page.Count];
                IGraphic[] newValues = new IGraphic[page.Count];

                for (int i = 0; i < page.Count; i++)
                {
                    oldValues[i] = (IGraphic)page.Elements[i].Clone();
                    newValues[i] = (IGraphic)page.Elements[i].Clone();
                    newValues[i].ReverseWinding();
                }

                for (int i = 0; i < page.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                        Assert.AreNotEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                    }
                }

                page.ReverseWinding();

                for (int i = 0; i < page.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreNotEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                        Assert.AreEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                    }
                }

                for (int i = 0; i < page.Count; i++)
                {
                    ((IGraphic)page.Elements[i]).Coordinates = oldValues[i].Coordinates;
                }

                // undo/redo
                UndoStack undoStack = new UndoStack();
                undoStack.StartCommand("command");
                page.ReverseWinding();
                undoStack.EndCommand();
                for (int i = 0; i < page.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreNotEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                        Assert.AreEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                    }
                }
                undoStack.Undo();
                for (int i = 0; i < page.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                        Assert.AreNotEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                    }
                }
                undoStack.Redo();
                for (int i = 0; i < page.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreNotEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                        Assert.AreEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)page.Elements[i]).Coordinates.ElementAt(n));
                    }
                }

                // a page with locked descendants cannot be transformed
                page[0].IsLocked = true;

                try
                {
                    page.ReverseWinding();
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                // a frozen page cannot be transformed
                page.Freeze();

                try
                {
                    page.ReverseWinding();
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }
            }
        }

        #endregion Geometry

        #region Properties

        [TestMethod]
        public void TargetNameTest()
        {
            IPage page;
            string defaultValue = Resources.Untitled;
            string newValue     = "new name";

            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                page          = CreateTestPage();
                page.PageType = type;

                PropertyValueTest(page,
                                  defaultValue,
                                  newValue,
                                  delegate(IPage obj) { return obj.Name; },
                                  delegate(IPage obj, string value) { obj.Name = value; },
                                  delegate(IPage obj, string expectedValue)
                                  {
                                      switch (obj.PageType)
                                      {
                                          case PageType.Model:
                                              Assert.AreEqual(expectedValue + ".ldr", obj.TargetName);
                                              break;

                                          case PageType.Subpart:
                                              Assert.AreEqual("s\\" + expectedValue + ".dat", obj.TargetName);
                                              break;

                                          case PageType.HiresPrimitive:
                                              Assert.AreEqual("48\\" + expectedValue + ".dat", obj.TargetName);
                                              break;

                                          default:
                                              Assert.AreEqual(expectedValue + ".dat", obj.TargetName);
                                              break;

                                      }
                                  },
                                  PropertyValueFlags.None);
            }

            // changing TargetName in a document should update all references to the page
            IDocument doc = new LDDocument();
            IPage page1 = CreateTestPage();
            IStep step1 = new LDStep();
            page1.Add(step1);
            page1.Name     = "page1";
            page1.PageType = PageType.Model;
            doc.Add(page1);

            IPage page2 = CreateTestPage();
            IStep step2 = new LDStep();
            page2.Add(step2);
            page2.Name     = "page2";
            page2.PageType = PageType.Model;
            doc.Add(page2);

            IReference r = new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 page2.ldr", false);
            step1.Add(r);
            Assert.AreEqual("page2.ldr", r.TargetName);
            Assert.AreSame(page2, r.Target);

            // rename it
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            page2.Name = "new name";
            undoStack.EndCommand();
            Assert.AreSame(page2, r.Target);
            Assert.AreEqual("new name.ldr", r.TargetName);
            undoStack.Undo();
            Assert.AreSame(page2, r.Target);
            Assert.AreEqual("page2.ldr", r.TargetName);
            undoStack.Redo();
            Assert.AreSame(page2, r.Target);
            Assert.AreEqual("new name.ldr", r.TargetName);

            // change its type
            undoStack.StartCommand("command");
            page2.PageType = PageType.Part;
            undoStack.EndCommand();
            Assert.AreSame(page2, r.Target);
            Assert.AreEqual("new name.dat", r.TargetName);
            undoStack.Undo();
            Assert.AreSame(page2, r.Target);
            Assert.AreEqual("new name.ldr", r.TargetName);
            undoStack.Redo();
            Assert.AreSame(page2, r.Target);
            Assert.AreEqual("new name.dat", r.TargetName);

            // renaming page2 to 'page1.ldr' should fail
            try
            {
                page2.Name     = "page1";
                page2.PageType = PageType.Model;
                Assert.Fail();
            }
            catch (DuplicateNameException)
            {
                Assert.AreEqual("page1", page1.Name);
            }
            catch (Exception e)
            {
                Assert.AreSame(typeof(DuplicateNameException), e.GetType());
            }

            // renaming page2 to 'page1.dat' should succeed
            page2.Name     = "page1";
            page2.PageType = PageType.Part;

            // a renamed page should override an external target
            r.TargetName = "3001.dat";
            Assert.IsNotNull(r.Target);
            Assert.AreNotSame(page2, r.Target);
            undoStack.StartCommand("command");
            page2.Name = "3001";
            undoStack.EndCommand();
            Assert.AreEqual("3001.dat", page2.TargetName);
            Assert.IsNotNull(r.Target);
            Assert.AreSame(page2, r.Target);
            undoStack.Undo();
            Assert.IsNotNull(r.Target);
            Assert.AreNotSame(page2, r.Target);
            undoStack.Redo();
            Assert.IsNotNull(r.Target);
            Assert.AreSame(page2, r.Target);

            // renaming the page now should cause the reference to follow
            page2.Name = "page2";
            Assert.IsNotNull(r.Target);
            Assert.AreSame(page2, r.Target);

            // try to create a circular reference by renaming a page
            r.TargetName = "3001.dat";
            Assert.IsNotNull(r.Target);
            Assert.AreNotSame(page1, r.Target);
            Assert.AreNotSame(page2, r.Target);
            page1.Name = "3001";
            try
            {
                // first the obvious one: make the page point at itself
                page1.PageType = PageType.Part;
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual("3001.ldr", page1.TargetName);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            // add a reference from page2 to page1
            step2.Add(new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 " + page1.TargetName, false));
            try
            {
                // and now try to force page1's reference to point at page2
                page2.Name = "3001";
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual("page2.dat", page2.TargetName);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }
        }

        [TestMethod]
        public void TargetNameChangedTest()
        {
            IPage page            = CreateTestPage();
            string valueToSet     = "new name";
            bool eventSeen        = false;
            bool changedEventSeen = false;

            page.TargetNameChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(page, sender);

                // TODO: check values
            };

            page.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.IsFalse(changedEventSeen);
                changedEventSeen = true;
                Assert.AreSame(page, sender);
            };

            page.Name = valueToSet;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(changedEventSeen);
        }

        [TestMethod]
        public void NameTest()
        {
            IPage page          = CreateTestPage();
            string defaultValue = Resources.Untitled;
            string newValue     = "new name";

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.Name; },
                              delegate(IPage obj, string value) { obj.Name = value; },
                              PropertyValueFlags.None);

            // certain characters are not allowed in Name
            page = CreateTestPage();

            try
            {
                page.Name = "<name>";
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                Assert.AreEqual(defaultValue, page.Name);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentException), e.GetType());
            }

            // there's also a length restriction
            string name = "";

            for (int i = 0; i < 255; i++)
            {
                name += "0";
            }

            page.Name = name;

            try
            {
                // add one more char...
                page.Name = name + "0";
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                Assert.AreEqual(name, page.Name);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentException), e.GetType());
            }
        }

        [TestMethod]
        public void NameChangedTest()
        {
            IPage page        = CreateTestPage();
            string valueToSet = "new name";

            PropertyChangedTest(page,
                                "NameChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<string> handler) { obj.NameChanged += handler; },
                                delegate(IPage obj) { return obj.Name; },
                                delegate(IPage obj, string value) { obj.Name = value; });
        }

        [TestMethod]
        public void TitleTest()
        {
            IPage page          = CreateTestPage();
            string defaultValue = Resources.NewModel;
            string newValue     = "new title";

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.Title; },
                              delegate(IPage obj, string value) { obj.Title = value; },
                              PropertyValueFlags.None);

            // should trim whitespace at both ends
            page = CreateTestPage();
            page.Title = "   title   ";
            Assert.AreEqual("title", page.Title);
        }

        [TestMethod]
        public void TitleChangedTest()
        {
            IPage page        = CreateTestPage();
            string valueToSet = "new title";

            PropertyChangedTest(page,
                                "TitleChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<string> handler) { obj.TitleChanged += handler; },
                                delegate(IPage obj) { return obj.Title; },
                                delegate(IPage obj, string value) { obj.Title = value; });
        }

        [TestMethod]
        public void ThemeTest()
        {
            IPage page          = CreateTestPage();
            string defaultValue = String.Empty;
            string newValue     = "new theme";

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.Theme; },
                              delegate(IPage obj, string value) { obj.Theme = value; },
                              PropertyValueFlags.None);

            // should not be settable for anything other than Models, and getting should return the empty string
            page = CreateTestPage();

            foreach (PageType type in Enum.GetValues(typeof(PageType)))
            {
                page.PageType = type;

                if (PageType.Model == type)
                {
                    page.Theme = "theme name";
                    Assert.AreEqual("theme name", page.Theme);
                }
                else
                {
                    try
                    {
                        Assert.AreEqual(String.Empty, page.Theme);
                        page.Theme = "theme name";
                        Assert.Fail();
                    }
                    catch (InvalidOperationException)
                    {
                        Assert.AreEqual(defaultValue, page.Theme);
                    }
                    catch (Exception e)
                    {
                        Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
                    }
                }
            }
        }

        [TestMethod]
        public void ThemeChangedTest()
        {
            IPage page        = CreateTestPage();
            string valueToSet = "new theme";

            PropertyChangedTest(page,
                                "ThemeChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<string> handler) { obj.ThemeChanged += handler; },
                                delegate(IPage obj) { return obj.Theme; },
                                delegate(IPage obj, string value) { obj.Theme = value; });
        }

        [TestMethod]
        public void AuthorTest()
        {
            IPage page          = CreateTestPage();
            string defaultValue = Configuration.Author;
            string newValue     = "author name";

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.Author; },
                              delegate(IPage obj, string value) { obj.Author = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void AuthorChangedTest()
        {
            IPage page        = CreateTestPage();
            string valueToSet = "author name";

            PropertyChangedTest(page,
                                "AuthorChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<string> handler) { obj.AuthorChanged += handler; },
                                delegate(IPage obj) { return obj.Author; },
                                delegate(IPage obj, string value) { obj.Author = value; });
        }

        [TestMethod]
        public void UserTest()
        {
            IPage page          = CreateTestPage();
            string defaultValue = Configuration.Username;
            string newValue     = "username";

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.User; },
                              delegate(IPage obj, string value) { obj.User = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void UserChangedTest()
        {
            IPage page        = CreateTestPage();
            string valueToSet = "username";

            PropertyChangedTest(page,
                                "UserChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<string> handler) { obj.UserChanged += handler; },
                                delegate(IPage obj) { return obj.User; },
                                delegate(IPage obj, string value) { obj.User = value; });
        }

        [TestMethod]
        public void PageTypeTest()
        {
            IPage page            = CreateTestPage();
            PageType defaultValue = PageType.Model;
            PageType newValue     = PageType.Part;

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.PageType; },
                              delegate(IPage obj, PageType value) { obj.PageType = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void PageTypeChangedTest()
        {
            IPage page          = CreateTestPage();
            PageType valueToSet = PageType.Part;

            PropertyChangedTest(page,
                                "PageTypeChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<PageType> handler) { obj.PageTypeChanged += handler; },
                                delegate(IPage obj) { return obj.PageType; },
                                delegate(IPage obj, PageType value) { obj.PageType = value; });
        }

        [TestMethod]
        public void LicenseTest()
        {
            IPage page           = CreateTestPage();
            License defaultValue;
            License newValue     = License.NonCCAL;

            if (null != page.Author || null != page.User)
                defaultValue = License.CCAL2;
            else
                defaultValue = License.None;

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.License; },
                              delegate(IPage obj, License value) { obj.License = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void LicenseChangedTest()
        {
            IPage page         = CreateTestPage();
            License valueToSet = License.NonCCAL;

            PropertyChangedTest(page,
                                "LicenseChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<License> handler) { obj.LicenseChanged += handler; },
                                delegate(IPage obj) { return obj.License; },
                                delegate(IPage obj, License value) { obj.License = value; });
        }

        [TestMethod]
        public void CategoryTest()
        {
            IPage page            = CreateTestPage();
            Category defaultValue = Category.Unknown;
            Category newValue     = Category.Brick;

            page.PageType = PageType.Part;

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.Category; },
                              delegate(IPage obj, Category value) { obj.Category = value; },
                              PropertyValueFlags.None);

            page = CreateTestPage();
            page.PageType = PageType.Part;
            page.Category = Category.Unknown;
            page.Title = "Brick 1 x 2";
            Assert.AreEqual(Category.Brick, page.Category);
            page.Title = "wibble";
            Assert.AreEqual(Category.Unknown, page.Category);

            // primitives and models can't have a category
            try
            {
                page.PageType = PageType.Primitive;
                page.Category = Category.Tyre;
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(Category.Primitive_Unknown, page.Category);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            try
            {
                page.PageType = PageType.HiresPrimitive;
                page.Category = Category.Tyre;
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(Category.Primitive_Unknown, page.Category);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            try
            {
                page.PageType = PageType.Model;
                page.Category = Category.Tyre;
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(Category.Unknown, page.Category);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            // changing the type to Primitive should clear the category
            page.PageType = PageType.Part;
            page.Category = Category.Turntable;
            Assert.AreEqual(Category.Turntable, page.Category);
            page.PageType = PageType.Primitive;
            Assert.AreEqual(Category.Primitive_Unknown, page.Category);
            page.PageType = PageType.HiresPrimitive;
            Assert.AreEqual(Category.Primitive_Unknown, page.Category);
        }

        [TestMethod]
        public void CategoryChangedTest()
        {
            IPage page          = CreateTestPage();
            Category valueToSet = Category.Brick;

            page.PageType = PageType.Part;

            PropertyChangedTest(page,
                                "CategoryChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<Category> handler) { obj.CategoryChanged += handler; },
                                delegate(IPage obj) { return obj.Category; },
                                delegate(IPage obj, Category value) { obj.Category = value; });
        }

        [TestMethod]
        public void BFCTest()
        {
            IPage page               = CreateTestPage();
            CullingMode defaultValue = CullingMode.NotSet;
            CullingMode newValue     = CullingMode.CertifiedCounterClockwise;

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.BFC; },
                              delegate(IPage obj, CullingMode value) { obj.BFC = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void BFCChangedTest()
        {
            IPage page             = CreateTestPage();
            CullingMode valueToSet = CullingMode.CertifiedCounterClockwise;

            PropertyChangedTest(page,
                                "BFCChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<CullingMode> handler) { obj.BFCChanged += handler; },
                                delegate(IPage obj) { return obj.BFC; },
                                delegate(IPage obj, CullingMode value) { obj.BFC = value; });
        }

        [TestMethod]
        public void DefaultColourTest()
        {
            IPage page         = CreateTestPage();
            uint defaultValue = Palette.MainColour;
            uint newValue     = 1U;

            page.PageType = PageType.Part;

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.DefaultColour; },
                              delegate(IPage obj, uint value) { obj.DefaultColour = value; },
                              PropertyValueFlags.None);

            // primitives and models can't have a default colour
            page = CreateTestPage();

            try
            {
                page.PageType = PageType.Primitive;
                page.DefaultColour = 1U;
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(Palette.MainColour, page.DefaultColour);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            try
            {
                page.PageType = PageType.HiresPrimitive;
                page.DefaultColour = 1U;
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(Palette.MainColour, page.DefaultColour);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            try
            {
                page.PageType = PageType.Model;
                page.DefaultColour = 1U;
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(Palette.MainColour, page.DefaultColour);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }
        }

        [TestMethod]
        public void DefaultColourChangedTest()
        {
            IPage page      = CreateTestPage();
            uint valueToSet = 1U;

            page.PageType = PageType.Part;

            PropertyChangedTest(page,
                                "DefaultColourChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<uint> handler) { obj.DefaultColourChanged += handler; },
                                delegate(IPage obj) { return obj.DefaultColour; },
                                delegate(IPage obj, uint value) { obj.DefaultColour = value; });
        }

        [TestMethod]
        public void UpdateTest()
        {
            IPage page             = CreateTestPage();
            LDUpdate? defaultValue = null;
            LDUpdate? newValue     = new LDUpdate();

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.Update; },
                              delegate(IPage obj, LDUpdate? value) { obj.Update = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void UpdateChangedTest()
        {
            IPage page           = CreateTestPage();
            LDUpdate? valueToSet = new LDUpdate();

            PropertyChangedTest(page,
                                "UpdateChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<LDUpdate?> handler) { obj.UpdateChanged += handler; },
                                delegate(IPage obj) { return obj.Update; },
                                delegate(IPage obj, LDUpdate? value) { obj.Update = value; });
        }

        [TestMethod]
        public void HelpTest()
        {
            IPage page          = CreateTestPage();
            string defaultValue = String.Empty;
            string newValue     = "help text";

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.Help; },
                              delegate(IPage obj, string value) { obj.Help = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void HelpChangedTest()
        {
            IPage page        = CreateTestPage();
            string valueToSet = "help text";

            PropertyChangedTest(page,
                                "HelpChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<string> handler) { obj.HelpChanged += handler; },
                                delegate(IPage obj) { return obj.Help; },
                                delegate(IPage obj, string value) { obj.Help = value; });
        }

        [TestMethod]
        public void KeywordsTest()
        {
            IPage page                       = CreateTestPage();
            IEnumerable<string> defaultValue = new string[0];
            IEnumerable<string> newValue     = new string[] { "keyword1", "keyword2" };

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.Keywords; },
                              delegate(IPage obj, IEnumerable<string> value) { obj.Keywords = value; },
                              delegate(IPage obj, IEnumerable<string> expectedValue)
                              {
                                  Assert.AreEqual(expectedValue.Count(), obj.Keywords.Count());

                                  for (int i = 0; i < expectedValue.Count(); i++)
                                  {
                                      Assert.AreEqual(expectedValue.ElementAt(i), obj.Keywords.ElementAt(i));
                                  }
                              },
                              PropertyValueFlags.None);

            // primitives can't have keywords set
            page = CreateTestPage();

            try
            {
                page.PageType = PageType.Primitive;
                page.Keywords = new string[] { "foo" };
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(0, page.Keywords.Count());
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            try
            {
                page.PageType = PageType.HiresPrimitive;
                page.Keywords = new string[] { "foo" };
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(0, page.Keywords.Count());
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            // whitespace should be collapsed
            page.PageType = PageType.Model;
            page.Keywords = new string[] { "keyword", "keyword 1", "keyword  2", "keyword\t3", " keyword 4 " };
            IList<string> keywords = new List<string>(page.Keywords);
            Assert.AreEqual("keyword", keywords[0]);
            Assert.AreEqual("keyword 1", keywords[1]);
            Assert.AreEqual("keyword 2", keywords[2]);
            Assert.AreEqual("keyword 3", keywords[3]);
            Assert.AreEqual("keyword 4", keywords[4]);
        }

        [TestMethod]
        public void KeywordsChangedTest()
        {
            IPage page                     = CreateTestPage();
            IEnumerable<string> valueToSet = new string[] { "keyword1", "keyword2" };

            PropertyChangedTest(page,
                                "KeywordsChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<IEnumerable<string>> handler) { obj.KeywordsChanged += handler; },
                                delegate(IPage obj) { return obj.Keywords; },
                                delegate(IPage obj, IEnumerable<string> value) { obj.Keywords = value; },
                                delegate(IPage obj, IEnumerable<string> oldValue, IEnumerable<string> newValue, PropertyChangedEventArgs<IEnumerable<string>> eventArgs)
                                {
                                    Assert.AreEqual(0, oldValue.Count());

                                    Assert.AreEqual(newValue.Count(), eventArgs.NewValue.Count());

                                    for (int i = 0; i < newValue.Count(); i++)
                                    {
                                        Assert.AreEqual(newValue.ElementAt(i), eventArgs.NewValue.ElementAt(i));
                                    }
                                });
        }

        [TestMethod]
        public void HistoryTest()
        {
            IPage page                          = CreateTestPage();
            IEnumerable<LDHistory> defaultValue = page.History;
            IEnumerable<LDHistory> newValue     = new LDHistory[] { new LDHistory("0 !HISTORY 2011-05-24 [username] description"), new LDHistory("0 !HISTORY 2012-06-21 [username2] description2") };

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.History; },
                              delegate(IPage obj, IEnumerable<LDHistory> value) { obj.History = value; },
                              delegate(IPage obj, IEnumerable<LDHistory> expectedValue)
                              {
                                  Assert.AreEqual(expectedValue.Count(), obj.History.Count());

                                  for (int i = 0; i < expectedValue.Count(); i++)
                                  {
                                      Assert.AreEqual(expectedValue.ElementAt(i), obj.History.ElementAt(i));
                                  }
                              },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void HistoryChangedTest()
        {
            IPage page                        = CreateTestPage();
            IEnumerable<LDHistory> valueToSet = new LDHistory[] { new LDHistory("0 !HISTORY 2011-05-24 [username] description"), new LDHistory("0 !HISTORY 2012-06-21 [username2] description2") };

            page.History = null;

            PropertyChangedTest(page,
                                "HistoryChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<IEnumerable<LDHistory>> handler) { obj.HistoryChanged += handler; },
                                delegate(IPage obj) { return obj.History; },
                                delegate(IPage obj, IEnumerable<LDHistory> value) { obj.History = value; },
                                delegate(IPage obj, IEnumerable<LDHistory> oldValue, IEnumerable<LDHistory> newValue, PropertyChangedEventArgs<IEnumerable<LDHistory>> eventArgs)
                                {
                                    Assert.AreEqual(0, oldValue.Count());

                                    Assert.AreEqual(newValue.Count(), eventArgs.NewValue.Count());

                                    for (int i = 0; i < newValue.Count(); i++)
                                    {
                                        Assert.AreEqual(newValue.ElementAt(i), eventArgs.NewValue.ElementAt(i));
                                    }
                                });
        }

        [TestMethod]
        public void RotationPointTest()
        {
            IPage page                            = CreateTestPage();
            MLCadRotationConfig.Type defaultValue = MLCadRotationConfig.Type.PartOrigin;
            MLCadRotationConfig.Type newValue     = MLCadRotationConfig.Type.PartCentre;

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.RotationPoint; },
                              delegate(IPage obj, MLCadRotationConfig.Type value) { obj.RotationPoint = value; },
                              PropertyValueFlags.None);

            // check that an out-of-range value is caught
            page = CreateTestPage();

            try
            {
                page.RotationPoint = (MLCadRotationConfig.Type)4;
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.AreEqual(defaultValue, page.RotationPoint);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }
        }

        [TestMethod]
        public void RotationPointChangedTest()
        {
            IPage page                          = CreateTestPage();
            MLCadRotationConfig.Type valueToSet = MLCadRotationConfig.Type.PartCentre;

            PropertyChangedTest(page,
                                "RotationPointChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<MLCadRotationConfig.Type> handler) { obj.RotationPointChanged += handler; },
                                delegate(IPage obj) { return obj.RotationPoint; },
                                delegate(IPage obj, MLCadRotationConfig.Type value) { obj.RotationPoint = value; });
        }

        [TestMethod]
        public void RotationPointVisibleTest()
        {
            IPage page        = CreateTestPage();
            bool defaultValue = false;
            bool newValue     = true;

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.RotationPointVisible; },
                              delegate(IPage obj, bool value) { obj.RotationPointVisible = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void RotationPointVisibleChangedTest()
        {
            IPage page      = CreateTestPage();
            bool valueToSet = true;

            PropertyChangedTest(page,
                                "RotationPointVisibleChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<bool> handler) { obj.RotationPointVisibleChanged += handler; },
                                delegate(IPage obj) { return obj.RotationPointVisible; },
                                delegate(IPage obj, bool value) { obj.RotationPointVisible = value; });
        }

        [TestMethod]
        public void RotationConfigTest()
        {
            IPage page                                    = CreateTestPage();
            IEnumerable<MLCadRotationConfig> defaultValue = new MLCadRotationConfig[0];
            IEnumerable<MLCadRotationConfig> newValue     = new MLCadRotationConfig[] { new MLCadRotationConfig("0 ROTATION CENTER 1 2 3 1 \"Point 1\""), new MLCadRotationConfig("0 ROTATION CENTER 4 5 6 1 \"Point 2\"") };

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.RotationConfig; },
                              delegate(IPage obj, IEnumerable<MLCadRotationConfig> value) { obj.RotationConfig = value; },
                              delegate(IPage obj, IEnumerable<MLCadRotationConfig> expectedValue)
                              {
                                  Assert.AreEqual(expectedValue.Count(), obj.RotationConfig.Count());

                                  for (int i = 0; i < expectedValue.Count(); i++)
                                  {
                                      Assert.AreEqual(expectedValue.ElementAt(i), obj.RotationConfig.ElementAt(i));
                                  }
                              },
                              PropertyValueFlags.None);

            // check that RotationPoint is reset if the number of configs drops
            page = CreateTestPage();
            page.RotationConfig = new MLCadRotationConfig[] { new MLCadRotationConfig(Vector3d.Zero, false, "name"), new MLCadRotationConfig(Vector3d.Zero, false, "name") };
            page.RotationPoint = (MLCadRotationConfig.Type)2;
            Assert.AreEqual(2, (int)page.RotationPoint);
            page.RotationConfig = new MLCadRotationConfig[] { new MLCadRotationConfig(Vector3d.Zero, false, "name") };
            Assert.AreEqual(1, (int)page.RotationPoint);

            // check that RotationPoint reverts if the change is undone
            page = CreateTestPage();
            page.RotationConfig = new MLCadRotationConfig[] { new MLCadRotationConfig(Vector3d.Zero, false, "name"), new MLCadRotationConfig(Vector3d.Zero, false, "name") };
            page.RotationPoint = (MLCadRotationConfig.Type)2;
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            page.RotationConfig = new MLCadRotationConfig[] { new MLCadRotationConfig(Vector3d.Zero, false, "name") };
            undoStack.EndCommand();
            undoStack.Undo();
            Assert.AreEqual(2, (int)page.RotationPoint);
            undoStack.Redo();
            Assert.AreEqual(1, (int)page.RotationPoint);

            // rotation configs must have a valid Name
            try
            {
                MLCadRotationConfig cfg = new MLCadRotationConfig();

                page.RotationConfig = new MLCadRotationConfig[] { cfg };
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentException), e.GetType());
            }
        }

        [TestMethod]
        public void RotationConfigChangedTest()
        {
            IPage page                                  = CreateTestPage();
            IEnumerable<MLCadRotationConfig> valueToSet = new MLCadRotationConfig[] { new MLCadRotationConfig("0 ROTATION CENTER 1 2 3 1 \"Point 1\""), new MLCadRotationConfig("0 ROTATION CENTER 4 5 6 1 \"Point 2\"") };

            PropertyChangedTest(page,
                                "RotationConfigChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<IEnumerable<MLCadRotationConfig>> handler) { obj.RotationConfigChanged += handler; },
                                delegate(IPage obj) { return obj.RotationConfig; },
                                delegate(IPage obj, IEnumerable<MLCadRotationConfig> value) { obj.RotationConfig = value; },
                                delegate(IPage obj, IEnumerable<MLCadRotationConfig> oldValue, IEnumerable<MLCadRotationConfig> newValue, PropertyChangedEventArgs<IEnumerable<MLCadRotationConfig>> eventArgs)
                                {
                                    Assert.AreEqual(0, oldValue.Count());

                                    Assert.AreEqual(newValue.Count(), eventArgs.NewValue.Count());

                                    for (int i = 0; i < newValue.Count(); i++)
                                    {
                                        Assert.AreEqual(newValue.ElementAt(i), eventArgs.NewValue.ElementAt(i));
                                    }
                                });
        }

        [TestMethod]
        public void InlineOnPublishTest()
        {
            IPage page        = CreateTestPage();
            bool defaultValue = false;
            bool newValue     = true;

            PropertyValueTest(page,
                              defaultValue,
                              newValue,
                              delegate(IPage obj) { return obj.InlineOnPublish; },
                              delegate(IPage obj, bool value) { obj.InlineOnPublish = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void InlineOnPublishChangedTest()
        {
            IPage page      = CreateTestPage();
            bool valueToSet = true;

            PropertyChangedTest(page,
                                "InlineOnPublishChanged",
                                valueToSet,
                                delegate(IPage obj, PropertyChangedEventHandler<bool> handler) { obj.InlineOnPublishChanged += handler; },
                                delegate(IPage obj) { return obj.InlineOnPublish; },
                                delegate(IPage obj, bool value) { obj.InlineOnPublish = value; });
        }

        #endregion Properties
    }
}
