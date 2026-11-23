# Mi Coffee — Plan de fases del Frontend

Plan individual del Frontend (Mextli) dentro de su zona: el proyecto Web (Razor Pages, Bootstrap y navegación). Parte de `mi-coffee-plan-phases.md` (orden de las fases y tareas por integrante) y de `mi-coffee-context.md` (alcance, arquitectura y convenciones). Este documento no repite el alcance funcional: cada fase indica la sección del contexto donde está definido.

Las decisiones marcadas como **pendiente** no están escritas en el contexto y se confirman con el equipo antes de implementarse. Las marcadas como **propuesta** son la opción más simple considerada y pueden cambiarse sin afectar el resto del plan.

---

## Cómo se avanza en cada fase

1. **Actualizar la rama.** `frontend` se actualiza con `main` antes de empezar y cada vez que se fusiona un PR.
2. **Esperar el contrato del Backend.** El PR pequeño de Mike con las entidades y las firmas de Business de la fase. Mientras no esté en `main`, solo se avanza con las vistas, en commits locales y sin conectarlas a servicios cuyos nombres no existen.
3. **Issue.** Se abre un issue `feat(<scope>)` al iniciar la fase (obligatorio para `feat` y `fix` según `CONTRIBUTING.md`).
4. **Pantallas.** Una carpeta por módulo con las páginas `Index`, `Create` y `Edit` (y `Details` o `Delete` cuando la fase lo pida). Cada página llama a los servicios de Business y usa las entidades; nunca escribe SQL ni usa `MySqlConnection`.
5. **Verificación.** Compilación con 0 errores y prueba manual contra el MySQL local creado con `schema.sql` y `seed.sql`, en tres anchos: 390 px, 768 px y escritorio.
6. **PR.** De `frontend` a `main`, con título `feat(<scope>): ...` y `Closes #<n>`. Mientras el PR está abierto no se hace push a `frontend`: el trabajo siguiente se guarda en commits locales y se sube después de fusionar. Solo se hace push con el PR abierto para corregir lo que pida la revisión.

### Decisiones transversales

- **Idioma.** El texto que ve el usuario va en español; los nombres de archivos, carpetas, clases y propiedades, en inglés.
- **Dos layouts.** `_CustomerLayout.cshtml` para la interfaz del cliente y `_StaffLayout.cshtml` para la del personal, ambos en `Pages/Shared/`. `_Layout.cshtml` no se modifica y queda como respaldo. El personal usa su layout mediante un `_ViewStart.cshtml` dentro de `Pages/Staff/`. El contexto indica que la navegación vive en `_Layout.cshtml`; el uso de dos layouts se avisa al Backend. **Pendiente:** confirmar con el equipo qué se hace con `_Layout.cshtml` cuando todas las páginas usen los suyos.
- **Estilo.** Paleta en variables CSS dentro de `:root` de `wwwroot/css/site.css`: azul `#88b8ce` (interfaz del cliente), ámbar `#f1c166` (interfaz del personal), crema `#fefaf0` (fondo) y café `#542916` (texto y botones principales). Franja de color de lado a lado, caja de borde punteado y esquinas redondeadas. Tipografía del nombre: Chelsea Market. Los estilos de formularios, tablas, botones y avisos del personal están limitados a `.staff-body`, y los del cliente a `.customer-body`.
- **Tablet y PC.** Diseño con Bootstrap responsivo. En la interfaz del cliente, botones grandes (alto mínimo de 48 px) para pantalla táctil. Las tablas van dentro de `table-responsive`.
- **Errores y avisos.** Cada PageModel expone una propiedad `Error` que la vista muestra en un aviso rojo; si la carga falla, no se muestra el formulario y se ofrece un botón Regresar. Los mensajes de las reglas de negocio (`BusinessException`) se muestran tal cual. Los errores inesperados se registran con `Debug.WriteLine` y al usuario solo se le muestra un mensaje general en español; nunca el texto de una excepción técnica ni la cadena de conexión.
- **Mensajes de éxito.** Después de crear, editar o cambiar un estado, la página redirige y el aviso verde sale de `TempData["Message"]`, que se muestra una sola vez en `_StaffLayout`.
- **Validación.** Cada campo lleva `asp-validation-for`; los mensajes vienen de los DataAnnotations de las entidades y la sección `Scripts` carga `_ValidationScriptsPartial`.
- **Roles.** El menú y los botones se ocultan según el rol (`User.IsInRole("Admin")`), pero la restricción real la aplica `Program.cs` por carpeta; Business no verifica rol.
- **Módulos futuros.** En el menú, un módulo cuya fase aún no existe se muestra con la insignia «Próximamente» y sin enlace, para no generar errores 404. Al terminar la fase, pasa a ser un enlace.
- **Sin SQL en Web.** Ninguna página contiene SQL, `MySqlConnection` ni reglas de negocio; el carrito, por ejemplo, usa el cálculo de total de Business.
- **Archivos compartidos.** `site.css`, `_ViewStart.cshtml`, `Program.cs` y las rutas de las páginas son contratos compartidos: se avisa al equipo antes de cambiarlos y solo se agregan estilos, nunca se borran los existentes.

