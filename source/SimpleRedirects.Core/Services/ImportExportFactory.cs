using System;
using SimpleRedirects.Core.Enums;

namespace SimpleRedirects.Core.Services;

public class ImportExportFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ImportExportFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IImportExportService GetDataRecordProvider(DataRecordProvider dataRecordProvider)
        => dataRecordProvider == DataRecordProvider.Csv
            ? (IImportExportService)_serviceProvider.GetService(typeof(CsvImportExportService))
            : (IImportExportService)_serviceProvider.GetService(typeof(ExcelImportExportService));
}