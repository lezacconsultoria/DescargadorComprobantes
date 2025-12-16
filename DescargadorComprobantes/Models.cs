using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DescargadorComprobantes
{
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class ComprobanteBusqueda
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("RazonSocial")]
        public string RazonSocial { get; set; }
    }

    public class ComprobanteDetalle
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("IdComprobanteAsociado")]
        public long? IdComprobanteAsociado { get; set; }

        [JsonProperty("IdUsuarioAdicional")]
        public int IdUsuarioAdicional { get; set; }

        [JsonProperty("IdCliente")]
        public long IdCliente { get; set; }

        [JsonProperty("RazonSocial")]
        public string RazonSocial { get; set; }

        [JsonProperty("FechaAlta")]
        public DateTime FechaAlta { get; set; }

        [JsonProperty("FechaEmision")]
        public DateTime FechaEmision { get; set; }

        [JsonProperty("FechaServDesde")]
        public DateTime? FechaServDesde { get; set; }

        [JsonProperty("FechaServHasta")]
        public DateTime? FechaServHasta { get; set; }

        [JsonProperty("Numero")]
        public string Numero { get; set; }

        [JsonProperty("TipoFc")]
        public string TipoFc { get; set; }

        [JsonProperty("Modo")]
        public string Modo { get; set; }

        [JsonProperty("Cae")]
        public string Cae { get; set; }

        [JsonProperty("ImporteTotalNeto")]
        public string ImporteTotalNeto { get; set; }

        [JsonProperty("ImporteTotalBruto")]
        public string ImporteTotalBruto { get; set; }

        [JsonProperty("Saldo")]
        public string Saldo { get; set; }

        [JsonProperty("PuntoVenta")]
        public int PuntoVenta { get; set; }

        [JsonProperty("Inventario")]
        public int Inventario { get; set; }

        [JsonProperty("CondicionVenta")]
        public string CondicionVenta { get; set; }

        [JsonProperty("FechaVencimiento")]
        public DateTime FechaVencimiento { get; set; }

        [JsonProperty("Observaciones")]
        public string Observaciones { get; set; }

        [JsonProperty("Canal")]
        public string Canal { get; set; }

        [JsonProperty("TipoConcepto")]
        public int TipoConcepto { get; set; }

        [JsonProperty("Descuento")]
        public string Descuento { get; set; }

        [JsonProperty("Recargo")]
        public string Recargo { get; set; }

        [JsonProperty("IDIntegracion")]
        public string IDIntegracion { get; set; }

        [JsonProperty("Origen")]
        public string Origen { get; set; }

        [JsonProperty("IDVentaIntegracion")]
        public string IDVentaIntegracion { get; set; }

        [JsonProperty("IDCondicionVenta")]
        public long? IDCondicionVenta { get; set; }

        [JsonProperty("IDTurno")]
        public long? IDTurno { get; set; }

        [JsonProperty("IDMoneda")]
        public long IDMoneda { get; set; }

        [JsonProperty("TipoDeCambio")]
        public decimal TipoDeCambio { get; set; }

        [JsonProperty("PercepcionIIBB")]
        public decimal PercepcionIIBB { get; set; }

        [JsonProperty("IDJurisdiccion")]
        public long? IDJurisdiccion { get; set; }

        [JsonProperty("RefExterna")]
        public string RefExterna { get; set; }

        [JsonProperty("fceMiPYME")]
        public bool FceMiPYME { get; set; }

        [JsonProperty("IDVendedor")]
        public long IDVendedor { get; set; }

        [JsonProperty("IDComprobanteDevolucion")]
        public long? IDComprobanteDevolucion { get; set; }

        [JsonProperty("Items")]
        public List<ItemComprobante> Items { get; set; }
    }

    public class ItemComprobante
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("IdConcepto")]
        public long? IdConcepto { get; set; }  // ← AGREGAR ? para nullable

        [JsonProperty("IDPlanDeCuenta")]
        public string IDPlanDeCuenta { get; set; }

        [JsonProperty("Cantidad")]
        public decimal Cantidad { get; set; }

        [JsonProperty("Concepto")]
        public string Concepto { get; set; }

        [JsonProperty("PrecioUnitario")]
        public decimal PrecioUnitario { get; set; }

        [JsonProperty("Iva")]
        public decimal Iva { get; set; }

        [JsonProperty("Bonificacion")]
        public decimal Bonificacion { get; set; }

        [JsonProperty("IDMoneda")]
        public long? IDMoneda { get; set; }

        [JsonProperty("Codigo")]
        public string Codigo { get; set; }

        [JsonProperty("Tipo")]
        public string Tipo { get; set; }

        [JsonProperty("IdRubro")]
        public long? IdRubro { get; set; }  // ← También hacer nullable

        [JsonProperty("IdSubRubro")]
        public long? IdSubRubro { get; set; }  // ← También hacer nullable
    }


    public class Cliente
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("RazonSocial")]
        public string RazonSocial { get; set; }

        [JsonProperty("NombreFantasia")]
        public string NombreFantasia { get; set; }

        [JsonProperty("CondicionIva")]
        public string CondicionIva { get; set; }

        [JsonProperty("TipoDoc")]
        public string TipoDoc { get; set; }

        [JsonProperty("NroDoc")]
        public string NroDoc { get; set; }

        [JsonProperty("Pais")]
        public string Pais { get; set; }

        [JsonProperty("Provincia")]
        public string Provincia { get; set; }

        [JsonProperty("Ciudad")]
        public string Ciudad { get; set; }

        [JsonProperty("Domicilio")]
        public string Domicilio { get; set; }

        [JsonProperty("Telefono")]
        public string Telefono { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Codigo")]
        public string Codigo { get; set; }

        [JsonProperty("PisoDepto")]
        public string PisoDepto { get; set; }

        [JsonProperty("Cp")]
        public string Cp { get; set; }

        [JsonProperty("Observaciones")]
        public string Observaciones { get; set; }

        [JsonProperty("Personeria")]
        public string Personeria { get; set; }

        [JsonProperty("IdPais")]
        public long? IdPais { get; set; }

        [JsonProperty("IdProvincia")]
        public long? IdProvincia { get; set; }

        [JsonProperty("IdCiudad")]
        public long? IdCiudad { get; set; }

        [JsonProperty("IdListaPrecio")]
        public long? IdListaPrecio { get; set; }

        [JsonProperty("IdUsuarioAdicional")]
        public object IdUsuarioAdicional { get; set; }

        [JsonProperty("Tags")]
        public object Tags { get; set; }
    }

    public class Producto
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Nombre")]
        public string Nombre { get; set; }

        [JsonProperty("Codigo")]
        public string Codigo { get; set; }

        [JsonProperty("CodigoOem")]
        public string CodigoOem { get; set; }

        [JsonProperty("CodigoBarras")]
        public string CodigoBarras { get; set; }

        [JsonProperty("CodigoProveedor")]
        public string CodigoProveedor { get; set; }

        [JsonProperty("Descripcion")]
        public string Descripcion { get; set; }

        [JsonProperty("Precio")]
        public decimal Precio { get; set; }

        [JsonProperty("PrecioFinal")]
        public decimal PrecioFinal { get; set; }

        [JsonProperty("SincronizaStock")]
        public bool SincronizaStock { get; set; }

        [JsonProperty("PrecioAutomatico")]
        public object PrecioAutomatico { get; set; }

        [JsonProperty("SincronizaPrecio")]
        public bool SincronizaPrecio { get; set; }

        [JsonProperty("Iva")]
        public decimal Iva { get; set; }

        [JsonProperty("Rentabilidad")]
        public decimal Rentabilidad { get; set; }

        [JsonProperty("CostoInterno")]
        public decimal CostoInterno { get; set; }

        [JsonProperty("Stock")]
        public decimal Stock { get; set; }

        [JsonProperty("StockMinimo")]
        public decimal StockMinimo { get; set; }

        [JsonProperty("StockInventario")]
        public decimal StockInventario { get; set; }

        [JsonProperty("IDProveedor")]
        public long? IDProveedor { get; set; }

        [JsonProperty("Observaciones")]
        public string Observaciones { get; set; }

        [JsonProperty("Estado")]
        public string Estado { get; set; }

        [JsonProperty("Tipo")]
        public string Tipo { get; set; }

        [JsonProperty("IdRubro")]
        public string IdRubro { get; set; }

        [JsonProperty("IdSubrubro")]
        public string IdSubrubro { get; set; }

        [JsonProperty("Foto")]
        public object Foto { get; set; }

        [JsonProperty("AplicaRG5329")]
        public bool AplicaRG5329 { get; set; }

        [JsonProperty("IDMoneda")]
        public long? IDMoneda { get; set; }

        [JsonProperty("ListasDePrecio")]
        public object ListasDePrecio { get; set; }

        [JsonProperty("Items")]
        public object Items { get; set; }
    }

    public class ApiResponse<T>
    {
        [JsonProperty("Items")]
        public List<T> Items { get; set; }
    }
}