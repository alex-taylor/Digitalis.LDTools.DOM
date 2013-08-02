#region License

//
// PaletteTest.cs
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
    using System.IO;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    [TestClass]
    public sealed class PaletteTest
    {
        #region Infrastructure

        private const int EventTimeout = 750;

        #endregion Infrastructure

        #region Constructor

        [TestMethod]
        public void PaletteConstructorTest()
        {
            using (Palette_Accessor target = new Palette_Accessor(Configuration.LDConfigPath))
            {
                Assert.AreNotEqual(0, target.Count);
                Assert.IsNotNull(target[Palette.MainColour]);
                Assert.IsNotNull(target[Palette.EdgeColour]);
                Assert.IsTrue(target.MaxCode >= Palette.EdgeColour);
            }
        }

        [TestMethod]
        public void PaletteConstructorTest1()
        {
            Palette_Accessor target = new Palette_Accessor();
            Assert.AreEqual(2, target.Count);
            Assert.IsNotNull(target[Palette.MainColour]);
            Assert.IsNotNull(target[Palette.EdgeColour]);
            Assert.AreEqual(Palette.EdgeColour, target.MaxCode);
        }

        #endregion Constructor

        #region Internals

        // TODO: PaletteTest.UpdateTest() disabled due to unreliability
        //[TestMethod]
        public void UpdateTest()
        {
            string ldconfigBackup        = Configuration.LDrawBase + @"\ldconfig.bak";
            const string ldconfigModified = @"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\LDConfig.ldr";
            string ldconfigPath          = Configuration.LDConfigPath;

            try
            {
                AutoResetEvent ev = new AutoResetEvent(false);
                Palette target    = Palette.SystemPalette;
                int count         = target.Count;

                target.ContentsChanged += delegate(object sender, EventArgs e)
                {
                    ev.Set();
                };

                // start with a known clean file
                File.Copy(ldconfigBackup, ldconfigPath, true);
                Assert.IsTrue(ev.WaitOne(EventTimeout));
                Assert.IsNotNull(target[0]);
                Assert.IsNull(target[666]);

                File.Copy(ldconfigModified, ldconfigPath, true);
                Assert.IsTrue(ev.WaitOne(EventTimeout));

                // colour added
                IColour colour = target[666];
                Assert.IsNotNull(colour);
                Assert.AreEqual("Test_Colour", colour.Name);
                Assert.AreEqual(666U, colour.Code);
                Assert.AreEqual(Color.FromArgb(0xFF, 0x12, 0x34, 0x56), colour.Value);
                Assert.AreEqual(0x2654321U, colour.EdgeCode);

                // colour changed
                colour = target[4];
                Assert.IsNotNull(colour);
                Assert.AreEqual("Test_Colour_Red", colour.Name);
                Assert.AreEqual(4U, colour.Code);
                Assert.AreEqual(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0), colour.Value);
                Assert.AreEqual(0x20F0F0FU, colour.EdgeCode);

                // colour removed
                colour = target[0];
                Assert.IsNull(colour);
            }
            finally
            {
                File.Copy(ldconfigBackup, ldconfigPath, true);
            }
        }

        [TestMethod]
        public void UpdateLDBaseTest()
        {
            string ldbase = Configuration.LDrawBase;
            Palette target = Palette.SystemPalette;
            AutoResetEvent ev = new AutoResetEvent(false);

            target.ContentsChanged += delegate(object sender, EventArgs e)
            {
                ev.Set();
            };

            Configuration.LDrawBase = @"Y:\Lego\LDraw";
            Assert.IsTrue(ev.WaitOne(5000));
            Assert.AreNotEqual(0, target.Count);
            Configuration.LDrawBase = ldbase;
        }

        [TestMethod]
        public void PaletteForElementTest()
        {
            Palette target;
            LDPage page = new LDPage();
            page.Add(new LDStep());

            // page with no LDColour content
            LDLine line = new LDLine();
            page[0].Add(line);
            target = Palette.PaletteForElement(line);
            Assert.IsNotNull(target);
            Assert.AreEqual(Palette.SystemPalette, target);

            // page with additional non-colour content
            page[0].Insert(0, new LDComment());
            target = Palette.PaletteForElement(line);
            Assert.IsNotNull(target);
            Assert.AreEqual(Palette.SystemPalette, target);

            // add a new LDColour ahead of the line
            page[0].Insert(0, new LDColour("Colour", 1000, Color.Black, Color.White, 0, new PlasticMaterial()));
            target = Palette.PaletteForElement(line);
            Assert.IsNotNull(target);
            Assert.AreNotEqual(Palette.SystemPalette, target);
            Assert.IsNotNull(target[1000]);

            // add one after the line
            page[0].Add(new LDColour("Colour2", 2000, Color.Black, Color.White, 0, new PlasticMaterial()));
            target = Palette.PaletteForElement(line);
            Assert.IsNotNull(target);
            Assert.AreNotEqual(Palette.SystemPalette, target);
            Assert.IsNotNull(target[1000]);
            Assert.IsNull(target[2000]);

            // add one which overrides something in the system-palette
            page[0].Insert(0, new LDColour("Override", 4, Color.Black, Color.White, 0, new PlasticMaterial()));
            target = Palette.PaletteForElement(line);
            Assert.IsNotNull(target);
            Assert.AreNotEqual(Palette.SystemPalette, target);
            Assert.IsNotNull(target[4]);
            Assert.AreNotEqual(Palette.SystemPalette[4], target[4]);
            Assert.AreNotEqual(page[0], target[4]);
            Assert.IsTrue(target[4].IsFrozen);

            // an element in a later step should still get the colours from earlier ones
            page.Add(new LDStep());
            page.Elements.Remove(line);
            page[1].Add(line);
            target = Palette.PaletteForElement(line);
            Assert.IsNotNull(target);
            Assert.AreNotEqual(Palette.SystemPalette, target);
            Assert.IsNotNull(target[4]);
            Assert.AreNotEqual(Palette.SystemPalette[4], target[4]);
            Assert.AreNotEqual(page[0], target[4]);
            Assert.IsTrue(target[4].IsFrozen);
        }

        #endregion Internals

        #region Properties

        [TestMethod]
        public void SystemPaletteTest()
        {
            Palette target = Palette.SystemPalette;

            Assert.IsNotNull(target);
            Assert.AreNotEqual(0, target.Count);
            Assert.IsNotNull(target[Palette.MainColour]);
            Assert.IsNotNull(target[Palette.EdgeColour]);
            Assert.IsTrue(target.MaxCode >= Palette.EdgeColour);
        }

        #endregion Properties
    }
}