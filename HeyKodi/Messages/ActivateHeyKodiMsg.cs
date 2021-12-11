using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyKodi.Messages
{
    public class ActivateHeyKodiMsg : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public ActivateHeyKodiMsg(object sender)
            : base(sender)
        {
        }
    }
}
