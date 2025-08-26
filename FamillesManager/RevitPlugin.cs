using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FamillesManager.UI;
using FamillesManager.ExtEvent;
using RevitStorage;
using MaterialDesignThemes.Wpf;

namespace FamillesManager
{
    [Transaction(TransactionMode.Manual)]
    internal class RevitPlugin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //PlatformType platform = PlatformType.x64;
                Theme theme = new Theme();

                FamilyReload familyReload = new FamilyReload();
                SysCache.SysCache.Instance.LoadEvent = ExternalEvent.Create(familyReload);

                // Show the main window
                MainWindow mainWindow = new MainWindow();
                mainWindow.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
