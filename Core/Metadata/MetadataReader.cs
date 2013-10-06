using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using Adglopez.ServiceDocumenter.Core.Model;
using Adglopez.ServiceDocumenter.Core.Metadata.Exceptions;

namespace Adglopez.ServiceDocumenter.Core.Metadata
{
    public class MetadataReader
    {
        /// <summary>
        /// Parse a Wsdl and return a model with the relevant information
        /// </summary>
        /// <param name="url">Url of the service</param>
        public Service ParseMetadata(string url)
        {
            Collection<ContractDescription> contracts;
            ServiceEndpointCollection allEndpoints;

            LoadMetadata(url, out contracts, out allEndpoints);

            Dictionary<string, IEnumerable<ServiceEndpoint>> endpointsForContracts;
            var generator = GenerateTypeInformationForContracts(contracts, allEndpoints, out endpointsForContracts);

            Type clientProxyType;

            CompileProxy(generator, out clientProxyType);

            // Once the proxy type is found it obtains the Interface that correspondes to the service contract
            Type contractInterface = clientProxyType.GetInterfaces().First(i => i.FullName != typeof(ICommunicationObject).FullName && i.FullName != typeof(IDisposable).FullName);

            var contract = contracts.Single(c => c.Name == contractInterface.Name);
            
            var modelService = new Service
            {
                Name = new Uri(url).Segments.Last(),
                Url = url,
                Contract = contract.Name,
                Namespace = contract.Namespace,
                Endpoints = AddEndpoints(endpointsForContracts),
                Operations = AddModelOperations(clientProxyType)
            };

            return modelService;
        }

        /// <summary>
        /// Parse a Wsdl and return a model with the relevant information
        /// </summary>
        /// <param name="url">Url of the service</param>
        /// <param name="proxyType">Type of the proxy created for the given service</param>
        /// <param name="proxyAssembly">Assembly that contains all the types related to the proxy</param>
        /// <param name="endpointsPerContract">Available endpoints to call the service</param>
        public void ObtainProxyTypes(string url, out Type proxyType, out Assembly proxyAssembly, out Dictionary<string, IEnumerable<ServiceEndpoint>> endpointsPerContract)
        {
            Collection<ContractDescription> contracts;
            ServiceEndpointCollection allEndpoints;

            LoadMetadata(url, out contracts, out allEndpoints);

            Dictionary<string, IEnumerable<ServiceEndpoint>> endpointsForContracts;
            var generator = GenerateTypeInformationForContracts(contracts, allEndpoints, out endpointsForContracts);

            Type clientProxyType;

            var assembly = CompileProxy(generator, out clientProxyType);
            
            endpointsPerContract = endpointsForContracts;
            proxyType = clientProxyType;
            proxyAssembly = assembly;
        }

        /// <summary>
        /// Executes an actual call to a particular proxy. It supports multiple operation parameters and needs the same set of object previously created using CreateProxy. If an instance of the proxy is not provided a new one can be created on the fly.
        /// </summary>
        /// <param name="instance">Nullable. A proxy instance</param>
        /// <param name="assembly">The assembly created by calling CreateProxy</param>
        /// <param name="proxyType">The proxy type created by calling CreateProxy</param>
        /// <param name="endpoint">Endpoint to be used if an instance of the proxy needs to be created. So it will pick the proper values from this.</param>
        /// <param name="opearationName">Service operation to be called</param>
        /// <param name="operationParameters">Operation parameters</param>
        /// <returns></returns>
        public object InvokeProxyOperation(object instance, Assembly assembly, Type proxyType, ServiceEndpoint endpoint, string opearationName, params object[] operationParameters)
        {
            // Get the operation's method, invoke it, and get the return value
            if (instance == null)
            {
                instance = InstantiateProxy(assembly, proxyType, endpoint);
            }

            var operation = instance.GetType().GetMethod(opearationName);

            object retVal = operation.Invoke(instance, operationParameters);

            Console.WriteLine(retVal.ToString());

            return retVal;
        }

        #region Metadata / Model Methods

