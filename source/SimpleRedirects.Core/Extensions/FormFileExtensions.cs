using Microsoft.AspNetCore.Http;
using SimpleRedirects.Core.Enums;

namespace SimpleRedirects.Core.Extensions;

internal static class FormFileExtensions
{
    /// <summary>
    /// Attempts to read a file's extension and translate this to a DataRecordProvider to be able to determine the correct service to use to handle the import file
    /// </summary>
    /// <param name="file">The file which will be used to determine the data record provider</param>
    /// <param name="dataRecordProvider"></param>
    /// <returns>True and a DataRecordProvider as out var when filetype can be matched to one, false and null when the file's type cannot be mapped to a DataRecordProvider</returns>
    internal static bool CanGetDataRecordProviderFromFile(this IFormFile file, out DataRecordProvider dataRecordProvider)
    {
        var fileType = System.IO.Path.GetExtension(file?.FileName);
        switch (fileType)
        {
            case ".csv":
                dataRecordProvider = DataRecordProvider.Csv;
                return true;
            case ".xlsx":
                dataRecordProvider = DataRecordProvider.Excel;
                return true;
            default:
                dataRecordProvider = DataRecordProvider.Csv;
                return false;
        }
    }
}