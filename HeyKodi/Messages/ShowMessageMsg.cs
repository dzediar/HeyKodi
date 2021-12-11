using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyKodi.Messages
{
    public class ShowMessageMsg : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public ShowMessageMsg(object sender, string title, string message, ShowMessageType messageType)
            : base(sender)
        {
            Title = title;
            Message = message;
            MessageType = messageType;
        }

        public ShowMessageMsg(object sender, string title, Exception exception)
            : base(sender)
        {
            Title = title;
            Exception = exception;
            MessageType = ShowMessageType.Error;
        }   

        public string Title { get; private set; }

        public string Message { get; private set; }

        public ShowMessageType MessageType { get; private set; }

        public Exception Exception { get; private set; }

        public bool Result { get; set; }
    }

    public enum ShowMessageType
    {
        Message,
        Question,
        Warning,
        Error
    }
}
