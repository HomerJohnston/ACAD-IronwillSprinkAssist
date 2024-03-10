using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq;

[assembly: CommandClass(typeof(Ironwill.Commands.Help.HelpCmd))]


namespace Ironwill.Commands.Help
{
	public class HelpCmd
	{
		[CommandMethod("ListCommandsFromThisAssembly")]
		static public void ListCommandsFromThisAssembly()
		{
			// Just get the commands for this assembly
			Editor editor = Session.GetEditor();
			Assembly assembly = Assembly.GetExecutingAssembly();

			string[] commands = GetCommands(assembly);

			foreach (string cmd in commands)
			{
				editor.WriteMessage(cmd + "\n");
			}
		}

		private static string[] GetCommands(Assembly asm)
		{
			StringCollection stringCollection = new StringCollection();
			Type[] types;

			object[] commandClassAttributes = asm.GetCustomAttributes(typeof(CommandClassAttribute), true);
			int numTypes = commandClassAttributes.Length;
			
			if (numTypes > 0)
			{
				types = new Type[numTypes];
				
				for (int i = 0; i < numTypes; i++)
				{
					CommandClassAttribute commandClassAttr = commandClassAttributes[i] as CommandClassAttribute;

					if (commandClassAttr != null)
					{
						types[i] = commandClassAttr.Type;
					}
				}
			}
			else
			{
				// If we're only looking for specifically
				// marked CommandClasses, then use an
				// empty list ??????????????????

				types = asm.GetExportedTypes();
			}
			
			foreach (Type type in types)
			{
				if (type == null)
				{
					continue;
				}

				MethodInfo[] methods = type.GetMethods();
				
				foreach (MethodInfo method in methods)
				{
					System.Collections.Generic.IEnumerable<CommandMethodAttribute> commandMethodAttributes = method.GetCustomAttributes<CommandMethodAttribute>(true);
					System.Collections.Generic.IEnumerable<CommandMethodDescription> commandMethodDescriptions = method.GetCustomAttributes<CommandMethodDescription>(true);

					string methodDescription = string.Empty;

					foreach (CommandMethodDescription commandMethodDescription in commandMethodDescriptions)
					{
						methodDescription = commandMethodDescription.Description;
					}

					const string indent = "    > ";

					foreach (CommandMethodAttribute obj in commandMethodAttributes) 
					{
						stringCollection.Add(obj.GlobalName);

						if (methodDescription != string.Empty)
						{
							stringCollection.Add(indent + methodDescription);
							stringCollection.Add("");
						}
						
						stringCollection.Add("----------");
					}
				}
			}

			string[] ret = new string[stringCollection.Count];
			stringCollection.CopyTo(ret, 0);
			return ret;
		}
	}
}