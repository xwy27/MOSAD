using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DataAccessLibrary;
using MyList.ViewModels;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.UI.Xaml.Media;

namespace MyList {
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application {
        public static Int64 id;

        public bool IsSuspending = false;
        private ListItemViewModel ViewModel = ListItemViewModel.GetInstance();
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App() {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            DataAccess.InitializeDatabase();
            LoadFromdb();
        }

        private void LoadFromdb() {
            try {
                using (var statement = DataAccess.connection.Prepare("SELECT * FROM MyList ORDER BY Id")) {
                    while (statement.Step() == SQLitePCL.SQLiteResult.ROW) {
                        bool completed = (Int64)statement[4] == 0 ? false : true;
                        BitmapImage image = BytesToBitmapImage((byte[])statement[5]);
                        var brush = new ImageBrush {
                            ImageSource = image
                        };
                        this.ViewModel.AddItem((Int64)statement[0], (string)statement[1], (string)statement[2],
                            DateTimeOffset.Parse((string)statement[3]), brush, (byte[])statement[5], completed);
                        id = id > (Int64)statement[0] ? id : (Int64)statement[0] + 1;
                    }
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        private BitmapImage BytesToBitmapImage(byte[] imgByte) {
            try {
                if (imgByte != null) {
                    MemoryStream stream = new MemoryStream(imgByte);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                    return bitmap;
                }
                return new BitmapImage(new Uri("ms-appx:///Assets/photo.jpg"));
            }
            catch (ArgumentNullException ex) {
                throw ex;
            }
        }

        private void OnResuming(object sender, object e) {
            // TODO: Whatever you need to do to resume your App

            // Clear the IsSyspending flag
            IsSuspending = false;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e) {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached) {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null) {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.Navigated += OnNavigated;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
                    //TODO: 从之前挂起的应用程序加载状态
                    if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NavigationState")) {
                        rootFrame.SetNavigationState((string)ApplicationData.Current.LocalSettings.Values["NavigationState"]);
                    }
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;

            }

            
            if (rootFrame.Content == null) {
                // 当导航堆栈尚未还原时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // 确保当前窗口处于活动状态
            Window.Current.Activate();


            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;
            /// Set the titleBar to be transparent
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            /// Set the color style for the command button of titleBar 
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: 保存应用程序状态并停止任何后台活动
            IsSuspending = true;

            // Get the frame navigation state serialized as a string and save in settings
            Frame frame = Window.Current.Content as Frame;
            ApplicationData.Current.LocalSettings.Values["NavigationState"] = frame.GetNavigationState();
            deferral.Complete();
        }

        private void OnNavigated(object sender, NavigationEventArgs e) {
            //根据页面是否可以返回，在窗口显示返回按钮
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ((Frame)sender).CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void BackRequested(object sender, BackRequestedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) return;

            if (!e.Handled && rootFrame.CanGoBack) {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }
    }
}
