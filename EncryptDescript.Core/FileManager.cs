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

        public char[] GetValidCharacters()
        {
            return _encryperDecryper.GetValidCharacters();
        }

        public async Task<ErrorDescription> GenerarArchivoDescencriptado(string destinyPath, string usuario)
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
