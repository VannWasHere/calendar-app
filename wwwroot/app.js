window.roomBook = {
  toggleTheme() {
    const root = document.documentElement;
    const next = root.dataset.theme === 'dark' ? 'light' : 'dark';
    root.dataset.theme = next;
    localStorage.setItem('roomBook.theme', next);
  },
  toggleMenu() { document.getElementById('app-sidebar')?.classList.toggle('open'); },
  toggleSidebarCollapse() { document.querySelector('.app-shell')?.classList.toggle('sidebar-collapsed'); },
  command: {
    open() { const dialog = document.getElementById('command-palette'); if (dialog && !dialog.open) { dialog.showModal(); setTimeout(() => dialog.querySelector('input')?.focus(), 10); } },
    close() { document.getElementById('command-palette')?.close(); }
  },
  scheduler: {
    init(root, dotnet) {
      if (!root || root.dataset.ready) return;
      root.dataset.ready = '1';
      const scroll = root.querySelector('.scheduler-scroll');
      const startDay = 480, endDay = 1200, ppm = 1.2;
      scroll.addEventListener('scroll', () => { const heads = root.querySelector('.scheduler-room-heads'); if (heads) heads.scrollLeft = scroll.scrollLeft; });
      let state = null;
      const snap = value => Math.max(startDay, Math.min(endDay, Math.round(value / 15) * 15));
      const laneAt = (x, y) => document.elementFromPoint(x, y)?.closest('.scheduler-lane');
      const minuteAt = (lane, y) => snap(startDay + (y - lane.getBoundingClientRect().top) / ppm);
      const clearDrag = current => {
        current.preview?.remove();
        if (!current.card) return;
        current.card.classList.remove('dragging');
        current.card.style.transform = '';
        current.card.style.height = '';
        current.card.style.pointerEvents = '';
      };
      root.addEventListener('pointerdown', event => {
        if (event.button !== 0) return;
        const lane = event.target.closest('.scheduler-lane');
        if (!lane) return;
        const card = event.target.closest('.scheduler-event');
        const resize = event.target.closest('.resize-handle');
        const minute = minuteAt(lane, event.clientY);
        state = { action: resize ? 'resize' : card ? 'move' : 'create', lane, card, roomId: +lane.dataset.roomId, start: minute, current: minute };
        if (card) {
          state.id = +card.dataset.booking;
          state.original = +card.dataset.start;
          state.duration = +card.dataset.duration;
          card.setPointerCapture(event.pointerId);
          card.classList.add('dragging');
          // Pointer capture keeps drag events flowing to the card; disable hit-testing
          // so elementFromPoint can identify the lane beneath it while it is moved.
          card.style.pointerEvents = 'none';
        }
        else { lane.setPointerCapture(event.pointerId); const preview = document.createElement('div'); preview.className = 'scheduler-selection'; lane.appendChild(preview); state.preview = preview; }
        event.preventDefault();
      });
      root.addEventListener('pointermove', event => {
        if (!state) return;
        const targetLane = laneAt(event.clientX, event.clientY) || state.lane;
        state.current = minuteAt(targetLane, event.clientY);
        state.targetRoom = +(targetLane.dataset.roomId || state.roomId);
        if (state.preview) {
          const top = Math.min(state.start, state.current);
          const end = Math.max(state.start + 15, state.current);
          state.preview.style.top = `${(top - startDay) * ppm}px`;
          state.preview.style.height = `${Math.max(18, (end - top) * ppm)}px`;
        }
        if (state.card && state.action === 'move') {
          const newStart = Math.min(endDay - state.duration, state.current);
          state.card.style.transform = `translateY(${(newStart - state.original) * ppm}px)`;
        }
        if (state.card && state.action === 'resize') state.card.style.height = `${Math.max(18, (state.current - state.original) * ppm)}px`;
      });
      root.addEventListener('pointerup', async () => {
        if (!state) return;
        const current = state;
        state = null;
        clearDrag(current);
        let start, end;
        if (current.action === 'create') { start = Math.min(current.start, current.current); end = Math.max(current.start + 15, current.current); }
        else if (current.action === 'move') { start = Math.min(endDay - current.duration, current.current); end = start + current.duration; }
        else { start = current.original; end = Math.max(start + 15, current.current); }
        await dotnet.invokeMethodAsync('HandleCalendarAction', current.action, current.id || 0, current.targetRoom || current.roomId, start, end);
      });
      root.addEventListener('pointercancel', () => {
        if (!state) return;
        const current = state;
        state = null;
        clearDrag(current);
      });
    }
  }
};

document.documentElement.dataset.theme = localStorage.getItem('roomBook.theme') || (matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
document.addEventListener('keydown', event => {
  if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); roomBook.command.open(); }
  if (event.key === 'Escape') roomBook.command.close();
});
