using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.IO;

namespace XamarinBlob
{
    public partial class MainPage : ContentPage
    {
        MediaFile file;
        static string _storageConnection = "DefaultEndpointsProtocol=https;AccountName=xamarinblob;AccountKey=4bVzkyGnKQrXsJphtzmjnBy0RoyQLgRC8WEvh6ecc9RbWocEMmUXY6df5pXEJPifSDLj6MawSg0aKD2APHWXwQ==;EndpointSuffix=core.windows.net";
        static  CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnection);
        static CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        static CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("images");
        
        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnPick_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            try
            {
                file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (file == null)
                    return;
                imgChoosed.Source = ImageSource.FromStream(() =>
                {
                    var imageStram = file.GetStream();
                    return imageStram;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void BtnStore_Clicked(object sender, EventArgs e)
        {

            string filePath = file.Path;
            string fileName = Path.GetFileName(filePath);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
            var blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
            await UploadImage(blockBlob, filePath);
        }

        private async void BtnGet_Clicked(object sender, EventArgs e)
        {
            string filePath = file.Path;
            string fileName = Path.GetFileName(filePath);
            var blockBlob = cloudBlobContainer.GetBlockBlobReference("xm.png");
            await DownloadImage(blockBlob, filePath);
        }

        private async void BtnDelete_Clicked(object sender, EventArgs e)
        {
            var blockBlob = cloudBlobContainer.GetBlockBlobReference("xm.png");
            await DeleteImage(blockBlob);
        }

        private static async Task UploadImage(CloudBlockBlob blob, string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                await blob.UploadFromStreamAsync(fileStream);
            }
        }

        private static async Task DownloadImage(CloudBlockBlob blob, string filePath)
        {
            if (blob.ExistsAsync().Result)
            {
                await blob.DownloadToFileAsync(filePath, FileMode.CreateNew);
            }
        }

        private static async Task DeleteImage(CloudBlockBlob blob)
        {
            if (blob.ExistsAsync().Result)
            {
                await blob.DeleteAsync();
            }
        }
    }
}
