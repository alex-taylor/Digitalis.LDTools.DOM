#region License

//
// Exceptions.cs
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

    #endregion Usings

    #region LDrawLibraryAlreadyLoadedException

    /// <summary>
    /// The exception that is thrown if <see cref="M:Digitalis.LDTools.Library.LibraryManager.Load"/> is called after the library has been successfully loaded.
    /// </summary>
    public sealed class LDrawLibraryAlreadyLoadedException : Exception
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDrawLibraryAlreadyLoadedException"/> class.
        /// </summary>
        public LDrawLibraryAlreadyLoadedException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDrawLibraryAlreadyLoadedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public LDrawLibraryAlreadyLoadedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDrawLibraryAlreadyLoadedException"/> class with a specified error message and a reference to the inner
        ///     exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public LDrawLibraryAlreadyLoadedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion Constructor
    }

    #endregion LDrawLibraryAlreadyLoadedException
}
