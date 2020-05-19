using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RDPStreaming.Model;

namespace RDPStreaming.PaketTools
{
    public static class PaketSplitter
    {
        private static int _paketSizeWithoutData = Paket.GetSizeOfPaketWithoutData();

        public static Paket[] GetPakets(int paketSize, PaketType paketType, byte[] data)
        {
            // Wenn data kleiner als die vorgegebene maximalgröße ist
            // braucht diese nicht gesplitted werden und muss in kein großen Buffer gesteckt werden
            paketSize = paketSize < (data.Length + _paketSizeWithoutData) ? paketSize : (data.Length + _paketSizeWithoutData);

            var paketGroupId = Guid.NewGuid();
            List<Paket> pakets = new List<Paket>();
            using (var memoryStream = new MemoryStream(data))
            {
                while (memoryStream.Position < memoryStream.Length)
                {
                    byte[] buffer = new byte[paketSize - _paketSizeWithoutData];
                    var paket = new Paket();
                    paket.PaketGroupId = paketGroupId;
                    paket.PaketType = paketType;
                    paket.PaketNumber = pakets.Count;

                    if (memoryStream.Position == 0)
                    {
                        paket.PaketPosition = PaketPosition.Start;
                        memoryStream.Read(buffer, 0, buffer.Length);
                        paket.Data = buffer;
                        pakets.Add(paket);
                        continue;
                    }

                    var readed = memoryStream.Read(buffer, 0, buffer.Length);
                    paket.Data = buffer.Take(readed).ToArray();

                    if (memoryStream.Position == memoryStream.Length)
                    {
                        paket.PaketPosition = PaketPosition.End;
                        pakets.Add(paket);
                        continue;
                    }

                    paket.PaketPosition = PaketPosition.Between;
                    pakets.Add(paket);
                }

                foreach (var paket in pakets)
                {
                    paket.PaketMaxCount = pakets.Count;
                }

                return pakets.ToArray();
            }
        }
    }
}
