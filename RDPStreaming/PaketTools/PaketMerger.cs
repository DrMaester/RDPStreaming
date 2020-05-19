using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDPStreaming.Model;

namespace RDPStreaming.PaketTools
{
    public static class PaketMerger
    {
        public static byte[] MergePakets(Paket[] pakets)
        {
            return pakets.SelectMany(p => p.Data).ToArray();
        }
    }
}
