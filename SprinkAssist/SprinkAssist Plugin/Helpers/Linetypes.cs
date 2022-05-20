using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	internal class Linetypes
	{
		// acad.lin / acadiso.lin
		public static string Continous	{ get { return "Continuous"; } }
		public static string Center		{ get { return "Center"; } }
		public static string Center2	{ get { return "Center2"; } }
		public static string CenterX2	{ get { return "CenterX2"; } }
		public static string Dashed		{ get { return "Dashed"; } }
		public static string Dashed2	{ get { return "Dashed2"; } }
		public static string DashedX2	{ get { return "DashedX2"; } }
		public static string DashDot	{ get { return "DashDot"; } }
		public static string DashDot2	{ get { return "DashDot2"; } }
		public static string DashDotX2	{ get { return "DashDotX2"; } }
		public static string Divide		{ get { return "Divide"; } }
		public static string Divide2	{ get { return "Divide2"; } }
		public static string DivideX2	{ get { return "DivideX2"; } }
		public static string Dot		{ get { return "Dot"; } }
		public static string Dot2		{ get { return "Dot2"; } }
		public static string DotX2		{ get { return "DotX2"; } }
		public static string Hidden		{ get { return "Hidden"; } }
		public static string Hidden2	{ get { return "Hidden2"; } }
		public static string HiddenX2	{ get { return "HiddenX2"; } }
		public static string Phantom	{ get { return "Phantom"; } }
		public static string Phantom2	{ get { return "Phantom2"; } }
		public static string PhantomX2	{ get { return "PhantomX2"; } }

		// spkassist.lin / spkassistiso.lin

		public static void LoadLinetype(string name, bool bForceReplace = false)
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				Database database = Session.GetDatabase();

				LinetypeTable linetypeTable = transaction.GetObject(database.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

				List<string> linetypeFiles = new List<string>()
				{
					"acad.lin",
					"spkassist.lin",
				};

				foreach (string linetypeFile in linetypeFiles)
				{
					Session.GetDatabase().LoadLineTypeFile(name, linetypeFile);

					if (linetypeTable.Has(name))
					{
						break;
					}
				}

				transaction.Commit();
			}
		}
	}
}
