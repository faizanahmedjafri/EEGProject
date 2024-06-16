using System.Threading.Tasks;

namespace SharedCore.Services
{
    public interface ILogService
    {
        Task WriteInFileAsync();
        Task CreateLogDataAsync(string data);
        Task ReadFileAsync();
    }
}
