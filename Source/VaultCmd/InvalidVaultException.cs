using System;

namespace VaultCmd
{
    public class InvalidVaultException : Exception
    {
        public InvalidVaultException(string message)
            : base(message)
        {
        }
    }
}
