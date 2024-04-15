namespace BPMInstaller.UI.Desktop.Utilities
{
    public static class DateTimeUtilities
    {
        public static string SecondsToString(int seconds)
        {
            int minutes = seconds / 60;
            int leftSeconds = minutes * 60 - seconds;
            return $"{minutes:D2}:{leftSeconds:D2}";
        }
    }
}
