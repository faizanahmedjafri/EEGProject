using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace SharedCore.Services.Bluetooth.Fake
{
    /// <summary>
    /// The fake implementation of the core BLE adapter for testing/development purposes.
    /// </summary>
    public sealed class FakeBleAdapter : AdapterBase
    {
        private readonly ConcurrentDictionary<Guid, IDevice> iConnectedDevices;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        public FakeBleAdapter()
        {
            iConnectedDevices = new ConcurrentDictionary<Guid, IDevice>();
        }

        /// <inheritdoc />
        protected override async Task StartScanningForDevicesNativeAsync(Guid[] aServiceUuids,
            bool aAllowDuplicatesKey, CancellationToken aScanCancellationToken)
        {
            DiscoveredDevices.Clear();

            await Task.Delay(400, aScanCancellationToken);

            aScanCancellationToken.ThrowIfCancellationRequested();

            var devices = CreateFakeDevicesWithServices();
            foreach (var device in devices)
            {
                HandleDiscoveredDevice(device);
            }
        }

        /// <inheritdoc />
        protected override void StopScanNative()
        {
            // do nothing
        }

        /// <inheritdoc />
        protected override async Task ConnectToDeviceNativeAsync(IDevice aDevice,
            ConnectParameters aConnectParameters,
            CancellationToken aCancellationToken)
        {
            await Task.Delay(10, aCancellationToken);

            aCancellationToken.ThrowIfCancellationRequested();

            iConnectedDevices.TryAdd(aDevice.Id, aDevice);

            HandleConnectedDevice(aDevice);
        }

        /// <inheritdoc />
        protected override void DisconnectDeviceNative(IDevice aDevice)
        {
            iConnectedDevices.TryRemove(aDevice.Id, out _);

            HandleDisconnectedDevice(true, aDevice);
        }

        /// <inheritdoc />
        public override async Task<IDevice> ConnectToKnownDeviceAsync(Guid aDeviceGuid,
            ConnectParameters aConnectParameters = new ConnectParameters(),
            CancellationToken aCancellationToken = new CancellationToken())
        {
            await Task.Delay(10, aCancellationToken);

            if (iConnectedDevices.TryGetValue(aDeviceGuid, out var device))
            {
                await ConnectToDeviceNativeAsync(device, aConnectParameters, aCancellationToken);
            }

            return device;
        }

        /// <inheritdoc />
        public override List<IDevice> GetSystemConnectedOrPairedDevices(Guid[] aServices = null)
        {
            return new List<IDevice>(0);
        }

        /// <inheritdoc />
        public override IList<IDevice> ConnectedDevices => new List<IDevice>(iConnectedDevices.Values);

        /// <summary>
        /// Generates 1 or more BIO devices.
        /// </summary>
        /// <returns></returns>
        private IReadOnlyList<IDevice> CreateFakeDevicesWithServices()
        {
            var discoveredDevicesCount = new Random().Next(1, 3);
            return Enumerable.Range(1, discoveredDevicesCount)
                .Select(CreateFakeDevice)
                .ToList();
        }

        /// <summary>
        /// Generates a device name with different characteristics included.
        /// </summary>
        /// <returns>created device name.</returns>
        private string CreateRandomDeviceName(int aId)
        {
            var names = new string[]
            {
                "SML BIO C# {0}",
                "SML BIO CSHARP {0}",
                "UNSUPPORTED DEV {0}",
                "#{0} BIO/v3-16-4-3",
                "#{0} BIO/v3-19-4-3",
                "#{0} BIO/v3-19-4-0",
                "#{0} BIO/v3-19-0-3",
                "#{0} BIO/v3-8-0-0",
                "#{0} BIO/v3-8-0",
                "#{0} BIO/v3-8",
                "#{0} BIO/v3",
                "#{0} BIO",
            };

            int nameIndex = new Random().Next(0, names.Length);

            return string.Format(names[nameIndex], aId);
        }

        /// <summary>
        /// Creates a device.
        /// </summary>
        /// <returns>created device.</returns>
        private FakeDevice CreateFakeDevice(int aId)
        {
            var device = new FakeDevice(this, CreateRandomDeviceName(aId));

            List<IService> services = new List<IService>();

            // battery service
            {
                var service = new FakeBleService(device, Guid.Parse("0000180f-0000-1000-8000-00805f9b34fb"));
                var characteristic = new FakeCharacteristic(service,
                    Guid.Parse("00002a19-0000-1000-8000-00805F9B34FB"),
                    CharacteristicPropertyType.Read |
                    CharacteristicPropertyType.Write |
                    CharacteristicPropertyType.Notify);

                var descriptor = new FakeDescriptor(Guid.Parse("00002902-0000-1000-8000-00805F9B34FB"), characteristic);
                descriptor.SetValue(new byte[] {0x00, 0x00});

                characteristic.SetValue(new byte[] {0x63});
                characteristic.Descriptors = new[] {descriptor};

                service.Characteristics = new[] {characteristic};
                services.Add(service);
            }

            // SML BIO service
            {
                var service = new FakeBleService(device, Guid.Parse("0000fff0-0000-1000-8000-00805f9b34fb"));

                var characteristic01 = new FakeCharacteristic(service,
                    Guid.Parse("0000fff1-0000-1000-8000-00805f9b34fb"),
                    CharacteristicPropertyType.Read |
                    CharacteristicPropertyType.Write |
                    CharacteristicPropertyType.Notify);
                var characteristic02 = new FakeCharacteristic(service,
                    Guid.Parse("0000fff2-0000-1000-8000-00805f9b34fb"),
                    CharacteristicPropertyType.Read |
                    CharacteristicPropertyType.Write |
                    CharacteristicPropertyType.Notify);
                var characteristic03 = new FakeCharacteristic(service,
                    Guid.Parse("0000fff3-0000-1000-8000-00805f9b34fb"),
                    CharacteristicPropertyType.Read |
                    CharacteristicPropertyType.Write |
                    CharacteristicPropertyType.Notify);
                var characteristic04 = new FakeLiveCharacteristic(service,
                    Guid.Parse("0000fff4-0000-1000-8000-00805f9b34fb"),
                    CharacteristicPropertyType.Read |
                    CharacteristicPropertyType.Write |
                    CharacteristicPropertyType.Notify);
                var characteristic05 = new FakeCharacteristic(service,
                    Guid.Parse("0000fff5-0000-1000-8000-00805f9b34fb"),
                    CharacteristicPropertyType.Read |
                    CharacteristicPropertyType.Write |
                    CharacteristicPropertyType.Notify);
                var characteristic06 = new FakeLiveCharacteristic(service,
                    Guid.Parse("0000fff6-0000-1000-8000-00805f9b34fb"),
                    CharacteristicPropertyType.Read |
                    CharacteristicPropertyType.Write |
                    CharacteristicPropertyType.Notify);

                characteristic01.SetValue(new byte[]{0x00,0x00,0x00,0x00,0x00, 19, 1, 0, 1, 6, 0x00,0x00,0x00,0x00});
                characteristic02.SetValue(new byte[]{0x01});
                characteristic03.SetValue(new byte[]{0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00});
                characteristic05.SetValue(new byte[]{0xFF,0xFF,0xFF,0xFF, 10, 10, 0x01,0xF4, 0x00,0x00, 10, 0x00,0x00});

                characteristic04.SetValue(new byte[0]);
                characteristic04.LiveValues = CreateBiopotChartData();
                characteristic04.UpdateInterval = TimeSpan.FromMilliseconds(100);

                characteristic06.SetValue(new byte[0]);
                characteristic06.LiveValues = new[]
                {
                    new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0x00, 9, 10, 0x00, 0x63, 0xFF, 14, 15 },
                    new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0x00, 9, 10, 0x00, 0x53, 0xAF, 14, 15 },
                    new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0x00, 9, 10, 0x00, 0x33, 0x8F, 14, 15 },
                    new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0x00, 9, 10, 0x00, 0x13, 0x6F, 14, 15 },
                    new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0x00, 9, 10, 0x00, 0x13, 0x2F, 14, 15 },
                    new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0x00, 9, 10, 0x00, 0x03, 0x0F, 14, 15 },
                };
                characteristic06.UpdateInterval = TimeSpan.FromSeconds(2);

                var descriptor01 = new FakeDescriptor(Guid.Parse("00002902-0000-1000-8000-00805F9B34FB"), characteristic01);
                descriptor01.SetValue(new byte[] { 0x00, 0x00 });
                var descriptor02 = new FakeDescriptor(Guid.Parse("00002902-0000-1000-8000-00805F9B34FB"), characteristic02);
                descriptor02.SetValue(new byte[] { 0x00, 0x00 });
                var descriptor03 = new FakeDescriptor(Guid.Parse("00002902-0000-1000-8000-00805F9B34FB"), characteristic03);
                descriptor03.SetValue(new byte[] { 0x00, 0x00 });
                var descriptor04 = new FakeDescriptor(Guid.Parse("00002902-0000-1000-8000-00805F9B34FB"), characteristic04);
                descriptor04.SetValue(new byte[] { 0x00, 0x00 });
                var descriptor05 = new FakeDescriptor(Guid.Parse("00002902-0000-1000-8000-00805F9B34FB"), characteristic05);
                descriptor05.SetValue(new byte[] { 0x00, 0x00 });
                var descriptor06 = new FakeDescriptor(Guid.Parse("00002902-0000-1000-8000-00805F9B34FB"), characteristic06);
                descriptor06.SetValue(new byte[] { 0x00, 0x00 });

                characteristic01.Descriptors = new[] {descriptor01};
                characteristic02.Descriptors = new[] {descriptor02};
                characteristic03.Descriptors = new[] {descriptor03};
                characteristic04.Descriptors = new[] {descriptor04};
                characteristic05.Descriptors = new[] {descriptor05};
                characteristic06.Descriptors = new[] {descriptor06};

                service.Characteristics = new[]
                {
                    characteristic01,
                    characteristic02,
                    characteristic03,
                    characteristic04,
                    characteristic05,
                    characteristic06,
                };
                services.Add(service);
            }

            device.Services = services;
            return device;
        }

        /// <summary>
        /// Creates fake biopot data.
        /// </summary>
        /// <returns>created samples to run in loop.</returns>
        private IReadOnlyCollection<byte[]> CreateBiopotChartData()
        {
            var data = new[]
            {
                new sbyte[]
                {
                    00, 00, 00, 01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 32, 78, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 124, 77, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -110, 75, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 106, 72, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 18, 68,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, -100, 62, 32, 78, 65, 77, -89, 74, 99, 70, -115, 64, 69, 57
                },
                new sbyte[]
                {
                    00, 00, 00, 01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, -74, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -128, 40, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -96, 31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 59, 22, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 120,
                    12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, -127, 2, -74, 48, 16, 39, -117, 28, 98, 17, -41, 5, 41, -6
                },
                new sbyte[]
                {
                    00, 00, 00, 01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, -98, -18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, -27, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -35, -37, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 78, -45, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 122,
                    -53, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, -125, -60, -98, -18, 117, -29, -16, -40, 74, -49, -69, -58, 115, -65
                },
                new sbyte[]
                {
                    00, 00, 00, 01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, -99, -71, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -37, -75, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 81, -77, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, -78, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, -78,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 81, -77, -99, -71, 89, -75, -65, -78, -32, -79, -65, -78, 89, -75
                },
                new sbyte[]
                {
                    00, 00, 00, 01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, -99, -71, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -122, -66, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -125, -60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 122, -53, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 78,
                    -45, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, -35, -37, -99, -71, 115, -65, -69, -58, 74, -49, -16, -40, 117, -29
                },
                new sbyte[]
                {
                    00, 00, 00, 01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, -98, -18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -128, -8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -127, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 120, 12, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 59,
                    22, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, -96, 31, -98, -18, 41, -6, -41, 5, 98, 17, -117, 28, 16, 39
                },
                new sbyte[]
                {
                    00, 00, 00, 01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, -74, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 31, 56, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -100, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 18, 68, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 106, 72,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, -110, 75, -74, 48, 69, 57, -115, 64, 99, 70, -89, 74, 65, 77
                },
            };

            return data.Select(x => x.Select(@byte => (byte) @byte)
                    .ToArray())
                .ToList();
        }
    }
}