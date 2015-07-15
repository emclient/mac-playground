//
//MonoMenuItem.cs
// 
//Author:
//	Lee Andrus <landrus2@by-rite.net>
//
//Copyright (c) 2009-2010 Lee Andrus
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace System.Windows.Forms.CocoaInternal
{

	//[ExportClass("MonoMenuItem", "NSMenuItem")]
	internal sealed class MonoMenuItem : NSMenuItem
	{
		internal delegate void VoidEventHandler ();
		internal event VoidEventHandler Click;

		private class NativeShortcut
		{
			public readonly string key;
			public readonly NSEventModifierMask modifiers;

			public NativeShortcut (string key, NSEventModifierMask modifiers)
			{
				this.key = key;
				this.modifiers = modifiers;
			}
		}
		private static readonly Dictionary<Shortcut,NativeShortcut> shortcutEquivalents = 
			new Dictionary<Shortcut, NativeShortcut> (Enum.GetValues (typeof (Shortcut)).Length);

		static MonoMenuItem ()
		{
			shortcutEquivalents[Shortcut.Alt0] = 
				new NativeShortcut ("0", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.Alt1] = 
				new NativeShortcut ("1", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.Alt2] = 
				new NativeShortcut ("2", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.Alt3] = 
				new NativeShortcut ("3", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.Alt4] = 
				new NativeShortcut ("4", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.Alt5] = 
				new NativeShortcut ("5", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.Alt6] = 
				new NativeShortcut ("6", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.Alt7] = 
				new NativeShortcut ("7", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.Alt8] = 
				new NativeShortcut ("8", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.Alt9] = 
				new NativeShortcut ("9", NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltBksp] = new NativeShortcut ( new String((char) /*NSKey.BackspaceCharacter*/0x08, 1), NSEventModifierMask.AlternateKeyMask);
#if NET_2_0
			shortcutEquivalents[Shortcut.AltDownArrow] = new NativeShortcut (NSString.stringWithCharacters_length (
 				new String((char) Enums.NSDownArrowFunctionKey, 1), 1), 
				Enums.NSFunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
#endif
			shortcutEquivalents[Shortcut.AltF1] = new NativeShortcut (
 				new String((char) NSKey.F1, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF10] = new NativeShortcut (
 				new String((char) NSKey.F10, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF11] = new NativeShortcut (
 				new String((char) NSKey.F11, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF12] = new NativeShortcut (
 				new String((char) NSKey.F12, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF2] = new NativeShortcut (
 				new String((char) NSKey.F2, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF3] = new NativeShortcut (
 				new String((char) NSKey.F3, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF4] = new NativeShortcut (
 				new String((char) NSKey.F4, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF5] = new NativeShortcut (
 				new String((char) NSKey.F5, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF6] = new NativeShortcut (
 				new String((char) NSKey.F6, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF7] = new NativeShortcut (
 				new String((char) NSKey.F7, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF8] = new NativeShortcut (
 				new String((char) NSKey.F8, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltF9] = new NativeShortcut (
 				new String((char) NSKey.F9, 1),
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
#if NET_2_0
			shortcutEquivalents[Shortcut.AltLeftArrow] = new NativeShortcut (
 				new String((char) Enums.NSLeftArrowFunctionKey, 1), 
				Enums.NSFunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltRightArrow] = new NativeShortcut (
 				new String((char) Enums.NSRightArrowFunctionKey, 1), 
				Enums.NSFunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
			shortcutEquivalents[Shortcut.AltUpArrow] = new NativeShortcut (
 				new String((char) Enums.NSUpArrowFunctionKey, 1), 
				Enums.NSFunctionKeyMask | NSEventModifierMask.AlternateKeyMask);
#endif
			shortcutEquivalents[Shortcut.Ctrl0] = 
				new NativeShortcut ("0", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Ctrl1] = 
				new NativeShortcut ("1", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Ctrl2] = 
				new NativeShortcut ("2", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Ctrl3] = 
				new NativeShortcut ("3", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Ctrl4] = 
				new NativeShortcut ("4", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Ctrl5] = 
				new NativeShortcut ("5", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Ctrl6] = 
				new NativeShortcut ("6", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Ctrl7] = 
				new NativeShortcut ("7", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Ctrl8] = 
				new NativeShortcut ("8", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Ctrl9] = 
				new NativeShortcut ("9", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlA] = 
				new NativeShortcut ("a", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlB] = 
				new NativeShortcut ("b", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlC] = 
				new NativeShortcut ("c", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlD] = 
				new NativeShortcut ("d", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlDel] = new NativeShortcut (
				new String((char) NSKey.Delete, 1), NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlE] = 
				new NativeShortcut ("e", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF] = 
				new NativeShortcut ("f", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF1] = new NativeShortcut (
 				new String((char) NSKey.F1, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF10] = new NativeShortcut (
 				new String((char) NSKey.F10, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF11] = new NativeShortcut (
 				new String((char) NSKey.F11, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF12] = new NativeShortcut (
 				new String((char) NSKey.F12, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF2] = new NativeShortcut (
 				new String((char) NSKey.F2, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF3] = new NativeShortcut (
 				new String((char) NSKey.F3, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF4] = new NativeShortcut (
 				new String((char) NSKey.F4, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF5] = new NativeShortcut (
 				new String((char) NSKey.F5, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF6] = new NativeShortcut (
 				new String((char) NSKey.F6, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF7] = new NativeShortcut (
 				new String((char) NSKey.F7, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF8] = new NativeShortcut (
 				new String((char) NSKey.F8, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlF9] = new NativeShortcut (
 				new String((char) NSKey.F9, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlG] = 
				new NativeShortcut ("g", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlH] = 
				new NativeShortcut ("h", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlI] = 
				new NativeShortcut ("i", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlIns] = new NativeShortcut (
 				new String((char) NSKey.Insert, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlJ] = 
				new NativeShortcut ("j", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlK] = 
				new NativeShortcut ("k", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlL] = 
				new NativeShortcut ("l", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlM] = 
				new NativeShortcut ("m", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlN] = 
				new NativeShortcut ("n", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlO] = 
				new NativeShortcut ("o", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlP] = 
				new NativeShortcut ("p", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlQ] = 
				new NativeShortcut ("q", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlR] = 
				new NativeShortcut ("r", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlS] = 
				new NativeShortcut ("s", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift0] = 
				new NativeShortcut ("0", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift1] = 
				new NativeShortcut ("1", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift2] = 
				new NativeShortcut ("2", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift3] = 
				new NativeShortcut ("3", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift4] = 
				new NativeShortcut ("4", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift5] = 
				new NativeShortcut ("5", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift6] = 
				new NativeShortcut ("6", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift7] = 
				new NativeShortcut ("7", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift8] = 
				new NativeShortcut ("8", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShift9] = 
				new NativeShortcut ("9", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftA] = 
				new NativeShortcut ("a", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftB] = 
				new NativeShortcut ("b", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftC] = 
				new NativeShortcut ("c", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftD] = 
				new NativeShortcut ("d", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftE] = 
				new NativeShortcut ("e", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF] = 
				new NativeShortcut ("f", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF1] = new NativeShortcut (
 				new String((char) NSKey.F1, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF10] = new NativeShortcut (
 				new String((char) NSKey.F10, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF11] = new NativeShortcut (
 				new String((char) NSKey.F11, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF12] = new NativeShortcut (
 				new String((char) NSKey.F12, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF2] = new NativeShortcut (
 				new String((char) NSKey.F2, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF3] = new NativeShortcut (
 				new String((char) NSKey.F3, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF4] = new NativeShortcut (
 				new String((char) NSKey.F4, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF5] = new NativeShortcut (
 				new String((char) NSKey.F5, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF6] = new NativeShortcut (
 				new String((char) NSKey.F6, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF7] = new NativeShortcut (
 				new String((char) NSKey.F7, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF8] = new NativeShortcut (
 				new String((char) NSKey.F8, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftF9] = new NativeShortcut (
 				new String((char) NSKey.F9, 1), 
				NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftG] = 
				new NativeShortcut ("g", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftH] = 
				new NativeShortcut ("h", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftI] = 
				new NativeShortcut ("i", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftJ] = 
				new NativeShortcut ("j", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftK] = 
				new NativeShortcut ("k", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftL] = 
				new NativeShortcut ("l", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftM] = 
				new NativeShortcut ("m", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftN] = 
				new NativeShortcut ("n", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftO] = 
				new NativeShortcut ("o", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftP] = 
				new NativeShortcut ("p", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftQ] = 
				new NativeShortcut ("q", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftR] = 
				new NativeShortcut ("r", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftS] = 
				new NativeShortcut ("s", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftT] = 
				new NativeShortcut ("t", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftU] = 
				new NativeShortcut ("u", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftV] = 
				new NativeShortcut ("v", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftW] = 
				new NativeShortcut ("w", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftX] = 
				new NativeShortcut ("x", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftY] = 
				new NativeShortcut ("y", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlShiftZ] = 
				new NativeShortcut ("z", NSEventModifierMask.ControlKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.CtrlT] = 
				new NativeShortcut ("t", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlU] = 
				new NativeShortcut ("u", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlV] = 
				new NativeShortcut ("v", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlW] = 
				new NativeShortcut ("w", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlX] = 
				new NativeShortcut ("x", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlY] = 
				new NativeShortcut ("y", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.CtrlZ] = 
				new NativeShortcut ("z", NSEventModifierMask.ControlKeyMask);
			shortcutEquivalents[Shortcut.Del] = new NativeShortcut (
				new String((char) NSKey.Delete, 1), 0);
			shortcutEquivalents[Shortcut.F1] = new NativeShortcut (
				new String((char) NSKey.F1, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F10] = new NativeShortcut (
				new String((char) NSKey.F10, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F11] = new NativeShortcut (
				new String((char) NSKey.F11, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F12] = new NativeShortcut (
				new String((char) NSKey.F12, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F2] = new NativeShortcut (
				new String((char) NSKey.F2, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F3] = new NativeShortcut (
				new String((char) NSKey.F3, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F4] = new NativeShortcut (
				new String((char) NSKey.F4, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F5] = new NativeShortcut (
				new String((char) NSKey.F5, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F6] = new NativeShortcut (
				new String((char) NSKey.F6, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F7] = new NativeShortcut (
				new String((char) NSKey.F7, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F8] = new NativeShortcut (
				new String((char) NSKey.F8, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.F9] = new NativeShortcut (
				new String((char) NSKey.F9, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.Ins] = new NativeShortcut (
				new String((char) NSKey.Insert, 1), NSEventModifierMask.FunctionKeyMask);
			shortcutEquivalents[Shortcut.None] = null;
			shortcutEquivalents[Shortcut.ShiftDel] = new NativeShortcut (
				new String((char) NSKey.DeleteChar, 1), NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF1] = new NativeShortcut (
				new String((char) NSKey.F1, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF10] = new NativeShortcut (
 				new String((char) NSKey.F10, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF11] = new NativeShortcut (
 				new String((char) NSKey.F11, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF12] = new NativeShortcut (
 				new String((char) NSKey.F12, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF2] = new NativeShortcut (
 				new String((char) NSKey.F2, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF3] = new NativeShortcut (
 				new String((char) NSKey.F3, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF4] = new NativeShortcut (
 				new String((char) NSKey.F4, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF5] = new NativeShortcut (
 				new String((char) NSKey.F5, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF6] = new NativeShortcut (
 				new String((char) NSKey.F6, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF7] = new NativeShortcut (
 				new String((char) NSKey.F7, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF8] = new NativeShortcut (
 				new String((char) NSKey.F8, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftF9] = new NativeShortcut (
 				new String((char) NSKey.F9, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
			shortcutEquivalents[Shortcut.ShiftIns] = new NativeShortcut (
 				new String((char) NSKey.Insert, 1), NSEventModifierMask.FunctionKeyMask | NSEventModifierMask.ShiftKeyMask);
		}

		private MonoMenuItem (IntPtr instance) : base (instance)
		{
			// Cocoa version of event delegate references OnClick ().
			//setAction ("OnClick:");
			//setTarget (this);
			Action = new MonoMac.ObjCRuntime.Selector("OnClick:");
			Target = this;
		}

		public MonoMenuItem ()
		{
		}

		public MonoMenuItem (MenuItem guestItem) : this ()
		{
			Click += guestItem.PerformClick;
			//guestItem.menu_handle = (IntPtr) this;
			string text = guestItem.Text;
			if (null != text)
				SetTitleWithMnemonic (text);
			Enabled = guestItem.Enabled;
			Hidden = guestItem.Visible;

			NativeShortcut shortcutEquivalent = shortcutEquivalents[guestItem.Shortcut];
			if (null != shortcutEquivalent) {
				KeyEquivalent = shortcutEquivalent.key;
				KeyEquivalentModifierMask = shortcutEquivalent.modifiers;
			}

			if (guestItem.Checked)
				State = NSCellStateValue.On;
		}

#pragma warning disable 169
		private void OnClick (NSObject sender)
#pragma warning restore 169
		{
			if (null != Click)
				Click ();
		}
	}
}
