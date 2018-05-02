using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;

namespace MyList.ViewModels {
    class ListItemViewModel {
        private static ListItemViewModel _instance;

        public static ListItemViewModel GetInstance() {
            return _instance ?? (_instance = new ListItemViewModel());
        }

        private ObservableCollection<Models.ListItem> allItems = new ObservableCollection<Models.ListItem>();
        public ObservableCollection<Models.ListItem> AllItems { get { return this.allItems; } }

        private Models.ListItem selectedItem = default(Models.ListItem);
        public Models.ListItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }

        private ListItemViewModel() { }

        public void AddItem(Int64 id, string title, string detail, DateTimeOffset date, Brush image, byte[] img, bool complete = false) {
            this.allItems.Add(new Models.ListItem(id, title, detail, date, img, image, complete));
        }

        public void RemoveItem(Int64 id) {
            if (selectedItem != null) {
                this.allItems.Remove(selectedItem);
            }
            this.selectedItem = null;
        }

        public void UpdateItem(Int64 id, string title, string detail, DateTimeOffset date, Brush image, byte[] img, bool isComplete) {
            if (this.selectedItem != null) {
                this.selectedItem.Title = title;
                this.selectedItem.detail = detail;
                this.selectedItem.date = date;
                this.selectedItem.Image = image;
                this.selectedItem.img = img;
                this.selectedItem.IsCompleted = isComplete;
            }
            this.selectedItem = null;
        }
    }
}
