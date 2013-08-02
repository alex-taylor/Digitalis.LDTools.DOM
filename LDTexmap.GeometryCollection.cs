#region License

//
// LDTexmap.GeometryCollection.cs
//
// Copyright (C) 2009-2013 Alex Taylor.  All Rights Reserved.
//
// This file is part of Digitalis.LDTools.DOM.dll
//
// Digitalis.LDTools.DOM.dll is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Digitalis.LDTools.DOM.dll is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Digitalis.LDTools.DOM.dll.  If not, see <http://www.gnu.org/licenses/>.
//

#endregion License

namespace Digitalis.LDTools.DOM
{
    #region Usings

    using System;
    using System.Drawing;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    public sealed partial class LDTexmap
    {
        [Serializable]
        private class GeometryCollection : ElementCollection, ITexmapGeometry
        {
            #region Cloning

            protected override void InitializeObject(IDOMObject obj)
            {
                GeometryCollection geometry = (GeometryCollection)obj;
                geometry._type              = GeometryType;
                geometry._typeName          = TypeName;
                base.InitializeObject(obj);
            }

            #endregion Cloning

            #region Collection-management

            public bool ContainsGraphics { get { return (_graphics > 0); } }
            private uint _graphics = 0;

            protected override void OnItemsAdded(UndoableListChangedEventArgs<IElement> e)
            {
                uint graphics = _graphics;

                foreach (IElement element in e.Items)
                {
                    if (element is IGraphic)
                        _graphics++;
                }

                try
                {
                    base.OnItemsAdded(e);
                }
                catch
                {
                    _graphics = graphics;
                    throw;
                }
            }

            protected override void OnItemsRemoved(UndoableListChangedEventArgs<IElement> e)
            {
                uint graphics = _graphics;

                foreach (IElement element in e.Items)
                {
                    if (element is IGraphic)
                        _graphics--;
                }

                try
                {
                    base.OnItemsRemoved(e);
                }
                catch
                {
                    _graphics = graphics;
                    throw;
                }
            }

            protected override void OnItemsReplaced(UndoableListReplacedEventArgs<IElement> e)
            {
                uint graphics = _graphics;

                foreach (IElement element in e.ItemsAdded.Items)
                {
                    if (element is IGraphic)
                        _graphics++;
                }

                foreach (IElement element in e.ItemsRemoved.Items)
                {
                    if (element is IGraphic)
                        _graphics--;
                }

                try
                {
                    base.OnItemsReplaced(e);
                }
                catch
                {
                    _graphics = graphics;
                    throw;
                }
            }

            protected override void OnCollectionCleared(UndoableListChangedEventArgs<IElement> e)
            {
                uint graphics = _graphics;
                _graphics = 0;

                try
                {
                    base.OnCollectionCleared(e);
                }
                catch
                {
                    _graphics = graphics;
                    throw;
                }
            }

            public override InsertCheckResult CanInsert(IElement element, InsertCheckFlags flags)
            {
                if (null == element || DOMObjectType.Texmap == element.ObjectType || element is GeometryCollection)
                    return InsertCheckResult.NotSupported;

                return base.CanInsert(element, flags);
            }

            public override InsertCheckResult CanReplace(IElement elementToInsert, IElement elementToReplace, InsertCheckFlags flags)
            {
                if (null == elementToInsert || DOMObjectType.Texmap == elementToInsert.ObjectType || elementToInsert is GeometryCollection)
                    return InsertCheckResult.NotSupported;

                return base.CanReplace(elementToInsert, elementToReplace, flags);
            }

            #endregion Collection-management

            #region Code-generation

            public override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsLocked && IsLocalLock && CodeStandards.PartsLibrary != codeFormat)
                    sb.AppendFormat("0 !DIGITALIS_LDTOOLS_DOM LOCKGEOM{0}", LineTerminator);

                if (TexmapGeometryType.Texture == GeometryType)
                {
                    StringBuilder textured = base.ToCode(new StringBuilder(), codeFormat, overrideColour, ref transform, winding);

                    if (textured.Length > 0)
                    {
                        const string prefix = "0 !: ";
                        int i = 0;

                        while (i < textured.Length - LineTerminator.Length)
                        {
                            if ('\n' == textured[i++] && '\r' != textured[i])
                            {
                                textured.Insert(i, prefix);
                                i += prefix.Length;
                            }
                        }

                        textured.Insert(0, prefix);
                        sb.Append(textured);
                    }
                }
                else
                {
                    sb = base.ToCode(sb, codeFormat, overrideColour, ref transform, winding);
                }

                return sb;
            }

            #endregion Code-generation

            #region Constructor

            // required to support Clone()
            public GeometryCollection()
            {
            }

            public GeometryCollection(LDTexmap texmap, TexmapGeometryType type, string typeName)
            {
                _texmap   = texmap;
                _type     = type;
                _typeName = typeName;

                texmap.PathToDocumentChanged += OnPathToDocumentChanged;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && null != Texmap)
                    throw new InvalidOperationException();

                base.Dispose(disposing);
            }

            #endregion Constructor

            #region Document-tree

            public ITexmap Texmap
            {
                get
                {
                    if (IsDisposed)
                        throw new ObjectDisposedException(null);

                    return _texmap;
                }
            }
            [NonSerialized]
            internal ITexmap _texmap;

            public override IElementCollection Parent
            {
                get
                {
                    if (IsDisposed)
                        throw new ObjectDisposedException(null);

                    return _texmap;
                }
            }

            public override IStep Step
            {
                get
                {
                    if (IsDisposed)
                        throw new ObjectDisposedException(null);

                    if (null == Parent)
                        return null;

                    return Parent.Step;
                }
            }

            private void OnPathToDocumentChanged(object sender, EventArgs e)
            {
                OnPathToDocumentChanged(e);
            }

            #endregion Document-tree

            #region Editor

            public override bool HasEditor
            {
                get
                {
                    if (IsDisposed)
                        throw new ObjectDisposedException(null);

                    return false;
                }
            }

            public override IElementEditor GetEditor()
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return null;
            }

            #endregion Editor

            #region Self-description

            public override DOMObjectType ObjectType { get { return DOMObjectType.Collection; } }

            public override bool IsImmutable { get { return false; } }

            public override Image Icon { get { return Resources.GeometryIcon; } }

            public override string TypeName { get { return _typeName; } }
            private string _typeName;

            public override string Description { get { return String.Empty; } }

            public override string ExtendedDescription { get { return String.Empty; } }

            public bool IsStateElement { get { return false; } }

            public bool IsTopLevelElement { get { return false; } }

            public override bool AllowsTopLevelElements { get { return false; } }

            public override bool IsReadOnly { get { return false; } }

            public TexmapGeometryType GeometryType { get { return _type; } }
            private TexmapGeometryType _type;

            #endregion Self-description
        }
    }
}
