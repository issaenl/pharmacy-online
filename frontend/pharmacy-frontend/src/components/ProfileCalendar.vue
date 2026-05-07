<template>
  <div class="calendar-wrapper">
    
    <div class="calendar-header card">
      <div class="header-top">
        <div class="title-block">
          <h2>{{ viewMode === 'list' ? 'Мои курсы' : 'Календарь приема' }}</h2>
          
          <div class="month-controls" v-if="viewMode === 'calendar' && !isAdding">
            <span class="month-label">{{ currentMonthLabel }}</span>
            <button class="btn-today" @click="goToToday">Сегодня</button>
          </div>
        </div>
        
        <div class="header-actions">
          <button class="btn-outline" @click="toggleView">
            {{ viewMode === 'list' ? '← В календарь' : 'Управление курсами' }}
          </button>
          <button class="btn-primary" @click="openAddForm" v-if="!isAdding && viewMode === 'calendar'">
            + Добавить курс
          </button>
        </div>
      </div>

      <div class="calendar-with-nav" v-if="!isAdding && viewMode === 'calendar'">
        
        <button class="side-nav-btn" @click="changeMonth(-1)" title="Предыдущий месяц">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="15 18 9 12 15 6"></polyline>
          </svg>
        </button>

        <div 
          class="month-grid-container" 
          @touchstart="handleTouchStart"
          @touchend="handleTouchEnd"
          @wheel="handleWheel"
        >
          <div class="weekdays-row">
            <div v-for="d in ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс']" :key="d" class="weekday-label">{{ d }}</div>
          </div>
          
          <div class="days-grid">
            <div 
              v-for="day in calendarGrid" 
              :key="day.dateString"
              class="day-cell"
              :class="{ 
                'is-selected': day.dateString === selectedDateString, 
                'is-today': day.isToday,
                'is-other-month': !day.isCurrentMonth
              }"
              @click="selectDate(day.date)"
            >
              <span class="day-number">{{ day.dayNumber }}</span>
              <div v-if="hasEvents(day.date)" class="event-dot" :class="{'other-month-dot': !day.isCurrentMonth}"></div>
            </div>
          </div>
        </div>

        <button class="side-nav-btn" @click="changeMonth(1)" title="Следующий месяц">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="9 18 15 12 9 6"></polyline>
          </svg>
        </button>

      </div>
    </div>

    <div class="add-form card" v-if="isAdding">
      <h3>{{ editingId ? 'Редактирование курса' : 'Новое напоминание' }}</h3>
      <form @submit.prevent="submitReminder">
        <div class="form-grid">
          <div class="form-group">
            <label>Название лекарства</label>
            <input type="text" v-model="form.medicationName" required placeholder="Например: Энтерол">
          </div>
          <div class="form-group">
            <label>Дозировка</label>
            <input type="text" v-model="form.dosage" placeholder="Например: 1 капсула 250мг">
          </div>
          <div class="form-group">
            <label>Начало курса</label>
            <input type="date" v-model="form.startDate" required>
          </div>
          <div class="form-group">
            <label>Конец курса</label>
            <input type="date" v-model="form.endDate" required>
          </div>
          
          <div class="form-group">
            <label>Частота приема</label>
            <select v-model="form.frequency">
              <option :value="0">Каждый день</option>
              <option :value="1">С интервалом в днях</option>
              <option :value="2">По дням недели</option>
            </select>
          </div>

          <div class="form-group" v-if="form.frequency === 1">
            <label>Раз в сколько дней?</label>
            <input type="number" v-model="form.intervalDays" min="1">
          </div>

          <div class="form-group full-width" v-if="form.frequency === 2">
            <label>Дни недели</label>
            <div class="weekdays-selector">
              <button 
                type="button" 
                v-for="day in weekDaysDef" 
                :key="day.value"
                class="weekday-btn"
                :class="{ 'active': form.selectedDays.includes(day.value) }"
                @click="toggleDay(day.value)"
              >
                {{ day.label }}
              </button>
            </div>
          </div>

          <div class="form-group full-width">
            <label>Время приема</label>
            <div class="time-inputs-list">
              <div v-for="(time, index) in form.timesOfDay" :key="index" class="time-input-row">
                <input type="time" v-model="form.timesOfDay[index]" required class="time-picker">
                <button type="button" class="btn-remove-time" @click="removeTime(index)" v-if="form.timesOfDay.length > 1">✕</button>
              </div>
              <button type="button" class="btn-add-time" @click="addTime">+ Добавить время</button>
            </div>

            <div class="form-group full-width notification-option">
              <label class="checkbox-container">
                <input type="checkbox" v-model="form.remindToBuy">
                <span class="checkmark"></span>
                <span class="label-text">Напомнить купить лекарство</span>
              </label>

              <div v-if="form.remindToBuy" class="remind-buy-settings">
                <div class="radio-group">
                  <label class="radio-label">
                    <input type="radio" v-model="form.remindToBuyMethod" :value="1">
                    По окончанию курса
                  </label>
                  <label class="radio-label">
                    <input type="radio" v-model="form.remindToBuyMethod" :value="2">
                    Рассчитать по количеству таблеток
                  </label>
                </div>

                <div v-if="form.remindToBuyMethod === 2" class="calc-grid">
                  <div class="form-group">
                    <label>Количество упаковок</label>
                    <input type="number" v-model="form.packQuantity" min="1">
                  </div>
                  <div class="form-group">
                    <label>В одной упаковке</label>
                    <input type="number" v-model="form.pillsPerPack" min="1">
                  </div>
                  <div class="form-group">
                    <label>Прием в день (штук)</label>
                    <input type="number" step="0.5" v-model="form.pillsPerDay" min="0.1">
                  </div>
                  
                  <div class="calc-result" v-if="calculatedDays > 0">
                    <template v-if="hasEnoughPillsForCourse">
                      <strong>Лекарства хватит на весь курс</strong><br>
                      <span class="small-text">Покупать новую упаковку не потребуется (запаса хватит на {{ calculatedDays }} дн.)</span>
                    </template>
                    
                    <template v-else>
                      Лекарства хватит на: <strong>{{ calculatedDays }} дн.</strong><br>
                      <span class="small-text">Напомним за 3 дня до {{ calculatedRunOutDate }}</span>
                    </template>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-cancel" @click="cancelEdit">Отмена</button>
          <button type="submit" class="btn-primary" :disabled="isLoading">Сохранить</button>
        </div>
      </form>
    </div>

    <div class="courses-list" v-else-if="viewMode === 'list'">
      <div v-if="reminders.length === 0" class="empty-state card">У вас пока нет курсов.</div>
      <div class="course-item card" v-for="r in reminders" :key="r.id">
        <div class="course-info">
          <h4>{{ r.medicationName }}</h4>
          <p class="dosage-text">{{ r.dosage }}</p>
          <div class="course-meta">
            <span class="meta-tag">С {{ formatDateRu(r.startDate) }} по {{ formatDateRu(r.endDate) }}</span>
            <span class="meta-tag">{{ formatFrequency(r) }}</span>
          </div>
        </div>
        <div class="course-actions">
          <button class="btn-edit" @click="editReminder(r)">Редактировать</button>
          <button class="btn-delete" @click="deleteReminder(r.id)">Удалить</button>
        </div>
      </div>
    </div>

    <div class="schedule-list" v-else>
      <h3 class="schedule-title">План на {{ formattedSelectedDate }}</h3>
      <div v-if="scheduleForSelectedDay.length === 0" class="empty-state">Нет приемов на этот день.</div>
      <div v-else class="schedule-item card" v-for="item in scheduleForSelectedDay" :key="item.reminder.id">
        <div class="med-info">
          <h4>{{ item.reminder.medicationName }}</h4>
          <p class="dosage-text">{{ item.reminder.dosage }}</p>
        </div>
        <div class="times-list">
          <div 
            class="time-pill" 
            v-for="time in item.times" 
            :key="time"
            :class="{ 'is-taken': isTaken(item.reminder, time) }"
            @click="toggleTake(item.reminder, time)"
          >
            <div class="pill-check">
              <svg v-if="isTaken(item.reminder, time)" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>
            </div>
            <span class="pill-time">{{ time }}</span>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });
