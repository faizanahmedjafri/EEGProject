using System;

namespace SharedCore.Services
{
    public interface ISaveDataService
    {
        event Action<int> OnError;
        void StartRecord();
        void StopRecord();
        void SaveAudioDate(string data);
    }
}