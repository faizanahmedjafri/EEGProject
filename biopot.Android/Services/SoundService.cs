using System.Threading.Tasks;
using Android.Media;
using biopot.Services;

namespace biopot.Droid.Services
{
    /// <summary>
    /// The android specific implementation of <see cref="ISoundService"/>
    /// </summary>
    public class SoundService : ISoundService
    {
        /// <inheritdoc/>
        public async void Play3Beeps()
        {
            var toneGenerator = new ToneGenerator(Stream.Alarm, 100);
            toneGenerator.StartTone(Tone.CdmaAlertCallGuard);

            // 3 beeps that long 250ms each
            await Task.Delay(750);

            toneGenerator.Release();
        }
    }
}