using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;

namespace Prokat.API.Services
{
    public interface IClientReportService
    {
        Task<IReadOnlyList<ClientReportRowDto>> GetReportAsync(DateTime? датаНачала, DateTime? датаОкончания, CancellationToken ct = default);
    }

    public class ClientReportService : IClientReportService
    {
        private readonly ApplicationDbContext _db;

        public ClientReportService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<ClientReportRowDto>> GetReportAsync(DateTime? датаНачала, DateTime? датаОкончания, CancellationToken ct = default)
        {
            var q =
                from c in _db.Clients
                join o in _db.Orders on c.ID_Заказа equals o.ID_Заказа into oj
                from o in oj.DefaultIfEmpty()
                join r in _db.RentalBookings on o.ID_Заказа equals r.ID_Заказа into rj
                from r in rj.DefaultIfEmpty()
                select new { c, o, r };

            if (датаНачала is DateTime dn)
                q = q.Where(x => x.r == null || x.r.ДатаОкончания >= dn);
            if (датаОкончания is DateTime dk)
                q = q.Where(x => x.r == null || x.r.ДатаНачала <= dk);

            var rows = await q
                .OrderByDescending(x => x.o != null ? x.o.Сумма_оплаты : 0m)
                .Select(x => new ClientReportRowDto
                {
                    ClientId = x.c.ID_Клиента,
                    FullName = (x.c.Фамилия ?? "") + " " + (x.c.Имя ?? ""),
                    Age = x.c.Возраст,
                    OrderId = x.o != null ? x.o.ID_Заказа : null,
                    Total = x.o != null ? x.o.Сумма_оплаты : null,
                    DepositStatus = x.c.Залог > 0 ? "Залог внесен" : "Залог не требуется",
                    RentalId = x.r != null ? x.r.ID_Аренды : null,
                    StartDate = x.r != null ? x.r.ДатаНачала : null,
                    EndDate = x.r != null ? x.r.ДатаОкончания : null,
                })
                .ToListAsync(ct);

            return rows;
        }
    }
}
