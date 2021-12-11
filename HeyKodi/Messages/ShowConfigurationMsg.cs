using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyKodi.Messages
{
    public class ShowConfigurationMsg : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public ShowConfigurationMsg(object sender)
            : base(sender)
        {
        }
    }
}
