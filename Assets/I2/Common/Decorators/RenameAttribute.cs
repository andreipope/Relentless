using UnityEngine;
using System.Collections;

namespace I2
{
	public class RenameAttribute : PropertyAttribute 
	{
		public readonly string Name, Tooltip;
		public readonly int HorizSpace;
		
		public RenameAttribute(int hspace, string name, string tooltip = default(string))
		{
			this.Name = name;
			this.Tooltip = tooltip;
			this.HorizSpace = hspace;
		}
		public RenameAttribute (string name, string tooltip = default(string)):this(0, name, tooltip){}
	}
}