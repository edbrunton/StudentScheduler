using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using static StudentScheduler.UserInteraction;

namespace StudentScheduler
{
	public class SessionDetails
	{
		public InterlockingBool SharedMonitor { get; } = new InterlockingBool();
		/// <summary>
		/// Locking var that forces ordered responses and non interrupted console writes
		/// </summary>
		public InterlockingBool ConsoleBeingUsed = new InterlockingBool();
		/// <summary>
		/// List of strings from the system args of the main program to use as user inputs (allows for automated runs + Unit testing_
		/// </summary>
		public List<string> BufferedResponseInput { get; } = new List<string>();
		/// <summary>
		/// List of strings from the system args of the main program to use as user inputs (allows for automated runs + Unit testing_
		/// </summary>
		public List<string> BufferedResponseOutput { get; } = new List<string>();
		/// <summary>
		/// A list of full texts for processes that lack the ability to launch files
		/// </summary>
		public List<string> BufferedFiles { get; } = new List<string>();
		/// <summary>
		/// List of strings to write to the log file (re-writes entire file each time)
		/// </summary>
		internal List<string> SystemLog { get; } = new List<string>();
		/// <summary>
		/// Pulls a blocking lock to prevent the programming from moving forward without user input
		/// </summary>
		public InterlockingBool WaitingForUserVal { get; } = new InterlockingBool();

		public void StartProcess(string fileName)
		{
			StartProcess("", fileName);
		}

		public void StartProcess(string processName, string command)
		{
			StartProcess(processName, new List<string> { command }, true);
		}



