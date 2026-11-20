# 线上授权服务运维手册（124.220.147.56）

> 生成时间：2026-11-16　｜　适用实例：124.220.147.56（OpenCloudOS 9.6 / 2 vCPU / 2 GB RAM / 50 GB 系统盘）
>
> **默认结论：部署已经全部完成，你不需要执行任何命令。**
> 本手册用于日常运维、故障排查，以及你想自己动手时的确切命令。

---

## 1. 现在的部署形态

| 层 | 组件 | 位置 |
| --- | --- | --- |
| 公网入口 | nginx 1.29.8（TLS 终止） | `/etc/nginx/conf.d/licensing.conf` |
| HTTPS 证书 | Let's Encrypt `shortlived` IP 证书（ECDSA P-256，IP SAN=124.220.147.56，160 小时有效，acme.sh 自动续期） | `/etc/nginx/ssl/licensing/` |
| 应用 | .NET 10 ASP.NET Core（`Licensing.Server.dll`），systemd 托管 | `/opt/licensing`，`licensing.service` |
| 私有监听 | `http://127.0.0.1:47835`（**不出公网**） | `server.local.json` 的 `ListenUrl` |
| 数据库 | SQLite（WAL），`licensing` 系统用户，目录 700 / 文件 600 | `/opt/licensing/data/leases.db` |
| 签名私钥 | 租约签名 RSA-3072，0600 | `/opt/licensing/data/keys/` |
| 配置 | `LICENSING_SERVER_CONFIG=/opt/licensing/server.local.json` | 640，owner `licensing` |
| 防火墙 | 自建链 `LICENSING-FW`，只对白名单 IP 放行 22/8888 | `/usr/local/sbin/licensing-firewall.sh` |
| 备份 | 每日 03:17 sqlite 在线备份 + 密钥 + 配置，保留 14 份 | `/opt/licensing-backups/` |
| 面板 | 宝塔 BT-Panel（`:8888`，安全入口 `/tencentcloud`） | 已纳入白名单策略 |

客户端已全部指向 `https://124.220.147.56/`（外部 `.exe.config`，无需重编受保护 EXE）。

---

## 2. 你需要自己做的事

| # | 事项 | 怎么做 |
| --- | --- | --- |
| 1 | **（可选，推荐）腾讯云安全组再收紧一层** | 控制台 → 云服务器 → 安全组 → 入站规则：删除 `22/8888` 的 `0.0.0.0/0` 放行，改为仅放行你的常用出口 IP。服务器内的 iptables 已生效，安全组是第二道锁。**注意：`80/443` 必须保持对全网放行**，否则所有客户端都无法激活/心跳。**`22` 若仍对 `0.0.0.0/0` 放行，安全组等于没起作用**（服务器内 iptables 仍只放行白名单，但多一层更好）。 |
| 2 | **记住管理入口** | 管理后台 `https://124.220.147.56/admin`（登录口令 = `LicensingSystem/.local/hardening-v2/server.json` 里的 `AdminToken`）。宝塔面板：`http://124.220.147.56:8888/tencentcloud`（用户名 `202e4c96`）。宝塔对**非浏览器 User-Agent 直接返回 404**（防扫描设计），用 curl 探测时必须带浏览器 UA，否则会误判为“面板坏了”。 |
| 3 | **换网络后放行新 IP** | 见 §4；换 IP 前先用腾讯云 OrcaTerm（走内网，不受影响）登录。 |

> 不要重新生成 `AdminToken` 或签名私钥：客户端内已 pin 租约签名公钥，换私钥必须重新 pin、重建、保护并分发客户端。

**在你自己的电脑上自检（PowerShell）：**

```powershell
Test-NetConnection 124.220.147.56 -Port 443    # 必须 True —— 客户端走这个口
Test-NetConnection 124.220.147.56 -Port 8888   # 宝塔面板；改了安全组后仍应为 True（仅你自己的 IP）
Test-NetConnection 124.220.147.56 -Port 22     # SSH；改动后应只有你自己的 IP 能连上
Test-NetConnection 124.220.147.56 -Port 47835  # 必须 False —— 私有监听口绝不能出公网
```

若 8888 变成 `False`：安全组规则的「来源」不是你当前出口 IP（或你换了网络）。先把安全组来源改成你当前 IP，再用 OrcaTerm 在服务器上执行 `/usr/local/sbin/licensing-fw-allow-me.sh`。

---

## 3. 日常运维命令速查

### 3.1 服务状态与日志

