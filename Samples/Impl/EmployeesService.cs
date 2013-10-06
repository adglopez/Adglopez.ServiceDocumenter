using System;
using Adglopez.ServiceDocumenter.Services.Contracts.Data;
using Adglopez.ServiceDocumenter.Services.Contracts.Services;

namespace Adglopez.ServiceDocumenter.Services.Impl
{
    public class EmployeesService : IEmployeesService
    {
        public AddEmployeeResp Add(AddEmployeeReq req)
        {
            return new AddEmployeeResp
                   {
                       Header = new SharedHeader
                                {
                                    Software = "Employee Service",
                                    TraceId = Guid.NewGuid(),
                                    User = "AUser"
                                },
                       Body = new Status
                              {
                                  IsSuccessfull = true,
                                  Description = string.Empty
                              }
                   };
        }

        //public DateTime Echo()
        //{
        //    return DateTime.UtcNow;
        //}
    }
}