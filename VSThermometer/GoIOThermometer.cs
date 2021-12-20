using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoIOdotNETCore;
using VSTCoreDefsdotNETCore;

namespace VSThermometer
{
    class GoIOThermometer
    {

        private readonly IntPtr _sensorHandle = IntPtr.Zero;

        public GoIOThermometer(int deviceIndex = 0)
        {

            IntPtr initResult = GoIO.Init();
            if (initResult.ToInt32() != 0)
            {
                throw new Exception("Unable to initialize library");
            }

            int numGoTempsFound = GoIO.UpdateListOfAvailableDevices(
                VST_USB_defs.VENDOR_ID,
                VST_USB_defs.PRODUCT_ID_GO_TEMP
            );

            StringBuilder deviceName = new(GoIO.MAX_SIZE_SENSOR_NAME);

            int status = GoIO.GetNthAvailableDeviceName(
                deviceName,
                deviceName.Capacity,
                VST_USB_defs.VENDOR_ID,
                VST_USB_defs.PRODUCT_ID_GO_TEMP,
                deviceIndex
            );

            if (status != 0)
            {
                throw new Exception("Unable to get the requested device name");
            }

            _sensorHandle = GoIO.Sensor_Open(
                deviceName.ToString(),
                VST_USB_defs.VENDOR_ID,
                VST_USB_defs.PRODUCT_ID_GO_TEMP,
                0
            );

            if (_sensorHandle == IntPtr.Zero)
            {
                throw new Exception("Unable to open sensor.");
            }
        }

        public void Dispose()
        {
            if (_sensorHandle != IntPtr.Zero)
            {
                GoIO.Sensor_Close(_sensorHandle);
                GoIO.Uninit();
            }
        }

        public void StartMeasurement()
        {
            GoIO.Sensor_SendCmdAndGetResponse4(
                _sensorHandle,
                GoIO_ParmBlk.CMD_ID_START_MEASUREMENTS,
                GoIO.TIMEOUT_MS_DEFAULT
            );
        }

        public void StopMeasurement()
        {
            GoIO.Sensor_SendCmdAndGetResponse4(
                _sensorHandle,
                GoIO_ParmBlk.CMD_ID_STOP_MEASUREMENTS,
                GoIO.TIMEOUT_MS_DEFAULT
            );
        }

        public double? GetMeasurement()
        {
            if (GoIO.Sensor_GetNumMeasurementsAvailable(_sensorHandle) < 1)
                return null;

            int rawMeasurement = GoIO.Sensor_GetLatestRawMeasurement(_sensorHandle);
            return ConvertRawData(rawMeasurement);
        }

        public double[] GetAllMeasurements()
        {
            int maxBuffer = 6000;
            int[] raw = new int[maxBuffer];
            int numMeasurements = GoIO.Sensor_ReadRawMeasurements(_sensorHandle, raw, (uint)raw.Length);

            double[] temperatures = raw.Take(numMeasurements).Select(t => ConvertRawData(t)).ToArray();

            return temperatures;
        }

        private double ConvertRawData(int rawData)
        {
            return GoIO.Sensor_CalibrateData(
                _sensorHandle,
                GoIO.Sensor_ConvertToVoltage(_sensorHandle, rawData)
            );
        }

        ~GoIOThermometer()
        {
            Dispose();
        }

    }
}


