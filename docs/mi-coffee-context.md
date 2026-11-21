# Mi Coffee — Contexto inicial del equipo

Este documento es el contexto general del proyecto: fija el alcance, la arquitectura y las convenciones que comparte el equipo. Cada integrante arma después su propio plan de fases dentro de su zona. No se asumen funcionalidades que no estén escritas aquí ni se agrega complejidad que no se haya pedido.

## Proyecto

Mi Coffee es un sistema gestor para una cafetería. Se construye a partir del código base del sistema **Veterinaria** (Práctica 5, Partes 1 y 2, materia Ambientes de Programación Visual, UABC), que define la estructura por capas, la integración con MySQL y el flujo de ramas. La prioridad es que sea simple pero funcional.

Una sola aplicación web con dos interfaces, ambas utilizables en tablet (tipo pantalla de autoservicio) y en PC:

- **Interfaz del cliente:** el cliente arma su pedido, ve el precio y confirma. No requiere inicio de sesión.
- **Interfaz del personal (empleados y admin):** gestión de órdenes, inventario, alertas y proveedores. Requiere inicio de sesión.

## Equipo

Tres integrantes, un rol por persona.

| Rol                                                            | Integrante | Zona de trabajo                               | Rama       |
| -------------------------------------------------------------- | ---------- | --------------------------------------------- | ---------- |
| Backend (administrador del repositorio y líder de integración) | Mike       | Entities, Business, Data, Connection          | `backend`  |
| Frontend                                                       | Mextli     | Web (Razor Pages, Bootstrap, navegación)      | `frontend` |
| DBA y Pruebas                                                  | Emir       | Carpeta `database/` (scripts MySQL) y pruebas | `dba`      |

## Repositorio y flujo de Git

- Repositorio remoto en GitHub: **https://github.com/team-mextli/mi-coffee**
- Ramas: `main` (versión estable e integrada), `backend`, `frontend`, `dba`. Nadie trabaja directamente en `main`; no se eliminan las ramas de trabajo.
- Herramienta: Git por línea de comandos.
- Integración: cada rama se une a `main` mediante Pull Request en GitHub.
- Antes de abrir un Pull Request, la solución debe compilar con 0 errores.

## Tecnologías

Visual Studio 2026, C#, .NET 10, ASP.NET Core Razor Pages, Bootstrap, MySQL Server, MySQL Workbench, paquete NuGet `MySql.Data`.

`MySql.Data` se instala solo en la capa Data y solo lo instala el Backend; los demás restauran la versión registrada en el `.csproj`.

## Arquitectura por capas

```text
Web  →  Business  →  Data  →  MySQL

Entities: clases compartidas, referenciadas por Web, Business y Data
```

| Proyecto | Responsabilidad                                       | No debe contener                   |
| -------- | ----------------------------------------------------- | ---------------------------------- |
| Web      | Razor Pages, Bootstrap, Layout, formularios, mensajes | SQL directo, `MySqlConnection`     |
| Business | Validaciones y reglas de negocio                      | HTML, Razor, Bootstrap             |
| Data     | SQL, `MySqlConnection`, `MySqlCommand`, `DataReader`  | Interfaz visual                    |
| Entities | Clases de entidad                                     | Referencias a Web, Business o Data |

Referencias permitidas: Web → Business y Entities; Business → Data y Entities; Data → Entities. Nunca en sentido contrario.

## Convenciones

- **Idioma:** inglés en el código, los comentarios, los nombres de proyectos, carpetas y archivos, y en las tablas y columnas de la base de datos. El texto que ve el usuario en la interfaz y la documentación del repositorio se escriben en español. El glosario indica el nombre en código de cada término.
- **Entities:** propiedades en PascalCase con DataAnnotations para validación (`Required`, `StringLength`, `Range`); cadenas obligatorias inicializadas con `string.Empty`, opcionales como `string?`.
- **Base de datos:** tablas en `snake_case` y singular; clave primaria `<tabla>_id`; clave foránea con el nombre de la clave que referencia; charset `utf8mb4`, collation `utf8mb4_unicode_ci`. Correspondencia con C#: `ProductId` ↔ `product_id`.
- **Data:** una clase `<Entity>Repository` por entidad; consultas siempre parametrizadas (`@parameter`); la conexión se obtiene de la clase estática `Connection`.
- **Business:** una clase `<Entity>Service` por entidad; valida y delega a su `<Entity>Repository`. `BusinessConfiguration` inicializa la conexión.
- **Web:** una carpeta por módulo en `Pages/` (por ejemplo, `Products`) con las páginas `Index`, `Create`, `Edit` y `Details`; navegación en `_Layout.cshtml`.
- **Script SQL:** el esquema vive en `database/schema.sql` (contiene `DROP DATABASE IF EXISTS`). Git comparte el script; la base física de MySQL es local a cada computadora.
- **Credenciales:** la cadena de conexión se lee de `ConnectionStrings:<nombre>` mediante User Secrets. El `UserSecretsId` en `Web.csproj` se versiona; el `secrets.json` es local y nunca se sube ni se copia entre integrantes. Ninguna contraseña en el repositorio.

## Alcance funcional

### Usuarios del sistema