const reminders = ref([]);
const isLoading = ref(false);
const viewMode = ref('calendar');
const isAdding = ref(false);
const editingId = ref(null);

let touchStartX = 0;
const handleTouchStart = (e) => {
  touchStartX = e.changedTouches[0].screenX;
};
const handleTouchEnd = (e) => {
  const touchEndX = e.changedTouches[0].screenX;
  if (touchStartX - touchEndX > 50) {
    changeMonth(1); 
  } else if (touchEndX - touchStartX > 50) {
    changeMonth(-1); 
  }
};

let isThrottled = false;
const handleWheel = (e) => {
  if (Math.abs(e.deltaX) > Math.abs(e.deltaY)) {
    e.preventDefault();
    if (isThrottled) return;
    if (e.deltaX > 20) { changeMonth(1); throttleScroll(); }
    else if (e.deltaX < -20) { changeMonth(-1); throttleScroll(); }
  }
};
const throttleScroll = () => {
  isThrottled = true;
  setTimeout(() => { isThrottled = false; }, 300);
};

const selectedDate = ref(new Date());
selectedDate.value.setHours(0,0,0,0);
const viewDate = ref(new Date(selectedDate.value.getFullYear(), selectedDate.value.getMonth(), 1));

const weekDaysDef = [
  { label: 'Пн', value: 'Monday' }, { label: 'Вт', value: 'Tuesday' }, { label: 'Ср', value: 'Wednesday' },
  { label: 'Чт', value: 'Thursday' }, { label: 'Пт', value: 'Friday' }, { label: 'Сб', value: 'Saturday' }, { label: 'Вс', value: 'Sunday' }
];

