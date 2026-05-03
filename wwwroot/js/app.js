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
    totalWithVat: "Итог",
    total: "Итог",
    inventorySummary: "Инвентарь",
    kind: "Тип",
    details: "Содержание",
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
    summary: "Комплект",
    canDelete: "Можно удалить",
    deleteBlockedReason: "Причина блокировки"
};

const ADMIN_RENTAL_COLS = [
    { key: "rentalId", label: "№ аренды" },
    { key: "clientName", label: "Клиент" },
    { key: "start", label: "Начало" },
    { key: "end", label: "Окончание" },
    { key: "status", label: "Статус" },
    { key: "inventorySummary", label: "Инвентарь" },
    { key: "total", label: "Итог" }
];

const MY_ORDER_COLS = [
    { key: "orderId", label: "№ заказа" },
    { key: "kind", label: "Тип" },
    { key: "rentalId", label: "№ аренды" },
    { key: "start", label: "Начало" },
    { key: "end", label: "Окончание" },
    { key: "status", label: "Статус" },
    { key: "details", label: "Содержание" },
    { key: "totalWithVat", label: "Итог" }
];

function equipmentTypeLabel(value) {
    return value === "Snowboard" ? "Сноуборд" : "Лыжи";
}

const CATALOG_SKI_TYPES = [
    { value: "Горные", label: "Горные" },
    { value: "Классические", label: "Классические" },
    { value: "Коньковые", label: "Коньковые" },
    { value: "Беговые", label: "Беговые" }
];
const CATALOG_BOARD_TYPES = [
    { value: "Фрирайд", label: "Фрирайд" },
    { value: "Фристайл", label: "Фристайл" },
    { value: "Универсальный", label: "Универсальный" },
    { value: "Олл-маунтин", label: "Олл-маунтин" }
];
const CATALOG_BOOT_TYPES = [
    { value: "Лыжные", label: "Лыжные" },
    { value: "Сноубордические", label: "Сноубордические" }
];
const CATALOG_POLE_TYPES = [
    { value: "Горные", label: "Горные" },
    { value: "Классические", label: "Классические" },
    { value: "Телескопические", label: "Телескопические" }
];

const CATALOG_CONFIG = {
    skis: {
        title: "Лыжи",
        endpoint: "/api/admin/catalog/skis",
        fields: [
            { key: "name", label: "Название", type: "text", required: true },
            { key: "skiType", label: "Тип", type: "select", required: true, options: CATALOG_SKI_TYPES },
            { key: "lengthCm", label: "Ростовка (см)", type: "number", min: 100, max: 220, required: true },
            { key: "level", label: "Уровень", type: "text" },
            { key: "note", label: "Примечание", type: "text" }
        ]
    },
    snowboards: {
        title: "Сноуборды",
        endpoint: "/api/admin/catalog/snowboards",
        fields: [
            { key: "name", label: "Название", type: "text", required: true },
            { key: "boardType", label: "Тип", type: "select", required: true, options: CATALOG_BOARD_TYPES },
            { key: "lengthCm", label: "Ростовка (см)", type: "number", min: 120, max: 190, required: true },
            { key: "stiffness", label: "Жесткость", type: "text" },
            { key: "note", label: "Примечание", type: "text" }
        ]
    },
    boots: {
        title: "Ботинки",
        endpoint: "/api/admin/catalog/boots",
        fields: [
            { key: "name", label: "Название", type: "text", required: true },
            { key: "bootType", label: "Тип", type: "select", required: true, options: CATALOG_BOOT_TYPES },
            { key: "sizeEu", label: "Размер EU", type: "number", min: 30, max: 52, required: true },
            { key: "note", label: "Примечание", type: "text" }
        ]
    },
    poles: {
        title: "Палки",
        endpoint: "/api/admin/catalog/poles",
        fields: [
            { key: "name", label: "Название", type: "text", required: true },
            { key: "polesType", label: "Тип", type: "select", required: true, options: CATALOG_POLE_TYPES },
            { key: "lengthCm", label: "Длина (см)", type: "number", min: 70, max: 170, required: true },
            { key: "note", label: "Примечание", type: "text" }
        ]
    },
    helmets: {
        title: "Шлемы",
        endpoint: "/api/admin/catalog/helmets",
        fields: [
            { key: "name", label: "Название", type: "text", required: true },
            { key: "size", label: "Размер", type: "text", required: true, placeholder: "S / M / L" },
            { key: "helmetType", label: "Тип", type: "text" }
        ]
    },
    goggles: {
        title: "Очки",
        endpoint: "/api/admin/catalog/goggles",
        fields: [
            { key: "name", label: "Название", type: "text", required: true },
            { key: "size", label: "Размер", type: "text", required: true, placeholder: "S / M / L" },
            { key: "lensType", label: "Тип линзы", type: "text" }
        ]
    }
};

let _activeCatalogKind = "skis";
let _skipassFlowStep = 1;

function formatCell(key, value) {
    if (value == null) return "";
    const lower = key.toLowerCase();
    if (lower.includes("start") || lower.includes("end")) return escapeHtml(fmt(value));
    const currency =
        lower === "total" ||
        lower === "totalwithvat" ||
        lower.endsWith("price") ||
        lower === "rentalamount" ||
        lower === "skipassamount" ||
        lower === "baseamount";
    if (currency) return escapeHtml(`${value} ₽`);
    if (typeof value === "boolean") return value ? "Да" : "Нет";
    return escapeHtml(value);
}

