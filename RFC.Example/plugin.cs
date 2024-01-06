using EnvDTE;
using LinqPad1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFC.Example
{
    public class GitPlugin
    {
        public static void Run(string commitm, DTE dte)
        {

            string solutionPath = Path.GetDirectoryName(dte.Solution.FullName);
            var _solutionDirectory = FindGitDirectory(solutionPath);

            ExecuteGitCommand("add .", _solutionDirectory);
            ExecuteGitCommand($"commit -m \"{commitm}\"", _solutionDirectory);
            ExecuteGitCommand("pull", _solutionDirectory);
            ExecuteGitCommand("push", _solutionDirectory);
        }

        static string FindGitDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                return null;
            }

            string gitPath = Path.Combine(directoryPath, ".git");
            if (Directory.Exists(gitPath))
            {
                return directoryPath;
            }

            return FindGitDirectory(Directory.GetParent(directoryPath)?.FullName);
        }

        static void ExecuteGitCommand(string command, string folder)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = folder,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }
    }
}
