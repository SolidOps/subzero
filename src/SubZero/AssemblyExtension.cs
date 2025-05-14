using System.IO;
using System.Reflection;

namespace SolidOps.SubZero
{
    public static class AssemblyExtension
    {
        public static string ReadResource(this Assembly ass, string resourceName)
        {
            string returnedValue = string.Empty;

            Stream stream = null;
            try
            {
                stream = ass.GetManifestResourceStream(resourceName);

                using (TextReader reader = new StreamReader(stream))
                {
                    stream = null;
                    returnedValue = reader.ReadToEnd();
                }
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }
            return returnedValue;
        }
    }
}
