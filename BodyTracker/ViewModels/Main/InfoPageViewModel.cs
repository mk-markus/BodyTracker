using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    public partial class InfoPageViewModel : ObservableObject
    {

        [ObservableProperty] private string license = string.Empty;
        [ObservableProperty] private string additionalLicense = string.Empty;


        public InfoPageViewModel() 
        {
            LoadDefaultLicense();
            LoadAdditionalLicense();
        }

       

        private void LoadDefaultLicense()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "./License/License.txt");

            Debug.WriteLine(path);

            if (File.Exists(path)) License = File.ReadAllText(path);
            else License = "No File";
        }

        private void LoadAdditionalLicense()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "./License/AdditionalLicense.txt");

            Debug.WriteLine(path);

            if (File.Exists(path)) AdditionalLicense = File.ReadAllText(path);
            else AdditionalLicense = "No File";
        }






    }
}
