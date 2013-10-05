using System;
using System.Runtime.Serialization;

namespace Adglopez.ServiceDocumenter.Services.Contracts.Data
{
    [DataContract(Namespace = Namespaces.Documenter)]
    public class SharedHeader
    {
        [DataMember]
        public string Software { get; set; }

        [DataMember]
        public  Guid? TraceId { get; set; }

        [DataMember]
        public string User { get; set; }
    }
}