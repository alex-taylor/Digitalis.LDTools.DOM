#region License

//
// MockDOMObject.cs
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
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    [Serializable]
    public class MockDOMObject : DOMObject
    {
        private bool _hasAncestor;
        private bool _ancestorIsFrozen;

        public MockDOMObject()
        {
        }

        public MockDOMObject(bool hasAncestor, bool ancestorIsFrozen)
        {
            _hasAncestor = hasAncestor;
            _ancestorIsFrozen = ancestorIsFrozen;
        }

        public override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return sb;
        }

        public override DOMObjectType ObjectType
        {
            get { return DOMObjectType.Collection; }
        }

        public override bool IsImmutable { get { return false; } }

        protected override void InitializeObject(IDOMObject obj)
        {
            InitializeElementCalled = true;
            base.InitializeObject(obj);
        }

        public bool InitializeElementCalled;

        protected override void Dispose(bool disposing)
        {
            if (disposing && _hasAncestor && _ancestorIsFrozen)
                throw new ObjectFrozenException();

            DisposeCalled = disposing;

            if (null != OnDisposing)
                OnDisposing(this, EventArgs.Empty);

            base.Dispose(disposing);
        }

        public bool DisposeCalled;

        public event EventHandler OnDisposing;

        public void OnChanged()
        {
            OnChanged(this, "TestEvent", EventArgs.Empty);
        }

        public override bool IsFrozen
        {
            get
            {
                return base.IsFrozen || _ancestorIsFrozen;
            }
        }

        protected override void OnFreezing(EventArgs e)
        {
            OnFreezingCalled = true;
            base.OnFreezing(e);
        }

        public bool OnFreezingCalled;

        protected override void OnFrozen(EventArgs e)
        {
            OnFrozenCalled = true;
            base.OnFreezing(e);
        }

        public bool OnFrozenCalled;
    }
}
