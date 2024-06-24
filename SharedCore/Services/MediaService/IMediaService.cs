using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCore.Services.MediaService
{
    public interface IMediaService
    {
        void Play();
        void Stop();
        void Pause();
        void LoadFileStream(string filePath);
    }
}
