#region License

//
// Enums.cs
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

namespace Digitalis.LDTools.DOM.API
{
    #region Usings

    using System;

    #endregion Usings

    #region BFCFlag

    /// <summary>
    /// Specifies the type of an <see cref="T:Digitalis.LDTools.DOM.API.IBFCFlag"/> as defined in the
    ///     <see href="http://www.ldraw.org/article/415.html">LDraw.org Language Extension for Back Face Culling (BFC)</see>.
    /// </summary>
    public enum BFCFlag
    {
        /// <summary>
        /// Sets the winding-mode to clockwise and leaves the enable state unchanged.
        /// </summary>
        SetWindingModeClockwise,

        /// <summary>
        /// Sets the winding-mode to counter-clockwise and leaves the enable state changed.
        /// </summary>
        SetWindingModeCounterClockwise,

        /// <summary>
        /// Enables Back Face Culling and leaves the winding-mode unchanged.
        /// </summary>
        EnableBackFaceCulling,

        /// <summary>
        /// Disables Back Face Culling.
        /// </summary>
        DisableBackFaceCulling,

        /// <summary>
        /// Enables Back Face Culling and sets the winding-mode to clockwise.
        /// </summary>
        EnableBackFaceCullingAndSetWindingModeClockwise,

        /// <summary>
        /// Enables Back Face Culling and sets the winding-mode to counter-clockwise.
        /// </summary>
        EnableBackFaceCullingAndSetWindingModeCounterClockwise
    }

    #endregion BFCFlag

    #region Category

    /// <summary>
    /// Specifies the category of an <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> as defined in the
    ///     <see href="http://www.ldraw.org/library/tracker/ref/catkeyfaq/">Categories and Keywords FAQ</see>.
    /// </summary>
    public enum Category
    {
        /// <summary>
        /// Category: unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Category: Animal.
        /// </summary>
        Animal,

        /// <summary>
        /// Category: Antenna.
        /// </summary>
        Antenna,

        /// <summary>
        /// Category: Arch.
        /// </summary>
        Arch,

        /// <summary>
        /// Category: Arm.
        /// </summary>
        Arm,

        /// <summary>
        /// Category: Bar.
        /// </summary>
        Bar,

        /// <summary>
        /// Category: Baseplate.
        /// </summary>
        Baseplate,

        /// <summary>
        /// Category: Belville.
        /// </summary>
        Belville,

        /// <summary>
        /// Category: Boat.
        /// </summary>
        Boat,

        /// <summary>
        /// Category: Bracket.
        /// </summary>
        Bracket,

        /// <summary>
        /// Category: Brick.
        /// </summary>
        Brick,

        /// <summary>
        /// Category: Canvas.
        /// </summary>
        Canvas,

        /// <summary>
        /// Category: Car.
        /// </summary>
        Car,

        /// <summary>
        /// Category: Cone.
        /// </summary>
        Cone,

        /// <summary>
        /// Category: Container.
        /// </summary>
        Container,

        /// <summary>
        /// Category: Conveyor.
        /// </summary>
        Conveyor,

        /// <summary>
        /// Category: Crane.
        /// </summary>
        Crane,

        /// <summary>
        /// Category: Cylinder.
        /// </summary>
        Cylinder,

        /// <summary>
        /// Category: Dish.
        /// </summary>
        Dish,

        /// <summary>
        /// Category: Door.
        /// </summary>
        Door,

        /// <summary>
        /// Category: Electric.
        /// </summary>
        Electric,

        /// <summary>
        /// Category: Exhaust.
        /// </summary>
        Exhaust,

        /// <summary>
        /// Category: Fence.
        /// </summary>
        Fence,

        /// <summary>
        /// Category: Figure.
        /// </summary>
        Figure,

        /// <summary>
        /// Category: Figure Accessory.
        /// </summary>
        FigureAccessory,

        /// <summary>
        /// Category: Flag.
        /// </summary>
        Flag,

