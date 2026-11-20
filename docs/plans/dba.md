# Mi Coffee — Plan de fases del DBA y Pruebas

Plan individual del DBA y Pruebas (Emir) dentro de su zona: la carpeta `database/` y las pruebas. Parte de `mi-coffee-plan-phases.md` (orden de las fases y tareas por integrante) y de `mi-coffee-context.md` (alcance, arquitectura y convenciones). Este documento no repite el alcance funcional: cada fase indica la sección del contexto donde está definido.

Las decisiones marcadas como **pendiente** se confirman con el equipo antes de implementarse. Las marcadas como **propuesta** son la opción más simple considerada y pueden cambiarse sin afectar el resto del plan.

---

## Cómo se avanza en cada fase

1. **Datos de prueba.** Se agregan a `02_datos` los registros de las tablas de la fase. El script se ejecuta después del esquema, sin modificaciones manuales.
2. **Verificación en MySQL.** El esquema y los datos se ejecutan desde cero en el MySQL local sin errores.
3. **Pruebas en la aplicación.** Cuando el Backend y el Frontend de la fase están en `main`, se ejecutan los casos de prueba de la fase y se registran en la matriz con su resultado observado.
4. **PR de la fase.** Título `feat(db): ...` o `test(db): ...`. Mientras el PR está abierto no se hace push a `dba`; el trabajo siguiente se guarda en commits locales y se sube después de fusionar.

### Decisiones transversales

- **Datos ficticios.** Ningún dato de prueba corresponde a personas reales.
- **Identificadores explícitos.** Los datos de prueba insertan sus claves primarias para que las relaciones entre tablas y las pruebas no dependan del orden de inserción.
- **Restricciones en la base.** Las reglas que el esquema puede hacer cumplir (`NOT NULL`, `UNIQUE`, `CHECK`, claves foráneas) se mantienen ahí, además de la validación que haga Business.
- **Nombres de los scripts.** **Pendiente:** la entrega de Base de datos pide `01_esquema`, `02_datos`, `03_consultas_vistas` y `04_rutinas_pruebas` o equivalentes. **Propuesta:** `01_schema.sql`, `02_data.sql`, `03_queries_views.sql` y `04_routines_tests.sql` en `database/`, renombrando el `schema.sql` actual.

---

## Fase 0 — Base y modelo de datos

Sección del contexto: «Alcance funcional». Terminada: modelo de datos completo y `database/schema.sql` con las 12 tablas en `main` (PR #1).

---

## Fase 1 — Acceso del personal

Sección del contexto: «Usuarios del sistema». Tabla: `user`.

**Datos.** Administrador inicial, dos empleados activos y un empleado inactivo. El `password_hash` del administrador lo genera el Backend con `PasswordHasher<User>`; la contraseña se comparte por fuera del repositorio.

**Pruebas.**

- El administrador y un empleado inician sesión; cada uno ve el menú de su rol.
- Una contraseña incorrecta y un usuario inactivo no pueden iniciar sesión.
- No se puede crear un usuario con un `username` repetido.
- No se puede desactivar ni quitar el rol al último administrador activo.
- En la tabla `user` ninguna contraseña aparece en texto plano.

---

## Fase 2 — Catálogo

Sección del contexto: «Catálogo (admin)». Tablas: `product_category`, `product`, `product_option`, `product_allowed_option`.

**Datos.** Categorías del menú, productos con al menos uno no disponible, opciones agrupadas (Tamaño, Leche, Endulzante, Extra) con y sin precio adicional, y opciones permitidas por producto (por ejemplo, un postre sin opciones de leche).

**Pruebas.**

- Alta y edición de categorías, productos y opciones.
- No se aceptan precios negativos ni nombres de categoría repetidos.
- Un producto no disponible no aparece para el cliente.
- Asignar y quitar opciones permitidas a un producto.

---

## Fase 3 — Pedidos del cliente

Sección del contexto: «Pedidos del cliente». Tablas: `customer_order`, `order_item`, `order_item_option`.

**Datos.** Órdenes de varios días con uno o varios productos, con y sin opciones, con y sin nombre de cliente y nota. Los precios copiados a la orden y el total son coherentes con la fórmula acordada por el Backend.

**Pruebas.**

- El total mostrado y el guardado coinciden con la suma de productos, opciones y cantidades.
- No se puede confirmar un carrito vacío, una cantidad menor a 1, un producto no disponible ni una opción no permitida para el producto.
- La confirmación muestra el número de orden y la orden nace en `Received`.
- Cambiar el precio de un producto no modifica las órdenes ya registradas.

---

## Fase 4 — Gestión de órdenes

Sección del contexto: «Gestión de órdenes (empleado y admin)». Tablas: `customer_order`, `order_status_history`.

**Datos.** Historial de cambios de estado coherente con el estado actual de cada orden, hecho por distintos usuarios del personal.

**Pruebas.**

- Cada cambio de estado registra usuario, estado anterior, estado nuevo y fecha.
- Las transiciones no permitidas se rechazan.
- El historial filtra por rango de fechas y por estado.
- Las órdenes no se pueden eliminar.

---

## Fase 5 — Proveedores

Sección del contexto: «Proveedores». Tabla: `supplier`.

**Datos.** Proveedores con y sin nota, al menos uno sin insumos asignados.

**Pruebas.**

- Alta, edición y consulta de proveedores; nombre y contacto obligatorios.
- El empleado consulta pero no da de alta ni edita.

---

## Fase 6 — Inventario y alertas

Sección del contexto: «Inventario y alertas». Tablas: `supply_category`, `supply`, con `supplier` para la alerta.

**Datos.** Categorías de insumo e insumos con distintas unidades; algunos con existencia igual o menor al límite mínimo y al menos uno sin proveedor asignado.

**Pruebas.**

- La alerta aparece cuando la existencia es igual o menor al límite y muestra el proveedor; sin proveedor, la alerta aparece igual.
- La alerta desaparece al registrar una entrada de mercancía.
- No se aceptan existencias ni límites negativos.

---

## Entregables de Base de datos

Además de las fases, la zona del DBA cubre estos puntos del proyecto final de Base de datos (reparto del equipo). Se avanzan en paralelo con las fases.

| Punto | Entregable | Cuándo |
| --- | --- | --- |
| 15 | `01_esquema` y `02_datos` | Fase 0 y cada fase |
| 16 y 17 | CRUD con validaciones y 10 consultas SQL en `03_consultas_vistas` | Con los datos de prueba completos |
| 9 y 10 | Diccionario de datos e integridad | Con el esquema cerrado |
| 1, 2, 3, 5, 6 y 8 | Análisis, requerimientos y diseño conceptual en el reporte | En paralelo |
| 7 | DER en cinco notaciones, editables y exportados | Con el modelo cerrado |
| 26 a 29 | Persistencia, matriz de pruebas e incidencias | Con la aplicación terminada |

**Pendiente.** Los triggers del Backend (punto 19) no registran el historial de estados, porque la aplicación ya lo guarda en la Fase 4 y el punto 21 pide no duplicar la operación. Las operaciones `DELETE` que piden los puntos 16 y 23 se acuerdan con el Backend sobre registros que no rompen la integridad.
