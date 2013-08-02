#region License

//
// SpeckleMaterialTest.cs
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
    using System.Drawing;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;
    using System.IO;

    #endregion Usings

    [TestClass]
    public sealed class SpeckleMaterialTest : IMaterialTest
    {
        #region Infrastructure

        protected override IMaterial CreateTestMaterial()
        {
            return new SpeckleMaterial();
        }

        #endregion Infrastructure

        #region Cloning

        [TestMethod]
        public override void CloneTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            SpeckleMaterial clone    = (SpeckleMaterial)material.Clone();

            Assert.AreEqual(material.Value, clone.Value);
            Assert.AreEqual(material.Luminance, clone.Luminance);
            Assert.AreEqual(material.Fraction, clone.Fraction);
            Assert.AreEqual(material.MinSize, clone.MinSize);
            Assert.AreEqual(material.MaxSize, clone.MaxSize);
        }

        [TestMethod]
        public override void IsEquivalentToTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();

            Assert.IsTrue(material.IsEquivalentTo(new SpeckleMaterial()));
            Assert.IsFalse(material.IsEquivalentTo(new PlasticMaterial()));
            Assert.IsFalse(material.IsEquivalentTo(null));

            material.Value = Color.FromArgb(0xFF, 0x59, 0x59, 0x59);
            material.MinSize = 1;
            material.MaxSize = 3;
            material.Fraction = 0.1;
            Assert.IsFalse(material.IsEquivalentTo(new SpeckleMaterial()));
            Assert.IsTrue(material.IsEquivalentTo(new SpeckleMaterial("MATERIAL SPECKLE VALUE #595959 FRACTION 0.1 MINSIZE 1 MAXSIZE 3")));
        }

        #endregion Cloning

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            Assert.AreEqual("MATERIAL SPECKLE VALUE #000000 ALPHA 0 FRACTION 0.5 SIZE 3", material.ToCode(new StringBuilder()).ToString());

            material.Value = Color.Red;
            material.Luminance = 50;
            Assert.AreEqual("MATERIAL SPECKLE VALUE #FF0000 LUMINANCE 50 FRACTION 0.5 SIZE 3", material.ToCode(new StringBuilder()).ToString());

            material.Value = Color.FromArgb(0x7f00ff00);
            material.MaxSize = 10;
            Assert.AreEqual("MATERIAL SPECKLE VALUE #00FF00 ALPHA 127 LUMINANCE 50 FRACTION 0.5 MINSIZE 3 MAXSIZE 10", material.ToCode(new StringBuilder()).ToString());

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Constructor

        [TestMethod]
        public void SpeckleMaterialConstructorTest()
        {
            // default ctor
            SpeckleMaterial material = new SpeckleMaterial();
            Assert.AreEqual(Color.Empty, material.Value);
            Assert.AreEqual(0, material.Luminance);
            Assert.AreEqual(0.5, material.Fraction);
            Assert.AreEqual(3U, material.MinSize);
            Assert.AreEqual(3U, material.MaxSize);

            // full syntax
            material = new SpeckleMaterial("MATERIAL SPECKLE VALUE #595959 ALPHA 127 LUMINANCE 50 FRACTION 0.4 MINSIZE 1 MAXSIZE 3");
            Assert.AreEqual(Color.FromArgb(0x7F595959), material.Value);
            Assert.AreEqual(50, material.Luminance);
            Assert.AreEqual(0.4, material.Fraction);
            Assert.AreEqual(1U, material.MinSize);
            Assert.AreEqual(3U, material.MaxSize);

            // minimal syntax
            material = new SpeckleMaterial("MATERIAL SPECKLE VALUE #595959 FRACTION 0.4 SIZE 5");
            Assert.AreEqual(Color.FromArgb(0xFF, 0x59, 0x59, 0x59), material.Value);
            Assert.AreEqual(0, material.Luminance);
            Assert.AreEqual(0.4, material.Fraction);
            Assert.AreEqual(5U, material.MinSize);
            Assert.AreEqual(5U, material.MaxSize);

            // invalid syntax
            try
            {
                // no data
                material = new SpeckleMaterial("");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }

            try
            {
                // wrong material
                material = new SpeckleMaterial("RUBBER");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }

            try
            {
                // missing parameters
                material = new SpeckleMaterial("MATERIAL SPECKLE VALUE #123456 SIZE 3");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }

            try
            {
                // unrecognised parameter - could indicate that the spec has changed
                material = new SpeckleMaterial("MATERIAL SPECKLE VALUE #123456 FRACTION 0.1 VFRACTION 1.0 SIZE 3");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }

            try
            {
                // invalid sizes
                material = new SpeckleMaterial("MATERIAL SPECKLE VALUE #123456 FRACTION 0.1 MINSIZE 4 MAXSIZE 1");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }
        }

        #endregion Constructor

        #region Properties

        [TestMethod]
        public void ValueTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            Color oldValue           = material.Value;
            Color newValue           = Color.Red;

            // default value
            Assert.AreEqual(Color.Empty, material.Value);

            // basic set/get
            material.Value = newValue;
            Assert.AreEqual(newValue, material.Value);

            // undo/redo
            material.Value = oldValue;
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            material.Value = newValue;
            undoStack.EndCommand();
            Assert.AreEqual(newValue, material.Value);
            undoStack.Undo();
            Assert.AreEqual(oldValue, material.Value);
            undoStack.Redo();
            Assert.AreEqual(newValue, material.Value);

            IColour colour = new LDColour();
            colour.Material = material;

            // Value cannot be set if the material is locked
            try
            {
                colour.IsLocked = true;
                material.Value = oldValue;
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(newValue, material.Value);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }
            finally
            {
                colour.IsLocked = false;
            }

            // Value cannot be set if the material is frozen
            try
            {
                colour.Freeze();
                material.Value = oldValue;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.AreEqual(newValue, material.Value);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }
        }

        [TestMethod]
        public void ValueChangedTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            Color oldValue           = material.Value;
            Color newValue           = Color.Red;
            bool eventSeen           = false;
            bool genericEventSeen    = false;

            material.ValueChanged += delegate(object sender, PropertyChangedEventArgs<Color> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(material, sender);
                Assert.AreEqual(oldValue, e.OldValue);
                Assert.AreEqual(newValue, e.NewValue);
            };

            material.Changed += delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(genericEventSeen);
                genericEventSeen = true;
                Assert.AreSame(material, sender);
                Assert.IsInstanceOfType(e, typeof(PropertyChangedEventArgs<Color>));

                PropertyChangedEventArgs<Color> args = (PropertyChangedEventArgs<Color>)e;
                Assert.AreEqual(oldValue, args.OldValue);
                Assert.AreEqual(newValue, args.NewValue);
            };

            material.Value = newValue;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);

            // setting to the same value should not generate the events
            eventSeen = false;
            genericEventSeen = false;
            material.Value = newValue;
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);
        }

        [TestMethod()]
        public void LuminanceTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            byte oldValue            = material.Luminance;
            byte newValue            = 10;

            // default value
            Assert.AreEqual(0, material.Luminance);

            // basic set/get
            material.Luminance = newValue;
            Assert.AreEqual(newValue, material.Luminance);

            // undo/redo
            material.Luminance = oldValue;
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            material.Luminance = newValue;
            undoStack.EndCommand();
            Assert.AreEqual(newValue, material.Luminance);
            undoStack.Undo();
            Assert.AreEqual(oldValue, material.Luminance);
            undoStack.Redo();
            Assert.AreEqual(newValue, material.Luminance);

            IColour colour = new LDColour();
            colour.Material = material;

            // Luminance cannot be set if the material is locked
            try
            {
                colour.IsLocked = true;
                material.Luminance = oldValue;
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(newValue, material.Luminance);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }
            finally
            {
                colour.IsLocked = false;
            }

            // Luminance cannot be set if the material is frozen
            try
            {
                colour.Freeze();
                material.Luminance = oldValue;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.AreEqual(newValue, material.Luminance);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }
        }

        [TestMethod]
        public void LuminanceChangedTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            byte oldValue            = material.Luminance;
            byte newValue            = 10;
            bool eventSeen           = false;
            bool genericEventSeen    = false;

            material.LuminanceChanged += delegate(object sender, PropertyChangedEventArgs<byte> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(material, sender);
                Assert.AreEqual(oldValue, e.OldValue);
                Assert.AreEqual(newValue, e.NewValue);
            };

            material.Changed += delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(genericEventSeen);
                genericEventSeen = true;
                Assert.AreSame(material, sender);
                Assert.IsInstanceOfType(e, typeof(PropertyChangedEventArgs<byte>));

                PropertyChangedEventArgs<byte> args = (PropertyChangedEventArgs<byte>)e;
                Assert.AreEqual(oldValue, args.OldValue);
                Assert.AreEqual(newValue, args.NewValue);
            };

            material.Luminance = newValue;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);

            // setting to the same value should not generate the events
            eventSeen          = false;
            genericEventSeen   = false;
            material.Luminance = newValue;
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);
        }

        [TestMethod()]
        public void FractionTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            double oldValue          = material.Fraction;
            double newValue          = 0.1;

            // default value
            Assert.AreEqual(0.5, material.Fraction);

            // min/max value clipping
            material.Fraction = 0.0;
            Assert.AreEqual(0.5, material.Fraction);
            material.Fraction = 1.0;
            Assert.AreEqual(0.5, material.Fraction);

            // basic set/get
            material.Fraction = newValue;
            Assert.AreEqual(newValue, material.Fraction);

            // undo/redo
            material.Fraction = oldValue;
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            material.Fraction = newValue;
            undoStack.EndCommand();
            Assert.AreEqual(newValue, material.Fraction);
            undoStack.Undo();
            Assert.AreEqual(oldValue, material.Fraction);
            undoStack.Redo();
            Assert.AreEqual(newValue, material.Fraction);

            IColour colour = new LDColour();
            colour.Material = material;

            // Fraction cannot be set if the material is locked
            try
            {
                colour.IsLocked = true;
                material.Fraction = oldValue;
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(newValue, material.Fraction);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }
            finally
            {
                colour.IsLocked = false;
            }

            // Fraction cannot be set if the material is frozen
            try
            {
                colour.Freeze();
                material.Fraction = oldValue;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.AreEqual(newValue, material.Fraction);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }
        }

        [TestMethod]
        public void FractionChangedTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            double oldValue          = material.Fraction;
            double newValue          = 0.1;
            bool eventSeen           = false;
            bool genericEventSeen    = false;

            material.FractionChanged += delegate(object sender, PropertyChangedEventArgs<double> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(material, sender);
                Assert.AreEqual(oldValue, e.OldValue);
                Assert.AreEqual(newValue, e.NewValue);
            };

            material.Changed += delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(genericEventSeen);
                genericEventSeen = true;
                Assert.AreSame(material, sender);
                Assert.IsInstanceOfType(e, typeof(PropertyChangedEventArgs<double>));

                PropertyChangedEventArgs<double> args = (PropertyChangedEventArgs<double>)e;
                Assert.AreEqual(oldValue, args.OldValue);
                Assert.AreEqual(newValue, args.NewValue);
            };

            material.Fraction = newValue;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);

            // setting to the same value should not generate the events
            eventSeen         = false;
            genericEventSeen  = false;
            material.Fraction = newValue;
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);
        }

        [TestMethod()]
        public void MaxSizeTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            uint oldValue            = material.MaxSize;
            uint newValue            = 5;

            // default value
            Assert.AreEqual(3U, material.MaxSize);

            // basic set/get
            material.MaxSize = newValue;
            Assert.AreEqual(newValue, material.MaxSize);

            // min value clipping
            material.MaxSize = 0;
            Assert.AreEqual(material.MinSize, material.MaxSize);
            material.MaxSize = material.MinSize - 1;
            Assert.AreEqual(material.MinSize, material.MaxSize);

            // undo/redo
            material.MaxSize = oldValue;
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            material.MaxSize = newValue;
            undoStack.EndCommand();
            Assert.AreEqual(newValue, material.MaxSize);
            undoStack.Undo();
            Assert.AreEqual(oldValue, material.MaxSize);
            undoStack.Redo();
            Assert.AreEqual(newValue, material.MaxSize);

            IColour colour = new LDColour();
            colour.Material = material;

            // MaxSize cannot be set if the material is locked
            try
            {
                colour.IsLocked = true;
                material.MaxSize = oldValue;
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(newValue, material.MaxSize);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }
            finally
            {
                colour.IsLocked = false;
            }

            // MaxSize cannot be set if the material is frozen
            try
            {
                colour.Freeze();
                material.MaxSize = oldValue;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.AreEqual(newValue, material.MaxSize);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }
        }

        [TestMethod]
        public void MaxSizeChangedTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            uint oldValue            = material.MaxSize;
            uint newValue            = 5;
            bool eventSeen           = false;
            bool genericEventSeen    = false;

            material.MaxSizeChanged += delegate(object sender, PropertyChangedEventArgs<uint> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(material, sender);
                Assert.AreEqual(oldValue, e.OldValue);
                Assert.AreEqual(newValue, e.NewValue);
            };

            material.Changed += delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(genericEventSeen);
                genericEventSeen = true;
                Assert.AreSame(material, sender);
                Assert.IsInstanceOfType(e, typeof(PropertyChangedEventArgs<uint>));

                PropertyChangedEventArgs<uint> args = (PropertyChangedEventArgs<uint>)e;
                Assert.AreEqual(oldValue, args.OldValue);
                Assert.AreEqual(newValue, args.NewValue);
            };

            material.MaxSize = newValue;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);

            // setting to the same value should not generate the events
            eventSeen        = false;
            genericEventSeen = false;
            material.MaxSize = newValue;
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);
        }

        [TestMethod()]
        public void MinSizeTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            uint oldValue            = material.MinSize;
            uint newValue            = 1;

            // default value
            Assert.AreEqual(3U, material.MinSize);

            // basic set/get
            material.MinSize = newValue;
            Assert.AreEqual(newValue, material.MinSize);

            // min value clipping
            material.MinSize = 0;
            Assert.AreEqual(1U, material.MinSize);
            material.MinSize = material.MaxSize + 1;
            Assert.AreEqual(material.MaxSize, material.MinSize);

            // undo/redo
            material.MinSize = oldValue;
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            material.MinSize = newValue;
            undoStack.EndCommand();
            Assert.AreEqual(newValue, material.MinSize);
            undoStack.Undo();
            Assert.AreEqual(oldValue, material.MinSize);
            undoStack.Redo();
            Assert.AreEqual(newValue, material.MinSize);

            IColour colour = new LDColour();
            colour.Material = material;

            // MinSize cannot be set if the material is locked
            try
            {
                colour.IsLocked = true;
                material.MinSize = oldValue;
                Assert.Fail();
            }
            catch (ElementLockedException)
            {
                Assert.AreEqual(newValue, material.MinSize);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }
            finally
            {
                colour.IsLocked = false;
            }

            // MaxSize cannot be set if the material is frozen
            try
            {
                colour.Freeze();
                material.MinSize = oldValue;
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.AreEqual(newValue, material.MinSize);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }
        }

        [TestMethod]
        public void MinSizeChangedTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            uint oldValue            = material.MinSize;
            uint newValue            = 1;
            bool eventSeen           = false;
            bool genericEventSeen    = false;

            material.MinSizeChanged += delegate(object sender, PropertyChangedEventArgs<uint> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(material, sender);
                Assert.AreEqual(oldValue, e.OldValue);
                Assert.AreEqual(newValue, e.NewValue);
            };

            material.Changed += delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(genericEventSeen);
                genericEventSeen = true;
                Assert.AreSame(material, sender);
                Assert.IsInstanceOfType(e, typeof(PropertyChangedEventArgs<uint>));

                PropertyChangedEventArgs<uint> args = (PropertyChangedEventArgs<uint>)e;
                Assert.AreEqual(oldValue, args.OldValue);
                Assert.AreEqual(newValue, args.NewValue);
            };

            material.MinSize = newValue;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);

            // setting to the same value should not generate the events
            eventSeen = false;
            genericEventSeen = false;
            material.MinSize = newValue;
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);
        }

        #endregion Properties

        #region Self-description

        [TestMethod]
        public void DescriptionTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();
            Assert.IsNotNull(material.Description);
            Assert.AreEqual(Resources.Material_Speckle, material.Description);
        }

        #endregion Self-description

        #region Serialization

        [TestMethod]
        public override void SerializeTest()
        {
            SpeckleMaterial material = new SpeckleMaterial();

            using (Stream stream = Utils.SerializeBinary(material))
            {
                SpeckleMaterial result = (SpeckleMaterial)Utils.DeserializeBinary(stream);
                Assert.AreEqual(material.Fraction, result.Fraction);
                Assert.AreEqual(material.MinSize, result.MinSize);
                Assert.AreEqual(material.MaxSize, result.MaxSize);
                Assert.AreEqual(material.Luminance, result.Luminance);
                Assert.AreEqual(material.Value, result.Value);
            }

            base.SerializeTest();
        }

        #endregion Serialization
    }
}