        private void LoadMetadata(string url, out Collection<ContractDescription> contracts, out ServiceEndpointCollection endpoints)
        {
            // Define the metadata address, contract name, operation name, and parameters. 
            // You can choose between MEX endpoint and HTTP GET by changing the address and enum value.
            var mexAddress = new Uri(url.EndsWith(".svc", StringComparison.InvariantCultureIgnoreCase) ? url + "?wsdl" : url);

            // For MEX endpoints use a MEX address and a mexMode of .MetadataExchange
            const MetadataExchangeClientMode mexMode = MetadataExchangeClientMode.HttpGet;

            // Get the metadata file from the service.
            var mexClient = new MetadataExchangeClient(mexAddress, mexMode) { ResolveMetadataReferences = true };
            MetadataSet metaSet = mexClient.GetMetadata();

            // Import all contracts and endpoints
            var importer = new WsdlImporter(metaSet);

            contracts = importer.ImportAllContracts();
            endpoints = importer.ImportAllEndpoints();

            if (contracts.Count == 0)
            {
                throw new WebException("Contract information could not be obtained from the specified url (" + url + ")");
            }

            if (endpoints.Count == 0)
            {
                throw new WebException("Endpoint information could not be obtained from the specified url (" + url + ")");
            }
        }

        private List<Endpoint> AddEndpoints(Dictionary<string, IEnumerable<ServiceEndpoint>> endpointsForContracts)
        {
            var endpoints = new List<Endpoint>();
            // List the available endpoints for the Contract
            foreach (var contractEndpoint in endpointsForContracts)
            {
                endpoints.AddRange(contractEndpoint.Value.Select(ep => new Endpoint
                {
                    Binding = new Binding
                    {
                        Name = ep.Binding.Name,
                        Type = ep.Binding.GetType().Name
                    },
                    Name = ep.Name,
                    Address = ep.Address.Uri,
                }));
            }
            return endpoints;
        }

        private List<Operation> AddModelOperations(Type clientProxyType)
        {
            var modelOperations = new List<Operation>();

            foreach (var operation in clientProxyType.GetMethods().Where(op => ReflectionHelper.IsServiceOperation(op)))
            {
                var modelOperation = new Operation { Name = operation.Name, Input = new List<ParameterType>(), Output = new List<ParameterType>() };

                foreach (var inputParameter in operation.GetParameters())
                {
                    var parameter = AddOperationParamter(inputParameter);

                    modelOperation.Input.Add(parameter);
                }

                if (operation.ReturnParameter != null)
                {
                    var parameter = AddOperationParamter(operation.ReturnParameter);

                    modelOperation.Output.Add(parameter);
                }

                modelOperations.Add(modelOperation);
            }
            return modelOperations;
        }

        private ParameterType AddOperationParamter(ParameterInfo parameterInfo)
        {
            var modelParameter = new ParameterType
            {
                Name = parameterInfo.Name,
                FullTypeName = parameterInfo.ParameterType.FullName,
                TypeName = parameterInfo.ParameterType.Name,
                Position = parameterInfo.Position,
                IsOut = parameterInfo.IsOut,
                IsOptional = parameterInfo.IsOptional,
                IsComplex = !ReflectionHelper.IsSimpleType(parameterInfo.ParameterType),
                IsCollection = ReflectionHelper.IsCollection(parameterInfo),
                Childs = new Dictionary<string, ParameterType>(),
                Properties = new Dictionary<string, string>()
            };

            var parameterType = parameterInfo.ParameterType;

            foreach (var paramProperty in parameterType.GetProperties())
            {
                var paramPropertyType = paramProperty.PropertyType;

                if (ReflectionHelper.IsSimpleType(paramPropertyType))
                {
                    modelParameter.Properties.Add(paramProperty.Name, ReflectionHelper.GetFriendlyTypeName(paramPropertyType));
                }
                else
                {
                    if (paramPropertyType.FullName != typeof(ExtensionDataObject).FullName)
                    {
                        modelParameter.Childs.Add(paramProperty.Name, RecurseChild(paramPropertyType));
                    }
                }
            }
            return modelParameter;
        }

