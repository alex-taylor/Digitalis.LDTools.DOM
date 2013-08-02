#region License

//
// IGeometricTest.cs
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

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Geom;

    #endregion Usings

    public static class IGeometricTest
    {
        #region Geometry

        public static void BoundingBoxTest(IGeometric geometric, IDOMObject containingObject)
        {
            Utils.DisposalAccessTest(containingObject, delegate() { Box3d bounds = geometric.BoundingBox; });
        }

        public static void OriginTest(IGeometric geometric, IDOMObject containingObject)
        {
            Utils.DisposalAccessTest(containingObject, delegate() { Vector3d origin = geometric.Origin; });
        }

        public static void WindingModeTest(IGeometric geometric, IElement containingElement)
        {
            // detatched geometric
            Assert.AreEqual(CullingMode.NotSet, geometric.WindingMode);

            IPage page = new LDPage();
            IStep step;

            if (DOMObjectType.Step == geometric.ObjectType)
            {
                step = (IStep)geometric;
            }
            else
            {
                step = new LDStep();
                step.Add(containingElement);
            }

            page.Add(step);

            // page with no BFC set
            page.BFC = CullingMode.NotSet;
            Assert.AreEqual(CullingMode.NotSet, geometric.WindingMode);

            // page with BFC disabled
            page.BFC = CullingMode.Disabled;
            Assert.AreEqual(CullingMode.Disabled, geometric.WindingMode);

            // page with BFC enabled
            page.BFC = CullingMode.CertifiedClockwise;
            Assert.AreEqual(CullingMode.CertifiedClockwise, geometric.WindingMode);
            page.BFC = CullingMode.CertifiedCounterClockwise;
            Assert.AreEqual(CullingMode.CertifiedCounterClockwise, geometric.WindingMode);

            // check that document-tree lookups work
            if (DOMObjectType.Step == geometric.ObjectType)
            {
                step = new LDStep();
                page.Insert(0, step);
            }

            step.Insert(0, new LDBFCFlag(BFCFlag.DisableBackFaceCulling));
            Assert.AreEqual(CullingMode.Disabled, geometric.WindingMode);

            step.Insert(1, new LDBFCFlag(BFCFlag.EnableBackFaceCulling));
            Assert.AreEqual(CullingMode.CertifiedCounterClockwise, geometric.WindingMode);

            IDOMObject objectToDispose;

            if (null != containingElement)
                objectToDispose = containingElement;
            else
                objectToDispose = geometric;

            Utils.DisposalAccessTest(objectToDispose, delegate() { CullingMode mode = geometric.WindingMode; });
        }

        public static void TransformTest(IGeometric geometric, IPageElement containingElement, ref Matrix4d transform)
        {
            // TODO: test for immutable IGeometric.Transform()

            containingElement.IsLocked = true;

            try
            {
                geometric.Transform(ref transform);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            containingElement.Freeze();

            try
            {
                geometric.Transform(ref transform);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            Utils.DisposalAccessTest(containingElement, delegate() { geometric.Transform(ref Matrix4d.Identity); });
        }

        public static void ReverseWindingTest(IGeometric geometric, IPageElement containingElement)
        {
            // TODO: test for immutable IGeometric.ReverseWinding()

            containingElement.IsLocked = true;

            try
            {
                geometric.ReverseWinding();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ElementLockedException), e.GetType());
            }

            containingElement.Freeze();

            try
            {
                geometric.ReverseWinding();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectFrozenException), e.GetType());
            }

            Utils.DisposalAccessTest(containingElement, delegate() { geometric.ReverseWinding(); });
        }

        #endregion Geometry
    }
}
