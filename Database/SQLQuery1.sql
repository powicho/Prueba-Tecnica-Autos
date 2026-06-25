-- =========================================================================
-- 1. CREACIÓN DE LA BASE DE DATOS
-- =========================================================================
-- Creamos un espacio lógico de almacenamiento aislado para nuestro proyecto.
CREATE DATABASE ControlVehiculos;
GO

-- Cambiamos el contexto de ejecución de la sesión a nuestra nueva base de datos.
USE ControlVehiculos;
GO


-- =========================================================================
-- 2. CREACIÓN DE TABLAS (ESTRUCTURAS FÍSICAS)
-- =========================================================================

-- Tabla: EstadosVehiculo (Catálogo de condiciones físicas)
CREATE TABLE EstadosVehiculo (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE
);
GO

-- Tabla: Trabajadores (Para el control de accesos y perfiles)
CREATE TABLE Trabajadores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Correo VARCHAR(150) NOT NULL UNIQUE,
    Contrasena VARCHAR(255) NOT NULL, -- Almacenará el hash seguro de la contraseña
    Pais VARCHAR(100) NOT NULL
);
GO

-- Tabla: Vehiculos (Inventario principal)
CREATE TABLE Vehiculos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Marca VARCHAR(100) NOT NULL,
    Color VARCHAR(50) NOT NULL,
    ModeloAño INT NOT NULL,
    PrecioCompra DECIMAL(18,2) NOT NULL,
    PrecioReventa DECIMAL(18,2) NULL, -- Permite nulos ya que puede no estar definido al inicio
    FechaRecepcion DATE NOT NULL DEFAULT GETDATE(), -- Por defecto toma la fecha del sistema
    Vendido BIT NOT NULL DEFAULT 0, -- 0 = En Venta, 1 = Vendido
    Descripcion VARCHAR(MAX) NULL,  -- Texto largo para detalles
    ImagenUrl VARCHAR(500) NULL,    -- Ruta o link de la fotografía

    -- Relaciones (Claves Foráneas)
    EstadoId INT NOT NULL,
    TrabajadorId INT NOT NULL,

    -- Restricciones de integridad referencial
    CONSTRAINT FK_Vehiculos_Estados FOREIGN KEY (EstadoId) REFERENCES EstadosVehiculo(Id),
    CONSTRAINT FK_Vehiculos_Trabajadores FOREIGN KEY (TrabajadorId) REFERENCES Trabajadores(Id)
);
GO


-- =========================================================================
-- 3. INSERCIÓN DE DATOS SEMILLA (SEED DATA)
-- =========================================================================
-- Insertamos las condiciones físicas requeridas por la prueba
INSERT INTO EstadosVehiculo (Nombre) VALUES 
('Como nuevo'),
('Muy bueno'),
('Bueno'),
('Regular'),
('Dañado'),
('Muy dañado');
GO

-- 1. INSERTAR TRABAJADORES DE PRUEBA (Para poder asociar los autos)
INSERT INTO Trabajadores (Nombre, Correo, Contrasena, Pais) VALUES
('Juan Perez', 'juan@correo.com', 'hash_pass_123', 'Guatemala'),
('Maria Lopez', 'maria@correo.com', 'hash_pass_456', 'El Salvador'),
('Carlos Gomez', 'carlos@correo.com', 'hash_pass_789', 'Honduras');
GO

-- 2. INSERTAR 10 VEHÍCULOS DE PRUEBA (En venta y vendidos, con diferentes estados)
INSERT INTO Vehiculos (Marca, Color, ModeloAño, PrecioCompra, PrecioReventa, Vendido, Descripcion, ImagenUrl, EstadoId, TrabajadorId) VALUES
-- Autos En Venta (Vendido = 0)
('Subaru', 'Azul', 2013, 10000.00, 13500.00, 0, 'Vehiculo en optimas condiciones, unico duenio, mantenimientos al dia.', 'images/subaru_azul.jpg', 1, 1),
('Honda', 'Gris', 2015, 8000.00, 10500.00, 0, 'Motor excelente, aire acondicionado frio, detalles esteticos menores.', 'images/honda_gris.jpg', 3, 2),
('Nissan', 'Blanco', 2010, 3500.00, 5000.00, 0, 'Ideal para proyecto, requiere reparacion de pintura y suspension.', 'images/nissan_blanco.jpg', 5, 3),
('Mazda', 'Azul', 2014, 7500.00, 9800.00, 0, 'Muy economico, automatico, interior limpio, listo para traspaso.', 'images/mazda_azul.jpg', 3, 1),
('Kia', 'Verde', 2019, 11000.00, 14000.00, 0, 'Semi-nuevo, poco kilometraje, mandos al timon, unico duenio.', 'images/kia_verde.jpg', 1, 3),
('BMW', 'Negro', 2016, 15000.00, 19500.00, 0, 'Version de lujo, tapiceria de cuero, servicios de agencia, impecable.', 'images/bmw_negro.jpg', 2, 1),

