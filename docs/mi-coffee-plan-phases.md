# Mi Coffee — Plan de fases general

El detalle de cada funcionalidad está en `mi-coffee-context.md`, en la sección indicada en cada fase. Este documento solo fija el orden de las fases y qué le toca a cada integrante en cada una. Cada integrante arma su plan individual dentro de su zona a partir de él.

Una fase termina cuando las tres ramas se integran en `main` y la funcionalidad completa funciona.

## Fase 0 — Base y modelo de datos

Sección del contexto: Alcance funcional (completo)

- **Mike (Backend):** plantilla en `main` y ramas de trabajo creadas.
- **Mextli (Frontend):** layout base de la interfaz del cliente y de la interfaz del personal.
- **Emir (DBA y Pruebas):** modelo de datos completo y `schema.sql` con todas las tablas.

## Fase 1 — Acceso del personal

Sección del contexto: Usuarios del sistema

- **Mike (Backend):** inicio de sesión, contraseñas con hash, acceso por rol y gestión de usuarios del personal.
- **Mextli (Frontend):** pantalla de inicio de sesión, menú del personal según el rol y pantallas de usuarios del personal.
- **Emir (DBA y Pruebas):** datos de prueba (administrador inicial) y pruebas.

## Fase 2 — Catálogo

Sección del contexto: Catálogo (admin)

- **Mike (Backend):** categorías de producto, productos y opciones.
- **Mextli (Frontend):** pantallas de administración del catálogo.
- **Emir (DBA y Pruebas):** datos de prueba del menú (productos y opciones) y pruebas.

## Fase 3 — Pedidos del cliente

Sección del contexto: Pedidos del cliente

- **Mike (Backend):** creación de la orden, con su total y su número de orden.
- **Mextli (Frontend):** interfaz completa del cliente (catálogo, personalización, carrito y confirmación).
- **Emir (DBA y Pruebas):** datos de prueba y pruebas de órdenes.

## Fase 4 — Gestión de órdenes

Sección del contexto: Gestión de órdenes (empleado y admin)

- **Mike (Backend):** lista de órdenes, cambio de estado con trazabilidad e historial con filtros.
- **Mextli (Frontend):** pantallas del personal para la lista de órdenes, el recibo, el cambio de estado y el historial.
- **Emir (DBA y Pruebas):** datos de prueba y pruebas de estados y trazabilidad.

## Fase 5 — Proveedores

Sección del contexto: Proveedores

- **Mike (Backend):** alta, edición y consulta de proveedores.
- **Mextli (Frontend):** pantallas de proveedores.
- **Emir (DBA y Pruebas):** datos de prueba y pruebas de proveedores.

## Fase 6 — Inventario y alertas

Sección del contexto: Inventario y alertas

- **Mike (Backend):** insumos, categorías de insumo, límites, actualización de existencias y alertas.
- **Mextli (Frontend):** pantallas de insumos y existencias, y alertas visibles en la interfaz del personal.
- **Emir (DBA y Pruebas):** datos de prueba y pruebas de límites y alertas.