const currentMonthLabel = computed(() => {
  return viewDate.value.toLocaleDateString('ru-RU', { month: 'long', year: 'numeric' });
});


const calendarGrid = computed(() => {
  const days = [];
  const year = viewDate.value.getFullYear();
  const month = viewDate.value.getMonth();
  const firstDayOfMonth = new Date(year, month, 1);
  const lastDayOfMonth = new Date(year, month + 1, 0);

  let firstDayWeekday = firstDayOfMonth.getDay();
  if (firstDayWeekday === 0) firstDayWeekday = 7;
  const todayStr = formatDateString(new Date());

  const prevMonthDays = firstDayWeekday - 1;
  const prevMonthLastDate = new Date(year, month, 0).getDate();
  for (let i = prevMonthDays; i > 0; i--) {
    const d = new Date(year, month - 1, prevMonthLastDate - i + 1);
    days.push({ date: d, dateString: formatDateString(d), dayNumber: d.getDate(), isToday: formatDateString(d) === todayStr, isCurrentMonth: false });
  }

  const totalDays = lastDayOfMonth.getDate();
  for (let i = 1; i <= totalDays; i++) {
    const d = new Date(year, month, i);
    days.push({ date: d, dateString: formatDateString(d), dayNumber: i, isToday: formatDateString(d) === todayStr, isCurrentMonth: true });
  }

  const remainingCells = (7 - (days.length % 7)) % 7;
  for (let i = 1; i <= remainingCells; i++) {
    const d = new Date(year, month + 1, i);
    days.push({ date: d, dateString: formatDateString(d), dayNumber: i, isToday: formatDateString(d) === todayStr, isCurrentMonth: false });
  }
  return days;
});

const selectedDateString = computed(() => formatDateString(selectedDate.value));
const formattedSelectedDate = computed(() => {
  return selectedDate.value.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' });
});

const changeMonth = (delta) => {
  viewDate.value = new Date(viewDate.value.getFullYear(), viewDate.value.getMonth() + delta, 1);
};

const goToToday = () => {
  const now = new Date();
  now.setHours(0,0,0,0);
  selectedDate.value = now;
  viewDate.value = new Date(now.getFullYear(), now.getMonth(), 1);
};

const selectDate = (date) => {
  selectedDate.value = new Date(date);
  if (date.getMonth() !== viewDate.value.getMonth()) {
    viewDate.value = new Date(date.getFullYear(), date.getMonth(), 1);
  }
};

