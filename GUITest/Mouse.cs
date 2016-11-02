using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using WinApi;

namespace GUITest
{
	public static class Mouse
	{
		public enum ClickType
		{
			Left,
			Right,
			DoubleClick
		}

		internal delegate Rectangle GetScreenRectangleOfControl<T>(T destinationObject);

		internal delegate void ThrowIfControlNotVisible<T>(T control, string message = null);

		internal static void Click<T>(ClickType clickType, GetScreenRectangleOfControl<T> getMouseDestinationDelegate, ThrowIfControlNotVisible<T> throwNotVisibleDelegate, T destinationObject)
		{
			throwNotVisibleDelegate(destinationObject);
			MOUSEEVENTF downFlags = (clickType == ClickType.Right) ? MOUSEEVENTF.RIGHTDOWN : MOUSEEVENTF.LEFTDOWN;
			MOUSEEVENTF upFlags = (clickType == ClickType.Right) ? MOUSEEVENTF.RIGHTUP : MOUSEEVENTF.LEFTUP;
			int numberOfClicks = (clickType == ClickType.DoubleClick) ? 2 : 1;
			for (int i = 0; i < numberOfClicks; i++)
			{
				MoveTo(getMouseDestinationDelegate, destinationObject);
				SendInput(downFlags);
				Thread.Sleep(SystemInformation.DoubleClickTime / 8);
				MoveTo(getMouseDestinationDelegate, destinationObject);
				SendInput(upFlags);
				Thread.Sleep(SystemInformation.DoubleClickTime / 8);

				if (clickType == ClickType.DoubleClick)
					Thread.Sleep(SystemInformation.DoubleClickTime / 4);
			}

		}

		private static void SendInput(MOUSEEVENTF flags)
		{
			var input = new INPUT { type = InputType.MOUSE };
			input.U.mi.dx = input.U.mi.dy = input.U.mi.mouseData = 0;
			input.U.mi.dwFlags = flags;
			Win32.SendInput(1, new INPUT[] { input }, INPUT.Size);
		}

		private static Point GetCenterOfRectangle(Rectangle rectangle)
		{
			return new Point((rectangle.Left + rectangle.Right) / 2, (rectangle.Top + rectangle.Bottom) / 2);
		}

		private static void MoveTo<T>(GetScreenRectangleOfControl<T> getScreenRectangleOfControl, T destinationObject)
		{
			var dt = 0.02;
			var step = new Point(0, 0);

			while (true)
			{
				Point center = GetCenterOfRectangle(UI.Perform(() => { return getScreenRectangleOfControl(destinationObject); }));
				Rectangle rectangle = new Rectangle(center.X - 1, center.Y - 1, 2, 2);

				Point cursorPosition;
				Win32.GetCursorPos(out cursorPosition);
				if (rectangle.Contains(cursorPosition))
					break;

				Point coordinatesDifference = new Point(center.X - cursorPosition.X, center.Y - cursorPosition.Y);
				double distance = Math.Sqrt(coordinatesDifference.X * coordinatesDifference.X + coordinatesDifference.Y * coordinatesDifference.Y);

				if (distance > 1)
				{
					double speed = SpeedFromDistance(distance); // Faster when far away, slower when approaching the destination.
					double pixelsPerInterval = speed * dt;
					step.X = (int)(pixelsPerInterval * (coordinatesDifference.X / distance));
					step.Y = (int)(pixelsPerInterval * (coordinatesDifference.Y / distance));
				}

				if (step.IsEmpty)
					break;

				var input = new INPUT { type = InputType.MOUSE };
				input.U.mi.dx = step.X;
				input.U.mi.dy = step.Y;
				input.U.mi.dwFlags = MOUSEEVENTF.MOVE;
				input.U.mi.mouseData = 0;

				Win32.SendInput(1, new INPUT[] { input }, INPUT.Size);
				UI.Sleep(dt);
			}

			UI.Sleep(dt);
		}

		private static readonly double[,] speeds = { { 2000, 4000 }, { 1000, 3000 }, { 100, 2000 }, { 10, 500 }, { 4, 100 } };

		private static double SpeedFromDistance(double distance)
		{
			// TODO: Make it independent on the DPI, so that it works the same way on every monitor.
			for (int i = 0; i < speeds.GetLength(0); i++)
				if (distance > speeds[i, 0])
					return speeds[i, 1];
			return speeds[speeds.GetLength(0) - 1, 1];
		}
	}
}
