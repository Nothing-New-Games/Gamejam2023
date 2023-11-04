using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using WatchDog;

namespace PigeonCarrier
{
#pragma warning disable IDE0063
#pragma warning disable IDE0052
#pragma warning disable IDE1006
#pragma warning disable CS0472

    public enum LogTypes
    {
        Error, Warning, Info, Debug, DiscordLog, CriticalError, Command, Other, None
    }


    /// <summary>
    /// Handles logging data from the operations of an application, organizing it in a useful way.
    /// <para>
    /// Organization will be via the ApplcationName and ObjectOwner.
    /// </para>
    /// </summary>
    public class Pigeon
    {
        #region Watchdog
        /// <summary>
        /// Checks if the message has a type, if none is provided, the default will be assigned.
        /// </summary>
        /// <param name="defaultType">Default type to use if none was provided.</param>
        /// <param name="currentArgs">The current arguments passed by the event.</param>
        public static EventArgObjects VerifyLogType(LogTypes defaultType, EventArgObjects currentArgs)
        {
            if (defaultType == null) defaultType = LogTypes.None;
            if (currentArgs.Message.MessageType == null)
                currentArgs.Message.SetMessageType(defaultType);

            return currentArgs;
        }
        /// <summary>
        /// <para><see langword="true"/> if a message is present.</para>
        /// <para><see langword="false"/> if a message is NOT present. Sends a warning log if one is not found.</para>
        /// </summary>
        public static bool IsMessagePresent(EventArgObjects currentArgs)
        {
            if (currentArgs.Message != null) return true;

            EventMessage message = new($"{currentArgs.CallerName} called without a message!", LogTypes.Warning);
            Watchdog.LogWarningCallback.Invoke(new(message));

            return false;
        }


