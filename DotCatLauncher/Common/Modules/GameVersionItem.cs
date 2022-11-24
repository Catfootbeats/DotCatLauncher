using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotCatLauncher.Common.Modules
{
    public class GameVersionItem
    {
		private string id;

		public string Id
        {
			get { return "Minecraft "+id; }
			set { id = value; }
		}

	}
}
