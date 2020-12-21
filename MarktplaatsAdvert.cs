using System;
using System.Collections.Generic;
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

    public class MarktplaatsGUIAdvert
    {
        public string AdvertTitle { get; set; }
        public string Status { get; set; }
        public bool IsChecked { get; set; }
        public string Views { get; set; }
    }
}
