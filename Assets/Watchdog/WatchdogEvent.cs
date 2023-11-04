#region Using, Namespace, and Warning Disables
using PigeonCarrier;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace WatchDog
{
#pragma warning disable IDE1006
#pragma warning disable CS8632
    #endregion

    public class WatchdogEvent
    {
        private event EventHandler<EventArgObjects>? _ManagedEvent;

        /// <summary>
        /// Subscribe the method to the desired event.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static WatchdogEvent operator +(WatchdogEvent events, EventHandler<EventArgObjects> handler)
        {
            events._ManagedEvent += handler;
            return events;
        }

        /// <summary>
        ///  Triggers the execution of all the subscribed event subscribers associated with this event raised.
        /// </summary>
        /// <param name="args">Class containing an array of objects passed when invoked. You'll need to parse them from object to your desired type.</param>
        public void Invoke(EventArgObjects args)
        {
            _ManagedEvent?.Invoke(this, args);
        }
    }

    /// <summary>
    /// Container of arguments
    /// </summary>
    public class EventArgObjects : EventArgs
    {
        public List<object> Args { get; private set; }
        public EventMessage Message { get; private set; }

        /// <summary>
        /// Useful for tracking caller data if you're not using Pigeon.
        /// </summary>
        public int LineCalled { get; private set; } = -1;
        /// <summary>
        /// Useful for tracking caller data if you're not using Pigeon.
        /// </summary>
        public string CallerName { get; private set; } = "No member name retrieved.";
        /// <summary>
        /// Useful for tracking caller data if you're not using Pigeon.
        /// </summary>
        public string CallerLocation { get; private set; } = "No member location retrieved.";

        public EventArgObjects(EventMessage message, params object[] args)
        {
            Message = message;
            Args = args.ToList();
        }
        public EventArgObjects(params object[] args)
        {
            Args = args.ToList();
        }


        public void GetCallerInfo([CallerLineNumber] int line = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerPath = "")
        {
            LineCalled = line;
            CallerName = callerName;
            CallerLocation = callerPath;
        }

        /// <summary>
        /// Grabs the argument at the indexed location and returns it as T
        /// </summary>
        /// <typeparam name="T">The type the argument is</typeparam>
        /// <param name="index">The location in the array the argument is.</param>
        /// <exception cref="ArgumentOutOfRangeException">Should the index be out of bounds, ArgumentOutOfRangeException gets thrown.</exception>
        public T GetArgument<T>(int index)
        {
            if (index < 0 || index >= Args.Count)
                index = 0;

            return (T)Args[index];
        }

        /// <summary>
        /// Grabs the first object in the array that can be converted to type T, without taking any arguments.
        /// </summary>
        /// <typeparam name="T">The desired type</typeparam>
        /// <returns>The first object of type T</returns>
        /// <exception cref="InvalidOperationException">Thrown when multiple indices can be converted to type T.</exception>
        /// <exception cref="ArgumentException">Thrown when no object of type T is found.</exception>
        public T GetArgument<T>()
        {
            bool found = false;
            int foundIndex = -1;

            for (int i = 0; i < Args.Count; i++)
            {
                if (Args[i] is T)
                {
                    if (found)
                        throw new InvalidOperationException("Multiple indices can be converted to type " + typeof(T) + ". Please specify the index.");

                    found = true;
                    foundIndex = i;
                }
            }

            if (found)
                return (T)Args[foundIndex];
            else
                throw new ArgumentException("No object of type " + typeof(T) + " found.");
        }

        /// <summary>
        /// Finds all arguments passed of a specific type and returns it as a list. <para>Calls a warning should it run in to an error, does not throw exceptions.</para>
        /// </summary>
        /// <returns>List of type T</returns>
        public List<T>? GetArguments<T>()
        {
            try
            {
                List<T> list = new();

                foreach (var arg in Args)
                {
                    if ((T)arg != null)
                        list.Add((T)arg);
                }

                return list;
            }
            catch (Exception e)
            {
                EventMessage message = new($"Error when getting arguments!\n{e.Message}");
                Watchdog.LogWarningCallback.Invoke(new(message));
            }
            return null;
        }



        public object[] GetAllArgs()
        {
            List<object> list = Args;
            if (Message != null)
                list.Add(Message);

            return list.ToArray();
        }

        public bool DoesEventContainArgs => Args.Count > 0;

        public int Count => Args.Count;
    }
    public class EventMessage
    {
        public LogTypes MessageType { get; private set; }
        public string Value { get; private set; }

        public int LineCalled { get; private set; }
        public string CallerName { get; private set; }
        public string FileLocation { get; private set; }

        public string GetFileName { get { return Path.GetFileName(FileLocation); } }

        public EventMessage(string message, LogTypes messageType = LogTypes.None, [CallerLineNumber] int lineCalled = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string fileLocation = "")
        {
            MessageType = messageType;
            Value = message;
            LineCalled = lineCalled;
            CallerName = callerName;
            FileLocation = fileLocation;
        }

        internal void SetMessageType(LogTypes newMessageType) { MessageType = newMessageType; }
    }

    public class Watchdog
    {
        /// <summary>
        /// Invoke to request a shutdown of the Bot.
        /// </summary>
        public static WatchdogEvent RequestShutdownCallback { get; set; } = new();

        /// <summary>
        /// Invoke to log a critical error. Shuts down the bot.
        /// </summary>
        public static WatchdogEvent CriticalErrorCallback { get; set; } = new();

        /// <summary>
        /// Invoked after the for the Bot is completed.
        /// </summary>
        public static WatchdogEvent StartupCompleteCallback { get; set; } = new();

        /// <summary>
        /// Invoked when the bot successfully shuts down.
        /// </summary>
        public static WatchdogEvent ShutdownCompletedCallback { get; set; } = new();

        /// <summary>
        /// Invoked when the bot is requested to restart.
        /// </summary>
        public static WatchdogEvent RestartRequestedCallback { get; set; } = new();

        /// <summary>
        /// Invoke this to log a warning message
        /// </summary>
        public static WatchdogEvent LogWarningCallback { get; set; } = new();

        /// <summary>
        /// Invoke to log a message to all outputs and will try to log to custom logging methods, such as channels.
        /// </summary>
        public static WatchdogEvent LogMessageCallback { get; set; } = new();

        /// <summary>
        /// Invoke to log a message to only the Console and File logs.
        /// </summary>
        public static WatchdogEvent LogMessageConsoleAndFileOnlyCallback { get; set; } = new();

        /// <summary>
        /// Invoke to log a message to File logs only.
        /// </summary>
        public static WatchdogEvent LogToFileOnlyCallback { get; set; } = new();

        /// <summary>
        /// Invoke to log a message to Console only.
        /// </summary>
        public static WatchdogEvent LogToConsoleOnlyCallback { get; set; } = new();

        /// <summary>
        /// Called when the main loop begins a cycle
        /// </summary>
        public static WatchdogEvent StartLoopCycleCallback { get; set; } = new();

        /// <summary>
        /// Called at the end of the main loop cycle
        /// </summary>
        public static WatchdogEvent PostLoopCycleCallback { get; set; } = new();

        /// <summary>
        /// Called before the main loop starts.
        /// </summary>
        public static WatchdogEvent MainLoopStartingCallback { get; set; } = new();

        /// <summary>
        /// Called when the main loop exits.
        /// </summary>
        public static WatchdogEvent MainLoopEndedCallback { get; set; } = new();
    }
}