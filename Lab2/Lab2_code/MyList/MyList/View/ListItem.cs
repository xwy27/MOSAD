using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;

namespace MyList.Models {
    public class ListItem : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool completed;
        private Brush image;

        public Int64 id;
        public string title;
        public string detail;
        public DateTimeOffset date;
        public byte[] img;

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

        public ListItem(Int64 _id, string _title, string _detail, DateTimeOffset _date, byte[] _img, Brush _image = null, bool _completed = false) {
            id = _id;
            title = _title;
            detail = _detail;
            date = _date;
            image = _image;
            img = _img;
            completed = _completed;
        }
    }
}
