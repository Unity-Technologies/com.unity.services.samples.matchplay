using System.Collections.Generic;
using UnityEngine;

namespace Matchplay.Shared.Tools
{
	public class Customization
	{
		static List<Color> s_Colors = new()
		{
			new Color(0.7f,0.2f, 0.2f),
			new Color(0.7f,0.4f, 0.2f),
			new Color(0.7f,0.6f, 0.1f),
			new Color(0.6f,0.2f, 0.5f),
			new Color(0.2f,0.5f, 0.1f),
			new Color(0.3f,0.7f, 0.7f),
			new Color(0.2f,0.4f, 0.7f),
			new Color(0.7f,0.5f, 0.9f),
			new Color(0.9f,0.6f, 0.6f),
			new Color(0.9f,0.9f, 0.9f),
			new Color(0.2f,0.7f, 0.3f),
		};

		public static Color IDToColor(ulong ID)
		{
			int IDint = Mathf.RoundToInt(ID);

			if(IDint>s_Colors.Count||IDint<0)
				return Color.black;

			return s_Colors[IDint];
		}
	}
}
