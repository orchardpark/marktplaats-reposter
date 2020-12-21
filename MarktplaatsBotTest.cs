using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium.Remote;

namespace marktplaatsreposter
{
    public class MarktplaatsBotTest
    {
        private MarktplaatsBot bot;
        [SetUp]
        public void SetUp()
        {
            bot = new MarktplaatsBot();
        }
        [TearDown]
        public void TearDown()
        {
            bot.Terminate();
        }
        [Test]
        public void GetAdverts()
        {
            var adverts = bot.GetAdverts();
            Assert.That(adverts.Count > 0);
        }
        [Test]
        public void RePost()
        {
            bot.RePost("Adname");
        }
    }
}
