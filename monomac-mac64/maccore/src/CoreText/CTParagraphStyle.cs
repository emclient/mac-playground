// 
// CTParagraphStyle.cs: Implements the managed CTParagraphStyle
//
// Authors: Mono Team
//     
// Copyright 2010 Novell, Inc
// Copyright 2011, 2012 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MonoMac.ObjCRuntime;
using MonoMac.Foundation;
using MonoMac.CoreFoundation;
using MonoMac.CoreGraphics;

namespace MonoMac.CoreText {

#region Paragraph Style Values
	[Since (3,2)]
	public enum CTTextAlignment : byte {
		Left      = 0,
		Right     = 1,
		Center    = 2,
		Justified = 3,
		Natural   = 4,
	}

	[Since (3,2)]
	public enum CTLineBreakMode : byte {
		WordWrapping      = 0,
		CharWrapping      = 1,
		Clipping          = 2,
		TruncatingHead    = 3,
		TruncatingTail    = 4,
		TruncatingMiddle  = 5,
	}

	[Since (3,2)]
	[Flags]
	public enum CTWritingDirection : sbyte {
		Natural     = -1,
		LeftToRight = 0,
		RightToLeft = 1,

		Embedding = (0 << 1),
		Override = (1 << 1)
	}

	internal enum CTParagraphStyleSpecifier : uint {
		Alignment               = 0,
		FirstLineHeadIndent     = 1,
		HeadIndent              = 2,
		TailIndent              = 3,
		TabStops                = 4,
		DefaultTabInterval      = 5,
		LineBreakMode           = 6,
		LineHeightMultiple      = 7,
		MaximumLineHeight       = 8,
		MinimumLineHeight       = 9,
		LineSpacing             = 10,
		ParagraphSpacing        = 11,
		ParagraphSpacingBefore  = 12,
		BaseWritingDirection    = 13,
		MaximumLineSpacing      = 14,
		MinimumLineSpacing      = 15,
		LineSpacingAdjustment   = 16,
		LineBoundsOptions       = 17,

		Count = 18,
	}

	internal struct CTParagraphStyleSetting {
		public CTParagraphStyleSpecifier spec;
		public uint valueSize;
		public IntPtr value;
	}
#endregion

	[StructLayout (LayoutKind.Explicit)]
	internal struct CTParagraphStyleSettingValue {
		[FieldOffset (0)] public byte int8;
		[FieldOffset (0)] public float single;
		[FieldOffset (0)] public IntPtr pointer;
	}

	internal abstract class CTParagraphStyleSpecifierValue {

		protected CTParagraphStyleSpecifierValue (CTParagraphStyleSpecifier spec)
		{
			Spec = spec;
		}

		internal CTParagraphStyleSpecifier Spec {get; private set;}

		internal abstract int ValueSize {get;}
		internal abstract void WriteValue (CTParagraphStyleSettingValue[] values, int index);

		public virtual void Dispose (CTParagraphStyleSettingValue[] values, int index)
		{
		}
	}

	internal class CTParagraphStyleSpecifierByteValue : CTParagraphStyleSpecifierValue {
		byte value;

		public CTParagraphStyleSpecifierByteValue (CTParagraphStyleSpecifier spec, byte value)
			: base (spec)
		{
			this.value = value;
		}

		internal override int ValueSize {
			get {return sizeof (byte);}
		}

		internal override void WriteValue (CTParagraphStyleSettingValue[] values, int index)
		{
			values [index].int8 = value;
		}
	}

	internal class CTParagraphStyleSpecifierSingleValue : CTParagraphStyleSpecifierValue {
		float value;

		public CTParagraphStyleSpecifierSingleValue (CTParagraphStyleSpecifier spec, float value)
			: base (spec)
		{
			this.value = value;
		}

		internal override int ValueSize {
			get {return sizeof (float);}
		}

		internal override void WriteValue (CTParagraphStyleSettingValue[] values, int index)
		{
			values [index].single = value;
		}
	}

	internal class CTParagraphStyleSpecifierIntPtrsValue : CTParagraphStyleSpecifierValue {
		CFArray value;

		public CTParagraphStyleSpecifierIntPtrsValue (CTParagraphStyleSpecifier spec, IntPtr[] value)
			: base (spec)
		{
			this.value = CFArray.FromIntPtrs (value);
		}

		internal override int ValueSize {
			get {return IntPtr.Size;}
		}

		internal override void WriteValue (CTParagraphStyleSettingValue[] values, int index)
		{
			values [index].pointer = value.Handle;
		}

		public override void Dispose (CTParagraphStyleSettingValue[] values, int index)
		{
			values [index].pointer = IntPtr.Zero;
			value.Dispose ();
			value = null;
		}
	}

