using Adglopez.ServiceDocumenter.Core.Model;

namespace Adglopez.ServiceDocumenter.Core.Metadata
{
    public interface IExporter
    {
        void Export(Service service);
    }
}
