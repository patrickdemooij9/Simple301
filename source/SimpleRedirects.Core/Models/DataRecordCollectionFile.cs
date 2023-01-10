using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SimpleRedirects.Core.Enums;

namespace SimpleRedirects.Core.Models;

public class DataRecordCollectionFile
{
    public DataRecordProvider DataRecordProvider { get; set; }
    public byte[] File { get; set; }
    public string ContentType => DataRecordProvider == DataRecordProvider.Csv ? "text/csv" : "application/vnd.ms-excel";
    public string FileExtension => DataRecordProvider == DataRecordProvider.Csv ? ".csv" : ".xlsx";
    public string FileName => $"SimpleRedirects-{DataRecordProvider}-Export-{DateTimeOffset.Now.ToString("dd-M-yyyy", CultureInfo.InvariantCulture)}{FileExtension}";

    public DataRecordCollectionFile(DataRecordProvider dataRecordProvider, byte[] file)
    {
        DataRecordProvider = dataRecordProvider;
        File = file;
    }

    public FileContentResult AsFileContentResult() => new(File, ContentType) { FileDownloadName = FileName };
}