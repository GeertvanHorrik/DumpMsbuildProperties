using System.Diagnostics;
using System.IO;
using Catel.Reflection;

namespace DumpMsBuildProperties
{
    using System;
    using Microsoft.Build.Evaluation;

    class Program
    {
        private static FileStream LogFile;
        private static StreamWriter LogFileWriter;

        static void Main(string[] args)
        {
            var assemblyDirectory = typeof (Program).Assembly.GetDirectory();

            LogFile = new FileStream(Path.Combine(assemblyDirectory, "output.log"), FileMode.Create);
            LogFileWriter = new StreamWriter(LogFile);

            using (LogFile)
            {
                using (LogFileWriter)
                {
                    LogFileWriter.AutoFlush = true;

                    var solutionFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.sln",
                        SearchOption.AllDirectories);
                    foreach (var solutionFile in solutionFiles)
                    {
                        Write("Solution file '{0}'", solutionFile);
                        Write("===========================================================");

                        var projects = ProjectHelper.GetProjects(solutionFile, "release");
                        foreach (var project in projects)
                        {
                            DumpMsBuildProperties(project);
                        }

                        Write("");
                    }

                    if (solutionFiles.Length == 0)
                    {
                        Write("No solution files found in '{0}'", Environment.CurrentDirectory);
                    }

#if DEBUG
                    Write("");
                    Write("Press any key to continue");

                    Console.ReadKey();
#endif
                }
            }
        }

        static void DumpMsBuildProperties(Project project)
        {
            Write("");
            Write("Properties for project '{0}'", project.FullPath);
            Write("-----------------------------------------------------------");

            foreach (var property in project.Properties)
            {
                Write("  {0} => {1} ({2})", property.Name, property.EvaluatedValue, property.UnevaluatedValue);
            }

            Write("");
            Write("");
        }

        static void Write(string format, params object[] args)
        {
            var line = string.Format(format, args);

            Debug.WriteLine(line);
            Console.WriteLine(line);

            LogFileWriter.WriteLine(line);
        }
    }
}