-- Autos ya Vendidos (Vendido = 1)
('Toyota', 'Rojo', 2018, 12000.00, 15000.00, 1, 'Vendido - Genero una ganancia rapida, excelente estado general.', 'images/toyota_rojo.jpg', 2, 1),
('Ford', 'Negro', 2012, 5000.00, 7200.00, 1, 'Vendido - Se entrego con mantenimiento completo realizado.', 'images/ford_negro.jpg', 4, 2),
('Hyundai', 'Plata', 2017, 9000.00, 11800.00, 1, 'Vendido - Financiamiento aprobado de inmediato.', 'images/hyundai_plata.jpg', 2, 3),
('Chevrolet', 'Amarillo', 2011, 4000.00, 5500.00, 1, 'Vendido - Comprado para repuestos/reconstruccion.', 'images/chevrolet_amarillo.jpg', 6, 2);
GO



-- =========================================================================
-- 4. PROCEDIMIENTOS ALMACENADOS (STORED PROCEDURES)
-- =========================================================================

-- SP 1: Filtrado, búsqueda y consulta de vehículos
ALTER PROCEDURE sp_ConsultarVehiculos
    @EstadoId INT = NULL,
    @Vendido BIT = NULL,
    @TrabajadorId INT = NULL,
    @Marca VARCHAR(100) = NULL,       -- NUEVO: Parámetro para buscar marca exacta
    @ModeloAño INT = NULL,           -- NUEVO: Parámetro para buscar año exacto
    @TextoBuscar VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON; 

    SELECT 
        v.Id,
        v.Marca,
        v.Color,
        v.ModeloAño,
        v.PrecioCompra,
        v.PrecioReventa,
        v.FechaRecepcion,
        v.Vendido,
        v.Descripcion,
        v.ImagenUrl,
        e.Nombre AS EstadoFisico,
        t.Nombre AS RegistradoPor
    FROM Vehiculos v
    INNER JOIN EstadosVehiculo e ON v.EstadoId = e.Id
    INNER JOIN Trabajadores t ON v.TrabajadorId = t.Id
    WHERE 
        (@EstadoId IS NULL OR v.EstadoId = @EstadoId)
        AND (@Vendido IS NULL OR v.Vendido = @Vendido)
        AND (@TrabajadorId IS NULL OR v.TrabajadorId = @TrabajadorId)
        AND (@Marca IS NULL OR v.Marca = @Marca) -- Filtro opcional por marca exacta
        AND (@ModeloAño IS NULL OR v.ModeloAño = @ModeloAño) -- Filtro opcional por año exacto
        AND (@TextoBuscar IS NULL 
             OR v.Marca LIKE '%' + @TextoBuscar + '%' 
             OR v.Color LIKE '%' + @TextoBuscar + '%')
    ORDER BY v.FechaRecepcion DESC;
END;
GO


-- SP 2: Estadísticas de desempeño para la vista de perfil del trabajador
ALTER PROCEDURE sp_ObtenerEstadisticasTrabajador
    @TrabajadorId INT = NULL -- ¡NUEVO: Ahora es opcional y por defecto es NULL!
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        t.Id AS TrabajadorId,
        t.Nombre AS NombreTrabajador,
        t.Pais AS PaisTrabajador,
        COUNT(v.Id) AS TotalRegistrados,
        SUM(CASE WHEN v.Vendido = 0 THEN 1 ELSE 0 END) AS TotalEnVenta,
        SUM(CASE WHEN v.Vendido = 1 THEN 1 ELSE 0 END) AS TotalVendidos,
        -- COALESCE garantiza que si la ganancia es NULL (ej. tiene 0 ventas), devuelva 0.00 en lugar de vacío
        COALESCE(SUM(CASE WHEN v.Vendido = 1 THEN (v.PrecioReventa - v.PrecioCompra) ELSE 0 END), 0) AS GananciaTotalGenerada
    FROM Trabajadores t
    -- Usamos LEFT JOIN para que los trabajadores nuevos (con 0 autos) no sean excluidos del reporte
    LEFT JOIN Vehiculos v ON t.Id = v.TrabajadorId
    WHERE 
        (@TrabajadorId IS NULL OR t.Id = @TrabajadorId)
    GROUP BY 
        t.Id, 
        t.Nombre, 
        t.Pais
    ORDER BY 
        GananciaTotalGenerada DESC; -- Genera un ranking automático del empleado más rentable al menos rentable
END;
GO

-- SP 3: reporte de años de vehículos más vendidos y sus ganancias
create PROCEDURE sp_ReporteVentasPorAño
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ModeloAño AS AñoVehiculo, 
        COUNT(*) AS CantidadVehiculosVendidos,
        SUM(PrecioReventa - PrecioCompra) AS GananciaTotalGenerada
    FROM Vehiculos
    WHERE Vendido = 1 -- Solo nos interesan los autos que ya se vendieron
    GROUP BY ModeloAño
    ORDER BY CantidadVehiculosVendidos DESC; -- Ordena de mayor a menor cantidad de ventas
