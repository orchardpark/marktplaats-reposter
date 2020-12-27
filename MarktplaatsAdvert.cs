using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Text;

namespace marktplaatsreposter
{   
    public enum ShippingDetailType
    {
        Delivery,
        Pickup,
        PickupAndDelivery
    }

    public class MarktplaatsFullAdvert
    {
        public string AdvertTitle { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public ShippingDetailType ShippingDetails { get; set; }
        public double ShippingPrice { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int NumImages { get; set; }
    }

    public class MarktplaatsCompactAdvert
    {

        public string AdvertTitle { get; set; }
        public string Status { get; set; }
        public string Views { get; set; }
    }

    public class MarktplaatsGUIAdvert : INotifyPropertyChanged
    {
        private string advertTitle;
        private string status;
        private bool isChecked;
        private string views;
        public string AdvertTitle
        {
            get
            {
                return advertTitle;
            }
            set
            {
                if (advertTitle != value)
                {
                    advertTitle = value;
                    NotifyPropertyChanged("AdvertTitle");
                }
            }
        }
        public string Status {
            get {
                return status;
            }
            set
            {
                if(status != value)
                {
                    status = value;
                    NotifyPropertyChanged("Status");
                }
            }
        }
        public bool IsChecked {
            get
            {
                return isChecked;
            }
            set
            {
                if(isChecked != value)
                {
                    isChecked = value;
                    NotifyPropertyChanged("IsChecked");
                }
            }
        }
        public string Views {
            get
            {
                return views;
            }
            set
            {
                if(views != value)
                {
                    views = value;
                    NotifyPropertyChanged("Views");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
