using System.Collections.Generic;

namespace PM8MP.Exception
{
    public class ByteCodeException : System.Exception
    {
        public ByteCodeException(IEnumerable<byte> code, string message) : base(message + "\t byte code is: " + string.Join(",", code))
        {
        }
    }
}