END;
GO

-- ver vheiculo especifico
SELECT * FROM Vehiculos
WHERE Marca = 'BMW' 
 
-- ver vehiculos vendidos y no vendidos
SELECT * FROM Vehiculos
WHERE Vendido = '0'


EXEC sp_ConsultarVehiculos;
EXEC sp_ConsultarVehiculos @Vendido = 0;
EXEC sp_ConsultarVehiculos @TextoBuscar = 'Azul';
EXEC sp_ConsultarVehiculos @Marca = 'Toyota', @Vendido = 1;
EXEC sp_ConsultarVehiculos @TrabajadorId = 1, @ModeloAño = 2013;


select * from dbo.EstadosVehiculo;
select * from dbo.Trabajadores;
select * from dbo.Vehiculos;


EXEC sp_ConsultarVehiculos @TrabajadorId = 2;
EXEC sp_ObtenerEstadisticasTrabajador @TrabajadorId = 2;
EXEC sp_ObtenerEstadisticasTrabajador 
EXEC sp_ReporteVentasPorAño;


/*
==========================================================================================
PROCEDIMIENTO 1: sp_ConsultarVehiculos
=========================================================================================
PARÁMETROS DE ENTRADA:
   * @EstadoId (INT - Opcional): ID del estado físico en catálogo (1 = Como nuevo, 5 = Dañado, etc.).
   * @Vendido (BIT - Opcional): Filtro de estado comercial (0 = En Venta, 1 = Vendido).
   * @TrabajadorId (INT - Opcional): Filtro para ver únicamente los autos registrados por este empleado.
   * @Marca (VARCHAR(100) - Opcional): Filtra por coincidencia exacta del nombre de la marca.
   * @ModeloAño (INT - Opcional): Filtra por el año exacto de fabricación del vehículo.
   * @TextoBuscar (VARCHAR(100) - Opcional): Buscador de texto libre que busca coincidencias parciales
                                            en la Marca o en el Color utilizando el comodín '%'.

EJEMPLOS DE USO / CASOS DE NEGOCIO:

   -- Caso 1: Cargar el catálogo general por defecto (Muestra todos los autos sin filtros)
   EXEC sp_ConsultarVehiculos;

   -- Caso 2: Cargar solo los autos que siguen "En Venta" (Filtro comercial rápido)
   EXEC sp_ConsultarVehiculos @Vendido = 0;

   -- Caso 3: Buscar coincidencias con la palabra "Azul" (Buscador de texto libre)
   EXEC sp_ConsultarVehiculos @TextoBuscar = 'Azul';

   -- Caso 4: Filtrar autos de la marca Toyota que ya se vendieron
   EXEC sp_ConsultarVehiculos @Marca = 'Toyota', @Vendido = 1;

   -- Caso 5: Ver el inventario de autos del año 2013 que registró el trabajador Juan Pérez (ID = 1)
   EXEC sp_ConsultarVehiculos @TrabajadorId = 1, @ModeloAño = 2013;
==========================================================================================
*/


/*
==========================================================================================
PROCEDIMIENTO 2: sp_ObtenerEstadisticasTrabajador
==========================================================================================
DATOS QUE RETORNA (COLUMNAS):
   * TotalRegistrados: Cantidad total de vehículos que el empleado ha ingresado al sistema.
   * TotalEnVenta: Cantidad de vehículos registrados por él que siguen disponibles para la compra.
   * TotalVendidos: Cantidad de vehículos registrados por él que ya fueron exitosamente vendidos.
   * GananciaTotalGenerada: El margen financiero acumulado de sus ventas (Suma de PrecioReventa - PrecioCompra 
                            únicamente para sus registros con Vendido = 1).

EJEMPLOS DE USO / CASOS DE NEGOCIO:

   -- Caso 1: Obtener el resumen de rendimiento e ingresos generados por María López (ID = 2)
   EXEC sp_ObtenerEstadisticasTrabajador @TrabajadorId = 2;

   -- Caso 2: Obtener el rendimiento e ingresos generados por Juan Pérez (ID = 1)
   EXEC sp_ObtenerEstadisticasTrabajador @TrabajadorId = 1;
==========================================================================================
*/


/*
==========================================================================================
PROCEDIMIENTO 3: sp_ReporteVentasPorAnio
==========================================================================================
DESCRIPCIÓN GLOBAL:
   Procedimiento de inteligencia de negocios (BI) que agrupa el histórico de ventas reales
   clasificándolas por el año de fabricación de los vehículos.

PARÁMETROS DE ENTRADA:
   * No requiere parámetros de entrada (analiza todo el universo de datos vendidos).

EJEMPLOS DE USO / CASOS DE NEGOCIO:

   -- Caso 1: Generar el reporte histórico de ventas agrupado por año (Ordenado de mayor a menor éxito)
   EXEC sp_ReporteVentasPorAnio;
==========================================================================================
*/