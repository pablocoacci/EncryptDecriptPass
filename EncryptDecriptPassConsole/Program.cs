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
            ShowMsgColorConsole("Ingrese al usuario");
            var usuario = Console.ReadLine();

            ShowMsgColorConsole("Ingrese la password para Encriptar-Descencriptar el archivo");
            var passEncriptDecript = Console.ReadLine();

            var serviceProvider = ConfigureServices(passEncriptDecript, usuario);

            fileManager = serviceProvider.GetRequiredService<FileManager>();
            var errorDesc = fileManager.LoadJsonPassEntitiesFileAsync(usuario).Result;

            while (true)
            {
                var operation = ShowPrincipalMenu(usuario);

                switch (operation)
                {
                    case EnumOperaciones.CrearNuevaPassword:
                        CrearNuevaPass();
                        break;
                    case EnumOperaciones.EliminarPassword:
                        EliminarPass();
                        break;
                    case EnumOperaciones.GuardarCambiosFinalizar:
                        GuardarCambios(usuario);
                        break;
                    case EnumOperaciones.VerListaPasswords:
                        VerListaPass(usuario);
                        break;
                    case EnumOperaciones.VerCaracteresValidos:
                        VerCaracteresValidos();
                        break;
                    case EnumOperaciones.Terminar:
                        return;
                }
            }

        }

        private static ServiceProvider ConfigureServices(string passEncryptDescrypt, string usuario)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            var jsonPathPassEntities = configuration.GetValue<string>("appConsoleConfig:passEntitiesFilePath") + usuario + "_Pass.txt";

            var services = new ServiceCollection();

            services.AddSingleton<IEncrypterDecrypter, EncrypterVigenere>();
            services.AddSingleton<FileManager>(provider => new FileManager(provider.GetRequiredService<IEncrypterDecrypter>(), passEncryptDescrypt, jsonPathPassEntities));

            return services.BuildServiceProvider();
        }

        private static EnumOperaciones ShowPrincipalMenu(string usuario)
        {
            ShowMsgColorConsole("Que desea realizar:");
            ShowMsgColorConsole("Ingrese 1: Visualizar todas las passwords para el usuario " + usuario);
            ShowMsgColorConsole("Ingrese 2: Crear una nueva password para el usuario " + usuario);
            ShowMsgColorConsole("Ingrese 3: Eleminar una password del usuario " + usuario);
            ShowMsgColorConsole("Ingrese 4: Guardar los cambios realizados y finalizar");
            ShowMsgColorConsole("Ingrese 5: Ver caracteres validos");
            ShowMsgColorConsole("Ingrese 6: Terminar ejecucion");

            int.TryParse(Console.ReadLine(), out int operation);

            if (operation == 0 || operation > 6)
            {
                ShowMsgColorConsole("La operacion no es valida", ConsoleColor.Red);
                operation = (int)ShowPrincipalMenu(usuario);
            }

            return (EnumOperaciones)operation;
        }

        private static void CrearNuevaPass()
        {

        }

        private static void EliminarPass()
        {

        }

        private static void GuardarCambios(string usuario)
        {
            var result = fileManager.SavePassEntitiesToFileAsync().Result;

            if (result.IsError)
                ShowMsgColorConsole("Ocurrio un error al guardar los cambios. " + result.Descripcion, ConsoleColor.Red);
            else
                Console.WriteLine("Los cambios se guardaron correctamente");

            result = fileManager.LoadJsonPassEntitiesFileAsync(usuario).Result;
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

        }

        private static void ShowMsgColorConsole(string msg, ConsoleColor colorText = ConsoleColor.Green)
        {
            Console.ForegroundColor = colorText;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
