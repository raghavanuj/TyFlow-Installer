using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace TyFlow_Installer
{
    internal class RegistryFuncions
    {
        string keyPath = @"SOFTWARE\Autodesk\3dsMax\"; // This is relative to HKEY_LOCAL_MACHINE
        public Dictionary<int, string> maxPaths = new Dictionary<int, string>();

        /// <summary>
        /// This functions reads registry to find all the valid 3ds max installations
        /// </summary>
        /// <returns></returns>
        public Dictionary<int,string> GetMaxInstallLocations()
        {
            // Open the HKLM\SOFTWARE key
            using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (baseKey != null)
                {
                    //prints all max versions listed under autodesk\3ds max registry key
                    Console.WriteLine($"Subkeys under HKLM\\{keyPath}:");
                    // Get all subkey names (all max versions )
                    string[] subkeyNames = baseKey.GetSubKeyNames();
                    // print total number of max versions found
                    Console.WriteLine(subkeyNames.Length);
                    // Iterate  each subkey (max versions)
                    foreach (string subkeyName in subkeyNames)
                    {
                        // read install path of each max installation
                        String  installDir = Registry.LocalMachine.OpenSubKey(keyPath+"\\"+subkeyName).GetValue("Installdir") as string;

                        //maxPaths.Add(CalculateMaxVersion(subkeyName), subkeyName.GetValue(valueName))
                        // Console.WriteLine(CalculateMaxVersion(subkeyName));

                        // check if install dir is not null or blank
                        if (installDir != null && installDir != "" )
                        {
                            maxPaths.Add(CalculateMaxVersion(subkeyName), installDir);
                            Console.WriteLine(CalculateMaxVersion(subkeyName)+": "+installDir);
                        }
                    }
                    return maxPaths;
                }
                else
                {
                    Console.WriteLine($"Key HKLM\\{keyPath} not found.");
                    return null;
                }
                
            }
        }

        /// <summary>
        /// This function converts numerical max version  for ex 24, 25, 26 to year based max version for ex 2022, 2023, 2024 etc.
        /// </summary>
        /// <param name="registryKey"> this is numerical version retrieved from registry </param>        
        /// <returns> returns year based max version number </returns>
        public int CalculateMaxVersion(string registryKey)
        {
           // float ver = float.Parse(registryKey);
            string vers = registryKey.Split('.')[0];// remove .0 from version number
            int seqentialVersion = int.Parse(vers);
            int yearlyVersion  = (seqentialVersion - 2)+2000;
            return yearlyVersion;
        }
    }
}
