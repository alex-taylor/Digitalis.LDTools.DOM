#region License

//
// IDOMObjectCollectionTest.cs
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

    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    #endregion Usings

    public static class IDOMObjectCollectionTest
    {
        #region Collection-management

        public static void ItemsAddedTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.ItemsAddedTest() not implemented for read-only collections");
            }
            else
            {
                bool eventSeen = false;

                UndoableListChangedEventHandler<T> handler = delegate(object sender, UndoableListChangedEventArgs<T> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(collection, sender);
                    Assert.AreEqual(1, e.Count);
                    Assert.AreSame(objectToAdd, e.Items.ElementAt(0));
                };

                collection.ItemsAdded += handler;
                collection.Add(objectToAdd);
                Assert.IsTrue(eventSeen);
                collection.ItemsAdded -= handler;
                collection.Clear();
            }
        }

        public static void ItemsRemovedTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.ItemsRemovedTest() not implemented for read-only collections");
            }
            else
            {
                bool eventSeen = false;

                collection.Add(objectToAdd);

                UndoableListChangedEventHandler<T> handler = delegate(object sender, UndoableListChangedEventArgs<T> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(collection, sender);
                    Assert.AreEqual(1, e.Count);
                    Assert.AreSame(objectToAdd, e.Items.ElementAt(0));
                };

                collection.ItemsRemoved += handler;
                collection.Remove(objectToAdd);
                Assert.IsTrue(eventSeen);
                collection.ItemsRemoved -= handler;
                collection.Clear();
            }
        }

        public static void ItemsReplacedTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.ItemsReplacedTest() not implemented for read-only collections");
            }
            else
            {
                T objectToReplace = (T)objectToAdd.Clone();
                bool eventSeen = false;

                collection.Add(objectToReplace);

                UndoableListReplacedEventHandler<T> handler = delegate(object sender, UndoableListReplacedEventArgs<T> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(collection, sender);

                    if (0 != e.ItemsAdded.Count)
                    {
                        Assert.AreEqual(1, e.ItemsAdded.Count);
                        Assert.AreEqual(0, e.ItemsAdded.FirstIndex);
                        Assert.AreSame(objectToAdd, e.ItemsAdded.Items.ElementAt(0));
                    }
                    else
                    {
                        Assert.AreEqual(1, e.ItemsRemoved.Count);
                        Assert.AreEqual(0, e.ItemsRemoved.FirstIndex);
                        Assert.AreSame(objectToReplace, e.ItemsRemoved.Items.ElementAt(0));
                    }
                };

                collection.ItemsReplaced += handler;
                collection[0] = objectToAdd;
                Assert.IsTrue(eventSeen);
                collection.ItemsReplaced -= handler;
                collection.Clear();
            }
        }

        public static void CollectionClearedTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.CollectionClearedTest() not implemented for read-only collections");
            }
            else
            {
                bool eventSeen = false;

                collection.Add(objectToAdd);

                UndoableListChangedEventHandler<T> handler = delegate(object sender, UndoableListChangedEventArgs<T> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(collection, sender);
                    Assert.AreEqual(1, e.Count);
                    Assert.AreSame(objectToAdd, e.Items.ElementAt(0));
                };

                collection.CollectionCleared += handler;
                collection.Clear();
                Assert.IsTrue(eventSeen);
                collection.CollectionCleared -= handler;
            }
        }

        public static void CanInsertTest<T>(IDOMObjectCollection<T> collection, IDOMObjectCollection<T> collection2, T objectToCheck) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanInsert(objectToCheck, InsertCheckFlags.None));
            }
            else
            {
                // basic function
                Assert.AreEqual(InsertCheckResult.CanInsert, collection.CanInsert(objectToCheck, InsertCheckFlags.None));

                // null cannot be added
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanInsert(null, InsertCheckFlags.None));

                // an object already in a collection cannot be added
                collection2.Add(objectToCheck);
                Assert.AreEqual(InsertCheckResult.AlreadyMember, collection.CanInsert(objectToCheck, InsertCheckFlags.None));

                // unless we specify the 'ignore' flag
                Assert.AreEqual(InsertCheckResult.CanInsert, collection.CanInsert(objectToCheck, InsertCheckFlags.IgnoreCurrentCollection));

                // frozen objects cannot be added
                T newObject = (T)objectToCheck.Clone();
                newObject.Freeze();
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanInsert(newObject, InsertCheckFlags.None));

                // disposed objects cannot be added
                newObject.Dispose();
                Assert.IsTrue(newObject.IsDisposed);
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanInsert(newObject, InsertCheckFlags.None));
            }
        }

        public static void CanReplaceTest<T>(IDOMObjectCollection<T> collection, IDOMObjectCollection<T> collection2, T objectToCheck, T objectToReplace) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanReplace(objectToCheck, objectToReplace, InsertCheckFlags.None));
            }
            else
            {
                // basic function
                Assert.AreEqual(InsertCheckResult.CanInsert, collection.CanReplace(objectToCheck, null, InsertCheckFlags.None));
                collection.Add(objectToReplace);
                Assert.AreEqual(InsertCheckResult.CanInsert, collection.CanReplace(objectToCheck, objectToReplace, InsertCheckFlags.None));

                // null cannot be added
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanReplace(null, objectToReplace, InsertCheckFlags.None));

                // an object already in a collection cannot be added
                collection2.Add(objectToCheck);
                Assert.AreEqual(InsertCheckResult.AlreadyMember, collection.CanReplace(objectToCheck, objectToReplace, InsertCheckFlags.None));

                // unless we specify the 'ignore' flag
                Assert.AreEqual(InsertCheckResult.CanInsert, collection.CanReplace(objectToCheck, objectToReplace, InsertCheckFlags.IgnoreCurrentCollection));
                collection2.Clear();

                // frozen objects cannot be added
                T newObject = (T)objectToCheck.Clone();
                newObject.Freeze();
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanReplace(newObject, objectToReplace, InsertCheckFlags.None));

                // disposed objects cannot be added
                newObject.Dispose();
                Assert.IsTrue(newObject.IsDisposed);
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanReplace(newObject, objectToReplace, InsertCheckFlags.None));
            }
        }

        public static void CountTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.CountTest() not implemented for read-only collections");
            }
            else
            {
                int count = collection.Count;

                collection.Add(objectToAdd);
                Assert.AreEqual(count + 1, collection.Count);
            }
        }

        public static void IndexOfTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.IndexOfTest() not implemented for read-only collections");
            }
            else
            {
                collection.Clear();
                collection.Add(objectToAdd);
                Assert.AreEqual(0, collection.IndexOf(objectToAdd));

                T newObject = (T)objectToAdd.Clone();

                if (DOMObjectType.Page == newObject.ObjectType)
                    ((IPage)newObject).Name += "_copy";

                Assert.AreEqual(-1, collection.IndexOf(newObject));
                collection.Insert(0, newObject);
                Assert.AreEqual(0, collection.IndexOf(newObject));
                Assert.AreEqual(1, collection.IndexOf(objectToAdd));
            }
        }

        public static void ContainsTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.ContainsTest() not implemented for read-only collections");
            }
            else
            {
                collection.Add(objectToAdd);
                Assert.IsTrue(collection.Contains(objectToAdd));

                T newObject = (T)objectToAdd.Clone();
                Assert.IsFalse(collection.Contains(newObject));
            }
        }

        public static void IndexerTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanInsert(objectToAdd, InsertCheckFlags.None));

                int count = collection.Count;

                try
                {
                    collection[0] = objectToAdd;
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                    Assert.AreEqual(count, collection.Count);
                    Assert.IsFalse(collection.Contains(objectToAdd));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(NotSupportedException), e.GetType());
                }
            }
            else
            {
                collection.Add(objectToAdd);
                Assert.AreEqual(1, collection.Count);

                // getter
                Assert.AreSame(objectToAdd, collection[0]);

                // getter: range-checks
                try
                {
                    Assert.IsNotNull(collection[-1]);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                try
                {
                    Assert.IsNotNull(collection[10]);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                // setter
                T newObject = (T)objectToAdd.Clone();
                collection[0] = newObject;
                Assert.AreEqual(1, collection.Count);
                Assert.AreSame(newObject, collection[0]);

                // setter: range-checks
                try
                {
                    collection[-1] = newObject;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(1, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                try
                {
                    collection[10] = newObject;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(1, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                // setter: undo/redo
                UndoStack undoStack = new UndoStack();
                undoStack.StartCommand("command");
                collection[0] = objectToAdd;
                undoStack.EndCommand();
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains(objectToAdd));
                undoStack.Undo();
                Assert.AreEqual(1, collection.Count);
                Assert.IsFalse(collection.Contains(objectToAdd));
                undoStack.Redo();
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains(objectToAdd));

                // TODO: CanReplace checks

                // setter: cannot add an object which is already a member
                try
                {
                    collection[0] = objectToAdd;
                    Assert.Fail();
                }
                catch (InvalidOperationException)
                {
                    Assert.AreEqual(1, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
                }

                // setter: cannot add null values
                try
                {
                    collection[0] = null;
                    Assert.Fail();
                }
                catch (ArgumentNullException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.AreSame(objectToAdd, collection[0]);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentNullException), e.GetType());
                }

                // setter: cannot add a frozen element
                newObject.Freeze();
                Assert.IsTrue(newObject.IsFrozen);

                try
                {
                    collection[0] = newObject;
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.AreSame(objectToAdd, collection[0]);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // setter: cannot add a disposed element
                Utils.DisposalAccessTest(newObject, delegate() { collection[0] = newObject; });
            }
        }

        public static void AddTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanInsert(objectToAdd, InsertCheckFlags.None));

                int count = collection.Count;

                try
                {
                    collection.Add(objectToAdd);
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                    Assert.AreEqual(count, collection.Count);
                    Assert.IsFalse(collection.Contains(objectToAdd));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(NotSupportedException), e.GetType());
                }
            }
            else
            {
                // basic function
                collection.Add(objectToAdd);
                Assert.AreEqual(1, collection.Count);
                Assert.AreSame(objectToAdd, collection[0]);

                // undo/redo
                UndoStack undoStack = new UndoStack();
                T newObject = (T)objectToAdd.Clone();

                if (DOMObjectType.Page == newObject.ObjectType)
                    ((IPage)newObject).Name += "_copy";

                undoStack.StartCommand("command");
                collection.Add(newObject);
                undoStack.EndCommand();
                Assert.AreEqual(2, collection.Count);
                Assert.IsTrue(collection.Contains(newObject));
                undoStack.Undo();
                Assert.AreEqual(1, collection.Count);
                Assert.IsFalse(collection.Contains(newObject));
                undoStack.Redo();
                Assert.AreEqual(2, collection.Count);
                Assert.IsTrue(collection.Contains(newObject));
                collection.Remove(newObject);

                // TODO: CanInsert checks

                // cannot add an object which is already a member
                try
                {
                    collection.Add(objectToAdd);
                    Assert.Fail();
                }
                catch (InvalidOperationException)
                {
                    Assert.AreEqual(1, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
                }

                // cannot add null values
                try
                {
                    collection.Add(null);
                    Assert.Fail();
                }
                catch (ArgumentNullException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.AreSame(objectToAdd, collection[0]);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentNullException), e.GetType());
                }

                // cannot add a frozen object
                newObject = (T)objectToAdd.Clone();
                newObject.Freeze();
                Assert.IsTrue(newObject.IsFrozen);

                try
                {
                    collection.Add(newObject);
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.AreSame(objectToAdd, collection[0]);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // cannot add a disposed element
                Utils.DisposalAccessTest(newObject, delegate() { collection.Add(newObject); });
            }
        }

        public static void InsertTest<T>(IDOMObjectCollection<T> collection, T objectToInsert) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanInsert(objectToInsert, InsertCheckFlags.None));

                int count = collection.Count;

                try
                {
                    collection.Insert(0, objectToInsert);
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                    Assert.AreEqual(count, collection.Count);
                    Assert.IsFalse(collection.Contains(objectToInsert));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(NotSupportedException), e.GetType());
                }
            }
            else
            {
                // basic function
                collection.Insert(0, objectToInsert);
                Assert.AreEqual(1, collection.Count);
                Assert.AreSame(objectToInsert, collection[0]);

                T newObject = (T)objectToInsert.Clone();

                if (DOMObjectType.Page == newObject.ObjectType)
                    ((IPage)newObject).Name += "_copy";

                // range-checks
                try
                {
                    collection.Insert(-1, newObject);
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsFalse(collection.Contains(newObject));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                try
                {
                    collection.Insert(10, newObject);
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(1, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                // undo/redo
                UndoStack undoStack = new UndoStack();
                undoStack.StartCommand("command");
                collection.Insert(0, newObject);
                undoStack.EndCommand();
                Assert.AreEqual(2, collection.Count);
                Assert.IsTrue(collection.Contains(newObject));
                undoStack.Undo();
                Assert.AreEqual(1, collection.Count);
                Assert.IsFalse(collection.Contains(newObject));
                undoStack.Redo();
                Assert.AreEqual(2, collection.Count);
                Assert.IsTrue(collection.Contains(newObject));
                collection.Remove(newObject);

                // TODO: CanInsert checks

                // cannot add an object which is already a member
                try
                {
                    collection.Insert(0, objectToInsert);
                    Assert.Fail();
                }
                catch (InvalidOperationException)
                {
                    Assert.AreEqual(1, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
                }

                // cannot add null values
                try
                {
                    collection.Insert(0, null);
                    Assert.Fail();
                }
                catch (ArgumentNullException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.AreSame(objectToInsert, collection[0]);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentNullException), e.GetType());
                }

                // cannot add a frozen object
                newObject.Freeze();
                Assert.IsTrue(newObject.IsFrozen);

                try
                {
                    collection.Insert(0, newObject);
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.AreSame(objectToInsert, collection[0]);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // setter: cannot add a disposed element
                Utils.DisposalAccessTest(newObject, delegate() { collection.Insert(0, newObject); });
            }
        }

        public static void RemoveTest<T>(IDOMObjectCollection<T> collection, T objectToRemove) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.RemoveTest() not implemented for read-only collections");
            }
            else
            {
                collection.Add(objectToRemove);
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains(objectToRemove));

                // basic function
                Assert.IsTrue(collection.Remove(objectToRemove));
                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(objectToRemove));

                // cannot remove an object not in the collection
                Assert.IsFalse(collection.Remove(objectToRemove));

                // cannot remove a null
                Assert.IsFalse(collection.Remove(null));

                // undo/redo
                UndoStack undoStack = new UndoStack();
                collection.Add(objectToRemove);
                undoStack.StartCommand("command");
                collection.Remove(objectToRemove);
                undoStack.EndCommand();
                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(objectToRemove));
                undoStack.Undo();
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains(objectToRemove));
                undoStack.Redo();
                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(objectToRemove));
            }
        }

        public static void RemoveAtTest<T>(IDOMObjectCollection<T> collection, T objectToRemove) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.RemoveAtTest() not implemented for read-only collections");
            }
            else
            {
                collection.Add(objectToRemove);
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains(objectToRemove));

                // range-checks
                try
                {
                    collection.RemoveAt(-1);
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsTrue(collection.Contains(objectToRemove));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                try
                {
                    collection.RemoveAt(10);
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsTrue(collection.Contains(objectToRemove));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                // basic function
                collection.RemoveAt(0);
                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(objectToRemove));

                // undo/redo
                UndoStack undoStack = new UndoStack();
                collection.Add(objectToRemove);
                undoStack.StartCommand("command");
                collection.RemoveAt(0);
                undoStack.EndCommand();
                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(objectToRemove));
                undoStack.Undo();
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains(objectToRemove));
                undoStack.Redo();
                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(objectToRemove));
            }
        }

        public static void ClearTest<T>(IDOMObjectCollection<T> collection, T objectToRemove) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.ClearTest() not implemented for read-only collections");
            }
            else
            {
                collection.Add(objectToRemove);
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains(objectToRemove));

                collection.Clear();
                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(objectToRemove));

                // undo/redo
                UndoStack undoStack = new UndoStack();
                collection.Add(objectToRemove);
                undoStack.StartCommand("command");
                collection.Clear();
                undoStack.EndCommand();
                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(objectToRemove));
                undoStack.Undo();
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains(objectToRemove));
                undoStack.Redo();
                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(objectToRemove));
            }
        }

        public static void CopyToTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.CopyToTest() not implemented for read-only collections");
            }
            else
            {
                T[] array;

                collection.Add(objectToAdd);

                array = new T[1];
                collection.CopyTo(array, 0);
                Assert.IsNotNull(array[0]);
                Assert.AreSame(objectToAdd, array[0]);

                // array cannot be null
                try
                {
                    collection.CopyTo(null, 0);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentNullException), e.GetType());
                }

                // range-checks
                try
                {
                    collection.CopyTo(array, -1);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                T newObject = (T)objectToAdd.Clone();

                if (DOMObjectType.Page == newObject.ObjectType)
                    ((IPage)newObject).Name += "_copy";

                collection.Add(newObject);

                try
                {
                    collection.CopyTo(array, 0);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentException), e.GetType());
                }

                array = new T[2];

                try
                {
                    collection.CopyTo(array, 1);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentException), e.GetType());
                }

                // basic function
                collection.CopyTo(array, 0);
            }
        }

        public static void GetEnumeratorTest<T>(IDOMObjectCollection<T> collection, T objectToAdd) where T : class, IDOMObject
        {
            if (collection.IsReadOnly)
            {
                throw new NotImplementedException("IDOMObjectCollectionTest.GetEnumeratorTest() not implemented for read-only collections");
            }
            else
            {
                int count = 0;
                T[] array = new T[10];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = (T)objectToAdd.Clone();

                    if (DOMObjectType.Page == array[i].ObjectType)
                        ((IPage)array[i]).Name += "_copy" + i;

                    collection.Add(array[i]);
                }

                foreach (T element in collection)
                {
                    Assert.AreSame(array[count], element);
                    count++;
                }

                Assert.AreEqual(collection.Count, count);
            }
        }

        #endregion Collection-management
    }
}
