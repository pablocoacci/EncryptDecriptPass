using EncryptDescript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EncryptDecriptPassWebAngular.WebHelpers
{
    public static class FileManagersWarehouse
    {
        private static Dictionary<string, FileManager> _warehouse = new Dictionary<string, FileManager>();
        
        private static string GenerateKey(string userName, string userPass)
            => userName + "_" + userPass;

        public static void InsertFileManagerInWarehouse(string userName, string userPass, FileManager fileManager)
        {
            var key = GenerateKey(userName, userPass);
            _warehouse.Add(key, fileManager);
        }

        public static FileManager GetFileManager(string userName, string userPass)
        {
            var key = GenerateKey(userName, userPass);
            return _warehouse[key];
        }
    }
}