        /// <summary>
        /// Runs a process and includes a list of arguments
        /// </summary>
        /// <param name="processName">The name of the process (probably has an .exe in it)</param>
        /// <param name="items">Each command</param>
        public void StartProcess(string processName, IEnumerable<string> items, bool dontWrap)
		{
			string masterCommand = CreateMultiArgCommand(items, dontWrap);
			try
			{
				if (!ConsoleExists)
				{
					if (string.IsNullOrEmpty(processName))
					{
						lock (BufferedFiles)
						{
							BufferedFiles.Add(File.ReadAllText(masterCommand));
						}
					}
					else
					{
						WriteLine(string.Format("This is a website so the following was not run: {0}", processName));
					}
				}
				else
				{
					if (string.IsNullOrEmpty(processName))
					{
						Process.Start(masterCommand);
					}
					else
					{
						Process.Start(processName, masterCommand);
					}
				}
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("{0} would not be run with arguments {1}", processName, masterCommand), e);
			}
		}
		/// <summary>
		/// Adds a line to the log. Only use if you know for sure that a log will be written
		/// </summary>
		/// <param name="newInfo">The info for the program to write</param>
		public void AddToLog(string newInfo)
		{
			SystemLog.Add(string.Format("Program Log: {0}", newInfo));
		}
		/// <summary>
		/// Adds a line to the log and then immedietly writes a log
		/// </summary>
		/// <param name="newInfo">The info for the program to write</param>
		public void AddToLogAndWrite(string newInfo)
		{
			try
			{
				SystemLog.Add(string.Format("Program Log: {0}", newInfo));
				WriteSystemLog();
			}
			catch
			{
				//should never crash here
			}
		}
		/// <summary>
		/// Writes a line, allowing for the color to be overriden (resets the colors once the line is written)
		/// </summary>
		/// <param name="line">The line to write</param>
		/// <param name="consoleColor">The console color</param>
		public void WriteLine(string line, ConsoleColor consoleColor)
		{
			lock (ConsoleBeingUsed)
			{
				lock (BufferedResponseInput)
				{
					ConsoleColor previousColor = Console.ForegroundColor;
					Console.ForegroundColor = consoleColor;
					WriteLine(line);
					Console.ForegroundColor = previousColor;
				}
			}

		}
		/// <summary>
		/// Writes a line in a multithreaded friendly way
		/// </summary>
		/// <param name="line">The line to write</param>
		public void WriteLine(string line = "")
		{
			WriteLine(line as object);
		}
		/// <summary>
		/// Writes a line (without a return) in a multithreaded friendly way
		/// </summary>
		/// <param name="line">The line to write</param>
		public void Write(string line = "")
		{
			lock (ConsoleBeingUsed)
			{
				lock (BufferedResponseInput)
				{
					if (!ConsoleExists)
					{
						lock (BufferedResponseOutput)
						{
							BufferedResponseOutput.Add(line);
						}
					}
					else
					{
						Console.Write(line);
					}
				}
			}

		}
		/// <summary>
		/// Writes a line
		/// </summary>
		/// <param name="value">Some object value</param>
		public void WriteLine(object value)
		{
			if (!ConsoleExists)
			{
				lock (BufferedResponseOutput)
				{
					BufferedResponseOutput.Add(value.ToString());
				}
			}
			else
			{
				lock (ConsoleBeingUsed)
				{
					lock (BufferedResponseInput)
					{
						Console.WriteLine(value);
					}
				}
			}
		}
		/// <summary>
		/// Writes the exit statement for the program
		/// </summary>
		/// <param name="e">The exit enum</param>
		public void WriteExitLog(Exit e)
		{
			if (e == Exit.Expected)
			{
				SystemLog.Add(string.Format("Expected exit at {0}", DateTime.Now.ToString("O")));
			}
			else
			{
				SystemLog.Add(string.Format("Unexpectedly exited at {0}", DateTime.Now.ToString("O")));
			}
			WriteSystemLog();

		}
		/// <summary>
		/// Reads a line from the console or buffered input (from launch)
		/// </summary>
		/// <param name="trim">If leading or trailing whitespace should be removed</param>
		/// <returns>The line that was read/returns>
		public string ReadLine(bool trim)
		{
			string input;
			if (!ConsoleExists)
			{
				bool isEmpty = true;
				while (isEmpty)
				{
					lock (BufferedResponseInput)
					{
						if (BufferedResponseInput.Count > 0)
						{
							WaitingForUserVal.SetValue(false);
							isEmpty = false;
						}
						else
						{
							WaitingForUserVal.SetValue(true);
						}
					}
					lock (SharedMonitor)
					{
						Monitor.Pulse(SharedMonitor);
					}
					if (isEmpty)
					{
						lock (SharedMonitor)
						{
							Monitor.Wait(SharedMonitor);
						}
					}
				}
			}
			lock (ConsoleBeingUsed)
			{
				lock (BufferedResponseInput)
				{
					bool wasBuffered = false;
					if (BufferedResponseInput.Count > 0)
					{
						wasBuffered = true;
						input = BufferedResponseInput[0];
						BufferedResponseInput.RemoveAt(0);
					}
					else
					{
						input = Console.ReadLine();
					}
					if (trim)
					{
						input = input.Trim();
					}
					if (wasBuffered)
					{
						SystemLog.Add(string.Format("{0} input at {1}: {2}", "System Input from buffer", DateTime.Now, input));
					}
					else
					{
						SystemLog.Add(string.Format("{0} input at {1}: {2}", "User", DateTime.Now, input));
					}
				}
			}
			return input;
		}
		/// <summary>
		/// Reads a line from the console or buffered input (from launch)
		/// </summary>
		/// <returns><The line that was read/returns>
		public string ReadLine()
		{
			return ReadLine(true);
		}
		/// <summary>
		/// Prompts the user and then reads the line (has the benefit of holding the console lock so no other text will interfer)
		/// </summary>
		/// <param name="prompt">Prompt to display the user</param>
		/// <param name="trim">Should the line that is read be trimmed of whitespace?</param>
		/// <returns>Whatever the user entered</returns>
		public string ReadLine(string prompt, bool trim)
		{
			WriteLine(prompt);
			return ReadLine(trim);
		}

