// ─── Thai Date Pickers (Flatpickr) ───────────────────────────────────────────
(function initThaiDatePickers() {
    var MONTHS_LONG  = ['มกราคม','กุมภาพันธ์','มีนาคม','เมษายน','พฤษภาคม','มิถุนายน',
                        'กรกฎาคม','สิงหาคม','กันยายน','ตุลาคม','พฤศจิกายน','ธันวาคม'];
    var MONTHS_SHORT = ['ม.ค.','ก.พ.','มี.ค.','เม.ย.','พ.ค.','มิ.ย.',
                        'ก.ค.','ส.ค.','ก.ย.','ต.ค.','พ.ย.','ธ.ค.'];
    var DAYS_SHORT   = ['อา.','จ.','อ.','พ.','พฤ.','ศ.','ส.'];
    var DAYS_LONG    = ['อาทิตย์','จันทร์','อังคาร','พุธ','พฤหัสบดี','ศุกร์','เสาร์'];

    function toBE(date) {
        return date.getDate() + ' ' + MONTHS_LONG[date.getMonth()] + ' ' + (date.getFullYear() + 543);
    }

    document.addEventListener('DOMContentLoaded', function () {
        if (typeof flatpickr === 'undefined') return;

        document.querySelectorAll('[data-datepicker]').forEach(function (el) {
            var originalClass = el.getAttribute('class') || '';
            var maxDate = el.getAttribute('max') || undefined;
            var minDate = el.getAttribute('min') || undefined;

            function showBE(fp) {
                if (fp.selectedDates.length > 0 && fp.altInput) {
                    fp.altInput.value = toBE(fp.selectedDates[0]);
                }
            }

            flatpickr(el, {
                locale: {
                    months:      { longhand: MONTHS_LONG, shorthand: MONTHS_SHORT },
                    weekdays:    { longhand: DAYS_LONG,   shorthand: DAYS_SHORT },
                    firstDayOfWeek: 0,
                    rangeSeparator: ' ถึง '
                },
                dateFormat:    'Y-m-d',
                altInput:      true,
                altInputClass: originalClass,
                altFormat:     'j F Y',   // fallback; overridden by showBE
                maxDate:       maxDate,
                minDate:       minDate,
                onReady:       function (_, __, fp) {
                    showBE(fp);
                    if (fp.altInput) fp.altInput.placeholder = 'วัน เดือน ปี (พ.ศ.)';
                },
                onChange:      function (_, __, fp) { showBE(fp); }
            });
        });
    });
})();
// ─────────────────────────────────────────────────────────────────────────────

// ─── Input Guards ────────────────────────────────────────────────────────────
// data-block="thai"        → ห้ามพิมพ์อักขระไทย (U+0E00–U+0E7F)
// data-block="digits-only" → รับเฉพาะตัวเลข 0-9
(function initInputGuards() {
    var THAI = /[฀-๿]/;

    function applyThaiBlock(el) {
        el.addEventListener('keydown', function (e) {
            if (e.key.length === 1 && THAI.test(e.key)) e.preventDefault();
        });
        el.addEventListener('input', function () {
            var clean = this.value.replace(/[฀-๿]/g, '');
            if (clean !== this.value) {
                var pos = this.selectionStart - (this.value.length - clean.length);
                this.value = clean;
                this.setSelectionRange(pos, pos);
            }
        });
    }

    function applyDigitsOnly(el) {
        var NAV = new Set(['Backspace','Delete','Tab','Enter',
                           'ArrowLeft','ArrowRight','ArrowUp','ArrowDown','Home','End']);
        el.addEventListener('keydown', function (e) {
            if (NAV.has(e.key) || e.ctrlKey || e.metaKey) return;
            if (!/^\d$/.test(e.key)) e.preventDefault();
        });
        el.addEventListener('input', function () {
            var clean = this.value.replace(/\D/g, '');
            if (clean !== this.value) this.value = clean;
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-block="thai"]').forEach(applyThaiBlock);
        document.querySelectorAll('[data-block="digits-only"]').forEach(applyDigitsOnly);
    });
})();
// ─────────────────────────────────────────────────────────────────────────────

// Auto-dismiss flash alerts after 6 seconds
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[role="alert"]').forEach(el => {
        setTimeout(() => {
            el.style.transition = 'opacity 0.4s';
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 400);
        }, 6000);
    });
});

// HTMX: re-run alert dismiss on every htmx:afterSwap
document.addEventListener('htmx:afterSwap', (e) => {
    e.detail.elt.querySelectorAll('[role="alert"]').forEach(el => {
        setTimeout(() => {
            el.style.transition = 'opacity 0.4s';
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 400);
        }, 6000);
    });
});
