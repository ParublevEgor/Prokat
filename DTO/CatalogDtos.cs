namespace Prokat.API.DTO
{
    public class SkiCatalogDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string SkiType { get; set; } = "";
        public int LengthCm { get; set; }
        public string? Level { get; set; }
        public string? Note { get; set; }
    }

    public class SkiCatalogUpsertDto
    {
        public string Name { get; set; } = "";
        public string SkiType { get; set; } = "";
        public int LengthCm { get; set; }
        public string? Level { get; set; }
        public string? Note { get; set; }
    }

    public class SnowboardCatalogDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string BoardType { get; set; } = "";
        public int LengthCm { get; set; }
        public string? Stiffness { get; set; }
        public string? Note { get; set; }
    }

    public class SnowboardCatalogUpsertDto
    {
        public string Name { get; set; } = "";
        public string BoardType { get; set; } = "";
        public int LengthCm { get; set; }
        public string? Stiffness { get; set; }
        public string? Note { get; set; }
    }

    public class BootsCatalogDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string BootType { get; set; } = "";
        public int SizeEu { get; set; }
        public string? Note { get; set; }
    }

    public class BootsCatalogUpsertDto
    {
        public string Name { get; set; } = "";
        public string BootType { get; set; } = "";
        public int SizeEu { get; set; }
        public string? Note { get; set; }
    }

    public class PolesCatalogDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string PolesType { get; set; } = "";
        public int LengthCm { get; set; }
        public string? Note { get; set; }
    }

    public class PolesCatalogUpsertDto
    {
        public string Name { get; set; } = "";
        public string PolesType { get; set; } = "";
        public int LengthCm { get; set; }
        public string? Note { get; set; }
    }

    public class HelmetCatalogDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Size { get; set; } = "";
        public string? HelmetType { get; set; }
    }

    public class HelmetCatalogUpsertDto
    {
        public string Name { get; set; } = "";
        public string Size { get; set; } = "";
        public string? HelmetType { get; set; }
    }

    public class GogglesCatalogDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Size { get; set; } = "";
        public string? LensType { get; set; }
    }

    public class GogglesCatalogUpsertDto
    {
        public string Name { get; set; } = "";
        public string Size { get; set; } = "";
        public string? LensType { get; set; }
    }
}
