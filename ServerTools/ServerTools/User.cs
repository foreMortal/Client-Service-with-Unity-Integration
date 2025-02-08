using System;

namespace ServerTools
{
    public struct User
    {
        public uint Id;
        public float Points;
        public RegistrationDate RegistrationDate;
        public string Name;
        private const int staticDataSize = 12;

        public static User ConvertToUser(int startIndex, byte[] array)
        {
            uint _Id = BitConverter.ToUInt32(array, startIndex);
            startIndex += 4;
            float _Points = BitConverter.ToSingle(array, startIndex);
            startIndex += 4;
            RegistrationDate _RegistrationDate = RegistrationDate.GetFromBytes(startIndex, array);
            startIndex += 4;

            return new User()
            {
                Id = _Id,
                Points = _Points,
                RegistrationDate = _RegistrationDate,
                Name = Tools.GetStringFromBytes(ref startIndex, array),
            };
        }

        public void CodeUserAsBytes(int startIndex, byte[] array)
        {
            BitConverter.GetBytes(Id).CopyTo(array, startIndex);
            startIndex += 4;
            BitConverter.GetBytes(Points).CopyTo(array, startIndex);
            startIndex += 4;
            this.RegistrationDate.CodeAsBytes(startIndex, array);
            startIndex += 4;
            Tools.CodeStringAsBytes(ref startIndex, array, Name);
        }

        public int GetBytesCount()
        {
            int nameLength = Name.Length + 1;
            return staticDataSize + nameLength;
        }
    }
}
