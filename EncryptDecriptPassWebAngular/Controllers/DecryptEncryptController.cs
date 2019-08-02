using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EncryptDecriptPassWebAngular.DTO.DecryptEncrypt;
using EncryptDecriptPassWebAngular.WebHelpers;
using EncryptDescript.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EncryptDecriptPassWebAngular.Controllers
{
    [Route("api/[controller]")]
    public class DecryptEncryptController : Controller
    {
        //private const string _fileManagerKey = "_fileManager";
        private const string _userNamerKey = "_userName";
        private const string _userPasswordKey = "_userPass";
        
        private readonly string _passEntitiesFilePath = @"C:\TestProjects\EncryptDecriptPass\PassUserPasswords\";
        
        [HttpPost("[action]")]
        public async Task<JsonResult> LoginUser([FromBody] LogginRequest rq)
        {
            EncryptDecrypLib.IEncrypterDecrypter ed = new EncryptDecrypLib.Encrypters.EncrypterVigenere();
            FileManager fileManager = new FileManager(ed);

            //Se verifica que el usuario exista
            var errorDescLoad = await fileManager.IsValidUserNameAsync(_passEntitiesFilePath, rq.UserName);
            
            if (!errorDescLoad.IsError)
            {
                //Se cargan las pass entities y se guardan en variables de session
                errorDescLoad = await fileManager.LoadJsonPassEntitiesFileAsync(_passEntitiesFilePath, rq.UserName, rq.Password, false);
                //HttpContext.Session.Set<FileManager>(_fileManagerKey, fileManager);
                HttpContext.Session.SetString(_userNamerKey, rq.UserName);
                HttpContext.Session.SetString(_userPasswordKey, rq.Password);
                FileManagersWarehouse.InsertFileManagerInWarehouse(rq.UserName, rq.Password, fileManager);
            }

            return Json(errorDescLoad);
        }
        
        [HttpGet("[action]")]
        public JsonResult GetPasswordsEntities()
        {
            //var fileManager = HttpContext.Session.Get<FileManager>(_fileManagerKey);
            var fileManager = FileManagersWarehouse.GetFileManager(HttpContext.Session.GetString(_userNamerKey), HttpContext.Session.GetString(_userPasswordKey));
            var passEntitiesList = fileManager.GetPassEntitiesForUser();
            return Json(passEntitiesList);
        }
       
    }
}