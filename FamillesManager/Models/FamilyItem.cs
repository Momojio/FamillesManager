using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FamillesManager.Models
{
    public class FamilyItem
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public BitmapSource IconPath { get; set; }

    }

}
