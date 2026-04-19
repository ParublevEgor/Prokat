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
let userProfile = null;

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

const COLUMN_LABELS = {
    rentalId: "ID аренды",
    orderId: "ID заказа",
    inventoryId: "ID инвентаря",
    clientName: "Клиент",
    start: "Начало",
    end: "Окончание",
    status: "Статус",
    totalWithVat: "Сумма с НДС",
    total: "Итог",
    inventorySummary: "Инвентарь",
    id: "ID",
    login: "Логин",
    role: "Роль",
    clientId: "ID клиента",
    type: "Тип",
    skis: "Лыжи",
    snowboard: "Сноуборд",
    boots: "Ботинки",
    poles: "Палки",
    durationText: "Время аренды",
    weekdayPrice: "Будни (руб.)",
    weekendPrice: "Выходные (руб.)",
    canDelete: "Можно удалить",
    deleteBlockedReason: "Причина блокировки"
};

function formatCell(key, value) {
    if (value == null) return "";
    const lower = key.toLowerCase();
    if (lower.includes("start") || lower.includes("end")) return escapeHtml(fmt(value));
    if (lower === "total" || lower.includes("sum") || lower.includes("price") || lower.includes("vat"))
        return escapeHtml(`${value} ₽`);
    if (typeof value === "boolean") return value ? "Да" : "Нет";
    return escapeHtml(value);
}

function renderTable(container, rows, labels = COLUMN_LABELS) {
    if (!rows || !rows.length) {
        container.classList.add("empty");
        container.textContent = "Нет данных.";
        return;
    }
    container.classList.remove("empty");
    const keys = Object.keys(rows[0]);
    const h = `<thead><tr>${keys.map(k=>`<th>${escapeHtml(labels[k] || k)}</th>`).join("")}</tr></thead>`;
    const b = rows.map(r=>`<tr>${keys.map(k=>`<td>${formatCell(k, r[k])}</td>`).join("")}</tr>`).join("");
    container.innerHTML = `<table>${h}<tbody>${b}</tbody></table>`;
}

function renderAdminUsersTable(container, rows) {
    if (!rows || !rows.length) {
        container.classList.add("empty");
        container.textContent = "Нет пользователей.";
        return;
    }
    container.classList.remove("empty");
    const head = "<thead><tr><th>ID</th><th>Логин</th><th>Роль</th><th>ID клиента</th><th>Удаление</th></tr></thead>";
    const body = rows.map((u) => {
        const disabled = !u.canDelete ? "disabled" : "";
        const title = u.deleteBlockedReason ? ` title="${escapeHtml(u.deleteBlockedReason)}"` : "";
        return `<tr>
            <td>${u.id}</td>
            <td>${escapeHtml(u.login || "")}</td>
            <td>${escapeHtml(u.role || "")}</td>
            <td>${escapeHtml(u.clientId ?? "")}</td>
            <td><button class="secondary btn-del-user" data-id="${u.id}" ${disabled}${title}>Удалить</button></td>
        </tr>`;
    }).join("");
    container.innerHTML = `<table>${head}<tbody>${body}</tbody></table>`;
    container.querySelectorAll(".btn-del-user").forEach((btn) => {
        btn.addEventListener("click", async () => {
            const id = Number(btn.getAttribute("data-id"));
            if (!id || !confirm("Удалить пользователя?")) return;
            try {
                await api(`/api/admin/users/${id}`, { method: "DELETE" });
                btn.closest("tr")?.remove();
            } catch (e) {
                alert(e.message);
            }
        });
    });
}

function setAvatar(src) {
    const img = byId("user-avatar");
    if (!img) return;
    img.src = src || "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='96' height='96'%3E%3Crect width='100%25' height='100%25' fill='%23e8edf5'/%3E%3Ccircle cx='48' cy='36' r='16' fill='%23b9c4d6'/%3E%3Crect x='20' y='58' width='56' height='24' rx='12' fill='%23b9c4d6'/%3E%3C/svg%3E";
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
    const unique = [...new Set(values.filter(v => v != null && v !== ""))];
    select.innerHTML = unique.map(v=>`<option value="${escapeHtml(v)}">${escapeHtml(v)}</option>`).join("");
    select.disabled = unique.length === 0;
}

