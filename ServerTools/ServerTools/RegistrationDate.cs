using System;

namespace ServerTools
{
    public struct RegistrationDate
    {
        public byte Day;
        public byte Month;
        public ushort Year;

        public static RegistrationDate GetFromBytes(int startIndex, byte[] array)
        {
            byte month = array[startIndex++];
            byte day = array[startIndex++];
            ushort year = BitConverter.ToUInt16(array, startIndex);

            return new RegistrationDate()
            {
                Day = day,
                Month = month,
                Year = year
            };
        }

        public void CodeAsBytes(int startIndex, byte[] array)
        {
            array[startIndex++] = Day;
            array[startIndex++] = Month;
            BitConverter.GetBytes(Year).CopyTo(array, startIndex);
        }
    }
}
