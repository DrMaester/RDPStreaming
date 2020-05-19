using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RDPStreaming.Model
{
    public class Paket
    {
        public int PaketNumber { get; set; }
        public int PaketMaxCount { get; set; }
        public Guid PaketGroupId { get; set; }
        public Guid JobId { get; set; }
        public PaketType PaketType { get; set; }
        public PaketPosition PaketPosition { get; set; }
        public byte[] Data { get; set; }

        public Paket()
        {

        }

        public Paket(PaketType paketType, byte[] dataBuffer)
        {
            PaketType = paketType;
            Data = dataBuffer;
            PaketGroupId = Guid.NewGuid();
            PaketMaxCount = 1;
            PaketPosition = PaketPosition.Start;
            PaketNumber = 1;
        }

        public Paket(byte[] paketBuffer)
        {
            using (var memoryStream = new MemoryStream(paketBuffer))
            {
                PaketNumber = memoryStream.ReadByte();
                PaketMaxCount = memoryStream.ReadByte();

                byte[] guidRaw = new byte[16];
                memoryStream.Read(guidRaw, 0, guidRaw.Length);
                PaketGroupId = new Guid(guidRaw);

                byte[] jobIdRaw = new byte[16];
                memoryStream.Read(jobIdRaw, 0, jobIdRaw.Length);
                JobId = new Guid(jobIdRaw);

                PaketType = (PaketType)memoryStream.ReadByte();
                PaketPosition = (PaketPosition)memoryStream.ReadByte();

                Data = new byte[memoryStream.Length - memoryStream.Position];
                memoryStream.Read(Data, 0, Data.Length);
            }
        }

        public byte[] ToBytes()
        {
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.WriteByte((byte)PaketNumber); // 1 Byte
                memoryStream.WriteByte((byte)PaketMaxCount); // 1 Byte
                var groupIdBuffer = PaketGroupId.ToByteArray();
                var jobIdBuffer = JobId.ToByteArray();
                memoryStream.Write(groupIdBuffer, 0, groupIdBuffer.Length); // 16 Bytes
                memoryStream.Write(jobIdBuffer, 0, jobIdBuffer.Length); // 16 Bytes
                memoryStream.WriteByte((byte)PaketType); // 1 Byte
                memoryStream.WriteByte((byte)PaketPosition); // 1 Byte
                if (Data != null)
                {
                    memoryStream.Write(Data, 0, Data.Length); // 
                }

                return memoryStream.ToArray();
            }
        }

        public static int GetSizeOfPaketWithoutData()
        {
            // PaketNumber (int) +1
            // PaketMaxCount (int) +1
            // PaketGroupId (Guid) +16
            // JobId (Guid) +16
            // PaketType (enum) +1
            // PaketPosition (enum) +1
            return 36;
        }
    }
}
