const TOKEN_KEY  = "prokat_token";
const ROLE_KEY   = "prokat_role";
const LOGIN_KEY  = "prokat_login";

function getToken() { return localStorage.getItem(TOKEN_KEY); }
function setAuth(token, role, login) {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(ROLE_KEY,  role);
    localStorage.setItem(LOGIN_KEY, login);
}
function clearAuth() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(LOGIN_KEY);
}
function getRole() { return localStorage.getItem(ROLE_KEY) || ""; }
function byId(id) { return document.getElementById(id); }
function on(id, event, handler) {
    const el = byId(id);
    if (el) el.addEventListener(event, handler);
}

/* ── базовый fetch-помощник ─────────────────────────────────────── */
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
        try { data = text ? JSON.parse(text) : null; } catch { data = text; }
        if (!r.ok) throw new Error((data && (data.message || data.title)) || `HTTP ${r.status}`);
        return data;
    });
}

/* ── утилиты ─────────────────────────────────────────────────────── */
function setMsg(el, text, ok) {
    el.textContent = text;
    el.classList.remove("ok", "err");
    el.classList.add(ok ? "ok" : "err");
}
function fmt(v) {
    if (!v) return "";
    const d = new Date(v);
    return isNaN(d.getTime()) ? String(v) : d.toLocaleString();
}
function escapeHtml(s) {
    return String(s).replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;");
}

function renderTable(container, rows) {
    if (!rows || !rows.length) {
        container.classList.add("empty");
        container.textContent = "Нет данных.";
        return;
    }
    container.classList.remove("empty");
    const keys = Object.keys(rows[0]);
    const h = `<thead><tr>${keys.map(k=>`<th>${escapeHtml(k)}</th>`).join("")}</tr></thead>`;
    const b = rows.map(r=>`<tr>${keys.map(k=>`<td>${escapeHtml(r[k]??``)}</td>`).join("")}</tr>`).join("");
    container.innerHTML = `<table>${h}<tbody>${b}</tbody></table>`;
}

function renderFreeInventoryByType(container, rows, equipmentType) {
    if (!rows || !rows.length) {
        container.classList.add("empty");
        container.textContent = "Свободного инвентаря нет на выбранное время.";
        return;
    }
    container.classList.remove("empty");
    const isSkis = equipmentType === "Лыжи";
    const head = isSkis
        ? "<thead><tr><th>ID</th><th>Лыжи</th><th>Ботинки</th><th>Палки</th><th>Шлем</th><th>Очки</th></tr></thead>"
        : "<thead><tr><th>ID</th><th>Сноуборд</th><th>Ботинки</th><th>Шлем</th><th>Очки</th></tr></thead>";
    const body = rows.map(r => {
        return isSkis
            ? `<tr><td>${r.id}</td><td>${escapeHtml(r.skis??"")}</td><td>${escapeHtml(r.boots??"")}</td><td>${escapeHtml(r.poles??"")}</td><td>${escapeHtml(r.helmet??"")}</td><td>${escapeHtml(r.goggles??"")}</td></tr>`
            : `<tr><td>${r.id}</td><td>${escapeHtml(r.snowboard??"")}</td><td>${escapeHtml(r.boots??"")}</td><td>${escapeHtml(r.helmet??"")}</td><td>${escapeHtml(r.goggles??"")}</td></tr>`;
    }).join("");
    container.innerHTML = `<table>${head}<tbody>${body}</tbody></table>`;
}

function fillSelectOptions(select, values) {
    const placeholder = select.options[0]?.textContent ?? "Любые";
    const unique = [...new Set(values.filter(v => v != null && v !== ""))];
    select.innerHTML = `<option value="">${escapeHtml(placeholder)}</option>`
        + unique.map(v=>`<option value="${escapeHtml(v)}">${escapeHtml(v)}</option>`).join("");
}

function filterInventoryRows(rows, form) {
    const boots   = form.querySelector('[name="ботинки"]').value;
    const poles   = form.querySelector('[name="палки"]').value;
    const helmet  = form.querySelector('[name="шлем"]').value;
    const goggles = form.querySelector('[name="очки"]').value;
    return rows.filter(r =>
        (!boots   || r.boots   === boots)   &&
        (!poles   || r.poles   === poles)   &&
        (!helmet  || r.helmet  === helmet)  &&
        (!goggles || r.goggles === goggles)
    );
}

