using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace biopot.Services.Sharing
{
    public interface IShareService
    {
        Task ShareFileAsync(IEnumerable<string> path);
    }
}
