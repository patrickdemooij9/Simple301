using System.Globalization;
using CsvHelper.Configuration;

namespace SimpleRedirects.Core.Models;

public sealed class RedirectMap : ClassMap<Redirect>
{
    public RedirectMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Id).Ignore();
    }
}