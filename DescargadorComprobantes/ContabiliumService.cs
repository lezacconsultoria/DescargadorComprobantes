using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace DescargadorComprobantes
{
    public class ContabiliumService
    {
        private string _accessToken;


        public bool ObtenerToken()
        {
            try
            {
                Console.WriteLine("1. 🔐 Obteniendo acceso a la API...");

                var request = (HttpWebRequest)WebRequest.Create($"{Configuracion.ApiBaseUrl}/token");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                var parameters = $"grant_type=client_credentials&client_id={Configuracion.ClientId}&client_secret={Configuracion.ClientSecret}";
                var data = Encoding.UTF8.GetBytes(parameters);

                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var jsonResponse = reader.ReadToEnd();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);
                    _accessToken = tokenResponse.AccessToken;
                }

                Console.WriteLine("   ✅ Conexión establecida\n");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error en autenticación: {ex.Message}");
                return false;
            }
        }

        public List<ComprobanteBusqueda> ObtenerIdsComprobantes(string fechaDesde, string fechaHasta)
        {
            var comprobantesBusqueda = new List<ComprobanteBusqueda>();
            int paginaActual = 1;
            bool hayMasPaginas = true;

            Console.WriteLine("2. 📋 Buscando comprobantes en el rango...");

            while (hayMasPaginas)
            {
                var url = string.Format(
                    "{0}/api/comprobantes/search?fechaDesde={1}&fechaHasta={2}&page={3}",
                    Configuracion.ApiBaseUrl,
                    fechaDesde,
                    fechaHasta,
                    paginaActual);

                try
                {
                    Console.Write("   📄 Procesando página " + paginaActual + "... ");

                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Headers["Authorization"] = "Bearer " + _accessToken;

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var jsonResponse = reader.ReadToEnd();
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ComprobanteBusqueda>>(jsonResponse);

                        if (apiResponse.Items != null && apiResponse.Items.Count > 0)
                        {
                            foreach (var comprobante in apiResponse.Items)
                            {
                                comprobantesBusqueda.Add(comprobante);
                            }

                            Console.WriteLine("✅ " + apiResponse.Items.Count + " comprobantes encontrados");
                            paginaActual++;
                            Thread.Sleep(Configuracion.DelayBetweenRequests);
                        }
                        else
                        {
                            hayMasPaginas = false;
                            Console.WriteLine("\n   🏁 Búsqueda completada. Total: " + comprobantesBusqueda.Count + " comprobantes encontrados\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error: " + ex.Message);
                    hayMasPaginas = false;
                }
            }

            return comprobantesBusqueda;
        }

        public ComprobanteDetalle ObtenerDetalleComprobante(long idComprobante)
        {
            var url = $"{Configuracion.ApiBaseUrl}/api/comprobantes/?id={idComprobante}";

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Headers["Authorization"] = $"Bearer {_accessToken}";

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var jsonResponse = reader.ReadToEnd();
                    var comprobante = JsonConvert.DeserializeObject<ComprobanteDetalle>(jsonResponse);
                    return comprobante;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error al obtener detalle ID {idComprobante}: {ex.Message}");
                return null;
            }
        }

        public List<ComprobanteDetalle> ObtenerDetallesComprobantes(List<ComprobanteBusqueda> comprobantesBusqueda)
        {
            var comprobantesDetallados = new List<ComprobanteDetalle>();
            int contadorExitos = 0;
            int contadorErrores = 0;

            Console.WriteLine("3. ⬇️  Descargando detalles de cada comprobante...");

            for (int i = 0; i < comprobantesBusqueda.Count; i++)
            {
                var compBusqueda = comprobantesBusqueda[i];
                var porcentaje = Math.Round(((i + 1) / (double)comprobantesBusqueda.Count) * 100);

                Console.Write(string.Format(
                    "   📦 [{0}/{1}] {2}% - ID {3}... ",
                    i + 1,
                    comprobantesBusqueda.Count,
                    porcentaje,
                    compBusqueda.Id));

                var detalle = ObtenerDetalleComprobante(compBusqueda.Id);

                if (detalle != null)
                {
                    // COMBINAR: Usar RazonSocial del endpoint de búsqueda
                    detalle.RazonSocial = compBusqueda.RazonSocial;

                    comprobantesDetallados.Add(detalle);
                    Console.WriteLine("✅");
                    contadorExitos++;
                }
                else
                {
                    Console.WriteLine("❌");
                    contadorErrores++;
                }

                Thread.Sleep(Configuracion.DelayBetweenRequests);
            }

            Console.WriteLine("\n   📊 Resumen descarga: " + contadorExitos + " exitosos, " + contadorErrores + " errores");
            return comprobantesDetallados;
        }

        public List<Cliente> ObtenerClientes()
        {
            var todosLosClientes = new List<Cliente>();
            int paginaActual = 1;
            bool hayMasPaginas = true;

            Console.WriteLine("📋 Descargando clientes...");

            while (hayMasPaginas)
            {
                var url = string.Format(
                    "{0}/api/clientes/search?pageSize=50&page={1}",
                    Configuracion.ApiBaseUrl,
                    paginaActual);

                try
                {
                    Console.Write("   📄 Procesando página " + paginaActual + "... ");

                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Headers["Authorization"] = "Bearer " + _accessToken;

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var jsonResponse = reader.ReadToEnd();
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Cliente>>(jsonResponse);

                        if (apiResponse.Items != null && apiResponse.Items.Count > 0)
                        {
                            todosLosClientes.AddRange(apiResponse.Items);
                            Console.WriteLine("✅ " + apiResponse.Items.Count + " clientes encontrados");
                            paginaActual++;
                            Thread.Sleep(Configuracion.DelayBetweenRequests);
                        }
                        else
                        {
                            hayMasPaginas = false;
                            Console.WriteLine("\n   🏁 Descarga completada. Total: " + todosLosClientes.Count + " clientes\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error: " + ex.Message);
                    hayMasPaginas = false;
                }
            }

            return todosLosClientes;
        }

        public List<Producto> ObtenerProductos()
        {
            var todosLosProductos = new List<Producto>();
            int paginaActual = 1;
            bool hayMasPaginas = true;

            Console.WriteLine("📋 Descargando productos...");

            while (hayMasPaginas)
            {
                var url = string.Format(
                    "{0}/api/conceptos/search?pageSize=50&page={1}",
                    Configuracion.ApiBaseUrl,
                    paginaActual);

                try
                {
                    Console.Write("   📄 Procesando página " + paginaActual + "... ");

                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Headers["Authorization"] = "Bearer " + _accessToken;

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var jsonResponse = reader.ReadToEnd();
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Producto>>(jsonResponse);

                        if (apiResponse.Items != null && apiResponse.Items.Count > 0)
                        {
                            todosLosProductos.AddRange(apiResponse.Items);
                            Console.WriteLine("✅ " + apiResponse.Items.Count + " productos encontrados");
                            paginaActual++;
                            Thread.Sleep(Configuracion.DelayBetweenRequests);
                        }
                        else
                        {
                            hayMasPaginas = false;
                            Console.WriteLine("\n   🏁 Descarga completada. Total: " + todosLosProductos.Count + " productos\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error: " + ex.Message);
                    hayMasPaginas = false;
                }
            }

            return todosLosProductos;
        }
    }
}