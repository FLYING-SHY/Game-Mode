'use strict';

const csrf = document.querySelector('meta[name="csrf-token"]').content;
const $ = selector => document.querySelector(selector);
const state = { productId: null, products: [], accountBound: false, epoch: 0, ready: false, view: 'codes', page: 1, pageSize: 25, total: 0, auditPage: 1, auditTotal: 0, issued: [], renewId: null, deleteRecords: [], deleteBatch: false, selected: new Map(), pageRecords: [], sessionCodeId: null, codesRequest: 0, sessionsRequest: 0, auditRequest: 0 };
const staleResponse = new Error('产品已切换。');
const statuses = { unused: '待激活', active: '有效', scheduled: '待生效', expired: '已过期', suspended: '已停用', revoked: '已撤销', online: '在线' };
const actions = { issue: '发放卡密', issue_batch: '批量发码', activate: '首次激活', activation: '激活', suspend: '停用', suspension: '修改停用状态', resume: '恢复', restore: '恢复', renew: '续期', unbind: '解绑设备', kick: '踢下线', delete: '删除卡密', logout: '登出', login: '管理员登录' };
const dateFormatter = new Intl.DateTimeFormat('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit', hourCycle: 'h23' });

function productContext() { return { productId: state.productId, epoch: state.epoch }; }
// An epoch identifies one product selection, including a return to the same product.
function isCurrent(context) { return context.epoch === state.epoch; }
function requireCurrent(context) { if (!isCurrent(context)) throw staleResponse; }
function adminPath(path, context) {
  const index = path.indexOf('?');
  const params = new URLSearchParams(index < 0 ? '' : path.slice(index + 1));
  if (context.productId) params.set('productId', context.productId);
  const suffix = params.toString();
  return '/api/admin/' + (index < 0 ? path : path.slice(0, index)) + (suffix ? '?' + suffix : '');
}
async function request(path, body, context) {
  try {
    requireCurrent(context);
    const response = await fetch(adminPath(path, context), {
      method: body === undefined ? 'GET' : 'POST', credentials: 'same-origin',
      headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': csrf },
      body: body === undefined ? undefined : JSON.stringify(body)
    });
    requireCurrent(context);
    if (response.status === 401) { location.assign('/admin/login'); throw new Error('登录已到期，请重新登录。'); }
    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      requireCurrent(context);
      throw new Error(error.message || '请求失败，HTTP ' + response.status);
    }
    return response;
  } catch (error) { requireCurrent(context); throw error; }
}
async function api(path, body, context = productContext()) {
  try {
    const response = await request(path, body, context);
    const text = await response.text();
    requireCurrent(context);
    return text ? JSON.parse(text) : null;
  } catch (error) { requireCurrent(context); throw error; }
}
function message(text, isError = false) {
  const element = $('#message');
  element.textContent = text;
  element.classList.toggle('error-message', isError);
}
function showError(error, context) { if (error !== staleResponse && isCurrent(context)) message(error.message || String(error), true); }
function readCurrent(load) {
  const context = productContext();
  load(context).catch(error => showError(error, context));
}
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
async function copyText(text, context = productContext()) {
  if (!isCurrent(context)) return;
  try {
    await navigator.clipboard.writeText(text);
    if (isCurrent(context)) message('已复制到剪贴板。');
  } catch {
    if (!isCurrent(context)) return;
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
  if (!state.ready) return;
  const context = productContext();
  element.disabled = true;
  try {
    const response = await request('codes/export?' + query(filters()), undefined, context);
    const blob = await response.blob();
    requireCurrent(context);
    download(blob, '完整卡密-' + new Date().toISOString().slice(0, 10) + '.csv');
    message('筛选结果已导出。');
  } catch (error) { showError(error, context); } finally { if (isCurrent(context)) element.disabled = false; }
}
async function mutate(record, action, body, element, success, context) {
  if (!isCurrent(context)) return;
  element.disabled = true;
  try {
    await api('codes/' + encodeURIComponent(record.id) + '/' + action, body, context);
    await refresh(context); requireCurrent(context); message(success);
  } catch (error) { showError(error, context); } finally { if (isCurrent(context)) element.disabled = false; }
}
function updateSelection() {
  const count = state.selected.size;
  $('#selection-count').textContent = '已选 ' + count + ' 条';
  $('#delete-selected').disabled = count === 0;
  const selectPage = $('#select-page');
  selectPage.disabled = state.pageRecords.length === 0;
  selectPage.checked = count > 0 && count === state.pageRecords.length;
  selectPage.indeterminate = count > 0 && count < state.pageRecords.length;
  for (const checkbox of $('#rows').querySelectorAll('.code-select')) {
    checkbox.checked = state.selected.has(checkbox.value);
    checkbox.closest('tr').classList.toggle('is-selected', checkbox.checked);
  }
}
function openDeleteDialog(records, batch = false) {
  if (!records.length) return;
  state.deleteRecords = records.slice(); state.deleteBatch = batch;
  const record = records[0];
  $('#delete-heading').textContent = batch ? '永久删除 ' + records.length + ' 条卡密' : '永久删除卡密';
  $('#delete-code').textContent = batch
    ? '尾号：' + records.slice(0, 5).map(item => item.codeSuffix).join('、') + (records.length > 5 ? '；另 ' + (records.length - 5) + ' 条' : '')
    : record.codeReadable !== false ? record.code : '卡密尾号：' + record.codeSuffix;
  $('#delete-note').textContent = batch ? '仅删除当前页已勾选的 ' + records.length + ' 条卡密。' : record.note ? '备注：' + record.note : '无备注';
  $('#delete-confirm').textContent = batch ? '确认删除 ' + records.length + ' 条' : '确认删除';
  $('#delete-error').textContent = '';
  $('#delete-dialog').showModal(); $('#delete-cancel').focus();
}
function renderCode(record) {
  const context = productContext();
  const row = node('tr'); row.dataset.id = record.id;
  const readable = record.codeReadable !== false;
  const codeCell = node('td');
  const selection = node('input', undefined, 'code-select'); selection.type = 'checkbox'; selection.value = record.id;
  selection.setAttribute('aria-label', '选择尾号 ' + record.codeSuffix);
  selection.addEventListener('change', () => {
    if (!isCurrent(context)) return;
    if (selection.checked) state.selected.set(record.id, record); else state.selected.delete(record.id);
    updateSelection();
  });
  codeCell.append(selection, node('code', readable ? record.code : '（密文不可解）', 'license-code' + (readable ? '' : ' unreadable')));
  const codeMeta = node('div', record.kind === 'permanent' ? '永久卡' : '限时卡 / ' + duration(record.durationSeconds), 'cell-meta');
  if (readable) codeMeta.append(button('复制', 'text-button', () => copyText(record.code, context)));
  else codeMeta.append(node('span', '该密文无法用当前密钥解开，需用明文重新加密后修复。', 'cell-meta'));
  codeCell.append(codeMeta); row.append(codeCell);
  const expiryCell = node('td');
  expiryCell.append(node('div', record.kind === 'permanent' ? '永久' : record.expiresAtUnix ? date(record.expiresAtUnix) : state.accountBound ? '兑换后起算' : '首次激活后起算'));
  if (state.accountBound && record.startsAtUnix != null) expiryCell.append(node('div', '起始：' + date(record.startsAtUnix), 'cell-meta'));
  expiryCell.append(node('div', record.activatedAtUnix ? (state.accountBound ? '兑换：' : '激活：') + date(record.activatedAtUnix) : state.accountBound ? '尚未兑换' : '尚未激活', 'cell-meta'));
  row.append(expiryCell);
  const deviceCell = node('td');
  const online = Number(record.activeSessionCount || 0);
  if (state.accountBound) {
    deviceCell.append(record.accountId ? node('code', record.accountId, 'account-id') : node('span', '未绑定账号', 'muted'));
    deviceCell.append(node('div', record.accountId ? '固定绑定 Zepp UID' : '首次兑换时绑定', 'cell-meta'));
  } else {
    if (record.boundDeviceId) { const hash = node('code', record.boundDeviceId, 'device-hash'); hash.title = record.boundDeviceId; deviceCell.append(hash); }
    else deviceCell.append(node('span', '未绑定', 'muted'));
    const deviceMeta = node('div', online ? online + ' 个有效会话' : '当前离线', 'cell-meta');
    if (online || record.boundDeviceId) deviceMeta.append(button('查看会话', 'text-button', () => { if (isCurrent(context)) selectView('sessions', record.id, record.codeSuffix); }));
    deviceCell.append(deviceMeta);
  }
  row.append(deviceCell);
  row.append(node('td', record.note || '—', 'note-cell'));
  const statusCell = node('td'); statusCell.append(node('span', statuses[record.state] || record.state, 'status status-' + record.state)); row.append(statusCell);
  const operations = node('td'); const group = node('div', undefined, 'row-actions');
  group.append(button(record.state === 'suspended' ? '恢复' : '停用', record.state === 'suspended' ? 'small secondary' : 'small quiet danger-text', element => mutate(record, 'suspension', { suspended: record.state !== 'suspended' }, element, record.state === 'suspended' ? '卡密已恢复。' : state.accountBound ? '卡密已停用，该卡权益已暂停。' : '卡密已停用，旧会话已撤销。', context)));
  if (record.kind === 'timed') group.append(button('续期', 'small quiet', () => {
    if (!isCurrent(context)) return;
    state.renewId = record.id; $('#renew-code').textContent = record.code;
    $('#renew-dialog').showModal(); $('#renew-duration').focus();
  }));
  if (!state.accountBound) {
    const unbind = button('解绑', 'small quiet', element => mutate(record, 'unbind', {}, element, '设备已解绑，旧会话已撤销，原计时保持不变。', context));
    unbind.disabled = !record.boundDeviceId; group.append(unbind);
    const kick = button('踢下线', 'small quiet', element => mutate(record, 'kick', {}, element, '有效会话已撤销。', context));
    kick.disabled = online === 0; group.append(kick);
  }
  group.append(button('删除', 'small quiet danger-text', () => { if (isCurrent(context)) openDeleteDialog([record]); }));
  operations.append(group); row.append(operations);
  return row;
}
async function loadCodes(context = productContext()) {
  if (!state.ready) return;
  const request = ++state.codesRequest;
  const data = await api('codes?' + query({ ...filters(), page: state.page, pageSize: state.pageSize }), undefined, context);
  if (!isCurrent(context) || request !== state.codesRequest) return;
  state.total = Number(data.total || 0);
  const pages = Math.max(1, Math.ceil(state.total / state.pageSize));
  if (state.page > pages) { state.page = pages; await loadCodes(context); return; }
  const rows = $('#rows'); rows.replaceChildren();
  const items = data.items || [];
  state.selected.clear(); state.pageRecords = items;
  for (const record of items) rows.append(renderCode(record));
  updateSelection();
  if (!items.length) emptyRow(rows, 6, state.total ? '本页没有记录，请返回上一页。' : filters().search || filters().status ? '没有匹配的卡密，请调整查询条件。' : '还没有卡密。先在上方生成一批卡密。');
  for (const name of ['total', 'unused', 'active', 'expired', 'suspended', 'online']) $('#stat-' + name).textContent = String(data.stats?.[name] ?? 0);
  $('#codes-count').textContent = '找到 ' + state.total + ' 条卡密';
  $('#page-number').textContent = state.page + ' / ' + pages;
  $('#prev-page').disabled = state.page <= 1; $('#next-page').disabled = state.page >= pages;
}
async function loadSessions(context = productContext()) {
  if (!state.ready || state.accountBound) return;
  const request = ++state.sessionsRequest;
  const data = await api('sessions?' + query({ codeId: state.sessionCodeId, activeOnly: $('#active-only').checked }), undefined, context);
  if (!isCurrent(context) || request !== state.sessionsRequest) return;
  const rows = $('#session-rows'); rows.replaceChildren();
  const items = data.items || [];
  for (const record of items) {
    const row = node('tr'); row.append(node('td', record.codeSuffix || '—'));
    const device = node('td'); device.append(node('code', record.deviceId || '—', 'device-hash'), node('div', record.installationId || '—', 'cell-meta identifier'));
    row.append(device);
    const identity = node('td'); identity.append(node('div', record.gameName || '—'), node('code', record.steamId || '—', 'cell-meta'));
    row.append(identity, node('td', date(record.lastSeenAtUnix)), node('td', date(record.leaseUntilUnix)));
    const operations = node('td'); operations.append(node('span', statuses[record.state] || record.state, 'status status-' + record.state));
    if (record.state === 'active' || record.state === 'online') operations.append(button('踢下线', 'small quiet danger-text', element => mutate({ id: record.codeId }, 'kick', {}, element, '会话已撤销。', context)));
    row.append(operations); rows.append(row);
  }
  if (!items.length) emptyRow(rows, 6, $('#active-only').checked ? '当前没有有效在线会话。' : '还没有符合条件的会话记录。');
}
async function loadAudit(context = productContext()) {
  if (!state.ready) return;
  const request = ++state.auditRequest;
  const data = await api('audit?' + query({ page: state.auditPage, pageSize: 50 }), undefined, context);
  if (!isCurrent(context) || request !== state.auditRequest) return;
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
async function refresh(context = productContext()) {
  requireCurrent(context);
  await loadCodes(context);
  requireCurrent(context);
  if (state.view === 'sessions') await loadSessions(context);
  else if (state.view === 'audit') await loadAudit(context);
}
function setView(view, codeId = null, suffix = '') {
  state.view = view; state.sessionCodeId = view === 'sessions' ? codeId : null;
  for (const tab of document.querySelectorAll('[data-view]')) {
    const selected = tab.dataset.view === view;
    tab.setAttribute('aria-selected', String(selected)); tab.tabIndex = selected ? 0 : -1;
    $('#panel-' + tab.dataset.view).hidden = !selected;
  }
  $('#panel-sessions .filter-bar p').textContent = codeId ? '正在查看尾号 ' + suffix + ' 的会话。点击“在线会话”可查看全部。' : '正常登出立即释放；异常退出在最后一次租约到期后释放。';
}
async function selectView(view, codeId = null, suffix = '') {
  if (!state.ready || (state.accountBound && view === 'sessions')) return;
  const context = productContext();
  setView(view, codeId, suffix);
  try { if (view === 'sessions') await loadSessions(context); else if (view === 'audit') await loadAudit(context); else await loadCodes(context); } catch (error) { showError(error, context); }
}
function applyProduct(settings) {
  state.accountBound = settings.accountBound === true;
  $('#product-name').textContent = settings.productName || settings.productId;
  $('#binding-heading').textContent = state.accountBound ? 'Zepp 账号 UID' : '设备与会话';
  $('#tab-sessions').hidden = state.accountBound;
  $('#stat-online-group').hidden = state.accountBound;
  $('#status-scheduled').hidden = !state.accountBound;
  $('#issue-description').textContent = state.accountBound ? '时长卡兑换后按账号已有权益依次排期；永久卡立即生效。卡密固定绑定 Zepp UID。' : '限时卡从首次成功激活起算；永久卡同样需要在线续租。';
  $('#renew-description').textContent = state.accountBound ? '已过期从现在起延长，仍有效则从原到期时间延长；账号绑定及后续卡的起始时间保持不变。' : '已过期从现在起延长，仍有效则从原到期时间延长；设备绑定保持不变。';
  $('#delete-description').textContent = state.accountBound ? '将永久删除所列卡密，该卡对应权益立即撤销；账号其他卡的权益和排期保持不变。历史兑换回执与操作记录保留，此操作不可撤销。' : '将永久删除所列卡密及关联会话、激活请求，操作记录保留。此操作不可撤销。已连接客户端将在下次心跳或短租约到期后停止授权。';
  $('#audit-description').textContent = state.accountBound ? '记录发码、兑换、停用、恢复、续期和删除等管理操作。' : '记录发码、停用、续期、解绑、删除和会话撤销等管理操作。';
  $('#page-footnote').textContent = state.accountBound ? '时间按当前浏览器时区显示。卡密固定绑定 Zepp UID；待生效卡按原定时段生效，停用不补时、不重排其他卡。' : '时间按当前浏览器时区显示。设备仅显示哈希标识；停用或踢线通过客户端心跳与短租约生效。';
}
function resetProduct() {
  state.ready = false; state.page = 1; state.total = 0; state.auditPage = 1; state.auditTotal = 0;
  state.issued = []; state.renewId = null; state.deleteRecords = []; state.deleteBatch = false;
  state.selected.clear(); state.pageRecords = [];
  state.codesRequest++; state.sessionsRequest++; state.auditRequest++;
  $('#search').value = ''; $('#status').value = ''; $('#active-only').checked = true;
  $('#issued').hidden = true; $('#issued-codes').value = ''; $('#issued-title').textContent = '本次生成';
  $('#renew-code').textContent = ''; $('#renew-duration').value = '30'; $('#renew-unit').value = '86400';
  $('#delete-heading').textContent = '永久删除卡密'; $('#delete-confirm').textContent = '确认删除';
  $('#delete-code').textContent = ''; $('#delete-note').textContent = ''; $('#delete-error').textContent = '';
  for (const dialog of document.querySelectorAll('dialog')) if (dialog.open) dialog.close();
  for (const id of ['issue-button', 'export-all']) $('#' + id).disabled = true;
  for (const id of ['renew-confirm', 'renew-cancel', 'delete-confirm', 'delete-cancel', 'refresh']) $('#' + id).disabled = false;
  for (const id of ['rows', 'session-rows', 'audit-rows']) $('#' + id).replaceChildren();
  for (const name of ['total', 'unused', 'active', 'expired', 'suspended', 'online']) $('#stat-' + name).textContent = '0';
  $('#codes-count').textContent = '正在加载…'; $('#page-number').textContent = '1';
  $('#audit-count').textContent = '尚未加载'; $('#audit-page').textContent = '1';
  for (const id of ['prev-page', 'next-page', 'audit-prev', 'audit-next']) $('#' + id).disabled = true;
  setView('codes'); updateSelection(); message('');
}
async function loadProduct(context = productContext()) {
  const settings = await api('settings', undefined, context);
  requireCurrent(context);
  if (context.productId && settings.productId !== context.productId) throw new Error('产品设置与当前选择不一致，请刷新重试。');
  if (!context.productId) { state.productId = settings.productId; context = productContext(); }
  state.products = settings.products?.length ? settings.products : [settings];
  const select = $('#product-select'); select.replaceChildren();
  for (const product of state.products) {
    const option = node('option', product.productName || product.productId); option.value = product.productId; select.append(option);
  }
  select.value = state.productId; select.disabled = state.products.length < 2;
  applyProduct(settings); state.ready = true;
  $('#issue-button').disabled = false; $('#export-all').disabled = false;
  await refresh(context);
  requireCurrent(context);
  return context;
}
$('#product-select').addEventListener('change', async () => {
  const productId = $('#product-select').value;
  const product = state.products.find(item => item.productId === productId);
  if (!product || productId === state.productId) return;
  state.epoch++; state.productId = productId;
  resetProduct(); applyProduct(product);
  const context = productContext();
  try { await loadProduct(context); } catch (error) { showError(error, context); }
});

$('#kind').addEventListener('change', () => {
  const timed = $('#kind').value === 'timed'; $('#duration-fields').hidden = !timed;
  $('#duration').required = timed;
});
$('#issue').addEventListener('submit', async event => {
  event.preventDefault(); const submit = $('#issue-button');
  if (!state.ready || submit.disabled) return;
  const context = productContext(); submit.disabled = true;
  try {
    const kind = $('#kind').value;
    const result = await api('codes', { kind, durationSeconds: kind === 'timed' ? Number($('#duration').value) * Number($('#duration-unit').value) : null, quantity: Number($('#quantity').value), note: $('#note').value.trim() }, context);
    requireCurrent(context);
    state.issued = result.items || [];
    $('#issued-codes').value = state.issued.map(item => item.code).join('\n');
    $('#issued-title').textContent = '本次生成 ' + state.issued.length + ' 条卡密'; $('#issued').hidden = false;
    state.page = 1; await refresh(context); requireCurrent(context); message('已生成 ' + state.issued.length + ' 条卡密。');
  } catch (error) { showError(error, context); } finally { if (isCurrent(context)) submit.disabled = false; }
});
$('#copy-issued').addEventListener('click', () => copyText($('#issued-codes').value));
$('#export-issued').addEventListener('click', exportIssued);
$('#close-issued').addEventListener('click', () => { $('#issued').hidden = true; });
$('#export-all').addEventListener('click', () => exportFiltered($('#export-all')));
$('#filter').addEventListener('submit', event => { event.preventDefault(); state.page = 1; readCurrent(loadCodes); });
$('#prev-page').addEventListener('click', () => { if (state.page > 1) { state.page--; readCurrent(loadCodes); } });
$('#next-page').addEventListener('click', () => { if (state.page * state.pageSize < state.total) { state.page++; readCurrent(loadCodes); } });
$('#active-only').addEventListener('change', () => readCurrent(loadSessions));
$('#audit-prev').addEventListener('click', () => { if (state.auditPage > 1) { state.auditPage--; readCurrent(loadAudit); } });
$('#audit-next').addEventListener('click', () => { if (state.auditPage * 50 < state.auditTotal) { state.auditPage++; readCurrent(loadAudit); } });
for (const tab of document.querySelectorAll('[data-view]')) {
  tab.addEventListener('click', () => selectView(tab.dataset.view));
  tab.addEventListener('keydown', event => {
    if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;
    event.preventDefault(); const tabs = [...document.querySelectorAll('[data-view]')].filter(item => !item.hidden);
    let index = tabs.indexOf(tab);
    if (event.key === 'Home') index = 0; else if (event.key === 'End') index = tabs.length - 1;
    else index = (index + (event.key === 'ArrowRight' ? 1 : -1) + tabs.length) % tabs.length;
    tabs[index].focus(); selectView(tabs[index].dataset.view);
  });
}
$('#renew-cancel').addEventListener('click', () => $('#renew-dialog').close());
$('#renew-dialog').addEventListener('close', () => { if (!$('#renew-dialog').open) state.renewId = null; });
$('#renew-form').addEventListener('submit', async event => {
  event.preventDefault(); const submit = $('#renew-confirm');
  const renewId = state.renewId;
  if (!state.ready || !renewId || submit.disabled) return;
  const context = productContext(); submit.disabled = true;
  try {
    await api('codes/' + encodeURIComponent(renewId) + '/renew', { durationSeconds: Number($('#renew-duration').value) * Number($('#renew-unit').value) }, context);
    requireCurrent(context); $('#renew-dialog').close(); await refresh(context); requireCurrent(context);
    message(state.accountBound ? '授权已续期，账号绑定及后续卡的起始时间保持不变。' : '授权已续期，设备绑定保持不变。');
  } catch (error) { showError(error, context); } finally { if (isCurrent(context)) submit.disabled = false; }
});
$('#select-page').addEventListener('change', () => {
  state.selected.clear();
  if ($('#select-page').checked) for (const record of state.pageRecords) state.selected.set(record.id, record);
  updateSelection();
});
$('#delete-selected').addEventListener('click', () => openDeleteDialog([...state.selected.values()], true));
$('#delete-cancel').addEventListener('click', () => $('#delete-dialog').close());
$('#delete-dialog').addEventListener('cancel', event => {
  if ($('#delete-confirm').disabled) event.preventDefault();
});
$('#delete-dialog').addEventListener('close', () => { if (!$('#delete-dialog').open) { state.deleteRecords = []; state.deleteBatch = false; } });
$('#delete-form').addEventListener('submit', async event => {
  event.preventDefault();
  const records = state.deleteRecords.slice(); const batch = state.deleteBatch;
  const submit = $('#delete-confirm');
  if (!state.ready || !records.length || submit.disabled) return;
  const context = productContext();
  submit.disabled = true; $('#delete-cancel').disabled = true;
  $('#delete-error').textContent = '';
  try {
    if (batch) await api('codes/delete-batch', { ids: records.map(record => record.id) }, context);
    else await api('codes/' + encodeURIComponent(records[0].id) + '/delete', {}, context);
    requireCurrent(context); $('#delete-dialog').close();
    const deleted = new Set(records.map(record => record.id));
    if (state.issued.some(item => deleted.has(item.id))) {
      state.issued = state.issued.filter(item => !deleted.has(item.id));
      $('#issued-codes').value = state.issued.map(item => item.code).join('\n');
      $('#issued-title').textContent = '本次剩余 ' + state.issued.length + ' 条卡密';
      $('#issued').hidden = state.issued.length === 0;
    }
    await refresh(context); requireCurrent(context);
    message((batch ? '已永久删除 ' + records.length + ' 条卡密' : '卡密已永久删除') + (state.accountBound ? '，该卡权益已撤销，历史回执与操作记录已保留。' : '，关联会话已清理，操作记录已保留。'));
  } catch (error) {
    if (!isCurrent(context) || error === staleResponse) return;
    if ($('#delete-dialog').open) $('#delete-error').textContent = error.message || String(error);
    else showError(error, context);
  } finally { if (isCurrent(context)) { submit.disabled = false; $('#delete-cancel').disabled = false; } }
});
$('#refresh').addEventListener('click', async () => {
  let context = productContext();
  const element = $('#refresh'); element.disabled = true;
  try {
    if (state.ready) await refresh(context);
    else context = await loadProduct(context);
    requireCurrent(context); message('记录已刷新。');
  } catch (error) { showError(error, context); } finally { if (isCurrent(context)) element.disabled = false; }
});
$('#logout').addEventListener('click', async () => {
  const context = productContext();
  try { await api('logout', {}, context); requireCurrent(context); location.assign('/admin/login'); } catch (error) { showError(error, context); }
});
const initialContext = productContext();
loadProduct(initialContext).catch(error => showError(error, initialContext));
