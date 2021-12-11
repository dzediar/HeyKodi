using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyKodi.Messages
{
    public class RunKodiCommandMsg : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public RunKodiCommandMsg(object sender, string command, object parameter)
            : base(sender)
        {
            Command = command;
            Parameter = parameter;
        }

        public string Command { get; private set; }

        public object Parameter { get; private set; }
    }
}