/* ── управление сессией / UI ─────────────────────────────────────── */
function applySessionUI() {
    const role  = getRole();
    const login = localStorage.getItem(LOGIN_KEY) || "";
    const auth  = !!getToken();
    byId("auth-section")?.classList.toggle("hidden",  auth);
    byId("user-panel")?.classList.toggle("hidden", !(auth && role === "User"));
    byId("admin-panel")?.classList.toggle("hidden", !(auth && role === "Admin"));
    byId("btn-logout")?.classList.toggle("hidden", !auth);
    if (byId("session-info")) byId("session-info").textContent = auth ? `Вы вошли как ${login} (${role})` : "";

    if (auth && role === "User") {
        loadFreeInventory();
    }
}

/* ── авторизация ─────────────────────────────────────────────────── */
async function submitLogin(endpoint) {
    const loginInput = byId("inp-login");
    const passInput = byId("inp-password");
    let login = loginInput ? loginInput.value.trim() : "";
    let password = passInput ? passInput.value : "";
    const msg = byId("login-msg");

    // Совместимость со старой разметкой, где был form#login-form
    if ((!login || !password) && byId("login-form")) {
        const fd = new FormData(byId("login-form"));
        login = String(fd.get("login") || "").trim();
        password = String(fd.get("password") || "");
    }

    if (!login || !password) {
        setMsg(msg, "Введите логин и пароль.", false);
        return;
    }
    try {
        const res = await api(endpoint, { method: "POST", body: { login, password } });
        setAuth(res.token, res.role, res.login);
        if (msg) setMsg(msg, "Вход выполнен.", true);
        applySessionUI();
    } catch (e) {
        if (msg) setMsg(msg, e.message, false);
    }
}

on("btn-login-user", "click", async () => {
    await submitLogin("/api/auth/login/user");
});

on("btn-login-admin", "click", async () => {
    await submitLogin("/api/auth/login/admin");
});

on("btn-show-register", "click", () => {
    const wrap = byId("register-wrap");
    if (!wrap) return;
    wrap.classList.remove("hidden");
    wrap.scrollIntoView({ behavior: "smooth", block: "start" });
});

on("btn-hide-register", "click", () => {
    const wrap = byId("register-wrap");
    if (wrap) wrap.classList.add("hidden");
});

on("register-form", "submit", async (ev) => {
    ev.preventDefault();
    const fd  = new FormData(ev.target);
    const msg = document.getElementById("register-msg");
    try {
        const res = await api("/api/auth/register", { method: "POST", body: {
            login:     fd.get("login"),
            password:  fd.get("password"),
            lastName:  fd.get("фамилия"),
            firstName: fd.get("имя"),
            age:       fd.get("возраст")     ? Number(fd.get("возраст"))     : null,
            height:    fd.get("рост")        ? Number(fd.get("рост"))        : null,
            weight:    fd.get("вес")         ? Number(fd.get("вес"))         : null,
            shoeSize:  fd.get("размерОбуви") ? Number(fd.get("размерОбуви")) : null
        }});
        setAuth(res.token, res.role, res.login);
        setMsg(msg, "Регистрация успешна.", true);
        applySessionUI();
    } catch (e) { setMsg(msg, e.message, false); }
});

on("btn-logout", "click", () => {
    clearAuth();
    applySessionUI();
});

