using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security;
using System.Threading.Tasks;
using biopot.Helpers;
using biopot.Models;
using biopot.Services.Charts;
using biopot.ViewModels;
using Newtonsoft.Json;
using SharedCore.Enums;
using SharedCore.Services;

namespace biopot.Services.SaveData
{
    public class SaveDataService : ISaveDataService
    {
        private readonly IFileIoService _fileIoService;
        private readonly IAppSettingsManagerService _appSettings;
        private readonly IChartManagerService _chartsManagerService;
        private PatientDetailsViewModel _patientVM;
        private SessionViewModel _sessionVM;
        private UserDetailsViewModel _doctorVM;
        private AudioRecognitionViewModel _audioRecognitionVM;
        private const string SAVE_TARGET_INTERNAL = "Internal Memory";
        private const string SAVE_TARGET_SD = "Internal SD Card";
        private const string SAVE_TARGET_USB = "USB OTG Cable";
        private const int MIN_MEMORY = 1;
        private string _path;

        public SaveDataService(IFileIoService fileIoService,
                              IAppSettingsManagerService appSettings,
                              IChartManagerService chartsService)
        {
            _fileIoService = fileIoService;
            _appSettings = appSettings;
            _chartsManagerService = chartsService;
        }

        #region --ISaveDataService--

        public event Action<int> OnError;

        public void SaveAudioDate()
        {
            try
            {
                string result = string.Join(", ", _audioRecognitionVM.PitchRecordDict.Select(kvp => $"{kvp.Key} : {kvp.Value}"));
                _fileIoService.WriteToFile(_path, result);
            }
            catch (IOException ioex)
            {
                if (ioex.Message != null && ioex.Message.Contains("Disk full"))
                {
                    SendError(DataSavingErrors.OutOfMemory);
                }
                else
                {
                    SendError(DataSavingErrors.IoException);
                }
            }
            catch (Exception e)
            {
                if (e is SecurityException | e is UnauthorizedAccessException)
                {
                    SendError(DataSavingErrors.MissingPermissions);
                }
                else
                {
                    SendError(DataSavingErrors.DevelopmentError);
                }
            }
        }

        public async void StartRecord()
        {
            if (_fileIoService.HasPermissions())
            {
                DateTime dateTime = DateTime.Now;

                await LoadSettingsAsync();

                var way = GetSaveTargetByString(_sessionVM.SavingTarget);
                try
                {
                    _path = GetFilePath(dateTime, way);

                    if (_fileIoService.GetAvailableSpaceMb(way) > MIN_MEMORY)
                    {
                        WriteHeaderToFile(dateTime);
                        Subscribe();
                    }
                    else
                    {
                        SendError(DataSavingErrors.OutOfMemory);
                    }
                }
                catch (Exception e)
                {
                    if (e is IOException)
                    {
                        SendError(DataSavingErrors.IoException);
                    }
                    else if (e is SecurityException | e is UnauthorizedAccessException)
                    {
                        SendError(DataSavingErrors.MissingPermissions);
                    }
                    else
                    {
                        SendError(DataSavingErrors.DevelopmentError);
                    }
                }
            }
            else
            {
                SendError(DataSavingErrors.MissingPermissions);
            }
        }

        public void StopRecord()
        {
            UnSubscribe();
        }

        public void CloseFile()
        {
            _fileIoService.CloseCurrentFile();
        }

        #endregion

        #region --Private helpers--

        private async Task LoadSettingsAsync()
        {
            _patientVM = await _appSettings.GetObjectAsync<PatientDetailsViewModel>(Constants.StorageKeys.PATIENT_DETAIL);
            _sessionVM = await _appSettings.GetObjectAsync<SessionViewModel>(Constants.StorageKeys.SESSION_DETAIL);
            _doctorVM = await _appSettings.GetObjectAsync<UserDetailsViewModel>(Constants.StorageKeys.USER_DETAIL);
            _audioRecognitionVM = await _appSettings.GetObjectAsync<AudioRecognitionViewModel>(Constants.StorageKeys.AUDIO_RECOGNITION_DETAIL);
        }

