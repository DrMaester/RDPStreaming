using System;
using System.Collections.Generic;
using System.Text;

namespace RDPStreaming.Model
{
    public enum PaketType
    {
        Registration,
        Connect,
        ConnectAsControl,
        Request,
        Confirmation,
        Greeting,
        Message,
        AudioMp3Buffer,
        ScreenCapture
    }
}
