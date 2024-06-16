using System;

namespace SharedCore.Extensions
{
    public static class StringToByteArrayExtension
    {
        /// <summary>
        /// Converts string to byte array.
        /// </summary>
        /// <param name="aData"> The data to convert in format: 'XX-XX-XX-XX-...'. </param>
        /// <returns> byte array </returns>
        public static byte[] ToByteArray(this string aData)
        {
            if (!aData.CanConvertToByteArray())
            {
                return new byte[0];
            }

            String[] data = aData.Split('-');
            byte[] bytes = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                bytes[i] = Convert.ToByte(data[i], 16);
            }

            return bytes;
        }

        /// <summary>
        /// Checks whether string can be converted to byte array.
        /// </summary>
        /// <param name="aData"> The string to convert. </param>
        /// <returns> true - string can be converted, otherwise - false. </returns>
        public static bool CanConvertToByteArray(this string aData)
        {
            if (string.IsNullOrEmpty(aData))
            {
                return false;
            }

            String[] data = aData.Split('-');

            foreach (string item in data)
            {
                try
                {
                    Convert.ToByte(item, 16);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether string can be converted to byte array of requested size.
        /// </summary>
        /// <param name="aData"> The string to convert. </param>
        /// <param name="aMaxByteArrayLength"> The max size of byte array.</param>
        /// <returns> true - string can be converted, otherwise - false. </returns>
        public static bool CanConvertToByteArrayOfSize(this string aData, int aMaxByteArrayLength = 244)
        {
            if (string.IsNullOrEmpty(aData))
            {
                return false;
            }

            String[] data = aData.Split('-');
            if (data.Length > aMaxByteArrayLength)
            {
                return false;
            }

            foreach (string item in data)
            {
                try
                {
                    Convert.ToByte(item, 16);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
