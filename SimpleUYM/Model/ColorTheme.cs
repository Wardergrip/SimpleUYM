using System.Windows.Media;

namespace SimpleUYM.Model
{
	public struct ColorTheme
	{
		public ColorTheme(bool isDefault = true)
		{
			if (isDefault)
			{
				BackgroundColor = Colors.White;
				TextColor = Colors.Black;
				SelectedTab = Colors.White;
				UnselectedTab = Colors.Gray;
				ButtonColor = Colors.LightGray;
				return;
			}
			BackgroundColor = (Color)ColorConverter.ConvertFromString("#1F1F1F");
			TextColor = Colors.White;
			SelectedTab = (Color)ColorConverter.ConvertFromString("#4D4D4D");
			UnselectedTab = (Color)ColorConverter.ConvertFromString("#363636");
			ButtonColor = (Color)ColorConverter.ConvertFromString("#666666");
		}

		public Color BackgroundColor { get; set; }
		public Color TextColor { get; set; }
		public Color SelectedTab { get; set; }
		public Color UnselectedTab { get; set; }
		public Color ButtonColor { get; set; }
	}
}
