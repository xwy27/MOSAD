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
using MyList.ViewModels;
using Windows.Storage.AccessCache;
using DataAccessLibrary;
using System.Threading.Tasks;
using System.IO;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MyList {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page {
        private string origin_photo = "ms-appx:///Assets/photo.jpg";
        private byte[] imgData;
        private ListItemViewModel ViewModel = ListItemViewModel.GetInstance();

        public NewPage() {
            this.InitializeComponent();
            /// Initialize the picture
            Photo.Fill = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(origin_photo))
            };
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            if (e.NavigationMode == NavigationMode.New) {
                ApplicationData.Current.LocalSettings.Values.Remove("Newpage");

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
                    imgData = selectedItem.img;
                    Create.Content = "Update";
                    Cancel.Content = "Delete";
                }
            } else {
                // Try to restore state if any, in case we were terminated
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Newpage")) {
                    Create.Content = "Update";
                    Cancel.Content = "Delete";

                    var composite = ApplicationData.Current.LocalSettings.Values["Newpage"] as ApplicationDataCompositeValue;
                    Title.Text = (string)composite["title"];
                    Description.Text = (string)composite["description"];
                    Date.Date = (DateTimeOffset)composite["date"];
                    StorageFile theFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(
                        (string)ApplicationData.Current.LocalSettings.Values["MyToken"]);
                    if (theFile != null) {
                        IRandomAccessStream ir = await theFile.OpenAsync(FileAccessMode.Read);
                        BitmapImage bi = new BitmapImage();
                        await bi.SetSourceAsync(ir);
                        var brush = new ImageBrush {
                            ImageSource = bi
                        };
                        Photo.Fill = brush;
                    }
                    // We're done with it, so remove it
                    ApplicationData.Current.LocalSettings.Values.Remove("Newpage");
                } else {
                    Create.Content = "Create";
                    Cancel.Content = "Cancel";
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            bool suspending = ((App)App.Current).IsSuspending;
            if (suspending) {
                // Save volatile state in case we get terminated later on, then
                // we can restore as if we'd never been gone :)
                var composite = new ApplicationDataCompositeValue {
                    ["title"] = Title.Text,
                    ["description"] = Description.Text,
                    ["date"] = Date.Date,
                    //["photo"] = (PhotoShadow.Source as BitmapImage).UriSource.OriginalString
                };
                ApplicationData.Current.LocalSettings.Values["Newpage"] = composite;
            }
        }

        private async void Cancel_clear(object sender, RoutedEventArgs e) {
            Button temp = (Button)sender;
            if ((string)temp.Content == "Cancel") {
                Photo.Fill = new ImageBrush {
                    ImageSource = new BitmapImage(new Uri(origin_photo))
                };
                Title.Text = "";
                Description.Text = "";
                Date.Date = DateTimeOffset.Now;
                Create.Content = "Create";
                ViewModel.SelectedItem = null;
            } else if ((string)temp.Content == "Delete") {
                DataAccess.DeleteData(ViewModel.SelectedItem.id);
                ViewModel.RemoveItem(ViewModel.SelectedItem.id);

                var dialog = new MessageDialog("Event is deleted!");
                await dialog.ShowAsync();

                Frame.Navigate(typeof(MainPage));
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
                    string title = Title.Text;
                    string detail = Description.Text;
                    string date = Date.Date.ToString();
                    DataAccess.AddData(App.id, title, detail, date, 0, imgData);
                    ViewModel.AddItem(App.id++, title, detail, Date.Date, Photo.Fill, imgData);
                    errMsg += "Create Successfully!\n";
                }

            } else if ((string)temp.Content == "Update") {
                Int64 _id = ViewModel.SelectedItem.id;
                string title = Title.Text;
                string detail = Description.Text;
                string date = Date.Date.ToString();
                int completed = ViewModel.SelectedItem.IsCompleted == true ? 1 : 0;
                DataAccess.UpdateDate(_id, title, detail, date, completed, imgData);
                ViewModel.UpdateItem(_id, title, detail, Date.Date, Photo.Fill, imgData, (bool)ViewModel.SelectedItem.IsCompleted);
                errMsg += "Update Successfully!\n";
            }

            var dialog = new MessageDialog(errMsg);
            await dialog.ShowAsync();

            Frame.Navigate(typeof(MainPage));
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
                ApplicationData.Current.LocalSettings.Values["MyToken"] =
                    StorageApplicationPermissions.FutureAccessList.Add(file);
                /// Load the selected picture
                var stream = await file.OpenReadAsync();
                /// image to byte
                using (var dataRender = new DataReader(stream)) {
                    var imgBytes = new byte[stream.Size];
                    await dataRender.LoadAsync((uint)stream.Size);
                    dataRender.ReadBytes(imgBytes);
                    var brush = new ImageBrush {
                        ImageSource = await BytesToBitmapImage(imgBytes)
                    };
                    Photo.Fill = brush;
                    //save bytes
                    imgData = imgBytes;
                }
            }
        }

        private async Task<BitmapImage> BytesToBitmapImage(byte[] imgByte) {
            try {
                MemoryStream stream = new MemoryStream(imgByte);
                BitmapImage bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                return bitmap;
            }
            catch (ArgumentNullException ex) {
                throw ex;
            }
        }
    }
}
