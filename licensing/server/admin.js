'use strict';
const csrf = document.querySelector('meta[name="csrf-token"]').content;
const message = document.querySelector('#message');
async function api(path, body) {
  const response = await fetch('/api/admin/' + path, {
    method: body === undefined ? 'GET' : 'POST', credentials: 'same-origin',
    headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': csrf },
    body: body === undefined ? undefined : JSON.stringify(body)
  });
  if (response.status === 401) { location.href = '/admin/login'; throw new Error('请重新登录。'); }
  if (!response.ok) {
    const data = await response.json().catch(() => ({}));
    throw new Error(data.message || '请求失败：HTTP ' + response.status);
  }
  const text = await response.text();
  return text ? JSON.parse(text) : null;
}
function showError(error) { message.textContent = error.message; }
function utc(value) { return value ? new Date(value).toISOString().replace('T', ' ').replace('.000Z', 'Z') : '—'; }
async function refresh() {
  const records = await api('codes');
  const rows = document.querySelector('#rows'); rows.replaceChildren();
  for (const record of records) {
    const tr = document.createElement('tr');
    for (const text of [record.codeHint + ' / ' + (record.kind === 'permanent' ? '永久' : record.days + ' 天'),
      record.deviceId || '未兑换', utc(record.activatedAtUtc), utc(record.expiresAtUtc)]) {
      const td = document.createElement('td'); td.textContent = text; tr.append(td);
    }
    const td = document.createElement('td');
    td.textContent = record.suspended ? '已停用 ' : record.deviceId ? '已兑换 ' : '待兑换 ';
    if (record.kind === 'timed') {
      const button = document.createElement('button'); button.className = 'secondary';
      button.textContent = record.suspended ? '恢复' : '停用';
      button.addEventListener('click', async () => {
        button.disabled = true;
        try { await api('codes/' + record.id + '/suspension', { suspended: !record.suspended }); await refresh(); }
        catch (error) { showError(error); button.disabled = false; }
      }); td.append(button);
    }
    tr.append(td); rows.append(tr);
  }
  message.textContent = '已加载 ' + records.length + ' 条记录（最多显示最近 1000 条）。';
}
document.querySelector('#kind').addEventListener('change', event => {
  document.querySelector('#days-label').hidden = event.target.value !== 'timed';
});
document.querySelector('#issue').addEventListener('submit', async event => {
  event.preventDefault(); const button = event.submitter; button.disabled = true;
  try {
    const kind = document.querySelector('#kind').value;
    const result = await api('codes', { kind, days: kind === 'timed' ? Number(document.querySelector('#days').value) : null });
    document.querySelector('#new-code').textContent = result.code;
    document.querySelector('#issued').hidden = false; await refresh();
  } catch (error) { showError(error); } finally { button.disabled = false; }
});
document.querySelector('#copy').addEventListener('click', async () => {
  try { await navigator.clipboard.writeText(document.querySelector('#new-code').textContent); message.textContent = '授权码已复制。'; }
  catch { message.textContent = '请选中完整授权码手动复制。'; }
});
document.querySelector('#refresh').addEventListener('click', () => refresh().catch(showError));
document.querySelector('#logout').addEventListener('click', async () => {
  try { await api('logout', {}); location.href = '/admin/login'; } catch (error) { showError(error); }
});
refresh().catch(showError);
