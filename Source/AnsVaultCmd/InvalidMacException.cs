using System;

namespace AnsVaultCmd
{
    public class InvalidMacException : Exception
    {
        public InvalidMacException(string message)
            : base(message)
        {
        }
    }
}
