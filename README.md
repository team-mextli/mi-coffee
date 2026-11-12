# Mi Coffee — Sistema de Cafetería

Sistema para administrar la operación de una cafetería: pedidos de los clientes, órdenes, catálogo de productos, inventario, alertas de insumos y proveedores. El cliente arma su pedido desde su propia interfaz, el personal lo atiende y le da seguimiento desde la suya. Todos los datos se guardan en una base de datos relacional.

---

## El problema

Cuando los pedidos, el inventario y los contactos de los proveedores se manejan por separado, no queda registro de lo que se pidió ni de quién lo atendió, un insumo se descubre agotado cuando ya no hay, y los datos de los proveedores quedan dispersos.

El sistema resuelve esto con:

### Pedidos desde la interfaz del cliente

El cliente elige su producto, lo personaliza con las opciones que define el administrador, ve el precio y el total, y al confirmar recibe un número de orden. La orden llega a la interfaz del personal.

### Trazabilidad de las órdenes

Cada orden pasa por estados (Recibida, En preparación, Lista, Entregada o Cancelada) y cada cambio registra quién lo hizo y cuándo. Las órdenes no se borran y el historial se puede consultar por fecha y estado.

### Inventario, alertas y proveedores

Cada insumo tiene una existencia y un límite mínimo definidos por el administrador. Cuando llega o baja de ese límite, la interfaz del personal muestra una alerta con el proveedor al que se le compra. Los proveedores se registran en una lista centralizada con nombre, contacto y nota.

---

## Documentación técnica

| Documento                                                   | Contenido                                                                                       |
| ----------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| [`CONTRIBUTING.md`](CONTRIBUTING.md)                        | **Guía de contribución** — desarrollo local, convención de commits, ramas y pull requests.      |
| [`mi-coffee-context.md`](docs/mi-coffee-context.md)         | **Contexto del proyecto** — alcance funcional, arquitectura por capas, convenciones y glosario. |
| [`mi-coffee-plan-phases.md`](docs/mi-coffee-plan-phases.md) | **Plan de fases** — orden de las fases y tareas de cada integrante en cada una.                 |
| [`repository-config.md`](docs/repository-config.md)         | **Ajustes del repositorio** — configuración de GitHub: ruleset de `main` y ajustes generales.   |

---

## Stack técnico

- **Plataforma:** ASP.NET Core Razor Pages (C#, .NET 10)
- **Entorno de desarrollo:** Visual Studio
- **Backend:** capas de Entidades, Datos y Negocios en C#
- **Frontend:** páginas Razor (HTML, CSS y JavaScript) con Bootstrap, en el proyecto Web
- **Base de datos:** MySQL

---

## Equipo de desarrollo

| Nombre                                                          | Rol / área |
| --------------------------------------------------------------- | ---------- |
| [Mike Armando Montano Valencia](https://github.com/MikeArmando) | Backend    |
| [Mextli Citlali Perez Aguirre](https://github.com/mxcitali)     | Frontend   |
| [Emir Alexander Alcantar Martinez](https://github.com/ALVXNDR)  | DBA        |

---

<sub>Proyecto desarrollado como parte de la asignatura «Ambientes de Programación Visual» de Ingeniería en Software y Tecnologías Emergentes, Facultad de Ingeniería, Arquitectura y Diseño, Universidad Autónoma de Baja California (UABC), Ensenada.</sub>
