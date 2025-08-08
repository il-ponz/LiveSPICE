using System;
using System.Linq;
using NAudio.Wave;

namespace Asio
{
    class Channel : Audio.Channel
    {
        private int index;
        private string name;
        public int Index { get { return index; } }
        public override string Name { get { return name; } }

        public Channel(int Index, string Name)
        {
            index = Index;
            name = Name;
        }

        public override string ToString()
        {
            return name;
        }
    }

    class Device : Audio.Device
    {
        private int deviceIndex;
        private bool isInputDevice;

        public Device(int DeviceIndex, string DeviceName, bool IsInputDevice)
        {
            deviceIndex = DeviceIndex;
            name = DeviceName;
            isInputDevice = IsInputDevice;

            // For simplicity, assume one input/output channel per device for now.
            // NAudio's WaveInEvent/WaveOutEvent typically represent a single device.
            if (isInputDevice)
            {
                inputs = new Audio.Channel[] { new Channel(0, "Input 1") };
                outputs = new Audio.Channel[0];
            }
            else
            {
                inputs = new Audio.Channel[0];
                outputs = new Audio.Channel[] { new Channel(0, "Output 1") };
            }
        }

        public override Audio.Stream Open(Audio.Stream.SampleHandler Callback, Audio.Channel[] Input, Audio.Channel[] Output)
        {
            return new Stream(
                deviceIndex,
                isInputDevice,
                Callback,
                Input.Cast<Channel>().ToArray(),
                Output.Cast<Channel>().ToArray());
        }
    }
}