const hasEvents = (date) => {
  const dateStr = formatDateString(date);
  const targetTime = getSafeTimestamp(dateStr);
  const targetDayName = date.toLocaleDateString('en-US', { weekday: 'long' });

  return reminders.value.some(r => {
    const start = getSafeTimestamp(r.startDate);
    const end = getSafeTimestamp(r.endDate);
    if (targetTime < start || targetTime > end) return false;
    if (r.frequency === 0) return true;
    if (r.frequency === 1) return (Math.floor(Math.abs(targetTime - start) / 86400000)) % r.intervalDays === 0;
    if (r.frequency === 2) return r.daysOfWeek?.includes(targetDayName);
    return false;
  });
};

const form = ref({
  medicationName: '', 
  dosage: '', 
  startDate: formatDateString(new Date()),
  endDate: formatDateString(new Date(new Date().setDate(new Date().getDate() + 14))),
  frequency: 0, 
  intervalDays: 2, 
  selectedDays: ['Monday'], 
  timesOfDay: ['08:00'],
  remindToBuy: false,
  remindToBuyMethod: 1,
  packQuantity: 1,      
  pillsPerPack: 20,     
  pillsPerDay: 1       
});

onMounted(fetchReminders);
async function fetchReminders() { 
  try { const res = await api.get('/Reminders'); reminders.value = res.data; } catch(e) { toast.error('Ошибка данных'); } 
}

const toggleView = () => { viewMode.value = viewMode.value === 'calendar' ? 'list' : 'calendar'; isAdding.value = false; };
const openAddForm = () => { isAdding.value = true; editingId.value = null; };
const cancelEdit = () => { isAdding.value = false; editingId.value = null; };
const addTime = () => form.value.timesOfDay.push('12:00');
const removeTime = (index) => form.value.timesOfDay.splice(index, 1);
const toggleDay = (val) => {
  const idx = form.value.selectedDays.indexOf(val);
  if (idx > -1) form.value.selectedDays.splice(idx, 1); else form.value.selectedDays.push(val);
};

const calculatedDays = computed(() => {
  if (form.value.packQuantity && form.value.pillsPerPack && form.value.pillsPerDay) {
    return Math.floor((form.value.packQuantity * form.value.pillsPerPack) / form.value.pillsPerDay);
  }
  return 0;
});

const calculatedRunOutDate = computed(() => {
  if (calculatedDays.value > 0 && form.value.startDate) {
    const d = new Date(form.value.startDate);
    d.setDate(d.getDate() + calculatedDays.value);
    return d.toLocaleDateString('ru-RU');
  }
  return '';
});

const hasEnoughPillsForCourse = computed(() => {
  if (calculatedDays.value > 0 && form.value.startDate && form.value.endDate) {
    const runOut = new Date(form.value.startDate);
    runOut.setDate(runOut.getDate() + calculatedDays.value);
    
    const courseEnd = new Date(form.value.endDate);
    runOut.setHours(0,0,0,0);
    courseEnd.setHours(0,0,0,0);
    
    return runOut >= courseEnd;
  }
  return false;
});

const submitReminder = async () => {
  isLoading.value = true;
  try {
    const payload = { 
      ...form.value, 
      timesOfDay: form.value.timesOfDay.join(', '), 
      daysOfWeek: form.value.selectedDays.join(','),
      remindToBuy: form.value.remindToBuy
    };
    if (editingId.value) await api.put(`/Reminders/${editingId.value}`, payload);
    else await api.post('/Reminders', payload);
    toast.success('Сохранено'); isAdding.value = false; fetchReminders();
  } catch(e) { toast.error('Ошибка'); } finally { isLoading.value = false; }
};

