#region License

//
// LDReference.cs
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
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.LDTools.Library;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IReference"/>. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To speed up loading and to save resources, <b>LDReference</b> maintains a cache of <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/>s which have been loaded
    /// in order to resolve <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>. The cache itself is private, but events are available to notify interested parties
    /// when cache-entries are <see cref="CacheEntryAdded">added</see> and <see cref="CacheEntryRemoved">removed</see>.
    /// </para>
    /// <para>
    /// The cache is thread-safe, and <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/>s in it are <see cref="P:Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>.
    /// </para>
    /// </remarks>
    [Serializable]
    [DefaultIcon(typeof(Resources), "PartIcon")]
    [TypeName(typeof(Resources), "Reference")]
    [ElementFlags(ElementFlags.HasEditor)]
    public sealed class LDReference : Graphic, IReference
    {
        #region Inner types

        private class SetInhibitFlagAction : IAction
        {
            private LDReference _reference;

            public SetInhibitFlagAction(LDReference r)
            {
                _reference = r;
            }

            public void Apply()
            {
            }

            public void Revert()
            {
                _reference._inhibitTargetNameUpdate = true;
            }
        }

        // Rule:   'Matrix' must not be singular
        // Type:   Error
        // Source: http://www.ldraw.org/article/512.html#matrix
        private class SingularMatrixProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_MatrixSingular; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_MatrixSingular; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public SingularMatrixProblem(LDReference r, bool fixable)
            {
                Element = r;

                if (fixable)
                    Fixes = new IFixDescriptor[] { new Fix(r) };
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_MatrixSingular_RepairMatrix; } }
                public string Instruction { get { return Resources.Analytics_FixThis; } }
                public string Action { get { return Resources.Analytics_FixedMatrix; } }
                public bool IsIntraElement { get { return true; } }

                private LDReference _element;

                public Fix(LDReference r)
                {
                    _element = r;
                }

                public bool Apply()
                {
                    // we can only fix the simple case: one all-zero row and one all-zero column
                    uint row = _element.MatrixZeroRows[0];
                    uint col = _element.MatrixZeroColumns[0];

                    Matrix4d matrix = _element.Matrix;

                    switch (row)
                    {
                        case 0:
                            switch (col)
                            {
                                case 0:
                                    matrix.M11 = 1.0;
                                    break;

                                case 1:
                                    matrix.M12 = 1.0;
                                    break;

                                case 2:
                                    matrix.M13 = 1.0;
                                    break;
                            }
                            break;

                        case 1:
                            switch (col)
                            {
                                case 0:
                                    matrix.M21 = 1.0;
                                    break;

                                case 1:
                                    matrix.M22 = 1.0;
                                    break;

                                case 2:
                                    matrix.M23 = 1.0;
                                    break;
                            }
                            break;

                        case 2:
                            switch (col)
                            {
                                case 0:
                                    matrix.M31 = 1.0;
                                    break;

                                case 1:
                                    matrix.M32 = 1.0;
                                    break;

                                case 2:
                                    matrix.M33 = 1.0;
                                    break;
                            }
                            break;
                    }

                    _element.Matrix = matrix;

                    return true;
                }
            }
        }

        // Rule:   Some types of 'Target' should not be scaled
        // Type:   Depends on the Target-type
        // Source: Convention established on the Parts Tracker
        private class TargetScaledProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TargetScaled; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get; private set; }
            public string Description { get; private set; }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public TargetScaledProblem(LDReference r, string description, Severity severity, bool isScaleX, bool isScaleY, bool isScaleZ)
            {
                Element     = r;
                Description = description;
                Severity    = severity;
                Fixes       = new IFixDescriptor[] { new Fix(r, isScaleX, isScaleY, isScaleZ) };
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TargetScaled_UnscaleMatrix; } }
                public string Instruction { get { return Resources.Analytics_FixThis; } }
                public string Action { get { return Resources.Analytics_FixedMatrix; } }
                public bool IsIntraElement { get { return true; } }

                private LDReference _ref;
                private Matrix4d _matrix;

                public Fix(LDReference r, bool isScaleX, bool isScaleY, bool isScaleZ)
                {
                    _ref = r;

                    Matrix4d matrix = _ref.Matrix;

                    if (isScaleX)
                        matrix.Row0.Normalize();

                    if (isScaleY)
                        matrix.Row1.Normalize();

                    if (isScaleZ)
                        matrix.Row2.Normalize();

                    _matrix = matrix;
                 }

                public bool Apply()
                {
                    _ref.Matrix = _matrix;
                    return true;
                }
            }
        }

        // Rule:   Some types of 'Target' should not be mirrored
        // Type:   Depends on the Target-type
        // Source: Convention established on the Parts Tracker
        private class TargetMirroredProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TargetMirrored; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get; private set; }
            public string Description { get; private set; }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public TargetMirroredProblem(LDReference r, string description, Severity severity, bool isMirrorX, bool isMirrorY, bool isMirrorZ)
            {
                Element = r;
                Description = description;
                Severity = severity;
                Fixes = new IFixDescriptor[] { new Fix(r, isMirrorX, isMirrorY, isMirrorZ) };
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TargetMirrored_UnmirrorMatrix; } }
                public string Instruction { get { return Resources.Analytics_FixThis; } }
                public string Action { get { return Resources.Analytics_FixedMatrix; } }
                public bool IsIntraElement { get { return true; } }

                private LDReference _ref;
                private Matrix4d _matrix;

                public Fix(LDReference r, bool isMirrorX, bool isMirrorY, bool isMirrorZ)
                {
                    _ref = r;

                    Matrix4d matrix    = _ref.Matrix;
                    Matrix3d transform = new Matrix3d(matrix.Row0.Xyz, matrix.Row1.Xyz, matrix.Row2.Xyz);
                    Matrix3d t         = transform * Matrix3d.Scale((isMirrorX) ? -1.0 : 1.0, (isMirrorY) ? -1.0 : 1.0, (isMirrorZ) ? -1.0 : 1.0);

                    _matrix = new Matrix4d(new Vector4d(t.Row0), new Vector4d(t.Row1), new Vector4d(t.Row2), matrix.Row3);
                }

                public bool Apply()
                {
                    _ref.Matrix = _matrix;
                    return true;
                }
            }
        }

        // Rule:   Some types of 'Target' should not be inverted
        // Type:   Depends on the Target-type
        // Source: Convention established on the Parts Tracker
        private class TargetInvertedProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TargetInverted; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get; private set; }
            public string Description { get; private set; }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public TargetInvertedProblem(LDReference r, string description, Severity severity)
            {
                Element     = r;
                Description = description;
                Severity    = severity;
                Fixes       = new IFixDescriptor[] { new Fix(r) };
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TargetInverted_ClearInvert; } }
                public string Instruction { get { return Resources.Analytics_FixThis; } }
                public string Action { get { return Resources.Analytics_FixedInvertedTarget; } }
                public bool IsIntraElement { get { return true; } }

                private LDReference _ref;

                public Fix(LDReference r)
                {
                    _ref = r;
                }

                public bool Apply()
                {
                    _ref.Invert = false;
                    return true;
                }
            }
        }

        // Rule:   'TargetName' should not refer to a '~Moved to' or 'Alias' target
        // Type:   Depends on the Target-type
        // Source: Convention established on the Parts Tracker
        private class TargetRedirectProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TargetRedirect; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get; private set; }
            public string Description { get; private set;  }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public TargetRedirectProblem(LDReference r, string description, Severity severity)
            {
                IReference redirect;
                IPage target        = r.Target;
                string redirectName  = Resources.Unknown;

                foreach (LDStep step in target)
                {
                    foreach (IDOMObject el in step)
                    {
                        redirect = el as LDReference;

                        if (null != redirect)
                        {
                            Fixes        = new IFixDescriptor[] { new Fix(r, redirect) };
                            redirectName = redirect.TargetName;
                            break;
                        }
                    }

                    if (null != Fixes)
                        break;
                }

                Element     = r;
                Description = String.Format(description, r.TargetName, redirectName);
                Severity    = severity;
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TargetRedirect_FollowRedirect; } }
                public string Instruction { get { return Resources.Analytics_FixTargetRedirect; } }
                public string Action { get { return Resources.Analytics_FixedTargetRedirect; } }
                public bool IsIntraElement { get { return true; } }

                private LDReference _ref;
                private string _targetName;
                private Matrix4d _matrix;
                private uint _colourValue;

                public Fix(LDReference r, IReference redirect)
                {
                    _ref         = r;
                    _targetName  = redirect.TargetName;
                    _matrix      = redirect.Matrix;
                    _colourValue = redirect.ColourValue;
                }

                public bool Apply()
                {
                    try
                    {
                        _ref.TargetName = _targetName;
                        _ref.Matrix    *= _matrix;

                        if (Palette.MainColour != _colourValue)
                            _ref.ColourValue = _colourValue;

                        return true;
                    }
                    catch
                    {
                        // ignore resolve-errors
                        return false;
                    }
                }
            }
        }

        // Rule:   'TargetName' should refer to an extant target
        // Type:   Depends on the Target-type
        // Source: Convention established on the Parts Tracker
        private class TargetMissingProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TargetMissing; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get; private set; }
            public string Description { get; private set; }

            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public TargetMissingProblem(LDReference r, Severity severity)
            {
                Element     = r;
                Description = String.Format(Resources.Analytics_TargetMissing, r.TargetName);
                Severity    = severity;
            }
        }

        // Rule:   'TargetName' should not refer to a target which would cause a circular-dependency
        // Type:   Error
        // Source: Breaks the document structure
        private class TargetCircularReferenceProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TargetCircularReference; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get; private set; }

            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public TargetCircularReferenceProblem(LDReference r)
            {
                Element = r;
                Description = String.Format(Resources.Analytics_TargetCircularReference, r.TargetName);
            }
        }

        // Rule:   'TargetName' should not refer to an unreleased target
        // Type:   Warning
        // Source: Convention established on the Parts Tracker
        private class TargetUnreleasedProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TargetUnreleased; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get; private set; }
            public string Description { get; private set; }

            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public TargetUnreleasedProblem(LDReference r, DocumentStatus status)
            {
                Element     = r;

                switch (status)
                {
                    case DocumentStatus.Held:
                        Description = String.Format(Resources.Analytics_TargetUnreleased_Held, r.TargetName);
                        Severity    = Severity.Warning;
                        break;

                    case DocumentStatus.Uncertified:
                        Description = String.Format(Resources.Analytics_TargetUnreleased_Uncertified, r.TargetName);
                        Severity    = Severity.Warning;
                        break;

                    case DocumentStatus.Unreleased:
                    case DocumentStatus.ReadyForRelease:
                    case DocumentStatus.Certified:
                        Description = String.Format(Resources.Analytics_TargetUnreleased_Unreleased, r.TargetName);
                        Severity    = Severity.Information;
                        break;
                }
            }
        }

        private class ChangeColour : IFixDescriptor
        {
            public Guid Guid { get { return Fix_ColourInvalid_SetToMainColour; } }
            public string Instruction { get; private set; }
            public string Action { get { return Resources.Analytics_FixedInvalidColour; } }
            public bool IsIntraElement { get { return true; } }

            private LDReference _ref;

            public ChangeColour(LDReference r)
            {
                _ref        = r;
                Instruction = String.Format(Resources.Analytics_FixInvalidColour, Palette.SystemPalette[Palette.MainColour].Name);
            }

            public bool Apply()
            {
                _ref.ColourValue = Palette.MainColour;
                return true;
            }
        }

        #endregion Inner types

        #region Internals

        // temporary storage for the original line of code - used by the LDPage/LDDocument parser to generate useful error-messages
        internal string LDrawCode;

        // temporary storage for the original line of code - used by the LDPage/LDDocument parser to generate useful error-messages
        internal uint CodeLine;

        #endregion Internals

        #region Analytics

        private bool _validated = false;

        // in official files, TargetName must conform to this format
        private static readonly Regex Regex_TargetName_InvalidChars = new Regex(@"^[-A-Za-z0-9_\.\\]+$", RegexOptions.IgnoreCase);

        private static readonly Matrix4d InvertX = Matrix4d.Scale(-1.0, 1.0, 1.0);
        private static readonly Matrix4d InvertY = Matrix4d.Scale(1.0, -1.0, 1.0);
        private static readonly Matrix4d InvertZ = Matrix4d.Scale(1.0, 1.0, -1.0);

        // studs which may be scaled/mirrored in the y-direction
        // this list may be incomplete, but it's based on http://www.ldraw.org/library/primref/
        private static readonly List<string> ScalableStudNames = new List<string>(new string[] { "stud3.dat", "stud3a.dat", "stud4.dat", "stud4a.dat", "stud4s.dat", "stud4s2.dat", "stud16.dat" });

        private void Validate()
        {
            if (_validated)
                return;

            _validated = true;

            // 3 decimal-places is the minimum needed for reasonable accuracy
            int ndp           = Math.Max(3, (int)Configuration.DecimalPlacesTransforms - 2);
            List<uint> zeroes = new List<uint>();
            Matrix4d matrix   = Matrix;
            Matrix3d m        = new Matrix3d(matrix.Row0.Xyz, matrix.Row1.Xyz, matrix.Row2.Xyz);
            double   det      = Math.Round(m.Determinant, ndp);

            IsMatrixSingular = (0.0 == det);

            if (Vector3d.Zero == m.Row0)
                zeroes.Add(0);

            if (Vector3d.Zero == m.Row1)
                zeroes.Add(1);

            if (Vector3d.Zero == m.Row2)
                zeroes.Add(2);

            MatrixZeroRows = zeroes.ToArray();
            zeroes.Clear();

            if (Vector3d.Zero == m.Column0)
                zeroes.Add(0);

            if (Vector3d.Zero == m.Column1)
                zeroes.Add(1);

            if (Vector3d.Zero == m.Column2)
                zeroes.Add(2);

            MatrixZeroColumns = zeroes.ToArray();

            try
            {
                IPage target = Target;

                if (null != target)
                {
                    PageType targetType = target.PageType;
                    string targetName   = target.TargetName.ToLower();

                    IsTargetMissing = false;
                    IsTargetCircularReference = false;
                    IsTargetInverted = false;

                    if (target.Title.StartsWith("~Moved to", StringComparison.OrdinalIgnoreCase))
                        IsTargetRedirect = TargetRedirectType.MovedTo;
                    else if (PageType.Part_Alias == targetType || PageType.Shortcut_Alias == targetType)
                        IsTargetRedirect = TargetRedirectType.Alias;
                    else
                        IsTargetRedirect = TargetRedirectType.NoRedirect;

                    _isScaleX = (1.0 != Math.Round(m.Row0.Length, ndp));
                    _isScaleY = (1.0 != Math.Round(m.Row1.Length, ndp));
                    _isScaleZ = (1.0 != Math.Round(m.Row2.Length, ndp));

                    _isMirrorX = (m.Row0.X < 0.0 || m.Row0.Y < 0.0 || m.Row0.Z < 0.0);
                    _isMirrorY = (m.Row1.X < 0.0 || m.Row1.Y < 0.0 || m.Row1.Z < 0.0);
                    _isMirrorZ = (m.Row2.X < 0.0 || m.Row2.Y < 0.0 || m.Row2.Z < 0.0);

                    if (PageType.Primitive == targetType || PageType.HiresPrimitive == targetType)
                    {
                        switch (target.Category)
                        {
                            case Category.Primitive_Unknown:
                            case Category.Primitive_Click:
                            case Category.Primitive_Hinge:
                            case Category.Primitive_Znap:
                                // Rule:   These primitives should not be scaled, mirrored or inverted
                                // Type:   Error - these primitives represent specific physical elements
                                // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                IsTargetScaled             = (_isScaleX || _isScaleY || _isScaleZ);
                                IsTargetMirrored           = (det < 0.0);
                                IsTargetInverted           = Invert;
                                _targetScaledDescription   = String.Format(Resources.Analytics_TargetScaled_Primitive, targetName);
                                _targetScaledSeverity      = Severity.Error;
                                _targetMirroredDescription = String.Format(Resources.Analytics_TargetMirrored_Primitive, targetName);
                                _targetMirroredSeverity    = Severity.Error;
                                _targetInvertedDescription = String.Format(Resources.Analytics_TargetInverted_Primitive, targetName);
                                _targetInvertedSeverity    = Severity.Error;
                                break;

                            case Category.Primitive_Chord:
                            case Category.Primitive_Disc:
                            case Category.Primitive_Edge:
                            case Category.Primitive_Rectangle:
                            case Category.Primitive_Ring:
                                // Rule:   These primitives should not be scaled in the Y-direction
                                // Type:   Error - Y-scaling is invalid for 2D primitives
                                // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                IsTargetScaled           = _isScaleY;
                                IsTargetMirrored         = false;
                                IsTargetInverted         = false;
                                _targetScaledDescription = String.Format(Resources.Analytics_TargetScaled_Primitive_Y, targetName);
                                _targetScaledSeverity    = Severity.Error;
                                _isScaleX                = false;
                                _isScaleZ                = false;
                                break;

                            case Category.Primitive_Text:
                                // Rule:   These primitives should not be scaled in the Y-direction, or mirrored
                                // Type:   Error - Y-scaling is invalid for 2D primitives and mirroring is inappropriate for text
                                // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                IsTargetScaled             = _isScaleY;
                                IsTargetMirrored           = (det < 0.0);
                                IsTargetInverted           = false;
                                _targetScaledDescription   = String.Format(Resources.Analytics_TargetScaled_Primitive_Y, targetName);
                                _targetScaledSeverity      = Severity.Error;
                                _targetMirroredDescription = String.Format(Resources.Analytics_TargetMirrored_Primitive, targetName);
                                _targetMirroredSeverity    = Severity.Error;
                                _isScaleX                  = false;
                                _isScaleZ                  = false;
                                break;

                            case Category.Primitive_Stud:
                                if (ScalableStudNames.Contains(targetName))
                                {
                                    // Rule:   These primitives should not be scaled or mirrored in the X-direction or Z-direction
                                    // Type:   Error - these primitives represent specific physical elements
                                    // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                    IsTargetScaled             = (_isScaleX || _isScaleZ);
                                    IsTargetMirrored           = (_isMirrorX || _isMirrorZ);
                                    _targetScaledDescription   = String.Format(Resources.Analytics_TargetScaled_Primitive_XZ, targetName);
                                    _targetScaledSeverity      = Severity.Error;
                                    _targetMirroredDescription = String.Format(Resources.Analytics_TargetMirrored_Primitive_XZ, targetName);
                                    _targetMirroredSeverity    = Severity.Error;
                                    _isScaleY                  = false;
                                    _isMirrorY                 = false;
                                }
                                else
                                {
                                    // Rule:   These primitives should not be scaled or mirrored
                                    // Type:   Error - these primitives represent specific physical elements
                                    // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                    IsTargetScaled             = (_isScaleX || _isScaleY || _isScaleZ);
                                    IsTargetMirrored           = (det < 0.0);
                                    _targetScaledDescription   = String.Format(Resources.Analytics_TargetScaled_Primitive, targetName);
                                    _targetScaledSeverity      = Severity.Error;
                                    _targetMirroredDescription = String.Format(Resources.Analytics_TargetMirrored_Primitive, targetName);
                                    _targetMirroredSeverity    = Severity.Error;
                                }

                                // Rule:   These primitives should not be inverted
                                // Type:   Error - these primitives represent specific physical elements
                                // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                IsTargetInverted           = Invert;
                                _targetInvertedDescription = String.Format(Resources.Analytics_TargetInverted_Primitive, targetName);
                                _targetInvertedSeverity    = Severity.Error;
                                break;

                            case Category.Primitive_Technic:
                                if (targetName.StartsWith("axl") || targetName.StartsWith("peghole") || targetName.StartsWith("npeghol"))
                                {
                                    if (0.0 == target.BoundingBox.SizeY)
                                    {
                                        // Rule:   These primitives should not be scaled
                                        // Type:   Error - these primitives represent specific physical elements
                                        // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                        IsTargetScaled           = (_isScaleX || _isScaleY || _isScaleZ);
                                        IsTargetMirrored         = false;
                                        _targetScaledDescription = String.Format(Resources.Analytics_TargetScaled_Primitive, targetName);
                                        _targetScaledSeverity    = Severity.Error;
                                    }
                                    else
                                    {
                                        // Rule:   These primitives should not be scaled or mirrored in the X-direction or Z-direction
                                        // Type:   Error - these primitives represent specific physical elements
                                        // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                        IsTargetScaled             = (_isScaleX || _isScaleZ);
                                        IsTargetMirrored           = (_isMirrorX || _isMirrorZ);
                                        _targetScaledDescription   = String.Format(Resources.Analytics_TargetScaled_Primitive_XZ, targetName);
                                        _targetScaledSeverity      = Severity.Error;
                                        _targetMirroredDescription = String.Format(Resources.Analytics_TargetMirrored_Primitive_XZ, targetName);
                                        _targetMirroredSeverity    = Severity.Error;
                                        _isScaleY                  = false;
                                        _isMirrorY                 = false;
                                    }
                                }
                                else
                                {
                                    // Rule:   These primitives should not be scaled or mirrored
                                    // Type:   Error - these primitives represent specific physical elements
                                    // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                    IsTargetScaled             = (_isScaleX || _isScaleY || _isScaleZ);
                                    IsTargetMirrored           = (det < 0.0);
                                    _targetScaledDescription   = String.Format(Resources.Analytics_TargetScaled_Primitive, targetName);
                                    _targetScaledSeverity      = Severity.Error;
                                    _targetMirroredDescription = String.Format(Resources.Analytics_TargetMirrored_Primitive, targetName);
                                    _targetMirroredSeverity    = Severity.Error;
                                }

                                // Rule:   These primitives should not be inverted
                                // Type:   Error - these primitives represent specific physical elements
                                // Source: http://www.ldraw.org/documentation/ldraw-org-quick-reference-guides/primitive-reference.html
                                if (targetName != "axlehol8.dat" && targetName != "axlehol7.dat" && targetName != "axl3hol8.dat")
                                {
                                    IsTargetInverted           = Invert;
                                    _targetInvertedDescription = String.Format(Resources.Analytics_TargetInverted_Primitive, targetName);
                                    _targetInvertedSeverity    = Severity.Error;
                                }
                                break;

                            default:
                                // everything else can be freely scaled/mirrored/inverted
                                IsTargetScaled   = false;
                                IsTargetMirrored = false;
                                IsTargetInverted = false;
                                break;
                        }
                    }
                    else if (PageType.Subpart != targetType)
                    {
                        if (PageType.Model == targetType)
                        {
                            // Rule:   Models should not normally be scaled
                            // Type:   Warning
                            // Source: Convention
                            IsTargetScaled             = (_isScaleX || _isScaleY || _isScaleZ);
                            IsTargetMirrored           = (det < 0.0);
                            IsTargetInverted           = Invert;
                            _targetScaledDescription   = Resources.Analytics_TargetScaled_Model;
                            _targetScaledSeverity      = Severity.Warning;
                            _targetMirroredDescription = Resources.Analytics_TargetMirrored_Model;
                            _targetMirroredSeverity    = Severity.Warning;
                            _targetInvertedDescription = Resources.Analytics_TargetInverted_Model;
                            _targetInvertedSeverity    = Severity.Warning;
                        }
                        else if (Category.LSynth == target.Category)
                        {
                            // these can be scaled, as they are pseudo-parts
                            IsTargetScaled   = false;
                            IsTargetMirrored = false;
                            IsTargetInverted = false;
                        }
                        else
                        {
                            // Rule:   Most parts should not normally be scaled
                            // Type:   Warning
                            // Source: Convention
                            IsTargetScaled             = (_isScaleX || _isScaleY || _isScaleZ);
                            IsTargetMirrored           = (det < 0.0);
                            IsTargetInverted           = Invert;
                            _targetScaledDescription   = Resources.Analytics_TargetScaled_Part;
                            _targetScaledSeverity      = Severity.Warning;
                            _targetMirroredDescription = Resources.Analytics_TargetMirrored_Part;
                            _targetMirroredSeverity    = Severity.Warning;
                            _targetInvertedDescription = Resources.Analytics_TargetInverted_Part;
                            _targetInvertedSeverity    = Severity.Warning;
                        }
                    }

                    // finally: if Target is not BFC-enabled then there's no point inverting it
                    if (!IsTargetInverted && Invert && CullingMode.CertifiedClockwise != target.BFC && CullingMode.CertifiedCounterClockwise != target.BFC)
                    {
                        IsTargetInverted           = true;
                        _targetInvertedSeverity    = Severity.Warning;
                        _targetInvertedDescription = String.Format(Resources.Analytics_TargetInverted_Invalid, targetName);
                    }
                }
                else
                {
                    IsTargetInverted          = false;
                    IsTargetMirrored          = false;
                    IsTargetScaled            = false;
                    IsTargetRedirect          = TargetRedirectType.NoRedirect;
                    IsTargetMissing           = true;
                    IsTargetCircularReference = (TargetStatus.CircularReference == TargetStatus);
                }
            }
            catch
            {
                // ignore resolve-errors
                IsTargetInverted          = false;
                IsTargetMirrored          = false;
                IsTargetScaled            = false;
                IsTargetRedirect          = TargetRedirectType.NoRedirect;
                IsTargetMissing           = true;
                IsTargetCircularReference = (TargetStatus.CircularReference == TargetStatus);
            }
        }

        /// <inheritdoc />
        public override bool IsColourInvalid { get { return (Palette.EdgeColour == ColourValue || base.IsColourInvalid); } }

        /// <inheritdoc />
        public override bool IsDuplicateOf(IGraphic graphic)
        {
            IReference dupe = graphic as IReference;

            if (null == dupe)
                return false;

            return (base.IsDuplicateOf(graphic) && dupe.Matrix == Matrix && dupe.TargetName.Equals(TargetName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsMatrixSingular"/> condition.
        /// </summary>
        public static readonly Guid Problem_MatrixSingular = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTargetScaled"/> condition.
        /// </summary>
        public static readonly Guid Problem_TargetScaled = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTargetMirrored"/> condition.
        /// </summary>
        public static readonly Guid Problem_TargetMirrored = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTargetInverted"/> condition.
        /// </summary>
        public static readonly Guid Problem_TargetInverted = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTargetMissing"/> condition.
        /// </summary>
        public static readonly Guid Problem_TargetMissing = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTargetCircularReference"/> condition.
        /// </summary>
        public static readonly Guid Problem_TargetCircularReference = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTargetRedirect"/> condition.
        /// </summary>
        public static readonly Guid Problem_TargetRedirect = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsTargetUnreleased"/> condition.
        /// </summary>
        public static readonly Guid Problem_TargetUnreleased = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="IsMatrixSingular"/> condition.
        /// </summary>
        public static readonly Guid Fix_MatrixSingular_RepairMatrix = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="IsTargetScaled"/> condition.
        /// </summary>
        public static readonly Guid Fix_TargetScaled_UnscaleMatrix = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="IsTargetMirrored"/> condition.
        /// </summary>
        public static readonly Guid Fix_TargetMirrored_UnmirrorMatrix = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="IsTargetInverted"/> condition.
        /// </summary>
        public static readonly Guid Fix_TargetInverted_ClearInvert = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="IsTargetRedirect"/> condition.
        /// </summary>
        public static readonly Guid Fix_TargetRedirect_FollowRedirect = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="IsColourInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_ColourInvalid_SetToMainColour = Guid.NewGuid();

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IReference.Matrix"/> is singular.
        /// </summary>
        public bool IsMatrixSingular { get { Validate(); return _isMatrixSingular; } private set { _isMatrixSingular = value; } }
        private bool _isMatrixSingular;

        /// <summary>
        /// Gets the indices of any all-zero rows in <see cref="P:Digitalis.LDTools.DOM.API.IReference.Matrix"/>.
        /// </summary>
        public uint[] MatrixZeroRows { get { Validate(); return _matrixZeroRows.Clone() as uint[]; } private set { _matrixZeroRows = value; } }
        private uint[] _matrixZeroRows = new uint[0];

        /// <summary>
        /// Gets the indices of any all-zero columns in <see cref="P:Digitalis.LDTools.DOM.API.IReference.Matrix"/>.
        /// </summary>
        public uint[] MatrixZeroColumns { get { Validate(); return _matrixZeroColumns.Clone() as uint[]; } private set { _matrixZeroColumns = value; } }
        private uint[] _matrixZeroColumns = new uint[0];

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IReference.Matrix"/> is scaling <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>
        ///     incorrectly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>s of types <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>,
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Part"/>, <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut"/> and their
        /// aliases should not normally be scaled. This property will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> is of one of
        /// those types and <see cref="P:Digitalis.LDTools.DOM.API.IReference.Matrix"/> applies a scale-factor to it.
        /// </para>
        /// </remarks>
        public bool IsTargetScaled { get { Validate(); return _isTargetScaled; } private set { _isTargetScaled = value; } }
        private bool _isTargetScaled;
        private string _targetScaledDescription;
        private Severity _targetScaledSeverity;
        private bool _isScaleX;
        private bool _isScaleY;
        private bool _isScaleZ;

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IReference.Matrix"/> is mirroring <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>
        ///     incorrectly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>s of types <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>,
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Part"/>, <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut"/> and their
        /// aliases should not normally be mirrored. This property will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> is of one of
        /// those types and <see cref="P:Digitalis.LDTools.DOM.API.IReference.Matrix"/> applies a negative scale-factor to it.
        /// </para>
        /// </remarks>
        public bool IsTargetMirrored { get { Validate(); return _isTargetMirrored; } private set { _isTargetMirrored = value; } }
        private bool _isTargetMirrored;
        private string _targetMirroredDescription;
        private Severity _targetMirroredSeverity;
        private bool _isMirrorX;
        private bool _isMirrorY;
        private bool _isMirrorZ;

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IReference.Invert"/> is incorrectly set for the type of
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>s of types <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>,
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Part"/>, <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut"/> and their
        /// aliases should not normally be inverted. This property will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> is of one of
        /// those types, or if <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> is not <see cref="P:Digitalis.LDTools.DOM.API.IPage.BFC">BFC-enabled</see>,
        /// and <see cref="P:Digitalis.LDTools.DOM.API.IReference.Invert"/> is <c>true</c>.
        /// </para>
        /// </remarks>
        public bool IsTargetInverted { get { Validate(); return _isTargetInverted; } private set { _isTargetInverted = value; } }
        private bool _isTargetInverted;
        private string _targetInvertedDescription;
        private Severity _targetInvertedSeverity;

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> refers to a file which cannot be loaded because it would
        ///     cause a circular-dependency.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Getting the value of this property will attempt to resolve <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>.
        /// </para>
        /// </remarks>
        public bool IsTargetCircularReference { get { Validate(); return _isTargetCircularReference; } private set { _isTargetCircularReference = value; } }
        private bool _isTargetCircularReference;

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> could be resolved.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that this is not the same as <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetStatus"/>. Getting the value of this property will attempt to resolve
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>.
        /// </para>
        /// </remarks>
        public bool IsTargetMissing { get { Validate(); return _isTargetMissing; } private set { _isTargetMissing = value; } }
        private bool _isTargetMissing;

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> refers to a "~Moved to" or Alias part.
        /// </summary>
        public TargetRedirectType IsTargetRedirect { get { Validate(); return _isTargetRedirect; } private set { _isTargetRedirect = value; } }
        private TargetRedirectType _isTargetRedirect = TargetRedirectType.NoRedirect;

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> refers to an unreleased part.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A part is considered to be 'unreleased' if its containing <see cref="P:Digitalis.LDTools.DOM.API.IDocument"/> has a
        /// <see cref="P:Digitalis.LDTools.DOM.API.IDocument.Status"/> of anything other than <see cref="Digitalis.LDTools.DOM.API.DocumentStatus.Private"/>,
        /// <see cref="Digitalis.LDTools.DOM.API.DocumentStatus.Released"/> or <see cref="Digitalis.LDTools.DOM.API.DocumentStatus.Rework"/>.
        /// </para>
        /// <para>
        /// If the <see cref="LDReference"/>'s containing <see cref="P:Digitalis.LDTools.DOM.API.IDocument"/> is <see cref="Digitalis.LDTools.DOM.API.DocumentStatus.Released"/>
        /// or <see cref="Digitalis.LDTools.DOM.API.DocumentStatus.Private"/> and the target's <see cref="P:Digitalis.LDTools.DOM.API.IDocument"/> is unreleased, this returns
        /// <c>true</c>.
        /// </para>
        /// </remarks>
        public bool IsTargetUnreleased
        {
            get
            {
                try
                {
                    IPage target = Target;

                    if (null == target)
                        return false;

                    IDocument doc = Document;

                    if (null != doc && DocumentStatus.Released != doc.Status && DocumentStatus.Private != doc.Status)
                        return false;

                    doc = target.Document;

                    if (null == doc)
                        return false;

                    DocumentStatus status = doc.Status;

                    return (DocumentStatus.Private != status && DocumentStatus.Released != status && DocumentStatus.Rework != status);
                }
                catch
                {
                    // ignore resolve-errors
                    return false;
                }
            }
        }

        /// <inheritdoc />
        public override bool HasProblems(CodeStandards mode)
        {
            return base.HasProblems(mode) ||
                   IsMatrixSingular ||
                   IsTargetMirrored ||
                   IsTargetScaled ||
                   IsTargetMissing ||
                   IsTargetInverted ||
                   IsTargetUnreleased ||
                   TargetRedirectType.NoRedirect != IsTargetRedirect;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// The <see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/> of the <see cref="T:Digitalis.LDTools.DOM.Analytics.IProblemDescriptor"/>s returned varies by
        /// <paramref name="mode"/>:
        /// <list type="table">
        ///   <listheader><term>Problem</term><description><see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/></description></listheader>
        ///   <item>
        ///     <term><see cref="Problem_MatrixSingular"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_TargetScaled"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>,
        ///       or if <see cref="Target"/> is a non-scalable primitive; <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_TargetMirrored"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>,
        ///       or if <see cref="Target"/> is a non-scalable primitive; <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_TargetInverted"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>,
        ///       or if <see cref="Target"/> is a non-scalable primitive; <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_TargetMissing"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_TargetRedirect"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/> and
        ///       <see cref="IPageElement.Page"/> is not a <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>;
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Information"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.Full"/> and <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.Page"/> is not a
        ///       <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>;
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_TargetUnreleased"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/>
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="P:Digitalis.LDTools.DOM.Graphic.Problem_ColourInvalid"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>
        ///     </description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        public override ICollection<IProblemDescriptor> Analyse(CodeStandards mode)
        {
            Validate();

            ICollection<IProblemDescriptor> problems = base.Analyse(mode);

            if (IsMatrixSingular)
                problems.Add(new SingularMatrixProblem(this, (1 == MatrixZeroRows.Length && 1 == MatrixZeroColumns.Length)));

            if (IsTargetCircularReference)
            {
                problems.Add(new TargetCircularReferenceProblem(this));
            }
            else if (IsTargetMissing)
            {
                Severity severity;

                if (CodeStandards.OfficialModelRepository == mode || CodeStandards.PartsLibrary == mode)
                    severity = Severity.Error;
                else
                    severity = Severity.Warning;

                problems.Add(new TargetMissingProblem(this, severity));
            }
            else
            {
                Severity severity;

                if (CodeStandards.OfficialModelRepository == mode || CodeStandards.PartsLibrary == mode)
                {
                    _targetScaledSeverity   = Severity.Error;
                    _targetMirroredSeverity = Severity.Error;
                    _targetInvertedSeverity = Severity.Error;
                }

                if (!IsMatrixSingular)
                {
                    if (IsTargetMirrored)
                        problems.Add(new TargetMirroredProblem(this, _targetMirroredDescription, _targetMirroredSeverity, _isMirrorX, _isMirrorY, _isMirrorZ));
                    else if (IsTargetScaled)
                        problems.Add(new TargetScaledProblem(this, _targetScaledDescription, _targetScaledSeverity, _isScaleX, _isScaleY, _isScaleZ));
                }

                if (IsTargetInverted)
                    problems.Add(new TargetInvertedProblem(this, _targetInvertedDescription, _targetInvertedSeverity));

                if (TargetRedirectType.NoRedirect != IsTargetRedirect)
                {
                    switch (mode)
                    {
                        case CodeStandards.OfficialModelRepository:
                        case CodeStandards.PartsLibrary:
                            if (null == Page || PageType.Model == Page.PageType)
                                severity = Severity.Warning;
                            else
                                severity = Severity.Error;
                            break;

                        default:
                            if (null == Page || PageType.Model == Page.PageType)
                                severity = Severity.Information;
                            else
                                severity = Severity.Warning;
                            break;
                    }

                    switch (IsTargetRedirect)
                    {
                        case TargetRedirectType.Alias:
                            problems.Add(new TargetRedirectProblem(this, Resources.Analytics_TargetAlias, severity));
                            break;

                        case TargetRedirectType.MovedTo:
                            problems.Add(new TargetRedirectProblem(this, Resources.Analytics_TargetMovedTo, severity));
                            break;
                    }
                }

                if (IsTargetUnreleased)
                {
                    try
                    {
                        IPage target = Target;

                        if (null != target)
                        {
                            IDocument doc = target.Document;

                            if (null != doc)
                                problems.Add(new TargetUnreleasedProblem(this, doc.Status));
                        }
                    }
                    catch
                    {
                    }
                }
            }

            // Rule:   References should not use EdgeColour
            // Type:   Error
            // Source: http://www.ldraw.org/article/218.html#lt1
            if (Palette.EdgeColour == ColourValue)
            {
                problems.Add(new InvalidColourProblem(this,
                                                      Graphic.Problem_ColourInvalid,
                                                      Severity.Error,
                                                      String.Format(Resources.Analytics_InvalidColour_Polygon, LDTranslationCatalog.GetColourName(Palette.SystemPalette[Palette.EdgeColour].Name)),
                                                      new IFixDescriptor[] { new ChangeColour(this) }));
            }

            return problems;
        }

        #endregion Analytics

        #region Cache

        /// <summary>
        /// Gets the <see cref="F:ReferenceCacheChangedEventArgs.Key"/> used to identify <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> in the cache.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> has not been resolved, or has resolved to an <see cref="P:Digitalis.LDTools.DOM.API.IPage"/> in
        /// the same document as the <see cref="T:Digitalis.LDTools.DOM.API.IReference"/>, this returns <c>null</c>. Otherwise it returns the value used to identify
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> in <see cref="LDReference"/>'s internal cache. This is the same value passed to subscribers to the
        /// <see cref="CacheEntryAdded"/> event.
        /// </para>
        /// </remarks>
        public string TargetKey { get; private set; }

        /// <summary>
        /// Occurs when an <see cref="T:Digitalis.LDTools.DOM.LDDocument"/> is added to the <see cref="LDReference"/> cache.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On adding itself to this event, the subscriber will immediately receive an event for each entry already present in the cache.
        /// </para>
        /// <para>
        /// Subscribers to this event are required to be thread-safe.
        /// </para>
        /// </remarks>
        public static event ReferenceCacheChangedEventHandler CacheEntryAdded
        {
            add
            {
                List<LDDocument> entries;

                lock (ReferenceCache)
                {
                    entries = new List<LDDocument>(ReferenceCache.Values);
                }

                foreach (LDDocument entry in entries)
                {
                    value(null, new ReferenceCacheChangedEventArgs(entry, entry.CacheKey));
                }

                _cacheEntryAdded += value;
            }
            remove { _cacheEntryAdded -= value; }
        }

        private static ReferenceCacheChangedEventHandler _cacheEntryAdded;

        /// <summary>
        /// Occurs when an <see cref="T:Digitalis.LDTools.DOM.LDDocument"/> is removed from the <see cref="LDReference"/> cache.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Subscribers to this event are required to be thread-safe.
        /// </para>
        /// </remarks>
        public static event ReferenceCacheChangedEventHandler CacheEntryRemoved;

        // used to hold the targetName of documents we're trying to load in order to detect circular-references
        private static Dictionary<string, string> LoadCache          = new Dictionary<string, string>();
        private static Dictionary<string, LDDocument> ReferenceCache = new Dictionary<string, LDDocument>();

        // used by the unit-tests
        internal static int CacheCount { get { return ReferenceCache.Count; } }

        private static void InitialiseCache()
        {
            Configuration.LDrawBaseChanged += delegate(object sender, EventArgs e)
            {
                List<LDDocument> entries;

                lock (ReferenceCache)
                {
                    entries = new List<LDDocument>(ReferenceCache.Values);
                    ReferenceCache.Clear();
                }

                if (null != CacheEntryRemoved)
                {
                    foreach (LDDocument doc in entries)
                    {
                        CacheEntryRemoved(null, new ReferenceCacheChangedEventArgs(doc, doc.CacheKey));
                        doc.Dispose();
                    }
                }
                else
                {
                    foreach (LDDocument doc in entries)
                    {
                        doc.Dispose();
                    }
                }

                List<Dictionary<LDReference, LDReference>> clients;

                lock (LibraryListeners)
                {
                    clients = new List<Dictionary<LDReference, LDReference>>(LibraryListeners.Values);
                    LibraryListeners.Clear();
                }

                foreach (Dictionary<LDReference, LDReference> clientList in clients)
                {
                    foreach (LDReference client in clientList.Keys)
                    {
                        client.ClearTarget(true);
                    }
                }
            };
        }

        private static LDDocument GetFromCache(string key)
        {
            LDDocument target;

            if (ReferenceCache.TryGetValue(key, out target))
            {
                target.RefCount++;
                return target;
            }

            return null;
        }

        private static LDDocument Claim(LDReference client, string targetName)
        {
            LDDocument doc;
            string     key = targetName.ToLower();

            lock (LoadCache)
            {
                if (LoadCache.ContainsKey(targetName))
                    throw new CircularReferenceException(String.Empty, null, targetName, client.LDrawCode, client.CodeLine);
            }

            lock (ReferenceCache)
            {
                doc = GetFromCache(key);
            }

            if (null != doc)
                return doc;

            if (Path.IsPathRooted(key))
            {
                // absolute filepath: either it exists, or it does not
                if (!File.Exists(key))
                    return null;

                try
                {
                    lock (LoadCache)
                    {
                        LoadCache.Add(targetName, targetName);
                        doc = new LDDocument(targetName, ParseFlags.None);
                        LoadCache.Remove(targetName);
                    }
                }
                catch (IOException)
                {
                    // not loadable
                    return null;
                }
            }
            else
            {
                // see if it's a filepath relative to the containing document
                IDocument containingDocument = client.Document;

                if (null != containingDocument && !containingDocument.IsLibraryPart && !String.IsNullOrWhiteSpace(containingDocument.Filepath))
                {
                    string path = Path.Combine(Path.GetDirectoryName(containingDocument.Filepath), targetName);

                    if (File.Exists(path))
                    {
                        try
                        {
                            lock (LoadCache)
                            {
                                LoadCache.Add(targetName, targetName);
                                doc = new LDDocument(path, ParseFlags.None);
                                LoadCache.Remove(targetName);
                            }

                            if (!doc.IsLibraryPart)
                                key = path.ToLower();
                        }
                        catch (IOException)
                        {
                            // not loadable, so carry on
                        }
                    }
                }
            }

            if (null == doc)
            {
                // if the LibraryManager cache has been loaded, try there first
                if (null != LibraryManager.Cache)
                {
                    IndexCard card = LibraryManager.Cache[key];

                    if (null != card && File.Exists(card.Filepath))
                    {
                        try
                        {
                            lock (LoadCache)
                            {
                                LoadCache.Add(targetName, targetName);
                                doc = new LDDocument(card.Filepath, ParseFlags.None);
                                LoadCache.Remove(targetName);
                            }
                        }
                        catch (IOException)
                        {
                            return null;
                        }
                    }
                }

                // couldn't find it in the library-cache, so try the search-path directly
                if (null == doc)
                {
                    foreach (string s in Configuration.PrimarySearchPath)
                    {
                        string path = Path.Combine(s, key);

                        if (File.Exists(path))
                        {
                            try
                            {
                                lock (LoadCache)
                                {
                                    LoadCache.Add(targetName, targetName);
                                    doc = new LDDocument(path, ParseFlags.None);
                                    LoadCache.Remove(targetName);
                                }
                                break;
                            }
                            catch (IOException)
                            {
                                // not loadable
                                return null;
                            }
                        }
                    }
                }

                if (null == doc)
                    return null;
            }

            // load up all its dependencies as well by getting its bounding-box
            Box3d bounds = doc[0].BoundingBox;

            // and finally add it to the cache
            doc.RefCount = 1;
            doc.CacheKey = key;
            doc.Freeze();

            lock (ReferenceCache)
            {
                // check that the same document wasn't loaded up by another caller
                LDDocument duplicate = GetFromCache(key);

                if (null != duplicate)
                {
                    doc.Dispose();
                    return duplicate;
                }

                ReferenceCache.Add(key, doc);

                if (null != _cacheEntryAdded)
                    _cacheEntryAdded(null, new ReferenceCacheChangedEventArgs(doc, key));
            }

            return doc;
        }

        private static void Release(IReference client, LDDocument target, bool forceEvict)
        {
            lock (ReferenceCache)
            {
                if (target.RefCount <= 0)
                    return;

                if (--target.RefCount > 0 && !forceEvict)
                    return;

                string key = target.CacheKey;

                if (!ReferenceCache.ContainsKey(key))
                    return;

                ReferenceCache.Remove(key);

                if (null != CacheEntryRemoved)
                    CacheEntryRemoved(null, new ReferenceCacheChangedEventArgs(target, target.CacheKey));
            }

            target.Dispose();
        }

        #endregion Cache

        #region Cloning

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            IReference reference = (IReference)obj;

            reference.Matrix     = Matrix;
            reference.TargetName = TargetName;
            reference.Invert     = Invert;
            base.InitializeObject(obj);
        }

        #endregion Cloning

        #region Colour

        /// <inheritdoc />
        public override uint OverrideableColourValue { get { return Palette.MainColour; } }

        /// <inheritdoc />
        public override bool ColourValueEnabled { get { return true; } }

        #endregion Colour

        #region Code-generation

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// If <paramref name="codeFormat"/> is <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>, no code is appended if the
        /// <see cref="T:Digitalis.LDTools.DOM.API.IReference"/> is either <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsVisible">hidden</see> or
        /// <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsGhosted">ghosted</see>; otherwise <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>s in the same
        /// <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> as the <see cref="T:Digitalis.LDTools.DOM.API.IReference"/> are inlined by calling their
        /// <see cref="M:Digitalis.LDTools.DOM.IDOMObject.ToCode"/> methods.
        /// </para>
        /// <para>
        /// If <paramref name="codeFormat"/> is <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> and
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> is a member of the same <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> as the
        /// <see cref="T:Digitalis.LDTools.DOM.API.IReference"/> and is marked as <see cref="P:Digitalis.LDTools.DOM.API.IPage.InlineOnPublish"/>, no code is appended if the
        /// <see cref="T:Digitalis.LDTools.DOM.API.IReference"/> is either <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsVisible">hidden</see> or
        /// <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsGhosted">ghosted</see>; otherwise <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>s in the same
        /// <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> as the <see cref="T:Digitalis.LDTools.DOM.API.IReference"/> are inlined by calling their
        /// <see cref="M:Digitalis.LDTools.DOM.IDOMObject.ToCode"/> methods.
        /// </para>
        /// </remarks>
        protected override StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if ((!IsVisible || IsGhosted) && CodeStandards.PartsLibrary == codeFormat)
                return base.GenerateCode(sb, codeFormat, overrideColour, ref transform, winding);

            IPage    target = null;
            IPage    page   = Page;
            Matrix4d matrix = Matrix;
            Matrix4d m;

            // use ColourValue if it's set to a fixed colour
            if (Palette.MainColour != ColourValue)
                overrideColour = ColourValue;

            // in 'PartsLibrary' mode we need to convert local-palette colours to Direct Colours
            if (CodeStandards.PartsLibrary == codeFormat && !LDColour.IsDirectColour(overrideColour))
            {
                IColour c = GetColour(overrideColour);

                // only local-palette colours will have a Parent
                if (null != c.Parent)
                    overrideColour = LDColour.ConvertRGBToDirectColour(c.Value);
            }

            Matrix4d.Mult(ref matrix, ref transform, out m);

            try
            {
                target = Target;
            }
            catch
            {
                // ignore resolve-errors
            }

            if (CodeStandards.Full == codeFormat || CodeStandards.OfficialModelRepository == codeFormat || null == target || !target.InlineOnPublish || (null != page && page.Document != target.Document))
            {
                // normal output: just append the code for the reference
                if (Invert && (CodeStandards.Full == codeFormat || WindingDirection.None != winding))
                    sb.AppendFormat("0 BFC INVERTNEXT{0}", LineTerminator);

                sb = base.GenerateCode(sb, codeFormat, overrideColour, ref transform, winding);

                uint ndp;

                if (null != page && (PageType.Primitive == page.PageType || PageType.HiresPrimitive == page.PageType))
                    ndp = Configuration.DecimalPlacesPrimitives;
                else
                    ndp = Configuration.DecimalPlacesCoordinates;

                string cFormat = Configuration.Formatters[ndp];
                string tFormat = Configuration.DecimalPlacesTransformsFormatter;

                sb.AppendFormat("1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}{14}",
                                LDColour.ColourValueToCode(overrideColour),
                                m.M41.ToString(cFormat, CultureInfo.InvariantCulture),
                                m.M42.ToString(cFormat, CultureInfo.InvariantCulture),
                                m.M43.ToString(cFormat, CultureInfo.InvariantCulture),
                                m.M11.ToString(tFormat, CultureInfo.InvariantCulture),
                                m.M21.ToString(tFormat, CultureInfo.InvariantCulture),
                                m.M31.ToString(tFormat, CultureInfo.InvariantCulture),
                                m.M12.ToString(tFormat, CultureInfo.InvariantCulture),
                                m.M22.ToString(tFormat, CultureInfo.InvariantCulture),
                                m.M32.ToString(tFormat, CultureInfo.InvariantCulture),
                                m.M13.ToString(tFormat, CultureInfo.InvariantCulture),
                                m.M23.ToString(tFormat, CultureInfo.InvariantCulture),
                                m.M33.ToString(tFormat, CultureInfo.InvariantCulture),
                                TargetName,
                                LineTerminator);
            }
            else if (!IsGhosted && IsVisible)
            {
                // extended mode: we want to inline the reference's Target
                if (WindingDirection.None == winding)
                    return target.ToCode(sb, codeFormat, overrideColour, ref m, winding);

                CullingMode ourWindingMode = WindingMode;

                if (CullingMode.NotSet == ourWindingMode || CullingMode.Disabled == ourWindingMode)
                    return target.ToCode(sb, codeFormat, overrideColour, ref m, WindingDirection.None);

                CullingMode targetWindingMode = target.BFC;

                if (CullingMode.NotSet == targetWindingMode || CullingMode.Disabled == targetWindingMode)
                {
                    if (CullingMode.NotSet == ourWindingMode || CullingMode.Disabled == ourWindingMode)
                    {
                        sb = target.ToCode(sb, codeFormat, overrideColour, ref m, WindingDirection.None);
                    }
                    else
                    {
                        sb = new LDBFCFlag(BFCFlag.DisableBackFaceCulling).ToCode(sb, codeFormat, overrideColour, ref m, winding);
                        sb = target.ToCode(sb, codeFormat, overrideColour, ref m, WindingDirection.None);

                        if (!IsLast())
                            sb = new LDBFCFlag(BFCFlag.EnableBackFaceCulling).ToCode(sb, codeFormat, overrideColour, ref m, winding);
                    }
                }
                else
                {
                    if (targetWindingMode != ourWindingMode)
                    {
                        if (WindingDirection.Normal == winding)
                            winding = WindingDirection.Reversed;
                        else
                            winding = WindingDirection.Normal;
                    }

                    sb = target.ToCode(sb, codeFormat, overrideColour, ref m, winding);
                }
            }

            return sb;
        }

        private bool IsLast()
        {
            IElementCollection parent = Parent;

            if (null == parent)
                return false;

            IDOMObject element = this;

            while (null != parent)
            {
                if (parent[parent.Count - 1] != element)
                    return false;

                element = parent;
                parent = parent.Parent;
            }

            return true;
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDReference"/> class with default values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The reference's <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> is set to <see cref="F:Digitalis.LDTools.DOM.Palette.MainColour"/>,
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Matrix"/> is set to <see cref="F:OpenTK.Matrix4d.Identity"/>,
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> is set to <c>"Undefined"</c> and <see cref="P:Digitalis.LDTools.DOM.API.IReference.Invert"/>
        /// is set to <c>false</c>.
        /// </para>
        /// </remarks>
        public LDReference()
            : this(Palette.MainColour, ref Matrix4d.Identity, Resources.Undefined, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDReference"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The colour of the reference.</param>
        /// <param name="matrix">The matrix of the reference.</param>
        /// <param name="targetName">The name of the sub-file.</param>
        /// <param name="invert">Specifies whether the reference is to be BFC-inverted.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="targetName"/> was <c>null</c> or empty.</exception>
        /// <remarks>
        /// <para>
        /// The matrix is defined in the following order, with reference to <see href="http://www.ldraw.org/article/218.html#lt1">the LDraw specifications</see>:
        /// <code>
        ///     / a d g 0 \
        ///     | b e h 0 |
        ///     | c f i 0 |
        ///     \ x y z 1 /
        /// </code>
        /// </para>
        /// </remarks>
        public LDReference(uint colour, ref Matrix4d matrix, string targetName, bool invert)
            : base(colour)
        {
            SetEventHandlers();
            Matrix = matrix;
            TargetName = targetName;
            Invert = invert;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDReference"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this line.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw reference code.</exception>
        /// <example>
        /// <code>
        /// LDReference reference = new LDReference("1 4 0 0 0 1 0 0 0 1 0 0 0 1 3005.dat");
        /// </code>
        /// </example>
        public LDReference(string code)
            : this(code, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDReference"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this line.</param>
        /// <param name="invert">Specifies whether the reference is to be BFC-inverted.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw reference code.</exception>
        /// <example>
        /// <code>
        /// LDReference reference = new LDReference("1 4 0 0 0 1 0 0 0 1 0 0 0 1 3005.dat", true);
        /// </code>
        /// </example>
        public LDReference(string code, bool invert)
            : base()
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 15)     // note that target names can have spaces in them, so there may be more than 15 fields
                throw new FormatException("LDraw reference code must have at least 15 fields");

            if ("1" != fields[0])
                throw new FormatException("LDraw reference code must start with '1'");

            SetEventHandlers();
            SetColourValue(fields[1]);

            Matrix = new Matrix4d(double.Parse(fields[5], CultureInfo.InvariantCulture),  // a
                                  double.Parse(fields[8], CultureInfo.InvariantCulture),  // d
                                  double.Parse(fields[11], CultureInfo.InvariantCulture), // g
                                  0,
                                  double.Parse(fields[6], CultureInfo.InvariantCulture),  // b
                                  double.Parse(fields[9], CultureInfo.InvariantCulture),  // e
                                  double.Parse(fields[12], CultureInfo.InvariantCulture), // h
                                  0,
                                  double.Parse(fields[7], CultureInfo.InvariantCulture),  // c
                                  double.Parse(fields[10], CultureInfo.InvariantCulture), // f
                                  double.Parse(fields[13], CultureInfo.InvariantCulture), // i
                                  0,
                                  double.Parse(fields[2], CultureInfo.InvariantCulture),  // x
                                  double.Parse(fields[3], CultureInfo.InvariantCulture),  // y
                                  double.Parse(fields[4], CultureInfo.InvariantCulture),  // z
                                  1);

            TargetName = code.Substring(code.IndexOf(fields[14]));
            Invert = invert;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveLibraryListeners(TargetName);
                ClearTarget();
            }

            base.Dispose(disposing);
        }

        private void SetEventHandlers()
        {
            _matrix.ValueChanged += delegate(object sender, PropertyChangedEventArgs<Matrix4d> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                _boundsDirty = true;
                _validated   = false;

                if (null != MatrixChanged)
                    MatrixChanged(this, e);

                OnChanged(this, "MatrixChanged", e);
            };

            _invert.ValueChanged += delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                _validated = false;

                if (null != InvertChanged)
                    InvertChanged(this, e);

                OnChanged(this, "InvertChanged", e);
            };

            _targetName.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (!_autoUpdate)
                {
                    if (IsFrozen)
                        throw new ObjectFrozenException();

                    if (IsLocked)
                        throw new ElementLockedException();
                }

                RemoveLibraryListeners(e.OldValue);
                AddLibraryListeners(e.NewValue);

                if (TargetStatus.Resolved != TargetStatus || !Target.TargetName.Equals(TargetName, StringComparison.OrdinalIgnoreCase))
                    ClearTarget(false);

                if (null != TargetNameChanged)
                    TargetNameChanged(this, e);

                OnChanged(this, "TargetNameChanged", e);
            };
        }

        #endregion Constructor

        #region Document-tree

        [NonSerialized]
        private IDocument _currentDocument;

        [NonSerialized]
        private bool _inhibitTargetNameUpdate;

        /// <inheritdoc />
        protected override void OnPathToDocumentChanged(EventArgs e)
        {
            if (null != _currentDocument)
            {
                foreach (IPage page in _currentDocument)
                {
                    page.TargetNameChanged -= OnPageTargetNameChanged;
                }

                _currentDocument.ItemsAdded      -= OnPagesAddedToDocument;
                _currentDocument.ItemsRemoved    -= OnPagesRemovedFromDocument;
                _currentDocument.ItemsReplaced   -= OnPagesReplacedInDocument;
                _currentDocument.FilepathChanged -= OnDocumentFilepathChanged;
                _currentDocument                  = null;
            }

            IDocument doc = Document;

            if (null != doc && !doc.IsLibraryPart)
            {
                _currentDocument     = doc;
                doc.FilepathChanged += OnDocumentFilepathChanged;
                doc.ItemsAdded      += OnPagesAddedToDocument;
                doc.ItemsRemoved    += OnPagesRemovedFromDocument;
                doc.ItemsReplaced   += OnPagesReplacedInDocument;

                foreach (IPage page in doc)
                {
                    page.TargetNameChanged += OnPageTargetNameChanged;
                }
            }

            ClearTarget();
            base.OnPathToDocumentChanged(e);
        }

        private void OnPageTargetNameChanged(object sender, PropertyChangedEventArgs<string> e)
        {
            TargetStatus status = TargetStatus;

            if (e.OldValue.Equals(TargetName, StringComparison.OrdinalIgnoreCase))
            {
                if (_inhibitTargetNameUpdate)
                {
                    // we are currently reverting a change which caused the page to match our TargetName
                    _inhibitTargetNameUpdate = false;
                    ClearTarget();
                }
                else
                {
                    // we are applying a change to a page which matches our TargetName, so we need to update ourselves
                    _autoUpdate = true;

                    UndoStack undoStack = UndoStack.CurrentStack;

                    if (null != undoStack)
                    {
                        undoStack.SuspendCommand();
                        TargetName = e.NewValue;
                        undoStack.ResumeCommand();
                    }
                    else
                    {
                        TargetName = e.NewValue;
                    }

                    _autoUpdate = false;
                }
            }
            else if (e.NewValue.Equals(TargetName, StringComparison.OrdinalIgnoreCase))
            {
                // a page has been changed to match our TargetName
                UndoStack undoStack = UndoStack.CurrentStack;

                if (null != undoStack && undoStack.IsCommandStarted)
                    UndoStack.AddAction(new SetInhibitFlagAction(this));

                ClearTarget();
            }
        }

        private void OnPagesAddedToDocument(object sender, UndoableListChangedEventArgs<IPage> e)
        {
            foreach (IPage page in e.Items)
            {
                page.TargetNameChanged += OnPageTargetNameChanged;

                if (page.TargetName.Equals(TargetName, StringComparison.OrdinalIgnoreCase))
                    ClearTarget();
            }
        }

        private void OnPagesRemovedFromDocument(object sender, UndoableListChangedEventArgs<IPage> e)
        {
            foreach (IPage page in e.Items)
            {
                page.TargetNameChanged -= OnPageTargetNameChanged;

                if (page.TargetName.Equals(TargetName, StringComparison.OrdinalIgnoreCase))
                    ClearTarget();
            }
        }

        private void OnPagesReplacedInDocument(object sender, UndoableListReplacedEventArgs<IPage> e)
        {
            bool clear = false;

            foreach (IPage page in e.ItemsRemoved.Items)
            {
                page.TargetNameChanged -= OnPageTargetNameChanged;

                if (!clear && page.TargetName.Equals(TargetName, StringComparison.OrdinalIgnoreCase))
                    clear = true;
            }

            foreach (IPage page in e.ItemsAdded.Items)
            {
                page.TargetNameChanged += OnPageTargetNameChanged;

                if (!clear && page.TargetName.Equals(TargetName, StringComparison.OrdinalIgnoreCase))
                    clear = true;
            }

            if (clear)
                ClearTarget();
        }

        private void OnDocumentFilepathChanged(object sender, EventArgs e)
        {
            RemoveFSTargetWatchers();
            AddFSTargetWatchers(TargetName);
        }

        #endregion Document-tree

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDReferenceEditor", typeof(LDReference));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDReference"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        /// <see cref="LDReference"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        /// <remarks>
        /// <para>
        /// This will resolve <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> if required.
        /// </para>
        /// </remarks>
        public override Box3d BoundingBox
        {
            get
            {
                if (_boundsDirty)
                {
                    if (null != Target)
                    {
                        Matrix4d matrix = Matrix;
                        _bounds         = Target.BoundingBox;
                        Box3d.Transform(ref _bounds, ref matrix, out _bounds);
                        _boundsDirty = false;
                    }
                    else
                    {
                        _bounds = new Box3d(Origin, Origin);
                    }
                }

                return _bounds;
            }
        }
        private Box3d _bounds;
        private bool  _boundsDirty = true;

        /// <inheritdoc />
        public override Vector3d Origin { get { return new Vector3d(Matrix.Row3); } }

        /// <inheritdoc />
        public override uint CoordinatesCount { get { return 0; } }

        /// <inheritdoc />
        public override void Transform(ref Matrix4d transform)
        {
            if (IsLocked)
                throw new InvalidOperationException();

            Matrix *= transform;
            _boundsDirty = true;
        }

        /// <inheritdoc />
        public override void ReverseWinding()
        {
            Invert = !Invert;
        }

        /// <inheritdoc />
        protected override void OnCoordinatesChanged(PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
        {
            _boundsDirty = true;
            base.OnCoordinatesChanged(e);
        }

        #endregion Geometry

        #region Library-monitoring

        private static Dictionary<string, Dictionary<LDReference, LDReference>> LibraryListeners = new Dictionary<string, Dictionary<LDReference, LDReference>>();

        static LDReference()
        {
            InitialiseCache();

            LibraryManager.Changed += delegate(object sender, LibraryChangedEventArgs e)
            {
                List<LDReference> clients = new List<LDReference>();

                lock (LibraryListeners)
                {
                    GetClientsForTarget(e.Added, clients);
                    GetClientsForTarget(e.Modified, clients);
                    GetClientsForTarget(e.Removed, clients);
                }

                foreach (LDReference client in clients)
                {
                    if (client.UsingLibraryTarget)
                    {
                        if (TargetStatus.Resolved == client.TargetStatus)
                            client.ClearTarget();
                        else if (null != client.TargetChanged)
                            client.TargetChanged(client, EventArgs.Empty);
                    }
                }
            };
        }

        private static void GetClientsForTarget(IEnumerable<string> targetNames, List<LDReference> clients)
        {
            if (null == targetNames)
                return;

            Dictionary<LDReference, LDReference> entry;

            foreach (string targetName in targetNames)
            {
                if (LibraryListeners.TryGetValue(targetName.ToLower(), out entry))
                    clients.AddRange(entry.Keys);
            }
        }

        private bool UsingLibraryTarget
        {
            get
            {
                if (Path.IsPathRooted(TargetName))
                    return false;

                if (TargetStatus.Resolved == TargetStatus)
                    return Target.Document.IsLibraryPart;

                // target is not resolved, so it could be in the library
                return true;
            }
        }

        private void AddLibraryListeners(string targetName)
        {
            if (null == targetName)
                return;

            Dictionary<LDReference, LDReference> cacheEntry;
            string key = targetName.ToLower();

            if (!Path.IsPathRooted(key))
            {
                lock (LibraryListeners)
                {
                    if (!LibraryListeners.TryGetValue(key, out cacheEntry))
                    {
                        cacheEntry = new Dictionary<LDReference, LDReference>();
                        LibraryListeners.Add(key, cacheEntry);
                    }

                    cacheEntry.Add(this, this);
                }
            }

            AddFSTargetWatchers(targetName);
        }

        private void RemoveLibraryListeners(string targetName)
        {
            if (null == targetName)
                return;

            Dictionary<LDReference, LDReference> cacheEntry;
            string key = targetName.ToLower();

            lock (LibraryListeners)
            {
                if (LibraryListeners.TryGetValue(key, out cacheEntry))
                {
                    cacheEntry.Remove(this);

                    if (0 == cacheEntry.Count)
                        LibraryListeners.Remove(key);
                }
            }

            RemoveFSTargetWatchers();
        }

        #endregion Library-monitoring

        #region Properties

        /// <inheritdoc />
        public Matrix4d Matrix
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _matrix.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (Vector4d.UnitW != value.Column3)
                    throw new ArgumentException("Matrix may not set column 4");

                if (Matrix != value)
                    _matrix.Value = value;
            }
        }
        private UndoableProperty<Matrix4d> _matrix = new UndoableProperty<Matrix4d>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<Matrix4d> MatrixChanged;

        /// <inheritdoc />
        public bool Invert
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _invert.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (Invert == value)
                    return;

                if (TargetStatus.Resolved == TargetStatus && value)
                {
                    // check for a 2D target and optimise
                    Box3d bounds = Target.BoundingBox;

                    if (0.0 == bounds.SizeX)
                    {
                        Matrix = InvertX * Matrix;
                        value  = false;
                    }
                    else if (0.0 == bounds.SizeY)
                    {
                        Matrix = InvertY * Matrix;
                        value  = false;
                    }
                    else if (0.0 == bounds.SizeZ)
                    {
                        Matrix = InvertZ * Matrix;
                        value  = false;
                    }
                }

                if (Invert != value)
                    _invert.Value = value;
            }
        }
        private UndoableProperty<bool> _invert = new UndoableProperty<bool>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<bool> InvertChanged;

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override DOMObjectType ObjectType { get { return DOMObjectType.Reference; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns the <see cref="P:Digitalis.LDTools.DOM.API.IDocumentElement.Icon"/> of <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>, or a default icon if
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> is unavailable.
        /// </para>
        /// </remarks>
        public override Image Icon
        {
            get
            {
                try
                {
                    IPage target = Target;

                    if (null != target)
                        return target.Icon;
                }
                catch
                {
                    // ignore resolve-errors
                }

                return Resources.PartIcon;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns the <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> of <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>, or
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Part"/> if <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> is unavailable.
        /// </para>
        /// </remarks>
        public override string TypeName
        {
            get
            {
                try
                {
                    IPage target = Target;

                    if (null != target)
                        return LDTranslationCatalog.GetPageType(target.PageType);
                }
                catch
                {
                    // ignore resolve-errors
                }

                return LDTranslationCatalog.GetPageType(PageType.Part);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns the <see cref="P:Digitalis.LDTools.DOM.API.IPage.Description"/> of <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>, or <c>"&lt;unknown&gt;"</c>
        /// if <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> is unavailable or has no <see cref="P:Digitalis.LDTools.DOM.API.IPage.Description"/>.
        /// </para>
        /// </remarks>
        public override string Description
        {
            get
            {
                try
                {
                    IPage target = Target;

                    if (null != target && !String.IsNullOrWhiteSpace(target.Description))
                        return target.Description;
                }
                catch
                {
                    // ignore resolve-errors
                }

                return Resources.Unknown;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns the <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/> plus <see cref="P:Digitalis.LDTools.DOM.API.IPage.Help"/> of
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>, or <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> if
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> is unavailable.
        /// </para>
        /// </remarks>
        public override string ExtendedDescription
        {
            get
            {
                try
                {
                    IPage target = Target;

                    if (null != target)
                    {
                        if (String.IsNullOrWhiteSpace(target.Help))
                            return target.TargetName;

                        return String.Format("{0}\r\n{1}", target.TargetName, target.Help);
                    }
                }
                catch
                {
                    // ignore resolve-errors
                }

                return TargetName;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IReference"/> is not a state-element.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IReference"/> is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        #endregion Self-description

        #region Target-management

        [NonSerialized]
        private bool _beginUpdate;
        [NonSerialized]
        private bool _refreshTarget;
        [NonSerialized]
        private bool _autoUpdate;

        /// <inheritdoc />
        public string TargetName
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _targetName.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();

                value = value.Replace('/', '\\');

                if (null != TargetName && TargetName.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return;

                _targetName.Value = value;
            }
        }
        private UndoableProperty<string> _targetName = new UndoableProperty<string>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> TargetNameChanged;

        /// <inheritdoc />
        public IDocument TargetContext
        {
            get { return _targetContext; }
            set
            {
                if (TargetContext == value)
                    return;

                _targetContext = value;
                ClearTarget(false);
            }
        }
        [NonSerialized]
        private IDocument _targetContext;

        /// <inheritdoc />
        public TargetStatus TargetStatus { get { return _targetStatus; } set { _targetStatus = value; } }
        [NonSerialized]
        private TargetStatus _targetStatus;

        /// <inheritdoc />
        public IPage Target
        {
            get
            {
                if (null != _target)
                    return _target;

                IPage     page       = Page;
                IDocument doc        = null;
                string    targetName = TargetName;

                TargetKey = null;

                // first, see if the target is a member of the same document as us
                if (null != page)
                {
                    // basic check: make sure we're not pointing at ourselves...
                    if (page.TargetName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        TargetStatus = TargetStatus.CircularReference;
                        return null;
                    }

                    doc = page.Document;

                    if (null != doc)
                    {
                        if (Path.IsPathRooted(targetName) && targetName.Equals(doc.Filepath, StringComparison.OrdinalIgnoreCase))
                        {
                            TargetStatus = TargetStatus.CircularReference;
                            return null;
                        }

                        FindLocalTarget(page, doc, targetName);
                    }
                }

                // then try the target-context
                if (null == _target && null != TargetContext)
                {
                    if (null == doc)
                        doc = TargetContext;

                    FindLocalTarget(null, TargetContext, targetName);
                }

                // if those failed, ask the cache to try and resolve the target for us
                if (null == _target)
                {
                    LDDocument cachedDocument = null;

                    try
                    {
                        UndoStack undoStack = UndoStack.CurrentStack;

                        if (null != undoStack)
                        {
                            // if the resolve is occurring during an undoable operation, we need to make sure we don't include any undoable ops of our
                            // own in the command, as this may cause recursion
                            undoStack.SuspendCommand();
                            cachedDocument = Claim(this, targetName);
                            undoStack.ResumeCommand();
                        }
                        else
                        {
                            cachedDocument = Claim(this, targetName);
                        }

                        if (null == cachedDocument)
                            TargetStatus = TargetStatus.Missing;
                    }
                    catch (CircularReferenceException)
                    {
                        TargetStatus = TargetStatus.CircularReference;
                    }
                    catch
                    {
                        TargetStatus = TargetStatus.Unloadable;
                    }

                    if (null == cachedDocument)
                        return null;

                    TargetKey       = cachedDocument.CacheKey;
                    _targetDocument = cachedDocument;
                    _target         = _targetDocument[0];
                }

                // verify that the target will not cause a circular-reference
                if (null != page && InsertCheckResult.CircularReference == Parent.CanInsert(this, InsertCheckFlags.IgnoreCurrentCollection | InsertCheckFlags.IgnoreIsLocked))
                {
                    ClearTarget(false, false);
                    TargetStatus = TargetStatus.CircularReference;
                    return null;
                }

                TargetStatus = TargetStatus.Resolved;

                if (Invert)
                {
                    // check for a 2D target and optimise
                    Box3d bounds = _target.BoundingBox;

                    if (0.0 == bounds.SizeX)
                    {
                        Matrix = InvertX * Matrix;
                        Invert = false;
                    }
                    else if (0.0 == bounds.SizeY)
                    {
                        Matrix = InvertY * Matrix;
                        Invert = false;
                    }
                    else if (0.0 == bounds.SizeZ)
                    {
                        Matrix = InvertZ * Matrix;
                        Invert = false;
                    }
                }

                // if this is a Baseplate then load up the HQ version of it as well, if one exists
                if (PageType.Part == _target.PageType && Category.Baseplate == _target.Category)
                {
                    targetName = _target.Name + "h.dat";
                    Claim(this, targetName);
                }

                // defer the event if we're in an update cycle
                if (_beginUpdate)
                    _refreshTarget = true;
                else
                    OnTargetChanged(this, EventArgs.Empty);

                return _target;
            }
        }
        [NonSerialized]
        private IDocument _targetDocument;
        [NonSerialized]
        private IPage _target;
        [NonSerialized]
        private FileSystemWatcher _localWatcher;

        /// <inheritdoc />
        [field: NonSerialized]
        public event EventHandler TargetChanged;

        private void AddFSTargetWatchers(string targetName)
        {
            if (null == _localWatcher)
            {
                string folder;

                if (Path.IsPathRooted(targetName))
                    folder = Path.GetDirectoryName(targetName);
                else if (null != Document && null != Document.Filepath && Path.IsPathRooted(Document.Filepath))
                    folder = Path.GetDirectoryName(Document.Filepath);
                else
                    return;

                _localWatcher = new FileSystemWatcher(folder, Path.GetFileName(targetName));
                _localWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;

                _localWatcher.Changed += delegate(object sender, FileSystemEventArgs e)
                {
                    ClearTarget(true);
                };

                _localWatcher.Created += delegate(object sender, FileSystemEventArgs e)
                {
                    ClearTarget(true);
                };

                _localWatcher.Deleted += delegate(object sender, FileSystemEventArgs e)
                {
                    ClearTarget(true);
                };

                _localWatcher.Renamed += delegate(object sender, RenamedEventArgs e)
                {
                    // note that we must set the internal value directly rather than going through the public property,
                    // as if we are locked the public API will throw an exception
                    if (Path.IsPathRooted(TargetName))
                        _targetName.Value = e.FullPath;
                    else
                        _targetName.Value = e.Name;
                };

                _localWatcher.Error += delegate(object sender, ErrorEventArgs e)
                {
                    AddFSTargetWatchers(targetName);
                };

                _localWatcher.EnableRaisingEvents = true;
            }
        }

        private void RemoveFSTargetWatchers()
        {
            if (null != _localWatcher)
            {
                _localWatcher.Dispose();
                _localWatcher = null;
            }
        }

        private void FindLocalTarget(IPage page, IDocument doc, string targetName)
        {
            foreach (IPage p in doc)
            {
                if (p != page && p.TargetName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    // register to hear about changes to our document so we can notify our own listeners
                    _targetDocument  = doc;
                    _target          = p;
                    _target.Changed += OnTargetElementTreeChanged;
                    doc.UpdateBegun += OnBeginUpdate;
                    doc.UpdateEnded += OnEndUpdate;
                    return;
                }
            }
        }

        private void OnTargetChanged(object sender, EventArgs e)
        {
            _boundsDirty = true;

            if (null != TargetChanged)
                TargetChanged(this, e);

            OnChanged(this, "TargetChanged", e);
        }

        /// <inheritdoc />
        public void ClearTarget()
        {
            ClearTarget(false);
        }

        private void ClearTarget(bool forceEvict)
        {
            ClearTarget(forceEvict, true);
        }

        private void ClearTarget(bool forceEvict, bool sendEvent)
        {
            if (null != _target)
            {
                IPage page = Page;

                if (null != TargetKey)
                {
                    Release(this, _targetDocument as LDDocument, forceEvict);
                }
                else
                {
                    _target.Changed             -= OnTargetElementTreeChanged;
                    _targetDocument.UpdateBegun -= OnBeginUpdate;
                    _targetDocument.UpdateEnded -= OnEndUpdate;
                }

                _targetDocument = null;
                _target         = null;
                _boundsDirty    = true;
            }
            else
            {
                // see if a suitable target can be obtained from the current document; if not then suppress the event since it's meaningless
                IDocument document = Document;

                if (null == document || document.IsDisposed || document.IsDisposing || null == document[TargetName])
                    sendEvent = false;
            }

            TargetStatus = TargetStatus.Unresolved;
            TargetKey    = null;
            _validated   = false;

            if (sendEvent)
                OnTargetChanged(this, EventArgs.Empty);
        }

        private void OnTargetElementTreeChanged(object sender, ObjectChangedEventArgs e)
        {
            // if the target is being removed from the document then we don't want to forward any more events from it
            if (null != Target.Document)
            {
                if (_beginUpdate)
                    _refreshTarget = true;
                else
                    OnTargetChanged(sender, e);
            }
        }

        private void OnBeginUpdate(object sender, EventArgs e)
        {
            // if we're inside a begin/end update pair, we need to defer any TargetChanged events we would otherwise
            // have sent until the 'end' event occurs in order to prevent the risk of recursion
            _beginUpdate   = true;
            _refreshTarget = false;
        }

        private void OnEndUpdate(object sender, EventArgs e)
        {
            if (_refreshTarget)
                OnTargetChanged(this, e);

            _refreshTarget = false;
            _beginUpdate   = false;
        }

        #endregion Target-management
    }
}
