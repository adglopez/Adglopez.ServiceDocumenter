using System;

namespace Adglopez.ServiceDocumenter.Core.Model
{
    public class Endpoint
    {
        public string Name { get; set; }
        public Uri Address { get; set; }
        public Binding Binding { get; set; }
        
    }
}