```bash
systemctl status licensing nginx licensing-firewall   # 三个服务都应 active (running)
journalctl -u licensing -f                            # 应用实时日志
journalctl -u licensing --since "1 hour ago" --no-pager | tail -n 100
tail -f /var/log/nginx/access.log                     # 边缘访问日志
journalctl -u nginx -f                                # nginx 错误日志
```

### 3.2 重启与健康检查

```bash
systemctl restart licensing                           # 仅重启应用（配置改动后）
systemctl restart nginx                               # 改完 nginx 配置后
nginx -t && systemctl reload nginx                    # 先语法检查再热加载

curl -s http://127.0.0.1:47835/health                 # 本机自检（应返回 status=ready）
curl -s https://124.220.147.56/health                 # 公网自检
curl -s https://124.220.147.56/api/v1/version         # 版本与产品号
ss -lntp | grep 47835                                 # 确认 47835 只绑 127.0.0.1
```

### 3.3 证书续期（acme.sh 已自带定时任务，每 6 小时检查一次）

```bash
/opt/acme.sh/acme.sh --info -d 124.220.147.56         # 查看有效期与下次续期时间
/opt/acme.sh/acme.sh --renew -d 124.220.147.56 --ecc  # 手动续期（成功后自动 reload nginx）
```

`shortlived` 证书有效期只有 160 小时，**不要关闭 acme.sh 的 crontab**（`crontab -l` 里应有 `52 0,6,12,18 * * * ... acme.sh --cron`）。

### 3.4 备份与恢复

```bash
/usr/local/bin/licensing-backup.sh                    # 手动打一份备份
ls -l /opt/licensing-backups/                         # 备份列表（每日 03:17 自动执行）
```

恢复（服务需先停，避免写冲突）：

```bash
systemctl stop licensing
cp /opt/licensing-backups/<时间戳>/leases.db /opt/licensing/data/leases.db
chown licensing:licensing /opt/licensing/data/leases.db
chmod 600 /opt/licensing/data/leases.db
systemctl start licensing
curl -s http://127.0.0.1:47835/health
```

备份目录里同时有 `keys/`（租约签名私钥 + Data Protection 密钥）与 `server.local.json`（含 AdminToken），**按机密文件对待**：不要传到公开位置。

### 3.5 发卡与查会话

最省事的方式是浏览器登录 `https://124.220.147.56/admin` 后在页面里操作（发卡 / 续期 / 解绑 / 踢下线 / 会话 / 审计）。

脚本化签发（本地执行，用本地私有配置里的 `AdminToken` 登录线上实例）：

```powershell
python .\CoH3\dev\SkyEye\scripts\prepare_live_cards.py `
  --private-config .\LicensingSystem\.local\hardening-v2\server.json `
  --private-out     .\LicensingSystem\.local\hardening-v2\cards-<日期> `
  --evidence-out    .\CoH3\dev\SkyEye\evidence\live-cards-<日期>.json `
  --base-url https://124.220.147.56/ --allow-host 124.220.147.56
```

---

## 4. 防火墙白名单（22 / 8888）

服务器上的 SSH 与宝塔面板只对以下来源放行：白名单管理员 IP、腾讯云内网 `10.0.0.0/8`（OrcaTerm 走这里）、本机回环。其余来源一律 DROP。80/443 不受影响。

```bash
/usr/local/sbin/licensing-fw-list.sh                  # 查看当前白名单及到期时间
/usr/local/sbin/licensing-fw-allow-me.sh              # 把「当前这条 SSH 连接」的来源 IP 放行 30 天
/usr/local/sbin/licensing-fw-list.sh --prune          # 清理过期条目并重建规则（每 10 分钟自动跑）
systemctl status licensing-firewall                   # 守护单元
```

**换了网络（IP 变了）怎么办：**

1. 用腾讯云控制台的 **OrcaTerm**（内网 10.0.0.x，不受白名单限制）登录；
2. 执行 `/usr/local/sbin/licensing-fw-allow-me.sh`（它会读取当前 SSH 连接的来源 IP 并放行 30 天）；
3. 回到自己的电脑重新 SSH。

**紧急回滚（把防火墙恢复到封锁前）：**

```bash
iptables-restore < /root/firewall-backup/iptables-latest.rules
ip6tables-restore < /root/firewall-backup/ip6tables-latest.rules
```

---

## 5. 常见故障排查

