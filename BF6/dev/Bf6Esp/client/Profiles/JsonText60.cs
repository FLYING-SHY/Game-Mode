using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Bf6Esp;

/// <summary>
/// JSON 读写助手。
/// net48 的 DataContractJsonSerializer 没有"缩进"选项，这里统一由本类做格式化，
/// 保证档案文件是人工可读、可手工编辑的。
/// </summary>
internal static class JsonText60
{
	/// <summary>序列化为带缩进的 JSON 文本。</summary>
	internal static string Serialize<T>(T value)
	{
		DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
		using (MemoryStream stream = new MemoryStream())
		{
			serializer.WriteObject(stream, value);
			return Pretty(Encoding.UTF8.GetString(stream.ToArray()));
		}
	}

	/// <summary>反序列化；失败时抛出 SerializationException。</summary>
	internal static T Deserialize<T>(string text)
	{
		DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
		using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(text ?? string.Empty)))
		{
			return (T)serializer.ReadObject(stream);
		}
	}

	/// <summary>把紧凑 JSON 整理成缩进风格；字符串内容原样保留。</summary>
	internal static string Pretty(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return json;
		}
		StringBuilder builder = new StringBuilder(json.Length * 2);
		bool inString = false;
		bool escape = false;
		int indent = 0;
		for (int i = 0; i < json.Length; i++)
		{
			char current = json[i];
			if (inString)
			{
				builder.Append(current);
				if (escape)
				{
					escape = false;
				}
				else if (current == '\\')
				{
					escape = true;
				}
				else if (current == '"')
				{
					inString = false;
				}
				continue;
			}
			switch (current)
			{
				case '"':
					inString = true;
					builder.Append(current);
					break;
				case '{':
				case '[':
					builder.Append(current);
					if (i + 1 < json.Length && (json[i + 1] == '}' || json[i + 1] == ']'))
					{
						i++;
						builder.Append(json[i]);
					}
					else
					{
						indent++;
						builder.Append('\n').Append(Indent(indent));
					}
					break;
				case '}':
				case ']':
					indent--;
					builder.Append('\n').Append(Indent(indent)).Append(current);
					break;
				case ',':
					builder.Append(current).Append('\n').Append(Indent(indent));
					break;
				case ':':
					builder.Append(": ");
					break;
				default:
					builder.Append(current);
					break;
			}
		}
		return builder.ToString();
	}

	private static string Indent(int level)
	{
		return new string(' ', level * 2);
	}
}
