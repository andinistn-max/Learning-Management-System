# ARSITEKTUR TEKNIS & PATTERN KODE

## 1. Folder Structure Pattern

EduPulse/
├── Controllers/          -> MVC Controllers (Razor Views)
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── GuruController.cs
│   └── SiswaController.cs
├── Controllers/Api/      -> Web API 2 Controllers (REST JSON)
├── Models/
│   ├── Entity/           -> Class Entity Framework 6 Mapping Tabel
│   ├── Generator/        -> DTO Khusus Join Query Relasi & Reporting
│   └── ViewModel/        -> DTO Form Request, Response, & ApiResponse<T>
├── Services/
│   ├── Base/             -> BaseService.cs (Context Holder)
│   ├── Context/          -> LmsDbContext.cs (DbSet Container)
│   ├── Interface/        -> IAuthWebService, IKelasWebService, dll
│   └── Impl/             -> Logika Query LINQ to Entities
├── Helpers/              -> JwtHelper, ExcelExportHelper, FileUploadHelper, HtmlHelpers
├── Views/                -> Razor Views (.cshtml) + Shared/_Layout.cshtml (Tabler UI)
├── Uploads/              -> Penyimpanan file lokal
└── Docs/                 -> 7 Dokumen Konteks untuk AI