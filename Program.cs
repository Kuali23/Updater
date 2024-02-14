// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");
//WriteFile();
Console.WriteLine("Leyendo archivo de version");

/// <summary>
/// Aqui va a funcionar tambien para generar un archivo de 
/// </summary>

Updater.Versions v = new();
var info = await v.CheckForUpdates();

Updater.Downloader d = new(info);
bool success = await d.DownloadAndUpdate();

if (success) 
    v.WriteFileVersion(new Updater.Versions.DecryptedFile() { Id = info.Id, Notes = info.Notes, NumVersion = info.NumVersion });

