[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let url = if argv.Length > 0 then argv.GetValue(0).ToString() else "http://localhost:56137/Employees.svc"
    let document = if argv.Length > 1 then argv.GetValue(1).ToString() else "Employee.xlsx"

    let metadataReader = new Adglopez.ServiceDocumenter.Core.Metadata.MetadataReader()
    let exporter = new Adglopez.ServiceDocumenter.Exporters.Excel.Expoter()

    let serviceInfo = metadataReader.ParseMetadata(url)

    exporter.Export(serviceInfo, document)

    System.Console.WriteLine("Exporting {0} to {1} completed...", url, document)

    ignore (System.Diagnostics.Process.Start("Excel.exe", document))

    0 // return an integer exit code
