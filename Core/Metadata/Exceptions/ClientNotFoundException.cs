using System;
using System.Runtime.Serialization;

namespace Adglopez.ServiceDocumenter.Core.Metadata.Exceptions
{
    [Serializable]
    public class ClientNotFoundException : Exception
    {
        private readonly string url;

        public ClientNotFoundException(string message) : base(message)
        {
        }

        public ClientNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info != null)
            {
                this.url = info.GetString("url");
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("url", this.url);
        }
    }
}
