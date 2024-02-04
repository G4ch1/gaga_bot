using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gaga_bot
{
    public enum VoiceStateEnums : byte
    {
        Normal = 0x0,
        Suppressed = 0x1,
        Muted = 0x2,
        Deafened = 0x4,
        SelfMuted = 0x8,
        SelfDeafened = 0x10,
        SelfStream = 0x20,
        SelfVideo = 0x40
    }

    public enum Estimate
    {
        Likes = 1,
        Dislikes = 0
    }

    public enum TimeEnum
    {
        Minutes,
        Hours,
        Days
    }

    public enum RoleInteraction
    {
        Give,
        Remove
    }

    public enum OnOff
    {
        Off,
        On
    }
}
