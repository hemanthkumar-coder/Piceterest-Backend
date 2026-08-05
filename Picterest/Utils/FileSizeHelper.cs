namespace Picterest.Utils
{
    public static class FileSizeHelper
    {
        private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB" };

        public static string FormatFileSize(long bytes)
        {
            if (bytes < 0)
                throw new ArgumentOutOfRangeException(nameof(bytes));

            if (bytes == 0)
                return "0 B";

            double size = bytes;
            int suffixIndex = 0;

            while (size >= 1024 && suffixIndex < SizeSuffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:0.##} {SizeSuffixes[suffixIndex]}";
        }
    }
}
