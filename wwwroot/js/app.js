const api = (path, opts) => fetch(path, opts).then(async (r) => {
    const text = await r.text();
    let data;
    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }
    if (!r.ok) {
        const msg = data && data.message ? data.message : r.statusText;
        throw new Error(msg);
    }
    return data;
});

function toIsoLocal(dtLocal) {
    if (!dtLocal) return null;
    const d = new Date(dtLocal);
    return isNaN(d.getTime()) ? null : d.toISOString();
}

function setMsg(el, text, ok) {
    el.textContent = text;
    el.classList.remove("ok", "err");
    el.classList.add(ok ? "ok" : "err");
}

function renderInventory(container, items) {
    if (!items || !items.length) {
        container.classList.add("empty");
        container.textContent = "Нет свободных комплектов на выбранные даты.";
        return;
    }
    container.classList.remove("empty");
    const rows = items.map(
        (i) =>
            `<tr><td>${i.id}</td><td>${escapeHtml(i.skis ?? "")}</td><td>${escapeHtml(i.snowboard ?? "")}</td><td>${escapeHtml(i.boots ?? "")}</td><td>${escapeHtml(i.poles ?? "")}</td></tr>`
    );
    container.innerHTML = `<table><thead><tr><th>ID</th><th>Лыжи</th><th>Сноуборд</th><th>Ботинки</th><th>Палки</th></tr></thead><tbody>${rows.join("")}</tbody></table>`;
}

function renderReport(container, rows) {
    if (!rows || !rows.length) {
        container.classList.add("empty");
        container.textContent = "Нет данных.";
        return;
    }
    container.classList.remove("empty");
    const h = `<thead><tr><th>Клиент</th><th>Возраст</th><th>Заказ</th><th>Сумма</th><th>Залог</th><th>Начало</th><th>Конец</th></tr></thead>`;
    const b = rows
        .map(
            (r) =>
                `<tr><td>${escapeHtml(r.fullName ?? "")}</td><td>${r.age ?? ""}</td><td>${r.orderId ?? ""}</td><td>${r.total ?? ""}</td><td>${escapeHtml(r.depositStatus ?? "")}</td><td>${fmt(r.startDate)}</td><td>${fmt(r.endDate)}</td></tr>`
        )
        .join("");
    container.innerHTML = `<table>${h}<tbody>${b}</tbody></table>`;
}

function fmt(v) {
    if (!v) return "";
    const d = new Date(v);
    return isNaN(d.getTime()) ? String(v) : d.toLocaleString();
}

function escapeHtml(s) {
    return String(s)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");
}

const form = document.getElementById("booking-form");
const msg = document.getElementById("booking-msg");
const freeList = document.getElementById("free-list");
const reportTable = document.getElementById("report-table");

document.getElementById("btn-free").addEventListener("click", async () => {
    const fd = new FormData(form);
    const start = toIsoLocal(fd.get("начало"));
    const end = toIsoLocal(fd.get("конец"));
    const type = fd.get("тип");
    if (!start || !end) {
        setMsg(msg, "Укажите дату и время начала и окончания.", false);
        return;
    }
    try {
        const q = new URLSearchParams({ start, end, type });
        const data = await api(`/api/inventory/free?${q}`);
        setMsg(msg, `Найдено: ${data.length} комплект(ов).`, true);
        renderInventory(freeList, data);
    } catch (e) {
        setMsg(msg, e.message, false);
    }
});

form.addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const fd = new FormData(form);
    const body = {
        lastName: fd.get("фамилия"),
        firstName: fd.get("имя"),
        age: fd.get("возраст") ? Number(fd.get("возраст")) : null,
        equipmentType: fd.get("тип"),
        startDate: toIsoLocal(fd.get("начало")),
        endDate: toIsoLocal(fd.get("конец")),
        vatRate: fd.get("ндс") ? Number(fd.get("ндс")) : 0.18,
    };
    try {
        const res = await api("/api/bookings", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body),
        });
        setMsg(
            msg,
            `Бронь создана. Заказ №${res.orderId}, сумма с НДС: ${res.totalWithVat} ₽`,
            true
        );
    } catch (e) {
        setMsg(msg, e.message, false);
    }
});

document.getElementById("btn-report").addEventListener("click", async () => {
    const from = document.getElementById("rep-from").value;
    const to = document.getElementById("rep-to").value;
    const q = new URLSearchParams();
    if (from) q.set("from", from);
    if (to) q.set("to", to);
    try {
        const data = await api(`/api/reports/clients?${q}`);
        renderReport(reportTable, data);
    } catch (e) {
        reportTable.classList.add("empty");
        reportTable.textContent = e.message;
    }
});

(function defaults() {
    const now = new Date();
    now.setDate(now.getDate() + 1);
    now.setHours(10, 0, 0, 0);
    const end = new Date(now);
    end.setHours(18, 0, 0, 0);
    const fmtLocal = (d) => {
        const pad = (n) => String(n).padStart(2, "0");
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    };
    form.querySelector('[name="начало"]').value = fmtLocal(now);
    form.querySelector('[name="конец"]').value = fmtLocal(end);
})();
