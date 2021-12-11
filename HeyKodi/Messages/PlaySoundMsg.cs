using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyKodi.Messages
{
    public class PlaySoundMsg : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public PlaySoundMsg(object sender, string soundSource)
            : base(sender)
        {
            SoundSource = soundSource;
        }

        public string SoundSource { get; private set; }
    }
}