| 现象 | 排查命令 | 常见原因 |
| --- | --- | --- |
| 客户端报 `400 https_required` | `curl -i http://124.220.147.56/health` | 请求没走 HTTPS；反向代理后必须用 `https://` |
| 客户端报 `400 invalid_host` | `grep AllowedHosts /opt/licensing/server.local.json` | `AllowedHosts` 必须包含 `124.220.147.56` |
| 证书过期 | `openssl s_client -connect 124.220.147.56:443 -servername 124.220.147.56 </dev/null \| openssl x509 -noout -dates` | acme.sh 续期失败，看 `cat /root/.acme.sh/acme.sh.log` |
| 端口 47835 也能从公网访问 | `ss -lntp \| grep 47835` | `ListenUrl` 必须是 `127.0.0.1` |
| 登录被限流（每分钟 10 次） | `journalctl -u licensing \| grep -i limit` | 限流按来源 IP 计数；反代未透传真实 IP 时会全部落到 127.0.0.1 同一个桶 |
| 服务起不来 | `journalctl -u licensing -n 100 --no-pager` | 常见是配置 JSON 写错或 `data/` 权限被改 |
| 磁盘满 | `df -h /` | 备份目录默认保留 14 份，旧的 `/opt/licensing-backups/*` 可手动删除 |

---

## 6. 变更流程（改配置 / 换密钥）

```bash
cp /opt/licensing/server.local.json /opt/licensing/server.local.json.bak
vi /opt/licensing/server.local.json        # AllowedHosts / TrustedProxies / ListenUrl 等
chown licensing:licensing /opt/licensing/server.local.json && chmod 640 /opt/licensing/server.local.json
systemctl restart licensing
curl -s http://127.0.0.1:47835/health
```

新增可信反代时，**`TrustedProxies` 只接受精确 IP**，且 nginx 必须与应用同机（当前就是同机，`127.0.0.1`）。

---

## 7. 跨机迁移卡密密文（`integrity_error` 修复）

### 7.1 症状与原因

旧版后台读取卡密列表时，任一 `LicenseCodes.CodeCipher` 解密失败都会使整个接口返回 503 `integrity_error`。审计、会话和发卡可能仍正常，因为它们没有读取这些旧密文。

本次迁移遗漏了旧卡密的重加密：源 Windows 的 Data Protection 密钥受 DPAPI 保护，目标 Linux 新建了另一套密钥环。直接复制数据库及租约签名私钥，并不会让目标机取得旧卡密的解密能力；原样复制仍受 DPAPI 保护的 XML 密钥文件也不足以完成跨系统迁移。

采用保留全部记录的处理方式：在原 Windows 用户环境导出旧卡密明文与 ID，在目标机逐条校验原文哈希，再用目标密钥重加密。数据库中的卡密 ID、哈希、有效期、绑定关系、会话和审计记录均予以保留。

### 7.2 本次修复材料与部署状态（2026-11-16）

- 已定位 41 条由 Windows 迁入的历史回归测试卡；这 41 条记录不是客户卡。服务器另有云端签发的记录，应一并保留。
- 本地私有映射为 `LicensingSystem/.local/migration/legacy-code-map.json`，共 41 条 `{卡密ID: 明文}`。不要将此映射加入公开服务端或客户发布包。
- 2026-11-16 已完成云端修复：41 条旧密文均已重新加密，原卡密与签名私钥保留；公网列表、CSV 和受保护客户端协议已通过验证。
- 修复版遇到单条不可解记录会显示 `codeReadable=false`，其余记录继续展示；后续升级仍应以实际健康检查和后台验证为准。

### 7.3 保留全部卡密的修复步骤

先将本次服务端 ZIP、`upgrade-server.sh` 和私有映射分别上传至服务器。使用 root 执行：

```bash
bash /root/licensing-upgrade/upgrade-server.sh   /root/licensing-upgrade/SkyEye-HardenedV2-Server.zip   /root/licensing-upgrade/legacy-code-map.json
```

脚本在停服务前校验 ZIP 清单与各文件 SHA-256，并拒绝包内的 `data/`、`server.local.json`、目录穿越及符号链接。备份成功后才替换程序，保留现有配置、密钥和数据库，使用 `licensing` 用户执行重加密。升级失败时尝试恢复旧程序并重新启动服务，诊断文件保留在输出的暂存目录。

首次完整修复应输出 `repaired=41 alreadyReadable=0 missing=0 mismatched=0 total=41`；重复执行可显示 `alreadyReadable=41`。`missing` 和 `mismatched` 必须为零，否则检查映射与目标库的对应关系。修复后登录后台核实卡密列表、历史记录和新签发记录。

仅在新程序已安装且已有备份时，才可单独执行离线重加密：

