#region License

//
// LDBFCFlagTest.cs
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
    using System.IO;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    [TestClass]
    public sealed class LDBFCFlagTest : IBFCFlagTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDBFCFlag); } }

        protected override IBFCFlag CreateTestBFCFlag()
        {
            return new LDBFCFlag();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            using (IBFCFlag bfcFlag = CreateTestBFCFlag())
            {
                Assert.IsTrue(TestClassType.IsSealed);
                Assert.IsFalse(bfcFlag.IsImmutable);

                TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
                Assert.IsNotNull(typeNameAttr);
                Assert.AreEqual(Resources.BFCFlag, typeNameAttr.Description);
                Assert.AreEqual(Resources.BFCFlag, bfcFlag.TypeName);

                DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
                Assert.IsNotNull(defaultIconAttr);
                Assert.IsNotNull(defaultIconAttr.Icon);
                Assert.IsNotNull(bfcFlag.Icon);

                ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
                Assert.IsNotNull(elementFlagsAttr);
                Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

                ElementCategoryAttribute categoryAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementCategoryAttribute)) as ElementCategoryAttribute;
                Assert.IsNotNull(categoryAttr);
                Assert.AreEqual(Resources.ElementCategory_MetaCommand, categoryAttr.Description);

                Assert.AreEqual(String.Empty, bfcFlag.ExtendedDescription);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Constructor

        [TestMethod]
        public void LDBFCFlagConstructorTest()
        {
            IBFCFlag bfcFlag;

            bfcFlag = new LDBFCFlag("0 BFC CLIP");
            Assert.AreEqual(BFCFlag.EnableBackFaceCulling, bfcFlag.Flag);

            bfcFlag = new LDBFCFlag("0 BFC CLIP CW");
            Assert.AreEqual(BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise, bfcFlag.Flag);

            bfcFlag = new LDBFCFlag("0 BFC CLIP CCW");
            Assert.AreEqual(BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise, bfcFlag.Flag);

            bfcFlag = new LDBFCFlag("0 BFC NOCLIP");
            Assert.AreEqual(BFCFlag.DisableBackFaceCulling, bfcFlag.Flag);

            bfcFlag = new LDBFCFlag("0 BFC CW");
            Assert.AreEqual(BFCFlag.SetWindingModeClockwise, bfcFlag.Flag);

            bfcFlag = new LDBFCFlag("0 BFC CCW");
            Assert.AreEqual(BFCFlag.SetWindingModeCounterClockwise, bfcFlag.Flag);

            try
            {
                bfcFlag = new LDBFCFlag("foo");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }
        }

        [TestMethod]
        public void LDBFCFlagConstructorTest1()
        {
            IBFCFlag bfcFlag = new LDBFCFlag();
            Assert.AreEqual(BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise, bfcFlag.Flag);
        }

        [TestMethod]
        public void LDBFCFlagConstructorTest2()
        {
            IBFCFlag bfcFlag;

            foreach (BFCFlag flag in Enum.GetValues(typeof(BFCFlag)))
            {
                bfcFlag = new LDBFCFlag(flag);
                Assert.AreEqual(flag, bfcFlag.Flag);
            }

            try
            {
                bfcFlag = new LDBFCFlag((BFCFlag)1000);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }
        }

        #endregion Constructor

        #region Parser

        [TestMethod]
        public void ParserTest()
        {
            bool documentModified;

            string code = "0 title\r\n" +
                          "0 Name: name.dat\r\n" +
                          "\r\n" +
                          "0 BFC CERTIFY CCW\r\n" +
                          "0 BFC CLIP\r\n" +
                          "0 BFC CLIP CCW\r\n" +
                          "0 BFC CLIP CW\r\n" +
                          "0 BFC CCW CLIP\r\n" +
                          "0 BFC CW CLIP\r\n" +
                          "0 BFC NOCLIP\r\n" +
                          "0 BFC CCW\r\n" +
                          "0 BFC CW\r\n" +
                          "0 BFC clipping not used here\r\n";

            IDocument doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            IPage page    = doc[0];
            IStep step    = page[0];
            Assert.AreEqual(9, step.Count);
            Assert.AreEqual(CullingMode.CertifiedCounterClockwise, page.BFC);
            Assert.IsInstanceOfType(step[0], typeof(LDBFCFlag));
            Assert.AreEqual(BFCFlag.EnableBackFaceCulling, (step[0] as LDBFCFlag).Flag);
            Assert.IsInstanceOfType(step[1], typeof(LDBFCFlag));
            Assert.AreEqual(BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise, (step[1] as LDBFCFlag).Flag);
            Assert.IsInstanceOfType(step[2], typeof(LDBFCFlag));
            Assert.AreEqual(BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise, (step[2] as LDBFCFlag).Flag);
            Assert.IsInstanceOfType(step[3], typeof(LDBFCFlag));
            Assert.AreEqual(BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise, (step[3] as LDBFCFlag).Flag);
            Assert.IsInstanceOfType(step[4], typeof(LDBFCFlag));
            Assert.AreEqual(BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise, (step[4] as LDBFCFlag).Flag);
            Assert.IsInstanceOfType(step[5], typeof(LDBFCFlag));
            Assert.AreEqual(BFCFlag.DisableBackFaceCulling, (step[5] as LDBFCFlag).Flag);
            Assert.IsInstanceOfType(step[6], typeof(LDBFCFlag));
            Assert.AreEqual(BFCFlag.SetWindingModeCounterClockwise, (step[6] as LDBFCFlag).Flag);
            Assert.IsInstanceOfType(step[7], typeof(LDBFCFlag));
            Assert.AreEqual(BFCFlag.SetWindingModeClockwise, (step[7] as LDBFCFlag).Flag);
            Assert.IsInstanceOfType(step[8], typeof(IComment));
        }

        #endregion Parser
    }
}
