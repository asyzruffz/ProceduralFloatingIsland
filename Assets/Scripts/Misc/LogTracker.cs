using UnityEngine;
using System.IO;

[System.Serializable]
public class LogTracker {

	public string name;
	public bool enabled;
	public bool printToScreen = false;
	public bool printToFile = false;

	private MemoryStream data;
	private FileStream fileWriter;
	private bool debug
	{
		get { return enabled && LoggerTool.debug; }
	}

	public LogTracker (string name, bool enabled) {
		this.name = name;
		this.enabled = enabled;

		data = new MemoryStream ();
		WriteToStream (data, " ----- Log: " + System.DateTime.Now.ToString ("dd-MM-yyyy") + "  ----- \n");
	}

	public void Configure (bool printToScreen, bool printToFile) {
		this.printToScreen = printToScreen;
		this.printToFile = printToFile;
	}

	public void Log (object logMsg) {
		if (debug) {
			string msgString = LogFormat (logMsg.ToString ());
			if (printToFile) {
				WriteToStream (data, msgString + "\n");
			}
			
			Flush (msgString);
		}
	}


	public void Flush (string msg) {
		if (printToScreen && debug) {
			Debug.Log (msg);
		}
	}

	public void Save () {
		if (debug && printToFile) {
			CheckDirectory ();
			string filePath = GetFileFullPath (name);
			fileWriter = File.Open (filePath, FileMode.Append);
			data.WriteTo (fileWriter);
			data = new MemoryStream ();
			fileWriter.Close ();
		}
	}
	

	#region virtual functions
	protected virtual string LogFormat (string msg) {
		return System.DateTime.Now.ToLongTimeString () + ":\t" + msg;
	}

	protected virtual string GetFileFullPath (string name) {
		return Application.dataPath + "/" + LoggerTool.Instance.savePath + "/" + name + "__" + System.DateTime.Now.ToString ("dd-MM-yyyy") + ".log";
	}

	protected virtual void CheckDirectory () {
		string dirPath = Application.dataPath + "/" + LoggerTool.Instance.savePath;
		if (!Directory.Exists(dirPath)) {
			Directory.CreateDirectory (dirPath);
		}
	}
	#endregion

	#region Null Instance
	private static LogTracker _nullLogTracker;
	public static LogTracker NullLogTracker {
		get {
			if (_nullLogTracker == null) {
				_nullLogTracker = new LogTracker ("NullLogTracker", false);
				_nullLogTracker.enabled = false;
			}
			return _nullLogTracker;
		}
	}
	public static bool IsNull (LogTracker l) {
		return l == NullLogTracker;
	}
	#endregion

	#region Helper
	private System.Text.UnicodeEncoding uniEncoding = new System.Text.UnicodeEncoding ();
	private void WriteToStream (Stream stream, string msg) {
		if (stream != null) {
			stream.Write (uniEncoding.GetBytes (msg),
			0, uniEncoding.GetByteCount (msg));
		}
	}
	#endregion
}
