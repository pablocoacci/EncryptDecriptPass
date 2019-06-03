using EncryptDecrypLib;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace EncryptDescript.Core.DTO
{
    public class PassEntity
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Descripcion { get; set; }
        public string Sitio { get; set; }
        public string Cuenta { get; set; }
        public string PassWord { get; set; }
        public string PreguntaSecreta { get; set; }
        public string RespuestaSecreta { get; set; }
        public string MailContacto { get; set; }
        public bool IsEncrypter { get; set; }

        #region Public Methods

        public async Task EncryptPassEntityAsync(IEncrypterDecrypter encrypterDecrypter, string passWordEncryptDecrypt)
        {
            await Task.Run(() =>
            {
                IsEncrypter = true;
                Cuenta = encrypterDecrypter.EncryptarCadena(Cuenta, passWordEncryptDecrypt);
                PassWord = encrypterDecrypter.EncryptarCadena(PassWord, passWordEncryptDecrypt);
                PreguntaSecreta = encrypterDecrypter.EncryptarCadena(PreguntaSecreta, passWordEncryptDecrypt);
                RespuestaSecreta = encrypterDecrypter.EncryptarCadena(RespuestaSecreta, passWordEncryptDecrypt);
                MailContacto = encrypterDecrypter.EncryptarCadena(MailContacto, passWordEncryptDecrypt);
            });
        }

        public async Task DescryptPassEntity(IEncrypterDecrypter encrypterDecrypter, string passWordEncryptDecrypt)
        {
            await Task.Run(() =>
            {
                IsEncrypter = false;
                Cuenta = encrypterDecrypter.DecryptarCadena(Cuenta, passWordEncryptDecrypt);
                PassWord = encrypterDecrypter.DecryptarCadena(PassWord, passWordEncryptDecrypt);
                PreguntaSecreta = encrypterDecrypter.DecryptarCadena(PreguntaSecreta, passWordEncryptDecrypt);
                RespuestaSecreta = encrypterDecrypter.DecryptarCadena(RespuestaSecreta, passWordEncryptDecrypt);
                MailContacto = encrypterDecrypter.DecryptarCadena(MailContacto, passWordEncryptDecrypt);
            });

        }

        public ErrorDescription IsValid(IEncrypterDecrypter encrypterDecrypter)
        {
            if (string.IsNullOrEmpty(Usuario))
                return new ErrorDescription(true, "El usuario no puede ser vacio");

            if (string.IsNullOrEmpty(Descripcion))
                return new ErrorDescription(true, "La descripcion es vacia o tiene caracteres invalidos");

            if (string.IsNullOrEmpty(Sitio))
                return new ErrorDescription(true, "El destino es un no puede ser vacio");

            if (string.IsNullOrEmpty(Cuenta) || IsStringValidEncrypt(Cuenta, encrypterDecrypter))
                return new ErrorDescription(true, "La cuenta es vacia o tiene caracteres invalidos");

            if (string.IsNullOrEmpty(PassWord) || IsStringValidEncrypt(PassWord, encrypterDecrypter))
                return new ErrorDescription(true, "La password esta vacia o tiene caracteres invalidos");

            if (IsStringValidEncrypt(PreguntaSecreta, encrypterDecrypter))
                return new ErrorDescription(true, "La pregunta secreta tiene caracteres no validos");

            if (IsStringValidEncrypt(RespuestaSecreta, encrypterDecrypter))
                return new ErrorDescription(true, "La respuesta secreta tiene caracteres invalidos");

            if (IsStringValidEncrypt(MailContacto, encrypterDecrypter))
                return new ErrorDescription(true, "El mail tiene caracteres invalidos");

            return new ErrorDescription(false);
        }

        #endregion

        #region Private Methods

        private bool IsStringValidEncrypt(string cadena, IEncrypterDecrypter encrypterDecrypter)
        {
            var validChars = encrypterDecrypter.GetValidCharacters();
            return cadena.Any(c => !validChars.Contains(c));
        }

        #endregion
    }
}
