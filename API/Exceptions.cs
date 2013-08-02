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

namespace Digitalis.LDTools.DOM.API
{
    #region Usings

    using System;

    #endregion Usings

    #region CircularReferenceException

    /// <summary>
    /// The exception that is thrown when a circular reference is detected whilst parsing LDraw code.
    /// </summary>
    public sealed class CircularReferenceException : ParserException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularReferenceException"/> class with a specified error message, a reference to the inner exception
        ///     that is the cause of this exception, the LDraw code which caused it and the line number on which the code appears.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        /// <param name="path">The path of the file.</param>
        /// <param name="code">The LDraw code which caused the exception.</param>
        /// <param name="line">The line number in the code which caused the exception.</param>
        public CircularReferenceException(string message, Exception innerException, string path, string code, uint line)
            : base(message, innerException, path, code, line)
        {
        }

        #endregion Constructor
    }

    #endregion CircularReferenceException

    #region DuplicateNameException

    /// <summary>
    /// The exception that is thrown when an element with a <b>Name</b> property has its <b>Name</b> changed in such a way that it collides with another element in
    /// the same collection.
    /// </summary>
    public sealed class DuplicateNameException : Exception
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateNameException"/> class.
        /// </summary>
        public DuplicateNameException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateNameException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public DuplicateNameException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateNameException"/> class with a specified error message and a reference to the inner
        ///     exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public DuplicateNameException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion Constructor
    }

    #endregion DuplicateNameException

    #region DuplicatePageException

    /// <summary>
    /// The exception that is thrown when an <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> with the same name as an existing <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>
    /// is detected whilst parsing LDraw code.
    /// </summary>
    public sealed class DuplicatePageException : ParserException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicatePageException"/> class with a specified error message, a reference to the inner exception
        ///     that is the cause of this exception, the LDraw code which caused it and the line number on which the code appears.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        /// <param name="path">The path of the file.</param>
        /// <param name="code">The LDraw code which caused the exception.</param>
        /// <param name="line">The line number in the code which caused the exception.</param>
        public DuplicatePageException(string message, Exception innerException, string path, string code, uint line)
            : base(message, innerException, path, code, line)
        {
        }

        #endregion Constructor
    }

    #endregion DuplicatePageException

    #region ParserException

    /// <summary>
    /// The base class for exceptions thrown when parsing LDraw code.
    /// </summary>
    public abstract class ParserException : Exception
    {
        #region Properties

        /// <summary>
        /// Gets the path of the file.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the LDraw code which caused the exception.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Gets the line number in the code which caused the exception.
        /// </summary>
        public uint Line { get; private set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserException"/> class with a specified error message, a reference to the inner exception
        ///     that is the cause of this exception, the LDraw code which caused it and the line number on which the code appears.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        /// <param name="path">The path of the file.</param>
        /// <param name="code">The LDraw code which caused the exception.</param>
        /// <param name="line">The line number in the code which caused the exception.</param>
        protected ParserException(string message, Exception innerException, string path, string code, uint line)
            : base(message, innerException)
        {
            Path = path;
            Code = code;
            Line = line;
        }

        #endregion Constructor
    }

    #endregion ParserException

    #region ElementLockedException

    /// <summary>
    /// The exception that is thrown if an attempt is made to modify an <see cref="T:Digitalis.LDTools.DOM.API.IPageElement"/> which is
    ///     <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see>.
    /// </summary>
    public sealed class ElementLockedException : InvalidOperationException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementLockedException"/> class.
        /// </summary>
        public ElementLockedException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementLockedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ElementLockedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementLockedException"/> class with a specified error message and a reference to the inner
        ///     exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public ElementLockedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion Constructor
    }

    #endregion ElementLockedException

    #region ObjectFrozenException

    /// <summary>
    /// The exception that is thrown if an attempt is made to modify an <see cref="T:Digitalis.LDTools.DOM.API.IDOMObject"/> which is
    ///     <see cref="P:Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>.
    /// </summary>
    public sealed class ObjectFrozenException : InvalidOperationException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFrozenException"/> class.
        /// </summary>
        public ObjectFrozenException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFrozenException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ObjectFrozenException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFrozenException"/> class with a specified error message and a reference to the inner
        ///     exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public ObjectFrozenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion Constructor
    }

    #endregion ObjectFrozenException

    #region SyntaxException

    /// <summary>
    /// The exception that is thrown when a syntax error is detected whilst parsing LDraw code.
    /// </summary>
    public sealed class SyntaxException : ParserException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxException"/> class with a specified error message, a reference to the inner exception
        ///     that is the cause of this exception, the LDraw code which caused it and the line number on which the code appears.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        /// <param name="path">The path of the file.</param>
        /// <param name="code">The LDraw code which caused the exception.</param>
        /// <param name="line">The line number in the code which caused the exception.</param>
        public SyntaxException(string message, Exception innerException, string path, string code, uint line)
            : base(message, innerException, path, code, line)
        {
        }

        #endregion Constructor
    }

    #endregion SyntaxException
}
