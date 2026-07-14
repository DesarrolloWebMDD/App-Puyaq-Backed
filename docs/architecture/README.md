# Arquitectura

PUYAQ inicia como monolito modular con Clean Architecture.

## Dependencias permitidas

```text
Api -> Application
Api -> Infrastructure
Api -> CrossCutting

Infrastructure -> Application
Infrastructure -> Domain
Infrastructure -> CrossCutting

Application -> Domain
Application -> CrossCutting

Domain -> ninguna capa interna
```

No se permite que Domain conozca Entity Framework, HTTP o proveedores externos.
