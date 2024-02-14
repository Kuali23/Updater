using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Updater
{
    internal class Versions
    {
        private string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly byte[] key = Encoding.UTF8.GetBytes("9j23ad3dkjvfYdhd");
        private readonly byte[] iv = Encoding.UTF8.GetBytes("98jaqnvfda90mlad");



        private DecryptedFile ReadFileVersion()
        {
            string versionFilePath = appDataPath + @"\KitBlue for Desktop\version.dll";
          
            if (!File.Exists(versionFilePath))
                RegenerateFile();

            try
            {
                var bytesFile = File.ReadAllBytes(versionFilePath);
                string stringFile = Security.AesSecurity.DecryptStringFromBytes_Aes(bytesFile, key, iv);
                var file = JsonConvert.DeserializeObject<DecryptedFile>(stringFile);

                if (file is null)
                    throw new ArgumentNullException(nameof(file));
                return file;
            }
            catch
            {
                RegenerateFile();
                return ReadFileVersion();
            }
            

               
        }

        private void RegenerateFile()
        {
            WriteFileVersion(new DecryptedFile() { Id = 0, Notes="", NumVersion = "0" });
        }

        public void WriteFileVersion(DecryptedFile file)
        {
            string versionFilePath = appDataPath + @"\KitBlue for Desktop\version.dll";
            Directory.CreateDirectory(appDataPath+ @"\KitBlue for Desktop\");
            string stringFile = JsonConvert.SerializeObject(file);

            var fileStream = Security.AesSecurity.EncryptStringToBytes_Aes(stringFile, key, iv);
            File.WriteAllBytes(versionFilePath, fileStream); 
        }


        private async Task<DecryptedFile?> RemoteLastVersion()
        {
            ApiJWTAaron.aaronJWT request = new();
            request.TypeProccess = "getLastVersion";
            request.Data = new System.Dynamic.ExpandoObject();

            try
            {
                if (!await request.Execute().ConfigureAwait(false))
                    return null;

                
                var response = request.GetResponseAsObject();
                System.Diagnostics.Trace.WriteLine(request.GetResponseAsString());
                var de = new DecryptedFile() { Notes = response.data.notas, NumVersion = response.data.version, Id = int.Parse((string)response.data.id) };
                return de;
            }
            catch(Exception er)
            {
                System.Diagnostics.Trace.WriteLine(er.ToString());
                return null;
            }

        }


        public async Task<VersionInfo> CheckForUpdates() {
            //try
            //{
                var local = ReadFileVersion();
                var remote = await RemoteLastVersion();

                //Remote podria ser null cuando fallo la conexion a la API
                if (remote is null || local is null)
                    return new VersionInfo(false);

                //Si las versiones son iguales, entonces no se debe actualizar
                if (remote.NumVersion == local.NumVersion)
                    return new VersionInfo(false);

                ApiJWTAaron.aaronJWT ver = new();
                ver.TypeProccess = "getVersionInfo";
                ver.Data = new System.Dynamic.ExpandoObject();
                ver.Data.idActualizacion = remote.Id;

                if (!await ver.Execute())
                    return new VersionInfo(false);

                System.Diagnostics.Trace.WriteLine(ver.GetResponseAsString());
                var response = ver.GetResponseAsObject();

                var versionInfo = new VersionInfo(true)
                {
                    Id = int.Parse(response.data.id),
                    Notes = response.data.notas,
                    Date = response.data.fecha,
                    NumVersion = remote.NumVersion
                };

                List<FileInfo> files = new();
                List<dynamic> dynamicsFiles = response.data.files;
                dynamicsFiles.ForEach(x =>
                {
                    files.Add(new FileInfo(int.Parse(x.id), x.data64, x.destino));
                });

                versionInfo.Files = files;

                return versionInfo;
            //}
            //catch(Exception er)
            //{
            //    Console.WriteLine(er.ToString());
            //    return new VersionInfo(false);
            //}

        }


        public class DecryptedFile
        {
            public string? NumVersion { get; set; }
            public string? Notes { get; set; }
            public int Id { set; get; }
        }
    }
}
