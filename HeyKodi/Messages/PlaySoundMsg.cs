using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyKodi.Messages
{
    public class PlaySoundMsg : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public PlaySoundMsg(object sender, string soundSource, string speech)
            : base(sender)
        {
            SoundSource = soundSource;
            Speech = speech;
        }

        public string SoundSource { get; private set; }

        public string Speech { get; private set; }
    }
}