	[Since (3,2)]
	public class CTParagraphStyleSettings {

		public CTParagraphStyleSettings ()
		{
		}

		public IEnumerable<CTTextTab> TabStops {get; set;}
		public CTTextAlignment? Alignment {get; set;}
		public CTLineBreakMode? LineBreakMode {get; set;}
		public CTWritingDirection? BaseWritingDirection {get; set;}
		public float? FirstLineHeadIndent {get; set;}
		public float? HeadIndent {get; set;}
		public float? TailIndent {get; set;}
		public float? DefaultTabInterval {get; set;}
		public float? LineHeightMultiple {get; set;}
		public float? MaximumLineHeight {get; set;}
		public float? MinimumLineHeight {get; set;}
		public float? LineSpacing {get; set;}
		public float? ParagraphSpacing {get; set;}
		public float? ParagraphSpacingBefore {get; set;}
		public float? MaximumLineSpacing { get; set;}
		public float? MinimumLineSpacing { get; set;}
		public float? LineSpacingAdjustment { get; set; }

		internal List<CTParagraphStyleSpecifierValue> GetSpecifiers ()
		{
			var values = new List<CTParagraphStyleSpecifierValue> ();

			if (TabStops != null)
				values.Add (CreateValue (CTParagraphStyleSpecifier.TabStops, TabStops));
			if (Alignment.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.Alignment, (byte) Alignment.Value));
			if (LineBreakMode.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.LineBreakMode, (byte) LineBreakMode.Value));
			if (BaseWritingDirection.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.BaseWritingDirection, (byte) BaseWritingDirection.Value));
			if (FirstLineHeadIndent.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.FirstLineHeadIndent, FirstLineHeadIndent.Value));
			if (HeadIndent.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.HeadIndent, HeadIndent.Value));
			if (TailIndent.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.TailIndent, TailIndent.Value));
			if (DefaultTabInterval.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.DefaultTabInterval, DefaultTabInterval.Value));
			if (LineHeightMultiple.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.LineHeightMultiple, LineHeightMultiple.Value));
			if (MaximumLineHeight.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.MaximumLineHeight, MaximumLineHeight.Value));
			if (MinimumLineHeight.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.MinimumLineHeight, MinimumLineHeight.Value));
			if (LineSpacing.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.LineSpacing, LineSpacing.Value));
			if (ParagraphSpacing.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.ParagraphSpacing, ParagraphSpacing.Value));
			if (ParagraphSpacingBefore.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.ParagraphSpacingBefore, ParagraphSpacingBefore.Value));
			if (MaximumLineSpacing.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.MaximumLineSpacing, MaximumLineSpacing.Value));
			if (MinimumLineSpacing.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.MinimumLineSpacing, MinimumLineSpacing.Value));
			if (LineSpacingAdjustment.HasValue)
				values.Add (CreateValue (CTParagraphStyleSpecifier.LineSpacingAdjustment, LineSpacingAdjustment.Value));
			return values;
		}

		static CTParagraphStyleSpecifierValue CreateValue (CTParagraphStyleSpecifier spec, IEnumerable<CTTextTab> value)
		{
			var handles = new List<IntPtr>();
			foreach (var ts in value)
				handles.Add (ts.Handle);
			return new CTParagraphStyleSpecifierIntPtrsValue (spec, handles.ToArray ());
		}

		static CTParagraphStyleSpecifierValue CreateValue (CTParagraphStyleSpecifier spec, byte value)
		{
			return new CTParagraphStyleSpecifierByteValue (spec, value);
		}

		static CTParagraphStyleSpecifierValue CreateValue (CTParagraphStyleSpecifier spec, float value)
		{
			return new CTParagraphStyleSpecifierSingleValue (spec, value);
		}
	}

	[Since (3,2)]
	public class CTParagraphStyle : INativeObject, IDisposable {
		internal IntPtr handle;

		internal CTParagraphStyle (IntPtr handle, bool owns)
		{
			if (handle == IntPtr.Zero)
				throw ConstructorError.ArgumentNull (this, "handle");

			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}
		
		public IntPtr Handle {
			get {return handle;}
		}

		~CTParagraphStyle ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}

