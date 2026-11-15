using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Security.Cryptography;
using NAudio.Wave;

namespace CoH3MemoryTool;

internal sealed class AudioClip : IDisposable
{
	private static readonly object allGate60 = new object();

	private static readonly HashSet<AudioClip> all60 = new HashSet<AudioClip>();

	private readonly SoundPlayer player;

	private readonly object gate = new object();

	private bool disposed;

	public static void StopAll60()
	{
		lock (allGate60)
		{
			foreach (AudioClip item in all60)
			{
				try
				{
					item.player.Stop();
				}
				catch (ObjectDisposedException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
	}

	public AudioClip(string file)
	{
		file = Path.GetFullPath(file);
		if (!File.Exists(file))
		{
			throw new FileNotFoundException("音频文件不存在。", file);
		}
		string extension = Path.GetExtension(file);
		if (extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
		{
			file = Decode(file);
		}
		else if (!extension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("请选择 MP3 或 WAV 音频。");
		}
		player = new SoundPlayer(file);
		try
		{
			player.Load();
			lock (allGate60)
			{
				all60.Add(this);
			}
		}
		catch
		{
			player.Dispose();
			throw;
		}
	}

	private static string Decode(string file)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		string text;
		using (FileStream inputStream = File.OpenRead(file))
		{
			using SHA256 sHA = SHA256.Create();
			text = BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace("-", "");
		}
		string text2 = Path.Combine(Path.GetTempPath(), "CoH3AudioCache-v21");
		Directory.CreateDirectory(text2);
		string text3 = Path.Combine(text2, text + ".wav");
		if (File.Exists(text3))
		{
			return text3;
		}
		string text4 = text3 + "." + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			Mp3FileReader val = new Mp3FileReader(file);
			try
			{
				if (((Stream)(object)val).Length <= 0)
				{
					throw new InvalidOperationException("MP3 没有可播放的音频。");
				}
				WaveFileWriter.CreateWaveFile(text4, (IWaveProvider)(object)val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			using (SoundPlayer soundPlayer = new SoundPlayer(text4))
			{
				soundPlayer.Load();
			}
			if (!File.Exists(text3))
			{
				File.Move(text4, text3);
			}
			return text3;
		}
		finally
		{
			if (File.Exists(text4))
			{
				File.Delete(text4);
			}
		}
	}

	public void PlaySync()
	{
		PlaySync(LicenseGate60.Epoch);
	}

	public void PlaySync(long epoch)
	{
		lock (gate)
		{
			if (disposed || !LicenseGate60.Allowed || LicenseGate60.Epoch != epoch)
			{
				return;
			}
		}
		try
		{
			player.PlaySync();
		}
		catch (ObjectDisposedException)
		{
			lock (gate)
			{
				if (!disposed)
				{
					throw;
				}
			}
		}
	}

	public void Dispose()
	{
		lock (gate)
		{
			if (disposed)
			{
				return;
			}
			disposed = true;
			player.Stop();
			player.Dispose();
		}
		lock (allGate60)
		{
			all60.Remove(this);
		}
	}
}