        /// <summary>
        /// Category: Freestyle.
        /// </summary>
        Freestyle,

        /// <summary>
        /// Category: Garage.
        /// </summary>
        Garage,

        /// <summary>
        /// Category: Gate.
        /// </summary>
        Gate,

        /// <summary>
        /// Category: Glass.
        /// </summary>
        Glass,

        /// <summary>
        /// Category: Hinge.
        /// </summary>
        Hinge,

        /// <summary>
        /// Category: Homemaker.
        /// </summary>
        Homemaker,

        /// <summary>
        /// Category: Hose.
        /// </summary>
        Hose,

        /// <summary>
        /// Category: Ladder.
        /// </summary>
        Ladder,

        /// <summary>
        /// Category: Magent.
        /// </summary>
        Magnet,

        /// <summary>
        /// Category: Minifig.
        /// </summary>
        Minifig,

        /// <summary>
        /// Category: Minifig Accessory.
        /// </summary>
        MinifigAccessory,

        /// <summary>
        /// Category: Minifig Footwear.
        /// </summary>
        MinifigFootwear,

        /// <summary>
        /// Category: Minifig Headwear.
        /// </summary>
        MinifigHeadwear,

        /// <summary>
        /// Category: Minifig Hipwear.
        /// </summary>
        MinifigHipwear,

        /// <summary>
        /// Category: Minifig Neckwear.
        /// </summary>
        MinifigNeckwear,

        /// <summary>
        /// Category: Monorail.
        /// </summary>
        Monorail,

        /// <summary>
        /// Category: Panel.
        /// </summary>
        Panel,

        /// <summary>
        /// Category: Plane.
        /// </summary>
        Plane,

        /// <summary>
        /// Category: Plant.
        /// </summary>
        Plant,

        /// <summary>
        /// Category: Plate.
        /// </summary>
        Plate,

        /// <summary>
        /// Category: Platform.
        /// </summary>
        Platform,

        /// <summary>
        /// Category: Propellor.
        /// </summary>
        Propellor,

        /// <summary>
        /// Category: Rack.
        /// </summary>
        Rack,

        /// <summary>
        /// Category: Roadsign.
        /// </summary>
        Roadsign,

        /// <summary>
        /// Category: Rock.
        /// </summary>
        Rock,

        /// <summary>
        /// Category: Scala.
        /// </summary>
        Scala,

        /// <summary>
        /// Category: Screw.
        /// </summary>
        Screw,

        /// <summary>
        /// Category: Sheet.
        /// </summary>
        Sheet,

        /// <summary>
        /// Category: Slope.
        /// </summary>
        Slope,

        /// <summary>
        /// Category: Sphere.
        /// </summary>
        Sphere,

        /// <summary>
        /// Category: Staircase.
        /// </summary>
        Staircase,

        /// <summary>
        /// Category: Sticker.
        /// </summary>
        Sticker,

        /// <summary>
        /// Category: Support.
        /// </summary>
        Support,

        /// <summary>
        /// Category: Tail.
        /// </summary>
        Tail,

        /// <summary>
        /// Category: Tap.
        /// </summary>
        Tap,

        /// <summary>
        /// Category: Technic.
        /// </summary>
        Technic,

        /// <summary>
        /// Category: Tile.
        /// </summary>
        Tile,

        /// <summary>
        /// Category: Tipper.
        /// </summary>
        Tipper,

        /// <summary>
        /// Category: Tractor.
        /// </summary>
        Tractor,

        /// <summary>
        /// Category: Train.
        /// </summary>
        Train,

        /// <summary>
        /// Category: Turntable.
        /// </summary>
        Turntable,

        /// <summary>
        /// Category: Tyre.
        /// </summary>
        Tyre,

        /// <summary>
        /// Category: Vehicle.
        /// </summary>
        Vehicle,

        /// <summary>
        /// Category: Wedge.
        /// </summary>
        Wedge,

