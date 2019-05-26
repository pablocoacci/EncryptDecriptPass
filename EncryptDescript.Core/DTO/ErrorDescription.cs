using System;
using System.Collections.Generic;
using System.Text;

namespace EncryptDescript.Core.DTO
{
    public class ErrorDescription
    {
        public ErrorDescription(bool isError, string descr = "")
        {
            IsError = isError;
            Descripcion = descr;
        }

        public bool IsError { get; private set; }
        public string Descripcion { get; private set; }
    }
}
