using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        public Paket(byte[] dataStream)
        {
            try
            {
                using (MemoryStream m = new MemoryStream(dataStream))
                {
                    using (BinaryReader reader = new BinaryReader(m))
                    {
                        TypeMessage = (DataIdentifier)reader.ReadInt32();
                        UserName = reader.ReadString();
                        Message = reader.ReadString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem Read DATA " + ex.Message);
            }
        }

        public byte[] GetDataStream()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write((int)TypeMessage);
                    writer.Write(UserName);
                    writer.Write(Message);
                }
                return m.ToArray();
            }
        }

    }
}
