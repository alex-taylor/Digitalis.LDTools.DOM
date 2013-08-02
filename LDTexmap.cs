#region License

//
// LDTexmap.cs
//
// Copyright (C) 2009-2012 Alex Taylor.  All Rights Reserved.
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.ITexmap"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [TypeName(typeof(Resources), "Texmap")]
    [DefaultIcon(typeof(Resources), "TexmapIcon")]
    [ElementFlags(ElementFlags.HasEditor)]
    public sealed partial class LDTexmap : Graphic, ITexmap
    {
        #region Inner types

        // Rule:   A full set of geometry must be provided
        // Type:   Error
        // Source: http://www.ldraw.org/documentation/ldraw-org-file-format-standards/language-extension-for-texture-mapping.html
        private class GeometryMissingProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_NoGeometry; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_TexmapGeometryMissing; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public GeometryMissingProblem(LDTexmap texmap)
            {
                Element = texmap;
            }
        }

        // Rule:   The texture image must exist and be a valid format
        // Type:   Error
        // Source: http://www.ldraw.org/documentation/ldraw-org-file-format-standards/language-extension-for-texture-mapping.html
        private class TextureMissingProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_NoTexture; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get; private set; }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public TextureMissingProblem(LDTexmap texmap)
            {
                Element     = texmap;
                Description = String.Format(Resources.Analytics_TexmapTextureMissing, texmap.Texture);
            }
        }

        // Rule:   The glossmap image, if specified, must exist and be a valid format
        // Type:   Error
        // Source: http://www.ldraw.org/documentation/ldraw-org-file-format-standards/language-extension-for-texture-mapping.html
        private class GlossmapMissingProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_NoGlossmap; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get; private set; }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public GlossmapMissingProblem(LDTexmap texmap)
            {
                Element     = texmap;
                Description = String.Format(Resources.Analytics_TexmapGlossmapMissing, texmap.Texture);
            }
        }

        // Rule:   The texture image path should be no more than 21 characters
        // Type:   Error
        // Source: http://www.ldraw.org/article/512.html#file_name
        private class TexturePathTooLongProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TextureTooLong; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_TexmapTexturePathTooLong; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public TexturePathTooLongProblem(LDTexmap texmap)
            {
                Element = texmap;
            }
        }

        // Rule:   The texture image path should only contain [A-Za-z0-9_-\.]
        // Type:   Error
        // Source: http://www.ldraw.org/article/512.html#file_name
        private class TexturePathInvalidCharsProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TextureInvalidChars; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_TexmapTexturePathInvalidChars; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public TexturePathInvalidCharsProblem(LDTexmap texmap)
            {
                Element = texmap;
            }
        }

        // Rule:   The glossmap image path, if specified, should be no more than 21 characters
        // Type:   Error
        // Source: http://www.ldraw.org/article/512.html#file_name
        private class GlossmapPathTooLongProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_GlossmapTooLong; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_TexmapGlossmapPathTooLong; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public GlossmapPathTooLongProblem(LDTexmap texmap)
            {
                Element = texmap;
            }
        }

        // Rule:   The glossmap image path, if specified, should only contain [A-Za-z0-9_-\.]
        // Type:   Error
        // Source: http://www.ldraw.org/article/512.html#file_name
        private class GlossmapPathInvalidCharsProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_GlossmapInvalidChars; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_TexmapGlossmapPathInvalidChars; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public GlossmapPathInvalidCharsProblem(LDTexmap texmap)
            {
                Element = texmap;
            }
        }

        #endregion Inner types

        #region Internals

        // temporary storage for the original line of code - used by the LDPage/LDDocument parser
        internal string LDrawCode;

        #endregion Internals

        #region Analytics

        // in official files, TexturePath and GlossmapPath may only contain these characters and be this length
        private static readonly Regex Regex_Path_InvalidChars = new Regex(@"^[-A-Za-z0-9_\.\\]+$", RegexOptions.IgnoreCase);
        private const int Max_PathLength                      = 21;

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsGeometryMissing"/> condition.
        /// </summary>
        public static readonly Guid Problem_NoGeometry = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTextureMissing"/> condition.
        /// </summary>
        public static readonly Guid Problem_NoTexture = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsGlossmapMissing"/> condition.
        /// </summary>
        public static readonly Guid Problem_NoGlossmap = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTextureInvalidChars"/> condition.
        /// </summary>
        public static readonly Guid Problem_TextureInvalidChars = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTextureTooLong"/> condition.
        /// </summary>
        public static readonly Guid Problem_TextureTooLong = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsGlossmapInvalidChars"/> condition.
        /// </summary>
        public static readonly Guid Problem_GlossmapInvalidChars = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsGlossmapTooLong"/> condition.
        /// </summary>
        public static readonly Guid Problem_GlossmapTooLong = Guid.NewGuid();

        /// <inheritdoc />
        public override bool IsDuplicateOf(IGraphic graphic)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether sufficient geometry has been supplied.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In order to render correctly, both textured and fallback geometry must be present. This property will return <c>true</c> if:
        /// <list type="bullet">
        ///   <item><term>All three geometry collections are empty</term></item>
        ///   <item><term>
        ///     <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.TextureGeometry"/> contains <see cref="T:Digitalis.LDTools.DOM.API.IGraphic"/>s but
        ///     <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.FallbackGeometry"/> does not (or vice-versa) and <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.SharedGeometry"/>
        ///     does not contain <see cref="T:Digitalis.LDTools.DOM.API.IGraphic"/>s</term></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_NoGeometry"/>
        public bool IsGeometryMissing
        {
            get
            {
                if ((TextureGeometry as GeometryCollection).ContainsGraphics && (FallbackGeometry as GeometryCollection).ContainsGraphics)
                    return false;

                return !(SharedGeometry as GeometryCollection).ContainsGraphics;
            }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Texture"/> is missing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Texture"/> is not set, or does not refer to a valid texture image file.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_NoTexture"/>
        public bool IsTextureMissing { get { Validate(); return _isTextureMissing; } set { _isTextureMissing = value; } }
        private bool _isTextureMissing;

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Glossmap"/> is missing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Glossmap"/> is set but does not refer to a valid glossmap image file.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_NoGlossmap"/>
        public bool IsGlossmapMissing { get { Validate(); return _isGlossmapMissing; } set { _isGlossmapMissing = value; } }
        private bool _isGlossmapMissing;

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Texture"/> contains characters disallowed by
        /// <see href="http://www.ldraw.org/article/512.html#file_name">the File Format Restrictions for the Official Library</see>.
        /// </summary>
        /// <seealso cref="Problem_TextureInvalidChars"/>
        public bool IsTextureInvalidChars { get { return (null != Texture && !Regex_Path_InvalidChars.IsMatch(Texture)); } }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Texture"/> exceeds the maximum number of characters permitted by
        /// <see href="http://www.ldraw.org/article/512.html#file_name">the File Format Restrictions for the Official Library</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Texture"/> is more than 21 characters in length.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_TextureTooLong"/>
        public bool IsTextureTooLong { get { return (null != Texture && Texture.Length > Max_PathLength); } }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Glossmap"/> contains characters disallowed by
        /// <see href="http://www.ldraw.org/article/512.html#file_name">the File Format Restrictions for the Official Library</see>.
        /// </summary>
        /// <seealso cref="Problem_GlossmapInvalidChars"/>
        public bool IsGlossmapInvalidChars { get { return (null != Glossmap && !Regex_Path_InvalidChars.IsMatch(Glossmap)); } }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Glossmap"/> exceeds the maximum number of characters permitted by
        /// <see href="http://www.ldraw.org/article/512.html#file_name">the File Format Restrictions for the Official Library</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Glossmap"/> is more than 21 characters in length.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_GlossmapTooLong"/>
        public bool IsGlossmapTooLong { get { return (null != Glossmap && Glossmap.Length > Max_PathLength); } }

        private void Validate()
        {
            if (_validated)
                return;

            _validated = true;

            string path = GetTexturePath(Texture);

            if (null == path)
            {
                IsTextureMissing = true;
            }
            else
            {
                try
                {
                    using (Image img = Image.FromFile(path))
                    {
                        IsTextureMissing = !img.RawFormat.Equals(ImageFormat.Png);
                    }
                }
                catch
                {
                    IsTextureMissing = true;
                }
            }

            if (null != Glossmap)
            {
                path = GetTexturePath(Glossmap);

                if (null == path)
                {
                    IsGlossmapMissing = (null != Glossmap);
                }
                else
                {
                    try
                    {
                        using (Image img = Image.FromFile(path))
                        {
                            IsGlossmapMissing = (!img.RawFormat.Equals(ImageFormat.Png) || !Image.IsAlphaPixelFormat(img.PixelFormat));
                        }
                    }
                    catch
                    {
                        IsGlossmapMissing = true;
                    }
                }
            }
        }

        private bool _validated = false;

        /// <inheritdoc />
        public override bool HasProblems(CodeStandards mode)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return IsGeometryMissing || IsTextureMissing || IsGlossmapMissing || IsTextureInvalidChars || IsTextureTooLong || IsGlossmapInvalidChars || IsGlossmapTooLong || base.HasProblems(mode);
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <list type="table">
        ///   <listheader><term>Problem</term><description><see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/></description></listheader>
        ///   <item>
        ///     <term><see cref="T:Digitalis.LDTools.DOM.Graphic.Problem_CoordinatesColocated"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///     <term><see cref="Problem_NoGeometry"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///     <term><see cref="Problem_NoTexture"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///     <term><see cref="Problem_NoGlossmap"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///     <term><see cref="Problem_TextureInvalidChars"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///     <term><see cref="Problem_TextureTooLong"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///     <term><see cref="Problem_GlossmapInvalidChars"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///     <term><see cref="Problem_GlossmapTooLong"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        public override ICollection<IProblemDescriptor> Analyse(CodeStandards mode)
        {
            ICollection<IProblemDescriptor> problems = base.Analyse(mode);

            if (IsColocated)
                problems.Add(new ColocationProblem(this, Graphic.Problem_CoordinatesColocated, Severity.Error, Resources.Analytics_Colocation, null));

            if (IsGeometryMissing)
                problems.Add(new GeometryMissingProblem(this));

            if (IsTextureMissing)
                problems.Add(new TextureMissingProblem(this));

            if (IsGlossmapMissing)
                problems.Add(new GlossmapMissingProblem(this));

            if (CodeStandards.Full != mode)
            {
                if (IsTextureInvalidChars)
                    problems.Add(new TexturePathInvalidCharsProblem(this));

                if (IsTextureTooLong)
                    problems.Add(new TexturePathTooLongProblem(this));

                if (IsGlossmapInvalidChars)
                    problems.Add(new GlossmapPathInvalidCharsProblem(this));

                if (IsGlossmapTooLong)
                    problems.Add(new GlossmapPathTooLongProblem(this));
            }

            return problems;
        }

        #endregion Analytics

        #region Attributes

        /// <inheritdoc />
        public override bool IsVisible
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _isVisible;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                foreach (IElement element in TextureGeometry)
                {
                    IGraphic graphic = element as IGraphic;

                    if (null != graphic)
                        graphic.IsVisible = value;
                }

                foreach (IElement element in SharedGeometry)
                {
                    IGraphic graphic = element as IGraphic;

                    if (null != graphic)
                        graphic.IsVisible = value;
                }

                foreach (IElement element in FallbackGeometry)
                {
                    IGraphic graphic = element as IGraphic;

                    if (null != graphic)
                        graphic.IsVisible = value;
                }
            }
        }
        private bool _isVisible = true;

        /// <inheritdoc />
        public override bool IsGhosted
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _isGhosted;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                foreach (IElement element in TextureGeometry)
                {
                    IGraphic graphic = element as IGraphic;

                    if (null != graphic)
                        graphic.IsGhosted = value;
                }

                foreach (IElement element in SharedGeometry)
                {
                    IGraphic graphic = element as IGraphic;

                    if (null != graphic)
                        graphic.IsGhosted = value;
                }

                foreach (IElement element in FallbackGeometry)
                {
                    IGraphic graphic = element as IGraphic;

                    if (null != graphic)
                        graphic.IsGhosted = value;
                }
            }
        }
        private bool _isGhosted = false;

        #endregion Attributes

        #region Cloning

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            ITexmap texMap = (ITexmap)obj;

            foreach (IElement e in TextureGeometry)
            {
                texMap.TextureGeometry.Add((IElement)e.Clone());
            }

            texMap.TextureGeometry.IsLocked = TextureGeometry.IsLocked;

            foreach (IElement e in SharedGeometry)
            {
                texMap.SharedGeometry.Add((IElement)e.Clone());
            }

            texMap.SharedGeometry.IsLocked = SharedGeometry.IsLocked;

            foreach (IElement e in FallbackGeometry)
            {
                texMap.FallbackGeometry.Add((IElement)e.Clone());
            }

            texMap.FallbackGeometry.IsLocked = FallbackGeometry.IsLocked;

            texMap.Projection       = Projection;
            texMap.Coordinates      = Coordinates;
            texMap.HorizontalExtent = HorizontalExtent;
            texMap.VerticalExtent   = VerticalExtent;
            texMap.Texture          = Texture;
            texMap.Glossmap         = Glossmap;
            base.InitializeObject(obj);
        }

        #endregion Cloning

        #region Code-generation

        /// <inheritdoc />
        protected override StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if (CodeStandards.PartsLibrary == codeFormat)
            {
                if (!IsVisible || IsGhosted)
                    return sb;

                if (0 == TextureGeometry.Count && 0 == SharedGeometry.Count && 0 == FallbackGeometry.Count)
                    return sb;
            }

            sb = base.GenerateCode(sb, codeFormat, overrideColour, ref transform, winding);

            bool shortMode;

            if (0 == TextureGeometry.Count && !TextureGeometry.IsLocalLock &&
                1 == SharedGeometry.Count && !SharedGeometry.IsLocalLock &&
                0 == FallbackGeometry.Count && !FallbackGeometry.IsLocalLock)
            {
                sb.Append("0 !TEXMAP NEXT ");
                shortMode = true;
            }
            else
            {
                sb.Append("0 !TEXMAP START ");
                shortMode = false;
            }

            switch (Projection)
            {
                case TexmapProjection.Planar:
                    sb.Append("PLANAR ");
                    break;

                case TexmapProjection.Cylindrical:
                    sb.Append("CYLINDRICAL ");
                    break;

                case TexmapProjection.Spherical:
                    sb.Append("SPHERICAL ");
                    break;
            }

            IPage page = Page;
            uint  ndp;

            if (null != page && (PageType.Primitive == page.PageType || PageType.HiresPrimitive == page.PageType))
                ndp = Configuration.DecimalPlacesPrimitives;
            else
                ndp = Configuration.DecimalPlacesCoordinates;

            string fmt = Configuration.Formatters[ndp];

            foreach (Vector3d v in Coordinates)
            {
                sb.AppendFormat("{0} {1} {2} ",
                                v.X.ToString(fmt, CultureInfo.InvariantCulture),
                                v.Y.ToString(fmt, CultureInfo.InvariantCulture),
                                v.Z.ToString(fmt, CultureInfo.InvariantCulture));
            }

            if (TexmapProjection.Cylindrical == Projection || TexmapProjection.Spherical == Projection)
                sb.AppendFormat("{0} ", HorizontalExtent.ToString(fmt, CultureInfo.InvariantCulture));

            if (TexmapProjection.Spherical == Projection)
                sb.AppendFormat("{0} ", VerticalExtent.ToString(fmt, CultureInfo.InvariantCulture));

            if (Texture.Contains(" "))
                sb.AppendFormat("\"{0}\"", Texture);
            else
                sb.Append(Texture);

            if (!string.IsNullOrWhiteSpace(Glossmap))
            {
                if (Glossmap.Contains(" "))
                    sb.AppendFormat(" GLOSSMAP \"{0}\"", Glossmap);
                else
                    sb.AppendFormat(" GLOSSMAP {0}", Glossmap);
            }

            sb.Append(LineTerminator);

            if (!shortMode)
                sb = TextureGeometry.ToCode(sb, codeFormat, overrideColour, ref transform, winding);

            sb = SharedGeometry.ToCode(sb, codeFormat, overrideColour, ref transform, winding);

            if (!shortMode)
            {
                if (FallbackGeometry.Count > 0 || FallbackGeometry.IsLocalLock)
                {
                    sb.AppendFormat("0 !TEXMAP FALLBACK{0}", LineTerminator);
                    sb = FallbackGeometry.ToCode(sb, codeFormat, overrideColour, ref transform, winding);
                }

                sb.AppendFormat("0 !TEXMAP END{0}", LineTerminator);
            }

            return sb;
        }

        #endregion Code-generation

        #region Collection-management

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IElement> ItemsAdded;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IElement> ItemsRemoved;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListReplacedEventHandler<IElement> ItemsReplaced;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IElement> CollectionCleared;

        /// <inheritdoc />
        public bool AllowsTopLevelElements { get { return false; } }

        /// <inheritdoc />
        public InsertCheckResult CanInsert(IElement element, InsertCheckFlags flags)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return InsertCheckResult.NotSupported;
        }

        /// <inheritdoc />
        public InsertCheckResult CanReplace(IElement elementToInsert, IElement elementToReplace, InsertCheckFlags flags)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return InsertCheckResult.NotSupported;
        }

        /// <inheritdoc />
        public bool ContainsColourElements
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return false;
            }
        }

        /// <inheritdoc />
        public bool ContainsBFCFlagElements
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return false;
            }
        }

        /// <inheritdoc />
        public bool HasLockedDescendants
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return TextureGeometry.HasLockedDescendants || SharedGeometry.HasLockedDescendants || FallbackGeometry.HasLockedDescendants;
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return SharedGeometry.Count + FallbackGeometry.Count;
            }
        }

        /// <inheritdoc />
        public int IndexOf(IElement element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            int idx = SharedGeometry.IndexOf(element);

            if (-1 != idx)
                return idx;

            idx = FallbackGeometry.IndexOf(element);

            if (-1 == idx)
                return -1;

            return idx + SharedGeometry.Count;
        }

        /// <inheritdoc />
        public bool Contains(IElement element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return SharedGeometry.Contains(element) || FallbackGeometry.Contains(element);
        }

        /// <inheritdoc />
        public IElement this[int index]
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();

                if (index < SharedGeometry.Count)
                    return SharedGeometry[index];

                return FallbackGeometry[index - SharedGeometry.Count];
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                throw new NotSupportedException("Texmaps cannot have their contents changed");
            }
        }

        /// <inheritdoc />
        public void Add(IElement element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            throw new NotSupportedException("Texmaps cannot have their contents changed");
        }

        /// <inheritdoc />
        public void Insert(int index, IElement element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            throw new NotSupportedException("Texmaps cannot have their contents changed");
        }

        /// <inheritdoc />
        public bool Remove(IElement element)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            throw new NotSupportedException("Texmaps cannot have their contents changed");
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            throw new NotSupportedException("Texmaps cannot have their contents changed");
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            throw new NotSupportedException("Texmaps cannot have their contents changed");
        }

        /// <inheritdoc />
        public void CopyTo(IElement[] array, int arrayIndex)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null == array)
                throw new ArgumentNullException();

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException();

            if (Count > array.Length - arrayIndex)
                throw new ArgumentException();

            if (0 != SharedGeometry.Count)
                SharedGeometry.CopyTo(array, arrayIndex);

            FallbackGeometry.CopyTo(array, arrayIndex + SharedGeometry.Count);
        }

        /// <inheritdoc />
        public IEnumerator<IElement> GetEnumerator()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            foreach (IElement element in SharedGeometry)
            {
                yield return element;
            }

            foreach (IElement element in FallbackGeometry)
            {
                yield return element;
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void OnGeometryChanged(IDOMObject sender, ObjectChangedEventArgs e)
        {
            base.OnChanged(e);

            switch (e.Operation)
            {
                case "IsVisibleChanged":
                case "IsGhostedChanged":
                case "ItemsAdded":
                case "ItemsRemoved":
                case "ItemsReplaced":
                case "CollectionCleared":
                    bool oldIsTexmapVisible = IsVisible;
                    bool isTexmapVisible    = IsTexmapVisible();

                    if (oldIsTexmapVisible != isTexmapVisible)
                    {
                        _isVisible = isTexmapVisible;
                        OnIsVisibleChanged(new PropertyChangedEventArgs<bool>(oldIsTexmapVisible, isTexmapVisible));
                    }

                    bool oldIsTexmapGhosted = IsGhosted;
                    bool isTexmapGhosted    = IsTexmapGhosted();

                    if (oldIsTexmapGhosted != isTexmapGhosted)
                    {
                        _isGhosted = isTexmapGhosted;
                        OnIsGhostedChanged(new PropertyChangedEventArgs<bool>(oldIsTexmapGhosted, isTexmapGhosted));
                    }
                    break;
            }
        }

        private bool IsTexmapVisible()
        {
            if (0 == TextureGeometry.Count && 0 == SharedGeometry.Count && 0 == FallbackGeometry.Count)
                return true;

            foreach (IElement element in TextureGeometry)
            {
                IGraphic graphic = element as IGraphic;

                if (null != graphic && graphic.IsVisible)
                    return true;
            }

            foreach (IElement element in SharedGeometry)
            {
                IGraphic graphic = element as IGraphic;

                if (null != graphic && graphic.IsVisible)
                    return true;
            }

            foreach (IElement element in FallbackGeometry)
            {
                IGraphic graphic = element as IGraphic;

                if (null != graphic && graphic.IsVisible)
                    return true;
            }

            return false;
        }

        private bool IsTexmapGhosted()
        {
            if (0 == TextureGeometry.Count && 0 == SharedGeometry.Count && 0 == FallbackGeometry.Count)
                return false;

            foreach (IElement element in TextureGeometry)
            {
                IGraphic graphic = element as IGraphic;

                if (null != graphic && !graphic.IsGhosted)
                    return false;
            }

            foreach (IElement element in SharedGeometry)
            {
                IGraphic graphic = element as IGraphic;

                if (null != graphic && !graphic.IsGhosted)
                    return false;
            }

            foreach (IElement element in FallbackGeometry)
            {
                IGraphic graphic = element as IGraphic;

                if (null != graphic && !graphic.IsGhosted)
                    return false;
            }

            return true;
        }

        #endregion Collection-management

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDTexmap"/> class with default values.
        /// </summary>
        public LDTexmap()
        {
            // to shut the compiler up - these come from IElementCollection but since an ITexmap is immutable they are not needed
            if (null != ItemsAdded) { }
            if (null != ItemsRemoved) { }
            if (null != ItemsReplaced) { }
            if (null != CollectionCleared) { }

            // to shut the compiler up temporarily
            if (null != TextureFileChanged) { }
            if (null != GlossmapFileChanged) { }

            GeometryCollection geometry  = new GeometryCollection(this, TexmapGeometryType.Texture, Resources.Geometry_Texture);
            geometry.Changed            += OnGeometryChanged;
            TextureGeometry              = geometry;

            geometry          = new GeometryCollection(this, TexmapGeometryType.Shared, Resources.Geometry_Shared);
            geometry.Changed += OnGeometryChanged;
            SharedGeometry    = geometry;

            geometry          = new GeometryCollection(this, TexmapGeometryType.Fallback, Resources.Geometry_Fallback);
            geometry.Changed += OnGeometryChanged;
            FallbackGeometry  = geometry;

            _projection.ValueChanged += delegate(object sender, PropertyChangedEventArgs<TexmapProjection> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != ProjectionChanged)
                    ProjectionChanged(this, e);

                OnChanged(this, "ProjectionChanged", e);
            };

            _horizontalExtent.ValueChanged += delegate(object sender, PropertyChangedEventArgs<double> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != HorizontalExtentChanged)
                    HorizontalExtentChanged(this, e);

                OnChanged(this, "HorizontalExtentChanged", e);
            };

            _verticalExtent.ValueChanged += delegate(object sender, PropertyChangedEventArgs<double> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != VerticalExtentChanged)
                    VerticalExtentChanged(this, e);

                OnChanged(this, "VerticalExtentChanged", e);
            };

            _texture.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != TextureChanged)
                    TextureChanged(this, e);

                OnChanged(this, "TextureChanged", e);
            };

            _glossmap.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != GlossmapChanged)
                    GlossmapChanged(this, e);

                OnChanged(this, "GlossmapChanged", e);
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDTexmap"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this texmap.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw texmap code.</exception>
        /// <remarks>
        /// <para>
        /// Only the <i>!TEXMAP START</i> and <i>!TEXMAP NEXT</i> meta-commands are recognised. Any geometry required must be loaded separately
        /// and added to either <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.TextureGeometry"/>, <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.SharedGeometry"/> or
        /// <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.FallbackGeometry"/> as appropriate.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// LDTexmap = new LDTexmap("0 !TEXMAP START PLANAR 0 0 0 1 1 1 2 2 2 texture.png");
        /// </code>
        /// </example>
        public LDTexmap(string code)
            : this()
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 14)
                throw new FormatException("LDraw texmap code must have at least 14 fields");

            if ("0" != fields[0] || "!TEXMAP" != fields[1].ToUpper() || ("START" != fields[2].ToUpper() && "NEXT" != fields[2].ToUpper()))
                throw new FormatException("LDraw texmap code must start with '0 !TEXMAP START' or '0 !TEXMAP NEXT'");

            int texture;

            switch (fields[3].ToUpper())
            {
                case "PLANAR":
                    Projection = TexmapProjection.Planar;
                    texture    = 13;
                    break;

                case "CYLINDRICAL":
                    if (fields.Length < 15)
                        throw new FormatException("LDraw texmap code must have at least 15 fields for projection CYLINDRICAL");

                    Projection       = TexmapProjection.Cylindrical;
                    HorizontalExtent = double.Parse(fields[13]);
                    texture          = 14;
                    break;

                case "SPHERICAL":
                    if (fields.Length < 16)
                        throw new FormatException("LDraw texmap code must have at least 16 fields for projection SPHERICAL");

                    Projection       = TexmapProjection.Spherical;
                    HorizontalExtent = double.Parse(fields[13]);
                    VerticalExtent   = double.Parse(fields[14]);
                    texture          = 15;
                    break;

                default:
                    throw new FormatException("Unrecognised texmap projection '" + fields[3] + "'");
            }

            Coordinates = new Vector3d[] { new Vector3d(double.Parse(fields[4], CultureInfo.InvariantCulture), double.Parse(fields[5], CultureInfo.InvariantCulture), double.Parse(fields[6], CultureInfo.InvariantCulture)),
                                           new Vector3d(double.Parse(fields[7], CultureInfo.InvariantCulture), double.Parse(fields[8], CultureInfo.InvariantCulture), double.Parse(fields[9], CultureInfo.InvariantCulture)),
                                           new Vector3d(double.Parse(fields[10], CultureInfo.InvariantCulture), double.Parse(fields[11], CultureInfo.InvariantCulture), double.Parse(fields[12], CultureInfo.InvariantCulture)) };

            string texturePath  = fields[texture];
            string glossmapPath = null;
            int startIdx;

            if ('"' == texturePath[0])
            {
                startIdx    = code.IndexOf('"');
                texturePath = ParsePath(code, startIdx);
                startIdx   += texturePath.Length + 2;

                if (code.Length > startIdx)
                {
                    glossmapPath = code.Substring(startIdx).Trim();

                    if (glossmapPath.StartsWith("GLOSSMAP", StringComparison.OrdinalIgnoreCase))
                        glossmapPath = glossmapPath.Substring("GLOSSMAP".Length).Trim();
                    else
                        glossmapPath = null;
                }
            }
            else if (fields.Length > texture + 2 && "GLOSSMAP" == fields[texture + 1].ToUpper())
            {
                glossmapPath = fields[texture + 2];
            }

            Texture = texturePath.Trim();

            if (!String.IsNullOrWhiteSpace(glossmapPath))
            {
                if ('"' == glossmapPath[0])
                    glossmapPath = ParsePath(glossmapPath, 0);

                Glossmap = glossmapPath.Trim();
            }
        }

        private string ParsePath(string code, int startIdx)
        {
            string path = code.Substring(startIdx + 1);
            int idx     = 0;

            while (true)
            {
                idx = path.IndexOf('"', idx);

                if (-1 == idx)
                    throw new FormatException(code);

                if ('\\' != path[idx - 1])
                {
                    path = path.Substring(0, idx);
                    return path;
                }

                idx++;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                ((GeometryCollection)TextureGeometry)._texmap = null;
                TextureGeometry.Dispose();
                ((GeometryCollection)SharedGeometry)._texmap = null;
                SharedGeometry.Dispose();
                ((GeometryCollection)FallbackGeometry)._texmap = null;
                FallbackGeometry.Dispose();
            }
        }

        #endregion Constructor

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDTexmapEditor", typeof(LDTexmap));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDTexmap"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override bool HasEditor
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return (null != EditorFactory);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDTexmap"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override IElementEditor GetEditor()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != EditorFactory)
                return EditorFactory.Invoke(new object[] { this }) as IElementEditor;

            return null;
        }

        #endregion Editor

        #region Geometry

        /// <inheritdoc />
        public override Box3d BoundingBox
        {
            get
            {
                Box3d bounds = TextureGeometry.BoundingBox;

                if ((TextureGeometry as GeometryCollection).ContainsGraphics)
                {
                    bounds = TextureGeometry.BoundingBox;

                    if ((SharedGeometry as GeometryCollection).ContainsGraphics)
                        bounds.Union(SharedGeometry.BoundingBox);

                    if ((FallbackGeometry as GeometryCollection).ContainsGraphics)
                        bounds.Union(FallbackGeometry.BoundingBox);
                }
                else if ((SharedGeometry as GeometryCollection).ContainsGraphics)
                {
                    bounds = SharedGeometry.BoundingBox;

                    if ((TextureGeometry as GeometryCollection).ContainsGraphics)
                        bounds.Union(TextureGeometry.BoundingBox);

                    if ((FallbackGeometry as GeometryCollection).ContainsGraphics)
                        bounds.Union(FallbackGeometry.BoundingBox);
                }
                else if ((FallbackGeometry as GeometryCollection).ContainsGraphics)
                {
                    bounds = FallbackGeometry.BoundingBox;

                    if ((SharedGeometry as GeometryCollection).ContainsGraphics)
                        bounds.Union(SharedGeometry.BoundingBox);

                    if ((TextureGeometry as GeometryCollection).ContainsGraphics)
                        bounds.Union(TextureGeometry.BoundingBox);
                }

                return bounds;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// This is the centre of the <see cref="BoundingBox"/>.
        /// </para>
        /// </remarks>
        public override Vector3d Origin { get { return BoundingBox.Centre; } }

        /// <inheritdoc />
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The <see cref="LDTexmap"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see>.</exception>
        public override void Transform(ref Matrix4d transform)
        {
            if (IsLocked)
                throw new ElementLockedException();

            TextureGeometry.Transform(ref transform);
            SharedGeometry.Transform(ref transform);
            FallbackGeometry.Transform(ref transform);
        }

        /// <inheritdoc />
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The <see cref="LDTexmap"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see>.</exception>
        public override void ReverseWinding()
        {
            if (IsLocked)
                throw new ElementLockedException();

            TextureGeometry.ReverseWinding();
            SharedGeometry.ReverseWinding();
            FallbackGeometry.ReverseWinding();
        }

        #endregion Geometry

        #region Properties

        /// <inheritdoc />
        public ITexmapGeometry TextureGeometry { get; private set; }

        /// <inheritdoc />
        public ITexmapGeometry SharedGeometry { get; private set; }

        /// <inheritdoc />
        public ITexmapGeometry FallbackGeometry { get; private set; }

        /// <inheritdoc />
        public TexmapProjection Projection
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _projection.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsLocked)
                    throw new ElementLockedException();

                if (!Enum.IsDefined(typeof(TexmapProjection), value))
                    throw new ArgumentOutOfRangeException();

                if (value != Projection)
                    _projection.Value = value;
            }
        }
        private UndoableProperty<TexmapProjection> _projection = new UndoableProperty<TexmapProjection>(TexmapProjection.Planar);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<TexmapProjection> ProjectionChanged;

        /// <inheritdoc />
        public Vector3d Point1 { get { return CoordinatesArray[0]; } set { Coordinates = new Vector3d[] { value, Point2, Point3 }; } }

        /// <inheritdoc />
        public Vector3d Point2 { get { return CoordinatesArray[1]; } set { Coordinates = new Vector3d[] { Point1, value, Point3 }; } }

        /// <inheritdoc />
        public Vector3d Point3 { get { return CoordinatesArray[2]; } set { Coordinates = new Vector3d[] { Point1, Point2, value }; } }

        /// <inheritdoc />
        public double HorizontalExtent
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _horizontalExtent.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (value <= 0.0 || value > 360.0)
                    throw new ArgumentOutOfRangeException("HorizontalExtent must be in the range 0<n<=360");

                if (value != HorizontalExtent)
                    _horizontalExtent.Value = value;
            }
        }
        private UndoableProperty<double> _horizontalExtent = new UndoableProperty<double>(360.0);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<double> HorizontalExtentChanged;

        /// <inheritdoc />
        public double VerticalExtent
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _verticalExtent.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (value <= 0.0 || value > 360.0)
                    throw new ArgumentOutOfRangeException("VerticalExtent must be in the range 0<n<=360");

                if (value != VerticalExtent)
                    _verticalExtent.Value = value;
            }
        }
        private UndoableProperty<double> _verticalExtent = new UndoableProperty<double>(360.0);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<double> VerticalExtentChanged;

        /// <inheritdoc />
        public string Texture
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _texture.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (value == Texture)
                    return;

                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();

                _validated = false;
                _texture.Value = value;
            }
        }
        private UndoableProperty<string> _texture = new UndoableProperty<string>(Resources.Undefined);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> TextureChanged;

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Raises the <see cref="TextureFileChanged"/> event when the image file changes on disk.
        /// </para>
        /// </remarks>
        public string TexturePath
        {
            get
            {
                if (!IsTextureMissing)
                    return GetTexturePath(Texture);

                return null;
            }
        }

        /// <summary>
        /// Occurs when the <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Texture"/> image file changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event will be raised when the file referred to by <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Texture"/> is loaded or changes on disk, or when
        /// <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.TexturePath"/> changes.
        /// </para>
        /// </remarks>
        [field: NonSerialized]
        public event EventHandler TextureFileChanged;

        /// <inheritdoc />
        public string Glossmap
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _glossmap.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (value == Glossmap)
                    return;

                if (String.IsNullOrWhiteSpace(value))
                    value = null;

                _validated      = false;
                _glossmap.Value = value;
            }
        }
        private UndoableProperty<string> _glossmap = new UndoableProperty<string>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> GlossmapChanged;

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Raises the <see cref="GlossmapFileChanged"/> event when the image file changes on disk.
        /// </para>
        /// </remarks>
        public string GlossmapPath
        {
            get
            {
                if (null != Glossmap && !IsGlossmapMissing)
                    return GetTexturePath(Glossmap);

                return null;
            }
        }

        /// <summary>
        /// Occurs when the <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Glossmap"/> image file changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event will be raised when the file referred to by <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Glossmap"/> is loaded or changes on disk, or when
        /// <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.GlossmapPath"/> changes.
        /// </para>
        /// </remarks>
        [field: NonSerialized]
        public event EventHandler GlossmapFileChanged;

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override DOMObjectType ObjectType { get { return DOMObjectType.Texmap; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.TexmapIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.Texmap; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns the filename component of <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.Texture"/>, without its extension.
        /// </para>
        /// </remarks>
        public override string Description { get { return Path.GetFileNameWithoutExtension(Texture); } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="F:System.String.Empty"/>.
        /// </para>
        /// </remarks>
        public override string ExtendedDescription { get { return String.Empty; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.ITexmap"/>s is not a state-element.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.ITexmap"/>s is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return true; } }

        /// <inheritdoc />
        public override uint OverrideableColourValue { get { return Palette.MainColour; } }

        /// <inheritdoc />
        public override bool ColourValueEnabled { get { return false; } }

        /// <inheritdoc />
        public override uint CoordinatesCount { get { return 3; } }

        #endregion Self-description

        #region Texture-management

        private string GetTexturePath(string filename)
        {
            string path;

            if (Path.IsPathRooted(filename))
                return filename;

            IDocument doc = Document;

            if (null != doc && null != doc.Filepath && Path.IsPathRooted(doc.Filepath))
            {
                string folder = Path.GetDirectoryName(doc.Filepath);

                path = Path.Combine(folder, "textures", filename);

                if (File.Exists(path))
                    return path;

                path = Path.Combine(folder, filename);

                if (File.Exists(path))
                    return path;
            }

            foreach (string folder in Configuration.FullSearchPath)
            {
                path = Path.Combine(folder, "textures", filename);

                if (File.Exists(path))
                    return path;
            }

            foreach (string folder in Configuration.FullSearchPath)
            {
                path = Path.Combine(folder, filename);

                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        #endregion Texture-management
    }
}
