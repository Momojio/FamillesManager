using Autodesk.Revit.UI;
using FamillesManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamillesManager.SysCache
{
    internal class SysCache
    {
        private static SysCache _instance;

        public static SysCache Instance
        {
            get
            {
                //if (_instance == null)
                if (ReferenceEquals(_instance, null))
                {
                    _instance = new SysCache();
                }
                return _instance;
            }
        }

        public string CurrentFileLocation { get; set; }
        public List<string> allList { get; set; }
        public ExternalEvent LoadEvent { get; set; }


    }
}
