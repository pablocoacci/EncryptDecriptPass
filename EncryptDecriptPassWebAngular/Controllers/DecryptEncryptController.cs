using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EncryptDescript.Core;
using Microsoft.AspNetCore.Mvc;

namespace EncryptDecriptPassWebAngular.Controllers
{
    public class RquestTest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    [Route("api/[controller]")]
    public class DecryptEncryptController : Controller
    {
        private readonly string _passEntitiesFilePath = @"C:\\TestProjects\\EncryptDecript\\PassUserPasswords\\";

        [HttpPost("[action]")]
        public JsonResult LoginUser([FromBody] RquestTest rq)
        {
            EncryptDecrypLib.IEncrypterDecrypter ed = new EncryptDecrypLib.Encrypters.EncrypterVigenere();
            FileManager fileManager = new FileManager(ed);
            var errorDescLoad = fileManager.LoadJsonPassEntitiesFileAsync(_passEntitiesFilePath, rq.UserName, rq.Password, true).Result;

            return Json(errorDescLoad);
        }
        
    }
}