using System;
using System.Collections.Generic;

namespace DescargadorComprobantes
{
    class Program
    {
        static void Main(string[] args)
        {
            // CONFIGURACIÓN SSL CRÍTICA PARA .NET 4.0
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls |
                (System.Net.SecurityProtocolType)768 |   // TLS 1.1
                (System.Net.SecurityProtocolType)3072;   // TLS 1.2

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            Console.WriteLine("=========================================");
            Console.WriteLine("   📋 DESCARGADOR DE COMPROBANTES");
            Console.WriteLine("   👥 CLIENTES Y PRODUCTOS");
            Console.WriteLine("=========================================\n");

            try
            {
                // Crear instancias de los servicios
                var contabiliumService = new ContabiliumService();
                var databaseManager = new DatabaseManager();

                // 1. Obtener token de la API
                Console.WriteLine("1. 🔐 Obteniendo acceso a la API...");
                bool tokenObtenido = contabiliumService.ObtenerToken();
                if (!tokenObtenido)
                {
                    Console.WriteLine("❌ No se pudo obtener acceso a la API. Verifica las credenciales.");
                    EsperarSalida();
                    return;
                }

                // MENÚ DE OPCIONES
                Console.WriteLine("\n🎯 ¿QUÉ DATOS DESEAS DESCARGAR?");
                Console.WriteLine("=========================================");

                bool descargarClientes = PreguntarSiNo("   📊 ¿Descargar CLIENTES?");
                bool descargarProductos = PreguntarSiNo("   🏷️  ¿Descargar PRODUCTOS?");
                bool descargarSaldos = PreguntarSiNo("   💰 ¿Descargar SALDOS?");
                bool descargarComprobantes = PreguntarSiNo("   📄 ¿Descargar COMPROBANTES?");
                bool descargarStock = PreguntarSiNo("   📦 ¿Descargar STOCK?");

                Console.WriteLine();

                // 2. PROCESAR CLIENTES
                if (descargarClientes)
                {
                    Console.WriteLine("2. 👥 PROCESANDO CLIENTES...");
                    databaseManager.CrearTablaClientesSiNoExiste();
                    var clientes = contabiliumService.ObtenerClientes();
                    if (clientes.Count > 0)
                    {
                        databaseManager.InsertarClientes(clientes);
                    }
                    else
                    {
                        Console.WriteLine("   ⚠️  No se encontraron clientes");
                    }
                    Console.WriteLine();
                }

                // 3. PROCESAR PRODUCTOS
                if (descargarProductos)
                {
                    Console.WriteLine("3. 🏷️  PROCESANDO PRODUCTOS...");
                    databaseManager.CrearTablaProductosSiNoExiste();
                    var productos = contabiliumService.ObtenerProductos();
                    if (productos.Count > 0)
                    {
                        databaseManager.InsertarProductos(productos);
                    }
                    else
                    {
                        Console.WriteLine("   ⚠️  No se encontraron productos");
                    }
                    Console.WriteLine();
                }

                // 4. PROCESAR SALDOS
                if (descargarSaldos)
                {
                    Console.WriteLine("4. 💰 PROCESANDO SALDOS...");

                    // Pedir fechas para saldos
                    string fechaDesdeSaldos, fechaHastaSaldos;

                    Console.WriteLine("📅 Por favor, ingresa las fechas para SALDOS en formato YYYY-MM-DD");
                    Console.Write("✅ Fecha DESDE (ej: 2025-01-01): ");
                    fechaDesdeSaldos = Console.ReadLine();

                    Console.Write("✅ Fecha HASTA (ej: 2025-12-31): ");
                    fechaHastaSaldos = Console.ReadLine();
                    Console.WriteLine();

                    // Validar fechas
                    if (string.IsNullOrEmpty(fechaDesdeSaldos) || string.IsNullOrEmpty(fechaHastaSaldos))
                    {
                        Console.WriteLine("❌ Error: Las fechas no pueden estar vacías");
                    }
                    else
                    {
                        // Crear tabla si no existe
                        databaseManager.CrearTablaSaldosSiNoExiste();

                        // Obtener comprobantes
                        var comprobantesBusquedaSaldos = contabiliumService.ObtenerIdsComprobantes(fechaDesdeSaldos, fechaHastaSaldos);

                        if (comprobantesBusquedaSaldos.Count > 0)
                        {
                            var comprobantesDetalladosSaldos = contabiliumService.ObtenerDetallesComprobantes(comprobantesBusquedaSaldos);

                            if (comprobantesDetalladosSaldos.Count > 0)
                            {
                                databaseManager.InsertarSaldos(comprobantesDetalladosSaldos, fechaDesdeSaldos, fechaHastaSaldos);

                                // Mostrar resumen saldos
                                Console.WriteLine("\n📊 RESUMEN SALDOS:");
                                Console.WriteLine("   • Fechas: " + fechaDesdeSaldos + " a " + fechaHastaSaldos);
                                Console.WriteLine("   • Saldos procesados: " + comprobantesDetalladosSaldos.Count);
                            }
                            else
                            {
                                Console.WriteLine("❌ No se pudieron descargar los detalles para saldos");
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ No se encontraron comprobantes en el rango de fechas especificado");
                        }
                    }
                    Console.WriteLine();
                }

                // 5. PROCESAR COMPROBANTES
                if (descargarComprobantes)
                {
                    Console.WriteLine("5. 📄 PROCESANDO COMPROBANTES...");

                    // Pedir fechas para comprobantes
                    string fechaDesde, fechaHasta;

                    if (args.Length == 2)
                    {
                        fechaDesde = args[0];
                        fechaHasta = args[1];
                        Console.WriteLine("📅 Fechas desde línea de comandos:");
                        Console.WriteLine("   DESDE: " + fechaDesde);
                        Console.WriteLine("   HASTA: " + fechaHasta + "\n");
                    }
                    else
                    {
                        Console.WriteLine("📅 Por favor, ingresa las fechas en formato YYYY-MM-DD");
                        Console.Write("✅ Fecha DESDE (ej: 2025-01-01): ");
                        fechaDesde = Console.ReadLine();

                        Console.Write("✅ Fecha HASTA (ej: 2025-12-31): ");
                        fechaHasta = Console.ReadLine();
                        Console.WriteLine();
                    }

                    // Validar fechas
                    if (string.IsNullOrEmpty(fechaDesde) || string.IsNullOrEmpty(fechaHasta))
                    {
                        Console.WriteLine("❌ Error: Las fechas no pueden estar vacías");
                    }
                    else
                    {
                        // Crear base de datos y tablas si no existen
                        databaseManager.CrearBaseDeDatosSiNoExiste();
                        databaseManager.CrearTablasSiNoExisten();

                        // Obtener comprobantes
                        var comprobantesBusqueda = contabiliumService.ObtenerIdsComprobantes(fechaDesde, fechaHasta);

                        if (comprobantesBusqueda.Count > 0)
                        {
                            var comprobantesDetallados = contabiliumService.ObtenerDetallesComprobantes(comprobantesBusqueda);

                            if (comprobantesDetallados.Count > 0)
                            {
                                databaseManager.InsertarComprobantes(comprobantesDetallados);

                                // Mostrar resumen comprobantes
                                Console.WriteLine("\n📊 RESUMEN COMPROBANTES:");
                                Console.WriteLine("   • Fechas: " + fechaDesde + " a " + fechaHasta);
                                Console.WriteLine("   • Comprobantes encontrados: " + comprobantesBusqueda.Count);
                                Console.WriteLine("   • Comprobantes procesados: " + comprobantesDetallados.Count);
                            }
                            else
                            {
                                Console.WriteLine("❌ No se pudieron descargar los detalles de los comprobantes");
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ No se encontraron comprobantes en el rango de fechas especificado");
                        }
                    }
                    Console.WriteLine();
                }

                // 6. PROCESAR STOCK
                if (descargarStock)
                {
                    Console.WriteLine("6. 📦 PROCESANDO STOCK...");

                    // Crear tablas
                    databaseManager.CrearTablaDepositosSiNoExiste();
                    databaseManager.CrearTablaStockSiNoExiste();

                    // Obtener depósitos
                    var depositos = contabiliumService.ObtenerDepositos();

                    if (depositos.Count > 0)
                    {
                        // Insertar depósitos
                        databaseManager.InsertarDepositos(depositos);

                        // Por cada depósito, obtener e insertar stock
                        int totalItemsStock = 0;
                        foreach (var deposito in depositos)
                        {
                            var stockItems = contabiliumService.ObtenerStockPorDeposito(deposito.Id, deposito.Nombre);
                            if (stockItems.Count > 0)
                            {
                                databaseManager.InsertarStock(stockItems, deposito.Id);
                                totalItemsStock += stockItems.Count;
                            }
                        }

                        // Mostrar resumen stock
                        Console.WriteLine("\n📊 RESUMEN STOCK:");
                        Console.WriteLine("   • Depósitos procesados: " + depositos.Count);
                        Console.WriteLine("   • Items de stock totales: " + totalItemsStock);
                    }
                    else
                    {
                        Console.WriteLine("❌ No se encontraron depósitos");
                    }
                    Console.WriteLine();
                }

                // RESUMEN FINAL
                Console.WriteLine("=========================================");
                Console.WriteLine("🎉 ¡PROCESO COMPLETADO!");
                Console.WriteLine("=========================================");
                Console.WriteLine("📋 RESUMEN FINAL:");

                if (descargarClientes)
                    Console.WriteLine("   ✅ Clientes descargados");
                else
                    Console.WriteLine("   ⏭️  Clientes omitidos");

                if (descargarProductos)
                    Console.WriteLine("   ✅ Productos descargados");
                else
                    Console.WriteLine("   ⏭️  Productos omitidos");

                if (descargarSaldos)
                    Console.WriteLine("   ✅ Saldos descargados");
                else
                    Console.WriteLine("   ⏭️  Saldos omitidos");

                if (descargarComprobantes)
                    Console.WriteLine("   ✅ Comprobantes descargados");
                else
                    Console.WriteLine("   ⏭️  Comprobantes omitidos");

                if (descargarStock)
                    Console.WriteLine("   ✅ Stock descargado");
                else
                    Console.WriteLine("   ⏭️  Stock omitido");

                Console.WriteLine("   💾 Base de datos: " + Configuracion.SqlDatabase);
                Console.WriteLine("=========================================\n");

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n❌ ERROR CRÍTICO: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("   Detalles: " + ex.InnerException.Message);
                }
            }

            EsperarSalida();
        }

        static bool PreguntarSiNo(string pregunta)
        {
            Console.Write(pregunta + " (s/n): ");
            var respuesta = Console.ReadLine()?.ToLower().Trim();
            return respuesta == "s" || respuesta == "si" || respuesta == "sí" || respuesta == "y" || respuesta == "yes";
        }

        static void EsperarSalida()
        {
            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}