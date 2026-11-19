# Mi Coffee — Plan de fases del Backend

Plan individual del Backend (Mike) dentro de su zona: Entities, Business, Data y Connection. Parte de `mi-coffee-plan-phases.md` (orden de las fases y tareas por integrante) y de `mi-coffee-context.md` (alcance, arquitectura y convenciones). Este documento no repite el alcance funcional: cada fase indica la sección del contexto donde está definido.

Las decisiones marcadas como **pendiente** no están escritas en el contexto y se confirman con el equipo antes de implementarse. Las marcadas como **propuesta** son la opción más simple considerada y pueden cambiarse sin afectar el resto del plan.

---

## Cómo se avanza en cada fase

Al iniciar cada fase se abre un issue `feat(<scope>)` (obligatorio según `CONTRIBUTING.md`). El PR de la Parte A lo referencia; el PR de implementación lo cierra con `Closes #<n>`.

Cada fase se divide en las mismas partes, en este orden:

1. **Parte A — PR de contrato.** Entidades y firmas de los métodos de Business (cuerpos con `NotImplementedException`), como pide «Acuerdos de trabajo» del contexto. Título `feat(<scope>): ...`.
2. **Parte B — Data.** `<Entity>Repository` con consultas parametrizadas y conexión desde `Connection`.
3. **Parte C — Business.** `<Entity>Service` con validaciones y reglas de la fase; sustituye los cuerpos de la Parte A.
4. **Parte D — Verificación y PR de implementación.** Compilación con 0 errores y prueba manual contra el MySQL local creado con `schema.sql`; el resto del flujo sigue `CONTRIBUTING.md`. Las Partes B y C pueden ir en un solo PR; si crece demasiado, se divide en sub-issues.

### Decisiones transversales

- **Errores de validación hacia Web.** Un único mecanismo para todos los servicios, definido en la Fase 1. **Pendiente:** revisar qué patrón usa el código base de Veterinaria y seguirlo; si no define ninguno, una excepción de negocio propia con mensaje en español para la interfaz.
- **Roles.** La restricción por rol se aplica en Web (políticas y `[Authorize]`). Los servicios de Business no verifican rol; reciben el `userId` solo cuando la regla lo requiere (trazabilidad).
- **Transacciones.** Toda operación que escribe en más de una tabla usa una sola transacción.

---

## Fase 0 — Base y modelo de datos

Sin trabajo pendiente del Backend: la plantilla está en `main`.

---

## Fase 1 — Acceso del personal

Sección del contexto: «Usuarios del sistema». Tabla: `user`. Scope de commits: `auth`.

**Parte 0 — Preparación.** Rama `backend` actualizada con `main`, `schema.sql` ejecutado en el MySQL local y `ConnectionStrings:MiCoffee` en User Secrets. El arranque en Development imprime `MYSQL OK.` cuando la conexión funciona.

**Parte A — Contrato.**

- `MiCoffee.Entities`: `User` (`UserId`, `FullName`, `Username`, `PasswordHash`, `Role`, `IsActive`; longitudes 100, 50 y 255 como en el esquema) y enum `UserRole` (`Employee`, `Admin`).
- `MiCoffee.Business`: `UserService` con `Authenticate(username, password)`, `GetAll`, `GetById`, `Create(user, password)`, `Update(user, newPassword)` y `SetActive(userId, isActive)`. La contraseña viaja como parámetro aparte para que `PasswordHash` nunca se llene desde un formulario.
- En la descripción del PR: el contrato de claims y el fragmento de `Program.cs` (ver Entregas).

**Parte B — Data.** `UserRepository`: `GetByUsername`, `GetAll`, `GetById`, `Insert`, `Update` y `UpdateActive`; con la regla del último Admin, también un conteo de administradores activos. El rol se guarda y se lee como texto (`'Employee'`, `'Admin'`).

**Parte C — Business.**

- Hash con `PasswordHasher<User>` del paquete `Microsoft.Extensions.Identity.Core`, instalado en Business (cambio de `.csproj` y paquetes: se avisa al equipo antes). Firmas verificadas en el código fuente de Microsoft: `HashPassword(user, password)` devuelve `string` y `VerifyHashedPassword(user, hashedPassword, providedPassword)` devuelve `PasswordVerificationResult`. Versión: la 10.x estable más reciente, elegida al instalar.
- `Authenticate` devuelve `null` si el usuario no existe, está inactivo o la contraseña no coincide, sin distinguir la causa.
- Campos requeridos y `username` único (revisión previa más la restricción UNIQUE como respaldo; la collation `utf8mb4_unicode_ci` no distingue mayúsculas).
- Los usuarios no se eliminan; solo se desactivan.

**Entregas a los demás** (después de la Parte C).

- A Emir: el hash del administrador inicial, generado con el código real de Business. La contraseña en texto plano se comparte por fuera del repositorio. Se comprueba con un hash real que cabe en `VARCHAR(255)`.
- A Mextli: fragmento de `Program.cs` (esquema de cookies, `UseAuthentication()` antes de `UseAuthorization()`, políticas «personal» —Employee o Admin— y «solo Admin», aplicadas por carpeta) y contrato de claims: `NameIdentifier` = `UserId` (lo requiere la Fase 4), `Name` = `Username`, `Role` = `Employee` o `Admin`. `Program.cs` es del proyecto Web, por lo que lo aplica el dueño de esa zona.

**Pendientes de la fase.**

