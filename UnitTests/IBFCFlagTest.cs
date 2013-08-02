#region License

//
// IBFCFlagTest.cs
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
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IBFCFlagTest : IMetaCommandTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IBFCFlag); } }

        protected sealed override IMetaCommand CreateTestMetaCommand()
        {
            return CreateTestBFCFlag();
        }

        protected abstract IBFCFlag CreateTestBFCFlag();

        protected sealed override string[] SyntaxExamples
        {
            get
            {
                return new string[]
                {
                    "0 BFC CW\r\n",
                    "0 BFC CCW\r\n",
                    "0 BFC CLIP\r\n",
                    "0 BFC CLIP CW\r\n",
                    "0 BFC CLIP CCW\r\n",
                    "0 BFC NOCLIP\r\n",
                    "0 BFC CW CLIP\r\n",
                    "0 BFC CCW CLIP\r\n"
                };
            }
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IBFCFlag bfcFlag = CreateTestBFCFlag();

            Assert.AreEqual(DOMObjectType.MetaCommand, bfcFlag.ObjectType);
            Assert.IsFalse(bfcFlag.IsTopLevelElement);
            Assert.IsTrue(bfcFlag.IsStateElement);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Cloning and Serialization

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            // Flag should be preserved
            Assert.AreEqual(((IBFCFlag)original).Flag, ((IBFCFlag)copy).Flag);

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            IBFCFlag bfcFlag = CreateTestBFCFlag();
            StringBuilder code;
            string expected;

            if (bfcFlag.IsImmutable)
            {
                throw new NotImplementedException("IBFCFlagTest.ToCodeTest() not implemented for immutable objects");
            }
            else
            {
                bfcFlag.Flag = BFCFlag.DisableBackFaceCulling;
                expected = "0 BFC NOCLIP\r\n";
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected,code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());

                bfcFlag.Flag = BFCFlag.EnableBackFaceCulling;
                expected = "0 BFC CLIP\r\n";
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());

                bfcFlag.Flag = BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise;
                expected = "0 BFC CLIP CW\r\n";
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());

                bfcFlag.Flag = BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise;
                expected = "0 BFC CLIP CCW\r\n";
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());

                bfcFlag.Flag = BFCFlag.SetWindingModeClockwise;
                expected = "0 BFC CW\r\n";
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());

                bfcFlag.Flag = BFCFlag.SetWindingModeCounterClockwise;
                expected = "0 BFC CCW\r\n";
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
                code = Utils.PreProcessCode(bfcFlag.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected, code.ToString());
            }

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Properties

        [TestMethod]
        public void FlagTest()
        {
            IBFCFlag bfcFlag     = CreateTestBFCFlag();
            BFCFlag defaultValue = BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise;
            BFCFlag newValue     = BFCFlag.DisableBackFaceCulling;

            PropertyValueTest(bfcFlag,
                              defaultValue,
                              newValue,
                              delegate(IBFCFlag obj) { return obj.Flag; },
                              delegate(IBFCFlag obj, BFCFlag value) { obj.Flag = value; },
                              PropertyValueFlags.None);
        }

        [TestMethod]
        public void FlagChangedTest()
        {
            IBFCFlag bfcFlag   = CreateTestBFCFlag();
            BFCFlag valueToSet = BFCFlag.DisableBackFaceCulling;

            PropertyChangedTest(bfcFlag,
                                "FlagChanged",
                                valueToSet,
                                delegate(IBFCFlag obj, PropertyChangedEventHandler<BFCFlag> handler) { obj.FlagChanged += handler; },
                                delegate(IBFCFlag obj) { return obj.Flag; },
                                delegate(IBFCFlag obj, BFCFlag value) { obj.Flag = value; });
        }

        #endregion Properties
    }
}