```bash
install -o licensing -g licensing -m 600 /root/licensing-upgrade/legacy-code-map.json /opt/licensing/data/legacy-code-map.json
systemctl stop licensing
runuser -u licensing -- env DOTNET_ROOT=/opt/dotnet LICENSING_SERVER_CONFIG=/opt/licensing/server.local.json /opt/dotnet/dotnet /opt/licensing/Licensing.Server.dll --reprotect-codes /opt/licensing/data/legacy-code-map.json
systemctl start licensing
```

升级脚本会把上传的映射复制到 `/opt/licensing/data/legacy-code-map.json`，设置 owner 为 `licensing:licensing`、权限为 600，再交给 `licensing` 用户读取；因此上传在 root 私有目录也可执行。修复映射继续按私有数据保留，不进入公开发布包。

两个离线模式 `--export-code-plaintext OUTPUT_JSON`、`--reprotect-codes MAP_JSON` 会启动 CLI 进程，不启动 HTTP 监听。导出文件含明文，须放在私有目录；输出路径与配置、数据库、密钥目录应分离。

## 8. 升级与迁移的保留要求

- 保留 `/opt/licensing/data/` 与 `/opt/licensing/server.local.json`；本次没有换租约签名私钥，不需要重新分发客户端。
- 备份必须包含 SQLite 一致性快照、Data Protection 密钥环、租约签名私钥与服务器配置。在线 SQLite 请使用既有在线备份脚本，不要仅复制主 `.db` 文件而遗漏 WAL 中的提交。
- 本次选择重新加密，不删除任何旧卡密。不要按 `Note LIKE 'E2E-%'` 批量删除，因为新的线上回归卡也可能采用相同备注。
- Linux 之间迁移时仍须保留数据库对应的密钥环及相同的 Data Protection application name；若另行配置了密钥加密，还须迁移解密依赖。

日常升级仅传入服务端 ZIP，保留现有卡密密文：

```bash
bash /root/licensing-upgrade/upgrade-server.sh /root/licensing-upgrade/SkyEye-HardenedV2-Server.zip
```

仅首次迁移修复时才显式传入第二个映射参数。管理员正常删除卡密后，历史映射可能包含已删除 ID，日常升级无需重新执行该映射。

## 9. 单张卡密永久删除

在管理后台“卡密记录”的“管理操作”中点击“删除”，核对弹窗中的完整卡密或尾号以及备注，再点击“确认删除”。“取消”与 Esc 会关闭弹窗并保留记录；提交期间按钮禁用，等待服务端处理结果。

- 删除在一个数据库事务中完成：先清理该卡全部激活请求与会话，再删除卡密，并追加一条 `delete` 操作记录；历史审计保留。发生存储错误时整笔回滚，弹窗显示错误并可重试。
- 被删除卡密再次激活及旧请求重放会返回失效状态，旧会话心跳失效；已运行客户端在下次心跳或当前短租约到期时停止授权。
- 后台同步刷新列表、统计、本次发码及导出内容；删除末页最后一条时自动返回有效页。其他卡密、密钥和绑定关系不受该操作影响。
- 此操作永久移除当前库中的该条授权记录；既有私有备份按原备份保留策略保存。删除接口为 `POST /api/admin/codes/{id}/delete`，沿用管理员 Cookie 与 CSRF 校验。

## 10. 批量删除卡密

在“卡密记录”中勾选各行复选框，或点击“全选当前页”，然后点击“批量删除”。弹窗显示将删除的数量与卡密尾号，核对后点击“确认删除 N 条”。全选仅覆盖当前页（每页 25 条），查询、翻页或刷新后清空勾选；取消和提交失败时保留勾选。

- 整批在同一数据库事务中处理；先核实所有 ID，任一卡不存在则整批返回错误，存储失败时整批回滚。重新查询后再勾选重试。
- 每张卡分别清理其全部激活请求与会话，追加一条 `delete` 审计；其他卡密、现有密钥、历史审计与备份保持原用途。
- 删除后自动刷新统计、有效页和本次发码结果，避免导出刚删除的卡密。已连接客户端仍按下次心跳或短租约到期停止授权。
- 批量接口为 `POST /api/admin/codes/delete-batch`，JSON 格式为 `{"ids":["卡密ID1","卡密ID2"]}`。需要管理员 Cookie 和 CSRF；原始数组长度限定 1 至 200，ID 按大小写精确匹配并去重，响应的 `deletedCount` 为实际删除的不同卡密数。此接口不会自动扩展到筛选结果或全部记录。