function renderFixedColumnsTable(container, rows, columns) {
    if (!rows?.length) {
        container.classList.add("empty");
        container.textContent = "Нет данных.";
        return;
    }
    container.classList.remove("empty");
    const h = `<thead><tr>${columns.map(c => `<th>${escapeHtml(c.label)}</th>`).join("")}</tr></thead>`;
    const b = rows.map(r =>
        `<tr>${columns.map(c => `<td>${formatCell(c.key, r[c.key])}</td>`).join("")}</tr>`
    ).join("");
    container.innerHTML = `<table>${h}<tbody>${b}</tbody></table>`;
}

async function refreshAdminUndoState() {
    const btn = byId("btn-admin-undo");
    if (!btn) return;
    try {
        const s = await api("/api/admin/users/undo-status");
        btn.disabled = !s.canUndo;
    } catch {
        btn.disabled = true;
    }
}

function openAdminUserDialog(d) {
    const dlg = byId("admin-user-dialog");
    const title = byId("admin-user-dialog-title");
    const dl = byId("admin-user-dialog-dl");
    if (!dlg || !title || !dl) return;
    title.textContent = d.login || "Пользователь";
    const pairs = [
        ["ID учётной записи", d.userId],
        ["Роль", d.role],
        ["Фамилия", d.lastName],
        ["Имя", d.firstName],
        ["Возраст", d.age],
        ["Рост (см)", d.height],
        ["Вес (кг)", d.weight],
        ["Размер обуви", d.shoeSize],
        ["Фото профиля", d.hasProfilePhoto],
        ["Залог (руб.)", d.deposit]
    ];
    dl.innerHTML = pairs.map(([k, v]) => {
        let display = v;
        if (typeof v === "boolean") display = v ? "Да" : "Нет";
        else if (v == null || v === "") display = "—";
        else display = String(v);
        return `<dt>${escapeHtml(k)}</dt><dd>${escapeHtml(display)}</dd>`;
    }).join("");
    dlg.showModal();
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
    const head = "<thead><tr><th>ID учётной записи</th><th>Логин</th><th>Роль</th><th>Действия</th></tr></thead>";
    const body = rows.map((u) => {
        const disabled = !u.canDelete ? "disabled" : "";
        const title = u.deleteBlockedReason ? ` title="${escapeHtml(u.deleteBlockedReason)}"` : "";
        return `<tr>
            <td>${u.id}</td>
            <td>${escapeHtml(u.login || "")}</td>
            <td>${escapeHtml(u.role || "")}</td>
            <td class="admin-user-actions">
                <button type="button" class="secondary btn-view-user" data-id="${u.id}">Просмотр</button>
                <button type="button" class="secondary btn-del-user" data-id="${u.id}" ${disabled}${title}>Удалить</button>
            </td>
        </tr>`;
    }).join("");
    container.innerHTML = `<table>${head}<tbody>${body}</tbody></table>`;
    container.querySelectorAll(".btn-view-user").forEach((btn) => {
        btn.addEventListener("click", async () => {
            const id = Number(btn.getAttribute("data-id"));
            if (!id) return;
            try {
                openAdminUserDialog(await api(`/api/admin/users/${id}/detail`));
            } catch (e) {
                alert(e.message);
            }
        });
    });
    container.querySelectorAll(".btn-del-user").forEach((btn) => {
        btn.addEventListener("click", async () => {
            const id = Number(btn.getAttribute("data-id"));
            if (!id || !confirm("Удалить пользователя?")) return;
            try {
                await api(`/api/admin/users/${id}`, { method: "DELETE" });
                await refreshAdminUndoState();
                renderAdminUsersTable(container, await api("/api/admin/users"));
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

function durationHuman(durationKey) {
    switch (String(durationKey || "")) {
        case "day": return "Весь день (9:00–21:00)";
        case "1": return "1 час";
        case "2": return "2 часа";
        case "4": return "4 часа";
        default: return String(durationKey ?? "");
    }
}

function computeWindowLocal(dateValue, durationKey) {
    if (!dateValue) return { start: null, end: null };
    const start = new Date(`${dateValue}T09:00:00`);
    const end = new Date(start);
    end.setHours(end.getHours() + (String(durationKey) === "day" ? 12 : Number(durationKey || 0)));
    return { start, end };
}

function setBookingStep(n) {
    for (let i = 1; i <= 5; i++) {
        const sec = byId(`booking-step-${i}`);
        if (sec) sec.classList.toggle("hidden", i !== n);
    }
    document.querySelectorAll("[data-step-indicator]").forEach((el) => {
        const sn = Number(el.getAttribute("data-step-indicator"));
        el.classList.toggle("active", sn === n);
        el.classList.toggle("done", sn < n);
    });
}

function resetBookingWizard() {
    const form = byId("booking-form-user");
    if (!form) return;
    form._freeRows = [];
    form._selectedInventoryId = null;
    form._filtersWired = false;
    const freeEl = byId("free-list-user");
    if (freeEl) {
        freeEl.classList.add("empty");
        freeEl.classList.remove("inventory-cards-grid");
        freeEl.innerHTML = "";
    }
    const nextKit = byId("btn-booking-next-4");
    if (nextKit) nextKit.disabled = true;
    const reco = byId("booking-recommendations");
    if (reco) reco.innerHTML = "";
    const sum = byId("booking-summary");
    if (sum) sum.innerHTML = "";
    document.querySelectorAll(".inv-card").forEach((c) => {
        c.classList.remove("inv-card--selected");
        c.setAttribute("aria-selected", "false");
    });
    const skiNo = form.querySelector('input[name="skiPassChoice"][value="no"]');
    if (skiNo) skiNo.checked = true;
    setBookingStep(1);
}

function renderRecommendationsPanel() {
    const el = byId("booking-recommendations");
    if (!el) return;
    const p = userProfile;
    const shoe = p?.shoeSize != null ? String(p.shoeSize) : "";
    const h = Number(p?.height ?? 0);
    const age = p?.age != null ? String(p.age) : "";
    const weight = p?.weight != null ? String(p.weight) : "";
    const sizeMark = h ? (h < 165 ? "S" : (h < 180 ? "M" : "L")) : "";

    if (!p || (!shoe && !h)) {
        el.innerHTML = `
            <h3 class="reco-title">Подсказки по размеру</h3>
            <p class="reco-hint">Укажите в профиле <strong>размер обуви</strong> и <strong>рост</strong> — мы подсветим подходящие комплекты меткой «Рекомендуем» при подборе.</p>`;
        return;
    }

    let blocks = `<h3 class="reco-title">Рекомендации по вашему профилю</h3><ul class="reco-list">`;
    if (shoe)
        blocks += `<li><strong>Ботинки:</strong> ориентир по размеру обуви EU ${escapeHtml(shoe)}.</li>`;
    if (h)
        blocks += `<li><strong>Шлем и маска:</strong> при росте ${escapeHtml(String(h))} см ориентир класса комплекта <strong>${escapeHtml(sizeMark)}</strong>.</li>`;
    if (age || weight) {
        const bits = [];
        if (age) bits.push(`возраст ${escapeHtml(age)}`);
        if (weight) bits.push(`вес ${escapeHtml(weight)} кг`);
        blocks += `<li><strong>Дополнительно:</strong> ${bits.join(", ")}.</li>`;
    }
    blocks += `</ul><p class="reco-hint">На шаге «Комплект» записи с меткой «Рекомендуем» лучше всего совпадают с этими параметрами.</p>`;
    el.innerHTML = blocks;
}

function renderInventoryCards(container, rows, equipmentType) {
    const isSkis = equipmentType === "Skis";
    if (!rows?.length) {
        container.classList.add("empty");
        container.classList.remove("inventory-cards-grid");
        container.innerHTML = "<p>Свободных комплектов не найдено. Измените дату или длительность.</p>";
        return;
    }
    container.classList.remove("empty");
    container.classList.add("inventory-cards-grid");
    const sorted = [...rows].sort((a, b) => Number(b.recommended) - Number(a.recommended));
    container.innerHTML = sorted.map((r) => {
        const badge = r.recommended ? "<span class=\"inv-badge\">Рекомендуем</span>" : "";
        const titleText = r.cardTitle || (`Комплект №${r.id}`);
        let bodyExtra = "";
        if (r.cardSubtitle) {
            bodyExtra = `<p class="inv-card-sub">${escapeHtml(r.cardSubtitle)}</p>`;
        } else {
            const bits = [];
            if (isSkis && r.skis) bits.push(escapeHtml(r.skis));
            if (!isSkis && r.snowboard) bits.push(escapeHtml(r.snowboard));
            if (r.boots) bits.push("бот.: " + escapeHtml(r.boots));
            if (isSkis && r.poles) bits.push("пал.: " + escapeHtml(r.poles));
            const hg = [r.helmet, r.goggles].filter(Boolean);
            if (hg.length) bits.push(hg.map(escapeHtml).join(", "));
            if (bits.length)
                bodyExtra = `<p class="inv-card-sub">${bits.join(" · ")}</p>`;
        }
        const mainModel = isSkis ? r.skis : r.snowboard;
        const ref = (r.modelReference || mainModel || "").trim();
        const details = ref
            ? `<details class="inv-details"><summary>Подробнее о модели</summary><span class="mono">${escapeHtml(ref)}</span></details>`
            : "";
        return `<article class="inv-card${r.recommended ? " inv-card--reco" : ""}" data-id="${r.id}" tabindex="0" role="option" aria-selected="false">
      ${badge}
      <h4 class="inv-card-title">${escapeHtml(titleText)}</h4>
      ${bodyExtra}
      ${details}
    </article>`;
    }).join("");
}

function selectInventoryCard(id) {
    const form = byId("booking-form-user");
    if (!form || !id) return;
    form._selectedInventoryId = id;
    const row = (form._freeRows || []).find((r) => r.id === id);
    if (row) {
        const setSel = (name, val) => {
            const s = form.querySelector(`[name="${name}"]`);
            if (s && val != null && val !== "") s.value = String(val);
        };
        setSel("ботинки", row.boots);
        if (form.querySelector('[name="equipmentType"]')?.value !== "Snowboard")
            setSel("палки", row.poles);
        setSel("шлем", row.helmet);
        setSel("очки", row.goggles);
    }
    form.querySelectorAll(".inv-card").forEach((c) => {
        const on = Number(c.dataset.id) === id;
        c.classList.toggle("inv-card--selected", on);
        c.setAttribute("aria-selected", on ? "true" : "false");
    });
    const next = byId("btn-booking-next-4");
    if (next) next.disabled = !id;
}

function refilterInventoryCards() {
    const form = byId("booking-form-user");
    const container = byId("free-list-user");
    if (!form || !container) return;
    const all = form._freeRows || [];
    const filtered = filterInventoryRows(all, form);
    const type = form.querySelector('[name="equipmentType"]').value;
    renderInventoryCards(container, filtered, type);
    const sel = form._selectedInventoryId;
    if (sel && !filtered.some((r) => r.id === sel)) {
        form._selectedInventoryId = null;
        const next = byId("btn-booking-next-4");
        if (next) next.disabled = true;
    } else if (sel)
        selectInventoryCard(sel);
}

function wireInventoryFilterSelects(form) {
    if (form._filtersWired) return;
    form._filtersWired = true;
    ["ботинки", "палки", "шлем", "очки"].forEach((name) => {
        form.querySelector(`[name="${name}"]`)?.addEventListener("change", () => refilterInventoryCards());
    });
}

async function renderBookingSummary() {
    const form = byId("booking-form-user");
    const box = byId("booking-summary");
    if (!form || !box) return;
    const fd = new FormData(form);
    const id = form._selectedInventoryId;
    const row = (form._freeRows || []).find((r) => r.id === id);
    const dateStr = fd.get("rentalDate");
    const typ = equipmentTypeLabel(String(fd.get("equipmentType") || "Skis"));
    let dtLabel = String(dateStr || "");
    try {
        if (dateStr)
            dtLabel = new Date(`${dateStr}T12:00:00`).toLocaleDateString("ru-RU");
    } catch (_) { /* keep raw */ }
    const durKey = String(fd.get("durationKey") || "");
    const dur = durationHuman(durKey);
    const kitTitle = row?.cardTitle ? escapeHtml(row.cardTitle) : (id ? `Комплект №${id}` : "—");
    const kitSub = row?.cardSubtitle ? escapeHtml(row.cardSubtitle) : "";
    const includeSkiPass = fd.get("skiPassChoice") === "yes";
    const window = computeWindowLocal(String(fd.get("rentalDate") || ""), durKey);
    const fromTo = (window.start && window.end)
        ? `${window.start.toLocaleTimeString("ru-RU", { hour: "2-digit", minute: "2-digit" })} — ${window.end.toLocaleTimeString("ru-RU", { hour: "2-digit", minute: "2-digit" })}`
        : "—";
    let quote;
    try {
        quote = await api("/api/bookings/quote", {
            method: "POST",
            body: {
                rentalDate: fd.get("rentalDate"),
                durationKey: durKey,
                includeSkiPass
            }
        });
    } catch {
        quote = null;
    }
    const priceLines = quote
        ? `<dt>Прокат</dt><dd>${quote.rentalAmount} ₽</dd>
           <dt>Ски-пасс</dt><dd>${quote.skiPassAmount} ₽</dd>
           <dt>К оплате</dt><dd><strong>${quote.totalWithVat} ₽</strong></dd>`
        : `<dt>К оплате</dt><dd>Будет рассчитано при подтверждении</dd>`;
    box.innerHTML = `
    <h3 style="margin:0 0 0.65rem;font-size:1.05rem">Проверьте данные</h3>
    <dl>
      <dt>Дата</dt><dd>${escapeHtml(dtLabel)}</dd>
      <dt>Длительность</dt><dd>${escapeHtml(dur)} (${escapeHtml(fromTo)})</dd>
      <dt>Тип</dt><dd>${escapeHtml(typ)}</dd>
      <dt>Ски-пасс</dt><dd>${includeSkiPass ? "Да" : "Нет"}</dd>
      <dt>Комплект</dt><dd><strong>${kitTitle}</strong>${kitSub ? `<br/><span style="color:var(--muted);font-size:0.9rem">${kitSub}</span>` : ""}</dd>
      ${priceLines}
    </dl>`;
}

function clearBookingMsg() {
    const msg = byId("booking-msg-user");
    if (!msg) return;
    msg.textContent = "";
    msg.classList.remove("ok", "err");
}

function toggleCollapsed(id) {
    const el = byId(id);
    if (!el) return false;
    el.classList.toggle("collapsed");
    return !el.classList.contains("collapsed");
}

function setCatalogTabs(kind) {
    document.querySelectorAll(".catalog-tab").forEach((btn) => {
        btn.classList.toggle("active", btn.dataset.kind === kind);
    });
}

function renderCatalogForm(kind, item = null) {
    const cfg = CATALOG_CONFIG[kind];
    const fieldsWrap = byId("catalog-fields");
    const title = byId("catalog-title");
    const editId = byId("catalog-edit-id");
    const saveBtn = byId("catalog-save-btn");
    if (!cfg || !fieldsWrap || !title || !editId || !saveBtn) return;
    title.textContent = `Справочник: ${cfg.title}`;
    editId.value = item?.id ? String(item.id) : "";
    saveBtn.textContent = item?.id ? "Сохранить изменения" : "Добавить запись";
    fieldsWrap.innerHTML = cfg.fields.map((f) => {
        const raw = item?.[f.key];
        const value = raw == null ? "" : String(raw);
        const minAttr = typeof f.min === "number" ? ` min="${f.min}"` : "";
        const maxAttr = typeof f.max === "number" ? ` max="${f.max}"` : "";
        const req = f.required ? " required" : "";
        const ph = f.placeholder ? ` placeholder="${escapeHtml(f.placeholder)}"` : "";
        if (f.type === "select" && Array.isArray(f.options)) {
            const opts = f.options.map((o) => {
                const sel = value === o.value ? " selected" : "";
                return `<option value="${escapeHtml(o.value)}"${sel}>${escapeHtml(o.label)}</option>`;
            }).join("");
            return `<label>${escapeHtml(f.label)}
                <select name="${f.key}"${req}>${opts}</select>
            </label>`;
        }
        return `<label>${escapeHtml(f.label)}
            <input name="${f.key}" type="${f.type}" value="${escapeHtml(value)}"${minAttr}${maxAttr}${req}${ph} />
        </label>`;
    }).join("");
}

function renderCatalogTable(kind, rows) {
    const cfg = CATALOG_CONFIG[kind];
    const wrap = byId("catalog-table");
    if (!cfg || !wrap) return;
    if (!rows?.length) {
        wrap.classList.add("empty");
        wrap.textContent = "Нет данных в справочнике.";
        return;
    }
    wrap.classList.remove("empty");
    const cols = cfg.fields.map((f) => ({ key: f.key, label: f.label }));
    const head = `<thead><tr>${cols.map((c) => `<th>${escapeHtml(c.label)}</th>`).join("")}<th>Действия</th></tr></thead>`;
    const body = rows.map((row) => {
        const cells = cols.map((c) => `<td>${formatCell(c.key, row[c.key])}</td>`).join("");
        return `<tr data-id="${row.id}">
            ${cells}
            <td class="admin-user-actions">
                <button type="button" class="secondary btn-catalog-edit" data-id="${row.id}">Изменить</button>
                <button type="button" class="secondary btn-catalog-delete" data-id="${row.id}">Удалить</button>
            </td>
        </tr>`;
    }).join("");
    wrap.innerHTML = `<table>${head}<tbody>${body}</tbody></table>`;
}

async function loadCatalog(kind) {
    const cfg = CATALOG_CONFIG[kind];
    if (!cfg) return;
    _activeCatalogKind = kind;
    setCatalogTabs(kind);
    const msg = byId("catalog-msg");
    try {
        const rows = await api(cfg.endpoint);
        byId("catalog-table").dataset.kind = kind;
        byId("catalog-table").dataset.rows = JSON.stringify(rows);
        renderCatalogTable(kind, rows);
        renderCatalogForm(kind);
        if (msg) {
            msg.textContent = "";
            msg.classList.remove("ok", "err");
        }
    } catch (e) {
        if (msg) setMsg(msg, e.message, false);
    }
}

function fillSelectOptions(select, values, withAll = false) {
    const unique = [...new Set(values.filter((v) => v != null && v !== ""))];
    const parts = [];
    if (withAll) parts.push("<option value=\"\">Все</option>");
    parts.push(...unique.map((v) => `<option value="${escapeHtml(v)}">${escapeHtml(v)}</option>`));
    select.innerHTML = parts.join("");
    select.disabled = unique.length === 0 && !withAll;
}

function filterInventoryRows(rows, form) {
    const boots   = form.querySelector('[name="ботинки"]')?.value ?? "";
    const poles   = form.querySelector('[name="палки"]')?.value ?? "";
    const helmet  = form.querySelector('[name="шлем"]')?.value ?? "";
    const goggles = form.querySelector('[name="очки"]')?.value ?? "";
    const isSnowboard = form.querySelector('[name="equipmentType"]')?.value === "Snowboard";
    return rows.filter((r) =>
        (!boots || r.boots === boots) &&
        (isSnowboard || !poles || r.poles === poles) &&
        (!helmet || r.helmet === helmet) &&
        (!goggles || r.goggles === goggles)
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

    if (auth && role === "Admin")
        refreshAdminUndoState();

    if (auth && role === "User") {
        applyEquipmentTypeUI();
        loadMyProfile();
        resetBookingWizard();
    } else {
        setAvatar("");
        byId("profile-form")?.classList.add("hidden");
    }
}

/* ── авторизация ─────────────────────────────────────────────────── */
async function submitLogin() {
    const loginInput = byId("inp-login");
    const passInput = byId("inp-password");
    const roleSelect = byId("sel-login-role");
    let login = loginInput ? loginInput.value.trim() : "";
    let password = passInput ? passInput.value : "";
    const role = roleSelect?.value === "Admin" ? "Admin" : "User";
    const endpoint = role === "Admin" ? "/api/auth/login/admin" : "/api/auth/login/user";
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

on("btn-login-submit", "click", async () => {
    await submitLogin();
});

on("inp-password", "keydown", async (ev) => {
    if (ev.key !== "Enter") return;
    ev.preventDefault();
    await submitLogin();
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

function applyEquipmentTypeUI() {
    const type = byId("sel-equipment-type")?.value;
    const polesLabel = byId("label-poles");
    if (!polesLabel) return;
    polesLabel.classList.toggle("hidden", type === "Snowboard");
}

/* ── инвентарь: загрузка и фильтрация ───────────────────────────── */
async function loadFreeInventory() {
    const form = document.getElementById("booking-form-user");
    const fd   = new FormData(form);
    const msgEl = document.getElementById("booking-msg-user");

    const rentalDate = fd.get("rentalDate");
    if (!rentalDate) {
        setMsg(msgEl, "Укажите день аренды на первом шаге.", false);
        return;
    }
    form._filtersWired = false;

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
            type:     fd.get("equipmentType"),
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

        fillSelectOptions(bootsSelect, bootsValues, true);
        fillSelectOptions(polesSelect, polesValues, true);
        fillSelectOptions(helmetSelect, helmetValues, true);
        fillSelectOptions(gogglesSelect, gogglesValues, true);

        applyEquipmentTypeUI();
        wireInventoryFilterSelects(form);
        renderInventoryCards(document.getElementById("free-list-user"), rows, fd.get("equipmentType"));
        setBookingStep(4);
        setMsg(msgEl, `Найдено комплектов: ${rows.length}. Выберите карточку или сузьте список фильтрами ниже.`, true);
    } catch (e) {
        setMsg(msgEl, `Ошибка загрузки инвентаря: ${e.message}`, false);
    }
}

on("btn-free-user", "click", loadFreeInventory);

/* При смене типа только скрываем палки для сноуборда — без автозагрузки */
on("sel-equipment-type", "change", () => {
    applyEquipmentTypeUI();
});

/* ── мастер бронирования ─────────────────────────────────────────── */
on("btn-booking-next-1", "click", () => {
    const form = byId("booking-form-user");
    const fd = new FormData(form);
    const msg = byId("booking-msg-user");
    if (!fd.get("rentalDate")) {
        setMsg(msg, "Укажите день аренды.", false);
        return;
    }
    clearBookingMsg();
    setBookingStep(2);
});

on("btn-booking-back-2", "click", () => {
    clearBookingMsg();
    setBookingStep(1);
});

on("btn-booking-next-2", "click", async () => {
    try {
        await loadMyProfile();
    } catch (_) { /* профиль недоступен — всё равно показываем подбор */ }
    renderRecommendationsPanel();
    clearBookingMsg();
    setBookingStep(3);
});

on("btn-booking-back-3", "click", () => {
    clearBookingMsg();
    setBookingStep(2);
});

on("btn-booking-next-4", "click", async () => {
    const form = byId("booking-form-user");
    const msg = byId("booking-msg-user");
    if (!form?._selectedInventoryId) {
        setMsg(msg, "Выберите комплект на карточках выше.", false);
        return;
    }
    await renderBookingSummary();
    clearBookingMsg();
    setBookingStep(5);
});

on("btn-booking-back-4", "click", () => {
    clearBookingMsg();
    setBookingStep(3);
});

on("btn-booking-back-5", "click", () => {
    clearBookingMsg();
    setBookingStep(4);
});

on("btn-booking-confirm", "click", async () => {
    const form = byId("booking-form-user");
    const msg  = byId("booking-msg-user");
    const inventoryId = form?._selectedInventoryId;
    if (!inventoryId) {
        setMsg(msg, "Комплект не выбран.", false);
        return;
    }
    const fd = new FormData(form);
    try {
        const res = await api("/api/bookings", {
            method: "POST",
            body: {
                rentalDate:    fd.get("rentalDate"),
                durationKey:   fd.get("durationKey"),
                equipmentType: fd.get("equipmentType"),
                includeSkiPass: fd.get("skiPassChoice") === "yes",
                inventoryId
            }
        });
        const sum = res.total ?? res.totalWithVat;
        const rental = res.rentalAmount ?? res.baseAmount ?? 0;
        const skiPass = res.skiPassAmount ?? 0;
        setMsg(msg, `Бронь подтверждена. Заказ №${res.orderId}, прокат: ${rental} ₽, ски-пасс: ${skiPass} ₽, к оплате: ${sum} ₽.`, true);
        resetBookingWizard();
        await loadMyProfile();
    } catch (e) {
        setMsg(msg, e.message, false);
    }
});

on("booking-form-user", "submit", (ev) => {
    ev.preventDefault();
});

(function wireBookingInventoryList() {
    const host = byId("free-list-user");
    if (!host) return;
    host.addEventListener("click", (ev) => {
        if (ev.target.closest(".inv-details")) return;
        const card = ev.target.closest(".inv-card");
        if (!card) return;
        selectInventoryCard(Number(card.dataset.id));
    });
    host.addEventListener("keydown", (ev) => {
        if (ev.key !== "Enter" && ev.key !== " ") return;
        if (ev.target.closest(".inv-details")) return;
        const card = ev.target.closest(".inv-card");
        if (!card) return;
        ev.preventDefault();
        selectInventoryCard(Number(card.dataset.id));
    });
})();

/* ── мои заказы (аренда + ски-пасс) ──────────────────────────────── */
on("btn-history", "click", async () => {
    const wrap = document.getElementById("history-table");
    const open = toggleCollapsed("history-table");
    if (!open) return;
    try {
        renderFixedColumnsTable(wrap, await api("/api/rentals/my"), MY_ORDER_COLS);
    } catch (e) {
        wrap.textContent = e.message;
    }
});

on("btn-fit-guide", "click", () => {
    toggleCollapsed("fit-guide-wrap");
});

on("btn-panel-prices", "click", () => {
    toggleCollapsed("panel-prices");
});

function setSkipassFlowStep(step) {
    _skipassFlowStep = step;
    for (let i = 1; i <= 3; i++) {
        byId(`skipass-flow-step-${i}`)?.classList.toggle("hidden", i !== step);
    }
    document.querySelectorAll("[data-skip-flow-indicator]").forEach((el) => {
        const sn = Number(el.getAttribute("data-skip-flow-indicator"));
        el.classList.toggle("active", sn === step);
        el.classList.toggle("done", sn < step);
    });
    byId("skipass-flow-back")?.classList.toggle("hidden", step === 1);
    byId("skipass-flow-next")?.classList.toggle("hidden", step === 3);
    byId("skipass-flow-submit")?.classList.toggle("hidden", step !== 3);
}

function syncSkipassFlowPanels() {
    const form = byId("skipass-flow-form");
    if (!form) return;
    const lifts = form.querySelector('input[name="skipassMode"]:checked')?.value === "lifts";
    byId("skipass-flow-time-panel")?.classList.toggle("hidden", lifts);
    byId("skipass-flow-lifts-panel")?.classList.toggle("hidden", !lifts);
}

function openSkipassOrderDialog() {
    const dlg = byId("skipass-order-dialog");
    if (!dlg) return;
    const msg = byId("skipass-flow-msg");
    if (msg) {
        msg.textContent = "";
        msg.classList.remove("ok", "err");
    }
    const form = byId("skipass-flow-form");
    form?.reset();
    const timeRadio = form?.querySelector('input[name="skipassMode"][value="time"]');
    if (timeRadio) timeRadio.checked = true;
    syncSkipassFlowPanels();
    setSkipassFlowStep(1);
    dlg.showModal();
}

function buildSkipassSummaryHtml(fd) {
    const form = byId("skipass-flow-form");
    const dayKind = fd.get("dayKind") === "weekend" ? "Выходные и праздники" : "Будни";
    const liftsMode = fd.get("skipassMode") === "lifts";
    const modeLabel = liftsMode ? "По числу подъёмов" : "По времени";
    let detail = "";
    if (liftsMode) {
        const liftsSel = form?.querySelector('select[name="liftCount"]');
        detail =
            liftsSel?.selectedOptions?.[0]?.text?.trim() ||
            `${fd.get("liftCount")} подъёмов`;
    } else {
        const slot = fd.get("timeSlot") || "";
        const slotLabels = { 2: "2 часа", 3: "3 часа", 4: "4 часа", day: "День" };
        detail = slotLabels[slot] || slot;
    }
    return `<dl class="skipass-summary-dl">
      <dt>Тип дня</dt><dd>${escapeHtml(dayKind)}</dd>
      <dt>Вариант</dt><dd>${escapeHtml(modeLabel)}</dd>
      <dt>Параметры</dt><dd>${escapeHtml(detail)}</dd>
    </dl>`;
}

on("btn-open-skipass", "click", () => openSkipassOrderDialog());

on("skipass-dialog-close", "click", () => byId("skipass-order-dialog")?.close());

on("skipass-flow-cancel", "click", () => byId("skipass-order-dialog")?.close());

on("skipass-flow-next", "click", () => {
    const form = byId("skipass-flow-form");
    const msg = byId("skipass-flow-msg");
    if (!form) return;
    if (msg) {
        msg.textContent = "";
        msg.classList.remove("ok", "err");
    }
    const fd = new FormData(form);
    if (_skipassFlowStep === 1) {
        syncSkipassFlowPanels();
        setSkipassFlowStep(2);
        return;
    }
    if (_skipassFlowStep === 2) {
        const sumBox = byId("skipass-flow-summary");
        if (sumBox) sumBox.innerHTML = buildSkipassSummaryHtml(fd);
        setSkipassFlowStep(3);
    }
});

on("skipass-flow-back", "click", () => {
    if (_skipassFlowStep <= 1) return;
    setSkipassFlowStep(_skipassFlowStep - 1);
});

on("skipass-flow-form", "submit", async (ev) => {
    ev.preventDefault();
    const form = ev.target;
    const fd = new FormData(form);
    const msg = byId("skipass-flow-msg");
    const mode = fd.get("skipassMode") === "lifts" ? "lifts" : "time";
    const body = {
        dayKind: fd.get("dayKind"),
        mode,
        timeSlot: mode === "time" ? fd.get("timeSlot") : null,
        liftCount: mode === "lifts" ? Number(fd.get("liftCount")) : null
    };
    try {
        const res = await api("/api/bookings/skipass", { method: "POST", body });
        const mainMsg = byId("booking-msg-user");
        const okText = `Ски-пасс оформлен: заказ №${res.orderId}, к оплате ${res.totalWithVat} ₽`;
        if (mainMsg) setMsg(mainMsg, okText, true);
        if (msg) setMsg(msg, okText, true);
        byId("skipass-order-dialog")?.close();
    } catch (e) {
        if (msg) setMsg(msg, e.message, false);
    }
});

(function wireSkipassFlowRadios() {
    const form = byId("skipass-flow-form");
    if (!form) return;
    form.querySelectorAll('input[name="skipassMode"]').forEach((r) =>
        r.addEventListener("change", syncSkipassFlowPanels));
})();

/* ── админ-панель ────────────────────────────────────────────────── */
on("btn-admin-users", "click", async () => {
    const open = toggleCollapsed("admin-users-wrap");
    if (!open) return;
    try {
        renderAdminUsersTable(document.getElementById("admin-users-table"), await api("/api/admin/users"));
    } catch (e) {
        document.getElementById("admin-users-table").textContent = e.message;
    }
});

on("btn-admin-undo", "click", async () => {
    try {
        await api("/api/admin/users/restore", { method: "POST" });
        await refreshAdminUndoState();
        const tbl = document.getElementById("admin-users-table");
        if (tbl && !tbl.classList.contains("empty"))
            renderAdminUsersTable(tbl, await api("/api/admin/users"));
    } catch (e) {
        alert(e.message);
    }
});

on("btn-admin-rentals", "click", async () => {
    const open = toggleCollapsed("admin-rentals-table");
    if (!open) return;
    try {
        renderFixedColumnsTable(
            document.getElementById("admin-rentals-table"),
            await api("/api/admin/rentals"),
            ADMIN_RENTAL_COLS
        );
    } catch (e) {
        document.getElementById("admin-rentals-table").textContent = e.message;
    }
});

on("btn-admin-catalog", "click", async () => {
    const open = toggleCollapsed("admin-catalog-wrap");
    if (!open) return;
    await loadCatalog(_activeCatalogKind);
});

document.querySelectorAll(".catalog-tab").forEach((btn) => {
    btn.addEventListener("click", async () => {
        const kind = btn.dataset.kind;
        if (!kind) return;
        await loadCatalog(kind);
    });
});

on("catalog-cancel-btn", "click", () => {
    renderCatalogForm(_activeCatalogKind);
});

on("catalog-form", "submit", async (ev) => {
    ev.preventDefault();
    const cfg = CATALOG_CONFIG[_activeCatalogKind];
    const msg = byId("catalog-msg");
    if (!cfg) return;
    const form = ev.target;
    const fd = new FormData(form);
    const editId = byId("catalog-edit-id")?.value || "";
    const body = {};
    cfg.fields.forEach((f) => {
        const v = fd.get(f.key);
        if (f.type === "number")
            body[f.key] = v === "" || v == null ? null : Number(v);
        else
            body[f.key] = v == null ? null : String(v).trim();
    });
    try {
        if (editId)
            await api(`${cfg.endpoint}/${editId}`, { method: "PUT", body });
        else
            await api(cfg.endpoint, { method: "POST", body });
        await loadCatalog(_activeCatalogKind);
        if (msg) setMsg(msg, editId ? "Изменения сохранены." : "Запись добавлена.", true);
    } catch (e) {
        if (msg) setMsg(msg, e.message, false);
    }
});

(function wireCatalogTableActions() {
    const wrap = byId("catalog-table");
    if (!wrap) return;
    wrap.addEventListener("click", async (ev) => {
        const editBtn = ev.target.closest(".btn-catalog-edit");
        const delBtn = ev.target.closest(".btn-catalog-delete");
        if (!editBtn && !delBtn) return;
        const cfg = CATALOG_CONFIG[_activeCatalogKind];
        const msg = byId("catalog-msg");
        if (!cfg) return;
        const id = Number((editBtn || delBtn).dataset.id);
        const rows = JSON.parse(wrap.dataset.rows || "[]");
        const row = rows.find((r) => r.id === id);
        if (!row) return;

        if (editBtn) {
            renderCatalogForm(_activeCatalogKind, row);
            byId("catalog-form")?.scrollIntoView({ behavior: "smooth", block: "center" });
            return;
        }

        if (!confirm("Удалить запись из справочника?")) return;
        try {
            await api(`${cfg.endpoint}/${id}`, { method: "DELETE" });
            await loadCatalog(_activeCatalogKind);
            if (msg) setMsg(msg, "Запись удалена.", true);
        } catch (e) {
            if (msg) setMsg(msg, e.message, false);
        }
    });
})();

on("admin-user-dialog-close", "click", () => {
    byId("admin-user-dialog")?.close();
});

/* ── начальные значения ──────────────────────────────────────────── */
(function defaults() {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    const p = n => String(n).padStart(2, "0");
    const dateValue = `${d.getFullYear()}-${p(d.getMonth()+1)}-${p(d.getDate())}`;
    const bookingDate = document.querySelector('#booking-form-user [name="rentalDate"]');
    if (bookingDate) bookingDate.value = dateValue;
})();

applySessionUI();
