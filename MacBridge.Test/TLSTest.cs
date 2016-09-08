using System;
using NUnit.Framework;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MacBridge.Test
{
    [TestFixture()]
    public class TLSTest
    {
        [SetUp]
        public void Setup()
        {
            Trace.Listeners.Add(new ConsoleTraceListener(true));
        }

        [Test]
        public void TestSetDefaultHandlerForURLScheme()
        {
//            var ctx = Thread.CurrentThread.ExecutionContext;
//            ctx.GetObjectData

            //Assert.AreEqual(0, status, "Failed setting default handler for mailto:");
        }

        [Test]
        public void TestAsyncMethod()
        {
            var begin = DateTime.Now;

            var task1 = Task.Factory.StartNew(() => DoSomeWork());
            var task2 = Task.Factory.StartNew(() => DoSomeWork());
            var task3 = Task.Factory.StartNew(() => DoSomeWork());

            Task.WaitAll(task1, task2, task3);
            Console.WriteLine("Duration: " + (DateTime.Now - begin).TotalSeconds);
        }

        public void DoSomeWork()
        {
            var ec = Thread.CurrentThread.ExecutionContext;
            var cc = Thread.CurrentContext;
            Console.WriteLine("EC: " + ec.ToString());
            Console.WriteLine("CC: " + ec.ToString());

            Thread.Sleep(2000);
        }
    }
}

