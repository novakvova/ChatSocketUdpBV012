using System;
using System.Collections.Generic;
using System.Text;

namespace ChatLib
{
    public enum DataIdentifier
    {
        Message,
        LogIn,
        LogOut,
        Null
    }

    public class Paket
    {
        public DataIdentifier TypeMessage { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }

        public Paket() { }

        /// <summary>
        /// Отримання даних від клієнта, розбираємо байти
        /// </summary>
        /// <param name="dataStream">масив байтів</param>
        public Paket(byte [] dataStream)
        {
            TypeMessage = (DataIdentifier)BitConverter.ToInt32(dataStream, 0);
            //довжина імені користувача
            int nameLength = BitConverter.ToInt32(dataStream, 4);
            //довжина повідомлення
            int msgLength = BitConverter.ToInt32(dataStream, 8);
            if (nameLength > 0)
                UserName = Encoding.UTF8.GetString(dataStream, 12, nameLength);
            else
                UserName = null;

            if (msgLength > 0)
                Message = Encoding.UTF8.GetString(dataStream, 12+nameLength, msgLength);
            else
                Message = null;
        }

        public byte[] GetDataStream()
        {
            List<byte> dataStream = new List<byte>();

            // Add the dataIdentifier
            dataStream.AddRange(BitConverter.GetBytes((int)TypeMessage));

            // Add the name length
            if (UserName != null)
                dataStream.AddRange(BitConverter.GetBytes(Encoding.UTF8.GetBytes(UserName).Length));
            else
                dataStream.AddRange(BitConverter.GetBytes(0));

            // Add the message length
            if (Message != null)
                dataStream.AddRange(BitConverter.GetBytes(Encoding.UTF8.GetBytes(Message).Length));
            else
                dataStream.AddRange(BitConverter.GetBytes(0));

            // Add the name
            if (UserName != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(UserName));

            // Add the message
            if (Message != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(Message));

            return dataStream.ToArray();
        }

    }
}
