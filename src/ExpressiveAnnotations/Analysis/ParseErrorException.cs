/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;

namespace ExpressiveAnnotations.Analysis
{
    using ExpressiveAnnotations.Infrastructure;

    /// <summary>
    ///     The exception thrown when parse operation detects error in a specified expression.
    /// </summary>
    [DataContract] // this attribute is not inherited from Exception and must be specified otherwise serialization will fail
    public class ParseErrorException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        public ParseErrorException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ParseErrorException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ParseErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="location">The error location.</param>
        public ParseErrorException(string error, string expression, Location location)
            : base(location.BuildParseError(error, expression))
        {
            Error = error;
            Expression = expression;
            Location = location.Clone();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="location">The error location.</param>        
        /// <param name="innerException">The inner exception.</param>
        public ParseErrorException(string error, string expression, Location location, Exception innerException)
            : base(location.BuildParseError(error, expression), innerException)
        {
            Error = error;
            Expression = expression;
            Location = location.Clone();
        }
        
        /// <summary>
        ///     Gets the error message.
        /// </summary>
        [DataMember]
        public string Error { get; private set; }

        /// <summary>
        ///     Gets the expression.
        /// </summary>        
        [DataMember]
        public string Expression { get; private set; }

        /// <summary>
        ///     Gets the error location.
        /// </summary>
        [DataMember]
        public Location Location { get; private set; }
    }
}