| Usuario  | Acceso               | Puede                                                                                                 |
| -------- | -------------------- | ----------------------------------------------------------------------------------------------------- |
| Cliente  | Sin inicio de sesión | Hacer pedidos                                                                                         |
| Empleado | Con inicio de sesión | Ver y gestionar órdenes, ver alertas, actualizar existencias, consultar proveedores                   |
| Admin    | Con inicio de sesión | Todo lo del empleado, más: catálogo, insumos y límites, gestión de proveedores, usuarios del personal |

Las contraseñas del personal se guardan con hash, nunca en texto plano. Siempre hay al menos un administrador activo: el último no se puede desactivar ni dejar de ser administrador.

### Pedidos del cliente

- Navegar el catálogo por categorías y elegir un producto. Los productos marcados como no disponibles no se pueden pedir.
- Personalizar el producto (tamaño, leche, endulzante, extras) con las opciones que el admin haya definido. Cada opción puede tener un precio adicional.
- Indicar cantidad, nota opcional y nombre opcional del cliente.
- Ver el carrito con el precio de cada producto y el total, actualizados en pantalla.
- Al confirmar, la pantalla muestra la confirmación con el número de orden.

### Gestión de órdenes (empleado y admin)

- Cada orden confirmada llega a la lista del personal como recibo: número de orden, fecha y hora, productos con sus opciones y total.
- Estados: Recibida → En preparación → Lista → Entregada. Una orden también puede quedar Cancelada. El personal cambia el estado.
- Trazabilidad: cada cambio de estado registra quién lo hizo y cuándo. Las órdenes no se borran. El historial se puede consultar filtrando por fecha y estado.

### Catálogo (admin)

- Categorías de producto, productos (nombre, categoría, precio base, disponible o no) y opciones de personalización (grupo, nombre, precio adicional).

### Inventario y alertas

- Insumo: lo que la cafetería compra para operar (granos de café, leche, azúcar, vasos, tapas, etc.), con categoría, unidad de medida, existencia actual y límite mínimo. El admin define las categorías y los límites.
- La existencia la actualiza manualmente el empleado o el admin (entradas de mercancía y ajustes).
- Cuando la existencia de un insumo llega o baja del límite mínimo, se muestra una alerta en la interfaz del personal con el insumo y el proveedor al que se le compra.
- Cada insumo puede tener asignado un proveedor.

### Proveedores

- Lista centralizada con nombre, contacto y nota de cada proveedor, para consultar a quién llamar cuando hay que comprar mercancía.

### Eliminación de registros

Solo el admin elimina registros. Un registro se elimina únicamente si ningún otro depende de él; si está en uso, el sistema rechaza la eliminación y muestra un mensaje. Lo que deja de usarse pero ya forma parte de órdenes se retira en lugar de eliminarse.

| Registro              | Se elimina si                                                          | Si está en uso                                |
| --------------------- | ---------------------------------------------------------------------- | --------------------------------------------- |
| Categoría de producto | No tiene productos                                                     | Mover o eliminar sus productos                |
| Producto              | No aparece en ninguna orden; sus opciones asignadas se eliminan con él | Marcarlo como no disponible                   |
| Opción                | No aparece en ninguna orden; sus asignaciones se eliminan con ella     | Quitarla de sus productos                     |
| Proveedor             | Ningún insumo lo tiene asignado                                        | Cambiar o quitar el proveedor de esos insumos |
| Categoría de insumo   | No tiene insumos                                                       | Mover o eliminar sus insumos                  |
| Insumo                | Siempre: ningún otro registro depende de él                            | —                                             |

Los usuarios del personal se desactivan y las órdenes, con su historial, no se eliminan.

### Fuera de alcance

- Cobro o pagos en línea: el sistema muestra el total; el cobro se hace fuera del sistema.
- Descuento automático del inventario por cada pedido.
- Comunicación en tiempo real, correo o SMS: bastan la actualización periódica de la página y los avisos dentro de la interfaz.
- Cuentas de cliente, puntos o promociones.
- Reportes avanzados y facturación.
- Varias sucursales.

## Glosario

La interfaz y la documentación usan el término en español; el código y la base de datos, el nombre en inglés.

| Término   | Significado                                                        |
| --------- | ------------------------------------------------------------------ |
| Producto  | Lo que el cliente pide (café u otro artículo del catálogo)         |
| Opción    | Personalización de un producto, con precio adicional               |
| Orden     | Pedido confirmado, con detalle, total y estado                     |
| Insumo    | Lo que la cafetería compra para operar                             |
| Proveedor | Quien surte los insumos (los repartidores)                         |
| Alerta    | Aviso de que un insumo está en su límite mínimo o por debajo de él |

## Acuerdos de trabajo

- Cada integrante es responsable de su zona y decide su implementación.
- Solo se avisa al equipo antes de cambiar un contrato compartido: nombres o propiedades de entidades, tablas o columnas, rutas de páginas, `Program.cs`, `.csproj` o versiones de paquetes.
- Si un archivo ajeno a la zona propia necesita cambios, se pide al dueño de la zona.
- En cada fase, el Backend sube primero las entidades y las firmas de los métodos de Business mediante un Pull Request pequeño, y cada integrante actualiza su rama con `main` antes de continuar.
