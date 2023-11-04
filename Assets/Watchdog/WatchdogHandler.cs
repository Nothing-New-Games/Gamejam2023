using UnityEngine;
using WatchDog;
using PigeonCarrier;
using Sirenix.OdinInspector;
using UnityEditor.VersionControl;

public class WatchdogHandler : MonoBehaviour
{
    private static WatchdogHandler instance;
    Pigeon pigeon;

    [Button]
    private void CollectLogs()
    {
        Watchdog.RequestShutdownCallback.Invoke(new(new EventMessage("Shutdown requested by Inspector Button!\n", LogTypes.Info)));
    }

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogWarning("Watchdog handler already exists!");
            Destroy(this);
            return;
        }

        pigeon = new("Pigeon");
        Watchdog.LogToConsoleOnlyCallback += DebugLog;
        Watchdog.LogMessageCallback += DebugLog;
        Watchdog.LogWarningCallback += DebugWarning;
        Watchdog.CriticalErrorCallback += DebugCriticalError;

        WorldItem.SpawnItemEvent += WorldItem.SpawnWorldItem;
    }

    private void OnApplicationQuit()
    {
        Watchdog.RequestShutdownCallback.Invoke(new(new EventMessage("Shutdown requested by Application Quit!\n", LogTypes.Info)));
    }


    private void DebugLog(object caller, EventArgObjects args)
    {
        string message;

        if (args.Message ==  null)
        {
            message = args.GetArgument<string>();
        }
        else
        {
            message = args.Message.Value;
        }

        message = Pigeon.GetPrintInfo(args.Message.MessageType, args.Message.LineCalled, args.Message.CallerName) + message;

        if (args.Message.MessageType == LogTypes.Warning)
            Debug.LogWarning(message);
        else if (args.Message.MessageType == LogTypes.Error)
            Debug.LogError(message);
        else if (args.Message.MessageType == LogTypes.CriticalError)
            Debug.LogError(message);
        else
            Debug.Log(message);
    }

    private void DebugWarning(object sender, EventArgObjects args)
    {
        EventMessage properMessage = new EventMessage(args.Message.Value, LogTypes.Warning, args.Message.LineCalled, args.Message.CallerName, args.Message.FileLocation);
        args = new(properMessage, args.Args);
        DebugLog(sender, args);
    }

    private void DebugCriticalError(object sender, EventArgObjects args)
    {
        EventMessage properMessage = new EventMessage(args.Message.Value, LogTypes.CriticalError, args.Message.LineCalled, args.Message.CallerName, args.Message.FileLocation);
        args = new(properMessage, args.Args);
        DebugLog(sender, args);
    }
}
