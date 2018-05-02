using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;

namespace MyList.Models {
    class ListItem : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string title;
        private bool completed;
        private Brush image;

        public string id;
        public string detail;
        public string imagePath;
        public DateTimeOffset date;
        
        public Brush Image {
            get { return this.image; }
            set {
                this.image = value;
                NotifyPropertyChanged("Image");
            }
        }

        public string Title {
            get { return this.title; }
            set {
                this.title = value;
                NotifyPropertyChanged("Title");
            }
        }

        public Nullable<bool> IsCompleted {
            get { return this.completed; }
            set {
                this.completed = (bool)value;
                this.NotifyPropertyChanged("IsCompleted");
            }
        }

        public ListItem(string _title, string _detail, DateTimeOffset _date, Brush _image = null, string _imagePath = "") {
            id = Guid.NewGuid().ToString(); //Create ID
            title = _title;
            detail = _detail;
            date = _date;
            image = _image;
            imagePath = "";
            completed = false;
        }
    }
}
