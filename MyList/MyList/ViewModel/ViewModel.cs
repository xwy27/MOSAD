using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MyList.ViewModels {
    class ListItemViewModel {
        private ObservableCollection<Models.ListItem> allItems = new ObservableCollection<Models.ListItem>();
        public ObservableCollection<Models.ListItem> AllItems { get { return this.allItems; } }

        private Models.ListItem selectedItem = default(Models.ListItem);
        public Models.ListItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }

        public ListItemViewModel() {
            string origin_photo = "ms-appx:///Assets/photo.jpg";
            var brush = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(origin_photo))
            };
            this.allItems.Add(new Models.ListItem("MOSAD", "Finish Homework", DateTime.Parse("2018-3-29"), brush));
            this.allItems.Add(new Models.ListItem("OS", "Review PPT", DateTime.Parse("2018-3-30"), brush));
        }

        public void AddItem(string title, string detail, DateTimeOffset date, Brush image, string imagePath) {
            this.allItems.Add(new Models.ListItem(title, detail, date, image, imagePath));
        }

        public void RemoveItem(string id) {
            if (selectedItem != null) {
                this.allItems.Remove(selectedItem);
            }
            this.selectedItem = null;
        }

        public void UpdateItem(string id, string title, string detail, DateTimeOffset date, Brush image, string imagePath) {
            if (this.selectedItem != null) {
                this.selectedItem.Title = title;
                this.selectedItem.detail = detail;
                this.selectedItem.date = date;
                this.selectedItem.Image = image;
                if (imagePath != selectedItem.imagePath) {
                    selectedItem.imagePath = imagePath;
                }
            }
            this.selectedItem = null;
        }
    }
}
