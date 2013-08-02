#region License
//
// LibraryManagerTest.cs
//
// Copyright (C) 2009-2012 Alex Taylor.  All Rights Reserved.
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
    using System.Reflection;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.Library;

    #endregion Usings

    [TestClass]
    public sealed class LibraryManagerTest
    {
        #region Infrastructure

        private const int EventTimeout = 1000;

        private static string ldrawBase;
        public const string MinimalLibraryBase = @"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\LDraw";

        private static void SetupMinimalLibrary()
        {
            if (Directory.Exists(MinimalLibraryBase))
                Directory.Delete(MinimalLibraryBase, true);

            Directory.CreateDirectory(MinimalLibraryBase);
            Directory.CreateDirectory(MinimalLibraryBase + @"\parts");
            Directory.CreateDirectory(MinimalLibraryBase + @"\parts\s");
            Directory.CreateDirectory(MinimalLibraryBase + @"\p");
            Directory.CreateDirectory(MinimalLibraryBase + @"\p\48");
            File.Copy(Configuration.LDConfigPath, MinimalLibraryBase + @"\ldconfig.ldr");
            File.Copy(Configuration.LDrawBase + @"\parts\1.dat", MinimalLibraryBase + @"\parts\1.dat");
            File.Copy(Configuration.LDrawBase + @"\parts\2.dat", MinimalLibraryBase + @"\parts\2.dat");
            File.Copy(Configuration.LDrawBase + @"\parts\3.dat", MinimalLibraryBase + @"\parts\3.dat");
            File.Copy(Configuration.LDrawBase + @"\parts\4.dat", MinimalLibraryBase + @"\parts\4.dat");
            File.Copy(Configuration.LDrawBase + @"\parts\5.dat", MinimalLibraryBase + @"\parts\5.dat");
            File.Copy(Configuration.LDrawBase + @"\parts\s\10s01.dat", MinimalLibraryBase + @"\parts\s\10s01.dat");
            File.Copy(Configuration.LDrawBase + @"\parts\s\14s01.dat", MinimalLibraryBase + @"\parts\s\14s01.dat");
            File.Copy(Configuration.LDrawBase + @"\parts\s\32as01.dat", MinimalLibraryBase + @"\parts\s\32as01.dat");
            File.Copy(Configuration.LDrawBase + @"\parts\s\32cs01.dat", MinimalLibraryBase + @"\parts\s\32cs01.dat");
            File.Copy(Configuration.LDrawBase + @"\parts\s\43ps1a.dat", MinimalLibraryBase + @"\parts\s\43ps1a.dat");
            File.Copy(Configuration.LDrawBase + @"\p\1-4ccyli.dat", MinimalLibraryBase + @"\p\1-4ccyli.dat");
            File.Copy(Configuration.LDrawBase + @"\p\1-4chrd.dat", MinimalLibraryBase + @"\p\1-4chrd.dat");
            File.Copy(Configuration.LDrawBase + @"\p\1-4con0.dat", MinimalLibraryBase + @"\p\1-4con0.dat");
            File.Copy(Configuration.LDrawBase + @"\p\1-4con1.dat", MinimalLibraryBase + @"\p\1-4con1.dat");
            File.Copy(Configuration.LDrawBase + @"\p\1-4con2.dat", MinimalLibraryBase + @"\p\1-4con2.dat");
            File.Copy(Configuration.LDrawBase + @"\p\48\1-3chrd.dat", MinimalLibraryBase + @"\p\48\1-3chrd.dat");
            File.Copy(Configuration.LDrawBase + @"\p\48\1-3cyli.dat", MinimalLibraryBase + @"\p\48\1-3cyli.dat");
            File.Copy(Configuration.LDrawBase + @"\p\48\1-3edge.dat", MinimalLibraryBase + @"\p\48\1-3edge.dat");
            File.Copy(Configuration.LDrawBase + @"\p\48\1-3ndis.dat", MinimalLibraryBase + @"\p\48\1-3ndis.dat");
            File.Copy(Configuration.LDrawBase + @"\p\48\1-3rin17.dat", MinimalLibraryBase + @"\p\48\1-3rin17.dat");

            ldrawBase = Configuration.LDrawBase;
            Configuration.LDrawBase = MinimalLibraryBase;
        }

        private static void TeardownMinimalLibrary()
        {
            LibraryManager.Unload();
            Configuration.LDrawBase = ldrawBase;

            if (Directory.Exists(MinimalLibraryBase))
                Directory.Delete(MinimalLibraryBase, true);
        }

        private LibraryManager Load()
        {
            if (null == LibraryManager.Cache)
                LibraryManager.Load(delegate(int progress, string status, string filename) { return true; }, false);

            return LibraryManager.Cache;
        }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public void DefinitionTest()
        {
            Assert.IsTrue(typeof(LibraryManager).IsSealed);
            Assert.IsTrue(typeof(LibraryManager).IsSerializable);
        }

        #endregion Definition Test

        #region API

        // TODO: GetEnumeratorTest() is unreliable
        //[TestMethod]
        public void GetEnumeratorTest()
        {
            LibraryManager cache = Load();
            string srcPath       = Path.Combine(Configuration.LDrawBase, "parts", "3001.dat");
            string dstPath       = Path.Combine(Configuration.LDrawBase, "My Parts", "3001test.dat");
            int count            = 0;
            int initialCount     = cache.Count;

            if (File.Exists(dstPath))
                File.Delete(dstPath);

            try
            {
                AutoResetEvent ev = new AutoResetEvent(false);

                LibraryManager.Changed += delegate(object sender, LibraryChangedEventArgs e)
                {
                    // the event should not be seen until the iterator has completed
                    Assert.AreEqual(initialCount, count);
                    ev.Set();
                };

                // verify that a change to the cache mid-iteration will not cause an exception, and will not be picked up by the iterator
                foreach (IndexCard card in cache)
                {
                    if (1000 == count)
                        File.Copy(srcPath, dstPath, true);

                    count++;
                }

                Assert.IsTrue(ev.WaitOne(5000));
                Assert.AreEqual(initialCount, count);
                Assert.AreEqual(cache.Count, count + 1);
            }
            finally
            {
                if (File.Exists(dstPath))
                    File.Delete(dstPath);
            }
        }

        // TODO: OverridesTest() is unreliable
        //[TestMethod]
        public void OverridesTest()
        {
            string srcPath = Path.Combine(Configuration.LDrawBase, "parts", "3001.dat");
            string dstPath = Path.Combine(Configuration.LDrawBase, "My Parts", "3001.dat");
            string midPath = Path.Combine(Configuration.LDrawBase, "unofficial", "parts", "3001.dat");
            string renPath = Path.Combine(Configuration.LDrawBase, "My Parts", "3001a.dat");

            if (File.Exists(dstPath))
                File.Delete(dstPath);

            if (File.Exists(midPath))
                File.Delete(midPath);

            if (File.Exists(renPath))
                File.Delete(renPath);

            try
            {
                AutoResetEvent ev = new AutoResetEvent(false);
                List<LibraryChangedEventArgs> eventArgs = new List<LibraryChangedEventArgs>();

                File.Copy(srcPath, dstPath, true);

                LibraryManager.Changed += delegate(object sender, LibraryChangedEventArgs e)
                {
                    eventArgs.Add(e);
                    ev.Set();
                };

                LibraryManager.Unload();
                LibraryManager cache = Load();
                IndexCard card = cache["3001.dat"];

                Assert.IsNotNull(card);
                Assert.AreEqual(dstPath, card.Filepath);
                Assert.AreEqual(0, card.Rank);

                // placing a new file inbetween these two in the search-order should not generate an event
                File.Copy(srcPath, midPath, true);
                Assert.IsFalse(ev.WaitOne(EventTimeout));
                Assert.AreEqual(0, eventArgs.Count);
                card = cache["3001.dat"];
                Assert.IsNotNull(card);
                Assert.AreEqual(dstPath, card.Filepath);
                Assert.AreEqual(0, card.Rank);

                // similarly, removing it again should do nothing
                File.Delete(midPath);
                Assert.IsFalse(ev.WaitOne(EventTimeout));
                Assert.AreEqual(0, eventArgs.Count);
                card = cache["3001.dat"];
                Assert.IsNotNull(card);
                Assert.AreEqual(dstPath, card.Filepath);
                Assert.AreEqual(0, card.Rank);

                // removing the top-level file should trigger the event
                File.Delete(dstPath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(1, eventArgs.Count);
                Assert.IsNull(eventArgs[0].Added);
                Assert.IsNull(eventArgs[0].Removed);
                Assert.IsNotNull(eventArgs[0].Modified);
                Assert.AreEqual(1, eventArgs[0].Modified.Count());
                Assert.AreEqual("3001.dat", eventArgs[0].Modified.ElementAt(0));
                eventArgs.Clear();
                card = cache["3001.dat"];
                Assert.IsNotNull(card);
                Assert.AreEqual(srcPath, card.Filepath);
                Assert.AreEqual(7, card.Rank);

                // as should reinstating it
                File.Copy(srcPath, dstPath, true);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(1, eventArgs.Count);
                Assert.IsNull(eventArgs[0].Added);
                Assert.IsNull(eventArgs[0].Removed);
                Assert.IsNotNull(eventArgs[0].Modified);
                Assert.AreEqual(1, eventArgs[0].Modified.Count());
                Assert.AreEqual("3001.dat", eventArgs[0].Modified.ElementAt(0));
                eventArgs.Clear();
                card = cache["3001.dat"];
                Assert.IsNotNull(card);
                Assert.AreEqual(dstPath, card.Filepath);
                Assert.AreEqual(0, card.Rank);

                // renaming it should trigger
                File.Move(dstPath, renPath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(1, eventArgs.Count);
                Assert.IsNull(eventArgs[0].Removed);
                Assert.IsNotNull(eventArgs[0].Modified);
                Assert.IsNotNull(eventArgs[0].Added);
                Assert.AreEqual(1, eventArgs[0].Added.Count());
                Assert.AreEqual("3001a.dat", eventArgs[0].Added.ElementAt(0));
                Assert.AreEqual(1, eventArgs[0].Modified.Count());
                Assert.AreEqual("3001.dat", eventArgs[0].Modified.ElementAt(0));
                eventArgs.Clear();
                card = cache["3001.dat"];
                Assert.IsNotNull(card);
                Assert.AreEqual(srcPath, card.Filepath);
                Assert.AreEqual(7, card.Rank);
                Assert.IsNotNull(cache["3001a.dat"]);

                // and back again
                File.Move(renPath, dstPath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(1, eventArgs.Count);
                Assert.IsNotNull(eventArgs[0].Removed);
                Assert.IsNotNull(eventArgs[0].Modified);
                Assert.IsNull(eventArgs[0].Added);
                Assert.AreEqual(1, eventArgs[0].Removed.Count());
                Assert.AreEqual("3001a.dat", eventArgs[0].Removed.ElementAt(0));
                Assert.AreEqual(1, eventArgs[0].Modified.Count());
                Assert.AreEqual("3001.dat", eventArgs[0].Modified.ElementAt(0));
                eventArgs.Clear();
                card = cache["3001.dat"];
                Assert.IsNotNull(card);
                Assert.AreEqual(dstPath, card.Filepath);
                Assert.AreEqual(0, card.Rank);
                Assert.IsNull(cache["3001a.dat"]);
            }
            finally
            {
                // give other processes a chance to release their file-locks
                Thread.Sleep(EventTimeout);

                if (File.Exists(dstPath))
                    File.Delete(dstPath);

                if (File.Exists(midPath))
                    File.Delete(midPath);

                if (File.Exists(renPath))
                    File.Delete(renPath);
            }
        }

        //TODO: ChangedTest() is unreliable
        //[TestMethod]
        public void ChangedTest()
        {
            SetupMinimalLibrary();

            AutoResetEvent ev = new AutoResetEvent(false);
            List<LibraryChangedEventArgs> eventArgs = new List<LibraryChangedEventArgs>();
            LibraryChangedEventArgs args;

            LibraryManager.Changed += delegate(object sender, LibraryChangedEventArgs e)
            {
                lock (eventArgs)
                {
                    if (null == ev)
                        return;

                    eventArgs.Add(e);
                    ev.Set();
                }
            };

            LibraryManager cache = Load();
            IndexCard card;
            int count                       = cache.Count;
            const string libraryTestFileSrc = @"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.LibraryManager.Newfile.dat";
            const string libraryTestFileDst = @"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\foo.dat";
            const string junkPath           = @"E:\junk.dat";
            string filePath                 = Path.Combine(Configuration.LDrawBase, @"parts\foo.dat");
            string newFilePath              = Path.Combine(Configuration.LDrawBase, @"parts\foobar.dat");
            string folderPath               = Path.Combine(Configuration.LDrawBase, "parts");
            string newFolderPath            = Path.Combine(Configuration.LDrawBase, "temp");

            if (File.Exists(filePath))
                File.Delete(filePath);

            if (File.Exists(newFilePath))
                File.Delete(newFilePath);

            if (File.Exists(junkPath))
                File.Delete(junkPath);

            try
            {
                // files
                File.Copy(libraryTestFileSrc, libraryTestFileDst, true);

                // 1. file added
                ev.Reset();
                File.Move(libraryTestFileDst, filePath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                lock (eventArgs)
                {
                    Assert.AreEqual(1, eventArgs.Count);
                    args = eventArgs[0];
                    eventArgs.Clear();
                }
                Assert.IsNotNull(args.Added);
                Assert.AreEqual(1, args.Added.Count());
                Assert.IsNull(args.Removed);
                Assert.IsNull(args.Modified);
                Assert.AreEqual("foo.dat", args.Added.ElementAt(0));
                Assert.AreEqual(count + 1, cache.Count);
                card = cache["foo.dat"];
                Assert.IsNotNull(card);
                Assert.AreEqual("foo.dat", card.TargetName);
                Assert.AreEqual(PageType.Part, card.Type);
                Assert.AreEqual("Tile  2 x  4 with Groove with Japanese \"Dragon God\" Pattern", card.Title);
                Assert.AreEqual(filePath, card.Filepath);

                // 2. file modified
                ev.Reset();
                using (TextWriter writer = File.CreateText(filePath))
                {
                    writer.Write("0 title\r\n0 foo.dat\r\n0 !LDRAW_ORG Shortcut\r\n0 comment\r\n");
                }
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                lock (eventArgs)
                {
                    //Assert.AreEqual(1, eventArgs.Count);
                    args = eventArgs[0];
                    eventArgs.Clear();
                }
                Assert.IsNull(args.Added);
                Assert.IsNull(args.Removed);
                Assert.IsNotNull(args.Modified);
                Assert.AreEqual(1, args.Modified.Count());
                Assert.AreEqual("foo.dat", args.Modified.ElementAt(0));
                Assert.AreNotEqual(count, cache.Count);
                card = cache["foo.dat"];
                Assert.IsNotNull(card);
                Assert.AreEqual("foo.dat", card.TargetName);
                Assert.AreEqual(PageType.Shortcut, card.Type);
                Assert.AreEqual("title", card.Title);
                Assert.AreEqual(filePath, card.Filepath);

                // 3. file renamed
                ev.Reset();
                File.Move(filePath, newFilePath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                lock (eventArgs)
                {
                    Assert.AreEqual(1, eventArgs.Count);
                    args = eventArgs[0];
                    eventArgs.Clear();
                }
                Assert.IsNotNull(args.Added);
                Assert.IsNotNull(args.Removed);
                Assert.IsNull(args.Modified);
                Assert.AreNotEqual(count, cache.Count);
                card = cache["foo.dat"];
                Assert.IsNull(card);
                card = cache["foobar.dat"];
                Assert.IsNotNull(card);
                Assert.AreEqual(PageType.Shortcut, card.Type);
                Assert.AreEqual("title", card.Title);
                Assert.AreEqual("foobar.dat", card.TargetName);

                // 4. file renamed, moving it out of the library: should look like a delete
                ev.Reset();
                File.Move(newFilePath, junkPath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                lock (eventArgs)
                {
                    Assert.AreEqual(1, eventArgs.Count);
                    args = eventArgs[0];
                    eventArgs.Clear();
                }
                Assert.IsNull(args.Added);
                Assert.IsNotNull(args.Removed);
                Assert.AreEqual(1, args.Removed.Count());
                Assert.IsNull(args.Modified);
                Assert.AreEqual("foobar.dat", args.Removed.ElementAt(0));
                Assert.AreEqual(count, cache.Count);
                card = cache["foobar.dat"];
                Assert.IsNull(card);

                // 5. file renamed, moving it into the library: should look like an add
                ev.Reset();
                File.Move(junkPath, newFilePath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                lock (eventArgs)
                {
                    Assert.AreEqual(1, eventArgs.Count);
                    args = eventArgs[0];
                    eventArgs.Clear();
                }
                Assert.IsNotNull(args.Added);
                Assert.AreEqual(1, args.Added.Count());
                Assert.IsNull(args.Removed);
                Assert.IsNull(args.Modified);
                Assert.AreEqual("foobar.dat", args.Added.ElementAt(0));
                Assert.AreEqual(count + 1, cache.Count);
                card = cache["foobar.dat"];
                Assert.IsNotNull(card);

                // 6. file deleted
                ev.Reset();
                File.Delete(newFilePath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                lock (eventArgs)
                {
                    Assert.AreEqual(1, eventArgs.Count);
                    args = eventArgs[0];
                    eventArgs.Clear();
                }
                Assert.IsNull(args.Added);
                Assert.IsNotNull(args.Removed);
                Assert.AreEqual(1, args.Removed.Count());
                Assert.IsNull(args.Modified);
                Assert.AreEqual("foobar.dat", args.Removed.ElementAt(0));
                Assert.AreEqual(count, cache.Count);
                card = cache["foobar.dat"];
                Assert.IsNull(card);

                // folders

                // 1. folder removed via rename
                ev.Reset();
                Directory.Move(folderPath, newFolderPath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                lock (eventArgs)
                {
                    Assert.AreEqual(1, eventArgs.Count);
                    args = eventArgs[0];
                    eventArgs.Clear();
                }
                Assert.IsNull(args.Added);
                Assert.IsNotNull(args.Removed);
                Assert.AreEqual(10, args.Removed.Count());
                Assert.IsNull(args.Modified);
                Assert.AreEqual(count - 10, cache.Count);

                // 2. empty folder added
                ev.Reset();
                Directory.CreateDirectory(folderPath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(count - 10, cache.Count);

                // 3. empty folder removed
                ev.Reset();
                Directory.Delete(folderPath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.AreEqual(count - 10, cache.Count);

                // 4. folder added via rename
                ev.Reset();
                Directory.Move(newFolderPath, folderPath);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                lock (eventArgs)
                {
                    Assert.AreEqual(1, eventArgs.Count);
                    args = eventArgs[0];
                    eventArgs.Clear();
                }
                Assert.IsNotNull(args.Added);
                Assert.AreEqual(10, args.Added.Count());
                Assert.IsNull(args.Removed);
                Assert.IsNull(args.Modified);
                Assert.AreEqual(count, cache.Count);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                if (File.Exists(newFilePath))
                    File.Delete(newFilePath);

                if (File.Exists(junkPath))
                    File.Delete(junkPath);

                TeardownMinimalLibrary();
            }
        }

        [TestMethod]
        public void LoadTest()
        {
            bool loadEventSeen   = false;
            bool unloadEventSeen = false;
            bool progressCalled  = false;
            int finalProgress    = -1;

            if (null != LibraryManager.Cache)
                LibraryManager.Unload();

            LoadProgressCallback callback = delegate(int progress, string status, string filename)
            {
                progressCalled = true;
                finalProgress = progress;
                Assert.IsNotNull(status);
                Assert.IsNotNull(filename);
                return true;
            };

            LibraryManager.CacheLoaded += delegate(object sender, EventArgs e)
            {
                loadEventSeen = true;
            };

            LibraryManager.CacheUnloaded += delegate(object sender, EventArgs e)
            {
                unloadEventSeen = true;
            };

            Assert.IsNull(LibraryManager.Cache);
            LibraryManager.Load(callback, false);
            Assert.IsTrue(loadEventSeen);
            Assert.IsTrue(progressCalled);
            Assert.AreEqual(100, finalProgress);
            Assert.IsNotNull(LibraryManager.Cache);
            LibraryManager.Unload();
            Assert.IsTrue(unloadEventSeen);
            Assert.IsNull(LibraryManager.Cache);

            // check we can cancel the load
            callback = delegate(int progress, string status, string filename)
            {
                return false;
            };

            try
            {
                LibraryManager.Load(callback, false);
                Assert.Fail();
            }
            catch (OperationCanceledException)
            {
            }

            Assert.IsNull(LibraryManager.Cache);
        }

        #endregion API

        #region Properties

        [TestMethod]
        public void CountTest()
        {
            LibraryManager cache = Load();
            Assert.AreNotEqual(0, cache.Count);
        }

        [TestMethod]
        public void InstalledVersionTest()
        {
            LibraryManager cache = Load();
            LDUpdate version = cache.InstalledVersion;
            Assert.AreNotEqual(0, version.Year);
            Assert.AreNotEqual(0, version.Release);
        }

        [TestMethod]
        public void IndexerTest()
        {
            LibraryManager cache = Load();
            IndexCard card = cache["1.dat"];
            Assert.IsNotNull(card);
            Assert.AreEqual("1.dat", card.TargetName);
            card = cache["1.DAT"];
            Assert.IsNotNull(card);
            Assert.AreEqual("1.dat", card.TargetName);
        }

        [TestMethod]
        public void PartsCategoriesTest()
        {
            LibraryManager cache = Load();
            IEnumerable<Category> actual;
            actual = cache.PartsCategories;
            Assert.AreNotEqual(0, actual.Count());
        }

        [TestMethod]
        public void PrimitivesCategoriesTest()
        {
            LibraryManager cache = Load();
            IEnumerable<Category> actual;
            actual = cache.PrimitivesCategories;
            Assert.AreNotEqual(0, actual.Count());
        }

        [TestMethod]
        public void SubpartsCategoriesTest()
        {
            LibraryManager cache = Load();
            IEnumerable<Category> actual;
            actual = cache.SubpartsCategories;
            Assert.AreNotEqual(0, actual.Count());
        }

        [TestMethod]
        public void StatisticsTest()
        {
            LibraryManager cache     = Load();
            LDUpdate originalVersion = cache.InstalledVersion;
            string path              = Path.Combine(Configuration.LDrawBase, "My Parts", "statisticstest.dat");
            const string code        = "0 title\r\n0 Name: statisticstest.dat\r\n0 !LDRAW_ORG Part UPDATE 3000-99\r\n";
            bool documentModified;

            if (File.Exists(path))
                File.Delete(path);

            AutoResetEvent ev = new AutoResetEvent(false);

            LibraryManager.Changed += delegate(object sender, LibraryChangedEventArgs e)
            {
                ev.Set();
            };

            try
            {
                LDDocument doc = new LDDocument(new StringReader(code), path, null, ParseFlags.None, out documentModified);
                ev.Reset();
                doc.Save();

                Assert.IsTrue(ev.WaitOne(EventTimeout));
                LDUpdate newVersion = cache.InstalledVersion;
                Assert.AreEqual(3000U, newVersion.Year);
                Assert.AreEqual(99U, newVersion.Release);

                ev.Reset();
                File.Delete(path);

                Assert.IsTrue(ev.WaitOne(EventTimeout));
                newVersion = cache.InstalledVersion;
                Assert.AreEqual(originalVersion.Year, newVersion.Year);
                Assert.AreEqual(originalVersion.Release, newVersion.Release);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        #endregion Properties

        #region Serialization

        [TestMethod]
        public void SerializeTest()
        {
            //LibraryManager target = Load();

            // TODO: SerializeTest()
        }

        #endregion Serialization
    }
}