        /// <summary>
        /// Category: Wheel.
        /// </summary>
        Wheel,

        /// <summary>
        /// Category: Winch.
        /// </summary>
        Winch,

        /// <summary>
        /// Category: Window.
        /// </summary>
        Window,

        /// <summary>
        /// Category: Windscreen.
        /// </summary>
        Windscreen,

        /// <summary>
        /// Category: Wing.
        /// </summary>
        Wing,

        /// <summary>
        /// Category: Znap.
        /// </summary>
        Znap,

        /// <summary>
        /// Category: LSynth.
        /// </summary>
        LSynth,

        /// <summary>
        /// Category: Primitive_Unknown.
        /// </summary>
        Primitive_Unknown,

        /// <summary>
        /// Category: Primitive_Box.
        /// </summary>
        Primitive_Box,

        /// <summary>
        /// Category: Primitive_Chord.
        /// </summary>
        Primitive_Chord,

        /// <summary>
        /// Category: Primitive_Click.
        /// </summary>
        Primitive_Click,

        /// <summary>
        /// Category: Primitive_Cone.
        /// </summary>
        Primitive_Cone,

        /// <summary>
        /// Category: Primitive_Cylinder.
        /// </summary>
        Primitive_Cylinder,

        /// <summary>
        /// Category: Primitive_Disc.
        /// </summary>
        Primitive_Disc,

        /// <summary>
        /// Category: Primitive_Edge.
        /// </summary>
        Primitive_Edge,

        /// <summary>
        /// Category: Primitive_Hinge.
        /// </summary>
        Primitive_Hinge,

        /// <summary>
        /// Category: Primitive_Rectangle.
        /// </summary>
        Primitive_Rectangle,

        /// <summary>
        /// Category: Primitive_Ring.
        /// </summary>
        Primitive_Ring,

        /// <summary>
        /// Category: Primitive_Sphere.
        /// </summary>
        Primitive_Sphere,

        /// <summary>
        /// Category: Primitive_Stud.
        /// </summary>
        Primitive_Stud,

        /// <summary>
        /// Category: Primitive_Technic.
        /// </summary>
        Primitive_Technic,

        /// <summary>
        /// Category: Primitive_Text.
        /// </summary>
        Primitive_Text,

        /// <summary>
        /// Category: Primitive_Torus.
        /// </summary>
        Primitive_Torus,

        /// <summary>
        /// Category: Primitive_Znap.
        /// </summary>
        Primitive_Znap
    }

    #endregion Category

    #region CodeStandards

    /// <summary>
    /// Specifies the LDraw code-format returned by <see cref="Digitalis.LDTools.DOM.API.IDOMObject.ToCode"/>.
    /// </summary>
    /// <seealso href="http://www.ldraw.org/article/218.html">LDraw.org File Format specification</seealso>
    /// <seealso href="http://www.ldraw.org/article/512.html">LDraw.org File Format Restrictions for the Official Library</seealso>
    /// <seealso href="http://www.ldraw.org/article/593.html">Official Model Library (OMR) Specification</seealso>
    public enum CodeStandards
    {
        /// <summary>
        /// Full LDraw code, including elements which are not permitted by the <see href="http://www.ldraw.org/article/512.html">official parts specification</see>
        /// or the <see href="http://www.ldraw.org/article/593.html">Official Model Library (OMR) Specification</see>.
        /// </summary>
        Full,

        /// <summary>
        /// Restricts the code to only those elements which are permitted by the <see href="http://www.ldraw.org/article/512.html">official parts specification</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that if you save an <see cref="T:Digitalis.LDTools.DOM.API.IDOMObject"/> in this mode, some data may be lost.
        /// </para>
        /// </remarks>
        PartsLibrary,

        /// <summary>
        /// Restricts the code to only those elements which are permitted by the <see href="http://www.ldraw.org/article/593.html">Official Model Library (OMR) Specification</see>.
        /// </summary>
        OfficialModelRepository
    }

