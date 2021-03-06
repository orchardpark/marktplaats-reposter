﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using marktplaatsreposter.Properties;
using System;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using Serilog;
using OpenQA.Selenium.Interactions;
using System.Globalization;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Windows.Media;

namespace marktplaatsreposter
{
    static class LevenshteinDistance
    {
        public static int Compute(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
    }

    public static class BotExtensions
    {
        public static void TypeSlow(this IWebElement e, String text)
        {
            var random = new Random();
            foreach (char c in text) {
                e.SendKeys("" + c);
                Thread.Sleep(random.Next(40, 100));
            }
        }

        public static IWebElement FindElementByCSSWithTimeout(this IWebDriver driver, string CSSSelector)
        {
            var timeout = TimeSpan.FromSeconds(10);
            WebDriverWait wait = new WebDriverWait(driver, timeout);
            var result = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector(CSSSelector)));
            return result;
        }

        public static void ScrollToElement(this IWebDriver driver, IWebElement element)
        {
            try
            {
                var random = new Random();
                Thread.Sleep(random.Next(50, 150));
                Actions actions = new Actions(driver);
                actions.MoveToElement(element);
                actions.Perform();
            }
            catch (WebDriverException)
            {
                Log.Debug("Could not scroll to element.");
            }
        }

        public static void RandomSleep(this IWebDriver e)
        {
            var random = new Random();
            Thread.Sleep(random.Next(1000, 3000));
        }
    }
    public class MarktplaatsBot : INotifyPropertyChanged
    {

        private const string markpltaatsBasePath = "https://www.marktplaats.nl";
        private const string marktplaatsAdOverviewPath = "https://www.marktplaats.nl/my-account/sell/index.html";
        private const string tmpImagePath = "images";
        private BotStatus status;
        private bool isSignedIn;
        private IWebDriver driver = null;

        public MarktplaatsBot()
        {
            Status = BotStatus.READY;
            var chromeDriverService = ChromeDriverService.CreateDefaultService("C:\\Chromedriver");
            chromeDriverService.HideCommandPromptWindow = true;

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("disable-infobars");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalCapability("useAutomationExtension", false);
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("window-size=1280,800");
            options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            driver = new ChromeDriver(chromeDriverService, options);
            driver.Manage().Window.Maximize();
        }
        ~MarktplaatsBot()
        {
            driver.Quit();
        }



        #region Helper methods
        private double GetPrice(string euroPrice)
        {
            var cultureInfo = new CultureInfo("nl");
            double price = double.Parse(euroPrice.Split("€")[1], cultureInfo);
            return price;
        }

        private ShippingDetailType GetShippingDetails(string shippingDetails)
        {
            if (shippingDetails.Contains("Verzenden") && shippingDetails.Contains("Ophalen"))
                return ShippingDetailType.PickupAndDelivery;
            else if (shippingDetails.Contains("Ophalen"))
                return ShippingDetailType.Pickup;
            else
                return ShippingDetailType.Delivery;
        }

        private string CleanTxt(string s)
        {
            return s.Replace(",", "").Replace("en", "").Replace("|", "");
        }

        private int Distance(string a, string b)
        {
            a = CleanTxt(a);
            b = CleanTxt(b);
            string[] wordsA = a.Split();
            string[] wordsB = b.Split();
            int totalDistance = 0;
            foreach (string wordA in wordsA)
            {
                int minDistance = int.MaxValue;
                foreach (string wordB in wordsB)
                {
                    int distance = LevenshteinDistance.Compute(wordA.ToLower(), wordB.ToLower());
                    minDistance = Math.Min(distance, minDistance);
                }
                totalDistance += minDistance;
            }
            return totalDistance;
        }

        private MarktplaatsFullAdvert DownloadFullAdvert(string adURL, string title)
        {
            // Get attributes
            driver.Navigate().GoToUrl(adURL);
            double price = GetPrice(driver.FindElementByCSSWithTimeout("span[data-value=\"price\"]").Text);
            double shippingPrice = GetPrice(driver.FindElementByCSSWithTimeout(".shipping-details-value.price").Text);
            string description = driver.FindElementByCSSWithTimeout("#vip-ad-description").Text;
            ShippingDetailType shippingDetails = GetShippingDetails(driver.FindElementByCSSWithTimeout(".shipping-details-value").Text);
            IWebElement categoryDescription = driver.FindElementByCSSWithTimeout(".category-description");
            string category = categoryDescription.FindElements(By.TagName("p"))[0].Text;
            string subCategory = categoryDescription.FindElements(By.TagName("p"))[1].Text;
            int numImages = 0;

            // click on first image
            driver.RandomSleep();
            driver.FindElement(By.ClassName("carousel-scroll")).FindElements(By.TagName("a"))[0].Click();
            driver.RandomSleep();
            driver.FindElement(By.Id("vip-image-viewer")).Click();

            // Download Images
            while (true)
            {
                var carouselList = driver.FindElementByCSSWithTimeout(".mp-Carousel-list");
                string imageSrc = carouselList.FindElements(By.TagName("img"))[numImages].GetAttribute("src");
                if (!Directory.Exists(tmpImagePath))
                {
                    Directory.CreateDirectory(tmpImagePath);
                }
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(imageSrc), $"{tmpImagePath}\\image_{numImages}.png");
                }
                // Move to next image
                numImages++;
                try
                {
                    driver.FindElementByCSSWithTimeout("#fullscreen-image-dialog > div.mp-Dialog-content > div:nth-child(2) > div > a.mp-Carousel-nav--right.mp-Button.mp-Button--round > span").Click();
                    driver.RandomSleep();
                }
                catch(WebDriverTimeoutException)
                {
                    break;
                }
            }
            // escape out
            driver.FindElementByCSSWithTimeout(".mp-Dialog-close").Click();

            var fullAdvert = new MarktplaatsFullAdvert
            {
                AdvertTitle = title,
                Price = price,
                ShippingPrice = shippingPrice,
                Description = description,
                ShippingDetails = shippingDetails,
                Category = category,
                SubCategory = subCategory,
                NumImages = numImages
            };
            return fullAdvert;
        }

        private void DeleteAdvertisement(string adURL)
        {
            driver.Navigate().GoToUrl(adURL);
            var deleteButton = driver.FindElementByCSSWithTimeout("button[data-action=\"delete\"]");
            deleteButton.Click();
            driver.RandomSleep();
            var noDealButton = driver.FindElementByCSSWithTimeout("#NO_DEAL_VIA_MP");
            noDealButton.Click();
        }

        private void PostNewAdvertisement(MarktplaatsFullAdvert advert)
        {
            driver.Navigate().GoToUrl(markpltaatsBasePath);
            var placeAdButton = driver.FindElementByCSSWithTimeout("a[data-role=\"placeAd\"]");
            placeAdButton.Click();
            // enter title
            var titleInput = driver.FindElementByCSSWithTimeout("#category-keywords");
            titleInput.TypeSlow(advert.AdvertTitle);
            // enter category
            var categorySelect = new SelectElement(driver.FindElementByCSSWithTimeout("#cat_sel_1"));
            categorySelect.SelectByText(advert.Category);
            driver.RandomSleep();
            // find most suitable subcategory
            var subCategorySelect = new SelectElement(driver.FindElementByCSSWithTimeout("#cat_sel_2"));
            var subCatOptions = subCategorySelect.Options;
            int minDistance = int.MaxValue;
            string bestCat2Option = null;
            string bestCat3Option = null;
            foreach (var subCatOption in subCatOptions)
            {
                if (subCatOption.Text.Equals("Kies subgroep") || subCatOption.Text.Equals("Kies categorie"))
                    continue;
                subCategorySelect.SelectByText(subCatOption.Text);
                var subSubCategorySelect = new SelectElement(driver.FindElementByCSSWithTimeout("#cat_sel_3"));
                var subSubCatOptions = subSubCategorySelect.Options;
                foreach (var subSubCatOption in subSubCatOptions)
                {
                    if (subSubCatOption.Text.Equals("Kies rubriek"))
                        continue;
                    var distancdeCat3 = Distance(advert.SubCategory, subSubCatOption.Text);
                    Console.WriteLine($"Ad: {advert.SubCategory} subSubCategory: {subSubCatOption.Text} -- Distance: {distancdeCat3}");
                    if (minDistance > distancdeCat3)
                    {
                        minDistance = distancdeCat3;
                        bestCat2Option = subCatOption.Text;
                        bestCat3Option = subSubCatOption.Text;
                    }
                }
            }
            subCategorySelect.SelectByText(bestCat2Option);
            driver.RandomSleep();
            new SelectElement(driver.FindElementByCSSWithTimeout("#cat_sel_3")).SelectByText(bestCat3Option);
            // click next
            driver.FindElementByCSSWithTimeout("#category-selection-submit").Click();

            // fotos
            string[] images = Directory.GetFiles(tmpImagePath);
            for (int i = 0; i < advert.NumImages; i++)
            {
                var fotoInput = driver.FindElementByCSSWithTimeout($"#uploader-container-{i}").FindElement(By.CssSelector("input[type=\"file\"]"));
                driver.ScrollToElement(fotoInput);
                var imageFile = images[i];
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), imageFile);
                fotoInput.SendKeys(fullPath);
                driver.RandomSleep();
            }
            // write description
            driver.RandomSleep();
            driver.SwitchTo().Frame("description_nl-NL_ifr");
            var description = driver.FindElementByCSSWithTimeout("#tinymce");
            driver.ScrollToElement(description);
            description.TypeSlow(advert.Description);
            driver.SwitchTo().ParentFrame();

            // write price
            var priceSelector = new SelectElement(driver.FindElementByCSSWithTimeout("#syi-price-type-dropdown > div > select"));
            priceSelector.SelectByText("Vraagprijs");
            driver.RandomSleep();
            var price = driver.FindElementByCSSWithTimeout("#syi-bidding-price > input");
            driver.ScrollToElement(price);
            price.TypeSlow(advert.Price.ToString());

            // shipping details
            var shippingSelectorElement = driver.FindElementByCSSWithTimeout("#deliveryMethod > div > select");
            var shippingSelector = new SelectElement(shippingSelectorElement);
            driver.ScrollToElement(shippingSelectorElement);
            switch (advert.ShippingDetails)
            {
                case ShippingDetailType.Delivery:
                    shippingSelector.SelectByText("Verzenden");
                    break;
                case ShippingDetailType.Pickup:
                    shippingSelector.SelectByText("Ophalen");
                    break;
                case ShippingDetailType.PickupAndDelivery:
                    shippingSelector.SelectByText("Ophalen of Verzenden");
                    break;
            }

            // shipping price
            var andersButton = driver.FindElementByCSSWithTimeout("#syi-shipping-method > label.form-label.syi-shipping-method-1 > span");
            driver.ScrollToElement(andersButton);
            andersButton.Click();
            var shippingPriceInput = driver.FindElementByCSSWithTimeout("#shipping-options > div:nth-child(3) > div > input");
            driver.ScrollToElement(shippingPriceInput);
            shippingPriceInput.Clear();
            shippingPriceInput.TypeSlow(advert.ShippingPrice.ToString());

            

            // select free option
            var freeOption = driver.FindElementByCSSWithTimeout("#js-products").FindElement(By.CssSelector("div[data-val=\"FREE\"]"));
            driver.ScrollToElement(freeOption);
            freeOption.Click();

            // Post ad
            var confirmPlaceAdButton = driver.FindElementByCSSWithTimeout("#syi-place-ad-button > span");
            driver.ScrollToElement(confirmPlaceAdButton);
            confirmPlaceAdButton.Click();
        }
        #endregion

        #region Public methods
        public List<MarktplaatsCompactAdvert> GetAdverts()
        {
            Status = BotStatus.PROCESSING;
            var adverts = new List<MarktplaatsCompactAdvert>();
            CheckSignedIn();
            if (!isSignedIn)
            {
                SignIn();
            }
            driver.Navigate().GoToUrl(marktplaatsAdOverviewPath);

            try
            {
                driver.FindElementByCSSWithTimeout("#idbtnCancel").Click();
            } catch (WebDriverTimeoutException e)
            {
                Log.Debug("No feedback button found!");
            }

            var adListingTable = driver.FindElementByCSSWithTimeout("#ad-listing-table-body");
            var adElements = adListingTable.FindElements(By.ClassName("ad-listing"));
            foreach (var adElement in adElements)
            {
                driver.ScrollToElement(adElement);
                string title = adElement.FindElement(By.CssSelector(".description-title .title")).Text;
                string status = adElement.FindElement(By.ClassName("listing-status")).Text;
                string views = adElement.FindElement(By.ClassName("amount")).Text;
                var compactAd = new MarktplaatsCompactAdvert
                {
                    AdvertTitle = title,
                    Status = status,
                    Views = views
                };
                adverts.Add(compactAd);
            }
            Status = BotStatus.READY;

            return adverts;
        }

        public void RePost(string adTitle, bool deleteOldAd)
        {
            Status = BotStatus.PROCESSING;
            CheckSignedIn();
            if (!isSignedIn)
            {
                SignIn();
            }
            driver.Navigate().GoToUrl(marktplaatsAdOverviewPath);
            Thread.Sleep(5000);
            var adListingTable = driver.FindElement(By.Id("ad-listing-table-body"));
            var adElements = adListingTable.FindElements(By.ClassName("ad-listing"));
            foreach (var adElement in adElements)
            {

                string title = adElement.FindElement(By.CssSelector(".description-title .title")).Text;
                string adURL = adElement.FindElement(By.TagName("a")).GetAttribute("href");
                driver.ScrollToElement(adElement);

                if (title.Equals(adTitle))
                {
                    var fullAdvert = DownloadFullAdvert(adURL, title);
                    if(deleteOldAd)
                        DeleteAdvertisement(adURL);
                    PostNewAdvertisement(fullAdvert);
                    break;
                }
            }
            Status = BotStatus.READY;
        }

        public void Terminate()
        {
            driver.Quit();
        }

        public void CheckSignedIn()
        {
            if (isSignedIn) return;
            driver.Navigate().GoToUrl(markpltaatsBasePath);
            try
            {
                var loginElement = driver.FindElement(By.CssSelector("a[data-role=login]"));
                isSignedIn = loginElement == null;
                if (!isSignedIn) Status = BotStatus.NOT_SIGNED_IN;
            } catch (NoSuchElementException e)
            {
                isSignedIn = true;
            }
        }

        public void SignIn()
        {
            Status = BotStatus.PROCESSING;
            driver.Navigate().GoToUrl(markpltaatsBasePath);
            var loginElement = driver.FindElement(By.CssSelector("a[data-role=login]"));
            loginElement.Click();
            var emailInputField = driver.FindElementByCSSWithTimeout("input[type=email]");
            var passwordField = driver.FindElement(By.CssSelector("input[type=password]"));
            emailInputField.TypeSlow(Settings.Default.email);
            passwordField.TypeSlow(Settings.Default.password);
            driver.RandomSleep();
            passwordField.SendKeys(Keys.Return);
            driver.RandomSleep();
            Status = BotStatus.READY;
        }
        #endregion

        #region Notify

        public BotStatus Status
        {
            get { return status; }
            set
            {
                if(status!= value)
                {
                    status = value;
                    NotifyPropertyChanged("Status");
                    NotifyPropertyChanged("GetColorForStatus");
                    NotifyPropertyChanged("SignOnEnabled");
                    NotifyPropertyChanged("RefreshEnabled");
                    NotifyPropertyChanged("RepostEnabled");
                }
            }
        }

        public bool SignOnEnabled
        {
            get
            {
                return (status == BotStatus.NOT_SIGNED_IN);
            }
        }

        public bool RefreshEnabled
        {
            get
            {
                return status == BotStatus.READY;
            }
        }

        public bool RepostEnabled
        {
            get
            {
                return status == BotStatus.READY;
            }
        }


        
        public Brush GetColorForStatus
        {
            get
            {
                switch (status)
                {
                    case BotStatus.NOT_SIGNED_IN:
                        return new SolidColorBrush(Color.FromRgb(255, 127, 80)); // Orange
                    case BotStatus.PROCESSING:
                        return new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red
                    case BotStatus.READY:
                        return new SolidColorBrush(Color.FromRgb(34, 139, 34)); // Green
                    default:
                        return new SolidColorBrush(Color.FromRgb(0, 0, 0)); // Black
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}
