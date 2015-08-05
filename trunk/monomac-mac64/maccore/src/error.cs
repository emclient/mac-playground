//
// error.cs: Error handling code for bmac/btouch
//
// Authors:
//   Rolf Bjarne Kvinge <rolf@xamarin.com
//   Sebastien Pouliot <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin, Inc.
//
//

using System;
using System.Collections.Generic;

// Error allocation
//
// BI0xxx	the generator itself, e.g. parameters, environment
// BI1xxx	code generation
//	BI10xx	errors
//		BI1001 Do not know how to make a trampoline for {0}
//		BI1002 Unknown kind {0} in method '{1}'
//		BI1003 The delegate method {0}.{1} needs to take at least one parameter
//		BI1004 The delegate method {0}.{1} is missing the [EventArgs] attribute (has {2} parameters)
//		BI1005 EventArgs in {0}.{1} attribute should not include the text `EventArgs' at the end
//		BI1006 The delegate method {0}.{1} is missing the [DelegateName] attribute (or EventArgs)
//		BI1007 Unknown attribute {0} on {1}
//		BI1008 [IsThreadStatic] is only valid on properties that are also [Static]
//		BI1009 No selector specified for method `{0}.{1}'
//		BI1010 No Export attribute on {0}.{1} property
//		BI1011 Do not know how to extract type {0}/{1} from an NSDictionary
//		BI1012 No Export or Bind attribute defined on {0}.{1}
//		BI1013 Unsupported type for Fields (string), you probably meant NSString
//		BI1014 Unsupported type for Fields: {0}
//		BI1015 In class {0} You specified the Events property, but did not bind those to names with Delegates
//		BI1016 The delegate method {0}.{1} is missing the [DefaultValue] attribute
//		BI1017 Do not know how to make a signature for {0}
//		BI1018 No [Export] attribute on property {0}.{1}
//		BI1019 Invalid [NoDefaultValue] attribute on method `{0}.{1}'
//		BI1020 Unsupported type {0} used on exported method {1}.{2}
//		BI1021 Unsupported type for read/write Fields: {0}
//              BI1022 Model classes can not be categories
//	BI11xx	warnings
//		BI1101 Trying to use a string as a [Target]
//		BI1102 Using the deprecated EventArgs for a delegate signature in {0}.{1}, please use DelegateName instead
// BI2xxx	reserved
// BI3xxx	reserved
// BI4xxx	reserved
// BI5xxx	reserved
// BI6xxx	reserved
// BI7xxx	reserved
// BI8xxx	reserved
// BI9xxx	reserved

public class BindingException : Exception {
	
	public BindingException (int code, string message, params object[] args) : 
		this (code, false, message, args)
	{
	}

	public BindingException (int code, bool error, string message, params object[] args) : 
		this (code, error, null, message, args)
	{
	}

	public BindingException (int code, bool error, Exception innerException, string message, params object[] args) : 
		base (String.Format (message, args), innerException)
	{
		Code = code;
		Error = error;
	}

	public int Code { get; private set; }
	
	public bool Error { get; private set; }
	
	// http://blogs.msdn.com/b/msbuild/archive/2006/11/03/msbuild-visual-studio-aware-error-messages-and-message-formats.aspx
	public override string ToString ()
	{
		 return String.Format ("{0} BI{1:0000}: {3}: {2}",
			Error ? "error" : "warning", Code, Message, BindingTouch.ToolName);
	}
}

public static class ErrorHelper {
	
	static public int Verbosity { get; set; }
	
	static public void Show (Exception e)
	{
		List<Exception> exceptions = new List<Exception> ();
		bool error = false;

		CollectExceptions (e, exceptions);

		foreach (var ex in exceptions)
			error |= ShowInternal (ex);

		if (error)
			Environment.Exit (1);
	}

	static void CollectExceptions (Exception ex, List<Exception> exceptions)
	{
#if NET_4_0
		AggregateException ae = ex as AggregateException;

		if (ae != null) {
			foreach (var ie in ae.InnerExceptions)
				CollectExceptions (ie, exceptions);
		} else {
			exceptions.Add (ex);
		}
#else
		exceptions.Add (ex);
#endif
	}

	static bool ShowInternal (Exception e)
	{
		BindingException mte = (e as BindingException);
		bool error = true;

		if (mte != null) {
			error = mte.Error;
			Console.Out.WriteLine (mte.ToString ());
			
			if (Verbosity > 1) {
				Exception ie = e.InnerException;
				if (ie != null) {
					if (Verbosity > 3) {
						Console.Error.WriteLine ("--- inner exception");
						Console.Error.WriteLine (ie);
						Console.Error.WriteLine ("---");
					} else {
						Console.Error.WriteLine ("\t{0}", ie.Message);
					}
				}
			}
			
			if (Verbosity > 2)
				Console.Error.WriteLine (e.StackTrace);
		} else {
			Console.Out.WriteLine ("error BI0000: Unexpected error - Please file a bug report at http://bugzilla.xamarin.com");
			Console.Out.WriteLine (e.ToString ());
		}

		return error;
	}
}
