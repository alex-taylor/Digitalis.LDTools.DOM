#region License

//
// IDocumentTest.cs
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
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.UndoSystem;

    using Configuration = Digitalis.LDTools.DOM.Configuration;

    #endregion Usings

    [TestClass]
    public abstract class IDocumentTest : IDOMObjectTest
    {
        #region Infrastructure

        protected sealed override Type InterfaceType { get { return typeof(IDocument); } }

        protected sealed override IDOMObject CreateTestObject()
        {
            return CreateTestDocument();
        }

        protected sealed override IDOMObject CreateTestObjectWithDocumentTree()
        {
            return CreateTestDocument();
        }

        protected sealed override IDOMObject CreateTestObjectWithFrozenAncestor()
        {
            return null;
        }

        protected abstract IDocument CreateTestDocument();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IDocument document = CreateTestDocument();
            Assert.AreEqual(DOMObjectType.Document, document.ObjectType);

            if (document.IsImmutable)
                Assert.IsTrue(document.IsReadOnly);

            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Change-notification

        [TestMethod]
        public void DocumentTreeChangedTest()
        {
            IDocument document        = CreateTestDocument();
            IPage page                = MocksFactory.CreateMockPage();
            bool treeEventSeen        = false;
            bool changedEventSeen     = false;
            bool originatingEventSeen = false;

            document.DocumentTreeChanged += delegate(IDocument sender, DocumentTreeChangedEventArgs e)
            {
                Assert.IsFalse(treeEventSeen);
                treeEventSeen = true;
                Assert.AreSame(document, sender);
                Assert.AreEqual(1, e.Count);

                ObjectChangedEventArgs args = e.Events.ElementAt(0);

                switch (args.Operation)
                {
                    case "ItemsAdded":
                        Assert.AreSame(document, args.Source);
                        Assert.IsInstanceOfType(args.Parameters, typeof(UndoableListChangedEventArgs<IPage>));

                        UndoableListChangedEventArgs<IPage> eventArgs = (UndoableListChangedEventArgs<IPage>)args.Parameters;
                        Assert.AreEqual(1, eventArgs.Count);
                        Assert.AreEqual(0, eventArgs.FirstIndex);
                        Assert.AreEqual(page, eventArgs.Items.ElementAt(0));
                        break;

                    case "NameChanged":
                        Assert.AreSame(page, args.Source);
                        break;

                    default:
                        Assert.Fail(args.Operation);
                        break;
                }
            };

            document.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.IsFalse(changedEventSeen);
                changedEventSeen = true;
                Assert.AreSame(document, sender);

                switch (e.Operation)
                {
                    case "ItemsAdded":
                        Assert.AreSame(document, e.Source);
                        Assert.IsInstanceOfType(e.Parameters, typeof(UndoableListChangedEventArgs<IPage>));

                        UndoableListChangedEventArgs<IPage> eventArgs = (UndoableListChangedEventArgs<IPage>)e.Parameters;
                        Assert.AreEqual(1, eventArgs.Count);
                        Assert.AreEqual(0, eventArgs.FirstIndex);
                        Assert.AreEqual(page, eventArgs.Items.ElementAt(0));
                        break;

                    case "NameChanged":
                        Assert.AreSame(page, e.Source);
                        break;

                    default:
                        Assert.Fail(e.Operation);
                        break;
                }
            };

            document.ItemsAdded += delegate(object sender, UndoableListChangedEventArgs<IPage> e)
            {
                Assert.IsFalse(originatingEventSeen);
                originatingEventSeen = true;
            };

            // once BeginUpdate() has been called, DocumentTreeChanged events should queue up until EndUpdate()
            // the originating event and Changed should be published, however
            document.BeginUpdate();
            document.Add(page);
            Assert.IsFalse(treeEventSeen);
            Assert.IsTrue(changedEventSeen);
            Assert.IsTrue(originatingEventSeen);
            document.EndUpdate();
            Assert.IsTrue(treeEventSeen);
            treeEventSeen = false;
            changedEventSeen = false;
            originatingEventSeen = false;

            // events from descendants should propagate up
            page.Name = "new name";
            Assert.IsTrue(treeEventSeen);
        }

        [TestMethod]
        public void UpdateBegunTest()
        {
            IDocument document = CreateTestDocument();
            bool eventSeen = false;

            document.UpdateBegun += delegate(object sender, EventArgs e)
            {
                eventSeen = true;
            };

            document.BeginUpdate();
            Assert.IsTrue(eventSeen);
        }

        [TestMethod]
        public void UpdateInProgressTest()
        {
            IDocument document = CreateTestDocument();
            bool eventSeen = false;

            document.UpdateInProgress += delegate(object sender, EventArgs e)
            {
                eventSeen = true;
            };

            // the event should not be seen unless wrapped in a BeginUpdate()/EndUpdate() pair
            document.Update();
            Assert.IsFalse(eventSeen);

            document.BeginUpdate();
            document.Update();
            document.EndUpdate();
            Assert.IsTrue(eventSeen);
        }

        [TestMethod]
        public void UpdateEndedTest()
        {
            IDocument document = new LDDocument();
            bool eventSeen = false;

            document.UpdateEnded += delegate(object sender, EventArgs e)
            {
                eventSeen = true;
            };

            // the event should not be output unless BeginUpdate() was called first
            try
            {
                document.EndUpdate();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            Assert.IsFalse(eventSeen);

            document.BeginUpdate();
            Assert.IsFalse(eventSeen);
            document.EndUpdate();
            Assert.IsTrue(eventSeen);
        }

        #endregion Change-notification

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            IDocument document = CreateTestDocument();
            IPage page         = MocksFactory.CreateMockPage();
            document.Add(page);
            document.Filepath = "filepath";
            return document;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            IDocument first  = (IDocument)original;
            IDocument second = (IDocument)copy;

            Assert.AreEqual(first.Filepath, second.Filepath);

            bool originalTreeEventSeen = false;
            bool copyTreeEventSeen     = false;

            original.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.IsFalse(originalTreeEventSeen);
                originalTreeEventSeen = true;
                Assert.AreSame(original, sender);
            };

            copy.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.IsFalse(copyTreeEventSeen);
                copyTreeEventSeen = true;
                Assert.AreSame(copy, sender);
            };

            Assert.AreEqual(first.Count, second.Count);

            for (int i = 0; i < first.Count; i++)
            {
                // basic check: can the new parent and child see each other?
                Assert.AreNotSame(first[i], second[i]);
                Assert.IsTrue(second.Contains(second[i]));
                Assert.AreSame(second, second[i].Document);

                // verify that the child is sending its Changed events to the correct collection
                originalTreeEventSeen    = false;
                copyTreeEventSeen        = false;
                first[i].InlineOnPublish = !first[i].InlineOnPublish;
                Assert.IsTrue(originalTreeEventSeen);
                Assert.IsFalse(copyTreeEventSeen);

                originalTreeEventSeen     = false;
                copyTreeEventSeen         = false;
                second[i].InlineOnPublish = !second[i].InlineOnPublish;
                Assert.IsFalse(originalTreeEventSeen);
                Assert.IsTrue(copyTreeEventSeen);
            }

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public override void ToCodeTest()
        {
            IDocument document = CreateTestDocument();
            StringBuilder code;
            StringBuilder expected;

            // an empty document cannot be converted to code
            try
            {
                document.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            IPage page    = MocksFactory.CreateMockPage();
            page.Name     = "page1";
            page.PageType = PageType.Part;
            document.Add(page);

            // a single-page document should be identical to the page
            expected = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            code = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), code.ToString());

            expected = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            code = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), code.ToString());

            expected = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            code = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), code.ToString());

            // a multi-page document should add '0 FILE' for each page
            IPage page2    = MocksFactory.CreateMockPage();
            page2.Name     = "page2";
            page2.PageType = PageType.Part;
            document.Add(page2);

            expected = new StringBuilder();
            expected.AppendFormat("0 FILE {0}\r\n", page.TargetName);
            page.ToCode(expected, CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal);
            expected.AppendFormat("0 FILE {0}\r\n", page2.TargetName);
            page2.ToCode(expected, CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal);
            expected = Utils.PreProcessCode(expected);
            code = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), code.ToString());

            expected = new StringBuilder();
            expected.AppendFormat("0 FILE {0}\r\n", page.TargetName);
            page.ToCode(expected, CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal);
            expected.AppendFormat("0 FILE {0}\r\n", page2.TargetName);
            page2.ToCode(expected, CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal);
            expected = Utils.PreProcessCode(expected);
            code = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), code.ToString());

            // multi-page documents are not allowed in PartsLibrary mode, so only the first page should be output
            expected = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            code = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), code.ToString());
        }

        #endregion Code-generation

        #region Collection-management

        [TestMethod]
        public void ItemsAddedTest()
        {
            IDocument document    = CreateTestDocument();
            IPage page            = MocksFactory.CreateMockPage();
            bool treeEventSeen    = false;
            bool changedEventSeen = false;

            document.DocumentTreeChanged += delegate(IDocument sender, DocumentTreeChangedEventArgs e)
            {
                if (0 != e.Count)
                {
                    ObjectChangedEventArgs args = e.Events.ElementAt(0);

                    if ("ItemsAdded" == args.Operation)
                    {
                        Assert.IsFalse(treeEventSeen);
                        treeEventSeen = true;
                        Assert.AreSame(document, sender);

                        Assert.AreSame(document, args.Source);
                        Assert.IsInstanceOfType(args.Parameters, typeof(UndoableListChangedEventArgs<IPage>));

                        UndoableListChangedEventArgs<IPage> eventArgs = (UndoableListChangedEventArgs<IPage>)args.Parameters;
                        Assert.AreEqual(1, eventArgs.Count);
                        Assert.AreSame(page, eventArgs.Items.ElementAt(0));
                    }
                }
            };

            document.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                if ("ItemsAdded" == e.Operation)
                {
                    Assert.IsFalse(changedEventSeen);
                    changedEventSeen = true;
                    Assert.AreSame(document, sender);
                    Assert.IsInstanceOfType(e.Parameters, typeof(UndoableListChangedEventArgs<IPage>));

                    UndoableListChangedEventArgs<IPage> eventArgs = (UndoableListChangedEventArgs<IPage>)e.Parameters;
                    Assert.AreEqual(1, eventArgs.Count);
                    Assert.AreSame(page, eventArgs.Items.ElementAt(0));
                }
            };

            IDOMObjectCollectionTest.ItemsAddedTest(document, page);
            Assert.IsTrue(treeEventSeen);
            Assert.IsTrue(changedEventSeen);
        }

        [TestMethod]
        public void ItemsRemovedTest()
        {
            IDocument document    = CreateTestDocument();
            IPage page            = MocksFactory.CreateMockPage();
            bool treeEventSeen    = false;
            bool changedEventSeen = false;

            document.DocumentTreeChanged += delegate(IDocument sender, DocumentTreeChangedEventArgs e)
            {
                if (0 != e.Count)
                {
                    ObjectChangedEventArgs args = e.Events.ElementAt(0);

                    if ("ItemsRemoved" == args.Operation)
                    {
                        Assert.IsFalse(treeEventSeen);
                        treeEventSeen = true;
                        Assert.AreSame(document, sender);
                        Assert.AreSame(document, args.Source);
                        Assert.IsInstanceOfType(args.Parameters, typeof(UndoableListChangedEventArgs<IPage>));

                        UndoableListChangedEventArgs<IPage> eventArgs = (UndoableListChangedEventArgs<IPage>)args.Parameters;
                        Assert.AreEqual(1, eventArgs.Count);
                        Assert.AreSame(page, eventArgs.Items.ElementAt(0));
                    }
                }
            };

            document.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                if ("ItemsRemoved" == e.Operation)
                {
                    Assert.IsFalse(changedEventSeen);
                    changedEventSeen = true;
                    Assert.AreSame(document, sender);
                    Assert.IsInstanceOfType(e.Parameters, typeof(UndoableListChangedEventArgs<IPage>));

                    UndoableListChangedEventArgs<IPage> eventArgs = (UndoableListChangedEventArgs<IPage>)e.Parameters;
                    Assert.AreEqual(1, eventArgs.Count);
                    Assert.AreSame(page, eventArgs.Items.ElementAt(0));
                }
            };

            IDOMObjectCollectionTest.ItemsRemovedTest(document, page);
            Assert.IsTrue(treeEventSeen);
            Assert.IsTrue(changedEventSeen);
        }

        [TestMethod]
        public void ItemsReplacedTest()
        {
            IDocument document    = CreateTestDocument();
            IPage page            = MocksFactory.CreateMockPage();
            bool treeEventSeen    = false;
            bool changedEventSeen = false;

            document.DocumentTreeChanged += delegate(IDocument sender, DocumentTreeChangedEventArgs e)
            {
                if (0 != e.Count)
                {
                    ObjectChangedEventArgs args = e.Events.ElementAt(0);

                    if ("ItemsReplaced" == args.Operation)
                    {
                        Assert.IsFalse(treeEventSeen);
                        treeEventSeen = true;
                        Assert.AreSame(document, sender);
                        Assert.AreSame(document, args.Source);
                        Assert.IsInstanceOfType(args.Parameters, typeof(UndoableListReplacedEventArgs<IPage>));

                        UndoableListReplacedEventArgs<IPage> eventArgs = (UndoableListReplacedEventArgs<IPage>)args.Parameters;

                        if (1 == eventArgs.ItemsAdded.Count)
                            Assert.AreSame(page, eventArgs.ItemsAdded.Items.ElementAt(0));
                        else
                            Assert.AreSame(page, eventArgs.ItemsRemoved.Items.ElementAt(0));
                    }
                }
            };

            document.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                if ("ItemsReplaced" == e.Operation)
                {
                    Assert.IsFalse(changedEventSeen);
                    changedEventSeen = true;
                    Assert.AreSame(document, sender);
                    Assert.IsInstanceOfType(e.Parameters, typeof(UndoableListReplacedEventArgs<IPage>));

                    UndoableListReplacedEventArgs<IPage> eventArgs = (UndoableListReplacedEventArgs<IPage>)e.Parameters;

                    if (1 == eventArgs.ItemsAdded.Count)
                        Assert.AreSame(page, eventArgs.ItemsAdded.Items.ElementAt(0));
                    else
                        Assert.AreSame(page, eventArgs.ItemsRemoved.Items.ElementAt(0));
                }
            };

            IDOMObjectCollectionTest.ItemsReplacedTest(document, page);
            Assert.IsTrue(treeEventSeen);
            Assert.IsTrue(changedEventSeen);
        }

        [TestMethod]
        public void CollectionClearedTest()
        {
            IDocument document    = CreateTestDocument();
            IPage page            = MocksFactory.CreateMockPage();
            bool treeEventSeen    = false;
            bool changedEventSeen = false;

            document.DocumentTreeChanged += delegate(IDocument sender, DocumentTreeChangedEventArgs e)
            {
                if (0 != e.Count)
                {
                    ObjectChangedEventArgs args = e.Events.ElementAt(0);

                    if ("CollectionCleared" == args.Operation)
                    {
                        Assert.IsFalse(treeEventSeen);
                        treeEventSeen = true;
                        Assert.AreSame(document, sender);
                        Assert.AreSame(document, args.Source);
                        Assert.IsInstanceOfType(args.Parameters, typeof(UndoableListChangedEventArgs<IPage>));

                        UndoableListChangedEventArgs<IPage> eventArgs = (UndoableListChangedEventArgs<IPage>)args.Parameters;
                        Assert.AreEqual(1, eventArgs.Count);
                        Assert.AreSame(page, eventArgs.Items.ElementAt(0));
                    }
                }
            };

            document.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                if ("CollectionCleared" == e.Operation)
                {
                    Assert.IsFalse(changedEventSeen);
                    changedEventSeen = true;
                    Assert.AreSame(document, sender);
                    Assert.IsInstanceOfType(e.Parameters, typeof(UndoableListChangedEventArgs<IPage>));

                    UndoableListChangedEventArgs<IPage> eventArgs = (UndoableListChangedEventArgs<IPage>)e.Parameters;
                    Assert.AreEqual(1, eventArgs.Count);
                    Assert.AreSame(page, eventArgs.Items.ElementAt(0));
                }
            };

            IDOMObjectCollectionTest.CollectionClearedTest(document, page);
            Assert.IsTrue(treeEventSeen);
            Assert.IsTrue(changedEventSeen);
        }

        [TestMethod]
        public void CanInsertTest()
        {
            IDOMObjectCollectionTest.CanInsertTest(CreateTestDocument(), CreateTestDocument(), MocksFactory.CreateMockPage());

            IDocument document = CreateTestDocument();

            CanInsertOrReplaceTest(document,
                                   MocksFactory.CreateMockPage(),
                                   delegate(IPage pageToInsert, IPage pageToReplace, InsertCheckFlags flags)
                                   { return document.CanInsert(pageToInsert, flags); });
        }

        [TestMethod]
        public void CanReplaceTest()
        {
            IDOMObjectCollectionTest.CanReplaceTest(CreateTestDocument(), CreateTestDocument(), MocksFactory.CreateMockPage(), MocksFactory.CreateMockPage());

            IDocument document = CreateTestDocument();

            CanInsertOrReplaceTest(document,
                                   MocksFactory.CreateMockPage(),
                                   delegate(IPage pageToInsert, IPage pageToReplace, InsertCheckFlags flags)
                                   { return document.CanReplace(pageToInsert, pageToReplace, flags); });

            document = CreateTestDocument();
            IPage page = MocksFactory.CreateMockPage();
            document.Add(page);

            // a page with a duplicate name can replace the original
            Assert.AreEqual(InsertCheckResult.CanInsert, document.CanReplace((IPage)page.Clone(), page, InsertCheckFlags.None));
        }

        private delegate InsertCheckResult CanInsertOrReplaceFunction(IPage pageToInsert, IPage pageToReplace, InsertCheckFlags flags);

        private static void CanInsertOrReplaceTest(IDocument document, IPage pageToCheck, CanInsertOrReplaceFunction function)
        {
            IPage pageToReplace = (IPage)pageToCheck.Clone();
            pageToReplace.Name += "_copy";

            if (document.IsImmutable || document.IsReadOnly)
            {
                Assert.AreEqual(InsertCheckResult.NotSupported, function(pageToCheck, pageToReplace, InsertCheckFlags.None));
            }
            else
            {
                document.Add(pageToReplace);

                // cannot add a page with the same name as an existing one
                Assert.AreEqual(InsertCheckResult.DuplicateName, function((IPage)pageToReplace.Clone(), null, InsertCheckFlags.None));

                CanInsertOrReplaceCircularRefTest(document, pageToCheck, function);

                // cannot add to a frozen document
                document.Freeze();
                Assert.IsTrue(document.IsFrozen);
                Assert.AreEqual(InsertCheckResult.NotSupported, function(pageToCheck, pageToReplace, InsertCheckFlags.None));
            }
        }

        private static void CanInsertOrReplaceCircularRefTest(IDocument document, IPage pageToCheck, CanInsertOrReplaceFunction function)
        {
            IPage pageToReplace = (IPage)pageToCheck.Clone();
            pageToReplace.Name += "_copy1";
            document.Add(pageToReplace);

            IStep step = MocksFactory.CreateMockStep();
            pageToReplace.Add(step);

            IReference reference = MocksFactory.CreateMockReference();
            reference.TargetName = pageToCheck.TargetName;
            step.Add(reference);

            step = MocksFactory.CreateMockStep();
            pageToCheck.Add(step);
            reference = MocksFactory.CreateMockReference();
            reference.TargetName = pageToReplace.TargetName;
            step.Add(reference);

            Assert.AreEqual(InsertCheckResult.CircularReference, function(pageToCheck, pageToReplace, InsertCheckFlags.None));

            step.Clear();
            reference = MocksFactory.CreateMockReference();
            reference.TargetName = "3001.dat";
            step.Add(reference);
            Assert.AreEqual(InsertCheckResult.CanInsert, function(pageToCheck, pageToReplace, InsertCheckFlags.None));
        }

        [TestMethod]
        public void CountTest()
        {
            IDOMObjectCollectionTest.CountTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void IndexOfTest()
        {
            IDOMObjectCollectionTest.IndexOfTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void ContainsTest()
        {
            IDOMObjectCollectionTest.ContainsTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void IndexerTest()
        {
            IDOMObjectCollectionTest.IndexerTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void AddTest()
        {
            IDOMObjectCollectionTest.AddTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void InsertTest()
        {
            IDOMObjectCollectionTest.InsertTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void RemoveTest()
        {
            IDOMObjectCollectionTest.RemoveTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void RemoveAtTest()
        {
            IDOMObjectCollectionTest.RemoveAtTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void ClearTest()
        {
            IDOMObjectCollectionTest.ClearTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void CopyToTest()
        {
            IDOMObjectCollectionTest.CopyToTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            IDOMObjectCollectionTest.GetEnumeratorTest(CreateTestDocument(), MocksFactory.CreateMockPage());
        }

        [TestMethod]
        public void IndexerTest2()
        {
            IDocument document = CreateTestDocument();
            IPage page = MocksFactory.CreateMockPage();
            document.Add(page);
            Assert.AreSame(page, document[page.TargetName]);

            Utils.DisposalAccessTest(document, delegate() { page = document[page.TargetName]; });
        }

        #endregion Collection-management

        #region Disposal

        [TestMethod]
        public override void DisposeTest()
        {
            IDocument document = CreateTestDocument();

            if (!document.IsImmutable && !document.IsReadOnly)
            {
                IPage page = MocksFactory.CreateMockPage();
                document.Add(page);

                IStep step = MocksFactory.CreateMockStep();
                page.Add(step);

                ILine line = MocksFactory.CreateMockLine();
                step.Add(line);

                document.Dispose();
                Assert.IsTrue(document.IsDisposed);
                Assert.IsTrue(page.IsDisposed);
                Assert.IsTrue(step.IsDisposed);
                Assert.IsTrue(line.IsDisposed);
            }

            base.DisposeTest();
        }

        #endregion Disposal

        #region Import and Export

        [TestMethod]
        public void ImportTest()
        {
            IDocument document = CreateTestDocument();
            IDocument import   = CreateTestDocument();

            IPage page = MocksFactory.CreateMockPage();
            import.Add(page);

            // the page should be transferred from 'import' to 'document'
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            document.Import(import);
            undoStack.EndCommand();
            Assert.AreEqual(0, import.Count);
            Assert.AreEqual(1, document.Count);
            Assert.IsFalse(import.Contains(page));
            Assert.IsTrue(document.Contains(page));

            undoStack.Undo();
            Assert.AreEqual(1, import.Count);
            Assert.AreEqual(0, document.Count);
            Assert.IsTrue(import.Contains(page));
            Assert.IsFalse(document.Contains(page));

            undoStack.Redo();
            Assert.AreEqual(0, import.Count);
            Assert.AreEqual(1, document.Count);
            Assert.IsFalse(import.Contains(page));
            Assert.IsTrue(document.Contains(page));

            // cannot import a page with a duplicate TargetName
            page          = MocksFactory.CreateMockPage();
            page.Name     = document[0].Name;
            page.PageType = document[0].PageType;
            import.Add(page);

            try
            {
                document.Import(import);
                Assert.Fail();
            }
            catch (DuplicateNameException)
            {
                Assert.IsTrue(import.Contains(page));
                Assert.IsFalse(document.Contains(page));
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(DuplicateNameException), e.GetType());
            }

            // cannot import from a frozen document
            page.Name += "_copy";
            import.Freeze();

            try
            {
                document.Import(import);
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.IsTrue(import.Contains(page));
                Assert.IsFalse(document.Contains(page));
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            // cannot import to a frozen document
            import = CreateTestDocument();
            page = MocksFactory.CreateMockPage();
            page.Name += "_copy";
            import.Add(page);
            document.Freeze();

            try
            {
                document.Import(import);
                Assert.Fail();
            }
            catch (ObjectFrozenException)
            {
                Assert.IsTrue(import.Contains(page));
                Assert.IsFalse(document.Contains(page));
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            // TODO: cannot import from an immutable/read-only document

            // TODO: cannot import to an immutable/read-only document

            Utils.DisposalAccessTest(document, delegate() { document.Import(import); });
        }

        [TestMethod]
        public void SaveTest()
        {
            IDocument document = CreateTestDocument();

            // TODO: test for Save()

            Utils.DisposalAccessTest(document, delegate() { document.Save(); });
        }

        [TestMethod]
        public void SaveTest2()
        {
            IDocument document = CreateTestDocument();
            StringBuilder code;

            SaveTest(document, delegate()
            {
                using (TextWriter writer = new StringWriter())
                {
                    document.Save(writer);
                    writer.Flush();
                    code = Utils.PreProcessCode(new StringBuilder(writer.ToString()));
                    return code.ToString();
                }
            });

            Utils.DisposalAccessTest(document, delegate() { document.Save(new StringWriter()); });
        }

        private delegate string SaveCallback();

        private void SaveTest(IDocument document, SaveCallback callback)
        {
            StringBuilder expected;

            // empty document
            try
            {
                callback();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            // single-page document
            IPage page1 = MocksFactory.CreateMockPage();
            page1.Name = "page1";
            document.Add(page1);
            expected = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), callback());

            // multi-page document
            IPage page2 = MocksFactory.CreateMockPage();
            page2.Name = "page2";
            document.Add(page2);
            expected = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), callback());
        }

        [TestMethod]
        public void PublishTest()
        {
            IDocument document = CreateTestDocument();

            PublishTest(document, delegate()
            {
                List<StringWriter> writers = new List<StringWriter>(document.Count);
                int idx = 0;

                document.Publish(delegate(string targetName)
                {
                    string path  = null;
                    int numPages = 0;

                    foreach (IPage page in document)
                    {
                        if (!page.InlineOnPublish)
                        {
                            numPages++;

                            if (null == path)
                                path = page.Name;
                        }
                    }

                    if (PageType.Model == document.DocumentType)
                    {
                        Assert.AreEqual(path + ((numPages > 1) ? ".mpd" : ".ldr"), targetName);
                    }
                    else
                    {
                        while (idx < document.Count && document[idx].InlineOnPublish)
                        {
                            idx++;
                        }

                        Assert.AreEqual(document[idx++].TargetName, targetName);
                    }

                    StringWriter writer = new StringWriter();
                    writers.Add(writer);
                    return writer;
                });

                string[] results = new string[writers.Count()];
                StringBuilder code;

                idx = 0;

                foreach (StringWriter writer in writers)
                {
                    writer.Flush();
                    code = Utils.PreProcessCode(new StringBuilder(writer.ToString()));
                    writer.Dispose();
                    results[idx++] = code.ToString();
                }

                return results;
            });

            Utils.DisposalAccessTest(document, delegate() { document.Publish((DocumentWriterCallback)null); });
        }

        [TestMethod]
        public void PublishTest2()
        {
            IDocument document = CreateTestDocument();

            // TODO: Publish(string folderPath)

            Utils.DisposalAccessTest(document, delegate() { document.Publish(""); });
        }

        private void PublishTest(IDocument document, ExportCallback callback)
        {
            StringBuilder expected;
            string[] results;

            // empty document
            try
            {
                callback();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            // single-page document
            IPage page1    = MocksFactory.CreateMockPage();
            page1.Name     = "page1";
            page1.PageType = PageType.Model;
            IStep step1    = new LDStep();
            page1.Add(step1);
            step1.Add(new LDComment("0 // comment"));
            document.Add(page1);
            Assert.AreEqual(PageType.Model, document.DocumentType);
            expected = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            results = callback();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(expected.ToString(), results[0]);

            // multi-page Model document with no inlining
            IPage page2 = MocksFactory.CreateMockPage();
            page2.Name = "page2";
            page2.PageType = PageType.Part;
            IStep step2 = new LDStep();
            page2.Add(step2);
            step2.Add(new LDLine("2 24 1 2 3 4 5 6"));
            document.Add(page2);
            Assert.AreEqual(PageType.Model, document.DocumentType);
            expected = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            results = callback();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(expected.ToString(), results[0]);

            // multi-page Model document with inlined page and unreferenced+inlined page
            step1.Add(new LDReference("1 16 0 0 0 2 0 0 0 4 0 0 0 6 " + page2.TargetName, false));
            page2.InlineOnPublish = true;
            IPage page3 = MocksFactory.CreateMockPage();
            page3.Name = "page3";
            page3.PageType = PageType.Subpart;
            page3.InlineOnPublish = true;
            IStep step3 = new LDStep();
            page3.Add(step3);
            step3.Add(new LDTriangle());
            document.Add(page3);
            Assert.AreEqual(PageType.Model, document.DocumentType);
            expected = Utils.PreProcessCode(page1.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            results = callback();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(expected.ToString(), results[0]);

            // multi-page Model document with first page inlined
            page2.InlineOnPublish = false;
            page1.InlineOnPublish = true;
            expected = Utils.PreProcessCode(page2.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            results = callback();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(expected.ToString(), results[0]);

            page1.InlineOnPublish = false;
            document.Clear();

            // single-page Part document
            page1.PageType = PageType.Part;
            document.Add(page1);
            expected = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            results = callback();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(expected.ToString(), results[0]);

            // multi-page Part document with no inlining
            document.Add(page2);
            results = callback();
            Assert.AreEqual(2, results.Length);
            expected = Utils.PreProcessCode(page1.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), results[0]);
            expected = Utils.PreProcessCode(page2.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), results[1]);

            // multi-page Part document with inlined page and unreferenced+inlined page
            page2.InlineOnPublish = true;
            document.Add(page3);
            results = callback();
            Assert.AreEqual(1, results.Length);
            expected = Utils.PreProcessCode(page1.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), results[0]);

            // multi-page Part document with first page inlined
            page2.InlineOnPublish = false;
            page1.InlineOnPublish = true;
            results = callback();
            Assert.AreEqual(1, results.Length);
            expected = Utils.PreProcessCode(page2.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(expected.ToString(), results[0]);

            // multi-page document with all pages marked as 'inline'
            page1.InlineOnPublish = true;
            page2.InlineOnPublish = true;
            page3.InlineOnPublish = true;

            try
            {
                callback();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }
        }

        [TestMethod]
        public void ExportTest()
        {
            IDocument document = CreateTestDocument();

            ExportTest(document, delegate()
            {
                List<StringWriter> writers = new List<StringWriter>(document.Count);
                string[] results = new string[document.Count];
                StringBuilder code;
                int idx = 0;

                document.Export(delegate(string targetName)
                {
                    Assert.AreEqual(document[idx++].TargetName, targetName);
                    StringWriter writer = new StringWriter();
                    writers.Add(writer);
                    return writer;
                });

                idx = 0;

                foreach (StringWriter writer in writers)
                {
                    writer.Flush();
                    code = Utils.PreProcessCode(new StringBuilder(writer.ToString()));
                    writer.Dispose();
                    results[idx++] = code.ToString();
                }

                return results;
            });

            Utils.DisposalAccessTest(document, delegate() { document.Export((DocumentWriterCallback)null); });
        }

        [TestMethod]
        public void ExportTest2()
        {
            IDocument document = CreateTestDocument();

            // TODO: Export(string folderPath)

            Utils.DisposalAccessTest(document, delegate() { document.Export(""); });
        }

        private delegate string[] ExportCallback();

        private void ExportTest(IDocument document, ExportCallback callback)
        {
            StringBuilder expected;
            string[] results;
            int idx = 0;

            // empty document
            try
            {
                callback();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            // single-page document
            IPage page1    = MocksFactory.CreateMockPage();
            page1.Name     = "page1";
            page1.PageType = PageType.Model;
            IStep step1    = new LDStep();
            page1.Add(step1);
            step1.Add(new LDComment("0 // comment"));
            document.Add(page1);
            expected = Utils.PreProcessCode(document.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            results = callback();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(expected.ToString(), results[0]);

            // multi-page document
            IPage page2    = MocksFactory.CreateMockPage();
            page2.Name     = "page2";
            page2.PageType = PageType.Model;
            IStep step2    = new LDStep();
            page2.Add(step2);
            step2.Add(new LDReference("1 16 0 0 0 1 0 0 0 1 0 0 0 1 3001.dat", false));
            document.Add(page2);

            results = callback();
            Assert.AreEqual(document.Count, results.Length);

            foreach (IPage page in document)
            {
                expected = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual(expected.ToString(), results[idx++]);
            }
        }

        #endregion Import and Export

        #region Properties

        [TestMethod]
        public void FilepathTest()
        {
            IDocument document = CreateTestDocument();

            // default value
            Assert.AreEqual("Untitled", document.Filepath);

            // set/get
            document.Filepath = "foo";
            Assert.AreEqual("foo", document.Filepath);

            // only filesystem-legal characters may be used
            try
            {
                document.Filepath = "<foo>";
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentException), e.GetType());
            }

            // TODO: Filepath can be set on an immutable IDocument

            // Filepath can be set on a frozen IDocument
            document.Freeze();
            document.Filepath = "filepath";

            Utils.DisposalAccessTest(document, delegate() { string filepath = document.Filepath; });

            document = CreateTestDocument();
            Utils.DisposalAccessTest(document, delegate() { document.Filepath = "filepath"; });
        }

        [TestMethod]
        public void FilepathChangedTest()
        {
            IDocument document = CreateTestDocument();
            bool eventSeen = false;

            document.FilepathChanged += delegate(object sender, EventArgs e)
            {
                eventSeen = true;
            };

            document.Filepath = "foo";
            Assert.IsTrue(eventSeen);
        }

        [TestMethod]
        public void IsLibraryPartTest()
        {
            IDocument document = CreateTestDocument();
            IPage page = MocksFactory.CreateMockPage();
            document.Add(page);

            Assert.IsFalse(document.IsLibraryPart);

            foreach (string path in Configuration.FullSearchPath)
            {
                // put it into the library
                document.Filepath = Path.Combine(path, "test.dat");

                foreach (PageType type in Enum.GetValues(typeof(PageType)))
                {
                    page.PageType = type;

                    if (PageType.Model == type)
                        Assert.IsFalse(document.IsLibraryPart);
                    else
                        Assert.IsTrue(document.IsLibraryPart);
                }
            }

            Utils.DisposalAccessTest(document, delegate() { bool isLibraryPart = document.IsLibraryPart; });
        }

        [TestMethod]
        public void StatusTest()
        {
            IDocument document = CreateTestDocument();

            // empty document
            Assert.AreEqual(DocumentStatus.Private, document.Status);

            // non-library document
            document.Add(MocksFactory.CreateMockPage());
            Assert.AreEqual(DocumentStatus.Private, document.Status);

            // TODO: fix these - API test cannot access the implementation-layer!
            // library Model
            document = new LDDocument(Path.Combine(Configuration.LDrawBase, "models", "Car.ldr"), ParseFlags.None);
            Assert.AreEqual(DocumentStatus.Private, document.Status);

            // library Part
            document = new LDDocument(Path.Combine(Configuration.LDrawBase, "parts", "3001.dat"), ParseFlags.None);
            Assert.AreEqual(DocumentStatus.Released, document.Status);


            Utils.DisposalAccessTest(document, delegate() { DocumentStatus status = document.Status; });
        }

        [TestMethod]
        public void DocumentTypeTest()
        {
            IDocument document = CreateTestDocument();
            IPage page         = MocksFactory.CreateMockPage();
            page.PageType      = PageType.Model;
            page.Name          = "name";
            page.Title         = "title";

            document.Add(page);
            Assert.AreEqual(PageType.Model, document.DocumentType);

            page.PageType = PageType.Part_Alias;
            Assert.AreEqual(PageType.Part_Alias, document.DocumentType);

            // if it has at least one 'Model' page then it's a Model
            page          = MocksFactory.CreateMockPage();
            page.PageType = PageType.Model;
            page.Name     = "model";
            page.Title    = "model";
            document.Add(page);
            Assert.AreEqual(PageType.Model, document.DocumentType);

            document.Clear();

            try
            {
                PageType type = document.DocumentType;
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(InvalidOperationException), e.GetType());
            }

            Utils.DisposalAccessTest(document, delegate() { PageType type = document.DocumentType; });
        }

        #endregion Properties
    }
}