---

## Fase 0 — Base y modelo de datos

Sección del contexto: «Alcance funcional» (completo). Scope: `ui`.

**Entregado.**

- `_CustomerLayout.cshtml`: franja azul con el logo y el nombre, y un área de contenido con borde punteado. Sin menú ni inicio de sesión.
- `_StaffLayout.cshtml`: misma estructura con franja ámbar.
- Variables de color y estilos de ambos layouts en `site.css`; logo en `wwwroot/images/logo_MiCoffee.png`.
- Páginas de prueba: `Pages/Index.cshtml` (cliente) y `Pages/Staff/Index.cshtml` (personal).

---

## Fase 1 — Acceso del personal

Sección del contexto: «Usuarios del sistema». Scope: `auth`.

**Parte A — Cookie y rutas (`Program.cs`).** `Program.cs` es del proyecto Web y lo aplica el Frontend, con el fragmento que entrega el Backend:

- Esquema de cookies con expiración fija de 8 horas y sin `SlidingExpiration`. La cookie no es persistente: no se usa `IsPersistent` ni una casilla «Recordarme».
- `UseAuthentication()` antes de `UseAuthorization()`.
- Dos políticas aplicadas por carpeta: personal (Employee o Admin) para `/Staff`, y solo Admin para `/Staff/Users`. `/Staff/Login` es anónima.
- Contrato de claims: `NameIdentifier` = `UserId` (lo requiere la Fase 4), `Name` = `Username` y `Role` = `Employee` o `Admin`.

**Parte B — Pantallas.**

| Ruta | Contenido |
| ---- | --------- |
| `/Staff/Login` | Formulario de usuario y contraseña. Un único aviso («Usuario o contraseña incorrectos.») sin distinguir la causa, igual que `UserService.Authenticate`. |
| `/Staff/Logout` | Cierra la sesión con un `POST` desde el menú y redirige al inicio de sesión. |
| `/Staff/Users` | Lista con nombre, usuario, rol y estado. Botones Editar y Activar o Desactivar; solo Admin. |
| `/Staff/Users/Create` | Nombre completo, usuario, rol y contraseña. |
| `/Staff/Users/Edit/{id}` | Nombre completo, usuario, rol y nueva contraseña opcional. El estado no se edita aquí: se cambia desde la lista con `SetActive`. |

**Parte C — Menú del personal.** En la franja ámbar, visible solo con sesión iniciada: Inicio, los módulos por rol, el nombre del usuario y el botón Cerrar sesión. El Admin ve además Usuarios y Catálogo. Los módulos de fases posteriores aparecen como «Próximamente».

**Decisiones.**

