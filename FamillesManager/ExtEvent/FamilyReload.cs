using Autodesk.Revit.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Document = Autodesk.Revit.DB.Document;

namespace FamillesManager.ExtEvent
{

    public class FamilyReload : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            UIDocument uidoc = app.ActiveUIDocument;

            string Path = SysCache.SysCache.Instance.CurrentFileLocation;
            string Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            if (string.IsNullOrEmpty(Path))
            {
                TaskDialog.Show("Error", "No family file path specified in cache.");
                return;
            }

            // II.Load the family into the document
            Family family = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .FirstOrDefault(f => f.Name == Name) as Family;

            if (family == null)
            {
                try
                {
                    // Transaction and reload
                    Transaction tx = new Transaction(doc, "Reload Family");
                    tx.Start();
                    doc.LoadFamily(Path, out family);
                    TaskDialog.Show("Success", $"Family '{Name}' reloaded successfully.");
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", $"Failed to reload family '{Name}': {ex.Message}");
                    //add to log    
                    System.IO.File.AppendAllText("FamillesManager.log", $"{DateTime.Now}: Failed to reload family '{Name}': {ex.Message}\n");
                }
            }

            FamilySymbol familySymbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;

            //Start to pose
            try
            {
                uidoc.PromptForFamilyInstancePlacement(familySymbol);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                TaskDialog.Show("Error", $"Failed to place family instance '{Name}': {ex.Message}");
                //add to log    
                System.IO.File.AppendAllText("FamillesManager.log", $"{DateTime.Now}: Failed to place family instance '{Name}': {ex.Message}\n");
            }
        }
        public string GetName()
        {
            return "FamilyReload";
        }
    }

}