        private ParameterType RecurseChild(Type type)
        {
            var parameter = new ParameterType
            {
                Name = type.Name,
                TypeName = type.ToString(),
                IsOut = null,
                IsCollection = ReflectionHelper.IsCollection(type),
                Properties = new Dictionary<string, string>(),
                Childs = new Dictionary<string, ParameterType>()
            };

            if (parameter.IsCollection)
            {
                type = type.GetElementType();
            }

            foreach (var paramProperty in type.GetProperties())
            {
                var paramPropertyType = paramProperty.PropertyType;

                if (ReflectionHelper.IsSimpleType(paramPropertyType))
                {
                    parameter.Properties.Add(paramProperty.Name, ReflectionHelper.GetFriendlyTypeName(paramPropertyType));
                }
                else
                {
                    if (paramPropertyType.FullName != typeof(ExtensionDataObject).FullName)
                    {
                        parameter.Childs.Add(paramProperty.Name, RecurseChild(paramPropertyType));
                    }
                }
            }

            return parameter;
        }

        #endregion

        #region Dynamic Proxy Methods

        private ServiceContractGenerator GenerateTypeInformationForContracts(IEnumerable<ContractDescription> contracts, ServiceEndpointCollection allEndpoints, out Dictionary<string, IEnumerable<ServiceEndpoint>> endpointsForContracts)
        {
            // Generate type information for each contract
            var generator = new ServiceContractGenerator();

            endpointsForContracts = new Dictionary<string, IEnumerable<ServiceEndpoint>>();

            foreach (ContractDescription contract in contracts)
            {
                generator.GenerateServiceContractType(contract);

                // Keep a list of each contract's endpoints
                endpointsForContracts[contract.Name] = allEndpoints.Where(se => se.Contract.Name == contract.Name).ToList();
            }

            if (generator.Errors.Count != 0)
            {
                throw new ProxyCompilerException("There were errors trying to generate types for the endpoints / contracts.");
            }
            return generator;
        }

        private Assembly CompileProxy(ServiceContractGenerator generator, out Type clientProxyType)
        {
            // Generate a code file for the contracts 
            var codeDomProvider = CodeDomProvider.CreateProvider("C#");

            // Compile the code file to an in-memory assembly
            // Don't forget to add all WCF-related assemblies as references
            var compilerParameters = new CompilerParameters(new[]
                                                            {
                                                                "System.dll", "System.ServiceModel.dll",
                                                                "System.Runtime.Serialization.dll"
                                                            }) { GenerateInMemory = true };

            var results = codeDomProvider.CompileAssemblyFromDom(compilerParameters, generator.TargetCompileUnit);

            if (results.Errors.Count > 0)
            {
                throw new ProxyCompilerException("There were errors during generated code compilation");
            }

            // Find the proxy type that was generated for the specified contract
            // (identified by a class that implements the contract and ICommunicationObject)
            clientProxyType = results.CompiledAssembly.GetTypes().FirstOrDefault(
                t => t.IsClass
                     && t.GetInterface(typeof(ICommunicationObject).Name) != null
                     && t.GetInterface(typeof(IDisposable).Name) != null);

            if (clientProxyType == null)
            {
                throw new ClientNotFoundException("Proxy assembly created correctly but no communication object could be created. Check the service conifguration and verify metadata is published correctly");
            }

            return results.CompiledAssembly;
        }        

        private object InstantiateProxy(Assembly asembly, Type clientProxyType, ServiceEndpoint serviceEndpoint)
        {
            // Create an instance of the proxy
            // Pass the endpoint's binding and address as parameters to the ctor
            object instance = asembly.CreateInstance(
                clientProxyType.Name,
                false,
                BindingFlags.CreateInstance,
                null,
                new object[] { serviceEndpoint.Binding, serviceEndpoint.Address },
                CultureInfo.CurrentCulture, null);

            return instance;
        }
                
        #endregion
    }
}
