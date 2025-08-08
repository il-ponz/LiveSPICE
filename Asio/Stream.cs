using System;
using NAudio.Wave;
using Util;

namespace Asio
{
    class Stream : Audio.Stream
    {
        private WaveInEvent waveIn;
        private WaveOutEvent waveOut;
        private BufferedWaveProvider waveOutProvider;

        private double sampleRate;
        public override double SampleRate { get { return sampleRate; } }

        private Audio.Stream.SampleHandler callback;
        private Audio.SampleBuffer[] inputBuffers;
        private Audio.SampleBuffer[] outputBuffers;

        private int bufferSize;
        private int bytesPerSample;

        public Stream(int DeviceIndex, bool IsInputDevice, Audio.Stream.SampleHandler Callback, Channel[] Input, Channel[] Output)
            : base(Input, Output)
        {
            Log.Global.WriteLine(MessageType.Info, "Instantiating NAudio stream with {0} input channels and {1} output channels.", Input.Length, Output.Length);

            callback = Callback;
            sampleRate = 44100; // Default sample rate
            bufferSize = 1024; // Default buffer size
            bytesPerSample = 4; // 32-bit float

            // Initialize input
            if (Input.Length > 0)
            {
                waveIn = new WaveInEvent();
                waveIn.DeviceNumber = DeviceIndex;
                waveIn.WaveFormat = new WaveFormat((int)sampleRate, 32, Input.Length); // 32-bit float
                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveIn.StartRecording();

                inputBuffers = new Audio.SampleBuffer[Input.Length];
                for (int i = 0; i < Input.Length; ++i)
                {
                    inputBuffers[i] = new Audio.SampleBuffer(bufferSize);
                }
            }
            else
            {
                inputBuffers = new Audio.SampleBuffer[0];
            }

            // Initialize output
            if (Output.Length > 0)
            {
                waveOut = new WaveOutEvent();
                waveOut.DeviceNumber = DeviceIndex;
                waveOutProvider = new BufferedWaveProvider(new WaveFormat((int)sampleRate, 32, Output.Length)); // 32-bit float
                waveOut.Init(waveOutProvider);
                waveOut.Play();

                outputBuffers = new Audio.SampleBuffer[Output.Length];
                for (int i = 0; i < Output.Length; ++i)
                {
                    outputBuffers[i] = new Audio.SampleBuffer(bufferSize);
                }
            }
            else
            {
                outputBuffers = new Audio.SampleBuffer[0];
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            int samplesRead = e.BytesRecorded / bytesPerSample / inputBuffers.Length;

            // Convert input bytes to double samples
            for (int channel = 0; channel < inputBuffers.Length; channel++)
            {
                for (int i = 0; i < samplesRead; i++)
                {
                    // Assuming interleaved stereo: L R L R...
                    int byteOffset = (i * inputBuffers.Length + channel) * bytesPerSample;
                    inputBuffers[channel].Samples[i] = BitConverter.ToSingle(e.Buffer, byteOffset);
                }
            }

            // Call the main audio callback
            callback(samplesRead, inputBuffers, outputBuffers, sampleRate);

            // Convert output double samples to bytes and write to output provider
            if (outputBuffers.Length > 0)
            {
                byte[] outputBytes = new byte[samplesRead * outputBuffers.Length * bytesPerSample];
                for (int channel = 0; channel < outputBuffers.Length; channel++)
                {
                    for (int i = 0; i < samplesRead; i++)
                    {
                        int byteOffset = (i * outputBuffers.Length + channel) * bytesPerSample;
                        byte[] sampleBytes = BitConverter.GetBytes((float)outputBuffers[channel].Samples[i]);
                        Buffer.BlockCopy(sampleBytes, 0, outputBytes, byteOffset, bytesPerSample);
                    }
                }
                waveOutProvider.AddSamples(outputBytes, 0, outputBytes.Length);
            }
        }

        public override void Stop()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
        }
    }
}
