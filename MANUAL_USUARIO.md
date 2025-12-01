# Manual de Usuario - Herramienta de Gestión OpenUP

## 1. Introducción
Bienvenido a la **Herramienta de Gestión OpenUP**. Este sistema ha sido diseñado para facilitar la administración, seguimiento y control de proyectos de software bajo la metodología OpenUP. A través de esta interfaz, podrá gestionar el ciclo de vida completo del proyecto, desde la fase de Incepción hasta la Transición, controlando iteraciones, tareas y artefactos documentales.

## 2. Acceso al Sistema
Para ingresar a la aplicación, asegúrese de que los servicios estén ejecutándose (ver Manual de Instalación) y abra su navegador web en la siguiente dirección:

> **URL:** http://localhost:4200

Al ingresar, verá la pantalla principal o **Dashboard**, donde se listan los proyectos existentes.

---

## 3. Gestión de Proyectos

### 3.1. Crear un Nuevo Proyecto
1.  En la pantalla principal, haga clic en el botón **"Nuevo Proyecto"**.
2.  Complete el formulario con la siguiente información:
    *   **Nombre del Proyecto:** Un nombre descriptivo.
    *   **Código:** Un identificador único (ej. `PROJ-001`).
    *   **Fecha de Inicio:** Fecha en la que arranca el proyecto.
    *   **Descripción:** Breve resumen del objetivo.
    *   **Responsable:** Nombre del líder del proyecto.
3.  Haga clic en **"Guardar"**.
    *   *Nota:* El sistema generará automáticamente las 4 fases de OpenUP (Incepción, Elaboración, Construcción, Transición) para este proyecto.

### 3.2. Ver y Editar Proyectos
*   En la lista de proyectos, haga clic sobre el nombre de un proyecto para entrar a su **Panel de Detalle**.
*   Aquí verá el progreso general y el estado de cada fase.

---

## 4. Gestión de Fases e Iteraciones

El corazón de OpenUP es el desarrollo iterativo. El sistema organiza el trabajo dentro de las 4 fases estándar.

### 4.1. Planificación de Iteraciones
1.  Dentro del detalle del proyecto, vaya a la pestaña o sección **"Planificación"**.
2.  Seleccione la Fase correspondiente (ej. *Elaboración*).
3.  Haga clic en **"Nueva Iteración"**.
4.  Defina:
    *   **Nombre:** (ej. *Iteración 1*).
    *   **Fechas:** Inicio y Fin.
    *   **Objetivo:** Meta principal de esta iteración.
5.  Guarde la iteración.

### 4.2. Gestión de Tareas
Dentro de una iteración creada:
1.  Haga clic en **"Agregar Tarea"**.
2.  Describa la tarea y asígnela a un miembro del equipo.
3.  A medida que se avance, puede cambiar el estado de la tarea (Pendiente, En Progreso, Completada).
    *   *El progreso de la iteración se calculará automáticamente basado en las tareas completadas.*

---

## 5. Gestión de Artefactos (Documentación)

OpenUP requiere la entrega de documentos específicos (Artefactos) en cada fase.

### 5.1. Subir un Artefacto
1.  Navegue a la sección **"Artefactos"** dentro de una fase.
2.  Verá la lista de artefactos requeridos (ej. *Documento de Visión*, *Plan de Proyecto*).
3.  Haga clic en el botón de **"Subir/Cargar"** junto al artefacto deseado.
4.  Seleccione el archivo desde su computadora y agregue un comentario de versión.
5.  El sistema guardará el archivo y creará una nueva versión (v1, v2, etc.).

### 5.2. Control de Versiones
*   Puede ver el historial de cambios de un documento haciendo clic en **"Ver Historial"**.
*   Esto le permitirá descargar versiones anteriores si es necesario.

---

## 6. Microincrementos
Para un control más granular del avance técnico:

1.  Acceda al módulo de **"Microincrementos"**.
2.  Registre pequeños avances diarios que no necesariamente son una tarea completa o un documento final.
3.  Vincule el microincremento a la iteración actual para que sume al progreso del equipo.

---

## 7. Preguntas Frecuentes (FAQ)

**¿Puedo eliminar un proyecto?**
Sí, pero tenga cuidado. La opción de eliminar borrará permanentemente toda la información, iteraciones y documentos asociados. Se recomienda usar la opción "Archivar" si solo desea ocultarlo de la lista principal.

**¿Cómo sé si he completado una fase?**
El sistema le indicará el porcentaje de avance basado en los artefactos obligatorios subidos y las tareas de las iteraciones completadas. Idealmente, no debe pasar a la siguiente fase hasta completar los hitos de la actual.
