using System;

namespace Synapsical.Synapse.SqlPool.Client
{
    // Custom exceptions for error handling
    public class SynapseSqlPoolException : Exception
    {
        public SynapseSqlPoolException(string message, Exception? inner = null) : base(message, inner) { }
    }
}
