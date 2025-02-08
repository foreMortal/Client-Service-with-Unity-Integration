using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTools
{
    public struct LoggingInfo
    {
        public string Login;
        public string Password;

        public static LoggingInfo GetFromBytes(int startIndex, byte[] array)
        {
            return new LoggingInfo()
            {
                Login = Tools.GetStringFromBytes(ref startIndex, array),
                Password = Tools.GetStringFromBytes(ref startIndex, array),
            };
        }

        public void CodeAsBytes(int startIndex, byte[] array)
        {
            Tools.CodeStringAsBytes(ref startIndex, array, Login);
            Tools.CodeStringAsBytes(ref startIndex, array, Password);
        }

        public int GetByteCount()
        {
            return 2 + Password.Length + Login.Length;
        }
    }
}