    #endregion CodeStandards

    #region CullingMode

    /// <summary>
    /// Specifies the winding-mode of the geometry in an <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> as defined in the
    ///     <see href="http://www.ldraw.org/article/415.html">LDraw.org Language Extension for Back Face Culling (BFC)</see>.
    /// </summary>
    public enum CullingMode
    {
        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> does not have a defined winding-mode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Back Face Culling should be disabled for all geometry (<see cref="T:Digitalis.LDTools.DOM.API.ILine"/>, <see cref="T:Digitalis.LDTools.DOM.API.ITriangle"/>,
        /// <see cref="T:Digitalis.LDTools.DOM.API.IQuadrilateral"/>, <see cref="T:Digitalis.LDTools.DOM.API.IOptionalLine"/>) when rendering.
        /// </para>
        /// <para>
        /// If the <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> has a type of <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>, Back Face Culling should be enabled
        /// for all <see cref="T:Digitalis.LDTools.DOM.API.IReference"/>s; otherwise it should be disabled.
        /// </para>
        /// </remarks>
        NotSet,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> does not support Back Face Culling.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Back Face Culling should be disabled when rendering.
        /// </para>
        /// </remarks>
        Disabled,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> supports Back Face Culling with a default winding-mode of clockwise.
        /// </summary>
        CertifiedClockwise,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> supports Back Face Culling with a default winding-mode of counter-clockwise.
        /// </summary>
        CertifiedCounterClockwise
    }

    #endregion CullingMOde

    #region DocumentStatus

    /// <summary>
    /// Specifies the current status of the file represented by an <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/>.
    /// </summary>
    public enum DocumentStatus
    {
        /// <summary>
        /// The file is not in the current Parts Library, nor on the Parts Tracker.
        /// </summary>
        Private,

        /// <summary>
        /// The file is on the Parts Tracker.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Normally one of the more specific values such as <see cref="Uncertified"/> or <see cref="ReadyForRelease"/> will
        /// be returned. This value is returned if the file's current status does not fall into one of the other categories.
        /// </para>
        /// </remarks>
        Unreleased,

        /// <summary>
        /// The file is in the current Parts Library.
        /// </summary>
        Released,

        /// <summary>
        /// The file is on the Parts Tracker, but has at least one 'hold' vote against it.
        /// </summary>
        Held,

        /// <summary>
        /// The file is on the Parts Tracker, but has not been certified by at least two reviewers.
        /// </summary>
        Uncertified,

        /// <summary>
        /// The file is on the Parts Tracker, has at least two 'certify' votes but has not yet been admin-certified.
        /// </summary>
        Certified,

        /// <summary>
        /// The file is on the Parts Tracker and has been admin-certified.
        /// </summary>
        ReadyForRelease,

        /// <summary>
        /// The file is in the current Parts Library, but is also on the Parts Tracker for further work.
        /// </summary>
        Rework
    }

    #endregion DocumentStatus

    #region DOMObjectType

    /// <summary>
    /// Specifies the type of an <see cref="T:Digitalis.LDTools.DOM.API.IDOMObject"/>.
    /// </summary>
    public enum DOMObjectType
    {
        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IComment"/>.
        /// </summary>
        Comment = 0,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IReference"/>.
        /// </summary>
        Reference,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.ILine"/>.
        /// </summary>
        Line,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.ITriangle"/>.
        /// </summary>
        Triangle,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IQuadrilateral"/>.
        /// </summary>
        Quadrilateral,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IOptionalLine"/>.
        /// </summary>
        OptionalLine,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IMetaCommand"/>.
        /// </summary>
        MetaCommand,

