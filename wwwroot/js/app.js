const TOKEN_KEY = "prokat_token";
const ROLE_KEY = "prokat_role";
const LOGIN_KEY = "prokat_login";

const DAY_START_HOUR = 9;
const FULL_DAY_HOURS = 12;

function getToken() {
    return localStorage.getItem(TOKEN_KEY);
}

function setAuth(token, role, login) {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(ROLE_KEY, role);
    localStorage.setItem(LOGIN_KEY, login);
}

function clearAuth() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(LOGIN_KEY);
}

function getRole() {
    return localStorage.getItem(ROLE_KEY) || "";
}

/** Согласовано с RentalWindowHelper на сервере */
function computeWindow(rentalDateInput, durationKey) {
    const raw = rentalDateInput;
    if (!raw) return { start: null, end: null };
    const [y, m, d] = raw.split("-").map(Number);
    const dayStart = new Date(y, m - 1, d, DAY_START_HOUR, 0, 0, 0);
    let end;
    switch (String(durationKey)) {
        case "1":
            end = new Date(dayStart);
            end.setHours(end.getHours() + 1);
            break;
        case "2":
            end = new Date(dayStart);
            end.setHours(end.getHours() + 2);
            break;
        case "4":
            end = new Date(dayStart);
            end.setHours(end.getHours() + 4);
            break;
        case "day":
            end = new Date(dayStart);
            end.setHours(end.getHours() + FULL_DAY_HOURS);
            break;
        default:
            return { start: null, end: null };
    }
    return { start: dayStart, end };
}

function toIsoUtc(d) {
    if (!d || isNaN(d.getTime())) return null;
    return d.toISOString();
}

function api(path, opts = {}) {
    const headers = { ...(opts.headers || {}) };
    const t = getToken();
    if (t) headers.Authorization = `Bearer ${t}`;
    let body = opts.body;
    if (body && typeof body === "object" && !(body instanceof FormData)) {
        headers["Content-Type"] = "application/json";
        body = JSON.stringify(body);
    }
    return fetch(path, { ...opts, headers, body }).then(async (r) => {
        const text = await r.text();
        let data;
        try {
            data = text ? JSON.parse(text) : null;
        } catch {
            data = text;
        }
        if (!r.ok) {
            if (r.status === 401 && t) {
                clearAuth();
                setTimeout(() => applySessionUI(), 0);
            }
            const msg = data && (data.message || data.title) ? data.message || data.title : r.statusText;
            throw new Error(r.status === 401 ? "Сессия истекла или доступ запрещён. Войдите снова." : msg);
        }
        return data;
    });
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

function renderHistory(container, rows) {
    if (!rows || !rows.length) {
        container.classList.add("empty");
        container.textContent = "Пока нет записей об арендах.";
        return;
    }
    container.classList.remove("empty");
    const h =
        "<thead><tr><th>Заказ</th><th>Начало</th><th>Конец</th><th>Статус</th><th>Сумма с НДС</th><th>Инвентарь</th></tr></thead>";
    const b = rows
        .map(
            (r) =>
                `<tr><td>${r.orderId}</td><td>${fmt(r.start)}</td><td>${fmt(r.end)}</td><td>${escapeHtml(r.status)}</td><td>${r.totalWithVat ?? "—"}</td><td>${escapeHtml(r.inventorySummary ?? "")}</td></tr>`
        )
        .join("");
    container.innerHTML = `<table>${h}<tbody>${b}</tbody></table>`;
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

async function refreshVatHints() {
    try {
        const { vatRate } = await api("/api/settings/vat");
        const pct = (vatRate * 100).toFixed(1);
        const u = document.getElementById("vat-hint-user");
        const a = document.getElementById("vat-hint-admin");
        const text = `Текущая ставка НДС: ${pct}% (для клиентов итог считается на сервере с этой ставкой).`;
        if (u) u.textContent = text;
        if (a) a.textContent = text;
        const vatInput = document.querySelector("#vat-form input[name=vat]");
        if (vatInput) vatInput.value = String(vatRate);
    } catch {
        /* ignore */
    }
}

function applySessionUI() {
    const role = getRole();
    const login = localStorage.getItem(LOGIN_KEY) || "";
    const authSec = document.getElementById("auth-section");
    const userPanel = document.getElementById("user-panel");
    const adminPanel = document.getElementById("admin-panel");
    const sessionInfo = document.getElementById("session-info");
    const btnLogout = document.getElementById("btn-logout");

    if (!getToken()) {
        authSec.classList.remove("hidden");
        userPanel.classList.add("hidden");
        adminPanel.classList.add("hidden");
        sessionInfo.textContent = "";
        btnLogout.classList.add("hidden");
        return;
    }

    authSec.classList.add("hidden");
    btnLogout.classList.remove("hidden");
    sessionInfo.textContent = `Вы вошли как ${login} (${role === "Admin" ? "администратор" : "пользователь"})`;

    if (role === "Admin") {
        userPanel.classList.add("hidden");
        adminPanel.classList.remove("hidden");
    } else {
        userPanel.classList.remove("hidden");
        adminPanel.classList.add("hidden");
    }
    refreshVatHints();
}

document.getElementById("login-form").addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const fd = new FormData(ev.target);
    const msg = document.getElementById("login-msg");
    try {
        const res = await api("/api/auth/login", {
            method: "POST",
            body: { login: fd.get("login"), password: fd.get("password") },
        });
        setAuth(res.token, res.role, res.login);
        setMsg(msg, "Вход выполнен.", true);
        applySessionUI();
    } catch (e) {
        setMsg(msg, e.message, false);
    }
});

