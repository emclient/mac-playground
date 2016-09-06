using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace GUITest
{
    public class TestRunner
    {
        Type mainFormType;

        BackgroundWorker worker = new BackgroundWorker();
        List<MethodInfo> succeded = new List<MethodInfo>();
        List<MethodInfo> failed = new List<MethodInfo>();

        public static void Run(string fullyQualifiedTypeName, string mainFunctionName, string[] args)
        {
            var main = FindMain(fullyQualifiedTypeName, mainFunctionName);
            var runner = new TestRunner();
            runner.Run();

            var result = main.Invoke(null, new object[] { args });
        }

        public static void Run(Action<string[]> main, string[] args, Type mainFormType)
        {
            var runner = new TestRunner(mainFormType);
            runner.Run();

            main(args);
        }

        internal TestRunner(Type mainFormType = null)
        {
            this.mainFormType = mainFormType;

            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += Completed;
        }

        internal void Run()
        {
            worker.RunWorkerAsync();
        }

        void DoWork(object sender, EventArgs e)
        {
            Console.WriteLine("Starting GUI tests..");
            Console.WriteLine("--------------------");

            UI.Initialize(mainFormType);

            foreach (var fixture in GetFixtures())
            {
                Console.WriteLine("Fixture: {0}", fixture.Name);
                Console.WriteLine("--------------------");
                var instance = Activator.CreateInstance(fixture);
                foreach (var test in GetTests(fixture))
                    Execute(instance, test);
            }
        }

        void Completed(object sender, EventArgs e)
        {
            Console.WriteLine("GUI tests finished.");
            Console.WriteLine("--------------------");
            Console.WriteLine("Succeeded: {0}", succeded.Count);
            Console.WriteLine("Failed:    {0}", failed.Count);
            Console.WriteLine("--------------------");

            UI.Close();
        }

        void Execute(object instance, MethodInfo method)
        {
            try
            {
                method.Invoke(instance, null);

                succeded.Add(method);
                Console.WriteLine("{0}: Success!", method.Name);
            }
            catch (Exception e)
            {
                failed.Add(method);
                Console.WriteLine("{0}: Failure! Details: '{1}'", method.Name, e);
            }
        }
    
        List<Type> GetFixtures()
        {
            var fixtures = new List<Type>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                if (null != type.GetCustomAttribute<TestFixtureAttribute>())
                    fixtures.Add(type);
            return fixtures;
        }

        List<MethodInfo> GetTests(Type fixture)
        {
            var tests = new List<MethodInfo>();
            foreach (var method in fixture.GetMethods())
                if (null != method.GetCustomAttribute<TestAttribute>())
                    tests.Add(method);
            return tests;
        }

        private static MethodInfo FindMain(string fullyQualifiedClassName, string methodName = "Main")
        {
            var type = Type.GetType(fullyQualifiedClassName);
            return type.GetMethod(methodName,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy,
                null,
                new Type[] { typeof(string[]) },
                null);
        }
    }
}