- Cambio de contraseña. **Propuesta:** el admin la fija al crear y puede asignar una nueva al editar (campo opcional); sin cambio por el propio usuario.
- Último Admin activo. **Propuesta:** no se permite desactivarlo ni cambiarle el rol.

---

## Fase 2 — Catálogo

Sección del contexto: «Catálogo (admin)». Tablas: `product_category`, `product`, `product_option`, `product_allowed_option`. Scope: `catalog`.

**Parte A — Contrato.** Entidades `ProductCategory`, `Product` y `ProductOption`; servicios `ProductCategoryService`, `ProductService` y `ProductOptionService`. La asignación de opciones a un producto (tabla `product_allowed_option`) se maneja con métodos de `ProductService` y `ProductRepository`, sin entidad propia (**propuesta**). Se incluye una consulta para la Fase 3: categorías, productos disponibles y opciones permitidas por producto.

**Parte B — Data.** Un repositorio por entidad; el de productos incluye leer y reemplazar las opciones permitidas de un producto.

**Parte C — Business.** `base_price` y `extra_price` mayores o iguales a 0; nombre de categoría único; categoría existente al crear un producto; campos requeridos con las longitudes del esquema.

**Pendiente.** Eliminación de categorías, productos y opciones: el contexto no la menciona y los productos están referenciados por `order_item`. **Propuesta:** sin eliminación; los productos se retiran con `is_available`.

---

## Fase 3 — Pedidos del cliente

Sección del contexto: «Pedidos del cliente». Tablas: `customer_order`, `order_item`, `order_item_option`. Scope: `orders`.

**Parte A — Contrato.** Entidades `CustomerOrder` (con su lista de `OrderItem`), `OrderItem` (con su lista de `OrderItemOption`) y `OrderItemOption`; `OrderService.Create(order)`, que devuelve el número de orden; y un método de cálculo de total reutilizable, para que el carrito de Web no replique reglas de negocio.

**Parte B — Data.** Inserción de orden, ítems y opciones en una sola transacción; lectura de producto y opciones para precios.

**Parte C — Business.**

- Al menos un ítem; cantidad mayor o igual a 1; producto disponible.
- Cada opción debe estar permitida para el producto (`product_allowed_option`).
- `unit_price` y `extra_price` se leen de la base al confirmar y se copian a la orden, como indican los comentarios del esquema. El total lo calcula Business; no se recibe del cliente.
- El número de orden es `customer_order_id`. El estado inicial es `Received`.
- La creación no registra fila en `order_status_history`: esa tabla exige `user_id` y el cliente no inicia sesión.

**Pendiente.** Fórmula de cada línea. **Propuesta:** `(unit_price + suma de extra_price) × quantity`.

---

## Fase 4 — Gestión de órdenes

Sección del contexto: «Gestión de órdenes (empleado y admin)». Tablas: `customer_order`, `order_status_history`. Scope: `orders`.

**Parte A — Contrato.** Enum `OrderStatus` (`Received`, `InPreparation`, `Ready`, `Delivered`, `Cancelled`, igual que el esquema), entidad `OrderStatusHistory` y, en `OrderService`: `GetActive`, `GetById` (recibo con ítems y opciones), `ChangeStatus(orderId, newStatus, userId)` y `Search(from, to, status)` para el historial.

**Parte B — Data.** `ChangeStatus` actualiza el estado e inserta el historial en una sola transacción. El `UPDATE` incluye el estado previo en el `WHERE` y se verifica el número de filas afectadas, para que dos cambios simultáneos sobre la misma orden no se pisen. El historial filtra por rango de `created_at` (el filtro incluye el día completo del extremo final) y por estado.

**Parte C — Business.** Transiciones válidas; `userId` obligatorio.

**Pendientes.**

- Transiciones permitidas. **Propuesta:** avance de un paso (`Received` → `InPreparation` → `Ready` → `Delivered`) y cancelación desde `Received`, `InPreparation` o `Ready`; `Delivered` y `Cancelled` son finales.
- Qué muestra la lista del personal. **Propuesta:** las órdenes en `Received`, `InPreparation` y `Ready`; el resto se consulta en el historial. Se confirma con Mextli, que construye la pantalla.

---

## Fase 5 — Proveedores

Sección del contexto: «Proveedores». Tabla: `supplier`. Scope: `suppliers`.

**Parte A — Contrato.** Entidad `Supplier` y `SupplierService` con `GetAll`, `GetById`, `Create` y `Update`. Sin eliminación: el contexto pide alta, edición y consulta, y `supply.supplier_id` referencia la tabla.

**Parte B y C.** `SupplierRepository`; `name` y `contact` requeridos (100), `note` opcional (255).

---

## Fase 6 — Inventario y alertas

Sección del contexto: «Inventario y alertas». Tablas: `supply_category`, `supply`, con `supplier` para la alerta. Scope: `inventory`.

**Parte A — Contrato.** Entidades `SupplyCategory` y `Supply` (`SupplierId` nullable; `Supplier` cargado en las alertas); `SupplyCategoryService` y `SupplyService` con alta y edición (categoría, unidad, límite mínimo, proveedor), `AddStock` (entrada de mercancía), `SetStock` (ajuste) y `GetAlerts`, que devuelve los `Supply` en alerta.

**Parte B — Data.** `GetAlerts` calcula la alerta en la consulta: `current_stock <= minimum_stock`, con unión izquierda a proveedor (`supplier_id` admite nulo). El esquema indica que las alertas no se guardan.

**Parte C — Business.** `current_stock` y `minimum_stock` mayores o iguales a 0 (restricciones del esquema); `AddStock` con cantidad mayor a 0; categoría existente.
