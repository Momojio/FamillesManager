using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamillesManager.Models;

namespace FamillesManager.UI
{
    public class Paging
    {
        public int[] ItemsToShow = { 10, 20, 30, 50, 100 };
        public int CurrentPage { get; set; }
        List<FamilyItem> PagedList = new List<FamilyItem>();
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages 
        { 
            get 
            {
                if (ItemsPerPage == 0) return 0;
                return (int)Math.Ceiling((double)TotalItems / ItemsPerPage); 
            } 
        }

        public List<FamilyItem> GetPagedData(List<FamilyItem> fullList, int ItemsPerPage)
        {
            int pageGroup = (CurrentPage - 1) * ItemsPerPage;
            PagedList = fullList.Skip(pageGroup).Take(ItemsPerPage).ToList();
            return PagedList;
        }

        


        public List<FamilyItem> FirstPage(List<FamilyItem> fullList)
        {
            CurrentPage = 1;
            return GetPagedData(fullList, ItemsPerPage);
        }
        public List<FamilyItem> LastPage(List<FamilyItem> fullList)
        {
            CurrentPage = TotalPages;
            return GetPagedData(fullList, ItemsPerPage);
        }
        public List<FamilyItem> PreviousPage(List<FamilyItem> fullList)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
            return GetPagedData(fullList, ItemsPerPage);
        }
        public List<FamilyItem> NextPage(List<FamilyItem> fullList)
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
            return GetPagedData(fullList, ItemsPerPage);
        }

    }
}
