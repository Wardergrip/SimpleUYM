using Newtonsoft.Json;
using System.Text;

namespace SimpleUYM.Model
{
	public class ConfigData
	{
		[JsonProperty(PropertyName = "PathToUnityYAMLMerge")]
		public string PathToUnityYAMLMerge { get; set; }
		[JsonProperty(PropertyName = "PathToRepository")]
		public string PathToRepository { get; set; }

		public static string GetDefaultContents()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("{");
			sb.AppendLine("\t\"PathToUnityYAMLMerge\":\"\",");
			sb.AppendLine("\t\"PathToRepository\":\"\"");
			sb.AppendLine("}");
			return sb.ToString();
		}
	}
}