function filterInventoryRows(rows, form) {
    const boots   = form.querySelector('[name="ботинки"]').value;
    const poles   = form.querySelector('[name="палки"]').value;
    const helmet  = form.querySelector('[name="шлем"]').value;
    const goggles = form.querySelector('[name="очки"]').value;
    const isSnowboard = form.querySelector('[name="тип"]').value === "Сноуборд";
    return rows.filter(r =>
        r.boots === boots &&
        (isSnowboard || r.poles === poles) &&
        r.helmet === helmet &&
        r.goggles === goggles
    );
}

/* ── управление сессией / UI ─────────────────────────────────────── */
function applySessionUI() {
    const token = getToken();
    const role = getRole();
    const login = localStorage.getItem(LOGIN_KEY) || "";
    if (token && (!role || !login)) {
        clearAuth();
    }
    const auth = !!getToken();
    byId("auth-section")?.classList.toggle("hidden", auth);
    byId("user-panel")?.classList.toggle("hidden", !(auth && role === "User"));
    byId("admin-panel")?.classList.toggle("hidden", !(auth && role === "Admin"));
    byId("btn-logout")?.classList.toggle("hidden", !auth);
    if (byId("session-info")) byId("session-info").textContent = auth ? `Вы вошли как ${login} (${role})` : "";

    if (auth && role === "User") {
        applyEquipmentTypeUI();
        loadFreeInventory();
        loadMyProfile();
    } else {
        setAvatar("");
        byId("profile-form")?.classList.add("hidden");
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

async function loadMyProfile() {
    try {
        const me = await api("/api/auth/me");
        userProfile = me;
        const form = byId("profile-form");
        if (form) {
            form.querySelector('[name="lastName"]').value = me.lastName ?? "";
            form.querySelector('[name="firstName"]').value = me.firstName ?? "";
            form.querySelector('[name="age"]').value = me.age ?? "";
            form.querySelector('[name="height"]').value = me.height ?? "";
            form.querySelector('[name="weight"]').value = me.weight ?? "";
            form.querySelector('[name="shoeSize"]').value = me.shoeSize ?? "";
        }
        setAvatar(me.profilePhotoBase64 || "");
    } catch (_) { }
}

on("btn-edit-profile", "click", () => {
    byId("profile-form")?.classList.remove("hidden");
});

on("btn-cancel-profile", "click", () => {
    byId("profile-form")?.classList.add("hidden");
});

on("profile-form", "submit", async (ev) => {
    ev.preventDefault();
    const form = ev.target;
    const fd = new FormData(form);
    const file = form.querySelector('[name="avatarFile"]').files?.[0] || null;

    const toDataUrl = (f) => new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(String(reader.result || ""));
        reader.onerror = reject;
        reader.readAsDataURL(f);
    });

    let avatarBase64 = null;
    const removeAvatar = form.dataset.removeAvatar === "1";
    if (file) {
        if (file.size > 1024 * 1024) {
            setMsg(byId("profile-msg"), "Файл аватара слишком большой (максимум 1 МБ).", false);
            return;
        }
        avatarBase64 = await toDataUrl(file);
    }

    const body = {
        lastName: fd.get("lastName") || null,
        firstName: fd.get("firstName") || null,
        age: fd.get("age") ? Number(fd.get("age")) : null,
        height: fd.get("height") ? Number(fd.get("height")) : null,
        weight: fd.get("weight") ? Number(fd.get("weight")) : null,
        shoeSize: fd.get("shoeSize") ? Number(fd.get("shoeSize")) : null,
        profilePhotoBase64: avatarBase64,
        removeProfilePhoto: removeAvatar
    };

    try {
        await api("/api/auth/me", { method: "PUT", body });
        setMsg(byId("profile-msg"), "Профиль обновлён.", true);
        byId("profile-form")?.classList.add("hidden");
        await loadMyProfile();
        form.dataset.removeAvatar = "0";
    } catch (e) {
        setMsg(byId("profile-msg"), e.message, false);
    }
});

on("btn-remove-avatar", "click", async () => {
    const form = byId("profile-form");
    if (!form) return;
    form.dataset.removeAvatar = "1";
    form.querySelector('[name="avatarFile"]').value = "";
    try {
        await api("/api/auth/me", { method: "PUT", body: { removeProfilePhoto: true } });
        setAvatar("");
        setMsg(byId("profile-msg"), "Аватар удалён.", true);
    } catch (e) {
        setMsg(byId("profile-msg"), e.message, false);
    }
});

function choosePreferredOption(values, preferredText) {
    if (!values.length) return "";
    if (!preferredText) return values[0];
    const needle = String(preferredText);
    const hit = values.find(v => String(v).includes(needle));
    return hit || values[0];
}

