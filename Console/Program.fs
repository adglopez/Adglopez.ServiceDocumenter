[<EntryPoint>]
let main argv = 
    
    let metadataReader = new Adglopez.ServiceDocumenter.Core.Metadata.MetadataReader()

    let url = if argv.Length > 0 then argv.GetValue(0).ToString()
              else "http://localhost/Adglopez.Documenter.Samples/Employees.svc"
              
    let outFile = if argv.Length > 1 then argv.GetValue(1).ToString()
                  else "Employees.xlsx"

    let service = metadataReader.ParseMetadata(url)
    
    printfn "%A" argv
    0 // return an integer exit code

