using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using SimpleRedirects.Core.Enums;
using SimpleRedirects.Core.Models;

namespace SimpleRedirects.Core.Services;

public class CsvImportExportService : IImportExportService
{
     private readonly RedirectRepository _redirectRepository;

    public CsvImportExportService(RedirectRepository redirectRepository)
    {
        _redirectRepository = redirectRepository;
    }

    public DataRecordCollectionFile ExportDataRecordCollection()
    {
        var records = _redirectRepository.GetAllRedirects();
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        csvWriter.Context.RegisterClassMap<RedirectMap>();
        csvWriter.WriteHeader<Redirect>();
        csvWriter.NextRecord();
        csvWriter.WriteRecords(records);
        csvWriter.Flush();
        streamWriter.Flush();
        memoryStream.Position = 0;

        return new DataRecordCollectionFile(DataRecordProvider.Csv, memoryStream.ToArray());
    }

    public ImportRedirectsResponse ImportRedirectsFromCollection(IFormFile file, bool overwriteMatches)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<RedirectMap>();
        var records = csv.GetRecords<Redirect>().ToArray();

        if (!records.Any()) return ImportRedirectsResponse.EmptyImportRecordResponse();
        var addedRedirects = 0;
        var updatedRedirects = 0;
        var existingRedirects = 0;
        var errorList = new List<Redirect>();

        foreach (var redirect in records)
        {
            if (_redirectRepository.FetchRedirectByOldUrl(redirect.OldUrl) is not { } existingRedirect)
            {
                try
                {
                    _redirectRepository.AddRedirect(redirect.IsRegex, redirect.OldUrl, redirect.NewUrl, redirect.RedirectCode, redirect.Notes);
                    addedRedirects++;
                }
                catch (ArgumentException e)
                {
                    redirect.Notes = e.Message;
                    errorList.Add(redirect);
                }
            }
            else if(overwriteMatches && !existingRedirect.Equals(redirect))
            {
                redirect.Id = existingRedirect.Id;
                try
                {
                    _redirectRepository.UpdateRedirect(redirect);
                    updatedRedirects++;
                }
                catch (ArgumentException e)
                {
                    redirect.Notes = e.Message;
                    errorList.Add(redirect);
                }
            }
            else
                existingRedirects++;
        }

        return ImportRedirectsResponse.FromImport(addedRedirects, updatedRedirects, existingRedirects, errorList.ToArray());
    }
}