function applyEquipmentTypeUI() {
    const type = byId("sel-type")?.value;
    const polesLabel = byId("label-poles");
    if (!polesLabel) return;
    polesLabel.classList.toggle("hidden", type === "Сноуборд");
}

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
        userProfile = me;
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
        const bootsValues = [...new Set(rows.map(r => r.boots).filter(Boolean))];
        const polesValues = [...new Set(rows.map(r => r.poles).filter(Boolean))];
        const helmetValues = [...new Set(rows.map(r => r.helmet).filter(Boolean))];
        const gogglesValues = [...new Set(rows.map(r => r.goggles).filter(Boolean))];

        const bootsSelect = form.querySelector('[name="ботинки"]');
        const polesSelect = form.querySelector('[name="палки"]');
        const helmetSelect = form.querySelector('[name="шлем"]');
        const gogglesSelect = form.querySelector('[name="очки"]');

        fillSelectOptions(bootsSelect, bootsValues);
        fillSelectOptions(polesSelect, polesValues);
        fillSelectOptions(helmetSelect, helmetValues);
        fillSelectOptions(gogglesSelect, gogglesValues);

        const profileBoot = userProfile?.shoeSize ? String(userProfile.shoeSize) : "";
        const h = Number(userProfile?.height ?? 0);
        const sizeMark = h ? (h < 165 ? "S" : (h < 180 ? "M" : "L")) : "";

        bootsSelect.value = choosePreferredOption(bootsValues, profileBoot);
        helmetSelect.value = choosePreferredOption(helmetValues, sizeMark);
        gogglesSelect.value = choosePreferredOption(gogglesValues, sizeMark);
        polesSelect.value = choosePreferredOption(polesValues, sizeMark);

        applyEquipmentTypeUI();
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
    applyEquipmentTypeUI();
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

function buildPriceHtml() {
    const columns = ["2 часа", "3 часа", "4 часа", "День"];
    const weekday = [
        ["Комплект (ботинки, крепления, сноуборд/лыжи)", [700, 750, 850, 1000]],
        ["Ботинки (сноуборд/горные лыжи)", [500, 550, 700, 800]],
        ["Сноуборд / горные лыжи", [500, 550, 700, 800]],
        ["Палки", [250, 300, 350, 400]],
        ["Шлем", [150, 200, 300, 400]],
        ["Маска", [150, 200, 300, 400]],
        ["Куртка / штаны", [400, 450, 500, 500]],
        ["Перчатки", [250, 300, 350, 350]]
    ];
    const weekend = [
        ["Комплект (ботинки, крепления, сноуборд/лыжи)", [800, 900, 1100, 1500]],
        ["Ботинки (сноуборд/горные лыжи)", [600, 650, 800, 1000]],
        ["Сноуборд / горные лыжи", [600, 650, 800, 1000]],
        ["Палки", [250, 300, 350, 400]],
        ["Шлем", [200, 250, 300, 400]],
        ["Маска", [200, 250, 300, 400]],
        ["Куртка / штаны", [450, 500, 550, 550]],
        ["Перчатки", [250, 300, 350, 350]]
    ];

    const render = (title, rows) => {
        const head = `<thead><tr><th>Позиция</th>${columns.map(c => `<th>${c}</th>`).join("")}</tr></thead>`;
        const body = rows.map(([name, prices]) =>
            `<tr><td>${escapeHtml(name)}</td>${prices.map(p => `<td>${p} ₽</td>`).join("")}</tr>`
        ).join("");
        return `<h3 style="margin:.65rem 0">${title}</h3><table>${head}<tbody>${body}</tbody></table>`;
    };

    return render("Будни", weekday) + render("Выходные и праздничные дни", weekend);
}

on("btn-load-tariffs", "click", () => {
    const wrap = byId("tariffs-table");
    if (!wrap) return;
    const shouldShow = wrap.classList.contains("hidden");
    wrap.classList.toggle("hidden", !shouldShow);
    if (shouldShow && !wrap.dataset.ready) {
        wrap.innerHTML = buildPriceHtml();
        wrap.dataset.ready = "1";
    }
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
    try { renderAdminUsersTable(document.getElementById("admin-users-table"), await api("/api/admin/users")); }
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

(async function loadVatDefault() {
    try {
        const res = await api("/api/settings/vat");
        const sel = document.querySelector('#vat-form [name="vat"]');
        if (sel) sel.value = Number(res.vatRate) >= 0.2 ? "0.20" : "0.18";
    } catch (_) { }
})();

applySessionUI();
