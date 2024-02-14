using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    internal class Downloader
    {

        private readonly VersionInfo _versionInfo;

        public Downloader(VersionInfo version)
        {
            _versionInfo = version;
        }


        public async Task<bool> DownloadAndUpdate()
        {
            if (_versionInfo is null)
                return false;

            var fi = await DownloadFiles();

            if (fi is null) 
                return false;

            var result = SaveFiles(fi);

            if (!result.Success)
                return false;

            if (!MoveFiles(result.Patches))
                return false;

            return true;
            

            //result.Patches.ForEach(x => Console.WriteLine(x));
        }

        /// <summary>
        /// Perfom a request that returns a json with the data in base64 format
        /// </summary>
        /// <returns></returns>
        private async Task<List<dynamic>?> DownloadFiles()
        {
            if (_versionInfo is null)
                return null;

            ApiJWTAaron.aaronJWT request = new();
            request.TypeProccess = "downloadFiles";
            request.Data = new System.Dynamic.ExpandoObject();
            request.Data.archivos = _versionInfo.Files?.Select(x => x.Id).ToList();

            if (!await request.Execute())
                return null;

            System.Diagnostics.Trace.WriteLine(request.GetResponseAsString());
            var response = request.GetResponseAsObject();
            return response.data.files as List<dynamic>;
        }

        private (bool Success, Dictionary<string, string> Patches) SaveFiles(List<dynamic> files)
        {
            Dictionary<string, string> patches = new();
            bool sucess = true;
            
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\KitBlue for Desktop\" + _versionInfo.NumVersion;
            Directory.CreateDirectory(appDataPath);
            

            foreach(var fil in files)
            {
                try
                {
                    byte[] b = Convert.FromBase64String(fil.encodedData);
                    string fileName = appDataPath + @"\" + fil.nombre;
                    File.WriteAllBytes(fileName, b);
                    patches.Add(fil.nombre, fileName);
                }
                catch(Exception er)
                {
                    Console.WriteLine(er.ToString());
                    sucess = false;
                }
            }

            return (sucess, patches);
        }

        private bool MoveFiles(Dictionary<string, string> patches)
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\KitBlue for Desktop\";
            bool success = true;

            foreach(var file in patches)
            {
                try
                {
                    File.Move(file.Value, programFiles+file.Key);
                }
                catch(Exception er)
                {
                    Console.WriteLine(er.ToString());
                    success = false;
                }
            }

            return success;
        }


    }
}
