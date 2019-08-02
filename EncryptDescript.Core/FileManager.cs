using EncryptDecrypLib;
using EncryptDescript.Core.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EncryptDescript.Core
{
    public class FileManager
    {
        private readonly IEncrypterDecrypter _encryperDecryper;
        private List<PassEntity> _passEntityList = new List<PassEntity>();
        
        public FileManager(IEncrypterDecrypter encryperDecryper)
        {
            _encryperDecryper = encryperDecryper;
        }

        #region Public Methods

        public ErrorDescription CrearNuevaPass(string usuario, string descipcion, string sitio, string cuenta, string pass, string pregSecreta, string rtaSecreta, string mailContacto)
        {
            var nuevaPass = new PassEntity()
            {
                Id = GetNextPassEntityId(),
                IsEncrypter = false,
                Usuario = usuario,
                Descripcion = descipcion,
                Sitio = sitio,
                Cuenta = cuenta,
                PassWord = pass,
                PreguntaSecreta = pregSecreta,
                RespuestaSecreta = rtaSecreta,
                MailContacto = mailContacto
            };

            var errorDesc = nuevaPass.IsValid(_encryperDecryper);

            if (!errorDesc.IsError)
                _passEntityList.Add(nuevaPass);

            return errorDesc;
        }

        public ErrorDescription EliminarPass(string cuenta)
        {
            var passEntity = _passEntityList.Where(p => p.Cuenta == cuenta).FirstOrDefault();

            try
            {
                EliminarPassFromList(passEntity);
            }
            catch (Exception ex)
            {
                return new ErrorDescription(true, "Ocurrio un error al eliminar el password. " + ex.Message);
            }

            return new ErrorDescription(false);
        }

        public ErrorDescription EliminarPass(int id)
        {
            var passEntity = _passEntityList.Where(p => p.Id == id).FirstOrDefault();
            
            try
            {
                EliminarPassFromList(passEntity);
            }
            catch(Exception ex)
            {
                return new ErrorDescription(true, "Ocurrio un error al eliminar el password. " + ex.Message);
            }

            return new ErrorDescription(false);
        }

        public async Task<ErrorDescription> SavePassEntitiesToFileAsync(string jsonPassFilePath, string usuario, string encryptPassword)
        {
            var encryptTasks = _passEntityList.Where(p => !p.IsEncrypter)
                .Select(p => p.EncryptPassEntityAsync(_encryperDecryper, encryptPassword)).ToArray();

            Task.WaitAll(encryptTasks);

            if (_passEntityList.Any(p => !p.IsEncrypter))
                return new ErrorDescription(true, "Algunas contraseñas no puedieron encriptarse");

            string passEntitiesJson = JsonConvert.SerializeObject(_passEntityList);

            try
            {
                Directory.CreateDirectory(jsonPassFilePath);

                string fullPath = jsonPassFilePath + usuario + "_Pass.txt";
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                using (StreamWriter sw = File.CreateText(fullPath))
                    await sw.WriteLineAsync(passEntitiesJson);
            }
            catch (Exception ex)
            {
                return new ErrorDescription(true, "No se puedo guardar las modificaciones de las passwords. Excepcion: " + ex.Message);
            }

            return new ErrorDescription(false);
        }

        /// <summary>
        /// Dada una ruta, un nombre de usuario y una password este metodo carga el json de las passwords desencriptadas dentro del field privado _passEntityList.
        /// En caso de que el archivo json no exista, este metodo lo crea.
        /// Luego para obtener una lista de las passwords de usuario se debe de utilizar el metodo GetPassEntitiesForUser()
        /// </summary>
        /// <param name="jsonPassFilePath">path al archivo al archivo json donde estan/deberian estar las passwords entities</param>
        /// <param name="usuario">usuario propietario de las passwords entities</param>
        /// <param name="encryptPassword">password utilizada para encryptar/descencryptar las passwords</param>
        /// <param name="decryptPassEntities">true: descencripta todas las passwords entities existentes en el json</param>
        /// <returns></returns>
        public async Task<ErrorDescription> LoadJsonPassEntitiesFileAsync(string jsonPassFilePath, string usuario, string encryptPassword, bool decryptPassEntities)
        {
            string passEntitiesJson = "";
            
            try
            {
                string fullPath = jsonPassFilePath + usuario + "_Pass.txt";
                if (!File.Exists(fullPath))
                {
                    var errorDesc = await SavePassEntitiesToFileAsync(jsonPassFilePath, usuario, encryptPassword);
                    if (errorDesc.IsError)
                        throw new Exception("Ocurrio un error al generar el Json de las pass entities. " + errorDesc.Descripcion);
                }

                using (StreamReader sr = File.OpenText(fullPath))
                    passEntitiesJson = await sr.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                return new ErrorDescription(true, "No se puedo cargar el archivo Json de las pass entitites. Excepcion: " + ex.Message);
            }

            _passEntityList = JsonConvert.DeserializeObject<List<PassEntity>>(passEntitiesJson);

            if (decryptPassEntities)
            {
                var encryptTasks = _passEntityList.Select(p => p.DescryptPassEntity(_encryperDecryper, encryptPassword)).ToArray();
                Task.WaitAll(encryptTasks);
            }

            return new ErrorDescription(false);
        }

        public List<PassEntity> GetPassEntitiesForUser()
        {
            return _passEntityList;
        }

        /// <summary>
        /// Retorna una lista de caracteres validos que pueden ser utilizados por el usuario para crear passwords
        /// </summary>
        /// <returns></returns>
        public char[] GetValidCharacters()
        {
            return _encryperDecryper.GetValidCharacters();
        }

        public async Task<ErrorDescription> GenerarArchivoDescencriptadoAsync(string destinyPath, string usuario)
        {
            try
            {
                Directory.CreateDirectory(destinyPath);
                string fullPath = destinyPath + usuario + "_Pass.csv";

                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                using (StreamWriter sw = File.CreateText(fullPath))
                {
                    await sw.WriteLineAsync("Descripcion,Cuenta,Password,Preg Secreta,Rta Secreta, Mail Contacto");
                    foreach (var pas in _passEntityList)
                        await sw.WriteLineAsync(pas.Descripcion + "," + pas.Cuenta + "," + pas.PassWord + "," + pas.PreguntaSecreta + "," + pas.RespuestaSecreta + "," + pas.MailContacto);
                }
            }
            catch(Exception ex)
            {
                return new ErrorDescription(true, "Ocurrio un error al generar el archivo. " + ex.Message);
            }

            return new ErrorDescription(false);
        }

        public ErrorDescription IsEncryptDecryptPassOk(string encryptPassword)
        {
            if (!_passEntityList.Any())
                return new ErrorDescription(false);

            return _passEntityList.FirstOrDefault().IsEncryptDecryptPassOk(_encryperDecryper, encryptPassword);
        }

        /// <summary>
        /// Valida si existe un json con las passwords entities que pertenezca al usuario dado
        /// </summary>
        /// <param name="jsonPassFilePath"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ErrorDescription IsValidUserName(string jsonPassFilePath, string userName)
        {
            DirectoryInfo d = new DirectoryInfo(jsonPassFilePath);
            var filesNames = d.GetFiles("*.txt").Select(f => f.Name.Replace("_Pass.txt",""));
            var existUser = filesNames.Where(n => n == userName).Any();

            ErrorDescription errorDesc = new ErrorDescription(false);
            if (!existUser)
                errorDesc = new ErrorDescription(true, "El nombre de usuario ya existe");

            return errorDesc;
        }

        /// <summary>
        /// Valida si existe un json con las passwords entities que pertenezca al usuario dado. Corre asyncronicamente
        /// </summary>
        /// <param name="jsonPassFilePath"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<ErrorDescription> IsValidUserNameAsync(string jsonPassFilePath, string userName)
        {
            return await Task.Run(()=>
            {
                return IsValidUserName(jsonPassFilePath, userName);
            });
        }

        #endregion

        #region Private Methods

        private void EliminarPassFromList(PassEntity passEntity)
        {
            if (passEntity == null)
                throw new Exception("passEntity no puede ser null");
            _passEntityList.Remove(passEntity);
        }

        private int GetNextPassEntityId()
        {
            int? id = _passEntityList.OrderByDescending(p => p.Id).FirstOrDefault()?.Id;
            if (id == null)
                id = 1;
            else
                id++;

            return (int)id;
        }

        #endregion

    }
}
