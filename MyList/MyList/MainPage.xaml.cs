using System;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using MyList.Models;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyList {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page {
        private string origin_photo = "ms-appx:///Assets/photo.jpg";
        private string imagePath = "";
        MyList.ViewModels.ListItemViewModel ViewModel { get; set; }

        public MainPage() {
            this.InitializeComponent();
            /// Initialize the picture
            var brush = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(origin_photo))
            };
            Photo.Fill = brush;
            this.ViewModel = new MyList.ViewModels.ListItemViewModel();
        }

        private void ToNewPage(object sender, RoutedEventArgs e) {
            /// Enable the nevigation button
            if (Detail_Part.Visibility == Visibility.Collapsed)
                Frame.Navigate(typeof(NewPage), ViewModel);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (e.Parameter.GetType() == typeof(MyList.ViewModels.ListItemViewModel)) {
                this.ViewModel = (MyList.ViewModels.ListItemViewModel)e.Parameter;
            }
            Frame rootFrame = Window.Current.Content as Frame;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void MyListItem_Clicked(object sender, ItemClickEventArgs e) {
            ViewModel.SelectedItem = (ListItem)e.ClickedItem;
            if (Window.Current.Bounds.Width < 800) {
                Frame.Navigate(typeof(NewPage), ViewModel);
            } else {
                Photo.Fill = ViewModel.SelectedItem.Image;
                Title.Text = ViewModel.SelectedItem.Title;
                Description.Text = ViewModel.SelectedItem.detail;
                Date.Date = ViewModel.SelectedItem.date;
                Create.Content = "Update";
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
                //Check if selectedItem is deleted
                if (ViewModel.SelectedItem != null) {
                    errMsg += "Update Successfully!\n";
                    ViewModel.UpdateItem(ViewModel.SelectedItem.id, Title.Text, Description.Text, Date.Date, Photo.Fill, imagePath);
                } else {
                    errMsg += "Event has been deleted!\n";
                    Title.Text = "";
                    Description.Text = "";
                    Date.Date = DateTimeOffset.Now;
                    Create.Content = "Create";
                    ViewModel.SelectedItem = null;
                }
            }

            var dialog = new MessageDialog(errMsg);
            await dialog.ShowAsync();
        }

        private void Cancel_Clear(object sender, RoutedEventArgs e) {
            var brush = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(origin_photo))
            };
            Photo.Fill = brush;
            Title.Text = "";
            Description.Text = "";
            Date.Date = DateTimeOffset.Now;
            Create.Content = "Create";
            ViewModel.SelectedItem = null;
        }

        private void ToEdit(object sender, RoutedEventArgs e) {
            dynamic temp = e.OriginalSource;
            ListItem selectedItem = (ListItem)temp.DataContext;
            if (Window.Current.Bounds.Width < 800) {
                Frame.Navigate(typeof(NewPage), ViewModel);
            } else {
                /// Right part to change
                Photo.Fill = selectedItem.Image;
                Title.Text = selectedItem.Title;
                Description.Text = selectedItem.detail;
                Date.Date = selectedItem.date;
                Create.Content = "Update";
            }
        }

        private void ToDelete(object sender, RoutedEventArgs e) {
            dynamic d = e.OriginalSource;
            ViewModel.SelectedItem = (ListItem)d.DataContext;
            if (ViewModel.SelectedItem != null) {
                ViewModel.RemoveItem(ViewModel.SelectedItem.id);
            }
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

    /// <summary>
    /// A conveter to help bind the line and checkbox
    /// </summary>
    class CompletedConveter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            bool result = (bool)value;
            if (result) {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
