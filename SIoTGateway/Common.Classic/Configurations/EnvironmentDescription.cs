using System;
using System.Globalization;
using System.IO;
using System.Xml;
#if !WINDOWS_UWP
using System.Xml.XPath;
#endif
#if WINDOWS_UWP
using Windows.Data.Xml.Dom;
using Windows.Storage;
using System.Xml.Linq;
using Microsoft.Data.OData.Query.SemanticAst;
using System.Threading.Tasks;
#endif

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations
{
    public class EnvironmentDescription : IDisposable
    {
        bool isDisposed = false;
        XmlDocument document = null;
        #if !WINDOWS_UWP
        XPathNavigator navigator = null;
#endif
        string fileName = null;
        int updatedValuesCount = 0;
        const string ValueAttributeName = "value";
        const string SettingXpath = "//setting[@name='{0}']";

        public EnvironmentDescription(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            this.fileName = fileName;
            this.document = new XmlDocument();
#if !WINDOWS_UWP
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                this.document.Load(reader);
            }
            this.navigator = this.document.CreateNavigator();
#endif
#if WINDOWS_UWP
            this.document.LoadXml(fileName);
#endif
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.isDisposed = true;
                if (this.updatedValuesCount > 0)
                {
#if !WINDOWS_UWP
                    this.document.Save(this.fileName);
                    Console.Out.WriteLine("Successfully updated {0} mapping(s) in {1}", this.updatedValuesCount, Path.GetFileName(this.fileName).Split('.')[0]);
#endif
#if WINDOWS_UWP
                    var file = ApplicationData.Current.RoamingFolder.CreateFileAsync(this.fileName, CreationCollisionOption.ReplaceExisting);
                    this.document.SaveToFileAsync((IStorageFile)file).AsTask().Wait();
#endif
                }
            }
        }

        public string GetSetting(string settingName, bool errorOnNull = true)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new ArgumentNullException("settingName");
            }

            string result = string.Empty;
#if !WINDOWS_UWP
            XmlNode node = this.GetSettingNode(settingName.Trim());
#endif
#if WINDOWS_UWP
            IXmlNode node = this.GetSettingNode(settingName.Trim());
#endif
            if (node != null)
            {
#if !WINDOWS_UWP
                result = node.Attributes[ValueAttributeName].Value;
#endif
#if WINDOWS_UWP
                result = node.Attributes.GetNamedItem(ValueAttributeName).NodeValue.ToString();
#endif
            }
            else
            {
                if (errorOnNull)
                {
                    var message = string.Format(CultureInfo.InvariantCulture, "{0} was not found", settingName);
                    throw new ArgumentException(message);
                }
            }
            return result;
        }

#if !WINDOWS_UWP
        XmlNode GetSettingNode(string settingName)

#endif
#if WINDOWS_UWP
        IXmlNode GetSettingNode(string settingName)
#endif
        {
            string xpath = string.Format(CultureInfo.InvariantCulture, SettingXpath, settingName);
            return this.document.SelectSingleNode(xpath);
        }

#if WINDOWS_UWP
        public EnvironmentDescription()
        {

        }

        public async Task Load(string file)
        {
            try
            {
                StorageFile tileTemplateFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{file}"));

                string fileContent;
                using (StreamReader reader = new StreamReader(await tileTemplateFile.OpenStreamForReadAsync()))
                {
                    fileContent = await reader.ReadToEndAsync();

                    this.document = new XmlDocument();
                    this.document.LoadXml(fileContent);
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"-> {e.Message}");
            }
        }
#endif
    }
}
