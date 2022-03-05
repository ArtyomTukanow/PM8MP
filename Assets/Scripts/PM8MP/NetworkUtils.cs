using PM8MP.Exception;

namespace PM8MP
{
    public static class NetworkUtils
    {
        public static string ToStringBytes(this byte[] bytes) => string.Join(",", bytes);
        
        public static bool IsByteCommand(this byte[] code)
        {
            if (code == null)
                return false;
            if(code.Length > byte.MaxValue)
                return false;
            if(code.Length < MPSettings.RESERVED_BYTES_LENGTH)
                return false;
            if(code.Length != code[0] + 1)
                return false;
            return true;
        }
        
        public static void ValidateByteCommand(this byte[] code)
        {
            if(code == null)
                throw new System.ArgumentNullException(nameof(code), "byte code is null");
            if(code.Length > byte.MaxValue)
                throw new ByteCodeException(code, "byte code length more than " + byte.MaxValue);
            if(code.Length < MPSettings.RESERVED_BYTES_LENGTH)
                throw new ByteCodeException(code, "byte's code command length must be more than 3");
            if(code.Length != code[0] + 1)
                throw new ByteCodeException(code, $"byte's code command length ({code.Length}) must equal first byte:{code[0]} + 1");
        }

        public static byte GetCommandType(this byte[] code) => code[1];
        public static byte GetCommandPlayer(this byte[] code) => code[2];
    }
}