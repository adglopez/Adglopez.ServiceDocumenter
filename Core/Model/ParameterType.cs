using System.Collections.Generic;

namespace Adglopez.ServiceDocumenter.Core.Model
{
    public class ParameterType
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public bool IsCollection { get; set; }
        public bool IsOptional { get; set; }
        public bool IsComplex { get; set; }
        public bool? IsOut { get; set; }
        public int Position { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public Dictionary<string, ParameterType> Childs { get; set; }        
    }
}
