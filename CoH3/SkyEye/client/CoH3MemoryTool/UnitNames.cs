using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoH3MemoryTool;

internal static class UnitNames
{
	private static readonly Dictionary<string, string> aliases;

	static UnitNames()
	{
		aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "riflemen_us", "RM" },
			{ "grenadier_ger", "大G" },
			{ "grenadiers_ger", "大G" },
			{ "tommy_africa_uk", "草帽" },
			{ "sherman_us", "馒头" },
			{ "sherman_recrewable_us", "馒头" },
			{ "sherman_76mm_us", "76馒头" },
			{ "sherman_easy_8_us", "E8" },
			{ "sherman_bulldozer_us", "推土馒头" },
			{ "sherman_whizbang_us", "火箭馒头" },
			{ "sniper_us", "美狙" },
			{ "sniper_ger", "德狙" },
			{ "engineer_us", "美工兵" },
			{ "assault_engineer_us", "突击工兵" },
			{ "scout_us", "侦察兵" },
			{ "pathfinder_us", "探路者" },
			{ "paratrooper_us", "美伞兵" },
			{ "ranger_us", "游骑兵" },
			{ "bazooka_team_us", "筒子队" },
			{ "ssf_commandos_us", "SSF" },
			{ "captain_us", "上尉" },
			{ "medic_us", "医护兵" },
			{ "stuart_africa_uk", "斯图亚特" },
			{ "stuart_us", "斯图亚特" },
			{ "grant_africa_uk", "格兰特" },
			{ "matilda_africa_uk", "玛蒂尔达" },
			{ "crusader_africa_uk", "十字军" },
			{ "bishop_africa_uk", "主教" },
			{ "sapper_africa_uk", "英工兵" },
			{ "gurkhas_africa_uk", "廓尔喀" },
			{ "guards_africa_uk", "英卫队" },
			{ "commando_africa_uk", "英突击队" },
			{ "chaffee_us", "霞飞" },
			{ "greyhound_us", "灰狗" },
			{ "greyhound_recrewable_us", "灰狗" },
			{ "hellcat_us", "地狱猫" },
			{ "scott_us", "斯科特" },
			{ "pershing_us", "潘兴" },
			{ "ambulance_us", "救护车" },
			{ "halftrack_us", "半履带" },
			{ "halftrack_quad_us", "四管半履带" },
			{ "halftrack_75mm_us", "75半履带" },
			{ "truck_4x4_us", "吉普" },
			{ "pioneer_ger", "德工兵" },
			{ "panzergrenadier_ger", "装甲掷弹兵" },
			{ "jaeger_ger", "猎兵" },
			{ "fallschirmjaeger_ger", "德伞兵" },
			{ "fallschirmpioneer_ger", "空降工兵" },
			{ "panzer_iv_ger", "四号" },
			{ "panther_ger", "豹式" },
			{ "tiger_ger", "虎式" },
			{ "stug_iii_ger", "三突" },
			{ "wespe_ger", "黄蜂" },
			{ "marder_iii_ger", "黄鼠狼" },
			{ "fallschirmjagers_ger", "德伞兵" },
			{ "fallschirmpioneers_ger", "空降工兵" },
			{ "fallschirmsniper_ger", "空降狙击手" },
			{ "assault_grenadier_ger", "突击掷弹兵" },
			{ "coastal_reserves_ger", "海岸预备队" },
			{ "coastal_officer_ger", "海岸军官" },
			{ "stormtrooper_ger", "风暴兵" },
			{ "sturmpioneer_ger", "突击工兵" },
			{ "howitzer_25pdr_africa_uk", "25磅炮" },
			{ "howitzer_5_5_inch_africa_uk", "5.5寸炮" },
			{ "howitzer_210mm_ger", "210炮" },
			{ "panzergrenadier_ak", "PG" },
			{ "halftrack_250_ak", "250" },
			{ "panzerpioneer_ak", "非洲工兵" },
			{ "assault_grenadier_ak", "突击掷弹兵" },
			{ "bersaglieri_ak", "意大利轻步兵" },
			{ "guastatori_ak", "意大利突击工兵" },
			{ "carro_armato_ak", "意大利轻坦" },
			{ "semovente_ak", "意大利突击炮" }
		};
		try
		{
			string[] array = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "单位简称.tsv"));
			foreach (string text in array)
			{
				string[] array2 = text.Split('\t');
				if (array2.Length == 2 && !text.StartsWith("#") && array2[1].Trim().Length > 0)
				{
					aliases[array2[0].Trim()] = array2[1].Trim();
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

	public static string Name(string path)
	{
		if (UserUnitNames56.TryGet(path, out var name))
		{
			return name;
		}
		string text = path.Substring(path.LastIndexOf('\\') + 1);
		if (text.Equals("panzergrenadier_ak", StringComparison.OrdinalIgnoreCase))
		{
			return "PG";
		}
		if (text.Equals("halftrack_250_ak", StringComparison.OrdinalIgnoreCase))
		{
			return "250";
		}
		if (!aliases.TryGetValue(path, out var value) && !aliases.TryGetValue(text, out value) && !UnitNamesBase.Names.TryGetValue(path, out value))
		{
			return text;
		}
		return value;
	}

	public static string Summary(PlayerInfoRow row)
	{
		if (!row.UnitsKnown)
		{
			return "读取中";
		}
		if (row.Units.Count != 0)
		{
			return string.Join(" · ", from x in row.Units
				group x by Name(x.Key) into x
				orderby x.Key
				select x.Key + " x" + x.Sum((KeyValuePair<string, int> v) => v.Value));
		}
		return "无";
	}
}
