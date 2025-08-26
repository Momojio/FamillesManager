using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FamillesManager.Models;
using RevitStorage.StructuredStorage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace FamillesManager.UI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    /// 


    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<FamilyItem> _families;
        public ObservableCollection<FamilyItem> Families
        {
            get => _families;
            set
            {
                _families = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            //Families = new ObservableCollection<FamilyItem>();
        }
    }


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new MainViewModel();
            }

            //Initialize pagination options
            foreach (int RecordGroup in pagedTable.ItemsToShow)
            {
                PageSelector.Items.Add(RecordGroup); 
            }

            PageSelector.SelectedIndex = 0;
        }

        // I.Event handler for the Browse button click
        private List<string> fileList = new List<string>(); //originally List<string> to hold file paths
        private Paging pagedTable = new Paging();
        //private ObservableCollection<FamilyItem> myList = new ObservableCollection<FamilyItem>(); //After transformation, this will be an ObservableCollection<FamilyItem>

        private void LibraryPathTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string path = LibraryPathTextBox.Text;
                LoadFamiliesFromPath(path);
            }
        }

        private void LoadFamiliesFromPath(string path)
        {
            if (Directory.Exists(path))
            {
                fileList = Directory.GetFiles(path, "*.rfa", SearchOption.AllDirectories).ToList();
                if (fileList.Count == 0)
                {
                    MessageBox.Show("Aucun fichier .rfa trouvé dans le dossier spécifié");
                }
                var viewModel = (MainViewModel)DataContext;
                SysCache.SysCache.Instance.allList = fileList; //cache all file paths
                var allFamilies = CreateFamilyCollection(fileList);
                viewModel.Families = new ObservableCollection<FamilyItem>(allFamilies);
                InitPaging(allFamilies);
                MessageBox.Show($"Trouvé {fileList.Count} fichier(s) .rfa dans le dossier");
            }
            else
            {
                MessageBox.Show("Le chemin spécifié n'existe pas. Veuillez vérifier et réessayer.");
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Sélectionnez un dossier contenant des familles .rfa";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Assuming you have a TextBox named 'LibraryPathTextBox' to display the selected folder path
                LibraryPathTextBox.Text = dlg.SelectedPath;
                // Step 2: Update the UI with the selected folder path
                LoadFamiliesFromPath(dlg.SelectedPath);

            }
        }





        private ObservableCollection<FamilyItem> CreateFamilyCollection(List<string> files)
        {
            return new ObservableCollection<FamilyItem>(files.Select(file => 
            {
                using (Storage storage = new Storage(file))
                {
                    return new FamilyItem
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        FilePath = file,
                        IconPath = GetImageStream(storage.ThumbnailImage.GetPreviewAsImage())// Assuming ThumbnailImage.Image is a System.Drawing.Image.
                    };
                 }
            }));
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public BitmapSource GetImageStream(System.Drawing.Image myImage)
        {
            var bitmap = new Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());


            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }

        // II.Event handler for the Search button click
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = Rechercher.Text.Trim();
            SearchFamily(searchText);
        }
        private void Rechercher_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string searchText = Rechercher.Text.Trim();
                SearchFamily(searchText);
            }
        }
        private void SearchFamily(string searchText)
        {
            var allList = SysCache.SysCache.Instance.allList;
            var pagedList = allList.Where(f => Path.GetFileNameWithoutExtension(f).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            if (pagedList.Count > 0)
            {
                var viewModel = (MainViewModel)DataContext;
                viewModel.Families = CreateFamilyCollection(pagedList);
                InitPaging(viewModel.Families);
            }
            else
            {
                MessageBox.Show($"Aucun fichier ne correspond à '{searchText}'");
            }
        }


        // III.Event handler for loading
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FamilyItem data = ((Button)sender).Tag as FamilyItem;
                SysCache.SysCache.Instance.CurrentFileLocation = data.FilePath;
                SysCache.SysCache.Instance.LoadEvent.Raise();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création de l'ExternalEvent: {ex.Message}");
                return;
            }

        }


        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            ItemsList.ItemsSource = pagedTable.LastPage(((MainViewModel)DataContext).Families.ToList());
            UpdatePageDisplay();
        }

        private void Firstpage_Click(object sender, RoutedEventArgs e)
        {
            ItemsList.ItemsSource = pagedTable.FirstPage(((MainViewModel)DataContext).Families.ToList());
            UpdatePageDisplay();    

        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            ItemsList.ItemsSource = pagedTable.PreviousPage(((MainViewModel)DataContext).Families.ToList());
            UpdatePageDisplay();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            ItemsList.ItemsSource = pagedTable.NextPage(((MainViewModel)DataContext).Families.ToList());
            UpdatePageDisplay();

        }


        private void InitPaging(ObservableCollection<FamilyItem> allFamilies)
        {
            pagedTable.ItemsPerPage = (int)PageSelector.SelectedItem;
            pagedTable.TotalItems = allFamilies.Count;

            ItemsList.ItemsSource = pagedTable.FirstPage(allFamilies.ToList());
            UpdatePageDisplay();

        }

        private void UpdatePageDisplay()
        {
            PageInfoTextBlock.Text = $"Page {pagedTable.CurrentPage} sur {pagedTable.TotalPages}";
        }   


        private void PageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSelector.SelectedItem != null)
            {
                var viewModel = (MainViewModel)DataContext;
                if (viewModel.Families != null && viewModel.Families.Any())
                {
                    pagedTable.ItemsPerPage = (int)PageSelector.SelectedItem;
                    var pagedData = pagedTable.FirstPage(viewModel.Families.ToList());
                    ItemsList.ItemsSource = pagedData;
                    UpdatePageDisplay();
                }
            }
        }

        private void Rechercher_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }
}
