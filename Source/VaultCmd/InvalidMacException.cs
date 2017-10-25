using System;

namespace VaultCmd
{
    public class InvalidMacException : Exception
    {
        public InvalidMacException(string message)
            : base(message)
        {
        }
    }
}
