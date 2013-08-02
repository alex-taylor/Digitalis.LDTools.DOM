#region License

//
// UnknownMaterialTest.cs
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

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;
    using System.IO;

    #endregion Usings

    [TestClass]
    public sealed class UnknownMaterialTest : IMaterialTest
    {
        #region Infrastructure

        protected override IMaterial CreateTestMaterial()
        {
            return new UnknownMaterial();
        }

        #endregion Infrastructure

        #region Cloning

        [TestMethod]
        public override void CloneTest()
        {
            UnknownMaterial material = new UnknownMaterial("params");
            UnknownMaterial clone    = (UnknownMaterial)material.Clone();

            Assert.AreEqual(material.Parameters, clone.Parameters);

            base.CloneTest();
        }

        [TestMethod]
        public override void IsEquivalentToTest()
        {
            UnknownMaterial material = new UnknownMaterial("params");

            Assert.IsTrue(material.IsEquivalentTo(new UnknownMaterial("params")));
            Assert.IsFalse(material.IsEquivalentTo(new UnknownMaterial("Params")));
            Assert.IsFalse(material.IsEquivalentTo(new UnknownMaterial()));
            Assert.IsFalse(material.IsEquivalentTo(new PlasticMaterial()));
            Assert.IsFalse(material.IsEquivalentTo(null));
        }

        #endregion Cloning

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            UnknownMaterial material = new UnknownMaterial();
            Assert.AreEqual(String.Empty, material.ToCode(new StringBuilder()).ToString());

            material = new UnknownMaterial("UNKNOWN MATERIAL");
            Assert.AreEqual("UNKNOWN MATERIAL", material.ToCode(new StringBuilder()).ToString());

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Constructor

        [TestMethod]
        public void UnknownMaterialConstructorTest()
        {
            // default ctor
            UnknownMaterial material = new UnknownMaterial();
            Assert.IsNull(material.Parameters);

            // code ctor
            material = new UnknownMaterial("UNKNOWN MATERIAL");
            Assert.AreEqual("UNKNOWN MATERIAL", material.Parameters);

            // will also recognise valid code
            material = new UnknownMaterial("METAL");
            Assert.AreEqual("METAL", material.Parameters);
        }

        #endregion Constructor

        #region Parameters

        [TestMethod]
        public void ParametersTest()
        {
            UnknownMaterial material = new UnknownMaterial();
            string oldParams = material.Parameters;
            string newParams = "new params";

            // default value
            Assert.IsNull(material.Parameters);

            // basic set/get
            material.Parameters = newParams;
            Assert.AreEqual(newParams, material.Parameters);

            // undo/redo
            material.Parameters = oldParams;
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            material.Parameters = newParams;
            undoStack.EndCommand();
            Assert.AreEqual(newParams, material.Parameters);
            undoStack.Undo();
            Assert.AreEqual(oldParams, material.Parameters);
            undoStack.Redo();
            Assert.AreEqual(newParams, material.Parameters);

            IColour colour = new LDColour();
            colour.Material = material;

            // Parameters cannot be set if the material is locked
            try
            {
                colour.IsLocked = true;
                material.Parameters = oldParams;
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(newParams, material.Parameters);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }
            finally
            {
                colour.IsLocked = false;
            }

            // Parameters cannot be set if the material is frozen
            try
            {
                colour.Freeze();
                material.Parameters = oldParams;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.AreEqual(newParams, material.Parameters);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }
        }

        [TestMethod]
        public void ParametersChangedTest()
        {
            UnknownMaterial material = new UnknownMaterial();
            string parameters        = "new params";
            bool eventSeen           = false;
            bool genericEventSeen    = false;

            material.ParametersChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(material, sender);
                Assert.IsNull(e.OldValue);
                Assert.AreEqual(parameters, e.NewValue);
            };

            material.Changed += delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(genericEventSeen);
                genericEventSeen = true;
                Assert.AreSame(material, sender);
                Assert.IsInstanceOfType(e, typeof(PropertyChangedEventArgs<string>));

                PropertyChangedEventArgs<string> args = (PropertyChangedEventArgs<string>)e;
                Assert.IsNull(args.OldValue);
                Assert.AreEqual(parameters, args.NewValue);
            };

            material.Parameters = parameters;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);

            // setting to the same value should not generate the events
            eventSeen = false;
            genericEventSeen = false;
            material.Parameters = parameters;
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);
        }

        #endregion Parameters

        #region Self-description

        [TestMethod]
        public void DescriptionTest()
        {
            UnknownMaterial material = new UnknownMaterial();
            Assert.IsNotNull(material.Description);
            Assert.AreEqual(Resources.Material_Unknown, material.Description);
        }

        #endregion Self-description

        #region Serialization

        [TestMethod]
        public override void SerializeTest()
        {
            UnknownMaterial material = new UnknownMaterial();

            using (Stream stream = Utils.SerializeBinary(material))
            {
                UnknownMaterial result = (UnknownMaterial)Utils.DeserializeBinary(stream);
                Assert.AreEqual(material.Parameters, result.Parameters);
            }

            base.SerializeTest();
        }

        #endregion Serialization
    }
}
