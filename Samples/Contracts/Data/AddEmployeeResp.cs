using System.Runtime.Serialization;

namespace Adglopez.ServiceDocumenter.Services.Contracts.Data
{
    [DataContract(Namespace = Namespaces.Documenter)]
    public class AddEmployeeResp
    {
        [DataMember]
        public SharedHeader Header { get; set; }

        [DataMember]
        public Status Body { get; set; }
    }
}