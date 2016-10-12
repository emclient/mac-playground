using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using WinApi;

using MailClient.UI.Controls.ControlDataGrid;

namespace GUITest
{
    public static class UI
    {
        static Form main;

        public static void Initialize(Type mainFormType)
        {
            main = WaitForForm(mainFormType, 60);
        }

        //public static void Initialize(string mainFormNameOrType)
        //{
        //    main = WaitForForm(mainFormNameOrType);
        //}

        public static Form WaitForForm(string nameOrType, double timeout = 3.0, double interval = 0.1)
        {
            var due = DateTime.Now.AddSeconds(timeout);
            while (DateTime.Now.CompareTo(due) < 0)
            {
                var form = FindForm(nameOrType);
                if (form != null)
                    return form;
                Sleep(interval);
            }
            return null;
        }

        //public static bool WaitForFormClosed(string nameOrType, double timeout = 5.0, double interval = 0.1)
        //{
        //    var due = DateTime.Now.AddSeconds(timeout);
        //    while (DateTime.Now.CompareTo(due) < 0)
        //    {
        //        var form = FindForm(nameOrType);
        //        if (form == null)
        //            return true;
        //        Sleep(interval);
        //    }
        //    return false;
        //}

        public static bool WaitForFormClosed(Form form, double timeout = 5.0, double interval = 0.1)
        {
            var due = DateTime.Now.AddSeconds(timeout);
            bool found = false;
            while (DateTime.Now.CompareTo(due) < 0)
            {
                foreach (var openForm in Application.OpenForms)
                {
                    if (openForm == form)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found || !form.Visible || form.IsDisposed)
                    return true;
                Sleep(interval);
            }
            return false;
        }

        //internal static void Type(params string[] args)
        //{
        //    foreach (var arg in args)
        //        Type(arg);
        //}

        internal static void Type(string text, double delay = 0.05)
        {
            var segments = SplitForSendKeys(text);
            foreach (var segment in segments)
            {
                SendKeys.SendWait(segment);
                Sleep(delay);
            }
        }

        public static IList<String> SplitForSendKeys(string text)
        {
            var segments = new List<string>();
            int modifier = -1;
            int from = 0, i;
            for (i = 0; i < text.Length;)
            {
                char c = text[i];
                switch (c)
                {
                    case '^':
                    case '+':
                    case '%': modifier = i; break;
                    case '(':
                        FinishSegment(segments, text, from, i - 1, modifier);
                        ExtractSegment(segments, text, ref i, ')', ref modifier);
                        from = i;
                        break;
                    case '{':
                        FinishSegment(segments, text, from, i - 1, modifier);
                        ExtractSegment(segments, text, ref i, '}', ref modifier);
                        from = i;
                        break;
                    default:
                        FinishSegment(segments, text, from, i, modifier);
                        i += 1;
                        from = i;
                        break;
                }
            }
            FinishSegment(segments, text, from, i, modifier);
            return segments;
        }

        static void FinishSegment(List<string> segments, string text, int from, int i, int modifier)
        {
            if (i >= text.Length)
                i = text.Length - 1;
            if (i == modifier - 1)
                i = i - 1;
            if (i >= from)
                segments.Add(text.Substring(from, i - from + 1));
        }

        static void ExtractSegment(List<string> segments, string text, ref int i, char c, ref int modifier)
        {
            int from = modifier == i - 1 && modifier >= 0 ? i - 1 : i;
            int to = text.IndexOf(c, i + 2);
            if (to >= 0)
            {
                var segment = text.Substring(from, to - from + 1);
                segments.Add(segment);
                i = to + 1;
                return;
            }

            // Error: the end of the sequence not found. Move forward by 1 char, ignoring it.
            i += 1;
        }

        public static Form WaitForForm(Type type, double timeout = 3.0, double interval = 0.1)
        {
            var due = DateTime.Now.AddSeconds(timeout);
            while (DateTime.Now.CompareTo(due) < 0)
            {
                var form = FindForm(type);
                if (form != null)
                    return form;
                Sleep(interval);
            }
            return null;
        }

        //public static Control GetControl(string path, int maxLevel = int.MaxValue)
        //{
        //    var parts = path.Split('.');

        //    if (parts.Length == 0)
        //        return null;

        //    var form = FindForm(parts[0]);
        //    ThrowIfNull(form, "Form not found: " + parts[0]);

        //    if (parts.Length == 1)
        //        return form;

        //    Control parent = form;
        //    Control member = null;
        //    for (int i = 1; i < parts.Length; ++i)
        //    {
        //        member = FindControl(parent, parts[i]);
        //        if (member == null)
        //            return null;

        //        parent = member;
        //    }

        //    ThrowIfNull(member, String.Format("Member {0} not found", path));
        //    return member;
        //}

        public static T GetMember<T>(string path, Type type, int maxLevel = int.MaxValue) where T : class
        {
            return GetMember(path, maxLevel = int.MaxValue) as T;
        }

