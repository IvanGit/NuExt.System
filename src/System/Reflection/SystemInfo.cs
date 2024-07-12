namespace System.Reflection
{
    public static class SystemInfo
    {
        private static Assembly? s_thisAssembly;
        public static Assembly ThisAssembly
        {
            get
            {
                s_thisAssembly ??= Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                return s_thisAssembly;
            }
        }

        private static string? s_company;
        public static string? Company
        {
            get
            {
                s_company ??= (Attribute.GetCustomAttribute(ThisAssembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute)?.Company;
                return s_company;
            }
        }

        private static string? s_copyright;
        public static string? Copyright
        {
            get
            {
                s_copyright ??= (Attribute.GetCustomAttribute(ThisAssembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute)?.Copyright;
                return s_copyright;
            }
        }

        private static string? s_product;
        public static string? Product
        {
            get
            {
                s_product ??= (Attribute.GetCustomAttribute(ThisAssembly, typeof(AssemblyProductAttribute)) as AssemblyProductAttribute)?.Product;
                return s_product;
            }
        }

        private static string? s_title;
        public static string? Title
        {
            get
            {
                s_title ??= (Attribute.GetCustomAttribute(ThisAssembly, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute)?.Title;
                return s_title;
            }
        }

        internal static AssemblyName AsmName { get; } = ThisAssembly.GetName();

        public static Version? Version { get; } = AsmName?.Version;
    }
}
