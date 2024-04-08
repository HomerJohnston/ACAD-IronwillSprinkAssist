using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Commands.SnapOverrule
{
	public class OsnapOverruleBase : OsnapOverrule
	{
		private bool active = false;

		protected string name = string.Empty;

		public void SetEnabled(bool newState)
		{
			if (newState)
			{
				if (name != string.Empty)
				{
					Session.Log($"Enabling snap overrule: {name}");
				}

				AddOverrule(RXObject.GetClass(typeof(Entity)), this, false);
				Overruling = true;
				active = true;
			}
			else
			{
				if (name != string.Empty)
				{
					Session.Log($"Disabling snap overrule: {name}");
				}

				RemoveOverrule(RXObject.GetClass(typeof(Entity)), this);
				active = false;
			}
		}

		public void ToggleEnabled()
		{
			if (active)
			{
				SetEnabled(false);
			}
			else
			{
				SetEnabled(true);
			}
		}
	}
}
