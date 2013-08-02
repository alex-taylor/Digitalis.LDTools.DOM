#region License

//
// ConfigurationTest.cs
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

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    [TestClass]
    public sealed class ConfigurationTest
    {
        #region API

        [TestMethod]
        public void GetCategoryFromNameTest()
        {
            Assert.AreEqual(Category.Unknown, Configuration.GetCategoryFromName("title", "name", PageType.Model));
            Assert.AreEqual(Category.Brick, Configuration.GetCategoryFromName("Brick 1 x 1", "name", PageType.Part));
            Assert.AreEqual(Category.Brick, Configuration.GetCategoryFromName("~Brick 1 x 1", "name", PageType.Subpart));
            Assert.AreEqual(Category.Brick, Configuration.GetCategoryFromName("_Brick 1 x 1", "name", PageType.Part_Physical_Colour));

            Assert.AreEqual(Category.Primitive_Edge, Configuration.GetCategoryFromName("title", "4-4edge.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Edge, Configuration.GetCategoryFromName("title", "4-4edgh.dat", PageType.Primitive));

            Assert.AreEqual(Category.Primitive_Disc, Configuration.GetCategoryFromName("title", "4-4ndis.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Disc, Configuration.GetCategoryFromName("title", "4-4disc.dat", PageType.Primitive));

            Assert.AreEqual(Category.Primitive_Chord, Configuration.GetCategoryFromName("title", "4-4chrd.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Chord, Configuration.GetCategoryFromName("title", "4-4chr.dat", PageType.Primitive));

            Assert.AreEqual(Category.Primitive_Cylinder, Configuration.GetCategoryFromName("title", "4-4cyli.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cylinder, Configuration.GetCategoryFromName("title", "4-4cyli2.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cylinder, Configuration.GetCategoryFromName("title", "4-4cyl2.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cylinder, Configuration.GetCategoryFromName("title", "4-4cyl.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cylinder, Configuration.GetCategoryFromName("title", "4-4cylo.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cylinder, Configuration.GetCategoryFromName("title", "4-4cyls.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cylinder, Configuration.GetCategoryFromName("title", "4-4cyls2.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cylinder, Configuration.GetCategoryFromName("title", "4-4cylc.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cylinder, Configuration.GetCategoryFromName("title", "4-4cylc2.dat", PageType.Primitive));

            Assert.AreEqual(Category.Primitive_Sphere, Configuration.GetCategoryFromName("title", "4-4sphe.dat", PageType.Primitive));

            Assert.AreEqual(Category.Primitive_Ring, Configuration.GetCategoryFromName("title", "aring.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Ring, Configuration.GetCategoryFromName("title", "ring1.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Ring, Configuration.GetCategoryFromName("title", "4-4ring1.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Ring, Configuration.GetCategoryFromName("title", "4-4rin10.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Ring, Configuration.GetCategoryFromName("title", "4-4ri10.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Ring, Configuration.GetCategoryFromName("title", "4-4r10.dat", PageType.Primitive));

            Assert.AreEqual(Category.Primitive_Cone, Configuration.GetCategoryFromName("title", "4-4cone1.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cone, Configuration.GetCategoryFromName("title", "4-4con1.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Cone, Configuration.GetCategoryFromName("title", "4-4co1.dat", PageType.Primitive));

            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "box.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "box0.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "box2-5.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "box3-7a.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "box3u2p.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "box3u4a.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "box3u6.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "box4o4a.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "box4t.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "boxjcyl4.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "tri3.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "tri3a1.dat", PageType.Primitive));
            Assert.AreEqual(Category.Primitive_Box, Configuration.GetCategoryFromName("title", "tri3u1.dat", PageType.Primitive));

            Assert.AreEqual(Category.Primitive_Text, Configuration.GetCategoryFromName("title", "typestn0.dat", PageType.Primitive));
        }

        #endregion API

        #region Constructor

        [TestMethod]
        public void ConfigurationConstructorTest()
        {
            Configuration_Accessor target = new Configuration_Accessor();
            Assert.IsNotNull(target._metaCommandImports);
        }

        #endregion Constructor

        #region LDraw Library location

        [TestMethod]
        public void ValidateBasePathTest()
        {
            List<string> primary;
            List<string> full;

            // invalid path
            try
            {
                Configuration_Accessor.ValidateBasePath(@"C:\foo", out primary, out full);
            }
            catch (DirectoryNotFoundException)
            {
            }

            // invalid folder
            try
            {
                Configuration_Accessor.ValidateBasePath(@"C:\", out primary, out full);
            }
            catch (FileNotFoundException)
            {
            }

            // valid path
            Configuration_Accessor.ValidateBasePath(@"C:\LDraw", out primary, out full);
            Assert.AreEqual(11, full.Count);
            Assert.AreEqual(6, primary.Count);
        }

        [TestMethod]
        public void FullSearchPathTest()
        {
            IEnumerable<string> actual;
            actual = Configuration.FullSearchPath;

            Assert.AreEqual(11, actual.Count());
            Assert.IsTrue(actual.ElementAt(0).EndsWith(@"\My Parts"));
            Assert.IsTrue(actual.ElementAt(1).EndsWith(@"\My Parts\s"));
            Assert.IsTrue(actual.ElementAt(2).EndsWith(@"\unofficial\parts"));
            Assert.IsTrue(actual.ElementAt(3).EndsWith(@"\unofficial\parts\s"));
            Assert.IsTrue(actual.ElementAt(4).EndsWith(@"\unofficial\p"));
            Assert.IsTrue(actual.ElementAt(5).EndsWith(@"\unofficial\p\48"));
            Assert.IsTrue(actual.ElementAt(6).EndsWith(@"\models"));
            Assert.IsTrue(actual.ElementAt(7).EndsWith(@"\parts"));
            Assert.IsTrue(actual.ElementAt(8).EndsWith(@"\parts\s"));
            Assert.IsTrue(actual.ElementAt(9).EndsWith(@"\p"));
            Assert.IsTrue(actual.ElementAt(10).EndsWith(@"\p\48"));
        }

        [TestMethod]
        public void LDConfigPathTest()
        {
            Assert.IsTrue(Configuration.LDConfigPath.EndsWith(@"\ldconfig.ldr"));
        }

        [TestMethod]
        public void LDrawBaseTest()
        {
            string ldbase = Configuration.LDrawBase;
            bool eventSeen = false;

            Assert.IsNotNull(ldbase);

            Configuration.LDrawBaseChanged += delegate(object sender, EventArgs e)
            {
                eventSeen = true;
            };

            try
            {
                Configuration.LDrawBase = "foo";
                Assert.Fail();
            }
            catch
            {
            }

            Assert.IsFalse(eventSeen);

            Configuration.LDrawBase = @"Y:\Lego\LDraw";
            Assert.IsTrue(eventSeen);

            Configuration.LDrawBase = ldbase;
        }

        [TestMethod]
        public void PrimarySearchPathTest()
        {
            IEnumerable<string> actual;
            actual = Configuration.PrimarySearchPath;

            Assert.AreEqual(6, actual.Count());
            Assert.IsTrue(actual.ElementAt(0).EndsWith(@"\My Parts"));
            Assert.IsTrue(actual.ElementAt(1).EndsWith(@"\unofficial\parts"));
            Assert.IsTrue(actual.ElementAt(2).EndsWith(@"\unofficial\p"));
            Assert.IsTrue(actual.ElementAt(3).EndsWith(@"\models"));
            Assert.IsTrue(actual.ElementAt(4).EndsWith(@"\parts"));
            Assert.IsTrue(actual.ElementAt(5).EndsWith(@"\p"));
        }

        #endregion LDraw Library location

        #region Pluggable elements

        [TestMethod]
        public void ElementTypesTest()
        {
            IEnumerable<ElementDefinition> elements = Configuration.ElementTypes;
            ElementDefinition definition;
            int count = 0;

            Assert.IsNotNull(elements);

            definition = GetElementType(elements, typeof(LDBFCFlag));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDBFCFlag), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.ElementCategory_MetaCommand, definition.Category);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("0 BFC CCW"));
            count++;

            definition = GetElementType(elements, typeof(LDClear));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDClear), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.ElementCategory_MetaCommand, definition.Category);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual((ElementFlags)0, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("0 CLEAR"));
            count++;

            definition = GetElementType(elements, typeof(LDColour));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDColour), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.ElementCategory_MetaCommand, definition.Category);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("0 !COLOUR Black CODE 0 VALUE #05131D EDGE #595959"));
            count++;

            definition = GetElementType(elements, typeof(LDComment));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDComment), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("0 comment"));
            count++;

            definition = GetElementType(elements, typeof(LDLine));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDLine), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("2 24 0 0 0 1 1 1"));
            count++;

            definition = GetElementType(elements, typeof(LDOptionalLine));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDOptionalLine), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("5 24 0 0 0 1 1 1 2 2 2 3 3 3"));
            count++;

            definition = GetElementType(elements, typeof(LDPause));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDPause), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.ElementCategory_MetaCommand, definition.Category);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual((ElementFlags)0, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("0 PAUSE"));
            count++;

            definition = GetElementType(elements, typeof(LDQuadrilateral));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDQuadrilateral), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("4 16 0 0 0 1 1 1 2 2 2 3 3 3"));
            count++;

            definition = GetElementType(elements, typeof(LDReference));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDReference), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("1 16 0 0 0 1 0 0 0 1 0 0 0 1 part.dat"));
            count++;

            definition = GetElementType(elements, typeof(LDSave));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDSave), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.ElementCategory_MetaCommand, definition.Category);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual((ElementFlags)0, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("0 SAVE"));
            count++;

            definition = GetElementType(elements, typeof(LDTexmap));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDTexmap), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 texture.png"));
            count++;

            definition = GetElementType(elements, typeof(LDTriangle));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDTriangle), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("3 16 0 0 0 1 1 1 2 2 2"));
            count++;

            definition = GetElementType(elements, typeof(LDWrite));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(LDWrite), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.AreEqual(Digitalis.LDTools.DOM.Properties.Resources.ElementCategory_MetaCommand, definition.Category);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("0 WRITE message"));
            count++;

            definition = GetElementType(elements, typeof(MLCadGroup));
            Assert.IsNotNull(definition);
            Assert.AreEqual(typeof(MLCadGroup), definition.Type);
            Assert.IsNotNull(definition.TypeName);
            Assert.IsNotNull(definition.DefaultIcon);
            Assert.AreEqual(ElementFlags.HasEditor | ElementFlags.TopLevelElement, definition.Flags);
            Assert.IsNotNull(definition.Create());
            Assert.IsNotNull(definition.Create("0 GROUP 1 name"));
            count++;

            Assert.AreEqual(count, elements.Count());
        }

        private ElementDefinition GetElementType(IEnumerable<ElementDefinition> elements, Type type)
        {
            foreach (ElementDefinition definition in elements)
            {
                if (definition.Type.Equals(type))
                    return definition;
            }

            return null;
        }

        #endregion Pluggable elements

        #region Properties

        [TestMethod]
        public void AuthorTest()
        {
            string author = Configuration.Author;

            // default value
            Assert.AreEqual(Environment.UserName, author);

            // and change it
            Configuration.Author = "author";
            Assert.AreEqual("author", Configuration.Author);

            // and restore because other tests rely on it!
            Configuration.Author = author;
        }

        [TestMethod]
        public void DecimalPlacesCoordinatesTest()
        {
            // default value
            Assert.AreEqual(3U, Configuration.DecimalPlacesCoordinates);
            Assert.AreEqual("0.###", Configuration.DecimalPlacesCoordinatesFormatter);

            // and change it
            Configuration.DecimalPlacesCoordinates = 6;
            Assert.AreEqual(6U, Configuration.DecimalPlacesCoordinates);
            Assert.AreEqual("0.######", Configuration.DecimalPlacesCoordinatesFormatter);
        }

        [TestMethod]
        public void DecimalPlacesPrimitivesTest()
        {
            // default value
            Assert.AreEqual(4U, Configuration.DecimalPlacesPrimitives);
            Assert.AreEqual("0.####", Configuration.DecimalPlacesPrimitivesFormatter);

            // and change it
            Configuration.DecimalPlacesPrimitives = 6;
            Assert.AreEqual(6U, Configuration.DecimalPlacesPrimitives);
            Assert.AreEqual("0.######", Configuration.DecimalPlacesPrimitivesFormatter);
        }

        [TestMethod]
        public void DecimalPlacesTransformsTest()
        {
            // default value
            Assert.AreEqual(5U, Configuration.DecimalPlacesTransforms);
            Assert.AreEqual("0.#####", Configuration.DecimalPlacesTransformsFormatter);

            // and change it
            Configuration.DecimalPlacesTransforms = 6;
            Assert.AreEqual(6U, Configuration.DecimalPlacesTransforms);
            Assert.AreEqual("0.######", Configuration.DecimalPlacesTransformsFormatter);
        }

        [TestMethod]
        public void UsernameTest()
        {
            Assert.IsNotNull(Configuration.Username);
        }

        #endregion Properties
    }
}
