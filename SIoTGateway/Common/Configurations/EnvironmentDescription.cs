using System;
using System.Globalization;
using System.IO;
using System.Xml;
//using System.Xml.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations
{
    public class EnvironmentDescription : IDisposable
    {
        bool isDisposed = false;
        XmlDocument document = null;
        //XPathNavigator navigator = null;
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
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                this.document.Load(reader);
            }
            //this.navigator = this.document.CreateNavigator();
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
                    //this.document.Save(this.fileName);
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
            XmlNode node = this.GetSettingNode(settingName.Trim());
            if (node != null)
            {
                result = node.Attributes[ValueAttributeName].Value;
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

        XmlNode GetSettingNode(string settingName)
        {
            string xpath = string.Format(CultureInfo.InvariantCulture, SettingXpath, settingName);
            //return this.document.SelectSingleNode(xpath);
            var list = this.document.GetElementsByTagName("setting");
            foreach(XmlNode xn in list)
            {
                var ss = xn.Attributes["name"];
                if(null != ss)
                {
                    if(ss.Value == settingName)
                    {
                        return xn;
                    }
                }
            }

            return null;
        }
    }
}
