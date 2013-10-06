using System;
using System.ServiceModel;
using Adglopez.ServiceDocumenter.Services.Contracts.Data;

namespace Adglopez.ServiceDocumenter.Services.Contracts.Services
{
    [ServiceContract(Namespace = Namespaces.Documenter)]
    public interface IEmployeesService
    {
        [OperationContract]
        AddEmployeeResp Add(AddEmployeeReq req);

        //[OperationContract]
        //DateTime Echo();
    }
}