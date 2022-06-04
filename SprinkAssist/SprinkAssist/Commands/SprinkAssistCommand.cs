using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	internal class SprinkAssistCommand
	{
		protected DBDictionary cmdSettings;

		public SprinkAssistCommand()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				cmdSettings = XRecordLibrary.GetCommandDictionaryForClass(transaction, GetType());
				transaction.Commit();
			}
		}
	}
}