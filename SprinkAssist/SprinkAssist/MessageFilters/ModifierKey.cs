using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Ironwill.MessageFilters
{
	internal class ModifierKey : IMessageFilter
	{
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_KEYUP = 0x0101;

		public bool controlPressed = false;
		public bool shiftPressed = false;
		public bool altPressed = false;

		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg != WM_KEYDOWN && m.Msg != WM_KEYUP)
			{
				return false;
			}

			Keys kc = (Keys)(int)m.WParam & Keys.KeyCode;

			bool value = m.Msg == WM_KEYDOWN;

			if (kc == Keys.ControlKey)
			{
				controlPressed = value;
				Session.RefreshViewport();
				return true;
			}
			else if (kc == Keys.ShiftKey)
			{
				shiftPressed = value;
				Session.RefreshViewport();
				return true;
			}
			else if (kc == Keys.Alt)
			{
				altPressed = value;
				Session.RefreshViewport();
				return true;
			}

			return false;
		}
	}
}
