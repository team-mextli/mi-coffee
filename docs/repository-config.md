# Ajustes del repositorio

Ajustes de GitHub del repositorio `team-mextli/mi-coffee`: qué hace cada ajuste y por qué se usa.

---

## Ruleset sobre `main`

Configurado en Settings → Rules → Rulesets.

El proyecto no tiene CI por ahora, así que el ruleset no exige ningún status check. La compilación con 0 errores se verifica a mano antes de abrir el Pull Request.

| Ajuste                                | Valor             | Efecto                                                                                         | Por qué                                                                                                                                                                                                                                                                                                              |
| ------------------------------------- | ----------------- | ---------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Require a pull request before merging | Activado          | GitHub rechaza cualquier `git push` directo a `main`; todo cambio entra vía un PR mergeado.    | Es el control base: sin esto, cualquiera puede saltarse el flujo con un push directo.                                                                                                                                                                                                                                |
| Required approvals                    | 1                 | El PR queda bloqueado hasta que otro integrante lo apruebe; el autor no puede aprobar el suyo. | Con tres integrantes siempre hay alguien más que revise antes de mergear.                                                                                                                                                                                                                                            |
| Allowed merge methods                 | Solo Merge commit | Squash y rebase quedan deshabilitados; todo PR entra a `main` con un merge commit.             | `backend`, `frontend` y `dba` viven todo el proyecto. Con squash o rebase, GitHub crea commits nuevos en `main` y la rama queda desfasada: el siguiente PR vuelve a mostrar cambios ya integrados y los conflictos se vuelven comunes. Con merge commit se conserva la historia de la rama y se puede seguir usando. |
| Restrict who can bypass               | Lista vacía       | Nadie, incluido el dueño del repo, puede saltarse las reglas anteriores.                       | Dejar la lista vacía es lo que hace que el ruleset realmente aplique, en vez de quedar configurado pero sin efecto.                                                                                                                                                                                                  |

---

## Configuración general del repo — Settings → General

| Ajuste                             | Valor       | Efecto                                        | Por qué                                                                                |
| ---------------------------------- | ----------- | --------------------------------------------- | -------------------------------------------------------------------------------------- |
| Automatically delete head branches | Desactivado | Al mergear un PR, la rama remota se conserva. | Las ramas de trabajo no se eliminan: cada integrante las reutiliza en todas las fases. |

---

## Resultado neto del conjunto

`main` solo avanza mediante PR → aprobación de otro integrante → merge commit. Las ramas de trabajo permanecen y se reutilizan fase tras fase. Ningún cambio llega a `main` sin que alguien más lo haya revisado, y la compilación se verifica a mano antes de abrir el PR.
