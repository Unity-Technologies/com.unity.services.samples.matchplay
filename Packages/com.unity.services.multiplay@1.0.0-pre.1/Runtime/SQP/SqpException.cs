using System;

namespace Unity.Ucg.Usqp
{
    /// <summary>
    /// Server Quality Protocol exception.
    /// </summary>
    public class SqpException : Exception
    {
        /// <summary>
        /// Constructs an Server Quality Protocol Exception.
        /// </summary>
        public SqpException()
        {
        }

        /// <summary>
        /// Constructs an Server Quality Protocol Exception.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public SqpException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs an Server Quality Protocol Exception.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The exception that caused this exception.</param>
        public SqpException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
