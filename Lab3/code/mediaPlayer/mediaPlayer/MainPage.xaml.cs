using System;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace mediaPlayer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page {
        // Slider pressed state
        private bool sliderPressed = false;
        
        // A timer keeps in sync with the media
        private DispatcherTimer timer;
       
        // Full screen state
        private bool isFullScreenToggle = false;
        public bool IsFullScreen {
            get { return isFullScreenToggle; }
            set { isFullScreenToggle = value; }
        }

        // Save size before full screen
        private Size previousSize = new Size();

        // Save volume before muted
        private double previousVolume;

        // Initialize page
        public MainPage() {
            this.InitializeComponent();
            /// Set the titleBar to be transparent
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            /// Set the color style for the command button of titleBar 
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Gray;
            ApplicationView.GetForCurrentView().TitleBar.ButtonHoverBackgroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonHoverForegroundColor = Colors.Black;
        }

        // Add pointer handler for the the slider
        private void MainPage_Loaded(object sender, RoutedEventArgs e) {
            timelineSlider.ValueChanged += TimelineSliderValueChanged;
            PointerEventHandler pointerPressedHandler = new PointerEventHandler(SliderPointerEntered);
            timelineSlider.AddHandler(PointerPressedEvent, pointerPressedHandler, true);

            PointerEventHandler pointerReleasedHandler = new PointerEventHandler(SliderPointerCaptureLost);
            timelineSlider.AddHandler(PointerCaptureLostEvent, pointerReleasedHandler, true);
        }

        // Play media
        private void MdeiaPlay(object sender, RoutedEventArgs e) {
            // Reset the play rate to normal
            if (myMediaPlayer.DefaultPlaybackRate != 1) {
                myMediaPlayer.DefaultPlaybackRate = 1.0;
            }
            myMediaPlayer.Play();
            if (Cover.Visibility == Visibility.Visible) {
                RotateCover.Begin();
            }
        }

        // Pause media
        private void MediaPause(object sender, RoutedEventArgs e) {
            myMediaPlayer.Pause();
            if (Cover.Visibility == Visibility.Visible) {
                RotateCover.Pause();
            }
        }

        // Stop media
        private void MediaStop(object sender, RoutedEventArgs e) {
            myMediaPlayer.Stop();
            if (Cover.Visibility == Visibility.Visible) {
                RotateCover.Stop();
            }
        }

        // Set back playing rate
        private void MediaBack(object sender, RoutedEventArgs e) {
            myMediaPlayer.DefaultPlaybackRate = -2.0;
            myMediaPlayer.Play();
        }

        // Set forward playing rate
        private void MediaForward(object sender, RoutedEventArgs e) {
            myMediaPlayer.DefaultPlaybackRate = 2.0;
            myMediaPlayer.Play();
        }

        // Set mute or not
        private void MediaMute(object sender, RoutedEventArgs e) {
            myMediaPlayer.IsMuted = !myMediaPlayer.IsMuted;
            if (myMediaPlayer.IsMuted) {
                Mute.Icon = new SymbolIcon(Symbol.Mute);
                previousVolume = myMediaPlayer.Volume;
                myMediaPlayer.Volume = 0;
            } else {
                Mute.Icon = new SymbolIcon(Symbol.Volume);
                myMediaPlayer.Volume = previousVolume;
            }
        }

        // Set full screen or not
        private void FullScreenToggle() {
            this.IsFullScreen = !this.IsFullScreen;

            if (this.IsFullScreen) {
                TransportControlsPanel.Visibility = Visibility.Collapsed;

                previousSize.Width = videoContainer.ActualWidth;
                previousSize.Height = videoContainer.ActualHeight;

                videoContainer.Width = Window.Current.Bounds.Width;
                videoContainer.Height = Window.Current.Bounds.Height;
                myMediaPlayer.Width = Window.Current.Bounds.Width;
                myMediaPlayer.Height = Window.Current.Bounds.Height;
            } else {
                TransportControlsPanel.Visibility = Visibility.Visible;
                
                videoContainer.Width = previousSize.Width;
                videoContainer.Height = previousSize.Height;
                myMediaPlayer.Width = previousSize.Width;
                myMediaPlayer.Height = previousSize.Height;
            }
        }

        private void MediaFullScreen(object sender, RoutedEventArgs e) {
            FullScreenToggle();
        }

        // Select media to play
        private async void MediaSelect(object sender, RoutedEventArgs e) {
            FileOpenPicker picker = new FileOpenPicker();
            /// Initialize the picture file type to take
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".wma");
            picker.FileTypeFilter.Add(".mp3");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null) {
                /// Load the selected picture
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                myMediaPlayer.SetSource(ir, file.ContentType);
                myMediaPlayer.Play();
                if (file.ContentType == "audio/mpeg") {
                    Cover.Visibility = Visibility.Visible;
                } else {
                    Cover.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Get the frequency of the steps on the slider's scale
        private double SliderFrequency(TimeSpan time) {
            double stepFrequency = -1;
            double absValue = (int)Math.Round(time.TotalSeconds, MidpointRounding.AwayFromZero);

            stepFrequency = (int)(Math.Round(absValue / 1000));

            if (time.TotalMinutes >= 10 && time.TotalMinutes < 30) {
                stepFrequency = 10;
            } else if (time.TotalMinutes >= 30 && time.TotalMinutes < 60) {
                stepFrequency = 30;
            } else if (time.TotalHours >= 1) {
                stepFrequency = 60;
            }

            if (stepFrequency == 0) {
                stepFrequency += 1;
            }

            if (stepFrequency == 1) {
                stepFrequency = absValue / 1000;
            }

            return stepFrequency;
        }

        // Initialize the timer
        private void SetupTimer() {
            timer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(timelineSlider.StepFrequency)
            };
            StartTimer();
        }

        // Keep timer in sync with media
        private void TimerTick(object sender, object e) {
            if (!sliderPressed) {
                timelineSlider.Value = myMediaPlayer.Position.TotalSeconds;
            }
        }

        // Start timer to count
        private void StartTimer() {
            timer.Tick += TimerTick;
            timer.Start();
        }

        // Stop timer to count
        private void StopTimer() {
            timer.Stop();
            timer.Tick -= TimerTick;
        }

        // slider pressed
        void SliderPointerEntered(object sender, PointerRoutedEventArgs e) {
            sliderPressed = true;
        }

        // slider lost pressed
        void SliderPointerCaptureLost(object sender, PointerRoutedEventArgs e) {
            // Set the media position to the current slider position
            myMediaPlayer.Position = TimeSpan.FromSeconds(timelineSlider.Value);
            sliderPressed = false;
        }

        void TimelineSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
            if (!sliderPressed) {
                myMediaPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
            }
        }

        // Media loaded
        private void MyMediaOpened(object sender, RoutedEventArgs e) {
            double absValue = (int)Math.Round(
                myMediaPlayer.NaturalDuration.TimeSpan.TotalSeconds, 
                MidpointRounding.AwayFromZero);

            // Initialize timer
            timelineSlider.Maximum = absValue;
            timelineSlider.StepFrequency = SliderFrequency(myMediaPlayer.NaturalDuration.TimeSpan);
            SetupTimer();
        }

        // Listen for the media state and timer
        private void MyMediaCurrentStateChanged(object sender, RoutedEventArgs e) {
            if (myMediaPlayer.CurrentState == MediaElementState.Playing) {
                if (sliderPressed) {
                    timer.Stop();
                } else {
                    timer.Start();
                }
            }

            if (myMediaPlayer.CurrentState == MediaElementState.Paused) {
                timer.Stop();
            }

            if (myMediaPlayer.CurrentState == MediaElementState.Stopped) {
                timer.Stop();
                timelineSlider.Value = 0;
            }
        }

        // Media ends
        private void MyMediaEnded(object sender, RoutedEventArgs e) {
            StopTimer();
            timelineSlider.Value = 0.0;
            if (Cover.Visibility == Visibility.Visible) {
                RotateCover.Stop();
            }
        }

        // Media Volume change
        private void VolumeChange(object sender, RoutedEventArgs e) {
            if (myMediaPlayer.Volume == 0) {
                Mute.Icon = new SymbolIcon(Symbol.Mute);
            } else {
                Mute.Icon = new SymbolIcon(Symbol.Volume);
            }
        }

        // Keyboard listener for one press
        private void VideoContainerKeyUp(object sender, KeyRoutedEventArgs e) {
            // Listen for [ESC] to exit full screen
            if (IsFullScreen && e.Key == Windows.System.VirtualKey.Escape) {
                FullScreenToggle();
            }

            // Listen for [Space] to stop or play media
            if (e.Key == Windows.System.VirtualKey.Space) {
                if (myMediaPlayer.CurrentState == MediaElementState.Playing) {
                    myMediaPlayer.Pause();
                } else {
                    myMediaPlayer.Play();
                }
            }

            // Listen for [Up] and [Down] to control volume
            if (e.Key == Windows.System.VirtualKey.Up) {
                if (myMediaPlayer.Volume < 1) {
                    myMediaPlayer.Volume += 0.1;
                }
            } else if (e.Key == Windows.System.VirtualKey.Down) {
                if (myMediaPlayer.Volume > 0) {
                    myMediaPlayer.Volume -= 0.1;
                }
            }

            e.Handled = true;
        }

        // Keyboard listener for holding press
        private void VideoContainerKeyDown(object sender, KeyRoutedEventArgs e) {
            // Listen for [Up] and [Down] to control volume
            if (e.Key == Windows.System.VirtualKey.Up) {
                if (myMediaPlayer.Volume < 1) {
                    myMediaPlayer.Volume += 0.1;
                }
            } else if (e.Key == Windows.System.VirtualKey.Down) {
                if (myMediaPlayer.Volume > 0) {
                    myMediaPlayer.Volume -= 0.1;
                }
            }
        }
    }
}
