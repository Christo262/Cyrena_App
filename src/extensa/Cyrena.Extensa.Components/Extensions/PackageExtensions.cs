using Cyrena.Extensa.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Extensa.Extensions
{
    public static class PackageExtensions
    {
        public static PackageVersion LatestVersion(this Package item)
        {
            return item.Versions.OrderByDescending(x => x.Version).FirstOrDefault() ?? new PackageVersion();
        }
    }
}
