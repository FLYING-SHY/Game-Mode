using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;

namespace CoH3MemoryTool;

internal sealed class AlertSounds : IDisposable
{
	private sealed class Pending60
	{
		public AlertKind Kind;

		public long Epoch;
	}

	public bool Enabled = true;

	private readonly Dictionary<AlertKind, string> custom = new Dictionary<AlertKind, string>();

	private readonly Queue<Pending60> queue = new Queue<Pending60>();

	private bool running;

	private bool disposed;

	private AudioClip active;

	public string LastError = "";

	private readonly string settingsFile;

	private static string Root => AppDomain.CurrentDomain.BaseDirectory;

	private static string Settings => Path.Combine(Root, "alert-sounds.txt");

	public AlertSounds(string settingsPath = null)
	{
		settingsFile = settingsPath ?? Settings;
		try
		{
			string[] array = File.ReadAllLines(settingsFile);
			foreach (string text in array)
			{
				int num = text.IndexOf('=');
				if (num >= 0)
				{
					string text2 = text.Substring(0, num);
					string text3 = text.Substring(num + 1);
					AlertKind result;
					if (text2 == "enabled")
					{
						Enabled = text3 == "1";
					}
					else if (Enum.TryParse<AlertKind>(text2, out result))
					{
						custom[result] = text3;
					}
				}
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}

	public string FileFor(AlertKind kind)
	{
		lock (custom)
		{
			string value;
			return (custom.TryGetValue(kind, out value) && !string.IsNullOrWhiteSpace(value)) ? value : DefaultSounds61.AlertFile(kind);
		}
	}

	public string DisplayFile(AlertKind kind)
	{
		string text = FileFor(kind);
		if (kind == AlertKind.EnemySldFlare && string.IsNullOrEmpty(text))
		{
			return "系统提示音";
		}
		if (!string.IsNullOrEmpty(text))
		{
			return Path.GetFileName(text);
		}
		return "未设置（静音）";
	}

	public void Set(AlertKind kind, string file)
	{
		if (file != null)
		{
			using (new AudioClip(file))
			{
			}
		}
		lock (custom)
		{
			custom[kind] = file ?? "";
			Save();
		}
	}

	public void Save()
	{
		lock (custom)
		{
			File.WriteAllLines(settingsFile, new string[1] { "enabled=" + (Enabled ? "1" : "0") }.Concat(custom.Select((KeyValuePair<AlertKind, string> x) => string.Concat(x.Key, "=", x.Value))));
		}
	}

	public void Play(AlertKind kind, bool preview = false)
	{
		if (!LicenseGate60.Allowed || (!Enabled && !preview) || disposed)
		{
			return;
		}
		if (kind == AlertKind.EnemySldFlare && string.IsNullOrEmpty(FileFor(kind)))
		{
			SystemSounds.Asterisk.Play();
		}
		else
		{
			if (string.IsNullOrEmpty(FileFor(kind)))
			{
				return;
			}
			lock (queue)
			{
				if (queue.Count < 16)
				{
					queue.Enqueue(new Pending60
					{
						Kind = kind,
						Epoch = LicenseGate60.Epoch
					});
				}
				if (running)
				{
					return;
				}
				running = true;
			}
			ThreadPool.QueueUserWorkItem(delegate
			{
				while (true)
				{
					Pending60 pending;
					lock (queue)
					{
						if (disposed || !LicenseGate60.Allowed || queue.Count == 0)
						{
							queue.Clear();
							running = false;
							break;
						}
						pending = queue.Dequeue();
					}
					if (pending.Epoch == LicenseGate60.Epoch)
					{
						try
						{
							string text = FileFor(pending.Kind);
							if (!string.IsNullOrEmpty(text))
							{
								using AudioClip audioClip = new AudioClip(text);
								lock (queue)
								{
									if (disposed)
									{
										running = false;
										break;
									}
									active = audioClip;
								}
								audioClip.PlaySync(pending.Epoch);
								lock (queue)
								{
									active = null;
								}
							}
						}
						catch (Exception)
						{
							LastError = "提示音无法播放，请重新选择有效的 MP3 或 WAV 音频文件。";
							active = null;
						}
					}
				}
			});
		}
	}

	public void Pause60()
	{
		lock (queue)
		{
			queue.Clear();
		}
		AudioClip.StopAll60();
	}

	public void Dispose()
	{
		lock (queue)
		{
			disposed = true;
			queue.Clear();
		}
		if (active != null)
		{
			try
			{
				active.Dispose();
			}
			catch
			{
			}
		}
	}
}
