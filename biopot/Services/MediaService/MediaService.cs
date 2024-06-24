using biopot.ViewModels;
using Plugin.SimpleAudioPlayer;
using SharedCore.Services.MediaService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace biopot.Services.MediaService
{
    public class MediaService : IMediaService
    {
        private ISimpleAudioPlayer _player;
        public MediaService()
        {
            _player = CrossSimpleAudioPlayer.Current;
        }
        public void LoadFileStream(string filePath)
        {
            var assembly = typeof(AudioRecognitionViewModel).GetTypeInfo().Assembly;
            Stream audioStream = assembly.GetManifestResourceStream(filePath);
            _player.Load(audioStream);
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void Play()
        {
            _player.Play();
        }

        public void Stop()
        {
            _player.Stop();
        }
    }
}
