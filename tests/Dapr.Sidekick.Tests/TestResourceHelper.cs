using System.IO;

namespace Dapr.Sidekick
{
    public static class TestResourceHelper
    {
        public static Stream GetResourceFileStream(string resourceFile)
        {
            var currentType = typeof(TestResourceHelper);
            var resource = $"Dapr.Sidekick.Resources.{resourceFile}";
            return currentType.Assembly.GetManifestResourceStream(resource);
        }

        public static byte[] GetResourceFileBytes(string resourceFile)
        {
            using var stream = GetResourceFileStream(resourceFile);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public static string GetResourceFileText(string resourceFile)
        {
            using var stream = GetResourceFileStream(resourceFile);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
#if NETFRAMEWORK

        public static string CompileTestSystemProcessExe()
        {
            var filename = Path.ChangeExtension(Path.GetTempFileName(), ".exe");
            var source = GetResourceFileText("ProcessProgram.cs");
            var provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp");
            var cp = new System.CodeDom.Compiler.CompilerParameters
            {
                GenerateExecutable = true,
                OutputAssembly = filename,
                GenerateInMemory = false
            };
            provider.CompileAssemblyFromSource(cp, source);

            return filename;
        }
#endif

        public static void DeleteTestProcess(string filename, int waitMilliseconds = 2000)
        {
            var loopCount = 0;
            var interval = 100;
            do
            {
                try
                {
                    if (!File.Exists(filename))
                    {
                        return;
                    }

                    File.Delete(filename);
                }
                catch
                {
                }

                loopCount++;
                System.Threading.Thread.Sleep(interval);
            }
            while (loopCount * interval < waitMilliseconds);
        }
    }
}
