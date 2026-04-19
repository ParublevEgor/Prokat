using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;
using Prokat.API.Models;

namespace Prokat.API.Controllers
{
    [Route("api/admin/catalog")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCatalogController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminCatalogController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("skis")]
        public async Task<IActionResult> ListSkis(CancellationToken ct)
        {
            var rows = await _db.Skis.AsNoTracking()
                .OrderBy(x => x.Название)
                .Select(x => new SkiCatalogDto
                {
                    Id = x.ID_Лыжи,
                    Name = x.Название,
                    SkiType = x.Тип,
                    LengthCm = x.РостовкаСм,
                    Level = x.Уровень,
                    Note = x.Примечание
                })
                .ToListAsync(ct);
            return Ok(rows);
        }

        [HttpPost("skis")]
        public async Task<IActionResult> CreateSki([FromBody] SkiCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = new SkiItem
            {
                Название = dto.Name.Trim(),
                Тип = dto.SkiType.Trim(),
                РостовкаСм = dto.LengthCm,
                Уровень = dto.Level?.Trim(),
                Примечание = dto.Note?.Trim()
            };
            _db.Skis.Add(e);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = e.ID_Лыжи });
        }

        [HttpPut("skis/{id:int}")]
        public async Task<IActionResult> UpdateSki(int id, [FromBody] SkiCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = await _db.Skis.FirstOrDefaultAsync(x => x.ID_Лыжи == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            e.Название = dto.Name.Trim();
            e.Тип = dto.SkiType.Trim();
            e.РостовкаСм = dto.LengthCm;
            e.Уровень = dto.Level?.Trim();
            e.Примечание = dto.Note?.Trim();
            await _db.SaveChangesAsync(ct);
            return Ok(new { updated = true });
        }

        [HttpDelete("skis/{id:int}")]
        public async Task<IActionResult> DeleteSki(int id, CancellationToken ct)
        {
            var e = await _db.Skis.FirstOrDefaultAsync(x => x.ID_Лыжи == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            _db.Skis.Remove(e);
            return await SaveWithReferenceGuard(ct);
        }

        [HttpGet("snowboards")]
        public async Task<IActionResult> ListSnowboards(CancellationToken ct)
        {
            var rows = await _db.Snowboards.AsNoTracking()
                .OrderBy(x => x.Название)
                .Select(x => new SnowboardCatalogDto
                {
                    Id = x.ID_Сноуборд,
                    Name = x.Название,
                    BoardType = x.Тип,
                    LengthCm = x.РостовкаСм,
                    Stiffness = x.Жесткость,
                    Note = x.Примечание
                })
                .ToListAsync(ct);
            return Ok(rows);
        }

        [HttpPost("snowboards")]
        public async Task<IActionResult> CreateSnowboard([FromBody] SnowboardCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = new SnowboardItem
            {
                Название = dto.Name.Trim(),
                Тип = dto.BoardType.Trim(),
                РостовкаСм = dto.LengthCm,
                Жесткость = dto.Stiffness?.Trim(),
                Примечание = dto.Note?.Trim()
            };
            _db.Snowboards.Add(e);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = e.ID_Сноуборд });
        }

        [HttpPut("snowboards/{id:int}")]
        public async Task<IActionResult> UpdateSnowboard(int id, [FromBody] SnowboardCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = await _db.Snowboards.FirstOrDefaultAsync(x => x.ID_Сноуборд == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            e.Название = dto.Name.Trim();
            e.Тип = dto.BoardType.Trim();
            e.РостовкаСм = dto.LengthCm;
            e.Жесткость = dto.Stiffness?.Trim();
            e.Примечание = dto.Note?.Trim();
            await _db.SaveChangesAsync(ct);
            return Ok(new { updated = true });
        }

        [HttpDelete("snowboards/{id:int}")]
        public async Task<IActionResult> DeleteSnowboard(int id, CancellationToken ct)
        {
            var e = await _db.Snowboards.FirstOrDefaultAsync(x => x.ID_Сноуборд == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            _db.Snowboards.Remove(e);
            return await SaveWithReferenceGuard(ct);
        }

        [HttpGet("boots")]
        public async Task<IActionResult> ListBoots(CancellationToken ct)
        {
            var rows = await _db.Boots.AsNoTracking()
                .OrderBy(x => x.РазмерEU)
                .Select(x => new BootsCatalogDto
                {
                    Id = x.ID_Ботинки,
                    Name = x.Название,
                    BootType = x.Тип,
                    SizeEu = x.РазмерEU,
                    Note = x.Примечание
                })
                .ToListAsync(ct);
            return Ok(rows);
        }

        [HttpPost("boots")]
        public async Task<IActionResult> CreateBoots([FromBody] BootsCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = new BootsItem
            {
                Название = dto.Name.Trim(),
                Тип = dto.BootType.Trim(),
                РазмерEU = dto.SizeEu,
                Примечание = dto.Note?.Trim()
            };
            _db.Boots.Add(e);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = e.ID_Ботинки });
        }

        [HttpPut("boots/{id:int}")]
        public async Task<IActionResult> UpdateBoots(int id, [FromBody] BootsCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = await _db.Boots.FirstOrDefaultAsync(x => x.ID_Ботинки == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            e.Название = dto.Name.Trim();
            e.Тип = dto.BootType.Trim();
            e.РазмерEU = dto.SizeEu;
            e.Примечание = dto.Note?.Trim();
            await _db.SaveChangesAsync(ct);
            return Ok(new { updated = true });
        }

        [HttpDelete("boots/{id:int}")]
        public async Task<IActionResult> DeleteBoots(int id, CancellationToken ct)
        {
            var e = await _db.Boots.FirstOrDefaultAsync(x => x.ID_Ботинки == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            _db.Boots.Remove(e);
            return await SaveWithReferenceGuard(ct);
        }

        [HttpGet("poles")]
        public async Task<IActionResult> ListPoles(CancellationToken ct)
        {
            var rows = await _db.Poles.AsNoTracking()
                .OrderBy(x => x.ДлинаСм)
                .Select(x => new PolesCatalogDto
                {
                    Id = x.ID_Палки,
                    Name = x.Название,
                    PolesType = x.Тип,
                    LengthCm = x.ДлинаСм,
                    Note = x.Примечание
                })
                .ToListAsync(ct);
            return Ok(rows);
        }

        [HttpPost("poles")]
        public async Task<IActionResult> CreatePoles([FromBody] PolesCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = new PolesItem
            {
                Название = dto.Name.Trim(),
                Тип = dto.PolesType.Trim(),
                ДлинаСм = dto.LengthCm,
                Примечание = dto.Note?.Trim()
            };
            _db.Poles.Add(e);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = e.ID_Палки });
        }

        [HttpPut("poles/{id:int}")]
        public async Task<IActionResult> UpdatePoles(int id, [FromBody] PolesCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = await _db.Poles.FirstOrDefaultAsync(x => x.ID_Палки == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            e.Название = dto.Name.Trim();
            e.Тип = dto.PolesType.Trim();
            e.ДлинаСм = dto.LengthCm;
            e.Примечание = dto.Note?.Trim();
            await _db.SaveChangesAsync(ct);
            return Ok(new { updated = true });
        }

        [HttpDelete("poles/{id:int}")]
        public async Task<IActionResult> DeletePoles(int id, CancellationToken ct)
        {
            var e = await _db.Poles.FirstOrDefaultAsync(x => x.ID_Палки == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            _db.Poles.Remove(e);
            return await SaveWithReferenceGuard(ct);
        }

        [HttpGet("helmets")]
        public async Task<IActionResult> ListHelmets(CancellationToken ct)
        {
            var rows = await _db.Helmets.AsNoTracking()
                .OrderBy(x => x.Размер)
                .ThenBy(x => x.Название)
                .Select(x => new HelmetCatalogDto
                {
                    Id = x.ID_Шлем,
                    Name = x.Название,
                    Size = x.Размер,
                    HelmetType = x.Тип
                })
                .ToListAsync(ct);
            return Ok(rows);
        }

        [HttpPost("helmets")]
        public async Task<IActionResult> CreateHelmet([FromBody] HelmetCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = new HelmetItem
            {
                Название = dto.Name.Trim(),
                Размер = dto.Size.Trim(),
                Тип = dto.HelmetType?.Trim()
            };
            _db.Helmets.Add(e);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = e.ID_Шлем });
        }

        [HttpPut("helmets/{id:int}")]
        public async Task<IActionResult> UpdateHelmet(int id, [FromBody] HelmetCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = await _db.Helmets.FirstOrDefaultAsync(x => x.ID_Шлем == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            e.Название = dto.Name.Trim();
            e.Размер = dto.Size.Trim();
            e.Тип = dto.HelmetType?.Trim();
            await _db.SaveChangesAsync(ct);
            return Ok(new { updated = true });
        }

        [HttpDelete("helmets/{id:int}")]
        public async Task<IActionResult> DeleteHelmet(int id, CancellationToken ct)
        {
            var e = await _db.Helmets.FirstOrDefaultAsync(x => x.ID_Шлем == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            _db.Helmets.Remove(e);
            return await SaveWithReferenceGuard(ct);
        }

        [HttpGet("goggles")]
        public async Task<IActionResult> ListGoggles(CancellationToken ct)
        {
            var rows = await _db.Goggles.AsNoTracking()
                .OrderBy(x => x.Размер)
                .ThenBy(x => x.Название)
                .Select(x => new GogglesCatalogDto
                {
                    Id = x.ID_Очки,
                    Name = x.Название,
                    Size = x.Размер,
                    LensType = x.ТипЛинзы
                })
                .ToListAsync(ct);
            return Ok(rows);
        }

        [HttpPost("goggles")]
        public async Task<IActionResult> CreateGoggles([FromBody] GogglesCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = new GogglesItem
            {
                Название = dto.Name.Trim(),
                Размер = dto.Size.Trim(),
                ТипЛинзы = dto.LensType?.Trim()
            };
            _db.Goggles.Add(e);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = e.ID_Очки });
        }

        [HttpPut("goggles/{id:int}")]
        public async Task<IActionResult> UpdateGoggles(int id, [FromBody] GogglesCatalogUpsertDto dto, CancellationToken ct)
        {
            var e = await _db.Goggles.FirstOrDefaultAsync(x => x.ID_Очки == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            e.Название = dto.Name.Trim();
            e.Размер = dto.Size.Trim();
            e.ТипЛинзы = dto.LensType?.Trim();
            await _db.SaveChangesAsync(ct);
            return Ok(new { updated = true });
        }

        [HttpDelete("goggles/{id:int}")]
        public async Task<IActionResult> DeleteGoggles(int id, CancellationToken ct)
        {
            var e = await _db.Goggles.FirstOrDefaultAsync(x => x.ID_Очки == id, ct);
            if (e is null) return NotFound(new { message = "Запись не найдена." });
            _db.Goggles.Remove(e);
            return await SaveWithReferenceGuard(ct);
        }

        private async Task<IActionResult> SaveWithReferenceGuard(CancellationToken ct)
        {
            try
            {
                await _db.SaveChangesAsync(ct);
                return Ok(new { deleted = true });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Нельзя удалить запись: она используется в комплектах инвентаря." });
            }
        }
    }
}