        /// <summary>
        /// A <see cref="T:Digitalis.LDTools.DOM.API.ITexmap"/>.
        /// </summary>
        Texmap,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.ICompositeElement"/>.
        /// </summary>
        CompositeElement,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IElementCollection"/>.
        /// </summary>
        Collection,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IStep"/>.
        /// </summary>
        Step,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IGroup"/>.
        /// </summary>
        Group,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>.
        /// </summary>
        Page,

        /// <summary>
        /// An <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/>.
        /// </summary>
        Document
    };

    #endregion DOMObjectType

    #region InsertCheckFlags

    /// <summary>
    /// Specifies the behaviour of an <see cref="IDOMObjectCollection{T}"/> insert-check.
    /// </summary>
    [Flags]
    public enum InsertCheckFlags
    {
        /// <summary>
        /// Normal behavour.
        /// </summary>
        None = 0,

        /// <summary>
        /// The current containing collection, if any, of the object to be checked should be ignored.
        /// </summary>
        IgnoreCurrentCollection = 1 << 0,

        /// <summary>
        /// The state of the collection's <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked"/> property should be ignored.
        /// </summary>
        IgnoreIsLocked
    }

    #endregion InsertCheckFlags

    #region InsertCheckResult

    /// <summary>
    /// Specifies the result of performing an insertion-check on a collection.
    /// </summary>
    public enum InsertCheckResult
    {
        /// <summary>
        /// The insertion is permitted.
        /// </summary>
        CanInsert,

        /// <summary>
        /// The element is a <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsTopLevelElement">top-level element</see> which is not supported by the collection.
        /// </summary>
        TopLevelElementNotAllowed,

        /// <summary>
        /// The insertion would create a circular reference.
        /// </summary>
        CircularReference,

        /// <summary>
        /// The element is already a member of a collection.
        /// </summary>
        AlreadyMember,

        /// <summary>
        /// The object to be inserted has the same name as an existing object in the collection.
        /// </summary>
        DuplicateName,

        /// <summary>
        /// The insertion is not supported by the collection for some other reason.
        /// </summary>
        NotSupported
    }

    #endregion InsertCheckResult

    #region License

    /// <summary>
    /// Specifies the license for an <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> as defined in
    ///     <see href="http://www.ldraw.org/article/349.html">The LDraw Organization Contributor Agreement</see>.
    /// </summary>
    public enum License
    {
        /// <summary>
        /// No license is set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The Creative Commons Attribution License, version 2.0.
        /// </summary>
        CCAL2,

        /// <summary>
        /// An unknown license.
        /// </summary>
        NonCCAL
    }

    #endregion License

    #region PageType

    /// <summary>
    /// Specifies the type of an <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> as defined in the
    ///     <see href="http://www.ldraw.org/article/398.html">LDraw.org Official Library Header Specification</see>.
    /// </summary>
    public enum PageType
    {
        /// <summary>
        /// A model.
        /// </summary>
        Model = 0,

        /// <summary>
        /// A part.
        /// </summary>
        Part,

        /// <summary>
        /// An alias part.
        /// </summary>
        Part_Alias,

        /// <summary>
        /// A physical-colour part.
        /// </summary>
        Part_Physical_Colour,

        /// <summary>
        /// A shortcut part.
        /// </summary>
        Shortcut,

        /// <summary>
        /// An alias shortcut.
        /// </summary>
        Shortcut_Alias,

        /// <summary>
        /// A physical-colour shortcut.
        /// </summary>
        Shortcut_Physical_Colour,

        /// <summary>
        /// A subpart.
        /// </summary>
        Subpart,

        /// <summary>
        /// A primitive.
        /// </summary>
        Primitive,

        /// <summary>
        /// A hi-res ('48') primitive.
        /// </summary>
        HiresPrimitive
    }

    #endregion PageType

    #region StepMode

