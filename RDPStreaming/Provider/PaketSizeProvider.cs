using System;
using System.Collections.Generic;
using System.Text;

namespace RDPStreaming.Provider
{
    public static class PaketSizeProvider
    {
        public static int GetMaxPaketSize()
        {
            return 60 * 1024;
        }
    }
}
