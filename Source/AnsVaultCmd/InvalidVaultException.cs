using System;

namespace AnsVaultCmd
{
    public class InvalidVaultException : Exception
    {
        public InvalidVaultException(string message)
            : base(message)
        {
        }
    }
}
