namespace MacApi.PrintCore
{
	//https://github.com/phracker/MacOSX-SDKs/blob/master/MacOSX10.6.sdk/System/Library/Frameworks/ApplicationServices.framework/Versions/A/Frameworks/PrintCore.framework/Versions/A/Headers/PMPrintSettingsKeys.h

	public static class PMPrintSettingsKeys
	{
		public const string kPMCopies = "com.apple.print.PrintSettings.PMCopies";
		public const string kPMCopyCollate = "com.apple.print.PrintSettings.PMCopyCollate";
		public const string kPMOutputOrder = "OutputOrder";
		public const string kPMPageSet = "page-set";
		public const string kPMMirror = "mirror";
		public const string kPMPrintSelectionOnly = "com.apple.print.PrintSettings.PMPrintSelectionOnly";
		public const string kPMBorder = "com.apple.print.PrintSettings.PMBorder";
		public const string kPMBorderType = "com.apple.print.PrintSettings.PMBorderType";

		public const string kPMLayoutNUp = "com.apple.print.PrintSettings.PMLayoutNUp";
		public const string kPMLayoutRows = "com.apple.print.PrintSettings.PMLayoutRows";
		public const string kPMLayoutColumns = "com.apple.print.PrintSettings.PMLayoutColumns";
		public const string kPMLayoutDirection = "com.apple.print.PrintSettings.PMLayoutDirection";
		public const string kPMLayoutTileOrientation = "com.apple.print.PrintSettings.PMLayoutTileOrientation";
		public const string kPMJobState = "com.apple.print.PrintSettings.PMJobState";
		public const string kPMJobHoldUntilTime = "com.apple.print.PrintSettings.PMJobHoldUntilTime";
		public const string kPMJobPriority = "com.apple.print.PrintSettings.PMJobPriority";
		public const string kPMDuplexing = "com.apple.print.PrintSettings.PMDuplexing";
		public const string kPMColorSyncProfileID = "com.apple.print.PrintSettings.PMColorSyncProfileID";
		public const string kPMPrimaryPaperFeed = "com.apple.print.PrintSettings.PMPrimaryPaperFeed";
		public const string kPMSecondaryPaperFeed = "com.apple.print.PrintSettings.PMSecondaryPaperFeed";
		public const string kPMPSErrorHandler = "com.apple.print.PrintSettings.PMPSErrorHandler";
		public const string kPMPSTraySwitch = "com.apple.print.PrintSettings.PMPSTraySwitch";
		public const string kPMTotalBeginPages = "com.apple.print.PrintSettings.PMTotalBeginPages";
		public const string kPMTotalSidesImaged = "com.apple.print.PrintSettings.PMTotalSidesImaged";
		public const string kPMFaxNumber = "phone";
		public const string kPMFaxTo = "faxTo";
		public const string kPMFaxPrefix = "faxPrefix";
		public const string kPMFaxSubject = "faxSubject";
		public const string kPMFaxCoverSheet = "faxCoverSheet";
		public const string kPMFaxCoverSheetMessage = "faxCoverSheetMessage";
		public const string kPMFaxToneDialing = "faxToneDialing";
		public const string kPMFaxUseSound = "faxUseSound";
		public const string kPMFaxWaitForDialTone = "faxWaitForDialTone";
		public const string kPMFaxToLabel = "faxToLabel";
		public const string kPMFaxFromLabel = "faxFromLabel";
		public const string kPMFaxDateLabel = "faxDateLabel";
		public const string kPMFaxSubjectLabel = "faxSubjectLabel";
		public const string kPMFaxSheetsLabel = "faxSheetsLabel";

		// Coverpagfe related
		public const string kPMCoverPage = "com.apple.print.PrintSettings.PMCoverPage";

		const int kPMCoverPageNone = 1;
		const int kPMCoverPageBefore = 2; // Print a cover page before printing the document.
		const int kPMCoverPageAfter = 3; // Print a cover page after printing the document.

		// If the kPMDuplexingKey is not in a print settings then kPMDuplexDefault should be assumed.
		const int kPMCoverPageDefault = kPMCoverPageNone;

		public const string kPMCoverPageSource = "com.apple.print.PrintSettings.PMCoverPageSource";
		public const string kPMDestinationPrinterID = "DestinationPrinterID";
		public const string kPMInlineWorkflow = "inlineWorkflow";
		public const string kPMPageToPaperMappingType = "com.apple.print.PageToPaperMappingType";
		public const string kPMPageToPaperMediaName = "com.apple.print.PageToPaperMappingMediaName";
		public const string kPMPageToPaperMappingAllowScalingUp = "com.apple.print.PageToPaperMappingAllowScalingUp";

		// The kPMCustomProfilePathKey key stores a CFString that corresponds to a custom profile setting for a given printer.
		public const string kPMCustomProfilePath = "PMCustomProfilePath";

		// Page to Paper Mapping Types
		const int kPMPageToPaperMappingNone = 1;
		const int kPMPageToPaperMappingScaleToFit = 2;
		//PMPageToPaperMappingType;

		// Possible values for the kPMColorMatchingModeKey
		public const string kPMVendorColorMatching = "AP_VendorColorMatching";
		public const string kPMApplicationColorMatching = "AP_ApplicationColorMatching";
		public const string kPMColorMatchingMode = "AP_ColorMatchingMode";

		// Begin: Use of these keys is discouraged. Use PMSessionSetDestination, PMSessionGetDestinationType, PMSessionCopyDestinationFormat, and PMSessionCopyDestinationLocation instead
		public const string kPMDestinationType = "com.apple.print.PrintSettings.PMDestinationType";
		public const string kPMOutputFilename = "com.apple.print.PrintSettings.PMOutputFilename";
		// End: Use of these keys is discouraged. Use PMSessionSetDestination, PMSessionGetDestinationType, PMSessionCopyDestinationFormat, and PMSessionCopyDestinationLocation instead

	}
}
