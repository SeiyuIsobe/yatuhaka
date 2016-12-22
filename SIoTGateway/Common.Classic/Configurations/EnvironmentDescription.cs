using System;
using System.Globalization;
using System.IO;
#if !WINDOWS_UWP
using System.Xml;
using System.Xml.XPath;
#endif
#if WINDOWS_UWP
using Windows.Data.Xml.Dom;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
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
            //InitAsync();
#endif
        }

#if WINDOWS_UWP
        public async Task InitAsync()
        {
            this.document = await XMLHelper.LoadXmlFile("Common", "cloud.settings.xml");
        }
#endif
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
        {
            string xpath = string.Format(CultureInfo.InvariantCulture, SettingXpath, settingName);
            return this.document.SelectSingleNode(xpath);
        }
#endif
#if WINDOWS_UWP
        private IXmlNode GetSettingNode(string settingName)
        {
            string xpath = string.Format(CultureInfo.InvariantCulture, SettingXpath, settingName);
            var list = this.document.GetElementsByTagName("setting");
            foreach (IXmlNode xn in list)
            {
                var ss = xn.Attributes.GetNamedItem("name");
                if (null != ss)
                {
                    if (ss.NodeValue.ToString() == settingName)
                    {
                        return xn;
                    }
                }
            }

            return null;
        }
#endif

    }
}
