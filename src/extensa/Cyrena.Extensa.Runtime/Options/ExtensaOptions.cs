using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Extensa.Options
{
    public class ExtensaOptions
    {
        public string InstallationsDirectory { get; set; } = default!;
        public string ExtensionsDirectory { get; set; } = default!;
        public string ExtensionInfoFileName { get; set; } = "extension.json";
    }
}
