using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System;
using System.Linq;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using System.Diagnostics;
using System.Threading;


// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace App1
{

    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BLEShows i;
        public MainPage()
        {
            this.InitializeComponent();
            StartTimer();
            i = new BLEShows();
            i.Communicate();
        }
        public void StartTimer()
        {
            TimerCallback callback = state =>
            {
                Debug.WriteLine("Ticks = " + DateTime.Now.Ticks);
                i.setNotyfyAll();
            };

            var timer = new Timer(callback, null, 100/* msec */, 10000 /* msec*/);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var task = i.pairlingFirstAsync();
            //i.SetNotyfyAix3();
            var task = i.getDeviceAsync();
        }
    }

    class BLEShows
    {
        private static Guid serviceUUID = new Guid("40079C01-007A-6565-6D6F-74652E636F6D");
        private static Guid serviceUUID_aix = new Guid("EBAD3530-226C-4EBB-A153-A3BC9567057D");
        private static Guid characteristicUUID = new Guid("0000DE0B-0000-1000-8000-00805F9B34FB");
        private static Guid characteristicUUID_aix = new Guid("EBAD3531-226C-4EBB-A153-A3BC9567057D");

        List<String> devicelist = new List<string>();
        //マップの定義
        IDictionary<string, DeviceInformation> DeviceMap = new Dictionary<string, DeviceInformation>();

        void callback(GattCharacteristic sender, GattValueChangedEventArgs eventArgs)
        {
            Debug.WriteLine("callback!!");
            Debug.WriteLine(System.Text.Encoding.ASCII.GetString(eventArgs.CharacteristicValue.ToArray()));
        }

        public void printDeviceId()
        {
            foreach (string num in devicelist) // 先頭から最後まで順番に表示
            {
                Debug.WriteLine("printDeviceId:"+num);
            }
        }
        public void setNotyfyAll()
        {
            foreach (string num in devicelist) // 先頭から最後まで順番に表示
            {
                Debug.WriteLine("setNotyfyAll:" + num);
                //setNotyfyFromId(num);
                
            }
            //setNotyfy2();
        }
        private async void setNotyfy(DeviceInformation info)
        {
            try
            {
                var service = await GattDeviceService.FromIdAsync(info.Id);
                Debug.WriteLine(string.Format("Id={0}", info.Id));
                if (service == null)
                {
                    Debug.WriteLine("No service:"+ string.Format("Id={0}", info.Id));
                    return;
                }
                IReadOnlyList<GattCharacteristic> rx_characteristics = service.GetCharacteristics(characteristicUUID);

                var rx_characteristic = rx_characteristics[0];

                rx_characteristic.ValueChanged += callback;
                GattCommunicationStatus status = await rx_characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                if (status == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("notify set success");
                }
                else
                {
                    Debug.WriteLine("notify set fail");
                }
                var temp = rx_characteristic.ReadValueAsync();
                Debug.WriteLine("read:" + temp.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Communicate()
        {

            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher =
                        DeviceInformation.CreateWatcher(
                                BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // EnumerationCompleted and Stopped are optional to implement.
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start the watcher.
            deviceWatcher.Start();
        }


        private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            return;
        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            return;
        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate info)
        {

            //Debug.WriteLine(string.Format("remove Id={0}", info.Id));
            devicelist.Remove(info.Id);

            DeviceMap.Remove(info.Id);
            return;
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate info)
        {

            Debug.WriteLine(string.Format("Update Id={0}", info.Id));
            return;
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation info)
        {

            //Debug.WriteLine(string.Format("Add Name={0} IsEnabled={1} Id={2}", info.Name, info.IsEnabled, info.Id));
            if (info.Name == "Zeemote JM1-L2D1000Z-2")
            {
                devicelist.Add(info.Id);
                DeviceMap.Add(info.Id, info);
                //setNotyfy2();
                //setNotyfy3(info);
            }
            if (info.Name == "AP-3x")
            {
                DeviceMap.Add(info.Id, info);
            }
            return;
        }

        public async Task pairlingFirstAsync()
        {
            if(DeviceMap.Count == 0)
            {
                return;
            }
            var deviceMap = DeviceMap.First();
            var deviceInfo = deviceMap.Value;
            DevicePairingResult result = await deviceInfo.Pairing.PairAsync();
            if (result.Status == DevicePairingResultStatus.Paired || result.Status == DevicePairingResultStatus.AlreadyPaired)
            {

            }
            else
            {
                // fail
            }
        }
        private async void setNotyfyFromId(string deviceId, Guid serviceUUID,Guid characteristicUUID)
        {
            /*
            
            */
            var device = await BluetoothLEDevice.FromIdAsync(deviceId);
            if (device == null)
            {
                Debug.WriteLine("No device");
                return;
            }

            var services = await device.GetGattServicesForUuidAsync(serviceUUID);

            var characteristics = await services.Services[0].GetCharacteristicsForUuidAsync(characteristicUUID);

            characteristics.Characteristics[0].ValueChanged += CharacteristicChanged;

            await characteristics.Characteristics[0].WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.Notify
            );

        }

        private async void setNotyfyFromId(string deviceId)
        {
            setNotyfyFromId(deviceId, serviceUUID, characteristicUUID);
        }

        private async void setNotyfy3(DeviceInformation deviceInfo)
        {
            setNotyfyFromId(deviceInfo.Id);
        }

        private void CharacteristicChanged(
                    GattCharacteristic sender,
                    GattValueChangedEventArgs eventArgs
                )
        {
            byte[] data = new byte[eventArgs.CharacteristicValue.Length];
            Windows.Storage.Streams.DataReader.FromBuffer(eventArgs.CharacteristicValue).ReadBytes(data);
            var str = System.Text.Encoding.ASCII.GetString(data);
        }

        public async void setNotyfy2()
        {
            var selector = GattDeviceService.GetDeviceSelectorFromUuid(serviceUUID);
            
            var devices = await DeviceInformation.FindAllAsync(selector);
            var deviceInformation = devices.FirstOrDefault();
            if (deviceInformation == null)
            {
                Debug.WriteLine("No device fund");

            }
            foreach (DeviceInformation info in devices)
            {
                //setNotyfy(info);
                setNotyfyFromId(info.Id);
            }
        }
        public async void SetNotyfy2(Guid serviceUUID,Guid characteristicUUID)
        {
            var selector = GattDeviceService.GetDeviceSelectorFromUuid(serviceUUID);

            var devices = await DeviceInformation.FindAllAsync(selector);
            var deviceInformation = devices.FirstOrDefault();
            if (deviceInformation == null)
            {
                Debug.WriteLine("No device fund");

            }
            foreach (DeviceInformation info in devices)
            {
                //setNotyfy(info);
                setNotyfyFromId(info.Id, serviceUUID,characteristicUUID);
            }
        }
         public void SetNotyfyAix3()
        {
            SetNotyfy2(serviceUUID_aix, characteristicUUID_aix);
            
        }

        public async Task getDeviceAsync()
        {

            BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(0xC93D72EC9A58);
            if (bluetoothLeDevice != null)
            {
                Debug.WriteLine("device fund");
            }
        }
    }

}
