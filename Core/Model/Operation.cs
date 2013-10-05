using System.Collections.Generic;

namespace Adglopez.ServiceDocumenter.Core.Model
{
    public class Operation
    {
        public string Name { get; set; }
        public List<ParameterType> Input { get; set; }
        public List<ParameterType> Output { get; set; } 
    }
}