- El formulario enlaza `Password` o `NewPassword` como propiedad aparte; `PasswordHash` nunca se enlaza a un formulario.
- La propiedad de la entidad en el PageModel se llama `StaffUser`, porque `PageModel.User` es el usuario en sesión.
- Los usuarios no se eliminan: solo se activan y desactivan. El mensaje de «último administrador» lo define Business y se muestra tal cual.
- Un cambio de rol o la desactivación surte efecto en el siguiente inicio de sesión; la página no revisa nada adicional en cada petición.
- **Propuesta:** no se crea `Users/Details`, porque la lista ya muestra todos los campos del usuario.

**Verificación.** Inicio de sesión con contraseña incorrecta y correcta; menú de Admin y de Employee; un Employee no entra a `/Staff/Users`; crear, editar, desactivar y activar; el rechazo al desactivar o degradar al último Admin; cierre de sesión; `/` sigue abierta sin sesión.

---

## Fase 2 — Catálogo

Sección del contexto: «Catálogo (admin)». Scope: `catalog`. Solo el Admin entra a estas pantallas.

**Pantallas (propuesta de rutas).**

| Módulo | Carpeta | Páginas |
| ------ | ------- | ------- |
| Categorías de producto | `Pages/Staff/Categories` | `Index`, `Create`, `Edit`, `Delete` |
| Productos | `Pages/Staff/Products` | `Index`, `Create`, `Edit`, `Delete` |
| Opciones de personalización | `Pages/Staff/Options` | `Index`, `Create`, `Edit`, `Delete` |

**Campos** (según `schema.sql`).

- Categoría: nombre (hasta 50, único).
- Producto: categoría, nombre (hasta 100), precio base (mayor o igual a 0) y disponible.
- Opción: grupo (hasta 50), nombre (hasta 50) y precio adicional (mayor o igual a 0).

**Opciones permitidas por producto.** La asignación (`product_allowed_option`) se hace con casillas dentro de `Products/Edit` y también al crear, agrupadas por grupo de opción. La guardan métodos de `ProductService`, sin entidad propia (**propuesta** del Backend).

**Eliminación.** La página `Delete` pide confirmación y muestra el mensaje que devuelve Business si el registro está en uso, sin el botón de eliminar. Qué se puede eliminar está en «Eliminación de registros» del contexto:

- Categoría: solo si no tiene productos.
- Producto: solo si no aparece en ninguna orden; si está en uso, se marca como no disponible desde `Edit`.
- Opción: solo si no aparece en ninguna orden; si está en uso, se quita de sus productos.

**Menú.** «Catálogo» deja de ser «Próximamente» y pasa a ser un enlace o un menú con Categorías, Productos y Opciones, solo para Admin.

**Pendiente.** Nombres de los servicios y de sus métodos, y la forma de leer y guardar las opciones permitidas de un producto: dependen del PR de contrato del Backend. Carpetas protegidas con la política solo Admin en `Program.cs`.

**Verificación.** Alta, edición y eliminación permitida de cada registro; el rechazo de cada eliminación en uso; productos no disponibles; precios negativos rechazados.

---

## Fase 3 — Pedidos del cliente

Sección del contexto: «Pedidos del cliente». Scope: `orders`. Interfaz del cliente: sin inicio de sesión, con `_CustomerLayout`.

**Pantallas.**

- **Catálogo:** categorías y productos disponibles; los no disponibles no se pueden pedir.
- **Personalización:** las opciones permitidas del producto agrupadas (tamaño, leche, endulzante, extras), con su precio adicional, además de cantidad, nota opcional y nombre opcional del cliente.
- **Carrito:** cada producto con su precio y el total, actualizados en pantalla.
- **Confirmación:** muestra el número de orden que devuelve Business.

**Decisiones.**

- El total lo calcula Business; la vista no replica reglas de precio.
- **Propuesta:** el carrito vive en la sesión del navegador del cliente, sin cuentas ni inicio de sesión.
- Botones grandes y fáciles de tocar, pensando en la tablet de autoservicio.
- **Pendiente:** rutas de la interfaz del cliente (hoy el inicio es `/`) y dónde se guarda el carrito.
- **Pendiente:** si el cliente puede quitar productos o cambiar cantidades en el carrito.
- Fuera de alcance: cobro o pagos en línea.

