namespace System.Reflection
{
    /// <summary>
    /// Provides information about the current assembly.
    /// </summary>
    public static class AssemblyInfo
    {
        private static Assembly? s_assembly;
        /// <summary>
        /// Gets the current assembly. If not already set, it retrieves 
        /// the entry assembly or executing assembly.
        /// </summary>
        public static Assembly Assembly
        {
            get
            {
                s_assembly ??= Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                return s_assembly;
            }
        }

        private static string? s_company;
        /// <summary>
        /// Gets the company name from the assembly's attributes.
        /// </summary>
        public static string? Company
        {
            get
            {
                s_company ??= (Attribute.GetCustomAttribute(Assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute)?.Company;
                return s_company;
            }
        }

        private static string? s_copyright;
        /// <summary>
        /// Gets the copyright information from the assembly's attributes.
        /// </summary>
        public static string? Copyright
        {
            get
            {
                s_copyright ??= (Attribute.GetCustomAttribute(Assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute)?.Copyright;
                return s_copyright;
            }
        }

        private static string? s_product;
        /// <summary>
        /// Gets the product name from the assembly's attributes.
        /// </summary>
        public static string? Product
        {
            get
            {
                s_product ??= (Attribute.GetCustomAttribute(Assembly, typeof(AssemblyProductAttribute)) as AssemblyProductAttribute)?.Product;
                return s_product;
            }
        }

        private static string? s_title;
        /// <summary>
        /// Gets the title of the assembly from the assembly's attributes.
        /// </summary>
        public static string? Title
        {
            get
            {
                s_title ??= (Attribute.GetCustomAttribute(Assembly, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute)?.Title;
                return s_title;
            }
        }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        internal static AssemblyName AssemblyName { get; } = Assembly.GetName();

        /// <summary>
        /// Gets the version of the assembly.
        /// </summary>
        public static Version? Version { get; } = AssemblyName.Version;
    }
}
