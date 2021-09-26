
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StudentScheduler
{
    /// <summary>
    /// Utility for managing IO, general program behavior, and core error logging
    /// </summary>
    public static class UserInteraction
    {
        /// <summary>
        /// Program configuration mode that determines what the target purpose of the program is
        /// </summary>
        public enum ProgramMode
        {
            Normal,//personal version for VDI Use
            LimitedToIO,//only opening function
            Beta,//Has annoying warnings that are slowly being improved overtime
        }
        /// <summary>
        /// The mode the executable is hard programmed to
        /// </summary>
        public static readonly ProgramMode Mode = ProgramMode.Normal;
        /// <summary>
        /// Allows a user to pose as an admin for alpha functionality
        /// </summary>
        public static bool AdminOveride = false;
        /// <summary>
        /// The path at which to write warning files
        /// </summary>
        public static string WarningFilePath { get; private set; }
        /// <summary>
        /// Captures the moment the process started for a uniquish stamp for the logging file
        /// </summary>
        private static readonly DateTime ProcessstartTime = DateTime.Now;
        /// <summary>
        /// The version ID to identify the current executable
        /// </summary>
        public static string VersionId => string.Format("{0}.{1}{2}", CommitVersion, SmallUpdateVersion, Mode.ToString());
        /// <summary>
        /// Determines if the console exists in the current context (and therefore can be cleared)
        /// </summary>
        public static bool ConsoleExists { get; set; } = true;
        /// <summary>
        /// The major version number - losely based around commits but not really
        /// </summary>
        private const string CommitVersion = "306";
        /// <summary>
        /// A version update number for when no commit is being performed
        /// </summary>
        private const string SmallUpdateVersion = "0";
        /// <summary>
        /// log files at local run location
        /// </summary>
        internal const string RunTimeLogFilePath = @"RunTimeLogFiles\";
        /// <summary>
        /// The name of the default admin
        /// </summary>
        private const string Admin = "ebrunton";
        /// <summary>
        /// The main folder repo for the program to work out of
        /// </summary>
        public static string RootFilePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\LabData\";
        /// <summary>
        /// The main folder repo for the program to work out of
        /// </summary>
        public static string OriginalRoot { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\LabData\";

        /// <summary>
        /// The fallback session accessible to any child sessions
        /// </summary>
        public static SessionDetails RuntimeSession { get; } = new SessionDetails();
        /// <summary>
        /// The unique identifier for the current session
        /// </summary>
        /// <returns>The session id</returns>
        public static string GetSessionId()
        {
            return "User" + "_" + VersionId + "_" + ProcessstartTime.ToString("MM") + "_" + ProcessstartTime.ToString("dd") + "_" + ProcessstartTime.ToString("HH") + ProcessstartTime.ToString("mm") + ProcessstartTime.ToString("ss") + ProcessstartTime.Millisecond.ToString() + "_" + Environment.MachineName;
        }
        /// <summary>
        /// Checks if an put indicates a user wants to quit
        /// </summary>
        /// <param name="input">A string from the user</param>
        /// <returns>True if input string indicates a desire to quit</returns>
        public static bool UserWantsToQuit(string input)
        {
            string lowInput = input.ToLower();
            return lowInput == "quit" || lowInput == "opt" || lowInput == "q";
        }

        /// <summary>
        /// Creates neceesary directories and verifies dependent dlls exist
        /// </summary>
        public static void Initialize()
        {
            WarningFilePath = RootFilePath + @"Warnings\";
            Directory.CreateDirectory(RootFilePath);
            Directory.CreateDirectory(WarningFilePath);
            Directory.CreateDirectory(RunTimeLogFilePath);
        }
        /// <summary>
        /// Combines multiple arguments from an array and returns them as a single striung ready to run in the command prompt 
        /// </summary>
        /// <param name="items">The list of command arguments</param>
        /// <param name="dontWrap">Force the commands not to be wrapped in quotes (will not wrap in quotes if already wrapper in quotes event if set to false)</param>
        /// <returns>The single command string</returns>
        public static string CreateMultiArgCommand(IEnumerable<string> items, bool dontWrap)
        {
            string masterCommand = "";
            foreach (string item in items)
            {
                if (item.StartsWith("\"") || dontWrap)
                {
                    masterCommand += item + " ";
                }
                else
                {
                    masterCommand += "\"" + item + "\" ";
                }
            }
            return masterCommand;
        }

        public enum YesNo
        {
            No = 0,
            Yes = 1,
            EmptyByUser = 2
        }

        /// <summary>
        /// Writes the original log, sets the title and handles the input arguments
        /// </summary>
        /// <param name="args"></param>
        public static void Initialize(string[] args)
        {
            if (ConsoleExists)
            {
                Console.Title = "EDB Utility";
            }
            Initialize();
            RuntimeSession.SystemLog.Add(GetRuntimeInformation(args));
        }
        public enum Exit
        {
            Expected = 0,
            Unexpected = 1
        }

        /// <summary>
        /// Identifies if additional debugging information should be shown
        /// </summary>
        /// <returns>True if belongs to the admin</returns>
        public static bool IsAdminComputer()
        {
            return AdminOveride || RootFilePath.Contains(Admin);
        }

        /// <summary>
        /// Indicates if resources files should be copied to the server
        /// </summary>
        /// <returns>True if they should be</returns>
        public static bool ShouldCopyResourceFilesToNetwork()
        {
            return Environment.CurrentDirectory.EndsWith("Debug") || Environment.CurrentDirectory.EndsWith("Release");
        }


        /// <summary>
        /// Buffers provided parameters and creates a system log
        /// </summary>
        /// <param name="args">Args from main of the primary program</param>
        /// <returns>An output string to be written to a system log</returns>
        private static string GetRuntimeInformation(string[] args)
        {
            string runtimeParams = ReadLaunchInfo(args);
            string user = "User";
            GetEnvironmentConstants(out string machineName, out string runLocation, out string osDetail, out string uptime);
            string startTime = ProcessstartTime.ToString("G");
            string programVersion = VersionId;
            ExecutingPathInfo(out string executablePath, out string workingDirectory);
            StringBuilder stringBuilder = null;
            AddToOutString(ref stringBuilder, "User", user);
            AddToOutString(ref stringBuilder, "Machine Name", machineName);
            AddToOutString(ref stringBuilder, "Runtime Params", runtimeParams);
            AddToOutString(ref stringBuilder, "Program Version", programVersion);
            AddToOutString(ref stringBuilder, "OS", osDetail);
            AddToOutString(ref stringBuilder, "Machine Uptime", uptime, "ms");
            AddToOutString(ref stringBuilder, "Start Time", startTime);
            AddToOutString(ref stringBuilder, "Run Location", runLocation);
            AddToOutString(ref stringBuilder, "Excutable Path", executablePath);
            AddToOutString(ref stringBuilder, "Working Directory", workingDirectory);
            return stringBuilder.ToString();
        }

        private static string ReadLaunchInfo(string[] args)
        {
            string runtimeParams = "None";
            if (args != null && args.Length > 0)
            {
                runtimeParams = string.Join("|", args);
                RuntimeSession.BufferedResponseInput.AddRange(args.Where(a => !string.IsNullOrEmpty(a)));
            }
            return runtimeParams;
        }

        public static bool ExecutingPathInfo(out string executablePath, out string workingDirectory)
        {
            if (ConsoleExists)
            {
                ExecutingPAthInfoUnsafe(out executablePath, out workingDirectory);
                return true;
            }
            else
            {
                executablePath = "RunningWebHost";
                workingDirectory = "RunningWebHost";
                return false;
            }
        }
        private static void ExecutingPAthInfoUnsafe(out string executablePath, out string workingDirectory)
        {
            executablePath = "";// Application.ExecutablePath;
            workingDirectory = "";//Application.StartupPath;
        }

        private static void GetEnvironmentConstants(out string machineName, out string runLocation, out string osDetail, out string uptime)
        {
            machineName = Environment.MachineName;
            runLocation = Environment.CurrentDirectory;
            osDetail = Environment.OSVersion.VersionString;
            uptime = Environment.TickCount.ToString();
        }

        private static StringBuilder AddToOutString(ref StringBuilder current, string title, string variable, string unit = "")
        {
            if (current == null)
            {
                current = new StringBuilder();
            }
            else
            {
                current.Append("\n");
            }
            current.Append(title);
            current.Append(": ");
            current.Append(variable);
            current.Append(" ");
            current.Append(unit);
            return current;
        }
    }
}
