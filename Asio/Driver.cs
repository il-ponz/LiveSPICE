using System;
using System.Linq;
using NAudio.Wave;
using Util; // Keep Util for Log.Global.WriteLine

namespace Asio
{
    public class Driver : Audio.Driver
    {
        public override string Name
        {
            get { return "NAudio"; }
        }

        public Driver()
        {
            // Enumerate input devices
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var caps = WaveInEvent.GetCapabilities(i);
                Device d = null;
                try
                {
                    d = new Device(i, caps.ProductName, true);
                    Log.Global.WriteLine(MessageType.Info, "Found NAudio input device '{0}'.", caps.ProductName);
                }
                catch (Exception Ex)
                {
                    Log.Global.WriteLine(MessageType.Warning, "Error instantiating NAudio input device '{0}': {1}", caps.ProductName, Ex.Message);
                }
                if (d != null)
                    devices.Add(d);
            }

            // Enumerate output devices
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                Device d = null;
                try
                {
                    d = new Device(i, caps.ProductName, false);
                    Log.Global.WriteLine(MessageType.Info, "Found NAudio output device '{0}'.", caps.ProductName);
                }
                catch (Exception Ex)
                {
                    Log.Global.WriteLine(MessageType.Warning, "Error instantiating NAudio output device '{0}': {1}", caps.ProductName, Ex.Message);
                }
                if (d != null)
                    devices.Add(d);
            }
        }
    }
}
