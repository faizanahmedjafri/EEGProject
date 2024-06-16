using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SharedCore;
using SharedCore.Enums;
using SharedCore.Services;

namespace biopot.Services.LogService
{
    public class LogService : ILogService
    {
        private StringBuilder _stringBuilder;
        private readonly IFileIoService _fileIoService;
        private readonly string _fileName = "log";
        private readonly string _folderName = "BioPotLog";
        private readonly int _overdueInDays = -30;
        //private readonly int _overdueInDays = -1;
        private string _logData;
        private string _path;

        public LogService(IFileIoService fileIoService)
        {
            _stringBuilder = new StringBuilder();
            _fileIoService = fileIoService;
        }

        #region -- ILogService implementation --

        public async Task CreateLogDataAsync(string data)
        {
            _logData = await AppendLogAsync(data);
        }

        private async Task<string> AppendLogAsync(string data)
        {
            string message = DateTime.Now.ToString(Constants.Logs.LOG_DATA_FORMAT) + " " + data;
            Debug.WriteLine(message);
            return _stringBuilder.Append(message).Append(Environment.NewLine).ToString();
        }

        public async Task WriteInFileAsync()
        {
            if (_fileIoService.HasPermissions())
            {
                try
                {
                    _path = GetFilePath();

                    if (File.Exists(_path))
                    {
                        File.AppendAllText(_path, _logData);
                    }
                    else
                    {
                        File.WriteAllText(_path, _logData);
                    }

                    ClearData();
                }
                catch (Exception ex)
                {
                    File.AppendAllText(_path, ex.Message + Environment.NewLine);
                }
            }
        }

        public async Task ReadFileAsync()
        {
            if (_fileIoService.HasPermissions())
            {
                try
                {
                    _path = GetFilePath();
                    if (File.Exists(_path))
                    {
                        var tmp = File.ReadAllText(_path);
                        var clearedStr = StaticHelpers.CheckLogTimeStampOverdue(tmp, _overdueInDays, DateTime.Now);
                        File.Delete(_path);
                        File.WriteAllText(_path, clearedStr);
                    }
                }
                catch(Exception ex)
                {
                    File.AppendAllText(_path, ex.Message + Environment.NewLine);
                }
            }
        }

        #endregion

        #region -- Private helpers --

        private void ClearData()
        {
            _logData = string.Empty;
            _stringBuilder = new StringBuilder();
        }

        private string GetFilePath()
        {
            DateTime dateTime = DateTime.Now;
            string path = _fileIoService.GetFolderPath(ESaveDataWays.Device, _folderName);
            var filename = $"{_fileName}.txt";

            return Path.Combine(path, filename);
        }

        #endregion
    }
}