const editReminder = (r) => {
  editingId.value = r.id;
  form.value = {
    medicationName: r.medicationName, 
    dosage: r.dosage, 
    startDate: formatDateString(r.startDate), 
    endDate: formatDateString(r.endDate),
    frequency: r.frequency, 
    intervalDays: r.intervalDays || 2, 
    selectedDays: r.daysOfWeek?.split(',') || [],
    timesOfDay: r.timesOfDay.split(',').map(t => t.trim()), 
    remindToBuy: r.remindToBuy || false,
    remindToBuyMethod: r.remindToBuyMethod || 1,
    packQuantity: r.packQuantity || 1,
    pillsPerPack: r.pillsPerPack || 20,
    pillsPerDay: r.pillsPerDay || 1,
  };
  isAdding.value = true;
};

const deleteReminder = async (id) => {
  if (!confirm('Удалить?')) return;
  try { await api.delete(`/Reminders/${id}`); toast.success('Удалено'); fetchReminders(); } catch(e) { toast.error('Ошибка'); }
};

const toggleTake = async (reminder, time) => {
  const taken = isTaken(reminder, time);
  const dateStr = selectedDateString.value;
  try {
    if (taken) await api.delete(`/Reminders/${reminder.id}/untake?date=${dateStr}&time=${time}`);
    else await api.post(`/Reminders/${reminder.id}/take?date=${dateStr}&time=${time}`);
    fetchReminders();
  } catch(e) { toast.error('Ошибка'); }
};

const isTaken = (reminder, time) => {
  return reminder.logs.some(l => l.takenDate.split('T')[0] === selectedDateString.value && l.scheduledTime === time);
};

function formatDateString(date) {
  const d = new Date(date); d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
  return d.toISOString().split('T')[0];
}

function getSafeTimestamp(dateStr) {
  const [y, m, d] = dateStr.split('T')[0].split('-');
  return new Date(y, m - 1, d).getTime();
}

function formatDateRu(dateStr) { return new Date(dateStr).toLocaleDateString('ru-RU'); }
function formatFrequency(r) {
  if (r.frequency === 0) return 'Ежедневно';
  if (r.frequency === 1) return `Раз в ${r.intervalDays} дн.`;
  return 'По дням недели';
}

const scheduleForSelectedDay = computed(() => {
  const targetTime = getSafeTimestamp(selectedDateString.value);
  const targetDayName = selectedDate.value.toLocaleDateString('en-US', { weekday: 'long' });
  return reminders.value.filter(r => {
    const start = getSafeTimestamp(r.startDate); const end = getSafeTimestamp(r.endDate);
    if (targetTime < start || targetTime > end) return false;
    if (r.frequency === 0) return true;
    if (r.frequency === 1) return (Math.floor(Math.abs(targetTime - start) / 86400000)) % r.intervalDays === 0;
    if (r.frequency === 2) return r.daysOfWeek?.includes(targetDayName);
    return false;
  }).map(r => ({ reminder: r, times: r.timesOfDay.split(',').map(t => t.trim()).sort() }));
});
</script>

<style scoped>
.calendar-wrapper { display: flex; flex-direction: column; gap: 20px; font-family: var(--main-font); }
.card { background: white; border-radius: 20px; padding: 25px; box-shadow: 0 4px 15px rgba(0,0,0,0.03); }

.header-top { 
  display: flex; 
  justify-content: space-between; 
  align-items: flex-start; 
  margin-bottom: 25px; 
  flex-wrap: wrap; 
  gap: 15px; 
}
.title-block { display: flex; flex-direction: column; gap: 10px; }
.title-block h2 { margin: 0; font-size: 24px; }

