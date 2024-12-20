﻿using GeneralUpdate.Core.Domain.DO;
using GeneralUpdate.Core.Events;
using GeneralUpdate.Core.Events.OSSArgs;
using GeneralUpdate.Maui.OSS.Domain.Entity;
using GeneralUpdate.Maui.OSS.Strategys;
using GeneralUpdate.Zip;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace GeneralUpdate.Maui.OSS
{
    /// <summary>
    /// All the code in this file is only included on Windows.
    /// </summary>
    public class Strategy : AbstractStrategy
    {
        private readonly string _appPath = AppDomain.CurrentDomain.BaseDirectory;
        private ParamsAndroid _parameter;

        public override void Create<T>(T parameter)
        {
            _parameter = parameter as ParamsAndroid;
        }

        public override async Task Execute()
        {
            try
            {
                //1.Download the JSON version configuration file.
                var jsonUrl = $"{_parameter.Url}/{_parameter.VersionFileName}";
                var jsonPath = Path.Combine(_appPath, _parameter.VersionFileName);
                await DownloadFileAsync(jsonUrl, jsonPath, (e, s) => EventManager.Instance.Dispatch<Action<object, OSSDownloadArgs>>(this, new OSSDownloadArgs(e, s)));
                if (!File.Exists(jsonPath)) throw new FileNotFoundException(jsonPath);

                //2.Parse the JSON version configuration file content.
                byte[] jsonBytes = File.ReadAllBytes(jsonPath);
                string json = Encoding.Default.GetString(jsonBytes);
                var versions = JsonConvert.DeserializeObject<List<VersionConfigDO>>(json);
                if (versions == null) throw new NullReferenceException(nameof(versions));

                //3.Compare with the latest version.
                versions = versions.OrderBy(v => v.PubTime).ToList();
                var currentVersion = new Version(_parameter.CurrentVersion);
                var lastVersion = new Version(versions[0].Version);
                if (currentVersion.Equals(lastVersion)) return;

                foreach (var version in versions)
                {
                    string filePath = _appPath;
                    var factory = new GeneralZipFactory();
                    factory.CreateOperate(Zip.Factory.OperationType.GZip, version.Name, _appPath, _appPath, true, Encoding.UTF8);
                    //factory.Completed += (s, e) => EventManager.Instance.Dispatch<UnZipCompletedEventHandler>(this, e);
                    //factory.UnZipProgress += (s, e) => EventManager.Instance.Dispatch<UnZipProgressEventHandler>(this, e);
                    factory.UnZip();
                }

                //5.Launch the main application.
                string appPath = Path.Combine(_appPath, _parameter.Apk);
                Process.Start(appPath);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
            finally
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}