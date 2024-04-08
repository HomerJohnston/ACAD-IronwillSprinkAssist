using System;
using System.Text;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Ironwill.Helpers.GetSetEnv))]

namespace Ironwill.Helpers
{
    /// <summary>
    /// Wrapper for ARX defined function acadGetEnv & acedSetEnv.
    /// </summary>

    public class GetSetEnv
    {
		/*
			[System.Security.SuppressUnmanagedCodeSecurity]
			[DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
			private static extern int acedGetEnv(string envName, StringBuilder result);

			[System.Security.SuppressUnmanagedCodeSecurity]
			[DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
			private static extern int acedSetEnv(string envName, string value);

			[CommandMethod(SprinkAssist.CommandMethodPrefix, "TestGetEnvWrapper", CommandFlags.NoBlockEditor)]
			static public void getenvwrapper()
			{
				Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;

				const int maxResultLength = 1024;

				StringBuilder sbRes = new StringBuilder(maxResultLength);

				acedGetEnv("ACAD", sbRes);

				ed.WriteMessage("\nValue of ACAD environment variable: {0}", sbRes.ToString());
			}

			[CommandMethod(SprinkAssist.CommandMethodPrefix, "TestSetEnvWrapper", CommandFlags.Undefined)]
			static public void setenvwrapper()
			{
				Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;

				const int maxResultLength = 1024;

				StringBuilder sbRes = new StringBuilder(maxResultLength);

				// get the current value of 'MaxHatch'
				acedGetEnv("MaxHatch", sbRes);
				ed.WriteMessage("\nCurrent value of 'MaxHatch' environment variable: {0}", sbRes.ToString());

				// set a new value
				acedSetEnv("MaxHatch", "10000");

				// get it again and print out new value
				acedGetEnv("MaxHatch", sbRes);
				ed.WriteMessage("\nNew value of 'MaxHatch' environment variable: {0}", sbRes.ToString());
			}
		*/
	}

}