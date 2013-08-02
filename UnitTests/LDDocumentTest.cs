#region License

//
// LDDocumentTest.cs
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
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public sealed class LDDocumentTest : IDocumentTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDDocument); } }

        protected override IDocument CreateTestDocument()
        {
            return new LDDocument();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IDocument document = CreateTestDocument())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(document.IsImmutable);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Constructor

        [TestMethod]
        public void LDDocumentConstructorTest()
        {
            IDocument target;
            IPage page;
            IReference r;
            bool documentModified;

            string path = Path.Combine(Digitalis.LDTools.DOM.Configuration.LDrawBase, @"parts\3001.dat");

            using (TextReader reader = File.OpenText(path))
            {
                bool delegateCalled = false;
                ParserProgressCallback cb = delegate(string name, int progress)
                {
                    delegateCalled = true;
                    return true;
                };

                target = new LDDocument(reader, path, cb, ParseFlags.None, out documentModified);
                Assert.AreEqual(1, target.Count);
                Assert.AreEqual(path, target.Filepath);
                Assert.IsFalse(documentModified);
                Assert.IsTrue(delegateCalled);
            }

            // blank lines at the start of a page should be skipped
            string data = "\r\n\r\n\r\n0 FILE name.dat\r\n0 Title\r\n0 Name: name.dat\r\n\r\n0 FILE name2.dat\r\n\r\n0 Title2\r\n0 Name: name2.dat\r\n";
            target = new LDDocument(new StringReader(data), "name.mpd", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(2, target.Count);
            page = target[0];
            Assert.AreEqual("Title", page.Title);
            Assert.AreEqual("name", page.Name);
            Assert.AreEqual(PageType.Part, page.PageType);
            Assert.AreEqual("name.dat", page.TargetName);
            page = target[1];
            Assert.AreEqual("Title2", page.Title);
            Assert.AreEqual("name2", page.Name);
            Assert.AreEqual(PageType.Part, page.PageType);
            Assert.AreEqual("name2.dat", page.TargetName);

            // ~Moved to files should be followed if ParseFlags.FollowRedirects is set
            data = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 s\\14s01.dat\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 s\\14s01.dat\r\n";
            target = new LDDocument(new StringReader(data), "name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, target.Count);
            page = target[0];
            LDStep step = page[0] as LDStep;
            Assert.AreEqual(2, step.Count);
            r = step[0] as LDReference;
            Assert.AreEqual("s\\14s01.dat", r.TargetName);
            r = step[1] as LDReference;
            Assert.AreEqual("s\\14s01.dat", r.TargetName);
            Assert.IsFalse(documentModified);

            target = new LDDocument(new StringReader(data), "name.dat", null, ParseFlags.FollowRedirects, out documentModified);
            Assert.AreEqual(1, target.Count);
            page = target[0];
            step = page[0] as LDStep;
            Assert.AreEqual(2, step.Count);
            r = step[0] as LDReference;
            Assert.AreEqual("s\\3350s01.dat", r.TargetName);
            r = step[1] as LDReference;
            Assert.AreEqual("s\\3350s01.dat", r.TargetName);
            Assert.IsTrue(documentModified);

            // ~Moved to with a non-identity transform: 75.dat => 76279.dat with transform [60 1 0 0 -1 0 1 0 0 0 0 1]
            data = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 75.dat\r\n";
            target = new LDDocument(new StringReader(data), "name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, target.Count);
            page = target[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            r = step[0] as LDReference;
            Assert.AreEqual("75.dat", r.TargetName);
            Assert.AreEqual(new Matrix4d(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1), r.Matrix);
            Assert.IsFalse(documentModified);

            target = new LDDocument(new StringReader(data), "name.dat", null, ParseFlags.FollowRedirects, out documentModified);
            Assert.AreEqual(1, target.Count);
            page = target[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            r = step[0] as LDReference;
            Assert.AreEqual("76279.dat", r.TargetName);
            Assert.AreEqual(new Matrix4d(0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 1, 0, 60, 1, 0, 1), r.Matrix);
            Assert.IsTrue(documentModified);

            // similar for Aliases
            data = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 3729.dat\r\n";
            target = new LDDocument(new StringReader(data), "name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, target.Count);
            page = target[0];
            Assert.AreEqual(1, page.Count);
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            r = step[0] as LDReference;
            Assert.AreEqual("3729.dat", r.TargetName);
            Assert.IsFalse(documentModified);

            target = new LDDocument(new StringReader(data), "name.dat", null, ParseFlags.FollowAliases, out documentModified);
            Assert.AreEqual(1, target.Count);
            page = target[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            r = step[0] as LDReference;
            Assert.AreEqual("3731.dat", r.TargetName);
            Assert.IsTrue(documentModified);

            // x-series redirects happen regardless
            data = "0 Title\r\n" +
                   "0 Name: name.dat\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 x699.dat\r\n";
            target = new LDDocument(new StringReader(data), "name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(1, target.Count);
            page = target[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            r = step[0] as LDReference;
            Assert.AreEqual("699.dat", r.TargetName);
            Assert.IsTrue(documentModified);

            // relative filepaths should be promoted to absolute
            target = new LDDocument(@"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.LDrawReference.Claim.ldr", ParseFlags.None);
            page = target[0];
            step = page[0] as LDStep;

            foreach (IDOMObject el in step)
            {
                r = el as LDReference;

                if (null != r)
                    Assert.IsNotNull(r.Target, "Failed to resolve target for " + r.TargetName);
            }

            // pages with illegal targetnames should be corrected, and refs to them updated
            data = "0 FILE page1.ldr\r\n" +
                   "0 Title\r\n" +
                   "0 Name: page1.ldr\r\n" +
                   "1 16 0 0 0 1 0 0 0 1 0 0 0 1 page2.mpd\r\n" +
                   "0 FILE page2.mpd\r\n" +
                   "0 Title\r\n" +
                   "0 Name: page2.mpd\r\n" +
                   "0 !LDRAW_ORG Model\r\n";
            target = new LDDocument(new StringReader(data), "name.dat", null, ParseFlags.None, out documentModified);
            Assert.AreEqual(2, target.Count);
            page = target[0];
            step = page[0] as LDStep;
            Assert.AreEqual(1, step.Count);
            r = step[0] as LDReference;
            page = target[1];
            Assert.AreEqual("page2.ldr", page.TargetName);
            Assert.AreEqual("page2.ldr", r.TargetName);
            Assert.AreEqual(page, r.Target);
        }

        [TestMethod]
        public void LDDocumentConstructorTest2()
        {
            LDDocument target = new LDDocument();
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual("Untitled", target.Filepath);
        }

        [TestMethod]
        public void LDDocumentConstructorTest3()
        {
            LDDocument target;

            // self-contained model-loop
            try
            {
                target = new LDDocument(@"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.LDrawDocument.CircularReferences.mpd", ParseFlags.None);
                Assert.Fail();
            }
            catch (CircularReferenceException)
            {
            }

            // external model-loop
            try
            {
                target = new LDDocument(@"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.LDrawDocument.ExternalCircularReferencesA.ldr", ParseFlags.None);
                Assert.Fail();
            }
            catch (CircularReferenceException)
            {
            }
        }

        #endregion Constructor
    }
}
