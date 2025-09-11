using UnityEngine;
using UnityEngine.UI;

namespace MiniGames
{
    public class Micro : AbstractMiniGame
    {

        public string selectedDevice;
        public int sampleWindow = 128;
        public float volume;

        public Slider Slider;

        private AudioClip micClip;
        protected override void MiniGameStart()
        {
            if (Microphone.devices.Length > 0)
            {
                selectedDevice = Microphone.devices[0];
                micClip = Microphone.Start(selectedDevice, true, 1, AudioSettings.outputSampleRate);
                Debug.Log("micro : " + selectedDevice);
            }
            else
            {
                Debug.Log("pas de micro détecté");
            }
        }

        float getVolume()
        {
            if (!micClip || !Microphone.IsRecording(selectedDevice)) return 0f;

            float[] samples = new float[sampleWindow];
            int micPosition = Microphone.GetPosition(selectedDevice) - sampleWindow;

            if (micPosition < 0) return 0f;

            micClip.GetData(samples, micPosition);

            float sum = 0f;
            for (int i = 0; i < sampleWindow; i++)
            {
                sum += samples[i] * samples[i];
            }

            return Mathf.Sqrt(sum / sampleWindow);
        }

        protected override void MiniGameUpdate()
        {
            volume = getVolume();
            Debug.Log("Volume : " + volume);

            Slider.value = volume;
        }

        public override void Win()
        {
        }
    }
}