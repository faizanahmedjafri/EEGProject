using System;
using System.Collections.Generic;

namespace SharedCore.Services
{
    public interface IFileIoService : IFolderService
    {
        void WriteToFile(string path, string data);//J.M

        string WriteToFile(string path, byte[] data);

        void DeleteFile(string path);

        void CloseCurrentFile();//J.M

        string ReadFile(string path);

        string GetFileUniqueId(string path, string fileName);

        IEnumerable<string> GetFilesGroupedByDay(string folderPath, DateTime dateTime);

        IEnumerable<string> GetFiles(string path, string fileTemplate);
    }
}