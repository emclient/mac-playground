namespace MailClient.Common.UI
{
    public static class CommonUIUtils
    {
        static string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        private static bool designMode;
        private static bool designModeCached = false;
        public static bool DesignMode
        {
            get
            {
                if (!designModeCached)
                {
                    //A typical YouDontWantToSeeThis hack.
                    designMode = processName.Equals("devenv") || processName.Equals("VCSExpress") || processName.Contains("eM Client Localizator");
                    designModeCached = true;
                }
                return designMode;
            }
        }
    }
}