		public YesNo GetYesNo(string value)
		{
			lock (ConsoleBeingUsed)
			{
				WriteLine(value);
				return GetYesNo();
			}
		}
		public YesNo GetYesNo()
		{
			if (ConsoleExists)
			{
				Write("Yes/No: ");
				string r = ReadLine();
				return TryGetYesNoFromSTring(r);
			}
			else
			{
				lock (ConsoleBeingUsed)
				{
					Write("Yes/No: ");
					string r = ReadLine();
					return TryGetYesNoFromSTring(r);
				}
			}

		}

		private YesNo TryGetYesNoFromSTring(string r)
		{
			try
			{
				if (r.Length == 0)
				{
					return YesNo.No;
				}
				if (r.Substring(0, 1).ToUpper() == "Y")
				{
					return YesNo.Yes;
				}
				return YesNo.No;
			}
			catch
			{
				return YesNo.No;
			}
		}

		/// <summary>
		/// Writes a runtime diagnostic file
		/// </summary>
		/// <param name="name">The diagnositc file name</param>
		/// <param name="content">A list of strings to include in the file (overwrites existing content)</param>
		public void WriteImmediateLogFile(string name, IEnumerable<string> content)
		{
			try
			{
				File.WriteAllLines(RunTimeLogFilePath + name, content);
			}
			catch
			{
				//might not have permission
			}
		}
		/// <summary>
		/// Writes the complete system log
		/// </summary>
		public void WriteSystemLog()
		{
			try
			{
				SystemLog.Add(string.Format("Wrote log at {0}", DateTime.Now));
				WriteImmediateLogFile(GetSessionId() + ".txt", SystemLog);
			}
			catch
			{
				//don't do anything. Just shouldn't crash
			}
		}
		/// <summary>
		/// Writes a runtime diagnostic file
		/// </summary>
		/// <param name="name">The diagnositc file name</param>
		/// <param name="content">A array of strings to include in the file (overwrites existing content)</param>
		public void WriteImmediateLogFile(string name, string[] content)
		{
			File.WriteAllLines(RunTimeLogFilePath + name, content);
		}
		/// <summary>
		/// Gets a valid enum from the user
		/// </summary>
		/// <typeparam name="AnEnum">The enum type</typeparam>
		/// <param name="writeLine">A prompting line</param>
		/// <returns>The return value</returns>
		public AnEnum GetValidOption<AnEnum>(string writeLine) where AnEnum : struct, Enum
		{
			return GetValidOption<AnEnum>(null, writeLine);
		}
		public AnEnum GetValidOption<AnEnum>(Action<SessionDetails> optionWriteOut) where AnEnum : struct, Enum
		{
			return GetValidOption<AnEnum>(optionWriteOut, null);
		}
		/// <summary>
		/// Runs a command as admin with an inlined window
		/// </summary>
		/// <param name="commands">The list of commands to run in a single process</param>
		public void RunAsAdmin(IEnumerable<string> commands, string workingDirectory)
		{
			RunAsAdmin(commands, workingDirectory, Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe");//command prompt's path
		}

		/// <summary>
		/// Runs a command as admin with an inlined window
		/// </summary>
		/// <param name="commands">The list of commands to run in a single process</param>
		public void RunAsAdmin(IEnumerable<string> commands, string workingDirectory, string executable)
		{
			string arguments = "/c " + string.Join("&", commands);
			if (!ConsoleExists)
			{
				WriteLine(string.Format("This is a website so the following was not run: {0} {1}", arguments, executable));
				return;
			}
			try
			{
				ProcessStartInfo proc1 = new ProcessStartInfo
				{
					UseShellExecute = false,
					WorkingDirectory = workingDirectory,
					FileName = executable,
					Verb = "runas",//elevate to admin
					Arguments = arguments,//execute multiple commands and close when done
					WindowStyle = ProcessWindowStyle.Normal
				};
				Process process = Process.Start(proc1);
				process.WaitForExit();
				process.Close();
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Failed to run {0} at {1} with commands {2}", executable, workingDirectory, arguments), e);
			}
		}

		public void RunAsAdminWithoutCmd(string executable, string command)
		{
			if (!ConsoleExists)
			{
				return;
			}
			ProcessStartInfo proc1 = new ProcessStartInfo
			{
				UseShellExecute = true,
				FileName = executable,//command prompt's path
				Verb = "runas",//elevate to admin
				Arguments = command,
				WindowStyle = ProcessWindowStyle.Normal//hide from user view
			};
			Process.Start(proc1);
		}
		/// <summary>
		/// Runs a command as admin
		/// </summary>
		/// <param name="commands">The list of commands to run in a single process</param>
		public void RunAsAdminNewCmdWindow(IEnumerable<string> commands)
		{
			if (!ConsoleExists)
			{
				WriteLine(string.Format("This is a website so the following was not run: {0}", string.Join("&", commands)));
				return;
			}
			string arguments = "/c " + string.Join("&", commands);//execute multiple commands and close when done
			ProcessStartInfo proc1 = new ProcessStartInfo
			{
				UseShellExecute = true,
				WorkingDirectory = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32",
				FileName = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe",//command prompt's path
				Verb = "runas",//elevate to admin
				Arguments = arguments,
				WindowStyle = ProcessWindowStyle.Hidden //hide from user view
			};
			Process.Start(proc1);
		}
		/// <summary>
		/// Writes a runtime diagnostic file
		/// </summary>
		/// <param name="name">The diagnositc file name</param>
		/// <param name="content">A string to include in the file (overwrites existing content)</param>
		public void WriteImmediateLogFile(string name, string content)
		{
			File.WriteAllText(RunTimeLogFilePath + name, content);
		}
		/// <summary>
		/// Stores previous enum selections indexed by the enum type name
		/// </summary>
		private Dictionary<string, Enum> PreviousSelections { get; } = new Dictionary<string, Enum>();
		private AnEnum GetValidOption<AnEnum>(Action<SessionDetails> optionWriteOut, string writeLine) where AnEnum : struct, Enum
		{
			bool validInput = false;
			AnEnum anEnum;
			while (!validInput)
			{
				if (!string.IsNullOrEmpty(writeLine))
				{
					WriteLine(writeLine);
				}
				else if (optionWriteOut != null)
				{
					optionWriteOut(this);
				}
				else
				{
					WriteAllOptions<AnEnum>();
				}
				string input = ReadLine();
				if (input == "?")
				{
					WriteAllOptions<AnEnum>();
					continue;
				}
				string typeName = typeof(AnEnum).FullName;
				if (input == "=" && PreviousSelections.ContainsKey(typeName))
				{
					anEnum = (AnEnum)PreviousSelections[typeName];
					validInput = true;
				}
				else
				{
					validInput = Enum.TryParse(input, out anEnum) && Enum.IsDefined(typeof(AnEnum), anEnum);
				}
				if (ConsoleExists)
				{
					Console.Clear();
				}
				if (!validInput)
				{
					WriteLine(string.Format("Nah, {0}'s not valid", input));
				}
				else
				{
					if (!PreviousSelections.ContainsKey(typeName))
					{
						PreviousSelections.Add(typeName, anEnum);
					}
					else
					{
						PreviousSelections[typeName] = anEnum;
					}
					return anEnum;
				}
			}
			//fallback
			Enum.TryParse("0", out anEnum);
			return anEnum;
		}

		public void WriteAllOptions<AnEnum>() where AnEnum : struct, Enum
		{
			foreach (AnEnum an in Enum.GetValues(typeof(AnEnum)).Cast<AnEnum>())
			{
				WriteLine(string.Format("{0}: {1}", an.ToString(), an.ToString("d")));
			}
		}
	}
}
