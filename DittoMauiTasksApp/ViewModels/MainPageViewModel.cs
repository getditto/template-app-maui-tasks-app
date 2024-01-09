
using System.Collections.ObjectModel;
using System.Text.Json;
#if ANDROID
using Android.Content;
using Android.Locations;
using Android.Widget;
#endif
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
#if IOS
using CoreLocation;
#endif
using DittoMauiTasksApp.Utils;
using DittoSDK;

namespace DittoMauiTasksApp.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly Ditto ditto;
        private readonly IPopupService popupService;

        [ObservableProperty]
        ObservableCollection<DittoTask> tasks;

        DittoPresenceObserver obs;

        public MainPageViewModel(Ditto ditto, IPopupService popupService)
        {
            this.ditto = ditto;
            this.popupService = popupService;

            CheckPermissions().ContinueWith(async t =>
            {
                var result = await t;

                ditto.DisableSyncWithV3();
                ditto.TransportConfig.PeerToPeer.Awdl.Enabled = false;
                ditto.StartSync();

                ObserveDittoTasksCollection();
            });

            obs = this.ditto.Presence.Observe(s =>
            {
                System.Diagnostics.Debug.WriteLine(" - - - ");
                foreach (var peer in s.LocalPeer.Connections)
                {
                    System.Diagnostics.Debug.WriteLine($"{peer.ConnectionType} - {BitConverter.ToString(peer.Peer1)} - {BitConverter.ToString(peer.Peer2)}");
                }
            });
        }

        private async Task<bool> CheckPermissions()
        {
            PermissionStatus bluetoothStatus = await CheckBluetoothPermissions();

            return  IsGranted(bluetoothStatus);
        }

        private async Task<PermissionStatus> CheckBluetoothPermissions()
        {
            PermissionStatus bluetoothStatus = PermissionStatus.Granted;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                if (DeviceInfo.Version.Major >= 12)
                {
                    bluetoothStatus = await CheckPermissions<NearbyDevicesPermission>();
                }
                else
                {
                    bluetoothStatus = await CheckPermissions<Permissions.LocationWhenInUse>();
                }
            }

            return bluetoothStatus;
        }

        private async Task<PermissionStatus> CheckPermissions<TPermission>() where TPermission : Permissions.BasePermission, new()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<TPermission>();
            }

            return status;
        }

        private static bool IsGranted(PermissionStatus status)
        {
            return status == PermissionStatus.Granted || status == PermissionStatus.Limited;
        }


        [RelayCommand]
        private async Task AddTaskAsync()
        {
            string taskData = await popupService.DisplayPromptAsync("Add Task", "Add a new task:");

            if (taskData == null)
            {
                //nothing was entered. 
                return; 
            }

            var dict = new Dictionary<string, object>
            {
                {"body", taskData},
                {"isCompleted", false},
                { "isDeleted", false }
            };

            await ditto.Store.ExecuteAsync($"INSERT INTO {DittoTask.CollectionName} DOCUMENTS (:doc1)", new Dictionary<string, object>()
            {
                { "doc1", dict }
            });
        }

        [RelayCommand]
        private void DeleteTask(DittoTask task)
        {
            var updateQuery = $"UPDATE {DittoTask.CollectionName} " +
                "SET isDeleted = true " +
                $"WHERE _id = '{task.Id}'";
            ditto.Store.ExecuteAsync(updateQuery);
        }

        private void ObserveDittoTasksCollection()
        {
            var query = $"SELECT * FROM {DittoTask.CollectionName} WHERE isDeleted = false";

            ditto.Sync.RegisterSubscription(query);
            ditto.Store.RegisterObserver(query, storeObservationHandler: async (queryResult) =>
            {
                Tasks = new ObservableCollection<DittoTask>(queryResult.Items.ConvertAll(d =>
                {
                    return JsonSerializer.Deserialize<DittoTask>(d.JsonString());
                }));
            });

            ditto.Store.ExecuteAsync($"EVICT FROM {DittoTask.CollectionName} WHERE isDeleted = false");
        }
    }
}

