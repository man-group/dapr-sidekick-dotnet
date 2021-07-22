using System.IO;
#if !NETFRAMEWORK
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#endif

namespace Man.Dapr.Sidekick
{
    public static class TestResourceHelper
    {
        public static Stream GetResourceFileStream(string resourceFile)
        {
            var currentType = typeof(TestResourceHelper);
            var resource = $"Man.Dapr.Sidekick.Resources.{resourceFile}";
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

        public static string CompileTestSystemProcessExe()
        {
            var source = GetResourceFileText("ProcessProgram.cs");
            var filename = Path.GetTempFileName();
            if (DaprConstants.IsWindows)
            {
                filename = Path.ChangeExtension(filename, "exe");
            }

#if NETFRAMEWORK
            var provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp");
            var cp = new System.CodeDom.Compiler.CompilerParameters
            {
                GenerateExecutable = true,
                OutputAssembly = filename,
                GenerateInMemory = false
            };
            provider.CompileAssemblyFromSource(cp, source);
#else
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation
                .Create(Path.GetFileName(filename))
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                .AddReferences(
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")))
                .AddSyntaxTrees(syntaxTree);
            var result = compilation.Emit(filename);
#endif

            return filename;
        }

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
