window.HCShopApi = (function () {
    const TOKEN_KEY = "hcshop_token";
    const USER_KEY = "hcshop_user";

    function getToken() { return localStorage.getItem(TOKEN_KEY); }
    function getUser() {
        try { return JSON.parse(localStorage.getItem(USER_KEY) || "null"); }
        catch { return null; }
    }
    function setAuth(data) {
        if (data && data.accessToken) localStorage.setItem(TOKEN_KEY, data.accessToken);
        localStorage.setItem(USER_KEY, JSON.stringify({
            taiKhoanId: data.taiKhoanId,
            hoTen: data.hoTen,
            email: data.email,
            vaiTro: data.vaiTro
        }));
        syncHeader();
    }
    function logout() {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        window.location.href = "/XacThuc";
    }

    async function request(url, options) {
        options = options || {};
        const headers = Object.assign({ "Content-Type": "application/json" }, options.headers || {});
        const token = getToken();
        if (token) headers["Authorization"] = "Bearer " + token;

        const res = await fetch(url, Object.assign({}, options, { headers: headers }));
        if (res.status === 401 && !options.ignoreAuthRedirect) {
            logout();
            return null;
        }
        const ct = res.headers.get("content-type") || "";
        if (ct.indexOf("application/json") !== -1) {
            const json = await res.json();
            if (!res.ok || json.success === false) {
                throw new Error(json.message || "Có lỗi xảy ra.");
            }
            return json;
        }
        if (!res.ok) throw new Error("Lỗi máy chủ (" + res.status + ").");
        return await res.text();
    }

    function formatVnd(v) { return Number(v || 0).toLocaleString("vi-VN") + " đ"; }

    function statusBadge(st) {
        const map = {
            ChoXacNhan: ["Chờ xác nhận", "hc-badge-warning"],
            DaXacNhan: ["Đã xác nhận", "hc-badge-neutral"],
            DangChuanBi: ["Đang chuẩn bị", "hc-badge-neutral"],
            DangGiao: ["Đang giao", "hc-badge-warning"],
            HoanThanh: ["Hoàn thành", "hc-badge-success"],
            DaHuy: ["Đã hủy", "hc-badge-danger"],
            HoatDong: ["Hoạt động", "hc-badge-success"],
            BiKhoa: ["Bị khóa", "hc-badge-danger"],
            DuHang: ["Đủ hàng", "hc-badge-success"],
            SapHet: ["Sắp hết hàng", "hc-badge-warning"],
            HetHang: ["Hết hàng", "hc-badge-danger"]
        };
        const item = map[st] || [st, "hc-badge-neutral"];
        return `<span class="badge rounded-pill ${item[1]}">${item[0]}</span>`;
    }

    async function refreshCartBadge() {
        const u = getUser();
        const badge = document.getElementById("hc-cart-badge");
        if (!badge || !u || u.vaiTro !== "KhachHang") return;
        try {
            const res = await request("/api/gio-hang", { ignoreAuthRedirect: true });
            if (res && res.data) badge.textContent = res.data.tongSoLuongKhaDung || 0;
        } catch { }
    }

    function syncHeader() {
        const u = getUser();
        const nameEl = document.getElementById("hc-user-name");
        const emailEl = document.getElementById("hc-user-email");
        const avatarEl = document.getElementById("hc-user-avatar");
        const portalLink = document.getElementById("hc-portal-menu-item");
        const authLink = document.getElementById("hc-auth-menu-item");
        const logoutBtn = document.getElementById("hc-logout-menu-item");
        const greetingEl = document.getElementById("hc-hero-greeting");

        if (u) {
            if (nameEl) nameEl.textContent = u.hoTen;
            if (emailEl) emailEl.textContent = u.email;
            if (avatarEl) avatarEl.textContent = (u.hoTen || "U").substring(0, 1).toUpperCase();
            if (greetingEl) greetingEl.textContent = "Xin chào, " + u.hoTen;
            if (authLink) authLink.classList.add("d-none");
            if (logoutBtn) logoutBtn.classList.remove("d-none");
            if (portalLink) {
                if (u.vaiTro === "QuanLy" || u.vaiTro === "NhanVien") portalLink.classList.remove("d-none");
                else portalLink.classList.add("d-none");
            }
            refreshCartBadge();
        } else {
            if (nameEl) nameEl.textContent = "Khách vãng lai";
            if (emailEl) emailEl.textContent = "Đăng nhập / Đăng ký";
            if (avatarEl) avatarEl.textContent = "G";
            if (greetingEl) greetingEl.textContent = "Chào mừng đến với HCShop";
            if (authLink) authLink.classList.remove("d-none");
            if (logoutBtn) logoutBtn.classList.add("d-none");
            if (portalLink) portalLink.classList.add("d-none");
        }
    }


    async function uploadImage(file, folder) {
        if (!file) throw new Error("Chưa chọn tệp ảnh.");
        const formData = new FormData();
        formData.append("file", file);
        const headers = {};
        const token = getToken();
        if (token) headers["Authorization"] = "Bearer " + token;

        const res = await fetch("/api/upload/hinh-anh?folder=" + encodeURIComponent(folder || "products"), {
            method: "POST",
            headers: headers,
            body: formData
        });
        const json = await res.json();
        if (!res.ok || json.success === false) {
            throw new Error(json.message || "Tải ảnh thất bại.");
        }
        return json.data.url;
    }

    document.addEventListener("DOMContentLoaded", syncHeader);
    return { getToken, getUser, setAuth, logout, request, uploadImage, formatVnd, statusBadge, refreshCartBadge, syncHeader };
})();
