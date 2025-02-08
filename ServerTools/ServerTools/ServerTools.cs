namespace ServerTools
{
    public static class Tools
    {
        public static char[] GetStringFromBytes(byte[] bytes, ref int startIndex)
        {
            int length = bytes[startIndex];
            startIndex++;

            char[] newString = new char[length];

            for (int i = 0; i + startIndex < startIndex + length; i++)
            {
                newString[i] = (char)bytes[startIndex + i];
            }

            startIndex += newString.Length;
            return newString;
        }

        public static string GetStringFromBytes(ref int startIndex, byte[] bytes)
        {
            int length = bytes[startIndex];
            startIndex++;

            char[] newString = new char[length];

            for (int i = 0; i + startIndex < startIndex + length; i++)
            {
                newString[i] = (char)bytes[startIndex + i];
            }

            startIndex += newString.Length;
            return new string(newString);
        }

        /// <summary>
        /// Codes name as bytes, first index filled with name's length
        /// </summary>
        /// <param name="startIdx"></param>
        /// <param name="bytes"></param>
        /// <param name="name"></param>
        public static void CodeStringAsBytes(ref int startIdx, byte[] bytes, string text)
        {
            bytes[startIdx] = (byte)text.Length;
            startIdx++;

            foreach (char c in text)
            {
                bytes[startIdx] = (byte)c;
                startIdx++;
            }
        }

        public static void CodeStringAsBytes(ref int startIdx, byte[] bytes, char[] text)
        {
            bytes[startIdx] = (byte)text.Length;
            startIdx++;

            foreach (char c in text)
            {
                bytes[startIdx] = (byte)c;
                startIdx++;
            }
        }
    }
}