        /// <summary>
        /// Method for when a critical error is thrown.
        /// </summary>
        /// <param name="args">string message, int line, string callerName, string filePath</param>
        private void OnCriticalErrorThrown(object caller, EventArgObjects args)
        {
            if (IsMessagePresent(args) == false) return;
            args = VerifyLogType(LogTypes.CriticalError, args);

            LogToConsole(caller, args);
        }
        private void OnWarningThrown(object caller, EventArgObjects args)
        {
            if (IsMessagePresent(args) == false) return;
            EventArgObjects verified = VerifyLogType(LogTypes.Warning, args);

            Log(caller, verified);
        }
        /// <summary>
        /// Utilizes default values to perform log actions.
        /// </summary>
        private void Log(object caller, EventArgObjects args)
        {
            if (IsMessagePresent(args) == false) return;
            args = VerifyLogType(LogTypes.None, args);

            LogToConsole(caller, args);
            LogToFile(caller, args);
        }
        /// <summary>
        /// Logs to the console and does not invoke any log related events.
        /// </summary>
        /// <param name="newMessage">Message to be logged.</param>
        private void LogToConsole(object caller, EventArgObjects args)
        {
            if (IsMessagePresent(args) == false) return;
            args = VerifyLogType(LogTypes.None, args);

            if (args.Message.Value.ToLower().Contains("consider removing the intent from your config."))
                return;

            if (args.Message.Value == "")
                return;

            //Log Type, line, file name
            string logInfo = GetPrintInfo((LogTypes)args.Message.MessageType, args.Message.LineCalled, args.Message.GetFileName);

            Console.ForegroundColor = args.Message.MessageType switch
            {
                LogTypes.Error => ConsoleColor.Red,
                LogTypes.Warning => ConsoleColor.Yellow,
                LogTypes.Info => ConsoleColor.Green,
                LogTypes.Debug => ConsoleColor.Cyan,
                LogTypes.DiscordLog => ConsoleColor.Blue,
                LogTypes.Other => ConsoleColor.DarkMagenta,
                LogTypes.CriticalError => ConsoleColor.DarkRed,
                LogTypes.None => ConsoleColor.White,
                _ => ConsoleColor.Gray,
            };

            Console.WriteLine($"{logInfo}{args.Message.Value}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Lock for file logs
        /// </summary>
        private readonly object _lock = new();
        /// <summary>
        /// Writes a file log without invoking any log related events.
        /// </summary>
        /// <param name="newMessage">Message to be logged.</param>
        public void LogToFile(object caller, EventArgObjects args)
        {
            if (IsMessagePresent(args) == false) return;
            args = VerifyLogType(LogTypes.None, args);

            lock (_lock)
            {
                string logInfo = GetPrintInfo((LogTypes)args.Message.MessageType, args.Message.LineCalled, args.Message.GetFileName);

                FileLogger.AddToLogQue($"{logInfo}{args.Message.Value}");
            }
        }
        #endregion

        #region Variables
        public static Pigeon Instance { get; private set; }

        /// <summary>
        /// The Application's name this belongs to.
        /// </summary>
        public string ApplicationName { get; private set; } = "Generic";

        /// <summary>
        /// Handles the thread for logging messages to a file
        /// </summary>
        private FileLogger _FileLogger { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logType">The log type that will be used by default.</param>
        public Pigeon(string applicationName = "", [CallerLineNumber] int line = 0, [CallerFilePath] string callerPath = "")
        {
            if (Instance != null)
            {
                EventMessage message = new("Warning, cannot have multiple pigeons at once! Airspace is too cramped!", LogTypes.Warning);
                Watchdog.LogWarningCallback.Invoke(new EventArgObjects(message));
            }

            if (applicationName != "")
                ApplicationName = applicationName;

            Watchdog.CriticalErrorCallback += OnCriticalErrorThrown;
            Watchdog.LogWarningCallback += OnWarningThrown;
            Watchdog.LogMessageCallback += Log;
            Watchdog.LogMessageConsoleAndFileOnlyCallback += Log;
            Watchdog.LogToConsoleOnlyCallback += LogToConsole;
            Watchdog.LogToFileOnlyCallback += LogToFile;


            _FileLogger = new(applicationName);

            Instance = this;
        }

        /// <summary>
        /// Creates the title info for writing a log.
        /// </summary>
        /// <param name="message">Event message</param>
        /// <param name="messageType">Message type</param>
        /// <param name="line">Line the event was triggered</param>
        /// <param name="caller">Caller of the triggering event</param>
        /// <returns>Title for writing a log.</returns>
        public static string GetPrintInfo(LogTypes messageType, int line, string caller)
        {
            string PrintInfo;

            //Add Application naeme, line, and date to print info
            if (caller == "")
                PrintInfo = $"{Pigeon.Instance.ApplicationName}\t[Line: {line}]\t[{DateTime.Now}]:\t";
            else //if we have an owner name, add it as well
                PrintInfo = $"{Pigeon.Instance.ApplicationName} ({caller})\t[Line: {line}]\t[{DateTime.Now}]:\t";

            //if we have a log type, add it
            if (messageType != LogTypes.None)
                PrintInfo = $"[{messageType}]\t{PrintInfo}";

            return PrintInfo;
        }




        /// <summary>
        /// Que system for writing logs to a file.
        /// </summary>
        private class FileLogger
        {
            /// <summary>
            /// Adds a message to the log que.
            /// </summary>
            /// <param name="message">Message that will be added to the que. Does NOT get edited.</param>
            public static void AddToLogQue(string message) =>
                instance.MessageQue.Add(message);

            /// <summary>
            /// Singleton instance of the FileLogger
            /// </summary>
            private static FileLogger instance;

            /// <summary>
            /// The que of all messages that needs to be written to the log.
            /// </summary>
            readonly List<string> MessageQue = new();
            /// <summary>
            /// Thread that handles writing to the log file.
            /// </summary>
            Thread LogThread { get; set; }
            /// <summary>
            /// The destination for the log
            /// </summary>
            string FileDirectory { get; set; }
            /// <summary>
            /// If true, the loop for file writing will not cease.
            /// </summary>
            private bool ContinueLoop = true;

            private readonly string directoryFormat = "{ExternalDirectory/}Logs/{ApplicationName}" + $"/{$"{DateTime.Now.DayOfWeek}-{DateTime.Now.Day}-{DateTime.Now:MMMM}-{DateTime.Now.Year}"}.txt";

            /// <summary>
            /// Creates a singleton instance of the file logger. This will listen to events from watchdog relating to logging to a file and write them accordingly.
            /// <para>Log file name will be the date the application was started on.</para>
            /// <para>Format used: {ExternalDirectory}/Logs/{ApplicationName}/{Date}</para>
            /// </summary>
            /// <param name="ApplicationName">This will be used to create a folder to track logs for this specific application.</param>
            /// <param name="ExternalDirectory">Use this to define where you want the logs to be saved to.</param>
            public FileLogger(string ApplicationName, string ExternalDirectory = "")
            {
                //Create the singleton instance of FileLogger if it is not already assigned
                instance ??= this;

                //Creates the 
                string finalFileDirectory = directoryFormat.Replace("{ExternalDirectory/}", ExternalDirectory).Replace("{ApplicationName}", ApplicationName);

                //Define the destination
                FileDirectory = finalFileDirectory;
                //Create the thread that will handle writing logs
                LogThread = new(ThreadMethod);
                //subscribe tot he critical error Watchdog so we know when to shut down
                Watchdog.CriticalErrorCallback += OnShutdownRequested;
                //Also needs to listen for shutdowns in general.
                Watchdog.RequestShutdownCallback += OnShutdownRequested;
                //Start the thread
                LogThread.Start();
            }

            /// <summary>
            /// Loops through until a shutdown is called and we run out of messages to write.
            /// </summary>
            private void ThreadMethod()
            {
                //Remove the file name so we can verify the directory path
                string folderDirectory = Path.GetDirectoryName(FileDirectory);
                string fileDirectory = Path.GetFullPath(FileDirectory);

                //Verify the log folder exists.
                if (!Directory.Exists(folderDirectory))
                    Directory.CreateDirectory(folderDirectory); //Create it if it cannot be found.

                //Verify the file exists
                if (!File.Exists(fileDirectory))
                    File.Create(fileDirectory).Close(); //Create it if it cannot be found.

                //Open a stream writer.
                using (StreamWriter writer = new(fileDirectory))
                {
                    //Write the start log
                    writer.WriteLine("New log started: " + DateTime.Now);

                    //Begin the loop
                    while ((ContinueLoop && MessageQue.Count > 0) || ContinueLoop)
                    {
                        //Verify we're not trying to write stuff that ins't there.
                        if (MessageQue.Count > 0)
                        {
                            writer.WriteLine(MessageQue[0]); //Write to the file
                            MessageQue.RemoveAt(0); //Remove the message
                        }
                    }
                    EventMessage shutdownMessage = new("Shutdown completed.", LogTypes.Info);
                    Watchdog.ShutdownCompletedCallback.Invoke(new(shutdownMessage));
                }
            }

            /// <summary>
            /// Shuts down the file logging loop once it runs out of stuff to write to the log file.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="args">string reasonForShutdown, int line, string callerName, string filePath</param>
            public void OnShutdownRequested(object sender, EventArgObjects args)
            {
                string ShutdownReason = args.Message.Value + $"Shutdown called by {args.Message.GetFileName} on line {args.Message.LineCalled}.";

                if (Pigeon.IsMessagePresent(args) == false) args = new(new EventMessage(ShutdownReason, LogTypes.Info), args.Args);
                args = Pigeon.VerifyLogType(LogTypes.None, args);

                AddToLogQue($"{Pigeon.GetPrintInfo((LogTypes)args.Message.MessageType, args.Message.LineCalled, args.Message.GetFileName)}{ShutdownReason}");

                ContinueLoop = false;
            }
        }
    }
}