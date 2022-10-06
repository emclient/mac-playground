using System;
using System.Diagnostics;
using NUnit.Framework;
using CoreGraphics;
using Foundation;

namespace MacBridge.Test
{
    [TestFixture()]
    public class LaunchServicesTest
    {
        [SetUp]
        public void Setup()
        {
            Trace.Listeners.Add(new ConsoleTraceListener(true));
        }

        //[Test]
        //public void TestDefaultApplicationUrlForUrl()
        //{
        //    var mailToUrl = new NSUrl("mailto:");
        //    var appUrl = LaunchServices.CopyDefaultApplicationUrlForUrl(mailToUrl, 0);
        //    Debug.WriteLine("url: {0}, appUrl:{1}", new object[] { mailToUrl, appUrl });
        //}

        //[Test]
        //public void TestSetDefaultHandlerForURLScheme()
        //{
        //    var status = LaunchServices.SetDefaultHandlerForURLScheme("mailto", "com.apple.mail");
        //    Assert.AreEqual(0, status, "Failed setting default handler for mailto:");
        //}
    }
}

