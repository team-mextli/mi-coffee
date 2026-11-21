# Guía de contribución

Guía para el equipo: cómo levantar el proyecto localmente y qué convenciones seguir para commits, ramas y pull requests.

---

## Desarrollo local

### Requisitos previos

- Las herramientas del stack del proyecto, en la versión que fije el repositorio
- Git

### Instalación

```bash
git clone <url-del-repositorio>
cd <nombre-del-repositorio>
```

Lo que el proyecto necesite fuera del repositorio (base de datos local, credenciales, variables de entorno) se configura en cada máquina y nunca se sube al repositorio.

### Ejecutar en desarrollo

Se ejecuta el proyecto con la opción de ejecución propia del stack o del entorno de desarrollo.

---

## Idioma

Inglés. Es el estándar de facto para commits, código y comentarios, independientemente del idioma de trabajo del equipo.

---

## Convención de commits, ramas y pull requests

### Issues

El repositorio vive dentro de una organización de GitHub; tanto el acceso al repositorio como al Project del equipo dependen de la membresía en esa organización.

#### Cuándo crear un issue

- **Obligatorio** para `feat` y `fix`: son los tipos que cambian el comportamiento, y son justo lo que un Project necesita rastrear. El issue se vincula al PR que lo resuelve (ver [Pull Requests](#pull-requests)) y se cierra solo al fusionar, sin actualizar el tablero a mano.
- **Opcional** para el resto de los tipos (`chore`, `docs`, `refactor`, `test`, `style`, `perf`, `ci`): son cambios de mantenimiento sin impacto en el comportamiento — exigir un issue para, por ejemplo, corregir una errata en el README es más proceso del que el cambio amerita.

Si un issue abarca más de lo que cabe en un PR razonable, se divide en sub-issues más chicos antes de empezar: permite avanzar en incrementos manejables y termina en pull requests más chicos y más fáciles de revisar.

#### Crear el issue

El título sigue el mismo formato que un commit (ver [Formato del mensaje](#formato-del-mensaje)): issue, PR y commit comparten un único formato de título.

```bash
gh issue create --title "feat(scope): short description of the change" --body "Explain the why and any context worth preserving."
```

Si el equipo usa un GitHub Project para dar seguimiento visual al trabajo, `--project <nombre>` agrega el issue ahí directamente al crearlo (requiere autorizar el scope `project` una sola vez: `gh auth refresh -s project`).

### Commits

#### Formato del mensaje

```
<type>(<optional scope>): <short description in lowercase, no trailing period>

[optional body — explains the "why", the context]

[optional footer — external links, issue references, and other meta-information]
```

Con merge commit, todos los commits de la rama se conservan en el historial de `main`: el cuerpo y el footer quedan visibles en `git log` y en la pestaña "Commits" del PR.

#### Tipos

| Tipo       | Cuándo usarlo                                                                   |
| ---------- | ------------------------------------------------------------------------------- |
| `feat`     | Nueva funcionalidad visible para la persona usuaria o la API                    |
| `fix`      | Corrección de un bug                                                            |
| `chore`    | Tareas de mantenimiento sin efecto en el comportamiento (deps, config, scripts) |
| `docs`     | Cambios solo de documentación (README, comentarios)                             |
| `refactor` | Cambio de código que no arregla un bug ni agrega funcionalidad                  |
| `test`     | Agregar o corregir pruebas                                                      |
| `style`    | Formato, espacios, punto y coma — sin cambio de lógica                          |
| `perf`     | Cambio que mejora rendimiento                                                   |
| `ci`       | Cambios en workflows / configuración de integración continua                    |

#### Scope

Opcional pero recomendado. Corresponde al módulo funcional que toca el cambio o a un área transversal. Un cambio que abarca varias capas (Entities, Data, Business, Web) usa el scope del módulo, no el de cada capa.

| Scope       | Cubre                                                                                               |
| ----------- | --------------------------------------------------------------------------------------------------- |
| `auth`      | Inicio de sesión, contraseñas con hash, acceso por rol y usuarios del personal                      |
| `catalog`   | Categorías de producto, productos y opciones de personalización                                     |
| `orders`    | Pedido del cliente (carrito y confirmación) y gestión de órdenes (estados, trazabilidad, historial) |
| `suppliers` | Alta, edición, eliminación y consulta de proveedores                                                |
| `inventory` | Insumos, categorías, límites, existencias y alertas                                                 |
| `db`        | Scripts de `database/`: esquema, datos de prueba y pruebas SQL                                      |
| `ui`        | Layout, navegación y estilos compartidos de las dos interfaces                                      |
| `config`    | `Program.cs`, conexión, archivos `.csproj`, `.gitignore` y ajustes del repositorio                  |
| `deps`      | Paquetes NuGet y bibliotecas del lado cliente                                                       |

Cuando varias funcionalidades pertenecen a un mismo grupo de la tabla (por ejemplo, categorías, productos y opciones dentro de `catalog`), se usa el nombre del grupo como scope en lugar del nombre de la entidad puntual.

Si el commit implementa un issue puntual (ver [Issues](#issues)), referenciar su número en la descripción del commit ayuda a mantener trazabilidad.

#### Breaking changes

Si un commit rompe compatibilidad (ej. cambia la forma de una respuesta ya en uso), se agrega `!` después del tipo/scope:

```
feat(api)!: rename `id` field to `userId` in response payload
```

El impacto debe describirse en la descripción del PR (ver [Pull Requests](#pull-requests)). Opcionalmente, si ayuda a documentar el porqué mientras la rama sigue en desarrollo, el cuerpo del commit puede incluir una nota breve al respecto.

#### Ejemplos

```
feat(auth): add password reset flow
fix(export): correct csv escaping for fields containing commas
chore: initial project scaffolding
chore(deps): bump vite to 6.0
docs: add installation instructions to readme
refactor(storage): extract local persistence logic into shared module
test(validation): cover required-field edge cases
```

#### Reglas rápidas

- Descripción corta en modo imperativo presente ("add", "fix", "remove"), no en pasado ni en tercera persona. Regla mental: el mensaje completa la frase "If applied, this commit will \_\_\_".
- Sin punto final en la línea de descripción.
- Un commit = un cambio lógico. Si la descripción necesita un "and", probablemente son dos commits.

### Ramas

El repositorio usa `main` (versión estable e integrada) y ramas de trabajo permanentes, una por rol o área del equipo; sus nombres están en el `mi-coffee-context`. Las ramas de trabajo no se eliminan: se reutilizan en cada fase, por lo que no se crean ramas por issue ni por cambio.

#### Trabajar en la rama propia

```bash
git checkout <rama-de-trabajo>
git fetch origin
git merge origin/main
```

- Nadie hace commits directamente en `main`.
- La rama se actualiza con `main` antes de empezar a trabajar y cada vez que se fusione un PR, para partir siempre de la versión integrada.

### Pull Requests

#### Título y descripción

- El **título** sigue el mismo formato que un commit (ver [Formato del mensaje](#formato-del-mensaje)).
- La **descripción** documenta el cambio: el contexto que se quiere preservar, el impacto y, si lo hay, el detalle de un breaking change.
- Ambos quedan registrados en el PR dentro de GitHub: vale la pena escribirlos con cuidado en vez de dejarlos en automático.

#### Crear el PR

El PR va de la rama de trabajo a `main`.

Si el PR resuelve uno o más issues, la descripción incluye la palabra clave `Closes` por cada issue (`Closes #<number>`). Esa palabra clave solo se interpreta cuando el PR apunta a la rama default del repositorio — al fusionarlo contra `main`, GitHub cierra los issues automáticamente y actualiza su estado en el Project sin acción manual.

```bash
gh pr create --title "<type>(<scope>): <description>" --body "<context to preserve>. Closes #<number>"
```

Si el repositorio tiene CI configurado con el trigger `on: pull_request`, se dispara automáticamente al crear el PR. Si no lo tiene, antes de abrir el PR se verifica a mano que el proyecto compila con 0 errores y funciona.

#### Revisión y fusión (merge)

El ruleset de `main` exige la aprobación de al menos otro integrante antes de fusionar; el autor no puede aprobar su propio PR (ver [`repository-config.md`](docs/repository-config.md)).

```bash
gh pr merge --merge
```

- `--merge` crea un merge commit y conserva los commits de la rama en el historial de `main`. Es el único método de fusión del repositorio: con ramas de trabajo permanentes, squash y rebase las desfasan de `main`.
- No se usa `--delete-branch`: las ramas de trabajo se conservan después de fusionar.
- Sin la aprobación requerida, el PR no se puede fusionar: se pide la revisión y se vuelve a intentar.
- Si `main` avanzó mientras el PR seguía abierto, conviene actualizar la rama antes de fusionar (y es obligatorio si aparecen conflictos):

```bash
git fetch origin
git merge origin/main
git push
```

o usar el botón **"Update branch"** en GitHub.

### Flujo de trabajo completo

Secuencia de punta a punta para cualquier cambio, de uno trivial a un módulo entero. Asume Git y GitHub CLI (`gh`) instalados; si el equipo no usa `gh`, los mismos pasos aplican creando y fusionando el PR desde la interfaz web de GitHub. También asume que la configuración de `main` descrita en [`repository-config.md`](docs/repository-config.md) ya existe — de lo contrario, algunos pasos no aplican tal cual.

1. **Crear el issue** (obligatorio para `feat`/`fix`; opcional para el resto — ver [Issues](#issues))
   ```bash
   gh issue create --title "feat(scope): short description of the change" --body "Explain the why and any context worth preserving."
   ```
2. **Actualizar la rama de trabajo con `main`**
   ```bash
   git checkout <rama-de-trabajo>
   git fetch origin
   git merge origin/main
   ```
3. **Hacer commits siguiendo la convención**
   ```bash
   git add .
   git commit -m "feat(scope): short description of the change"
   ```
4. **Subir la rama**
   ```bash
   git push -u origin <rama-de-trabajo>
   ```
   Solo se necesita `-u` la primera vez; después basta `git push`.
5. **Verificar que el proyecto compila y funciona** (si no hay CI, se hace a mano)
6. **Abrir el PR**
   ```bash
   gh pr create --title "feat(scope): short description of the change" --body "Explain the why and any context worth preserving. Closes #<number>"
   ```
7. **Conseguir la aprobación de otro integrante**
8. **Fusionar**
   ```bash
   gh pr merge --merge
   ```
9. **Actualizar la rama de trabajo con `main`** (mismo comando del paso 2)

   Trae a la rama el merge commit recién creado en `main`. Las ramas de trabajo no se borran.
