
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DittoMauiTasksApp.Utils;
using DittoSDK;

namespace DittoMauiTasksApp.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly Ditto ditto;
        private readonly IPopupService popupService;
        private DittoLiveQuery liveQuery;
        private DittoSubscription subscription;

        [ObservableProperty]
        ObservableCollection<DittoTask> tasks;

        public MainPageViewModel(Ditto ditto, IPopupService popupService)
        {
            this.ditto = ditto;
            this.popupService = popupService;

            ObserveDittoTasksCollection();
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

            ditto.Store[DittoTask.CollectionName].Upsert(dict);
        }

        [RelayCommand]
        private void DeleteTask(DittoTask task)
        {
            ditto.Store[DittoTask.CollectionName].FindById(task.Id).Update((mutableDoc) =>
            {
                if (mutableDoc == null) return;
                mutableDoc["isDeleted"].Set(true);
            });
        }

        private void ObserveDittoTasksCollection()
        {
            subscription = ditto.Store[DittoTask.CollectionName].Find("!isDeleted").Subscribe();
            liveQuery = ditto.Store[DittoTask.CollectionName].Find("!isDeleted").ObserveLocal((docs, dittoEvent) =>
            {
                if (dittoEvent is DittoLiveQueryEvent.Update updateEvent)
                {
                    System.Diagnostics.Debug.WriteLine($"{docs.Count()} - {dittoEvent.GetType()} - {updateEvent.Deletions} - {updateEvent.Moves} - {updateEvent.Insertions}");
                }
                Tasks = new ObservableCollection<DittoTask>(docs.ConvertAll(d => new DittoTask(d)));
            });

            ditto.Store[DittoTask.CollectionName].Find("isDeleted == true").Evict();
        }
    }
}

