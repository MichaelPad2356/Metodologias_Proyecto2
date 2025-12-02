-- Script para agregar campos de artefactos de transición (HU-009)
-- Ejecutar en SQLite

-- Agregar campos para Build Final
ALTER TABLE Artifacts ADD COLUMN BuildIdentifier TEXT NULL;
ALTER TABLE Artifacts ADD COLUMN BuildDownloadUrl TEXT NULL;

-- Agregar campo para Checklist de Cierre
ALTER TABLE Artifacts ADD COLUMN ClosureChecklistJson TEXT NULL;