document.getElementById("register-form").addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const fd = new FormData(ev.target);
    const msg = document.getElementById("register-msg");
    try {
        const res = await api("/api/auth/register", {
            method: "POST",
            body: {
                login: fd.get("login"),
                password: fd.get("password"),
                lastName: fd.get("фамилия"),
                firstName: fd.get("имя"),
                age: fd.get("возраст") ? Number(fd.get("возраст")) : null,
            },
        });
        setAuth(res.token, res.role, res.login);
        setMsg(msg, "Регистрация успешна, вы вошли в систему.", true);
        applySessionUI();
    } catch (e) {
        setMsg(msg, e.message, false);
    }
});

document.getElementById("btn-logout").addEventListener("click", () => {
    clearAuth();
    applySessionUI();
});

function wireFreeInventory(formId, btnId, listId, msgId) {
    const form = document.getElementById(formId);
    document.getElementById(btnId).addEventListener("click", async () => {
        const fd = new FormData(form);
        const rentalDate = fd.get("rentalDate");
        const durationKey = fd.get("durationKey");
        const type = fd.get("тип");
        const { start, end } = computeWindow(rentalDate, durationKey);
        const elMsg = document.getElementById(msgId);
        const freeList = document.getElementById(listId);
        if (!start || !end) {
            setMsg(elMsg, "Укажите день и длительность.", false);
            return;
        }
        try {
            const q = new URLSearchParams({ start: toIsoUtc(start), end: toIsoUtc(end), type });
            const data = await api(`/api/inventory/free?${q}`);
            setMsg(elMsg, `Найдено: ${data.length} комплект(ов).`, true);
            renderInventory(freeList, data);
        } catch (e) {
            setMsg(elMsg, e.message, false);
        }
    });
}

wireFreeInventory("booking-form-user", "btn-free-user", "free-list-user", "booking-msg-user");
wireFreeInventory("booking-form-admin", "btn-free-admin", "free-list-admin", "booking-msg-admin");

document.getElementById("booking-form-user").addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const fd = new FormData(ev.target);
    const msg = document.getElementById("booking-msg-user");
    const body = {
        rentalDate: fd.get("rentalDate"),
        durationKey: fd.get("durationKey"),
        equipmentType: fd.get("тип"),
    };
    try {
        const res = await api("/api/bookings", { method: "POST", body });
        setMsg(
            msg,
            `Бронь создана. Заказ №${res.orderId}, сумма с НДС: ${res.totalWithVat} ₽`,
            true
        );
        const ht = document.getElementById("history-table");
        if (ht) {
            try {
                const hist = await api("/api/rentals/my");
                renderHistory(ht, hist);
            } catch {
                /* ignore */
            }
        }
    } catch (e) {
        setMsg(msg, e.message, false);
    }
});

document.getElementById("booking-form-admin").addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const fd = new FormData(ev.target);
    const msg = document.getElementById("booking-msg-admin");
    const body = {
        lastName: fd.get("фамилия"),
        firstName: fd.get("имя"),
        age: fd.get("возраст") ? Number(fd.get("возраст")) : null,
        rentalDate: fd.get("rentalDate"),
        durationKey: fd.get("durationKey"),
        equipmentType: fd.get("тип"),
    };
    try {
        const res = await api("/api/bookings", { method: "POST", body });
        setMsg(
            msg,
            `Бронь создана. Заказ №${res.orderId}, сумма с НДС: ${res.totalWithVat} ₽`,
            true
        );
    } catch (e) {
        setMsg(msg, e.message, false);
    }
});

document.getElementById("vat-form").addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const fd = new FormData(ev.target);
    const msg = document.getElementById("vat-msg");
    const v = Number(fd.get("vat"));
    try {
        await api("/api/settings/vat", {
            method: "PUT",
            body: { vatRate: v },
        });
        setMsg(msg, "Ставка НДС сохранена.", true);
        refreshVatHints();
    } catch (e) {
        setMsg(msg, e.message, false);
    }
});

document.getElementById("btn-history").addEventListener("click", async () => {
    const el = document.getElementById("history-table");
    try {
        const data = await api("/api/rentals/my");
        renderHistory(el, data);
    } catch (e) {
        el.classList.add("empty");
        el.textContent = e.message;
    }
});

