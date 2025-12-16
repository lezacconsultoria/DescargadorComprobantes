using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace DescargadorComprobantes
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager()
        {
            Console.WriteLine("=== DEBUG CONEXIÓN ===");
            Console.WriteLine("SqlServer: " + Configuracion.SqlServer);
            Console.WriteLine("SqlDatabase: " + Configuracion.SqlDatabase);
            Console.WriteLine("SqlUsername: " + Configuracion.SqlUsername);
            Console.WriteLine("SqlPassword: " + Configuracion.SqlPassword.Length + " caracteres");
            Console.WriteLine("ConnectionString: " + Configuracion.ConnectionString);
            Console.WriteLine("=== FIN DEBUG ===");

            _connectionString = Configuracion.ConnectionString;
        }

        public void CrearBaseDeDatosSiNoExiste()
        {
            Console.WriteLine("DEBUG - Cadena conexión: " + _connectionString);

            string masterConnectionString = _connectionString.Replace(Configuracion.SqlDatabase, "contab");

            using (var connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();

                // Verificar si la base de datos existe
                string checkDbQuery = $@"
                    SELECT name FROM sys.databases WHERE name = '{Configuracion.SqlDatabase}'";

                using (var command = new SqlCommand(checkDbQuery, connection))
                {
                    var result = command.ExecuteScalar();

                    if (result == null)
                    {
                        Console.WriteLine("   📦 Creando base de datos...");
                        string createDbQuery = $"CREATE DATABASE {Configuracion.SqlDatabase}";
                        using (var createCommand = new SqlCommand(createDbQuery, connection))
                        {
                            createCommand.ExecuteNonQuery();
                        }
                        Console.WriteLine("   ✅ Base de datos creada");
                    }
                    else
                    {
                        Console.WriteLine("   ✅ Base de datos encontrada");
                    }
                }
            }
        }

        public void CrearTablasSiNoExisten()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                Console.WriteLine("   🔍 Verificando tablas...");

                // Tabla Comprobantes
                string queryComprobantes = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Comprobantes' AND xtype='U')
                    CREATE TABLE Comprobantes (
                        Id BIGINT PRIMARY KEY,
                        IdComprobanteAsociado BIGINT NULL,
                        IdUsuarioAdicional INT,
                        IdCliente BIGINT,
                        RazonSocial NVARCHAR(MAX),
                        FechaAlta DATETIME,
                        FechaEmision DATETIME,
                        FechaServDesde DATETIME NULL,
                        FechaServHasta DATETIME NULL,
                        Numero NVARCHAR(100),
                        TipoFc NVARCHAR(10),
                        Modo NVARCHAR(1),
                        Cae NVARCHAR(100),
                        ImporteTotalNeto NVARCHAR(100),
                        ImporteTotalBruto NVARCHAR(100),
                        Saldo NVARCHAR(100),
                        PuntoVenta INT,
                        Inventario INT,
                        CondicionVenta NVARCHAR(100),
                        FechaVencimiento DATETIME,
                        Observaciones NVARCHAR(MAX),
                        Canal NVARCHAR(100),
                        TipoConcepto INT,
                        Descuento NVARCHAR(100) NULL,
                        Recargo NVARCHAR(100) NULL,
                        IDIntegracion NVARCHAR(100) NULL,
                        Origen NVARCHAR(100),
                        IDVentaIntegracion NVARCHAR(100) NULL,
                        IDCondicionVenta BIGINT NULL,
                        IDTurno BIGINT NULL,
                        IDMoneda BIGINT,
                        TipoDeCambio DECIMAL(18,2),
                        PercepcionIIBB DECIMAL(18,3),
                        IDJurisdiccion BIGINT NULL,
                        RefExterna NVARCHAR(100) NULL,
                        fceMiPYME BIT,
                        IDVendedor BIGINT,
                        IDComprobanteDevolucion BIGINT NULL,
                        FechaDescarga DATETIME DEFAULT GETDATE()
                    )";

                // Tabla ItemsComprobantes
                string queryItems = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ItemsComprobantes' AND xtype='U')
                    CREATE TABLE ItemsComprobantes (
                        IdItem BIGINT PRIMARY KEY,
                        IdComprobante BIGINT NOT NULL,
                        IdConcepto BIGINT,
                        IDPlanDeCuenta NVARCHAR(100),
                        Cantidad DECIMAL(18,4),
                        Concepto NVARCHAR(MAX),
                        PrecioUnitario DECIMAL(18,6),
                        Iva DECIMAL(18,3),
                        Bonificacion DECIMAL(18,3),
                        IDMoneda BIGINT NULL,
                        Codigo NVARCHAR(100),
                        Tipo NVARCHAR(1),
                        IdRubro BIGINT,
                        IdSubRubro BIGINT,
                        FechaDescarga DATETIME DEFAULT GETDATE(),
                        FOREIGN KEY (IdComprobante) REFERENCES Comprobantes(Id)
                    )";

                using (var command = new SqlCommand(queryComprobantes, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SqlCommand(queryItems, connection))
                {
                    command.ExecuteNonQuery();
                }

                Console.WriteLine("   ✅ Tablas verificadas/creadas");
            }
        }

        public void InsertarComprobantes(List<ComprobanteDetalle> comprobantes)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                Console.WriteLine("   💾 Insertando comprobantes...");

                // DEBUG: Ver primer comprobante
                if (comprobantes.Count > 0)
                {
                    var primerComp = comprobantes[0];
                    Console.WriteLine("DEBUG - Primer comprobante:");
                    Console.WriteLine("  ID: " + primerComp.Id);
                    Console.WriteLine("  RazonSocial: '" + primerComp.RazonSocial + "'");
                    Console.WriteLine("  Es null?: " + (primerComp.RazonSocial == null));
                }

                int comprobantesInsertados = 0;
                int itemsInsertados = 0;

                foreach (var comp in comprobantes)
                {
                    // Insertar comprobante
                    string queryComprobante = @"
                        IF NOT EXISTS (SELECT 1 FROM Comprobantes WHERE Id = @Id)
                        INSERT INTO Comprobantes (
                            Id, IdComprobanteAsociado, IdUsuarioAdicional, IdCliente, RazonSocial,
                            FechaAlta, FechaEmision, FechaServDesde, FechaServHasta, Numero, TipoFc,
                            Modo, Cae, ImporteTotalNeto, ImporteTotalBruto, Saldo, PuntoVenta, Inventario,
                            CondicionVenta, FechaVencimiento, Observaciones, Canal, TipoConcepto, Descuento,
                            Recargo, IDIntegracion, Origen, IDVentaIntegracion, IDCondicionVenta, IDTurno,
                            IDMoneda, TipoDeCambio, PercepcionIIBB, IDJurisdiccion, RefExterna, fceMiPYME, 
                            IDVendedor, IDComprobanteDevolucion
                        ) VALUES (
                            @Id, @IdComprobanteAsociado, @IdUsuarioAdicional, @IdCliente, @RazonSocial,
                            @FechaAlta, @FechaEmision, @FechaServDesde, @FechaServHasta, @Numero, @TipoFc,
                            @Modo, @Cae, @ImporteTotalNeto, @ImporteTotalBruto, @Saldo, @PuntoVenta, @Inventario,
                            @CondicionVenta, @FechaVencimiento, @Observaciones, @Canal, @TipoConcepto, @Descuento,
                            @Recargo, @IDIntegracion, @Origen, @IDVentaIntegracion, @IDCondicionVenta, @IDTurno,
                            @IDMoneda, @TipoDeCambio, @PercepcionIIBB, @IDJurisdiccion, @RefExterna, @fceMiPYME,
                            @IDVendedor, @IDComprobanteDevolucion
                        )";

                    using (var command = new SqlCommand(queryComprobante, connection))
                    {
                        // Agregar TODOS los parámetros
                        command.Parameters.AddWithValue("@Id", comp.Id);
                        command.Parameters.AddWithValue("@IdComprobanteAsociado", (object)comp.IdComprobanteAsociado ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IdUsuarioAdicional", comp.IdUsuarioAdicional);
                        command.Parameters.AddWithValue("@IdCliente", comp.IdCliente);
                        command.Parameters.AddWithValue("@RazonSocial",
                        string.IsNullOrEmpty(comp.RazonSocial) ? (object)DBNull.Value : comp.RazonSocial);
                        command.Parameters.AddWithValue("@FechaAlta", comp.FechaAlta);
                        command.Parameters.AddWithValue("@FechaEmision", comp.FechaEmision);
                        command.Parameters.AddWithValue("@FechaServDesde", (object)comp.FechaServDesde ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FechaServHasta", (object)comp.FechaServHasta ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Numero", comp.Numero);
                        command.Parameters.AddWithValue("@TipoFc", comp.TipoFc);
                        command.Parameters.AddWithValue("@Modo", comp.Modo);
                        command.Parameters.AddWithValue("@Cae", string.IsNullOrEmpty(comp.Cae) ? (object)DBNull.Value : comp.Cae);
                        command.Parameters.AddWithValue("@ImporteTotalNeto", comp.ImporteTotalNeto);
                        command.Parameters.AddWithValue("@ImporteTotalBruto", comp.ImporteTotalBruto);
                        command.Parameters.AddWithValue("@Saldo", comp.Saldo);
                        command.Parameters.AddWithValue("@PuntoVenta", comp.PuntoVenta);
                        command.Parameters.AddWithValue("@Inventario", comp.Inventario);
                        command.Parameters.AddWithValue("@CondicionVenta", comp.CondicionVenta);
                        command.Parameters.AddWithValue("@FechaVencimiento", comp.FechaVencimiento);
                        command.Parameters.AddWithValue("@Observaciones", (object)comp.Observaciones ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Canal", comp.Canal);
                        command.Parameters.AddWithValue("@TipoConcepto", comp.TipoConcepto);
                        command.Parameters.AddWithValue("@Descuento", (object)comp.Descuento ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Recargo", (object)comp.Recargo ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDIntegracion", (object)comp.IDIntegracion ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Origen", comp.Origen);
                        command.Parameters.AddWithValue("@IDVentaIntegracion", (object)comp.IDVentaIntegracion ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDCondicionVenta", (object)comp.IDCondicionVenta ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDTurno", (object)comp.IDTurno ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDMoneda", comp.IDMoneda);
                        command.Parameters.AddWithValue("@TipoDeCambio", comp.TipoDeCambio);
                        command.Parameters.AddWithValue("@PercepcionIIBB", comp.PercepcionIIBB);
                        command.Parameters.AddWithValue("@IDJurisdiccion", (object)comp.IDJurisdiccion ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RefExterna", (object)comp.RefExterna ?? DBNull.Value);
                        command.Parameters.AddWithValue("@fceMiPYME", comp.FceMiPYME);
                        command.Parameters.AddWithValue("@IDVendedor", comp.IDVendedor);
                        command.Parameters.AddWithValue("@IDComprobanteDevolucion", (object)comp.IDComprobanteDevolucion ?? DBNull.Value);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            comprobantesInsertados++;
                    }

                    // Insertar items si existen
                    if (comp.Items != null && comp.Items.Count > 0)
                    {
                        foreach (var item in comp.Items)
                        {
                            string queryItem = @"
                                IF NOT EXISTS (SELECT 1 FROM ItemsComprobantes WHERE IdItem = @IdItem)
                                INSERT INTO ItemsComprobantes (
                                    IdItem, IdComprobante, IdConcepto, IDPlanDeCuenta, Cantidad, Concepto,
                                    PrecioUnitario, Iva, Bonificacion, IDMoneda, Codigo, Tipo, IdRubro, IdSubRubro
                                ) VALUES (
                                    @IdItem, @IdComprobante, @IdConcepto, @IDPlanDeCuenta, @Cantidad, @Concepto,
                                    @PrecioUnitario, @Iva, @Bonificacion, @IDMoneda, @Codigo, @Tipo, @IdRubro, @IdSubRubro
                                )";

                            using (var command = new SqlCommand(queryItem, connection))
                            {
                                command.Parameters.AddWithValue("@IdItem", item.Id);
                                command.Parameters.AddWithValue("@IdComprobante", comp.Id);
                                command.Parameters.AddWithValue("@IdConcepto", (object)item.IdConcepto ?? DBNull.Value);
                                command.Parameters.AddWithValue("@IDPlanDeCuenta", (object)item.IDPlanDeCuenta ?? DBNull.Value);
                                command.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                command.Parameters.AddWithValue("@Concepto", (object)item.Concepto ?? DBNull.Value);
                                command.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
                                command.Parameters.AddWithValue("@Iva", item.Iva);
                                command.Parameters.AddWithValue("@Bonificacion", item.Bonificacion);
                                command.Parameters.AddWithValue("@IDMoneda", (object)item.IDMoneda ?? DBNull.Value);
                                command.Parameters.AddWithValue("@Codigo", (object)item.Codigo ?? DBNull.Value);
                                command.Parameters.AddWithValue("@Tipo", (object)item.Tipo ?? DBNull.Value);
                                command.Parameters.AddWithValue("@IdRubro", (object)item.IdRubro ?? DBNull.Value);
                                command.Parameters.AddWithValue("@IdSubRubro", (object)item.IdSubRubro ?? DBNull.Value);

                                int itemRowsAffected = command.ExecuteNonQuery();
                                if (itemRowsAffected > 0)
                                    itemsInsertados++;
                            }
                        }
                    }
                }

                Console.WriteLine($"   ✅ {comprobantesInsertados} comprobantes insertados");
                Console.WriteLine($"   ✅ {itemsInsertados} items insertados");
            }
        }


        public void CrearTablaClientesSiNoExiste()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string queryClientes = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clientes' AND xtype='U')
            CREATE TABLE Clientes (
                Id BIGINT PRIMARY KEY,
                RazonSocial NVARCHAR(255),
                NombreFantasia NVARCHAR(255),
                CondicionIva NVARCHAR(10),
                TipoDoc NVARCHAR(20),
                NroDoc NVARCHAR(20),
                Pais NVARCHAR(100),
                Provincia NVARCHAR(100),
                Ciudad NVARCHAR(100),
                Domicilio NVARCHAR(255),
                Telefono NVARCHAR(50),
                Email NVARCHAR(255),
                Codigo NVARCHAR(100),
                PisoDepto NVARCHAR(100),
                Cp NVARCHAR(20),
                Observaciones NVARCHAR(MAX),
                Personeria NVARCHAR(1),
                IdPais BIGINT,
                IdProvincia BIGINT,
                IdCiudad BIGINT,
                IdListaPrecio BIGINT,
                IdUsuarioAdicional BIGINT NULL,
                Tags NVARCHAR(MAX) NULL,
                FechaDescarga DATETIME DEFAULT GETDATE()
            )";

                using (var command = new SqlCommand(queryClientes, connection))
                {
                    command.ExecuteNonQuery();
                }

                Console.WriteLine("   ✅ Tabla Clientes verificada/creada");
            }
        }

        public void CrearTablaProductosSiNoExiste()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string queryProductos = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Productos' AND xtype='U')
            CREATE TABLE Productos (
                Id BIGINT PRIMARY KEY,
                Nombre NVARCHAR(255),
                Codigo NVARCHAR(100),
                CodigoOem NVARCHAR(100),
                CodigoBarras NVARCHAR(100),
                CodigoProveedor NVARCHAR(100),
                Descripcion NVARCHAR(MAX),
                Precio DECIMAL(18,2),
                PrecioFinal DECIMAL(18,2),
                SincronizaStock BIT,
                PrecioAutomatico NVARCHAR(100) NULL,
                SincronizaPrecio BIT,
                Iva DECIMAL(18,3),
                Rentabilidad DECIMAL(18,3),
                CostoInterno DECIMAL(18,3),
                Stock DECIMAL(18,4),
                StockMinimo DECIMAL(18,4),
                StockInventario DECIMAL(18,4),
                IDProveedor BIGINT,
                Observaciones NVARCHAR(MAX),
                Estado NVARCHAR(50),
                Tipo NVARCHAR(50),
                IdRubro NVARCHAR(100),
                IdSubrubro NVARCHAR(100),
                Foto NVARCHAR(MAX) NULL,
                AplicaRG5329 BIT,
                IDMoneda BIGINT,
                ListasDePrecio NVARCHAR(MAX) NULL,
                Items NVARCHAR(MAX) NULL,
                FechaDescarga DATETIME DEFAULT GETDATE()
            )";

                using (var command = new SqlCommand(queryProductos, connection))
                {
                    command.ExecuteNonQuery();
                }

                Console.WriteLine("   ✅ Tabla Productos verificada/creada");
            }
        }

        public void InsertarClientes(List<Cliente> clientes)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                Console.WriteLine("   💾 Insertando clientes...");

                // LIMPIAR TABLA EXISTENTE
                using (var truncateCommand = new SqlCommand("DELETE FROM Clientes", connection))
                {
                    truncateCommand.ExecuteNonQuery();
                }

                int clientesInsertados = 0;

                foreach (var cliente in clientes)
                {
                    string query = @"
                INSERT INTO Clientes (
                    Id, RazonSocial, NombreFantasia, CondicionIva, TipoDoc, NroDoc,
                    Pais, Provincia, Ciudad, Domicilio, Telefono, Email, Codigo,
                    PisoDepto, Cp, Observaciones, Personeria, IdPais, IdProvincia,
                    IdCiudad, IdListaPrecio, IdUsuarioAdicional, Tags
                ) VALUES (
                    @Id, @RazonSocial, @NombreFantasia, @CondicionIva, @TipoDoc, @NroDoc,
                    @Pais, @Provincia, @Ciudad, @Domicilio, @Telefono, @Email, @Codigo,
                    @PisoDepto, @Cp, @Observaciones, @Personeria, @IdPais, @IdProvincia,
                    @IdCiudad, @IdListaPrecio, @IdUsuarioAdicional, @Tags
                )";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", cliente.Id);
                        command.Parameters.AddWithValue("@RazonSocial", string.IsNullOrEmpty(cliente.RazonSocial) ? (object)DBNull.Value : cliente.RazonSocial);
                        command.Parameters.AddWithValue("@NombreFantasia", string.IsNullOrEmpty(cliente.NombreFantasia) ? (object)DBNull.Value : cliente.NombreFantasia);
                        command.Parameters.AddWithValue("@CondicionIva", string.IsNullOrEmpty(cliente.CondicionIva) ? (object)DBNull.Value : cliente.CondicionIva);
                        command.Parameters.AddWithValue("@TipoDoc", string.IsNullOrEmpty(cliente.TipoDoc) ? (object)DBNull.Value : cliente.TipoDoc);
                        command.Parameters.AddWithValue("@NroDoc", string.IsNullOrEmpty(cliente.NroDoc) ? (object)DBNull.Value : cliente.NroDoc);
                        command.Parameters.AddWithValue("@Pais", string.IsNullOrEmpty(cliente.Pais) ? (object)DBNull.Value : cliente.Pais);
                        command.Parameters.AddWithValue("@Provincia", string.IsNullOrEmpty(cliente.Provincia) ? (object)DBNull.Value : cliente.Provincia);
                        command.Parameters.AddWithValue("@Ciudad", string.IsNullOrEmpty(cliente.Ciudad) ? (object)DBNull.Value : cliente.Ciudad);
                        command.Parameters.AddWithValue("@Domicilio", string.IsNullOrEmpty(cliente.Domicilio) ? (object)DBNull.Value : cliente.Domicilio);
                        command.Parameters.AddWithValue("@Telefono", string.IsNullOrEmpty(cliente.Telefono) ? (object)DBNull.Value : cliente.Telefono);
                        command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(cliente.Email) ? (object)DBNull.Value : cliente.Email);
                        command.Parameters.AddWithValue("@Codigo", string.IsNullOrEmpty(cliente.Codigo) ? (object)DBNull.Value : cliente.Codigo);
                        command.Parameters.AddWithValue("@PisoDepto", string.IsNullOrEmpty(cliente.PisoDepto) ? (object)DBNull.Value : cliente.PisoDepto);
                        command.Parameters.AddWithValue("@Cp", string.IsNullOrEmpty(cliente.Cp) ? (object)DBNull.Value : cliente.Cp);
                        command.Parameters.AddWithValue("@Observaciones", string.IsNullOrEmpty(cliente.Observaciones) ? (object)DBNull.Value : cliente.Observaciones);
                        command.Parameters.AddWithValue("@Personeria", string.IsNullOrEmpty(cliente.Personeria) ? (object)DBNull.Value : cliente.Personeria);
                        command.Parameters.AddWithValue("@IdPais", cliente.IdPais);
                        command.Parameters.AddWithValue("@IdProvincia", cliente.IdProvincia);
                        command.Parameters.AddWithValue("@IdCiudad", cliente.IdCiudad);
                        command.Parameters.AddWithValue("@IdListaPrecio", cliente.IdListaPrecio ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IdUsuarioAdicional", cliente.IdUsuarioAdicional ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Tags", cliente.Tags ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                        clientesInsertados++;
                    }
                }

                Console.WriteLine("   ✅ " + clientesInsertados + " clientes insertados");
            }
        }

        public void InsertarProductos(List<Producto> productos)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                Console.WriteLine("   💾 Insertando productos...");

                // LIMPIAR TABLA EXISTENTE - CON DEBUG
                Console.WriteLine("   🗑️  Limpiando tabla existente...");
                using (var truncateCommand = new SqlCommand("DELETE FROM Productos", connection))
                {
                    int filasEliminadas = truncateCommand.ExecuteNonQuery();
                    Console.WriteLine("   ✅ " + filasEliminadas + " registros eliminados");
                }

                int productosInsertados = 0;
                int productosDuplicados = 0;
                int productosConError = 0;

                foreach (var producto in productos)
                {
                    // VERIFICAR SI EL ID YA EXISTE (por si acaso)
                    string checkQuery = "SELECT COUNT(1) FROM Productos WHERE Id = @Id";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Id", producto.Id);
                        var existe = (int)checkCommand.ExecuteScalar();

                        if (existe > 0)
                        {
                            Console.WriteLine("   ⚠️  ID duplicado omitido: " + producto.Id);
                            productosDuplicados++;
                            continue; // Saltar este producto
                        }
                    }

                    string query = @"
                INSERT INTO Productos (
                    Id, Nombre, Codigo, CodigoOem, CodigoBarras, CodigoProveedor,
                    Descripcion, Precio, PrecioFinal, SincronizaStock, PrecioAutomatico,
                    SincronizaPrecio, Iva, Rentabilidad, CostoInterno, Stock, StockMinimo,
                    StockInventario, IDProveedor, Observaciones, Estado, Tipo, IdRubro,
                    IdSubrubro, Foto, AplicaRG5329, IDMoneda, ListasDePrecio, Items
                ) VALUES (
                    @Id, @Nombre, @Codigo, @CodigoOem, @CodigoBarras, @CodigoProveedor,
                    @Descripcion, @Precio, @PrecioFinal, @SincronizaStock, @PrecioAutomatico,
                    @SincronizaPrecio, @Iva, @Rentabilidad, @CostoInterno, @Stock, @StockMinimo,
                    @StockInventario, @IDProveedor, @Observaciones, @Estado, @Tipo, @IdRubro,
                    @IdSubrubro, @Foto, @AplicaRG5329, @IDMoneda, @ListasDePrecio, @Items
                )";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", producto.Id);
                        command.Parameters.AddWithValue("@Nombre", string.IsNullOrEmpty(producto.Nombre) ? (object)DBNull.Value : producto.Nombre);
                        command.Parameters.AddWithValue("@Codigo", string.IsNullOrEmpty(producto.Codigo) ? (object)DBNull.Value : producto.Codigo);
                        command.Parameters.AddWithValue("@CodigoOem", string.IsNullOrEmpty(producto.CodigoOem) ? (object)DBNull.Value : producto.CodigoOem);
                        command.Parameters.AddWithValue("@CodigoBarras", string.IsNullOrEmpty(producto.CodigoBarras) ? (object)DBNull.Value : producto.CodigoBarras);
                        command.Parameters.AddWithValue("@CodigoProveedor", string.IsNullOrEmpty(producto.CodigoProveedor) ? (object)DBNull.Value : producto.CodigoProveedor);
                        command.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(producto.Descripcion) ? (object)DBNull.Value : producto.Descripcion);
                        command.Parameters.AddWithValue("@Precio", producto.Precio);
                        command.Parameters.AddWithValue("@PrecioFinal", producto.PrecioFinal);
                        command.Parameters.AddWithValue("@SincronizaStock", producto.SincronizaStock);
                        command.Parameters.AddWithValue("@PrecioAutomatico", producto.PrecioAutomatico ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SincronizaPrecio", producto.SincronizaPrecio);
                        command.Parameters.AddWithValue("@Iva", producto.Iva);
                        command.Parameters.AddWithValue("@Rentabilidad", producto.Rentabilidad);
                        command.Parameters.AddWithValue("@CostoInterno", producto.CostoInterno);
                        command.Parameters.AddWithValue("@Stock", producto.Stock);
                        command.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                        command.Parameters.AddWithValue("@StockInventario", producto.StockInventario);
                        command.Parameters.AddWithValue("@IDProveedor", producto.IDProveedor ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Observaciones", string.IsNullOrEmpty(producto.Observaciones) ? (object)DBNull.Value : producto.Observaciones);
                        command.Parameters.AddWithValue("@Estado", string.IsNullOrEmpty(producto.Estado) ? (object)DBNull.Value : producto.Estado);
                        command.Parameters.AddWithValue("@Tipo", string.IsNullOrEmpty(producto.Tipo) ? (object)DBNull.Value : producto.Tipo);
                        command.Parameters.AddWithValue("@IdRubro", string.IsNullOrEmpty(producto.IdRubro) ? (object)DBNull.Value : producto.IdRubro);
                        command.Parameters.AddWithValue("@IdSubrubro", string.IsNullOrEmpty(producto.IdSubrubro) ? (object)DBNull.Value : producto.IdSubrubro);
                        command.Parameters.AddWithValue("@Foto", producto.Foto ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@AplicaRG5329", producto.AplicaRG5329);
                        command.Parameters.AddWithValue("@IDMoneda", producto.IDMoneda ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ListasDePrecio", producto.ListasDePrecio ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Items", producto.Items ?? (object)DBNull.Value);

                        try
                        {
                            command.ExecuteNonQuery();
                            productosInsertados++;

                            // Mostrar progreso cada 100 registros
                            if (productosInsertados % 100 == 0)
                            {
                                Console.WriteLine("   📦 " + productosInsertados + " productos insertados...");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("   ❌ Error insertando producto ID " + producto.Id + ": " + ex.Message);
                            productosConError++;
                        }
                    }
                }

                Console.WriteLine("   ✅ " + productosInsertados + " productos insertados exitosamente");
                if (productosDuplicados > 0)
                    Console.WriteLine("   ⚠️  " + productosDuplicados + " productos duplicados omitidos");
                if (productosConError > 0)
                    Console.WriteLine("   ❌ " + productosConError + " productos con errores");
                Console.WriteLine("   📊 Total procesados: " + productos.Count + " productos");
            }
        }
    }
}