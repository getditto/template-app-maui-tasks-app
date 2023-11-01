using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DittoMauiTasksApp.Utils;

namespace DittoMauiTasksApp.ViewModels
{
    public partial class TasksPageviewModel : ObservableObject
    {
        private readonly IPopupService popupService;

        [ObservableProperty]
        ObservableCollection<DittoTask> tasks = new();

        public TasksPageviewModel(IPopupService popupService)
        {
            this.popupService = popupService;
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

            Tasks.Add(new DittoTask() {
                IsCompleted = false,
                Body = taskData
            });
        }

        [RelayCommand]
        private void DeleteTask(DittoTask task)
        {
            Tasks.Remove(task);
        }
    }
}

