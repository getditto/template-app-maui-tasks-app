using CommunityToolkit.Mvvm.ComponentModel;
using DittoSDK;

namespace DittoMauiTasksApp
{
    public partial class DittoTask : ObservableObject
    {
        public const string CollectionName = "tasks";

        [ObservableProperty]
        string id;

        [ObservableProperty]
        string body;

        [ObservableProperty]
        bool isCompleted;

        public DittoTask(DittoDocument document)
        {
            id = document["_id"].StringValue;
            body = document["body"].StringValue;
            isCompleted = document["isCompleted"].BooleanValue;
        }

        partial void OnIsCompletedChanged(bool value)
        {
            var ditto = Utils.ServiceProvider.GetService<Ditto>();
            var collection = ditto.Store[CollectionName];

            collection.FindById(Id).Update(mutableDoc =>
            {
                if (mutableDoc["isCompleted"].BooleanValue != IsCompleted)
                {
                    mutableDoc["isCompleted"].Set(IsCompleted);
                }
            });
        }
    }
}

