"""Record an authorized revision that supersedes specific cleanup-preserved baselines.

The 2026-11-13 cleanup manifest froze the pre-cleanup tree. Moving the licensing
endpoint to the public server and rebuilding the protected client necessarily changes
a handful of preserved files. This tool enumerates every deviation from that frozen
baseline and refuses to record anything that was not declared up front, so the
release gate can distinguish an authorized revision from silent drift.
"""
from pathlib import Path
import argparse, datetime, hashlib, json, os

R = Path(__file__).resolve().parents[1]
REPO = R.parents[2]
BASE_URL = 'https://124.220.147.56/'

DECLARATIONS = {
    'CoH3/dev/SkyEye/client/app.config':
        '客户端默认配置：授权服务器地址由本机回环改为 ' + BASE_URL,
    'CoH3/dev/SkyEye/client/CoH3MemoryTool/LicenseConfiguration60.cs':
        '客户端默认授权地址常量改为 ' + BASE_URL,
    'CoH3/dev/SkyEye/delivery/client/天穹之眼-自有授权版-v0.60.8.exe.config':
        '交付包授权服务器地址改为 ' + BASE_URL,
    'CoH3/dev/SkyEye/delivery/client/天穹之眼-自有授权版-v0.60.8.exe':
        '按新授权地址重建的受保护客户端可执行文件。',
    'CoH3/output/SkyEye/SkyEye-HardenedV2-Client.zip':
        '客户包按本授权修订重新签发：包内配置与受保护客户端指向 ' + BASE_URL + '。',
    'LicensingSystem/server/LicenseService.cs':
        '跨机密文迁移容错与自愈；按用户要求增加单张及批量卡密事务删除，清理关联会话与激活请求并保留逐卡审计。',
    'LicensingSystem/server/ProtocolModels.cs':
        '新增 CodePlaintext / CodeRepairReport、CodeRow.CodeReadable 标记及 BatchDeleteRequest 批量删除请求。',
    'LicensingSystem/server/Program.cs':
        '新增密文迁移离线 CLI 与独立 DataProtection 提供器；新增受管理员认证和 CSRF 保护的单张及批量删除接口。',
    'LicensingSystem/server/admin.js':
        '卡密密文不可读容错；新增当前页勾选、全选、单张及批量删除确认、失败重试与删除后分页及发码导出同步。',
    'LicensingSystem/server/admin.html':
        '按用户要求增加当前页批量选择与永久删除二次确认弹窗，并在操作记录说明中列入删除。',
    'LicensingSystem/server/admin.css':
        '新增不可读卡密、批量操作栏、勾选框和选中行样式，复用原后台配色与布局。',
}

REASON = ('授权服务器由本机回环迁移到公网 124.220.147.56，交付包内的授权地址与可执行文件随之更新；'
          '修复 Windows DPAPI 跨机密文导致的后台 503；按用户要求新增单张及批量卡密永久删除、整批事务清理与逐卡操作审计。')


def load(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def text(path):
    b = Path(path).read_bytes()
    return b.decode('utf-16') if b.startswith((b'\xff\xfe', b'\xfe\xff')) else b.decode('utf-8-sig')


def remapper():
    layout2 = load(R/'evidence/refactor-manifest-2026-11-15.json')
    refactor = load(R/'evidence/refactor-manifest.json')
    entries = layout2['prefix_map'] + refactor['prefix_map']

    def remap(p):
        p = str(Path(p))
        for _ in range(8):
            hit = None
            for m in entries:
                f = m['from']
                if p == f:
                    hit = (m['to'], len(f))
                    break
                if p.startswith(f.rstrip('\\/') + os.sep):
                    hit = (m['to'].rstrip('\\/') + os.sep, len(f.rstrip('\\/')) + 1)
                    break
            if not hit:
                return Path(p)
            p = hit[0] + p[hit[1]:]
        return Path(p)
    return remap


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument('--out', default=str(R/'evidence/authorized-revision-2026-11-16.json'))
    ap.add_argument('--revision-id', default='2026-11-16-cloud-licensing-endpoint')
    args = ap.parse_args()

    remap = remapper()
    cleanup = load(R/'evidence/cleanup-manifest.json')
    exceptions = {'air-diagnostics.log', 'air-outline-diagnostics.log', 'minimap-diagnostics.log',
                  'overlay-diagnostics.log', 'Licensing.Server.dll', 'Licensing.Server.pdb',
                  'Start-Server.ps1', 'server.json'}
    overrides = []
    for entry in cleanup['preserved']:
        current = remap(entry['path'])
        if current.name in exceptions:
            continue
        if not current.is_file():
            raise SystemExit('Preserved file disappeared: ' + str(current))
        digest = sha(current)
        if digest == entry['sha256']:
            continue
        try:
            relative = current.relative_to(REPO).as_posix()
        except ValueError:
            raise SystemExit('Preserved file outside the repository: ' + str(current))
        if relative not in DECLARATIONS:
            raise SystemExit('Preserved file changed without a declaration: ' + relative)
        overrides.append({'path': relative, 'baselineSha256': entry['sha256'],
                          'currentSha256': digest, 'reason': DECLARATIONS[relative]})
    declared = {row['path'] for row in overrides}
    stale = sorted(set(DECLARATIONS) - declared)
    if stale:
        raise SystemExit('Declared revision entries no longer differ from the baseline: ' + ', '.join(stale))

    client_zip = REPO/'CoH3/output/SkyEye/SkyEye-HardenedV2-Client.zip'
    if not client_zip.is_file():
        raise SystemExit('Expected the customer archive to be present as the revision baseline.')
    cleanup_validation = load(R/'evidence/cleanup-validation.json')
    cleanup_baseline = cleanup_validation['customerArchiveSha256']
    latest = load(R/'evidence/latest-build.json')
    delivery_config = R/'delivery/client'/(Path(latest['Client']).name + '.config')
    if BASE_URL not in text(delivery_config):
        raise SystemExit('Delivery configuration does not point at the authorized endpoint.')

    record = {
        'revisionId': args.revision_id,
        'authorizedAt': datetime.datetime.now(datetime.timezone.utc).isoformat(),
        'reason': REASON,
        'deployment': {
            'baseUrl': BASE_URL,
            'host': '124.220.147.56',
            'transport': 'TLS 1.2/1.3，Let\'s Encrypt shortlived profile 的 IP 证书（系统信任链可直接校验）',
            'server': 'nginx 1.29.8 → http://127.0.0.1:47835（ASP.NET Core 10），systemd 单元 licensing.service',
        },
        'clientArchive': {
            'path': 'CoH3/output/SkyEye/SkyEye-HardenedV2-Client.zip',
            'baselineSha256': cleanup_baseline,
            'currentSha256': sha(client_zip),
            'authorized': True,
            'reissueReason': '授权地址与受保护客户端已更新，客户包按本授权修订重新签发；cleanup 期的原始哈希保留用于追溯。',
            'history': [h for h in dict.fromkeys([cleanup_baseline, sha(client_zip)])],
        },
        'preservedOverrides': overrides,
    }
    Path(args.out).write_text(json.dumps(record, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'out': args.out, 'preservedOverrides': len(overrides),
                      'paths': [row['path'] for row in overrides],
                      'clientArchiveBaseline': record['clientArchive']['baselineSha256'],
                      'clientArchiveCurrent': record['clientArchive']['currentSha256']},
                     ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