.month-controls { 
  display: flex; 
  align-items: center; 
  gap: 15px; 
}
.month-label { font-weight: 700; font-size: 18px; color: #333; text-transform: capitalize; }
.btn-today { 
  background: white; 
  border: 2px solid #eee; 
  padding: 6px 12px; 
  border-radius: 10px; 
  font-size: 13px; 
  font-weight: 600; 
  color: #555; 
  cursor: pointer; 
  transition: 0.2s; 
  font-family: inherit; 
}
.btn-today:hover { border-color: #689D6D; color: #689D6D; }

.header-actions { display: flex; gap: 10px; }
.btn-primary { background: #BB4E58; color: white; border: none; padding: 10px 20px; border-radius: 12px; font-weight: 600; cursor: pointer; transition: 0.2s; font-family: inherit; }
.btn-primary:hover:not(:disabled) { filter: brightness(0.9); }
.btn-outline { background: transparent; color: #689D6D; border: 2px solid #689D6D; padding: 8px 18px; border-radius: 12px; font-weight: 600; cursor: pointer; transition: 0.2s; font-family: inherit; }
.btn-outline:hover { background: #E8F4EA; }

.calendar-with-nav {
  display: flex;
  align-items: center;
  gap: 15px;
}

.side-nav-btn {
  background: white;
  border: 1px solid #eee;
  border-radius: 12px;
  width: 45px;
  height: 45px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #888;
  transition: 0.2s;
  box-shadow: 0 2px 5px rgba(0,0,0,0.02);
  flex-shrink: 0;
}
.side-nav-btn:hover { background: #E8F4EA; color: #689D6D; border-color: #689D6D; }

.month-grid-container { 
  flex: 1;
  display: flex; 
  flex-direction: column; 
  gap: 10px; 
}

.weekdays-row { display: grid; grid-template-columns: repeat(7, 1fr); text-align: center; }
.weekday-label { font-size: 13px; color: #aaa; font-weight: 600; padding-bottom: 5px; }

.days-grid { display: grid; grid-template-columns: repeat(7, 1fr); gap: 4px; }

.day-cell { 
  display: flex; 
  flex-direction: column; 
  align-items: center; 
  justify-content: center; 
  border-radius: 10px; 
  cursor: pointer; 
  transition: 0.2s; 
  position: relative; 
  border: 2px solid transparent; 
  aspect-ratio: 1; 
  max-width: 50px; 
  max-height: 50px;
  margin: 0 auto;
  width: 100%;
}

.day-cell:hover { background: #f0f0f0; }
.day-number { font-size: 15px; font-weight: 600; }
.day-cell.is-today { color: #BB4E58; font-weight: 800; }
.day-cell.is-selected { background: #E8F4EA; border-color: #689D6D; color: #689D6D; }
.day-cell.is-other-month { color: #ccc; }
.day-cell.is-other-month:hover { background: #f9f9f9; }

.event-dot { 
  width: 4px; 
  height: 4px; 
  background: #689D6D; 
  border-radius: 50%; 
  position: absolute; 
  bottom: 4px; 
}
.event-dot.other-month-dot { background: #ccc; }

.notification-option {
  margin-top: 10px;
  padding: 15px;
  background: #f9fafb;
  border-radius: 12px;
  border: 1px dashed #ddd;
}

.checkbox-container {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  font-size: 14px;
  color: #555;
  user-select: none;
}

.checkbox-container input {
  width: 20px;
  height: 20px;
  cursor: pointer;
  accent-color: #689D6D;
}

.label-text {
  line-height: 1.4;
  font-weight: 500;
}

.schedule-title { margin: 0 0 15px 0; font-size: 20px; color: #333; }
.empty-state { text-align: center; padding: 40px; color: #888; font-size: 16px; }
.schedule-item { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }
.med-info h4 { margin: 0 0 5px 0; font-size: 18px; }
.dosage-text { margin: 0; color: #888; font-size: 14px; }
.times-list { display: flex; gap: 15px; }
.time-pill { display: flex; flex-direction: column; align-items: center; gap: 5px; cursor: pointer; }
.pill-check { width: 36px; height: 36px; border-radius: 50%; border: 2px solid #ddd; display: flex; align-items: center; justify-content: center; transition: 0.2s; background: white; }
.time-pill:hover .pill-check { border-color: #689D6D; }
.pill-time { font-size: 12px; font-weight: 700; color: #666; }
.time-pill.is-taken .pill-check { background: #689D6D; border-color: #689D6D; }

.courses-list { display: flex; flex-direction: column; gap: 15px; }
.course-item { display: flex; justify-content: space-between; align-items: center; padding: 20px; }
.course-info h4 { margin: 0 0 5px 0; font-size: 18px;}
.course-meta { display: flex; gap: 10px; margin-top: 10px; flex-wrap: wrap; }
.meta-tag { background: #f0f0f0; padding: 4px 10px; border-radius: 6px; font-size: 13px; color: #555; font-weight: 500; }
.course-actions { display: flex; gap: 10px; }
.btn-edit { background: transparent; color: #689D6D; border: 1px solid #689D6D; padding: 8px 15px; border-radius: 10px; cursor: pointer; font-weight: 600; font-family: inherit; }
.btn-edit:hover { background: #E8F4EA; }
.btn-delete { background: transparent; color: #BB4E58; border: 1px solid #BB4E58; padding: 8px 15px; border-radius: 10px; cursor: pointer; font-weight: 600; font-family: inherit; }
.btn-delete:hover { background: #FDE8E8; }

.btn-cancel { background: #f0f0f0; color: #333; border: none; padding: 10px 20px; border-radius: 12px; font-weight: 600; cursor: pointer; transition: 0.2s; font-family: inherit; }
.btn-cancel:hover { background: #e4e4e4; }
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 20px; }
.form-group { display: flex; flex-direction: column; gap: 5px; }
.full-width { grid-column: span 2; }
.form-group label { font-size: 14px; font-weight: 600; color: #555; }
.form-group input, .form-group select { padding: 10px; border: 1px solid #ddd; border-radius: 10px; outline: none; font-family: inherit; }
.form-group input:focus, .form-group select:focus { border-color: #689D6D; }
.weekdays-selector { display: flex; gap: 8px; flex-wrap: wrap; }
.weekday-btn { width: 40px; height: 40px; border-radius: 50%; border: 1px solid #ddd; cursor: pointer; background: white; font-weight: 600; color: #555; font-family: inherit; }
.weekday-btn.active { background: #E8F4EA; border-color: #689D6D; color: #689D6D; }
.time-inputs-list { display: flex; flex-direction: column; gap: 10px; }
.time-input-row { display: flex; gap: 10px; align-items: center; }
.time-picker { flex: 1; max-width: 150px; }
.btn-remove-time { background: #FDE8E8; color: #BB4E58; border: none; width: 38px; height: 38px; border-radius: 10px; cursor: pointer; }
.btn-add-time { background: transparent; border: 2px dashed #ddd; color: #888; padding: 10px; border-radius: 10px; cursor: pointer; font-weight: 600; font-family: inherit; width: fit-content; }
.form-actions { display: flex; gap: 10px; justify-content: flex-end; }

.remind-buy-settings {
  margin-top: 15px;
  padding-top: 15px;
  border-top: 1px solid #eaeaea;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.radio-group {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.radio-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #444;
  cursor: pointer;
}

.radio-label input[type="radio"] {
  accent-color: #689D6D;
  width: 16px;
  height: 16px;
  cursor: pointer;
}

.calc-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
  background: #fff;
  padding: 15px;
  border-radius: 10px;
  border: 1px solid #eaeaea;
}

.calc-result {
  grid-column: span 3;
  margin-top: 5px;
  padding: 10px;
  background: #E8F4EA;
  color: #2c5e33;
  border-radius: 8px;
  font-size: 14px;
  text-align: center;
  transition: 0.3s;
}

.calc-result strong {
  font-size: 16px;
}

.small-text {
  font-size: 12px;
  opacity: 0.8;
}

@media (max-width: 600px) {
  .header-top { flex-direction: column; align-items: stretch; }
  .header-actions { display: flex; width: 100%; }
  .btn-outline, .btn-primary { flex: 1; text-align: center; }
  .month-controls { justify-content: space-between; width: 100%; }
  
  .calendar-with-nav { gap: 5px; }
  .side-nav-btn { width: 35px; height: 35px; }
  
  .schedule-item { flex-direction: column; align-items: flex-start; gap: 15px; }
  .course-item { flex-direction: column; align-items: flex-start; gap: 15px; }
  .course-actions { width: 100%; display: flex; }
  .btn-edit, .btn-delete { flex: 1; text-align: center; }
  .form-grid { grid-template-columns: 1fr; }
  .full-width { grid-column: span 1; }
  .calc-grid {
    grid-template-columns: 1fr;
  }
  .calc-result {
    grid-column: span 1;
  }
}
</style>