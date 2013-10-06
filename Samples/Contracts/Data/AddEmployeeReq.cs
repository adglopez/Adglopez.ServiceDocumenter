using System.Runtime.Serialization;

namespace Adglopez.ServiceDocumenter.Services.Contracts.Data
{
    public class AddEmployeeReq
    {
        [DataMember]
        public string RequestID { get; set; }

        [DataMember]
        public SharedHeader Header { get; set; }

        [DataMember]
        public Employee Body { get; set; }

        [DataMember]
        public Employee[] OldEmployees { get; set; }
    }
}