    /// <summary>
    /// Specifies the operation-mode of an <see cref="T:Digitalis.LDTools.DOM.API.IStep"/>.
    /// </summary>
    public enum StepMode
    {
        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IStep"/> specifies that the viewpoint should be set to a specific rotation.
        /// </summary>
        Absolute,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IStep"/> specifies that the viewpoint should be rotated by the specified amount, relative to its current angles.
        /// </summary>
        Additive,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IStep"/> specifies that the viewpoint should be rotated by the specified amount, relative to its default angles.
        /// </summary>
        Relative,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IStep"/> specifies that the viewpoint should be restored to its default angles.
        /// </summary>
        Reset
    }

    #endregion StepMode

    #region TargetStatus

    /// <summary>
    /// Specifies the current status of the <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> of an <see cref="T:Digitalis.LDTools.DOM.API.IReference"/>.
    /// </summary>
    public enum TargetStatus
    {
        /// <summary>
        /// The <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> has not yet been resolved.
        /// </summary>
        Unresolved,

        /// <summary>
        /// The <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> cannot be found.
        /// </summary>
        Missing,

        /// <summary>
        /// The <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> cannot be loaded because it would cause a circular-dependency.
        /// </summary>
        CircularReference,

        /// <summary>
        /// The <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> cannot be loaded because it is not a valid LDraw file.
        /// </summary>
        Unloadable,

        /// <summary>
        /// The <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/> has been resolved.
        /// </summary>
        Resolved
    }

    #endregion TargetStatus

    #region TexmapGeometryType

    /// <summary>
    /// Specifies the type of an <see cref="T:Digitalis.LDTools.DOM.API.ITextureGeometry" />.
    /// </summary>
    public enum TexmapGeometryType
    {
        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.ITextureGeometry"/> is the <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.TextureGeometry"/> collection.
        /// </summary>
        Texture,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.ITextureGeometry"/> is the <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.SharedGeometry"/> collection.
        /// </summary>
        Shared,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.ITextureGeometry"/> is the <see cref="P:Digitalis.LDTools.DOM.API.ITexmap.FallbackGeometry"/> collection.
        /// </summary>
        Fallback
    }

    #endregion TexmapGeometryType

    #region TexmapProjection

    /// <summary>
    /// Specifies the projection of an <see cref="T:Digitalis.LDTools.DOM.API.ITexmap"/>.
    /// </summary>
    public enum TexmapProjection
    {
        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.ITexmap"/> uses
        ///     <see href="http://www.ldraw.org/documentation/ldraw-org-file-format-standards/language-extension-for-texture-mapping.html#planar">planar</see> projection.
        /// </summary>
        Planar,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.ITexmap"/> uses
        ///     <see href="http://www.ldraw.org/documentation/ldraw-org-file-format-standards/language-extension-for-texture-mapping.html#cylindrical">cylindrical</see> projection.
        /// </summary>
        Cylindrical,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.ITexmap"/> uses
        ///     <see href="http://www.ldraw.org/documentation/ldraw-org-file-format-standards/language-extension-for-texture-mapping.html#spherical">spherical</see> projection.
        /// </summary>
        Spherical
    }

    #endregion TexmapProjection

    #region WindingDirection

    /// <summary>
    /// Specifies the winding-direction to be used by the geometry returned by <see cref="Digitalis.LDTools.DOM.API.IDOMObject.ToCode"/>.
    /// </summary>
    /// <seealso href="http://www.ldraw.org/article/415.html">Language Extension for Back Face Culling</seealso>
    public enum WindingDirection
    {
        /// <summary>
        /// Back Face Culling is disabled.
        /// </summary>
        None,

        /// <summary>
        /// Back Face Culling is enabled, and the <see cref="T:Digitalis.LDTools.DOM.API.IDOMObject"/> should output its geometry with its normal winding-direction.
        /// </summary>
        Normal,

        /// <summary>
        /// Back Face Culling is enabled, and the <see cref="T:Digitalis.LDTools.DOM.API.IDOMObject"/> should output its geometry with the reverse of its normal winding-direction.
        /// </summary>
        Reversed
    }

    #endregion WindingDirection
}
