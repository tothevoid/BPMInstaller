namespace BPMInstaller.UI.Desktop.Utilities
{
    public static class DateTimeUtilities
    {
        public static string SecondsToString(int seconds)
        {
            int minutes = seconds / 60;
            int leftSeconds = seconds - minutes * 60;
            return $"{minutes:D2}:{leftSeconds:D2}";
        }
    }
}
