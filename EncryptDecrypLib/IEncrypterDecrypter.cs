using System;
using System.Collections.Generic;
using System.Text;

namespace EncryptDecrypLib
{
    public interface IEncrypterDecrypter
    {
        string EncryptarCadena(string cadena, string password);

        string DecryptarCadena(string cadena, string password);

        char[] GetValidCharacters();
    }
}