        public static object GetMember(string path, int maxLevel = int.MaxValue)
        {
            var parts = path.Split('.');

            if (parts.Length == 0)
                return null;

            var form = FindForm(parts[0]);
            ThrowIfNull(form, "Form not found: " + parts[0]);

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

            ThrowIfNull(member, string.Format("Member {0} not found", path));
            return member;
        }

		public static T TryGetControl<T>(string name, int maxLevel = 1) where T : class
		{
			T control = Perform(() =>
			{
				return GetMember(name) as T;
			});
			ThrowIfNull(control, string.Format("Failed locating control: {0}", control));
			return control;
		}

		public static Control TryGetControl(string name, int maxLevel = 1)
		{
			return TryGetControl<Control>(name, maxLevel);
		}

		internal static void Close()
        {
            Perform(() => { if (main != null) main.Close(); });
        }

        public static Form FindForm(Type type)
        {
            return Perform(() => DoFindForm(type));
        }

        private static Form DoFindForm(Type type)
        {
            foreach (Form form in Application.OpenForms)
                if (type == null || form.GetType().Equals(type))
                    return form;
            return null;
        }

        public static Form FindForm(string nameOrType)
        {
            return Perform(() => DoFindForm(nameOrType));
        }

        private static Form DoFindForm(string nameOrType)
        {
            foreach (Form form in Application.OpenForms)
                if (nameOrType == null || nameOrType.Equals(form.Name) || nameOrType.Equals(form.Text))
                    return form;

            foreach (Form form in Application.OpenForms)
            {
                var typeName = form.GetType().Name;
                if (typeName.Equals(nameOrType) || typeName.EndsWith("." + nameOrType))
                    return form;
            }

            return null;
        }

        public static Control FindControl(Control root, string name, int maxLevel = int.MaxValue)
        {
			return FindControls(root, name, 1, maxLevel)[0];
		}

		public static List<Control> FindControls(Control root, string name, int count, int maxLevel = int.MaxValue)
		{
			return Perform(() => { return DoFindControls(root, name, count, maxLevel); });
		}

