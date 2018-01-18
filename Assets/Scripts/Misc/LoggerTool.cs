using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggerTool : Singleton<LoggerTool> {

	[Tooltip ("The save path of log files, relative to the Asset path or application root path")]
	public string savePath;
	public bool printToScreen = true;
	public bool printToFile = true;
	[SerializeField]
	private bool debugMode = true;

	LogTracker defaultLogger;

	// whether debug is on
	public static bool debug {
		get { return Instance != null && Instance.enabled && Instance.debugMode; }
	}

	protected override void SingletonAwake () {
		DontDestroyOnLoad (gameObject);

		defaultLogger = new LogTracker ("GameLog", true);
		defaultLogger.Configure (printToScreen, printToFile);
	}
	
	/// <summary>
	/// log some message and post it.
	/// </summary>
	/// <param name="logMsg"></param>
	public void Post (object logMsg) {
		if (!debug) {
			return;
		}

		defaultLogger.Log (logMsg);
		defaultLogger.Save ();
	}
}
