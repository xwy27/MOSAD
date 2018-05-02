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
using MyList.ViewModels;
using System.Diagnostics;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.Storage.AccessCache;
using Windows.ApplicationModel.DataTransfer;
using DataAccessLibrary;
using System.Threading.Tasks;
using System.IO;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyList {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page {
        private string origin_photo = "ms-appx:///Assets/photo.jpg";
        private byte[] imgData;
        private ListItemViewModel ViewModel = ListItemViewModel.GetInstance();

        // Intialize
        public MainPage() {
            this.InitializeComponent();
            /// Initialize the picture
            var brush = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(origin_photo))
            };
            Photo.Fill = brush;
            PhotoShadow.Source = new BitmapImage(new Uri(origin_photo));

            var count = ViewModel.AllItems.Count;
            if (count == 0) return;
            for (int i = 0; i < count; ++i) {
                InitialTile(ViewModel.AllItems[i]);
            }

        }

        private void InitialTile(ListItem listItem) {
            XmlDocument document = new XmlDocument();
            document.LoadXml(System.IO.File.ReadAllText("Tile.xml"));
            XmlNodeList textElements = document.GetElementsByTagName("text");
            textElements[0].InnerText = listItem.title;
            textElements[2].InnerText = listItem.title;
            textElements[3].InnerText = listItem.detail;
            textElements[4].InnerText = listItem.title;
            textElements[5].InnerText = listItem.detail;
            textElements[6].InnerText = listItem.title;
            textElements[7].InnerText = listItem.detail;
            var tileNotification = new TileNotification(document);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }


        // Page Change
        private void ToNewPage(object sender, RoutedEventArgs e) {
            /// Enable the nevigation button
            if (Detail_Part.Visibility == Visibility.Collapsed)
                Frame.Navigate(typeof(NewPage));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            if (e.NavigationMode == NavigationMode.New) {
                ApplicationData.Current.LocalSettings.Values.Remove("Mainpage");
                Create.Content = "Create";
                Cancel.Content = "Cancel";
            } else {
                // Try to restore state if any, in case we were terminated
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Mainpage")) {
                    Create.Content = "Update";
                    Cancel.Content = "Delete";

                    var composite = ApplicationData.Current.LocalSettings.Values["Mainpage"] as ApplicationDataCompositeValue;
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
            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            try {
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
                    ApplicationData.Current.LocalSettings.Values["Mainpage"] = composite;
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message + ex.StackTrace);
            }
            DataTransferManager.GetForCurrentView().DataRequested -= OnShareDataRequested;
        }


        // Button Click
        private void ShareClick(object sender, RoutedEventArgs e) {
            dynamic temp = e.OriginalSource;
            ViewModel.SelectedItem = (ListItem)(temp.DataContext);
            DataTransferManager.ShowShareUI();
        }

        void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args) {
            DataRequest request = args.Request;
            request.Data.Properties.Title = ViewModel.SelectedItem.Title;
            request.Data.Properties.Description = ViewModel.SelectedItem.detail;
            request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/background.jpg")));
            DataRequestDeferral deferal = request.GetDeferral();
            request.Data.SetText(ViewModel.SelectedItem.detail);
            deferal.Complete();
        }

        private void MyListItem_Clicked(object sender, ItemClickEventArgs e) {
            ViewModel.SelectedItem = (ListItem)e.ClickedItem;
            if (Window.Current.Bounds.Width < 800) {
                Frame.Navigate(typeof(NewPage));
            } else {
                Photo.Fill = ViewModel.SelectedItem.Image;
                Title.Text = ViewModel.SelectedItem.Title;
                Description.Text = ViewModel.SelectedItem.detail;
                Date.Date = ViewModel.SelectedItem.date;
                imgData = ViewModel.SelectedItem.img;
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
                    string title = Title.Text;
                    string detail = Description.Text;
                    string date = Date.Date.ToString();
                    DataAccess.AddData(App.id, title, detail, date, 0, imgData);
                    ViewModel.AddItem(App.id++, title, detail, Date.Date, Photo.Fill, imgData);
                    errMsg += "Create Successfully!\n";
                }

            } else if ((string)temp.Content == "Update") {
                //Check if selectedItem is deleted
                if (ViewModel.SelectedItem != null) {
                    string title = Title.Text;
                    string detail = Description.Text;
                    string date = Date.Date.ToString();
                    int completed = ViewModel.SelectedItem.IsCompleted == true ? 1 : 0;
                    DataAccess.UpdateDate(ViewModel.SelectedItem.id, title, detail, date, completed, imgData);
                    ViewModel.UpdateItem(ViewModel.SelectedItem.id, title, detail, Date.Date, Photo.Fill,
                        imgData, (bool)ViewModel.SelectedItem.IsCompleted);
                    errMsg += "Update Successfully!\n";
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
            Photo.Fill = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(origin_photo))
            };
            PhotoShadow.Source = new BitmapImage(new Uri(origin_photo));
            Title.Text = "";
            Description.Text = "";
            Date.Date = DateTimeOffset.Now;
            Create.Content = "Create";
            ViewModel.SelectedItem = null;
        }

        private void ToEdit(object sender, RoutedEventArgs e) {
            dynamic temp = e.OriginalSource;
            ListItem selectedItem = (ListItem)temp.DataContext;
            ViewModel.SelectedItem = selectedItem;
            if (Window.Current.Bounds.Width < 800) {
                Frame.Navigate(typeof(NewPage));
            } else {
                /// Right part to change
                Photo.Fill = selectedItem.Image;
                Title.Text = selectedItem.Title;
                Description.Text = selectedItem.detail;
                Date.Date = selectedItem.date;
                Create.Content = "Update";
            }
        }

        private async void ToDelete(object sender, RoutedEventArgs e) {
            dynamic d = e.OriginalSource;
            ViewModel.SelectedItem = (ListItem)d.DataContext;
            if (ViewModel.SelectedItem != null) {
                DataAccess.DeleteData(ViewModel.SelectedItem.id);
                ViewModel.RemoveItem(ViewModel.SelectedItem.id);
                var dialog = new MessageDialog("Event is deleted!");
                await dialog.ShowAsync();
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

        private void Tile_Click(object sender, RoutedEventArgs e) {
            XmlDocument document = new XmlDocument();
            document.LoadXml(System.IO.File.ReadAllText("Tile.xml"));
            XmlNodeList textElements = document.GetElementsByTagName("text");
            var count = ViewModel.AllItems.Count;
            if (count == 0) return;
            textElements[0].InnerText = ViewModel.AllItems[count - 1].title;
            textElements[2].InnerText = ViewModel.AllItems[count - 1].title;
            textElements[3].InnerText = ViewModel.AllItems[count - 1].detail;
            textElements[4].InnerText = ViewModel.AllItems[count - 1].title;
            textElements[5].InnerText = ViewModel.AllItems[count - 1].detail;
            textElements[6].InnerText = ViewModel.AllItems[count - 1].title;
            textElements[7].InnerText = ViewModel.AllItems[count - 1].detail;
            var tileNotification = new TileNotification(document);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }

        private async void Search_Click(object sender, RoutedEventArgs e) {
            try {
                string ans = "";
                Int64 type = Type.SelectedIndex;
                if (type == 2) {
                    ans = DataAccess.VagueQueryData(QueryText.Text);
                    ans = ans.Insert(0, "Vague Matched:\n");
                } else if (type == 1) {
                    ans = DataAccess.DateQueryData(QueryText.Text);
                    ans = ans.Insert(0, "Date Matched:\n");
                } else {
                    ans = DataAccess.TitleQueryData(QueryText.Text);
                    ans = ans.Insert(0, "Title Matched:\n");
                }
                var dialog = new MessageDialog(ans);
                await dialog.ShowAsync();
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        private void Check(object sender, RoutedEventArgs e) {
            dynamic temp = e.OriginalSource;
            ListItem selectedItem = (ListItem)temp.DataContext;
            ViewModel.SelectedItem = selectedItem;
            string title = selectedItem.title;
            string detail = selectedItem.detail;
            string date = selectedItem.date.ToString();
            int completed = ViewModel.SelectedItem.IsCompleted == true ? 1 : 0;
            DataAccess.UpdateDate(ViewModel.SelectedItem.id, title, detail, date, completed, ViewModel.SelectedItem.img);
            ViewModel.UpdateItem(ViewModel.SelectedItem.id, title, detail, ViewModel.SelectedItem.date, Photo.Fill, 
                ViewModel.SelectedItem.img, (bool)ViewModel.SelectedItem.IsCompleted);
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
