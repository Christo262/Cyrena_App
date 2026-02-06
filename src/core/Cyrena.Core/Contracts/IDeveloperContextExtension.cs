using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Contracts
{
    public interface IDeveloperContextExtension
    {
        int Priority { get; }
        Task ExtendAsync(IDeveloperContextBuilder builder);
    }
}
