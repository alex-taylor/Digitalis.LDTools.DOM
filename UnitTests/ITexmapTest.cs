#region License

//
// ITexmapTest.cs
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
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    [TestClass]
    public abstract class ITexmapTest : IGraphicTest
    {
        #region Infrastructure

        protected override Type InterfaceType { get { return typeof(ITexmap); } }

        protected sealed override IGraphic CreateTestGraphic()
        {
            return CreateTestTexmap();
        }

        protected sealed override IGraphic CreateTestGraphicWithCoordinates()
        {
            return CreateTestTexmap();
        }

        protected sealed override IGraphic CreateTestGraphicWithColour()
        {
            return null;
        }

        protected sealed override IGraphic CreateTestGraphicWithNoColour()
        {
            return CreateTestTexmap();
        }

        protected abstract ITexmap CreateTestTexmap();

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.AreEqual(DOMObjectType.Texmap, texmap.ObjectType);
            Assert.IsFalse(texmap.IsStateElement);
            Assert.IsFalse(texmap.IsTopLevelElement);
            Assert.IsFalse(texmap.AllowsTopLevelElements);
            Assert.IsTrue(texmap.IsReadOnly);
            Assert.IsFalse(texmap.ColourValueEnabled);
            Assert.AreEqual(3U, texmap.CoordinatesCount);
            base.DefinitionTest();
        }

        #endregion Definition Test

        #region Analytics

        [TestMethod]
        public sealed override void IsDuplicateOfTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.IsFalse(texmap.IsDuplicateOf(texmap));
            Assert.IsFalse(texmap.IsDuplicateOf(CreateTestTexmap()));

            Utils.DisposalAccessTest(texmap, delegate() { bool isDuplicate = texmap.IsDuplicateOf(texmap); });
        }

        #endregion Analytics

        #region Attributes

        [TestMethod]
        public sealed override void IsVisibleTest()
        {
            ITexmap texmap = CreateTestTexmap();

            // an empty texmap is visible
            Assert.IsTrue(texmap.IsVisible);

            ILine line = MocksFactory.CreateMockLine();
            texmap.TextureGeometry.Add(line);
            Assert.IsTrue(line.IsVisible);
            Assert.IsTrue(texmap.IsVisible);

            // hiding the line should cause the ITexmap to become invisible
            line.IsVisible = false;
            Assert.IsFalse(texmap.IsVisible);
            line.IsVisible = true;
            Assert.IsTrue(texmap.IsVisible);

            // similarly, hiding the ITexmap should affect the line
            texmap.IsVisible = false;
            Assert.IsFalse(line.IsVisible);
            texmap.IsVisible = true;
            Assert.IsTrue(line.IsVisible);

            // it also shouldn't matter which geometry the elements are in
            ITriangle triangle = MocksFactory.CreateMockTriangle();
            texmap.FallbackGeometry.Add(triangle);
            Assert.IsTrue(texmap.IsVisible);

            // hiding one but not the other should leave the texmap visible
            triangle.IsVisible = false;
            Assert.IsTrue(line.IsVisible);
            Assert.IsFalse(triangle.IsVisible);
            Assert.IsTrue(texmap.IsVisible);

            // hide both and the texmap should hide
            line.IsVisible = false;
            Assert.IsFalse(texmap.IsVisible);

            // return one and the texmap should reappear
            triangle.IsVisible = true;
            Assert.IsTrue(texmap.IsVisible);

            // and hiding the texmap should hide both elements
            texmap.IsVisible = false;
            Assert.IsFalse(line.IsVisible);
            Assert.IsFalse(triangle.IsVisible);

            // undo/redo
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            texmap.IsVisible = true;
            undoStack.EndCommand();
            Assert.IsTrue(line.IsVisible);
            Assert.IsTrue(triangle.IsVisible);
            Assert.IsTrue(texmap.IsVisible);

            undoStack.Undo();
            Assert.IsFalse(line.IsVisible);
            Assert.IsFalse(triangle.IsVisible);
            Assert.IsFalse(texmap.IsVisible);

            undoStack.Redo();
            Assert.IsTrue(line.IsVisible);
            Assert.IsTrue(triangle.IsVisible);
            Assert.IsTrue(texmap.IsVisible);


            texmap = CreateTestTexmap();

            // adding a hidden element to an empty texmap should cause the texmap to hide
            line = MocksFactory.CreateMockLine();
            line.IsVisible = false;
            texmap.SharedGeometry.Add(line);
            Assert.IsFalse(texmap.IsVisible);

            // adding a visible element should cause it to show again
            triangle = MocksFactory.CreateMockTriangle();
            texmap.TextureGeometry.Add(triangle);
            Assert.IsTrue(texmap.IsVisible);

            Utils.DisposalAccessTest(texmap, delegate() { bool isVisible = texmap.IsVisible; });
        }

        [TestMethod]
        public sealed override void IsVisibleChangedTest()
        {
            ITexmap texmap        = CreateTestTexmap();
            bool eventSeen        = false;
            bool genericEventSeen = false;

            texmap.IsVisibleChanged += delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(texmap, sender);
            };

            ObjectChangedEventHandler genericHandler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.AreSame(texmap, sender);

                if ("IsVisibleChanged" == e.Operation)
                {
                    if (texmap == e.Source)
                    {
                        Assert.IsFalse(genericEventSeen);
                        genericEventSeen = true;

                        Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<bool>));

                        PropertyChangedEventArgs<bool> args = (PropertyChangedEventArgs<bool>)e.Parameters;
                        Assert.AreEqual(!texmap.IsVisible, args.OldValue);
                        Assert.AreEqual(texmap.IsVisible, args.NewValue);
                    }
                    else
                    {
                        Assert.AreNotSame(texmap, e.Source);
                    }
                }
                else
                {
                    Assert.AreNotSame(texmap, e.Source);
                }
            };

            texmap.Changed += genericHandler;

            // adding a hidden element to an empty texmap should trigger the event, as the texmap will become hidden
            ILine line     = MocksFactory.CreateMockLine();
            line.IsVisible = false;
            texmap.TextureGeometry.Add(line);
            Assert.IsFalse(texmap.IsVisible);
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // adding a visible element should trigger the event, as the texmap will become visible
            ITriangle triangle = MocksFactory.CreateMockTriangle();
            texmap.TextureGeometry.Add(triangle);
            Assert.IsTrue(texmap.IsVisible);
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // adding a hidden element should have no effect, as the texmap still contains visible content
            IQuadrilateral quad = MocksFactory.CreateMockQuadrilateral();
            quad.IsVisible      = false;
            texmap.TextureGeometry.Add(quad);
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);

            // hiding the texmap should trigger the event
            texmap.IsVisible = false;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // as should unhiding it
            texmap.IsVisible = true;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // hiding one of its members should not trigger, as there are still visible elements
            quad.IsVisible = false;
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);

            // but hiding all of them should trigger
            line.IsVisible     = false;
            triangle.IsVisible = false;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // removing the only visible element should trigger
            line.IsVisible   = true;
            eventSeen        = false;
            genericEventSeen = false;
            Assert.IsTrue(texmap.IsVisible);
            texmap.TextureGeometry.Remove(line);
            Assert.IsFalse(texmap.IsVisible);
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // and removing everything should trigger
            texmap.TextureGeometry.Remove(quad);
            texmap.TextureGeometry.Remove(triangle);
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            texmap.Changed -= genericHandler;
        }

        [TestMethod]
        public sealed override void IsGhostedTest()
        {
            ITexmap texmap = CreateTestTexmap();

            // an empty texmap is not ghosted
            Assert.IsFalse(texmap.IsGhosted);

            ILine line = MocksFactory.CreateMockLine();
            texmap.TextureGeometry.Add(line);
            Assert.IsFalse(line.IsGhosted);
            Assert.IsFalse(texmap.IsGhosted);

            // ghosting the line should cause the ITexmap to become ghosted
            line.IsGhosted = true;
            Assert.IsTrue(texmap.IsGhosted);
            line.IsGhosted = false;
            Assert.IsFalse(texmap.IsGhosted);

            // similarly, ghosting the ITexmap should affect the line
            texmap.IsGhosted = true;
            Assert.IsTrue(line.IsGhosted);
            texmap.IsGhosted = false;
            Assert.IsFalse(line.IsGhosted);

            // it also shouldn't matter which geometry the elements are in
            ITriangle triangle = MocksFactory.CreateMockTriangle();
            texmap.FallbackGeometry.Add(triangle);
            Assert.IsFalse(texmap.IsGhosted);

            // ghosting one but not the other should leave the texmap unghosted
            triangle.IsGhosted = true;
            Assert.IsFalse(line.IsGhosted);
            Assert.IsTrue(triangle.IsGhosted);
            Assert.IsFalse(texmap.IsGhosted);

            // ghost both and the texmap should ghost
            line.IsGhosted = true;
            Assert.IsTrue(texmap.IsGhosted);

            // return one and the texmap should unghost
            triangle.IsGhosted = false;
            Assert.IsFalse(texmap.IsGhosted);

            // and ghost the texmap should ghost both elements
            texmap.IsGhosted = true;
            Assert.IsTrue(line.IsGhosted);
            Assert.IsTrue(triangle.IsGhosted);

            // undo/redo
            UndoStack undoStack = new UndoStack();
            undoStack.StartCommand("command");
            texmap.IsGhosted = false;
            undoStack.EndCommand();
            Assert.IsFalse(line.IsGhosted);
            Assert.IsFalse(triangle.IsGhosted);
            Assert.IsFalse(texmap.IsGhosted);

            undoStack.Undo();
            Assert.IsTrue(line.IsGhosted);
            Assert.IsTrue(triangle.IsGhosted);
            Assert.IsTrue(texmap.IsGhosted);

            undoStack.Redo();
            Assert.IsFalse(line.IsGhosted);
            Assert.IsFalse(triangle.IsGhosted);
            Assert.IsFalse(texmap.IsGhosted);

            texmap = CreateTestTexmap();

            // adding a ghosted element to an empty texmap should cause the texmap to ghost
            line = MocksFactory.CreateMockLine();
            line.IsGhosted = true;
            texmap.SharedGeometry.Add(line);
            Assert.IsTrue(texmap.IsGhosted);

            // adding an unghosted element should cause it to unghost again
            triangle = MocksFactory.CreateMockTriangle();
            texmap.TextureGeometry.Add(triangle);
            Assert.IsFalse(texmap.IsGhosted);

            Utils.DisposalAccessTest(texmap, delegate() { bool isGhosted = texmap.IsGhosted; });
        }

        [TestMethod]
        public sealed override void IsGhostedChangedTest()
        {
            ITexmap texmap        = CreateTestTexmap();
            bool eventSeen        = false;
            bool genericEventSeen = false;

            texmap.IsGhostedChanged += delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(texmap, sender);
            };

            ObjectChangedEventHandler genericHandler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.AreSame(texmap, sender);

                if ("IsGhostedChanged" == e.Operation)
                {
                    if (texmap == e.Source)
                    {
                        Assert.IsFalse(genericEventSeen);
                        genericEventSeen = true;

                        Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<bool>));

                        PropertyChangedEventArgs<bool> args = (PropertyChangedEventArgs<bool>)e.Parameters;
                        Assert.AreEqual(!texmap.IsGhosted, args.OldValue);
                        Assert.AreEqual(texmap.IsGhosted, args.NewValue);
                    }
                    else
                    {
                        Assert.AreNotSame(texmap, e.Source);
                    }
                }
                else
                {
                    Assert.AreNotSame(texmap, e.Source);
                }
            };

            texmap.Changed += genericHandler;

            // adding a ghosted element to an empty texmap should trigger the event, as the texmap will become ghosted
            ILine line     = MocksFactory.CreateMockLine();
            line.IsGhosted = true;
            texmap.TextureGeometry.Add(line);
            Assert.IsTrue(texmap.IsGhosted);
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // adding an unghosted element should trigger the event, as the texmap will become unghosted
            ITriangle triangle = MocksFactory.CreateMockTriangle();
            texmap.TextureGeometry.Add(triangle);
            Assert.IsFalse(texmap.IsGhosted);
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // adding a ghosted element should have no effect, as the texmap still contains unghosted content
            IQuadrilateral quad = MocksFactory.CreateMockQuadrilateral();
            quad.IsGhosted      = true;
            texmap.TextureGeometry.Add(quad);
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);

            // ghosting the texmap should trigger the event
            texmap.IsGhosted = true;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // as should unghosting it
            texmap.IsGhosted = false;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // ghosting one of its members should not trigger, as there are still unghosted elements
            quad.IsGhosted = true;
            Assert.IsFalse(eventSeen);
            Assert.IsFalse(genericEventSeen);

            // but ghosting all of them should trigger
            line.IsGhosted     = true;
            triangle.IsGhosted = true;
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // removing the only unghosted element should trigger
            line.IsGhosted   = false;
            eventSeen        = false;
            genericEventSeen = false;
            Assert.IsFalse(texmap.IsGhosted);
            texmap.TextureGeometry.Remove(line);
            Assert.IsTrue(texmap.IsGhosted);
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            // and removing everything should trigger
            texmap.TextureGeometry.Remove(quad);
            texmap.TextureGeometry.Remove(triangle);
            Assert.IsTrue(eventSeen);
            Assert.IsTrue(genericEventSeen);
            eventSeen        = false;
            genericEventSeen = false;

            texmap.Changed -= genericHandler;
        }

        #endregion Attributes

        #region Cloning and Serialization

        protected override IDOMObject CreateTestObjectForCopying()
        {
            ITexmap texmap = CreateTestTexmap();

            if (!texmap.IsImmutable)
            {
                ILine line = MocksFactory.CreateMockLine();
                texmap.TextureGeometry.Add(line);

                ITriangle triangle = MocksFactory.CreateMockTriangle();
                texmap.SharedGeometry.Add(triangle);

                IQuadrilateral quad = MocksFactory.CreateMockQuadrilateral();
                texmap.FallbackGeometry.Add(quad);

                texmap.Projection       = TexmapProjection.Cylindrical;
                texmap.Point1           = Vector3d.UnitX;
                texmap.Point2           = Vector3d.UnitY;
                texmap.Point3           = Vector3d.UnitZ;
                texmap.HorizontalExtent = 45.0;
                texmap.VerticalExtent   = 90.0;
                texmap.Texture          = "texture";
                texmap.Glossmap         = "glossmap";
            }

            return texmap;
        }

        protected override void CompareCopiedObjects(IDOMObject original, IDOMObject copy)
        {
            ITexmap first  = (ITexmap)original;
            ITexmap second = (ITexmap)copy;

            Assert.AreEqual(first.Projection, second.Projection);
            Assert.AreEqual(first.Point1, second.Point1);
            Assert.AreEqual(first.Point2, second.Point2);
            Assert.AreEqual(first.Point3, second.Point3);
            Assert.AreEqual(first.HorizontalExtent, second.HorizontalExtent);
            Assert.AreEqual(first.VerticalExtent, second.VerticalExtent);
            Assert.AreEqual(first.Texture, second.Texture);
            Assert.AreEqual(first.Glossmap, second.Glossmap);

            Assert.AreEqual(first.TextureGeometry.IsLocked, second.TextureGeometry.IsLocked);

            for (int i = 0; i < first.TextureGeometry.Count; i++)
            {
                Assert.AreNotSame(first.TextureGeometry[i], second.TextureGeometry[i]);
                Assert.IsTrue(((IGraphic)first.TextureGeometry[i]).IsDuplicateOf((IGraphic)second.TextureGeometry[i]));
            }

            Assert.AreEqual(first.SharedGeometry.IsLocked, second.SharedGeometry.IsLocked);

            for (int i = 0; i < first.SharedGeometry.Count; i++)
            {
                Assert.AreNotSame(first.SharedGeometry[i], second.SharedGeometry[i]);
                Assert.IsTrue(((IGraphic)first.SharedGeometry[i]).IsDuplicateOf((IGraphic)second.SharedGeometry[i]));
            }

            Assert.AreEqual(first.FallbackGeometry.IsLocked, second.FallbackGeometry.IsLocked);

            for (int i = 0; i < first.FallbackGeometry.Count; i++)
            {
                Assert.AreNotSame(first.FallbackGeometry[i], second.FallbackGeometry[i]);
                Assert.IsTrue(((IGraphic)first.FallbackGeometry[i]).IsDuplicateOf((IGraphic)second.FallbackGeometry[i]));
            }

            base.CompareCopiedObjects(original, copy);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        [TestMethod]
        public sealed override void ToCodeTest()
        {
            ITexmap texmap = CreateTestTexmap();
            IPage page;
            IGroup group;
            StringBuilder code;

            // empty texmap: should be optimised out in PartsLibrary mode
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.Full, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START PLANAR 0 0 0 0 0 0 0 0 0 Undefined\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(String.Empty, code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.EdgeColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START PLANAR 0 0 0 0 0 0 0 0 0 Undefined\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());

            // single-line (all three projections)
            texmap.Point1           = new Vector3d(1, 2, 3);
            texmap.Point2           = new Vector3d(4, 5, 6);
            texmap.Point3           = new Vector3d(7, 8, 9);
            texmap.HorizontalExtent = 90.0;
            texmap.VerticalExtent   = 45.0;
            texmap.Glossmap         = "glossmap.png";
            ILine line              = MocksFactory.CreateMockLine();
            line.Vertex1            = new Vector3d(1, 2, 3);
            line.Vertex2            = new Vector3d(4, 5, 6);
            texmap.SharedGeometry.Add(line);

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                texmap.Projection = TexmapProjection.Planar;
                texmap.Texture    = "texture.png";
                texmap.Glossmap   = "glossmap.png";
                code              = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP NEXT PLANAR 1 2 3 4 5 6 7 8 9 texture.png GLOSSMAP glossmap.png\r\n" +
                                "2 24 1 2 3 4 5 6\r\n",
                                code.ToString());

                texmap.Projection = TexmapProjection.Cylindrical;
                texmap.Texture    = "texture name.png";
                code              = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP NEXT CYLINDRICAL 1 2 3 4 5 6 7 8 9 90 \"texture name.png\" GLOSSMAP glossmap.png\r\n" +
                                "2 24 1 2 3 4 5 6\r\n",
                                code.ToString());

                texmap.Projection = TexmapProjection.Spherical;
                texmap.Texture    = "texture.png";
                texmap.Glossmap   = null;
                code              = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP NEXT SPHERICAL 1 2 3 4 5 6 7 8 9 90 45 texture.png\r\n2 24 1 2 3 4 5 6\r\n", code.ToString());
            }

            // single-line and locked
            texmap.IsLocked = true;
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                            "0 !TEXMAP NEXT SPHERICAL 1 2 3 4 5 6 7 8 9 90 45 texture.png\r\n" +
                            "2 24 1 2 3 4 5 6\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP NEXT SPHERICAL 1 2 3 4 5 6 7 8 9 90 45 texture.png\r\n" +
                            "2 24 1 2 3 4 5 6\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                            "0 !TEXMAP NEXT SPHERICAL 1 2 3 4 5 6 7 8 9 90 45 texture.png\r\n" +
                            "2 24 1 2 3 4 5 6\r\n",
                            code.ToString());
            texmap.IsLocked = false;

            // single-line but with a locked geometry: should be converted to multi-line
            texmap.FallbackGeometry.IsLocked = true;
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START SPHERICAL 1 2 3 4 5 6 7 8 9 90 45 texture.png\r\n" +
                            "2 24 1 2 3 4 5 6\r\n" +
                            "0 !TEXMAP FALLBACK\r\n" +
                            "0 !DIGITALIS_LDTOOLS_DOM LOCKGEOM\r\n" +
                            "0 !TEXMAP END\r\n", code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START SPHERICAL 1 2 3 4 5 6 7 8 9 90 45 texture.png\r\n" +
                            "2 24 1 2 3 4 5 6\r\n" +
                            "0 !TEXMAP FALLBACK\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START SPHERICAL 1 2 3 4 5 6 7 8 9 90 45 texture.png\r\n" +
                            "2 24 1 2 3 4 5 6\r\n" +
                            "0 !TEXMAP FALLBACK\r\n" +
                            "0 !DIGITALIS_LDTOOLS_DOM LOCKGEOM\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            texmap.FallbackGeometry.IsLocked = false;

            // multi-line without fallback or shared geometry
            texmap.SharedGeometry.Clear();
            texmap.Projection  = TexmapProjection.Planar;
            ITriangle triangle = MocksFactory.CreateMockTriangle();
            triangle.Vertex1   = new Vector3d(1, 2, 3);
            triangle.Vertex2   = new Vector3d(4, 5, 6);
            triangle.Vertex3   = new Vector3d(7, 8, 9);
            texmap.TextureGeometry.Add(line);
            texmap.TextureGeometry.Add(triangle);

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                                "0 !: 2 24 1 2 3 4 5 6\r\n" +
                                "0 !: 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                                "0 !TEXMAP END\r\n",
                                code.ToString());
            }

            // multi-line and locked
            texmap.IsLocked = true;
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                            "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 !: 2 24 1 2 3 4 5 6\r\n" +
                            "0 !: 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 !: 2 24 1 2 3 4 5 6\r\n" +
                            "0 !: 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT\r\n" +
                            "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 !: 2 24 1 2 3 4 5 6\r\n" +
                            "0 !: 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            texmap.IsLocked = false;

            // multi-line with fallback
            IQuadrilateral quad = MocksFactory.CreateMockQuadrilateral();
            quad.Vertex1        = new Vector3d(1, 2, 3);
            quad.Vertex2        = new Vector3d(4, 5, 6);
            quad.Vertex3        = new Vector3d(7, 8, 9);
            quad.Vertex4        = new Vector3d(10, 11, 12);
            texmap.FallbackGeometry.Add(quad);

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                                "0 !: 2 24 1 2 3 4 5 6\r\n" +
                                "0 !: 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                                "0 !TEXMAP FALLBACK\r\n" +
                                "4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                                "0 !TEXMAP END\r\n",
                                code.ToString());
            }

            // single-line in a group
            texmap.TextureGeometry.Clear();
            texmap.SharedGeometry.Clear();
            texmap.FallbackGeometry.Clear();
            texmap.SharedGeometry.Add(line);
            page          = MocksFactory.CreateMockPage();
            page.PageType = PageType.Part;
            page.License  = License.None;
            page.Author   = null;
            page.History  = null;
            IStep step    = MocksFactory.CreateMockStep();
            page.Add(step);
            step.Add(texmap);
            group      = MocksFactory.CreateMockGroup();
            group.Name = "group";
            step.Add(group);
            texmap.Group = group;
            code = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 New Model\r\n" +
                            "0 Name: Untitled.dat\r\n" +
                            "0 !LDRAW_ORG Unofficial_Part\r\n" +
                            "0 MLCAD BTG group\r\n" +
                            "0 !TEXMAP NEXT PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 MLCAD BTG group\r\n" +
                            "2 24 1 2 3 4 5 6\r\n" +
                            "0 GROUP 2 group\r\n", code.ToString());
            // groups not allowed in the library
            code = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 New Model\r\n" +
                            "0 Name: Untitled.dat\r\n" +
                            "0 !LDRAW_ORG Unofficial_Part\r\n" +
                            "0 !TEXMAP NEXT PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "2 24 1 2 3 4 5 6\r\n", code.ToString());
            code = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 New Model\r\n" +
                            "0 Name: Untitled.dat\r\n" +
                            "0 !LDRAW_ORG Unofficial_Part\r\n" +
                            "0 !TEXMAP NEXT PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "2 24 1 2 3 4 5 6\r\n", code.ToString());

            // multi-line in a group
            line.Parent = null;
            texmap.TextureGeometry.Add(line);
            texmap.TextureGeometry.Add(triangle);
            code = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 New Model\r\n" +
                            "0 Name: Untitled.dat\r\n" +
                            "0 !LDRAW_ORG Unofficial_Part\r\n" +
                            "0 MLCAD BTG group\r\n" +
                            "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 MLCAD BTG group\r\n" +
                            "0 !: 2 24 1 2 3 4 5 6\r\n" +
                            "0 MLCAD BTG group\r\n" +
                            "0 !: 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 MLCAD BTG group\r\n" +
                            "0 !TEXMAP END\r\n" +
                            "0 GROUP 4 group\r\n", code.ToString());
            // groups not allowed in the library
            code = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 New Model\r\n" +
                            "0 Name: Untitled.dat\r\n" +
                            "0 !LDRAW_ORG Unofficial_Part\r\n" +
                            "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 !: 2 24 1 2 3 4 5 6\r\n" +
                            "0 !: 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 New Model\r\n" +
                            "0 Name: Untitled.dat\r\n" +
                            "0 !LDRAW_ORG Unofficial_Part\r\n" +
                            "0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 !: 2 24 1 2 3 4 5 6\r\n" +
                            "0 !: 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 !TEXMAP END\r\n", code.ToString());
            ((IElement)texmap).Parent = null;

            // fallback-only
            texmap.TextureGeometry.Clear();
            texmap.SharedGeometry.Clear();
            line.Parent = null;
            texmap.FallbackGeometry.Add(line);

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                                "0 !TEXMAP FALLBACK\r\n" +
                                "2 24 1 2 3 4 5 6\r\n" +
                                "0 !TEXMAP END\r\n",
                                code.ToString());
            }

            // shared-only
            texmap.FallbackGeometry.Clear();
            texmap.SharedGeometry.Add(line);
            ILine line2 = MocksFactory.CreateMockLine();
            line2.Vertex1 = new Vector3d(6, 5, 4);
            line2.Vertex2 = new Vector3d(3, 2, 1);
            texmap.SharedGeometry.Add(line2);

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                                "2 24 1 2 3 4 5 6\r\n" +
                                "2 24 6 5 4 3 2 1\r\n" +
                                "0 !TEXMAP END\r\n",
                                code.ToString());
            }

            // main+shared
            texmap.SharedGeometry.Clear();
            texmap.TextureGeometry.Add(triangle);
            texmap.SharedGeometry.Add(line);

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                                "0 !: 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                                "2 24 1 2 3 4 5 6\r\n" +
                                "0 !TEXMAP END\r\n",
                                code.ToString());
            }

            // shared+fallback
            texmap.TextureGeometry.Clear();
            texmap.SharedGeometry.Clear();
            texmap.FallbackGeometry.Add(triangle);
            texmap.SharedGeometry.Add(line);

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                                "2 24 1 2 3 4 5 6\r\n" +
                                "0 !TEXMAP FALLBACK\r\n" +
                                "3 16 1 2 3 4 5 6 7 8 9\r\n" +
                                "0 !TEXMAP END\r\n",
                                code.ToString());
            }

            // main+shared+fallback
            texmap.TextureGeometry.Add(quad);

            foreach (CodeStandards codeFormat in Enum.GetValues(typeof(CodeStandards)))
            {
                code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), codeFormat, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                                "0 !: 4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                                "2 24 1 2 3 4 5 6\r\n" +
                                "0 !TEXMAP FALLBACK\r\n" +
                                "3 16 1 2 3 4 5 6 7 8 9\r\n" +
                                "0 !TEXMAP END\r\n",
                                code.ToString());
            }

            // hidden
            texmap.IsVisible = false;
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 !: 0 MLCAD HIDE 4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            "0 MLCAD HIDE 2 24 1 2 3 4 5 6\r\n" +
                            "0 !TEXMAP FALLBACK\r\n" +
                            "0 MLCAD HIDE 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(String.Empty, code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 !: 0 MLCAD HIDE 4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            "0 MLCAD HIDE 2 24 1 2 3 4 5 6\r\n" +
                            "0 !TEXMAP FALLBACK\r\n" +
                            "0 MLCAD HIDE 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            texmap.IsVisible = true;

            // ghosted
            texmap.IsGhosted = true;
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 !: 0 GHOST 4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            "0 GHOST 2 24 1 2 3 4 5 6\r\n" +
                            "0 !TEXMAP FALLBACK\r\n" +
                            "0 GHOST 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual(String.Empty, code.ToString());
            code = Utils.PreProcessCode(texmap.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
            Assert.AreEqual("0 !TEXMAP START PLANAR 1 2 3 4 5 6 7 8 9 texture.png\r\n" +
                            "0 !: 0 GHOST 4 16 1 2 3 4 5 6 7 8 9 10 11 12\r\n" +
                            "0 GHOST 2 24 1 2 3 4 5 6\r\n" +
                            "0 !TEXMAP FALLBACK\r\n" +
                            "0 GHOST 3 16 1 2 3 4 5 6 7 8 9\r\n" +
                            "0 !TEXMAP END\r\n",
                            code.ToString());

            base.ToCodeTest();
        }

        #endregion Code-generation

        #region Collection-management

        [TestMethod]
        public void CanInsertTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.AreEqual(InsertCheckResult.NotSupported, texmap.CanInsert(MocksFactory.CreateMockLine(), InsertCheckFlags.None));

            Utils.DisposalAccessTest(texmap, delegate() { InsertCheckResult canInsert = texmap.CanInsert(MocksFactory.CreateMockLine(), InsertCheckFlags.None); });
        }

        [TestMethod]
        public void CanReplaceTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.AreEqual(InsertCheckResult.NotSupported, texmap.CanReplace(MocksFactory.CreateMockLine(), MocksFactory.CreateMockTriangle(), InsertCheckFlags.None));

            Utils.DisposalAccessTest(texmap, delegate() { InsertCheckResult canReplace = texmap.CanReplace(MocksFactory.CreateMockLine(), MocksFactory.CreateMockTriangle(), InsertCheckFlags.None); });
        }

        [TestMethod]
        public void ContainsColourElementsTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.IsFalse(texmap.ContainsColourElements);

            Utils.DisposalAccessTest(texmap, delegate() { bool containsColours = texmap.ContainsColourElements; });
        }

        [TestMethod]
        public void ContainsBFCFlagElementsTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.IsFalse(texmap.ContainsBFCFlagElements);

            Utils.DisposalAccessTest(texmap, delegate() { bool containsBFC = texmap.ContainsBFCFlagElements; });
        }

        [TestMethod]
        public void HasLockedDescendantsTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.IsFalse(texmap.HasLockedDescendants);
            texmap.TextureGeometry.Add(MocksFactory.CreateMockLine());
            Assert.IsFalse(texmap.HasLockedDescendants);
            texmap.TextureGeometry[0].IsLocked = true;
            Assert.IsTrue(texmap.HasLockedDescendants);

            Utils.DisposalAccessTest(texmap, delegate() { bool hasLocked = texmap.HasLockedDescendants; });
        }

        [TestMethod]
        public void CountTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.AreEqual(0, texmap.Count);

            texmap.SharedGeometry.Add(MocksFactory.CreateMockLine());
            Assert.AreEqual(1, texmap.Count);

            texmap.FallbackGeometry.Add(MocksFactory.CreateMockLine());
            Assert.AreEqual(2, texmap.Count);

            // TextureGeometry does not count towards the total
            texmap.TextureGeometry.Add(MocksFactory.CreateMockLine());
            Assert.AreEqual(2, texmap.Count);

            Utils.DisposalAccessTest(texmap, delegate() { int count = texmap.Count; });
        }

        [TestMethod]
        public void IndexOfTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.AreEqual(-1, texmap.IndexOf(null));
            Assert.AreEqual(-1, texmap.IndexOf(MocksFactory.CreateMockLine()));

            ILine line = MocksFactory.CreateMockLine();
            texmap.SharedGeometry.Add(line);
            Assert.AreEqual(0, texmap.IndexOf(line));

            ITriangle triangle = MocksFactory.CreateMockTriangle();
            texmap.FallbackGeometry.Add(triangle);
            Assert.AreEqual(1, texmap.IndexOf(triangle));

            // TextureGeometry does not count towards the total
            IQuadrilateral quad = MocksFactory.CreateMockQuadrilateral();
            texmap.TextureGeometry.Add(quad);
            Assert.AreEqual(-1, texmap.IndexOf(quad));

            Utils.DisposalAccessTest(texmap, delegate() { int idx = texmap.IndexOf(line); });
        }

        [TestMethod]
        public void ContainsTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.IsFalse(texmap.Contains(null));
            Assert.IsFalse(texmap.Contains(MocksFactory.CreateMockLine()));

            ILine line = MocksFactory.CreateMockLine();
            texmap.SharedGeometry.Add(line);
            Assert.IsTrue(texmap.Contains(line));

            ITriangle triangle = MocksFactory.CreateMockTriangle();
            texmap.FallbackGeometry.Add(triangle);
            Assert.IsTrue(texmap.Contains(triangle));

            // TextureGeometry does not count towards the total
            IQuadrilateral quad = MocksFactory.CreateMockQuadrilateral();
            texmap.TextureGeometry.Add(quad);
            Assert.IsFalse(texmap.Contains(quad));

            Utils.DisposalAccessTest(texmap, delegate() { bool contains = texmap.Contains(line); });
        }

        [TestMethod]
        public void IndexerTest()
        {
            ITexmap texmap = CreateTestTexmap();

            // range-checks
            try
            {
                IElement element = texmap[-1];
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            try
            {
                IElement element = texmap[texmap.Count];
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            // successful 'get'
            ILine line = MocksFactory.CreateMockLine();
            texmap.SharedGeometry.Add(line);
            Assert.AreSame(line, texmap[0]);

            ITriangle triangle = MocksFactory.CreateMockTriangle();
            texmap.FallbackGeometry.Add(triangle);
            Assert.AreSame(triangle, texmap[1]);

            // setting not permitted
            try
            {
                texmap[0] = MocksFactory.CreateMockLine();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            }

            // TODO: test when locked, frozen, immutable

            Utils.DisposalAccessTest(texmap, delegate() { IDOMObject obj = texmap[0]; });
        }

        [TestMethod]
        public void AddTest()
        {
            ITexmap texmap = CreateTestTexmap();
            int count      = texmap.Count;

            try
            {
                texmap.Add(MocksFactory.CreateMockLine());
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
                Assert.AreEqual(count, texmap.Count);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            }

            // TODO: test when locked, frozen, immutable

            Utils.DisposalAccessTest(texmap, delegate() { texmap.Add(MocksFactory.CreateMockLine()); });
        }

        [TestMethod]
        public void InsertTest()
        {
            ITexmap texmap = CreateTestTexmap();
            int count      = texmap.Count;

            try
            {
                texmap.Insert(0, MocksFactory.CreateMockLine());
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
                Assert.AreEqual(count, texmap.Count);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            }

            // TODO: test when locked, frozen, immutable

            Utils.DisposalAccessTest(texmap, delegate() { texmap.Insert(0, MocksFactory.CreateMockLine()); });
        }

        [TestMethod]
        public void RemoveTest()
        {
            ITexmap texmap = CreateTestTexmap();
            ILine line     = MocksFactory.CreateMockLine();
            texmap.FallbackGeometry.Add(line);

            int count = texmap.Count;

            try
            {
                texmap.Remove(line);
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
                Assert.AreEqual(count, texmap.Count);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            }

            // TODO: test when locked, frozen, immutable

            Utils.DisposalAccessTest(texmap, delegate() { texmap.Remove(line); });
        }

        [TestMethod]
        public void RemoveAtTest()
        {
            ITexmap texmap = CreateTestTexmap();
            int count      = texmap.Count;

            try
            {
                texmap.RemoveAt(0);
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
                Assert.AreEqual(count, texmap.Count);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            }

            // TODO: test when locked, frozen, immutable

            Utils.DisposalAccessTest(texmap, delegate() { texmap.RemoveAt(0); });
        }

        [TestMethod]
        public void ClearTest()
        {
            ITexmap texmap = CreateTestTexmap();
            int count      = texmap.Count;

            try
            {
                texmap.Clear();
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
                Assert.AreEqual(count, texmap.Count);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(NotSupportedException), e.GetType());
            }

            // TODO: test when locked, frozen, immutable

            Utils.DisposalAccessTest(texmap, delegate() { texmap.Clear(); });
        }

        [TestMethod]
        public void CopyToTest()
        {
            ITexmap texmap   = CreateTestTexmap();
            IElement[] array = new IElement[texmap.Count];

            try
            {
                texmap.CopyTo(null, 0);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentNullException), e.GetType());
            }

            try
            {
                texmap.CopyTo(array, -1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
            }

            // array too small
            try
            {
                texmap.CopyTo(array, 1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentException), e.GetType());
            }

            texmap.CopyTo(array, 0);

            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreSame(texmap[i], array[i]);
            }

            Utils.DisposalAccessTest(texmap, delegate() { texmap.CopyTo(array, 0); });
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            ITexmap texmap = CreateTestTexmap();
            int count      = 0;

            texmap.SharedGeometry.Add(MocksFactory.CreateMockLine());
            texmap.FallbackGeometry.Add(MocksFactory.CreateMockLine());

            foreach (IElement element in texmap)
            {
                count++;
            }

            Assert.AreEqual(texmap.Count, count);

            Utils.DisposalAccessTest(texmap, delegate() { foreach (IElement element in texmap) { } });
        }

        #endregion Collection-management

        #region Document-tree

        [TestMethod]
        public void CollectionTreeChangedTest()
        {
            ITexmap texmap = CreateTestTexmap();
            ITexmapGeometry geom = null;
            ILine line           = null;
            bool eventSeen       = false;

            ObjectChangedEventHandler handler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                eventSeen = true;
                Assert.AreSame(texmap, sender);

                switch (e.Operation)
                {
                    case "ElementsAdded":
                    case "ElementsRemoved":
                    case "CollectionCleared":
                        Assert.AreSame(geom, e.Source);
                        break;

                    case "ColourValueChanged":
                        Assert.AreSame(line, e.Source);
                        break;

                    case "ParentChanged":
                        Assert.AreSame(line, e.Source);
                        break;

                    default:
                        Assert.Fail(e.Operation);
                        break;
                }
            };

            texmap.Changed += handler;

            foreach (ITexmapGeometry geometryCollection in texmap)
            {
                geom = geometryCollection;

                // add an element
                line = MocksFactory.CreateMockLine();
                geometryCollection.Add(line);
                Assert.IsTrue(eventSeen);
                eventSeen = false;

                // modify it
                line.ColourValue = 1U;
                Assert.IsTrue(eventSeen);
                eventSeen = false;

                // remove it
                geometryCollection.Remove(line);
                Assert.IsTrue(eventSeen);
                eventSeen = false;

                // clear all elements
                geometryCollection.Add(line);
                eventSeen = false;
                geometryCollection.Clear();
                Assert.IsTrue(eventSeen);
                eventSeen = false;
            }

            texmap.Changed -= handler;
        }

        [TestMethod]
        public sealed override void ParentTest()
        {
            ITexmap texmap = (ITexmap)CreateTestPageElementWithDocumentTree();
            Assert.IsNotNull(((IElement)texmap).Parent);
            Assert.IsNotNull(((IElementCollection)texmap).Parent);
            Assert.AreSame(((IElement)texmap).Parent, ((IElementCollection)texmap).Parent);

            base.ParentTest();
        }

        #endregion Document-tree

        #region Geometry

        [TestMethod]
        public sealed override void BoundingBoxTest()
        {
            ITexmap texmap = CreateTestTexmap();
            Assert.AreEqual(new Box3d(), texmap.BoundingBox);

            ITriangle triangle = MocksFactory.CreateMockTriangle();
            triangle.Vertex1   = new Vector3d(1, 2, 3);
            triangle.Vertex2   = new Vector3d(4, 5, 6);
            triangle.Vertex3   = new Vector3d(7, 8, 9);
            texmap.TextureGeometry.Add(triangle);

            triangle         = MocksFactory.CreateMockTriangle();
            triangle.Vertex1 = new Vector3d(9, 8, 7);
            triangle.Vertex2 = new Vector3d(6, 5, 4);
            triangle.Vertex3 = new Vector3d(3, 2, 1);
            texmap.FallbackGeometry.Add(triangle);

            Assert.AreEqual(new Box3d(1, 2, 1, 9, 8, 9), texmap.BoundingBox);
        }

        [TestMethod]
        public sealed override void OriginTest()
        {
            ITexmap texmap = CreateTestTexmap();

            ITriangle triangle = MocksFactory.CreateMockTriangle();
            triangle.Vertex1   = new Vector3d(1, 2, 3);
            triangle.Vertex2   = new Vector3d(4, 5, 6);
            triangle.Vertex3   = new Vector3d(7, 8, 9);
            texmap.TextureGeometry.Add(triangle);

            triangle         = MocksFactory.CreateMockTriangle();
            triangle.Vertex1 = new Vector3d(9, 8, 7);
            triangle.Vertex2 = new Vector3d(6, 5, 4);
            triangle.Vertex3 = new Vector3d(3, 2, 1);
            texmap.FallbackGeometry.Add(triangle);

            Assert.AreEqual(texmap.BoundingBox.Centre, texmap.Origin);
        }

        [TestMethod]
        public sealed override void TransformTest()
        {
            ITexmap texmap     = CreateTestTexmap();
            Matrix4d transform = new Matrix4d(10, 0, 0, 0, 0, 5, 0, 0, 0, 0, 15, 0, 0, 0, 0, 1);

            ITriangle triangle = MocksFactory.CreateMockTriangle();
            triangle.Vertex1   = new Vector3d(1, 2, 3);
            triangle.Vertex2   = new Vector3d(4, 5, 6);
            triangle.Vertex3   = new Vector3d(7, 8, 9);
            texmap.TextureGeometry.Add(triangle);

            triangle         = MocksFactory.CreateMockTriangle();
            triangle.Vertex1 = new Vector3d(9, 8, 7);
            triangle.Vertex2 = new Vector3d(6, 5, 4);
            triangle.Vertex3 = new Vector3d(3, 2, 1);
            texmap.FallbackGeometry.Add(triangle);

            texmap.Transform(ref transform);

            triangle = (ITriangle)texmap.TextureGeometry[0];
            Assert.AreEqual(new Vector3d(10, 10, 45), triangle.Vertex1);
            Assert.AreEqual(new Vector3d(40, 25, 90), triangle.Vertex2);
            Assert.AreEqual(new Vector3d(70, 40, 135), triangle.Vertex3);

            triangle = (ITriangle)texmap.FallbackGeometry[0];
            Assert.AreEqual(new Vector3d(90, 40, 105), triangle.Vertex1);
            Assert.AreEqual(new Vector3d(60, 25, 60), triangle.Vertex2);
            Assert.AreEqual(new Vector3d(30, 10, 15), triangle.Vertex3);
        }

        [TestMethod]
        public sealed override void ReverseWindingTest()
        {
            ITexmap texmap = CreateTestTexmap();

            ITriangle triangle = MocksFactory.CreateMockTriangle();
            triangle.Vertex1   = new Vector3d(1, 2, 3);
            triangle.Vertex2   = new Vector3d(4, 5, 6);
            triangle.Vertex3   = new Vector3d(7, 8, 9);
            texmap.TextureGeometry.Add(triangle);

            triangle         = MocksFactory.CreateMockTriangle();
            triangle.Vertex1 = new Vector3d(9, 8, 7);
            triangle.Vertex2 = new Vector3d(6, 5, 4);
            triangle.Vertex3 = new Vector3d(3, 2, 1);
            texmap.FallbackGeometry.Add(triangle);

            texmap.ReverseWinding();

            triangle = (ITriangle)texmap.TextureGeometry[0];
            Assert.AreEqual(new Vector3d(7, 8, 9), triangle.Vertex1);
            Assert.AreEqual(new Vector3d(4, 5, 6), triangle.Vertex2);
            Assert.AreEqual(new Vector3d(1, 2, 3), triangle.Vertex3);

            triangle = (ITriangle)texmap.FallbackGeometry[0];
            Assert.AreEqual(new Vector3d(3, 2, 1), triangle.Vertex1);
            Assert.AreEqual(new Vector3d(6, 5, 4), triangle.Vertex2);
            Assert.AreEqual(new Vector3d(9, 8, 7), triangle.Vertex3);
        }

        #endregion Geometry

        #region Properties

        [TestMethod]
        public void ProjectionTest()
        {
            ITexmap texmap                = CreateTestTexmap();
            TexmapProjection defaultValue = TexmapProjection.Planar;
            TexmapProjection newValue     = TexmapProjection.Cylindrical;

            PropertyValueTest(texmap,
                                defaultValue,
                                newValue,
                                delegate(ITexmap obj) { return obj.Projection; },
                                delegate(ITexmap obj, TexmapProjection value) { obj.Projection = value; },
                                PropertyValueFlags.None);
        }

        [TestMethod]
        public void ProjectionChangedTest()
        {
            ITexmap texmap              = CreateTestTexmap();
            TexmapProjection valueToSet = TexmapProjection.Cylindrical;

            PropertyChangedTest(texmap,
                                "ProjectionChanged",
                                valueToSet,
                                delegate(ITexmap obj, PropertyChangedEventHandler<TexmapProjection> handler) { obj.ProjectionChanged += handler; },
                                delegate(ITexmap obj) { return obj.Projection; },
                                delegate(ITexmap obj, TexmapProjection value) { obj.Projection = value; });
        }

        [TestMethod]
        public void Point1Test()
        {
            ITexmap texmap        = CreateTestTexmap();
            Vector3d defaultValue = Vector3d.Zero;
            Vector3d newValue     = Vector3d.UnitX;

            PropertyValueTest(texmap,
                                defaultValue,
                                newValue,
                                delegate(ITexmap obj) { return obj.Point1; },
                                delegate(ITexmap obj, Vector3d value) { obj.Point1 = value; },
                                PropertyValueFlags.None);

            // Point1==Coordinates[0]
            texmap = CreateTestTexmap();

            if (!texmap.IsImmutable)
                texmap.Point1 = Vector3d.UnitX;

            Assert.AreEqual(texmap.Point1, texmap.Coordinates.ElementAt(0));
        }

        [TestMethod]
        public void Point1ChangedTest()
        {
            ITexmap texmap                 = CreateTestTexmap();
            IEnumerable<Vector3d> oldValue = texmap.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.UnitX, Vector3d.Zero, Vector3d.Zero };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!texmap.IsImmutable)
            {
                texmap.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(texmap, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                };

                texmap.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(texmap, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                };

                texmap.Point1 = newValue.ElementAt(0);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen        = false;
                genericEventSeen = false;
                texmap.Point1    = newValue.ElementAt(0);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void Point2Test()
        {
            ITexmap texmap        = CreateTestTexmap();
            Vector3d defaultValue = Vector3d.Zero;
            Vector3d newValue     = Vector3d.UnitX;

            PropertyValueTest(texmap,
                                defaultValue,
                                newValue,
                                delegate(ITexmap obj) { return obj.Point2; },
                                delegate(ITexmap obj, Vector3d value) { obj.Point2 = value; },
                                PropertyValueFlags.None);

            // Point2==Coordinates[1]
            texmap = CreateTestTexmap();

            if (!texmap.IsImmutable)
                texmap.Point2 = Vector3d.UnitX;

            Assert.AreEqual(texmap.Point2, texmap.Coordinates.ElementAt(1));
        }

        [TestMethod]
        public void Point2ChangedTest()
        {
            ITexmap texmap                 = CreateTestTexmap();
            IEnumerable<Vector3d> oldValue = texmap.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.Zero, Vector3d.UnitX, Vector3d.Zero };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!texmap.IsImmutable)
            {
                texmap.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(texmap, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                };

                texmap.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(texmap, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                };

                texmap.Point2 = newValue.ElementAt(1);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen        = false;
                genericEventSeen = false;
                texmap.Point2    = newValue.ElementAt(1);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void Point3Test()
        {
            ITexmap texmap        = CreateTestTexmap();
            Vector3d defaultValue = Vector3d.Zero;
            Vector3d newValue     = Vector3d.UnitX;

            PropertyValueTest(texmap,
                                defaultValue,
                                newValue,
                                delegate(ITexmap obj) { return obj.Point3; },
                                delegate(ITexmap obj, Vector3d value) { obj.Point3 = value; },
                                PropertyValueFlags.None);

            // Point3==Coordinates[2]
            texmap = CreateTestTexmap();

            if (!texmap.IsImmutable)
                texmap.Point3 = Vector3d.UnitX;

            Assert.AreEqual(texmap.Point3, texmap.Coordinates.ElementAt(2));
        }

        [TestMethod]
        public void Point3ChangedTest()
        {
            ITexmap texmap                 = CreateTestTexmap();
            IEnumerable<Vector3d> oldValue = texmap.Coordinates;
            IEnumerable<Vector3d> newValue = new Vector3d[] { Vector3d.Zero, Vector3d.Zero, Vector3d.UnitX };
            bool eventSeen                 = false;
            bool genericEventSeen          = false;

            if (!texmap.IsImmutable)
            {
                texmap.CoordinatesChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
                {
                    Assert.IsFalse(eventSeen);
                    eventSeen = true;
                    Assert.AreSame(texmap, sender);
                    Assert.AreEqual(oldValue.ElementAt(0), e.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), e.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), e.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), e.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), e.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), e.NewValue.ElementAt(2));
                };

                texmap.Changed += delegate(IDOMObject sender, ObjectChangedEventArgs e)
                {
                    Assert.IsFalse(genericEventSeen);
                    genericEventSeen = true;
                    Assert.AreSame(texmap, sender);
                    Assert.AreEqual("CoordinatesChanged", e.Operation);
                    Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<IEnumerable<Vector3d>>));

                    PropertyChangedEventArgs<IEnumerable<Vector3d>> args = (PropertyChangedEventArgs<IEnumerable<Vector3d>>)e.Parameters;
                    Assert.AreEqual(oldValue.ElementAt(0), args.OldValue.ElementAt(0));
                    Assert.AreEqual(oldValue.ElementAt(1), args.OldValue.ElementAt(1));
                    Assert.AreEqual(oldValue.ElementAt(2), args.OldValue.ElementAt(2));
                    Assert.AreEqual(newValue.ElementAt(0), args.NewValue.ElementAt(0));
                    Assert.AreEqual(newValue.ElementAt(1), args.NewValue.ElementAt(1));
                    Assert.AreEqual(newValue.ElementAt(2), args.NewValue.ElementAt(2));
                };

                texmap.Point3 = newValue.ElementAt(2);
                Assert.IsTrue(eventSeen);
                Assert.IsTrue(genericEventSeen);

                // setting to the same value should not generate the events
                eventSeen        = false;
                genericEventSeen = false;
                texmap.Point3    = newValue.ElementAt(2);
                Assert.IsFalse(eventSeen);
                Assert.IsFalse(genericEventSeen);
            }
        }

        [TestMethod]
        public void HorizontalExtentTest()
        {
            ITexmap texmap      = CreateTestTexmap();
            double defaultValue = 360.0;
            double newValue     = 90.0;

            PropertyValueTest(texmap,
                                defaultValue,
                                newValue,
                                delegate(ITexmap obj) { return obj.HorizontalExtent; },
                                delegate(ITexmap obj, double value) { obj.HorizontalExtent = value; },
                                PropertyValueFlags.None);

            texmap = CreateTestTexmap();

            if (!texmap.IsImmutable)
            {
                try
                {
                    texmap.HorizontalExtent = 0.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(360.0, texmap.HorizontalExtent);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                try
                {
                    texmap.HorizontalExtent = 361.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(360.0, texmap.HorizontalExtent);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void HorizontalExtentChangedTest()
        {
            ITexmap texmap    = CreateTestTexmap();
            double valueToSet = 90.0;

            PropertyChangedTest(texmap,
                                "HorizontalExtentChanged",
                                valueToSet,
                                delegate(ITexmap obj, PropertyChangedEventHandler<double> handler) { obj.HorizontalExtentChanged += handler; },
                                delegate(ITexmap obj) { return obj.HorizontalExtent; },
                                delegate(ITexmap obj, double value) { obj.HorizontalExtent = value; });
        }

        [TestMethod]
        public void VerticalExtentTest()
        {
            ITexmap texmap      = CreateTestTexmap();
            double defaultValue = 360.0;
            double newValue     = 90.0;

            PropertyValueTest(texmap,
                                defaultValue,
                                newValue,
                                delegate(ITexmap obj) { return obj.VerticalExtent; },
                                delegate(ITexmap obj, double value) { obj.VerticalExtent = value; },
                                PropertyValueFlags.None);

            texmap = CreateTestTexmap();

            if (!texmap.IsImmutable)
            {
                try
                {
                    texmap.VerticalExtent = 0.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(360.0, texmap.VerticalExtent);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }

                try
                {
                    texmap.VerticalExtent = 361.0;
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.AreEqual(360.0, texmap.VerticalExtent);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentOutOfRangeException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void VerticalExtentChangedTest()
        {
            ITexmap texmap    = CreateTestTexmap();
            double valueToSet = 90.0;

            PropertyChangedTest(texmap,
                                "VerticalExtentChanged",
                                valueToSet,
                                delegate(ITexmap obj, PropertyChangedEventHandler<double> handler) { obj.VerticalExtentChanged += handler; },
                                delegate(ITexmap obj) { return obj.VerticalExtent; },
                                delegate(ITexmap obj, double value) { obj.VerticalExtent = value; });
        }

        [TestMethod]
        public void TextureTest()
        {
            ITexmap texmap      = CreateTestTexmap();
            string defaultValue = Resources.Undefined;
            string newValue     = "texture";

            PropertyValueTest(texmap,
                                defaultValue,
                                newValue,
                                delegate(ITexmap obj) { return obj.Texture; },
                                delegate(ITexmap obj, string value) { obj.Texture = value; },
                                PropertyValueFlags.None);

            // TargetName may not be null/empty/whitespace
            texmap = CreateTestTexmap();

            if (!texmap.IsImmutable)
            {
                string oldValue = texmap.Texture;

                try
                {
                    texmap.Texture = null;
                    Assert.Fail();
                }
                catch (ArgumentNullException)
                {
                    Assert.AreEqual(oldValue, texmap.Texture);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentException), e.GetType());
                }

                try
                {
                    texmap.Texture = "";
                    Assert.Fail();
                }
                catch (ArgumentNullException)
                {
                    Assert.AreEqual(oldValue, texmap.Texture);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentException), e.GetType());
                }

                try
                {
                    texmap.Texture = "  ";
                    Assert.Fail();
                }
                catch (ArgumentNullException)
                {
                    Assert.AreEqual(oldValue, texmap.Texture);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(typeof(ArgumentException), e.GetType());
                }
            }
        }

        [TestMethod]
        public void TextureChangedTest()
        {
            ITexmap texmap    = CreateTestTexmap();
            string valueToSet = "texture";

            PropertyChangedTest(texmap,
                                "TextureChanged",
                                valueToSet,
                                delegate(ITexmap obj, PropertyChangedEventHandler<string> handler) { obj.TextureChanged += handler; },
                                delegate(ITexmap obj) { return obj.Texture; },
                                delegate(ITexmap obj, string value) { obj.Texture = value; });
        }

        [TestMethod]
        public void TexturePathTest()
        {
            ITexmap texmap = CreateTestTexmap();

            // missing file
            texmap.Texture = "foo.png";
            Assert.IsNull(texmap.TexturePath);

            // invalid file
            texmap.Texture = "3001.dat";
            Assert.IsNull(texmap.TexturePath);

            // valid file
            texmap.Texture = "indy_face.png";
            Assert.IsNotNull(texmap.TexturePath);
        }

        [TestMethod]
        public void GlossmapTest()
        {
            ITexmap texmap      = CreateTestTexmap();
            string defaultValue = Resources.Undefined;
            string newValue     = "glossmap";

            PropertyValueTest(texmap,
                                defaultValue,
                                newValue,
                                delegate(ITexmap obj) { return obj.Glossmap; },
                                delegate(ITexmap obj, string value) { obj.Glossmap = value; },
                                PropertyValueFlags.None);
        }

        [TestMethod]
        public void GlossmapChangedTest()
        {
            ITexmap texmap    = CreateTestTexmap();
            string valueToSet = "glossmap";

            PropertyChangedTest(texmap,
                                "GlossmapChanged",
                                valueToSet,
                                delegate(ITexmap obj, PropertyChangedEventHandler<string> handler) { obj.GlossmapChanged += handler; },
                                delegate(ITexmap obj) { return obj.Glossmap; },
                                delegate(ITexmap obj, string value) { obj.Glossmap = value; });
        }

        [TestMethod]
        public void GlossmapPathTest()
        {
            ITexmap texmap = CreateTestTexmap();

            // missing file
            texmap.Glossmap = "foo.png";
            Assert.IsNull(texmap.GlossmapPath);

            // invalid file
            texmap.Glossmap = "3001.dat";
            Assert.IsNull(texmap.GlossmapPath);

            // invalid file (no alpha)
            texmap.Glossmap = "myspchar2.png";
            Assert.IsNull(texmap.GlossmapPath);

            // valid file
            texmap.Glossmap = "indy_face.png";
            Assert.IsNotNull(texmap.GlossmapPath);
        }

        protected override void PropertyChangedTest<C, T>(C obj, string eventName, T valueToSet, IDOMObjectTest.AttachPropertyChangedHandler<C, T> attachHandler, IDOMObjectTest.PropertyValueGetter<C, T> getter, IDOMObjectTest.PropertyValueSetter<C, T> setter)
        {
            ITexmap texmap = (ITexmap)obj;
            T oldValue     = getter(obj);
            bool eventSeen = false;

            ObjectChangedEventHandler handler = delegate(IDOMObject sender, ObjectChangedEventArgs e)
            {
                Assert.IsFalse(eventSeen);
                eventSeen = true;
                Assert.AreSame(texmap, sender);
                Assert.AreEqual(eventName, e.Operation);
                Assert.IsInstanceOfType(e.Parameters, typeof(PropertyChangedEventArgs<T>));

                PropertyChangedEventArgs<T> args = (PropertyChangedEventArgs<T>)e.Parameters;
                Assert.AreEqual(oldValue, args.OldValue);
                Assert.AreEqual(valueToSet, args.NewValue);
            };

            texmap.Changed += handler;

            setter(obj, valueToSet);
            Assert.IsTrue(eventSeen);

            // setting the same value again should not trigger the event
            eventSeen = false;
            setter(obj, valueToSet);
            Assert.IsFalse(eventSeen);

            texmap.Changed -= handler;
            setter(obj, oldValue);

            base.PropertyChangedTest<C, T>(obj, eventName, valueToSet, attachHandler, getter, setter);
        }

        #endregion Properties
    }
}
