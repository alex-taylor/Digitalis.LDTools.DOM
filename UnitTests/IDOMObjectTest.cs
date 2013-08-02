#region License

//
// IDOMObjectTest.cs
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
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class IDOMObjectTest : TestBase
    {
        #region Infrastructure

        protected override Type InterfaceType { get { return typeof(IDOMObject); } }

        protected abstract IDOMObject CreateTestObject();

        protected abstract IDOMObject CreateTestObjectWithDocumentTree();

        protected abstract IDOMObject CreateTestObjectWithFrozenAncestor();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            // all non-abstract types must have a default constructor, as it is required for Clone() to function
            if (!TestClassType.IsAbstract)
                Assert.IsNotNull(TestClassType.GetConstructor(Type.EmptyTypes), TestClassType.FullName + " must have a default constructor");

            Assert.IsTrue(TestClassType.IsSerializable, TestClassType.FullName + " must be marked as serializable");

            IDOMObject obj = CreateTestObject();
            Assert.IsTrue(Enum.IsDefined(typeof(DOMObjectType), obj.ObjectType), TestClassType.FullName + ".ObjectType must be a member of DOMObjectType");

            ElementFlagsAttribute elementFlagsAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;

            if (null != elementFlagsAttr)
            {
                bool isImmutable = (0 != (ElementFlags.Immutable & elementFlagsAttr.Flags));
                Assert.AreEqual(obj.IsImmutable, isImmutable, TestClassType.FullName + ".IsImmutable and ElementFlagsAttribute must have the same value");
            }
            else if (obj.IsImmutable)
            {
                Assert.Fail("An ElementFlagsAttribute with ElementFlags.Immutable set must be present on the " + TestClassType.FullName);
            }

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Cloning and Serialization

        protected virtual IDOMObject CreateTestObjectForCopying()
        {
            return CreateTestObject();
        }

        protected virtual void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            Assert.AreNotSame(original, copy, "The copied " + TestClassType.FullName + " is the same object as the original");
            Assert.AreEqual(original.ObjectType, copy.ObjectType);
            Assert.AreEqual(original.IsImmutable, copy.IsImmutable);
        }

        [TestMethod]
        public void CloneTest()
        {
            IDOMObject obj   = CreateTestObjectForCopying();
            IDOMObject clone = obj.Clone();

            // let the subclass do its checks
            CompareCopiedObjects(obj, clone);

            // IsFrozen should not be preserved
            obj.Freeze();
            clone = obj.Clone();
            Assert.IsFalse(clone.IsFrozen, "The cloned " + TestClassType.FullName + " should not be frozen");

            // a disposed object cannot be cloned
            obj = CreateTestObject();
            Utils.DisposalAccessTest(obj, delegate() { obj.Clone(); });
        }

        [TestMethod]
        public void SerializeBinaryTest()
        {
            IDOMObject obj = CreateTestObjectForCopying();

            // let the subclass do its checks
            using (Stream stream = Utils.SerializeBinary(obj))
            {
                IDOMObject result = (IDOMObject)Utils.DeserializeBinary(stream);
                CompareCopiedObjects(obj, result);
            }

            obj = CreateTestObjectForCopying();
            obj.Freeze();

            using (Stream stream = Utils.SerializeBinary(obj))
            {
                IDOMObject result = (IDOMObject)Utils.DeserializeBinary(stream);

                // IsFrozen should be preserved
                Assert.IsTrue(result.IsFrozen, "The deserialized " + TestClassType.FullName + " should be frozen");
            }

            // a disposed object cannot be serialized
            obj = CreateTestObjectForCopying();
            Utils.DisposalAccessTest(obj, delegate() { Utils.SerializeBinary(obj); });
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public virtual void ToCodeTest()
        {
            IDOMObject obj   = CreateTestObject();
            StringBuilder sb = new StringBuilder();

            Assert.AreSame(sb, obj.ToCode(sb, CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));

            Utils.DisposalAccessTest(obj, delegate()
            {
                obj.ToCode(sb, CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal);
            });
        }

        #endregion Code-generation

        #region Disposal

        [TestMethod]
        public virtual void DisposeTest()
        {
            // a 'loose' object should dispose without errors
            IDOMObject obj = CreateTestObject();
            obj.Dispose();

            // calling Dispose() again should have no effect
            obj.Dispose();

            // one attached to a document-tree should also clean up
            obj = CreateTestObjectWithDocumentTree();
            obj.Dispose();

            // but one attached to a frozen tree should throw an exception
            obj = CreateTestObjectWithFrozenAncestor();

            if (null != obj)
            {
                try
                {
                    obj.Dispose();
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType(), TestClassType.FullName + " disposed whilst attached to a frozen parent");
                }
            }

            // a frozen object can be disposed of
            obj = CreateTestObject();
            obj.Freeze();
            obj.Dispose();
        }

        [TestMethod]
        public void IsDisposedTest()
        {
            IDOMObject obj          = CreateTestObject();
            IDOMObject objToDispose = obj;

            Assert.IsFalse(obj.IsDisposed);

            if (obj is ITexmapGeometry)
                objToDispose = ((ITexmapGeometry)obj).Texmap;

            objToDispose.Dispose();
            Assert.IsTrue(obj.IsDisposed, TestClassType.FullName + " failed to dispose");

            // calling Dispose() again should have no effect
            objToDispose.Dispose();
            Assert.IsTrue(obj.IsDisposed, TestClassType.FullName + " changed the value of IsDisposed to false after disposal");
        }

        #endregion Disposal

        #region Freezing

        [TestMethod]
        public virtual void IsFrozenTest()
        {
            IDOMObject obj = CreateTestObject();

            Assert.IsFalse(obj.IsFrozen);
            obj.Freeze();
            Assert.IsTrue(obj.IsFrozen, TestClassType.FullName + " failed to freeze");

            // objects are frozen implicitly if an ancestor is frozen
            obj = CreateTestObjectWithFrozenAncestor();

            if (null != obj)
                Assert.IsTrue(obj.IsFrozen, TestClassType.FullName + " has a frozen ancestor but reports itself to not be frozen");

            // once disposed, the property should not function
            obj = CreateTestObject();
            Utils.DisposalAccessTest(obj, delegate() { bool isFrozen = obj.IsFrozen; });
        }

        [TestMethod]
        public void FreezeTest()
        {
            IDOMObject obj = CreateTestObject();

            Assert.IsFalse(obj.IsFrozen);
            obj.Freeze();
            Assert.IsTrue(obj.IsFrozen, TestClassType.FullName + " failed to freeze");

            // calling Freeze() again should have no effect
            obj.Freeze();

            // a disposed object is not freezeable
            obj = CreateTestObject();
            Utils.DisposalAccessTest(obj, delegate() { obj.Freeze(); });
        }

        #endregion Freezing

        #region Properties

        protected enum PropertyValueFlags
        {
            None = 0,
            SettableWhenLocked = 1 << 0,
            NotDisposable = 1 << 1
        }

        protected delegate T PropertyValueGetter<C, T>(C obj) where C : IDOMObject;

        protected delegate void PropertyValueSetter<C, T>(C obj, T value) where C : IDOMObject;

        protected delegate void PropertyValueComparer<C, T>(C obj, T expectedValue) where C : IDOMObject;

        protected delegate void AttachPropertyChangedHandler<C, T>(C obj, PropertyChangedEventHandler<T> handler) where C : IDOMObject;

        protected delegate void PropertyChangedComparer<C, T>(C obj, T oldValue, T newValue, PropertyChangedEventArgs<T> e) where C : IDOMObject;

        protected virtual void PropertyValueTest<C, T>(C obj,
                                                      T defaultValue,
                                                      T newValue,
                                                      PropertyValueGetter<C,T> getter,
                                                      PropertyValueSetter<C,T> setter,
                                                      PropertyValueFlags flags) where C : IDOMObject
        {
            PropertyValueTest<C,T>(obj, defaultValue, newValue, getter, setter, null, flags);
        }

        protected virtual void PropertyValueTest<C,T>(C obj,
                                                      T defaultValue,
                                                      T newValue,
                                                      PropertyValueGetter<C,T> getter,
                                                      PropertyValueSetter<C,T> setter,
                                                      PropertyValueComparer<C,T> comparer,
                                                      PropertyValueFlags flags) where C : IDOMObject
        {
            T oldValue = getter(obj);

            if (null == comparer)
                comparer = delegate(C obj2, T expectedValue) { Assert.AreEqual(expectedValue, getter(obj2)); };

            if (obj.IsImmutable)
            {
                try
                {
                    setter(obj, newValue);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(NotSupportedException), e.GetType());
                    comparer(obj, oldValue);
                }
            }
            else
            {
                // default value
                comparer(obj, defaultValue);

                // basic set/get
                setter(obj, newValue);
                comparer(obj, newValue);
                setter(obj, oldValue);
                comparer(obj, oldValue);

                // range-check
                if (defaultValue is Enum)
                {
                    try
                    {
                        object minValue = Enum.GetValues(defaultValue.GetType()).Cast<int>().Min() - 1;
                        setter(obj, (T)minValue);
                        Assert.Fail();
                    }
                    catch (Exception e)
                    {
                        Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                    }

                    try
                    {
                        object maxValue = Enum.GetValues(defaultValue.GetType()).Cast<int>().Max() + 1;
                        setter(obj, (T)maxValue);
                        Assert.Fail();
                    }
                    catch (Exception e)
                    {
                        Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                    }
                }

                // undo/redo
                UndoStack undoStack = new UndoStack();
                undoStack.StartCommand("command");
                setter(obj, newValue);
                undoStack.EndCommand();
                comparer(obj, newValue);
                undoStack.Undo();
                comparer(obj, oldValue);
                undoStack.Redo();
                comparer(obj, newValue);
                setter(obj, oldValue);

                // it should not be possible to undo the command once the object is frozen
                undoStack.StartCommand("command");
                setter(obj, newValue);
                undoStack.EndCommand();
                obj.Freeze();
                Assert.IsTrue(obj.IsFrozen);

                try
                {
                    undoStack.Undo();
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    comparer(obj, newValue);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // the property should not be settable once the object is frozen
                try
                {
                    setter(obj, oldValue);
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    comparer(obj, newValue);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // the property should not be gettable or settable once the object has been disposed of
                if (0 == (PropertyValueFlags.NotDisposable & flags))
                {
                    obj = (C)obj.Clone();

                    Utils.DisposalAccessTest(obj, delegate() { setter(obj, oldValue); });
                    Utils.DisposalAccessTest(obj, delegate() { getter(obj); });
                }
            }
        }

        protected virtual void PropertyChangedTest<C, T>(C obj,
                                                         string eventName,
                                                         T valueToSet,
                                                         AttachPropertyChangedHandler<C, T> attachHandler,
                                                         PropertyValueGetter<C, T> getter,
                                                         PropertyValueSetter<C, T> setter) where C : IDOMObject
        {
            PropertyChangedTest<C, T>(obj, eventName, valueToSet, attachHandler, getter, setter, null);
        }

        protected virtual void PropertyChangedTest<C, T>(C obj,
                                                         string eventName,
                                                         T valueToSet,
                                                         AttachPropertyChangedHandler<C, T> attachHandler,
                                                         PropertyValueGetter<C, T> getter,
                                                         PropertyValueSetter<C, T> setter,
                                                         PropertyChangedComparer<C, T> comparer) where C : IDOMObject
        {
            if (!obj.IsImmutable)
            {
                T originalValue       = getter(obj);
                bool eventSeen        = false;
                bool genericEventSeen = false;

                if (null == comparer)
                {
                    comparer = delegate(C obj2, T oldValue, T newValue, PropertyChangedEventArgs<T> e)
                    {
                        Assert.AreEqual(oldValue, e.OldValue);
                        Assert.AreEqual(newValue, e.NewValue);
                    };
                }

                // property-specific event
                attachHandler(obj, delegate(object sender, PropertyChangedEventArgs<T> e)
                {
                    if (obj.IsDisposing || obj.IsDisposed)
                        return;

                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(obj, sender);
                    comparer(obj, originalValue, valueToSet, e);
                });

                // generic 'something changed' event
                obj.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    if (obj.IsDisposing || obj.IsDisposed)
                        return;

                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(obj, sender);
                    Assert.AreSame(obj, e.Source);
                    Assert.AreEqual(eventName, e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<T>));
                    comparer(obj, originalValue, valueToSet, (PropertyChangedEventArgs<T>)e.Parameters);
                };

                // setting the value should trigger both events
                setter(obj, valueToSet);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);
                eventSeen        = false;
                genericEventSeen = false;

                // setting the same value again should not trigger the events
                setter(obj, valueToSet);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);

                // cloning the object should not persist the event-handlers
                C copy                    = (C)obj.Clone();
                bool copyEventSeen        = false;
                bool copyGenericEventSeen = false;

                // property-specific event
                PropertyChangedEventHandler<T> propertyEventHandler = delegate(object sender, PropertyChangedEventArgs<T> e)
                {
                    if (copy.IsDisposing || copy.IsDisposed)
                        return;

                    Assert.IsFalse(copyEventSeen);
                    copyEventSeen = true;
                    Assert.AreSame(copy, sender);
                    comparer(copy, originalValue, valueToSet, e);
                };

                // generic 'something changed' event
                ObjectChangedEventHandler genericEventHandler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    if (copy.IsDisposing || copy.IsDisposed)
                        return;

                    Assert.IsFalse(copyGenericEventSeen);
                    copyGenericEventSeen = true;
                    Assert.AreSame(copy, sender);
                    Assert.AreEqual(eventName, e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<T>));
                    comparer(copy, originalValue, valueToSet, (PropertyChangedEventArgs<T>)e.Parameters);
                };

                if (obj is IElement)
                    ((IElement)copy).Parent = ((IElement)obj).Parent;

                if (valueToSet is IMaterial)
                    valueToSet = (T)((IMaterial)valueToSet).Clone();

                setter(copy, originalValue);
                attachHandler(copy, propertyEventHandler);
                copy.Changed += genericEventHandler;
                setter(copy, valueToSet);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
                Assert.IsTrue(copyEventSeen);
                Assert.IsTrue(copyGenericEventSeen);
                copyEventSeen        = false;
                copyGenericEventSeen = false;

                // copy-by-serialization should not persist them either
                using (Stream stream = Utils.SerializeBinary(obj))
                {
                    copy = (C)Utils.DeserializeBinary(stream);
                }

                if (obj is IElement)
                    ((IElement)copy).Parent = ((IElement)obj).Parent;

                if (valueToSet is IMaterial)
                    valueToSet = (T)((IMaterial)valueToSet).Clone();

                setter(copy, originalValue);
                attachHandler(copy, propertyEventHandler);
                copy.Changed += genericEventHandler;
                setter(copy, valueToSet);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
                Assert.IsTrue(copyEventSeen);
                Assert.IsTrue(copyGenericEventSeen);
            }
        }

        #endregion Properties
    }
}
