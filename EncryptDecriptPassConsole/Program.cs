using EncryptDecriptPassConsole.DTO;
using EncryptDecrypLib;
using EncryptDecrypLib.Encrypters;
using EncryptDescript.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EncryptDecriptPassConsole
{
    class Program
    {
        private static FileManager fileManager;

        static void Main(string[] args)
        {
            var configuration = GetConfiguration();
            var jsonPathPassEntities = configuration.GetValue<string>("appConsoleConfig:passEntitiesFilePath");

            ShowMsgColorConsole("Ingrese al usuario");
            var usuario = Console.ReadLine();

            ShowMsgColorConsole("Ingrese la password para Encriptar-Descencriptar el archivo");
            var passEncriptDecript = Console.ReadLine();

            var serviceProvider = ConfigureServices(passEncriptDecript, usuario);

            fileManager = serviceProvider.GetRequiredService<FileManager>();
            var errorDesc = fileManager.LoadJsonPassEntitiesFileAsync(jsonPathPassEntities, usuario, passEncriptDecript).Result;

            while (true)
            {
                var operation = ShowPrincipalMenu();

                switch (operation)
                {
                    case EnumOperaciones.CrearNuevaPassword:
                        CrearNuevaPass(usuario);
                        break;
                    case EnumOperaciones.EliminarPassword:
                        EliminarPass();
                        break;
                    case EnumOperaciones.GuardarCambiosFinalizar:
                        GuardarCambios(jsonPathPassEntities, usuario, passEncriptDecript);
                        break;
                    case EnumOperaciones.VerListaPasswords:
                        VerListaPass(usuario);
                        break;
                    case EnumOperaciones.VerCaracteresValidos:
                        VerCaracteresValidos();
                        break;
                    case EnumOperaciones.Terminar:
                        return;
                    case EnumOperaciones.GenerarArchivoDesencriptado:
                        GenerarArchivoDescencriptado(usuario);
                        break;
                }
            }

        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            return configuration;
        }

        private static ServiceProvider ConfigureServices(string passEncryptDescrypt, string usuario)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IEncrypterDecrypter, EncrypterVigenere>();
            services.AddSingleton<FileManager>(provider => new FileManager(provider.GetRequiredService<IEncrypterDecrypter>()));

            return services.BuildServiceProvider();
        }

        private static EnumOperaciones ShowPrincipalMenu()
        {
            Console.WriteLine("");
            ShowMsgColorConsole("Que desea realizar:");
            ShowMsgColorConsole("Ingrese 1: Visualizar todas las passwords");
            ShowMsgColorConsole("Ingrese 2: Crear una nueva password");
            ShowMsgColorConsole("Ingrese 3: Eleminar una password");
            ShowMsgColorConsole("Ingrese 4: Guardar los cambios realizados");
            ShowMsgColorConsole("Ingrese 5: Ver caracteres validos");
            ShowMsgColorConsole("Ingrese 6: Terminar ejecucion");
            ShowMsgColorConsole("Ingrese 7: Generar archivo de las passwords descencriptado");

            int.TryParse(Console.ReadLine(), out int operation);

            if (operation == 0 || operation > 7)
            {
                ShowMsgColorConsole("La operacion no es valida", ConsoleColor.Red);
                operation = (int)ShowPrincipalMenu();
            }

            return (EnumOperaciones)operation;
        }

        private static void CrearNuevaPass(string usuario)
        {
            ShowMsgColorConsole("Ingrese una descripcion del sitio");
            string desc = Console.ReadLine();
            ShowMsgColorConsole("Ingrese el nombre de la cuenta/usuario del sitio");
            string cuenta = Console.ReadLine();
            ShowMsgColorConsole("Ingrese la contraseña del sitio");
            string pass = Console.ReadLine();
            ShowMsgColorConsole("Ingrese la pregunta secreta confiurada en el sitio");
            string pregSecreta = Console.ReadLine();
            ShowMsgColorConsole("Ingrese la respuesta secreta del sitio");
            string rtaSecreta = Console.ReadLine();
            ShowMsgColorConsole("Ingrese mail de contacto");
            string mail = Console.ReadLine();

            fileManager.CrearNuevaPass(usuario, desc, cuenta, pass, pregSecreta, rtaSecreta, mail);

            ShowMsgColorConsole("La password se ha creado exitosamente");
        }

        private static void EliminarPass()
        {
            ShowMsgColorConsole("Ingrese el Id de la pass que desea eliminar");
            string idstr = Console.ReadLine();
            int.TryParse(idstr, out int id);

            if (id == 0)
                ShowMsgColorConsole("El id ingresado no es valido", ConsoleColor.Red);

            var descError = fileManager.EliminarPass(id);

            if(descError.IsError)
                ShowMsgColorConsole(descError.Descripcion, ConsoleColor.Red);
        }

        private static void GuardarCambios(string jsonPathFile, string usuario, string passEncryptDecrypt)
        {
            var result = fileManager.SavePassEntitiesToFileAsync(jsonPathFile, usuario, passEncryptDecrypt).Result;

            if (result.IsError)
                ShowMsgColorConsole("Ocurrio un error al guardar los cambios. " + result.Descripcion, ConsoleColor.Red);
            else
                Console.WriteLine("Los cambios se guardaron correctamente");

            result = fileManager.LoadJsonPassEntitiesFileAsync(jsonPathFile, usuario, passEncryptDecrypt).Result;
        }

        private static void VerListaPass(string usuario)
        {
            var listaPassEntities = fileManager.GetPassEntitiesForUser(usuario);

            ShowMsgColorConsole("Lista de passwords para el usuario " + usuario);

            foreach (var pass in listaPassEntities)
            {
                Console.WriteLine("Id: " + pass.Id);
                Console.WriteLine("Descripccion: " + pass.Descripcion);
                Console.WriteLine("Cuenta: " + pass.Cuenta);
                Console.WriteLine("Password: " + pass.PassWord);
                Console.WriteLine("Pregunta Secreta: " + pass.PreguntaSecreta);
                Console.WriteLine("Rta Secreta: " + pass.RespuestaSecreta);
                Console.WriteLine("Mail Contacto: " + pass.MailContacto);
                Console.WriteLine("---------------------------------------------------------------------------------------------------");
            }
        }

        private static void VerCaracteresValidos()
        {
            char[] characters = fileManager.GetValidCharacters();
            Console.WriteLine(string.Join(",", characters));
        }

        private static void GenerarArchivoDescencriptado(string usuario)
        {
            ShowMsgColorConsole(@"Ingrese el path donde desea generar el archivo. Ej: C:\carpeta1\carpeta2\");
            string path = Console.ReadLine();
            var errorDesc = fileManager.GenerarArchivoDescencriptado(path, usuario).Result;

            if (errorDesc.IsError)
                ShowMsgColorConsole(errorDesc.Descripcion, ConsoleColor.Red);
            else
                ShowMsgColorConsole("El archivo se genero exitosamente");
        }

        private static void ShowMsgColorConsole(string msg, ConsoleColor colorText = ConsoleColor.Green)
        {
            Console.ForegroundColor = colorText;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
