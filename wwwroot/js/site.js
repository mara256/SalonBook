function toggleSidebar() {
    const open = document.getElementById('sidebar').classList.contains('open');
    document.getElementById('sidebar').classList.toggle('open', !open);
    document.getElementById('overlay').classList.toggle('open', !open);
    const ico = document.getElementById('hico');
    if (!open) {
        ico.innerHTML = '<line x1="4" y1="4" x2="18" y2="18" stroke="currentColor" stroke-width="2" stroke-linecap="round"/><line x1="18" y1="4" x2="4" y2="18" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>';
    } else {
        ico.innerHTML = '<rect x="3" y="5" width="16" height="2" rx="1" fill="currentColor"/><rect x="3" y="10" width="16" height="2" rx="1" fill="currentColor"/><rect x="3" y="15" width="16" height="2" rx="1" fill="currentColor"/>';
    }
}

function closeSidebar() {
    document.getElementById('sidebar').classList.remove('open');
    document.getElementById('overlay').classList.remove('open');
    const ico = document.getElementById('hico');
    if (ico) ico.innerHTML = '<rect x="3" y="5" width="16" height="2" rx="1" fill="currentColor"/><rect x="3" y="10" width="16" height="2" rx="1" fill="currentColor"/><rect x="3" y="15" width="16" height="2" rx="1" fill="currentColor"/>';
}

let selectedDate = null;
let selectedTime = null;
let serviciuId = null;

function initBooking(svId) {
    serviciuId = svId;
    const container = document.getElementById('date-container');
    if (!container) return;

    const zile = ['Dum', 'Lun', 'Mar', 'Mie', 'Joi', 'Vin', 'Sam'];
    const azi = new Date();

    for (let i = 0; i < 14; i++) {
        const d = new Date(azi);
        d.setDate(azi.getDate() + i);
        const pill = document.createElement('div');
        pill.className = 'date-pill' + (i === 0 ? ' selected' : '');
        pill.innerHTML = `<div>${zile[d.getDay()]}</div><div class="date-num">${d.getDate()}</div>`;
        pill.dataset.date = d.toISOString().split('T')[0];
        pill.onclick = () => selectDate(pill, d.toISOString().split('T')[0]);
        container.appendChild(pill);
    }

    selectDate(container.querySelector('.date-pill'), azi.toISOString().split('T')[0]);
}

function selectDate(el, date) {
    document.querySelectorAll('.date-pill').forEach(p => p.classList.remove('selected'));
    el.classList.add('selected');
    selectedDate = date;
    selectedTime = null;
    document.getElementById('selected-time').value = '';
    loadOre(date);
}

async function loadOre(date) {
    const container = document.getElementById('time-container');
    container.innerHTML = '<div style="color:#9b9b9b;font-size:12px;padding:8px">Se incarca...</div>';

    try {
        const res = await fetch(`/Booking/OreDisponibile?serviciuId=${serviciuId}&data=${date}`);
        const ore = await res.json();

        container.innerHTML = '';
        if (ore.length === 0) {
            container.innerHTML = '<div style="color:#9b9b9b;font-size:12px;padding:8px">Nicio ora disponibila in aceasta zi.</div>';
            return;
        }

        ore.forEach(ora => {
            const slot = document.createElement('div');
            slot.className = 'time-slot';
            slot.textContent = ora;
            slot.onclick = () => selectTime(slot, ora);
            container.appendChild(slot);
        });
    } catch (e) {
        container.innerHTML = '<div style="color:#A32D2D;font-size:12px;padding:8px">Eroare la incarcare.</div>';
    }
}

function selectTime(el, ora) {
    document.querySelectorAll('.time-slot').forEach(s => s.classList.remove('selected'));
    el.classList.add('selected');
    selectedTime = ora;
    document.getElementById('selected-time').value = selectedDate + ' ' + ora;
    document.getElementById('btn-continua').disabled = false;
}
