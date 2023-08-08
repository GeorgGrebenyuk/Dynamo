using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace DynamoInstallDetective
{
    /// <summary>
    /// Utility class for install detective
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Finds all unique Dynamo installations on the system
        /// </summary>
        /// <param name="additionalDynamoPath">Additional path for Dynamo binaries
        /// to be included in search</param>
        /// <returns>List of KeyValuePair of install location and version info 
        /// as Tuple. The returned list is sorted based on version info.</returns>
        public static IEnumerable FindDynamoInstallations(string additionalDynamoPath)
        {
            var installs = DynamoProducts.FindDynamoInstallations(additionalDynamoPath);
            return
                installs.Products.Select(
                    p =>
                        new KeyValuePair<string, Tuple<int, int, int, int>>(
                        p.InstallLocation,
                        p.VersionInfo));
        }

        /// <summary>
        /// Finds all unique Dynamo installation on the system that has file 
        /// identifiable by the given fileLocator.
        /// </summary>
        /// <param name="additionalDynamoPath">Additional path for Dynamo binaries
        /// to be included in search</param>
        /// <param name="fileLocator">A callback method to locate dynamo specific files.</param>
        /// <returns>List of KeyValuePair of install location and version info 
        /// as Tuple. The returned list is sorted based on version info.</returns>
        public static IEnumerable LocateDynamoInstallations(string additionalDynamoPath, Func<string, string> fileLocator)
        {
            var installs = DynamoProducts.FindDynamoInstallations(additionalDynamoPath, new InstalledProductLookUp("Dynamo", fileLocator));
            return
                installs.Products.Select(
                    p =>
                        new KeyValuePair<string, Tuple<int, int, int, int>>(
                        p.InstallLocation,
                        p.VersionInfo));
        }

        /// <summary>
        /// Finds all products installed on the system with given product name
        /// search pattern and file name search pattern. e.g. to find Dynamo
        /// installations, we can use Dynamo as product search pattern and
        /// DynamoCore.dll as file search pattern.
        /// </summary>
        /// <param name="productSearchPattern">search key for product</param>
        /// <param name="fileSearchPattern">search key for files</param>
        /// <returns>List of KeyValuePair of product install location and 
        /// version info as Tuple of the file found in the installation based 
        /// on file search pattern. The returned list is sorted based on version 
        /// info.</returns>
        public static IEnumerable FindProductInstallations(string productSearchPattern, string fileSearchPattern)
        {
            var installs = new InstalledProducts();
            installs.LookUpAndInitProducts(new InstalledProductLookUp(productSearchPattern, fileSearchPattern));

            return
                installs.Products.Select(
                    p =>
                        new KeyValuePair<string, Tuple<int, int, int, int>>(
                        p.InstallLocation,
                        p.VersionInfo));
        }

        /// <summary>
        /// Finds all products installed on the system with given product name
        /// search pattern and file name search pattern. e.g. to find Dynamo
        /// installations, we can use Dynamo as product search pattern and
        /// DynamoCore.dll as file search pattern.
        /// </summary>
        /// <param name="productSearchPattern">search keys for product</param>
        /// <param name="fileSearchPattern">search key for files</param>
        /// <returns>List of KeyValuePair of product install location and 
        /// version info as Tuple of the file found in the installation based 
        /// on file search pattern. The returned list is sorted based on version 
        /// info.</returns>
        public static IEnumerable FindMultipleProductInstallations(List<string> productSearchPatterns, string fileSearchPattern)
        {
            using (RegUtils.StartCache())
            {
                var installs = new InstalledProducts();
                // Look up products with ASM installed on user's computer
                foreach (var productSearchPattern in productSearchPatterns)
                {
                    installs.LookUpAndInitProducts(new InstalledProductLookUp(productSearchPattern, fileSearchPattern));
                }

                return
                    installs.Products.Select(
                        p =>
                            new KeyValuePair<string, Tuple<int, int, int, int>>(
                            p.InstallLocation,
                            p.VersionInfo));
            }
        }
        public static IEnumerable FindInternalASMBinaries (string current_directory)
        {
            /*
             ASM_version = 227
            TODO: для будущих реализов сделать учет других версий от более новых версий Autodesk продуктов
             */
            string ASM_folder = Path.Combine(current_directory, "ASM");
            List<KeyValuePair<string, Tuple<int, int, int, int>>> asm_libs = new List<KeyValuePair<string, Tuple<int, int, int, int>>>();
            //TODO: предусмотреть исключение
            if (!Directory.Exists(ASM_folder)) return asm_libs;
            else
            {
                foreach (string sub_dir_path in Directory.GetDirectories(ASM_folder, "*.*", SearchOption.TopDirectoryOnly))
                {
                    string dir_asm_name = new DirectoryInfo(sub_dir_path).Name;
                    Tuple<int, int, int, int> version = null;
                    if (dir_asm_name == "227") version = Tuple.Create(227, 0, 0, 0);
                    else if (dir_asm_name == "228") version = Tuple.Create(228, 0, 0, 0);

                    if (version != null) asm_libs.Add(new KeyValuePair<string, Tuple<int, int, int, int>>(sub_dir_path, version));

                }
                return asm_libs;
            }


        }
    }
}
