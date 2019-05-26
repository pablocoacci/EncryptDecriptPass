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
        private readonly string _encryptPassword;
        private readonly string _jsonPassFilePath;
        private List<PassEntity> _passEntityList = new List<PassEntity>();

        public FileManager(IEncrypterDecrypter encryperDecryper, string encryptPassword, string jsonPassFilePath)
        {
            _encryperDecryper = encryperDecryper;
            _encryptPassword = encryptPassword;
            _jsonPassFilePath = jsonPassFilePath;
        }

        #region Public Methods

        public ErrorDescription CrearNuevaPass(string usuario, string descipcion, string cuenta, string pass, string pregSecreta, string rtaSecreta, string mailContacto)
        {
            var nuevaPass = new PassEntity()
            {
                Id = GetNextPassEntityId(),
                IsEncrypter = false,
                Usuario = usuario,
                Descripcion = descipcion,
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

        public void EliminarPass(string cuenta)
        {
            var passEntity = _passEntityList.Where(p => p.Cuenta == cuenta).FirstOrDefault();
            EliminarPassFromList(passEntity);
        }

        public void EliminarPass(int id)
        {
            var passEntity = _passEntityList.Where(p => p.Id == id).FirstOrDefault();
            EliminarPassFromList(passEntity);
        }

        public async Task<ErrorDescription> SavePassEntitiesToFileAsync()
        {
            var encryptTasks = _passEntityList.Where(p => !p.IsEncrypter)
                .Select(p => p.EncryptPassEntityAsync(_encryperDecryper, _encryptPassword)).ToArray();

            Task.WaitAll(encryptTasks);

            if (_passEntityList.Any(p => !p.IsEncrypter))
                return new ErrorDescription(true, "Algunas contraseñas no puedieron encriptarse");

            string passEntitiesJson = JsonConvert.SerializeObject(_passEntityList);

            try
            {
                if (File.Exists(_jsonPassFilePath))
                    File.Delete(_jsonPassFilePath);

                using (StreamWriter sw = File.CreateText(_jsonPassFilePath))
                    await sw.WriteLineAsync(passEntitiesJson);
            }
            catch (Exception ex)
            {
                return new ErrorDescription(true, "No se puedo guardar las modificaciones de las passwords. Excepcion: " + ex.Message);
            }

            return new ErrorDescription(false);
        }

        public async Task<ErrorDescription> LoadJsonPassEntitiesFileAsync(string usuario)
        {
            string passEntitiesJson = "";
            
            try
            {
                if (!File.Exists(_jsonPassFilePath))
                {
                    var errorDesc = await SavePassEntitiesToFileAsync();
                    if (errorDesc.IsError)
                        throw new Exception("Ocurrio un error al generar el Json de las pass entities. " + errorDesc.Descripcion);
                }

                using (StreamReader sr = File.OpenText(_jsonPassFilePath))
                    passEntitiesJson = await sr.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                return new ErrorDescription(true, "No se puedo cargar el archivo Json de las pass entitites. Excepcion: " + ex.Message);
            }

            _passEntityList = JsonConvert.DeserializeObject<List<PassEntity>>(passEntitiesJson);

            var encryptTasks = _passEntityList.Where(p => p.Usuario == usuario)
                .Select(p => p.DescryptPassEntity(_encryperDecryper, _encryptPassword)).ToArray();

            Task.WaitAll(encryptTasks);

            return new ErrorDescription(false);
        }

        public List<PassEntity> GetPassEntitiesForUser(string user)
        {
            return _passEntityList.Where(p => p.Usuario == user).ToList();
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
