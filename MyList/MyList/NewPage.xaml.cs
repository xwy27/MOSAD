using System;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyList.Models;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MyList {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page {
        private string origin_photo = "ms-appx:///Assets/photo.jpg";
        private string imagePath = "";
        MyList.ViewModels.ListItemViewModel ViewModel;

        public NewPage() {
            this.InitializeComponent();
            /// Initialize the picture
            var brush = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(origin_photo))
            };
            Photo.Fill = brush;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            ViewModel = (MyList.ViewModels.ListItemViewModel)e.Parameter;
            if (ViewModel.SelectedItem == null) {
                /// Create condition
                Create.Content = "Create";
                Cancel.Content = "Cancel";
            } else {
                /// Update condition
                var selectedItem = ViewModel.SelectedItem;
                Photo.Fill = selectedItem.Image;
                Title.Text = selectedItem.Title;
                Description.Text = selectedItem.detail;
                Date.Date = selectedItem.date;
                Create.Content = "Update";
                Cancel.Content = "Delete";
            }
        }

        private async void Cancel_clear(object sender, RoutedEventArgs e) {
            Button temp = (Button)sender;
            if ((string)temp.Content == "Cancel") {
                var brush = new ImageBrush {
                    ImageSource = new BitmapImage(new Uri(origin_photo))
                };
                Photo.Fill = brush;
                Title.Text = "";
                Description.Text = "";
                Date.Date = DateTimeOffset.Now;
                Create.Content = "Create";
                ViewModel.SelectedItem = null;
            } else if ((string)temp.Content == "Delete") {
                ViewModel.RemoveItem(ViewModel.SelectedItem.id);

                var dialog = new MessageDialog("Event is deleted!");
                await dialog.ShowAsync();

                Frame.Navigate(typeof(MainPage), ViewModel);
            }
        }

        private async void CreateAsync(object sender, RoutedEventArgs e) {
            Button temp = (Button)sender;
            var errMsg = "";

            if ((string)temp.Content == "Create") {
                if (Title.Text == "") errMsg += "Title should not be empty!\n";
                if (Description.Text == "") errMsg += "Description should not be empty!\n";
                if (Date.Date <= DateTimeOffset.Now) errMsg += "Due Date should be in the future!\n";

                if (errMsg == "") {
                    errMsg += "Create Successfully!\n";
                    ViewModel.AddItem(Title.Text, Description.Text, Date.Date, Photo.Fill, imagePath);
                }

            } else if ((string)temp.Content == "Update") {
                errMsg += "Update Successfully!\n";
                ViewModel.UpdateItem(ViewModel.SelectedItem.id, Title.Text, Description.Text, Date.Date, Photo.Fill, imagePath);
            }

            var dialog = new MessageDialog(errMsg);
            await dialog.ShowAsync();

            Frame.Navigate(typeof(MainPage), ViewModel);
        }

        private async void Select_Photo(object sender, RoutedEventArgs e) {
            FileOpenPicker picker = new FileOpenPicker();
            /// Initialize the picture file type to take
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null) {
                /// Load the selected picture
                imagePath = file.Path;
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                var brush = new ImageBrush {
                    ImageSource = bi
                };
                Photo.Fill = brush;
            }
        }
    }
}
