using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Excel;
using Microsoft.AspNetCore.Http;
using SimpleRedirects.Core.Enums;
using SimpleRedirects.Core.Models;

namespace SimpleRedirects.Core.Services;

public class ExcelImportExportService : IImportExportService
{
    private readonly RedirectRepository _redirectRepository;

    public ExcelImportExportService(RedirectRepository redirectRepository)
    {
        _redirectRepository = redirectRepository;
    }

    public DataRecordCollectionFile ExportDataRecordCollection()
    {
        var records = _redirectRepository.GetAllRedirects();
        using var memoryStream = new MemoryStream();
        using (var excelWriter = new ExcelWriter(memoryStream, "Redirect list", CultureInfo.InvariantCulture))
        {
            excelWriter.Context.RegisterClassMap<RedirectMap>();
            excelWriter.WriteHeader<Redirect>();
            excelWriter.NextRecord();
            excelWriter.WriteRecords(records);
        }

        return new DataRecordCollectionFile(DataRecordProvider.Excel, memoryStream.ToArray());
    }

    public ImportRedirectsResponse ImportRedirectsFromCollection(IFormFile file, bool overwriteMatches)
    {
        if (file.Length <= 0) return ImportRedirectsResponse.EmptyImportRecordResponse();
        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        using var excelParser = new ExcelParser(memoryStream, CultureInfo.InvariantCulture);
        using var csvReader = new CsvReader(excelParser);
        csvReader.Context.RegisterClassMap<RedirectMap>();
        var records = csvReader.GetRecords<Redirect>().ToArray();
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