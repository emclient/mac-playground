using System;
using FormsTest;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;

namespace GUITest
{
	internal class MainTestSuite
	{
		#region Fields

		BackgroundWorker worker = new BackgroundWorker();
		List<Action> succeded = new List<Action>();
		List<Action> failed = new List<Action>();

		MainForm mainForm;

		#endregion //Fields

		#region Public API

		public MainTestSuite Initialize()
		{
			worker.DoWork += Run;
			worker.RunWorkerCompleted += Completed;
			worker.RunWorkerAsync();

			return this;
		}

		public void Done()
		{
		}

		#endregion //Public API

		#region Tests

		void DropDownTest()
		{
			
		}

		void BrowserTest()
		{
			var button = (Button)GetMember("MainForm.button2", typeof(Button));
			button.PerformClick();

			var webForm = WaitForForm("WebForm");
			ThrowIfNull(webForm, "WebForm not found");

			//webForm.Close();
		}

		void DialogTest()
		{
			var button = (Button)GetMember("MainForm.button3", typeof(Button));

			WaitForFormAsyncAndCloseIt("Color", 10, 0.2);
			button.PerformClick();
		}

		#endregion //Tests

		#region Internals

		void Run(object sender, EventArgs e)
		{
			Console.WriteLine("Starting GUI tests..");
			Console.WriteLine("--------------------");

			mainForm = WaitForForm(typeof(MainForm)) as MainForm;

			Execute(BrowserTest);
			Execute(DialogTest);
		}

		void Completed(object sender, EventArgs e)
		{
			Console.WriteLine("GUI tests finished.");
			Console.WriteLine("--------------------");
			Console.WriteLine($"Succeeded: {succeded.Count}");
			Console.WriteLine($"Failed:    {failed.Count}");
			Console.WriteLine("--------------------");

			if (mainForm != null)
				mainForm.Close();
		}

		void Execute(Action action)
		{
			mainForm.UIThread(() =>
			{
				try
				{
					action();

					succeded.Add(action);
					Console.WriteLine($"{action.Method.Name}: Success!");
				}
				catch (Exception e)
				{
					failed.Add(action);
					Console.WriteLine($"{action.Method.Name}: Failure! Details: '{e}'");
				}
			});
		}

		Form WaitForForm(string nameOrType, double timeout = 3.0, double interval = 0.1)
		{
			var due = DateTime.Now.AddSeconds(timeout);
			while (DateTime.Now.CompareTo(due) < 0)
			{
				var form = FindForm(nameOrType);
				if (form != null)
					return form;
				Thread.Sleep((int)(1000 * interval));
			}
			return null;
		}

		Form WaitForForm(Type type, double timeout = 3.0, double interval = 0.1)
		{
			var due = DateTime.Now.AddSeconds(timeout);
			while (DateTime.Now.CompareTo(due) < 0)
			{
				var form = FindForm(type);
				if (form != null)
					return form;
				Thread.Sleep((int)(1000 * interval));
			}
			return null;
		}

		object GetMember(string path, Type type, int maxLevel = int.MaxValue)
		{
			var parts = path.Split('.');

			if (parts.Length == 0)
				return null;

			var form = FindForm(parts[0]);
			ThrowIfNull(form, $"Form not found: {type.Name}");

			if (parts.Length == 1)
				return form;

			object parent = form;
			object member = null;
			for (int i = 1; i < parts.Length; ++i)
			{
				member = FindMember(parent, parts[i]);
				if (member == null)
					return null;

				parent = member;
			}

			ThrowIfNull(member, $"Member {path} not found");
			return member;
		}

		void ThrowIfNull(object instance, string message = "Instance not found")
		{
			if (instance == null)
				throw new ApplicationException(message);
		}

		Form FindForm(Type type)
		{
			foreach (Form form in Application.OpenForms)
				if (form.GetType().Equals(type))
					return form;
			return null;
		}

		Form FindForm(string nameOrType)
		{
			foreach (Form form in Application.OpenForms)
				if (nameOrType.Equals(form.Name) || nameOrType.Equals(form.Text))
					return form;

			foreach (Form form in Application.OpenForms)
			{
				var typeName = form.GetType().Name;
				if (typeName.Equals(nameOrType) || typeName.EndsWith("." + nameOrType))
					return form;
			}

			return null;
		}

		public static object FindMember(object instance, string name, Type type = null, int maxLevel = 1)
		{
			var field = FindField(instance, name, maxLevel);
			if (field != null)
			{
				var value = field.GetValue(instance);
				if (value != null || value.GetType().Equals(type))
					return value;
			}
			return null;
		}

		public static FieldInfo FindField(object instance, string name, int maxLevel = 1)
		{
			if (instance == null)
				return null;

			var type = instance.GetType();
			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

			foreach (var field in fields)
				if (field.Name.Equals(name))
					return field;

			return null;
		}

		void WaitForFormAsyncAndCloseIt(string nameOrTextOrType, int attempts, double interval)
		{
			Form form = null;
			System.Threading.Timer timer = null;
			timer = new System.Threading.Timer((e) =>
			{
				form = FindForm(nameOrTextOrType);
				if (form != null)
				{
					timer.Change(int.MaxValue, int.MaxValue);
					form.UIThread(() => { form.Close(); });
				}
				if (--attempts < 0)
					ThrowIfNull(form);
			});
			timer.Change((int)(1000 * interval), (int) (1000 * interval));
		}
		#endregion // internals
	}
}