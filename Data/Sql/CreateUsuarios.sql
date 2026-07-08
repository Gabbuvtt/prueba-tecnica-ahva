CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NumeroDocumento VARCHAR(20) NOT NULL UNIQUE,
    TipoDocumento VARCHAR(10) NOT NULL,
    Contrasena VARCHAR(255) NOT NULL,
    Nombres VARCHAR(100) NOT NULL,
    PrimerApellido VARCHAR(100) NOT NULL,
    SegundoApellido VARCHAR(100) NULL,
    FechaNacimiento DATE NOT NULL,
    Nacionalidad VARCHAR(50) NOT NULL,
    Sexo VARCHAR(20) NOT NULL,
    CorreoPrincipal VARCHAR(150) NOT NULL,
    CorreoSecundario VARCHAR(150) NULL,
    TelefonoMovil VARCHAR(20) NOT NULL,
    TelefonoSecundario VARCHAR(20) NULL,
    TipoContratacion VARCHAR(50) NOT NULL,
    FechaContratacion DATE NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Activo'
);