**Verificación.** Pedido completo de punta a punta; producto no disponible bloqueado; total correcto con opciones de precio adicional; número de orden visible; funciona en tablet y en PC.

---

## Fase 4 — Gestión de órdenes

Sección del contexto: «Gestión de órdenes (empleado y admin)». Scope: `orders`. Pantallas para Employee y Admin.

**Pantallas (propuesta de rutas).**

| Ruta | Contenido |
| ---- | --------- |
| `/Staff/Orders` | Lista de órdenes con su estado y actualización periódica de la página. |
| `/Staff/Orders/Details/{id}` | Recibo: número de orden, fecha y hora, productos con sus opciones, total, el cambio de estado y la trazabilidad. |
| `/Staff/Orders/History` | Historial con filtros por fecha y estado. |

**Decisiones.**

- Estados: Recibida, En preparación, Lista y Entregada, un paso a la vez. Se puede cancelar mientras la orden está en Recibida, En preparación o Lista; Entregada y Cancelada son finales. La pantalla solo ofrece las transiciones válidas.
- Cada cambio registra quién lo hizo y cuándo; el Frontend envía el `UserId` tomado del claim `NameIdentifier`.
- Las órdenes no se eliminan: no hay botón de eliminar.
- Sin comunicación en tiempo real, correo ni SMS: bastan la actualización periódica de la página y los avisos dentro de la interfaz.

**Verificación.** Recorrido completo de estados; cancelación; transición inválida rechazada; trazabilidad visible; filtros del historial.

---

## Fase 5 — Proveedores

Sección del contexto: «Proveedores». Scope: `suppliers`.

**Pantallas (propuesta).** `Pages/Staff/Suppliers` con `Index`, `Create`, `Edit` y `Delete`. Campos: nombre, contacto y nota.

**Permisos.** Employee y Admin ven la lista; solo el Admin da de alta, edita y elimina. Los botones se ocultan con `User.IsInRole("Admin")` y la protección real de `Create`, `Edit` y `Delete` va por política en `Program.cs`.

**Eliminación.** Solo si ningún insumo lo tiene asignado; si está en uso, se muestra el mensaje y se sugiere cambiar o quitar el proveedor de esos insumos.

**Verificación.** Empleado de solo consulta; Admin con alta, edición y eliminación; rechazo cuando hay insumos asignados.

---

## Fase 6 — Inventario y alertas

Sección del contexto: «Inventario y alertas». Scope: `inventory`.

**Pantallas (propuesta).**

- **Insumos:** lista con categoría, unidad de medida, existencia y límite mínimo; alta y edición del Admin.
- **Categorías de insumo:** solo Admin.
- **Existencias:** actualización manual (entradas de mercancía y ajustes) por Employee y Admin.
- **Alertas:** aviso visible en la interfaz del personal cuando la existencia llega o baja del límite mínimo, con el insumo y su proveedor.

**Decisiones.**

- **Propuesta:** las alertas se muestran en `_StaffLayout`, junto a la franja, para que se vean en cualquier pantalla del personal.
- El descuento automático del inventario por pedido está fuera de alcance.
- Eliminar un insumo es siempre posible; una categoría de insumo, solo si no tiene insumos.

**Verificación.** Alerta al llegar y al bajar del límite; alerta con proveedor; existencias actualizadas por el empleado; límites y categorías solo para el Admin.

---

## Pruebas de la zona Web

Con el Backend y el DBA de cada fase en `main`, se recorren las pantallas de la fase y se anotan los resultados:

- Navegación con cada rol (Admin, Employee) y sin sesión.
- Validaciones y mensajes de error visibles en español.
- Ningún error técnico llega al usuario.
- Anchos de 390 px, 768 px y escritorio, sin desbordes horizontales.
- Compilación con 0 errores.
