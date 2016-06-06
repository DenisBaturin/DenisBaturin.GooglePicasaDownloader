using System;
using System.Reflection;

namespace DenisBaturin.GooglePicasaDownloader
{
    public class AssemblyInfoHelper
    {
        private readonly Assembly _assembly;

        public AssemblyInfoHelper(Assembly assembly)
        {
            _assembly = assembly;
        }

        public string Name
            => _assembly.GetName().Name;

        public string FullName
            => _assembly.GetName().FullName;

        public string CodeBase
            => _assembly.CodeBase;

        public string Version
            => _assembly.GetName().Version.ToString();

        public string Copyright
            => ((AssemblyCopyrightAttribute) GetCustomAttribute(typeof (AssemblyCopyrightAttribute))).Copyright;

        public string Company
            => ((AssemblyCompanyAttribute) GetCustomAttribute(typeof (AssemblyCompanyAttribute))).Company;

        public string Description
            => ((AssemblyDescriptionAttribute) GetCustomAttribute(typeof (AssemblyDescriptionAttribute))).Description;

        public string Product
            => ((AssemblyProductAttribute) GetCustomAttribute(typeof (AssemblyProductAttribute))).Product;

        public string Title
            => ((AssemblyTitleAttribute) GetCustomAttribute(typeof (AssemblyTitleAttribute))).Title;

        private Attribute GetCustomAttribute(Type type)
        {
            var customAttributes = _assembly.GetCustomAttributes(type, false);
            var customAttribute = (Attribute) customAttributes[0];
            return customAttribute;
        }
    }
}
