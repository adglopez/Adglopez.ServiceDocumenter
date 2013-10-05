using System.Runtime.Serialization;

namespace Adglopez.ServiceDocumenter.Services.Contracts.Data
{
    [DataContract(Namespace = Namespaces.Documenter)]
    public class Status
    {
        [DataMember]
        public bool IsSuccessfull { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}