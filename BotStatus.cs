using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace marktplaatsreposter
{
    public enum BotStatus
    {
        NOT_SIGNED_IN,
        PROCESSING,
        READY
    }

    public static class BotStatusMethods
    {
        public static Color GetColorForStatus(BotStatus status)
        {
            switch (status)
            {
                case BotStatus.NOT_SIGNED_IN:
                    return Color.Orange;
                case BotStatus.PROCESSING:
                    return Color.Red;
                case BotStatus.READY:
                    return Color.Green;
                default:
                    return Color.Black;
            }
        }
    }
}
