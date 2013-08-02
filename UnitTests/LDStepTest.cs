#region License

//
// LDStepTest.cs
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

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    [TestClass]
    public class LDStepTest : IStepTest
    {
        #region Infrastructure

        protected override Type TestClassType { get { return typeof(LDStep); } }

        protected override IStep CreateTestStep()
        {
            return new LDStep();
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IStep step = CreateTestStep();
            Assert.IsTrue(TestClassType.IsSealed);
            Assert.IsFalse(step.IsImmutable);

            TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
            Assert.IsNotNull(typeNameAttr);
            Assert.AreEqual(Resources.Step, typeNameAttr.Description);
            Assert.AreEqual(Resources.Step, step.TypeName);

            DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
            Assert.IsNotNull(defaultIconAttr);
            Assert.IsNotNull(defaultIconAttr.Icon);
            Assert.IsNotNull(step.Icon);

            ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;
            Assert.IsNotNull(elementFlagsAttr);
            Assert.AreEqual(ElementFlags.HasEditor, elementFlagsAttr.Flags);

            Assert.AreEqual(String.Empty, step.ExtendedDescription);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Constructor

        [TestMethod]
        public void LDStepConstructorTest()
        {
            LDStep target = new LDStep();
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual(String.Empty, target.Description);
            Assert.AreEqual(StepMode.Additive, target.Mode);
            Assert.AreEqual(0.0, target.X);
            Assert.AreEqual(0.0, target.Y);
            Assert.AreEqual(0.0, target.Z);
            Assert.AreEqual(Matrix4d.Identity, target.StepTransform);
        }

        [TestMethod]
        public void LDStepConstructorTest1()
        {
            LDStep target = new LDStep("0 STEP");
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual(String.Empty, target.Description);
            Assert.AreEqual(StepMode.Additive, target.Mode);
            Assert.AreEqual(0.0, target.X);
            Assert.AreEqual(0.0, target.Y);
            Assert.AreEqual(0.0, target.Z);
            Assert.AreEqual(Matrix4d.Identity, target.StepTransform);
        }

        [TestMethod]
        public void LDStepConstructorTest2()
        {
            LDStep target = new LDStep("0 ROTSTEP 10.0 20.0 30.0 REL");
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual("Rotate the viewpoint by (x:10° y:20° z:30°) relative to its default setting", target.Description);
            Assert.AreEqual(StepMode.Relative, target.Mode);
            Assert.AreEqual(10.0, target.X);
            Assert.AreEqual(20.0, target.Y);
            Assert.AreEqual(30.0, target.Z);
            Assert.AreEqual(TestMatrix, target.StepTransform);

            // 'REL' is optional
            target = new LDStep("0 ROTSTEP 10.0 20.0 30.0");
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual("Rotate the viewpoint by (x:10° y:20° z:30°) relative to its default setting", target.Description);
            Assert.AreEqual(StepMode.Relative, target.Mode);
            Assert.AreEqual(10.0, target.X);
            Assert.AreEqual(20.0, target.Y);
            Assert.AreEqual(30.0, target.Z);
            Assert.AreEqual(TestMatrix, target.StepTransform);

            // out-of-range angles should be normalised
            target = new LDStep("0 ROTSTEP 370.0 380.0 390.0");
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual("Rotate the viewpoint by (x:10° y:20° z:30°) relative to its default setting", target.Description);
            Assert.AreEqual(StepMode.Relative, target.Mode);
            Assert.AreEqual(10.0, target.X);
            Assert.AreEqual(20.0, target.Y);
            Assert.AreEqual(30.0, target.Z);

            // out-of-range angles should be normalised
            target = new LDStep("0 ROTSTEP -370.0 -380.0 -390.0");
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual("Rotate the viewpoint by (x:-10° y:-20° z:-30°) relative to its default setting", target.Description);
            Assert.AreEqual(StepMode.Relative, target.Mode);
            Assert.AreEqual(-10.0, target.X);
            Assert.AreEqual(-20.0, target.Y);
            Assert.AreEqual(-30.0, target.Z);

            // optimisation
            target.X = 0.0;
            target.Y = 0.0;
            target.Z = 0.0;
            Assert.AreEqual("Restore the viewpoint to its default setting", target.Description);
        }

        [TestMethod]
        public void LDStepConstructorTest3()
        {
            LDStep target = new LDStep("0 ROTSTEP 10.0 20.0 30.0 ABS");
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual("Set the viewpoint rotation to (x:10° y:20° z:30°)", target.Description);
            Assert.AreEqual(StepMode.Absolute, target.Mode);
            Assert.AreEqual(10.0, target.X);
            Assert.AreEqual(20.0, target.Y);
            Assert.AreEqual(30.0, target.Z);
            Assert.AreEqual(TestMatrix, target.StepTransform);
        }

        [TestMethod]
        public void LDStepConstructorTest4()
        {
            LDStep target = new LDStep("0 ROTSTEP 10.0 20.0 30.0 ADD");
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual("Rotate the viewpoint by (x:10° y:20° z:30°) relative to the previous setting", target.Description);
            Assert.AreEqual(StepMode.Additive, target.Mode);
            Assert.AreEqual(10.0, target.X);
            Assert.AreEqual(20.0, target.Y);
            Assert.AreEqual(30.0, target.Z);
            Assert.AreEqual(TestMatrix, target.StepTransform);
        }

        [TestMethod]
        public void LDStepConstructorTest5()
        {
            LDStep target = new LDStep("0 ROTSTEP END");
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual("Restore the viewpoint to its default setting", target.Description);
            Assert.AreEqual(StepMode.Reset, target.Mode);
            Assert.AreEqual(0.0, target.X);
            Assert.AreEqual(0.0, target.Y);
            Assert.AreEqual(0.0, target.Z);
            Assert.AreEqual(Matrix4d.Identity, target.StepTransform);
        }

        [TestMethod]
        public void LDStepConstructorTest6()
        {
            LDStep target = new LDStep(StepMode.Absolute, 10.0, 20.0, 30.0);
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(new Box3d(), target.BoundingBox);
            Assert.AreEqual(Vector3d.Zero, target.Origin);
            Assert.AreEqual("Set the viewpoint rotation to (x:10° y:20° z:30°)", target.Description);
            Assert.AreEqual(StepMode.Absolute, target.Mode);
            Assert.AreEqual(10.0, target.X);
            Assert.AreEqual(20.0, target.Y);
            Assert.AreEqual(30.0, target.Z);
            Assert.AreEqual(TestMatrix, target.StepTransform);
            Assert.AreEqual(TestMatrix, target.StepTransform);
        }

        #endregion Constructor

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            LDStep step = new LDStep();
            LDPage page = new LDPage();

            Assert.AreEqual(0, page.PathToDocumentChangedSubscribers);
            Assert.AreEqual(0, step.ChangedSubscribers);
            page.Add(step);
            Assert.AreEqual(1, page.PathToDocumentChangedSubscribers);
            Assert.AreEqual(1, step.ChangedSubscribers);
            step.Dispose();
            Assert.AreEqual(0, page.PathToDocumentChangedSubscribers);
            Assert.AreEqual(0, step.ChangedSubscribers);

            base.DisposeTest();
        }

        #endregion Disposal

        #region Document-tree

        [TestMethod]
        public override void PageTest()
        {
            LDStep step = new LDStep();
            LDPage page = new LDPage();

            Assert.AreEqual(0, page.PathToDocumentChangedSubscribers);
            Assert.AreEqual(0, step.ChangedSubscribers);
            ((IStep)step).Page = page;
            Assert.AreEqual(1, page.PathToDocumentChangedSubscribers);
            Assert.AreEqual(1, step.ChangedSubscribers);
            ((IStep)step).Page = null;
            Assert.AreEqual(0, page.PathToDocumentChangedSubscribers);
            Assert.AreEqual(0, step.ChangedSubscribers);

            base.PageTest();
        }

        #endregion Document-tree

        #region Parser

        [TestMethod]
        public void ParserTest()
        {
            bool documentModified;

            string code = "0 title\r\n" +
                          "0 Name: name.dat\r\n" +
                          "\r\n" +
                          "0 STEP\r\n" +
                          "0 STEP MARKER\r\n" +             // invalid: should become a comment
                          "0 ROTSTEP\r\n" +                 // invalid: should become a comment
                          "0 ROTSTEP END\r\n" +
                          "0 ROTSTEP 10 20 30\r\n" +
                          "0 ROTSTEP 11 21 31 REL\r\n" +
                          "0 ROTSTEP 12 22 32 ABS\r\n" +
                          "0 ROTSTEP 13 23 33 ADD\r\n" +
                          "0 comment\r\n";                  // this should generate an implicit step

            IDocument doc = new LDDocument(new StringReader(code), "name.dat", null, ParseFlags.None, out documentModified);
            IPage page = doc[0];
            IStep step;
            Assert.AreEqual(7, page.Count);
            Assert.IsInstanceOfType(page[0], typeof(LDStep));
            step = page[0] as LDStep;
            Assert.AreEqual(0, step.Count);
            Assert.AreEqual(StepMode.Additive, step.Mode);
            Assert.AreEqual(0, step.X);
            Assert.AreEqual(0, step.Y);
            Assert.AreEqual(0, step.Z);
            Assert.IsInstanceOfType(page[1], typeof(LDStep));
            step = page[1] as LDStep;
            Assert.AreEqual(2, step.Count);
            Assert.AreEqual(StepMode.Reset, step.Mode);
            Assert.AreEqual(0, step.X);
            Assert.AreEqual(0, step.Y);
            Assert.AreEqual(0, step.Z);
            Assert.IsInstanceOfType(page[2], typeof(LDStep));
            step = page[2] as LDStep;
            Assert.AreEqual(0, step.Count);
            Assert.AreEqual(StepMode.Relative, step.Mode);
            Assert.AreEqual(10, step.X);
            Assert.AreEqual(20, step.Y);
            Assert.AreEqual(30, step.Z);
            Assert.IsInstanceOfType(page[3], typeof(LDStep));
            step = page[3] as LDStep;
            Assert.AreEqual(0, step.Count);
            Assert.AreEqual(StepMode.Relative, step.Mode);
            Assert.AreEqual(11, step.X);
            Assert.AreEqual(21, step.Y);
            Assert.AreEqual(31, step.Z);
            Assert.IsInstanceOfType(page[4], typeof(LDStep));
            step = page[4] as LDStep;
            Assert.AreEqual(0, step.Count);
            Assert.AreEqual(StepMode.Absolute, step.Mode);
            Assert.AreEqual(12, step.X);
            Assert.AreEqual(22, step.Y);
            Assert.AreEqual(32, step.Z);
            Assert.IsInstanceOfType(page[5], typeof(LDStep));
            step = page[5] as LDStep;
            Assert.AreEqual(0, step.Count);
            Assert.AreEqual(StepMode.Additive, step.Mode);
            Assert.AreEqual(13, step.X);
            Assert.AreEqual(23, step.Y);
            Assert.AreEqual(33, step.Z);
            Assert.IsInstanceOfType(page[6], typeof(LDStep));
            step = page[6] as LDStep;
            Assert.AreEqual(1, step.Count);
            Assert.AreEqual(StepMode.Additive, step.Mode);
            Assert.AreEqual(0, step.X);
            Assert.AreEqual(0, step.Y);
            Assert.AreEqual(0, step.Z);
        }

        #endregion Parser
    }
}
