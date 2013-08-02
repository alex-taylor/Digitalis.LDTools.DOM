#region License

//
// IndexCardTest.cs
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

    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.Library;

    #endregion Usings

    [TestClass]
    public sealed class IndexCardTest
    {
        #region Definition Test

        [TestMethod]
        public void DefinitionTest()
        {
            Assert.IsFalse(typeof(IndexCard).IsSealed);
            Assert.IsTrue(typeof(IndexCard).IsSerializable);
        }

        #endregion Definition Test

        #region Constructor

        [TestMethod]
        public void IndexCardConstructorTest()
        {
            string path = Path.Combine(Configuration.LDrawBase, "parts", "3001.dat");
            FileInfo file = new FileInfo(path);
            IndexCard target = new IndexCard(file);
            Assert.AreEqual("3001.dat", target.TargetName);
            Assert.AreEqual("3001.dat", target.Name);
            Assert.AreEqual(path, target.Filepath);
            Assert.AreEqual(3001U, target.Number);
            Assert.AreEqual(3001U, target.Alias);
            Assert.AreEqual(PageType.Part, target.Type);
            Assert.AreEqual(Category.Brick, target.Category);
            Assert.AreEqual("Brick  2 x  4", target.Title);
            Assert.IsNull(target.Theme);
            Assert.AreEqual(file.LastWriteTime.ToFileTimeUtc(), target.Modified);
            Assert.AreEqual(0, target.Keywords.Count());
            Assert.AreEqual(null, target.Help);
            Assert.AreEqual(Palette.MainColour, target.DefaultColour);
            Assert.IsNotNull(target.Update);
            Assert.AreEqual(2004U, ((LDUpdate)target.Update).Year);
            Assert.AreEqual(3U, ((LDUpdate)target.Update).Release);
            Assert.IsTrue(target.IsPart);
            Assert.IsFalse(target.IsPrimitive);
            Assert.IsFalse(target.IsPatterned);
            Assert.IsFalse(target.IsRedirect);
            Assert.IsFalse(target.IsSubAssembly);
            Assert.IsFalse(target.IsSubpart);
            Assert.IsFalse(target.IsObsolete);
            Assert.IsFalse(target.IsModel);

            path = Path.Combine(Configuration.LDrawBase, "parts", "3001p02.dat");
            file = new FileInfo(path);
            target = new IndexCard(file);
            Assert.AreEqual("3001p02.dat", target.TargetName);
            Assert.AreEqual("3001p02.dat", target.Name);
            Assert.AreEqual(path, target.Filepath);
            Assert.AreEqual(3001U, target.Number);
            Assert.AreEqual(3001U, target.Alias);
            Assert.AreEqual(PageType.Part, target.Type);
            Assert.AreEqual(Category.Brick, target.Category);
            Assert.AreEqual("Brick  2 x  4 with Red Stripe and 4 Black Windows Pattern", target.Title);
            Assert.IsNull(target.Theme);
            Assert.AreEqual(file.LastWriteTime.ToFileTimeUtc(), target.Modified);
            Assert.AreEqual(0, target.Keywords.Count());
            Assert.AreEqual(null, target.Help);
            Assert.AreEqual(Palette.MainColour, target.DefaultColour);
            Assert.IsTrue(target.IsPart);
            Assert.IsFalse(target.IsPrimitive);
            Assert.IsTrue(target.IsPatterned);
            Assert.IsFalse(target.IsRedirect);
            Assert.IsFalse(target.IsSubAssembly);
            Assert.IsFalse(target.IsSubpart);
            Assert.IsFalse(target.IsObsolete);
            Assert.IsFalse(target.IsModel);

            path = Path.Combine(Configuration.LDrawBase, "parts", "u586p01c01.dat");
            file = new FileInfo(path);
            target = new IndexCard(file);
            Assert.AreEqual("u586p01c01.dat", target.TargetName);
            Assert.AreEqual("u586p01c01.dat", target.Name);
            Assert.AreEqual(path, target.Filepath);
            Assert.AreEqual(586U, target.Number);
            Assert.AreEqual(586U, target.Alias);
            Assert.AreEqual(PageType.Shortcut, target.Type);
            Assert.AreEqual(Category.Figure, target.Category);
            Assert.AreEqual("Figure Fabuland Bird Head  1 with Neck", target.Title);
            Assert.IsNull(target.Theme);
            Assert.AreEqual(file.LastWriteTime.ToFileTimeUtc(), target.Modified);
            Assert.AreEqual(0, target.Keywords.Count());
            Assert.AreEqual(null, target.Help);
            Assert.AreEqual(Palette.MainColour, target.DefaultColour);
            Assert.IsTrue(target.IsPart);
            Assert.IsFalse(target.IsPrimitive);
            Assert.IsTrue(target.IsPatterned);
            Assert.IsFalse(target.IsRedirect);
            Assert.IsFalse(target.IsSubAssembly);
            Assert.IsFalse(target.IsSubpart);
            Assert.IsFalse(target.IsObsolete);
            Assert.IsFalse(target.IsModel);

            path = Path.Combine(Configuration.LDrawBase, "parts", "s", "10s01.dat");
            file = new FileInfo(path);
            target = new IndexCard(file);
            Assert.AreEqual(@"s\10s01.dat", target.TargetName);
            Assert.AreEqual("10s01.dat", target.Name);
            Assert.AreEqual(path, target.Filepath);
            Assert.AreEqual(10U, target.Number);
            Assert.AreEqual(10U, target.Alias);
            Assert.AreEqual(PageType.Subpart, target.Type);
            Assert.AreEqual(Category.Baseplate, target.Category);
            Assert.AreEqual("~Baseplate 24 x 32 with Rounded Corners without Studs", target.Title);
            Assert.IsNull(target.Theme);
            Assert.AreEqual(file.LastWriteTime.ToFileTimeUtc(), target.Modified);
            Assert.AreEqual(0, target.Keywords.Count());
            Assert.AreEqual(null, target.Help);
            Assert.AreEqual(Palette.MainColour, target.DefaultColour);
            Assert.IsFalse(target.IsPart);
            Assert.IsFalse(target.IsPrimitive);
            Assert.IsFalse(target.IsPatterned);
            Assert.IsFalse(target.IsRedirect);
            Assert.IsFalse(target.IsSubAssembly);
            Assert.IsTrue(target.IsSubpart);
            Assert.IsFalse(target.IsObsolete);
            Assert.IsFalse(target.IsModel);

            path = Path.Combine(Configuration.LDrawBase, "unofficial", "parts", "10h.dat");
            file = new FileInfo(path);
            target = new IndexCard(file);
            Assert.AreEqual("10h.dat", target.TargetName);
            Assert.AreEqual("10h.dat", target.Name);
            Assert.AreEqual(path, target.Filepath);
            Assert.AreEqual(10U, target.Number);
            Assert.AreEqual(10U, target.Alias);
            Assert.AreEqual(PageType.Part, target.Type);
            Assert.AreEqual(Category.Baseplate, target.Category);
            Assert.AreEqual("Baseplate 24 x 32", target.Title);
            Assert.IsNull(target.Theme);
            Assert.AreEqual(file.LastWriteTime.ToFileTimeUtc(), target.Modified);
            Assert.AreEqual(0, target.Keywords.Count());
            Assert.AreEqual(null, target.Help);
            Assert.AreEqual(Palette.MainColour, target.DefaultColour);
            Assert.IsTrue(target.IsPart);
            Assert.IsFalse(target.IsPrimitive);
            Assert.IsFalse(target.IsPatterned);
            Assert.IsFalse(target.IsRedirect);
            Assert.IsFalse(target.IsSubAssembly);
            Assert.IsFalse(target.IsSubpart);
            Assert.IsFalse(target.IsObsolete);
            Assert.IsFalse(target.IsModel);

            path = Path.Combine(Configuration.LDrawBase, "unofficial", "parts", "42073.dat");
            file = new FileInfo(path);
            target = new IndexCard(file);
            Assert.AreEqual("42073.dat", target.TargetName);
            Assert.AreEqual("42073.dat", target.Name);
            Assert.AreEqual(path, target.Filepath);
            Assert.AreEqual(42073U, target.Number);
            Assert.AreEqual(42073U, target.Alias);
            Assert.AreEqual(PageType.Part, target.Type);
            Assert.AreEqual(Category.Brick, target.Category);
            Assert.AreEqual("~Motor Windup  2 x  6 x  2.333 Case", target.Title);
            Assert.IsNull(target.Theme);
            Assert.AreEqual(file.LastWriteTime.ToFileTimeUtc(), target.Modified);
            Assert.AreEqual(0, target.Keywords.Count());
            Assert.AreEqual(null, target.Help);
            Assert.AreEqual(Palette.MainColour, target.DefaultColour);
            Assert.IsTrue(target.IsPart);
            Assert.IsFalse(target.IsPrimitive);
            Assert.IsFalse(target.IsPatterned);
            Assert.IsFalse(target.IsRedirect);
            Assert.IsTrue(target.IsSubAssembly);
            Assert.IsFalse(target.IsSubpart);
            Assert.IsFalse(target.IsObsolete);
            Assert.IsFalse(target.IsModel);
        }

        #endregion Constructor

        #region Serialization

        [TestMethod]
        public void SerializeTest()
        {
            //IndexCard target = new IndexCard(new FileInfo(Path.Combine(Configuration.LDrawBase, "parts", "3001.dat")));

            // TODO: SerializeTest
        }

        #endregion Serialization
    }
}