/* ── инвентарь: загрузка и фильтрация ───────────────────────────── */
async function loadFreeInventory() {
    const form = document.getElementById("booking-form-user");
    const fd   = new FormData(form);
    const msgEl = document.getElementById("booking-msg-user");

    const rentalDate = fd.get("rentalDate");
    if (!rentalDate) return;

    const start = new Date(`${rentalDate}T09:00:00`);
    const end   = new Date(start);
    const dur   = fd.get("durationKey");
    end.setHours(end.getHours() + (dur === "day" ? 12 : Number(dur)));

    let shoeSize = "", height = "";
    try {
        const me = await api("/api/auth/me");
        shoeSize = me.shoeSize ?? "";
        height   = me.height   ?? "";
    } catch (_) { /* профиль недоступен — продолжаем без рекомендаций */ }

    try {
        const q = new URLSearchParams({
            start:    start.toISOString(),
            end:      end.toISOString(),
            type:     fd.get("тип"),
            shoeSize,
            height
        });
        const rows = await api(`/api/inventory/free?${q}`);
        form._freeRows = rows;

        fillSelectOptions(form.querySelector('[name="ботинки"]'), rows.map(r => r.boots));
        fillSelectOptions(form.querySelector('[name="палки"]'),   rows.map(r => r.poles));
        fillSelectOptions(form.querySelector('[name="шлем"]'),    rows.map(r => r.helmet));
        fillSelectOptions(form.querySelector('[name="очки"]'),    rows.map(r => r.goggles));
        renderFreeInventoryByType(
            document.getElementById("free-list-user"),
            rows,
            fd.get("тип")
        );
    } catch (e) {
        setMsg(msgEl, `Ошибка загрузки инвентаря: ${e.message}`, false);
    }
}

on("btn-free-user", "click", loadFreeInventory);

/* Перезагружать список при смене типа (Лыжи/Сноуборд) */
on("sel-type", "change", () => {
    if (byId("user-panel")?.classList.contains("hidden")) return;
    loadFreeInventory();
});

/* ── бронирование ────────────────────────────────────────────────── */
on("booking-form-user", "submit", async (ev) => {
    ev.preventDefault();
    const fd   = new FormData(ev.target);
    const form = ev.target;
    const msg  = document.getElementById("booking-msg-user");
    try {
        const allRows  = form._freeRows || [];
        const filtered = filterInventoryRows(allRows, form);
        const inventoryId = filtered[0]?.id ?? null;

        if (!inventoryId) {
            setMsg(msg, "Нет доступного инвентаря. Нажмите «Найти свободный инвентарь».", false);
            return;
        }

        const res = await api("/api/bookings", {
            method: "POST",
            body: {
                rentalDate:    fd.get("rentalDate"),
                durationKey:   fd.get("durationKey"),
                equipmentType: fd.get("тип"),
                inventoryId
            }
        });
        setMsg(msg, `Бронь подтверждена. Заказ №${res.orderId}, итог: ${res.totalWithVat} ₽`, true);
    } catch (e) {
        setMsg(msg, e.message, false);
    }
});

/* ── история аренд ───────────────────────────────────────────────── */
on("btn-history", "click", async () => {
    try { renderTable(document.getElementById("history-table"), await api("/api/rentals/my")); }
    catch (e) { document.getElementById("history-table").textContent = e.message; }
});

/* ── админ-панель ────────────────────────────────────────────────── */
on("vat-form", "submit", async (ev) => {
    ev.preventDefault();
    const v = Number(new FormData(ev.target).get("vat"));
    try {
        await api("/api/settings/vat", { method: "PUT", body: { vatRate: v } });
        setMsg(document.getElementById("vat-msg"), "Ставка сохранена", true);
    } catch (e) { setMsg(document.getElementById("vat-msg"), e.message, false); }
});

on("btn-admin-users", "click", async () => {
    try { renderTable(document.getElementById("admin-users-table"), await api("/api/admin/users")); }
    catch (e) { document.getElementById("admin-users-table").textContent = e.message; }
});

on("btn-admin-rentals", "click", async () => {
    try { renderTable(document.getElementById("admin-rentals-table"), await api("/api/admin/rentals")); }
    catch (e) { document.getElementById("admin-rentals-table").textContent = e.message; }
});

on("btn-admin-inventory", "click", async () => {
    try { renderTable(document.getElementById("admin-inventory-table"), await api("/api/admin/inventory/status")); }
    catch (e) { document.getElementById("admin-inventory-table").textContent = e.message; }
});

/* ── начальные значения ──────────────────────────────────────────── */
(function defaults() {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    const p = n => String(n).padStart(2, "0");
    const el = document.querySelector('#booking-form-user [name="rentalDate"]');
    if (el) el.value = `${d.getFullYear()}-${p(d.getMonth()+1)}-${p(d.getDate())}`;
})();

applySessionUI();