document.getElementById("btn-admin-stats").addEventListener("click", async () => {
    const el = document.getElementById("admin-stats");
    try {
        const s = await api("/api/admin/stats");
        el.textContent = `Зарегистрированных пользователей: ${s.registeredUsers}, администраторов: ${s.administrators}, заказов всего: ${s.totalOrders}, активных аренд (не «Отмена»): ${s.activeRentals}.`;
    } catch (e) {
        el.textContent = e.message;
    }
});

document.getElementById("btn-admin-users").addEventListener("click", async () => {
    const container = document.getElementById("admin-users-table");
    try {
        const rows = await api("/api/admin/users");
        if (!rows.length) {
            container.classList.add("empty");
            container.textContent = "Нет учётных записей.";
            return;
        }
        container.classList.remove("empty");
        const head = "<thead><tr><th>ID</th><th>Логин</th><th>Роль</th><th>ID клиента</th><th></th></tr></thead>";
        const body = rows
            .map((u) => {
                const del =
                    u.role === "Admin"
                        ? "—"
                        : `<button type="button" class="secondary btn-del-user" data-id="${u.id}">Удалить</button>`;
                return `<tr><td>${u.id}</td><td>${escapeHtml(u.login)}</td><td>${escapeHtml(u.role)}</td><td>${u.clientId ?? "—"}</td><td>${del}</td></tr>`;
            })
            .join("");
        container.innerHTML = `<table>${head}<tbody>${body}</tbody></table>`;
        container.querySelectorAll(".btn-del-user").forEach((btn) => {
            btn.addEventListener("click", async () => {
                if (!confirm("Удалить учётную запись пользователя? Вход будет невозможен.")) return;
                try {
                    await api(`/api/admin/users/${btn.dataset.id}`, { method: "DELETE" });
                    btn.closest("tr")?.remove();
                } catch (e) {
                    alert(e.message);
                }
            });
        });
    } catch (e) {
        container.classList.add("empty");
        container.textContent = e.message;
    }
});

document.getElementById("btn-admin-rentals").addEventListener("click", async () => {
    const container = document.getElementById("admin-rentals-table");
    try {
        const rows = await api("/api/admin/rentals");
        if (!rows.length) {
            container.classList.add("empty");
            container.textContent = "Нет аренд.";
            return;
        }
        container.classList.remove("empty");
        const head =
            "<thead><tr><th>ID аренды</th><th>Заказ</th><th>Клиент</th><th>Начало</th><th>Конец</th><th>Статус</th><th>Сумма</th><th></th></tr></thead>";
        const body = rows
            .map((r) => {
                const cancel =
                    r.status === "Отмена"
                        ? "—"
                        : `<button type="button" class="secondary btn-cancel-rental" data-id="${r.rentalId}">Отменить</button>`;
                return `<tr><td>${r.rentalId}</td><td>${r.orderId}</td><td>${escapeHtml(r.clientName ?? "—")}</td><td>${fmt(r.start)}</td><td>${fmt(r.end)}</td><td>${escapeHtml(r.status)}</td><td>${r.totalWithVat ?? "—"}</td><td>${cancel}</td></tr>`;
            })
            .join("");
        container.innerHTML = `<table>${head}<tbody>${body}</tbody></table>`;
        container.querySelectorAll(".btn-cancel-rental").forEach((btn) => {
            btn.addEventListener("click", async () => {
                if (!confirm("Отменить эту аренду?")) return;
                try {
                    await api(`/api/admin/rentals/${btn.dataset.id}/cancel`, { method: "POST" });
                    const tr = btn.closest("tr");
                    if (tr) {
                        tr.cells[5].textContent = "Отмена";
                        btn.replaceWith("—");
                    }
                } catch (e) {
                    alert(e.message);
                }
            });
        });
    } catch (e) {
        container.classList.add("empty");
        container.textContent = e.message;
    }
});

document.getElementById("btn-report").addEventListener("click", async () => {
    const from = document.getElementById("rep-from").value;
    const to = document.getElementById("rep-to").value;
    const q = new URLSearchParams();
    if (from) q.set("from", from);
    if (to) q.set("to", to);
    const reportTable = document.getElementById("report-table");
    try {
        const data = await api(`/api/reports/clients?${q}`);
        renderReport(reportTable, data);
    } catch (e) {
        reportTable.classList.add("empty");
        reportTable.textContent = e.message;
    }
});

(function defaults() {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const pad = (n) => String(n).padStart(2, "0");
    const y = tomorrow.getFullYear();
    const m = pad(tomorrow.getMonth() + 1);
    const d = pad(tomorrow.getDate());
    const iso = `${y}-${m}-${d}`;
    const u = document.querySelector("#booking-form-user [name=rentalDate]");
    const a = document.querySelector("#booking-form-admin [name=rentalDate]");
    if (u) u.value = iso;
    if (a) a.value = iso;
})();

applySessionUI();
