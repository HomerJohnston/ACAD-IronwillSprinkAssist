// (C) Copyright 2020 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Reflection;
using Autodesk.AutoCAD.Colors;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

// This line is not mandatory, but improves loading performances
[assembly: ExtensionApplication(typeof(Ironwill.SprinkAssist))]

namespace Ironwill
{
	// This class is instantiated by AutoCAD once and kept alive for the 
	// duration of the session. If you don't do any one time initialization 
	// then you should remove this class.
	public partial class SprinkAssist : IExtensionApplication
	{
		void IExtensionApplication.Initialize()
		{
			// Add one time initialization here
			// One common scenario is to setup a callback function here that 
			// unmanaged code can call. 
			// To do this:
			// 1. Export a function from unmanaged code that takes a function
			//    pointer and stores the passed in value in a global variable.
			// 2. Call this exported function in this function passing delegate.
			// 3. When unmanaged code needs the services of this managed module
			//    you simply call acrxLoadApp() and by the time acrxLoadApp 
			//    returns  global function pointer is initialized to point to
			//    the C# delegate.
			// For more info see: 
			// http://msdn2.microsoft.com/en-US/library/5zwkzwf4(VS.80).aspx
			// http://msdn2.microsoft.com/en-us/library/44ey4b32(VS.80).aspx
			// http://msdn2.microsoft.com/en-US/library/7esfatk4.aspx
			// as well as some of the existing AutoCAD managed apps.

			// Initialize your plug-in application here
			//Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Initializing SprinkAssist");
			DisplayVersion(null, null);
			AcApplication.DocumentManager.DocumentCreated += DisplayVersion;

			Ironwill.Commands.DisableObjectSnaps.Commands.EnableHeadSnapping();
		}

		void IExtensionApplication.Terminate()
		{
			// Do plug-in application clean up here
			AcApplication.DocumentManager.DocumentCreated -= DisplayVersion;
		}

		private void DisplayVersion(object sender, DocumentCollectionEventArgs e)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string assemblyName = AssemblyName.GetAssemblyName(assembly.Location).Version.ToString();

			//Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Helloe Workd");
			Session.Log(
				"{0}" +
				"-------------------{0}" +
				"SprinkAssist Loaded{0}" +
				"Version: {1}{0}" +
				//"-------------------", Environment.NewLine, GhettoVersion));
				"-------------------", Environment.NewLine, assemblyName);
		}
	}
}

namespace LoadedAssemblies
{

	public class Commands

	{

		[LispFunction("GetAssemblies")]

		public ResultBuffer GetLoadedAssemblies(ResultBuffer rb)

		{

			// Get the list of loaded assemblies



			Assembly[] assems = AppDomain.CurrentDomain.GetAssemblies();



			// A ResultBuffer to populate and return



			ResultBuffer res = new ResultBuffer();



			// List the assemblies in the current application domain.



			const string start = "Version=";



			foreach (Assembly assem in assems)

			{

				// We want the name and version of each assembly



				string dllname = assem.ManifestModule.Name;



				string version = assem.FullName;



				// If our assembly name includes version info...



				if (version.Contains(start))

				{

					// Get the string starting with the version number



					version =

					  version.Substring(

						version.IndexOf(start) + start.Length

					  );



					// Strip off anything after (and including) the comma



					version =

					  version.Remove(version.IndexOf(','));

				}

				else

					version = "";



				// Add a dotted pair of the name with the version



				res.Add(new TypedValue((int)LispDataType.ListBegin));

				res.Add(new TypedValue((int)LispDataType.Text, dllname));

				res.Add(new TypedValue((int)LispDataType.Text, version));

				res.Add(new TypedValue((int)LispDataType.DottedPair));

			}

			return res;

		}

	}

}