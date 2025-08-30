using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace TutorBot.Test.DevOps
{
    internal static class DevOpsHelper
    {
        public static string ProjectPath { get; } = GetPath();
        public static string DevOpsPath { get; } = Path.Combine(ProjectPath, "DevOps");

        public static string AppSettings_test_json_path { get; } = CheckFile(DevOpsPath, "appsettings.test.json");
        public static string AppSettings_json_path { get; } = CheckFile(ProjectPath, "..", "TutorBot.App", "appsettings.json");
        public static string AppSettings_private_json_path { get; } = CheckFile(ProjectPath, "..", "TutorBot.App", "appsettings.private.json");

        private static string CheckFile(params string[] parts)
        {
            string fullFileName = Path.Combine([DevOpsPath, .. parts]);

            if (!File.Exists(fullFileName))
                throw new FileNotFoundException(fullFileName);

            return fullFileName;
        }

        private static string GetPath()
        {
            string? rootPath = AppContext.BaseDirectory;
            string projectName = typeof(DevOpsHelper).Assembly.GetName().Name! + ".csproj";

            while (true)
            {
                if (string.IsNullOrEmpty(rootPath))
                    throw new FileNotFoundException(Path.Combine(AppContext.BaseDirectory, projectName));

                string project = Path.Combine(rootPath, projectName);
                if (File.Exists(project))
                    return rootPath;
                rootPath = Path.GetDirectoryName(rootPath);
            }
        }

    }
}
