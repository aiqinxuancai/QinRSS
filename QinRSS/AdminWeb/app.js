const $ = (s, root = document) => root.querySelector(s);
const $$ = (s, root = document) => [...root.querySelectorAll(s)];
let state = { connected: [], subscriptions: [] };
const titles = { overview: '运行概览', subscriptions: '订阅管理', send: '发送消息', logs: '运行日志', config: '系统配置' };

async function api(url, options = {}) {
  const response = await fetch(url, { headers: { 'Content-Type': 'application/json' }, ...options });
  const data = await response.json().catch(() => ({}));
  if (!response.ok) throw new Error(data.error || `请求失败 (${response.status})`);
  return data;
}
function toast(message, error = false) {
  const el = $('#toast'); el.textContent = message; el.classList.toggle('error', error); el.classList.add('show');
  setTimeout(() => el.classList.remove('show'), 2800);
}
function showView(name) {
  $$('.nav-item').forEach(item => item.classList.toggle('active', item.dataset.view === name));
  $$('.view').forEach(view => view.classList.toggle('active', view.id === `view-${name}`));
  $('#page-title').textContent = titles[name];
  if (name === 'logs') loadLogs();
  if (name === 'config') loadConfig();
}
async function loadStatus() {
  try {
    state = await api('/api/status');
    $('#side-status').textContent = '服务在线';
    $('#stat-bots').textContent = state.connected.length;
    $('#stat-subs').textContent = state.subscriptionCount;
    $('#stat-interval').textContent = state.config?.runInterval || '-';
    $('#stat-dir').textContent = state.dataDir || '-';
    $('#last-sent').textContent = state.lastSentTime ? new Date(state.lastSentTime).toLocaleString() : '暂无记录';
    renderSubscriptions(); renderBotOptions();
  } catch (e) { $('#side-status').textContent = '连接失败'; toast(e.message, true); }
}
function renderBotOptions() {
  const select = $('#send-self'); const values = state.connected || [];
  select.innerHTML = values.length ? values.map(id => `<option value="${esc(id)}">${esc(id)}</option>`).join('') : '<option value="">暂无在线 Bot</option>';
}
function renderSubscriptions() {
  const rows = $('#subscription-rows'); $('#sub-count').textContent = `${state.subscriptionCount || 0} 条`;
  if (!state.subscriptions?.length) { rows.innerHTML = '<tr><td colspan="6" class="empty">暂无订阅，先添加一个 RSS 源吧</td></tr>'; return; }
  rows.innerHTML = state.subscriptions.map((item, index) => `<tr><td>${esc(item.customName || item.name || '未命名')}<br><span class="tag">${esc(item.subscriptionType || 'Normal')}</span></td><td>${esc(item.selfId)}</td><td>${esc(item.guildId ? `${item.guildId} / ${item.groupOrChannelId}` : item.groupOrChannelId)}</td><td title="${esc(item.url)}">${esc(shorten(item.url, 34))}</td><td>${item.translate ? '<span class="tag">翻译</span>' : ''}${item.translateOnly ? '<span class="tag">仅译文</span>' : ''}</td><td><button class="danger" data-delete="${index}">删除</button></td></tr>`).join('');
  $$('[data-delete]', rows).forEach(button => button.onclick = async () => {
    const item = state.subscriptions[Number(button.dataset.delete)];
    if (!confirm(`删除订阅“${item.customName || item.name}”？`)) return;
    try { await api(`/api/subscriptions?selfId=${encodeURIComponent(item.selfId)}&guildId=${encodeURIComponent(item.guildId || '')}&targetId=${encodeURIComponent(item.groupOrChannelId || '')}&name=${encodeURIComponent(item.customName || '')}`, { method: 'DELETE' }); toast('订阅已删除'); await loadStatus(); } catch (e) { toast(e.message, true); }
  });
}
async function loadLogs() { try { const data = await api(`/api/logs?lines=${$('#log-lines').value}`); $('#log-content').textContent = data.content || '暂无日志'; $('#log-file').textContent = data.file || ''; $('#log-content').scrollTop = $('#log-content').scrollHeight; } catch (e) { toast(e.message, true); } }
async function loadConfig() {
  try {
    const config = await api('/api/config'); const form = $('#config-form');
    Object.entries(config).forEach(([key, value]) => { const el = form.elements[key]; if (!el) return; if (el.type === 'checkbox') el.checked = Boolean(value); else if (key === 'groupAdmins') el.value = (value || []).join(', '); else if (key === 'guildAdmins') el.value = (value || []).join(', '); else el.value = value ?? ''; });
  } catch (e) { toast(e.message, true); }
}
function formDataObject(form) { const data = {}; for (const el of form.elements) if (el.name) data[el.name] = el.type === 'checkbox' ? el.checked : el.value; return data; }
function esc(value) { return String(value ?? '').replace(/[&<>'"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[c])); }
function shorten(value, n) { value = String(value || ''); return value.length > n ? `${value.slice(0, n)}…` : value; }

$$('.nav-item').forEach(button => button.onclick = () => showView(button.dataset.view));
$$('[data-go]').forEach(button => button.onclick = () => showView(button.dataset.go));
$('#refresh').onclick = loadStatus;
$('#refresh-subs').onclick = async () => { try { await api('/api/subscriptions/refresh', { method: 'POST' }); toast('订阅任务已刷新'); await loadStatus(); } catch (e) { toast(e.message, true); } };
$('#refresh-logs').onclick = loadLogs; $('#log-lines').onchange = loadLogs;
$('#subscription-form').onsubmit = async e => { e.preventDefault(); try { const data = formDataObject(e.target); await api('/api/subscriptions', { method: 'POST', body: JSON.stringify(data) }); toast('订阅已添加'); e.target.reset(); await loadStatus(); } catch (err) { toast(err.message, true); } };
$('#send-form').onsubmit = async e => { e.preventDefault(); try { await api('/api/send', { method: 'POST', body: JSON.stringify(formDataObject(e.target)) }); toast('消息已发送'); e.target.elements.message.value = ''; } catch (err) { toast(err.message, true); } };
$('#config-form').onsubmit = async e => { e.preventDefault(); try { const data = formDataObject(e.target); data.runInterval = Number(data.runInterval); data.sendInterval = Number(data.sendInterval); data.adminPort = Number(data.adminPort); data.groupAdmins = String(data.groupAdmins || '').split(',').map(s => Number(s.trim())).filter(Number.isFinite); data.guildAdmins = String(data.guildAdmins || '').split(',').map(s => s.trim()).filter(Boolean); const result = await api('/api/config', { method: 'POST', body: JSON.stringify(data) }); toast('配置已保存并立即生效'); if (result.restartUrl) setTimeout(() => location.href = result.restartUrl, 1000); else await loadStatus(); } catch (err) { toast(err.message, true); } };
loadStatus(); setInterval(loadStatus, 15000);
