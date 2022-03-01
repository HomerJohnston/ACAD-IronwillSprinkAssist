using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Ironwill.Test))]

namespace Ironwill
{
	class Test
	{
		[CommandMethod("TestAddGroup")]
		public void TestAddGroupCmd()
		{
			ResultBuffer data = new ResultBuffer();

			int stringDxfCode = (int)DxfCode.XTextString;

			data.Add(new TypedValue(stringDxfCode, "Main Floor"));

			//DataStore.SetXrecordData("testxrecord", data, DictionaryPath._PipeGroups);
		}

		[CommandMethod("TestWX")]
		public void TestWriteX()
		{
			ResultBuffer data = new ResultBuffer();

			data.Add(new TypedValue((int)DxfCode.XTextString, "wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd wtaslkdhjaslkdhjklasjdlkajsdkljaslkdjalksdjklajsdklajsdlkajslkdja lksjd lkajsldkajs dlkjasdlk jas dlkjas dlkja slkja sldkj alskjd aslksj dalskdj asldkjas dlkasjd laksjd lkj alskdj dlaksjd lkjas dlkasjd lkasjd "));
			//DataStore.SetXrecordData("testxrecord", data, "SomeSubPath", "SomeSubSubPath");
		}

		[CommandMethod("TestRX")]
		public void TestReadX()
		{
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Editor editor = doc.Editor;

			ResultBuffer resultBuffer = null;// DataStore.GetXrecordData("testxrecord", "SomeSubPath", "SomeSubSubPath");

			if (resultBuffer == null)
			{
				return;
			}

			TypedValue x = resultBuffer.AsArray()[0];
			string s = x.Value as String;

			if (s != null)
			{
				editor.WriteMessage(s);
			}
		}
	}
}
