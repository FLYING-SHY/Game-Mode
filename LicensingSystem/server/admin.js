'use strict';

const csrf = document.querySelector('meta[name="csrf-token"]').content;
const $ = selector => document.querySelector(selector);
const state = { view: 'codes', page: 1, pageSize: 25, total: 0, auditPage: 1, auditTotal: 0, issued: [], renewId: null, sessionCodeId: null, codesRequest: 0, sessionsRequest: 0 };
const statuses = { unused: '待激活', active: '有效', expired: '已过期', suspended: '已停用', revoked: '已撤销', online: '在线' };
const actions = { issue: '发放卡密', issue_batch: '批量发码', activate: '首次激活', activation: '激活', suspend: '停用', suspension: '修改停用状态', resume: '恢复', restore: '恢复', renew: '续期', unbind: '解绑设备', kick: '踢下线', logout: '登出', login: '管理员登录' };
const dateFormatter = new Intl.DateTimeFormat('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit', hourCycle: 'h23' });

async function api(path, body) {
  const response = await fetch('/api/admin/' + path, {
    method: body === undefined ? 'GET' : 'POST', credentials: 'same-origin',
    headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': csrf },
    body: body === undefined ? undefined : JSON.stringify(body)
  });
  if (response.status === 401) { location.assign('/admin/login'); throw new Error('登录已到期，请重新登录。'); }
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || '请求失败，HTTP ' + response.status);
  }
  const text = await response.text();
  return text ? JSON.parse(text) : null;
}
function message(text, isError = false) {
  const element = $('#message');
  element.textContent = text;
  element.classList.toggle('error-message', isError);
}
function showError(error) { message(error.message || String(error), true); }
function node(tag, text, className) {
  const element = document.createElement(tag);
  if (text !== undefined && text !== null) element.textContent = String(text);
  if (className) element.className = className;
  return element;
}
function button(text, className, handler) {
  const element = node('button', text, className);
  element.type = 'button';
  element.addEventListener('click', () => handler(element));
  return element;
}
function date(value) {
  if (value === null || value === undefined || value === '') return '—';
  const parsed = new Date(Number(value) * 1000);
  return Number.isFinite(parsed.getTime()) ? dateFormatter.format(parsed) : '—';
}
function duration(seconds) {
  const value = Number(seconds);
  if (!Number.isFinite(value) || value <= 0) return '—';
  if (value % 86400 === 0) return (value / 86400) + ' 天';
  if (value % 3600 === 0) return (value / 3600) + ' 小时';
  return value + ' 秒';
}
function emptyRow(tbody, columns, text) {
  const row = node('tr');
  const cell = node('td', text, 'empty-state');
  cell.colSpan = columns;
  row.append(cell); tbody.append(row);
}
function filters() { return { search: $('#search').value.trim(), status: $('#status').value }; }
function query(values) {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(values)) if (value !== '' && value !== null && value !== undefined) params.set(key, String(value));
  return params.toString();
}
async function copyText(text) {
  try {
    await navigator.clipboard.writeText(text);
    message('已复制到剪贴板。');
  } catch {
    const textarea = node('textarea'); textarea.value = text; textarea.className = 'clipboard-fallback';
    document.body.append(textarea); textarea.select();
    const copied = document.execCommand('copy'); textarea.remove();
    message(copied ? '已复制到剪贴板。' : '请选中完整卡密并按 Ctrl+C 复制。', !copied);
  }
}
function download(blob, filename) {
  const url = URL.createObjectURL(blob);
  const link = node('a'); link.href = url; link.download = filename;
  document.body.append(link); link.click(); link.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
function csvCell(value) {
  let text = String(value ?? '');
  if (/^[\s]*[=+\-@]/.test(text) || /^[\t\r]/.test(text)) text = "'" + text;
  return '"' + text.replaceAll('"', '""') + '"';
}
function exportIssued() {
  const rows = [['完整卡密', '类型', '使用秒数', '备注'], ...state.issued.map(item => [item.code, item.kind === 'permanent' ? '永久' : '限时', item.durationSeconds ?? '', item.note ?? ''])];
  const csv = '\uFEFF' + rows.map(row => row.map(csvCell).join(',')).join('\r\n') + '\r\n';
  download(new Blob([csv], { type: 'text/csv;charset=utf-8' }), '本次发码-' + new Date().toISOString().slice(0, 10) + '.csv');
}
async function exportFiltered(element) {
  element.disabled = true;
  try {
    const response = await fetch('/api/admin/codes/export?' + query(filters()), { credentials: 'same-origin' });
    if (response.status === 401) { location.assign('/admin/login'); return; }
    if (!response.ok) { const error = await response.json().catch(() => ({})); throw new Error(error.message || '导出失败，HTTP ' + response.status); }
    download(await response.blob(), '完整卡密-' + new Date().toISOString().slice(0, 10) + '.csv');
    message('筛选结果已导出。');
  } catch (error) { showError(error); } finally { element.disabled = false; }
}
async function mutate(record, action, body, element, success) {
  element.disabled = true;
  try {
    await api('codes/' + encodeURIComponent(record.id) + '/' + action, body);
    await refresh(); message(success);
  } catch (error) { showError(error); } finally { element.disabled = false; }
}
function renderCode(record) {
  const row = node('tr'); row.dataset.id = record.id;
  const codeCell = node('td');
  codeCell.append(node('code', record.code, 'license-code'));
  const codeMeta = node('div', record.kind === 'permanent' ? '永久卡' : '限时卡 / ' + duration(record.durationSeconds), 'cell-meta');
  codeMeta.append(button('复制', 'text-button', () => copyText(record.code)));
  codeCell.append(codeMeta); row.append(codeCell);
  const expiryCell = node('td');
  expiryCell.append(node('div', record.kind === 'permanent' ? '永久' : record.expiresAtUnix ? date(record.expiresAtUnix) : '首次激活后起算'));
  expiryCell.append(node('div', record.activatedAtUnix ? '激活：' + date(record.activatedAtUnix) : '尚未激活', 'cell-meta'));
  row.append(expiryCell);
  const deviceCell = node('td');
  if (record.boundDeviceId) { const hash = node('code', record.boundDeviceId, 'device-hash'); hash.title = record.boundDeviceId; deviceCell.append(hash); }
  else deviceCell.append(node('span', '未绑定', 'muted'));
  const online = Number(record.activeSessionCount || 0);
  const deviceMeta = node('div', online ? online + ' 个有效会话' : '当前离线', 'cell-meta');
  if (online || record.boundDeviceId) deviceMeta.append(button('查看会话', 'text-button', () => selectView('sessions', record.id, record.codeSuffix)));
  deviceCell.append(deviceMeta); row.append(deviceCell);
  row.append(node('td', record.note || '—', 'note-cell'));
  const statusCell = node('td'); statusCell.append(node('span', statuses[record.state] || record.state, 'status status-' + record.state)); row.append(statusCell);
  const operations = node('td'); const group = node('div', undefined, 'row-actions');
  group.append(button(record.state === 'suspended' ? '恢复' : '停用', record.state === 'suspended' ? 'small secondary' : 'small quiet danger-text', element => mutate(record, 'suspension', { suspended: record.state !== 'suspended' }, element, record.state === 'suspended' ? '卡密已恢复。' : '卡密已停用，旧会话已撤销。')));
  if (record.kind === 'timed') group.append(button('续期', 'small quiet', () => {
    state.renewId = record.id; $('#renew-code').textContent = record.code;
    $('#renew-dialog').showModal(); $('#renew-duration').focus();
  }));
  const unbind = button('解绑', 'small quiet', element => mutate(record, 'unbind', {}, element, '设备已解绑，旧会话已撤销，原计时保持不变。'));
  unbind.disabled = !record.boundDeviceId; group.append(unbind);
  const kick = button('踢下线', 'small quiet', element => mutate(record, 'kick', {}, element, '有效会话已撤销。'));
  kick.disabled = online === 0; group.append(kick);
  operations.append(group); row.append(operations);
  return row;
}
async function loadCodes() {
  const request = ++state.codesRequest;
  const data = await api('codes?' + query({ ...filters(), page: state.page, pageSize: state.pageSize }));
  if (request !== state.codesRequest) return;
  state.total = Number(data.total || 0);
  const rows = $('#rows'); rows.replaceChildren();
  const items = data.items || [];
  for (const record of items) rows.append(renderCode(record));
  if (!items.length) emptyRow(rows, 6, state.total ? '本页没有记录，请返回上一页。' : filters().search || filters().status ? '没有匹配的卡密，请调整查询条件。' : '还没有卡密。先在上方生成一批卡密。');
  for (const name of ['total', 'unused', 'active', 'expired', 'suspended', 'online']) $('#stat-' + name).textContent = String(data.stats?.[name] ?? 0);
  const pages = Math.max(1, Math.ceil(state.total / state.pageSize));
  $('#codes-count').textContent = '找到 ' + state.total + ' 条卡密';
  $('#page-number').textContent = state.page + ' / ' + pages;
  $('#prev-page').disabled = state.page <= 1; $('#next-page').disabled = state.page >= pages;
}
async function loadSessions() {
  const request = ++state.sessionsRequest;
  const data = await api('sessions?' + query({ codeId: state.sessionCodeId, activeOnly: $('#active-only').checked }));
  if (request !== state.sessionsRequest) return;
  const rows = $('#session-rows'); rows.replaceChildren();
  const items = data.items || [];
  for (const record of items) {
    const row = node('tr'); row.append(node('td', record.codeSuffix || '—'));
    const device = node('td'); device.append(node('code', record.deviceId || '—', 'device-hash'), node('div', record.installationId || '—', 'cell-meta identifier'));
    row.append(device);
    const identity = node('td'); identity.append(node('div', record.gameName || '—'), node('code', record.steamId || '—', 'cell-meta'));
    row.append(identity, node('td', date(record.lastSeenAtUnix)), node('td', date(record.leaseUntilUnix)));
    const operations = node('td'); operations.append(node('span', statuses[record.state] || record.state, 'status status-' + record.state));
    if (record.state === 'active' || record.state === 'online') operations.append(button('踢下线', 'small quiet danger-text', element => mutate({ id: record.codeId }, 'kick', {}, element, '会话已撤销。')));
    row.append(operations); rows.append(row);
  }
  if (!items.length) emptyRow(rows, 6, $('#active-only').checked ? '当前没有有效在线会话。' : '还没有符合条件的会话记录。');
}
async function loadAudit() {
  const data = await api('audit?' + query({ page: state.auditPage, pageSize: 50 }));
  state.auditTotal = Number(data.total || 0);
  const rows = $('#audit-rows'); rows.replaceChildren();
  for (const record of data.items || []) {
    const row = node('tr');
    const details = typeof record.details === 'string' ? record.details : JSON.stringify(record.details ?? '');
    row.append(node('td', date(record.occurredAtUnix)), node('td', actions[record.action] || record.action), node('td', record.codeId || '—', 'identifier'), node('td', details || '—'));
    rows.append(row);
  }
  if (!(data.items || []).length) emptyRow(rows, 4, '还没有管理操作记录。');
  const pages = Math.max(1, Math.ceil(state.auditTotal / 50));
  $('#audit-count').textContent = '共 ' + state.auditTotal + ' 条操作'; $('#audit-page').textContent = state.auditPage + ' / ' + pages;
  $('#audit-prev').disabled = state.auditPage <= 1; $('#audit-next').disabled = state.auditPage >= pages;
}
async function refresh() {
  await loadCodes();
  if (state.view === 'sessions') await loadSessions();
  else if (state.view === 'audit') await loadAudit();
}
async function selectView(view, codeId = null, suffix = '') {
  state.view = view; state.sessionCodeId = view === 'sessions' ? codeId : null;
  for (const tab of document.querySelectorAll('[data-view]')) {
    const selected = tab.dataset.view === view;
    tab.setAttribute('aria-selected', String(selected)); tab.tabIndex = selected ? 0 : -1;
    $('#panel-' + tab.dataset.view).hidden = !selected;
  }
  $('#panel-sessions .filter-bar p').textContent = codeId ? '正在查看尾号 ' + suffix + ' 的会话。点击“在线会话”可查看全部。' : '正常登出立即释放；异常退出在最后一次租约到期后释放。';
  try { if (view === 'sessions') await loadSessions(); else if (view === 'audit') await loadAudit(); else await loadCodes(); } catch (error) { showError(error); }
}

$('#kind').addEventListener('change', () => {
  const timed = $('#kind').value === 'timed'; $('#duration-fields').hidden = !timed;
  $('#duration').required = timed;
});
$('#issue').addEventListener('submit', async event => {
  event.preventDefault(); const submit = $('#issue-button'); submit.disabled = true;
  try {
    const kind = $('#kind').value;
    const result = await api('codes', { kind, durationSeconds: kind === 'timed' ? Number($('#duration').value) * Number($('#duration-unit').value) : null, quantity: Number($('#quantity').value), note: $('#note').value.trim() });
    state.issued = result.items || [];
    $('#issued-codes').value = state.issued.map(item => item.code).join('\n');
    $('#issued-title').textContent = '本次生成 ' + state.issued.length + ' 条卡密'; $('#issued').hidden = false;
    state.page = 1; await refresh(); message('已生成 ' + state.issued.length + ' 条卡密。');
  } catch (error) { showError(error); } finally { submit.disabled = false; }
});
$('#copy-issued').addEventListener('click', () => copyText($('#issued-codes').value));
$('#export-issued').addEventListener('click', exportIssued);
$('#close-issued').addEventListener('click', () => { $('#issued').hidden = true; });
$('#export-all').addEventListener('click', () => exportFiltered($('#export-all')));
$('#filter').addEventListener('submit', event => { event.preventDefault(); state.page = 1; loadCodes().catch(showError); });
$('#prev-page').addEventListener('click', () => { if (state.page > 1) { state.page--; loadCodes().catch(showError); } });
$('#next-page').addEventListener('click', () => { if (state.page * state.pageSize < state.total) { state.page++; loadCodes().catch(showError); } });
$('#active-only').addEventListener('change', () => loadSessions().catch(showError));
$('#audit-prev').addEventListener('click', () => { if (state.auditPage > 1) { state.auditPage--; loadAudit().catch(showError); } });
$('#audit-next').addEventListener('click', () => { if (state.auditPage * 50 < state.auditTotal) { state.auditPage++; loadAudit().catch(showError); } });
for (const tab of document.querySelectorAll('[data-view]')) {
  tab.addEventListener('click', () => selectView(tab.dataset.view));
  tab.addEventListener('keydown', event => {
    if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;
    event.preventDefault(); const tabs = [...document.querySelectorAll('[data-view]')];
    let index = tabs.indexOf(tab);
    if (event.key === 'Home') index = 0; else if (event.key === 'End') index = tabs.length - 1;
    else index = (index + (event.key === 'ArrowRight' ? 1 : -1) + tabs.length) % tabs.length;
    tabs[index].focus(); selectView(tabs[index].dataset.view);
  });
}
$('#renew-cancel').addEventListener('click', () => $('#renew-dialog').close());
$('#renew-form').addEventListener('submit', async event => {
  event.preventDefault(); const submit = event.submitter; submit.disabled = true;
  try {
    await api('codes/' + encodeURIComponent(state.renewId) + '/renew', { durationSeconds: Number($('#renew-duration').value) * Number($('#renew-unit').value) });
    $('#renew-dialog').close(); await refresh(); message('授权已续期，设备绑定保持不变。');
  } catch (error) { showError(error); } finally { submit.disabled = false; }
});
$('#refresh').addEventListener('click', async () => {
  const element = $('#refresh'); element.disabled = true;
  try { await refresh(); message('记录已刷新。'); } catch (error) { showError(error); } finally { element.disabled = false; }
});
$('#logout').addEventListener('click', async () => {
  try { await api('logout', {}); location.assign('/admin/login'); } catch (error) { showError(error); }
});
(async () => {
  try {
    const settings = await api('settings');
    $('#product-name').textContent = settings.productName || '卡密、设备绑定与在线会话';
    await refresh();
  } catch (error) { showError(error); }
})();
