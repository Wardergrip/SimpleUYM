using Newtonsoft.Json;
using System.Text;

namespace SimpleUYM.Model
{
	public class ConfigData
	{
		[JsonProperty(PropertyName = "PathToGitBash")]
		public string PathToGitBash { get; set; }
		[JsonProperty(PropertyName = "PathToRepository")]
		public string PathToRepository { get; set; }

		public static string GetDefaultContents()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("{");
			sb.AppendLine("\t\"PathToGitBash\":\"\",");
			sb.AppendLine("\t\"PathToRepository\":\"\"");
			sb.AppendLine("}");
			return sb.ToString();
		}
	}
}