#region Paragraph Style Creation
		[DllImport (Constants.CoreTextLibrary)]
		static extern IntPtr CTParagraphStyleCreate (CTParagraphStyleSetting[] settings, int settingCount);
		public CTParagraphStyle (CTParagraphStyleSettings settings)
		{
			handle = settings == null 
				? CTParagraphStyleCreate (null, 0)
				: CreateFromSettings (settings);

			if (handle == IntPtr.Zero)
				throw ConstructorError.Unknown (this);
		}

		static unsafe IntPtr CreateFromSettings (CTParagraphStyleSettings s)
		{
			var handle = IntPtr.Zero;

			var specifiers = s.GetSpecifiers ();

			var settings  = new CTParagraphStyleSetting [specifiers.Count];
			var values    = new CTParagraphStyleSettingValue [specifiers.Count];

			int i = 0;
			foreach (var e in specifiers) {
				e.WriteValue (values, i);
				settings [i].spec       = e.Spec;
				settings [i].valueSize  = (uint) e.ValueSize;
				++i;
			}

			fixed (CTParagraphStyleSettingValue* pv = values) {
				for (i = 0; i < settings.Length; ++i) {
					// TODO: is this safe on the ARM?
					byte* p = &pv[i].int8;
					settings[i].value = (IntPtr) p;
				}
				handle = CTParagraphStyleCreate (settings, settings.Length);
			}

			i = 0;
			foreach (var e in specifiers) {
				e.Dispose (values, i);
			}

			return handle;
		}

		public CTParagraphStyle ()
			: this (null)
		{
		}

		[DllImport (Constants.CoreTextLibrary)]
		static extern IntPtr CTParagraphStyleCreateCopy (IntPtr paragraphStyle);
		public CTParagraphStyle Clone ()
		{
			return new CTParagraphStyle (CTParagraphStyleCreateCopy (handle), true);
		}
#endregion

#region Paragraph Style Access
		[DllImport (Constants.CoreTextLibrary)]
		static extern unsafe bool CTParagraphStyleGetValueForSpecifier (IntPtr paragraphStyle, CTParagraphStyleSpecifier spec, uint valueBufferSize, void* valueBuffer);

		public unsafe CTTextTab[] GetTabStops ()
		{
			IntPtr cfArrayRef;
			if (!CTParagraphStyleGetValueForSpecifier (handle, CTParagraphStyleSpecifier.TabStops, (uint) IntPtr.Size, (void*) &cfArrayRef))
				throw new InvalidOperationException ("Unable to get property value.");
			if (cfArrayRef == IntPtr.Zero)
				return new CTTextTab [0];
			return NSArray.ArrayFromHandle (cfArrayRef, p => new CTTextTab (p, false));
		}

		public CTTextAlignment Alignment {
			get {return (CTTextAlignment) GetByteValue (CTParagraphStyleSpecifier.Alignment);}
		}

		unsafe byte GetByteValue (CTParagraphStyleSpecifier spec)
		{
			byte value;
			if (!CTParagraphStyleGetValueForSpecifier (handle, spec, sizeof (byte), &value))
				throw new InvalidOperationException ("Unable to get property value.");
			return value;
		}

		public CTLineBreakMode LineBreakMode {
			get {return (CTLineBreakMode) GetByteValue (CTParagraphStyleSpecifier.Alignment);}
		}

		public CTWritingDirection BaseWritingDirection {
			get {return (CTWritingDirection) GetByteValue (CTParagraphStyleSpecifier.Alignment);}
		}

		public float FirstLineHeadIndent {
			get {return GetFloatValue (CTParagraphStyleSpecifier.FirstLineHeadIndent);}
		}

		unsafe float GetFloatValue (CTParagraphStyleSpecifier spec)
		{
			float value;
			if (!CTParagraphStyleGetValueForSpecifier (handle, spec, sizeof (float), &value))
				throw new InvalidOperationException ("Unable to get property value.");
			return value;
		}

		public float HeadIndent {
			get {return GetFloatValue (CTParagraphStyleSpecifier.HeadIndent);}
		}

		public float TailIndent {
			get {return GetFloatValue (CTParagraphStyleSpecifier.TailIndent);}
		}

		public float DefaultTabInterval {
			get {return GetFloatValue (CTParagraphStyleSpecifier.DefaultTabInterval);}
		}

		public float LineHeightMultiple {
			get {return GetFloatValue (CTParagraphStyleSpecifier.LineHeightMultiple);}
		}

		public float MaximumLineHeight {
			get {return GetFloatValue (CTParagraphStyleSpecifier.MaximumLineHeight);}
		}

		public float MinimumLineHeight {
			get {return GetFloatValue (CTParagraphStyleSpecifier.MinimumLineHeight);}
		}

		public float LineSpacing {
			get {return GetFloatValue (CTParagraphStyleSpecifier.LineSpacing);}
		}

		public float ParagraphSpacing {
			get {return GetFloatValue (CTParagraphStyleSpecifier.ParagraphSpacing);}
		}

		public float ParagraphSpacingBefore {
			get {return GetFloatValue (CTParagraphStyleSpecifier.ParagraphSpacingBefore);}
		}
#endregion
	}
}