        private void WriteHeaderToFile(DateTime dateTime)
        {
            try
            {
                //Patient Details:
                _fileIoService.WriteToFile(_path, JsonConvert.SerializeObject(_patientVM.PatientsInformation));

                //Session Details:
                _fileIoService.WriteToFile(_path, _sessionVM.SavingTarget);
                _fileIoService.WriteToFile(_path, _sessionVM.FolderName);
                _fileIoService.WriteToFile(_path, _sessionVM.FileName);
                _fileIoService.WriteToFile(_path, _sessionVM.IsDateInName ? dateTime.ToString("yyyyMMdd") : string.Empty);
                _fileIoService.WriteToFile(_path, _sessionVM.IsTimeInName ? dateTime.ToString("HH.mm.ss") : string.Empty);

                //Doctor Details
                _fileIoService.WriteToFile(_path, _doctorVM.Name);
                _fileIoService.WriteToFile(_path, _doctorVM.EmpId);
                _fileIoService.WriteToFile(_path, _doctorVM.Role);
                _fileIoService.WriteToFile(_path, _doctorVM.Email);
            }
            catch (IOException ioex)
            {
                if (ioex.Message != null && ioex.Message.Contains("Disk full"))
                {
                    SendError(DataSavingErrors.OutOfMemory);
                }
                else
                {
                    SendError(DataSavingErrors.IoException);
                }
            }
            catch (Exception e)
            {
                if (e is SecurityException | e is UnauthorizedAccessException)
                {
                    SendError(DataSavingErrors.MissingPermissions);
                }
                else
                {
                    SendError(DataSavingErrors.DevelopmentError);
                }
            }
        }

        private void OnReceivedData(object sender, EventArgs aEventArgs)
        {
            var way = GetSaveTargetByString(_sessionVM.SavingTarget);

            if (_fileIoService.GetAvailableSpaceMb(way) > MIN_MEMORY)
            {
                var eventArgsData = (aEventArgs as BiopodDataEventArgs);

                if (eventArgsData != null)
                {
                    try
                    {
                        foreach (var oneDataArray in eventArgsData.FullData)
                        {
							//J.M.
                            //TODO need implement real time of receiving byte[]
                            //   _fileIoService.WriteToFile(_path, Environment.NewLine + "Time [" + DateTime.Now.ToString("dd mm yyyy hh:mm:ss.FFF") + "]      Value :" );

                            //  _fileIoService.WriteToFile(_path, BitConverter.ToString(oneDataArray));
                            _path = _fileIoService.WriteToFile(_path,oneDataArray);
                        }
                    }
                    catch (IOException ioex)
                    {
                        if (ioex.Message != null && ioex.Message.Contains("Disk full"))
                        {
                            SendError(DataSavingErrors.OutOfMemory);
                        }
                        else
                        {
                            SendError(DataSavingErrors.IoException);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is SecurityException | e is UnauthorizedAccessException)
                        {
                            SendError(DataSavingErrors.MissingPermissions);
                        }
                        else
                        {
                            SendError(DataSavingErrors.DevelopmentError);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                SendError(DataSavingErrors.OutOfMemory);
            }
        }

        private string GetFilePath(DateTime dateTime, ESaveDataWays way)
        {
            var fileTemplate = SaveDataHelper.PrepareFileName(_sessionVM, dateTime);
            string path = _fileIoService.GetFolderPath(way, _sessionVM.FolderName);
            var count = _fileIoService.GetFileUniqueId(path, fileTemplate);
            string filename;
            if (Constants.Files.FILE_TYPE == Constants.Files.FILE_TYPE_BINARY)
                filename = $"{fileTemplate}{count}.dat";
            else
                filename = $"{fileTemplate}{count}.txt";

            //var filename = $"{fileTemplate}{count}.txt";

            return Path.Combine(path, filename);
        }

        private ESaveDataWays GetSaveTargetByString(string targetString)
        {
            ESaveDataWays target;
            switch (targetString)
            {
                case SAVE_TARGET_INTERNAL:
                    target = ESaveDataWays.Device;
                    break;
                case SAVE_TARGET_SD:
                    target = ESaveDataWays.SD;
                    break;
                case SAVE_TARGET_USB:
                    target = ESaveDataWays.Usb;
                    break;
                default:
                    throw new ArgumentException();
            }
            return target;
        }

        private void Subscribe()
        {
            _chartsManagerService.DataLoaded += OnReceivedData;
        }

        private void UnSubscribe()
        {
            _chartsManagerService.DataLoaded -= OnReceivedData;
        }

        private void SendError(DataSavingErrors error)
        {
            OnError?.Invoke((int)error);
            StopRecord();
            CloseFile();
        }

        #endregion

    }
}