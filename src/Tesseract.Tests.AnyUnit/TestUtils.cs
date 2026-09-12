using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tesseract.Tests
{
    public static class TestUtils
    {
        /// <summary>
        /// Normalise new line characters to unix (\n) so they are all the same.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string NormaliseNewLine(string text)
        {
            return text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");
        }

        public static void Cmd(string command, params object[] arguments)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            var argumentStr = String.Join(" ", arguments.Select(x => $"\"{x}\""));
            processInfo = new ProcessStartInfo(command, argumentStr);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            // Process.Start(ProcessStartInfo) is genuinely nullable -- it returns null if
            // no new process resource was started (e.g. reusing an existing process),
            // which can't happen here (always a fresh ProcessStartInfo), but the compiler
            // can't know that, and silently dereferencing a null here would otherwise be a
            // real NullReferenceException with no useful message.
            process = Process.Start(processInfo)
                ?? throw new InvalidOperationException($"Process.Start returned null for command '{command}'.");
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();
        }
    }
}
