using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace Ironwill
{
    public static class EditorExtension
    {
        public static void Zoom(this Editor ed, Extents3d ext)
        {
            if (ed == null)
                throw new ArgumentNullException("ed");
            using (ViewTableRecord view = ed.GetCurrentView())
            {
                Matrix3d worldToEye = Matrix3d.WorldToPlane(view.ViewDirection) *
                    Matrix3d.Displacement(Point3d.Origin - view.Target) *
                    Matrix3d.Rotation(view.ViewTwist, view.ViewDirection, view.Target);
                ext.TransformBy(worldToEye);
                view.Width = ext.MaxPoint.X - ext.MinPoint.X;
                view.Height = ext.MaxPoint.Y - ext.MinPoint.Y;
                view.CenterPoint = new Point2d(
                    (ext.MaxPoint.X + ext.MinPoint.X) / 2.0,
                    (ext.MaxPoint.Y + ext.MinPoint.Y) / 2.0);
                ed.SetCurrentView(view);
            }
        }

        public static void ZoomExtents(this Editor ed)
        {
            Database db = ed.Document.Database;
            db.UpdateExt(false);
            Extents3d ext = (short)Application.GetSystemVariable("cvport") == 1 ?
                new Extents3d(db.Pextmin, db.Pextmax) :
                new Extents3d(db.Extmin, db.Extmax);
            ed.Zoom(ext);
        }
    }
}