		internal static List<Control> DoFindControls(Control instance, string name, int count, int maxLevel)
		{
			List<Control> controls = new List<Control>();

			Queue<Control> q = new Queue<Control>();
			q.Enqueue(instance);

			if (instance.Name.Equals(name))
				controls.Add(instance);

			while ((q.Count > 0) && (controls.Count < count))
			{
				Control control = q.Dequeue();

				if (--maxLevel >= 0)
					foreach (Control child in control.Controls)
					{
						if (child.Name.Equals(name))
							controls.Add(child);

						if (child is ContainerControl)
							q.Enqueue(child);
					}
			}

			if (controls.Count == 0)
			{
				return null;
			}
			return controls;
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

        public static void Delay(double interval, Action action)
        {
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((e) => { action(); });
            timer.Change((int)(1000 * interval), int.MaxValue);
        }

        // Use this method to execute your action in the UI thread.
        public static void Perform(Action action)
        {
            // test - initialization
            if (main == null)
                action();

            //ThrowIfNull(main, "UI not initialized - root control is null.");
            if (main.InvokeRequired)
                main.UIThread(action);
            else
                action();
        }

        public static T Perform<T>(Func<T> function)
        {
            // test - initialization
            if (main == null)
                return function();

            //ThrowIfNull(main, "UI not initialized - root control is null.");
            if (main.InvokeRequired)
                return main.UIThread(function);
            else
                return function();
        }

        public static void ThrowIfNull(object instance, string message = "Instance not found")
        {
            if (instance == null)
                throw new ApplicationException(message);
        }

        public static void Type(char c, double delay = 0.05)
        {
            Key(c);
            Sleep(delay / 2);
            Key(c, KEYEVENTF.KEYUP);
            Sleep(delay / 2);
        }

        internal static void Key(char c, KEYEVENTF flags = 0)
        {
            var input = new INPUT { type = InputType.KEYBOARD };
            input.U.ki.wScan = (ScanCodeShort)c;
            input.U.ki.wVk = 0;
            input.U.ki.dwFlags = KEYEVENTF.UNICODE | flags;
            Win32.SendInput(1, new INPUT[] { input }, INPUT.Size);
        }

        private static void Sleep(double interval)
        {
            Thread.Sleep((int)(1000 * interval));
        }

		public static class Mouse
		{
			public static void Click(Control control, double delay = 0.2)
			{
				Click(GetCenterOfControl, control, delay);
			}

			public static void RightClick(Control control, double delay = 0.2)
			{
				Click(GetCenterOfControl, control, delay, true);
			}

			public static void Click(ControlDataGrid control, int row, double delay = 0.2)
			{
				Click(GetCenterOfControlDataGridRow, new ControlDataGridWithPosition(control, row), delay);
			}

			public static void RightClick(ControlDataGrid control, int row, double delay = 0.2)
			{
				Click(GetCenterOfControlDataGridRow, new ControlDataGridWithPosition(control, row), delay, true);
			}

			public static void Click(ToolStripMenuItem control, double delay = 0.2)
			{
				Click(GetCenterOfToolStripMenuItem, control, delay);
			}

			private static void Click<T>(GetMouseDestinationDelegate<T> getMouseDestinationDelegate, T destinationObject, double delay = 0.2, bool rightClick = false)
			{
				MOUSEEVENTF downFlags;
				MOUSEEVENTF upFlags;
				if (!rightClick)
				{
					downFlags = MOUSEEVENTF.LEFTDOWN;
					upFlags = MOUSEEVENTF.LEFTUP;
				}
				else
				{
					downFlags = MOUSEEVENTF.RIGHTDOWN;
					upFlags = MOUSEEVENTF.RIGHTUP;
				}
				MoveTo(getMouseDestinationDelegate, destinationObject);
				SendInput(downFlags);
				Sleep(delay);
				MoveTo(getMouseDestinationDelegate, destinationObject);
				SendInput(upFlags);
				Sleep(delay);
			}

			private static void SendInput(MOUSEEVENTF flags)
			{
				var input = new INPUT { type = InputType.MOUSE };
				input.U.mi.dx = input.U.mi.dy = input.U.mi.mouseData = 0;
				input.U.mi.dwFlags = flags;
				Win32.SendInput(1, new INPUT[] { input }, INPUT.Size);
			}

			private static Point GetCenterOfControl(Control control)
			{
				var rectangleControl = Rectangle.Empty;
				//Perform(() => { rectangleControl = (control.Parent ?? control).RectangleToScreen(control.Bounds); });
				Perform(() => { rectangleControl = control.RectangleToScreen(control.ClientRectangle); });
				return new Point((rectangleControl.Left + rectangleControl.Right) / 2, (rectangleControl.Top + rectangleControl.Bottom) / 2);
			}

			private static Point GetCenterOfControlDataGridRow(ControlDataGridWithPosition control)
			{
				var dataGridRectangle = Rectangle.Empty;
				Perform(() => { dataGridRectangle = (control.dataGrid.Parent ?? control.dataGrid).RectangleToScreen(control.dataGrid.Bounds); });
				var rowRectangle = new Rectangle(
					dataGridRectangle.X, 
					dataGridRectangle.Y + control.dataGrid.RowHeight * control.row,
					dataGridRectangle.Width,
					control.dataGrid.RowHeight);
				return new Point((rowRectangle.Left + rowRectangle.Right) / 2, (rowRectangle.Top + rowRectangle.Bottom) / 2);
			}

			private static Point GetCenterOfToolStripMenuItem(ToolStripMenuItem control)
			{
				var rectangleControl = Rectangle.Empty;
				Perform(() => { rectangleControl = control.Owner.RectangleToScreen(control.Bounds); });
				return new Point((rectangleControl.Left + rectangleControl.Right) / 2, (rectangleControl.Top + rectangleControl.Bottom) / 2);
			}

			private delegate Point GetMouseDestinationDelegate<T>(T destinationObject);

			private static void MoveTo<T>(GetMouseDestinationDelegate<T> getMouseDestinationDelegate, T destinationObject)
			{
				var dt = 0.02;
				var step = new Point(0, 0);

				while (true)
				{
					var rectangle = Rectangle.Empty;
					var center = getMouseDestinationDelegate(destinationObject);
					rectangle = new Rectangle(center.X - 1, center.Y - 1, 2, 2);

					Point p;
					Win32.GetCursorPos(out p);
					if (rectangle.Contains(p))
						break;

					var d = new Point(center.X - p.X, center.Y - p.Y);
					double l = Math.Sqrt(d.X * d.X + d.Y * d.Y);

					if (Math.Abs(l) > 1)
					{
						var speed = SpeedFromDistance(l); // Faster when far away, slower when approaching the destination.
						var pixelsPerInterval = speed * dt;
						step.X = (int)(pixelsPerInterval * (d.X / l));
						step.Y = (int)(pixelsPerInterval * (d.Y / l));
					}
					if (step.IsEmpty)
						break;

					var input = new INPUT { type = InputType.MOUSE };
					input.U.mi.dx = step.X;
					input.U.mi.dy = step.Y;
					input.U.mi.dwFlags = MOUSEEVENTF.MOVE;
					input.U.mi.mouseData = 0;

					Win32.SendInput(1, new INPUT[] { input }, INPUT.Size);
					Sleep(dt);
				}

				Sleep(dt);
			}

			private static readonly double[] speeds = { 2000, 4000, 1000, 3000, 100, 2000, 10, 500, 4, 50};

			private static double SpeedFromDistance(double l)
			{
				// TODO: Make it independent on the DPI, so that it works the same way on every monitor.
				for (int i = 0; i < speeds.Length; i += 2)
					if (l > speeds[i])
						return speeds[1 + i];
				return speeds[speeds.Length - 1];
			}
		}
	}
}
