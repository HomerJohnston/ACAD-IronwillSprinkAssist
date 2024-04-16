using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD;
using global::Autodesk.AutoCAD.DatabaseServices;
using global::Autodesk.AutoCAD.Internal;
using System;

namespace Ironwill.Helpers
{
	/*
	namespace Autodesk.AutoCAD.DatabaseServices
	{
		public class UndoGroupTransaction : Transaction
		{
			protected internal UndoGroupTransaction(Transaction tr)
				: base(tr.UnmanagedObject, tr.AutoDelete)
			{
				Runtime.Interop.DetachUnmanagedObject(tr);
				GC.SuppressFinalize(tr);
				Utils.SetUndoMark(true);
			}

			protected override void Dispose(bool A_1)
			{
				base.Dispose(A_1);
				if (A_1)
					Utils.SetUndoMark(false);
			}
		}

		public static class TransactionManagerExtension
		{
			public static UndoGroupTransaction StartUndoGroupTransaction(this TransactionManager mgr)
			{
				return new UndoGroupTransaction(mgr.StartTransaction());
			}
		}
	}
	*/
}
