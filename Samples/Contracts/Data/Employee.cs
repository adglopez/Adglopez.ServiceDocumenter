using System;
using System.Runtime.Serialization;

namespace Adglopez.ServiceDocumenter.Services.Contracts.Data
{
    [DataContract]
    public class Employee
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Age { get; set; }
    }
}