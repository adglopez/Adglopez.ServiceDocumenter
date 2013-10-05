using System.Collections.Generic;

namespace Adglopez.ServiceDocumenter.Core.Model
{
    public class Service
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Url { get; set; }
        public string Contract { get; set; }
        public List<Endpoint> Endpoints { get; set; }
        public List<Operation> Operations { get; set; }
    }
}
