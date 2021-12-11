using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyKodi.Messages
{
    public class MinimizeHeyKodiMsg : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public MinimizeHeyKodiMsg(object sender)
            : base(sender)
        {
        }
    }
}
