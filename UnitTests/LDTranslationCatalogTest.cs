#region License
//
// ConfigurationTest.cs
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
using Digitalis.LDTools.DOM;
using Digitalis.LDTools.DOM.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for ConfigurationTest and is intended
    ///to contain all ConfigurationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LDTranslationCatalogTest
    {
        private static readonly string LocaleUK = "en-GB";
        private static readonly string LocaleDE = "de";

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void InstalledLocalesTest()
        {
            string[] locales = LDTranslationCatalog.InstalledLocales;
            Assert.IsNotNull(locales);
            Assert.AreEqual(1, locales.Length);
            Assert.AreEqual(LocaleDE, locales[0]);
        }

        [TestMethod()]
        public void LoadCatalogTest()
        {
            LDTranslationCatalog[] locale = LDTranslationCatalog.LoadCatalog(LocaleUK);
            Assert.IsNull(locale);

            locale = LDTranslationCatalog.LoadCatalog(LocaleDE);
            Assert.IsNotNull(locale);
            Assert.AreEqual(4, locale.Length);

            Assert.IsNotNull(locale[(int)CatalogType.Categories]);
            Assert.AreEqual(CatalogType.Categories, locale[(int)CatalogType.Categories].Type);
            Assert.AreEqual(LocaleDE, locale[(int)CatalogType.Categories].LocaleName);
            Assert.AreEqual(65, locale[(int)CatalogType.Categories].Count);

            Assert.IsNotNull(locale[(int)CatalogType.Colours]);
            Assert.AreEqual(CatalogType.Colours, locale[(int)CatalogType.Colours].Type);
            Assert.AreEqual(LocaleDE, locale[(int)CatalogType.Colours].LocaleName);
            Assert.AreEqual(127, locale[(int)CatalogType.Colours].Count);

            Assert.IsNull(locale[(int)CatalogType.Keywords]);

            Assert.IsNotNull(locale[(int)CatalogType.Titles]);
            Assert.AreEqual(CatalogType.Titles, locale[(int)CatalogType.Titles].Type);
            Assert.AreEqual(LocaleDE, locale[(int)CatalogType.Titles].LocaleName);
            Assert.AreEqual(0, locale[(int)CatalogType.Titles].Count);
        }

        [TestMethod()]
        public void MovedToTemplateTest()
        {
            LDTranslationCatalog catalog = new LDTranslationCatalog("dummy", CatalogType.Titles);

            try
            {
                catalog.MovedToTemplate = "foo";
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }

            try
            {
                catalog.MovedToTemplate = "foo {0} {1}";
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }

            try
            {
                catalog.MovedToTemplate = "foo {1}";
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }

            catalog.MovedToTemplate = "";
            catalog.MovedToTemplate = "foo {0}";
            catalog.MovedToTemplate = "{0} foo";
            catalog.MovedToTemplate = "foo {0} bar";

            catalog = new LDTranslationCatalog("dummy", CatalogType.Categories);

            try
            {
                catalog.MovedToTemplate = "foo {0}";
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            catalog = new LDTranslationCatalog("dummy", CatalogType.Colours);

            try
            {
                catalog.MovedToTemplate = "foo {0}";
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            catalog = new LDTranslationCatalog("dummy", CatalogType.Keywords);

            try
            {
                catalog.MovedToTemplate = "foo {0}";
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }
        }

        [TestMethod()]
        public void MovedToTest()
        {
            Assert.AreEqual("~Moved to 12345.dat", LDTranslationCatalog.GetPartTitle("~Moved to 12345.dat"));
            Assert.AreEqual("xxx 12345.dat xxx", LDTranslationCatalog.GetPartTitle(LocaleDE, "~Moved to 12345.dat"));

            LDTranslationCatalog[] locale = LDTranslationCatalog.LoadCatalog(LocaleDE);
            Assert.AreEqual("xxx 12345.dat xxx", locale[(int)CatalogType.Titles]["~Moved to 12345.dat"]);

            Assert.IsNull(locale[(int)CatalogType.Categories]["~Moved to 12345.dat"]);
            Assert.IsNull(locale[(int)CatalogType.Colours]["~Moved to 12345.dat"]);
        }

        [TestMethod()]
        public void SaveTest()
        {
            LDTranslationCatalog titles = new LDTranslationCatalog("mylocale", CatalogType.Titles);

            Assert.IsFalse(titles.ContainsKey("foo"));
            titles["foo"] = "bar";
            Assert.IsTrue(titles.ContainsKey("foo"));
            Assert.AreEqual("bar", titles["foo"]);

            titles.Save();
            titles = new LDTranslationCatalog("mylocale", CatalogType.Titles);
            Assert.IsFalse(titles.ContainsKey("foo"));
            titles.Load();
            Assert.IsTrue(titles.ContainsKey("foo"));
            Assert.AreEqual("bar", titles["foo"]);

            titles.Remove("foo");
            Assert.IsFalse(titles.ContainsKey("foo"));
            titles.Save();
            titles = new LDTranslationCatalog("mylocale", CatalogType.Titles);
            Assert.IsFalse(titles.ContainsKey("foo"));
            titles.Load();
            Assert.IsFalse(titles.ContainsKey("foo"));

            Directory.Delete(Path.Combine(Configuration.LDrawBase, "localisations\\mylocale"), true);
        }

        /// <summary>
        ///A test for GetPageType
        ///</summary>
        [TestMethod()]
        public void GetPageTypeTest()
        {
            // en-GB; all other locales are DLL-dependent
            Assert.AreEqual("Model", LDTranslationCatalog.GetPageType(PageType.Model));
            Assert.AreEqual("Part", LDTranslationCatalog.GetPageType(PageType.Part));
            Assert.AreEqual("Subpart", LDTranslationCatalog.GetPageType(PageType.Subpart));
            Assert.AreEqual("Primitive", LDTranslationCatalog.GetPageType(PageType.Primitive));
            Assert.AreEqual("Hi-res Primitive", LDTranslationCatalog.GetPageType(PageType.HiresPrimitive));
            Assert.AreEqual("Part (Alias)", LDTranslationCatalog.GetPageType(PageType.Part_Alias));
            Assert.AreEqual("Part (Physical-Colour)", LDTranslationCatalog.GetPageType(PageType.Part_Physical_Colour));
            Assert.AreEqual("Shortcut", LDTranslationCatalog.GetPageType(PageType.Shortcut));
            Assert.AreEqual("Shortcut (Alias)", LDTranslationCatalog.GetPageType(PageType.Shortcut_Alias));
            Assert.AreEqual("Shortcut (Physical-Colour)", LDTranslationCatalog.GetPageType(PageType.Shortcut_Physical_Colour));
        }

        /// <summary>
        ///A test for GetColourName
        ///</summary>
        [TestMethod()]
        public void GetColourNameTest()
        {
            Assert.AreEqual("Red", LDTranslationCatalog.GetColourName("Red"));
            Assert.AreEqual("Red", LDTranslationCatalog.GetColourName(LocaleUK, "Red"));
            Assert.AreEqual("Rot", LDTranslationCatalog.GetColourName(LocaleDE, "Red"));
            Assert.AreEqual("Foo", LDTranslationCatalog.GetColourName(LocaleDE, "Foo"));

            // underscores should be replaced with spaces
            Assert.AreEqual("Glow In Dark Opaque", LDTranslationCatalog.GetColourName("Glow_In_Dark_Opaque"));
        }

        [TestMethod()]
        public void GetCategoryTest()
        {
            // basic test
            Assert.AreEqual("Animal", LDTranslationCatalog.GetCategory(Category.Animal));
            Assert.AreEqual("Animal", LDTranslationCatalog.GetCategory(LocaleUK, Category.Animal));
            Assert.AreEqual("Tier", LDTranslationCatalog.GetCategory(LocaleDE, Category.Animal));

            // check the multi-word categories
            Assert.AreEqual("Figure Accessory", LDTranslationCatalog.GetCategory(Category.FigureAccessory));
            Assert.AreEqual("Minifig Accessory", LDTranslationCatalog.GetCategory(Category.MinifigAccessory));
            Assert.AreEqual("Minifig Footwear", LDTranslationCatalog.GetCategory(Category.MinifigFootwear));
            Assert.AreEqual("Minifig Headwear", LDTranslationCatalog.GetCategory(Category.MinifigHeadwear));
            Assert.AreEqual("Minifig Hipwear", LDTranslationCatalog.GetCategory(Category.MinifigHipwear));
            Assert.AreEqual("Minifig Neckwear", LDTranslationCatalog.GetCategory(Category.MinifigNeckwear));

            Assert.AreEqual("Minifig Zubehör", LDTranslationCatalog.GetCategory(LocaleDE, Category.MinifigAccessory));
            Assert.AreEqual("Minifig Footwear", LDTranslationCatalog.GetCategory(LocaleDE, Category.MinifigFootwear));
        }
    }
}
