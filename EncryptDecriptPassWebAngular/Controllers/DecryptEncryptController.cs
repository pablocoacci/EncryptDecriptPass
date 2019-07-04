using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EncryptDescript.Core;
using Microsoft.AspNetCore.Mvc;

namespace EncryptDecriptPassWebAngular.Controllers
{
    [Route("api/[controller]")]
    public class DecryptEncryptController : Controller
    {
        private readonly string _passEntitiesFilePath = @"C:\\TestProjects\\EncryptDecript\\PassUserPasswords\\";

        [HttpGet("[action]")]
        public JsonResult LoginUser(string userName, string password)
        {
            EncryptDecrypLib.IEncrypterDecrypter ed = new EncryptDecrypLib.Encrypters.EncrypterVigenere();
            FileManager fileManager = new FileManager(ed);
            var errorDescLoad = fileManager.LoadJsonPassEntitiesFileAsync(_passEntitiesFilePath, userName, password, true).Result;

            return Json(errorDescLoad);
        }
    }
}