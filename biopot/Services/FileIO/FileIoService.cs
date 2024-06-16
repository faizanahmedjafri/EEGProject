
//#define Binary

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using biopot.Helpers;
using SharedCore.Enums;
using SharedCore.Services;

namespace biopot.Services.FileIO
{
    public class FileIoService : IFileIoService
    {
        private readonly IFolderService _folderService;

        private string _filePath = null;

        public FileIoService(IFolderService folderService)
        {
            _folderService = folderService;
        }

        #region --IFileIoService--

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteToFile(string path, string data)
        {
            if (File.Exists(path))
            {
                File.AppendAllText(path, data + Environment.NewLine);
                
            }
            else
            {
                File.WriteAllText(path, data + Environment.NewLine);
            }
        }
        BinaryWriter binaryWriter;
        StreamWriter textWriter;
        private static long maxFileSize = Constants.Files.MAX_FILE_SIZE;
        public string WriteToFile(string path, byte[] data)
        {
            //string dataString = Encoding.UTF8.GetString(data);
            //check if we are saving to any opened file
            //if no open new file and save the writer oject 
#if Binary
            if (binaryWriter == null)
            {
                FileStream fileStream = new FileStream(path, FileMode.Append);
                binaryWriter = new BinaryWriter(fileStream);
                binaryWriter.Write(data);
            }
            //write to file 
            else
            {
                binaryWriter.Write(data);
                //fileWriter.Flush();
            }
            if (binaryWriter.BaseStream.Length >= maxFileSize) //50MB
            {
                path = ChangeFileIndex(path);
                binaryWriter.Flush();
                binaryWriter.Close();
                binaryWriter = null;
            }
#else
            if (textWriter == null)
            {
                FileStream fileStream = new FileStream(path, FileMode.Append);
                textWriter = new StreamWriter(fileStream);
                textWriter.Write(BitConverter.ToString(data) + Environment.NewLine);
            }
            //write to file 
            else
            {
                textWriter.Write(BitConverter.ToString(data) + Environment.NewLine);
                //fileWriter.Flush();
            }
            if (textWriter.BaseStream.Length >= maxFileSize) //50MB
            {
                path = ChangeFileIndex(path);
                textWriter.Flush();
                textWriter.Close();
                textWriter = null;
            }

#endif
            return path;
        }
        /**
         * 
         * 
         **/
        string ChangeFileIndex(string path)
        {
            if (path != null)
            {
                try
                {
                    //string ending = path.Substring(path.Length - 8);
                    //string indexString = ending.Substring(0, 4);
                    var index = GetIdFromFile(path, null);
                    index = (index + 1) % 10000;
                    var indexString = index.ToString().PadLeft(4, '0');
                    path = path.Substring(0, path.Length - 8);
                    string filename;
                    if (Constants.Files.FILE_TYPE == Constants.Files.FILE_TYPE_BINARY)
                        filename = $"{path}{indexString}.dat";
                    else
                        filename = $"{path}{indexString}.txt";
                    return filename;
                }
                catch(Exception e)
                {
                    e.ToString();
                }

            }
            return null;
        }

        public void CloseCurrentFile()
        {
#if Binary
            try
            {
                binaryWriter.Flush();
                binaryWriter.Close();
                binaryWriter = null;
            }catch(Exception e)
            {
                e.ToString();
            }
#else
            try
            {
                textWriter.Flush();
                textWriter.Close();
                textWriter = null;
            }
            catch (Exception e)
            {
                e.ToString();
            }
#endif
        }

        public IEnumerable<string> GetFiles(string path, string fileTemplate)
        {
            IEnumerable<string> files;
            if (Constants.Files.FILE_TYPE == Constants.Files.FILE_TYPE_BINARY)
                files = from file in Directory.EnumerateFiles(path, "*.dat")
                        where file.Contains(fileTemplate)
                        select file;
            else
                files = from file in Directory.EnumerateFiles(path, "*.txt")
                        where file.Contains(fileTemplate)
                        select file;
            return files;
        }

        public string GetFileUniqueId(string path, string fileName)
        {

            var listIds = GetIdsByPath(path, fileName);
            var id = listIds.Max();

            var tempId = ++id;

            return tempId.ToString().PadLeft(4, '0');
        }

        public IEnumerable<string> GetFilesGroupedByDay(string folderPath, DateTime dateTime)
        {
            DirectoryInfo info = new DirectoryInfo(folderPath);
            var fileList = info.GetFiles().
                               Where(p => 
                                     p.CreationTime.Year == dateTime.Year && 
                                     p.CreationTime.Month == dateTime.Month && 
                                     p.CreationTime.Day == dateTime.Day).
                               Select(x=>x.FullName);
            return fileList;
        }

#endregion

#region --IFolderService--

        public string GetFolderPath(ESaveDataWays way, string innerFolder)
        {
            return _folderService.GetFolderPath(way, innerFolder);
        }

        public double GetAvailableSpaceMb(ESaveDataWays way)
        {
            return _folderService.GetAvailableSpaceMb(way);
        }

        public bool HasPermissions()
        {
            return _folderService.HasPermissions();
        }

#endregion

#region --Private helpers--

        private IList<int> GetIdsByPath(string path, string fileName)
        {
            var listIds = new List<int>() { 0 };
            try
            {
                var fileTemplate = Path.Combine(path, fileName);

                var files = GetFiles(path, "_");

                foreach (var file in files)
                {
                    var curId = GetIdFromFile(file, fileTemplate);
                    listIds.Add(curId);
                }
            }
            catch (UnauthorizedAccessException UAEx)
            {
                System.Diagnostics.Debug.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                System.Diagnostics.Debug.WriteLine(PathEx.Message);
            }

            return listIds;
        }

        private int GetIdFromFile(string file, string fileName)
        {
            var source = string.Empty;

            var arr = file.Split('_');

            if (arr.Length > 0)
            {
                source = arr[arr.Length - 1];
                if (Constants.Files.FILE_TYPE == Constants.Files.FILE_TYPE_BINARY)
                    source = source.Replace(".dat", string.Empty);
                else 
                    source = source.Replace(".txt", string.Empty);
            }

            int.TryParse(source, out int id);

            return id;
        }

#endregion
    }
}