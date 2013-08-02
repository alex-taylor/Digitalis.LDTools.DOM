#region License

//
// IElementCollectionTest.cs
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
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.UndoSystem;

    using Configuration = Digitalis.LDTools.DOM.Configuration;

    #endregion Usings

    public static class IElementCollectionTest
    {
        #region Change-notification

        public static void ChangedTest(IElementCollection collection)
        {
            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.ChangedTest() not implemented for immutable/read-only objects");
            }
            else
            {
                ILine line     = MocksFactory.CreateMockLine();
                bool eventSeen = false;

                collection.Add(line);

                collection.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(collection, sender);
                    Assert.AreSame(line, e.Source);
                    Assert.AreEqual("ColourValueChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<uint>));

                    PropertyChangedEventArgs<uint> args = (PropertyChangedEventArgs<uint>)e.Parameters;
                    Assert.AreEqual(Palette.EdgeColour, args.OldValue);
                    Assert.AreEqual(1U, args.NewValue);
                };

                // Changed events issued by children of the collection should propagate up
                Assert.IsFalse(eventSeen);
                line.ColourValue = 1U;
                Assert.IsTrue(eventSeen);
            }
        }

        #endregion Change-notification

        #region Cloning and Serialization

        public static void PrepareForCloning(IElementCollection collection)
        {
            if (!collection.IsImmutable && !collection.IsReadOnly)
                CreateGeometry(collection);
        }

        public static void CompareClonedObjects(IElementCollection original, IElementCollection copy)
        {
            bool originalTreeEventSeen = false;
            bool copyTreeEventSeen     = false;
            bool originalPathChanged   = false;
            bool copyPathChanged       = false;

            EventHandler originalPathToDocumentChanged = delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(originalPathChanged);
                originalPathChanged = true;
            };

            EventHandler copyPathToDocumentChanged = delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(copyPathChanged);
                copyPathChanged = true;
            };

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

            Assert.AreEqual(original.Count, copy.Count);

            for (int i = 0; i < original.Count; i++)
            {
                // basic check: can the new parent and child see each other?
                Assert.AreNotSame(original[i], copy[i]);
                Assert.IsTrue(copy.Contains(copy[i]));
                Assert.AreSame(copy, copy[i].Parent);

                if (original[i] is IGraphic)
                {
                    IGraphic originalGraphic = (IGraphic)original[i];
                    IGraphic copyGraphic     = (IGraphic)copy[i];

                    Assert.AreEqual(originalGraphic.CoordinatesCount, copyGraphic.CoordinatesCount);

                    for (int n = 0; n < originalGraphic.CoordinatesCount; n++)
                    {
                        Assert.AreEqual(originalGraphic.Coordinates.ElementAt(n), copyGraphic.Coordinates.ElementAt(n));
                    }
                }

                // verify that the child is sending its Changed events to the correct collection
                originalTreeEventSeen = false;
                copyTreeEventSeen     = false;
                original[i].IsLocked  = !original[i].IsLocked;
                Assert.IsTrue(originalTreeEventSeen);
                Assert.IsFalse(copyTreeEventSeen);

                originalTreeEventSeen = false;
                copyTreeEventSeen     = false;
                copy[i].IsLocked      = !copy[i].IsLocked;
                Assert.IsFalse(originalTreeEventSeen);
                Assert.IsTrue(copyTreeEventSeen);

                // verify that the child reacts to PathToDocumentChanged events from its collection
                original[i].PathToDocumentChanged += originalPathToDocumentChanged;
                copy[i].PathToDocumentChanged     += copyPathToDocumentChanged;

                switch (original.ObjectType)
                {
                    case DOMObjectType.Page:
                        IDocument document = MocksFactory.CreateMockDocument();

                        originalTreeEventSeen = false;
                        copyTreeEventSeen     = false;
                        originalPathChanged   = false;
                        copyPathChanged       = false;
                        document.Add((IPage)original);
                        Assert.IsTrue(originalPathChanged);
                        Assert.IsFalse(copyPathChanged);
                        originalTreeEventSeen = false;
                        copyTreeEventSeen     = false;
                        originalPathChanged   = false;
                        copyPathChanged       = false;
                        document.Remove((IPage)original);

                        originalTreeEventSeen = false;
                        copyTreeEventSeen     = false;
                        originalPathChanged   = false;
                        copyPathChanged       = false;
                        document.Add((IPage)copy);
                        Assert.IsTrue(copyPathChanged);
                        Assert.IsFalse(originalPathChanged);
                        originalTreeEventSeen = false;
                        copyTreeEventSeen     = false;
                        originalPathChanged   = false;
                        copyPathChanged       = false;
                        document.Remove((IPage)copy);
                        break;

                    case DOMObjectType.Step:
                        IPage page = MocksFactory.CreateMockPage();

                        originalTreeEventSeen = false;
                        copyTreeEventSeen     = false;
                        originalPathChanged   = false;
                        copyPathChanged       = false;
                        page.Add((IStep)original);
                        Assert.IsTrue(originalPathChanged);
                        Assert.IsFalse(copyPathChanged);
                        originalTreeEventSeen = false;
                        copyTreeEventSeen     = false;
                        originalPathChanged   = false;
                        copyPathChanged       = false;
                        page.Remove((IStep)original);

                        originalTreeEventSeen = false;
                        copyTreeEventSeen     = false;
                        originalPathChanged   = false;
                        copyPathChanged       = false;
                        page.Add((IStep)copy);
                        Assert.IsTrue(copyPathChanged);
                        Assert.IsFalse(originalPathChanged);
                        originalTreeEventSeen = false;
                        copyTreeEventSeen     = false;
                        originalPathChanged   = false;
                        copyPathChanged       = false;
                        page.Remove((IStep)copy);
                        break;

                    default:
                        if (original is IElement)
                        {
                            IStep step = MocksFactory.CreateMockStep();

                            originalTreeEventSeen = false;
                            copyTreeEventSeen     = false;
                            originalPathChanged   = false;
                            copyPathChanged       = false;
                            step.Add((IElement)original);
                            Assert.IsTrue(originalPathChanged);
                            Assert.IsFalse(copyPathChanged);
                            originalTreeEventSeen = false;
                            copyTreeEventSeen     = false;
                            originalPathChanged   = false;
                            copyPathChanged       = false;
                            step.Remove((IElement)original);

                            originalTreeEventSeen = false;
                            copyTreeEventSeen     = false;
                            originalPathChanged   = false;
                            copyPathChanged       = false;
                            step.Add((IElement)copy);
                            Assert.IsTrue(copyPathChanged);
                            Assert.IsFalse(originalPathChanged);
                            originalTreeEventSeen = false;
                            copyTreeEventSeen     = false;
                            originalPathChanged   = false;
                            copyPathChanged       = false;
                            step.Remove((IElement)copy);
                        }
                        break;
                }

                original[i].PathToDocumentChanged -= originalPathToDocumentChanged;
                copy[i].PathToDocumentChanged     -= copyPathToDocumentChanged;
            }
        }

        #endregion Cloning and Serialization

        #region Code-generation

        public static void ToCodeTest(IElementCollection collection, IElement containingElement, string prefix)
        {
            StringBuilder code;
            ILine line                 = MocksFactory.CreateMockLine();
            ITriangle triangle         = MocksFactory.CreateMockTriangle();
            IQuadrilateral quad        = MocksFactory.CreateMockQuadrilateral();
            IOptionalLine optionalLine = MocksFactory.CreateMockOptionalLine();
            IReference reference       = MocksFactory.CreateMockReference();

            line.Vertex1 = new Vector3d(1, 2, 3);
            line.Vertex2 = new Vector3d(4, 5, 6);

            triangle.Vertex1 = new Vector3d(1, 2, 3);
            triangle.Vertex2 = new Vector3d(4, 5, 6);
            triangle.Vertex3 = new Vector3d(7, 8, 9);

            quad.Vertex1 = new Vector3d(1, 2, 3);
            quad.Vertex2 = new Vector3d(4, 5, 6);
            quad.Vertex3 = new Vector3d(7, 8, 9);
            quad.Vertex4 = new Vector3d(10, 11, 12);

            optionalLine.Vertex1  = new Vector3d(1, 2, 3);
            optionalLine.Vertex2  = new Vector3d(4, 5, 6);
            optionalLine.Control1 = new Vector3d(7, 8, 9);
            optionalLine.Control2 = new Vector3d(10, 11, 12);

            reference.TargetName = "test.dat";

            // empty collection
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(String.Empty, code.ToString());

            // add some basic geometry elements
            collection.Add(line);
            collection.Add(triangle);
            collection.Add(quad);
            collection.Add(optionalLine);
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 1 2 3 4 5 6\r\n" +
                            prefix + "3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 24 1 2 3 4 5 6 7 8 9 10 11 12\r\n",
                            code.ToString());

            // add a reference element
            collection.Add(reference);
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 1 2 3 4 5 6\r\n" +
                            prefix + "3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 24 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n",
                            code.ToString());

            // add a child-collection
            MockElementCollection child = new MockElementCollection();
            collection.Add(child);
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 1 2 3 4 5 6\r\n" +
                            prefix + "3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 24 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n",
                            code.ToString());

            // and add some elements to the child
            ILine line2 = MocksFactory.CreateMockLine();
            line2.Vertex1 = new Vector3d(1, 0, 0);
            line2.Vertex2 = new Vector3d(2, 0, 0);
            line2.ColourValue = 1U;
            child.Add(line2);
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 1 2 3 4 5 6\r\n" +
                            prefix + "3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 24 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            // output in PartsLibrary mode
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 1 2 3 4 5 6\r\n" +
                            prefix + "3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 24 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            // and in OMR mode
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 1 2 3 4 5 6\r\n" +
                            prefix + "3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 24 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 16 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            // set a fixed-colour from the system-palette: edges should become Direct Colours as there is no palette equivalent for the RGB value
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, 1U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 #2333333 1 2 3 4 5 6\r\n" +
                            prefix + "3 1 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #2333333 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, 1U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 #2333333 1 2 3 4 5 6\r\n" +
                            prefix + "3 1 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #2333333 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, 1U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 #2333333 1 2 3 4 5 6\r\n" +
                            prefix + "3 1 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #2333333 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            // invalid colour: should be ignored and the edges left as colour-24
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, 1000U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 1 2 3 4 5 6\r\n" +
                            prefix + "3 1000 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 24 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1000 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, 1000U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 1 2 3 4 5 6\r\n" +
                            prefix + "3 1000 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 24 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1000 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, 1000U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 1 2 3 4 5 6\r\n" +
                            prefix + "3 1000 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 24 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1000 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            // Direct colour: edges should be black
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, 0x2FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 #2000000 1 2 3 4 5 6\r\n" +
                            prefix + "3 #2FF00FF 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 #2FF00FF 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #2000000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 #2FF00FF 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, 0x2FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 #2000000 1 2 3 4 5 6\r\n" +
                            prefix + "3 #2FF00FF 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 #2FF00FF 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #2000000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 #2FF00FF 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, 0x2FF00FF, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 #2000000 1 2 3 4 5 6\r\n" +
                            prefix + "3 #2FF00FF 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 #2FF00FF 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #2000000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 #2FF00FF 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            // local-palette with an indexed edge
            IElementCollection parent = new MockElementCollection();
            IColour colour            = MocksFactory.CreateMockColour();
            colour.Name               = "colour";
            colour.Code               = 1000U;
            colour.Value              = Color.Red;
            colour.EdgeCode           = 1U;
            parent.Add(colour);
            parent.Add(containingElement);
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, 1000U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 1 1 2 3 4 5 6\r\n" +
                            prefix + "3 1000 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 1 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1000 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, 1000U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 1 1 2 3 4 5 6\r\n" +
                            prefix + "3 #2FF0000 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 #2FF0000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 1 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 #2FF0000 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, 1000U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 1 1 2 3 4 5 6\r\n" +
                            prefix + "3 1000 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 1 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1000 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            // local-palette with a Direct edge
            colour          = MocksFactory.CreateMockColour();
            colour.Name     = "colour";
            colour.Code     = 1001U;
            colour.Value    = Color.Red;
            colour.EdgeCode = 0x200FF00;
            parent.Insert(0, colour);
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, 1001U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 #200FF00 1 2 3 4 5 6\r\n" +
                            prefix + "3 1001 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1001 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #200FF00 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1001 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, 1001U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 #200FF00 1 2 3 4 5 6\r\n" +
                            prefix + "3 #2FF0000 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 #2FF0000 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #200FF00 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 #2FF0000 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, 1001U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 #200FF00 1 2 3 4 5 6\r\n" +
                            prefix + "3 1001 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1001 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #200FF00 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1001 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            // hide an element
            ((IGraphic)collection[0]).IsVisible = false;
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, 1001U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "0 MLCAD HIDE 2 #200FF00 1 2 3 4 5 6\r\n" +
                            prefix + "3 1001 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1001 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #200FF00 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1001 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            // ghost an element
            ((IGraphic)collection[1]).IsGhosted = true;
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, 1001U, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "0 MLCAD HIDE 2 #200FF00 1 2 3 4 5 6\r\n" +
                            prefix + "0 GHOST 3 1001 1 2 3 4 5 6 7 8 9\r\n" +
                            prefix + "4 1001 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "5 #200FF00 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            prefix + "1 1001 0 0 0 1 0 0 0 1 0 0 0 1 test.dat\r\n" +
                            prefix + "2 1 1 0 0 2 0 0\r\n",
                            code.ToString());

            parent.Remove(containingElement);
            collection.Clear();

            // BFC-optimisation
            IPage page       = new LDPage();
            IStep step       = new LDStep();
            IBFCFlag bfcFlag = MocksFactory.CreateMockBFCFlag();
            bfcFlag.Flag     = BFCFlag.SetWindingModeClockwise;
            page.BFC         = CullingMode.CertifiedCounterClockwise;
            page.Name        = "page";
            page.PageType    = PageType.Part;
            page.Add(step);
            step.Add(containingElement);
            line = MocksFactory.CreateMockLine();
            line.Vertex2 = new Vector3d(1, 1, 1);
            collection.Add(line);
            line = MocksFactory.CreateMockLine();
            line.Vertex1 = new Vector3d(2, 2, 2);
            line.Vertex2 = new Vector3d(3, 3, 3);
            collection.Add(line);

            // reverse the winding direction before the second line
            collection.Insert(1, bfcFlag);
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 0 0 0 1 1 1\r\n" +
                            prefix + "0 BFC CW\r\n" +
                            prefix + "2 24 2 2 2 3 3 3\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 0 0 0 1 1 1\r\n" +
                            prefix + "0 BFC CW\r\n" +
                            prefix + "2 24 2 2 2 3 3 3\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 0 0 0 1 1 1\r\n" +
                            prefix + "2 24 3 3 3 2 2 2\r\n",
                            code.ToString());

            // disable winding before the second line
            bfcFlag.Flag = BFCFlag.DisableBackFaceCulling;
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 0 0 0 1 1 1\r\n" +
                            prefix + "0 BFC NOCLIP\r\n" +
                            prefix + "2 24 2 2 2 3 3 3\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 0 0 0 1 1 1\r\n" +
                            prefix + "0 BFC NOCLIP\r\n" +
                            prefix + "2 24 2 2 2 3 3 3\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 0 0 0 1 1 1\r\n" +
                            prefix + "0 BFC NOCLIP\r\n" +
                            prefix + "2 24 2 2 2 3 3 3\r\n",
                            code.ToString());

            // enable winding again, return to counter-clockwise and add a third line
            collection.Add(new LDBFCFlag(BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise));
            line = MocksFactory.CreateMockLine();
            line.Vertex1 = new Vector3d(4, 4, 4);
            line.Vertex2 = new Vector3d(5, 5, 5);
            collection.Add(line);
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 0 0 0 1 1 1\r\n" +
                            prefix + "0 BFC NOCLIP\r\n" +
                            prefix + "2 24 2 2 2 3 3 3\r\n" +
                            prefix + "0 BFC CLIP CCW\r\n" +
                            prefix + "2 24 4 4 4 5 5 5\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 0 0 0 1 1 1\r\n" +
                            prefix + "0 BFC NOCLIP\r\n" +
                            prefix + "2 24 2 2 2 3 3 3\r\n" +
                            prefix + "0 BFC CLIP CCW\r\n" +
                            prefix + "2 24 4 4 4 5 5 5\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(collection.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(prefix + "2 24 0 0 0 1 1 1\r\n" +
                            prefix + "0 BFC NOCLIP\r\n" +
                            prefix + "2 24 2 2 2 3 3 3\r\n" +
                            prefix + "0 BFC CLIP\r\n" +
                            prefix + "2 24 4 4 4 5 5 5\r\n",
                            code.ToString());
        }

        #endregion Code-generation

        #region Collection-management

        public static void ItemsAddedTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.ItemsAddedTest(collection, elementToAdd);

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.ItemsAddedTest() not implemented for immutable collections");
            }
            else
            {
                bool parentChangedEventSeen = false;
                bool itemsAddedEventSeen    = false;

                ObjectChangedEventHandler changedHandler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.AreSame(collection, sender);

                    // both these events should appear, but not necessarily in any particular order
                    switch (e.Operation)
                    {
                        case "ItemsAdded":
                            {
                                Assert.IsFalse(itemsAddedEventSeen);
                                itemsAddedEventSeen = true;

                                Assert.AreSame(collection, e.Source);
                                Assert.IsInstanceOfType(e.Parameters, typeof(UndoableListChangedEventArgs<IElement>));

                                UndoableListChangedEventArgs<IElement> args = (UndoableListChangedEventArgs<IElement>)e.Parameters;
                                Assert.AreEqual(1, args.Count);
                                Assert.AreSame(elementToAdd, args.Items.ElementAt(0));
                            }
                            break;

                        case "ParentChanged":
                            {
                                Assert.IsFalse(parentChangedEventSeen);
                                parentChangedEventSeen = true;

                                Assert.AreSame(elementToAdd, e.Source);
                                Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IElementCollection>));

                                PropertyChangedEventArgs<IElementCollection> args = (PropertyChangedEventArgs<IElementCollection>)e.Parameters;
                                Assert.IsNull(args.OldValue);
                                Assert.AreSame(collection, args.NewValue);
                            }
                            break;

                        default:
                            Assert.Fail(e.Operation);
                            break;
                    }
                };

                collection.Changed += changedHandler;
                collection.Add(elementToAdd);
                collection.Changed -= changedHandler;
                Assert.IsTrue(parentChangedEventSeen);
                Assert.IsTrue(itemsAddedEventSeen);
            }
        }

        public static void ItemsRemovedTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.ItemsRemovedTest(collection, elementToAdd);

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.ItemsRemovedTest() not implemented for immutable collections");
            }
            else
            {
                bool parentChangedEventSeen = false;
                bool itemsRemovedEventSeen  = false;

                collection.Add(elementToAdd);

                ObjectChangedEventHandler changedHandler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.AreSame(collection, sender);

                    // both these events should appear, but not necessarily in any particular order
                    switch (e.Operation)
                    {
                        case "ItemsRemoved":
                            {
                                Assert.IsFalse(itemsRemovedEventSeen);
                                itemsRemovedEventSeen = true;

                                Assert.AreSame(collection, e.Source);
                                Assert.IsInstanceOfType(e.Parameters, typeof(UndoableListChangedEventArgs<IElement>));

                                UndoableListChangedEventArgs<IElement> args = (UndoableListChangedEventArgs<IElement>)e.Parameters;
                                Assert.AreEqual(1, args.Count);
                                Assert.AreSame(elementToAdd, args.Items.ElementAt(0));
                            }
                            break;

                        case "ParentChanged":
                            {
                                Assert.IsFalse(parentChangedEventSeen);
                                parentChangedEventSeen = true;

                                Assert.AreSame(elementToAdd, e.Source);
                                Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IElementCollection>));

                                PropertyChangedEventArgs<IElementCollection> args = (PropertyChangedEventArgs<IElementCollection>)e.Parameters;
                                Assert.AreSame(collection, args.OldValue);
                                Assert.IsNull(args.NewValue);
                            }
                            break;

                        default:
                            Assert.Fail(e.Operation);
                            break;
                    }
                };

                collection.Changed += changedHandler;
                collection.Remove(elementToAdd);
                collection.Changed -= changedHandler;
                Assert.IsTrue(parentChangedEventSeen);
                Assert.IsTrue(itemsRemovedEventSeen);
            }
        }

        public static void ItemsReplacedTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.ItemsReplacedTest(collection, elementToAdd);

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.ItemsReplacedTest() not implemented for immutable collections");
            }
            else
            {
                IElement elementToReplace      = (IElement)elementToAdd.Clone();
                bool oldParentChangedEventSeen = false;
                bool newParentChangedEventSeen = false;
                bool itemsReplacedEventSeen    = false;

                collection.Add(elementToReplace);

                ObjectChangedEventHandler changedHandler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.AreSame(collection, sender);

                    // both these events should appear, but not necessarily in any particular order
                    // ParentChanged should appear twice - once for each element
                    switch (e.Operation)
                    {
                        case "ItemsReplaced":
                            {
                                Assert.IsFalse(itemsReplacedEventSeen);
                                itemsReplacedEventSeen = true;

                                Assert.AreSame(collection, e.Source);
                                Assert.IsInstanceOfType(e.Parameters, typeof(UndoableListReplacedEventArgs<IElement>));

                                UndoableListReplacedEventArgs<IElement> args = (UndoableListReplacedEventArgs<IElement>)e.Parameters;

                                if (0 != args.ItemsAdded.Count)
                                {
                                    Assert.AreEqual(1, args.ItemsAdded.Count);
                                    Assert.AreEqual(0, args.ItemsAdded.FirstIndex);
                                    Assert.AreSame(elementToAdd, args.ItemsAdded.Items.ElementAt(0));
                                }
                                else
                                {
                                    Assert.AreEqual(1, args.ItemsRemoved.Count);
                                    Assert.AreEqual(0, args.ItemsRemoved.FirstIndex);
                                    Assert.AreSame(elementToReplace, args.ItemsRemoved.Items.ElementAt(0));
                                }
                            }
                            break;

                        case "ParentChanged":
                            {
                                Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IElementCollection>));

                                PropertyChangedEventArgs<IElementCollection> args = (PropertyChangedEventArgs<IElementCollection>)e.Parameters;

                                if (e.Source == elementToAdd)
                                {
                                    Assert.IsFalse(newParentChangedEventSeen);
                                    newParentChangedEventSeen = true;

                                    Assert.IsNull(args.OldValue);
                                    Assert.AreSame(collection, args.NewValue);
                                }
                                else if (e.Source == elementToReplace)
                                {
                                    Assert.IsFalse(oldParentChangedEventSeen);
                                    oldParentChangedEventSeen = true;

                                    Assert.AreSame(collection, args.OldValue);
                                    Assert.IsNull(args.NewValue);
                                }
                                else
                                {
                                    Assert.Fail("Unexpected element seen during a collection-replace operation");
                                }
                            }
                            break;

                        default:
                            Assert.Fail(e.Operation);
                            break;
                    }
                };

                collection.Changed += changedHandler;
                collection[0] = elementToAdd;
                collection.Changed -= changedHandler;
                Assert.IsTrue(oldParentChangedEventSeen);
                Assert.IsTrue(newParentChangedEventSeen);
                Assert.IsTrue(itemsReplacedEventSeen);
                collection.Clear();
            }
        }

        public static void CollectionClearedTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.CollectionClearedTest(collection, elementToAdd);

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.CollectionClearedTest() not implemented for immutable collections");
            }
            else
            {
                bool parentChangedEventSeen     = false;
                bool collectionClearedEventSeen = false;

                collection.Add(elementToAdd);

                ObjectChangedEventHandler changedHandler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.AreSame(collection, sender);

                    // both these events should appear, but not necessarily in any particular order
                    switch (e.Operation)
                    {
                        case "CollectionCleared":
                            {
                                Assert.IsFalse(collectionClearedEventSeen);
                                collectionClearedEventSeen = true;

                                Assert.AreSame(collection, e.Source);
                                Assert.IsInstanceOfType(e.Parameters, typeof(UndoableListChangedEventArgs<IElement>));

                                UndoableListChangedEventArgs<IElement> args = (UndoableListChangedEventArgs<IElement>)e.Parameters;
                                Assert.AreEqual(1, args.Count);
                                Assert.AreSame(elementToAdd, args.Items.ElementAt(0));
                            }
                            break;

                        case "ParentChanged":
                            {
                                Assert.IsFalse(parentChangedEventSeen);
                                parentChangedEventSeen = true;

                                Assert.AreSame(elementToAdd, e.Source);
                                Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IElementCollection>));

                                PropertyChangedEventArgs<IElementCollection> args = (PropertyChangedEventArgs<IElementCollection>)e.Parameters;
                                Assert.AreSame(collection, args.OldValue);
                                Assert.IsNull(args.NewValue);
                            }
                            break;

                        default:
                            Assert.Fail(e.Operation);
                            break;
                    }
                };

                collection.Changed += changedHandler;
                collection.Clear();
                collection.Changed -= changedHandler;
                Assert.IsTrue(parentChangedEventSeen);
                Assert.IsTrue(collectionClearedEventSeen);
            }
        }

        public static void CanInsertTest(IElementCollection collection, IElement element, IElement elementToCheck)
        {
            IDOMObjectCollectionTest.CanInsertTest(collection, (IElementCollection)collection.Clone(), (IElement)elementToCheck.Clone());

            CanInsertOrReplaceTest(collection,
                                   element,
                                   elementToCheck,
                                   null,
                                   delegate(IElement elementToInsert, IElement elementToReplace, InsertCheckFlags flags)
                                        { return collection.CanInsert(elementToInsert, flags); });
        }

        public static void CanReplaceTest(IElementCollection collection, IElement element, IElement elementToCheck)
        {
            IElementCollection collection2 = (IElementCollection)collection.Clone();
            IDOMObjectCollectionTest.CanReplaceTest(collection, collection2, (IElement)elementToCheck.Clone(), (IElement)elementToCheck.Clone());

            CanInsertOrReplaceTest(collection,
                                   element,
                                   elementToCheck,
                                   (IElement)elementToCheck.Clone(),
                                   delegate(IElement elementToInsert, IElement elementToReplace, InsertCheckFlags flags)
                                        { return collection.CanReplace(elementToInsert, elementToReplace, flags); });
        }

        private delegate InsertCheckResult CanInsertOrReplaceFunction(IElement elementToInsert, IElement elementToReplace, InsertCheckFlags flags);

        private static void CanInsertOrReplaceTest(IElementCollection collection, IElement element, IElement elementToCheck, IElement elementToReplace, CanInsertOrReplaceFunction function)
        {
            if (collection.IsImmutable || collection.IsReadOnly)
            {
                Assert.AreEqual(InsertCheckResult.NotSupported, function(elementToCheck, elementToReplace, InsertCheckFlags.None));
            }
            else
            {
                if (null != elementToReplace)
                    collection.Add(elementToReplace);

                // Direct Colours IColour elements cannot be added
                IColour colour = MocksFactory.CreateMockColour();
                colour.Code    = 0x2123456;
                Assert.AreEqual(InsertCheckResult.NotSupported, function(colour, elementToReplace, InsertCheckFlags.None));

                // but regular ones should work
                colour.Code = 1U;
                Assert.AreEqual(InsertCheckResult.CanInsert, function(colour, elementToReplace, InsertCheckFlags.None));

                // locked elements can be added
                elementToCheck.IsLocked = true;
                Assert.AreEqual(InsertCheckResult.CanInsert, function(elementToCheck, elementToReplace, InsertCheckFlags.None));
                elementToCheck.IsLocked = false;

                // cannot add TopLevel elements unless the collection supports them
                if (collection.AllowsTopLevelElements)
                {
                    Assert.AreEqual(InsertCheckResult.CanInsert, function(MocksFactory.CreateMockGroup(), elementToReplace, InsertCheckFlags.None));

                    // IGroups must have unique names within the containing IPage
                    IPage page = MocksFactory.CreateMockPage();
                    IStep step;

                    if (DOMObjectType.Step == collection.ObjectType)
                    {
                        step = (IStep)collection;
                    }
                    else
                    {
                        step = new LDStep();
                        step.Add(element);
                    }

                    page.Add(step);

                    IGroup group1 = MocksFactory.CreateMockGroup();
                    group1.Name = "group";
                    Assert.AreEqual(InsertCheckResult.CanInsert, function(group1, elementToReplace, InsertCheckFlags.None));
                    step.Add(group1);
                    IGroup group2 = MocksFactory.CreateMockGroup();
                    group2.Name = "group";
                    Assert.AreEqual(InsertCheckResult.DuplicateName, function(group2, elementToReplace, InsertCheckFlags.None));

                    // group names are case-sensitive
                    group2.Name = "Group";
                    Assert.AreEqual(InsertCheckResult.CanInsert, function(group2, elementToReplace, InsertCheckFlags.None));

                    if (DOMObjectType.Step == collection.ObjectType)
                        step.Page = null;
                    else
                        element.Parent = null;
                }
                else
                {
                    Assert.AreEqual(InsertCheckResult.TopLevelElementNotAllowed, function(MocksFactory.CreateMockGroup(), elementToReplace, InsertCheckFlags.None));
                }

                CanInsertOrReplaceCircularRefTest(collection, element, elementToCheck, elementToReplace, function);

                // cannot add to a locked collection
                collection.IsLocked = true;
                Assert.AreEqual(InsertCheckResult.NotSupported, function(elementToCheck, elementToReplace, InsertCheckFlags.None));
                // but if we override this it should work
                Assert.AreEqual(InsertCheckResult.CanInsert, function(elementToCheck, elementToReplace, InsertCheckFlags.IgnoreIsLocked));

                // cannot add to a frozen collection
                collection.Freeze();
                Assert.IsTrue(collection.IsFrozen);
                Assert.AreEqual(InsertCheckResult.NotSupported, function(elementToCheck, elementToReplace, InsertCheckFlags.None));

                Utils.DisposalAccessTest(collection, delegate() { InsertCheckResult result = function(elementToCheck, elementToReplace, InsertCheckFlags.None); });
            }
        }

        private static void CanInsertOrReplaceCircularRefTest(IElementCollection collection, IElement element, IElement elementToCheck, IElement elementToReplace, CanInsertOrReplaceFunction function)
        {
            IPage pageA = MocksFactory.CreateMockPage();
            IStep stepA;

            if (DOMObjectType.Step == collection.ObjectType)
            {
                stepA = (IStep)collection;
            }
            else
            {
                stepA = MocksFactory.CreateMockStep();
                stepA.Add(element);
            }

            pageA.Add(stepA);
            pageA.Name = "PageA";

            IPage pageB = MocksFactory.CreateMockPage();
            IStep stepB = MocksFactory.CreateMockStep();
            pageB.Add(stepB);
            pageB.Name = "PageB";

            IPage pageC = MocksFactory.CreateMockPage();
            IStep stepC = MocksFactory.CreateMockStep();
            pageC.Add(stepC);
            pageC.Name = "PageC";

            IDocument docA = MocksFactory.CreateMockDocument();
            docA.Add(pageA);
            docA.Add(pageB);
            docA.Add(pageC);

            // cannot add an IReference which points back to the containing IPage
            IReference reference = MocksFactory.CreateMockReference();
            reference.TargetName = pageA.TargetName;
            Assert.AreEqual(InsertCheckResult.CircularReference, function(reference, elementToReplace, InsertCheckFlags.None));

            // but an IReference pointing to an unrelated page can be added
            reference.TargetName = pageB.TargetName;
            Assert.AreEqual(InsertCheckResult.CanInsert, function(reference, elementToReplace, InsertCheckFlags.None));

            // circular-reference: A->B->A
            reference.TargetName = pageA.TargetName;
            stepB.Add(reference);
            reference = MocksFactory.CreateMockReference();
            reference.TargetName = pageB.TargetName;
            Assert.AreEqual(InsertCheckResult.CircularReference, function(reference, elementToReplace, InsertCheckFlags.None));

            // circular-reference: A->C->B->A
            stepC.Add(reference);
            reference = MocksFactory.CreateMockReference();
            reference.TargetName = pageC.TargetName;
            Assert.AreEqual(InsertCheckResult.CircularReference, function(reference, elementToReplace, InsertCheckFlags.None));

            // circular-reference: a collection cannot be added to itself
            if (null != element)
                Assert.AreEqual(InsertCheckResult.NotSupported, function(element, elementToReplace, InsertCheckFlags.None));

            // should not be possible to add or alter an IReference to point at its containing document
            // TODO: tests shouldn't be using implementation types or referencing specific filesystem paths
            docA = new LDDocument(Path.Combine(Configuration.LDrawBase, @"parts\3001.dat"), ParseFlags.None);
            pageA = docA[0];

            if (DOMObjectType.Step == collection.ObjectType)
            {
                stepA.Page = null;
                stepA.Page = pageA;
            }
            else
            {
                element.Parent = null;
                element.Parent = pageA[0];
            }

            reference = MocksFactory.CreateMockReference();
            reference.TargetName = "3001.dat";
            Assert.AreEqual(InsertCheckResult.CircularReference, function(reference, elementToReplace, InsertCheckFlags.None));
            reference.TargetName = "foo.dat";
            Assert.AreEqual(InsertCheckResult.CanInsert, function(reference, elementToReplace, InsertCheckFlags.None));
            pageA[0].Add(reference);
            reference.TargetName = "3001.dat";
            Assert.IsNull(reference.Target);
            Assert.AreEqual(TargetStatus.CircularReference, reference.TargetStatus);

            // same as above but with an absolute filepath
            // TODO: tests shouldn't be using implementation types or referencing specific filesystem paths
            string path = @"E:\Development\Digitalis\Digitalis.LDTools.DOM\Test Data\Test.MLCadGroup.ldr";
            docA = new LDDocument(Path.Combine(Configuration.LDrawBase, path), ParseFlags.None);
            pageA = docA[0];

            if (DOMObjectType.Step == collection.ObjectType)
            {
                stepA.Page = null;
                stepA.Page = pageA;
            }
            else
            {
                element.Parent = null;
                element.Parent = pageA[0];
            }

            reference = MocksFactory.CreateMockReference();
            reference.TargetName = path;
            Assert.AreEqual(InsertCheckResult.CircularReference, function(reference, elementToReplace, InsertCheckFlags.None));
            reference.TargetName = "foo.dat";
            Assert.AreEqual(InsertCheckResult.CanInsert, function(reference, elementToReplace, InsertCheckFlags.None));
            pageA[0].Add(reference);
            reference.TargetName = path;
            Assert.IsNull(reference.Target);
            Assert.AreEqual(TargetStatus.CircularReference, reference.TargetStatus);

            if (DOMObjectType.Step == collection.ObjectType)
                stepA.Page = null;
            else
                element.Parent = null;
        }

        public static void ContainsColourElementsTest(IElementCollection collection)
        {
            Assert.IsFalse(collection.ContainsColourElements);

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("ITexmapGeometryTest.ContainsColourElementsTest() not implemented for immutable/read-only objects");
            }
            else
            {
                collection.Add(MocksFactory.CreateMockLine());
                Assert.IsFalse(collection.ContainsColourElements);

                collection.Add(MocksFactory.CreateMockColour());
                Assert.IsTrue(collection.ContainsColourElements);
            }

            Utils.DisposalAccessTest(collection, delegate() { bool result = collection.ContainsColourElements; });
        }

        public static void ContainsBFCFlagElementsTest(IElementCollection collection)
        {
            Assert.IsFalse(collection.ContainsBFCFlagElements);

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.ContainsBFCFlagElementsTest() not implemented for immutable/read-only objects");
            }
            else
            {
                collection.Add(MocksFactory.CreateMockLine());
                Assert.IsFalse(collection.ContainsBFCFlagElements);

                collection.Add(MocksFactory.CreateMockBFCFlag());
                Assert.IsTrue(collection.ContainsBFCFlagElements);
            }

            Utils.DisposalAccessTest(collection, delegate() { bool result = collection.ContainsBFCFlagElements; });
        }

        public static void HasLockedDescendantsTest(IElementCollection collection)
        {
            Assert.IsFalse(collection.HasLockedDescendants);

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.HasLockedDescendantsTest() not implemented for immutable/read-only objects");
            }
            else
            {
                ILine line = MocksFactory.CreateMockLine();
                collection.Add(line);
                Assert.IsFalse(collection.HasLockedDescendants);
                line.IsLocked = true;
                Assert.IsTrue(collection.HasLockedDescendants);
            }

            Utils.DisposalAccessTest(collection, delegate() { bool result = collection.HasLockedDescendants; });
        }

        public static void IsReadOnlyTest(IElementCollection collection)
        {
            if (collection.IsImmutable)
                Assert.IsTrue(collection.IsReadOnly);
        }

        public static void CountTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.CountTest(collection, elementToAdd);

            if (collection.IsImmutable)
                throw new NotImplementedException("IElementCollectionTest.CountTest() not implemented for immutable collections");

            Utils.DisposalAccessTest(collection, delegate() { int count = collection.Count; });
        }

        public static void IndexOfTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.IndexOfTest(collection, elementToAdd);

            if (collection.IsImmutable)
                throw new NotImplementedException("IElementCollectionTest.IndexOfTest() not implemented for immutable collections");

            Utils.DisposalAccessTest(collection, delegate() { int idx = collection.IndexOf(elementToAdd); });
        }

        public static void ContainsTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.ContainsTest(collection, elementToAdd);

            if (collection.IsImmutable)
                throw new NotImplementedException("IElementCollectionTest.ContainsTest() not implemented for immutable collections");

            Utils.DisposalAccessTest(collection, delegate() { bool contains = collection.Contains(elementToAdd); });
        }

        public static void CheckCollectionEvents(IElementCollection collection, IElement element)
        {
            bool elementIsMember = collection.Contains(element);

            // the collection should raise Changed when the element changes in some way
            bool treeChangedEventSeen = false;

            ObjectChangedEventHandler treeChangedHandler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.IsFalse(treeChangedEventSeen);
                treeChangedEventSeen = true;
                Assert.AreSame(collection, sender);
            };

            collection.Changed += treeChangedHandler;
            element.IsLocked = !element.IsLocked;
            collection.Changed -= treeChangedHandler;

            if (elementIsMember)
                Assert.IsTrue(treeChangedEventSeen, "Changing " + element.GetType().FullName + " after adding did not trigger Changed in " + collection.GetType().FullName);
            else
                Assert.IsFalse(treeChangedEventSeen, "Changing " + element.GetType().FullName + " after removal triggered Changed in " + collection.GetType().FullName);

            // the element should raise PathToDocumentChanged event when its containing collection's path changes
            bool pathChangedEventSeen = false;

            EventHandler pathChangedHandler = delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(pathChangedEventSeen);
                pathChangedEventSeen = true;
                Assert.AreSame(element, sender);
            };

            switch (collection.ObjectType)
            {
                case DOMObjectType.Page:
                    IDocument document = MocksFactory.CreateMockDocument();
                    element.PathToDocumentChanged += pathChangedHandler;
                    document.Add((IPage)collection);
                    element.PathToDocumentChanged -= pathChangedHandler;

                    if (elementIsMember)
                        Assert.IsTrue(pathChangedEventSeen, "Changing the document-path of " + collection.GetType().FullName + " did not trigger PathToDocumentChanged in child " + element.GetType().FullName);
                    else
                        Assert.IsFalse(pathChangedEventSeen, "Changing the document-path of " + collection.GetType().FullName + " triggered PathToDocumentChanged in removed " + element.GetType().FullName);

                    document.Clear();
                    break;

                case DOMObjectType.Step:
                    IPage page = MocksFactory.CreateMockPage();
                    element.PathToDocumentChanged += pathChangedHandler;
                    page.Add((IStep)collection);
                    element.PathToDocumentChanged -= pathChangedHandler;

                    if (elementIsMember)
                        Assert.IsTrue(pathChangedEventSeen, "Changing the document-path of " + collection.GetType().FullName + " did not trigger PathToDocumentChanged in child " + element.GetType().FullName);
                    else
                        Assert.IsFalse(pathChangedEventSeen, "Changing the document-path of " + collection.GetType().FullName + " triggered PathToDocumentChanged in removed " + element.GetType().FullName);

                    page.Clear();
                    break;

                default:
                    if (collection is IElement)
                    {
                        IStep step = MocksFactory.CreateMockStep();
                        element.PathToDocumentChanged += pathChangedHandler;
                        step.Add((IElement)collection);
                        element.PathToDocumentChanged -= pathChangedHandler;

                        if (elementIsMember)
                            Assert.IsTrue(pathChangedEventSeen, "Changing the document-path of " + collection.GetType().FullName + " did not trigger PathToDocumentChanged in child " + element.GetType().FullName);
                        else
                            Assert.IsFalse(pathChangedEventSeen, "Changing the document-path of " + collection.GetType().FullName + " triggered PathToDocumentChanged in removed " + element.GetType().FullName);

                        step.Clear();
                    }
                    break;
            }
        }

        public static void IndexerTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.IndexerTest(collection, elementToAdd);

            IElement newElement = (IElement)elementToAdd.Clone();

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                int count = collection.Count;

                try
                {
                    collection[0] = newElement;
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsFalse(collection.Contains(newElement));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(NotSupportedException), e.GetType());
                }
            }
            else
            {
                // basic operation
                collection[0] = newElement;
                Assert.IsTrue(collection.Contains(newElement));
                Assert.AreSame(collection, newElement.Parent);

                CheckCollectionEvents(collection, newElement);

                // TODO: CanReplace checks

                // cannot add to a locked collection
                collection.IsLocked = true;

                try
                {
                    collection[0] = elementToAdd;
                    Assert.Fail();
                }
                catch (ElementLockedException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.AreSame(newElement, collection[0]);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                // cannot add to a frozen collection
                collection.Freeze();
                Assert.IsTrue(collection.IsFrozen);

                try
                {
                    collection[0] = elementToAdd;
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.AreSame(newElement, collection[0]);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // cannot add to a disposed collection
                Utils.DisposalAccessTest(collection, delegate() { collection[0] = elementToAdd; });
            }
        }

        public static void AddTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.AddTest(collection, elementToAdd);

            IElement newElement = (IElement)elementToAdd.Clone();

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                Assert.AreEqual(InsertCheckResult.NotSupported, collection.CanInsert(elementToAdd, InsertCheckFlags.None));

                int count = collection.Count;

                try
                {
                    collection.Add(newElement);
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsFalse(collection.Contains(newElement));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(NotSupportedException), e.GetType());
                }
            }
            else
            {
                // basic operation
                collection.Add(newElement);
                Assert.IsTrue(collection.Contains(newElement));
                Assert.AreSame(collection, newElement.Parent);

                CheckCollectionEvents(collection, newElement);

                // TODO: CanInsert checks

                // cannot add to a locked collection
                collection.Clear();
                collection.IsLocked = true;

                try
                {
                    collection.Add(newElement);
                    Assert.Fail();
                }
                catch (ElementLockedException)
                {
                    Assert.AreEqual(0, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                // cannot add to a frozen collection
                collection.Freeze();
                Assert.IsTrue(collection.IsFrozen);

                try
                {
                    collection.Add(newElement);
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    Assert.AreEqual(0, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // cannot add to a disposed collection
                Utils.DisposalAccessTest(collection, delegate() { collection.Add(elementToAdd); });
            }
        }

        public static void InsertTest(IElementCollection collection, IElement elementToInsert)
        {
            IDOMObjectCollectionTest.InsertTest(collection, elementToInsert);

            IElement newElement = (IElement)elementToInsert.Clone();

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                int count = collection.Count;

                try
                {
                    collection.Insert(0, newElement);
                    Assert.Fail();
                }
                catch (NotSupportedException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsFalse(collection.Contains(newElement));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(NotSupportedException), e.GetType());
                }
            }
            else
            {
                // basic operation
                collection.Insert(0, newElement);
                Assert.IsTrue(collection.Contains(newElement));
                Assert.AreSame(collection, newElement.Parent);

                CheckCollectionEvents(collection, newElement);

                // TODO: CanInsert checks

                // cannot add to a locked collection
                collection.Clear();
                collection.IsLocked = true;

                try
                {
                    collection.Insert(0, newElement);
                    Assert.Fail();
                }
                catch (ElementLockedException)
                {
                    Assert.AreEqual(0, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                // cannot add to a frozen collection
                collection.Freeze();
                Assert.IsTrue(collection.IsFrozen);

                try
                {
                    collection.Insert(0, newElement);
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    Assert.AreEqual(0, collection.Count);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // cannot add to a disposed collection
                Utils.DisposalAccessTest(collection, delegate() { collection.Insert(0, newElement); });
            }
        }

        public static void RemoveTest(IElementCollection collection, IElement elementToRemove)
        {
            IDOMObjectCollectionTest.RemoveTest(collection, elementToRemove);

            IElement newElement = (IElement)elementToRemove.Clone();

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.RemoveTest() not implemented for immutable collections");
            }
            else
            {
                // basic operation
                collection.Add(elementToRemove);
                Assert.IsTrue(collection.Contains(elementToRemove));
                Assert.AreSame(collection, elementToRemove.Parent);
                collection.Remove(elementToRemove);
                Assert.IsFalse(collection.Contains(elementToRemove));
                Assert.IsNull(elementToRemove.Parent);

                CheckCollectionEvents(collection, elementToRemove);

                // cannot remove from a locked collection
                collection.Add(elementToRemove);
                collection.IsLocked = true;

                try
                {
                    collection.Remove(elementToRemove);
                    Assert.Fail();
                }
                catch (ElementLockedException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsTrue(collection.Contains(elementToRemove));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                // cannot remove from a frozen collection
                collection.Freeze();

                try
                {
                    collection.Remove(elementToRemove);
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsTrue(collection.Contains(elementToRemove));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // cannot remove from a disposed collection
                Utils.DisposalAccessTest(collection, delegate() { collection.Remove(elementToRemove); });
            }
        }

        public static void RemoveAtTest(IElementCollection collection, IElement elementToRemove)
        {
            IDOMObjectCollectionTest.RemoveAtTest(collection, elementToRemove);

            IElement newElement = (IElement)elementToRemove.Clone();

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.RemoveAtTest() not implemented for immutable collections");
            }
            else
            {
                // basic operation
                collection.Add(elementToRemove);
                Assert.IsTrue(collection.Contains(elementToRemove));
                Assert.AreSame(collection, elementToRemove.Parent);
                collection.RemoveAt(0);
                Assert.IsFalse(collection.Contains(elementToRemove));
                Assert.IsNull(elementToRemove.Parent);

                CheckCollectionEvents(collection, elementToRemove);

                // cannot remove from a locked collection
                collection.Add(elementToRemove);
                collection.IsLocked = true;

                try
                {
                    collection.RemoveAt(0);
                    Assert.Fail();
                }
                catch (ElementLockedException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsTrue(collection.Contains(elementToRemove));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                // cannot remove from a frozen collection
                collection.Freeze();

                try
                {
                    collection.RemoveAt(0);
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsTrue(collection.Contains(elementToRemove));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // cannot remove from a disposed collection
                Utils.DisposalAccessTest(collection, delegate() { collection.RemoveAt(0); });
            }
        }

        public static void ClearTest(IElementCollection collection, IElement elementToRemove)
        {
            IDOMObjectCollectionTest.ClearTest(collection, elementToRemove);

            IElement newElement = (IElement)elementToRemove.Clone();

            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.ClearTest() not implemented for immutable collections");
            }
            else
            {
                // basic operation
                collection.Add(elementToRemove);
                Assert.IsTrue(collection.Contains(elementToRemove));
                Assert.AreSame(collection, elementToRemove.Parent);
                collection.Clear();
                Assert.IsFalse(collection.Contains(elementToRemove));
                Assert.IsNull(elementToRemove.Parent);

                CheckCollectionEvents(collection, elementToRemove);

                // cannot clear a locked collection
                collection.Add(elementToRemove);
                collection.IsLocked = true;

                try
                {
                    collection.Clear();
                    Assert.Fail();
                }
                catch (ElementLockedException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsTrue(collection.Contains(elementToRemove));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                // cannot clear a frozen collection
                collection.Freeze();

                try
                {
                    collection.Clear();
                    Assert.Fail();
                }
                catch (ObjectFrozenException)
                {
                    Assert.AreEqual(1, collection.Count);
                    Assert.IsTrue(collection.Contains(elementToRemove));
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
                }

                // cannot clear a disposed collection
                Utils.DisposalAccessTest(collection, delegate() { collection.Clear(); });
            }
        }

        public static void CopyToTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.CopyToTest(collection, elementToAdd);

            if (collection.IsImmutable)
                throw new NotImplementedException("IElementCollectionTest.CopyToTest() not implemented for immutable collections");

            Utils.DisposalAccessTest(collection, delegate() { collection.CopyTo(new IElement[0], 0); });
        }

        public static void GetEnumeratorTest(IElementCollection collection, IElement elementToAdd)
        {
            IDOMObjectCollectionTest.GetEnumeratorTest(collection, elementToAdd);

            if (collection.IsImmutable)
                throw new NotImplementedException("IElementCollectionTest.GetEnumeratorTest() not implemented for immutable collections");

            Utils.DisposalAccessTest(collection, delegate() { foreach (IElement element in collection); });
        }

        #endregion Collection-management

        #region Disposal

        public static void DisposeTest(IElementCollection collection)
        {
            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.DisposeTest() not implemented for immutable/read-only collections");
            }
            else
            {
                ILine line = MocksFactory.CreateMockLine();
                collection.Add(line);
                collection.Dispose();
                Assert.IsTrue(collection.IsDisposed);
                Assert.IsTrue(line.IsDisposed);
            }
        }

        #endregion Disposal

        #region Document-tree

        public static void PathToDocumentChangedTest(IElementCollection collection, IElement containingElement)
        {
            bool eventSeen = false;

            collection.PathToDocumentChanged += delegate(object sender, EventArgs e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(collection, sender);
            };

            IStep step;

            if (DOMObjectType.Step == collection.ObjectType)
            {
                step = (IStep)collection;
            }
            else
            {
                step = MocksFactory.CreateMockStep();
                step.Add(containingElement);
                Assert.IsTrue(eventSeen);
                eventSeen = false;
            }

            IPage page = MocksFactory.CreateMockPage();
            page.Add(step);
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            IDocument document = MocksFactory.CreateMockDocument();
            document.Add(page);
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            page.Remove(step);
            Assert.IsTrue(eventSeen);
            eventSeen = false;

            if (null != containingElement)
            {
                step.Remove(containingElement);
                Assert.IsTrue(eventSeen);
            }
        }

        #endregion Document-tree

        #region Geometry

        private static void CreateGeometry(IElementCollection collection)
        {
            Random random = new Random();
            double x1     = 1.0;
            double y1     = 2.0;
            double z1     = 3.0;
            double x2     = 4.0;
            double y2     = 5.0;
            double z2     = 6.0;

            for (int i = 0; i < 10; i++)
            {
                x1 *= random.NextDouble();
                y1 *= random.NextDouble();
                z1 *= random.NextDouble();
                x2 *= random.NextDouble();
                y2 *= random.NextDouble();
                z2 *= random.NextDouble();

                ILine line = MocksFactory.CreateMockLine();
                line.Vertex1 = new Vector3d(x1, y1, z1);
                line.Vertex2 = new Vector3d(x2, y2, z2);
                collection.Add(line);
            }
        }

        public static void BoundingBoxTest(IElementCollection collection)
        {
            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.BoundingBoxTest() not implemented for immutable collections");
            }
            else
            {
                Box3d expectedBounds;
                CreateGeometry(collection);

                expectedBounds = ((IGraphic)collection[0]).BoundingBox;

                for (int i = 1; i < collection.Count; i++)
                {
                    expectedBounds.Union(((IGraphic)collection[i]).BoundingBox);
                }

                Assert.AreEqual(expectedBounds, collection.BoundingBox);
                collection.Clear();
            }

            IGeometricTest.BoundingBoxTest(collection, collection);
        }

        public static void OriginTest(IElementCollection collection)
        {
            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.OriginTest() not implemented for immutable collections");
            }
            else
            {
                CreateGeometry(collection);
                Assert.AreEqual(collection.BoundingBox.Centre, collection.Origin);
                collection.Clear();
            }

            IGeometricTest.OriginTest(collection, collection);
        }

        public static void WindingModeTest(IElementCollection collection, IElement containingElement)
        {
            IGeometricTest.WindingModeTest(collection, containingElement);
        }

        public static void TransformTest(IElementCollection collection)
        {
            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.TransformTest() not implemented for immutable collections");
            }
            else
            {
                CreateGeometry(collection);

                Matrix4d transform = Matrix4d.Scale(1.0, 2.0, 3.0);
                IGraphic[] oldValues = new IGraphic[collection.Count];
                IGraphic[] newValues = new IGraphic[collection.Count];

                for (int i = 0; i < collection.Count; i++)
                {
                    oldValues[i] = (IGraphic)collection[i].Clone();
                    newValues[i] = (IGraphic)collection[i].Clone();
                    newValues[i].Transform(ref transform);
                }

                for (int i = 0; i < collection.Count; i++)
                {
                    Assert.IsTrue(oldValues[i].IsDuplicateOf((IGraphic)collection[i]));
                    Assert.IsFalse(newValues[i].IsDuplicateOf((IGraphic)collection[i]));
                }

                collection.Transform(ref transform);

                for (int i = 0; i < collection.Count; i++)
                {
                    Assert.IsFalse(oldValues[i].IsDuplicateOf((IGraphic)collection[i]));
                    Assert.IsTrue(newValues[i].IsDuplicateOf((IGraphic)collection[i]));
                }

                for (int i = 0; i < collection.Count; i++)
                {
                    ((IGraphic)collection[i]).Coordinates = oldValues[i].Coordinates;
                }

                // undo/redo
                UndoStack undoStack = new UndoStack();
                undoStack.StartCommand("command");
                collection.Transform(ref transform);
                undoStack.EndCommand();
                for (int i = 0; i < collection.Count; i++)
                {
                    Assert.IsFalse(oldValues[i].IsDuplicateOf((IGraphic)collection[i]));
                    Assert.IsTrue(newValues[i].IsDuplicateOf((IGraphic)collection[i]));
                }
                undoStack.Undo();
                for (int i = 0; i < collection.Count; i++)
                {
                    Assert.IsTrue(oldValues[i].IsDuplicateOf((IGraphic)collection[i]));
                    Assert.IsFalse(newValues[i].IsDuplicateOf((IGraphic)collection[i]));
                }
                undoStack.Redo();
                for (int i = 0; i < collection.Count; i++)
                {
                    Assert.IsFalse(oldValues[i].IsDuplicateOf((IGraphic)collection[i]));
                    Assert.IsTrue(newValues[i].IsDuplicateOf((IGraphic)collection[i]));
                }

                // a collection with locked descendants cannot be transformed
                collection[0].IsLocked = true;

                try
                {
                    collection.Transform(ref transform);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                collection[0].IsLocked = false;
            }

            IGeometricTest.TransformTest(collection, collection, ref Matrix4d.Identity);
        }

        public static void ReverseWindingTest(IElementCollection collection)
        {
            if (collection.IsImmutable || collection.IsReadOnly)
            {
                throw new NotImplementedException("IElementCollectionTest.ReverseWindingTest() not implemented for immutable collections");
            }
            else
            {
                CreateGeometry(collection);

                IGraphic[] oldValues = new IGraphic[collection.Count];
                IGraphic[] newValues = new IGraphic[collection.Count];

                for (int i = 0; i < collection.Count; i++)
                {
                    oldValues[i] = (IGraphic)collection[i].Clone();
                    newValues[i] = (IGraphic)collection[i].Clone();
                    newValues[i].ReverseWinding();
                }

                for (int i = 0; i < collection.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                        Assert.AreNotEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                    }
                }

                collection.ReverseWinding();

                for (int i = 0; i < collection.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreNotEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                        Assert.AreEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                    }
                }

                for (int i = 0; i < collection.Count; i++)
                {
                    ((IGraphic)collection[i]).Coordinates = oldValues[i].Coordinates;
                }

                // undo/redo
                UndoStack undoStack = new UndoStack();
                undoStack.StartCommand("command");
                collection.ReverseWinding();
                undoStack.EndCommand();
                for (int i = 0; i < collection.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreNotEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                        Assert.AreEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                    }
                }
                undoStack.Undo();
                for (int i = 0; i < collection.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                        Assert.AreNotEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                    }
                }
                undoStack.Redo();
                for (int i = 0; i < collection.Count; i++)
                {
                    for (int n = 0; n < newValues[i].CoordinatesCount; n++)
                    {
                        Assert.AreNotEqual(oldValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                        Assert.AreEqual(newValues[i].Coordinates.ElementAt(n), ((IGraphic)collection[i]).Coordinates.ElementAt(n));
                    }
                }

                // a collection with locked descendants cannot be transformed
                collection[0].IsLocked = true;

                try
                {
                    collection.ReverseWinding();
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ElementLockedException), e.GetType());
                }

                collection[0].IsLocked = false;
            }

            IGeometricTest.ReverseWindingTest(collection, collection);
        }

        #endregion Geometry
    }
}
