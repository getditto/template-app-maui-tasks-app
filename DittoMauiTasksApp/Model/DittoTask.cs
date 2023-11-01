using CommunityToolkit.Mvvm.ComponentModel;

namespace DittoMauiTasksApp
{
    public partial class DittoTask : ObservableObject
    {
        [ObservableProperty]
        string id;

        [ObservableProperty]
        string body;

        [ObservableProperty]
        bool isCompleted;
    }
}