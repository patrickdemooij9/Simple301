using Microsoft.AspNetCore.Http;
using SimpleRedirects.Core.Models;

namespace SimpleRedirects.Core.Services;

public interface IImportExportService
{
    DataRecordCollectionFile ExportDataRecordCollection();
    ImportRedirectsResponse ImportRedirectsFromCollection(IFormFile file, bool overwriteMatches);
}